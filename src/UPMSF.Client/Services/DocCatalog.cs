using UPMSF.Shared;

namespace UPMSF.Client.Services;

/// <summary>Display labels + ordering for every document slot (shared by the upload step and preview).</summary>
public static class DocCatalog
{
    public record Slot(DocumentType Type, string Label);

    public static readonly Slot[] Slots =
    {
        new(DocumentType.RegistrationCertificate, "Updated Registration Certificate of Institution"),
        new(DocumentType.Bylaws, "Bylaws / Memorandum of Association"),
        new(DocumentType.Resolution, "Resolution / Proposal passed by Society/Trust/Company"),
        new(DocumentType.LandRecords, "Teaching land records"),
        new(DocumentType.TeachingBlockFireNoc, "Teaching Block Fire NOC"),
        new(DocumentType.HostelLandRecords, "Hostel land records"),
        new(DocumentType.HostelBlockFireNoc, "Hostel Block Fire NOC"),
        new(DocumentType.HospitalLandRecords, "Hospital land records"),
        new(DocumentType.CmoRegistrationCert, "CMO Registration Certificate"),
        new(DocumentType.PcbCertificate, "Pollution Control Board (PCB) Certificate"),
        new(DocumentType.HospitalFireNoc, "Hospital Fire NOC"),
        new(DocumentType.InstitutionBalanceSheets, "Institution 2-year Balance Sheets"),
        new(DocumentType.HospitalBalanceSheets, "Hospital 2-year Balance Sheets"),
        new(DocumentType.BirthRegisterRecord, "Birth Registration Register (last 3 months)"),
        new(DocumentType.AncRegisterRecord, "ANC Register (last 3 months)"),
        new(DocumentType.OpdIpdRecord, "OPD/IPD Record (last 3 months)"),
        new(DocumentType.TeachingBlockPhoto, "Teaching Block Photograph (Front/Back/Side)"),
        new(DocumentType.HostelBlockPhoto, "Hostel Block Photograph (Front/Back/Side)"),
        new(DocumentType.HospitalPhoto, "Hospital Photograph (Front/Back/Side)"),
        new(DocumentType.HospitalStaffDetails, "Hospital Staff Details (Doctors/Nurses/Paramedics)"),
        new(DocumentType.AffidavitNotary, "Affidavit on stamp paper attested by a Notary"),
    };

    public static string Label(DocumentType t) =>
        Array.Find(Slots, s => s.Type == t)?.Label ?? t.ToString();
}
