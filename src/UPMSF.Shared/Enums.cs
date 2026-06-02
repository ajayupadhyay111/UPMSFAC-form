namespace UPMSF.Shared;

public enum ApplicantType
{
    Society = 1,
    Trust = 2,
    Company = 3
}

/// <summary>What the applicant is applying for in a given application.</summary>
public enum AppliedFor
{
    NewCourse = 1,
    SeatEnhancement = 2
}

/// <summary>Course category. Maps to the columns of the seeded course list.</summary>
public enum CourseType
{
    Diploma = 1,
    Degree = 2,   // Degree (UG)
    Masters = 3   // Degree (PG)
}

/// <summary>
/// The wizard step an application is currently on. Used to resume drafts and to
/// lock previously-saved sections in the UI.
/// </summary>
public enum ApplicationStep
{
    CourseDetails = 1,
    FormPart1 = 2,
    FormPart2 = 3,
    Documents = 4,
    Payment = 5,
    Submitted = 6
}

public enum ApplicationStatus
{
    Draft = 1,
    PaymentPending = 2,
    Submitted = 3
}

public enum PaymentStatus
{
    Pending = 0,
    Success = 1,
    Failed = 2
}
