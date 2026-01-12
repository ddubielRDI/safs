using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Sasquatch.Core.Data;
using Sasquatch.Core.Models.Collection;
using Sasquatch.Core.Models.Shared;
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
    private readonly IInstructionService _instructionService;
    private readonly IEnrollmentFileParser _fileParser;

    // For demo, school year is fixed; district comes from session
    private const string DemoSchoolYear = "2024-25";

    // Washington State County-to-ESD mapping (39 counties, 9 ESDs)
    // County codes are the first 2 digits of CCDDD district codes
    private static readonly Dictionary<string, string> CountyToEsd = new()
    {
        // ESD 101 - Northeast Washington (Spokane region)
        { "01", "101" }, // Adams
        { "02", "101" }, // Asotin
        { "13", "101" }, // Columbia
        { "16", "101" }, // Ferry
        { "06", "101" }, // Garfield
        { "22", "101" }, // Lincoln
        { "28", "101" }, // Pend Oreille
        { "32", "101" }, // Spokane
        { "33", "101" }, // Stevens
        { "38", "101" }, // Whitman

        // ESD 105 - Yakima Region (Central)
        { "07", "105" }, // Chelan
        { "12", "105" }, // Douglas
        { "19", "105" }, // Grant
        { "20", "105" }, // Kittitas
        { "21", "105" }, // Klickitat
        { "25", "105" }, // Okanogan
        { "39", "105" }, // Yakima

        // ESD 112 - Southwest Washington (Vancouver)
        { "08", "112" }, // Clark
        { "10", "112" }, // Cowlitz
        { "36", "112" }, // Wahkiakum
        { "30", "112" }, // Skamania

        // ESD 113 - Capital Region (Olympia/Tumwater)
        { "18", "113" }, // Grays Harbor
        { "23", "113" }, // Lewis
        { "24", "113" }, // Mason
        { "26", "113" }, // Pacific
        { "34", "113" }, // Thurston (Tumwater)

        // ESD 114 - Olympic Region (Bremerton)
        { "09", "114" }, // Clallam
        { "15", "114" }, // Jefferson
        { "35", "114" }, // Kitsap

        // ESD 121 - Puget Sound (Seattle/King)
        { "17", "121" }, // King (Seattle) - using actual WA county code
        { "27", "121" }, // Pierce (Tacoma)

        // ESD 123 - Southeast Washington (Tri-Cities)
        { "03", "123" }, // Benton
        { "14", "123" }, // Franklin
        { "37", "123" }, // Walla Walla

        // ESD 171 - North Central (Wenatchee)
        // Note: Some counties shared with ESD 105

        // ESD 189 - Northwest Washington (Bellingham)
        { "29", "189" }, // San Juan
        { "31", "189" }, // Skagit
        { "04", "189" }, // Snohomish
        { "40", "189" }, // Whatcom
        { "11", "189" }, // Island
    };

    // Valid grade levels by school type for ENR-002 validation
    private static readonly Dictionary<string, HashSet<string>> ValidGradesBySchoolType = new()
    {
        { "Elementary", new HashSet<string> { "PK", "K", "01", "02", "03", "04", "05", "06" } },
        { "Middle", new HashSet<string> { "06", "07", "08" } },
        { "High", new HashSet<string> { "09", "10", "11", "12" } },
        // Alternative/Combined schools can have any grade
    };

    /// <summary>
    /// Get the current demo district from session
    /// </summary>
    private string GetCurrentDistrict() => _tabService.GetCurrentDistrict(HttpContext);

    public EnrollmentController(
        SasquatchDbContext context,
        ILogger<EnrollmentController> logger,
        IWorkflowTabService tabService,
        IInstructionService instructionService,
        IEnrollmentFileParser fileParser)
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
            .FirstOrDefaultAsync(d => d.DistrictCode == GetCurrentDistrict());

        if (district == null)
        {
            return View("NoData");
        }

        var submissions = await _context.EnrollmentSubmissions
            .Where(s => s.DistrictCode == GetCurrentDistrict() && s.SchoolYear == DemoSchoolYear)
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

        ViewBag.InstructionsJson = JsonSerializer.Serialize(_instructionService.GetInstructions("Enrollment", "Index"));
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

        ViewBag.InstructionsJson = JsonSerializer.Serialize(_instructionService.GetInstructions("Enrollment", "Details"));
        return View(viewModel);
    }

    /// <summary>
    /// File upload page for enrollment data
    /// </summary>
    public IActionResult Upload()
    {
        ViewBag.Tabs = GetTabViewModel();
        ViewBag.InstructionsJson = JsonSerializer.Serialize(_instructionService.GetInstructions("Enrollment", "Upload"));
        return View();
    }

    /// <summary>
    /// Process uploaded enrollment file
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile file, byte month)
    {
        var result = new EnrollmentUploadResult();

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
            var submission = new EnrollmentSubmission
            {
                DistrictCode = GetCurrentDistrict(),
                SchoolYear = DemoSchoolYear,
                Month = month > 0 ? month : (byte)2, // Default to October
                SubmissionStatus = "Draft",
                CreatedDate = DateTime.UtcNow
            };
            _context.EnrollmentSubmissions.Add(submission);
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
                _context.EnrollmentData.AddRange(parseResult.Data);
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            // Build success result
            result.Success = true;
            result.Message = $"File '{file.FileName}' processed successfully.";
            result.RecordsProcessed = parseResult.RecordsProcessed;
            result.Warnings = parseResult.Warnings;
            result.WarningCount = parseResult.WarningCount;
            result.SubmissionId = submission.SubmissionId;

            _logger.LogInformation("Enrollment file uploaded: {FileName} for month {Month}, {RecordCount} records",
                file.FileName, month, parseResult.RecordsProcessed);

            return View("UploadResult", result);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error processing enrollment upload");
            result.Success = false;
            result.Message = "An error occurred processing the file.";
            result.Errors.Add(ex.Message);
            return View("UploadResult", result);
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
                ?? new EnrollmentSubmission { DistrictCode = GetCurrentDistrict(), SchoolYear = DemoSchoolYear };
        }
        else
        {
            submission = new EnrollmentSubmission
            {
                DistrictCode = GetCurrentDistrict(),
                SchoolYear = DemoSchoolYear,
                Month = 2 // October (month 2 in school year)
            };
        }

        var schools = await _context.Schools
            .Where(s => s.DistrictCode == GetCurrentDistrict() && s.IsActive)
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

        ViewBag.InstructionsJson = JsonSerializer.Serialize(_instructionService.GetInstructions("Enrollment", "ManualEntry"));
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
    /// Delete an enrollment submission and all associated data
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int submissionId)
    {
        var submission = await _context.EnrollmentSubmissions
            .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

        if (submission == null)
        {
            return NotFound();
        }

        if (submission.IsLocked || submission.SubmissionStatus == "Approved")
        {
            TempData["Error"] = "Cannot delete locked or approved submissions.";
            return RedirectToAction(nameof(Index));
        }

        // Cascade delete: edits, data, then submission
        var edits = _context.EnrollmentEdits.Where(e => e.SubmissionId == submissionId);
        var data = _context.EnrollmentData.Where(d => d.SubmissionId == submissionId);

        _context.EnrollmentEdits.RemoveRange(edits);
        _context.EnrollmentData.RemoveRange(data);
        _context.EnrollmentSubmissions.Remove(submission);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Enrollment submission {SubmissionId} deleted", submissionId);
        TempData["Success"] = "Submission deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Bulk import page - upload statewide enrollment file
    /// </summary>
    public IActionResult BulkImport()
    {
        var viewModel = new BulkImportViewModel
        {
            Tabs = GetTabViewModel(),
            SchoolYear = DemoSchoolYear
        };
        ViewBag.InstructionsJson = JsonSerializer.Serialize(_instructionService.GetInstructions("Enrollment", "BulkImport"));
        return View(viewModel);
    }

    /// <summary>
    /// Process bulk import of statewide enrollment file
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50MB limit for large statewide files
    public async Task<IActionResult> BulkImport(IFormFile file, string schoolYear, byte month)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new BulkImportResultViewModel
        {
            Tabs = GetTabViewModel(),
            SchoolYear = schoolYear ?? DemoSchoolYear,
            Month = month > 0 ? month : (byte)2
        };

        // Validate file
        var (isValid, error) = _fileParser.ValidateFile(file);
        if (!isValid)
        {
            result.Success = false;
            result.Message = error ?? "Invalid file.";
            result.Duration = stopwatch.Elapsed;
            return View("BulkImportResult", result);
        }

        try
        {
            // Parse the file, grouping by district
            var parseResult = await _fileParser.ParseBulkAsync(file);

            if (!parseResult.Success)
            {
                result.Success = false;
                result.Message = "Failed to parse file.";
                result.Errors = parseResult.Errors;
                result.Duration = stopwatch.Elapsed;
                return View("BulkImportResult", result);
            }

            result.Warnings = parseResult.Warnings;

            // Process each district
            foreach (var kvp in parseResult.DistrictDataMap)
            {
                var districtCode = kvp.Key;
                var districtData = kvp.Value;
                var districtResult = new DistrictImportResult
                {
                    DistrictCode = districtCode,
                    DistrictName = districtData.DistrictName
                };

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Ensure district exists
                    var (district, districtWasCreated) = await EnsureDistrictAsync(districtCode, districtData.DistrictName);
                    if (district == null)
                    {
                        throw new Exception($"Failed to create district {districtCode}");
                    }
                    if (districtWasCreated)
                    {
                        result.DistrictsCreated++;
                    }

                    // Ensure schools exist
                    var schoolsCreated = 0;
                    foreach (var schoolRow in districtData.Schools)
                    {
                        var (school, schoolWasCreated) = await EnsureSchoolAsync(districtCode, schoolRow.SchoolCode, schoolRow.SchoolName);
                        if (schoolWasCreated)
                        {
                            schoolsCreated++;
                        }
                    }
                    districtResult.SchoolCount = districtData.Schools.Count;

                    // Create submission
                    var submission = new EnrollmentSubmission
                    {
                        DistrictCode = districtCode,
                        SchoolYear = result.SchoolYear,
                        Month = result.Month,
                        SubmissionStatus = "Draft",
                        CreatedDate = DateTime.UtcNow
                    };
                    _context.EnrollmentSubmissions.Add(submission);
                    await _context.SaveChangesAsync();

                    // Add enrollment data
                    var recordCount = 0;
                    foreach (var schoolRow in districtData.Schools)
                    {
                        foreach (var enrollmentRecord in schoolRow.EnrollmentRecords)
                        {
                            enrollmentRecord.SubmissionId = submission.SubmissionId;
                            _context.EnrollmentData.Add(enrollmentRecord);
                            recordCount++;
                        }
                    }
                    await _context.SaveChangesAsync();

                    // Populate prior month data for variance validation
                    await PopulatePriorMonthDataAsync(submission, districtCode, result.SchoolYear, result.Month);

                    // Run validation
                    await RunValidation(submission.SubmissionId);

                    // Get validation counts
                    var edits = await _context.EnrollmentEdits
                        .Where(e => e.SubmissionId == submission.SubmissionId)
                        .ToListAsync();
                    districtResult.WarningCount = edits.Count(e => e.Severity == "Warning");
                    districtResult.ErrorCount = edits.Count(e => e.Severity == "Error");

                    await transaction.CommitAsync();

                    districtResult.Success = true;
                    districtResult.SubmissionId = submission.SubmissionId;
                    districtResult.RecordCount = recordCount;
                    result.SubmissionsCreated++;
                    result.RecordsCreated += recordCount;
                    result.SchoolsCreated += schoolsCreated;

                    _logger.LogInformation("Bulk import: District {DistrictCode} - {RecordCount} records",
                        districtCode, recordCount);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    districtResult.Success = false;
                    districtResult.Error = ex.Message;
                    result.DistrictsFailed++;
                    _logger.LogError(ex, "Bulk import failed for district {DistrictCode}", districtCode);
                }

                result.DistrictResults.Add(districtResult);
            }

            // Determine success
            result.Success = result.DistrictsFailed == 0;
            result.Message = result.Success
                ? $"Successfully imported {result.SubmissionsCreated} districts with {result.RecordsCreated:N0} enrollment records."
                : $"Import completed with {result.DistrictsFailed} failures. {result.SubmissionsCreated} districts imported successfully.";

            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;

            _logger.LogInformation("Bulk import completed: {Districts} districts, {Records} records, {Duration}ms",
                result.SubmissionsCreated, result.RecordsCreated, result.Duration.TotalMilliseconds);

            return View("BulkImportResult", result);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Bulk import failed");
            result.Success = false;
            result.Message = $"Import failed: {ex.Message}";
            result.Duration = stopwatch.Elapsed;
            return View("BulkImportResult", result);
        }
    }

    /// <summary>
    /// Ensure a district exists, creating it if necessary
    /// Returns (district, wasCreated)
    /// </summary>
    private async Task<(District? District, bool WasCreated)> EnsureDistrictAsync(string districtCode, string districtName)
    {
        if (string.IsNullOrWhiteSpace(districtCode))
        {
            return (null, false);
        }

        var existing = await _context.Districts.FirstOrDefaultAsync(d => d.DistrictCode == districtCode);
        if (existing != null)
        {
            return (existing, false);
        }

        // Derive county code and ESD code from district code
        // CCDDD format: first 2 digits are county, DDD is district number
        var countyCode = districtCode.Length >= 2 ? districtCode.Substring(0, 2) : "00";

        // Look up ESD by county code, default to 113 (Capital Region) if not found
        var esdCode = CountyToEsd.TryGetValue(countyCode, out var mappedEsd) ? mappedEsd : "113";

        var district = new District
        {
            DistrictCode = districtCode,
            DistrictName = districtName ?? districtCode, // Fallback to code if name is null
            CountyCode = countyCode,
            EsdCode = esdCode,
            Class = 1,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        _context.Districts.Add(district);
        await _context.SaveChangesAsync();

        return (district, true);
    }

    /// <summary>
    /// Ensure a school exists, creating it if necessary
    /// Returns (school, wasCreated)
    /// </summary>
    private async Task<(School? School, bool WasCreated)> EnsureSchoolAsync(string districtCode, string schoolCode, string schoolName)
    {
        if (string.IsNullOrWhiteSpace(districtCode) || string.IsNullOrWhiteSpace(schoolCode))
        {
            return (null, false);
        }

        var existing = await _context.Schools
            .FirstOrDefaultAsync(s => s.DistrictCode == districtCode && s.SchoolCode == schoolCode);

        if (existing != null)
        {
            return (existing, false);
        }

        // Infer school type from name (with null safety)
        string? schoolType = null;
        var safeName = schoolName ?? "";
        if (safeName.Contains("Elementary", StringComparison.OrdinalIgnoreCase) ||
            safeName.Contains("Primary", StringComparison.OrdinalIgnoreCase))
        {
            schoolType = "Elementary";
        }
        else if (safeName.Contains("Middle", StringComparison.OrdinalIgnoreCase) ||
                 safeName.Contains("Junior", StringComparison.OrdinalIgnoreCase))
        {
            schoolType = "Middle";
        }
        else if (safeName.Contains("High", StringComparison.OrdinalIgnoreCase))
        {
            schoolType = "High";
        }

        var school = new School
        {
            SchoolCode = schoolCode,
            DistrictCode = districtCode,
            SchoolName = schoolName ?? $"School {schoolCode}", // Fallback to code if name is null
            SchoolType = schoolType,
            IsActive = true
        };
        _context.Schools.Add(school);
        await _context.SaveChangesAsync();

        return (school, true);
    }

    /// <summary>
    /// Get the prior school year (e.g., "2024-25" -> "2023-24")
    /// </summary>
    private static string GetPriorSchoolYear(string schoolYear)
    {
        if (string.IsNullOrEmpty(schoolYear) || schoolYear.Length < 7)
            return schoolYear;

        // Parse "2024-25" format
        if (int.TryParse(schoolYear.Substring(0, 4), out var startYear))
        {
            return $"{startYear - 1}-{(startYear - 1) % 100:D2}";
        }
        return schoolYear;
    }

    /// <summary>
    /// Populate prior month headcount/FTE data for variance validation
    /// Queries the prior month's submission and matches records by school/grade/program
    /// </summary>
    private async Task PopulatePriorMonthDataAsync(EnrollmentSubmission submission, string districtCode, string schoolYear, byte month)
    {
        // Determine prior month and year
        var priorMonth = month == 1 ? (byte)12 : (byte)(month - 1);
        var priorYear = month == 1 ? GetPriorSchoolYear(schoolYear) : schoolYear;

        // Find prior month submission for this district
        var priorSubmission = await _context.EnrollmentSubmissions
            .Include(s => s.EnrollmentData)
            .FirstOrDefaultAsync(s => s.DistrictCode == districtCode
                && s.SchoolYear == priorYear
                && s.Month == priorMonth
                && s.SubmissionStatus != "Rejected");

        if (priorSubmission == null)
        {
            _logger.LogInformation("No prior month submission found for district {DistrictCode}, {SchoolYear} month {Month}",
                districtCode, priorYear, priorMonth);
            return;
        }

        // Null safety for EnrollmentData collection
        if (priorSubmission.EnrollmentData == null || !priorSubmission.EnrollmentData.Any())
        {
            _logger.LogWarning("Prior submission {SubmissionId} has no enrollment data", priorSubmission.SubmissionId);
            return;
        }

        // Build lookup by composite key: SchoolCode + GradeLevel + ProgramType
        // Filter out records with null keys to prevent dictionary exceptions
        var priorDataLookup = priorSubmission.EnrollmentData
            .Where(d => !string.IsNullOrEmpty(d.SchoolCode) && !string.IsNullOrEmpty(d.GradeLevel))
            .ToDictionary(
                d => (d.SchoolCode, d.GradeLevel, d.ProgramType ?? ""),
                d => (d.Headcount, d.FTE));

        // Reload current submission data to update
        var currentData = await _context.EnrollmentData
            .Where(d => d.SubmissionId == submission.SubmissionId)
            .ToListAsync();

        var matchCount = 0;
        foreach (var data in currentData)
        {
            // Use same key format as lookup (null ProgramType -> empty string)
            var key = (data.SchoolCode, data.GradeLevel, data.ProgramType ?? "");
            if (priorDataLookup.TryGetValue(key, out var prior))
            {
                data.PriorMonthHeadcount = prior.Headcount;
                data.PriorMonthFTE = prior.FTE;
                matchCount++;
            }
        }

        if (matchCount > 0)
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Populated prior month data for {MatchCount} records in district {DistrictCode}",
                matchCount, districtCode);
        }
    }

    /// <summary>
    /// Run validation rules on a submission
    /// Implements all 7 ENR rules per EditRules table
    /// </summary>
    private async Task RunValidation(int submissionId)
    {
        var submission = await _context.EnrollmentSubmissions
            .Include(s => s.EnrollmentData)
            .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

        if (submission == null) return;

        // Clear existing edits
        var existingEdits = await _context.EnrollmentEdits
            .Where(e => e.SubmissionId == submissionId)
            .ToListAsync();
        _context.EnrollmentEdits.RemoveRange(existingEdits);

        // Pre-load school types for ENR-002 validation
        var schoolCodes = submission.EnrollmentData.Select(d => d.SchoolCode).Distinct().ToList();
        var schoolTypes = await _context.Schools
            .Where(s => schoolCodes.Contains(s.SchoolCode))
            .ToDictionaryAsync(s => s.SchoolCode, s => s.SchoolType);

        // Apply all 7 ENR validation rules
        foreach (var data in submission.EnrollmentData)
        {
            // ENR-001: Month-over-month headcount variance > 10% (Warning)
            if (data.PriorMonthHeadcount.HasValue && data.PriorMonthHeadcount.Value > 0)
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
                        FieldValue = $"{data.Headcount} (was {data.PriorMonthHeadcount})",
                        CreatedDate = DateTime.UtcNow
                    });
                }
            }

            // ENR-002: Invalid grade for school type (Error)
            if (schoolTypes.TryGetValue(data.SchoolCode, out var schoolType) && !string.IsNullOrEmpty(schoolType))
            {
                if (ValidGradesBySchoolType.TryGetValue(schoolType, out var validGrades))
                {
                    // Normalize grade (e.g., "K" stays "K", "1" -> "01")
                    var normalizedGrade = data.GradeLevel.Length == 1 && char.IsDigit(data.GradeLevel[0])
                        ? data.GradeLevel.PadLeft(2, '0')
                        : data.GradeLevel;

                    if (!validGrades.Contains(normalizedGrade) && !validGrades.Contains(data.GradeLevel))
                    {
                        _context.EnrollmentEdits.Add(new EnrollmentEdit
                        {
                            SubmissionId = submissionId,
                            EditRuleId = "ENR-002",
                            Severity = "Error",
                            Message = $"Grade {data.GradeLevel} is invalid for {schoolType} school",
                            FieldName = "GradeLevel",
                            FieldValue = $"Grade: {data.GradeLevel}, School Type: {schoolType}",
                            CreatedDate = DateTime.UtcNow
                        });
                    }
                }
            }

            // ENR-003: FTE exceeds headcount (Error)
            if (data.FTE > data.Headcount)
            {
                _context.EnrollmentEdits.Add(new EnrollmentEdit
                {
                    SubmissionId = submissionId,
                    EditRuleId = "ENR-003",
                    Severity = "Error",
                    Message = "FTE cannot exceed headcount",
                    FieldName = "FTE",
                    FieldValue = $"FTE: {data.FTE}, Headcount: {data.Headcount}",
                    CreatedDate = DateTime.UtcNow
                });
            }

            // ENR-004: Negative values (Error)
            if (data.Headcount < 0 || data.FTE < 0)
            {
                _context.EnrollmentEdits.Add(new EnrollmentEdit
                {
                    SubmissionId = submissionId,
                    EditRuleId = "ENR-004",
                    Severity = "Error",
                    Message = "Headcount and FTE must be non-negative",
                    FieldName = data.Headcount < 0 ? "Headcount" : "FTE",
                    FieldValue = $"Headcount: {data.Headcount}, FTE: {data.FTE}",
                    CreatedDate = DateTime.UtcNow
                });
            }

            // ENR-005: Suspicious FTE values
            // Check if FTE equals Headcount exactly (common data entry error - FTE rarely equals headcount exactly)
            // Also check if FTE has unusual decimal pattern suggesting truncated headcount
            if (data.Headcount > 0 && data.FTE > 0)
            {
                // If FTE exactly equals Headcount, it's suspicious (part-time students would make FTE < HC)
                if ((int)data.FTE == data.Headcount && data.FTE == Math.Floor(data.FTE))
                {
                    _context.EnrollmentEdits.Add(new EnrollmentEdit
                    {
                        SubmissionId = submissionId,
                        EditRuleId = "ENR-005",
                        Severity = "Warning",
                        Message = "FTE equals Headcount exactly - verify this is correct (typically FTE < Headcount)",
                        FieldName = "FTE",
                        FieldValue = $"Headcount: {data.Headcount}, FTE: {data.FTE}",
                        CreatedDate = DateTime.UtcNow
                    });
                }
            }

            // ENR-006: Large enrollment drop > 25% (Warning)
            if (data.PriorMonthHeadcount.HasValue && data.PriorMonthHeadcount.Value > 0)
            {
                var changePercent = (decimal)(data.Headcount - data.PriorMonthHeadcount.Value) / data.PriorMonthHeadcount.Value * 100;
                if (changePercent < -25)
                {
                    _context.EnrollmentEdits.Add(new EnrollmentEdit
                    {
                        SubmissionId = submissionId,
                        EditRuleId = "ENR-006",
                        Severity = "Warning",
                        Message = $"Large enrollment decrease of {Math.Abs(changePercent):F1}% from prior month",
                        FieldName = "Headcount",
                        FieldValue = $"{data.Headcount} (was {data.PriorMonthHeadcount})",
                        CreatedDate = DateTime.UtcNow
                    });
                }
            }

            // ENR-007: Missing school code (Error)
            if (string.IsNullOrWhiteSpace(data.SchoolCode))
            {
                _context.EnrollmentEdits.Add(new EnrollmentEdit
                {
                    SubmissionId = submissionId,
                    EditRuleId = "ENR-007",
                    Severity = "Error",
                    Message = "School code is required",
                    FieldName = "SchoolCode",
                    FieldValue = "(empty)",
                    CreatedDate = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();
    }
}
