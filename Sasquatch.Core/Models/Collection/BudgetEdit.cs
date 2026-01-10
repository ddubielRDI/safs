using Sasquatch.Core.Models.Shared;

namespace Sasquatch.Core.Models.Collection;

/// <summary>
/// Validation/edit results for budget submissions
/// </summary>
public class BudgetEdit
{
    public int EditId { get; set; }
    public int SubmissionId { get; set; }
    public string EditRuleId { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;  // Error, Warning, Info
    public string Message { get; set; } = string.Empty;
    public string? FieldName { get; set; }
    public string? ExpectedValue { get; set; }
    public string? ActualValue { get; set; }
    public string? DistrictComment { get; set; }
    public bool IsResolved { get; set; }
    public string? ResolvedBy { get; set; }
    public DateTime? ResolvedDate { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public BudgetSubmission? Submission { get; set; }
    public EditRule? EditRule { get; set; }
}
