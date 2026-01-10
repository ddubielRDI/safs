using Sasquatch.Core.Models.Collection;

namespace Sasquatch.Core.Models.Shared;

/// <summary>
/// School District - CCDDD format (e.g., 34033 for Tumwater)
/// </summary>
public class District
{
    public string DistrictCode { get; set; } = string.Empty;
    public string DistrictName { get; set; } = string.Empty;
    public string CountyCode { get; set; } = string.Empty;
    public string EsdCode { get; set; } = string.Empty;
    public byte Class { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Esd? Esd { get; set; }
    public ICollection<School> Schools { get; set; } = new List<School>();
    public ICollection<EnrollmentSubmission> EnrollmentSubmissions { get; set; } = new List<EnrollmentSubmission>();
    public ICollection<BudgetSubmission> BudgetSubmissions { get; set; } = new List<BudgetSubmission>();
    public ICollection<User> Users { get; set; } = new List<User>();
}
