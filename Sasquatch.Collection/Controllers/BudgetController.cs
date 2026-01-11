using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sasquatch.Core.Data;
using Sasquatch.Core.Models.Collection;
using Sasquatch.Collection.ViewModels;
using Sasquatch.Collection.Services;

namespace Sasquatch.Collection.Controllers;

/// <summary>
/// Controller for budget data collection (F-195 forms)
/// Demo Section 1: Data Collection - Full Implementation
/// </summary>
[Area("Collection")]
public class BudgetController : Controller
{
    private readonly SasquatchDbContext _context;
    private readonly ILogger<BudgetController> _logger;
    private readonly IWorkflowTabService _tabService;
    private readonly IInstructionService _instructionService;
    private readonly IBudgetFileParser _fileParser;

    // For demo, we'll use Tumwater district
    private const string DemoDistrictCode = "34033";
    private const string DemoSchoolYear = "2024-25";

    public BudgetController(
        SasquatchDbContext context,
        ILogger<BudgetController> logger,
        IWorkflowTabService tabService,
        IInstructionService instructionService,
        IBudgetFileParser fileParser)
    {
        _context = context;
        _logger = logger;
        _tabService = tabService;
        _instructionService = instructionService;
        _fileParser = fileParser;
    }

    /// <summary>
    /// Get the workflow tab view model for the current user
    /// </summary>
    private WorkflowTabViewModel GetTabViewModel()
    {
        var currentRole = _tabService.GetCurrentRole(HttpContext);
        return new WorkflowTabViewModel
        {
            ActiveTab = "budget",
            VisibleTabs = _tabService.GetVisibleTabsForRole(currentRole),
            UserRole = currentRole,
            AvailableRoles = _tabService.GetAvailableRoles()
        };
    }

    /// <summary>
    /// District dashboard showing all budget submissions
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var district = await _context.Districts
            .Include(d => d.Esd)
            .FirstOrDefaultAsync(d => d.DistrictCode == DemoDistrictCode);

        if (district == null)
        {
            return View("NoData");
        }

        // For demo, create sample budget submissions
        var submissions = new List<BudgetSubmissionSummary>
        {
            new BudgetSubmissionSummary
            {
                SubmissionId = 1,
                SchoolYear = DemoSchoolYear,
                BudgetType = "Original",
                SubmissionStatus = "Approved",
                TotalRevenues = 45_250_000.00m,
                TotalExpenditures = 44_850_000.00m,
                FundBalance = 400_000.00m,
                ErrorCount = 0,
                WarningCount = 0,
                SubmittedDate = new DateTime(2024, 8, 15),
                IsLocked = true
            },
            new BudgetSubmissionSummary
            {
                SubmissionId = 2,
                SchoolYear = DemoSchoolYear,
                BudgetType = "Revised",
                SubmissionStatus = "Draft",
                TotalRevenues = 46_100_000.00m,
                TotalExpenditures = 45_950_000.00m,
                FundBalance = 150_000.00m,
                ErrorCount = 0,
                WarningCount = 3,
                SubmittedDate = null,
                IsLocked = false
            }
        };

        var viewModel = new BudgetDashboardViewModel
        {
            Tabs = GetTabViewModel(),
            District = district,
            Submissions = submissions,
            SchoolYear = DemoSchoolYear
        };

