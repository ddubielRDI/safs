namespace Sasquatch.Core.Interfaces.Calculation;

/// <summary>
/// Interface for apportionment calculation operations.
/// Implemented by Section 2 (Data Calculation) vendor.
/// </summary>
public interface IApportionmentEngine
{
    /// <summary>Run apportionment calculation for a single district</summary>
    Task<ApportionmentResult> CalculateDistrictAsync(string districtCode, string schoolYear);

    /// <summary>Run apportionment calculation for all districts</summary>
    Task<IEnumerable<ApportionmentResult>> CalculateAllAsync(string schoolYear);

    /// <summary>Run apportionment calculation for districts in an ESD</summary>
    Task<IEnumerable<ApportionmentResult>> CalculateByEsdAsync(string esdCode, string schoolYear);

    /// <summary>Get current state constants for calculations</summary>
    Task<IEnumerable<StateConstantDto>> GetStateConstantsAsync(string schoolYear);

    /// <summary>Update a state constant (OSPI admin only)</summary>
    Task<bool> UpdateStateConstantAsync(string constantId, decimal newValue, string updatedBy, string reason);
}

/// <summary>
/// Interface for sandbox scenario management.
/// Implemented by Section 2 (Data Calculation) vendor.
/// </summary>
public interface IScenarioService
{
    /// <summary>Create a new scenario from production data</summary>
    Task<ScenarioDto> CreateScenarioAsync(string name, string description, string ownerId, string ownerType);

    /// <summary>Get scenarios for a user/population</summary>
    Task<IEnumerable<ScenarioDto>> GetScenariosAsync(string? ownerId = null, string? ownerType = null);

    /// <summary>Get a specific scenario with its modifications</summary>
    Task<ScenarioDto?> GetScenarioAsync(Guid scenarioId);

    /// <summary>Update scenario constants/data</summary>
    Task<bool> UpdateScenarioConstantAsync(Guid scenarioId, string constantId, decimal newValue);

    /// <summary>Run calculation within a scenario</summary>
    Task<IEnumerable<ApportionmentResult>> CalculateScenarioAsync(Guid scenarioId);

    /// <summary>Compare scenario results to production</summary>
    Task<ScenarioComparisonDto> CompareToProductionAsync(Guid scenarioId);

    /// <summary>Compare two scenarios</summary>
    Task<ScenarioComparisonDto> CompareScenariosAsync(Guid scenarioId1, Guid scenarioId2);

    /// <summary>Delete a scenario</summary>
    Task<bool> DeleteScenarioAsync(Guid scenarioId);
}

/// <summary>Result of apportionment calculation</summary>
public class ApportionmentResult
{
    public string DistrictCode { get; set; } = string.Empty;
    public string DistrictName { get; set; } = string.Empty;
    public string SchoolYear { get; set; } = string.Empty;
    public DateTime CalculatedDate { get; set; }

    // Breakdown
    public decimal BasicEducation { get; set; }
    public decimal K3ClassSizeEnhancement { get; set; }
    public decimal SpecialEducation { get; set; }
    public decimal RunningStart { get; set; }
    public decimal BilingualEducation { get; set; }
    public decimal CareerTechnical { get; set; }
    public decimal HighlyCapable { get; set; }
    public decimal TransportationAllocation { get; set; }

    // Totals
    public decimal TotalApportionment { get; set; }
    public decimal TotalFTE { get; set; }
    public decimal PerPupilAmount => TotalFTE > 0 ? TotalApportionment / TotalFTE : 0;
}

/// <summary>State constant for calculations</summary>
public class StateConstantDto
{
    public string ConstantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string SchoolYear { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }
    public string? Category { get; set; }
}

/// <summary>Scenario for what-if analysis</summary>
public class ScenarioDto
{
    public Guid ScenarioId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public string OwnerType { get; set; } = string.Empty;  // OSPI, District, Legislature
    public DateTime CreatedDate { get; set; }
    public DateTime? LastCalculatedDate { get; set; }
    public decimal? TotalApportionment { get; set; }
    public decimal? VarianceFromProduction { get; set; }
    public List<ScenarioConstantDto> ModifiedConstants { get; set; } = new();
}

/// <summary>Modified constant in a scenario</summary>
public class ScenarioConstantDto
{
    public string ConstantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal OriginalValue { get; set; }
    public decimal ModifiedValue { get; set; }
    public decimal Variance => ModifiedValue - OriginalValue;
    public decimal VariancePct => OriginalValue != 0 ? (Variance / OriginalValue) * 100 : 0;
}

/// <summary>Comparison between scenarios or scenario vs production</summary>
public class ScenarioComparisonDto
{
    public ScenarioDto? Scenario1 { get; set; }
    public ScenarioDto? Scenario2 { get; set; }
    public bool IsProductionComparison { get; set; }
    public decimal TotalDifference { get; set; }
    public decimal PercentDifference { get; set; }
    public List<DistrictComparisonDto> DistrictComparisons { get; set; } = new();
}

/// <summary>Per-district comparison in a scenario comparison</summary>
public class DistrictComparisonDto
{
    public string DistrictCode { get; set; } = string.Empty;
    public string DistrictName { get; set; } = string.Empty;
    public decimal Amount1 { get; set; }
    public decimal Amount2 { get; set; }
    public decimal Difference => Amount2 - Amount1;
    public decimal PercentDifference => Amount1 != 0 ? (Difference / Amount1) * 100 : 0;
}
