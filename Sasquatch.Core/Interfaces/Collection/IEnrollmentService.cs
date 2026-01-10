using Sasquatch.Core.Models.Collection;
using Sasquatch.Core.Models.Shared;

namespace Sasquatch.Core.Interfaces.Collection;

/// <summary>
/// Interface for enrollment data collection operations.
/// Implemented by Section 1 (Data Collection) vendor.
/// </summary>
public interface IEnrollmentService
{
    /// <summary>Get all submissions for a district</summary>
    Task<IEnumerable<EnrollmentSubmission>> GetSubmissionsAsync(string districtCode, string schoolYear);

    /// <summary>Get a specific submission with all related data</summary>
    Task<EnrollmentSubmission?> GetSubmissionAsync(int submissionId);

    /// <summary>Get enrollment data rows for a submission</summary>
    Task<IEnumerable<EnrollmentData>> GetEnrollmentDataAsync(int submissionId);

    /// <summary>Save enrollment data (create or update)</summary>
    Task<EnrollmentData> SaveEnrollmentDataAsync(EnrollmentData data);

    /// <summary>Submit enrollment for OSPI review</summary>
    Task<bool> SubmitForReviewAsync(int submissionId, string submittedBy);

    /// <summary>Process uploaded enrollment file</summary>
    Task<UploadResult> ProcessUploadAsync(Stream fileStream, string districtCode, string schoolYear, byte month);
}

/// <summary>
/// Interface for budget data collection operations.
/// Implemented by Section 1 (Data Collection) vendor.
/// </summary>
public interface IBudgetService
{
    /// <summary>Get all budget submissions for a district</summary>
    Task<IEnumerable<BudgetSubmission>> GetSubmissionsAsync(string districtCode, string fiscalYear);

    /// <summary>Get a specific budget submission with all related data</summary>
    Task<BudgetSubmission?> GetSubmissionAsync(int submissionId);

    /// <summary>Get budget data rows for a submission</summary>
    Task<IEnumerable<BudgetData>> GetBudgetDataAsync(int submissionId);

    /// <summary>Save budget data (create or update)</summary>
    Task<BudgetData> SaveBudgetDataAsync(BudgetData data);

    /// <summary>Submit budget for OSPI review</summary>
    Task<bool> SubmitForReviewAsync(int submissionId, string submittedBy);

    /// <summary>Process uploaded budget file</summary>
    Task<UploadResult> ProcessUploadAsync(Stream fileStream, string districtCode, string fiscalYear, string formType);
}

/// <summary>
/// Interface for validation engine operations.
/// Implemented by Section 1 (Data Collection) vendor.
/// </summary>
public interface IValidationEngine
{
    /// <summary>Run validation rules against enrollment submission</summary>
    Task<IEnumerable<EnrollmentEdit>> ValidateEnrollmentAsync(int submissionId);

    /// <summary>Run validation rules against budget submission</summary>
    Task<IEnumerable<BudgetEdit>> ValidateBudgetAsync(int submissionId);

    /// <summary>Get all active edit rules for a form type</summary>
    Task<IEnumerable<EditRule>> GetEditRulesAsync(string formType);

    /// <summary>Check if submission has blocking errors</summary>
    Task<bool> HasBlockingErrorsAsync(int submissionId, string formType);
}

/// <summary>
/// Interface for data lock management.
/// Implemented by Section 1 (Data Collection) vendor.
/// </summary>
public interface IDataLockService
{
    /// <summary>Get all active locks</summary>
    Task<IEnumerable<DataLock>> GetActiveLocksAsync();

    /// <summary>Check if a district is locked for a specific form/period</summary>
    Task<bool> IsLockedAsync(string districtCode, string? formType, string schoolYear, byte? month);

    /// <summary>Create a new lock</summary>
    Task<DataLock> CreateLockAsync(DataLock lockData);

    /// <summary>Remove a lock</summary>
    Task<bool> RemoveLockAsync(int lockId, string unlockedBy);
}

/// <summary>Result of file upload processing</summary>
public class UploadResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int RecordsProcessed { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
