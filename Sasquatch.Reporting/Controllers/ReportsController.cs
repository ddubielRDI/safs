using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sasquatch.Core.Data;

namespace Sasquatch.Reporting.Controllers;

/// <summary>
/// Report generation and viewing
/// Demo Section 3: Data Reporting
/// </summary>
[Area("Reporting")]
public class ReportsController : Controller
{
    private readonly SasquatchDbContext _context;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(SasquatchDbContext context, ILogger<ReportsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Reports dashboard - list available reports
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Enrollment reports (P-223)
    /// </summary>
    public IActionResult Enrollment()
    {
        return View();
    }

    /// <summary>
    /// Budget reports (F-195)
    /// </summary>
    public IActionResult Budget()
    {
        return View();
    }

    /// <summary>
    /// Financial statement report (F-196)
    /// </summary>
    public IActionResult FinancialStatement()
    {
        return View();
    }

    /// <summary>
    /// Export to Excel
    /// </summary>
    public IActionResult Export(string reportType)
    {
        // Placeholder for Excel export
        return View();
    }
}
