using System.ComponentModel.DataAnnotations;

namespace UPMSF.Shared;

/// <summary>Fresh-applicant registration ("Applicant's Details" section).</summary>
public class RegisterRequest
{
    [Required]
    public ApplicantType ApplicantType { get; set; } = ApplicantType.Society;

    [Required, StringLength(200)]
    public string SocietyName { get; set; } = "";          // Name of Society/Trust/Company

    [Required, StringLength(200)]
    public string ProposedInstituteName { get; set; } = "";

    [Required, StringLength(500)]
    public string Address { get; set; } = "";

    [Required]
    public int DistrictId { get; set; }

    [Required, RegularExpression(@"^\d{6}$", ErrorMessage = "Pin Code must be 6 digits")]
    public string PinCode { get; set; } = "";

    [Required, StringLength(150)]
    public string ContactPersonName { get; set; } = "";

    // Indian phone: a 10-digit mobile (starts 6-9) OR a landline starting with 0 (10–12 digits).
    [Required, RegularExpression(@"^(0\d{9,11}|[6-9]\d{9})$", ErrorMessage = "Enter a valid Indian phone number")]
    public string Phone { get; set; } = "";

    // Indian mobile: exactly 10 digits, starting with 6, 7, 8 or 9.
    [Required, RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid 10-digit Indian mobile number (starting with 6-9)")]
    public string Mobile { get; set; } = "";

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = "";
}

/// <summary>Returned immediately after registration — shown once to the applicant.</summary>
public class RegisterResponse
{
    public string RegistrationId { get; set; } = "";   // e.g. AH91000003 (also the login code)
    public string Phone { get; set; } = "";
    public string Message { get; set; } = "";
}

/// <summary>Login = Phone + Registration code (AH91000003).</summary>
public class LoginRequest
{
    [Required, RegularExpression(@"^\d{10,12}$", ErrorMessage = "Enter a valid phone number")]
    public string Phone { get; set; } = "";

    [Required]
    public string RegistrationId { get; set; } = "";   // the code acts as the password
}

public class LoginResponse
{
    public string Token { get; set; } = "";
    public string RegistrationId { get; set; } = "";
    public string SocietyName { get; set; } = "";
    public DateTime ExpiresAtUtc { get; set; }
}

/// <summary>The logged-in applicant's registration details (read-only) for the dashboard.</summary>
public class ProfileDto
{
    public string RegistrationId { get; set; } = "";
    public ApplicantType ApplicantType { get; set; }
    public string SocietyName { get; set; } = "";
    public string ProposedInstituteName { get; set; } = "";
    public string Address { get; set; } = "";
    public string DistrictName { get; set; } = "";
    public string PinCode { get; set; } = "";
    public string ContactPersonName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Mobile { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; }
}
