using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace UPMSF.Client.Services;

/// <summary>
/// Reads the JWT from localStorage so the session survives tab-close / reload / power-cut.
/// </summary>
public class AppAuthStateProvider : AuthenticationStateProvider
{
    public const string TokenKey = "upmsf_token";
    private readonly ILocalStorageService _storage;
    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public AppAuthStateProvider(ILocalStorageService storage) => _storage = storage;

    public string? CurrentRegistrationId { get; private set; }
    public string? CurrentSocietyName { get; private set; }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _storage.GetItemAsStringAsync(TokenKey);
        if (string.IsNullOrWhiteSpace(token)) return Anonymous;

        var claims = ParseClaims(token);
        if (claims is null) { await _storage.RemoveItemAsync(TokenKey); return Anonymous; }

        // expiry check
        var exp = claims.FirstOrDefault(c => c.Type == "exp")?.Value;
        if (long.TryParse(exp, out var expSeconds) &&
            DateTimeOffset.FromUnixTimeSeconds(expSeconds) < DateTimeOffset.UtcNow)
        {
            await _storage.RemoveItemAsync(TokenKey);
            return Anonymous;
        }

        CurrentRegistrationId = claims.FirstOrDefault(c => c.Type == "rid")?.Value;
        CurrentSocietyName = claims.FirstOrDefault(c => c.Type == "name")?.Value;
        var identity = new ClaimsIdentity(claims, "jwt", "name", ClaimTypes.Role);
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task SignInAsync(string token)
    {
        await _storage.SetItemAsStringAsync(TokenKey, token);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task SignOutAsync()
    {
        await _storage.RemoveItemAsync(TokenKey);
        CurrentRegistrationId = null;
        CurrentSocietyName = null;
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
    }

    private static List<Claim>? ParseClaims(string jwt)
    {
        try
        {
            var payload = jwt.Split('.')[1];
            var json = Base64UrlDecode(payload);
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            if (dict is null) return null;
            var claims = new List<Claim>();
            foreach (var kv in dict)
                claims.Add(new Claim(kv.Key, kv.Value.ToString()));
            return claims;
        }
        catch { return null; }
    }

    private static string Base64UrlDecode(string input)
    {
        var s = input.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4) { case 2: s += "=="; break; case 3: s += "="; break; }
        return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(s));
    }
}
