namespace Sasquatch.Core.Models.Shared;

/// <summary>
/// Data lock control for submissions
/// </summary>
public class DataLock
{
    public int LockId { get; set; }
    public string LockScope { get; set; } = string.Empty;  // All, ESD, District
    public string? ScopeValue { get; set; }  // NULL for All, ESD code, or District code
    public string? FormType { get; set; }  // NULL = all forms, or specific form
    public string LockType { get; set; } = string.Empty;  // Monthly, Annual, Audit
    public string? SchoolYear { get; set; }
    public byte? Month { get; set; }
    public string LockedBy { get; set; } = string.Empty;
    public DateTime LockedDate { get; set; } = DateTime.UtcNow;
    public string? UnlockedBy { get; set; }
    public DateTime? UnlockedDate { get; set; }
    public string? Reason { get; set; }
    public bool IsActive { get; set; } = true;
}
