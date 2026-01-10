using Sasquatch.Core.Models.Collection;

namespace Sasquatch.Core.Models.Shared;

/// <summary>
/// Edit/Validation rule configuration
/// </summary>
public class EditRule
{
    public string RuleId { get; set; } = string.Empty;  // e.g., "ENR-001"
    public string RuleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FormType { get; set; } = string.Empty;  // P-223, F-195, F-200
    public string Severity { get; set; } = string.Empty;  // Error, Warning, Info
    public string? Formula { get; set; }  // Calculation/comparison logic
    public decimal? Threshold { get; set; }  // Variance threshold (e.g., 10.0 = 10%)
    public bool BlocksSubmission { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<EnrollmentEdit> EnrollmentEdits { get; set; } = new List<EnrollmentEdit>();
    public ICollection<BudgetEdit> BudgetEdits { get; set; } = new List<BudgetEdit>();
}
