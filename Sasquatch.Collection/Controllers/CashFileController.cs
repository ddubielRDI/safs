using Microsoft.AspNetCore.Mvc;
using Sasquatch.Core.Data;
using Sasquatch.Collection.Services;
using Sasquatch.Collection.ViewModels;

namespace Sasquatch.Collection.Controllers;

/// <summary>
/// Controller for F-197 Cash File Report
/// Demo Section 1: Data Collection - STUB
/// </summary>
[Area("Collection")]
public class CashFileController : WorkflowControllerBase
{
    public CashFileController(SasquatchDbContext context, IWorkflowTabService tabService)
        : base(context, tabService) { }

    public IActionResult Index()
    {
        var viewModel = new StubWorkflowViewModel
        {
            Tabs = GetTabViewModel("cashfile"),
            WorkflowName = "F-197 Cash File Report",
            WorkflowDescription = "Cash balance monitoring and reconciliation with County Treasurers.",
            FormNumber = "F-197",
            CurrentStatus = "Coming Soon",
            Features = new List<string>
            {
                "County treasurer data integration",
                "Automated reconciliation rules",
                "Variance detection and flagging",
                "Error notification workflow",
                "Historical trend analysis"
            },
            DataSource = "https://ospi.k12.wa.us/safs-data-files"
        };

        return View(viewModel);
    }
}
