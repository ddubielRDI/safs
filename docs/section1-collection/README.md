# Section 1: Data Collection

## Overview
Electronic interfaces for school districts to submit enrollment (P-223) and budget (F-195) data with validation, correction workflows, and OSPI administrative controls.

## Implementation Status
- **Enrollment Upload:** Implemented
- **Manual Entry:** Implemented
- **Validation Engine:** Implemented
- **OSPI Dashboard:** Implemented
- **Lock Controls:** Implemented
- **Budget Upload:** Placeholder

## Key Components

### Controllers
- `EnrollmentController` - District enrollment submission and editing
- `AdminController` - OSPI administrative dashboard

### Models (in Sasquatch.Core)
- `EnrollmentSubmission` - Monthly enrollment submission header
- `EnrollmentData` - Individual enrollment records
- `EnrollmentEdit` - Validation warnings/errors
- `BudgetSubmission` - Budget submission header
- `BudgetData` - Budget line items
- `BudgetEdit` - Budget validation results

### Views
- `/Collection/Enrollment/Index` - District dashboard
- `/Collection/Enrollment/Details` - Submission detail/edit
- `/Collection/Enrollment/Upload` - File upload interface
- `/Collection/Admin/Index` - OSPI dashboard
- `/Collection/Admin/Locks` - Lock controls

## Demo Script

### Duration: 25-30 minutes

1. **Enrollment Upload (5 min)**
   - Navigate to `/Collection/Enrollment/Upload`
   - Upload Tumwater enrollment CSV
   - View validation results
   - Show month-over-month comparison

2. **Manual Entry & Correction (5 min)**
   - Edit enrollment values inline
   - Trigger validation warning
   - Add explanation comment
   - Submit for approval

3. **OSPI Review (5 min)**
   - Navigate to `/Collection/Admin`
   - Filter by ESD
   - Review Tumwater submission
   - Approve submission

4. **Lock Controls (3 min)**
   - Navigate to `/Collection/Admin/Locks`
   - Create district lock
   - Verify district cannot submit
   - Remove lock

## Validation Rules

| Rule ID | Description | Severity |
|---------|-------------|----------|
| ENR-001 | Month-over-month headcount variance > 10% | Warning |
| ENR-002 | Missing school data | Error |
| ENR-003 | FTE exceeds headcount | Error |
| BUD-001 | Revenue variance > 5% from prior month | Warning |
| BUD-002 | Invalid program/activity/object combination | Error |

## Data Files
Sample files for demo are in `/data/collection/`:
- `tumwater_enrollment_oct2024.csv`
- `tumwater_budget_2024.xlsx`
