using Sasquatch.Core.Models.Shared;

namespace Sasquatch.Core.Interfaces.Shared;

/// <summary>
/// Interface for audit logging operations.
/// Shared across all sections.
/// </summary>
public interface IAuditService
{
    /// <summary>Log an audit entry</summary>
    Task LogAsync(string tableName, int recordId, string action, string? fieldName,
                  string? oldValue, string? newValue, string changedBy, string? reason = null);

    /// <summary>Log multiple changes for a single record</summary>
    Task LogChangesAsync(string tableName, int recordId, string action,
                         Dictionary<string, (string? OldValue, string? NewValue)> changes,
                         string changedBy, string? reason = null);

    /// <summary>Get audit history for a record</summary>
    Task<IEnumerable<AuditLog>> GetHistoryAsync(string tableName, int recordId);

    /// <summary>Get recent audit entries with optional filtering</summary>
    Task<IEnumerable<AuditLog>> GetRecentAsync(string? tableName = null,
                                                 string? changedBy = null,
                                                 DateTime? fromDate = null,
                                                 DateTime? toDate = null,
                                                 int limit = 100);
}

/// <summary>
/// Interface for district/reference data access.
/// Shared across all sections.
/// </summary>
public interface IReferenceDataService
{
    /// <summary>Get all ESDs</summary>
    Task<IEnumerable<Esd>> GetEsdsAsync();

    /// <summary>Get districts, optionally filtered by ESD</summary>
    Task<IEnumerable<District>> GetDistrictsAsync(string? esdCode = null);

    /// <summary>Get a specific district with schools</summary>
    Task<District?> GetDistrictAsync(string districtCode);

    /// <summary>Get schools for a district</summary>
    Task<IEnumerable<School>> GetSchoolsAsync(string districtCode);

    /// <summary>Get a specific school</summary>
    Task<School?> GetSchoolAsync(string schoolCode);
}

/// <summary>
/// Interface for user management (demo purposes).
/// Shared across all sections.
/// </summary>
public interface IUserService
{
    /// <summary>Get current user context</summary>
    Task<User?> GetCurrentUserAsync();

    /// <summary>Get user by username</summary>
    Task<User?> GetUserAsync(string username);

    /// <summary>Get users for a district</summary>
    Task<IEnumerable<User>> GetDistrictUsersAsync(string districtCode);

    /// <summary>Check if user has a specific role</summary>
    Task<bool> HasRoleAsync(string username, string role);

    /// <summary>Check if user can access a district</summary>
    Task<bool> CanAccessDistrictAsync(string username, string districtCode);
}
