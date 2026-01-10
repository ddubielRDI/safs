using Microsoft.AspNetCore.Mvc;
using Sasquatch.Core.Data;
using Sasquatch.Collection.Services;
using Sasquatch.Collection.ViewModels;

namespace Sasquatch.Collection.Controllers;

/// <summary>
/// Controller for F-200 Budget Extensions
/// Demo Section 1: Data Collection - STUB
/// </summary>
[Area("Collection")]
public class ExtensionController : WorkflowControllerBase
{
    public ExtensionController(SasquatchDbContext context, IWorkflowTabService tabService)
        : base(context, tabService) { }

    public IActionResult Index()
    {
        var viewModel = new StubWorkflowViewModel
        {
            Tabs = GetTabViewModel("extension"),
            WorkflowName = "F-200 Budget Extensions",
            WorkflowDescription = "Budget extension requests when districts need to modify approved budgets.",
            FormNumber = "F-200",
            CurrentStatus = "Coming Soon",
            Features = new List<string>
            {
                "Extension request submission",
                "RCW compliance validation",
                "ESD review and approval workflow",
                "Automatic F-195 updates after approval",
                "Audit trail for all changes"
            },
            DataSource = "https://ospi.k12.wa.us/safs-data-files"
        };

        return View(viewModel);
    }
}
