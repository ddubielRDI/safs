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
/// Controller for enrollment data collection (P-223 forms)
/// Demo Section 1: Data Collection
/// </summary>
[Area("Collection")]
public class EnrollmentController : Controller
{
    private readonly SasquatchDbContext _context;
    private readonly ILogger<EnrollmentController> _logger;
    private readonly IWorkflowTabService _tabService;

    // For demo, we'll use Tumwater district
    private const string DemoDistrictCode = "34033";
    private const string DemoSchoolYear = "2024-25";

    public EnrollmentController(
        SasquatchDbContext context,
        ILogger<EnrollmentController> logger,
        IWorkflowTabService tabService)
    {
        _context = context;
        _logger = logger;
        _tabService = tabService;
    }

    /// <summary>
    /// Get the workflow tab view model for the current user
    /// </summary>
    private WorkflowTabViewModel GetTabViewModel()
    {
        var currentRole = _tabService.GetCurrentRole(HttpContext);
        return new WorkflowTabViewModel
        {
            ActiveTab = "enrollment",
            VisibleTabs = _tabService.GetVisibleTabsForRole(currentRole),
            UserRole = currentRole,
            AvailableRoles = _tabService.GetAvailableRoles()
        };
    }

    /// <summary>
    /// District dashboard showing all enrollment submissions
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

        var submissions = await _context.EnrollmentSubmissions
            .Where(s => s.DistrictCode == DemoDistrictCode && s.SchoolYear == DemoSchoolYear)
            .OrderByDescending(s => s.Month)
            .Select(s => new EnrollmentSubmissionSummary
            {
                SubmissionId = s.SubmissionId,
                SchoolYear = s.SchoolYear,
                Month = s.Month,
                SubmissionStatus = s.SubmissionStatus,
                TotalHeadcount = s.EnrollmentData.Sum(d => d.Headcount),
                TotalFTE = s.EnrollmentData.Sum(d => d.FTE),
                ErrorCount = s.EnrollmentEdits.Count(e => e.Severity == "Error" && !e.IsResolved),
                WarningCount = s.EnrollmentEdits.Count(e => e.Severity == "Warning" && !e.IsResolved),
                SubmittedDate = s.SubmittedDate,
                IsLocked = s.IsLocked
            })
            .ToListAsync();

        var viewModel = new EnrollmentDashboardViewModel
        {
            Tabs = GetTabViewModel(),
            District = district,
            Submissions = submissions,
            SchoolYear = DemoSchoolYear
        };

