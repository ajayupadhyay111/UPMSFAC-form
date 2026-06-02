using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UPMSF.Server.Data;
using UPMSF.Server.Services;
using UPMSF.Shared;

namespace UPMSF.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtTokenService _jwt;
    public AuthController(AppDbContext db, JwtTokenService jwt) { _db = db; _jwt = jwt; }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register(RegisterRequest req)
    {
        if (!await _db.Districts.AnyAsync(d => d.Id == req.DistrictId))
            return BadRequest(new { message = "Invalid district." });

        // One account per phone number.
        if (await _db.Applicants.AnyAsync(a => a.Phone == req.Phone))
            return Conflict(new { message = "This phone number is already registered. Please login." });

        // Atomic next number from the SQL sequence -> AH9 + value (e.g. AH91000003).
        var registrationId = $"AH9{await NextRegistrationSeqAsync()}";

        var applicant = new Applicant
        {
            RegistrationId = registrationId,
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
            CodeHash = PasswordHasher.Hash(registrationId),
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.Applicants.Add(applicant);
        await _db.SaveChangesAsync();

        return Ok(new RegisterResponse
        {
            RegistrationId = registrationId,
            Phone = applicant.Phone,
            Message = "Registration successful. Please note your Registration Code — you will use it with your phone number to login."
        });
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

    /// <summary>Atomically fetch the next registration number from the SQL sequence.</summary>
    private async Task<int> NextRegistrationSeqAsync()
    {
        var conn = _db.Database.GetDbConnection();
        var wasClosed = conn.State != System.Data.ConnectionState.Open;
        if (wasClosed) await conn.OpenAsync();
        try
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT NEXT VALUE FOR RegistrationSeq";
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }
        finally
        {
            if (wasClosed) await conn.CloseAsync();
        }
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
