using Microsoft.AspNetCore.Http;

namespace Sasquatch.Collection.Services;

/// <summary>
/// Service for managing workflow tab visibility based on user role.
/// Provides role-based filtering to show only relevant tabs to each user type.
/// </summary>
public interface IWorkflowTabService
{
    /// <summary>
    /// Get the list of visible tabs for a given role
    /// </summary>
    List<WorkflowTabInfo> GetVisibleTabsForRole(string role);

    /// <summary>
    /// Get the current demo role from session
    /// </summary>
    string GetCurrentRole(HttpContext context);

    /// <summary>
    /// Set the demo role in session
    /// </summary>
    void SetDemoRole(HttpContext context, string role);

    /// <summary>
    /// Get all available roles for the role switcher
    /// </summary>
    List<RoleInfo> GetAvailableRoles();
}

/// <summary>
/// Information about a workflow tab
/// </summary>
public class WorkflowTabInfo
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Controller { get; set; } = string.Empty;
    public string Action { get; set; } = "Index";
    public int? PendingCount { get; set; }
    public bool IsAdminTab { get; set; }
    public bool IsStub { get; set; }
}

/// <summary>
/// Information about a user role for the role switcher
/// </summary>
public class RoleInfo
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}

/// <summary>
/// Implementation of workflow tab service
/// </summary>
public class WorkflowTabService : IWorkflowTabService
{
    private const string SessionKey = "DemoRole";
    private const string DefaultRole = "District";

    /// <summary>
    /// All available workflow tabs with their metadata
    /// </summary>
    private static readonly List<WorkflowTabInfo> AllTabs = new()
    {
        new WorkflowTabInfo
        {
            Id = "enrollment",
            DisplayName = "P-223 Enrollment",
            ShortName = "P-223",
            Description = "Monthly enrollment reporting",
            Icon = "bi-people",
            Controller = "Enrollment",
            IsStub = false
        },
        new WorkflowTabInfo
        {
            Id = "budget",
            DisplayName = "F-195 Budget",
            ShortName = "F-195",
            Description = "Annual budget reporting",
            Icon = "bi-currency-dollar",
            Controller = "Budget",
            IsStub = false
        },
        new WorkflowTabInfo
        {
            Id = "expenditure",
            DisplayName = "F-196 Expenditures",
            ShortName = "F-196",
            Description = "Annual financial statement",
            Icon = "bi-receipt",
            Controller = "Expenditure",
            IsStub = true
        },
        new WorkflowTabInfo
        {
            Id = "cashfile",
            DisplayName = "F-197 Cash File",
            ShortName = "F-197",
            Description = "Cash balance monitoring",
            Icon = "bi-cash-stack",
            Controller = "CashFile",
            IsStub = true
        },
        new WorkflowTabInfo
        {
            Id = "extension",
            DisplayName = "F-200 Extensions",
            ShortName = "F-200",
            Description = "Budget extension requests",
            Icon = "bi-calendar-plus",
            Controller = "Extension",
            IsStub = true
        },
        new WorkflowTabInfo
        {
            Id = "projection",
            DisplayName = "F-203 Projections",
            ShortName = "F-203",
            Description = "State revenue estimates",
            Icon = "bi-graph-up",
            Controller = "Projection",
            IsStub = true
        },
        new WorkflowTabInfo
        {
            Id = "staff",
            DisplayName = "S-275 Staff",
            ShortName = "S-275",
            Description = "Annual personnel reporting",
            Icon = "bi-person-badge",
            Controller = "Staff",
            IsStub = true
        },
        new WorkflowTabInfo
        {
            Id = "locks",
            DisplayName = "Lock Controls",
            ShortName = "Locks",
            Description = "Data submission locks",
            Icon = "bi-lock",
            Controller = "Admin",
            Action = "Locks",
            IsAdminTab = true
        },
        new WorkflowTabInfo
        {
            Id = "audit",
            DisplayName = "Audit Log",
            ShortName = "Audit",
            Description = "System audit trail",
            Icon = "bi-journal-text",
            Controller = "Admin",
            Action = "AuditLog",
            IsAdminTab = true
        }
    };

    /// <summary>
    /// Available roles for demo mode
    /// </summary>
    private static readonly List<RoleInfo> AvailableRoles = new()
    {
        new RoleInfo
        {
            Id = "District",
            DisplayName = "District User",
            Description = "School district staff submitting data",
            Icon = "bi-building"
        },
        new RoleInfo
        {
            Id = "ESD",
            DisplayName = "ESD User",
            Description = "Educational Service District reviewer",
            Icon = "bi-diagram-3"
        },
        new RoleInfo
        {
            Id = "OSPI",
            DisplayName = "OSPI Admin",
            Description = "OSPI administrator with full access",
            Icon = "bi-shield-lock"
        }
    };

    public List<WorkflowTabInfo> GetVisibleTabsForRole(string role)
    {
        return role switch
        {
            "District" => AllTabs
                .Where(t => !t.IsAdminTab && t.Id != "cashfile") // Districts don't see F-197 or Admin
                .ToList(),

            "ESD" => AllTabs
                .Where(t => !t.IsAdminTab) // ESDs see all workflows but not Admin
                .ToList(),

            "OSPI" => AllTabs.ToList(), // OSPI sees everything

            _ => AllTabs
                .Where(t => !t.IsAdminTab && t.Id != "cashfile")
                .ToList()
        };
    }

    public string GetCurrentRole(HttpContext context)
    {
        return context.Session.GetString(SessionKey) ?? DefaultRole;
    }

    public void SetDemoRole(HttpContext context, string role)
    {
        // Validate role
        if (AvailableRoles.Any(r => r.Id == role))
        {
            context.Session.SetString(SessionKey, role);
        }
    }

    public List<RoleInfo> GetAvailableRoles()
    {
        return AvailableRoles;
    }
}
