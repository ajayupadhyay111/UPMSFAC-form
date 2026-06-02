using Microsoft.EntityFrameworkCore;

namespace UPMSF.Server.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Applicant> Applicants => Set<Applicant>();
    public DbSet<Application> Applications => Set<Application>();
    public DbSet<FormPart1> FormPart1s => Set<FormPart1>();
    public DbSet<FormPart2> FormPart2s => Set<FormPart2>();
    public DbSet<ApplicationDocument> Documents => Set<ApplicationDocument>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<District> Districts => Set<District>();
    public DbSet<Council> Councils => Set<Council>();
    public DbSet<Course> Courses => Set<Course>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // Atomic source of registration numbers (AH9 + value). Safe under concurrency.
        b.HasSequence<int>("RegistrationSeq").StartsAt(1000001).IncrementsBy(1);

        b.Entity<Applicant>(e =>
        {
            e.HasIndex(x => x.RegistrationId).IsUnique();
            e.HasIndex(x => new { x.Phone, x.RegistrationId });
        });

        b.Entity<Application>(e =>
        {
            e.HasIndex(x => x.ApplicationNumber).IsUnique();
            e.HasOne(x => x.Part1).WithOne(p => p.Application).HasForeignKey<FormPart1>(p => p.ApplicationId);
            e.HasOne(x => x.Part2).WithOne(p => p.Application).HasForeignKey<FormPart2>(p => p.ApplicationId);
            e.HasOne(x => x.Payment).WithOne(p => p.Application).HasForeignKey<Payment>(p => p.ApplicationId);
        });

        // decimals
        b.Entity<FormPart1>().Property(x => x.TotalLandInUpHectare).HasPrecision(12, 4);
        foreach (var prop in new[] { "Deposit", "FixedAssetsValue", "CurrentAssetsValue", "CapitalInvestment" })
            b.Entity<FormPart2>().Property(prop).HasPrecision(18, 2);
        foreach (var prop in new[] { "BaseFee", "GstPercent", "GstAmount", "TotalAmount" })
            b.Entity<Payment>().Property(prop).HasPrecision(18, 2);

        // ---- seed lookups ----
        var districts = new List<District>();
        for (int i = 0; i < SeedData.UpDistricts.Length; i++)
            districts.Add(new District { Id = i + 1, Name = SeedData.UpDistricts[i] });
        b.Entity<District>().HasData(districts);

        b.Entity<Council>().HasData(SeedData.Councils.Select(c => new Council { Id = c.CouncilId, Name = c.Name }));

        var courses = new List<Course>();
        for (int i = 0; i < SeedData.Courses.Length; i++)
        {
            var c = SeedData.Courses[i];
            courses.Add(new Course { Id = i + 1, Name = c.Name, ShortCode = c.Short, CourseType = c.Type, CouncilId = 1 });
        }
        b.Entity<Course>().HasData(courses);
    }
}
