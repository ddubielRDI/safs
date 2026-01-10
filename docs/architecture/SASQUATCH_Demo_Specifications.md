# SASQUATCH RFP Demo Analysis & Specifications

## Executive Summary

**Project:** Washington State SASQUATCH (School Apportionment and Financial Systems)
**Client:** OSPI (Office of Superintendent of Public Instruction)
**Purpose:** Win contract by demonstrating working software for school apportionment system modernization

### Team Decisions
- **Scope:** All 3 Work Sections
- **Technology:** .NET/Azure (ASP.NET Core + Azure SQL + Blazor/React)
- **Timeline:** 1-2 months for demo preparation

### Demo Format
- **Duration:** 2 hours total (includes 30-45 minutes Q&A)
- **Scoring:** 300 points per work section (900 points max)
- **Data:** Tumwater School District, 2024-25 (District Code 34033)
- **Data Source:** https://ospi.k12.wa.us/safs-data-files
- **Format:** Working software preferred; small stand-alone functional units acceptable

---

# DEMO SECTION 1: DATA COLLECTION

## Overview
Demonstrate interfaces for school districts to submit enrollment and budget data, with validation, correction workflows, and OSPI administrative controls.

## Demo Acceptance Criteria (from RFP)

### 1.1 Enrollment Upload Example
| Requirement | Demo Must Show |
|-------------|----------------|
| Electronic upload interface | District can upload monthly enrollment file |
| By resident district | Data includes resident district breakdown |
| By school level | Enrollment broken down by individual schools |
| Manual entry | District can type/edit enrollment data directly |
| Limited technical support | UI is intuitive, self-explanatory |

### 1.2 Enrollment Validation Scenarios
| Scenario | What to Demonstrate |
|----------|---------------------|
| Month-to-month comparison | System compares current month to prior month |
| Statistical significance detection | System flags unusual variances automatically |
| District correction workflow | Districts can review edits, make corrections |
| Comment submission | Districts can submit explanations for variances |

### 1.3 Budget Upload Example
| Requirement | Demo Must Show |
|-------------|----------------|
| Electronic upload | District uploads monthly financial data file |
| Completeness checks | System validates all required fields present |
| Month-to-month comparison | Budget data compared to prior month |
| Statistical significance | Flags significant month-over-month changes |
| Unreasonable amount detection | Flags values outside expected ranges |
| Program/Activity/Object validation | Validates valid code combinations |

### 1.4 OSPI Admin Interface
| Requirement | Demo Must Show |
|-------------|----------------|
| District data review | OSPI user can view all district submissions |
| Regional (ESD) view | Group districts by Educational Service District |
| State-level view | Aggregate view across all districts |
| Approval workflow | OSPI can approve submitted data sets |

### 1.5 Data Lock Controls
| Requirement | Demo Must Show |
|-------------|----------------|
| Lock all districts | Prevent all submissions during calculation |
| Lock subset | Lock specific districts or ESDs |
| Lock single district | Lock one district for audit |
| Monthly lock | Lock for monthly calculation processing |
| Annual lock | Lock for year-end audit purposes |

## Technical Specifications

### Enrollment Data Model
```
Enrollment (P-223)
├── DistrictCode (CCDDD - e.g., 34033 for Tumwater)
├── SchoolCode
├── Month (September=1 through August=12)
├── SchoolYear (e.g., 2024-25)
├── GradeLevel (K, 1-12)
├── Headcount (integer)
├── FTE (decimal, 2 places)
├── ResidentDistrictCode
├── ProgramType (Basic Ed, Running Start, Open Doors, ALE, etc.)
└── SubmissionStatus (Draft, Submitted, Approved, Locked)
```

### Budget Data Model
```
Budget (F-195/F-200)
├── DistrictCode
├── FiscalYear
├── FundCode (General, Capital, Debt Service, ASB, Transportation)
├── ProgramCode
├── ActivityCode
├── ObjectCode
├── Amount (decimal)
├── PriorMonthAmount
├── Variance
├── VariancePercent
└── SubmissionStatus
```

### Validation Rules Engine
```csharp
// Example edit rules structure
public class EditRule
{
    public string RuleId { get; set; }           // e.g., "ENR-001"
    public string Description { get; set; }
    public EditSeverity Severity { get; set; }   // Error, Warning, Info
    public string Formula { get; set; }          // Calculation logic
    public decimal Threshold { get; set; }       // Variance threshold
    public bool BlocksSubmission { get; set; }
}

// Month-over-month variance check
public EditResult CheckMonthOverMonth(Enrollment current, Enrollment prior)
{
    var variance = (current.Headcount - prior.Headcount) / prior.Headcount;
    if (Math.Abs(variance) > 0.10) // 10% threshold
        return new EditResult {
            Triggered = true,
            Message = $"Headcount changed by {variance:P1} from prior month"
        };
}
```

