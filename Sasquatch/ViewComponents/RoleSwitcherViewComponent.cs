using Microsoft.AspNetCore.Mvc;
using Sasquatch.Collection.Services;
using Sasquatch.Collection.ViewModels;

namespace Sasquatch.ViewComponents;

/// <summary>
/// View component for the role switcher dropdown in the navbar.
/// Shows current role and allows switching between District, ESD, and OSPI.
/// </summary>
public class RoleSwitcherViewComponent : ViewComponent
{
    private readonly IWorkflowTabService _tabService;

    public RoleSwitcherViewComponent(IWorkflowTabService tabService)
    {
        _tabService = tabService;
    }

    public IViewComponentResult Invoke()
    {
        var currentRole = _tabService.GetCurrentRole(HttpContext);
        var availableRoles = _tabService.GetAvailableRoles();
        var currentRoleInfo = availableRoles.FirstOrDefault(r => r.Id == currentRole);

        var viewModel = new RoleSwitcherViewModel
        {
            CurrentRole = currentRole,
            CurrentRoleDisplayName = currentRoleInfo?.DisplayName ?? "Unknown",
            CurrentRoleIcon = currentRoleInfo?.Icon ?? "bi-person",
            AvailableRoles = availableRoles
        };

        return View(viewModel);
    }
}
