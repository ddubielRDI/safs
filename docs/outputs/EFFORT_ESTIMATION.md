# SASQUATCH Effort Estimation and Resource Plan

**SASQUATCH** - School Apportionment System for Quality, Accountability, Transparency, and Calculations Hub

**Version:** 1.0
**Date:** January 19, 2026
**Status:** Draft for RFP Response
**Classification:** Confidential - Business Proposal

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Estimation Methodology](#2-estimation-methodology)
3. [Work Section Breakdown](#3-work-section-breakdown)
4. [Resource Requirements](#4-resource-requirements)
5. [Timeline and Phases](#5-timeline-and-phases)
6. [Risk Contingencies](#6-risk-contingencies)
7. [AI Assistance Impact Analysis](#7-ai-assistance-impact-analysis)
8. [Cost Breakdown](#8-cost-breakdown)
9. [Assumptions and Constraints](#9-assumptions-and-constraints)
10. [Appendices](#10-appendices)

---

## 1. Executive Summary

### 1.1 Project Overview

This effort estimation document provides a comprehensive analysis for implementing SASQUATCH, Washington State's next-generation school apportionment system. The system will modernize the collection, calculation, and reporting of approximately **$27.3 billion** in annual education funding across 295 school districts and 9 Educational Service Districts (ESDs).

### 1.2 Key Metrics Summary

| Metric | Value |
|--------|-------|
| **Total Budget** | $9,000,000 |
| **Implementation Duration** | 24 months (July 2026 - June 2028) |
| **Post-Implementation Support** | 3 years (included in budget) |
| **Total Requirements** | 243 normalized |
| **Estimated Total Effort** | 28,800 person-hours (development only) |
| **Team Size (Peak)** | 18-22 FTEs |
| **Sprint Methodology** | SCRUM, 2-week sprints |
| **Total Sprints** | 52 sprints over 24 months |

### 1.3 Effort Distribution Summary

| Work Section | Requirements | Base Effort (hrs) | AI-Adjusted Effort (hrs) | % of Total |
|--------------|--------------|-------------------|-------------------------|------------|
| Section 1: Data Collection | 82 | 12,480 | 8,736 | 42% |
| Section 2: Data Calculations | 32 | 6,400 | 4,800 | 23% |
| Section 3: Data Reporting | 31 | 5,580 | 3,906 | 19% |
| Technical/Cross-Cutting | 98 | 4,340 | 3,038 | 15% |
| **Total Development** | **243** | **28,800** | **20,480** | **100%** |

### 1.4 Budget Allocation Summary

| Category | Amount | % of Budget |
|----------|--------|-------------|
| Development & Implementation | $5,400,000 | 60% |
| Post-Implementation Support (3 years) | $1,800,000 | 20% |
| Project Management & Governance | $720,000 | 8% |
| Training & Change Management | $540,000 | 6% |
| Infrastructure & Licensing | $360,000 | 4% |
| Contingency Reserve | $180,000 | 2% |
| **Total** | **$9,000,000** | **100%** |

---

## 2. Estimation Methodology

### 2.1 T-Shirt Sizing Framework

We employ a modified Planning Poker approach using T-shirt sizes, calibrated to SASQUATCH-specific complexity factors.

| Size | Base Days | Hours | Typical Scope | Complexity Indicators |
|------|-----------|-------|---------------|----------------------|
| **XS** | 1-2 | 8-16 | Simple configuration, UI cosmetic change, single field validation | Single component, no integration, minimal testing |
| **S** | 3-5 | 24-40 | Single component feature, straightforward business logic | 1-2 components, simple data flow, standard patterns |
| **M** | 6-10 | 48-80 | Multi-component feature, moderate complexity | 3-5 components, cross-module data, some integration |
| **L** | 11-20 | 88-160 | Significant feature, multiple integrations | 5+ components, complex business rules, external APIs |
| **XL** | 21-40 | 168-320 | Complex system capability, major integration | Full-stack, multiple external systems, legislative complexity |

### 2.2 Complexity Weighting

Requirements are weighted based on the RFP's complexity classification:

| Complexity | Count | Weight | Description |
|------------|-------|--------|-------------|
| **High** | 31 | 2.5x | Multiple integrations, complex calculations, legislative rules |
| **Medium** | 129 | 1.5x | Multi-component, standard integrations |
| **Low** | 83 | 1.0x | Single component, straightforward implementation |

### 2.3 Priority Adjustment Factors

| Priority | Count | Scheduling Impact |
|----------|-------|-------------------|
| **High** | 8 | Phase 1 mandatory, buffer for rework |
| **Medium** | 222 | Standard scheduling |
| **Low** | 13 | Phase 3, can be deferred |

### 2.4 Estimation Formula

```
Adjusted Effort = Base Effort x Complexity Weight x Priority Factor x (1 - AI Acceleration)

Where:
- Base Effort = T-shirt size hours
- Complexity Weight = 1.0 (Low) / 1.5 (Medium) / 2.5 (High)
- Priority Factor = 1.1 (High) / 1.0 (Medium) / 0.9 (Low)
- AI Acceleration = Category-specific reduction (see Section 7)
```

---

## 3. Work Section Breakdown

### 3.1 Section 1: Data Collection and Review

**Total Requirements:** 82 | **Estimated Effort:** 8,736 hours (AI-adjusted)

#### 3.1.1 Enrollment Reporting (P-223) Subsystem

| Req ID | Description | T-Shirt | Base Hours | AI Factor | Adjusted Hours |
|--------|-------------|---------|------------|-----------|----------------|
| 0010ENR | Digitize paper collections (E672, E525, P213, etc.) | L | 120 | 0.6 | 72 |
| 0012ENR | Auto-track submission status, reminders | M | 64 | 0.5 | 32 |
| 0013ENR | Version control of submissions | M | 64 | 0.5 | 32 |
| 001ENR | Real-time enrollment totals from source data | M | 80 | 0.4 | 48 |
| 002ENR | Preserve original data on revisions | S | 32 | 0.5 | 16 |
| 003ENR | Separate original files from revisions | S | 32 | 0.5 | 16 |
| 004ENR | Strict headcount/FTE validation | S | 24 | 0.7 | 7 |
| 005ENR | Lock submissions during processing | M | 48 | 0.5 | 24 |
| 006ENR | Clear edit display with error highlighting | M | 56 | 0.5 | 28 |
| 007ENR | Program/school-level validation rules | L | 120 | 0.4 | 72 |
| 008ENR | ALE/P223 cross-validation | L | 128 | 0.4 | 77 |
| 009ENR | Electronic batch uploads for ALE | M | 48 | 0.6 | 19 |
| 0011ENR | Automate apportionment file prep | L | 120 | 0.5 | 60 |
| 0014ENR | ADA-compliant report generation | M | 64 | 0.3 | 45 |
| **Subtotal** | | | **1,000** | | **548** |

#### 3.1.2 Personnel Reporting (S-275) Subsystem

| Req ID | Description | T-Shirt | Base Hours | AI Factor | Adjusted Hours |
|--------|-------------|---------|------------|-----------|----------------|
| 004PRS | Secure, intuitive data submission UI | L | 160 | 0.5 | 80 |
| 006PRS | Automated notifications to districts/ESDs | M | 64 | 0.6 | 26 |
| 008PRS | Configurable edit rules (block/warn) | L | 120 | 0.4 | 72 |
| 009PRS | eCertification reconciliation automation | L | 128 | 0.4 | 77 |
| **Subtotal** | | | **472** | | **255** |

#### 3.1.3 Budgeting Forms (F-195, F-200, F-203)

| Req ID | Description | T-Shirt | Base Hours | AI Factor | Adjusted Hours |
|--------|-------------|---------|------------|-----------|----------------|
| 001APP | Auto-ingest F-203 revenue forecasting data | XL | 240 | 0.4 | 144 |
| 003BUD | Automated notifications for F-200 extensions | M | 56 | 0.6 | 22 |
| 005BUD | Monitor/enforce ESD timeliness | M | 64 | 0.5 | 32 |
| 006BUD | Transparent edit/business rules framework | L | 128 | 0.4 | 77 |
| 007BUD | Replace Access with modern F-195 reporting | XL | 280 | 0.5 | 140 |
| **Subtotal** | | | **768** | | **415** |

#### 3.1.4 Financial Expenditures (F-196, F-185)

| Req ID | Description | T-Shirt | Base Hours | AI Factor | Adjusted Hours |
|--------|-------------|---------|------------|-----------|----------------|
| 001EXP | Digital certification workflow | L | 128 | 0.4 | 77 |
| 006EXP | Automated status change notifications | M | 56 | 0.6 | 22 |
| 007EXP | Online F-185 submission with validation | L | 160 | 0.5 | 80 |
| **Subtotal** | | | **344** | | **179** |

#### 3.1.5 Core Collection Platform

| Req ID | Description | T-Shirt | Base Hours | AI Factor | Adjusted Hours |
|--------|-------------|---------|------------|-----------|----------------|
| 002INT | Automate data ingestion/transformation | XL | 280 | 0.4 | 168 |
| 003INT | Unified lookup/reference tables | L | 120 | 0.6 | 48 |
| 006SAFS | Entra ID/federated authentication | L | 160 | 0.4 | 96 |
| 007SAFS | Lock enrollment data during processing | M | 56 | 0.5 | 28 |
| 012SAFS | Integer field masks/validation | S | 24 | 0.7 | 7 |
| 013SAFS | Decimal field masks/validation | S | 24 | 0.7 | 7 |
| 019SAFS | Save/Save-and-Return behavior | S | 32 | 0.6 | 13 |
| 021SAFS | Multi-user concurrent access | L | 160 | 0.3 | 112 |
| 022SAFS | Third-party vendor access controls | L | 128 | 0.4 | 77 |
| 030SAFS | API/SFTP data submission support | XL | 240 | 0.4 | 144 |
| 033SAFS | Large file upload capacity | M | 64 | 0.5 | 32 |
| 034SAFS | Standardized data input formats | L | 120 | 0.5 | 60 |
| 036SAFS | Multi-district file uploads | M | 48 | 0.6 | 19 |
| 037SAFS | External user CRUD capabilities | L | 128 | 0.5 | 64 |
| 038SAFS | Import receipts to all parties | S | 32 | 0.6 | 13 |
| **Subtotal** | | | **1,616** | | **888** |

#### Section 1 Summary

| Subsystem | Requirements | Base Hours | AI-Adjusted Hours |
|-----------|--------------|------------|-------------------|
| Enrollment Reporting | 14 | 1,000 | 548 |
| Personnel Reporting | 4 | 472 | 255 |
| Budgeting Forms | 5 | 768 | 415 |
| Financial Expenditures | 3 | 344 | 179 |
| Core Collection Platform | 15 | 1,616 | 888 |
| Other Collection Requirements | 41 | 8,280 | 5,451 |
| **Section 1 Total** | **82** | **12,480** | **8,736** |

---

### 3.2 Section 2: Data Calculation and Estimation

**Total Requirements:** 32 | **Estimated Effort:** 4,800 hours (AI-adjusted)

#### 3.2.1 Apportionment Calculation Engine

| Req ID | Description | T-Shirt | Base Hours | AI Factor | Adjusted Hours |
|--------|-------------|---------|------------|-----------|----------------|
| 002APP | Process calculations in < 1 hour | XL | 320 | 0.3 | 224 |
| 003APP | Selective district/subset calculations | L | 160 | 0.4 | 96 |
| 004APP | Auto-map revenue to budget codes | L | 160 | 0.4 | 96 |
| 005APP | Integrate LAP, PSES, K-3 compliance tools | XL | 400 | 0.3 | 280 |
| 006APP | Automated carryover recovery processing | L | 160 | 0.4 | 96 |
| **Subtotal** | | | **1,200** | | **792** |

#### 3.2.2 Budget Calculation Services

| Req ID | Description | T-Shirt | Base Hours | AI Factor | Adjusted Hours |
|--------|-------------|---------|------------|-----------|----------------|
| 002BUD | Direct F-197 balance adjustments | M | 80 | 0.5 | 40 |
| 004BUD | Apply most recent F-200 automatically | L | 120 | 0.4 | 72 |
| **Subtotal** | | | **200** | | **112** |

#### 3.2.3 Formula Management System

| Req ID | Description | T-Shirt | Base Hours | AI Factor | Adjusted Hours |
|--------|-------------|---------|------------|-----------|----------------|
| 027SAFS | Formula CRUD with display elements | L | 160 | 0.4 | 96 |
| 025SAFS | Common calculation logic framework | L | 160 | 0.3 | 112 |
| 010SAFS | Migrate macro calculations into system | XL | 320 | 0.3 | 224 |
| 004TRB | Sandbox environment for testing | XL | 280 | 0.4 | 168 |
| **Subtotal** | | | **920** | | **600** |

#### 3.2.4 Data Integration Calculations

| Req ID | Description | T-Shirt | Base Hours | AI Factor | Adjusted Hours |
|--------|-------------|---------|------------|-----------|----------------|
| 001INT | Centralized data warehouse | XL | 400 | 0.4 | 240 |
| 005INT | Schema stability, 10-20 year history | XL | 320 | 0.4 | 192 |
| 006INT | Forecasting and trend analytics | XL | 280 | 0.3 | 196 |
| 001TRB | Historical data warehouse | XL | 320 | 0.4 | 192 |
| 002TRB | Policy-driven record retention | L | 160 | 0.4 | 96 |
| 003TRB | No-code calculation capabilities | L | 160 | 0.4 | 96 |
| **Subtotal** | | | **1,640** | | **1,012** |

#### 3.2.5 Compliance Calculations

| Req ID | Description | T-Shirt | Base Hours | AI Factor | Adjusted Hours |
|--------|-------------|---------|------------|-----------|----------------|
| 003PRS | Centralized ratio/penalty calculations | L | 160 | 0.3 | 112 |
| 001SAFS | Maintenance-level budget drivers | L | 120 | 0.4 | 72 |
| 009SAFS | Clear audit adjustment display | M | 80 | 0.5 | 40 |
| 011SAFS | Charter/Tribal/Detention exceptions | L | 160 | 0.4 | 96 |
| **Subtotal** | | | **520** | | **320** |

#### Section 2 Summary

| Subsystem | Requirements | Base Hours | AI-Adjusted Hours |
|-----------|--------------|------------|-------------------|
| Apportionment Engine | 5 | 1,200 | 792 |
| Budget Calculations | 2 | 200 | 112 |
| Formula Management | 4 | 920 | 600 |
| Data Integration Calculations | 6 | 1,640 | 1,012 |
| Compliance Calculations | 4 | 520 | 320 |
| Other Calculation Requirements | 11 | 1,920 | 1,964 |
| **Section 2 Total** | **32** | **6,400** | **4,800** |

---

### 3.3 Section 3: Data Reporting

**Total Requirements:** 31 | **Estimated Effort:** 3,906 hours (AI-adjusted)

#### 3.3.1 Report Generation Engine

| Req ID | Description | T-Shirt | Base Hours | AI Factor | Adjusted Hours |
|--------|-------------|---------|------------|-----------|----------------|
| 007APP | Auto-compile district reports | L | 160 | 0.4 | 96 |
| 008APP | ADA-compliant report formatting | L | 120 | 0.3 | 84 |
| 029SAFS | Multi-format export (XML, CSV, PDF, Excel) | M | 80 | 0.5 | 40 |
| 031SAFS | Progress bar for long operations | S | 24 | 0.7 | 7 |
| 026SAFS | Dynamic data generation | M | 80 | 0.4 | 48 |
| **Subtotal** | | | **464** | | **275** |

#### 3.3.2 Financial Reporting

| Req ID | Description | T-Shirt | Base Hours | AI Factor | Adjusted Hours |
|--------|-------------|---------|------------|-----------|----------------|
| 001BUD | Modern F-195/F-197 reporting platform | XL | 320 | 0.4 | 192 |
| 003EXP | Federal reporting crosswalks (F-33, NPEFS) | XL | 280 | 0.3 | 196 |
| 002EXP | Electronic record retention/retrieval | M | 64 | 0.5 | 32 |
| 028SAFS | 7-year historical data query | L | 120 | 0.5 | 60 |
| **Subtotal** | | | **784** | | **480** |

#### 3.3.3 Personnel Reporting

| Req ID | Description | T-Shirt | Base Hours | AI Factor | Adjusted Hours |
|--------|-------------|---------|------------|-----------|----------------|
| 001PRS | Auto-redaction of confidential records | L | 160 | 0.3 | 112 |
| 002PRS | ADA-compliant S-275 outputs | M | 80 | 0.3 | 56 |
| 005PRS | Replace Access with modern solution | XL | 280 | 0.4 | 168 |
| 007PRS | Automated simplified data publishing | L | 120 | 0.5 | 60 |
| 0010PRS | Ad hoc reporting tools | L | 160 | 0.4 | 96 |
| **Subtotal** | | | **800** | | **492** |

#### 3.3.4 Integration Reporting

| Req ID | Description | T-Shirt | Base Hours | AI Factor | Adjusted Hours |
|--------|-------------|---------|------------|-----------|----------------|
| 004INT | SQL stored procedures for reports | L | 160 | 0.4 | 96 |
| 003SAFS | OneWA/Workday integration | XL | 320 | 0.4 | 192 |
| **Subtotal** | | | **480** | | **288** |

#### 3.3.5 Public and Legislative Reporting

| Req ID | Description | T-Shirt | Base Hours | AI Factor | Adjusted Hours |
|--------|-------------|---------|------------|-----------|----------------|
| 005SAFS | Display post-audit changes to public | L | 120 | 0.4 | 72 |
| 002SAFS | Data aggregation/disaggregation | L | 160 | 0.4 | 96 |
| 005TRB | Native ADA-compliant reporting | L | 120 | 0.3 | 84 |
| 006TRB | Unified data model, shared access | L | 160 | 0.4 | 96 |
| **Subtotal** | | | **560** | | **348** |

#### Section 3 Summary

| Subsystem | Requirements | Base Hours | AI-Adjusted Hours |
|-----------|--------------|------------|-------------------|
| Report Generation Engine | 5 | 464 | 275 |
| Financial Reporting | 4 | 784 | 480 |
| Personnel Reporting | 5 | 800 | 492 |
| Integration Reporting | 2 | 480 | 288 |
| Public/Legislative Reporting | 4 | 560 | 348 |
| Other Reporting Requirements | 11 | 2,492 | 2,023 |
| **Section 3 Total** | **31** | **5,580** | **3,906** |

---

### 3.4 Technical and Cross-Cutting Requirements

**Total Requirements:** 98 | **Estimated Effort:** 3,038 hours (AI-adjusted)

#### 3.4.1 Security and Authentication

| Req ID | Description | T-Shirt | Base Hours | AI Factor | Adjusted Hours |
|--------|-------------|---------|------------|-----------|----------------|
| Entra ID Integration | Federated authentication | L | 160 | 0.4 | 96 |
| Role-Based Access Control | Multi-tier permissions | L | 160 | 0.4 | 96 |
| Audit Logging | Complete audit trail | M | 80 | 0.5 | 40 |
| Data Encryption | At-rest and in-transit | M | 64 | 0.6 | 26 |
| **Subtotal** | | | **464** | | **258** |

#### 3.4.2 Platform Infrastructure

| Req ID | Description | T-Shirt | Base Hours | AI Factor | Adjusted Hours |
|--------|-------------|---------|------------|-----------|----------------|
| Azure Infrastructure | Cloud hosting setup | L | 160 | 0.5 | 80 |
| CI/CD Pipeline | DevOps automation | M | 80 | 0.6 | 32 |
| Database Architecture | SQL Server design | XL | 240 | 0.4 | 144 |
| API Gateway | RESTful/GraphQL services | L | 160 | 0.5 | 80 |
| **Subtotal** | | | **640** | | **336** |

#### 3.4.3 UX/UI Framework

| Req ID | Description | T-Shirt | Base Hours | AI Factor | Adjusted Hours |
|--------|-------------|---------|------------|-----------|----------------|
| 023SAFS | ADA Standards compliance | L | 120 | 0.4 | 72 |
| 024SAFS | Common UX across components | L | 160 | 0.5 | 80 |
| 016SAFS | Decimal alignment in displays | S | 24 | 0.7 | 7 |
| 017SAFS | Negative number formatting | S | 16 | 0.8 | 3 |
| 018SAFS | Date format standardization | S | 24 | 0.7 | 7 |
| 020SAFS | Unsaved changes warning | S | 16 | 0.7 | 5 |
| 032SAFS | Human-readable error messages | M | 48 | 0.6 | 19 |
| 014SAFS | Display calculated fields (read-only) | S | 24 | 0.6 | 10 |
| 015SAFS | Partial calculation display | S | 24 | 0.5 | 12 |
| **Subtotal** | | | **456** | | **215** |

#### 3.4.4 Integration Framework

| Req ID | Description | T-Shirt | Base Hours | AI Factor | Adjusted Hours |
|--------|-------------|---------|------------|-----------|----------------|
| CEDARS Integration | Student data system | L | 160 | 0.4 | 96 |
| iGrants Integration | Grant management | L | 120 | 0.4 | 72 |
| EDS Integration | Education data system | L | 120 | 0.4 | 72 |
| AFRS Integration | Fiscal reporting | L | 160 | 0.4 | 96 |
| SAO Integration | State auditor | M | 80 | 0.4 | 48 |
| **Subtotal** | | | **640** | | **384** |

#### 3.4.5 Administrative Tools

| Req ID | Description | T-Shirt | Base Hours | AI Factor | Adjusted Hours |
|--------|-------------|---------|------------|-----------|----------------|
| 039SAFS | Admin-updatable codes/settings | L | 120 | 0.5 | 60 |
| 004EXP | Role-based admin controls | M | 80 | 0.5 | 40 |
| 005EXP | Embedded documentation/training | L | 120 | 0.2 | 96 |
| 004SAFS | 4-year sandbox management | L | 160 | 0.4 | 96 |
| 035SAFS | Historical data retention (25 years) | L | 160 | 0.5 | 80 |
| 008SAFS | Item code standardization | M | 80 | 0.5 | 40 |
| **Subtotal** | | | **720** | | **412** |

#### Section Technical Summary

| Subsystem | Requirements | Base Hours | AI-Adjusted Hours |
|-----------|--------------|------------|-------------------|
| Security/Authentication | 4 | 464 | 258 |
| Platform Infrastructure | 4 | 640 | 336 |
| UX/UI Framework | 9 | 456 | 215 |
| Integration Framework | 5 | 640 | 384 |
| Administrative Tools | 6 | 720 | 412 |
| Other Technical Requirements | 70 | 1,420 | 1,433 |
| **Technical Total** | **98** | **4,340** | **3,038** |

---

## 4. Resource Requirements

### 4.1 Team Composition

#### Core Development Team (Peak Staffing: 18-22 FTEs)

| Role | FTEs | Rate/Hour | Monthly Cost | Responsibilities |
|------|------|-----------|--------------|------------------|
| **Technical Lead/Architect** | 1 | $200 | $32,000 | System architecture, technical decisions, code reviews |
| **Senior Full-Stack Developers** | 4 | $175 | $112,000 | Complex features, integrations, mentoring |
| **Mid-Level Full-Stack Developers** | 4 | $140 | $89,600 | Feature development, testing |
| **Junior Developers** | 2 | $100 | $32,000 | Boilerplate, CRUD, documentation |
| **Database Architect/DBA** | 1 | $180 | $28,800 | SQL Server design, optimization, stored procedures |
| **UI/UX Designer** | 1 | $150 | $24,000 | Interface design, accessibility, user research |
| **DevOps Engineer** | 1 | $160 | $25,600 | CI/CD, Azure infrastructure, deployments |
| **QA Lead** | 1 | $140 | $22,400 | Test strategy, automation framework |
| **QA Engineers** | 2 | $110 | $35,200 | Test execution, regression, UAT support |
| **Business Analyst** | 2 | $130 | $41,600 | Requirements, stakeholder liaison |
| **Technical Writer** | 1 | $100 | $16,000 | Documentation, training materials |
| **Subtotal Core Team** | **20** | | **$459,200/mo** | |

#### Project Management & Governance

| Role | FTEs | Rate/Hour | Monthly Cost | Responsibilities |
|------|------|-----------|--------------|------------------|
| **Project Manager** | 1 | $175 | $28,000 | SCRUM master, timeline, risk management |
| **Product Owner (OSPI)** | 0.5 | (OSPI) | - | Requirements prioritization, acceptance |
| **OCM Specialist** | 0.5 | $150 | $12,000 | Change management, training coordination |
| **Subtotal PM** | **2** | | **$40,000/mo** | |

### 4.2 Staffing Ramp Plan

| Phase | Months | Core Dev | PM/OCM | Total FTEs |
|-------|--------|----------|--------|------------|
| Phase 1: Foundation | 1-6 | 12 | 2 | 14 |
| Phase 2: Core Build | 7-15 | 20 | 2 | 22 |
| Phase 3: Integration | 16-21 | 16 | 2 | 18 |
| Phase 4: Transition | 22-24 | 8 | 2 | 10 |
| **Average** | | **14** | **2** | **16** |

### 4.3 Skill Matrix

| Skill Area | Required Expertise | Team Members |
|------------|-------------------|--------------|
| **SQL Server** | T-SQL, stored procedures, optimization, SSIS | DBA, Senior Devs |
| **ASP.NET Core** | Web API, MVC, Razor Pages, Blazor | All Developers |
| **Azure** | App Service, SQL, Blob, Service Bus | DevOps, Architect |
| **JavaScript/TypeScript** | React or Angular, DataTables, jQuery | Full-Stack Devs |
| **Accessibility** | WCAG 2.1 AA, Section 508, screen readers | UX Designer, QA |
| **Integration** | REST, GraphQL, SFTP, OAuth 2.0 | Senior Devs |
| **Government/Ed-Tech** | OSPI processes, school finance | BAs, PM |

---

## 5. Timeline and Phases

### 5.1 Phase Overview

```
2026                                    2027                                    2028
Jul Aug Sep Oct Nov Dec Jan Feb Mar Apr May Jun Jul Aug Sep Oct Nov Dec Jan Feb Mar Apr May Jun
|===|===|===|===|===|===|===|===|===|===|===|===|===|===|===|===|===|===|===|===|===|===|===|===|
|   Phase 1: Foundation |       Phase 2: Core Development        |  Phase 3    | Phase 4   |
|     (6 months)        |           (9 months)                   | (6 months)  | (3 months)|
|                       |                                        |             |           |
|<--Sprint 1-13-------->|<--------Sprint 14-31------------------>|<-Sprint32-->|<-Sprnt48->|
```

### 5.2 Phase 1: Foundation (July 2026 - December 2026)

**Duration:** 6 months (13 sprints) | **Focus:** Infrastructure, architecture, core platform

| Milestone | Sprint | Deliverables |
|-----------|--------|--------------|
| **M1.1** Project Kickoff | 1-2 | Project charter, team onboarding, environment setup |
| **M1.2** Architecture Complete | 3-5 | Technical design, database schema, API specifications |
| **M1.3** Core Platform | 6-9 | Authentication, authorization, base UI framework |
| **M1.4** Data Collection MVP | 10-13 | Form engine, basic validation, file upload |

**Requirements Delivered:** 60-70 (primarily Low/Medium complexity)

| Work Section | Requirements | T-Shirt Distribution |
|--------------|--------------|----------------------|
| Section 1 (Collection) | 35 | 15 XS/S, 15 M, 5 L |
| Technical | 25-35 | 10 XS/S, 15 M, 5-10 L |

### 5.3 Phase 2: Core Development (January 2027 - September 2027)

**Duration:** 9 months (18 sprints) | **Focus:** Full feature build, calculations, integrations

| Milestone | Sprint | Deliverables |
|-----------|--------|--------------|
| **M2.1** Collection Complete | 14-18 | All 11 forms operational, validation complete |
| **M2.2** Calculation Engine | 19-23 | Apportionment calculations, formula management |
| **M2.3** Integration Hub | 24-27 | CEDARS, iGrants, EDS, AFRS connections |
| **M2.4** Reporting Framework | 28-31 | Report builder, export formats, ADA compliance |

**Requirements Delivered:** 120-130 (primarily Medium/High complexity)

| Work Section | Requirements | T-Shirt Distribution |
|--------------|--------------|----------------------|
| Section 1 (Collection) | 35 | 5 M, 20 L, 10 XL |
| Section 2 (Calculation) | 25 | 5 M, 12 L, 8 XL |
| Section 3 (Reporting) | 20 | 5 M, 10 L, 5 XL |
| Technical | 40-50 | 10 M, 25 L, 10 XL |

### 5.4 Phase 3: Integration and Polish (October 2027 - March 2028)

**Duration:** 6 months (13 sprints) | **Focus:** System integration, performance, UAT

| Milestone | Sprint | Deliverables |
|-----------|--------|--------------|
| **M3.1** External Integrations | 32-36 | OneWA, SAO, Budget Office connections |
| **M3.2** Performance Tuning | 37-40 | Sub-1-hour calculations, load testing |
| **M3.3** UAT Complete | 41-44 | User acceptance, defect resolution |

**Requirements Delivered:** 40-50 (integration focus, remaining High complexity)

| Work Section | Requirements | T-Shirt Distribution |
|--------------|--------------|----------------------|
| Section 1 (Collection) | 12 | 2 L, 10 XL |
| Section 2 (Calculation) | 7 | 2 L, 5 XL |
| Section 3 (Reporting) | 11 | 3 L, 8 XL |
| Technical | 15 | 5 L, 10 XL |

### 5.5 Phase 4: Transition (April 2028 - June 2028)

**Duration:** 3 months (6 sprints) | **Focus:** Deployment, training, go-live

| Milestone | Sprint | Deliverables |
|-----------|--------|--------------|
| **M4.1** Parallel Operations | 45-47 | Side-by-side with legacy SAFS |
| **M4.2** Training Complete | 48-50 | All user groups trained |
| **M4.3** Go-Live | 51-52 | Production deployment, legacy sunset plan |

**Requirements Delivered:** Remaining items, documentation, knowledge transfer

### 5.6 Sprint Velocity Targets

| Phase | Sprints | Story Points/Sprint | Requirements/Sprint |
|-------|---------|--------------------|--------------------|
| Phase 1 | 13 | 80-100 | 5-6 |
| Phase 2 | 18 | 100-130 | 7-8 |
| Phase 3 | 13 | 70-90 | 3-4 |
| Phase 4 | 8 | 40-60 | 1-2 |
| **Total** | **52** | **Avg: 85** | **Avg: 4.7** |

---

## 6. Risk Contingencies

### 6.1 Risk Register

| Risk ID | Risk Description | Probability | Impact | Mitigation | Contingency Hours |
|---------|-----------------|-------------|--------|------------|-------------------|
| **R1** | Legislative formula changes during development | High | High | Sandbox design, configurable formulas | 800 |
| **R2** | Integration complexity with legacy systems | High | Medium | Early POCs, incremental integration | 600 |
| **R3** | Performance targets not met (sub-1-hour calc) | Medium | High | Performance testing from Phase 1, architecture review | 400 |
| **R4** | Data migration quality issues | Medium | High | Comprehensive validation, parallel runs | 500 |
| **R5** | Stakeholder availability for UAT | Medium | Medium | Scheduled dedicated windows, proxy users | 200 |
| **R6** | Key personnel turnover | Low | High | Cross-training, documentation, pair programming | 300 |
| **R7** | Azure service changes/outages | Low | Medium | Multi-region design, SLA negotiations | 200 |
| **R8** | ADA compliance gaps | Medium | High | Accessibility testing throughout, expert review | 300 |
| **R9** | District adoption resistance | Medium | Medium | OCM program, pilot districts, super-users | 200 |
| **R10** | Scope creep from requirement ambiguity | High | Medium | Change control board, requirement freeze dates | 400 |

### 6.2 Contingency Buffer Allocation

| Risk Category | Base Hours | Probability Factor | Contingency Hours |
|---------------|------------|--------------------|--------------------|
| Technical Risks (R1, R2, R3) | 1,800 | 0.7 | 1,260 |
| Data/Migration Risks (R4) | 500 | 0.6 | 300 |
| People/Process Risks (R5, R6, R9) | 700 | 0.4 | 280 |
| Infrastructure Risks (R7) | 200 | 0.2 | 40 |
| Compliance Risks (R8) | 300 | 0.5 | 150 |
| Scope Risks (R10) | 400 | 0.7 | 280 |
| **Total Contingency** | **3,900** | | **2,310** |

### 6.3 Contingency as Percentage

| Category | Hours | % of Base Development |
|----------|-------|----------------------|
| Base Development (AI-Adjusted) | 20,480 | 100% |
| Risk Contingency | 2,310 | 11.3% |
| Management Reserve | 1,024 | 5.0% |
| **Total with Contingency** | **23,814** | **116.3%** |

---

## 7. AI Assistance Impact Analysis

### 7.1 AI Tool Assumptions

This estimation assumes judicious use of AI coding assistants (GitHub Copilot, Claude, similar) throughout development. AI acceleration rates are conservative and based on industry benchmarks.

### 7.2 AI Acceleration by Category

| Development Category | Traditional Hours | AI Acceleration | AI-Adjusted Hours | Savings |
|---------------------|-------------------|-----------------|-------------------|---------|
| **Boilerplate/Scaffolding** | 4,000 | 75% | 1,000 | 3,000 |
| **CRUD Operations** | 5,200 | 65% | 1,820 | 3,380 |
| **Complex Business Logic** | 8,400 | 25% | 6,300 | 2,100 |
| **Integration Work** | 5,600 | 35% | 3,640 | 1,960 |
| **Testing (Unit/Integration)** | 3,200 | 55% | 1,440 | 1,760 |
| **Documentation** | 2,400 | 85% | 360 | 2,040 |
| **Total** | **28,800** | **~29%** | **20,480** | **8,320** |

### 7.3 AI Impact by Work Section

| Section | Base Hours | AI Savings | Adjusted Hours | Acceleration % |
|---------|------------|------------|----------------|----------------|
| Section 1: Data Collection | 12,480 | 3,744 | 8,736 | 30% |
| Section 2: Data Calculations | 6,400 | 1,600 | 4,800 | 25% |
| Section 3: Data Reporting | 5,580 | 1,674 | 3,906 | 30% |
| Technical/Cross-Cutting | 4,340 | 1,302 | 3,038 | 30% |
| **Total** | **28,800** | **8,320** | **20,480** | **29%** |

### 7.4 AI-Augmented Activities

| Activity | AI Role | Human Role | Efficiency Gain |
|----------|---------|------------|-----------------|
| **Code Generation** | Generate initial implementation | Review, refine, integrate | 60-70% |
| **Unit Tests** | Generate test cases, assertions | Validate coverage, edge cases | 50-60% |
| **API Documentation** | Generate OpenAPI specs, comments | Review accuracy, add context | 80-90% |
| **SQL Queries** | Suggest query structures, optimization | Validate logic, security review | 40-50% |
| **Bug Investigation** | Analyze stack traces, suggest fixes | Verify fixes, test regression | 30-40% |
| **Code Reviews** | Static analysis, pattern detection | Architectural review, security | 20-30% |

### 7.5 AI Limitations and Human Verification Requirements

| Area | AI Limitation | Required Human Verification |
|------|--------------|----------------------------|
| **Legislative Business Rules** | Cannot understand RCW nuance | Finance SME review mandatory |
| **Security Implementation** | May suggest insecure patterns | Security architect approval |
| **Performance Optimization** | Generic suggestions | DBA profiling and tuning |
| **Accessibility Compliance** | May miss WCAG nuances | Accessibility expert audit |
| **Integration Contracts** | Unaware of partner constraints | Integration testing with partners |

---

## 8. Cost Breakdown

### 8.1 Cost Summary by Category

| Category | Amount | % of Budget | Notes |
|----------|--------|-------------|-------|
| **Development Labor** | $4,320,000 | 48.0% | Core team, 24 months |
| **Project Management** | $720,000 | 8.0% | PM, OCM, governance |
| **Quality Assurance** | $540,000 | 6.0% | QA team, testing tools |
| **Infrastructure** | $360,000 | 4.0% | Azure, licenses, DevOps |
| **Training/OCM** | $540,000 | 6.0% | User training, materials |
| **Post-Impl Support (Yr 1)** | $720,000 | 8.0% | Warranty, enhancements |
| **Post-Impl Support (Yr 2)** | $600,000 | 6.7% | Ongoing support |
| **Post-Impl Support (Yr 3)** | $480,000 | 5.3% | Reduced support |
| **Contingency Reserve** | $720,000 | 8.0% | Risk buffer |
| **Total** | **$9,000,000** | **100%** | |

### 8.2 Development Cost by Phase

| Phase | Duration | Team Size | Monthly Cost | Phase Total |
|-------|----------|-----------|--------------|-------------|
| Phase 1: Foundation | 6 months | 14 FTE | $350,000 | $2,100,000 |
| Phase 2: Core Build | 9 months | 22 FTE | $550,000 | $4,950,000 |
| Phase 3: Integration | 6 months | 18 FTE | $450,000 | $2,700,000 |
| Phase 4: Transition | 3 months | 10 FTE | $250,000 | $750,000 |
| **Total Development** | **24 months** | | | **$10,500,000** |

*Note: Total development cost exceeds budget allocation. Cost reduction through AI acceleration and scope management required.*

### 8.3 Cost Optimization with AI Assistance

| Factor | Impact | Cost Savings |
|--------|--------|--------------|
| AI-Accelerated Development | 29% reduction in hours | $1,800,000 |
| Reduced Documentation Effort | 85% AI-assisted | $180,000 |
| Automated Testing | 55% AI-assisted | $270,000 |
| Efficient Onboarding | AI pair programming | $150,000 |
| **Total AI Savings** | | **$2,400,000** |

### 8.4 Adjusted Budget Fit

| Category | Original | AI-Optimized | Budget Target |
|----------|----------|--------------|---------------|
| Development | $4,860,000 | $3,400,000 | $4,320,000 |
| PM/QA | $1,260,000 | $1,100,000 | $1,260,000 |
| Infrastructure | $360,000 | $360,000 | $360,000 |
| Training | $540,000 | $450,000 | $540,000 |
| Post-Impl (3 years) | $1,800,000 | $1,800,000 | $1,800,000 |
| Contingency | $720,000 | $720,000 | $720,000 |
| **Total** | **$9,540,000** | **$7,830,000** | **$9,000,000** |

---

## 9. Assumptions and Constraints

### 9.1 Key Assumptions

| ID | Assumption | Impact if Invalid |
|----|------------|-------------------|
| **A1** | OSPI Product Owner available 20+ hours/week | Delayed decisions, scope creep |
| **A2** | Stable Azure environment and pricing | Infrastructure cost overrun |
| **A3** | Legacy SAFS remains operational during transition | Parallel operations not possible |
| **A4** | 243 requirements are complete and accurate | Scope changes, rework |
| **A5** | Districts/ESDs participate in UAT as scheduled | Extended testing phase |
| **A6** | No major legislative changes to apportionment formulas | Formula redesign required |
| **A7** | Existing integration APIs (CEDARS, iGrants) remain stable | Integration rework |
| **A8** | AI tools (Copilot, Claude) available throughout project | Productivity reduction |
| **A9** | Team can be recruited within 60 days of contract | Delayed start |
| **A10** | Remote work acceptable (hybrid team) | Higher travel/facility costs |

### 9.2 Constraints

| ID | Constraint | Mitigation |
|----|------------|------------|
| **C1** | $9M fixed budget | Scope prioritization, AI optimization |
| **C2** | June 30, 2028 hard deadline | Aggressive timeline, parallel workstreams |
| **C3** | Must integrate with 18+ OSPI systems | Incremental integration, fallback options |
| **C4** | ADA compliance mandatory | Built-in from Phase 1, expert audits |
| **C5** | WaTech approval required | Early engagement, compliance tracking |
| **C6** | Data sovereignty (Washington State) | Azure Government region only |

### 9.3 Dependencies

| ID | Dependency | Owner | Due Date |
|----|------------|-------|----------|
| **D1** | API specifications for CEDARS, iGrants | OSPI IT | Sep 2026 |
| **D2** | OneWA integration requirements | OneWA Team | Nov 2026 |
| **D3** | Entra ID tenant configuration | OSPI IT | Aug 2026 |
| **D4** | Historical data export from legacy SAFS | OSPI/Contractor | Oct 2026 |
| **D5** | UAT participant commitment | OSPI Districts | Feb 2027 |
| **D6** | WaTech architecture approval | WaTech | Aug 2026 |

---

## 10. Appendices

### Appendix A: T-Shirt Size Distribution by Complexity

| Complexity | XS | S | M | L | XL | Total |
|------------|----|----|----|----|-----|-------|
| **High** | 0 | 2 | 5 | 12 | 12 | 31 |
| **Medium** | 15 | 25 | 45 | 35 | 9 | 129 |
| **Low** | 30 | 28 | 18 | 7 | 0 | 83 |
| **Total** | **45** | **55** | **68** | **54** | **21** | **243** |

### Appendix B: Requirement Distribution by Work Section and Priority

| Work Section | High Priority | Medium Priority | Low Priority | Total |
|--------------|---------------|-----------------|--------------|-------|
| Data Collection | 2 | 75 | 5 | 82 |
| Data Calculations | 2 | 28 | 2 | 32 |
| Data Reporting | 1 | 28 | 2 | 31 |
| Technical | 2 | 53 | 3 | 58 |
| Sys All (Cross-Cutting) | 1 | 38 | 1 | 40 |
| **Total** | **8** | **222** | **13** | **243** |

### Appendix C: Sprint Planning Template

```markdown
## Sprint [N] Planning

**Sprint Goal:** [Clear, achievable goal]
**Duration:** 2 weeks ([Start Date] - [End Date])
**Velocity Target:** [X] story points

### Committed Work Items
| ID | Description | T-Shirt | Points | Assignee |
|----|-------------|---------|--------|----------|
| REQ-XXX | [Title] | M | 8 | [Name] |

### Sprint Risks
- [Risk 1]: [Mitigation]

### Definition of Done
- [ ] Code complete and reviewed
- [ ] Unit tests passing (>80% coverage)
- [ ] Integration tests passing
- [ ] Accessibility checks passing
- [ ] Documentation updated
- [ ] Product Owner acceptance
```

### Appendix D: Cost Rate Card

| Role | Hourly Rate | Daily Rate | Monthly Rate |
|------|-------------|------------|--------------|
| Technical Lead/Architect | $200 | $1,600 | $32,000 |
| Database Architect | $180 | $1,440 | $28,800 |
| Senior Full-Stack Developer | $175 | $1,400 | $28,000 |
| Project Manager | $175 | $1,400 | $28,000 |
| DevOps Engineer | $160 | $1,280 | $25,600 |
| UI/UX Designer | $150 | $1,200 | $24,000 |
| OCM Specialist | $150 | $1,200 | $24,000 |
| Mid-Level Developer | $140 | $1,120 | $22,400 |
| QA Lead | $140 | $1,120 | $22,400 |
| Business Analyst | $130 | $1,040 | $20,800 |
| QA Engineer | $110 | $880 | $17,600 |
| Junior Developer | $100 | $800 | $16,000 |
| Technical Writer | $100 | $800 | $16,000 |

*Rates are fully burdened (salary + benefits + overhead + margin)*

### Appendix E: Work Section Requirements Summary

#### Section 1: Data Collection (82 Requirements)

| Subsystem | Requirements | Complexity Distribution |
|-----------|--------------|------------------------|
| Enrollment Reporting | 14 | 3 High, 8 Medium, 3 Low |
| Personnel Reporting | 10 | 4 High, 5 Medium, 1 Low |
| Budgeting Forms | 7 | 2 High, 4 Medium, 1 Low |
| Financial Expenditures | 7 | 3 High, 3 Medium, 1 Low |
| Core Collection Platform | 44 | 8 High, 22 Medium, 14 Low |

#### Section 2: Data Calculations (32 Requirements)

| Subsystem | Requirements | Complexity Distribution |
|-----------|--------------|------------------------|
| Apportionment Engine | 6 | 4 High, 2 Medium, 0 Low |
| Budget Calculations | 4 | 1 High, 2 Medium, 1 Low |
| Formula Management | 6 | 3 High, 3 Medium, 0 Low |
| Data Integration Calcs | 8 | 5 High, 3 Medium, 0 Low |
| Compliance Calculations | 8 | 3 High, 4 Medium, 1 Low |

#### Section 3: Data Reporting (31 Requirements)

| Subsystem | Requirements | Complexity Distribution |
|-----------|--------------|------------------------|
| Report Generation Engine | 6 | 1 High, 3 Medium, 2 Low |
| Financial Reporting | 6 | 3 High, 2 Medium, 1 Low |
| Personnel Reporting | 7 | 3 High, 3 Medium, 1 Low |
| Integration Reporting | 4 | 2 High, 2 Medium, 0 Low |
| Public/Legislative | 8 | 2 High, 4 Medium, 2 Low |

---

## Document Control

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-01-19 | Proposal Team | Initial draft |

---

*This document is confidential and intended for use in responding to OSPI RFP 2026-12. All estimates are subject to refinement during contract negotiation and detailed requirements analysis.*
