using Sasquatch.Core.Models.Shared;
using Sasquatch.Core.Models.Collection;

namespace Sasquatch.Collection.ViewModels;

/// <summary>
/// Dashboard view model showing all budget submissions for a district
/// </summary>
public class BudgetDashboardViewModel : IWorkflowViewModel
{
    public WorkflowTabViewModel Tabs { get; set; } = new();
    public District? District { get; set; }
    public List<BudgetSubmissionSummary> Submissions { get; set; } = new();
    public string SchoolYear { get; set; } = "2024-25";
}

/// <summary>
/// Summary of a budget submission for list display
/// </summary>
public class BudgetSubmissionSummary
{
    public int SubmissionId { get; set; }
    public string SchoolYear { get; set; } = string.Empty;
    public string BudgetType { get; set; } = "Original"; // Original, Revised, Final
    public string SubmissionStatus { get; set; } = "Draft";
    public decimal TotalRevenues { get; set; }
    public decimal TotalExpenditures { get; set; }
    public decimal FundBalance { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public bool IsLocked { get; set; }

    public decimal Variance => TotalRevenues - TotalExpenditures;
    public bool IsBalanced => Math.Abs(Variance) < 0.01m;
}

/// <summary>
/// View model for budget submission detail/edit
/// </summary>
public class BudgetSubmissionViewModel : IWorkflowViewModel
{
    public WorkflowTabViewModel Tabs { get; set; } = new();
    public BudgetSubmission Submission { get; set; } = new();
    public List<BudgetDataRow> DataRows { get; set; } = new();
    public List<BudgetEdit> Edits { get; set; } = new();
    public bool CanEdit { get; set; }
    public bool CanSubmit { get; set; }

    // Lookup lists for dropdowns
    public List<CodeLookup> FundCodes { get; set; } = new();
    public List<CodeLookup> ProgramCodes { get; set; } = new();
    public List<CodeLookup> ActivityCodes { get; set; } = new();
    public List<CodeLookup> ObjectCodes { get; set; } = new();

    // Summary totals
    public decimal TotalRevenues => DataRows.Where(r => r.IsRevenue).Sum(r => r.Amount);
    public decimal TotalExpenditures => DataRows.Where(r => !r.IsRevenue).Sum(r => r.Amount);
    public decimal FundBalance => TotalRevenues - TotalExpenditures;
}

/// <summary>
/// Budget data row for grid display
/// </summary>
public class BudgetDataRow
{
    public int BudgetDataId { get; set; }
    public string FundCode { get; set; } = string.Empty;
    public string FundName { get; set; } = string.Empty;
    public string ProgramCode { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public string ActivityCode { get; set; } = string.Empty;
    public string ActivityName { get; set; } = string.Empty;
    public string ObjectCode { get; set; } = string.Empty;
    public string ObjectName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal? PriorYearAmount { get; set; }
    public bool IsRevenue { get; set; }
    public bool HasWarning { get; set; }
    public bool HasError { get; set; }

    public decimal Variance => Amount - (PriorYearAmount ?? Amount);
    public decimal? VariancePercent => PriorYearAmount > 0
        ? (Amount - PriorYearAmount.Value) / PriorYearAmount.Value * 100
        : null;
}

/// <summary>
/// Code lookup for dropdowns
/// </summary>
public class CodeLookup
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FullDisplay => $"{Code} - {Name}";
}

/// <summary>
/// View model for budget file upload
/// </summary>
public class BudgetUploadViewModel : IWorkflowViewModel
{
    public WorkflowTabViewModel Tabs { get; set; } = new();
    public string SchoolYear { get; set; } = "2024-25";
    public string DistrictName { get; set; } = string.Empty;
    public List<string> AcceptedFormats { get; set; } = new() { ".csv", ".xlsx", ".xls" };
}

/// <summary>
/// View model for file upload result
/// </summary>
public class BudgetUploadResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int RecordsProcessed { get; set; }
    public int RevenueRecords { get; set; }
    public int ExpenditureRecords { get; set; }
    public decimal TotalRevenues { get; set; }
    public decimal TotalExpenditures { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public int? SubmissionId { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
