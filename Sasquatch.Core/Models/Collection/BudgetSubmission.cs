using Sasquatch.Core.Models.Shared;

namespace Sasquatch.Core.Models.Collection;

/// <summary>
/// Budget submission header (F-195/F-200) - one per district/year/form
/// </summary>
public class BudgetSubmission
{
    public int SubmissionId { get; set; }
    public string DistrictCode { get; set; } = string.Empty;
    public string FiscalYear { get; set; } = string.Empty;  // '2024-25'
    public string FormType { get; set; } = string.Empty;  // F-195, F-200
    public string SubmissionStatus { get; set; } = "Draft";  // Draft, Submitted, Approved, Locked
    public string? SubmittedBy { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public bool IsLocked { get; set; }
    public string? LockedBy { get; set; }
    public DateTime? LockedDate { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedDate { get; set; }

    // Navigation properties
    public District? District { get; set; }
    public ICollection<BudgetData> BudgetData { get; set; } = new List<BudgetData>();
    public ICollection<BudgetEdit> BudgetEdits { get; set; } = new List<BudgetEdit>();
}
