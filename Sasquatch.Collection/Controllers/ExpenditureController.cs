using Microsoft.AspNetCore.Mvc;
using Sasquatch.Core.Data;
using Sasquatch.Collection.Services;
using Sasquatch.Collection.ViewModels;

namespace Sasquatch.Collection.Controllers;

/// <summary>
/// Controller for F-196 Expenditures (Annual Financial Statement)
/// Demo Section 1: Data Collection - STUB
/// </summary>
[Area("Collection")]
public class ExpenditureController : WorkflowControllerBase
{
    public ExpenditureController(SasquatchDbContext context, IWorkflowTabService tabService)
        : base(context, tabService) { }

    public IActionResult Index()
    {
        var viewModel = new StubWorkflowViewModel
        {
            Tabs = GetTabViewModel("expenditure"),
            WorkflowName = "F-196 Expenditures",
            WorkflowDescription = "Annual Financial Statement reporting for school districts.",
            FormNumber = "F-196",
            CurrentStatus = "Coming Soon",
            Features = new List<string>
            {
                "Expenditure data collection from districts",
                "Digital certification page with signatures",
                "Fund balance reconciliation",
                "EDS data export automation",
                "SAO audit package generation"
            },
            DataSource = "https://ospi.k12.wa.us/safs-data-files"
        };

        return View(viewModel);
    }
}
