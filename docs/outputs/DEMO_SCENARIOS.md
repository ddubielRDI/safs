# SASQUATCH Demonstration Scenarios Specification

**SASQUATCH** - School Apportionment System for Quality, Accountability, Transparency, and Calculations Hub

**Version:** 1.0
**Date:** January 19, 2026
**Status:** Draft for RFP Response
**Classification:** Public

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Demonstration Overview](#2-demonstration-overview)
3. [Test Data Specifications](#3-test-data-specifications)
4. [Section 1: Data Collection Scenarios](#4-section-1-data-collection-scenarios)
5. [Section 2: Data Calculation Scenarios](#5-section-2-data-calculation-scenarios)
6. [Section 3: Data Reporting Scenarios](#6-section-3-data-reporting-scenarios)
7. [Cross-Module Integration Points](#7-cross-module-integration-points)
8. [Evaluation Rubric](#8-evaluation-rubric)
9. [Appendices](#9-appendices)

---

## 1. Executive Summary

### 1.1 Purpose

This document provides detailed demonstration scenarios for the SASQUATCH system as required by OSPI RFP No. 2026-12 Attachment C. Each scenario is designed to showcase working software functionality using real-world Washington State education data.

### 1.2 Demonstration Objectives

| Objective | Description |
|-----------|-------------|
| **Functional Validation** | Demonstrate working software meeting RFP requirements |
| **User Experience** | Show intuitive interfaces requiring minimal training |
| **Data Integrity** | Prove validation, audit trails, and data quality controls |
| **Integration Capability** | Illustrate module interconnections and external interfaces |
| **Scalability** | Evidence of handling statewide data volumes |

### 1.3 Scoring Summary

| Section | Maximum Points | Time Allocation |
|---------|---------------|-----------------|
| Data Collection | 300 | ~45 minutes |
| Data Calculation | 300 | ~45 minutes |
| Data Reporting | 300 | ~45 minutes |
| Q&A Session | - | 30-45 minutes |
| **Total** | **900** | **2 hours** |

---

## 2. Demonstration Overview

### 2.1 Format and Logistics

| Aspect | Specification |
|--------|---------------|
| **Duration** | 2 hours per work section |
| **Q&A Time** | 30-45 minutes reserved |
| **Delivery Method** | Online via Microsoft Teams (preferred) or in-person |
| **Recording** | Not recorded (protects consultant IP) |
| **Focus** | Working software demonstration, not background/credentials |

### 2.2 Demonstration Principles

Per RFP guidance:

1. **Working Software Priority**: Demonstrate functional software over mockups or process flows
2. **Stand-Alone Units**: End-to-end flow not required; small functional units acceptable
3. **Real Data**: Must use publicly available SAFS data files
4. **Connective Tissue**: Explain interfaces between modules even if demonstrating only one section

### 2.3 Key Success Factors

- Clear demonstration of each acceptance criterion
- Intuitive user interface requiring minimal technical training
- Visible data validation and error handling
- Traceable audit trails
- Responsive performance under realistic data volumes

---

## 3. Test Data Specifications

### 3.1 Required Data Set

| Parameter | Value |
|-----------|-------|
| **District** | Tumwater School District |
| **District Code** | 34033 |
| **County** | Thurston |
| **ESD** | ESD 113 (Capital Region) |
| **School Year** | 2024-25 |
| **Data Source** | https://ospi.k12.wa.us/safs-data-files |

### 3.2 Tumwater School District Profile

| Metric | Approximate Value |
|--------|-------------------|
| **Total Enrollment** | ~6,500 students |
| **Schools** | ~15 schools |
| **Grade Levels** | K-12 |
| **Annual Budget** | ~$90 million |
| **Staff FTE** | ~800 |

### 3.3 Required Data Files

| Form | Description | Demo Usage |
|------|-------------|------------|
| **P-223** | Monthly Enrollment Report | Enrollment scenarios |
| **P-223H** | Special Education Enrollment | Enrollment validation |
| **P-223RS** | Running Start Enrollment | Cross-district scenarios |
| **F-195** | Annual Budget | Budget upload scenarios |
| **F-200** | Budget Extension/Revision | Budget revision scenarios |
| **F-196** | Annual Financial Statement | Financial reporting |
| **F-197** | Monthly Treasurer Report | Monthly financial data |
| **F-203** | Revenue Estimation | Calculation scenarios |
| **S-275** | Personnel Report | Staff data validation |

### 3.4 Data Preparation Checklist

- [ ] Download all required forms from OSPI SAFS Data Files
- [ ] Verify data integrity and completeness for 2024-25 school year
- [ ] Prepare intentional error scenarios for validation demonstrations
- [ ] Create comparison data sets (prior year, prior month)
- [ ] Establish baseline calculations for verification

---

## 4. Section 1: Data Collection Scenarios

### Scenario DC-1: Enrollment Data Upload

**Scenario ID:** DC-1-ENR-UPLOAD
**Category:** Enrollment Collection
**Points:** Up to 75

#### Acceptance Criteria

| ID | Criterion | Validation Method |
|----|-----------|-------------------|
| AC-1.1 | District can upload monthly enrollment electronically | File upload completion |
| AC-1.2 | Upload includes resident district breakdown | Data field verification |
| AC-1.3 | Upload includes school-level detail | Record count validation |
| AC-1.4 | System provides upload confirmation | UI confirmation message |
| AC-1.5 | System logs upload activity | Audit trail display |

#### Step-by-Step Walkthrough

**Pre-Conditions:**
- User authenticated as Tumwater SD district administrator
- P-223 enrollment file prepared for September 2024

**Steps:**

| Step | Actor | Action | Expected Result |
|------|-------|--------|-----------------|
| 1 | District User | Navigate to Data Collection > Enrollment > Monthly Upload | Upload interface displays with district context |
| 2 | District User | Select reporting month: September 2024 | Form fields update for selected period |
| 3 | District User | Click "Upload File" and select P-223 file | File browser opens; file selected |
| 4 | System | Validate file format and structure | Format validation passes; progress indicator shows |
| 5 | System | Parse enrollment data by school and grade | Data preview table populates |
| 6 | District User | Review parsed data showing: Total K-12 enrollment by school, Resident district breakdown, Special program counts | All data fields visible and accurate |
| 7 | District User | Click "Submit for Validation" | System initiates validation rules |
| 8 | System | Run enrollment validation rules | Validation results display with any warnings |
| 9 | District User | Review validation summary | Green checkmarks for passed validations |
| 10 | System | Log submission with timestamp and user ID | Audit entry created and viewable |

**Expected Outcomes:**
- Enrollment data successfully loaded into system
- School-level breakdown visible (e.g., Black Lake Elementary: 425, Bush Middle: 678, etc.)
- Resident district counts captured for cross-district students
- Upload logged with timestamp, user, filename, record count

**Test Data Requirements:**
- P-223 file for Tumwater SD, September 2024
- Minimum 10 schools with enrollment data
- At least 3 resident district codes represented
- Both regular and special education counts

**Validation Points:**
- [ ] File format validation (CSV/Excel structure)
- [ ] Required field completeness check
- [ ] District code verification (34033)
- [ ] School code cross-reference validation
- [ ] Numeric range checks on enrollment counts
- [ ] Grade level sum verification

---

### Scenario DC-2: Manual Enrollment Entry and Revision

**Scenario ID:** DC-2-ENR-MANUAL
**Category:** Enrollment Collection
**Points:** Up to 75

#### Acceptance Criteria

| ID | Criterion | Validation Method |
|----|-----------|-------------------|
| AC-2.1 | District can manually enter enrollment data | Data entry completion |
| AC-2.2 | District can revise previously submitted data | Edit history preserved |
| AC-2.3 | Interface requires minimal technical training | Intuitive navigation |
| AC-2.4 | System preserves version history | Version comparison display |
| AC-2.5 | Original submissions remain accessible for audit | Historical data retrieval |

#### Step-by-Step Walkthrough

**Pre-Conditions:**
- User authenticated as Tumwater SD district user
- September 2024 enrollment previously uploaded (from DC-1)

**Steps:**

| Step | Actor | Action | Expected Result |
|------|-------|--------|-----------------|
| 1 | District User | Navigate to Data Collection > Enrollment > Manual Entry | Manual entry grid displays |
| 2 | District User | Select school: Tumwater High School | School context established |
| 3 | District User | Select month: September 2024 | Existing data populates grid |
| 4 | District User | Modify Grade 10 count from 298 to 302 | Cell highlights as modified |
| 5 | District User | Add revision comment: "Late enrollments from 9/28" | Comment field accepts text |
| 6 | District User | Click "Save Draft" | Draft saved; status shows "Draft" |
| 7 | District User | Review changes summary showing original vs. modified values | Side-by-side comparison visible |
| 8 | District User | Click "Submit Revision" | Confirmation dialog appears |
| 9 | System | Create new version; preserve original | Version 2 created; Version 1 archived |
| 10 | District User | Access Version History | Both versions accessible with timestamps |

**Expected Outcomes:**
- Manual edits successfully saved and submitted
- Change tracking shows: Original value (298), New value (302), Delta (+4)
- Revision comment linked to specific change
- Both versions accessible for audit purposes
- User-friendly interface with clear visual cues

**Test Data Requirements:**
- Pre-existing enrollment submission for comparison
- Specific school with known enrollment counts
- Realistic revision scenario (late enrollments)

**Validation Points:**
- [ ] Edit permission verification (district can only edit own data)
- [ ] Version control integrity
- [ ] Revision comment required for changes
- [ ] Audit trail completeness
- [ ] Original data preservation

---

### Scenario DC-3: Enrollment Validation - Month-to-Month Comparison

**Scenario ID:** DC-3-ENR-VALIDATE
**Category:** Enrollment Validation
**Points:** Up to 75

#### Acceptance Criteria

| ID | Criterion | Validation Method |
|----|-----------|-------------------|
| AC-3.1 | System compares current month to prior month | Comparison results display |
| AC-3.2 | Statistically significant differences trigger edits | Edit notification generated |
| AC-3.3 | Districts can review flagged items | Exception list accessible |
| AC-3.4 | Districts can make corrections | Edit capability confirmed |
| AC-3.5 | Districts can submit explanatory comments | Comment submission verified |

#### Step-by-Step Walkthrough

**Pre-Conditions:**
- September 2024 enrollment submitted (from DC-1)
- October 2024 enrollment file prepared with intentional variations

**Steps:**

| Step | Actor | Action | Expected Result |
|------|-------|--------|-----------------|
| 1 | District User | Upload October 2024 P-223 file | File accepted and parsed |
| 2 | System | Execute month-to-month comparison rules | Validation engine processes |
| 3 | System | Flag schools with >5% enrollment change | Exception list generated |
| 4 | System | Display validation results dashboard showing: Schools within normal range (green), Schools with significant changes (yellow), Schools requiring explanation (red) | Color-coded status indicators |
| 5 | District User | Click on flagged school: "East Olympia Elementary" | Detail view opens |
| 6 | System | Show comparison: Sept: 485, Oct: 512, Change: +27 (+5.6%) | Statistical details visible |
| 7 | District User | Option A: Click "Correct Data" to modify enrollment | Edit interface opens |
| 8 | District User | Option B: Click "Submit Explanation" | Comment dialog opens |
| 9 | District User | Enter explanation: "New housing development opened; influx of families" | Comment saved and linked |
| 10 | System | Mark item as "Reviewed - Comment Submitted" | Status updates; workflow advances |

**Expected Outcomes:**
- Automatic detection of enrollment anomalies
- Clear visualization of month-over-month changes
- Flexible workflow: correct OR explain
- Comments preserved for OSPI review
- Validation status tracking per school

**Test Data Requirements:**
- October 2024 enrollment with:
  - 2-3 schools within normal variance (<5%)
  - 2-3 schools with moderate variance (5-10%)
  - 1 school with significant variance (>10%)
- Baseline September 2024 data for comparison

**Validation Points:**
- [ ] Statistical threshold accuracy (configurable)
- [ ] Comparison calculation correctness
- [ ] Exception flagging logic
- [ ] Comment association with specific edits
- [ ] Workflow status transitions

---

### Scenario DC-4: Budget Data Upload and Validation

**Scenario ID:** DC-4-BUD-UPLOAD
**Category:** Budget Collection
**Points:** Up to 75

#### Acceptance Criteria

| ID | Criterion | Validation Method |
|----|-----------|-------------------|
| AC-4.1 | District can upload monthly financial information | File upload success |
| AC-4.2 | System validates completeness of incoming report | Completeness check results |
| AC-4.3 | System compares to prior month's financial data | Comparison display |
| AC-4.4 | System flags statistically significant differences | Edit triggers visible |
| AC-4.5 | System detects unreasonable amounts | Amount validation alerts |
| AC-4.6 | System validates program/activity/object combinations | Code validation results |

#### Step-by-Step Walkthrough

**Pre-Conditions:**
- User authenticated as Tumwater SD fiscal administrator
- F-197 (Monthly Treasurer Report) prepared for October 2024
- September 2024 financial data already in system

**Steps:**

| Step | Actor | Action | Expected Result |
|------|-------|--------|-----------------|
| 1 | District User | Navigate to Data Collection > Financial > Monthly Upload | Financial upload interface displays |
| 2 | District User | Select report type: F-197 Monthly Treasurer Report | Form context established |
| 3 | District User | Select period: October 2024 | Period validated against calendar |
| 4 | District User | Upload F-197 file | File accepted; parsing begins |
| 5 | System | Validate completeness: Check all required sections present (Revenue, Expenditures, Fund Balance) | Completeness indicator: 100% |
| 6 | System | Compare to September 2024 data | Variance analysis generated |
| 7 | System | Flag significant variances: - General Fund expenditures +15% - Transportation costs +25% | Variance alerts displayed |
| 8 | System | Detect unreasonable amounts: - Negative fund balance in Activity Fund - Salary expenditure exceeding budget 200% | Amount alerts displayed |
| 9 | System | Validate account codes: - Program 21 + Activity 27 + Object 3 = Valid - Program 99 + Activity 00 + Object 9 = Invalid | Code validation results |
| 10 | District User | Review all validation findings | Consolidated findings dashboard |
| 11 | District User | Address each finding: correct data or provide explanation | Workflow progresses |
| 12 | System | Log all actions and generate submission receipt | Audit trail complete |

**Expected Outcomes:**
- Comprehensive validation across multiple dimensions
- Clear categorization of issues by type and severity
- Efficient review and resolution workflow
- Account code validation against OSPI chart of accounts
- Complete audit trail of submission and corrections

**Test Data Requirements:**
- F-197 file with intentional issues:
  - Missing revenue section (completeness test)
  - 25% variance in transportation (threshold test)
  - Negative fund balance (reasonableness test)
  - Invalid account code combination (code validation test)
- Prior month (September 2024) baseline data

**Validation Points:**
- [ ] Required section completeness (configurable checklist)
- [ ] Month-to-month variance thresholds by category
- [ ] Unreasonable amount detection rules
- [ ] OSPI Chart of Accounts code validation
- [ ] Cross-fund balance verification

---

### Scenario DC-5: OSPI User Interface - Data Review and Approval

**Scenario ID:** DC-5-OSPI-REVIEW
**Category:** Administrative Interface
**Points:** Up to 37.5

#### Acceptance Criteria

| ID | Criterion | Validation Method |
|----|-----------|-------------------|
| AC-5.1 | OSPI user can enter data on behalf of districts | Data entry as OSPI user |
| AC-5.2 | OSPI user can review data by district | District data accessible |
| AC-5.3 | Data can be approved at regional (ESD) level | ESD approval workflow |
| AC-5.4 | Data can be approved at state level | State approval workflow |
| AC-5.5 | Interface supports bulk operations | Multi-district actions |

#### Step-by-Step Walkthrough

**Pre-Conditions:**
- User authenticated as OSPI Enrollment Program Specialist
- Multiple districts have submitted October 2024 enrollment
- ESD 113 aggregation required

**Steps:**

| Step | Actor | Action | Expected Result |
|------|-------|--------|-----------------|
| 1 | OSPI User | Navigate to Admin > Data Review Dashboard | Statewide submission status displays |
| 2 | OSPI User | Filter by: ESD 113, Period: October 2024 | ESD 113 districts listed |
| 3 | System | Display submission matrix showing: Districts submitted (green), Districts with pending edits (yellow), Districts not submitted (red) | Visual status grid |
| 4 | OSPI User | Click on Tumwater SD (34033) | District detail panel opens |
| 5 | OSPI User | Review enrollment summary with validation status | All data and findings visible |
| 6 | OSPI User | Enter data manually for non-submitting district | Manual entry form opens |
| 7 | OSPI User | Input enrollment data for Yelm SD (received via email) | Data accepted with OSPI attribution |
| 8 | OSPI User | Return to ESD 113 dashboard | Dashboard refreshes |
| 9 | OSPI User | Select all submitted districts | Bulk selection enabled |
| 10 | OSPI User | Click "Approve for ESD" | Confirmation dialog appears |
| 11 | System | Update status to "ESD Approved" for selected districts | Status indicators update |
| 12 | OSPI User | Escalate to State Approval queue | Districts move to state queue |
| 13 | OSPI User | Navigate to State Approval queue | State-level review interface |
| 14 | OSPI User | Apply final state approval | Status: "State Approved" |

**Expected Outcomes:**
- Clear visibility into statewide submission status
- Flexible filtering by ESD, district, period, status
- Ability to enter data on behalf of non-digital districts
- Tiered approval workflow (District -> ESD -> State)
- Bulk operations for efficiency

**Test Data Requirements:**
- Mix of submission statuses across ESD 113 districts
- At least one district requiring manual OSPI entry
- Multiple districts ready for approval

**Validation Points:**
- [ ] Role-based access (OSPI vs. district permissions)
- [ ] Manual entry attribution (who entered data)
- [ ] Approval workflow state transitions
- [ ] Bulk operation integrity
- [ ] Audit trail for all OSPI actions

---

### Scenario DC-6: Data Locking

**Scenario ID:** DC-6-DATA-LOCK
**Category:** Data Security
**Points:** Up to 37.5

#### Acceptance Criteria

| ID | Criterion | Validation Method |
|----|-----------|-------------------|
| AC-6.1 | Lock data for all districts simultaneously | Statewide lock applied |
| AC-6.2 | Lock data for a subset of districts | Subset lock verified |
| AC-6.3 | Lock data for a single district | Individual lock applied |
| AC-6.4 | Monthly calculation lock prevents changes | Edit attempt blocked |
| AC-6.5 | Annual audit lock preserves final data | Permanent lock verified |

#### Step-by-Step Walkthrough

**Pre-Conditions:**
- User authenticated as OSPI Data Administrator
- October 2024 enrollment approved for all districts
- Calculation cycle ready to begin

**Steps:**

| Step | Actor | Action | Expected Result |
|------|-------|--------|-----------------|
| 1 | OSPI Admin | Navigate to Admin > Data Locking | Lock management interface |
| 2 | OSPI Admin | Select Lock Type: Monthly Calculation Lock | Lock type configured |
| 3 | OSPI Admin | Select Scope: All Districts | Scope set to statewide |
| 4 | OSPI Admin | Select Data: Enrollment, October 2024 | Data set identified |
| 5 | OSPI Admin | Click "Apply Lock" | Confirmation dialog |
| 6 | System | Apply lock to all 295 districts | Lock status updates statewide |
| 7 | District User | (As Tumwater SD) Attempt to edit October enrollment | Error: "Data locked for calculation period" |
| 8 | OSPI Admin | Remove monthly lock after calculations | Lock released |
| 9 | OSPI Admin | Apply lock to subset: ESD 113 only | Subset lock applied |
| 10 | District User | (As Tumwater SD) Attempt edit | Edit blocked (ESD 113 locked) |
| 11 | District User | (As Kennewick SD - ESD 123) Attempt edit | Edit allowed (different ESD) |
| 12 | OSPI Admin | Apply Annual Audit Lock: 2023-24 school year | Permanent lock applied |
| 13 | System | Mark 2023-24 data as "Audit Final" | Historical data preserved |
| 14 | OSPI Admin | Attempt to remove Audit Lock | Warning: "Audit locks are permanent" |

**Expected Outcomes:**
- Granular lock control (all/subset/single)
- Clear user feedback when lock prevents action
- Monthly locks reversible; audit locks permanent
- Lock status visible in user interface
- Comprehensive audit log of lock operations

**Test Data Requirements:**
- Multiple districts with submitted data
- Clear ESD groupings for subset testing
- Prior year data for audit lock demonstration

**Validation Points:**
- [ ] Lock scope accuracy (correct districts affected)
- [ ] Lock enforcement (edits truly blocked)
- [ ] Lock visibility (users see lock status)
- [ ] Monthly vs. audit lock differentiation
- [ ] Lock audit trail

---

## 5. Section 2: Data Calculation Scenarios

### Scenario CA-1: Production Environment - Data Updates and Validation

**Scenario ID:** CA-1-PROD-UPDATE
**Category:** Production Calculations
**Points:** Up to 100

#### Acceptance Criteria

| ID | Criterion | Validation Method |
|----|-----------|-------------------|
| AC-1.1 | Data updates from districts added after validation | Update visibility in production |
| AC-1.2 | Data updates from districts added after approval | Approval workflow confirmed |
| AC-1.3 | OSPI can make adjustments to submitted data | Adjustment capability verified |
| AC-1.4 | Calculations can run for all districts | Statewide calculation execution |
| AC-1.5 | Calculations can run for subset of districts | Subset calculation execution |
| AC-1.6 | Calculations can run for single district | Individual calculation execution |
| AC-1.7 | Audit trails exist for calculations | Calculation audit log |
| AC-1.8 | Audit trails exist for constant changes | Constant modification log |
| AC-1.9 | Audit trails exist for data adjustments | Data change log |

#### Step-by-Step Walkthrough

**Pre-Conditions:**
- October 2024 enrollment data approved for Tumwater SD
- Previous apportionment calculation exists (September 2024)
- OSPI user with calculation authority authenticated

**Steps:**

| Step | Actor | Action | Expected Result |
|------|-------|--------|-----------------|
| 1 | System | Display Production Environment dashboard | Current data status visible |
| 2 | OSPI User | Navigate to Calculation > Run Calculations | Calculation control panel |
| 3 | OSPI User | View pending data updates awaiting calculation | List of updated districts |
| 4 | System | Show: Tumwater SD - October enrollment updated, Validated: Yes, Approved: Yes | Update status confirmed |
| 5 | OSPI User | Select calculation scope: Single District - Tumwater SD | Scope configured |
| 6 | OSPI User | Click "Run Calculation" | Calculation job initiated |
| 7 | System | Execute apportionment formulas for Tumwater SD: - Basic Education - Special Education - Transportation - LEA - Other programs | Progress indicator shows steps |
| 8 | System | Display calculation results summary: Total Apportionment: $XX,XXX,XXX, Change from prior: +$XXX,XXX | Results displayed |
| 9 | OSPI User | Click "View Calculation Details" | Formula breakdown visible |
| 10 | OSPI User | Navigate to Admin > Make Adjustment | Adjustment interface |
| 11 | OSPI User | Adjust Tumwater transportation factor due to route change | Adjustment form completes |
| 12 | OSPI User | Provide adjustment reason: "Approved route modification per board meeting 10/15" | Reason required and logged |
| 13 | OSPI User | Run calculation for subset: All ESD 113 districts | Subset calculation executes |
| 14 | OSPI User | Run calculation for all districts (statewide) | Statewide calculation executes |
| 15 | OSPI User | Navigate to Audit > Calculation Logs | Audit trail displayed |
| 16 | System | Display audit entries: Who, When, What calculated, Parameters used | Complete audit history |

**Expected Outcomes:**
- Seamless data flow from collection to calculation
- Flexible calculation scope (single/subset/all)
- Transparent adjustment process with mandatory reasoning
- Complete audit trail of all calculation activities
- Calculation results immediately visible

**Test Data Requirements:**
- Approved October 2024 enrollment for Tumwater SD
- State constants for 2024-25 school year
- Apportionment formulas configured
- Prior calculation for comparison

**Validation Points:**
- [ ] Data validation gate before calculation
- [ ] Approval gate before calculation
- [ ] Calculation result accuracy (spot-check against manual calculation)
- [ ] Adjustment attribution and reason capture
- [ ] Audit log completeness (who, what, when, why)
- [ ] Performance: Single district < 30 seconds
- [ ] Performance: Statewide < 60 minutes

---

### Scenario CA-2: Production Environment - Audit Trail Deep Dive

**Scenario ID:** CA-2-PROD-AUDIT
**Category:** Production Audit
**Points:** Up to 50

#### Acceptance Criteria

| ID | Criterion | Validation Method |
|----|-----------|-------------------|
| AC-2.1 | All calculation runs are logged | Log entry for each run |
| AC-2.2 | State constant changes are logged | Constant modification history |
| AC-2.3 | District data adjustments are logged | Adjustment history preserved |
| AC-2.4 | Logs include user, timestamp, before/after values | Log detail completeness |
| AC-2.5 | Logs are searchable and exportable | Search and export functions |

#### Step-by-Step Walkthrough

**Pre-Conditions:**
- Multiple calculations have been run (from CA-1)
- OSPI administrator with audit access authenticated

**Steps:**

| Step | Actor | Action | Expected Result |
|------|-------|--------|-----------------|
| 1 | OSPI Admin | Navigate to Audit > Comprehensive Audit Log | Audit dashboard displays |
| 2 | OSPI Admin | Filter by: Type = Calculation, District = Tumwater | Filtered results |
| 3 | System | Display calculation audit entries: Date/Time, User, Scope, Duration, Status | Calculation history |
| 4 | OSPI Admin | Click on specific calculation entry | Detail view opens |
| 5 | System | Show calculation parameters: Input data snapshot, Constants used, Formulas applied, Output values | Complete context |
| 6 | OSPI Admin | Filter by: Type = Constant Change | Constant change log |
| 7 | System | Display: Constant Name, Old Value, New Value, Changed By, Effective Date, Reason | Constant history |
| 8 | OSPI Admin | Filter by: Type = Data Adjustment | Adjustment log |
| 9 | System | Display: District, Field, Old Value, New Value, Adjusted By, Reason, Approval | Adjustment history |
| 10 | OSPI Admin | Export audit log to Excel | File downloads |
| 11 | OSPI Admin | Search audit log: "transportation" | Relevant entries filtered |

**Expected Outcomes:**
- Complete traceability of all calculation activities
- Before/after values for all changes
- Attribution to specific users
- Mandatory reason capture for adjustments
- Flexible search and export capabilities

**Test Data Requirements:**
- Multiple calculation runs with varying scopes
- At least one state constant change
- At least one district data adjustment
- Mix of users performing actions

**Validation Points:**
- [ ] No calculation runs without audit entry
- [ ] All constant changes captured with before/after
- [ ] All adjustments attributed with reason
- [ ] Search functionality accuracy
- [ ] Export includes all visible columns

---

### Scenario CA-3: Sandbox Environment - Scenario Creation

**Scenario ID:** CA-3-SANDBOX-CREATE
**Category:** Sandbox Operations
**Points:** Up to 50

#### Acceptance Criteria

| ID | Criterion | Validation Method |
|----|-----------|-------------------|
| AC-3.1 | OSPI users can copy data from production | Data copy successful |
| AC-3.2 | District users can copy data they're entitled to see | Permission-limited copy |
| AC-3.3 | Legislature users can copy relevant data | Legislative data access |
| AC-3.4 | Statewide constants can be copied | Constants in sandbox |
| AC-3.5 | Formulae can be copied | Formulas in sandbox |
| AC-3.6 | Copied data can be modified | Sandbox edits work |
| AC-3.7 | Modifications don't affect production | Isolation verified |

#### Step-by-Step Walkthrough

**Pre-Conditions:**
- Production data exists for October 2024
- Users from three populations authenticated (OSPI, District, Legislature)

**Steps:**

| Step | Actor | Action | Expected Result |
|------|-------|--------|-----------------|
| 1 | OSPI User | Navigate to Sandbox > Create New Scenario | Scenario creation wizard |
| 2 | OSPI User | Name scenario: "FY25 Budget Impact Analysis" | Name accepted |
| 3 | OSPI User | Select data source: Production, October 2024 | Data source configured |
| 4 | OSPI User | Select scope: Statewide (all districts) | OSPI has full access |
| 5 | OSPI User | Include: Constants, Formulas, District Data | All components selected |
| 6 | OSPI User | Click "Create Scenario" | Scenario copying initiates |
| 7 | System | Copy production data to isolated sandbox | Progress indicator; completion message |
| 8 | District User | (As Tumwater SD) Navigate to Sandbox > Create Scenario | Scenario wizard opens |
| 9 | District User | Select data scope | Only Tumwater SD data available |
| 10 | System | Enforce data entitlement (district sees only own data) | Other districts not selectable |
| 11 | District User | Create scenario: "Tumwater Enrollment Projection" | Scenario created with limited scope |
| 12 | Legislative User | Create scenario: "Statewide Funding Analysis" | Scenario created with statewide view |
| 13 | OSPI User | Open scenario and modify: Increase MSOC rate by 3% | Sandbox constant modified |
| 14 | OSPI User | Run sandbox calculation | Sandbox results generated |
| 15 | OSPI User | Verify production unchanged | Production values same as before |

**Expected Outcomes:**
- Each user population can create scenarios
- Data entitlements enforced (districts see only their data)
- Complete isolation from production
- Full calculation capability in sandbox
- No leakage between sandbox and production

**Test Data Requirements:**
- Complete production dataset
- User accounts for each population type
- Clear data entitlement rules

**Validation Points:**
- [ ] Copy operation completeness
- [ ] Data entitlement enforcement
- [ ] Sandbox isolation from production
- [ ] Constant and formula modification capability
- [ ] Calculation execution in sandbox

---

### Scenario CA-4: Sandbox Environment - Multi-Scenario Management

**Scenario ID:** CA-4-SANDBOX-MULTI
**Category:** Sandbox Operations
**Points:** Up to 50

#### Acceptance Criteria

| ID | Criterion | Validation Method |
|----|-----------|-------------------|
| AC-4.1 | Users can create multiple scenarios simultaneously | Multiple scenarios exist |
| AC-4.2 | Users can manipulate each scenario independently | Independent edits verified |
| AC-4.3 | Scenarios persist until deliberately deleted | Persistence confirmed |
| AC-4.4 | Users can name and organize scenarios | Naming and tagging works |

#### Step-by-Step Walkthrough

**Pre-Conditions:**
- OSPI user authenticated
- One sandbox scenario already exists (from CA-3)

**Steps:**

| Step | Actor | Action | Expected Result |
|------|-------|--------|-----------------|
| 1 | OSPI User | Navigate to Sandbox > My Scenarios | Scenario list displays |
| 2 | System | Show existing scenario: "FY25 Budget Impact Analysis" | Previous scenario visible |
| 3 | OSPI User | Click "Create Additional Scenario" | New scenario wizard |
| 4 | OSPI User | Create: "COLA 2% Scenario" | Second scenario created |
| 5 | OSPI User | Create: "COLA 4% Scenario" | Third scenario created |
| 6 | OSPI User | Create: "Enrollment Decline 5%" | Fourth scenario created |
| 7 | System | Display all four scenarios in list | All scenarios visible |
| 8 | OSPI User | Open "COLA 2% Scenario" | Scenario workspace opens |
| 9 | OSPI User | Modify COLA constant to 2% | Change saved to this scenario |
| 10 | OSPI User | Open "COLA 4% Scenario" in new tab | Separate workspace opens |
| 11 | OSPI User | Verify COLA still at baseline (not 2%) | Scenarios are independent |
| 12 | OSPI User | Modify COLA to 4% in this scenario | Change saved independently |
| 13 | OSPI User | Add tags to scenarios: "Legislative Request", "Internal Analysis" | Tags applied |
| 14 | OSPI User | Run calculations in multiple scenarios | Each produces unique results |
| 15 | OSPI User | Close browser, reopen next day | Scenarios persist |

**Expected Outcomes:**
- Multiple concurrent scenarios supported
- Complete independence between scenarios
- Organizational capabilities (naming, tagging)
- Persistent storage (survives session end)
- Parallel work in multiple scenarios

**Test Data Requirements:**
- Ability to modify different constants
- Multiple calculation variables for differentiation
- Tagging taxonomy

**Validation Points:**
- [ ] Maximum concurrent scenarios (no artificial limit)
- [ ] Data independence between scenarios
- [ ] Session persistence
- [ ] Multi-tab/multi-window operation
- [ ] Organizational metadata (names, tags, dates)

---

### Scenario CA-5: Sandbox Environment - Scenario Comparison

**Scenario ID:** CA-5-SANDBOX-COMPARE
**Category:** Sandbox Operations
**Points:** Up to 50

#### Acceptance Criteria

| ID | Criterion | Validation Method |
|----|-----------|-------------------|
| AC-5.1 | Users can view multiple scenarios simultaneously | Side-by-side display |
| AC-5.2 | Users can compare multiple scenarios | Comparison analysis |
| AC-5.3 | Users can compare scenario to production | Sandbox vs. production view |
| AC-5.4 | Comparison highlights differences | Variance highlighting |

#### Step-by-Step Walkthrough

**Pre-Conditions:**
- Multiple sandbox scenarios exist with different calculations (from CA-4)
- Production data available for comparison

**Steps:**

| Step | Actor | Action | Expected Result |
|------|-------|--------|-----------------|
| 1 | OSPI User | Navigate to Sandbox > Compare Scenarios | Comparison interface |
| 2 | OSPI User | Select scenarios: "COLA 2%", "COLA 4%" | Two scenarios selected |
| 3 | System | Display side-by-side comparison: Scenario name, Modified constants, Calculation results | Comparison table |
| 4 | System | Highlight differences: Total apportionment delta, Per-district variance | Differences emphasized |
| 5 | OSPI User | Add third scenario: "Enrollment Decline 5%" | Three-way comparison |
| 6 | System | Expand comparison to three columns | All three visible |
| 7 | OSPI User | Click "Add Production Baseline" | Production added as column |
| 8 | System | Show production values alongside scenarios | Four columns displayed |
| 9 | OSPI User | Sort by largest variance from production | Districts reordered |
| 10 | OSPI User | Filter to Tumwater SD only | Single district comparison |
| 11 | System | Display Tumwater comparison: Scenario 1 (COLA 2%): $89,500,000, Scenario 2 (COLA 4%): $91,200,000, Scenario 3 (Enrollment -5%): $85,100,000, Production: $88,000,000 | Clear comparison values |
| 12 | OSPI User | Export comparison to Excel | Comparison data exported |
| 13 | OSPI User | Generate comparison report (PDF) | Formatted report generated |

**Expected Outcomes:**
- Intuitive multi-scenario comparison
- Production baseline always available for reference
- Clear variance highlighting
- Flexible filtering and sorting
- Export and reporting capabilities

**Test Data Requirements:**
- Scenarios with meaningfully different assumptions
- Production baseline data
- Calculation results in all scenarios

**Validation Points:**
- [ ] Simultaneous display of 3+ scenarios
- [ ] Production comparison accuracy
- [ ] Variance calculation correctness
- [ ] Export completeness
- [ ] Report formatting quality

---

## 6. Section 3: Data Reporting Scenarios

### Scenario RE-1: Monthly Enrollment Reports

**Scenario ID:** RE-1-ENR-MONTHLY
**Category:** Enrollment Reporting
**Points:** Up to 60

#### Acceptance Criteria

| ID | Criterion | Validation Method |
|----|-----------|-------------------|
| AC-1.1 | System generates monthly enrollment reports | Report generation |
| AC-1.2 | Reports include required enrollment categories | Content verification |
| AC-1.3 | Reports are accurate to source data | Data accuracy check |
| AC-1.4 | Reports are available in multiple formats | Format options |
| AC-1.5 | Reports support filtering by district/ESD/state | Filter functionality |

#### Step-by-Step Walkthrough

**Pre-Conditions:**
- October 2024 enrollment data approved for Tumwater SD
- User authenticated with reporting access

**Steps:**

| Step | Actor | Action | Expected Result |
|------|-------|--------|-----------------|
| 1 | User | Navigate to Reports > Enrollment > Monthly Reports | Report selection interface |
| 2 | User | Select report: P-223 Monthly Enrollment Summary | Report parameters display |
| 3 | User | Select parameters: District: Tumwater SD (34033), Month: October 2024 | Parameters configured |
| 4 | User | Click "Generate Report" | Report processing begins |
| 5 | System | Generate report including: Total Headcount by Grade, FTE Calculations, Special Education Counts, Running Start Counts, Resident District Breakdown | Report content generated |
| 6 | System | Display report preview | On-screen preview |
| 7 | User | Verify totals match submitted data | Data accuracy confirmed |
| 8 | User | Select format: PDF | Format selected |
| 9 | User | Click "Download" | PDF downloads |
| 10 | User | Select format: Excel | Format selected |
| 11 | User | Click "Download" | Excel downloads |
| 12 | User | Change scope: ESD 113 Summary | Aggregated report |
| 13 | System | Generate ESD-level summary | Regional totals displayed |
| 14 | User | Change scope: Statewide Summary | State-level report |
| 15 | System | Generate statewide enrollment summary | All 295 districts aggregated |

**Expected Outcomes:**
- Professional-quality monthly enrollment reports
- Multiple output formats (PDF, Excel, on-screen)
- Drill-down capability (State -> ESD -> District -> School)
- Data accuracy maintained through reporting chain
- Efficient generation even at statewide scale

**Test Data Requirements:**
- Complete October 2024 enrollment for Tumwater SD
- Enrollment data for additional ESD 113 districts
- Statewide enrollment data for aggregation

**Validation Points:**
- [ ] Report content matches RFP requirements
- [ ] PDF formatting quality (professional, printable)
- [ ] Excel export completeness (all data, proper formatting)
- [ ] Aggregation accuracy (school -> district -> ESD -> state)
- [ ] Performance: District report < 5 seconds

---

### Scenario RE-2: Annual Budget Report ("Projections")

**Scenario ID:** RE-2-BUD-ANNUAL
**Category:** Budget Reporting
**Points:** Up to 60

#### Acceptance Criteria

| ID | Criterion | Validation Method |
|----|-----------|-------------------|
| AC-2.1 | System generates annual budget report | Report generation |
| AC-2.2 | Report follows F-195 format requirements | Format compliance |
| AC-2.3 | Report includes all budget components | Content completeness |
| AC-2.4 | Report meets State Auditor requirements | SAO compliance |
| AC-2.5 | Report is printable in high-quality format | Print quality |

#### Step-by-Step Walkthrough

**Pre-Conditions:**
- 2024-25 F-195 budget data submitted for Tumwater SD
- Budget approved and calculations run

**Steps:**

| Step | Actor | Action | Expected Result |
|------|-------|--------|-----------------|
| 1 | User | Navigate to Reports > Financial > Annual Budget | Budget report interface |
| 2 | User | Select: District: Tumwater SD, Year: 2024-25 | Parameters set |
| 3 | User | Click "Generate F-195 Report" | Report generation |
| 4 | System | Generate comprehensive budget document: Revenue by Source (Local, State, Federal), Expenditures by Function, Expenditures by Object, Fund Balances, Multi-Year Comparison | Complete budget document |
| 5 | System | Apply State Auditor formatting requirements | SAO-compliant format |
| 6 | User | Preview report on screen | Report displays |
| 7 | User | Verify section completeness | All required sections present |
| 8 | User | Verify calculated fields: Total Revenue, Total Expenditures, Ending Fund Balance | Calculations correct |
| 9 | User | Export as PDF (print-ready) | High-quality PDF generated |
| 10 | User | Print report | Professional output |
| 11 | User | Generate comparative report (2023-24 vs 2024-25) | Year-over-year comparison |
| 12 | System | Show variance analysis | Changes highlighted |

**Expected Outcomes:**
- SAO-compliant budget report format
- All required budget sections included
- Calculation accuracy throughout
- Professional print quality
- Year-over-year comparison capability

**Test Data Requirements:**
- Complete 2024-25 F-195 budget for Tumwater SD
- Prior year (2023-24) budget for comparison
- SAO format specifications

**Validation Points:**
- [ ] All F-195 required sections present
- [ ] SAO format compliance
- [ ] Calculation accuracy (spot checks)
- [ ] Print quality (no truncation, proper pagination)
- [ ] Comparative analysis accuracy

---

### Scenario RE-3: Annual Financial Statement ("Actuals")

**Scenario ID:** RE-3-FIN-ANNUAL
**Category:** Financial Reporting
**Points:** Up to 60

#### Acceptance Criteria

| ID | Criterion | Validation Method |
|----|-----------|-------------------|
| AC-3.1 | System generates annual financial statement | Report generation |
| AC-3.2 | Report follows F-196 format requirements | Format compliance |
| AC-3.3 | Report includes all financial components | Content completeness |
| AC-3.4 | Report meets State Auditor requirements | SAO compliance |
| AC-3.5 | Report supports audit process | Audit utility |

#### Step-by-Step Walkthrough

**Pre-Conditions:**
- 2023-24 F-196 financial data finalized for Tumwater SD
- Annual audit lock applied

**Steps:**

| Step | Actor | Action | Expected Result |
|------|-------|--------|-----------------|
| 1 | User | Navigate to Reports > Financial > Annual Statement | Financial statement interface |
| 2 | User | Select: District: Tumwater SD, Year: 2023-24 | Parameters set |
| 3 | User | Click "Generate F-196 Report" | Report generation |
| 4 | System | Generate comprehensive financial statement: Statement of Revenues and Expenditures, Balance Sheet, Statement of Changes in Fund Balance, Notes and Schedules | Complete financial statement |
| 5 | System | Apply GAAP and SAO formatting | Compliant format |
| 6 | User | Preview Statement of Revenues | Revenue section displays |
| 7 | User | Verify actual vs. budget comparison | Variance analysis visible |
| 8 | User | Preview Balance Sheet | Fund balances display |
| 9 | User | Verify fund balance reconciliation | Opening + Changes = Closing |
| 10 | User | Export as PDF (audit-ready) | PDF generated |
| 11 | User | Generate supporting schedules | Detail schedules generated |
| 12 | User | Export all components as package | Complete package downloads |

**Expected Outcomes:**
- GAAP-compliant financial statements
- SAO format requirements met
- Complete supporting schedules
- Audit-ready documentation
- Variance analysis against budget

**Test Data Requirements:**
- Complete 2023-24 F-196 data for Tumwater SD
- Approved budget for comparison
- SAO format specifications

**Validation Points:**
- [ ] All F-196 required components present
- [ ] GAAP compliance
- [ ] SAO format compliance
- [ ] Fund balance reconciliation accuracy
- [ ] Supporting schedule completeness

---

### Scenario RE-4: Excel Export for Ad-Hoc Reporting

**Scenario ID:** RE-4-ADHOC-EXCEL
**Category:** Ad-Hoc Reporting
**Points:** Up to 60

#### Acceptance Criteria

| ID | Criterion | Validation Method |
|----|-----------|-------------------|
| AC-4.1 | System generates Excel exports | Export generation |
| AC-4.2 | Users can create custom reports with minimal training | Usability test |
| AC-4.3 | Exports include all relevant data fields | Data completeness |
| AC-4.4 | Data is structured for analysis | Pivot-ready format |
| AC-4.5 | Large datasets export efficiently | Performance test |

#### Step-by-Step Walkthrough

**Pre-Conditions:**
- User with reporting access authenticated
- Multiple data types available (enrollment, financial, apportionment)

**Steps:**

| Step | Actor | Action | Expected Result |
|------|-------|--------|-----------------|
| 1 | User | Navigate to Reports > Data Export | Export builder interface |
| 2 | User | Select data type: Enrollment | Enrollment fields available |
| 3 | User | Drag fields to export: District Name, Month, Grade, Headcount, FTE | Fields selected |
| 4 | User | Apply filter: ESD 113, October 2024 | Filter configured |
| 5 | User | Click "Preview" | Sample data displays |
| 6 | User | Click "Export to Excel" | Export processing |
| 7 | System | Generate structured Excel file with: Data worksheet, Field definitions worksheet, Filter documentation | Complete workbook |
| 8 | User | Open Excel file | File opens correctly |
| 9 | User | Create pivot table from exported data | Pivot table functional |
| 10 | User | Save export configuration as template: "ESD Enrollment Analysis" | Template saved |
| 11 | User | Schedule export: Monthly, email delivery | Schedule configured |
| 12 | User | Test large export: Statewide, all months, all fields | Large dataset processing |
| 13 | System | Generate file in reasonable time | Export completes < 2 minutes |

**Expected Outcomes:**
- Intuitive export builder requiring minimal training
- Flexible field selection and filtering
- Excel-ready format supporting pivot tables
- Template saving and scheduling
- Efficient handling of large datasets

**Test Data Requirements:**
- Multiple months of enrollment data
- Multiple districts for filtering
- Large dataset for performance testing

**Validation Points:**
- [ ] User interface intuitiveness (no technical training needed)
- [ ] Export completeness (all selected fields present)
- [ ] Excel compatibility (opens without errors)
- [ ] Pivot table readiness (proper data structure)
- [ ] Large export performance (<2 minutes for statewide)

---

### Scenario RE-5: API Integration for External Systems

**Scenario ID:** RE-5-API-INTEGRATION
**Category:** System Integration
**Points:** Up to 60

#### Acceptance Criteria

| ID | Criterion | Validation Method |
|----|-----------|-------------------|
| AC-5.1 | System provides API for data sharing | API availability |
| AC-5.2 | API supports customized integration | Flexibility verification |
| AC-5.3 | API is secured with authentication | Security test |
| AC-5.4 | API documentation is comprehensive | Documentation review |
| AC-5.5 | API handles reasonable request volumes | Performance test |

#### Step-by-Step Walkthrough

**Pre-Conditions:**
- API endpoints configured
- Test client application available
- API credentials issued

**Steps:**

| Step | Actor | Action | Expected Result |
|------|-------|--------|-----------------|
| 1 | Developer | Access API documentation portal | Documentation displays |
| 2 | Developer | Review available endpoints: /enrollment, /financial, /apportionment, /districts, /constants | Endpoint catalog |
| 3 | Developer | Request API credentials | OAuth2 credentials issued |
| 4 | Developer | Authenticate with API | Access token received |
| 5 | Developer | Query: GET /api/v1/enrollment?district=34033&month=2024-10 | Tumwater enrollment data returned |
| 6 | System | Return JSON response with enrollment data | Structured JSON |
| 7 | Developer | Query: GET /api/v1/apportionment?district=34033&year=2024-25 | Apportionment data returned |
| 8 | Developer | Query with invalid token | 401 Unauthorized |
| 9 | Developer | Query for unauthorized district (as district user) | 403 Forbidden |
| 10 | Developer | Test bulk query: GET /api/v1/enrollment?esd=113 | ESD data returned |
| 11 | Developer | Test rate limiting | Rate limit headers present |
| 12 | System | Log all API access | Audit log populated |
| 13 | Developer | Request data in different format: Accept: text/csv | CSV response returned |

**Expected Outcomes:**
- RESTful API with comprehensive endpoints
- OAuth2 authentication and authorization
- Role-based data access control
- Multiple response formats (JSON, CSV)
- Comprehensive documentation and logging

**Test Data Requirements:**
- Active dataset for query results
- Multiple user roles for access testing
- Rate limiting configuration

**Validation Points:**
- [ ] Endpoint coverage (all major data types)
- [ ] Authentication enforcement (no anonymous access)
- [ ] Authorization accuracy (data entitlements)
- [ ] Response format correctness
- [ ] Documentation completeness
- [ ] Audit logging of API access

---

## 7. Cross-Module Integration Points

### 7.1 Collection to Calculation Integration

| Integration Point | Description | Demonstration |
|-------------------|-------------|---------------|
| **Validation Gate** | Data must pass collection validation before calculation | Show rejected calculation for invalid data |
| **Approval Gate** | Data must be approved before calculation | Demonstrate approval workflow |
| **Data Transfer** | Approved data flows to calculation tables | Show data appearing in calculation input |
| **Lock Coordination** | Collection locks align with calculation cycles | Demonstrate lock/unlock sequence |

### 7.2 Calculation to Reporting Integration

| Integration Point | Description | Demonstration |
|-------------------|-------------|---------------|
| **Result Availability** | Calculation results immediately available for reporting | Generate report after calculation |
| **Version Alignment** | Reports use correct calculation version | Show version metadata on reports |
| **Sandbox Reporting** | Sandbox scenarios can generate reports | Run report from sandbox data |
| **Audit Correlation** | Report audit trails link to calculation logs | Navigate from report to calculation audit |

### 7.3 End-to-End Flow

```
+----------------+     +-----------------+     +----------------+
|                |     |                 |     |                |
|    DISTRICT    |     |      OSPI       |     |    EXTERNAL    |
|    PORTAL      |     |   DASHBOARD     |     |    SYSTEMS     |
|                |     |                 |     |                |
+-------+--------+     +--------+--------+     +--------+-------+
        |                       |                       ^
        | Upload/Enter          | Review/Approve        | API
        | Data                  | Data                  | Access
        v                       v                       |
+-------+--------+     +--------+--------+     +--------+-------+
|                |     |                 |     |                |
|     DATA       +---->+     DATA        +---->+     DATA       |
|   COLLECTION   |     |  CALCULATION    |     |   REPORTING    |
|                |     |                 |     |                |
| - Validation   |     | - Production    |     | - Enrollment   |
| - Approval     |     | - Sandbox       |     | - Financial    |
| - Locking      |     | - Audit Trail   |     | - Ad-Hoc       |
|                |     |                 |     | - API          |
+----------------+     +-----------------+     +----------------+
```

---

## 8. Evaluation Rubric

### 8.1 Scoring Matrix (Per Scenario)

| Criterion | Weight | Description |
|-----------|--------|-------------|
| **Functionality** | 40% | Does the demonstrated feature work correctly? |
| **Usability** | 25% | Is the interface intuitive and user-friendly? |
| **Completeness** | 20% | Are all acceptance criteria addressed? |
| **Performance** | 10% | Does the system respond within acceptable time? |
| **Documentation** | 5% | Are actions audited and traceable? |

### 8.2 Point Allocation by Section

#### Data Collection (300 points)

| Scenario | Max Points | Functionality | Usability | Completeness | Performance | Documentation |
|----------|------------|---------------|-----------|--------------|-------------|---------------|
| DC-1 Enrollment Upload | 75 | 30 | 19 | 15 | 8 | 3 |
| DC-2 Manual Entry/Revision | 75 | 30 | 19 | 15 | 8 | 3 |
| DC-3 Enrollment Validation | 75 | 30 | 19 | 15 | 8 | 3 |
| DC-4 Budget Upload/Validation | 75 | 30 | 19 | 15 | 8 | 3 |
| DC-5 OSPI Review Interface | 37.5 | 15 | 9 | 8 | 4 | 1.5 |
| DC-6 Data Locking | 37.5 | 15 | 9 | 8 | 4 | 1.5 |

#### Data Calculation (300 points)

| Scenario | Max Points | Functionality | Usability | Completeness | Performance | Documentation |
|----------|------------|---------------|-----------|--------------|-------------|---------------|
| CA-1 Production Updates | 100 | 40 | 25 | 20 | 10 | 5 |
| CA-2 Audit Trail | 50 | 20 | 12.5 | 10 | 5 | 2.5 |
| CA-3 Sandbox Creation | 50 | 20 | 12.5 | 10 | 5 | 2.5 |
| CA-4 Multi-Scenario | 50 | 20 | 12.5 | 10 | 5 | 2.5 |
| CA-5 Scenario Comparison | 50 | 20 | 12.5 | 10 | 5 | 2.5 |

#### Data Reporting (300 points)

| Scenario | Max Points | Functionality | Usability | Completeness | Performance | Documentation |
|----------|------------|---------------|-----------|--------------|-------------|---------------|
| RE-1 Monthly Enrollment | 60 | 24 | 15 | 12 | 6 | 3 |
| RE-2 Annual Budget | 60 | 24 | 15 | 12 | 6 | 3 |
| RE-3 Annual Financial | 60 | 24 | 15 | 12 | 6 | 3 |
| RE-4 Excel Ad-Hoc | 60 | 24 | 15 | 12 | 6 | 3 |
| RE-5 API Integration | 60 | 24 | 15 | 12 | 6 | 3 |

### 8.3 Deduction Guidelines

| Issue | Deduction |
|-------|-----------|
| Feature does not work at all | -100% of functionality points |
| Feature works with manual intervention | -50% of functionality points |
| Interface requires technical training | -50% of usability points |
| Acceptance criterion not demonstrated | -100% of completeness points for that criterion |
| Response time > 30 seconds | -50% of performance points |
| No audit trail visible | -100% of documentation points |

---

## 9. Appendices

### 9.1 Acronyms and Definitions

| Term | Definition |
|------|------------|
| **AAFTE** | Annual Average Full-Time Equivalent |
| **ESD** | Educational Service District |
| **F-195** | Annual Budget Document |
| **F-196** | Annual Financial Statement |
| **F-197** | Monthly Treasurer Report |
| **F-200** | Budget Extension/Revision |
| **F-203** | Revenue Estimation Tool |
| **FTE** | Full-Time Equivalent |
| **LEA** | Local Effort Assistance |
| **MSOC** | Materials, Supplies, and Operating Costs |
| **OSPI** | Office of Superintendent of Public Instruction |
| **P-223** | Monthly Enrollment Report |
| **SAFS** | School Apportionment Financial System |
| **SAO** | State Auditor's Office |
| **S-275** | Annual Personnel Report |

### 9.2 Data File References

| File | URL |
|------|-----|
| SAFS Data Files | https://ospi.k12.wa.us/safs-data-files |
| F-195 Format | https://ospi.k12.wa.us/sites/default/files/public/safs/pub/bud/f195.pdf |
| F-196 Format | https://ospi.k12.wa.us/sites/default/files/public/safs/pub/fin/f196.pdf |
| Chart of Accounts | https://ospi.k12.wa.us/accounting-manual |

### 9.3 Requirements Traceability

| Scenario | Related Requirements |
|----------|---------------------|
| DC-1 | 0013ENR, 0010ENR |
| DC-2 | 0012ENR, 0013ENR |
| DC-3 | Enrollment validation rules |
| DC-4 | 001APP, Budget completeness |
| DC-5 | OSPI workflow requirements |
| DC-6 | Data security, audit requirements |
| CA-1 | Calculation engine requirements |
| CA-2 | Audit trail requirements |
| CA-3-5 | Sandbox/scenario requirements |
| RE-1-5 | Reporting requirements catalog |

### 9.4 Technical Environment Assumptions

| Component | Assumption |
|-----------|------------|
| **Browser** | Chrome 100+, Edge 100+, Firefox 100+ |
| **Network** | Stable internet connection for demonstration |
| **Screen** | 1920x1080 minimum resolution |
| **Demo Data** | Pre-loaded prior to demonstration |
| **User Accounts** | Pre-configured for each user type |

---

*Document End*

**SASQUATCH Demonstration Scenarios Specification v1.0**
**Prepared for OSPI RFP No. 2026-12**
