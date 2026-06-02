namespace UPMSF.Shared;

/// <summary>
/// Every file slot in the form. Each maps to one upload control. The allowed
/// extension is encoded so client + server validate identically.
/// </summary>
public enum DocumentType
{
    // ---- Part I ----
    RegistrationCertificate = 1,   // pdf - अद्यतन रजिस्ट्रेशन प्रमाण-पत्र
    Bylaws = 2,                    // pdf - बाइलाज/मेमोरेण्डम ऑफ एसोसिएसन
    Resolution = 3,                // pdf - रिजोल्यूशन/प्रस्ताव

    // ---- Part II : Land / Teaching / Hostel ----
    LandRecords = 10,              // zip - भूमि के अभिलेख
    TeachingBlockFireNoc = 11,     // pdf - टीचिंग ब्लाक अग्निशमन प्रमाण पत्र
    HostelLandRecords = 12,        // zip - हास्टल भूमि के अभिलेख
    HostelBlockFireNoc = 13,       // pdf - हास्टल ब्लाक अग्निशमन प्रमाण पत्र

    // ---- Part II : Hospital ----
    HospitalLandRecords = 20,      // zip - चिकित्सालय भूमि के अभिलेख
    CmoRegistrationCert = 21,      // pdf - मुख्य चिकित्सा अधिकारी पंजीकरण प्रमाण पत्र
    PcbCertificate = 22,           // pdf - प्रदूषण नियंत्रण बोर्ड प्रमाण-पत्र
    HospitalFireNoc = 23,          // pdf - हास्पिटल अग्निशमन प्रमाण पत्र

    // ---- Part II : Financial ----
    InstitutionBalanceSheets = 30, // zip - संस्था की दो वर्ष की बैलेन्सशीट
    HospitalBalanceSheets = 31,    // zip - चिकित्सालय की दो वर्ष की बैलेन्सशीट
    BirthRegisterRecord = 32,      // zip - जन्म पंजीकरण रजिस्टर
    AncRegisterRecord = 33,        // zip - ANC Register
    OpdIpdRecord = 34,             // zip - OPD/IPD रिकार्ड

    // ---- Attachments ----
    TeachingBlockPhoto = 40,       // zip
    HostelBlockPhoto = 41,         // zip
    HospitalPhoto = 42,            // zip
    HospitalStaffDetails = 43,     // zip
    AffidavitNotary = 44           // pdf
}

public static class DocumentRules
{
    /// <summary>Allowed file extension (without dot) per document slot.</summary>
    public static string AllowedExtension(DocumentType type) => type switch
    {
        DocumentType.LandRecords or DocumentType.HostelLandRecords or DocumentType.HospitalLandRecords
            or DocumentType.InstitutionBalanceSheets or DocumentType.HospitalBalanceSheets
            or DocumentType.BirthRegisterRecord or DocumentType.AncRegisterRecord or DocumentType.OpdIpdRecord
            or DocumentType.TeachingBlockPhoto or DocumentType.HostelBlockPhoto or DocumentType.HospitalPhoto
            or DocumentType.HospitalStaffDetails => "zip",
        _ => "pdf"
    };

    public const long MaxFileSizeBytes = 25 * 1024 * 1024; // 25 MB per file
}

public class DocumentDto
{
    public DocumentType DocumentType { get; set; }
    public string OriginalFileName { get; set; } = "";
    public long SizeBytes { get; set; }
    public DateTime UploadedAtUtc { get; set; }
}
