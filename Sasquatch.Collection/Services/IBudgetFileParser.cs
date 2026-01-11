using Microsoft.AspNetCore.Http;
using Sasquatch.Core.Models.Collection;

namespace Sasquatch.Collection.Services;

/// <summary>
/// Interface for parsing budget data files (xlsx, csv)
/// </summary>
public interface IBudgetFileParser
{
    /// <summary>
    /// Parse a budget file and return BudgetData records (all fiscal years)
    /// </summary>
    /// <param name="file">The uploaded file</param>
    /// <param name="submissionId">The submission ID to associate records with</param>
    /// <returns>Parse result with data, warnings, and errors</returns>
    Task<FileParseResult<BudgetData>> ParseAsync(IFormFile file, int submissionId);

    /// <summary>
    /// Validate file format without parsing
    /// </summary>
    (bool IsValid, string? Error) ValidateFile(IFormFile file);
}
