namespace Sasquatch.Core.Interfaces.Reporting;

/// <summary>
/// Interface for report generation operations.
/// Implemented by Section 3 (Data Reporting) vendor.
/// </summary>
public interface IReportService
{
    /// <summary>Get available report definitions</summary>
    Task<IEnumerable<ReportDefinitionDto>> GetAvailableReportsAsync();

    /// <summary>Generate a report with specified parameters</summary>
    Task<ReportResultDto> GenerateReportAsync(ReportRequestDto request);

    /// <summary>Get report generation status (for async reports)</summary>
    Task<ReportStatusDto> GetReportStatusAsync(Guid reportJobId);

    /// <summary>Download a generated report</summary>
    Task<Stream?> DownloadReportAsync(Guid reportJobId);
}

/// <summary>
/// Interface for data export operations.
/// Implemented by Section 3 (Data Reporting) vendor.
/// </summary>
public interface IExportService
{
    /// <summary>Export enrollment data to Excel</summary>
    Task<Stream> ExportEnrollmentToExcelAsync(ExportRequestDto request);

    /// <summary>Export budget data to Excel</summary>
    Task<Stream> ExportBudgetToExcelAsync(ExportRequestDto request);

    /// <summary>Export apportionment results to Excel</summary>
    Task<Stream> ExportApportionmentToExcelAsync(ExportRequestDto request);

    /// <summary>Export data to CSV format</summary>
    Task<Stream> ExportToCsvAsync(ExportRequestDto request);
}

/// <summary>
/// Interface for external API operations.
/// Implemented by Section 3 (Data Reporting) vendor.
/// </summary>
public interface IApiDataService
{
    /// <summary>Get enrollment data for API consumers</summary>
    Task<ApiEnrollmentDataDto> GetEnrollmentDataAsync(string districtCode, string schoolYear, byte? month = null);

    /// <summary>Get budget data for API consumers</summary>
    Task<ApiBudgetDataDto> GetBudgetDataAsync(string districtCode, string fiscalYear, string? formType = null);

    /// <summary>Get apportionment results for API consumers</summary>
    Task<ApiApportionmentDto> GetApportionmentAsync(string districtCode, string schoolYear);

    /// <summary>Get district summary for API consumers</summary>
    Task<ApiDistrictSummaryDto> GetDistrictSummaryAsync(string districtCode);
}

/// <summary>Report definition metadata</summary>
public class ReportDefinitionDto
{
    public string ReportId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;  // Enrollment, Budget, Apportionment
    public List<ReportParameterDto> Parameters { get; set; } = new();
    public List<string> SupportedFormats { get; set; } = new();  // PDF, Excel, CSV
}

/// <summary>Report parameter definition</summary>
public class ReportParameterDto
{
    public string ParameterId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;  // String, Date, Number, List
    public bool IsRequired { get; set; }
    public string? DefaultValue { get; set; }
    public List<string>? AllowedValues { get; set; }
}

/// <summary>Report generation request</summary>
public class ReportRequestDto
{
    public string ReportId { get; set; } = string.Empty;
    public string Format { get; set; } = "PDF";  // PDF, Excel, CSV
    public Dictionary<string, string> Parameters { get; set; } = new();
    public string RequestedBy { get; set; } = string.Empty;
}

/// <summary>Report generation result</summary>
public class ReportResultDto
{
    public Guid ReportJobId { get; set; }
    public string Status { get; set; } = string.Empty;  // Queued, Processing, Complete, Failed
    public string? DownloadUrl { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>Report job status</summary>
public class ReportStatusDto
{
    public Guid ReportJobId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int PercentComplete { get; set; }
    public string? CurrentStep { get; set; }
    public DateTime? EstimatedCompletion { get; set; }
}

/// <summary>Export request parameters</summary>
public class ExportRequestDto
{
    public string DataType { get; set; } = string.Empty;  // Enrollment, Budget, Apportionment
    public string? DistrictCode { get; set; }
    public string? EsdCode { get; set; }
    public string? SchoolYear { get; set; }
    public byte? Month { get; set; }
    public List<string>? Columns { get; set; }
    public string? SortBy { get; set; }
    public bool IncludeHeaders { get; set; } = true;
}

// API DTOs for external consumers

public class ApiEnrollmentDataDto
{
    public string DistrictCode { get; set; } = string.Empty;
    public string DistrictName { get; set; } = string.Empty;
    public string SchoolYear { get; set; } = string.Empty;
    public List<ApiEnrollmentMonthDto> Months { get; set; } = new();
}

public class ApiEnrollmentMonthDto
{
    public byte Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TotalHeadcount { get; set; }
    public decimal TotalFTE { get; set; }
    public DateTime? SubmittedDate { get; set; }
}

public class ApiBudgetDataDto
{
    public string DistrictCode { get; set; } = string.Empty;
    public string DistrictName { get; set; } = string.Empty;
    public string FiscalYear { get; set; } = string.Empty;
    public List<ApiBudgetFormDto> Forms { get; set; } = new();
}

public class ApiBudgetFormDto
{
    public string FormType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalRevenues { get; set; }
    public decimal TotalExpenditures { get; set; }
    public decimal EndingFundBalance { get; set; }
    public DateTime? SubmittedDate { get; set; }
}

public class ApiApportionmentDto
{
    public string DistrictCode { get; set; } = string.Empty;
    public string DistrictName { get; set; } = string.Empty;
    public string SchoolYear { get; set; } = string.Empty;
    public decimal TotalApportionment { get; set; }
    public decimal TotalFTE { get; set; }
    public decimal PerPupilAmount { get; set; }
    public DateTime? CalculatedDate { get; set; }
    public Dictionary<string, decimal> Breakdown { get; set; } = new();
}

public class ApiDistrictSummaryDto
{
    public string DistrictCode { get; set; } = string.Empty;
    public string DistrictName { get; set; } = string.Empty;
    public string EsdCode { get; set; } = string.Empty;
    public string EsdName { get; set; } = string.Empty;
    public string CountyCode { get; set; } = string.Empty;
    public byte Class { get; set; }
    public int SchoolCount { get; set; }
    public bool IsActive { get; set; }
}