### UI Components Required
1. **File Upload Component**
   - Drag-and-drop zone
   - CSV/Excel format detection
   - Progress indicator
   - Validation summary

2. **Data Entry Grid**
   - Editable cells
   - Calculated field highlighting (gray background)
   - Validation error indicators (red borders)
   - Save/Save and Return buttons

3. **Edit Review Panel**
   - List of triggered edits by severity
   - Explanation text box for each edit
   - Bulk acknowledge option

4. **OSPI Dashboard**
   - District submission status grid
   - Filter by ESD, status, date
   - Lock/Unlock controls
   - Approval workflow buttons

## Demo Script Outline (Section 1)

**Duration Target:** 25-30 minutes (leaves time for Q&A)

1. **Enrollment Upload (5 min)**
   - Show file upload interface
   - Upload Tumwater enrollment CSV
   - Display validation results
   - Show month-over-month comparison

2. **Enrollment Manual Entry (3 min)**
   - Navigate to manual entry screen
   - Edit a headcount value
   - Show validation trigger
   - Submit correction with comment

3. **Budget Upload (5 min)**
   - Upload Tumwater budget file
   - Show completeness validation
   - Show unreasonable amount detection
   - Show program/activity/object validation

4. **OSPI Review Interface (5 min)**
   - Switch to OSPI user role
   - Show district overview dashboard
   - Filter by ESD (show Tumwater's ESD grouping)
   - Review Tumwater submission
   - Approve submission

5. **Lock Controls (3 min)**
   - Demonstrate lock for single district
   - Show district cannot submit when locked
   - Unlock district

---

# DEMO SECTION 2: DATA CALCULATION

## Overview
Demonstrate apportionment calculation engine with Production and Sandbox environments, supporting what-if scenarios for OSPI, districts, and legislature.

## Demo Acceptance Criteria (from RFP)

### 2.1 Production Environment
| Requirement | Demo Must Show |
|-------------|----------------|
| Live data tables | Data flows from Collection after validation |
| OSPI adjustments | OSPI can modify data before calculation |
| Run calculations | Execute apportionment for all/subset/single district |
| Audit trail | Log all adjustments to calculations, constants, data |

### 2.2 Sandbox Environment
| Requirement | Demo Must Show |
|-------------|----------------|
| Copy from production | Users can copy data, constants, formulae |
| Three user populations | OSPI, Districts, Legislature all have access |
| Multiple scenarios | Users can create many scenarios simultaneously |
| View multiple scenarios | Compare scenarios side-by-side |
| Compare to production | View scenario vs. production differences |

## Technical Specifications

### Calculation Engine Architecture
```
Production Environment
├── State Constants Table (inflation rates, enrollment factors, etc.)
├── District Data Tables (enrollment, staff ratios, etc.)
├── Formula Repository (apportionment calculations)
├── Calculation Results
└── Audit Log

Sandbox Environment
├── Scenario Metadata (owner, created date, description)
├── Cloned Constants (modifiable copy)
├── Cloned Data (modifiable copy)
├── Modified Formulae (if applicable)
├── Scenario Results
└── Comparison Views
```

### State Constants Example
```csharp
public class StateConstant
{
    public string ConstantId { get; set; }
    public string Name { get; set; }
    public decimal Value { get; set; }
    public string SchoolYear { get; set; }
    public DateTime EffectiveDate { get; set; }
}

// Examples for demo:
// - Base Allocation per Pupil: $5,500
// - K-3 Class Size Factor: 1.2
// - Special Education Weight: 0.9309
// - Running Start Funding Rate: 0.85
// - Levy Equalization Factor: varies by district
```

### Apportionment Calculation (Simplified)
```csharp
public class ApportionmentCalculation
{
    public decimal Calculate(District district, StateConstants constants)
    {
        // Basic Education Allocation
        var basicEd = district.BasicEdFTE * constants.BaseAllocationPerPupil;

        // K-3 Class Size Enhancement
        var k3Enhancement = district.K3FTE * constants.K3ClassSizeFactor;

        // Special Education
        var specialEd = district.SpecialEdFTE * constants.SpecialEdWeight
                       * constants.BaseAllocationPerPupil;

        // Running Start (community college)
        var runningStart = district.RunningStartFTE * constants.RunningStartRate
                          * constants.BaseAllocationPerPupil;

        // Total Apportionment
        return basicEd + k3Enhancement + specialEd + runningStart;
    }
}
```

### Audit Trail Model
```csharp
public class AuditEntry
{
    public Guid AuditId { get; set; }
    public DateTime Timestamp { get; set; }
    public string UserId { get; set; }
    public string EntityType { get; set; }      // "StateConstant", "DistrictData", "Formula"
    public string EntityId { get; set; }
    public string FieldName { get; set; }
    public string OldValue { get; set; }
    public string NewValue { get; set; }
    public string Reason { get; set; }
}
```

### Sandbox Scenario Model
```csharp
public class Scenario
{
    public Guid ScenarioId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string OwnerId { get; set; }
    public UserPopulation OwnerType { get; set; }  // OSPI, District, Legislature
    public DateTime CreatedDate { get; set; }
    public Guid BaselineSnapshotId { get; set; }   // Production snapshot copied from

    // Modifiable data
    public List<ScenarioConstant> ModifiedConstants { get; set; }
    public List<ScenarioDistrictData> ModifiedData { get; set; }

    // Results
    public decimal TotalApportionment { get; set; }
    public decimal VarianceFromProduction { get; set; }
}

public enum UserPopulation { OSPI, District, Legislature }
```

### UI Components Required

1. **Production Dashboard**
   - District list with calculation status
   - Run calculation buttons (All, ESD, Single District)
   - Calculation progress indicator
   - Results summary

2. **Adjustment Interface**
   - Data grid with editable values
   - Reason for adjustment (required field)
   - Before/After comparison
   - Save with audit logging

3. **Sandbox Manager**
   - Scenario list with metadata
   - Create new scenario button
   - Clone from production
   - Delete scenario

4. **Scenario Editor**
   - Constants modification grid
   - Data overrides
   - Run calculation in sandbox
   - Compare to production

5. **Comparison View**
   - Side-by-side scenario results
   - Variance highlighting
   - Export comparison

## Demo Script Outline (Section 2)

**Duration Target:** 25-30 minutes

1. **Production Overview (3 min)**
   - Show production data tables
   - Display state constants
   - Show current calculation status

2. **Run Calculation (5 min)**
   - Select Tumwater district
   - Execute apportionment calculation
   - Display results breakdown
   - Show calculation completed in seconds (vs. legacy 4-6 hours)

3. **OSPI Adjustment (5 min)**
   - Make adjustment to Tumwater data
   - Enter reason for adjustment
   - Re-run calculation
   - Show audit trail entry

4. **Create Sandbox Scenario (5 min)**
   - Create new scenario as Legislature user
   - Copy current production data
   - Name scenario "FY25 Budget Proposal"

5. **Modify Scenario (5 min)**
   - Increase base allocation by 5%
   - Modify K-3 class size factor
   - Run sandbox calculation
   - Show impact on Tumwater funding

6. **Compare Scenarios (5 min)**
   - Create second scenario with different assumptions
   - View both scenarios side-by-side
   - Compare to production
   - Export comparison report

---

# DEMO SECTION 3: DATA REPORTING

## Overview
Demonstrate report generation capabilities including monthly enrollment, annual budget, financial statements, Excel exports, and API integration.

## Demo Acceptance Criteria (from RFP)

| Requirement | Demo Must Show |
|-------------|----------------|
| Monthly enrollment reports | Generate P-223 monthly enrollment report |
| Annual budget report | Generate F-195 budget projections |
| Annual financial statement | Generate F-196 actuals report |
| Excel export | Users can export data to Excel for ad-hoc analysis |
| API integration | Native functionality to share data with external systems |

## Technical Specifications

### Report Types

#### 1. Monthly Enrollment Report (P-223)
```
Report: District Monthly Enrollment Summary
Filters: District, Month, School Year
Columns:
- School Name
- Grade Level
- Headcount (prior month)
- Headcount (current month)
- Change
- FTE (prior month)
- FTE (current month)
- Change
- Program Type breakdown
```

#### 2. Annual Budget Report (F-195)
```
Report: District Budget Summary
Filters: District, Fiscal Year, Fund
Sections:
- Beginning Fund Balance
- Revenues by Source
  - Local (taxes, fees)
  - State (apportionment)
  - Federal (grants)
- Expenditures by Program
  - Basic Education
  - Special Education
  - Career/Technical
  - Compensatory
- Ending Fund Balance
```

#### 3. Annual Financial Statement (F-196)
```
Report: End-of-Year Financial Statement
Filters: District, Fiscal Year
Sections:
- Actual Revenues (vs. budgeted)
- Actual Expenditures (vs. budgeted)
- Variance Analysis
- Fund Balance Changes
- Audit Adjustments (if any)
```

### Export Formats
| Format | Use Case |
|--------|----------|
| PDF | Official reports, public posting |
| Excel (.xlsx) | Ad-hoc analysis, data manipulation |
| CSV | Data integration, bulk analysis |
| XML | System integration |
| Web Archive | Archival |

### API Specification
```yaml
openapi: 3.0.0
info:
  title: SAFS Data API
  version: 1.0.0
paths:
  /api/v1/districts/{districtCode}/enrollment:
    get:
      summary: Get district enrollment data
      parameters:
        - name: districtCode
          in: path
          required: true
          schema:
            type: string
        - name: schoolYear
          in: query
          schema:
            type: string
        - name: month
          in: query
          schema:
            type: integer
      responses:
        '200':
          description: Enrollment data
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/EnrollmentResponse'

  /api/v1/districts/{districtCode}/apportionment:
    get:
      summary: Get district apportionment calculation results

  /api/v1/districts/{districtCode}/budget:
    get:
      summary: Get district budget data

  /api/v1/reports/generate:
    post:
      summary: Generate a report
      requestBody:
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/ReportRequest'
```

### UI Components Required

1. **Report Selector**
   - Report type dropdown
   - Filter parameters
   - Date range selector
   - District/ESD selector
   - Generate button

2. **Report Viewer**
   - Paginated display
   - Print preview
   - Export buttons (PDF, Excel, CSV)
   - ADA-compliant formatting

3. **Export Manager**
   - Batch export queue
   - Progress indicator
   - Download completed exports

4. **API Documentation Portal**
   - Swagger/OpenAPI UI
   - Authentication guide
   - Example requests
   - Rate limiting info

## Demo Script Outline (Section 3)

**Duration Target:** 20-25 minutes

1. **Monthly Enrollment Report (5 min)**
   - Select P-223 report
   - Filter for Tumwater, October 2024
   - Generate report
   - Show formatted output
   - Export to PDF

2. **Annual Budget Report (5 min)**
   - Select F-195 report
   - Filter for Tumwater, 2024-25
   - Generate budget projections
   - Show revenue/expenditure breakdown
   - Export to Excel

3. **Financial Statement (3 min)**
   - Select F-196 report
   - Generate actuals report
   - Show variance from budget

4. **Excel Ad-Hoc Export (3 min)**
   - Show export data function
   - Select multiple data fields
   - Export raw data for analysis
   - Open in Excel to demonstrate

5. **API Integration (5 min)**
   - Open API documentation portal
   - Show available endpoints
   - Execute sample API call
   - Display JSON response
   - Explain integration patterns

---

# IMPLEMENTATION ROADMAP

## Phase 1: Foundation (Week 1-2)
- [ ] Set up Azure environment
- [ ] Create database schema
- [ ] Load Tumwater sample data
- [ ] Build authentication framework

## Phase 2: Data Collection (Week 2-3)
- [ ] File upload component
- [ ] Validation engine
- [ ] Edit rules configuration
- [ ] District entry screens
- [ ] OSPI dashboard

## Phase 3: Data Calculation (Week 3-4)
- [ ] Calculation engine
- [ ] Production data flow
- [ ] Audit logging
- [ ] Sandbox creation
- [ ] Scenario comparison

## Phase 4: Data Reporting (Week 4-5)
- [ ] Report generator
- [ ] PDF/Excel export
- [ ] API endpoints
- [ ] Documentation portal

## Phase 5: Integration & Polish (Week 5-6)
- [ ] End-to-end data flow
- [ ] UI polish
- [ ] Demo script rehearsal
- [ ] Q&A preparation

---

# KEY PAIN POINTS TO ADDRESS IN DEMO

The RFP reveals these major issues with the legacy system:

| Current Pain Point | How Our Demo Addresses It |
|-------------------|---------------------------|
| Calculations take 4-6 hours | Show calculation completing in seconds |
| No subset calculations | Demonstrate single district calculation |
| Excel-based processes | Show automated data flows |
| No sandbox for legislature | Demonstrate scenario modeling |
| Non-ADA compliant reports | Generate accessible PDF |
| Manual notifications | Show automated alerts |
| 16-bit macro dependencies | Modern web-based solution |
| No audit trails | Show full audit logging |
| Access database reliance | SQL Server/Azure SQL |

---

# APPENDIX: TUMWATER DATA STRUCTURE

**District Code:** 34033
**County:** Thurston (34)
**ESD:** ESD 113 (Capital Region)

Typical schools to include in demo data:
- Tumwater High School (grades 9-12)
- Black Hills High School (grades 9-12)
- Tumwater Middle School (grades 6-8)
- Bush Middle School (grades 6-8)
- Multiple Elementary Schools (grades K-5)

---

*Analysis Date: 2026-01-09*
*Status: Ready for team review - specifications complete*
