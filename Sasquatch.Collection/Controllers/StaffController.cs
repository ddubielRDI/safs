using Microsoft.AspNetCore.Mvc;
using Sasquatch.Core.Data;
using Sasquatch.Collection.Services;
using Sasquatch.Collection.ViewModels;

namespace Sasquatch.Collection.Controllers;

/// <summary>
/// Controller for S-275 Staff Reporting
/// Demo Section 1: Data Collection - STUB
/// </summary>
[Area("Collection")]
public class StaffController : WorkflowControllerBase
{
    public StaffController(SasquatchDbContext context, IWorkflowTabService tabService)
        : base(context, tabService) { }

    public IActionResult Index()
    {
        var viewModel = new StubWorkflowViewModel
        {
            Tabs = GetTabViewModel("staff"),
            WorkflowName = "S-275 Staff Reporting",
            WorkflowDescription = "Annual personnel reporting including certification, FTE, salary, and benefits.",
            FormNumber = "S-275",
            CurrentStatus = "Coming Soon",
            Features = new List<string>
            {
                "Personnel data collection",
                "Certificate number verification (eCert integration)",
                "Salary mix factor calculations",
                "Automated PII redaction (SSN, birthdate)",
                "ADA-compliant report generation",
                "Statewide average salary analysis"
            },
            DataSource = "https://ospi.k12.wa.us/safs-data-files"
        };

        return View(viewModel);
    }
}
