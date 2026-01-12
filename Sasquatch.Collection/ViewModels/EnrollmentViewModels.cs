using Sasquatch.Core.Models.Shared;
using Sasquatch.Core.Models.Collection;

namespace Sasquatch.Collection.ViewModels;

/// <summary>
/// Dashboard view model showing all submissions for a district
/// </summary>
public class EnrollmentDashboardViewModel : IWorkflowViewModel
{
    public WorkflowTabViewModel Tabs { get; set; } = new();
    public District? District { get; set; }
    public List<EnrollmentSubmissionSummary> Submissions { get; set; } = new();
    public string SchoolYear { get; set; } = "2024-25";
}

/// <summary>
/// Summary of an enrollment submission for list display
/// </summary>
public class EnrollmentSubmissionSummary
{
    public int SubmissionId { get; set; }
    public string SchoolYear { get; set; } = string.Empty;
    public byte Month { get; set; }
    public string MonthName => GetMonthName(Month);
    public string SubmissionStatus { get; set; } = string.Empty;
    public int TotalHeadcount { get; set; }
    public decimal TotalFTE { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public bool IsLocked { get; set; }

    private static string GetMonthName(byte month) => month switch
    {
        1 => "September",
        2 => "October",
        3 => "November",
        4 => "December",
        5 => "January",
        6 => "February",
        7 => "March",
        8 => "April",
        9 => "May",
        10 => "June",
        11 => "July",
        12 => "August",
        _ => "Unknown"
    };
}

/// <summary>
/// View model for enrollment submission detail/edit
/// </summary>
public class EnrollmentSubmissionViewModel : IWorkflowViewModel
{
    public WorkflowTabViewModel Tabs { get; set; } = new();
    public EnrollmentSubmission Submission { get; set; } = new();
    public List<EnrollmentDataRow> DataRows { get; set; } = new();
    public List<EnrollmentEdit> Edits { get; set; } = new();
    public List<School> Schools { get; set; } = new();
    public bool CanEdit { get; set; }
    public bool CanSubmit { get; set; }
}

/// <summary>
/// Enrollment data row with school info for grid display
/// </summary>
public class EnrollmentDataRow
{
    public int EnrollmentId { get; set; }
    public string SchoolCode { get; set; } = string.Empty;
    public string SchoolName { get; set; } = string.Empty;
    public string SchoolType { get; set; } = string.Empty;
    public string GradeLevel { get; set; } = string.Empty;
    public string ProgramType { get; set; } = string.Empty;
    public int Headcount { get; set; }
    public decimal FTE { get; set; }
    public int? PriorMonthHeadcount { get; set; }
    public decimal? PriorMonthFTE { get; set; }
    public int HeadcountVariance => Headcount - (PriorMonthHeadcount ?? Headcount);
    public decimal FTEVariance => FTE - (PriorMonthFTE ?? FTE);
    public decimal? HeadcountVariancePct => PriorMonthHeadcount > 0
        ? (decimal)(Headcount - PriorMonthHeadcount.Value) / PriorMonthHeadcount.Value * 100
        : null;
    public bool HasWarning { get; set; }
    public bool HasError { get; set; }
}

/// <summary>
/// View model for file upload result
/// </summary>
public class EnrollmentUploadResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int RecordsProcessed { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public int? SubmissionId { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// OSPI Admin dashboard view model
/// </summary>
public class OspiDashboardViewModel
{
    public List<DistrictSubmissionStatus> DistrictStatuses { get; set; } = new();
    public List<Esd> ESDs { get; set; } = new();
    public string? SelectedEsdCode { get; set; }
    public string SchoolYear { get; set; } = "2024-25";
    public byte? SelectedMonth { get; set; }
    public int TotalDistricts { get; set; }
    public int SubmittedCount { get; set; }
    public int ApprovedCount { get; set; }
    public int PendingCount { get; set; }

    // State-level aggregate data (SAFS-002 compliance)
    public int StatewideHeadcount { get; set; }
    public decimal StatewideFTE { get; set; }
    public int StatewideSchoolCount { get; set; }
    public int StatewideSubmissionCount { get; set; }
}

/// <summary>
/// District submission status for OSPI dashboard
/// </summary>
public class DistrictSubmissionStatus
{
    public string DistrictCode { get; set; } = string.Empty;
    public string DistrictName { get; set; } = string.Empty;
    public string EsdCode { get; set; } = string.Empty;
    public string EsdName { get; set; } = string.Empty;
    public string EnrollmentStatus { get; set; } = "Not Started";
    public string BudgetStatus { get; set; } = "Not Started";
    public bool IsLocked { get; set; }
    public DateTime? LastSubmission { get; set; }
}

/// <summary>
/// View model for bulk import page
/// </summary>
public class BulkImportViewModel : IWorkflowViewModel
{
    public WorkflowTabViewModel Tabs { get; set; } = new();
    public string SchoolYear { get; set; } = "2024-25";
    public List<(byte Month, string Name)> AvailableMonths { get; set; } = new()
    {
        (1, "September"),
        (2, "October"),
        (3, "November"),
        (4, "December"),
        (5, "January"),
        (6, "February"),
        (7, "March"),
        (8, "April"),
        (9, "May"),
        (10, "June")
    };
}

/// <summary>
/// Result of bulk import operation
/// </summary>
public class BulkImportResultViewModel : IWorkflowViewModel
{
    public WorkflowTabViewModel Tabs { get; set; } = new();
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string SchoolYear { get; set; } = string.Empty;
    public byte Month { get; set; }
    public string MonthName => Month switch
    {
        1 => "September", 2 => "October", 3 => "November", 4 => "December",
        5 => "January", 6 => "February", 7 => "March", 8 => "April",
        9 => "May", 10 => "June", 11 => "July", 12 => "August", _ => "Unknown"
    };

    public int DistrictsCreated { get; set; }
    public int SchoolsCreated { get; set; }
    public int SubmissionsCreated { get; set; }
    public int RecordsCreated { get; set; }
    public int DistrictsFailed { get; set; }

    public TimeSpan Duration { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<DistrictImportResult> DistrictResults { get; set; } = new();
}

/// <summary>
/// Result for a single district in bulk import
/// </summary>
public class DistrictImportResult
{
    public string DistrictCode { get; set; } = string.Empty;
    public string DistrictName { get; set; } = string.Empty;
    public int? SubmissionId { get; set; }
    public bool Success { get; set; }
    public string? Error { get; set; }
    public int SchoolCount { get; set; }
    public int RecordCount { get; set; }
    public int WarningCount { get; set; }
    public int ErrorCount { get; set; }
}
