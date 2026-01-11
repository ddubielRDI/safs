using System.Globalization;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using Sasquatch.Core.Models.Collection;

namespace Sasquatch.Collection.Services;

/// <summary>
/// Parser for F-195 budget projection Excel/CSV files
/// Imports all 4 fiscal years from the file
/// </summary>
public class BudgetFileParser : IBudgetFileParser
{
    private const int MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB
    private static readonly string[] ValidExtensions = { ".xlsx", ".csv" };

    // Expected fiscal years in the demo file
    private static readonly string[] FiscalYears = { "2024-25", "2025-26", "2026-27", "2027-28" };

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

    public async Task<FileParseResult<BudgetData>> ParseAsync(IFormFile file, int submissionId)
    {
        var result = new FileParseResult<BudgetData>();

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
            result.AddError("CSV parsing not yet implemented for budget files.");
        }

        return result;
    }

    private async Task ParseExcelAsync(IFormFile file, int submissionId, FileParseResult<BudgetData> result)
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

        // Find header row
        int headerRow = FindHeaderRow(worksheet);
        if (headerRow == -1)
        {
            result.AddError("Could not find header row. Expected columns: District, Fund, Item Code");
            return;
        }

        // Build column index map
        var columnMap = BuildColumnMap(worksheet, headerRow, result);

        // Validate required columns
        if (!columnMap.ContainsKey("Fund"))
        {
            result.AddError("Required column 'Fund' not found.");
            return;
        }

        // Find fiscal year columns
        var yearColumns = FindYearColumns(worksheet, headerRow, result);
        if (yearColumns.Count == 0)
        {
            result.AddWarning("No fiscal year columns found. Looking for patterns like '24-25' or '2024-25'.");
        }

        // Parse data rows
        int rowCount = worksheet.Dimension?.Rows ?? 0;
        int dataRowStart = headerRow + 1;

        for (int row = dataRowStart; row <= rowCount; row++)
        {
            // Get fund code - skip if empty
            var fundCode = GetCellValue(worksheet, row, columnMap.GetValueOrDefault("Fund", -1));
            if (string.IsNullOrWhiteSpace(fundCode))
                continue;

            // Parse this budget row (creates one record per fiscal year)
            ParseBudgetRow(worksheet, row, submissionId, columnMap, yearColumns, result);
        }

        if (result.Data.Count == 0)
        {
            result.AddWarning("No budget records were parsed from the file.");
        }
    }

    private int FindHeaderRow(ExcelWorksheet worksheet)
    {
        int maxSearch = Math.Min(5, worksheet.Dimension?.Rows ?? 0);

        for (int row = 1; row <= maxSearch; row++)
        {
            for (int col = 1; col <= Math.Min(10, worksheet.Dimension?.Columns ?? 0); col++)
            {
                var value = worksheet.Cells[row, col].Value?.ToString()?.Trim();
                if (value?.Equals("District", StringComparison.OrdinalIgnoreCase) == true ||
                    value?.Equals("Fund", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return row;
                }
            }
        }

        return -1;
    }

    private Dictionary<string, int> BuildColumnMap(ExcelWorksheet worksheet, int headerRow, FileParseResult<BudgetData> result)
    {
        var map = new Dictionary<string, int>();
        int colCount = worksheet.Dimension?.Columns ?? 0;

        for (int col = 1; col <= colCount; col++)
        {
            var header = worksheet.Cells[headerRow, col].Value?.ToString()?.Trim() ?? "";
            var headerUpper = header.ToUpperInvariant();

            if (headerUpper == "DISTRICT")
                map["District"] = col;
            else if (headerUpper == "FUND")
                map["Fund"] = col;
            else if (headerUpper == "FUND DESCRIPTION" || headerUpper == "FUNDDESCRIPTION")
                map["FundDescription"] = col;
            else if (headerUpper == "ITEM CODE" || headerUpper == "ITEMCODE")
                map["ItemCode"] = col;
            else if (headerUpper == "CODE DESCRIPTION" || headerUpper == "CODEDESCRIPTION")
                map["CodeDescription"] = col;
            else if (headerUpper == "PROGRAM" || headerUpper == "PROGRAM CODE")
                map["ProgramCode"] = col;
            else if (headerUpper == "ACTIVITY" || headerUpper == "ACTIVITY CODE")
                map["ActivityCode"] = col;
            else if (headerUpper == "OBJECT" || headerUpper == "OBJECT CODE")
                map["ObjectCode"] = col;
        }

        return map;
    }

    private Dictionary<string, int> FindYearColumns(ExcelWorksheet worksheet, int headerRow, FileParseResult<BudgetData> result)
    {
        var yearColumns = new Dictionary<string, int>();
        int colCount = worksheet.Dimension?.Columns ?? 0;

        for (int col = 1; col <= colCount; col++)
        {
            var header = worksheet.Cells[headerRow, col].Value?.ToString()?.Trim() ?? "";

            // Look for year patterns like "24-25", "2024-25", "FY24-25"
            foreach (var year in FiscalYears)
            {
                // Match "24-25" or "2024-25"
                string shortYear = year.Substring(2, 2) + "-" + year.Substring(7, 2); // "24-25"
                if (header.Contains(year) || header.Contains(shortYear) || header == shortYear)
                {
                    yearColumns[year] = col;
                    break;
                }
            }
        }

        // If no explicit year columns, check for numeric columns after description columns
        if (yearColumns.Count == 0)
        {
            result.AddWarning("Could not find year-labeled columns. Looking for numeric columns.");

            // Find first numeric column after item description
            int numericStart = -1;
            for (int col = 5; col <= colCount; col++)
            {
                var header = worksheet.Cells[headerRow, col].Value?.ToString()?.Trim() ?? "";
                // Check if header looks like a year or is numeric
                if (decimal.TryParse(header.Replace("-", "").Replace("/", ""), out _) ||
                    header.Contains("24") || header.Contains("25") || header.Contains("26") || header.Contains("27"))
                {
                    numericStart = col;
                    break;
                }
            }

            if (numericStart > 0)
            {
                // Assign fiscal years sequentially
                for (int i = 0; i < FiscalYears.Length && (numericStart + i) <= colCount; i++)
                {
                    yearColumns[FiscalYears[i]] = numericStart + i;
                }
            }
        }

        return yearColumns;
    }

    private void ParseBudgetRow(ExcelWorksheet worksheet, int row, int submissionId,
        Dictionary<string, int> columnMap, Dictionary<string, int> yearColumns,
        FileParseResult<BudgetData> result)
    {
        // Get common fields
        var fundCode = GetCellValue(worksheet, row, columnMap.GetValueOrDefault("Fund", -1)) ?? "";
        var fundDesc = GetCellValue(worksheet, row, columnMap.GetValueOrDefault("FundDescription", -1));
        var itemCode = GetCellValue(worksheet, row, columnMap.GetValueOrDefault("ItemCode", -1));
        var itemDesc = GetCellValue(worksheet, row, columnMap.GetValueOrDefault("CodeDescription", -1));
        var programCode = GetCellValue(worksheet, row, columnMap.GetValueOrDefault("ProgramCode", -1));
        var activityCode = GetCellValue(worksheet, row, columnMap.GetValueOrDefault("ActivityCode", -1));
        var objectCode = GetCellValue(worksheet, row, columnMap.GetValueOrDefault("ObjectCode", -1));

        // Normalize fund code to 2 characters
        if (fundCode.Length == 1)
            fundCode = fundCode.PadLeft(2, '0');

        // Create one record per fiscal year
        foreach (var yearCol in yearColumns)
        {
            var amount = ParseDecimal(worksheet, row, yearCol.Value);

            // Skip zero amounts to reduce noise
            if (amount == 0)
                continue;

            result.Data.Add(new BudgetData
            {
                SubmissionId = submissionId,
                FundCode = fundCode.Length <= 2 ? fundCode : fundCode.Substring(0, 2),
                ProgramCode = programCode?.Length <= 2 ? programCode : programCode?.Substring(0, 2),
                ActivityCode = activityCode?.Length <= 2 ? activityCode : activityCode?.Substring(0, 2),
                ObjectCode = objectCode?.Length <= 3 ? objectCode : objectCode?.Substring(0, 3),
                ItemCode = itemCode,
                ItemDescription = itemDesc ?? fundDesc,
                FiscalYear = yearCol.Key,
                Amount = amount
            });
        }
    }

    private decimal ParseDecimal(ExcelWorksheet worksheet, int row, int col)
    {
        if (col < 1) return 0;

        var value = worksheet.Cells[row, col].Value;
        if (value == null) return 0;

        if (value is double d)
            return (decimal)d;
        if (value is decimal dec)
            return dec;
        if (value is int i)
            return i;
        if (value is long l)
            return l;

        var str = value.ToString()?.Replace(",", "").Replace("$", "").Trim();
        if (decimal.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;

        return 0;
    }

    private string? GetCellValue(ExcelWorksheet worksheet, int row, int col)
    {
        if (col < 1) return null;
        return worksheet.Cells[row, col].Value?.ToString()?.Trim();
    }
}
