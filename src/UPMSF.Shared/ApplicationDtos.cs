using System.ComponentModel.DataAnnotations;

namespace UPMSF.Shared;

// ============================ Step 1 : Course Details ============================
public class CourseDetailsDto
{
    [Required]
    public AppliedFor AppliedFor { get; set; } = AppliedFor.NewCourse;

    [Required]
    public int CouncilId { get; set; }

    // --- New Course only (Seat Enhancement hides the course selection) ---
    public CourseType? CourseType { get; set; }
    public int? CourseId { get; set; }

    // Seat numbers are captured in Form Part-I (see FormPart1Dto). Exposed here read-only for display.
    public int? NoOfExistingSeats { get; set; }
    public int? NoOfSeatsApplied { get; set; }

    /// <summary>Validate the course step. Seat numbers are validated separately because their
    /// location differs: full apps capture them in Form-I, short apps in this step.</summary>
    public IValidatableObjectResult? Validate(bool requireSeats = false)
    {
        if (AppliedFor == AppliedFor.NewCourse)
        {
            if (CourseType is null) return IValidatableObjectResult.Fail("Course Type is required.");
            if (CourseId is null) return IValidatableObjectResult.Fail("Please select a Course.");
        }
        else if (requireSeats)
        {
            if (NoOfExistingSeats is null or < 0) return IValidatableObjectResult.Fail("No. of Existing Seats is required.");
            if (NoOfSeatsApplied is null or <= 0) return IValidatableObjectResult.Fail("No. of Seats Applied for (Enhancement) is required.");
        }
        return null;
    }
}

/// <summary>Tiny result helper so validation works the same on client and server without MVC.</summary>
public class IValidatableObjectResult
{
    public string Error { get; private set; } = "";
    public static IValidatableObjectResult Fail(string msg) => new() { Error = msg };
}

// ============================ Step 2 : Form Part I ============================
public class FormPart1Dto
{
    // --- Seat Enhancement only (full/registration-flow apps): shown at top of Form Part-I. ---
    public int? NoOfSeatsApplied { get; set; }     // No of Seats Applied for (Enhancement)
    public int? NoOfExistingSeats { get; set; }    // No of Existing Seats

    // Society / Trust / Company
    [Required, StringLength(100)]
    public string NatureOfInstitution { get; set; } = "";          // संस्था की प्रकृति (PRIVATE etc.)

    [Required, StringLength(300)]
    public string WhereRegistered { get; set; } = "";              // संस्था कहाँ पंजीकृत है?

    [Required]
    public DateTime? RegistrationDate { get; set; }                // रजिस्ट्रेशन का दिनांक

    [Required, StringLength(100)]
    public string RegistrationNumber { get; set; } = "";           // रजिस्ट्रेशन संख्या

    [Required, Range(0, 100000)]
    public decimal TotalLandInUpHectare { get; set; }              // सम्पूर्ण उ0प्र0 में भूमि (हेक्टेयर)

    [Required, StringLength(200)]
    public string HeadNameAndMobile { get; set; } = "";            // मुखिया का नाम एवं मोबाइल

    [Required, StringLength(200)]
    public string ExecutivePersonNameAndMobile { get; set; } = ""; // कार्यकारी व्यक्ति का नाम एवं मोबाइल

    // Training Center
    [Required, StringLength(200)]
    public string TrainingCenterName { get; set; } = "";

    [Required, StringLength(500)]
    public string TrainingCenterAddress { get; set; } = "";

    [Required, Range(0, 100000)]
    public int CurrentAdmissionCapacity { get; set; }              // वर्तमान भर्ती क्षमता

    [Required, Range(0, 100000)]
    public int EnhancedAdmissionCapacity { get; set; }             // बढ़ाकर भर्ती क्षमता

    [StringLength(300)]
    public string? RecognitionOrderNumberAndDate { get; set; }     // शासनादेश संख्या व दिनांक

    [StringLength(500)]
    public string? OtherOngoingTrainings { get; set; }             // अन्य प्रशिक्षणों के नाम
}

