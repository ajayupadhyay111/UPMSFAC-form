using System.ComponentModel.DataAnnotations;
using UPMSF.Shared;

namespace UPMSF.Server.Data;

/// <summary>The registered account. One per society/trust/company. Login = Phone + RegistrationId.</summary>
public class Applicant
{
    public int Id { get; set; }

    [MaxLength(20)]
    public string RegistrationId { get; set; } = "";   // AH91000003 (unique, also the login code)

    public ApplicantType ApplicantType { get; set; }
    [MaxLength(200)] public string SocietyName { get; set; } = "";
    [MaxLength(200)] public string ProposedInstituteName { get; set; } = "";
    [MaxLength(500)] public string Address { get; set; } = "";
    public int DistrictId { get; set; }
    public District District { get; set; } = null!;
    [MaxLength(6)] public string PinCode { get; set; } = "";
    [MaxLength(150)] public string ContactPersonName { get; set; } = "";
    [MaxLength(15)] public string Phone { get; set; } = "";
    [MaxLength(10)] public string Mobile { get; set; } = "";
    [MaxLength(150)] public string Email { get; set; } = "";

    /// <summary>Hash of the registration code (the login secret).</summary>
    [MaxLength(200)] public string CodeHash { get; set; } = "";

    public DateTime CreatedAtUtc { get; set; }

    /// <summary>Running counter for this applicant's application sub-numbers (AH91000003-1, -2 ...).</summary>
    public int ApplicationSequence { get; set; }

    public List<Application> Applications { get; set; } = new();
}

public class Application
{
    public int Id { get; set; }
    public int ApplicantId { get; set; }
    public Applicant Applicant { get; set; } = null!;

    [MaxLength(30)] public string ApplicationNumber { get; set; } = ""; // AH91000003-1

    public AppliedFor AppliedFor { get; set; }
    public int CouncilId { get; set; }
    public Council Council { get; set; } = null!;

    public CourseType? CourseType { get; set; }
    public int? CourseId { get; set; }
    public Course? Course { get; set; }

    public int? NoOfExistingSeats { get; set; }
    public int? NoOfSeatsApplied { get; set; }

    public ApplicationStep CurrentStep { get; set; } = ApplicationStep.CourseDetails;
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Draft;

    /// <summary>True for dashboard applications that reuse the first application's institutional
    /// details (Part-I/II/documents) — only course/seat is collected, no fee.</summary>
    public bool IsShortForm { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime? SubmittedAtUtc { get; set; }

    // Optimistic concurrency for 400 concurrent users editing drafts.
    [Timestamp] public byte[]? RowVersion { get; set; }

    public FormPart1? Part1 { get; set; }
    public FormPart2? Part2 { get; set; }
    public Payment? Payment { get; set; }
    public List<ApplicationDocument> Documents { get; set; } = new();
}

public class FormPart1
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public Application Application { get; set; } = null!;

    [MaxLength(100)] public string NatureOfInstitution { get; set; } = "";
    [MaxLength(300)] public string WhereRegistered { get; set; } = "";
    public DateTime? RegistrationDate { get; set; }
    [MaxLength(100)] public string RegistrationNumber { get; set; } = "";
    public decimal TotalLandInUpHectare { get; set; }
    [MaxLength(200)] public string HeadNameAndMobile { get; set; } = "";
    [MaxLength(200)] public string ExecutivePersonNameAndMobile { get; set; } = "";
    [MaxLength(200)] public string TrainingCenterName { get; set; } = "";
    [MaxLength(500)] public string TrainingCenterAddress { get; set; } = "";
    public int CurrentAdmissionCapacity { get; set; }
    public int EnhancedAdmissionCapacity { get; set; }
    [MaxLength(300)] public string? RecognitionOrderNumberAndDate { get; set; }
    [MaxLength(500)] public string? OtherOngoingTrainings { get; set; }
}

public class FormPart2
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public Application Application { get; set; } = null!;

    [MaxLength(200)] public string TeachingLandOwnerName { get; set; } = "";
    [MaxLength(100)] public string TeachingKhasraPlotNo { get; set; } = "";
    public int LandAreaSqft { get; set; }
    public int TeachingBlockBuiltAreaSqft { get; set; }
    [MaxLength(100)] public string? TeachingFireRegNo { get; set; }
    public DateTime? TeachingFireValidity { get; set; }

    public int HostelBlockBuiltAreaSqft { get; set; }
    [MaxLength(100)] public string HostelKhasraPlotNo { get; set; } = "";
    [MaxLength(100)] public string? HostelFireRegNo { get; set; }
    public DateTime? HostelFireValidity { get; set; }

    [MaxLength(200)] public string HospitalLandOwnerName { get; set; } = "";
    [MaxLength(100)] public string HospitalKhasraPlotNo { get; set; } = "";
    [MaxLength(200)] public string HospitalName { get; set; } = "";
    [MaxLength(500)] public string HospitalAddress { get; set; } = "";
    public int HospitalTotalBeds { get; set; }
    public int? HospitalBedsAtPreviousApplication { get; set; }

    [MaxLength(100)] public string? CmoRegNo { get; set; }
    public DateTime? CmoValidity { get; set; }
    public int? CmoBedCount { get; set; }

    [MaxLength(100)] public string? PcbRegNo { get; set; }
    public DateTime? PcbValidity { get; set; }
    public int? PcbBedCount { get; set; }

    [MaxLength(100)] public string? HospitalFireRegNo { get; set; }
    public DateTime? HospitalFireValidity { get; set; }

    public bool IsHospitalEmpanelledPmjay { get; set; }

    public decimal Deposit { get; set; }
    public decimal FixedAssetsValue { get; set; }
    public decimal CurrentAssetsValue { get; set; }
    public decimal CapitalInvestment { get; set; }
    [MaxLength(50)] public string InstitutionAccountNumber { get; set; } = "";
}

public class ApplicationDocument
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public Application Application { get; set; } = null!;
    public DocumentType DocumentType { get; set; }
    [MaxLength(260)] public string OriginalFileName { get; set; } = "";
    [MaxLength(400)] public string StoredPath { get; set; } = "";
    [MaxLength(100)] public string ContentType { get; set; } = "";
    public long SizeBytes { get; set; }
    public DateTime UploadedAtUtc { get; set; }
}

public class Payment
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public Application Application { get; set; } = null!;
    public decimal BaseFee { get; set; }
    public decimal GstPercent { get; set; }
    public decimal GstAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public PaymentStatus Status { get; set; }
    [MaxLength(50)] public string? TransactionId { get; set; }
    public DateTime? PaidAtUtc { get; set; }
}

// ----- Lookups -----
public class District
{
    public int Id { get; set; }
    [MaxLength(100)] public string Name { get; set; } = "";
}

public class Council
{
    public int Id { get; set; }
    [MaxLength(100)] public string Name { get; set; } = "";
}

public class Course
{
    public int Id { get; set; }
    [MaxLength(200)] public string Name { get; set; } = "";
    [MaxLength(30)] public string ShortCode { get; set; } = "";
    public CourseType CourseType { get; set; }
    public int CouncilId { get; set; }
}
