using Sasquatch.Core.Models.Collection;

namespace Sasquatch.Core.Models.Shared;

/// <summary>
/// School/Building within a district
/// </summary>
public class School
{
    public string SchoolCode { get; set; } = string.Empty;
    public string DistrictCode { get; set; } = string.Empty;
    public string SchoolName { get; set; } = string.Empty;
    public string? SchoolType { get; set; }  // Elementary, Middle, High, Other
    public string? GradeLow { get; set; }
    public string? GradeHigh { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public District? District { get; set; }
    public ICollection<EnrollmentData> EnrollmentData { get; set; } = new List<EnrollmentData>();
}
