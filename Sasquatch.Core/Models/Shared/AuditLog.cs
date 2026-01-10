namespace Sasquatch.Core.Models.Shared;

/// <summary>
/// Audit trail for data changes
/// </summary>
public class AuditLog
{
    public int AuditId { get; set; }
    public string TableName { get; set; } = string.Empty;
    public int RecordId { get; set; }
    public string Action { get; set; } = string.Empty;  // INSERT, UPDATE, DELETE
    public string? FieldName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public DateTime ChangedDate { get; set; } = DateTime.UtcNow;
    public string? Reason { get; set; }
}
