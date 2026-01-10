namespace Sasquatch.Collection.ViewModels;

/// <summary>
/// View model for stub workflow pages (Coming Soon).
/// Used for workflows that are not yet fully implemented.
/// </summary>
public class StubWorkflowViewModel : IWorkflowViewModel
{
    public WorkflowTabViewModel Tabs { get; set; } = new();

    /// <summary>
    /// Full name of the workflow (e.g., "F-196 Expenditures")
    /// </summary>
    public string WorkflowName { get; set; } = string.Empty;

    /// <summary>
    /// Description of what this workflow does
    /// </summary>
    public string WorkflowDescription { get; set; } = string.Empty;

    /// <summary>
    /// Form number (e.g., "F-196", "S-275")
    /// </summary>
    public string FormNumber { get; set; } = string.Empty;

    /// <summary>
    /// Current status (e.g., "Coming Soon", "In Development")
    /// </summary>
    public string CurrentStatus { get; set; } = "Coming Soon";

    /// <summary>
    /// List of features that will be implemented
    /// </summary>
    public List<string> Features { get; set; } = new();

    /// <summary>
    /// Link to official SAFS data files
    /// </summary>
    public string DataSource { get; set; } = "https://ospi.k12.wa.us/safs-data-files";

    /// <summary>
    /// Optional: Sample statistics from real data
    /// </summary>
    public StubWorkflowStats? Stats { get; set; }
}

/// <summary>
/// Sample statistics for stub workflows
/// </summary>
public class StubWorkflowStats
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Icon { get; set; } = "bi-info-circle";
}
