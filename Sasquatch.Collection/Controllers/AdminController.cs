using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sasquatch.Core.Data;
using Sasquatch.Core.Models.Shared;
using Sasquatch.Collection.ViewModels;

namespace Sasquatch.Collection.Controllers;

/// <summary>
/// OSPI Administrative dashboard and controls
/// Demo Section 1: Data Collection - OSPI Admin Interface
/// </summary>
[Area("Collection")]
public class AdminController : Controller
{
    private readonly SasquatchDbContext _context;
    private readonly ILogger<AdminController> _logger;

    private const string DemoSchoolYear = "2024-25";

    public AdminController(SasquatchDbContext context, ILogger<AdminController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// OSPI Dashboard - overview of all district submissions
    /// </summary>
    public async Task<IActionResult> Index(string? esdCode = null, byte? month = null)
    {
        var query = _context.Districts
            .Include(d => d.Esd)
            .Where(d => d.IsActive);

        if (!string.IsNullOrEmpty(esdCode))
        {
            query = query.Where(d => d.EsdCode == esdCode);
        }

        var districts = await query.ToListAsync();

        var districtStatuses = new List<DistrictSubmissionStatus>();

        foreach (var district in districts)
        {
            var enrollmentSubmission = await _context.EnrollmentSubmissions
                .Where(s => s.DistrictCode == district.DistrictCode
                         && s.SchoolYear == DemoSchoolYear
                         && (month == null || s.Month == month))
                .OrderByDescending(s => s.Month)
                .FirstOrDefaultAsync();

            var budgetSubmission = await _context.BudgetSubmissions
                .Where(s => s.DistrictCode == district.DistrictCode
                         && s.FiscalYear == DemoSchoolYear)
                .OrderByDescending(s => s.SubmittedDate)
                .FirstOrDefaultAsync();

            districtStatuses.Add(new DistrictSubmissionStatus
            {
                DistrictCode = district.DistrictCode,
                DistrictName = district.DistrictName,
                EsdCode = district.EsdCode,
                EsdName = district.Esd?.EsdName ?? "Unknown",
                EnrollmentStatus = enrollmentSubmission?.SubmissionStatus ?? "Not Started",
                BudgetStatus = budgetSubmission?.SubmissionStatus ?? "Not Started",
                IsLocked = enrollmentSubmission?.IsLocked ?? false,
                LastSubmission = enrollmentSubmission?.SubmittedDate ?? budgetSubmission?.SubmittedDate
            });
        }

        var esds = await _context.ESDs.OrderBy(e => e.EsdName).ToListAsync();

        // Calculate state-level aggregates (SAFS-002: State-level view)
        var enrollmentQuery = _context.EnrollmentSubmissions
            .Where(s => s.SchoolYear == DemoSchoolYear);

        if (!string.IsNullOrEmpty(esdCode))
        {
            var esdDistricts = await _context.Districts
                .Where(d => d.EsdCode == esdCode)
                .Select(d => d.DistrictCode)
                .ToListAsync();
            enrollmentQuery = enrollmentQuery.Where(s => esdDistricts.Contains(s.DistrictCode));
        }

        if (month.HasValue)
        {
            enrollmentQuery = enrollmentQuery.Where(s => s.Month == month.Value);
        }

        var submissionIds = await enrollmentQuery.Select(s => s.SubmissionId).ToListAsync();

        var aggregates = await _context.EnrollmentData
            .Where(d => submissionIds.Contains(d.SubmissionId))
            .GroupBy(d => 1)
            .Select(g => new
            {
                TotalHeadcount = g.Sum(d => d.Headcount),
                TotalFTE = g.Sum(d => d.FTE),
                SchoolCount = g.Select(d => d.SchoolCode).Distinct().Count()
            })
            .FirstOrDefaultAsync();

        var viewModel = new OspiDashboardViewModel
        {
            DistrictStatuses = districtStatuses.OrderBy(d => d.EsdName).ThenBy(d => d.DistrictName).ToList(),
            ESDs = esds,
            SelectedEsdCode = esdCode,
            SelectedMonth = month,
            SchoolYear = DemoSchoolYear,
            TotalDistricts = districtStatuses.Count,
            SubmittedCount = districtStatuses.Count(d => d.EnrollmentStatus == "Submitted"),
            ApprovedCount = districtStatuses.Count(d => d.EnrollmentStatus == "Approved"),
            PendingCount = districtStatuses.Count(d => d.EnrollmentStatus == "Draft" || d.EnrollmentStatus == "Not Started"),
            // State-level aggregates
            StatewideHeadcount = aggregates?.TotalHeadcount ?? 0,
            StatewideFTE = aggregates?.TotalFTE ?? 0,
            StatewideSchoolCount = aggregates?.SchoolCount ?? 0,
            StatewideSubmissionCount = submissionIds.Count
        };

        return View(viewModel);
    }

    /// <summary>
    /// View details for a specific district
    /// </summary>
    public async Task<IActionResult> DistrictDetail(string districtCode)
    {
        var district = await _context.Districts
            .Include(d => d.Esd)
            .Include(d => d.Schools)
            .FirstOrDefaultAsync(d => d.DistrictCode == districtCode);

        if (district == null)
        {
            return NotFound();
        }

        var enrollmentSubmissions = await _context.EnrollmentSubmissions
            .Where(s => s.DistrictCode == districtCode && s.SchoolYear == DemoSchoolYear)
            .OrderByDescending(s => s.Month)
            .ToListAsync();

        ViewBag.District = district;
        ViewBag.EnrollmentSubmissions = enrollmentSubmissions;

        return View(district);
    }

    /// <summary>
    /// Approve a district's submission
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveSubmission(int submissionId, string returnUrl)
    {
        var submission = await _context.EnrollmentSubmissions
            .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

        if (submission == null)
        {
            return NotFound();
        }

        submission.SubmissionStatus = "Approved";
        submission.ApprovedBy = "ospi.admin"; // Demo user
        submission.ApprovedDate = DateTime.UtcNow;
        submission.ModifiedDate = DateTime.UtcNow;

        // Log to audit
        _context.AuditLogs.Add(new AuditLog
        {
            TableName = "EnrollmentSubmissions",
            RecordId = submissionId,
            Action = "UPDATE",
            FieldName = "SubmissionStatus",
            OldValue = "Submitted",
            NewValue = "Approved",
            ChangedBy = "ospi.admin",
            Reason = "OSPI approval"
        });

        await _context.SaveChangesAsync();

        TempData["Success"] = $"Submission {submissionId} approved successfully.";

        if (!string.IsNullOrEmpty(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Lock controls page
    /// </summary>
    public async Task<IActionResult> Locks()
    {
        var activeLocks = await _context.DataLocks
            .Where(l => l.IsActive)
            .OrderByDescending(l => l.LockedDate)
            .ToListAsync();

        var esds = await _context.ESDs.OrderBy(e => e.EsdName).ToListAsync();
        var districts = await _context.Districts.Where(d => d.IsActive).OrderBy(d => d.DistrictName).ToListAsync();

        ViewBag.ActiveLocks = activeLocks;
        ViewBag.ESDs = esds;
        ViewBag.Districts = districts;

        return View();
    }

    /// <summary>
    /// Create a new data lock
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLock(string lockScope, string? scopeValue, string lockType, string? reason)
    {
        var newLock = new DataLock
        {
            LockScope = lockScope,
            ScopeValue = scopeValue,
            LockType = lockType,
            SchoolYear = DemoSchoolYear,
            LockedBy = "ospi.admin",
            LockedDate = DateTime.UtcNow,
            Reason = reason,
            IsActive = true
        };

        _context.DataLocks.Add(newLock);

        // Apply lock to affected submissions
        var submissionsQuery = _context.EnrollmentSubmissions
            .Where(s => s.SchoolYear == DemoSchoolYear);

        if (lockScope == "District" && !string.IsNullOrEmpty(scopeValue))
        {
            submissionsQuery = submissionsQuery.Where(s => s.DistrictCode == scopeValue);
        }
        else if (lockScope == "ESD" && !string.IsNullOrEmpty(scopeValue))
        {
            var esdDistricts = await _context.Districts
                .Where(d => d.EsdCode == scopeValue)
                .Select(d => d.DistrictCode)
                .ToListAsync();
            submissionsQuery = submissionsQuery.Where(s => esdDistricts.Contains(s.DistrictCode));
        }

        var submissions = await submissionsQuery.ToListAsync();
        foreach (var sub in submissions)
        {
            sub.IsLocked = true;
            sub.LockedBy = "ospi.admin";
            sub.LockedDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = $"Lock created successfully. {submissions.Count} submission(s) affected.";
        return RedirectToAction(nameof(Locks));
    }

    /// <summary>
    /// Remove a data lock
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveLock(int lockId)
    {
        var dataLock = await _context.DataLocks.FindAsync(lockId);

        if (dataLock == null)
        {
            return NotFound();
        }

        dataLock.IsActive = false;
        dataLock.UnlockedBy = "ospi.admin";
        dataLock.UnlockedDate = DateTime.UtcNow;

        // Unlock affected submissions
        var submissionsQuery = _context.EnrollmentSubmissions
            .Where(s => s.SchoolYear == DemoSchoolYear && s.IsLocked);

        if (dataLock.LockScope == "District" && !string.IsNullOrEmpty(dataLock.ScopeValue))
        {
            submissionsQuery = submissionsQuery.Where(s => s.DistrictCode == dataLock.ScopeValue);
        }
        else if (dataLock.LockScope == "ESD" && !string.IsNullOrEmpty(dataLock.ScopeValue))
        {
            var esdDistricts = await _context.Districts
                .Where(d => d.EsdCode == dataLock.ScopeValue)
                .Select(d => d.DistrictCode)
                .ToListAsync();
            submissionsQuery = submissionsQuery.Where(s => esdDistricts.Contains(s.DistrictCode));
        }

        var submissions = await submissionsQuery.ToListAsync();
        foreach (var sub in submissions)
        {
            sub.IsLocked = false;
            sub.LockedBy = null;
            sub.LockedDate = null;
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = $"Lock removed. {submissions.Count} submission(s) unlocked.";
        return RedirectToAction(nameof(Locks));
    }

    /// <summary>
    /// Audit log viewer
    /// </summary>
    public async Task<IActionResult> AuditLog(string? tableName = null, int page = 1)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (!string.IsNullOrEmpty(tableName))
        {
            query = query.Where(a => a.TableName == tableName);
        }

        var totalCount = await query.CountAsync();
        var pageSize = 50;
        var logs = await query
            .OrderByDescending(a => a.ChangedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.TotalCount = totalCount;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TableName = tableName;

        return View(logs);
    }
}
