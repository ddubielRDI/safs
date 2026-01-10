using Microsoft.AspNetCore.Mvc;
using Sasquatch.Collection.Services;

namespace Sasquatch.Collection.Controllers;

/// <summary>
/// Collection area home controller for role switching and common actions
/// </summary>
[Area("Collection")]
public class HomeController : Controller
{
    private readonly IWorkflowTabService _tabService;

    public HomeController(IWorkflowTabService tabService)
    {
        _tabService = tabService;
    }

    /// <summary>
    /// Switch the demo role (District, ESD, OSPI)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SwitchRole(string role)
    {
        _tabService.SetDemoRole(HttpContext, role);

        // Redirect back to the referrer or default to Enrollment
        var referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referer))
        {
            return Redirect(referer);
        }

        return RedirectToAction("Index", "Enrollment", new { area = "Collection" });
    }

    /// <summary>
    /// GET endpoint for role switch (fallback)
    /// </summary>
    [HttpGet]
    public IActionResult SwitchRole()
    {
        return RedirectToAction("Index", "Enrollment", new { area = "Collection" });
    }
}
