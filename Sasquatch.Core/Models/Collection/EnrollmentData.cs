using Sasquatch.Core.Models.Shared;

namespace Sasquatch.Core.Models.Collection;

/// <summary>
/// Enrollment detail data - one row per school/grade/program
/// </summary>
public class EnrollmentData
{
    public int EnrollmentId { get; set; }
    public int SubmissionId { get; set; }
    public string SchoolCode { get; set; } = string.Empty;
    public string GradeLevel { get; set; } = string.Empty;  // K, 01-12, PK
    public string ProgramType { get; set; } = "BasicEd";  // BasicEd, RunningStart, OpenDoors, ALE, SpecialEd
    public string? ResidentDistrictCode { get; set; }
    public int Headcount { get; set; }
    public decimal FTE { get; set; }
    public int? PriorMonthHeadcount { get; set; }
    public decimal? PriorMonthFTE { get; set; }

    // Computed properties (match SQL computed columns)
    public int HeadcountVariance => Headcount - (PriorMonthHeadcount ?? Headcount);
    public decimal FTEVariance => FTE - (PriorMonthFTE ?? FTE);
    public decimal? HeadcountVariancePct => PriorMonthHeadcount > 0
        ? (decimal)(Headcount - PriorMonthHeadcount.Value) / PriorMonthHeadcount.Value * 100
        : null;

    // Navigation properties
    public EnrollmentSubmission? Submission { get; set; }
    public School? School { get; set; }
}
