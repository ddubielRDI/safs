using Microsoft.AspNetCore.Mvc;
using Sasquatch.Collection.Services;
using Sasquatch.Collection.ViewModels;

namespace Sasquatch.ViewComponents;

/// <summary>
/// View component for the district switcher dropdown in the navbar.
/// Shows current district and allows switching between demo districts.
/// </summary>
public class DistrictSwitcherViewComponent : ViewComponent
{
    private readonly IWorkflowTabService _tabService;

    public DistrictSwitcherViewComponent(IWorkflowTabService tabService)
    {
        _tabService = tabService;
    }

    public IViewComponentResult Invoke()
    {
        var currentDistrictCode = _tabService.GetCurrentDistrict(HttpContext);
        var availableDistricts = _tabService.GetAvailableDistricts();
        var currentDistrict = availableDistricts.FirstOrDefault(d => d.DistrictCode == currentDistrictCode);

        var viewModel = new DistrictSwitcherViewModel
        {
            CurrentDistrictCode = currentDistrictCode,
            CurrentDistrictName = currentDistrict?.DistrictName ?? "Unknown",
            AvailableDistricts = availableDistricts
        };

        return View(viewModel);
    }
}
