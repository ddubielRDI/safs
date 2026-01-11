using System.Globalization;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using Sasquatch.Core.Models.Collection;

namespace Sasquatch.Collection.Services;

/// <summary>
/// Parser for school enrollment Excel/CSV files
/// Pivots wide format (one row per school, columns per grade/program) to tall format
/// </summary>
public class EnrollmentFileParser : IEnrollmentFileParser
{
    private const int MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB
    private static readonly string[] ValidExtensions = { ".xlsx", ".csv" };

    // Column mappings for the demo enrollment file format
    // Format: Header text -> (GradeLevel, ProgramType)
    // null GradeLevel means the grade is embedded in the column name
    private static readonly Dictionary<string, (string? Grade, string Program)> ColumnMappings = new()
    {
        // Non-ALE Basic Education grades (columns 5-17 in typical file)
        // These are detected by position after the header row
    };

    // Grade level mappings
    private static readonly Dictionary<string, string> GradeMappings = new()
    {
        { "K", "K" },
        { "1st", "01" },
        { "2nd", "02" },
        { "3rd", "03" },
        { "4th", "04" },
        { "5th", "05" },
        { "6th", "06" },
        { "7th", "07" },
        { "8th", "08" },
        { "9th", "09" },
        { "10th", "10" },
        { "11th", "11" },
        { "12th", "12" }
    };

    public (bool IsValid, string? Error) ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return (false, "Please select a file to upload.");

        if (file.Length > MaxFileSizeBytes)
            return (false, $"File exceeds {MaxFileSizeBytes / 1024 / 1024}MB limit.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!ValidExtensions.Contains(ext))
            return (false, $"Only {string.Join(", ", ValidExtensions)} files are supported.");

        return (true, null);
    }

    public async Task<FileParseResult<EnrollmentData>> ParseAsync(IFormFile file, int submissionId)
    {
        var result = new FileParseResult<EnrollmentData>();

        var (isValid, error) = ValidateFile(file);
        if (!isValid)
        {
            result.AddError(error!);
            return result;
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext == ".xlsx")
        {
            await ParseExcelAsync(file, submissionId, result);
        }
        else
        {
            result.AddError("CSV parsing not yet implemented for enrollment files.");
        }

        return result;
    }

    private async Task ParseExcelAsync(IFormFile file, int submissionId, FileParseResult<EnrollmentData> result)
    {
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;

        using var package = new ExcelPackage(stream);
        var worksheet = package.Workbook.Worksheets.FirstOrDefault();

        if (worksheet == null)
        {
            result.AddError("No worksheet found in the Excel file.");
            return;
        }

        // Find header row (look for "CCDDD" or similar in first few rows)
        int headerRow = FindHeaderRow(worksheet);
        if (headerRow == -1)
        {
            result.AddError("Could not find header row. Expected columns: CCDDD, District, S #, School");
            return;
        }

        // Build column index map
        var columnMap = BuildColumnMap(worksheet, headerRow, result);
        if (!columnMap.ContainsKey("SchoolCode"))
        {
            result.AddError("Required column 'S #' (School Code) not found.");
            return;
        }

        // Parse data rows
        int rowCount = worksheet.Dimension?.Rows ?? 0;
        int dataRowStart = headerRow + 1;

        for (int row = dataRowStart; row <= rowCount; row++)
        {
            // Skip summary rows (empty school code or "State Summary")
            var schoolCodeCell = GetCellValue(worksheet, row, columnMap.GetValueOrDefault("SchoolCode", -1));
            if (string.IsNullOrWhiteSpace(schoolCodeCell))
                continue;

            var schoolName = GetCellValue(worksheet, row, columnMap.GetValueOrDefault("SchoolName", -1));
            if (schoolName?.Contains("Summary", StringComparison.OrdinalIgnoreCase) == true)
                continue;

            // Parse this school's enrollment data
            ParseSchoolRow(worksheet, row, submissionId, schoolCodeCell, columnMap, result);
        }

        if (result.Data.Count == 0)
        {
            result.AddWarning("No enrollment records were parsed from the file.");
        }
    }

