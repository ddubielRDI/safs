using System.ComponentModel.DataAnnotations;

namespace Sasquatch.Core.Models.Reporting;

/// <summary>
/// Report definition and configuration
/// </summary>
public class ReportDefinition
{
    [Key]
    public int ReportId { get; set; }

    [Required]
    [MaxLength(50)]
    public string ReportCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string ReportName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// SQL query or stored procedure name
    /// </summary>
    public string? QueryDefinition { get; set; }

    /// <summary>
    /// JSON configuration for report parameters
    /// </summary>
    public string? ParameterConfig { get; set; }

    /// <summary>
    /// JSON configuration for column display
    /// </summary>
    public string? ColumnConfig { get; set; }

    public bool IsActive { get; set; } = true;

    public int DisplayOrder { get; set; }
}

/// <summary>
/// Saved report execution history
/// </summary>
public class ReportExecution
{
    [Key]
    public int ExecutionId { get; set; }

    public int ReportId { get; set; }

    [MaxLength(50)]
    public string? ExecutedBy { get; set; }

    public DateTime ExecutedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// JSON of parameters used
    /// </summary>
    public string? ParametersUsed { get; set; }

    public int RowsReturned { get; set; }

    public int ExecutionTimeMs { get; set; }

    // Navigation property
    public virtual ReportDefinition? ReportDefinition { get; set; }
}
