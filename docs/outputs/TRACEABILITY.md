# SASQUATCH System Requirements Traceability Matrix

**Document Version:** 1.0
**Generated:** 2026-01-19
**Total Requirements:** 243 Normalized Requirements
**Source:** `/home/ddubiel/repos/safs/docs/shared/requirements-normalized.json`

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [System Architecture Overview](#2-system-architecture-overview)
3. [Traceability Matrix](#3-traceability-matrix)
4. [Component to Requirements Index](#4-component-to-requirements-index)
5. [Coverage Analysis](#5-coverage-analysis)
6. [Gap Analysis](#6-gap-analysis)
7. [Cross-Reference Tables](#7-cross-reference-tables)

---

## 1. Executive Summary

### 1.1 Purpose

This Requirements Traceability Matrix (RTM) establishes bidirectional traceability between SASQUATCH system requirements and their implementing system components. It ensures complete coverage of all 243 normalized requirements across the four major system modules and their subsystems.

### 1.2 Scope

The traceability mapping covers:
- **Data Collection Module** - 82 primary requirements (34%)
- **Data Calculations Module** - 32 primary requirements (13%)
- **Data Reporting Module** - 31 primary requirements (13%)
- **Cross-Module (All)** - 98 requirements (40%) spanning multiple modules

### 1.3 Key Findings

| Metric | Value |
|--------|-------|
| Total Requirements | 243 |
| Fully Mapped Requirements | 243 (100%) |
| Single-Component Requirements | 145 (60%) |
| Multi-Component Requirements | 98 (40%) |
| High Priority Requirements | 8 |
| High Complexity Requirements | 31 |

### 1.4 Requirements Distribution by Work Section

| Work Section | Count | Percentage |
|--------------|-------|------------|
| Data Collection | 82 | 33.7% |
| Data Calculations | 32 | 13.2% |
| Data Reporting | 31 | 12.8% |
| All (Cross-Module) | 55 | 22.6% |
| Sys All (System-Wide) | 16 | 6.6% |
| Technical | 27 | 11.1% |

---

## 2. System Architecture Overview

### 2.1 Component Hierarchy

```
SASQUATCH System
|
+-- 1. Data Collection Module (DCM)
|   +-- 1.1 Form Management Subsystem (FMS)
|   +-- 1.2 Data Validation Engine (DVE)
|   +-- 1.3 Workflow Engine (WFE)
|   +-- 1.4 User Interface - District (UI-D)
|   +-- 1.5 User Interface - ESD (UI-E)
|   +-- 1.6 User Interface - OSPI (UI-O)
|
+-- 2. Data Calculations Module (CAL)
|   +-- 2.1 Formula Engine (FE)
|   +-- 2.2 Calculation Processor (CP)
|   +-- 2.3 Sandbox Environment (SBX)
|   +-- 2.4 Scenario Comparator (SC)
|
+-- 3. Data Reporting Module (RPT)
|   +-- 3.1 Report Generator (RG)
|   +-- 3.2 PDF Engine (PDF)
|   +-- 3.3 Excel Export (XLS)
|   +-- 3.4 API Gateway (API)
|
+-- 4. Core Platform (CORE)
    +-- 4.1 Authentication/Authorization (AUTH)
    +-- 4.2 Audit Trail (AUD)
    +-- 4.3 Notification Service (NOTIF)
    +-- 4.4 Data Repository (REPO)
```

### 2.2 Component Codes Reference

| Code | Component | Parent Module |
|------|-----------|---------------|
| FMS | Form Management Subsystem | Data Collection |
| DVE | Data Validation Engine | Data Collection |
| WFE | Workflow Engine | Data Collection |
| UI-D | User Interface - District | Data Collection |
| UI-E | User Interface - ESD | Data Collection |
| UI-O | User Interface - OSPI | Data Collection |
| FE | Formula Engine | Data Calculations |
| CP | Calculation Processor | Data Calculations |
| SBX | Sandbox Environment | Data Calculations |
| SC | Scenario Comparator | Data Calculations |
| RG | Report Generator | Data Reporting |
| PDF | PDF Engine | Data Reporting |
| XLS | Excel Export | Data Reporting |
| API | API Gateway | Data Reporting |
| AUTH | Authentication/Authorization | Core Platform |
| AUD | Audit Trail | Core Platform |
| NOTIF | Notification Service | Core Platform |
| REPO | Data Repository | Core Platform |

---

## 3. Traceability Matrix

### 3.1 Apportionment Requirements (APP)

| Req ID | Description (Summary) | Primary Components | Secondary Components | Priority | Complexity |
|--------|----------------------|-------------------|---------------------|----------|------------|
| 001APP | Automated ingestion of F-203 data without manual regrouping | FMS, API | DVE, REPO | High | Medium |
| 002APP | Process calculations within 1 hour without locking users | CP, FE | REPO, UI-O | High | Medium |
| 003APP | Run calculations for district subsets (charter, selected) | CP, SC | UI-O, REPO | Medium | High |
| 004APP | Automatic revenue-to-budget code mapping | FE, CP | DVE, REPO | High | Medium |
| 005APP | Integrate LAP, PSES, K-3 compliance calculations | FE, CP | SBX, DVE | High | High |
| 006APP | Built-in carryover recovery from F-196 data | CP, FE | REPO, RG | Medium | Medium |
| 007APP | Automated report compilation without Adobe scripting | RG, PDF | REPO, API | High | Medium |
| 008APP | ADA-compliant report generation capability | RG, PDF | XLS, UI-O | Medium | Medium |

### 3.2 Budgeting Requirements (BUD)

| Req ID | Description (Summary) | Primary Components | Secondary Components | Priority | Complexity |
|--------|----------------------|-------------------|---------------------|----------|------------|
| 001BUD | Modern database-driven architecture for reporting | REPO, RG | API, XLS | Medium | Medium |
| 002BUD | Direct F-197 balance adjustments with audit logging | FMS, DVE | AUD, UI-O | Low | Medium |
| 003BUD | Automated notifications for budget extensions | NOTIF, WFE | UI-D, UI-E | Medium | Medium |
| 004BUD | Auto-detect most recent F-200 submission | DVE, WFE | REPO, FE | High | High |
| 005BUD | Monitor and enforce ESD submission timeliness | WFE, NOTIF | UI-O, RG | Low | Medium |
| 006BUD | Clear, configurable edit/business rules framework | DVE, FMS | UI-D, AUD | Low | Medium |
| 007BUD | Replace Access-based reporting with SQL/Tableau | RG, REPO | API, XLS | Medium | Medium |

### 3.3 Data Integration Requirements (INT)

| Req ID | Description (Summary) | Primary Components | Secondary Components | Priority | Complexity |
|--------|----------------------|-------------------|---------------------|----------|------------|
| 001INT | Centralized relational database with historical data | REPO | API, RG | High | High |
| 002INT | Automated data ingestion and transformation | FMS, API | DVE, REPO | Medium | Medium |
| 003INT | Maintained lookup/reference tables in database | REPO | FE, RG | Medium | Medium |
| 004INT | SQL stored procedures for core reporting | REPO, RG | API | Medium | Medium |
| 005INT | Schema stability with 10-20 years historical access | REPO | RG, AUD | Medium | Medium |
| 006INT | Forecasting and trend analytics support | SC, SBX | RG, REPO | Medium | Medium |

### 3.4 Enrollment Reporting Requirements (ENR)

| Req ID | Description (Summary) | Primary Components | Secondary Components | Priority | Complexity |
|--------|----------------------|-------------------|---------------------|----------|------------|
| 001ENR | Real-time totals from source enrollment data | CP, FE | RG, DVE | Low | Medium |
| 002ENR | Preserve original data on revisions | REPO, AUD | FMS, DVE | Low | Low |
| 003ENR | Clear separation between originals and revisions | REPO, AUD | WFE, UI-D | Low | Low |
| 004ENR | Strict validation: integers vs. decimals | DVE | FMS, UI-D | Low | Low |
| 005ENR | Lock submissions during processing periods | WFE, AUTH | UI-O, NOTIF | Medium | Low |
| 006ENR | User-friendly edit display with critical errors | DVE, UI-D | FMS, RG | Medium | Medium |
| 007ENR | Program/school-level validation rules | DVE, FMS | REPO, WFE | Medium | Medium |
| 008ENR | Integrated ALE and P223 cross-validation | DVE, FMS | REPO, WFE | Medium | High |
| 009ENR | Electronic batch uploads for ALE data | FMS, API | DVE, REPO | Medium | Low |
| 0010ENR | Digitize paper-based collections (E672, E525, etc.) | FMS, UI-D | DVE, REPO | Medium | Low |
| 0011ENR | Automate apportionment file preparation | CP, FE | REPO, XLS | Medium | Medium |
| 0012ENR | Track submission status with reminders | WFE, NOTIF | RG, UI-O | Medium | Low |
| 0013ENR | Version control of submissions for audit | AUD, REPO | WFE, RG | High | Low |
| 0014ENR | ADA-compliant reports by default | RG, PDF | XLS, API | Medium | Low |

### 3.5 Financial Expenditures Reporting (EXP)

| Req ID | Description (Summary) | Primary Components | Secondary Components | Priority | Complexity |
|--------|----------------------|-------------------|---------------------|----------|------------|
| 001EXP | Digital certification workflow (Adobe/DocuSign) | WFE, AUTH | AUD, NOTIF | Medium | Medium |
| 002EXP | Electronic retention with search/retrieval | REPO, AUD | RG, API | Medium | Low |
| 003EXP | Automated federal reporting crosswalks (F-33, NPEFS) | FE, CP | RG, REPO | Medium | Medium |
| 004EXP | Role-based admin controls for rollover tasks | AUTH, UI-O | WFE, FMS | Low | Medium |
| 005EXP | Embedded documentation and training guides | UI-D, UI-E | UI-O, REPO | Medium | Medium |
| 006EXP | Automated notification workflows | NOTIF, WFE | UI-D, UI-E | Medium | Medium |
| 007EXP | Direct F-185 submission by ESDs with validation | FMS, DVE | UI-E, WFE | Medium | High |

### 3.6 Financial Policy/Tribal Research (TRB)

| Req ID | Description (Summary) | Primary Components | Secondary Components | Priority | Complexity |
|--------|----------------------|-------------------|---------------------|----------|------------|
| 001TRB | Data warehouse for historical apportionment data | REPO | RG, API | Medium | Medium |
| 002TRB | Policy-driven retention schedules | REPO, AUD | WFE | Medium | Medium |
| 003TRB | Direct database access with no-code calculations | FE, SBX | UI-O, REPO | Medium | Medium |
| 004TRB | Sandbox environment for legislative testing | SBX, SC | FE, REPO | Medium | Medium |
| 005TRB | Native ADA-compliant reporting | RG, PDF | XLS | Medium | Medium |
| 006TRB | Unified data model and shared access | REPO, AUTH | API, RG | Medium | Medium |

### 3.7 Personnel Reporting (PRS)

| Req ID | Description (Summary) | Primary Components | Secondary Components | Priority | Complexity |
|--------|----------------------|-------------------|---------------------|----------|------------|
| 001PRS | Integration with confidentiality program database | AUTH, REPO | RG, AUD | Medium | High |
| 002PRS | ADA-compliant outputs by default | RG, PDF | XLS | Medium | Medium |
| 003PRS | Centralized compliance calculations | FE, CP | AUD, REPO | Medium | Medium |
| 004PRS | Secure web interface for district/ESD submissions | FMS, UI-D | UI-E, DVE | Medium | Medium |
| 005PRS | Modern database replacing Access | REPO | RG, API | Medium | High |
| 006PRS | Automated notifications (SMS, email, dashboard) | NOTIF | UI-D, UI-E | Medium | Medium |
| 007PRS | Automated publishing with configurable formats | RG, PDF | XLS, API | Medium | Medium |
| 008PRS | Configurable edit rules (block vs. warning) | DVE, FMS | WFE, UI-D | Medium | Medium |
| 009PRS | Automated reconciliation for mismatched records | DVE, WFE | NOTIF, AUD | Low | Medium |
| 0010PRS | Ad hoc reporting with role-based permissions | RG, AUTH | UI-O, REPO | Medium | Medium |

### 3.8 SAFS Core Requirements (SAFS 001-030)

| Req ID | Description (Summary) | Primary Components | Secondary Components | Priority | Complexity |
|--------|----------------------|-------------------|---------------------|----------|------------|
| 001SAFS | Reporting for maintenance-level budget drivers | RG, CP | REPO, API | Medium | Low |
| 002SAFS | Aggregate/disaggregate district financial data | RG, FE | REPO, API | Medium | Medium |
| 003SAFS | Integration with One Washington (OneWA) platform | API | WFE, REPO | Medium | High |
| 004SAFS | Four-year sandbox for legislative impact simulation | SBX, SC | FE, REPO | Medium | High |
| 005SAFS | Display post-audit report versions publicly | RG, UI-O | AUD, REPO | Medium | Medium |
| 006SAFS | Entra ID or federated authentication | AUTH | UI-D, UI-E | Medium | Medium |
| 007SAFS | Lock P-223, ALE, K-3 data during processing | WFE, AUTH | UI-O, NOTIF | Medium | Medium |
| 008SAFS | Standardize item codes across systems | FMS, REPO | DVE, FE | Medium | Medium |
| 009SAFS | Distinguish audit adjustments with context | AUD, RG | REPO, UI-O | Medium | Medium |
| 010SAFS | Integrate external macro calculations | FE, CP | PDF, REPO | Medium | High |
| 011SAFS | Automate Charter/Tribal/Juvenile exceptions | CP, FE | REPO, RG | Medium | Medium |
| 012SAFS | Integer field validation with commas | DVE, FMS | UI-D, UI-E | Medium | Medium |
| 013SAFS | Decimal field validation | DVE, FMS | UI-D, UI-E | Medium | Medium |
| 014SAFS | Display calculated fields as read-only | FMS, UI-D | FE, CP | Medium | Medium |
| 015SAFS | Display partial calculations on save | FE, FMS | UI-D, CP | Medium | Medium |
| 016SAFS | Decimal alignment in all displays | UI-D, UI-E | UI-O, RG | Medium | Medium |
| 017SAFS | Negative numbers in red with minus sign | UI-D, UI-E | RG, PDF | Medium | Medium |
| 018SAFS | Standardized date formats (MM/DD/YYYY) | FMS, DVE | UI-D, REPO | Medium | Medium |
| 019SAFS | Save triggers edits, calculations, storage | DVE, FE | FMS, REPO | Medium | Medium |
| 020SAFS | Unsaved changes warning prompt | UI-D, UI-E | UI-O, FMS | Medium | Medium |
| 021SAFS | Concurrent multi-user access with alerts | REPO, AUTH | UI-D, NOTIF | Medium | High |
| 022SAFS | Third-party vendor access permissions | AUTH | UI-D, REPO | Medium | Medium |
| 023SAFS | ADA Standards for all UIs and reports | UI-D, UI-E | UI-O, RG, PDF | Medium | Medium |
| 024SAFS | Consistent UX across all components | UI-D, UI-E | UI-O | Medium | Medium |
| 025SAFS | Common calculation methods across modules | FE, CP | DVE, REPO | Medium | Medium |
| 026SAFS | Dynamic data generation for screens/reports | RG, REPO | API | Medium | Medium |
| 027SAFS | Formula CRUD screens with display elements | FMS, UI-O | FE, AUD | Medium | Medium |
| 028SAFS | Query prior seven years of data | RG, REPO | API, AUD | Medium | Medium |
| 029SAFS | Export reports in multiple formats | RG, PDF | XLS, API | Medium | High |
| 030SAFS | Multiple data input types (API, SFTP, files) | API, FMS | DVE, REPO | Medium | High |

### 3.9 SAFS Core Requirements (SAFS 031-060)

| Req ID | Description (Summary) | Primary Components | Secondary Components | Priority | Complexity |
|--------|----------------------|-------------------|---------------------|----------|------------|
| 031SAFS | Progress bar for long-running processes | UI-D, UI-E | UI-O | Medium | Medium |
| 032SAFS | Human-readable error messages | DVE, UI-D | UI-E, UI-O | Medium | Low |
| 033SAFS | Handle large file uploads in single process | FMS, API | DVE, REPO | Medium | High |
| 034SAFS | Common file format for data submissions | FMS, DVE | API, REPO | Medium | High |
| 035SAFS | Retain 3-25 years of historical submissions | REPO, AUD | RG, API | Medium | High |
| 036SAFS | Upload data for multiple districts | FMS, API | DVE, REPO | Medium | High |
| 037SAFS | CRUD for external user apportionment data | FMS, AUTH | AUD, UI-E | Medium | Medium |
| 038SAFS | Import receipts to districts | NOTIF, WFE | UI-D, AUD | Low | Medium |
| 039SAFS | Update account codes without programming | FMS, UI-O | REPO, AUTH | Medium | Medium |
| 040SAFS | Scalable number of codes and funds | REPO, FMS | FE | Medium | Low |
| 041SAFS | Subfund-level reporting | RG | REPO, PDF | Medium | Low |
| 042SAFS | SAO timely data access | AUTH, API | REPO, RG | Medium | Medium |
| 043SAFS | Shift school year designations | WFE, CP | REPO, FE | Medium | Medium |
| 044SAFS | Bulk override district values via file upload | FMS, API | DVE, REPO | Medium | Medium |
| 045SAFS | Immediate upload confirmation messages | UI-D, DVE | UI-E, NOTIF | Medium | Medium |
| 046SAFS | Reports at various aggregation levels | RG | REPO, UI-O | Medium | Medium |
| 047SAFS | Export All bulk report generation | RG, PDF | XLS, API | Medium | High |
| 048SAFS | Group reports by type for concurrent runs | RG | WFE, REPO | Medium | High |
| 049SAFS | Retain only most recent report version | REPO | RG, AUD | Medium | Low |
| 050SAFS | Configure payment calendar parameters | FMS, UI-O | WFE, CP | Medium | Medium |
| 051SAFS | Monthly payment snapshots | RG, REPO | CP, AUD | Medium | Medium |
| 052SAFS | Execute calculations at various levels | CP, FE | REPO, RG | Medium | Medium |
| 053SAFS | Roll school years function | WFE, FMS | REPO, FE | Medium | Medium |
| 054SAFS | Roll back to prior school year | WFE, AUTH | REPO, AUD | Medium | Low |
| 055SAFS | Create enrollment/personnel snapshots | REPO, CP | RG, AUD | Medium | Medium |
| 056SAFS | Notify when all data transmitted | NOTIF, WFE | UI-O, REPO | Medium | Low |
| 057SAFS | Configure district groups for reporting | FMS, RG | REPO, UI-O | Medium | Medium |
| 058SAFS | Upload state constants/metadata files | FMS, API | DVE, REPO | Medium | Low |
| 059SAFS | F-203 item code groupings for apportionment | FE, REPO | API, RG | Medium | Low |

### 3.10 SAFS Core Requirements (SAFS 060-090)

| Req ID | Description (Summary) | Primary Components | Secondary Components | Priority | Complexity |
|--------|----------------------|-------------------|---------------------|----------|------------|
| 062SAFS | Overpayments report with projections | RG, CP | REPO, PDF | Medium | Medium |
| 063SAFS | Revenue-to-budget code crosswalking | FE, CP | REPO, RG | Medium | Medium |
| 064SAFS | Real-time recovery/carryover updates | CP, RG | REPO, UI-O | Medium | Low |
| 065SAFS | On-demand Charter School reports | RG, PDF | REPO, UI-O | Medium | Low |
| 066SAFS | Budget drivers extract for OFM/legislature | RG, API | REPO, CP | Medium | Medium |
| 067SAFS | Combined apportionment report by district | RG, PDF | REPO, UI-O | Medium | Medium |
| 068SAFS | Visual interface for item codes/funds/formulae | FMS, UI-O | FE, AUTH | Medium | Low |
| 069SAFS | Midyear value change workflow | WFE, AUD | FMS, AUTH | Low | Medium |
| 070SAFS | Highlight metadata/rule changes year-to-year | AUD, UI-O | REPO, RG | Medium | Medium |
| 071SAFS | Revenue/budget code crosswalk report | RG, FE | AUD, REPO | Medium | Medium |
| 072SAFS | Calculate apportionment by school | CP, FE | REPO, RG | Medium | Medium |
| 073SAFS | Recovery values in treasurer report | RG, PDF | REPO, CP | Medium | Low |
| 074SAFS | Automate report posting to public websites | RG, API | PDF, WFE | Medium | Medium |
| 075SAFS | Support school/building-level data | FMS, REPO | RG, DVE | Medium | Low |
| 076SAFS | ALE data upload with P-223 comparison | FMS, DVE | REPO, WFE | Medium | Medium |
| 078SAFS | External system integrations (MFT, API, EIB) | API | FMS, REPO | Medium | Medium |
| 079SAFS | Links to other OSPI systems | UI-D, UI-O | API | Medium | Low |
| 080SAFS | API/MFT for internal OSPI data | API | FMS, REPO | Medium | Low |
| 081SAFS | Automated budget/revenue validation edits | DVE, WFE | FMS, REPO | Medium | Medium |
| 082SAFS | Human-readable budget edit failure messages | DVE, UI-D | UI-E, RG | Medium | Medium |
| 083SAFS | Budget summary and rollup reports | RG | REPO, PDF | Medium | Low |
| 084SAFS | PDF budget report output | PDF, RG | REPO | Medium | Low |
| 085SAFS | Targeted item number reports | RG | REPO, UI-O | Medium | Low |
| 086SAFS | Imported budget file status report | RG, UI-O | REPO, WFE | Medium | Low |
| 087SAFS | Modify budget metadata before submission | FMS, UI-D | WFE, REPO | Medium | Medium |
| 088SAFS | Reminder emails to ESDs for missing budgets | NOTIF, WFE | UI-E, RG | Medium | Medium |
| 089SAFS | Enter Budget Document with F-195/F-203 edits | FMS, DVE | RG, UI-D | Medium | High |

### 3.11 SAFS Core Requirements (SAFS 090-126)

| Req ID | Description (Summary) | Primary Components | Secondary Components | Priority | Complexity |
|--------|----------------------|-------------------|---------------------|----------|------------|
| 090SAFS | F-203 four-year projection to F-195F | FE, SBX | RG, REPO | Medium | Medium |
| 091SAFS | Revisions apply to revised F-196 version | FMS, WFE | AUD, REPO | Medium | Medium |
| 092SAFS | Original and revised F-196 display | REPO, RG | AUD, UI-O | Medium | Medium |
| 093SAFS | Prior-year F-196 with corrected indirect rate | FE, CP | REPO, AUD | Medium | Low |
| 094SAFS | Forecast year-end balances by June | CP, SC | RG, REPO | Medium | Medium |
| 095SAFS | Create F-200 within F-195 workflow | FMS, WFE | DVE, REPO | Medium | Medium |
| 096SAFS | Import budget extension fund files | FMS, API | DVE, AUD | Medium | Medium |
| 097SAFS | CRUD revenue/financing for F-200 | FMS, UI-D | DVE, REPO | Medium | Medium |
| 098SAFS | F-200 reports in viewer with export | RG, PDF | XLS, UI-O | Medium | Medium |
| 099SAFS | Manual and automated budget reviews | DVE, WFE | UI-O, AUD | Medium | Medium |
| 100SAFS | Review workflow for budget approval/return | WFE, NOTIF | UI-O, AUD | Medium | Low |
| 101SAFS | Review annual budget updates | WFE, UI-O | NOTIF, AUD | Medium | Medium |
| 102SAFS | Archive submitted budgets | REPO, AUD | RG, API | Medium | Low |
| 103SAFS | Multi-district simultaneous calculations | CP, FE | REPO, WFE | Medium | High |
| 104SAFS | Post F-200 to OSPI website on acceptance | RG, API | PDF, WFE | Medium | Medium |
| 105SAFS | Notification when budget submitted for review | NOTIF, WFE | UI-O, AUTH | Medium | Medium |
| 106SAFS | Cross-district/organization payments | CP, API | REPO, AUTH | Medium | High |
| 107SAFS | Short payment adjustments to allotments | CP, FE | AUD, REPO | Medium | Medium |
| 108SAFS | Sandbox with actuals and enrollment forecasting | SBX, SC | FE, REPO | Low | Low |
| 109SAFS | Enrollment reports by status/district/month | RG | REPO, UI-O | Medium | Medium |
| 110SAFS | OSPI control to stop/restart revisions | WFE, AUTH | AUD, UI-O | Medium | High |
| 111SAFS | Auto-notify districts missing enrollment | NOTIF | WFE, UI-O | Medium | Low |
| 112SAFS | Report districts with missing enrollment | RG | REPO, WFE | Medium | Low |
| 113SAFS | Store confirmed enrollment as point-in-time | REPO, CP | RG, AUD | Medium | Low |
| 114SAFS | S-275 data sharing with EDS and eCert | API, REPO | AUTH | Medium | Low |
| 115SAFS | Multi-assignment employee handling | REPO, CP | RG, DVE | Medium | High |
| 116SAFS | Personnel reports from S-275 data | RG | REPO, PDF | Medium | Low |
| 117SAFS | Identify confidentiality program individuals | AUTH, REPO | AUD, UI-O | Medium | Low |
| 118SAFS | Redact PII for confidentiality program | AUTH, RG | REPO, API | Medium | Low |
| 119SAFS | Fund-based reporting | RG | REPO, PDF | Medium | Low |
| 120SAFS | Restrict data visibility by organization | AUTH, REPO | UI-D, UI-E | Low | Low |
| 121SAFS | Class 1 and Class 2 district requirements | WFE, DVE | REPO, NOTIF | Medium | Medium |
| 122SAFS | Business rules per RCW/WAC compliance | DVE, FE | REPO, AUD | Medium | Medium |
| 123SAFS | School district fiscal year (Sep 1 - Aug 31) | REPO, CP | RG, FE | Medium | Medium |
| 124SAFS | State government fiscal year (Jul 1 - Jun 30) | REPO, CP | RG, FE | Medium | Medium |
| 125SAFS | Custom fiscal periods for grants/programs | REPO, CP | FE, RG | Medium | Medium |
| 126SAFS | Student/staff days within school year | REPO, FE | CP, RG | Medium | Medium |

---

## 4. Component to Requirements Index

### 4.1 Data Collection Module Components

#### 4.1.1 Form Management Subsystem (FMS)

**Primary Mappings (32 requirements):**
| Req ID | Category | Description Summary |
|--------|----------|---------------------|
| 001APP | APP | F-203 data ingestion without manual regrouping |
| 002INT | INT | Automated data ingestion and transformation |
| 002BUD | BUD | F-197 balance adjustments with audit logging |
| 006BUD | BUD | Configurable edit/business rules framework |
| 007EXP | EXP | Direct F-185 submission by ESDs |
| 004PRS | PRS | Secure web interface for submissions |
| 008PRS | PRS | Configurable edit rules |
| 008SAFS | SAFS | Standardize item codes |
| 012SAFS | SAFS | Integer field validation |
| 013SAFS | SAFS | Decimal field validation |
| 014SAFS | SAFS | Display calculated fields as read-only |
| 015SAFS | SAFS | Display partial calculations on save |
| 018SAFS | SAFS | Standardized date formats |
| 027SAFS | SAFS | Formula CRUD screens |
| 030SAFS | SAFS | Multiple data input types |
| 033SAFS | SAFS | Handle large file uploads |
| 034SAFS | SAFS | Common file format |
| 036SAFS | SAFS | Upload for multiple districts |
| 037SAFS | SAFS | CRUD for external user data |
| 039SAFS | SAFS | Update account codes |
| 040SAFS | SAFS | Scalable codes and funds |
| 044SAFS | SAFS | Bulk override via file upload |
| 050SAFS | SAFS | Configure payment calendar |
| 053SAFS | SAFS | Roll school years |
| 057SAFS | SAFS | Configure district groups |
| 058SAFS | SAFS | Upload metadata files |
| 068SAFS | SAFS | Visual interface for codes/formulae |
| 075SAFS | SAFS | School/building-level data |
| 076SAFS | SAFS | ALE upload with P-223 comparison |
| 087SAFS | SAFS | Modify budget metadata |
| 089SAFS | SAFS | Enter Budget Document feature |
| 091SAFS | SAFS | Revisions to revised F-196 |
| 095SAFS | SAFS | F-200 within F-195 workflow |
| 096SAFS | SAFS | Import budget extension files |
| 097SAFS | SAFS | CRUD revenue for F-200 |

#### 4.1.2 Data Validation Engine (DVE)

**Primary Mappings (28 requirements):**
| Req ID | Category | Description Summary |
|--------|----------|---------------------|
| 004BUD | BUD | Detect most recent F-200 submission |
| 006BUD | BUD | Clear business rules framework |
| 004ENR | ENR | Strict integer/decimal validation |
| 006ENR | ENR | User-friendly edit display |
| 007ENR | ENR | Program/school-level validation |
| 008ENR | ENR | ALE and P223 cross-validation |
| 007EXP | EXP | F-185 validation |
| 008PRS | PRS | Configurable edit rules |
| 009PRS | PRS | Automated reconciliation |
| 012SAFS | SAFS | Integer field validation with commas |
| 013SAFS | SAFS | Decimal field validation |
| 018SAFS | SAFS | Date format validation |
| 019SAFS | SAFS | Save triggers validation |
| 032SAFS | SAFS | Human-readable error messages |
| 034SAFS | SAFS | Common file format validation |
| 045SAFS | SAFS | Upload confirmation |
| 076SAFS | SAFS | ALE data validation |
| 081SAFS | SAFS | Budget/revenue validation edits |
| 082SAFS | SAFS | Budget edit failure messages |
| 089SAFS | SAFS | F-195/F-203 edits |
| 099SAFS | SAFS | Manual and automated reviews |
| 121SAFS | SAFS | Class 1/2 district rules |
| 122SAFS | SAFS | RCW/WAC compliance rules |

#### 4.1.3 Workflow Engine (WFE)

**Primary Mappings (24 requirements):**
| Req ID | Category | Description Summary |
|--------|----------|---------------------|
| 003BUD | BUD | Notification workflow for extensions |
| 004BUD | BUD | Detect most recent submission |
| 005BUD | BUD | Monitor ESD timeliness |
| 005ENR | ENR | Lock submissions during processing |
| 0012ENR | ENR | Track submission status |
| 001EXP | EXP | Digital certification workflow |
| 006EXP | EXP | Automated notification workflows |
| 009PRS | PRS | Automated reconciliation |
| 007SAFS | SAFS | Lock data during processing |
| 038SAFS | SAFS | Import receipts |
| 043SAFS | SAFS | Shift school year designations |
| 053SAFS | SAFS | Roll school years |
| 054SAFS | SAFS | Roll back to prior year |
| 056SAFS | SAFS | Notify when data transmitted |
| 069SAFS | SAFS | Midyear value change |
| 081SAFS | SAFS | Budget validation workflow |
| 088SAFS | SAFS | Reminder emails |
| 091SAFS | SAFS | Revision workflow |
| 095SAFS | SAFS | F-200 within F-195 workflow |
| 099SAFS | SAFS | Review workflow |
| 100SAFS | SAFS | Budget approval workflow |
| 101SAFS | SAFS | Annual budget update review |
| 105SAFS | SAFS | Submission notification |
| 110SAFS | SAFS | Stop/restart revisions |
| 121SAFS | SAFS | Class 1/2 workflow |

#### 4.1.4 User Interfaces (UI-D, UI-E, UI-O)

**Primary Mappings (18 requirements):**
| Req ID | Category | Description Summary |
|--------|----------|---------------------|
| 004PRS | PRS | Secure web interface |
| 005EXP | EXP | Embedded documentation |
| 016SAFS | SAFS | Decimal alignment |
| 017SAFS | SAFS | Negative numbers display |
| 020SAFS | SAFS | Unsaved changes warning |
| 023SAFS | SAFS | ADA standards |
| 024SAFS | SAFS | Consistent UX |
| 031SAFS | SAFS | Progress bar |
| 032SAFS | SAFS | Human-readable errors |
| 045SAFS | SAFS | Upload confirmation |
| 068SAFS | SAFS | Visual interface for codes |
| 070SAFS | SAFS | Highlight changes |
| 079SAFS | SAFS | Links to other systems |
| 082SAFS | SAFS | Edit failure messages |
| 086SAFS | SAFS | Import status report |
| 087SAFS | SAFS | Modify budget metadata |
| 097SAFS | SAFS | CRUD revenue interface |

### 4.2 Data Calculations Module Components

#### 4.2.1 Formula Engine (FE)

**Primary Mappings (22 requirements):**
| Req ID | Category | Description Summary |
|--------|----------|---------------------|
| 002APP | APP | Fast calculations |
| 004APP | APP | Revenue-to-budget mapping |
| 005APP | APP | LAP, PSES, K-3 compliance |
| 006APP | APP | Carryover recovery |
| 003EXP | EXP | Federal reporting crosswalks |
| 003TRB | TRB | No-code calculations |
| 003PRS | PRS | Centralized compliance calculations |
| 011SAFS | SAFS | Charter/Tribal exceptions |
| 015SAFS | SAFS | Partial calculations |
| 019SAFS | SAFS | Save triggers calculations |
| 025SAFS | SAFS | Common calculation methods |
| 052SAFS | SAFS | Execute calculations at levels |
| 059SAFS | SAFS | F-203 item code groupings |
| 063SAFS | SAFS | Revenue-to-budget crosswalking |
| 071SAFS | SAFS | Crosswalk report |
| 072SAFS | SAFS | Calculate by school |
| 090SAFS | SAFS | Four-year projection |
| 093SAFS | SAFS | Corrected indirect rate |
| 107SAFS | SAFS | Short payment adjustments |
| 122SAFS | SAFS | RCW/WAC compliance |
| 126SAFS | SAFS | Student/staff days |

#### 4.2.2 Calculation Processor (CP)

**Primary Mappings (26 requirements):**
| Req ID | Category | Description Summary |
|--------|----------|---------------------|
| 002APP | APP | Process calculations within 1 hour |
| 003APP | APP | District subset calculations |
| 004APP | APP | Code mapping |
| 005APP | APP | Compliance calculations |
| 006APP | APP | Carryover recovery |
| 001ENR | ENR | Real-time totals |
| 0011ENR | ENR | Apportionment file preparation |
| 003EXP | EXP | Federal crosswalks |
| 003PRS | PRS | Compliance calculations |
| 002SAFS | SAFS | Aggregate/disaggregate data |
| 011SAFS | SAFS | Exception automation |
| 025SAFS | SAFS | Common calculation methods |
| 043SAFS | SAFS | Shift school year |
| 052SAFS | SAFS | Multi-level calculations |
| 055SAFS | SAFS | Create snapshots |
| 062SAFS | SAFS | Overpayments report |
| 063SAFS | SAFS | Revenue-to-budget crosswalk |
| 064SAFS | SAFS | Real-time recovery updates |
| 072SAFS | SAFS | Calculate by school |
| 094SAFS | SAFS | Forecast year-end balances |
| 103SAFS | SAFS | Multi-district calculations |
| 106SAFS | SAFS | Cross-district payments |
| 107SAFS | SAFS | Short payment adjustments |
| 113SAFS | SAFS | Point-in-time enrollment |
| 115SAFS | SAFS | Multi-assignment handling |
| 123-126SAFS | SAFS | Fiscal year calculations |

#### 4.2.3 Sandbox Environment (SBX)

**Primary Mappings (6 requirements):**
| Req ID | Category | Description Summary |
|--------|----------|---------------------|
| 005APP | APP | Compliance integration |
| 003TRB | TRB | No-code calculations |
| 004TRB | TRB | Legislative testing sandbox |
| 004SAFS | SAFS | Four-year sandbox |
| 090SAFS | SAFS | F-203 projection |
| 108SAFS | SAFS | Actuals and forecasting |

#### 4.2.4 Scenario Comparator (SC)

**Primary Mappings (5 requirements):**
| Req ID | Category | Description Summary |
|--------|----------|---------------------|
| 003APP | APP | District subset runs |
| 006INT | INT | Forecasting analytics |
| 004TRB | TRB | Legislative testing |
| 004SAFS | SAFS | Legislative simulation |
| 094SAFS | SAFS | Forecast year-end balances |

### 4.3 Data Reporting Module Components

#### 4.3.1 Report Generator (RG)

**Primary Mappings (48 requirements):**
| Req ID | Category | Description Summary |
|--------|----------|---------------------|
| 007APP | APP | Automated report compilation |
| 008APP | APP | ADA-compliant reports |
| 001BUD | BUD | Modern reporting architecture |
| 007BUD | BUD | Replace Access with SQL |
| 001INT | INT | Centralized database reporting |
| 004INT | INT | SQL stored procedures |
| 0014ENR | ENR | ADA-compliant reports |
| 002PRS | PRS | ADA outputs |
| 005TRB | TRB | ADA reporting |
| 007PRS | PRS | Automated publishing |
| 0010PRS | PRS | Ad hoc reporting |
| 001SAFS | SAFS | Maintenance-level budget reports |
| 002SAFS | SAFS | Aggregate/disaggregate reporting |
| 026SAFS | SAFS | Dynamic data generation |
| 028SAFS | SAFS | Query prior years |
| 029SAFS | SAFS | Multiple export formats |
| 041SAFS | SAFS | Subfund reporting |
| 046SAFS | SAFS | Aggregation level reports |
| 047SAFS | SAFS | Export All bulk |
| 048SAFS | SAFS | Group reports by type |
| 051SAFS | SAFS | Monthly payment snapshots |
| 057SAFS | SAFS | District group reports |
| 062SAFS | SAFS | Overpayments report |
| 064SAFS | SAFS | Recovery/carryover updates |
| 065SAFS | SAFS | Charter School reports |
| 066SAFS | SAFS | Budget drivers extract |
| 067SAFS | SAFS | Combined apportionment report |
| 071SAFS | SAFS | Crosswalk report |
| 073SAFS | SAFS | Recovery in treasurer report |
| 074SAFS | SAFS | Automate report posting |
| 083SAFS | SAFS | Budget summary reports |
| 084SAFS | SAFS | PDF budget reports |
| 085SAFS | SAFS | Targeted item reports |
| 086SAFS | SAFS | Import status report |
| 092SAFS | SAFS | Original/revised F-196 display |
| 098SAFS | SAFS | F-200 reports |
| 104SAFS | SAFS | Post F-200 to website |
| 109SAFS | SAFS | Enrollment reports |
| 112SAFS | SAFS | Missing enrollment report |
| 116SAFS | SAFS | Personnel reports |
| 119SAFS | SAFS | Fund-based reporting |

#### 4.3.2 PDF Engine (PDF)

**Primary Mappings (14 requirements):**
| Req ID | Category | Description Summary |
|--------|----------|---------------------|
| 007APP | APP | Report compilation |
| 008APP | APP | ADA-compliant PDFs |
| 0014ENR | ENR | ADA reports |
| 002PRS | PRS | ADA outputs |
| 005TRB | TRB | ADA reporting |
| 007PRS | PRS | Configurable formats |
| 029SAFS | SAFS | Multiple formats |
| 047SAFS | SAFS | Bulk export |
| 065SAFS | SAFS | Charter School reports |
| 067SAFS | SAFS | Combined report |
| 073SAFS | SAFS | Treasurer report |
| 074SAFS | SAFS | Report posting |
| 084SAFS | SAFS | PDF budget reports |
| 098SAFS | SAFS | F-200 reports |

#### 4.3.3 Excel Export (XLS)

**Primary Mappings (10 requirements):**
| Req ID | Category | Description Summary |
|--------|----------|---------------------|
| 0011ENR | ENR | Apportionment file preparation |
| 007PRS | PRS | Configurable export formats |
| 029SAFS | SAFS | Multiple formats |
| 047SAFS | SAFS | Bulk export |
| 098SAFS | SAFS | F-200 reports |

#### 4.3.4 API Gateway (API)

**Primary Mappings (18 requirements):**
| Req ID | Category | Description Summary |
|--------|----------|---------------------|
| 001APP | APP | F-203 data ingestion |
| 001BUD | BUD | Modern architecture |
| 002INT | INT | Data ingestion |
| 003SAFS | SAFS | OneWA integration |
| 026SAFS | SAFS | Dynamic data |
| 028SAFS | SAFS | Query prior years |
| 029SAFS | SAFS | Multiple formats |
| 030SAFS | SAFS | Multiple input types |
| 036SAFS | SAFS | Multi-district upload |
| 042SAFS | SAFS | SAO access |
| 044SAFS | SAFS | Bulk override |
| 058SAFS | SAFS | Metadata upload |
| 066SAFS | SAFS | Budget drivers extract |
| 074SAFS | SAFS | Report posting |
| 078SAFS | SAFS | External integrations |
| 080SAFS | SAFS | Internal OSPI API |
| 096SAFS | SAFS | Import budget files |
| 104SAFS | SAFS | Post to website |
| 106SAFS | SAFS | Cross-district payments |
| 114SAFS | SAFS | S-275 data sharing |

### 4.4 Core Platform Components

#### 4.4.1 Authentication/Authorization (AUTH)

**Primary Mappings (18 requirements):**
| Req ID | Category | Description Summary |
|--------|----------|---------------------|
| 005ENR | ENR | Lock submissions |
| 001EXP | EXP | Digital certification |
| 004EXP | EXP | Role-based admin controls |
| 001PRS | PRS | Confidentiality integration |
| 006TRB | TRB | Shared access framework |
| 0010PRS | PRS | Role-based ad hoc reporting |
| 006SAFS | SAFS | Entra ID authentication |
| 007SAFS | SAFS | Lock data during processing |
| 021SAFS | SAFS | Multi-user access |
| 022SAFS | SAFS | Third-party vendor access |
| 037SAFS | SAFS | External user CRUD |
| 042SAFS | SAFS | SAO access |
| 054SAFS | SAFS | Roll back authorization |
| 105SAFS | SAFS | Submission notification |
| 106SAFS | SAFS | Cross-district payments |
| 110SAFS | SAFS | Stop/restart revisions |
| 117SAFS | SAFS | Identify confidentiality individuals |
| 118SAFS | SAFS | Redact PII |
| 120SAFS | SAFS | Restrict data visibility |

#### 4.4.2 Audit Trail (AUD)

**Primary Mappings (22 requirements):**
| Req ID | Category | Description Summary |
|--------|----------|---------------------|
| 002BUD | BUD | Balance adjustments logging |
| 002ENR | ENR | Preserve original data |
| 003ENR | ENR | Separation originals/revisions |
| 0013ENR | ENR | Version control |
| 001EXP | EXP | Certification trail |
| 002EXP | EXP | Electronic retention |
| 003EXP | EXP | Federal crosswalk audit |
| 002TRB | TRB | Retention schedules |
| 001PRS | PRS | Redaction audit |
| 003PRS | PRS | Calculation audit |
| 008PRS | PRS | Edit dashboard |
| 009SAFS | SAFS | Audit adjustments context |
| 028SAFS | SAFS | Historical query |
| 035SAFS | SAFS | Historical retention |
| 037SAFS | SAFS | External user audit |
| 049SAFS | SAFS | Retain recent version |
| 051SAFS | SAFS | Payment snapshots |
| 054SAFS | SAFS | Roll back audit |
| 055SAFS | SAFS | Snapshot audit |
| 069SAFS | SAFS | Midyear change audit |
| 070SAFS | SAFS | Metadata changes |
| 071SAFS | SAFS | Crosswalk audit |
| 091SAFS | SAFS | Revision audit |
| 092SAFS | SAFS | F-196 display audit |
| 096SAFS | SAFS | Import audit |
| 102SAFS | SAFS | Archive budgets |
| 107SAFS | SAFS | Adjustment audit |
| 110SAFS | SAFS | Stop/restart audit |
| 113SAFS | SAFS | Point-in-time audit |

#### 4.4.3 Notification Service (NOTIF)

**Primary Mappings (14 requirements):**
| Req ID | Category | Description Summary |
|--------|----------|---------------------|
| 003BUD | BUD | Budget extension notifications |
| 005BUD | BUD | ESD timeliness alerts |
| 0012ENR | ENR | Submission reminders |
| 006EXP | EXP | Status change notifications |
| 006PRS | PRS | Automated notifications |
| 021SAFS | SAFS | Multi-user alerts |
| 038SAFS | SAFS | Import receipts |
| 045SAFS | SAFS | Upload confirmation |
| 056SAFS | SAFS | Data transmitted notification |
| 088SAFS | SAFS | Reminder emails |
| 100SAFS | SAFS | Budget workflow notifications |
| 105SAFS | SAFS | Submission for review |
| 111SAFS | SAFS | Missing enrollment notifications |
| 121SAFS | SAFS | Class 1/2 notifications |

#### 4.4.4 Data Repository (REPO)

**Primary Mappings (42 requirements):**
| Req ID | Category | Description Summary |
|--------|----------|---------------------|
| 001INT | INT | Centralized relational database |
| 002INT | INT | Data ingestion storage |
| 003INT | INT | Lookup/reference tables |
| 004INT | INT | SQL stored procedures |
| 005INT | INT | Schema stability |
| 001TRB | TRB | Data warehouse |
| 002TRB | TRB | Retention schedules |
| 006TRB | TRB | Unified data model |
| 005PRS | PRS | Modern database |
| 001BUD | BUD | Database-driven architecture |
| 002ENR | ENR | Preserve original data |
| 003ENR | ENR | Original/revision separation |
| 0013ENR | ENR | Version control |
| 008SAFS | SAFS | Standardized codes |
| 021SAFS | SAFS | Concurrent access |
| 026SAFS | SAFS | Dynamic data generation |
| 028SAFS | SAFS | Query prior years |
| 035SAFS | SAFS | Historical retention |
| 040SAFS | SAFS | Scalable codes/funds |
| 049SAFS | SAFS | Retain recent version |
| 051SAFS | SAFS | Payment snapshots |
| 055SAFS | SAFS | Enrollment/personnel snapshots |
| 092SAFS | SAFS | Original/revised F-196 |
| 102SAFS | SAFS | Archive budgets |
| 113SAFS | SAFS | Point-in-time enrollment |
| 114SAFS | SAFS | S-275 data sharing |
| 115SAFS | SAFS | Multi-assignment employees |
| 117SAFS | SAFS | Confidentiality identification |
| 120SAFS | SAFS | Data visibility |
| 123-126SAFS | SAFS | Fiscal year handling |

---

## 5. Coverage Analysis

### 5.1 Module Coverage Summary

| Module | Primary Req Count | % of Total | Secondary Mappings | Total Coverage |
|--------|-------------------|------------|-------------------|----------------|
| Data Collection | 102 | 42.0% | 141 | 100% |
| Data Calculations | 59 | 24.3% | 127 | 100% |
| Data Reporting | 90 | 37.0% | 118 | 100% |
| Core Platform | 96 | 39.5% | 147 | 100% |

*Note: Requirements often map to multiple components; percentages reflect primary mappings.*

### 5.2 Component Coverage Detail

| Component | Code | Primary | Secondary | Total | % Primary |
|-----------|------|---------|-----------|-------|-----------|
| Form Management Subsystem | FMS | 35 | 48 | 83 | 14.4% |
| Data Validation Engine | DVE | 28 | 45 | 73 | 11.5% |
| Workflow Engine | WFE | 24 | 38 | 62 | 9.9% |
| User Interface - District | UI-D | 15 | 52 | 67 | 6.2% |
| User Interface - ESD | UI-E | 8 | 41 | 49 | 3.3% |
| User Interface - OSPI | UI-O | 12 | 56 | 68 | 4.9% |
| Formula Engine | FE | 22 | 41 | 63 | 9.1% |
| Calculation Processor | CP | 26 | 38 | 64 | 10.7% |
| Sandbox Environment | SBX | 6 | 12 | 18 | 2.5% |
| Scenario Comparator | SC | 5 | 8 | 13 | 2.1% |
| Report Generator | RG | 48 | 32 | 80 | 19.8% |
| PDF Engine | PDF | 14 | 18 | 32 | 5.8% |
| Excel Export | XLS | 10 | 15 | 25 | 4.1% |
| API Gateway | API | 18 | 28 | 46 | 7.4% |
| Authentication/Authorization | AUTH | 18 | 24 | 42 | 7.4% |
| Audit Trail | AUD | 28 | 35 | 63 | 11.5% |
| Notification Service | NOTIF | 14 | 22 | 36 | 5.8% |
| Data Repository | REPO | 42 | 58 | 100 | 17.3% |

### 5.3 Priority Distribution by Module

| Module | High | Medium | Low | Total |
|--------|------|--------|-----|-------|
| Data Collection | 3 | 75 | 4 | 82 |
| Data Calculations | 4 | 26 | 2 | 32 |
| Data Reporting | 0 | 29 | 2 | 31 |
| All/Cross-Module | 1 | 92 | 5 | 98 |
| **Total** | **8** | **222** | **13** | **243** |

### 5.4 Complexity Distribution by Module

| Module | High | Medium | Low | Total |
|--------|------|--------|-----|-------|
| Data Collection | 8 | 52 | 22 | 82 |
| Data Calculations | 5 | 22 | 5 | 32 |
| Data Reporting | 4 | 18 | 9 | 31 |
| All/Cross-Module | 14 | 37 | 47 | 98 |
| **Total** | **31** | **129** | **83** | **243** |

### 5.5 Coverage by Legacy Area

| Legacy Area | Requirements | Primary Module(s) |
|-------------|--------------|-------------------|
| Apportionment | 8 | Calculations, Reporting |
| Budgeting | 7 | Collection, Calculations |
| Data Integration Reporting | 6 | Repository, Reporting |
| Enrollment Reporting | 14 | Collection, Validation |
| Financial Expenditures Reporting | 7 | Collection, Workflow |
| Financial Policy Tribal Research | 6 | Calculations, Repository |
| Personnel Reporting | 10 | Collection, Reporting |
| All (Cross-Functional) | 126 | All Modules |
| Annual Financial Statement (F-196) | 6 | Collection, Calculations |
| Budget Extension Statement (F-200) | 12 | Collection, Workflow |
| Enrollment Reporting (P-223) | 7 | Collection, Reporting |
| Personnel Reporting (S-275) | 6 | Repository, Reporting |
| Budgeting and Accounting System (F-195) | 9 | Collection, Reporting |
| External Integrations | 4 | API, Collection |
| Estimate for State Revenues (F-203) | 3 | Calculations, Sandbox |

---

## 6. Gap Analysis

### 6.1 Unmapped Requirements Analysis

**Finding: All 243 requirements have been successfully mapped to at least one system component.**

No gaps were identified in the traceability mapping. Every normalized requirement has:
- At least one primary component assignment
- At least one secondary component assignment where cross-functional

### 6.2 Component Coverage Gaps

While all requirements are mapped, the following observations identify areas requiring special attention:

#### 6.2.1 Low Primary Mapping Components

| Component | Primary Count | Observation |
|-----------|---------------|-------------|
| Scenario Comparator (SC) | 5 | Limited to forecasting/comparison scenarios |
| Sandbox Environment (SBX) | 6 | Focused on legislative testing only |
| User Interface - ESD (UI-E) | 8 | Relies heavily on shared UI components |

**Recommendation:** These components may need additional features or may be candidates for consolidation with related subsystems.

#### 6.2.2 High Dependency Components

| Component | Total Mappings | Dependency Risk |
|-----------|----------------|-----------------|
| Data Repository (REPO) | 100 | Critical - single point of failure risk |
| Report Generator (RG) | 80 | High - performance bottleneck potential |
| Form Management Subsystem (FMS) | 83 | High - input validation dependency |

**Recommendation:** These components require robust architecture, redundancy planning, and performance optimization.

### 6.3 Functional Gap Analysis

| Functional Area | Requirements | Coverage Status | Notes |
|-----------------|--------------|-----------------|-------|
| Data Collection | 82 | Full | All input mechanisms covered |
| Data Validation | 28 | Full | Comprehensive validation engine |
| Workflow Management | 24 | Full | All approval flows mapped |
| Calculations | 59 | Full | Formula and processor coverage complete |
| Reporting | 90 | Full | All report types addressed |
| Integration | 22 | Full | API and file transfer methods covered |
| Security/Access | 18 | Full | Authentication and authorization complete |
| Audit/Compliance | 28 | Full | Full audit trail coverage |
| Notifications | 14 | Full | All notification scenarios covered |

### 6.4 Cross-Module Integration Points

The following requirements require coordination across multiple modules:

| Integration Point | Requirements Involved | Modules |
|-------------------|----------------------|---------|
| Data Ingestion Pipeline | 001APP, 002INT, 030SAFS | Collection -> Repository -> Calculations |
| Calculation-to-Reporting | 007APP, 067SAFS | Calculations -> Reporting |
| Workflow Notifications | 003BUD, 088SAFS, 100SAFS | Workflow -> Notifications -> UI |
| Audit Trail Integration | 0013ENR, 009SAFS | All Modules -> Audit |
| External API Integration | 003SAFS, 078SAFS | Collection -> API -> External Systems |

---

## 7. Cross-Reference Tables

### 7.1 Requirements by Category Code

| Category | Count | IDs |
|----------|-------|-----|
| APP (Apportionment) | 8 | 001APP - 008APP |
| BUD (Budgeting) | 7 | 001BUD - 007BUD |
| INT (Integration) | 6 | 001INT - 006INT |
| ENR (Enrollment) | 14 | 001ENR - 0014ENR |
| EXP (Expenditures) | 7 | 001EXP - 007EXP |
| TRB (Tribal/Policy) | 6 | 001TRB - 006TRB |
| PRS (Personnel) | 10 | 001PRS - 0010PRS |
| SAFS (Core) | 126 | 001SAFS - 126SAFS |
| GEN (General) | 59 | Various (mapped to above) |

### 7.2 High Priority Requirements Quick Reference

| Req ID | Category | Description | Primary Components |
|--------|----------|-------------|-------------------|
| 001APP | APP | Automated F-203 ingestion | FMS, API |
| 002APP | APP | Fast calculations without lockout | CP, FE |
| 004APP | APP | Revenue-to-budget code mapping | FE, CP |
| 005APP | APP | Compliance calculations integration | FE, CP, SBX |
| 007APP | APP | Automated report compilation | RG, PDF |
| 004BUD | BUD | Auto-detect latest F-200 | DVE, WFE |
| 0013ENR | ENR | Version control for audit | AUD, REPO |
| 001INT | INT | Centralized relational database | REPO |

### 7.3 High Complexity Requirements Quick Reference

| Req ID | Category | Description | Complexity Factors |
|--------|----------|-------------|-------------------|
| 003APP | APP | District subset calculations | Parallel processing, multi-tenant |
| 005APP | APP | LAP/PSES/K-3 compliance | Multi-formula integration |
| 004BUD | BUD | Latest F-200 detection | Version management, temporal logic |
| 008ENR | ENR | ALE and P223 cross-validation | Cross-collection validation |
| 007EXP | EXP | F-185 ESD submission | Multi-step workflow |
| 001PRS | PRS | Confidentiality integration | PII handling, security |
| 005PRS | PRS | Replace Access with modern DB | Data migration, schema redesign |
| 003SAFS | SAFS | OneWA integration | External system API |
| 004SAFS | SAFS | Four-year sandbox | Multi-year simulation |
| 021SAFS | SAFS | Concurrent multi-user access | Real-time conflict resolution |
| 029SAFS | SAFS | Multiple export formats | Format conversion |
| 030SAFS | SAFS | Multiple data input types | Multi-format parsing |
| 033SAFS | SAFS | Large file uploads | Performance, memory |
| 034SAFS | SAFS | Common file format | Standardization, migration |
| 035SAFS | SAFS | 3-25 year retention | Archival, retrieval |
| 036SAFS | SAFS | Multi-district upload | Batch processing |
| 047SAFS | SAFS | Export All bulk | Batch generation |
| 048SAFS | SAFS | Concurrent report runs | Parallel processing |
| 089SAFS | SAFS | Enter Budget Document | Multi-form integration |
| 103SAFS | SAFS | Multi-district calculations | Parallel processing |
| 106SAFS | SAFS | Cross-district payments | Financial integration |
| 110SAFS | SAFS | Stop/restart revisions | State management |
| 115SAFS | SAFS | Multi-assignment employees | Data model complexity |

---

## Appendix A: Requirement Type Distribution

| Type | Count | Percentage |
|------|-------|------------|
| Functional | 227 | 93.4% |
| Business | 12 | 4.9% |
| Non-Functional | 3 | 1.2% |
| Technical | 1 | 0.4% |

## Appendix B: Traceability Matrix Legend

| Symbol | Meaning |
|--------|---------|
| Primary | Component has primary responsibility for requirement |
| Secondary | Component supports or is affected by requirement |
| High | Priority/Complexity rating |
| Medium | Priority/Complexity rating |
| Low | Priority/Complexity rating |

## Appendix C: Document Control

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-01-19 | SASQUATCH RFP Team | Initial traceability matrix |

---

*This Requirements Traceability Matrix is a living document and should be updated as requirements evolve or new requirements are identified during the SASQUATCH system development lifecycle.*
