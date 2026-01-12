using Microsoft.AspNetCore.Http;
using Sasquatch.Core.Models.Collection;

namespace Sasquatch.Collection.Services;

/// <summary>
/// Interface for parsing enrollment data files (xlsx, csv)
/// </summary>
public interface IEnrollmentFileParser
{
    /// <summary>
    /// Parse an enrollment file and return EnrollmentData records
    /// </summary>
    /// <param name="file">The uploaded file</param>
    /// <param name="submissionId">The submission ID to associate records with</param>
    /// <returns>Parse result with data, warnings, and errors</returns>
    Task<FileParseResult<EnrollmentData>> ParseAsync(IFormFile file, int submissionId);

    /// <summary>
    /// Parse a bulk statewide enrollment file, grouping data by district
    /// </summary>
    /// <param name="file">The uploaded file</param>
    /// <returns>Bulk parse result grouped by district code</returns>
    Task<BulkParseResult> ParseBulkAsync(IFormFile file);

    /// <summary>
    /// Validate file format without parsing
    /// </summary>
    (bool IsValid, string? Error) ValidateFile(IFormFile file);
}
