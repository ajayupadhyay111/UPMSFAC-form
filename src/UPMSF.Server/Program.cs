using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
if (string.IsNullOrWhiteSpace(jwtOptions.Key))
    jwtOptions.Key = "DEV_ONLY_change_me_with_Jwt__Key_env_var_min_32_chars_long!!";

builder.Services.AddSingleton(jwtOptions);
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddScoped<FileStorageService>();

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
const string CorsPolicy = "client";
builder.Services.AddCors(o => o.AddPolicy(CorsPolicy, p =>
{
    var origins = builder.Configuration["Cors:Origins"]?.Split(',', StringSplitOptions.RemoveEmptyEntries);
    if (origins is { Length: > 0 }) p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
    else p.SetIsOriginAllowed(_ => true).AllowAnyHeader().AllowAnyMethod();
}));

var app = builder.Build();

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
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
