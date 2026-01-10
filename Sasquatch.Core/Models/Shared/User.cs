namespace Sasquatch.Core.Models.Shared;

/// <summary>
/// Application user (for demo purposes)
/// </summary>
public class User
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string UserRole { get; set; } = string.Empty;  // District, ESD, OSPI, Legislature
    public string? DistrictCode { get; set; }
    public string? EsdCode { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public District? District { get; set; }
    public Esd? Esd { get; set; }
}