        return View(viewModel);
    }

    /// <summary>
    /// View/edit a specific enrollment submission
    /// </summary>
    public async Task<IActionResult> Details(int id)
    {
        var submission = await _context.EnrollmentSubmissions
            .Include(s => s.District)
            .Include(s => s.EnrollmentEdits)
            .FirstOrDefaultAsync(s => s.SubmissionId == id);

        if (submission == null)
        {
            return NotFound();
        }

        var dataRows = await _context.EnrollmentData
            .Where(d => d.SubmissionId == id)
            .Join(_context.Schools, d => d.SchoolCode, s => s.SchoolCode, (d, s) => new EnrollmentDataRow
            {
                EnrollmentId = d.EnrollmentId,
                SchoolCode = d.SchoolCode,
                SchoolName = s.SchoolName,
                SchoolType = s.SchoolType ?? "",
                GradeLevel = d.GradeLevel,
                ProgramType = d.ProgramType,
                Headcount = d.Headcount,
                FTE = d.FTE,
                PriorMonthHeadcount = d.PriorMonthHeadcount,
                PriorMonthFTE = d.PriorMonthFTE
            })
            .OrderBy(d => d.SchoolName)
            .ThenBy(d => d.GradeLevel)
            .ThenBy(d => d.ProgramType)
            .ToListAsync();

        // Mark rows with warnings/errors
        var edits = submission.EnrollmentEdits.Where(e => !e.IsResolved).ToList();
        foreach (var row in dataRows)
        {
            row.HasError = edits.Any(e => e.Severity == "Error" && e.FieldValue?.Contains(row.SchoolCode) == true);
            row.HasWarning = edits.Any(e => e.Severity == "Warning");
        }

        var schools = await _context.Schools
            .Where(s => s.DistrictCode == submission.DistrictCode && s.IsActive)
            .OrderBy(s => s.SchoolName)
            .ToListAsync();

        var viewModel = new EnrollmentSubmissionViewModel
        {
            Tabs = GetTabViewModel(),
            Submission = submission,
            DataRows = dataRows,
            Edits = submission.EnrollmentEdits.OrderByDescending(e => e.Severity).ToList(),
            Schools = schools,
            CanEdit = !submission.IsLocked && submission.SubmissionStatus != "Approved",
            CanSubmit = !submission.IsLocked && submission.SubmissionStatus == "Draft"
        };

        return View(viewModel);
    }

    /// <summary>
    /// File upload page for enrollment data
    /// </summary>
    public IActionResult Upload()
    {
        ViewBag.Tabs = GetTabViewModel();
        return View();
    }

    /// <summary>
    /// Process uploaded enrollment file
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Upload(IFormFile file, byte month)
    {
        var result = new EnrollmentUploadResult();

        if (file == null || file.Length == 0)
        {
            result.Success = false;
            result.Message = "Please select a file to upload.";
            return Task.FromResult<IActionResult>(View("UploadResult", result));
        }

        try
        {
            // For demo, we'll simulate file processing
            // In production, this would parse CSV/Excel and validate

            result.Success = true;
            result.Message = $"File '{file.FileName}' processed successfully.";
            result.RecordsProcessed = 50; // Demo value
            result.WarningCount = 2;

            // Log the upload
            _logger.LogInformation("Enrollment file uploaded: {FileName} for month {Month}", file.FileName, month);

            return Task.FromResult<IActionResult>(View("UploadResult", result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing enrollment upload");
            result.Success = false;
            result.Message = "An error occurred processing the file.";
            result.Errors.Add(ex.Message);
            return Task.FromResult<IActionResult>(View("UploadResult", result));
        }
    }

    /// <summary>
    /// Manual data entry page
    /// </summary>
    public async Task<IActionResult> ManualEntry(int? submissionId)
    {
        EnrollmentSubmission submission;

        if (submissionId.HasValue)
        {
            submission = await _context.EnrollmentSubmissions
                .Include(s => s.District)
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId.Value)
                ?? new EnrollmentSubmission { DistrictCode = DemoDistrictCode, SchoolYear = DemoSchoolYear };
        }
        else
        {
            submission = new EnrollmentSubmission
            {
                DistrictCode = DemoDistrictCode,
                SchoolYear = DemoSchoolYear,
                Month = 2 // October (month 2 in school year)
            };
        }

        var schools = await _context.Schools
            .Where(s => s.DistrictCode == DemoDistrictCode && s.IsActive)
            .OrderBy(s => s.SchoolType)
            .ThenBy(s => s.SchoolName)
            .ToListAsync();

        var viewModel = new EnrollmentSubmissionViewModel
        {
            Tabs = GetTabViewModel(),
            Submission = submission,
            Schools = schools,
            CanEdit = true,
            CanSubmit = true
        };

        return View(viewModel);
    }

    /// <summary>
    /// Save enrollment data (AJAX endpoint)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveData([FromBody] EnrollmentDataRow data)
    {
        try
        {
            var enrollment = await _context.EnrollmentData
                .FirstOrDefaultAsync(e => e.EnrollmentId == data.EnrollmentId);

            if (enrollment == null)
            {
                return Json(new { success = false, message = "Record not found" });
            }

            // Update values
            enrollment.Headcount = data.Headcount;
            enrollment.FTE = data.FTE;

            await _context.SaveChangesAsync();

            // Re-run validation
            await RunValidation(enrollment.SubmissionId);

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving enrollment data");
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Submit enrollment for approval
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(int submissionId)
    {
        var submission = await _context.EnrollmentSubmissions
            .Include(s => s.EnrollmentEdits)
            .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

        if (submission == null)
        {
            return NotFound();
        }

        // Check for blocking errors
        var blockingErrors = submission.EnrollmentEdits
            .Where(e => e.Severity == "Error" && !e.IsResolved)
            .ToList();

        if (blockingErrors.Any())
        {
            TempData["Error"] = $"Cannot submit: {blockingErrors.Count} unresolved error(s) must be fixed first.";
            return RedirectToAction(nameof(Details), new { id = submissionId });
        }

        submission.SubmissionStatus = "Submitted";
        submission.SubmittedBy = "tumwater.admin"; // Demo user
        submission.SubmittedDate = DateTime.UtcNow;
        submission.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Enrollment submission has been submitted for approval.";
        return RedirectToAction(nameof(Details), new { id = submissionId });
    }

    /// <summary>
    /// Add comment to an edit/warning
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(int editId, string comment)
    {
        var edit = await _context.EnrollmentEdits.FindAsync(editId);

        if (edit == null)
        {
            return Json(new { success = false, message = "Edit not found" });
        }

        edit.DistrictComment = comment;
        edit.IsResolved = true;
        edit.ResolvedBy = "tumwater.admin";
        edit.ResolvedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Json(new { success = true });
    }

    /// <summary>
    /// Run validation rules on a submission
    /// </summary>
    private async Task RunValidation(int submissionId)
    {
        var submission = await _context.EnrollmentSubmissions
            .Include(s => s.EnrollmentData)
            .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

        if (submission == null) return;

        // Get active edit rules
        var rules = await _context.EditRules
            .Where(r => r.FormType == "P-223" && r.IsActive)
            .ToListAsync();

        // Clear existing edits
        var existingEdits = await _context.EnrollmentEdits
            .Where(e => e.SubmissionId == submissionId)
            .ToListAsync();
        _context.EnrollmentEdits.RemoveRange(existingEdits);

        // Apply rules
        foreach (var data in submission.EnrollmentData)
        {
            // ENR-001: Month-over-month variance check
            if (data.PriorMonthHeadcount > 0)
            {
                var variance = Math.Abs((decimal)(data.Headcount - data.PriorMonthHeadcount.Value) / data.PriorMonthHeadcount.Value * 100);
                if (variance > 10)
                {
                    _context.EnrollmentEdits.Add(new EnrollmentEdit
                    {
                        SubmissionId = submissionId,
                        EditRuleId = "ENR-001",
                        Severity = "Warning",
                        Message = $"Headcount changed by {variance:F1}% from prior month",
                        FieldName = "Headcount",
                        FieldValue = $"{data.Headcount} (was {data.PriorMonthHeadcount})"
                    });
                }
            }

            // ENR-003: FTE exceeds headcount
            if (data.FTE > data.Headcount)
            {
                _context.EnrollmentEdits.Add(new EnrollmentEdit
                {
                    SubmissionId = submissionId,
                    EditRuleId = "ENR-003",
                    Severity = "Error",
                    Message = "FTE cannot exceed headcount",
                    FieldName = "FTE",
                    FieldValue = $"FTE: {data.FTE}, Headcount: {data.Headcount}"
                });
            }
        }

        await _context.SaveChangesAsync();
    }
}
