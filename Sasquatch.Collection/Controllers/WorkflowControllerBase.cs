using Microsoft.AspNetCore.Mvc;
using Sasquatch.Core.Data;
using Sasquatch.Collection.Services;
using Sasquatch.Collection.ViewModels;

namespace Sasquatch.Collection.Controllers;

/// <summary>
/// Base controller for all workflow controllers in Section 1 (Data Collection).
/// Provides shared functionality for tab navigation and common demo settings.
/// </summary>
[Area("Collection")]
public abstract class WorkflowControllerBase : Controller
{
    protected readonly SasquatchDbContext Context;
    protected readonly IWorkflowTabService TabService;

    // Demo constants - Tumwater School District
    protected const string DemoDistrictCode = "34033";
    protected const string DemoSchoolYear = "2024-25";

    protected WorkflowControllerBase(SasquatchDbContext context, IWorkflowTabService tabService)
    {
        Context = context;
        TabService = tabService;
    }

    /// <summary>
    /// Get the workflow tab view model for the current user.
    /// Override in derived classes to set the correct active tab.
    /// </summary>
    /// <param name="activeTabId">The ID of the active tab (e.g., "enrollment", "budget")</param>
    protected WorkflowTabViewModel GetTabViewModel(string activeTabId)
    {
        var currentRole = TabService.GetCurrentRole(HttpContext);
        return new WorkflowTabViewModel
        {
            ActiveTab = activeTabId,
            VisibleTabs = TabService.GetVisibleTabsForRole(currentRole),
            UserRole = currentRole,
            AvailableRoles = TabService.GetAvailableRoles()
        };
    }
}