    private int FindHeaderRow(ExcelWorksheet worksheet)
    {
        int maxSearch = Math.Min(10, worksheet.Dimension?.Rows ?? 0);

        for (int row = 1; row <= maxSearch; row++)
        {
            for (int col = 1; col <= Math.Min(5, worksheet.Dimension?.Columns ?? 0); col++)
            {
                var value = worksheet.Cells[row, col].Value?.ToString()?.Trim();
                if (value?.Equals("CCDDD", StringComparison.OrdinalIgnoreCase) == true ||
                    value?.Contains("District", StringComparison.OrdinalIgnoreCase) == true &&
                    value?.Length < 20)
                {
                    return row;
                }
            }
        }

        return -1;
    }

    private Dictionary<string, int> BuildColumnMap(ExcelWorksheet worksheet, int headerRow, FileParseResult<EnrollmentData> result)
    {
        var map = new Dictionary<string, int>();
        int colCount = worksheet.Dimension?.Columns ?? 0;

        // Track whether we're in Non-ALE or ALE section
        bool inAleSection = false;
        int nonAleGradeStart = -1;
        int aleGradeStart = -1;

        for (int col = 1; col <= colCount; col++)
        {
            var header = worksheet.Cells[headerRow, col].Value?.ToString()?.Trim() ?? "";
            var headerUpper = header.ToUpperInvariant();

            // Fixed columns
            if (headerUpper == "CCDDD")
                map["DistrictCode"] = col;
            else if (headerUpper == "DISTRICT" && !map.ContainsKey("DistrictName"))
                map["DistrictName"] = col;
            else if (headerUpper == "S #" || headerUpper == "S#")
                map["SchoolCode"] = col;
            else if (headerUpper == "SCHOOL")
                map["SchoolName"] = col;

            // Check for ALE section marker (row above might have "ALE" grouping)
            // Check cell above current header
            var groupHeader = worksheet.Cells[headerRow - 1, col].Value?.ToString()?.Trim() ?? "";
            if (groupHeader.Contains("ALE", StringComparison.OrdinalIgnoreCase) &&
                !groupHeader.Contains("Non-ALE", StringComparison.OrdinalIgnoreCase))
            {
                inAleSection = true;
            }

            // Grade columns (K, 1st, 2nd, etc.)
            if (GradeMappings.TryGetValue(header, out var grade))
            {
                string program = inAleSection ? "ALE" : "BasicEd";
                string key = $"Grade_{grade}_{program}";
                map[key] = col;

                if (!inAleSection && nonAleGradeStart == -1)
                    nonAleGradeStart = col;
                if (inAleSection && aleGradeStart == -1)
                    aleGradeStart = col;
            }

            // Total columns mark section boundaries
            if (headerUpper == "TOTAL")
            {
                if (!inAleSection)
                {
                    // After Non-ALE total, next grades are ALE
                    inAleSection = true;
                }
            }

            // Special program columns
            if (headerUpper.Contains("RUN START") || headerUpper.Contains("RUNNING START"))
            {
                if (headerUpper.Contains("VOC"))
                    map["RunningStartVoc"] = col;
                else
                    map["RunningStartNonVoc"] = col;
            }
            else if (headerUpper.Contains("OPEN DOORS"))
            {
                if (headerUpper.Contains("VOC"))
                    map["OpenDoorsVoc"] = col;
                else
                    map["OpenDoorsNonVoc"] = col;
            }
            else if (headerUpper.Contains("SPED") || headerUpper.Contains("SPECIAL ED"))
            {
                if (headerUpper.Contains("K-21"))
                {
                    if (headerUpper.Contains("TIER 1"))
                        map["SpedK21Tier1"] = col;
                    else if (headerUpper.Contains("OTHER"))
                        map["SpedK21Other"] = col;
                }
                else if (headerUpper.Contains("TK"))
                {
                    map["SpedTK"] = col;
                }
                else if (headerUpper.Contains("3-5") || headerUpper.Contains("AGE"))
                {
                    map["SpedAge35"] = col;
                }
            }
            else if (headerUpper.Contains("TBIP"))
            {
                if (headerUpper.Contains("K-6"))
                    map["TBIPK6"] = col;
                else if (headerUpper.Contains("7-12"))
                    map["TBIP712"] = col;
                else if (headerUpper.Contains("TK"))
                    map["TBIPTK"] = col;
            }
            else if (headerUpper.Contains("CTE"))
            {
                if (headerUpper.Contains("7-8"))
                {
                    if (headerUpper.Contains("ALE"))
                        map["CTE78ALE"] = col;
                    else
                        map["CTE78NonALE"] = col;
                }
                else if (headerUpper.Contains("9-12"))
                {
                    if (headerUpper.Contains("ALE"))
                        map["CTE912ALE"] = col;
                    else
                        map["CTE912NonALE"] = col;
                }
            }
            else if (headerUpper == "TK")
            {
                map["TK"] = col;
            }
        }

        return map;
    }

