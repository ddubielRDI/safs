using Sasquatch.Collection.Services;

namespace Sasquatch.Collection.ViewModels;

/// <summary>
/// View model for the workflow tab navigation component.
/// Contains visible tabs, active tab, and current user role.
/// </summary>
public class WorkflowTabViewModel
{
    /// <summary>
    /// List of tabs visible to the current user based on their role
    /// </summary>
    public List<WorkflowTabInfo> VisibleTabs { get; set; } = new();

    /// <summary>
    /// ID of the currently active tab (based on current controller)
    /// </summary>
    public string ActiveTab { get; set; } = string.Empty;

    /// <summary>
    /// Current user role (District, ESD, OSPI)
    /// </summary>
    public string UserRole { get; set; } = "District";

    /// <summary>
    /// Available roles for the role switcher dropdown
    /// </summary>
    public List<RoleInfo> AvailableRoles { get; set; } = new();

    /// <summary>
    /// Check if a tab is the currently active tab
    /// </summary>
    public bool IsActive(string tabId) =>
        string.Equals(ActiveTab, tabId, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Check if a tab is the currently active tab by controller name
    /// </summary>
    public bool IsActiveController(string controller) =>
        VisibleTabs.Any(t =>
            string.Equals(t.Controller, controller, StringComparison.OrdinalIgnoreCase) &&
            IsActive(t.Id));
}

/// <summary>
/// View model for the role switcher component
/// </summary>
public class RoleSwitcherViewModel
{
    /// <summary>
    /// Current role ID
    /// </summary>
    public string CurrentRole { get; set; } = "District";

    /// <summary>
    /// Display name for current role
    /// </summary>
    public string CurrentRoleDisplayName { get; set; } = "District User";

    /// <summary>
    /// Icon for current role
    /// </summary>
    public string CurrentRoleIcon { get; set; } = "bi-building";

    /// <summary>
    /// All available roles for switching
    /// </summary>
    public List<RoleInfo> AvailableRoles { get; set; } = new();
}

/// <summary>
/// View model for the district switcher component
/// </summary>
public class DistrictSwitcherViewModel
{
    /// <summary>
    /// Current district code
    /// </summary>
    public string CurrentDistrictCode { get; set; } = "34033";

    /// <summary>
    /// Display name for current district
    /// </summary>
    public string CurrentDistrictName { get; set; } = "Tumwater";

    /// <summary>
    /// All available districts for switching
    /// </summary>
    public List<DistrictInfo> AvailableDistricts { get; set; } = new();
}

/// <summary>
/// Base view model interface for workflow views that need tab support
/// </summary>
public interface IWorkflowViewModel
{
    WorkflowTabViewModel Tabs { get; set; }
}

/// <summary>
/// Extension methods for workflow view models
/// </summary>
public static class WorkflowViewModelExtensions
{
    /// <summary>
    /// Get the CSS class for a tab based on active state
    /// </summary>
    public static string GetTabClass(this WorkflowTabViewModel tabs, string tabId)
    {
        var isActive = tabs.IsActive(tabId);
        return isActive ? "nav-link active" : "nav-link";
    }

    /// <summary>
    /// Get ARIA selected attribute value for a tab
    /// </summary>
    public static string GetAriaSelected(this WorkflowTabViewModel tabs, string tabId)
    {
        return tabs.IsActive(tabId) ? "true" : "false";
    }
}