        ViewBag.InstructionsJson = JsonSerializer.Serialize(_instructionService.GetInstructions("Budget", "Index"));
        return View(viewModel);
    }

    /// <summary>
    /// View/edit a specific budget submission
    /// </summary>
    public async Task<IActionResult> Details(int id)
    {
        var district = await _context.Districts
            .Include(d => d.Esd)
            .FirstOrDefaultAsync(d => d.DistrictCode == DemoDistrictCode);

        // Create demo budget data rows
        var dataRows = GetDemoBudgetData();

        // Create demo edits
        var edits = new List<BudgetEdit>
        {
            new BudgetEdit
            {
                EditId = 1,
                EditRuleId = "BUD-003",
                Severity = "Warning",
                Message = "Certificated salaries increased 12% from prior year",
                FieldName = "Object 100",
                ActualValue = "Prior: $18,500,000 → Current: $20,720,000"
            },
            new BudgetEdit
            {
                EditId = 2,
                EditRuleId = "BUD-003",
                Severity = "Warning",
                Message = "Pupil transportation increased 15% from prior year",
                FieldName = "Activity 51",
                ActualValue = "Prior: $2,100,000 → Current: $2,415,000"
            },
            new BudgetEdit
            {
                EditId = 3,
                EditRuleId = "BUD-005",
                Severity = "Warning",
                Message = "Technology supplies exceeds typical range",
                FieldName = "Object 450",
                ActualValue = "$850,000 (typical range: $200,000-$600,000)"
            }
        };

        var submission = new BudgetSubmission
        {
            SubmissionId = id,
            DistrictCode = DemoDistrictCode,
            FiscalYear = DemoSchoolYear,
            FormType = id == 1 ? "Original" : "Revised",
            SubmissionStatus = id == 1 ? "Approved" : "Draft",
            IsLocked = id == 1,
            District = district
        };

        var viewModel = new BudgetSubmissionViewModel
        {
            Tabs = GetTabViewModel(),
            Submission = submission,
            DataRows = dataRows,
            Edits = edits,
            CanEdit = !submission.IsLocked && submission.SubmissionStatus != "Approved",
            CanSubmit = !submission.IsLocked && submission.SubmissionStatus == "Draft",
            FundCodes = GetFundCodes(),
            ProgramCodes = GetProgramCodes(),
            ActivityCodes = GetActivityCodes(),
            ObjectCodes = GetObjectCodes()
        };

        ViewBag.InstructionsJson = JsonSerializer.Serialize(_instructionService.GetInstructions("Budget", "Details"));
        return View(viewModel);
    }

    /// <summary>
    /// File upload page for budget data
    /// </summary>
    public async Task<IActionResult> Upload()
    {
        var district = await _context.Districts
            .FirstOrDefaultAsync(d => d.DistrictCode == DemoDistrictCode);

        var viewModel = new BudgetUploadViewModel
        {
            Tabs = GetTabViewModel(),
            SchoolYear = DemoSchoolYear,
            DistrictName = district?.DistrictName ?? "Tumwater School District"
        };

        ViewBag.InstructionsJson = JsonSerializer.Serialize(_instructionService.GetInstructions("Budget", "Upload"));
        return View(viewModel);
    }

    /// <summary>
    /// Process uploaded budget file
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile file, string budgetType)
    {
        var result = new BudgetUploadResult();

        // Validate file
        var (isValid, error) = _fileParser.ValidateFile(file);
        if (!isValid)
        {
            result.Success = false;
            result.Message = error ?? "Invalid file.";
            return View("UploadResult", result);
        }

        // Use transaction for atomic save
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Create new submission record
            var submission = new BudgetSubmission
            {
                DistrictCode = DemoDistrictCode,
                FiscalYear = DemoSchoolYear,
                FormType = string.IsNullOrEmpty(budgetType) ? "Original" : budgetType,
                SubmissionStatus = "Draft",
                CreatedDate = DateTime.UtcNow
            };
            _context.BudgetSubmissions.Add(submission);
            await _context.SaveChangesAsync();

            // Parse the file
            var parseResult = await _fileParser.ParseAsync(file, submission.SubmissionId);

            if (!parseResult.Success)
            {
                await transaction.RollbackAsync();
                result.Success = false;
                result.Message = "Failed to parse file.";
                result.Errors = parseResult.Errors;
                return View("UploadResult", result);
            }

            // Save parsed data
            if (parseResult.Data.Any())
            {
                _context.BudgetData.AddRange(parseResult.Data);
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            // Build success result
            result.Success = true;
            result.Message = $"File '{file.FileName}' processed successfully.";
            result.RecordsProcessed = parseResult.RecordsProcessed;
            result.TotalRevenues = parseResult.Data.Where(d => d.Amount > 0).Sum(d => d.Amount);
            result.TotalExpenditures = parseResult.Data.Where(d => d.Amount < 0).Sum(d => Math.Abs(d.Amount));
            result.RevenueRecords = parseResult.Data.Count(d => d.Amount > 0);
            result.ExpenditureRecords = parseResult.Data.Count(d => d.Amount < 0);
            result.Warnings = parseResult.Warnings;
            result.WarningCount = parseResult.WarningCount;
            result.SubmissionId = submission.SubmissionId;

            _logger.LogInformation("Budget file uploaded: {FileName} for type {BudgetType}, {RecordCount} records across {YearCount} fiscal years",
                file.FileName, budgetType, parseResult.RecordsProcessed,
                parseResult.Data.Select(d => d.FiscalYear).Distinct().Count());

            return View("UploadResult", result);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error processing budget upload");
            result.Success = false;
            result.Message = "An error occurred processing the file.";
            result.Errors.Add(ex.Message);
            return View("UploadResult", result);
        }
    }

    /// <summary>
    /// Save budget data (AJAX endpoint)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> SaveData([FromBody] BudgetDataRow data)
    {
        try
        {
            // For demo, just return success
            _logger.LogInformation("Budget data saved: {BudgetDataId} Amount: {Amount}",
                data.BudgetDataId, data.Amount);

            return Task.FromResult<IActionResult>(Json(new { success = true }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving budget data");
            return Task.FromResult<IActionResult>(Json(new { success = false, message = ex.Message }));
        }
    }

    /// <summary>
    /// Submit budget for approval
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Submit(int submissionId)
    {
        // For demo, just redirect with success message
        TempData["Success"] = "Budget submission has been submitted for approval.";
        return RedirectToAction(nameof(Details), new { id = submissionId });
    }

    /// <summary>
    /// Add comment to an edit/warning
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddComment(int editId, string comment)
    {
        // For demo, just return success
        return Json(new { success = true });
    }

    #region Demo Data Helpers

    private List<BudgetDataRow> GetDemoBudgetData()
    {
        return new List<BudgetDataRow>
        {
            // Revenues
            new BudgetDataRow
            {
                BudgetDataId = 1,
                FundCode = "10",
                FundName = "General Fund",
                ProgramCode = "00",
                ProgramName = "Regular Instruction",
                ActivityCode = "00",
                ActivityName = "General",
                ObjectCode = "100",
                ObjectName = "Local Taxes",
                Description = "Property tax revenue",
                Amount = 15_500_000.00m,
                PriorYearAmount = 14_800_000.00m,
                IsRevenue = true
            },
            new BudgetDataRow
            {
                BudgetDataId = 2,
                FundCode = "10",
                FundName = "General Fund",
                ProgramCode = "00",
                ProgramName = "Regular Instruction",
                ActivityCode = "00",
                ActivityName = "General",
                ObjectCode = "300",
                ObjectName = "State Apportionment",
                Description = "State basic education funding",
                Amount = 28_100_000.00m,
                PriorYearAmount = 26_500_000.00m,
                IsRevenue = true
            },
            new BudgetDataRow
            {
                BudgetDataId = 3,
                FundCode = "10",
                FundName = "General Fund",
                ProgramCode = "00",
                ProgramName = "Regular Instruction",
                ActivityCode = "00",
                ActivityName = "General",
                ObjectCode = "500",
                ObjectName = "Federal Grants",
                Description = "Title I, IDEA, etc.",
                Amount = 2_500_000.00m,
                PriorYearAmount = 2_350_000.00m,
                IsRevenue = true
            },

            // Expenditures
            new BudgetDataRow
            {
                BudgetDataId = 10,
                FundCode = "10",
                FundName = "General Fund",
                ProgramCode = "01",
                ProgramName = "Basic Education",
                ActivityCode = "21",
                ActivityName = "Supervision",
                ObjectCode = "100",
                ObjectName = "Certificated Salaries",
                Description = "Teacher salaries",
                Amount = 20_720_000.00m,
                PriorYearAmount = 18_500_000.00m,
                IsRevenue = false,
                HasWarning = true
            },
            new BudgetDataRow
            {
                BudgetDataId = 11,
                FundCode = "10",
                FundName = "General Fund",
                ProgramCode = "01",
                ProgramName = "Basic Education",
                ActivityCode = "21",
                ActivityName = "Supervision",
                ObjectCode = "200",
                ObjectName = "Classified Salaries",
                Description = "Support staff salaries",
                Amount = 8_200_000.00m,
                PriorYearAmount = 7_800_000.00m,
                IsRevenue = false
            },
            new BudgetDataRow
            {
                BudgetDataId = 12,
                FundCode = "10",
                FundName = "General Fund",
                ProgramCode = "01",
                ProgramName = "Basic Education",
                ActivityCode = "21",
                ActivityName = "Supervision",
                ObjectCode = "300",
                ObjectName = "Employee Benefits",
                Description = "Insurance, retirement",
                Amount = 9_500_000.00m,
                PriorYearAmount = 8_900_000.00m,
                IsRevenue = false
            },
            new BudgetDataRow
            {
                BudgetDataId = 13,
                FundCode = "10",
                FundName = "General Fund",
                ProgramCode = "01",
                ProgramName = "Basic Education",
                ActivityCode = "51",
                ActivityName = "Pupil Transportation",
                ObjectCode = "400",
                ObjectName = "Supplies & Materials",
                Description = "Fuel, maintenance supplies",
                Amount = 2_415_000.00m,
                PriorYearAmount = 2_100_000.00m,
                IsRevenue = false,
                HasWarning = true
            },
            new BudgetDataRow
            {
                BudgetDataId = 14,
                FundCode = "10",
                FundName = "General Fund",
                ProgramCode = "01",
                ProgramName = "Basic Education",
                ActivityCode = "61",
                ActivityName = "Maintenance",
                ObjectCode = "450",
                ObjectName = "Technology Supplies",
                Description = "Computers, software",
                Amount = 850_000.00m,
                PriorYearAmount = 450_000.00m,
                IsRevenue = false,
                HasWarning = true
            },
            new BudgetDataRow
            {
                BudgetDataId = 15,
                FundCode = "10",
                FundName = "General Fund",
                ProgramCode = "01",
                ProgramName = "Basic Education",
                ActivityCode = "61",
                ActivityName = "Maintenance",
                ObjectCode = "500",
                ObjectName = "Capital Outlay",
                Description = "Building improvements",
                Amount = 1_200_000.00m,
                PriorYearAmount = 1_150_000.00m,
                IsRevenue = false
            },
            new BudgetDataRow
            {
                BudgetDataId = 16,
                FundCode = "10",
                FundName = "General Fund",
                ProgramCode = "99",
                ProgramName = "Other",
                ActivityCode = "72",
                ActivityName = "Debt Service",
                ObjectCode = "600",
                ObjectName = "Debt Principal",
                Description = "Bond payments",
                Amount = 3_065_000.00m,
                PriorYearAmount = 3_000_000.00m,
                IsRevenue = false
            }
        };
    }

    private List<CodeLookup> GetFundCodes() => new()
    {
        new CodeLookup { Code = "10", Name = "General Fund" },
        new CodeLookup { Code = "20", Name = "Capital Projects Fund" },
        new CodeLookup { Code = "30", Name = "Debt Service Fund" },
        new CodeLookup { Code = "40", Name = "Associated Student Body Fund" },
        new CodeLookup { Code = "50", Name = "Transportation Vehicle Fund" }
    };

    private List<CodeLookup> GetProgramCodes() => new()
    {
        new CodeLookup { Code = "00", Name = "Regular Instruction" },
        new CodeLookup { Code = "01", Name = "Basic Education" },
        new CodeLookup { Code = "21", Name = "Special Education" },
        new CodeLookup { Code = "31", Name = "Vocational Education" },
        new CodeLookup { Code = "55", Name = "Learning Assistance" },
        new CodeLookup { Code = "97", Name = "District-wide Support" },
        new CodeLookup { Code = "99", Name = "Other" }
    };

    private List<CodeLookup> GetActivityCodes() => new()
    {
        new CodeLookup { Code = "00", Name = "General" },
        new CodeLookup { Code = "21", Name = "Supervision" },
        new CodeLookup { Code = "24", Name = "Guidance & Counseling" },
        new CodeLookup { Code = "27", Name = "Teaching" },
        new CodeLookup { Code = "51", Name = "Pupil Transportation" },
        new CodeLookup { Code = "61", Name = "Maintenance" },
        new CodeLookup { Code = "72", Name = "Debt Service" }
    };

    private List<CodeLookup> GetObjectCodes() => new()
    {
        new CodeLookup { Code = "100", Name = "Certificated Salaries" },
        new CodeLookup { Code = "200", Name = "Classified Salaries" },
        new CodeLookup { Code = "300", Name = "Employee Benefits" },
        new CodeLookup { Code = "400", Name = "Supplies & Materials" },
        new CodeLookup { Code = "450", Name = "Technology Supplies" },
        new CodeLookup { Code = "500", Name = "Capital Outlay" },
        new CodeLookup { Code = "600", Name = "Debt Principal" }
    };

    #endregion
}
