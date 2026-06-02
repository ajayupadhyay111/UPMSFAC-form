using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UPMSF.Server.Data;
using UPMSF.Shared;

namespace UPMSF.Server.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public partial class ApplicationsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ApplicationsController(AppDbContext db) => _db = db;

    private int ApplicantId =>
        int.Parse(User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ---------------------------------------------------------------- list
    [HttpGet]
    public async Task<List<ApplicationSummaryDto>> List()
    {
        var id = ApplicantId;
        return await _db.Applications
            .Where(a => a.ApplicantId == id)
            .OrderByDescending(a => a.CreatedAtUtc)
            .Select(a => new ApplicationSummaryDto
            {
                Id = a.Id,
                ApplicationNumber = a.ApplicationNumber,
                AppliedFor = a.AppliedFor,
                CourseName = a.Course != null ? a.Course.Name
                             : (a.AppliedFor == AppliedFor.SeatEnhancement ? "Seat Enhancement" : ""),
                NoOfExistingSeats = a.NoOfExistingSeats,
                NoOfSeatsApplied = a.NoOfSeatsApplied,
                IsShortForm = a.IsShortForm,
                CurrentStep = a.CurrentStep,
                Status = a.Status,
                CreatedAtUtc = a.CreatedAtUtc,
                SubmittedAtUtc = a.SubmittedAtUtc
            }).ToListAsync();
    }

    // ---------------------------------------------------------------- create (Step 1: course details)
    [HttpPost]
    public async Task<ActionResult<CreateApplicationResponse>> Create(CourseDetailsDto dto)
    {
        var err = dto.Validate();
        if (err is not null) return BadRequest(new { message = err.Error });
        if (!await _db.Councils.AnyAsync(c => c.Id == dto.CouncilId))
            return BadRequest(new { message = "Invalid council." });
        if (dto.AppliedFor == AppliedFor.NewCourse &&
            !await _db.Courses.AnyAsync(c => c.Id == dto.CourseId && c.CourseType == dto.CourseType && c.CouncilId == dto.CouncilId))
            return BadRequest(new { message = "Invalid course for the selected type/council." });

        var applicant = await _db.Applicants.FirstAsync(a => a.Id == ApplicantId);

        // The most recent SUBMITTED full application is the source whose institutional
        // details (Part-I/II/documents) are reused by short (dashboard) applications.
        var source = await _db.Applications
            .Include(a => a.Part1).Include(a => a.Part2).Include(a => a.Documents)
            .Where(a => a.ApplicantId == applicant.Id && !a.IsShortForm
                        && a.Status == ApplicationStatus.Submitted && a.Part2 != null)
            .OrderByDescending(a => a.SubmittedAtUtc)
            .FirstOrDefaultAsync();
        // Short (reuse details) only when a submitted source exists; otherwise it is a full
        // application — including a first-time Seat Enhancement, which collects full details.
        var isShort = source is not null;

        // Seat numbers: short apps capture them here (Form-I is skipped); full apps capture
        // them in Form-I, so they are not required at creation.
        if (dto.AppliedFor == AppliedFor.SeatEnhancement && isShort)
        {
            if (dto.NoOfExistingSeats is null or < 0)
                return BadRequest(new { message = "No. of Existing Seats is required." });
            if (dto.NoOfSeatsApplied is null or <= 0)
                return BadRequest(new { message = "No. of Seats Applied for (Enhancement) is required." });
        }

        applicant.ApplicationSequence += 1;              // AH9...-N
        var number = $"{applicant.RegistrationId}-{applicant.ApplicationSequence}";

        var now = DateTime.UtcNow;
        var app = new Application
        {
            ApplicantId = applicant.Id,
            ApplicationNumber = number,
            AppliedFor = dto.AppliedFor,
            CouncilId = dto.CouncilId,
            CourseType = dto.AppliedFor == AppliedFor.NewCourse ? dto.CourseType : null,
            CourseId = dto.AppliedFor == AppliedFor.NewCourse ? dto.CourseId : null,
            NoOfExistingSeats = dto.AppliedFor == AppliedFor.SeatEnhancement ? dto.NoOfExistingSeats : null,
            NoOfSeatsApplied = dto.AppliedFor == AppliedFor.SeatEnhancement ? dto.NoOfSeatsApplied : null,
            IsShortForm = isShort,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        if (isShort)
        {
            // Reuse institutional details from the source application (snapshot copy).
            app.Part1 = CopyPart1(source!.Part1!);
            app.Part2 = CopyPart2(source.Part2!);
            app.Documents = source.Documents.Select(CopyDocument).ToList();
            app.CurrentStep = ApplicationStep.Payment;   // skip Part-I/II/Documents; no fee — straight to submit
            app.Status = ApplicationStatus.Draft;
        }
        else
        {
            app.CurrentStep = ApplicationStep.FormPart1; // first/full application
            app.Status = ApplicationStatus.Draft;
        }

        _db.Applications.Add(app);
        await _db.SaveChangesAsync();

        return Ok(new CreateApplicationResponse { Id = app.Id, ApplicationNumber = app.ApplicationNumber });
    }

    // ---------------------------------------------------------------- detail / resume
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApplicationDetailDto>> Get(int id)
    {
        var app = await Owned(id)
            .Include(a => a.Course)
            .Include(a => a.Applicant).ThenInclude(a => a.District)
            .Include(a => a.Part1).Include(a => a.Part2).Include(a => a.Payment)
            .Include(a => a.Documents)
            .FirstOrDefaultAsync();
        if (app is null) return NotFound();
        return Ok(Map(app));
    }

    // ---------------------------------------------------------------- Step 2: Part I
    [HttpPut("{id:int}/part1")]
    public async Task<IActionResult> SavePart1(int id, FormPart1Dto dto)
    {
        var app = await Owned(id).Include(a => a.Part1).FirstOrDefaultAsync();
        if (app is null) return NotFound();
        if (app.CurrentStep != ApplicationStep.FormPart1) return Locked(app);

        // Full Seat Enhancement apps capture seats here (in Form-I).
        if (app.AppliedFor == AppliedFor.SeatEnhancement)
        {
            if (dto.NoOfExistingSeats is null or < 0)
                return BadRequest(new { message = "No. of Existing Seats is required." });
            if (dto.NoOfSeatsApplied is null or <= 0)
                return BadRequest(new { message = "No. of Seats Applied for (Enhancement) is required." });
            app.NoOfExistingSeats = dto.NoOfExistingSeats;
            app.NoOfSeatsApplied = dto.NoOfSeatsApplied;
        }

        app.Part1 ??= new FormPart1 { ApplicationId = app.Id };
        var p = app.Part1;
        p.NatureOfInstitution = dto.NatureOfInstitution.Trim();
        p.WhereRegistered = dto.WhereRegistered.Trim();
        p.RegistrationDate = dto.RegistrationDate;
        p.RegistrationNumber = dto.RegistrationNumber.Trim();
        p.TotalLandInUpHectare = dto.TotalLandInUpHectare;
        p.HeadNameAndMobile = dto.HeadNameAndMobile.Trim();
        p.ExecutivePersonNameAndMobile = dto.ExecutivePersonNameAndMobile.Trim();
        p.TrainingCenterName = dto.TrainingCenterName.Trim();
        p.TrainingCenterAddress = dto.TrainingCenterAddress.Trim();
        p.CurrentAdmissionCapacity = dto.CurrentAdmissionCapacity;
        p.EnhancedAdmissionCapacity = dto.EnhancedAdmissionCapacity;
        p.RecognitionOrderNumberAndDate = dto.RecognitionOrderNumberAndDate;
        p.OtherOngoingTrainings = dto.OtherOngoingTrainings;

        app.CurrentStep = ApplicationStep.FormPart2;
        app.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ---------------------------------------------------------------- Step 3: Part II
    [HttpPut("{id:int}/part2")]
    public async Task<IActionResult> SavePart2(int id, FormPart2Dto dto)
    {
        var app = await Owned(id).Include(a => a.Part2).FirstOrDefaultAsync();
        if (app is null) return NotFound();
        if (app.CurrentStep != ApplicationStep.FormPart2) return Locked(app);

        app.Part2 ??= new FormPart2 { ApplicationId = app.Id };
        var p = app.Part2;
        p.TeachingLandOwnerName = dto.TeachingLandOwnerName.Trim();
        p.TeachingKhasraPlotNo = dto.TeachingKhasraPlotNo.Trim();
        p.LandAreaSqft = dto.LandAreaSqft;
        p.TeachingBlockBuiltAreaSqft = dto.TeachingBlockBuiltAreaSqft;
        p.TeachingFireRegNo = dto.TeachingFireRegNo;
        p.TeachingFireValidity = dto.TeachingFireValidity;
        p.HostelBlockBuiltAreaSqft = dto.HostelBlockBuiltAreaSqft;
        p.HostelKhasraPlotNo = dto.HostelKhasraPlotNo.Trim();
        p.HostelFireRegNo = dto.HostelFireRegNo;
        p.HostelFireValidity = dto.HostelFireValidity;
        p.HospitalLandOwnerName = dto.HospitalLandOwnerName.Trim();
        p.HospitalKhasraPlotNo = dto.HospitalKhasraPlotNo.Trim();
        p.HospitalName = dto.HospitalName.Trim();
        p.HospitalAddress = dto.HospitalAddress.Trim();
        p.HospitalTotalBeds = dto.HospitalTotalBeds;
        p.HospitalBedsAtPreviousApplication = dto.HospitalBedsAtPreviousApplication;
        p.CmoRegNo = dto.CmoRegNo;
        p.CmoValidity = dto.CmoValidity;
        p.CmoBedCount = dto.CmoBedCount;
        p.PcbRegNo = dto.PcbRegNo;
        p.PcbValidity = dto.PcbValidity;
        p.PcbBedCount = dto.PcbBedCount;
        p.HospitalFireRegNo = dto.HospitalFireRegNo;
        p.HospitalFireValidity = dto.HospitalFireValidity;
        p.IsHospitalEmpanelledPmjay = dto.IsHospitalEmpanelledPmjay;
        p.Deposit = dto.Deposit;
        p.FixedAssetsValue = dto.FixedAssetsValue;
        p.CurrentAssetsValue = dto.CurrentAssetsValue;
        p.CapitalInvestment = dto.CapitalInvestment;
        p.InstitutionAccountNumber = dto.InstitutionAccountNumber.Trim();

        app.CurrentStep = ApplicationStep.Documents;
        app.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ---------------------------------------------------------------- helpers
    private IQueryable<Application> Owned(int id)
    {
        var aid = ApplicantId;
        return _db.Applications.Where(a => a.Id == id && a.ApplicantId == aid);
    }

    private ObjectResult Locked(Application app) =>
        StatusCode(StatusCodes.Status409Conflict,
            new { message = $"This section is already saved and locked. Current step: {app.CurrentStep}." });

    internal static ApplicationDetailDto Map(Application a) => new()
    {
        Id = a.Id,
        ApplicationNumber = a.ApplicationNumber,
        CurrentStep = a.CurrentStep,
        Status = a.Status,
        IsShortForm = a.IsShortForm,
        CourseName = a.Course?.Name ?? (a.AppliedFor == AppliedFor.SeatEnhancement ? "Seat Enhancement" : ""),
        SocietyName = a.Applicant?.SocietyName ?? "",
        ProposedInstituteName = a.Applicant?.ProposedInstituteName ?? "",
        DistrictName = a.Applicant?.District?.Name ?? "",
        Address = a.Applicant?.Address ?? "",
        CourseDetails = new CourseDetailsDto
        {
            AppliedFor = a.AppliedFor,
            CouncilId = a.CouncilId,
            CourseType = a.CourseType,
            CourseId = a.CourseId,
            NoOfExistingSeats = a.NoOfExistingSeats,
            NoOfSeatsApplied = a.NoOfSeatsApplied
        },
        Part1 = a.Part1 is null ? null : new FormPart1Dto
        {
            NoOfSeatsApplied = a.NoOfSeatsApplied,
            NoOfExistingSeats = a.NoOfExistingSeats,
            NatureOfInstitution = a.Part1.NatureOfInstitution,
            WhereRegistered = a.Part1.WhereRegistered,
            RegistrationDate = a.Part1.RegistrationDate,
            RegistrationNumber = a.Part1.RegistrationNumber,
            TotalLandInUpHectare = a.Part1.TotalLandInUpHectare,
            HeadNameAndMobile = a.Part1.HeadNameAndMobile,
            ExecutivePersonNameAndMobile = a.Part1.ExecutivePersonNameAndMobile,
            TrainingCenterName = a.Part1.TrainingCenterName,
            TrainingCenterAddress = a.Part1.TrainingCenterAddress,
            CurrentAdmissionCapacity = a.Part1.CurrentAdmissionCapacity,
            EnhancedAdmissionCapacity = a.Part1.EnhancedAdmissionCapacity,
            RecognitionOrderNumberAndDate = a.Part1.RecognitionOrderNumberAndDate,
            OtherOngoingTrainings = a.Part1.OtherOngoingTrainings
        },
        Part2 = a.Part2 is null ? null : new FormPart2Dto
        {
            TeachingLandOwnerName = a.Part2.TeachingLandOwnerName,
            TeachingKhasraPlotNo = a.Part2.TeachingKhasraPlotNo,
            LandAreaSqft = a.Part2.LandAreaSqft,
            TeachingBlockBuiltAreaSqft = a.Part2.TeachingBlockBuiltAreaSqft,
            TeachingFireRegNo = a.Part2.TeachingFireRegNo,
            TeachingFireValidity = a.Part2.TeachingFireValidity,
            HostelBlockBuiltAreaSqft = a.Part2.HostelBlockBuiltAreaSqft,
            HostelKhasraPlotNo = a.Part2.HostelKhasraPlotNo,
            HostelFireRegNo = a.Part2.HostelFireRegNo,
            HostelFireValidity = a.Part2.HostelFireValidity,
            HospitalLandOwnerName = a.Part2.HospitalLandOwnerName,
            HospitalKhasraPlotNo = a.Part2.HospitalKhasraPlotNo,
            HospitalName = a.Part2.HospitalName,
            HospitalAddress = a.Part2.HospitalAddress,
            HospitalTotalBeds = a.Part2.HospitalTotalBeds,
            HospitalBedsAtPreviousApplication = a.Part2.HospitalBedsAtPreviousApplication,
            CmoRegNo = a.Part2.CmoRegNo,
            CmoValidity = a.Part2.CmoValidity,
            CmoBedCount = a.Part2.CmoBedCount,
            PcbRegNo = a.Part2.PcbRegNo,
            PcbValidity = a.Part2.PcbValidity,
            PcbBedCount = a.Part2.PcbBedCount,
            HospitalFireRegNo = a.Part2.HospitalFireRegNo,
            HospitalFireValidity = a.Part2.HospitalFireValidity,
            IsHospitalEmpanelledPmjay = a.Part2.IsHospitalEmpanelledPmjay,
            Deposit = a.Part2.Deposit,
            FixedAssetsValue = a.Part2.FixedAssetsValue,
            CurrentAssetsValue = a.Part2.CurrentAssetsValue,
            CapitalInvestment = a.Part2.CapitalInvestment,
            InstitutionAccountNumber = a.Part2.InstitutionAccountNumber
        },
        Documents = a.Documents.Select(d => new DocumentDto
        {
            DocumentType = d.DocumentType,
            OriginalFileName = d.OriginalFileName,
            SizeBytes = d.SizeBytes,
            UploadedAtUtc = d.UploadedAtUtc
        }).ToList(),
        Payment = a.Payment is null
            ? (a.IsShortForm ? new PaymentDto { BaseFee = 0m, GstPercent = 0m } : new PaymentDto())
            : new PaymentDto
            {
                BaseFee = a.Payment.BaseFee,
                GstPercent = a.Payment.GstPercent,
                Status = a.Payment.Status,
                TransactionId = a.Payment.TransactionId,
                PaidAtUtc = a.Payment.PaidAtUtc
            }
    };

    // ---- snapshot-copy helpers for short (dashboard) applications ----
    private static FormPart1 CopyPart1(FormPart1 s) => new()
    {
        NatureOfInstitution = s.NatureOfInstitution,
        WhereRegistered = s.WhereRegistered,
        RegistrationDate = s.RegistrationDate,
        RegistrationNumber = s.RegistrationNumber,
        TotalLandInUpHectare = s.TotalLandInUpHectare,
        HeadNameAndMobile = s.HeadNameAndMobile,
        ExecutivePersonNameAndMobile = s.ExecutivePersonNameAndMobile,
        TrainingCenterName = s.TrainingCenterName,
        TrainingCenterAddress = s.TrainingCenterAddress,
        CurrentAdmissionCapacity = s.CurrentAdmissionCapacity,
        EnhancedAdmissionCapacity = s.EnhancedAdmissionCapacity,
        RecognitionOrderNumberAndDate = s.RecognitionOrderNumberAndDate,
        OtherOngoingTrainings = s.OtherOngoingTrainings
    };

    private static FormPart2 CopyPart2(FormPart2 s) => new()
    {
        TeachingLandOwnerName = s.TeachingLandOwnerName,
        TeachingKhasraPlotNo = s.TeachingKhasraPlotNo,
        LandAreaSqft = s.LandAreaSqft,
        TeachingBlockBuiltAreaSqft = s.TeachingBlockBuiltAreaSqft,
        TeachingFireRegNo = s.TeachingFireRegNo,
        TeachingFireValidity = s.TeachingFireValidity,
        HostelBlockBuiltAreaSqft = s.HostelBlockBuiltAreaSqft,
        HostelKhasraPlotNo = s.HostelKhasraPlotNo,
        HostelFireRegNo = s.HostelFireRegNo,
        HostelFireValidity = s.HostelFireValidity,
        HospitalLandOwnerName = s.HospitalLandOwnerName,
        HospitalKhasraPlotNo = s.HospitalKhasraPlotNo,
        HospitalName = s.HospitalName,
        HospitalAddress = s.HospitalAddress,
        HospitalTotalBeds = s.HospitalTotalBeds,
        HospitalBedsAtPreviousApplication = s.HospitalBedsAtPreviousApplication,
        CmoRegNo = s.CmoRegNo,
        CmoValidity = s.CmoValidity,
        CmoBedCount = s.CmoBedCount,
        PcbRegNo = s.PcbRegNo,
        PcbValidity = s.PcbValidity,
        PcbBedCount = s.PcbBedCount,
        HospitalFireRegNo = s.HospitalFireRegNo,
        HospitalFireValidity = s.HospitalFireValidity,
        IsHospitalEmpanelledPmjay = s.IsHospitalEmpanelledPmjay,
        Deposit = s.Deposit,
        FixedAssetsValue = s.FixedAssetsValue,
        CurrentAssetsValue = s.CurrentAssetsValue,
        CapitalInvestment = s.CapitalInvestment,
        InstitutionAccountNumber = s.InstitutionAccountNumber
    };

    private static ApplicationDocument CopyDocument(ApplicationDocument s) => new()
    {
        DocumentType = s.DocumentType,
        OriginalFileName = s.OriginalFileName,
        StoredPath = s.StoredPath,        // reuse the same stored file (read-only)
        ContentType = s.ContentType,
        SizeBytes = s.SizeBytes,
        UploadedAtUtc = s.UploadedAtUtc
    };
}
