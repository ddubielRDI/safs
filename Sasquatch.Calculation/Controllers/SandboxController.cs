using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sasquatch.Core.Data;

namespace Sasquatch.Calculation.Controllers;

/// <summary>
/// Sandbox environment for what-if scenarios
/// Demo Section 2: Data Calculation - Sandbox Mode
/// </summary>
[Area("Calculation")]
public class SandboxController : Controller
{
    private readonly SasquatchDbContext _context;
    private readonly ILogger<SandboxController> _logger;

    public SandboxController(SasquatchDbContext context, ILogger<SandboxController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Sandbox dashboard - manage scenarios
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Create new scenario
    /// </summary>
    public IActionResult Create()
    {
        return View();
    }

    /// <summary>
    /// Compare scenarios
    /// </summary>
    public IActionResult Compare()
    {
        return View();
    }
}