// ============================ Step 3 : Form Part II ============================
public class FormPart2Dto
{
    // Teaching land
    [Required, StringLength(200)] public string TeachingLandOwnerName { get; set; } = "";
    [Required, StringLength(100)] public string TeachingKhasraPlotNo { get; set; } = "";
    [Required, Range(0, 1_000_000)] public int LandAreaSqft { get; set; }
    [Required, Range(20000, 10_000_000)] public int TeachingBlockBuiltAreaSqft { get; set; } // >= 20000
    [StringLength(100)] public string? TeachingFireRegNo { get; set; }
    public DateTime? TeachingFireValidity { get; set; }

    // Hostel
    [Required, Range(17500, 10_000_000)] public int HostelBlockBuiltAreaSqft { get; set; }   // >= 17500
    [Required, StringLength(100)] public string HostelKhasraPlotNo { get; set; } = "";
    [StringLength(100)] public string? HostelFireRegNo { get; set; }
    public DateTime? HostelFireValidity { get; set; }

    // Hospital
    [Required, StringLength(200)] public string HospitalLandOwnerName { get; set; } = "";
    [Required, StringLength(100)] public string HospitalKhasraPlotNo { get; set; } = "";
    [Required, StringLength(200)] public string HospitalName { get; set; } = "";
    [Required, StringLength(500)] public string HospitalAddress { get; set; } = "";
    [Required, Range(0, 100000)] public int HospitalTotalBeds { get; set; }
    [Range(0, 100000)] public int? HospitalBedsAtPreviousApplication { get; set; }

    [StringLength(100)] public string? CmoRegNo { get; set; }
    public DateTime? CmoValidity { get; set; }
    [Range(0, 100000)] public int? CmoBedCount { get; set; }

    [StringLength(100)] public string? PcbRegNo { get; set; }
    public DateTime? PcbValidity { get; set; }
    [Range(0, 100000)] public int? PcbBedCount { get; set; }

    [StringLength(100)] public string? HospitalFireRegNo { get; set; }
    public DateTime? HospitalFireValidity { get; set; }

    public bool IsHospitalEmpanelledPmjay { get; set; }

    // Financial
    [Required, Range(0, 1e12)] public decimal Deposit { get; set; }
    [Required, Range(0, 1e12)] public decimal FixedAssetsValue { get; set; }
    [Required, Range(0, 1e12)] public decimal CurrentAssetsValue { get; set; }
    [Required, Range(0, 1e12)] public decimal CapitalInvestment { get; set; }
    [Required, StringLength(50)] public string InstitutionAccountNumber { get; set; } = "";
}

// ============================ Step 5 : Payment ============================
public class PaymentDto
{
    public decimal BaseFee { get; set; } = 400000m;
    public decimal GstPercent { get; set; } = 18m;
    public decimal GstAmount => Math.Round(BaseFee * GstPercent / 100m, 2);
    public decimal TotalAmount => BaseFee + GstAmount;
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? TransactionId { get; set; }
    public DateTime? PaidAtUtc { get; set; }
}

// ============================ Composite / list ============================
public class ApplicationSummaryDto
{
    public int Id { get; set; }
    public string ApplicationNumber { get; set; } = "";   // AH91000003-1
    public AppliedFor AppliedFor { get; set; }
    public string CourseName { get; set; } = "";
    public int? NoOfExistingSeats { get; set; }
    public int? NoOfSeatsApplied { get; set; }
    public bool IsShortForm { get; set; }
    public ApplicationStep CurrentStep { get; set; }
    public ApplicationStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? SubmittedAtUtc { get; set; }
}

/// <summary>Full application used to resume a draft (all saved steps in one shot).</summary>
public class ApplicationDetailDto
{
    public int Id { get; set; }
    public string ApplicationNumber { get; set; } = "";
    public ApplicationStep CurrentStep { get; set; }
    public ApplicationStatus Status { get; set; }
    public bool IsShortForm { get; set; }

    public CourseDetailsDto? CourseDetails { get; set; }
    public FormPart1Dto? Part1 { get; set; }
    public FormPart2Dto? Part2 { get; set; }
    public List<DocumentDto> Documents { get; set; } = new();
    public PaymentDto Payment { get; set; } = new();

    public string CourseName { get; set; } = "";

    // Applicant snapshot (read-only, pre-filled from account)
    public string SocietyName { get; set; } = "";
    public string ProposedInstituteName { get; set; } = "";
    public string DistrictName { get; set; } = "";
    public string Address { get; set; } = "";
}

public class CreateApplicationResponse
{
    public int Id { get; set; }
    public string ApplicationNumber { get; set; } = "";
}
