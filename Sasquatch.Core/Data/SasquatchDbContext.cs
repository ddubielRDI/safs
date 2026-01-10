using Microsoft.EntityFrameworkCore;
using Sasquatch.Core.Models.Shared;
using Sasquatch.Core.Models.Collection;

namespace Sasquatch.Core.Data;

/// <summary>
/// Entity Framework DbContext for Sasquatch database
/// Shared across all sections for data consistency
/// </summary>
public class SasquatchDbContext : DbContext
{
    public SasquatchDbContext(DbContextOptions<SasquatchDbContext> options) : base(options)
    {
    }

    // ===== SHARED REFERENCE TABLES =====
    public DbSet<Esd> ESDs => Set<Esd>();
    public DbSet<District> Districts => Set<District>();
    public DbSet<School> Schools => Set<School>();
    public DbSet<User> Users => Set<User>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<DataLock> DataLocks => Set<DataLock>();
    public DbSet<EditRule> EditRules => Set<EditRule>();

    // ===== SECTION 1: DATA COLLECTION =====
    public DbSet<EnrollmentSubmission> EnrollmentSubmissions => Set<EnrollmentSubmission>();
    public DbSet<EnrollmentData> EnrollmentData => Set<EnrollmentData>();
    public DbSet<EnrollmentEdit> EnrollmentEdits => Set<EnrollmentEdit>();
    public DbSet<BudgetSubmission> BudgetSubmissions => Set<BudgetSubmission>();
    public DbSet<BudgetData> BudgetData => Set<BudgetData>();
    public DbSet<BudgetEdit> BudgetEdits => Set<BudgetEdit>();

    // ===== SECTION 2: DATA CALCULATION =====
    // TODO: Add StateConstant, Scenario, ApportionmentResult DbSets

    // ===== SECTION 3: DATA REPORTING =====
    // TODO: Add ReportDefinition DbSets

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===== SHARED ENTITIES =====

        // ESD Configuration
        modelBuilder.Entity<Esd>(entity =>
        {
            entity.ToTable("ESDs");
            entity.HasKey(e => e.EsdCode);
            entity.Property(e => e.EsdCode).HasColumnName("ESDCode").HasMaxLength(3).IsFixedLength();
            entity.Property(e => e.EsdName).HasColumnName("ESDName").HasMaxLength(100).IsRequired();
            entity.Property(e => e.RegionName).HasMaxLength(50);
        });

        // District Configuration
        modelBuilder.Entity<District>(entity =>
        {
            entity.ToTable("Districts");
            entity.HasKey(e => e.DistrictCode);
            entity.Property(e => e.DistrictCode).HasMaxLength(5).IsFixedLength();
            entity.Property(e => e.DistrictName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.CountyCode).HasMaxLength(2).IsFixedLength().IsRequired();
            entity.Property(e => e.EsdCode).HasColumnName("ESDCode").HasMaxLength(3).IsFixedLength();

            entity.HasOne(d => d.Esd)
                .WithMany(e => e.Districts)
                .HasForeignKey(d => d.EsdCode);
        });

