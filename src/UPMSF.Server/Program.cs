using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UPMSF.Server.Data;
using UPMSF.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// ---- Configuration (env-var friendly for Azure / any host) ----
// Connection string: ConnectionStrings__Default ; JWT key: Jwt__Key ; etc.
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Server=(localdb)\\MSSQLLocalDB;Database=UPMSF;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

var jwtOptions = new JwtOptions();
builder.Configuration.GetSection("Jwt").Bind(jwtOptions);

// SECURITY (S3): never silently run on a known/dev key in Production.
// In Production a missing or short key is a fatal misconfiguration — fail fast
// rather than signing forgeable tokens. Development keeps a convenience fallback.
if (string.IsNullOrWhiteSpace(jwtOptions.Key) || jwtOptions.Key.Length < 32)
{
    if (builder.Environment.IsProduction())
        throw new InvalidOperationException(
            "Jwt__Key is missing or shorter than 32 characters. Set a long random secret (e.g. `openssl rand -base64 48`).");
    jwtOptions.Key = "DEV_ONLY_change_me_with_Jwt__Key_env_var_min_32_chars_long!!";
}

builder.Services.AddSingleton(jwtOptions);
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddScoped<FileStorageService>();
builder.Services.AddMemoryCache();

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// SECURITY (S2): per-IP rate limiting for the auth endpoints (brute-force / enumeration guard).
// Generous enough for real users; trips automated abuse to HTTP 429.
const string AuthRateLimit = "auth";
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy(AuthRateLimit, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

// Behind Caddy/any reverse proxy: trust X-Forwarded-* so RemoteIpAddress (used by the
// rate limiter) and the scheme are the real client's, not the proxy's.
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    o.KnownNetworks.Clear();
    o.KnownProxies.Clear();
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key))
        };
    });
builder.Services.AddAuthorization();

// CORS for when the Blazor client is hosted/run separately.
// SECURITY (S4): permissive only in Development. In Production we allow exactly the
// configured origins; with none set, no cross-origin is permitted (the hosted client
// is same-origin, so the normal single-unit deployment is unaffected).
const string CorsPolicy = "client";
builder.Services.AddCors(o => o.AddPolicy(CorsPolicy, p =>
{
    var origins = builder.Configuration["Cors:Origins"]?.Split(',', StringSplitOptions.RemoveEmptyEntries);
    if (origins is { Length: > 0 })
        p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
    else if (builder.Environment.IsDevelopment())
        p.SetIsOriginAllowed(_ => true).AllowAnyHeader().AllowAnyMethod();
    // else (Production, no origins configured): leave the policy empty -> cross-origin denied.
}));

var app = builder.Build();

app.UseForwardedHeaders();

// ---- Global exception handler (A2): consistent { message } JSON the client already
// understands; maps DB conflicts -> 409 and bad principal -> 401. ----
app.UseExceptionHandler(errApp => errApp.Run(async ctx =>
{
    var ex = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;
    var (status, message) = ex switch
    {
        UnauthorizedAppException => (StatusCodes.Status401Unauthorized, ex.Message),
        DbUpdateConcurrencyException => (StatusCodes.Status409Conflict,
            "This record was modified by another request. Please reload and try again."),
        DbUpdateException => (StatusCodes.Status409Conflict,
            "A conflicting record already exists. Please retry."),
        _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
    };
    if (status == StatusCodes.Status500InternalServerError)
        app.Logger.LogError(ex, "Unhandled exception");
    ctx.Response.StatusCode = status;
    ctx.Response.ContentType = "application/json";
    await ctx.Response.WriteAsJsonAsync(new { message });
}));

// ---- Auto-apply migrations on startup (retry while the DB container warms up) ----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    for (var attempt = 1; ; attempt++)
    {
        try { db.Database.Migrate(); break; }
        catch (Exception ex) when (attempt < 12)
        {
            app.Logger.LogWarning(ex, "Database not ready (attempt {Attempt}/12). Retrying in 5s…", attempt);
            Thread.Sleep(5000);
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Serve the Blazor WebAssembly client as static files (single-unit deployment).
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseCors(CorsPolicy);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
