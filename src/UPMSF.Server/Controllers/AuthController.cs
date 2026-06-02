using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using UPMSF.Server.Data;
using UPMSF.Server.Services;
using UPMSF.Shared;

namespace UPMSF.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth")]   // SECURITY (S2): per-IP throttle on register/login
public class AuthController : ControllerBase
{
    // SECURITY (S1): registration code alphabet — crypto-random, unambiguous (no 0/O/1/I).
    // The code is the login secret, so it must be high-entropy, not a predictable sequence.
    private static readonly char[] CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

    private readonly AppDbContext _db;
    private readonly JwtTokenService _jwt;
    public AuthController(AppDbContext db, JwtTokenService jwt) { _db = db; _jwt = jwt; }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register(RegisterRequest req)
    {
        if (!await _db.Districts.AnyAsync(d => d.Id == req.DistrictId))
            return BadRequest(new { message = "Invalid district." });

        // One account per phone number (friendly pre-check; the DB unique index is the
        // real guard against the check-then-insert race — see the catch below).
        if (await _db.Applicants.AnyAsync(a => a.Phone == req.Phone))
            return Conflict(new { message = "This phone number is already registered. Please login." });

        var applicant = new Applicant
        {
            ApplicantType = req.ApplicantType,
            SocietyName = req.SocietyName.Trim(),
            ProposedInstituteName = req.ProposedInstituteName.Trim(),
            Address = req.Address.Trim(),
            DistrictId = req.DistrictId,
            PinCode = req.PinCode,
            ContactPersonName = req.ContactPersonName.Trim(),
            Phone = req.Phone,
            Mobile = req.Mobile,
            Email = req.Email.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.Applicants.Add(applicant);

        // Generate a high-entropy registration code; the unique index enforces uniqueness.
        // Retry on the (astronomically unlikely) code collision; surface a phone-race as 409.
        for (var attempt = 1; ; attempt++)
        {
            var registrationId = NewRegistrationId();
            applicant.RegistrationId = registrationId;
            applicant.CodeHash = PasswordHasher.Hash(registrationId);
            try
            {
                await _db.SaveChangesAsync();
                return Ok(new RegisterResponse
                {
                    RegistrationId = registrationId,
                    Phone = applicant.Phone,
                    Message = "Registration successful. Please note your Registration Code — you will use it with your phone number to login."
                });
            }
            catch (DbUpdateException) when (attempt < 5)
            {
                // Phone uniqueness lost a race -> friendly conflict. Otherwise treat as a
                // registration-code collision and retry with a fresh code.
                if (await _db.Applicants.AnyAsync(a => a.Phone == req.Phone && a.Id != applicant.Id))
                    return Conflict(new { message = "This phone number is already registered. Please login." });
            }
        }
    }

    /// <summary>Crypto-random registration code: AH9 + 8 unambiguous chars (~40 bits).</summary>
    private static string NewRegistrationId()
    {
        Span<byte> bytes = stackalloc byte[8];
        RandomNumberGenerator.Fill(bytes);
        var chars = new char[8];
        for (var i = 0; i < chars.Length; i++)
            chars[i] = CodeAlphabet[bytes[i] % CodeAlphabet.Length];
        return "AH9" + new string(chars);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ProfileDto>> Me()
    {
        var idStr = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idStr, out var id)) return Unauthorized();

        var a = await _db.Applicants.Include(x => x.District).FirstOrDefaultAsync(x => x.Id == id);
        if (a is null) return NotFound();

        return new ProfileDto
        {
            RegistrationId = a.RegistrationId,
            ApplicantType = a.ApplicantType,
            SocietyName = a.SocietyName,
            ProposedInstituteName = a.ProposedInstituteName,
            Address = a.Address,
            DistrictName = a.District?.Name ?? "",
            PinCode = a.PinCode,
            ContactPersonName = a.ContactPersonName,
            Phone = a.Phone,
            Mobile = a.Mobile,
            Email = a.Email,
            CreatedAtUtc = a.CreatedAtUtc
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest req)
    {
        var applicant = await _db.Applicants
            .FirstOrDefaultAsync(a => a.Phone == req.Phone && a.RegistrationId == req.RegistrationId.Trim());

        if (applicant is null || !PasswordHasher.Verify(req.RegistrationId.Trim(), applicant.CodeHash))
            return Unauthorized(new { message = "Invalid phone number or registration code." });

        var (token, expires) = _jwt.Create(applicant.Id, applicant.RegistrationId, applicant.SocietyName);
        return Ok(new LoginResponse
        {
            Token = token,
            RegistrationId = applicant.RegistrationId,
            SocietyName = applicant.SocietyName,
            ExpiresAtUtc = expires
        });
    }
}
