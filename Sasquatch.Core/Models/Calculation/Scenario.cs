using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sasquatch.Core.Models.Shared;

namespace Sasquatch.Core.Models.Calculation;

/// <summary>
/// Sandbox scenario for what-if analysis
/// </summary>
public class Scenario
{
    [Key]
    public int ScenarioId { get; set; }

    [Required]
    [MaxLength(100)]
    public string ScenarioName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(10)]
    public string SchoolYear { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedDate { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// If true, this scenario uses modified state constants
    /// </summary>
    public bool UsesModifiedConstants { get; set; }

    /// <summary>
    /// If true, this scenario uses modified enrollment projections
    /// </summary>
    public bool UsesModifiedEnrollment { get; set; }
}

/// <summary>
/// State constants used in apportionment calculations
/// </summary>
public class StateConstant
{
    [Key]
    public int ConstantId { get; set; }

    [Required]
    [MaxLength(50)]
    public string ConstantCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string ConstantName { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,6)")]
    public decimal Value { get; set; }

    [Required]
    [MaxLength(10)]
    public string SchoolYear { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Category { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Apportionment calculation result
/// </summary>
public class ApportionmentResult
{
    [Key]
    public int ResultId { get; set; }

    [Required]
    [MaxLength(10)]
    public string DistrictCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string SchoolYear { get; set; } = string.Empty;

    public byte Month { get; set; }

    /// <summary>
    /// Null for production, ScenarioId for sandbox
    /// </summary>
    public int? ScenarioId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalApportionment { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal BasicEducation { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal SpecialEducation { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Vocational { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Bilingual { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Transportation { get; set; }

    public DateTime CalculatedDate { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string? CalculatedBy { get; set; }

    // Navigation properties
    public virtual District? District { get; set; }
    public virtual Scenario? Scenario { get; set; }
}
