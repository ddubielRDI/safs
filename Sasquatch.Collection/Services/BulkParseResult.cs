using Sasquatch.Core.Models.Collection;

namespace Sasquatch.Collection.Services;

/// <summary>
/// Data parsed from one row of the bulk import file
/// </summary>
public class ParsedSchoolRow
{
    public string DistrictCode { get; set; } = string.Empty;
    public string DistrictName { get; set; } = string.Empty;
    public string SchoolCode { get; set; } = string.Empty;
    public string SchoolName { get; set; } = string.Empty;
    public List<EnrollmentData> EnrollmentRecords { get; set; } = new();
}

/// <summary>
/// Result of bulk parsing a statewide enrollment file
/// Groups data by district for processing
/// </summary>
public class BulkParseResult
{
    /// <summary>
    /// Data grouped by district code
    /// Key: CCDDD (district code)
    /// Value: List of parsed school rows for that district
    /// </summary>
    public Dictionary<string, DistrictData> DistrictDataMap { get; set; } = new();

    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();

    public bool Success => !Errors.Any();
    public int DistrictCount => DistrictDataMap.Count;
    public int TotalSchoolRows => DistrictDataMap.Values.Sum(d => d.Schools.Count);
    public int TotalRecords => DistrictDataMap.Values.Sum(d => d.Schools.Sum(s => s.EnrollmentRecords.Count));

    public void AddWarning(string message) => Warnings.Add(message);
    public void AddError(string message) => Errors.Add(message);
}

/// <summary>
/// All data for a single district
/// </summary>
public class DistrictData
{
    public string DistrictCode { get; set; } = string.Empty;
    public string DistrictName { get; set; } = string.Empty;
    public List<ParsedSchoolRow> Schools { get; set; } = new();
}
