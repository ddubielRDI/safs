using Microsoft.AspNetCore.Mvc;
using Sasquatch.Core.Data;
using Sasquatch.Collection.Services;
using Sasquatch.Collection.ViewModels;

namespace Sasquatch.Collection.Controllers;

/// <summary>
/// Controller for F-203 Budget Projections
/// Demo Section 1: Data Collection - STUB
/// </summary>
[Area("Collection")]
public class ProjectionController : WorkflowControllerBase
{
    public ProjectionController(SasquatchDbContext context, IWorkflowTabService tabService)
        : base(context, tabService) { }

    public IActionResult Index()
    {
        var viewModel = new StubWorkflowViewModel
        {
            Tabs = GetTabViewModel("projection"),
            WorkflowName = "F-203 Budget Projections",
            WorkflowDescription = "State revenue estimates and budget projections for legislative analysis.",
            FormNumber = "F-203",
            CurrentStatus = "Coming Soon",
            Features = new List<string>
            {
                "Baseline creation from F-196 data",
                "XML import from external sources",
                "Scenario modeling for legislature",
                "State constant management",
                "Multi-year projection support"
            },
            DataSource = "https://ospi.k12.wa.us/safs-data-files"
        };

        return View(viewModel);
    }
}