    private void ParseSchoolRow(ExcelWorksheet worksheet, int row, int submissionId,
        string schoolCode, Dictionary<string, int> columnMap, FileParseResult<EnrollmentData> result)
    {
        // Normalize school code to 4 digits
        schoolCode = schoolCode.Trim().PadLeft(4, '0');

        // Parse grade-level enrollment (Non-ALE BasicEd)
        foreach (var gradeMap in GradeMappings)
        {
            string key = $"Grade_{gradeMap.Value}_BasicEd";
            if (columnMap.TryGetValue(key, out int col))
            {
                var fte = ParseDecimal(worksheet, row, col);
                if (fte >= 0.01m) // Skip near-zero values
                {
                    result.Data.Add(new EnrollmentData
                    {
                        SubmissionId = submissionId,
                        SchoolCode = schoolCode,
                        GradeLevel = gradeMap.Value,
                        ProgramType = "BasicEd",
                        Headcount = (int)Math.Round(fte),
                        FTE = fte
                    });
                }
            }
        }

        // Parse grade-level enrollment (ALE)
        foreach (var gradeMap in GradeMappings)
        {
            string key = $"Grade_{gradeMap.Value}_ALE";
            if (columnMap.TryGetValue(key, out int col))
            {
                var fte = ParseDecimal(worksheet, row, col);
                if (fte >= 0.01m)
                {
                    result.Data.Add(new EnrollmentData
                    {
                        SubmissionId = submissionId,
                        SchoolCode = schoolCode,
                        GradeLevel = gradeMap.Value,
                        ProgramType = "ALE",
                        Headcount = (int)Math.Round(fte),
                        FTE = fte
                    });
                }
            }
        }

        // Parse Running Start
        AddProgramEnrollment(worksheet, row, submissionId, schoolCode, columnMap,
            "RunningStartNonVoc", "11", "RunningStart", result);
        AddProgramEnrollment(worksheet, row, submissionId, schoolCode, columnMap,
            "RunningStartVoc", "11", "RunningStart", result);

        // Parse Open Doors
        AddProgramEnrollment(worksheet, row, submissionId, schoolCode, columnMap,
            "OpenDoorsNonVoc", "12", "OpenDoors", result);
        AddProgramEnrollment(worksheet, row, submissionId, schoolCode, columnMap,
            "OpenDoorsVoc", "12", "OpenDoors", result);

        // Parse Special Ed
        AddProgramEnrollment(worksheet, row, submissionId, schoolCode, columnMap,
            "SpedK21Tier1", "K", "SpecialEd", result);
        AddProgramEnrollment(worksheet, row, submissionId, schoolCode, columnMap,
            "SpedK21Other", "K", "SpecialEd", result);
        AddProgramEnrollment(worksheet, row, submissionId, schoolCode, columnMap,
            "SpedAge35", "PK", "SpecialEd", result);
    }

    private void AddProgramEnrollment(ExcelWorksheet worksheet, int row, int submissionId,
        string schoolCode, Dictionary<string, int> columnMap, string mapKey,
        string defaultGrade, string programType, FileParseResult<EnrollmentData> result)
    {
        if (columnMap.TryGetValue(mapKey, out int col))
        {
            var fte = ParseDecimal(worksheet, row, col);
            if (fte >= 0.01m)
            {
                result.Data.Add(new EnrollmentData
                {
                    SubmissionId = submissionId,
                    SchoolCode = schoolCode,
                    GradeLevel = defaultGrade,
                    ProgramType = programType,
                    Headcount = (int)Math.Round(fte),
                    FTE = fte
                });
            }
        }
    }

    private decimal ParseDecimal(ExcelWorksheet worksheet, int row, int col)
    {
        var value = worksheet.Cells[row, col].Value;
        if (value == null) return 0;

        if (value is double d)
            return (decimal)d;
        if (value is decimal dec)
            return dec;
        if (value is int i)
            return i;

        if (decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;

        return 0;
    }

    private string? GetCellValue(ExcelWorksheet worksheet, int row, int col)
    {
        if (col < 1) return null;
        return worksheet.Cells[row, col].Value?.ToString()?.Trim();
    }
}
