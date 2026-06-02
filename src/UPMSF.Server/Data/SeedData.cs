using UPMSF.Shared;

namespace UPMSF.Server.Data;

/// <summary>Static seed lists for lookup tables (applied via EF HasData / migrations).</summary>
public static class SeedData
{
    public static readonly string[] UpDistricts =
    {
        "AGRA","ALIGARH","AMBEDKAR NAGAR","AMETHI","AMROHA","AURAIYA","AYODHYA","AZAMGARH",
        "BAGHPAT","BAHRAICH","BALLIA","BALRAMPUR","BANDA","BARABANKI","BAREILLY","BASTI",
        "BHADOHI","BIJNOR","BUDAUN","BULANDSHAHR","CHANDAULI","CHITRAKOOT","DEORIA","ETAH",
        "ETAWAH","FARRUKHABAD","FATEHPUR","FIROZABAD","GAUTAM BUDDHA NAGAR","GHAZIABAD",
        "GHAZIPUR","GONDA","GORAKHPUR","HAMIRPUR","HAPUR","HARDOI","HATHRAS","JALAUN",
        "JAUNPUR","JHANSI","KANNAUJ","KANPUR DEHAT","KANPUR NAGAR","KASGANJ","KAUSHAMBI",
        "KHERI","KUSHINAGAR","LALITPUR","LUCKNOW","MAHARAJGANJ","MAHOBA","MAINPURI",
        "MATHURA","MAU","MEERUT","MIRZAPUR","MORADABAD","MUZAFFARNAGAR","PILIBHIT",
        "PRATAPGARH","PRAYAGRAJ","RAE BARELI","RAMPUR","SAHARANPUR","SAMBHAL","SANT KABIR NAGAR",
        "SHAHJAHANPUR","SHAMLI","SHRAVASTI","SIDDHARTHNAGAR","SITAPUR","SONBHADRA","SULTANPUR",
        "UNNAO","VARANASI"
    };

    public static readonly (string Name, int CouncilId)[] Councils =
    {
        ("Paramedical Council", 1)
    };

    // (Name, ShortCode, CourseType). All under council 1 (Paramedical).
    public static readonly (string Name, string Short, CourseType Type)[] Courses =
    {
        // ---- Diploma ----
        ("Diploma in Anaesthesia and Operation Theatre Technology", "D.AOTT", CourseType.Diploma),
        ("Diploma of Radiotherapy Technology", "D.RT", CourseType.Diploma),
        ("Diploma of Dialysis Technology", "D.DT", CourseType.Diploma),
        ("Diploma of Health Information Management", "D.HIM", CourseType.Diploma),

        // ---- Degree (UG) ----
        ("Bachelor of Medical Laboratory Science", "B.MLS", CourseType.Degree),
        ("Bachelor of Emergency Medical Technologist (Paramedic)", "B.EMT", CourseType.Degree),
        ("Bachelor of Anaesthesia and Operation Theatre Technology", "B.AOTT", CourseType.Degree),
        ("Bachelor of Physiotherapy", "B.PT", CourseType.Degree),
        ("Bachelor of Nutrition and Dietetics (Honours)", "B.ND", CourseType.Degree),
        ("Bachelor of Optometry", "B.OPTOM", CourseType.Degree),
        ("Bachelor of Occupational Therapy", "B.OT", CourseType.Degree),
        ("Bachelor of Psychology", "B.Psy", CourseType.Degree),
        ("Bachelor of Medical and Psychiatric Social Work", "B.MPSW", CourseType.Degree),
        ("Bachelor of Medical Radiology and Imaging Technology", "B.MRIT", CourseType.Degree),
        ("Bachelor of Radiation Therapy Technology", "B.RTT", CourseType.Degree),
        ("Bachelor of Science in Nuclear Medicine Technology", "B.Sc.NMT", CourseType.Degree),
        ("Bachelor of Physician Associates", "B.PA", CourseType.Degree),
        ("Bachelor of Dialysis Therapy Technology", "B.DTT", CourseType.Degree),
        ("Bachelor of Respiratory Technology", "B.RT", CourseType.Degree),
        ("Bachelor of Science in Health Information Management", "B.Sc.HIM", CourseType.Degree),

        // ---- Degree (PG) / Masters ----
        ("Master of Medical Laboratory Science", "M.MLS", CourseType.Masters),
        ("Master of Advanced Care Paramedic", "M.ACP", CourseType.Masters),
        ("Master of Anaesthesia and Operation Theatre Technology", "M.AOTT", CourseType.Masters),
        ("Master of Physiotherapy", "M.PT", CourseType.Masters),
        ("Master of Nutrition and Dietetics (Honours)", "M.ND", CourseType.Masters),
        ("Master of Optometry", "M.OPTOM", CourseType.Masters),
        ("Master of Occupational Therapy", "M.OT", CourseType.Masters),
        ("Master of Medical Social Work / Master of Psychiatric Social Work", "M.MSW/M.PSW", CourseType.Masters),
        ("Master of Medical Radiology and Imaging Technology", "M.MRIT", CourseType.Masters),
        ("Master of Radiation Therapy Technology", "M.RTT", CourseType.Masters),
        ("Master of Science in Nuclear Medicine Technology", "M.Sc.NMT", CourseType.Masters),
        ("Master of Science in Medical Physics", "M.Sc. Medical Physics", CourseType.Masters),
        ("Post Master Diploma in Radiological/Medical Physics or Advanced Master Degree in Radiological/Medical Physics", "PMD.RMP", CourseType.Masters),
        ("Master of Physician Associates", "M.PA", CourseType.Masters),
        ("Master of Dialysis Therapy", "M.DT", CourseType.Masters),
        ("Master of Respiratory Technology", "M.RT", CourseType.Masters),
        ("Master of Science in Health Information Management", "M.Sc.HIM", CourseType.Masters),
    };
}