        // School Configuration
        modelBuilder.Entity<School>(entity =>
        {
            entity.ToTable("Schools");
            entity.HasKey(e => e.SchoolCode);
            entity.Property(e => e.SchoolCode).HasMaxLength(4).IsFixedLength();
            entity.Property(e => e.DistrictCode).HasMaxLength(5).IsFixedLength().IsRequired();
            entity.Property(e => e.SchoolName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.SchoolType).HasMaxLength(20);
            entity.Property(e => e.GradeLow).HasMaxLength(2).IsFixedLength();
            entity.Property(e => e.GradeHigh).HasMaxLength(2).IsFixedLength();

            entity.HasOne(s => s.District)
                .WithMany(d => d.Schools)
                .HasForeignKey(s => s.DistrictCode);
        });

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.Username).HasMaxLength(100).IsRequired();
            entity.Property(e => e.DisplayName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.UserRole).HasMaxLength(20).IsRequired();
            entity.Property(e => e.DistrictCode).HasMaxLength(5).IsFixedLength();
            entity.Property(e => e.EsdCode).HasColumnName("ESDCode").HasMaxLength(3).IsFixedLength();

            entity.HasIndex(e => e.Username).IsUnique();

            entity.HasOne(u => u.District)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DistrictCode);

            entity.HasOne(u => u.Esd)
                .WithMany(e => e.Users)
                .HasForeignKey(u => u.EsdCode);
        });

        // AuditLog Configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLog");
            entity.HasKey(e => e.AuditId);
            entity.Property(e => e.TableName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Action).HasMaxLength(10).IsRequired();
            entity.Property(e => e.FieldName).HasMaxLength(50);
            entity.Property(e => e.ChangedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Reason).HasMaxLength(500);

            entity.HasIndex(e => new { e.TableName, e.RecordId });
            entity.HasIndex(e => e.ChangedDate);
        });

        // DataLock Configuration
        modelBuilder.Entity<DataLock>(entity =>
        {
            entity.ToTable("DataLocks");
            entity.HasKey(e => e.LockId);
            entity.Property(e => e.LockScope).HasMaxLength(20).IsRequired();
            entity.Property(e => e.ScopeValue).HasMaxLength(10);
            entity.Property(e => e.FormType).HasMaxLength(10);
            entity.Property(e => e.LockType).HasMaxLength(20).IsRequired();
            entity.Property(e => e.SchoolYear).HasMaxLength(7);
            entity.Property(e => e.LockedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UnlockedBy).HasMaxLength(100);
            entity.Property(e => e.Reason).HasMaxLength(500);
        });

        // EditRule Configuration
        modelBuilder.Entity<EditRule>(entity =>
        {
            entity.ToTable("EditRules");
            entity.HasKey(e => e.RuleId);
            entity.Property(e => e.RuleId).HasMaxLength(20);
            entity.Property(e => e.RuleName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.FormType).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Severity).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Formula).HasMaxLength(1000);
            entity.Property(e => e.Threshold).HasPrecision(10, 4);
        });

        // ===== SECTION 1: COLLECTION ENTITIES =====

        // EnrollmentSubmission Configuration
        modelBuilder.Entity<EnrollmentSubmission>(entity =>
        {
            entity.ToTable("EnrollmentSubmissions");
            entity.HasKey(e => e.SubmissionId);
            entity.Property(e => e.DistrictCode).HasMaxLength(5).IsFixedLength().IsRequired();
            entity.Property(e => e.SchoolYear).HasMaxLength(7).IsRequired();
            entity.Property(e => e.SubmissionStatus).HasMaxLength(20).IsRequired();
            entity.Property(e => e.SubmittedBy).HasMaxLength(100);
            entity.Property(e => e.ApprovedBy).HasMaxLength(100);
            entity.Property(e => e.LockedBy).HasMaxLength(100);

            entity.HasIndex(e => new { e.DistrictCode, e.SchoolYear, e.Month }).IsUnique();

            entity.HasOne(e => e.District)
                .WithMany(d => d.EnrollmentSubmissions)
                .HasForeignKey(e => e.DistrictCode);
        });

        // EnrollmentData Configuration
        modelBuilder.Entity<EnrollmentData>(entity =>
        {
            entity.ToTable("EnrollmentData");
            entity.HasKey(e => e.EnrollmentId);
            entity.Property(e => e.SchoolCode).HasMaxLength(4).IsFixedLength().IsRequired();
            entity.Property(e => e.GradeLevel).HasMaxLength(2).IsFixedLength().IsRequired();
            entity.Property(e => e.ProgramType).HasMaxLength(30).IsRequired();
            entity.Property(e => e.ResidentDistrictCode).HasMaxLength(5).IsFixedLength();
            entity.Property(e => e.FTE).HasPrecision(10, 2);
            entity.Property(e => e.PriorMonthFTE).HasPrecision(10, 2);

            // Ignore computed properties - these are calculated in C#
            entity.Ignore(e => e.HeadcountVariance);
            entity.Ignore(e => e.FTEVariance);
            entity.Ignore(e => e.HeadcountVariancePct);

            entity.HasOne(e => e.Submission)
                .WithMany(s => s.EnrollmentData)
                .HasForeignKey(e => e.SubmissionId);

            entity.HasOne(e => e.School)
                .WithMany(s => s.EnrollmentData)
                .HasForeignKey(e => e.SchoolCode);
        });

        // EnrollmentEdit Configuration
        modelBuilder.Entity<EnrollmentEdit>(entity =>
        {
            entity.ToTable("EnrollmentEdits");
            entity.HasKey(e => e.EditId);
            entity.Property(e => e.EditRuleId).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Severity).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Message).HasMaxLength(500).IsRequired();
            entity.Property(e => e.FieldName).HasMaxLength(50);
            entity.Property(e => e.FieldValue).HasMaxLength(100);
            entity.Property(e => e.DistrictComment).HasMaxLength(1000);
            entity.Property(e => e.ResolvedBy).HasMaxLength(100);

            entity.HasOne(e => e.Submission)
                .WithMany(s => s.EnrollmentEdits)
                .HasForeignKey(e => e.SubmissionId);

            entity.HasOne(e => e.EditRule)
                .WithMany(r => r.EnrollmentEdits)
                .HasForeignKey(e => e.EditRuleId);
        });

        // BudgetSubmission Configuration
        modelBuilder.Entity<BudgetSubmission>(entity =>
        {
            entity.ToTable("BudgetSubmissions");
            entity.HasKey(e => e.SubmissionId);
            entity.Property(e => e.DistrictCode).HasMaxLength(5).IsFixedLength().IsRequired();
            entity.Property(e => e.FiscalYear).HasMaxLength(7).IsRequired();
            entity.Property(e => e.FormType).HasMaxLength(10).IsRequired();
            entity.Property(e => e.SubmissionStatus).HasMaxLength(20).IsRequired();
            entity.Property(e => e.SubmittedBy).HasMaxLength(100);
            entity.Property(e => e.ApprovedBy).HasMaxLength(100);
            entity.Property(e => e.LockedBy).HasMaxLength(100);

            entity.HasIndex(e => new { e.DistrictCode, e.FiscalYear, e.FormType }).IsUnique();

            entity.HasOne(e => e.District)
                .WithMany(d => d.BudgetSubmissions)
                .HasForeignKey(e => e.DistrictCode);
        });

        // BudgetData Configuration
        modelBuilder.Entity<BudgetData>(entity =>
        {
            entity.ToTable("BudgetData");
            entity.HasKey(e => e.BudgetId);
            entity.Property(e => e.FundCode).HasMaxLength(2).IsFixedLength().IsRequired();
            entity.Property(e => e.ProgramCode).HasMaxLength(2).IsFixedLength();
            entity.Property(e => e.ActivityCode).HasMaxLength(2).IsFixedLength();
            entity.Property(e => e.ObjectCode).HasMaxLength(3).IsFixedLength();
            entity.Property(e => e.ItemCode).HasMaxLength(10);
            entity.Property(e => e.ItemDescription).HasMaxLength(200);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.PriorMonthAmount).HasPrecision(18, 2);

            // Ignore computed properties
            entity.Ignore(e => e.Variance);
            entity.Ignore(e => e.VariancePct);

            entity.HasOne(e => e.Submission)
                .WithMany(s => s.BudgetData)
                .HasForeignKey(e => e.SubmissionId);
        });

        // BudgetEdit Configuration
        modelBuilder.Entity<BudgetEdit>(entity =>
        {
            entity.ToTable("BudgetEdits");
            entity.HasKey(e => e.EditId);
            entity.Property(e => e.EditRuleId).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Severity).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Message).HasMaxLength(500).IsRequired();
            entity.Property(e => e.FieldName).HasMaxLength(50);
            entity.Property(e => e.ExpectedValue).HasMaxLength(100);
            entity.Property(e => e.ActualValue).HasMaxLength(100);
            entity.Property(e => e.DistrictComment).HasMaxLength(1000);
            entity.Property(e => e.ResolvedBy).HasMaxLength(100);

            entity.HasOne(e => e.Submission)
                .WithMany(s => s.BudgetEdits)
                .HasForeignKey(e => e.SubmissionId);

            entity.HasOne(e => e.EditRule)
                .WithMany(r => r.BudgetEdits)
                .HasForeignKey(e => e.EditRuleId);
        });
    }
}
