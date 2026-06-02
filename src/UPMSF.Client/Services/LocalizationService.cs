namespace UPMSF.Client.Services;

public enum Lang { En, Hi }

/// <summary>
/// Bilingual labels for the form. The UI can toggle Hindi/English, but all values
/// the user enters are stored in English only (see the red note on the form).
/// </summary>
public class LocalizationService
{
    public Lang Current { get; private set; } = Lang.En;
    public event Action? OnChange;

    public void Toggle()
    {
        Current = Current == Lang.En ? Lang.Hi : Lang.En;
        OnChange?.Invoke();
    }

    public string this[string key] => Get(key);

    public string Get(string key)
    {
        if (Labels.TryGetValue(key, out var pair))
            return Current == Lang.En ? pair.En : pair.Hi;
        return key;
    }

    private static readonly Dictionary<string, (string En, string Hi)> Labels = new()
    {
        // ----- generic -----
        ["english_only_note"] = ("Please fill the form below in English language only.", "कृपया नीचे दिया गया फॉर्म केवल अंग्रेज़ी भाषा में भरें।"),
        ["save_proceed"] = ("Save & Proceed Next", "सहेजें एवं आगे बढ़ें"),
        ["locked"] = ("Saved & locked", "सहेजा गया एवं लॉक"),
        ["required"] = ("This field is required.", "यह फ़ील्ड आवश्यक है।"),

        // ----- applicant details -----
        ["applicant_details"] = ("Applicant's Details", "आवेदक का विवरण"),
        ["applicant_type"] = ("Applicant Type", "आवेदक का प्रकार"),
        ["society_name"] = ("Name of Society/Trust/Company", "सोसाइटी/ट्रस्ट/कम्पनी का नाम"),
        ["proposed_institute"] = ("Proposed Institute Name", "प्रस्तावित संस्थान का नाम"),
        ["address"] = ("Address", "पता"),
        ["district"] = ("District", "जनपद"),
        ["pin_code"] = ("Pin Code", "पिन कोड"),
        ["contact_person"] = ("Contact Person Name", "सम्पर्क व्यक्ति का नाम"),
        ["phone"] = ("Phone", "फ़ोन"),
        ["mobile"] = ("Mobile", "मोबाइल"),
        ["email"] = ("Email", "ईमेल"),

        // ----- course details -----
        ["course_details"] = ("Application for the Course Details", "पाठ्यक्रम विवरण हेतु आवेदन"),
        ["applied_for"] = ("Applied for", "आवेदन हेतु"),
        ["new_course"] = ("New Course", "नया पाठ्यक्रम"),
        ["seat_enhancement"] = ("Seat Enhancement", "सीट वृद्धि"),
        ["council"] = ("Council", "परिषद"),
        ["course_type"] = ("Course Type", "पाठ्यक्रम का प्रकार"),
        ["select_course"] = ("Select Course", "पाठ्यक्रम चुनें"),
        ["existing_seats"] = ("No. of Existing Seats", "वर्तमान सीटों की संख्या"),
        ["enhancement_seats"] = ("No. of Seats Applied for (Enhancement)", "आवेदित सीटों की संख्या (वृद्धि)"),

        // ----- form part I -----
        ["part1_title"] = ("Form Details Part-I (About Society/Trust/Company & Training Center)", "फॉर्म विवरण भाग-I (सोसाइटी/ट्रस्ट/कम्पनी एवं प्रशिक्षण केन्द्र के बारे में)"),
        ["nature_of_institution"] = ("Nature of Institution", "संस्था की प्रकृति"),
        ["where_registered"] = ("Where is the institution registered?", "संस्था कहाँ पंजीकृत है?"),
        ["registration_date"] = ("Date of Registration of Institution", "संस्था के रजिस्ट्रेशन का दिनांक"),
        ["registration_number"] = ("Registration Number", "रजिस्ट्रेशन संख्या"),
        ["total_land_up"] = ("Total land of institution in U.P. (in Hectare)", "संस्था के नाम सम्पूर्ण उ0प्र0 में भूमि (हेक्टेयर में)"),
        ["head_name_mobile"] = ("Name & Mobile of Head of Institution", "संस्था के मुखिया का नाम एवं मोबाइल नं."),
        ["exec_name_mobile"] = ("Name & Mobile of Executive Person", "संस्था के कार्यकारी व्यक्ति का नाम एवं मोबाइल नं."),
        ["training_center"] = ("Training Center Details", "प्रशिक्षण केन्द्र का विवरण"),
        ["training_center_name"] = ("Training Center Name", "प्रशिक्षण केन्द्र का नाम"),
        ["training_center_address"] = ("Training Center Address", "प्रशिक्षण केन्द्र का पता"),
        ["current_capacity"] = ("Current Admission Capacity", "वर्तमान भर्ती क्षमता"),
        ["enhanced_capacity"] = ("Enhanced Admission Capacity", "बढ़ाकर भर्ती क्षमता"),
        ["recognition_order"] = ("Govt. Order No. & Date under which recognition granted", "शासनादेश संख्या व दिनांक जिसके अन्तर्गत मान्यता प्रदान की गयी"),
        ["other_trainings"] = ("Names of other ongoing trainings", "वर्तमान में चलाये जा रहे अन्य प्रशिक्षणों के नाम"),

        // ----- form part II -----
        ["part2_title"] = ("Form Details Part-II", "फॉर्म विवरण भाग-II"),
        ["teaching_land_owner"] = ("Teaching Center land ownership (name)", "प्रशिक्षण केन्द्र की भूमि का स्वामित्व किसके नाम"),
        ["khasra_plot"] = ("Khasra/Plot Number", "खसरा/प्लाट संख्या"),
        ["land_area"] = ("Land area (sqft)", "भूमि का क्षेत्रफल (वर्गफिट)"),
        ["teaching_built_area"] = ("Teaching Block built area (must be 20000 sqft)", "टीचिंग ब्लाक का निर्मित क्षेत्रफल (20000 वर्गफिट)"),
        ["teaching_fire_reg"] = ("Teaching Block Fire NOC - Registration No.", "टीचिंग ब्लाक अग्निशमन - पंजीयन संख्या"),
        ["fire_validity"] = ("Validity Date", "वैधता तिथि"),
        ["teaching_fire_validity"] = ("Teaching Block Fire NOC - Validity Date", "टीचिंग ब्लाक अग्निशमन - वैधता तिथि"),
        ["hostel_built_area"] = ("Hostel Block built area (must be 17500 sqft)", "हास्टल ब्लाक का निर्मित क्षेत्रफल (17500 वर्गफिट)"),
        ["hostel_fire_reg"] = ("Hostel Block Fire NOC - Registration No.", "हास्टल ब्लाक अग्निशमन - पंजीयन संख्या"),
        ["hostel_fire_validity"] = ("Hostel Block Fire NOC - Validity Date", "हास्टल ब्लाक अग्निशमन - वैधता तिथि"),
        ["hospital_land_owner"] = ("Hospital land ownership (name)", "चिकित्सालय की भूमि का स्वामित्व किसके नाम"),
        ["hospital_name"] = ("Hospital Name", "चिकित्सालय का नाम"),
        ["hospital_address"] = ("Hospital Address", "चिकित्सालय का पता"),
        ["hospital_total_beds"] = ("Total beds in Hospital", "चिकित्सालय में कुल बेडों की संख्या"),
        ["hospital_prev_beds"] = ("Beds at time of previous application", "बेडों की संख्या – पूर्व आवेदन के समय"),
        ["cmo_reg"] = ("CMO Registration Certificate - Reg. No.", "मुख्य चिकित्सा अधिकारी पंजीकरण - पंजीयन संख्या"),
        ["cmo_validity"] = ("CMO Certificate - Validity Date", "मुख्य चिकित्सा अधिकारी प्रमाण पत्र - वैधता तिथि"),
        ["cmo_bed"] = ("CMO Certificate - Bed Count", "मुख्य चिकित्सा अधिकारी प्रमाण पत्र - बेड संख्या"),
        ["bed_count"] = ("Bed Count", "बेड संख्या"),
        ["pcb_reg"] = ("PCB Certificate - Reg. No.", "प्रदूषण नियंत्रण बोर्ड - पंजीयन संख्या"),
        ["pcb_validity"] = ("PCB Certificate - Validity Date", "प्रदूषण नियंत्रण बोर्ड - वैधता तिथि"),
        ["pcb_bed"] = ("PCB Certificate - Bed Count", "प्रदूषण नियंत्रण बोर्ड - बेड संख्या"),
        ["hospital_fire_reg"] = ("Hospital Fire NOC - Registration No.", "हास्पिटल अग्निशमन - पंजीयन संख्या"),
        ["hospital_fire_validity"] = ("Hospital Fire NOC - Validity Date", "हास्पिटल अग्निशमन - वैधता तिथि"),
        ["pmjay"] = ("Is the Hospital Empanelled with PMJAY?", "क्या चिकित्सालय PMJAY से सम्बद्ध है?"),
        ["financial_resources"] = ("Financial Resources", "वित्तीय संसाधन"),
        ["deposit"] = ("Deposit", "डिपाजिट"),
        ["fixed_assets"] = ("Value of fixed assets", "स्थायी परिसम्पत्तियों का मूल्य"),
        ["current_assets"] = ("Value of current assets", "अस्थायी परिसम्पत्तियों का मूल्य"),
        ["capital_investment"] = ("Capital investment", "विभिन्न मदों में दर्शाया गया पूंजी निवेश"),
        ["account_number"] = ("Institution Account Number", "संस्था का खाता संख्या"),

        // ----- documents -----
        ["documents_title"] = ("Upload Related Documents", "सम्बंधित दस्तावेज़ अपलोड करें"),
        ["yes"] = ("Yes", "हाँ"),
        ["no"] = ("No", "नहीं"),

        // ----- payment -----
        ["payment_title"] = ("Application Fee & Inspection Fee Payment", "आवेदन शुल्क एवं निरीक्षण शुल्क भुगतान"),
        ["base_fee"] = ("Application & Inspection Fee", "आवेदन एवं निरीक्षण शुल्क"),
        ["gst"] = ("GST", "जी.एस.टी."),
        ["total"] = ("Total Amount", "कुल धनराशि"),
        ["pay_now"] = ("Pay Now", "अभी भुगतान करें"),
        ["final_submit"] = ("Final Submission", "अंतिम प्रस्तुतीकरण"),
    };
}
