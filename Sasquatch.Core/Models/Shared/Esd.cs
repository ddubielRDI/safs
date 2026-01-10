namespace Sasquatch.Core.Models.Shared;

/// <summary>
/// Educational Service District (ESD) - regional administrative units
/// </summary>
public class Esd
{
    public string EsdCode { get; set; } = string.Empty;
    public string EsdName { get; set; } = string.Empty;
    public string? RegionName { get; set; }

    // Navigation properties
    public ICollection<District> Districts { get; set; } = new List<District>();
    public ICollection<User> Users { get; set; } = new List<User>();
}
