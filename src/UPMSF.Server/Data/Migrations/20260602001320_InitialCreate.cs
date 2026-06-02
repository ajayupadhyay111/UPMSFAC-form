using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace UPMSF.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "RegistrationSeq",
                startValue: 1000001L);

            migrationBuilder.CreateTable(
                name: "Councils",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Councils", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ShortCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CourseType = table.Column<int>(type: "int", nullable: false),
                    CouncilId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Districts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Districts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Applicants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RegistrationId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ApplicantType = table.Column<int>(type: "int", nullable: false),
                    SocietyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProposedInstituteName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DistrictId = table.Column<int>(type: "int", nullable: false),
                    PinCode = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    ContactPersonName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Mobile = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CodeHash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApplicationSequence = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applicants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Applicants_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicantId = table.Column<int>(type: "int", nullable: false),
                    ApplicationNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AppliedFor = table.Column<int>(type: "int", nullable: false),
                    CouncilId = table.Column<int>(type: "int", nullable: false),
                    CourseType = table.Column<int>(type: "int", nullable: true),
                    CourseId = table.Column<int>(type: "int", nullable: true),
                    NoOfExistingSeats = table.Column<int>(type: "int", nullable: true),
                    NoOfSeatsApplied = table.Column<int>(type: "int", nullable: true),
                    CurrentStep = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmittedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Applications_Applicants_ApplicantId",
                        column: x => x.ApplicantId,
                        principalTable: "Applicants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Applications_Councils_CouncilId",
                        column: x => x.CouncilId,
                        principalTable: "Councils",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Applications_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    DocumentType = table.Column<int>(type: "int", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    StoredPath = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FormPart1s",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    NatureOfInstitution = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    WhereRegistered = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TotalLandInUpHectare = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: false),
                    HeadNameAndMobile = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ExecutivePersonNameAndMobile = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TrainingCenterName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TrainingCenterAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CurrentAdmissionCapacity = table.Column<int>(type: "int", nullable: false),
                    EnhancedAdmissionCapacity = table.Column<int>(type: "int", nullable: false),
                    RecognitionOrderNumberAndDate = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    OtherOngoingTrainings = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormPart1s", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormPart1s_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FormPart2s",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    TeachingLandOwnerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TeachingKhasraPlotNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LandAreaSqft = table.Column<int>(type: "int", nullable: false),
                    TeachingBlockBuiltAreaSqft = table.Column<int>(type: "int", nullable: false),
                    TeachingFireRegNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TeachingFireValidity = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HostelBlockBuiltAreaSqft = table.Column<int>(type: "int", nullable: false),
                    HostelKhasraPlotNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HostelFireRegNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HostelFireValidity = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HospitalLandOwnerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HospitalKhasraPlotNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HospitalName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HospitalAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    HospitalTotalBeds = table.Column<int>(type: "int", nullable: false),
                    HospitalBedsAtPreviousApplication = table.Column<int>(type: "int", nullable: true),
                    CmoRegNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CmoValidity = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CmoBedCount = table.Column<int>(type: "int", nullable: true),
                    PcbRegNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PcbValidity = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PcbBedCount = table.Column<int>(type: "int", nullable: true),
                    HospitalFireRegNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HospitalFireValidity = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsHospitalEmpanelledPmjay = table.Column<bool>(type: "bit", nullable: false),
                    Deposit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FixedAssetsValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrentAssetsValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CapitalInvestment = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    InstitutionAccountNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormPart2s", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormPart2s_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    BaseFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GstPercent = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GstAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TransactionId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PaidAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Councils",
                columns: new[] { "Id", "Name" },
                values: new object[] { 1, "Paramedical Council" });

            migrationBuilder.InsertData(
                table: "Courses",
                columns: new[] { "Id", "CouncilId", "CourseType", "Name", "ShortCode" },
                values: new object[,]
                {
                    { 1, 1, 1, "Diploma in Anaesthesia and Operation Theatre Technology", "D.AOTT" },
                    { 2, 1, 1, "Diploma of Radiotherapy Technology", "D.RT" },
                    { 3, 1, 1, "Diploma of Dialysis Technology", "D.DT" },
                    { 4, 1, 1, "Diploma of Health Information Management", "D.HIM" },
                    { 5, 1, 2, "Bachelor of Medical Laboratory Science", "B.MLS" },
                    { 6, 1, 2, "Bachelor of Emergency Medical Technologist (Paramedic)", "B.EMT" },
                    { 7, 1, 2, "Bachelor of Anaesthesia and Operation Theatre Technology", "B.AOTT" },
                    { 8, 1, 2, "Bachelor of Physiotherapy", "B.PT" },
                    { 9, 1, 2, "Bachelor of Nutrition and Dietetics (Honours)", "B.ND" },
                    { 10, 1, 2, "Bachelor of Optometry", "B.OPTOM" },
                    { 11, 1, 2, "Bachelor of Occupational Therapy", "B.OT" },
                    { 12, 1, 2, "Bachelor of Psychology", "B.Psy" },
                    { 13, 1, 2, "Bachelor of Medical and Psychiatric Social Work", "B.MPSW" },
                    { 14, 1, 2, "Bachelor of Medical Radiology and Imaging Technology", "B.MRIT" },
                    { 15, 1, 2, "Bachelor of Radiation Therapy Technology", "B.RTT" },
                    { 16, 1, 2, "Bachelor of Science in Nuclear Medicine Technology", "B.Sc.NMT" },
                    { 17, 1, 2, "Bachelor of Physician Associates", "B.PA" },
                    { 18, 1, 2, "Bachelor of Dialysis Therapy Technology", "B.DTT" },
                    { 19, 1, 2, "Bachelor of Respiratory Technology", "B.RT" },
                    { 20, 1, 2, "Bachelor of Science in Health Information Management", "B.Sc.HIM" },
                    { 21, 1, 3, "Master of Medical Laboratory Science", "M.MLS" },
                    { 22, 1, 3, "Master of Advanced Care Paramedic", "M.ACP" },
                    { 23, 1, 3, "Master of Anaesthesia and Operation Theatre Technology", "M.AOTT" },
                    { 24, 1, 3, "Master of Physiotherapy", "M.PT" },
                    { 25, 1, 3, "Master of Nutrition and Dietetics (Honours)", "M.ND" },
                    { 26, 1, 3, "Master of Optometry", "M.OPTOM" },
                    { 27, 1, 3, "Master of Occupational Therapy", "M.OT" },
                    { 28, 1, 3, "Master of Medical Social Work / Master of Psychiatric Social Work", "M.MSW/M.PSW" },
                    { 29, 1, 3, "Master of Medical Radiology and Imaging Technology", "M.MRIT" },
                    { 30, 1, 3, "Master of Radiation Therapy Technology", "M.RTT" },
                    { 31, 1, 3, "Master of Science in Nuclear Medicine Technology", "M.Sc.NMT" },
                    { 32, 1, 3, "Master of Science in Medical Physics", "M.Sc. Medical Physics" },
                    { 33, 1, 3, "Post Master Diploma in Radiological/Medical Physics or Advanced Master Degree in Radiological/Medical Physics", "PMD.RMP" },
                    { 34, 1, 3, "Master of Physician Associates", "M.PA" },
                    { 35, 1, 3, "Master of Dialysis Therapy", "M.DT" },
                    { 36, 1, 3, "Master of Respiratory Technology", "M.RT" },
                    { 37, 1, 3, "Master of Science in Health Information Management", "M.Sc.HIM" }
                });

            migrationBuilder.InsertData(
                table: "Districts",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "AGRA" },
                    { 2, "ALIGARH" },
                    { 3, "AMBEDKAR NAGAR" },
                    { 4, "AMETHI" },
                    { 5, "AMROHA" },
                    { 6, "AURAIYA" },
                    { 7, "AYODHYA" },
                    { 8, "AZAMGARH" },
                    { 9, "BAGHPAT" },
                    { 10, "BAHRAICH" },
                    { 11, "BALLIA" },
                    { 12, "BALRAMPUR" },
                    { 13, "BANDA" },
                    { 14, "BARABANKI" },
                    { 15, "BAREILLY" },
                    { 16, "BASTI" },
                    { 17, "BHADOHI" },
                    { 18, "BIJNOR" },
                    { 19, "BUDAUN" },
                    { 20, "BULANDSHAHR" },
                    { 21, "CHANDAULI" },
                    { 22, "CHITRAKOOT" },
                    { 23, "DEORIA" },
                    { 24, "ETAH" },
                    { 25, "ETAWAH" },
                    { 26, "FARRUKHABAD" },
                    { 27, "FATEHPUR" },
                    { 28, "FIROZABAD" },
                    { 29, "GAUTAM BUDDHA NAGAR" },
                    { 30, "GHAZIABAD" },
                    { 31, "GHAZIPUR" },
                    { 32, "GONDA" },
                    { 33, "GORAKHPUR" },
                    { 34, "HAMIRPUR" },
                    { 35, "HAPUR" },
                    { 36, "HARDOI" },
                    { 37, "HATHRAS" },
                    { 38, "JALAUN" },
                    { 39, "JAUNPUR" },
                    { 40, "JHANSI" },
                    { 41, "KANNAUJ" },
                    { 42, "KANPUR DEHAT" },
                    { 43, "KANPUR NAGAR" },
                    { 44, "KASGANJ" },
                    { 45, "KAUSHAMBI" },
                    { 46, "KHERI" },
                    { 47, "KUSHINAGAR" },
                    { 48, "LALITPUR" },
                    { 49, "LUCKNOW" },
                    { 50, "MAHARAJGANJ" },
                    { 51, "MAHOBA" },
                    { 52, "MAINPURI" },
                    { 53, "MATHURA" },
                    { 54, "MAU" },
                    { 55, "MEERUT" },
                    { 56, "MIRZAPUR" },
                    { 57, "MORADABAD" },
                    { 58, "MUZAFFARNAGAR" },
                    { 59, "PILIBHIT" },
                    { 60, "PRATAPGARH" },
                    { 61, "PRAYAGRAJ" },
                    { 62, "RAE BARELI" },
                    { 63, "RAMPUR" },
                    { 64, "SAHARANPUR" },
                    { 65, "SAMBHAL" },
                    { 66, "SANT KABIR NAGAR" },
                    { 67, "SHAHJAHANPUR" },
                    { 68, "SHAMLI" },
                    { 69, "SHRAVASTI" },
                    { 70, "SIDDHARTHNAGAR" },
                    { 71, "SITAPUR" },
                    { 72, "SONBHADRA" },
                    { 73, "SULTANPUR" },
                    { 74, "UNNAO" },
                    { 75, "VARANASI" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Applicants_DistrictId",
                table: "Applicants",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Applicants_Phone_RegistrationId",
                table: "Applicants",
                columns: new[] { "Phone", "RegistrationId" });

            migrationBuilder.CreateIndex(
                name: "IX_Applicants_RegistrationId",
                table: "Applicants",
                column: "RegistrationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ApplicantId",
                table: "Applications",
                column: "ApplicantId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ApplicationNumber",
                table: "Applications",
                column: "ApplicationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Applications_CouncilId",
                table: "Applications",
                column: "CouncilId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_CourseId",
                table: "Applications",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ApplicationId",
                table: "Documents",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_FormPart1s_ApplicationId",
                table: "FormPart1s",
                column: "ApplicationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FormPart2s_ApplicationId",
                table: "FormPart2s",
                column: "ApplicationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ApplicationId",
                table: "Payments",
                column: "ApplicationId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "FormPart1s");

            migrationBuilder.DropTable(
                name: "FormPart2s");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Applications");

            migrationBuilder.DropTable(
                name: "Applicants");

            migrationBuilder.DropTable(
                name: "Councils");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "Districts");

            migrationBuilder.DropSequence(
                name: "RegistrationSeq");
        }
    }
}
