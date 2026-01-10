using Sasquatch.Core.Models.Shared;

namespace Sasquatch.Core.Models.Collection;

/// <summary>
/// Enrollment submission header (P-223) - one per district/month
/// </summary>
public class EnrollmentSubmission
{
    public int SubmissionId { get; set; }
    public string DistrictCode { get; set; } = string.Empty;
    public string SchoolYear { get; set; } = string.Empty;  // '2024-25'
    public byte Month { get; set; }  // 1=Sept, 12=Aug
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
    public ICollection<EnrollmentData> EnrollmentData { get; set; } = new List<EnrollmentData>();
    public ICollection<EnrollmentEdit> EnrollmentEdits { get; set; } = new List<EnrollmentEdit>();
}
