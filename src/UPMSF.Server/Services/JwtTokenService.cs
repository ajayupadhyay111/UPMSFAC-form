using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace UPMSF.Server.Services;

public class JwtOptions
{
    public string Key { get; set; } = "";
    public string Issuer { get; set; } = "UPMSF";
    public string Audience { get; set; } = "UPMSF";
    public int ExpiryHours { get; set; } = 12;
}

public class JwtTokenService
{
    private readonly JwtOptions _opt;
    public JwtTokenService(JwtOptions opt) => _opt = opt;

    public (string token, DateTime expiresUtc) Create(int applicantId, string registrationId, string societyName)
    {
        var expires = DateTime.UtcNow.AddHours(_opt.ExpiryHours);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, applicantId.ToString()),
            new Claim("rid", registrationId),
            new Claim("name", societyName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(_opt.Issuer, _opt.Audience, claims,
            expires: expires, signingCredentials: creds);
        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
