# SASQUATCH Modular Architecture Specification

**SASQUATCH** - School Apportionment System for Quality, Accountability, Transparency, and Calculations Hub

**Version:** 1.0
**Date:** January 19, 2026
**Status:** Draft for RFP Response
**Classification:** Public

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Architecture Overview](#2-architecture-overview)
3. [Module Descriptions](#3-module-descriptions)
4. [Data Flow Diagrams](#4-data-flow-diagrams)
5. [Integration Points](#5-integration-points)
6. [Technology Recommendations](#6-technology-recommendations)
7. [Scalability Considerations](#7-scalability-considerations)
8. [Security Architecture](#8-security-architecture)
9. [Deployment Model](#9-deployment-model)
10. [Appendices](#10-appendices)

---

## 1. Executive Summary

### 1.1 Purpose

This document defines the modular architecture for SASQUATCH, the next-generation school apportionment system that will replace the legacy School Apportionment Financial System (SAFS). SASQUATCH will modernize how Washington State's Office of Superintendent of Public Instruction (OSPI) collects, calculates, and reports education funding data for 295 school districts and 9 Educational Service Districts (ESDs).

### 1.2 System Scope

SASQUATCH manages the distribution of approximately **$27.3 billion annually** in state and federal education funding to 380 educational entities. The system encompasses:

| Dimension | Scope |
|-----------|-------|
| **Annual Funding Volume** | $27.3+ billion |
| **School Districts** | 295 |
| **Educational Service Districts** | 9 |
| **Input Forms** | 11 (F-195, F-196, F-197, F-200, F-203, P-223, S-275, etc.) |
| **Internal System Integrations** | 18 OSPI systems |
| **External Integrations** | SAO, Budget Office, AFRS |
| **Core Users** | ~20 OSPI staff + district/ESD users statewide |

### 1.3 Key Objectives

| Objective | Description |
|-----------|-------------|
| **Agility** | Respond to annual legislative updates without code changes |
| **Integrated Architecture** | Unified platform for collection, calculation, and reporting |
| **Automation** | Eliminate manual data handling, crosswalks, and report compilation |
| **Self-Service** | Enable OSPI staff to update formulas, rules, and metadata |
| **Performance** | Reduce calculation time from 4-6 hours to under 1 hour |
| **Transparency** | Plain-English formula display and public reporting |

### 1.4 Budget and Timeline

- **Total Budget:** $9,000,000 (including 3 years post-implementation support)
- **Implementation Period:** July 1, 2026 - June 30, 2028 (24 months)
- **Optional Support Extension:** Through June 30, 2031

---

## 2. Architecture Overview

### 2.1 High-Level Architecture Diagram

```
+==============================================================================+
|                           SASQUATCH SYSTEM ARCHITECTURE                       |
+==============================================================================+

                              EXTERNAL USERS
    +----------+    +----------+    +----------+    +----------+
    | School   |    |   ESD    |    |  Public  |    |Legislature|
    | Districts|    |  Staff   |    | Website  |    |  Staff   |
    +----+-----+    +----+-----+    +----+-----+    +----+-----+
         |              |              |              |
         +------+-------+------+-------+------+-------+
                |              |              |
                v              v              v
+===============+==============+==============+===============+
|                    PRESENTATION LAYER                        |
|  +------------------+  +------------------+  +-------------+ |
|  |   District/ESD   |  |   OSPI Staff     |  |   Public    | |
|  |   Portal (Web)   |  |   Dashboard      |  |   Reports   | |
|  +------------------+  +------------------+  +-------------+ |
+===============+==============+==============+===============+
                |              |              |
                v              v              v
+===============+==============+==============+===============+
|                      API GATEWAY LAYER                       |
|  +----------------------------------------------------------+|
|  |  RESTful APIs  |  GraphQL  |  File Upload  |  WebSocket  ||
|  +----------------------------------------------------------+|
|  |           Authentication & Authorization (OAuth 2.0)     ||
|  +----------------------------------------------------------+|
+==============================================================+
                |              |              |
                v              v              v
+==============+===============+===============+===============+
|                                                              |
|   +==================+  +==================+  +============+ |
|   |    SECTION 1     |  |    SECTION 2     |  | SECTION 3  | |
|   |                  |  |                  |  |            | |
|   |  DATA COLLECTION |  | DATA CALCULATION |  |   DATA     | |
|   |   AND REVIEW     |  | AND ESTIMATION   |  | REPORTING  | |
|   |                  |  |                  |  |            | |
|   |  +-----------+   |  |  +-----------+   |  | +--------+ | |
|   |  |Form Engine|   |  |  |Calculation|   |  | |Report  | | |
|   |  |Validation |   |  |  |  Engine   |   |  | |Builder | | |
|   |  |Workflow   |   |  |  | Sandbox   |   |  | |Export  | | |
|   |  |Calendar   |   |  |  | Factors   |   |  | |Publish | | |
|   |  +-----------+   |  |  +-----------+   |  | +--------+ | |
|   |                  |  |                  |  |            | |
|   +==================+  +==================+  +============+ |
|              |                  |                  |         |
|   APPLICATION SERVICES LAYER                                 |
+==============+===============+===============+===============+
               |               |               |
               v               v               v
+==============+===============+===============+===============+
|                     DATA ACCESS LAYER                        |
|  +----------------------------------------------------------+|
|  |    Entity Framework Core    |    Dapper (Performance)   ||
|  +----------------------------------------------------------+|
|  |              Repository Pattern | Unit of Work           ||
|  +----------------------------------------------------------+|
+==============================================================+
               |
               v
+==============+===============================================+
|                      DATA LAYER                              |
|  +------------------+  +------------------+  +--------------+|
|  |  Operational DB  |  |  Historical DB   |  | Audit/Log DB||
|  |   (SQL Server)   |  | (Data Warehouse) |  | (Azure Log) ||
|  +------------------+  +------------------+  +--------------+|
|  +----------------------------------------------------------+|
|  |              Azure Blob Storage (Documents)              ||
|  +----------------------------------------------------------+|
+==============================================================+
               |
               v
+==============+===============================================+
|                   INTEGRATION LAYER                          |
|  +----------------------------------------------------------+|
|  | CEDARS | iGrants | STARS | CNS | EMS | AFRS | SAO | ...  ||
|  +----------------------------------------------------------+|
|  |        Azure Service Bus  |  Azure Logic Apps            ||
|  +----------------------------------------------------------+|
+==============================================================+
```

### 2.2 Architecture Principles

| Principle | Implementation |
|-----------|----------------|
| **Modularity** | Three independent work sections with defined interfaces |
| **Loose Coupling** | Event-driven communication via message bus |
| **High Cohesion** | Each module owns its domain logic completely |
| **Separation of Concerns** | Clear boundaries between UI, business logic, and data |
| **Configuration over Code** | Formula and rule changes via admin interface |
| **Audit Everything** | Complete audit trail for all data changes |
| **Fail Gracefully** | Partial processing, retry logic, graceful degradation |

### 2.3 Module Interaction Pattern

```
+-------------------+       Events/Messages       +-------------------+
|                   |  ========================>  |                   |
|   DATA COLLECTION |                             | DATA CALCULATION  |
|                   |  <========================  |                   |
+-------------------+    Validation Results       +-------------------+
         |                                                 |
         |              +-------------------+              |
         |              |                   |              |
         +=============>|   DATA REPORTING  |<=============+
                        |                   |
                        +-------------------+
                               |
                               v
                    +-------------------+
                    |  Public Website   |
                    |  AFRS/Budget      |
                    |  SAO Auditors     |
                    +-------------------+
```

---

## 3. Module Descriptions

### 3.1 Section 1: Data Collection and Review Module

**Purpose:** Provide a unified platform for school districts and ESDs to submit financial, enrollment, and personnel data to OSPI through 11 standardized forms.

#### 3.1.1 Component Architecture

```
+=========================================================================+
|                    DATA COLLECTION AND REVIEW MODULE                     |
+=========================================================================+
|                                                                          |
|  +---------------------------+    +---------------------------+          |
|  |     FORM ENGINE          |    |    WORKFLOW ENGINE        |          |
|  |  +---------+---------+   |    |  +---------+---------+    |          |
|  |  | F-195   | F-196   |   |    |  | State   | State   |    |          |
|  |  | Budget  | Annual  |   |    |  | Machine | History |    |          |
|  |  +---------+---------+   |    |  +---------+---------+    |          |
|  |  | F-197   | F-200   |   |    |  | Approval| Routing |    |          |
|  |  | Monthly | Amend   |   |    |  | Rules   | Logic   |    |          |
|  |  +---------+---------+   |    |  +---------+---------+    |          |
|  |  | F-203   | P-223   |   |    |  | Notifi- | Escala- |    |          |
|  |  | Revenue | Enroll  |   |    |  | cations | tion    |    |          |
|  |  +---------+---------+   |    |  +---------+---------+    |          |
|  |  | S-275   | Others  |   |    +---------------------------+          |
|  |  | Staff   |         |   |                                           |
|  |  +---------+---------+   |    +---------------------------+          |
|  +---------------------------+    |    VALIDATION ENGINE      |          |
|                                   |  +----------------------+ |          |
|  +---------------------------+    |  | Rule Repository     | |          |
|  |    CALENDAR MANAGER       |    |  | Cross-Form Checks   | |          |
|  |  +----------------------+ |    |  | Historical Compare  | |          |
|  |  | Collection Periods   | |    |  | Error Highlighting  | |          |
|  |  | Lock/Unlock Windows  | |    |  | Comment Enforcement | |          |
|  |  | Extension Requests   | |    |  +----------------------+ |          |
|  |  | Reminders/Alerts     | |    +---------------------------+          |
|  |  +----------------------+ |                                           |
|  +---------------------------+    +---------------------------+          |
|                                   |    SUBMISSION TRACKER     |          |
|  +---------------------------+    |  +----------------------+ |          |
|  |    FILE PROCESSOR         |    |  | District Status     | |          |
|  |  +----------------------+ |    |  | Compliance Dashboard| |          |
|  |  | CSV/Excel Import     | |    |  | Late Reporter Mgmt  | |          |
|  |  | API Ingestion        | |    |  | Version History     | |          |
|  |  | Format Validation    | |    |  +----------------------+ |          |
|  |  | Data Transformation  | |    +---------------------------+          |
|  |  +----------------------+ |                                           |
|  +---------------------------+                                           |
|                                                                          |
+=========================================================================+
```

#### 3.1.2 Form Specifications

| Form ID | Name | Frequency | Data Type | Approx. Records/Year |
|---------|------|-----------|-----------|---------------------|
| F-195 | Budget Summary | Annual | Financial | 295 districts |
| F-196 | Annual Financial Statement | Annual | Financial | 295 districts |
| F-197 | Monthly Financial Report | Monthly | Financial | 3,540 submissions |
| F-200 | Budget Amendments | As Needed | Financial | ~500 amendments |
| F-203 | Revenue Forecasting | Monthly | Financial | 3,540 submissions |
| P-223 | Enrollment Reporting | Monthly | Enrollment | 3,540 submissions |
| S-275 | Personnel Reporting | Annual | Personnel | 295 districts |
| E-672 | Institutional Education | Monthly | Enrollment | 12 reports |
| E-525 | Home/Hospital | Monthly | Enrollment | 12 reports |
| P-213 | Non-High | Annual | Enrollment | 100+ reports |
| ALE | Alternative Learning | Various | Enrollment | Varies |

#### 3.1.3 Workflow States

```
+----------+     +------------+     +-------------+     +----------+
|  DRAFT   | --> | SUBMITTED  | --> | ESD REVIEW  | --> |   OSPI   |
|          |     |            |     |             |     |  REVIEW  |
+----------+     +------------+     +-------------+     +----------+
     ^                 |                   |                  |
     |                 v                   v                  v
     |           +------------+     +-------------+     +----------+
     +-----------| RETURNED   |     |  REJECTED   |     | APPROVED |
                 | FOR EDITS  |     |             |     |          |
                 +------------+     +-------------+     +----------+
                                                              |
                                                              v
                                                        +----------+
                                                        |  LOCKED  |
                                                        | (Final)  |
                                                        +----------+
```

#### 3.1.4 Key Capabilities

| Capability | Description |
|------------|-------------|
| **Multi-Channel Submission** | Web portal, API, CSV upload, legacy file formats |
| **Real-Time Validation** | Instant feedback with error highlighting and severity levels |
| **Cross-Form Validation** | Detect inconsistencies across related submissions |
| **Comment Enforcement** | Block submission until meaningful explanations provided for anomalies |
| **Version Control** | Full history of all submissions with diff capability |
| **Lock Window Management** | OSPI-controlled periods to prevent changes during processing |
| **Automated Reminders** | Email/dashboard alerts for upcoming deadlines |
| **Extension Management** | Formal request/approval workflow for deadline extensions |

---

### 3.2 Section 2: Data Calculations and Estimations Module

**Purpose:** Execute the complex apportionment formulas that determine funding distribution, provide scenario modeling capabilities, and enable self-service formula management.

#### 3.2.1 Component Architecture

```
+=========================================================================+
|               DATA CALCULATIONS AND ESTIMATIONS MODULE                   |
+=========================================================================+
|                                                                          |
|  +---------------------------+    +---------------------------+          |
|  |   CALCULATION ENGINE     |    |    FORMULA MANAGER         |         |
|  |  +----------------------+|    |  +----------------------+  |          |
|  |  | Apportionment Core  ||    |  | Plain-English Editor |  |          |
|  |  | - Basic Education   ||    |  | Version Control      |  |          |
|  |  | - Special Education ||    |  | Effective Date Mgmt  |  |          |
|  |  | - Transportation    ||    |  | Approval Workflow    |  |          |
|  |  | - MSOC Factors      ||    |  | Audit Trail          |  |          |
|  |  +----------------------+|    |  +----------------------+  |          |
|  |  | LAP Calculations    ||    +---------------------------+          |
|  |  | High Poverty        ||                                            |
|  |  | K-3 Class Size      ||    +---------------------------+          |
|  |  | PSES Compliance     ||    |    FACTOR REPOSITORY      |          |
|  |  +----------------------+|    |  +----------------------+  |          |
|  +---------------------------+    |  | Legislative Factors |  |          |
|                                   |  | Inflation Rates     |  |          |
|  +---------------------------+    |  | Cost Allocations    |  |          |
|  |   EXECUTION MANAGER       |    |  | Regional Adjustments|  |          |
|  |  +----------------------+ |    |  | Historical Archive  |  |          |
|  |  | Batch Processing     | |    |  +----------------------+  |          |
|  |  | Parallel Execution   | |    +---------------------------+          |
|  |  | Subset Selection     | |                                           |
|  |  | Progress Monitoring  | |    +---------------------------+          |
|  |  | Error Recovery       | |    |    CODE CROSSWALK         |          |
|  |  +----------------------+ |    |  +----------------------+  |          |
|  +---------------------------+    |  | Revenue -> Budget    |  |          |
|                                   |  | Program Codes        |  |          |
|  +---------------------------+    |  | Object Codes         |  |          |
|  |   SANDBOX ENVIRONMENT     |    |  | Automated Mapping    |  |          |
|  |  +----------------------+ |    |  +----------------------+  |          |
|  |  | Scenario Modeling    | |    +---------------------------+          |
|  |  | What-If Analysis     | |                                           |
|  |  | Side-by-Side Compare | |    +---------------------------+          |
|  |  | Historical Data Use  | |    |    PROJECTION ENGINE      |          |
|  |  | Formula Prototyping  | |    |  +----------------------+  |          |
|  |  | Legislature Support  | |    |  | 4-Year Forecasting  |  |          |
|  |  +----------------------+ |    |  | District Estimator  |  |          |
|  +---------------------------+    |  | Factor Adjustment   |  |          |
|                                   |  +----------------------+  |          |
|                                   +---------------------------+          |
|                                                                          |
+=========================================================================+
```

#### 3.2.2 Calculation Categories

| Category | Description | Complexity |
|----------|-------------|------------|
| **Basic Education** | Core per-pupil funding allocation | High |
| **Special Education** | Weighted funding for IEP students | High |
| **Transportation** | Distance and ridership-based funding | Medium |
| **MSOC** | Materials, Supplies, Operating Costs factors | Medium |
| **LAP** | Learning Assistance Program allocations | High |
| **High Poverty** | Additional funding for high-poverty schools | Medium |
| **K-3 Class Size** | Class size reduction compliance funding | Medium |
| **PSES** | Physical, Social, Emotional Support funding | Medium |
| **Carryover Recovery** | Year-end fund reconciliation and clawback | High |
| **Levy Equalization** | Local effort assistance calculations | High |

#### 3.2.3 Performance Requirements

| Metric | Current State | Target State |
|--------|--------------|--------------|
| Full Calculation Run | 4-6 hours | < 1 hour |
| Subset Calculation (10 districts) | N/A (not supported) | < 5 minutes |
| User Lock-out During Processing | Required | Eliminated |
| Concurrent Calculation Runs | 1 | Multiple (parallel) |

#### 3.2.4 Sandbox Capabilities

```
+=========================================================================+
|                       SANDBOX ARCHITECTURE                               |
+=========================================================================+
|                                                                          |
|   PRODUCTION DATA          SANDBOX INSTANCE(S)                           |
|   +-------------+          +-------------------+                         |
|   | Current     |  ------> | Sandbox #1        |                         |
|   | Year Data   |  Copy    | (Legislature A)   |                         |
|   +-------------+          +-------------------+                         |
|         |                          |                                     |
|         |                  +-------------------+                         |
|         +----------------> | Sandbox #2        |                         |
|                    Copy    | (District Est.)   |                         |
|                            +-------------------+                         |
|                                    |                                     |
|                            +-------------------+                         |
|                            | Sandbox #3        |                         |
|                            | (What-If)         |                         |
|                            +-------------------+                         |
|                                                                          |
|   COMPARISON ENGINE                                                      |
|   +---------------------------------------------------------------+     |
|   | +----------+  +----------+  +----------+  +----------+        |     |
|   | | Scenario | vs| Scenario | vs| Scenario | vs|Production|      |     |
|   | |    A     |  |    B     |  |    C     |  |  Actual  |        |     |
|   | +----------+  +----------+  +----------+  +----------+        |     |
|   +---------------------------------------------------------------+     |
|                              |                                           |
|                              v                                           |
|   +---------------------------------------------------------------+     |
|   |              SIDE-BY-SIDE COMPARISON REPORT                    |     |
|   |   District | Prod $ | Scenario A | Scenario B | Variance      |     |
|   |   ---------|--------|------------|------------|------------   |     |
|   |   Seattle  | $XXX   |   $YYY     |   $ZZZ     |   +/-$        |     |
|   +---------------------------------------------------------------+     |
|                                                                          |
+=========================================================================+
```

---

### 3.3 Section 3: Data Reporting Module

**Purpose:** Generate, distribute, and publish standardized and custom reports for districts, ESDs, the legislature, auditors, and the public.

#### 3.3.1 Component Architecture

```
+=========================================================================+
|                      DATA REPORTING MODULE                               |
+=========================================================================+
|                                                                          |
|  +---------------------------+    +---------------------------+          |
|  |    REPORT GENERATOR      |    |    REPORT CATALOG         |          |
|  |  +----------------------+|    |  +----------------------+  |          |
|  |  | Template Engine     ||    |  | District Reports     |  |          |
|  |  | - Crystal Reports   ||    |  | ESD Reports          |  |          |
|  |  | - SSRS              ||    |  | State-Level Reports  |  |          |
|  |  | - Custom Templates  ||    |  | Legislative Reports  |  |          |
|  |  +----------------------+|    |  | Audit Reports        |  |          |
|  |  | Data Binding        ||    |  +----------------------+  |          |
|  |  | Aggregation Engine  ||    +---------------------------+          |
|  |  | Multi-Format Export ||                                            |
|  |  +----------------------+|    +---------------------------+          |
|  +---------------------------+    |    CUSTOM REPORT BUILDER  |          |
|                                   |  +----------------------+  |          |
|  +---------------------------+    |  | Drag-Drop Interface  |  |          |
|  |    OUTPUT PROCESSOR       |    |  | Field Selection      |  |          |
|  |  +----------------------+ |    |  | Filter Builder       |  |          |
|  |  | PDF Generation       | |    |  | Grouping/Sorting     |  |          |
|  |  | - ADA Compliant      | |    |  | Save/Share Templates |  |          |
|  |  | - Auto-Tagged        | |    |  +----------------------+  |          |
|  |  +----------------------+ |    +---------------------------+          |
|  |  | Excel Export         | |                                           |
|  |  | - Formatted Sheets   | |    +---------------------------+          |
|  |  | - Pivot-Ready Data   | |    |    DISTRIBUTION ENGINE    |          |
|  |  +----------------------+ |    |  +----------------------+  |          |
|  |  | CSV/Data Feeds       | |    |  | Scheduled Delivery   |  |          |
|  |  | API Access           | |    |  | Email Distribution   |  |          |
|  |  +----------------------+ |    |  | Portal Publish       |  |          |
|  +---------------------------+    |  | Public Website Push  |  |          |
|                                   |  +----------------------+  |          |
|  +---------------------------+    +---------------------------+          |
|  |    CONSOLIDATION ENGINE   |                                           |
|  |  +----------------------+ |    +---------------------------+          |
|  |  | Multi-District Merge | |    |    VERSION MANAGER        |          |
|  |  | Statewide Rollups    | |    |  +----------------------+  |          |
|  |  | Batch Compilation    | |    |  | Report Versioning    |  |          |
|  |  | Package Builder      | |    |  | Change Notifications |  |          |
|  |  +----------------------+ |    |  | Historical Access    |  |          |
|  +---------------------------+    |  +----------------------+  |          |
|                                   +---------------------------+          |
|                                                                          |
+=========================================================================+
```

#### 3.3.2 Report Categories

| Category | Audience | Format | Frequency |
|----------|----------|--------|-----------|
| **District Summary** | District Finance Staff | PDF, Excel | Monthly |
| **ESD Rollup** | ESD Administrators | PDF, Excel | Monthly |
| **State Aggregate** | Legislature, OSPI Leadership | PDF | Monthly/Annual |
| **Apportionment Detail** | Budget Office, AFRS | Excel, CSV | Monthly |
| **Personnel Summary** | Districts, Unions, SAO | PDF, Excel | Annual |
| **Enrollment Trends** | Public, Researchers | PDF, Web | Annual |
| **Audit Package** | SAO Auditors | PDF Package | On-Demand |
| **Legislative Projections** | Legislature Fiscal Staff | PDF, Excel | As Requested |

#### 3.3.3 ADA Compliance Architecture

```
+=========================================================================+
|                    ADA COMPLIANCE PIPELINE                               |
+=========================================================================+
|                                                                          |
|   RAW REPORT DATA                                                        |
|        |                                                                 |
|        v                                                                 |
|   +------------------+                                                   |
|   | PDF Generation   |                                                   |
|   +------------------+                                                   |
|        |                                                                 |
|        v                                                                 |
|   +------------------+    +------------------+                           |
|   | Auto-Tagging     | -->| Structure Tags   |                           |
|   | Engine           |    | - Headings       |                           |
|   +------------------+    | - Tables         |                           |
|        |                  | - Lists          |                           |
|        |                  | - Reading Order  |                           |
|        v                  +------------------+                           |
|   +------------------+                                                   |
|   | Alt-Text         |    +------------------+                           |
|   | Generator        | -->| Image Descriptions|                          |
|   +------------------+    | Chart Summaries  |                           |
|        |                  +------------------+                           |
|        v                                                                 |
|   +------------------+                                                   |
|   | WCAG 2.0 AA      |    +------------------+                           |
|   | Validator        | -->| Compliance Check |                           |
|   +------------------+    | Pass/Fail Report |                           |
|        |                  +------------------+                           |
|        v                                                                 |
|   +------------------+                                                   |
|   | Final PDF        |                                                   |
|   | (Accessible)     |                                                   |
|   +------------------+                                                   |
|                                                                          |
+=========================================================================+
```

#### 3.3.4 Public Website Integration

```
+------------------+       +------------------+       +------------------+
|  SASQUATCH       |       |   CDN / Cache    |       |  OSPI Public     |
|  Report Server   | ----> |   (Azure CDN)    | ----> |  Website         |
+------------------+       +------------------+       +------------------+
        |                                                     |
        v                                                     v
+------------------+                               +------------------+
| Static Reports   |                               | Interactive      |
| (PDF, Excel)     |                               | Data Explorer    |
+------------------+                               +------------------+
        |                                                     |
        v                                                     v
+------------------+                               +------------------+
| Scheduled Push   |                               | Drill-Down       |
| (Monthly/Annual) |                               | Filters          |
+------------------+                               +------------------+
```

---

## 4. Data Flow Diagrams

### 4.1 End-to-End Data Flow

```
+==============================================================================+
|                    SASQUATCH END-TO-END DATA FLOW                            |
+==============================================================================+

 EXTERNAL SOURCES                    SASQUATCH CORE                    OUTPUTS
 ================                    ==============                    =======

+-------------+                                                    +-------------+
| School      |     +---------------+     +---------------+        | District    |
| Districts   | --> |               |     |               | -----> | Reports     |
| (295)       |     |    DATA       |     |    DATA       |        +-------------+
+-------------+     |  COLLECTION   |     |  CALCULATION  |
                    |    MODULE     |     |    MODULE     |        +-------------+
+-------------+     |               |     |               | -----> | ESD         |
| ESDs        | --> | +-----------+ |     | +-----------+ |        | Reports     |
| (9)         |     | | Validate  | |     | | Calculate | |        +-------------+
+-------------+     | | Transform | | --> | | Allocate  | |
                    | | Store     | |     | | Project   | |        +-------------+
+-------------+     | +-----------+ |     | +-----------+ | -----> | Legislative |
| CEDARS      | --> |               |     |               |        | Reports     |
| (Enrollment)|     +---------------+     +---------------+        +-------------+
+-------------+            |                     |
                          |                     |                  +-------------+
+-------------+            |                     |                  | Public      |
| iGrants     | -----------+                     +----------------> | Website     |
| (Grants)    |            |                     |                  +-------------+
+-------------+            v                     |
                    +---------------+            |                  +-------------+
+-------------+     |    SHARED     |            |                  | AFRS        |
| STARS       | --> |    DATA       | <----------+----------------> | (Payments)  |
| (Transport) |     |   SERVICES    |            |                  +-------------+
+-------------+     | +-----------+ |            |
                    | | Audit Log | |            |                  +-------------+
+-------------+     | | Ref Data  | |            |                  | SAO         |
| CNS         | --> | | Security  | |            +----------------> | (Auditors)  |
| (Nutrition) |     | +-----------+ |                               +-------------+
+-------------+     +---------------+
                          |
+-------------+            |          +---------------+             +-------------+
| Budget      | -----------+          |    DATA       |             | Budget      |
| Office      |            +--------> |  REPORTING    | ----------> | Office      |
+-------------+                       |    MODULE     |             +-------------+
                                      | +-----------+ |
                                      | | Generate  | |
                                      | | Export    | |
                                      | | Publish   | |
                                      | +-----------+ |
                                      +---------------+
```

### 4.2 Monthly Processing Cycle

```
+=============================================================================+
|                      MONTHLY APPORTIONMENT CYCLE                             |
+=============================================================================+

   DAY 1-5                    DAY 6-15                   DAY 16-20
   Collection Window          Processing Window          Distribution
   ==================         =================          ============

+----------------+        +------------------+       +------------------+
|  Districts     |        |   OSPI Staff     |       |    OUTPUTS       |
|  Submit Data   |        |   Review/Approve |       |                  |
|                |        |                  |       |                  |
| +------------+ |        | +------------+   |       | +------------+   |
| | F-203      | | -----> | | Validate   |   |       | | Reports    |   |
| | P-223      | |        | | Cross-Check|   |       | | Generated  |   |
| | etc.       | |        | +------------+   |       | +------------+   |
| +------------+ |        |       |          |       |       |          |
+----------------+        |       v          |       |       v          |
       |                  | +------------+   |       | +------------+   |
       |                  | | Lock       |   |       | | Distribute |   |
       v                  | | Submissions|   | ----> | | to AFRS    |   |
+----------------+        | +------------+   |       | +------------+   |
| ESD Review     |        |       |          |       |       |          |
| (Optional)     |        |       v          |       |       v          |
+----------------+        | +------------+   |       | +------------+   |
       |                  | | Run        |   |       | | Post to    |   |
       v                  | | Calculations|  |       | | Website    |   |
+----------------+        | +------------+   |       | +------------+   |
| Auto-Reminders |        |       |          |       |                  |
| for Late       |        |       v          |       +------------------+
| Submitters     |        | +------------+   |              |
+----------------+        | | Generate   |   |              v
                          | | Reports    |   |       +------------------+
                          | +------------+   |       | District         |
                          |       |          |       | Payments         |
                          |       v          |       | Processed        |
                          | +------------+   |       +------------------+
                          | | OSPI Final |   |
                          | | Approval   |   |
                          | +------------+   |
                          +------------------+
```

### 4.3 Sandbox Scenario Flow

```
+=============================================================================+
|                     LEGISLATIVE SCENARIO MODELING                            |
+=============================================================================+

   REQUEST                    SANDBOX                      DELIVERABLE
   =======                    =======                      ===========

+----------------+        +------------------+       +------------------+
| Legislature    |        |  SANDBOX ENV     |       |   COMPARISON     |
| Submits        |        |                  |       |   REPORT         |
| Scenario       |        | +------------+   |       |                  |
| Request        |        | | Clone Prod |   |       | +------------+   |
|                |        | | Data       |   |       | | Scenario A |   |
| "What if we    | -----> | +------------+   |       | | vs         |   |
|  change MSOC   |        |       |          |       | | Scenario B |   |
|  factor to X?" |        |       v          |       | | vs         |   |
|                |        | +------------+   |       | | Production |   |
+----------------+        | | Apply New  |   |       | +------------+   |
                          | | Formula    |   |       |       |          |
                          | +------------+   |       |       v          |
                          |       |          |       | +------------+   |
                          |       v          |       | | District-  |   |
                          | +------------+   | ----> | | by-District|   |
                          | | Run        |   |       | | Impact     |   |
                          | | Calculation|   |       | +------------+   |
                          | +------------+   |       |       |          |
                          |       |          |       |       v          |
                          |       v          |       | +------------+   |
                          | +------------+   |       | | Winner/    |   |
                          | | Compare    |   |       | | Loser      |   |
                          | | Results    |   |       | | Analysis   |   |
                          | +------------+   |       | +------------+   |
                          +------------------+       +------------------+
```

---

## 5. Integration Points

### 5.1 Integration Architecture

```
+=============================================================================+
|                      INTEGRATION ARCHITECTURE                                |
+=============================================================================+

                          +------------------+
                          |   SASQUATCH      |
                          |   INTEGRATION    |
                          |   HUB            |
                          +--------+---------+
                                   |
        +----------+---------------+---------------+----------+
        |          |               |               |          |
        v          v               v               v          v
+-------+--+ +-----+----+ +-------+------+ +------+---+ +-----+----+
|  INBOUND | | OUTBOUND | | BI-DIRECTIONAL| | MESSAGE  | |  FILE    |
|  APIs    | |  APIs    | |  SYNC         | |  QUEUE   | | TRANSFER |
+----------+ +----------+ +--------------+ +----------+ +----------+
     |            |              |              |             |
     v            v              v              v             v
+---------+ +---------+  +------------+  +---------+  +-----------+
| CEDARS  | |  AFRS   |  |  iGrants   |  | Event   |  | SAO       |
| EMS     | | Budget  |  |  Grants    |  | Notif.  |  | SFTP      |
| CNS     | | Office  |  |  Claims    |  | System  |  | Uploads   |
+---------+ +---------+  +------------+  +---------+  +-----------+
```

### 5.2 Internal OSPI System Integrations

| System | Integration Type | Data Flow | Frequency |
|--------|-----------------|-----------|-----------|
| **CEDARS** | API (REST) | Inbound | Daily |
| **EMS (Enrollment)** | Database Sync | Bidirectional | Monthly |
| **iGrants** | API (REST) | Inbound | Monthly |
| **Grants Claims** | API (REST) | Inbound | Monthly |
| **STARS (Transportation)** | File Import | Inbound | Annual |
| **CNS (Child Nutrition)** | API (REST) | Inbound | Monthly |
| **eCertification** | API (REST) | Inbound | Continuous |
| **Report Card** | Database Sync | Outbound | Annual |
| **Open Doors** | File Import | Inbound | Annual |
| **Skill Center** | File Import | Inbound | Annual |
| **Voc Ed** | File Import | Inbound | Annual |
| **SPED** | API (REST) | Inbound | Monthly |
| **Highly Capable** | File Import | Inbound | Annual |
| **Migrant** | File Import | Inbound | Annual |
| **LAP** | Database Sync | Bidirectional | Monthly |
| **Bilingual** | File Import | Inbound | Annual |
| **LEP** | File Import | Inbound | Annual |
| **F-780** | File Import | Inbound | Annual |

### 5.3 External System Integrations

| System | Organization | Integration Type | Data Flow | Security |
|--------|-------------|-----------------|-----------|----------|
| **AFRS** | WA State | File/API | Outbound | Encrypted |
| **Budget Office** | WA State | File | Outbound | SFTP |
| **SAO** | State Auditor | SFTP | Outbound | Encrypted |
| **Tribal Compacts** | Tribal Nations | Web Portal | Inbound | OAuth |

### 5.4 Integration Patterns

```
+=============================================================================+
|                      INTEGRATION PATTERNS                                    |
+=============================================================================+

   PATTERN 1: API INTEGRATION                  PATTERN 2: FILE-BASED
   ==========================                  ======================

+----------+     +----------+     +--------+  +--------+     +--------+
| External |     |   API    |     | SAFS   |  | Source |     | Azure  |
|  System  | --> | Gateway  | --> |  DB    |  | System | --> | Blob   |
+----------+     +----------+     +--------+  +--------+     +--------+
                      |                             |              |
                      v                             v              v
                 +----------+                  +--------+     +--------+
                 | Rate     |                  | Drop   |     | Import |
                 | Limiting |                  | File   |     | Service|
                 | Auth     |                  +--------+     +--------+
                 +----------+                                      |
                                                                   v
                                                              +--------+
                                                              | SAFS   |
                                                              |  DB    |
                                                              +--------+

   PATTERN 3: EVENT-DRIVEN                     PATTERN 4: DATABASE SYNC
   =======================                     ========================

+----------+     +----------+     +--------+  +--------+     +--------+
| Publisher|     | Service  |     | Sub-   |  | Source |     | Change |
|          | --> |   Bus    | --> | scriber|  |   DB   | --> | Data   |
+----------+     +----------+     +--------+  +--------+     | Capture|
                      |                                      +--------+
                      v                                           |
                 +----------+                                     v
                 | Dead     |                               +--------+
                 | Letter   |                               | Sync   |
                 | Queue    |                               | Service|
                 +----------+                               +--------+
                                                                  |
                                                                  v
                                                             +--------+
                                                             | Target |
                                                             |   DB   |
                                                             +--------+
```

---

## 6. Technology Recommendations

### 6.1 Technology Stack Overview

```
+=============================================================================+
|                       RECOMMENDED TECHNOLOGY STACK                           |
+=============================================================================+

   PRESENTATION TIER              APPLICATION TIER              DATA TIER
   ================              ================              =========

+------------------+         +--------------------+        +----------------+
|   WEB FRONTEND   |         |   APPLICATION      |        |   DATABASES    |
|                  |         |   SERVICES         |        |                |
| +------------+   |         | +--------------+   |        | +----------+   |
| | React/     |   |         | | ASP.NET Core |   |        | | SQL      |   |
| | TypeScript |   |         | | 8.0 LTS      |   |        | | Server   |   |
| +------------+   |         | +--------------+   |        | | 2022     |   |
|       |          |         |        |           |        | +----------+   |
|       v          |         |        v           |        |      |         |
| +------------+   |         | +--------------+   |        |      v         |
| | Tailwind   |   |   <-->  | | REST APIs    |   |  <-->  | +----------+   |
| | CSS        |   |         | | GraphQL      |   |        | | Redis    |   |
| +------------+   |         | +--------------+   |        | | Cache    |   |
|       |          |         |        |           |        | +----------+   |
|       v          |         |        v           |        |      |         |
| +------------+   |         | +--------------+   |        |      v         |
| | AG Grid    |   |         | | SignalR      |   |        | +----------+   |
| | (Tables)   |   |         | | (Real-time)  |   |        | | Azure    |   |
| +------------+   |         | +--------------+   |        | | Blob     |   |
+------------------+         +--------------------+        +----------------+
        |                            |                            |
        v                            v                            v
+------------------+         +--------------------+        +----------------+
|   MOBILE/PWA     |         |   BACKGROUND       |        |   ANALYTICS    |
|                  |         |   SERVICES         |        |                |
| +------------+   |         | +--------------+   |        | +----------+   |
| | Progressive|   |         | | Azure        |   |        | | Azure    |   |
| | Web App    |   |         | | Functions    |   |        | | Synapse  |   |
| +------------+   |         | +--------------+   |        | +----------+   |
|                  |         |        |           |        |      |         |
|                  |         |        v           |        |      v         |
|                  |         | +--------------+   |        | +----------+   |
|                  |         | | Hangfire     |   |        | | Power BI |   |
|                  |         | | (Jobs)       |   |        | | Embedded |   |
|                  |         | +--------------+   |        | +----------+   |
+------------------+         +--------------------+        +----------------+
```

### 6.2 Technology Selection Matrix

| Layer | Technology | Rationale |
|-------|------------|-----------|
| **Frontend Framework** | React 18 + TypeScript | Type safety, component reuse, large ecosystem |
| **UI Components** | Shadcn/UI + Tailwind | Accessible, customizable, ADA compliance support |
| **Data Grid** | AG Grid Enterprise | Handles 100K+ rows, Excel-like functionality |
| **Backend Framework** | ASP.NET Core 8.0 LTS | Enterprise-grade, Azure-native, long-term support |
| **API Layer** | REST + GraphQL | REST for simple operations, GraphQL for complex queries |
| **Real-Time** | SignalR | Calculation progress, notifications |
| **Database** | SQL Server 2022 | OSPI standard, Azure SQL compatible |
| **Caching** | Redis (Azure Cache) | Session, query results, distributed cache |
| **Message Queue** | Azure Service Bus | Reliable event-driven integration |
| **File Storage** | Azure Blob Storage | Document storage, report archive |
| **Background Jobs** | Hangfire + Azure Functions | Long-running calculations, scheduled tasks |
| **Reporting** | SSRS + Custom PDF | Existing OSPI standard, ADA-compliant PDF generation |
| **Search** | Azure Cognitive Search | Full-text search across reports and data |
| **Monitoring** | Application Insights | Azure-native APM, logging, alerting |
| **Identity** | Azure AD B2C | District/ESD user authentication, MFA |

### 6.3 Development Standards

| Category | Standard |
|----------|----------|
| **Version Control** | Azure DevOps Git |
| **CI/CD** | Azure Pipelines |
| **Code Quality** | SonarQube, StyleCop |
| **Testing** | xUnit, Playwright, SpecFlow |
| **Documentation** | OpenAPI 3.0, Markdown |
| **Methodology** | Scrum (2-week sprints) |
| **Code Review** | PR-based, 2 approvers minimum |

---

## 7. Scalability Considerations

### 7.1 Scalability Architecture

```
+=============================================================================+
|                     SCALABILITY ARCHITECTURE                                 |
+=============================================================================+

                          +------------------+
                          |  Azure Traffic   |
                          |    Manager       |
                          +--------+---------+
                                   |
                    +-------------+-------------+
                    |                           |
                    v                           v
           +-------+-------+           +-------+-------+
           |  Region 1     |           |  Region 2     |
           |  (Primary)    |           |  (DR/Failover)|
           +-------+-------+           +---------------+
                   |
        +----------+----------+
        |          |          |
        v          v          v
+-------+--+ +-----+----+ +---+------+
|  Web     | |  API     | |  Worker  |
|  Tier    | |  Tier    | |  Tier    |
|  (Scale) | |  (Scale) | |  (Scale) |
+----------+ +----------+ +----------+
     |            |            |
     +-----+------+-----+------+
           |            |
           v            v
    +------+----+ +-----+-----+
    |  SQL      | |   Redis   |
    |  Elastic  | |   Cluster |
    |  Pool     | |           |
    +-----------+ +-----------+
```

### 7.2 Scaling Dimensions

| Dimension | Approach | Trigger |
|-----------|----------|---------|
| **Web Tier** | Horizontal (Auto-scale) | CPU > 70%, Requests > 1000/sec |
| **API Tier** | Horizontal (Auto-scale) | CPU > 70%, Queue depth > 100 |
| **Calculation Workers** | Horizontal (Manual + Auto) | Job queue depth |
| **Database** | Vertical + Read Replicas | DTU > 80%, Connection pool exhaustion |
| **Cache** | Cluster scaling | Memory > 80%, Hit ratio < 90% |

### 7.3 Performance Targets

| Scenario | Target | Measurement |
|----------|--------|-------------|
| **Form Load** | < 2 seconds | P95 latency |
| **Form Submit** | < 5 seconds | P95 latency |
| **Validation Run** | < 10 seconds | Per form |
| **Full Calculation** | < 60 minutes | All 295 districts |
| **Subset Calculation** | < 5 minutes | 10 districts |
| **Report Generation** | < 30 seconds | Single district report |
| **Bulk Report Export** | < 10 minutes | All district package |
| **Dashboard Load** | < 3 seconds | OSPI staff dashboard |
| **Concurrent Users** | 500+ | Peak load (submission deadline) |

### 7.4 Load Characteristics

```
+=============================================================================+
|                     LOAD PATTERN ANALYSIS                                    |
+=============================================================================+

   MONTHLY CYCLE                                ANNUAL CYCLE
   =============                                ============

   Load                                         Load
    ^                                            ^
    |     Peak                                   |        Year-End
    |    (Day 1-5)                               |         Peak
100%|    /\                                  100%|         /\
    |   /  \                                     |        /  \
    |  /    \                                    |       /    \
 50%| /      \____                            50%|      /      \____
    |/            \_____                         |_____/            \_____
    +-----------------------> Day               +--------------------------> Month
       1  5  10  15  20  25  30                    J F M A M J J A S O N D

   PEAK PERIODS:                                 PEAK PERIODS:
   - Day 1-5: Collection opens                   - September: School year start
   - Day 15-20: OSPI processing                  - December-January: Mid-year
   - Month-end: Report distribution              - June-August: Year-end close

   SCALING STRATEGY:                             SCALING STRATEGY:
   - Pre-scale Day 1                             - Reserve capacity Aug-Sep
   - Auto-scale Day 1-5                          - Manual scale June
   - Scale down Day 20+                          - Alert thresholds adjusted
```

---

## 8. Security Architecture

### 8.1 Security Overview

```
+=============================================================================+
|                       SECURITY ARCHITECTURE                                  |
+=============================================================================+

                    +---------------------------+
                    |     SECURITY PERIMETER    |
                    |  +---------------------+  |
                    |  | Azure Front Door    |  |
                    |  | - DDoS Protection   |  |
                    |  | - WAF Rules         |  |
                    |  | - Geo-Filtering     |  |
                    |  +---------------------+  |
                    +------------+-------------+
                                 |
                    +------------v-------------+
                    |     IDENTITY LAYER       |
                    |  +---------------------+  |
                    |  | Azure AD B2C        |  |
                    |  | - MFA Required      |  |
                    |  | - Conditional Access|  |
                    |  | - Role-Based Access |  |
                    |  +---------------------+  |
                    +------------+-------------+
                                 |
        +------------------------+------------------------+
        |                        |                        |
+-------v-------+       +--------v--------+       +-------v-------+
|  APPLICATION  |       |    API LAYER    |       |    DATA       |
|    LAYER      |       |                 |       |    LAYER      |
|  +---------+  |       |  +---------+    |       |  +---------+  |
|  | Input   |  |       |  | API Key |    |       |  | TDE     |  |
|  | Valid.  |  |       |  | Mgmt    |    |       |  | (Rest)  |  |
|  +---------+  |       |  +---------+    |       |  +---------+  |
|  | CSRF    |  |       |  | Rate    |    |       |  | TLS 1.3 |  |
|  | Protect |  |       |  | Limiting|    |       |  | (Motion)|  |
|  +---------+  |       |  +---------+    |       |  +---------+  |
|  | Output  |  |       |  | OAuth   |    |       |  | Column  |  |
|  | Encoding|  |       |  | 2.0     |    |       |  | Level   |  |
|  +---------+  |       |  +---------+    |       |  +---------+  |
+---------------+       +-----------------+       +---------------+
        |                        |                        |
        +------------------------+------------------------+
                                 |
                    +------------v-------------+
                    |     AUDIT & MONITORING   |
                    |  +---------------------+  |
                    |  | Azure Sentinel      |  |
                    |  | - SIEM              |  |
                    |  | - Threat Detection  |  |
                    |  | - Compliance Reports|  |
                    |  +---------------------+  |
                    +-------------------------+
```

### 8.2 Authentication and Authorization

| User Type | Authentication Method | Authorization Model |
|-----------|----------------------|---------------------|
| **OSPI Staff** | Azure AD (SSO) | Role-Based (RBAC) |
| **District Users** | Azure AD B2C | Role + District Scope |
| **ESD Users** | Azure AD B2C | Role + ESD Scope |
| **API Consumers** | OAuth 2.0 + API Key | Scope-based |
| **Public** | Anonymous (read-only) | Public reports only |

### 8.3 Role Definitions

| Role | Permissions | Scope |
|------|-------------|-------|
| **OSPI Administrator** | Full system access | Statewide |
| **OSPI Analyst** | Read all, approve submissions | Statewide |
| **OSPI Finance** | Run calculations, generate reports | Statewide |
| **ESD Administrator** | Review/approve district submissions | ESD region |
| **District Finance Director** | Submit/edit forms, view reports | Single district |
| **District Data Entry** | Submit forms (limited) | Single district |
| **Auditor (SAO)** | Read-only access to reports | Statewide |
| **Public** | View published reports | Public reports only |

### 8.4 Data Classification

| Classification | Examples | Protection |
|----------------|----------|------------|
| **Category 4 (Confidential)** | SSN (masked), personnel data | Encryption + Access Control + Audit |
| **Category 3 (Sensitive)** | Financial data, enrollment details | Encryption + Access Control |
| **Category 2 (Internal)** | Aggregate reports, metadata | Access Control |
| **Category 1 (Public)** | Published reports, summary data | None (public access) |

### 8.5 Compliance Requirements

| Standard | Requirement | Implementation |
|----------|-------------|----------------|
| **WaTech Security** | State IT security policies | Azure Security Center, CIS benchmarks |
| **FERPA** | Student data privacy | Data masking, access controls |
| **WCAG 2.0 AA** | Accessibility | Automated testing, manual review |
| **SOC 2 Type II** | Azure compliance | Inherited from Azure |
| **State Audit** | Financial system controls | Comprehensive audit logging |

### 8.6 Audit Trail Requirements

```
+=============================================================================+
|                         AUDIT TRAIL STRUCTURE                                |
+=============================================================================+

   AUDIT EVENT RECORD
   ==================

   +------------------------------------------------------------------+
   | Field           | Description                     | Example      |
   +-----------------+---------------------------------+--------------+
   | EventId         | Unique identifier               | GUID         |
   | Timestamp       | UTC timestamp                   | ISO 8601     |
   | UserId          | Authenticated user              | user@k12.wa  |
   | UserRole        | Role at time of action          | DistrictFin  |
   | Action          | CRUD operation                  | UPDATE       |
   | EntityType      | What was affected               | F196Form     |
   | EntityId        | Specific record                 | 12345        |
   | DistrictId      | District context                | 01147        |
   | OldValue        | Previous state (JSON)           | {...}        |
   | NewValue        | New state (JSON)                | {...}        |
   | IPAddress       | Client IP                       | 10.x.x.x     |
   | UserAgent       | Browser/client info             | Chrome/...   |
   | CorrelationId   | Request trace ID                | GUID         |
   +------------------------------------------------------------------+

   RETENTION: 7 years (aligned with state audit requirements)
   STORAGE: Immutable Azure Blob + Azure Data Explorer
```

---

## 9. Deployment Model

### 9.1 Environment Strategy

```
+=============================================================================+
|                       DEPLOYMENT ENVIRONMENTS                                |
+=============================================================================+

   DEVELOPMENT              STAGING                 PRODUCTION
   ===========              =======                 ==========

+---------------+       +---------------+       +---------------+
|  DEV          |       |  STAGING      |       |  PROD         |
|  Environment  |       |  Environment  |       |  Environment  |
+---------------+       +---------------+       +---------------+
|               |       |               |       |               |
| Azure Sub: Dev|       | Azure Sub: Stg|       | Azure Sub: Prd|
|               |       |               |       |               |
| +----------+  |       | +----------+  |       | +----------+  |
| | App Svc  |  |       | | App Svc  |  |       | | App Svc  |  |
| | (B1)     |  |       | | (S2)     |  |       | | (P2v3)   |  |
| +----------+  |       | +----------+  |       | +----------+  |
|               |       |               |       |               |
| +----------+  |       | +----------+  |       | +----------+  |
| | SQL      |  |       | | SQL      |  |       | | SQL      |  |
| | (Basic)  |  |       | | (S3)     |  |       | | (P6)     |  |
| +----------+  |       | +----------+  |       | +----------+  |
|               |       |               |       |               |
| Data: Synth   |       | Data: Masked  |       | Data: Real    |
| Users: Dev    |       | Users: QA+UAT |       | Users: All    |
+---------------+       +---------------+       +---------------+
        |                       |                       |
        v                       v                       v
   Daily Deploys          Weekly Deploys          Bi-Weekly Deploys
   (Auto on PR)           (Manual Gate)           (Change Board)
```

### 9.2 Azure Resource Architecture

```
+=============================================================================+
|                       AZURE RESOURCE LAYOUT                                  |
+=============================================================================+

   RESOURCE GROUP: rg-sasquatch-prod-westus2
   =========================================

   +-------------------------------------------------------------------+
   |                                                                   |
   |   COMPUTE                                                         |
   |   +-------------------+  +-------------------+                    |
   |   | App Service Plan  |  | Function App      |                    |
   |   | (P2v3 - Premium)  |  | (Consumption)     |                    |
   |   +-------------------+  +-------------------+                    |
   |           |                      |                                |
   |           v                      v                                |
   |   +-------------------+  +-------------------+                    |
   |   | Web App           |  | Worker Functions  |                    |
   |   | (sasquatch-web)   |  | (calc-workers)    |                    |
   |   +-------------------+  +-------------------+                    |
   |                                                                   |
   |   DATA                                                            |
   |   +-------------------+  +-------------------+  +---------------+ |
   |   | Azure SQL         |  | Redis Cache       |  | Blob Storage  | |
   |   | (Business Crit.)  |  | (Premium P1)      |  | (LRS)         | |
   |   +-------------------+  +-------------------+  +---------------+ |
   |                                                                   |
   |   INTEGRATION                                                     |
   |   +-------------------+  +-------------------+                    |
   |   | Service Bus       |  | Logic Apps        |                    |
   |   | (Standard)        |  | (Integration)     |                    |
   |   +-------------------+  +-------------------+                    |
   |                                                                   |
   |   SECURITY & MONITORING                                           |
   |   +-------------------+  +-------------------+  +---------------+ |
   |   | Key Vault         |  | App Insights      |  | Log Analytics | |
   |   | (Standard)        |  | (per-GB)          |  | (per-GB)      | |
   |   +-------------------+  +-------------------+  +---------------+ |
   |                                                                   |
   +-------------------------------------------------------------------+
```

### 9.3 CI/CD Pipeline

```
+=============================================================================+
|                         CI/CD PIPELINE                                       |
+=============================================================================+

   CONTINUOUS INTEGRATION                    CONTINUOUS DEPLOYMENT
   ======================                    =====================

+----------+    +----------+    +----------+    +----------+    +----------+
|  Source  |    |  Build   |    |  Test    |    |  Package |    |  Deploy  |
|  (Git)   | -> |  (dotnet)| -> |  (xUnit) | -> |  (Docker)| -> |  (Azure) |
+----------+    +----------+    +----------+    +----------+    +----------+
     |               |               |               |               |
     v               v               v               v               v
+---------+    +-----------+   +-----------+   +-----------+   +-----------+
|Push/PR  |    |Compile    |   |Unit Tests |   |Container  |   |App Service|
|Trigger  |    |Restore    |   |Integration|   |Registry   |   |Deployment |
|         |    |Analyze    |   |E2E Tests  |   |Helm Charts|   |Slots      |
+---------+    +-----------+   +-----------+   +-----------+   +-----------+
                    |               |                               |
                    v               v                               v
              +-----------+   +-----------+                   +-----------+
              |SonarQube  |   |Code       |                   |Blue/Green |
              |Quality    |   |Coverage   |                   |Swap       |
              |Gate       |   |> 80%      |                   |           |
              +-----------+   +-----------+                   +-----------+

   DEPLOYMENT GATES
   ================

   DEV          STAGING         PROD
    |              |              |
    v              v              v
+-------+     +--------+     +--------+
| Auto  |     | Manual |     | Change |
| Deploy| --> | QA     | --> | Board  |
| on PR |     | Approval|    | Approval|
+-------+     +--------+     +--------+
```

### 9.4 Disaster Recovery

| Component | RPO | RTO | Strategy |
|-----------|-----|-----|----------|
| **Application** | 0 | 15 min | Multi-region deployment, Traffic Manager failover |
| **Database** | 5 min | 1 hour | Geo-replication, auto-failover groups |
| **Blob Storage** | 0 | Instant | GRS (Geo-Redundant Storage) |
| **Cache** | N/A | 5 min | Rebuild from database |
| **Configuration** | 0 | 15 min | Azure App Configuration (replicated) |

### 9.5 Backup Strategy

| Data Type | Frequency | Retention | Storage |
|-----------|-----------|-----------|---------|
| **Database (Full)** | Daily | 35 days | Azure Backup |
| **Database (PITR)** | Continuous | 7 days | Azure SQL built-in |
| **Blob Storage** | Continuous | 365 days | Soft delete + versioning |
| **Configuration** | On change | 90 days | Git + Key Vault |
| **Audit Logs** | Continuous | 7 years | Immutable Blob + ADX |

---

## 10. Appendices

### 10.1 Glossary

| Term | Definition |
|------|------------|
| **Apportionment** | The process of calculating and distributing state education funding |
| **CEDARS** | Comprehensive Education Data and Research System |
| **ESD** | Educational Service District (9 regions in WA) |
| **FERPA** | Family Educational Rights and Privacy Act |
| **GAAP** | Generally Accepted Accounting Principles |
| **LAP** | Learning Assistance Program |
| **MSOC** | Materials, Supplies, and Operating Costs |
| **OSPI** | Office of Superintendent of Public Instruction |
| **PSES** | Physical, Social, and Emotional Support |
| **SAFS** | School Apportionment Financial System (legacy) |
| **SAO** | State Auditor's Office |
| **SASQUATCH** | School Apportionment System for Quality, Accountability, Transparency, and Calculations Hub |
| **WaTech** | Washington Technology Solutions |

### 10.2 Reference Documents

| Document | Location |
|----------|----------|
| RFP 2026-12 | rfp_2026-12_sasquatch_apportionment.pdf |
| Attachment A - Requirements | Attachment_A_Sasquatch_System_RequirementsV2.xlsx |
| Attachment B - AS-IS Workflows | attachment_b_high-level_as-is_workflows.pdf |
| Attachment C - Demo Scenarios | attachment_c_sasquatch_rfp_demonstration_scenarios.docx |
| Requirements Catalog | REQUIREMENTS_CATALOG.md |

### 10.3 Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-01-19 | Architecture Team | Initial draft |

---

*This document is part of the SASQUATCH RFP response documentation.*
