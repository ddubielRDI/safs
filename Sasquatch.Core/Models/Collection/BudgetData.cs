namespace Sasquatch.Core.Models.Collection;

/// <summary>
/// Budget detail data - one row per fund/program/activity/object
/// </summary>
public class BudgetData
{
    public int BudgetId { get; set; }
    public int SubmissionId { get; set; }
    public string FundCode { get; set; } = string.Empty;  // 10=General, 20=Capital, etc.
    public string? ProgramCode { get; set; }
    public string? ActivityCode { get; set; }
    public string? ObjectCode { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemDescription { get; set; }
    public string FiscalYear { get; set; } = string.Empty;  // e.g., "2024-25"
    public decimal Amount { get; set; }
    public decimal? PriorMonthAmount { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Computed property (match SQL computed column)
    public decimal Variance => Amount - (PriorMonthAmount ?? Amount);
    public decimal? VariancePct => PriorMonthAmount > 0
        ? (Amount - PriorMonthAmount.Value) / PriorMonthAmount.Value * 100
        : null;

    // Navigation properties
    public BudgetSubmission? Submission { get; set; }
}
