# Interoperability Requirements

This document defines all system integrations for SAFS, categorized by internal OSPI systems and external third-party systems.

**Primary Requirements:** `[INT-001]` through `[INT-006]`, `[SAFS-003]`, `[SAFS-079]`, `[SAFS-080]`, `[PRS-009]`, `[SAFS-114]`

---

## Overview

| Category | Definition | Examples |
|----------|------------|----------|
| **Internal Interoperability** | Systems within OSPI's control | EDS, eCertification, WINS, Highly Capable Program |
| **External Interoperability** | Third-party systems outside OSPI | Workday/OneWA, State Auditor's Office |

---

## 1. Internal OSPI Systems

### 1.1 Education Data System (EDS)

**Traces To:** `[SAFS-114]`, `[INT-001]`

**Current State:** S-275 personnel data is manually exported and shared with EDS.

**RFP Requirement:** `[SAFS-114]` - "Personnel Reporting (S-275) data is fed to the Education Data System (EDS)"

| Integration Point | Direction | Data | Frequency | Traces To |
|-------------------|-----------|------|-----------|-----------|
| Personnel Data Sync | SAFS → EDS | S-275 staff records | Real-time | `[SAFS-114]` |
| Enrollment Data | EDS → SAFS | Student enrollment for validation | Daily | `[INT-002]` |

**API Specification:**
```yaml
# EDS Integration API [SAFS-114], [INT-001]
POST /api/v1/eds/personnel/sync
  Request: StaffRecord[]
  Response: SyncResult { success: bool, errors: string[] }

GET /api/v1/eds/enrollment/{districtCode}
  Response: EnrollmentData[]
```

---

### 1.2 eCertification System

**Traces To:** `[PRS-009]`

**Current State:** Manual data reconciliation between S-275 and eCertification. Exception handling for mismatched SSN/name changes done manually.

**RFP Requirement:** `[PRS-009]` - "Data synchronization between S-275 and eCertification should be near real time"

| Integration Point | Direction | Data | Frequency | Traces To |
|-------------------|-----------|------|-----------|-----------|
| Certification Validation | SAFS → eCert | Certificate numbers for validation | Per submission | `[PRS-009]` |
| Certification Status | eCert → SAFS | Certificate status, expiration | Daily sync | `[PRS-009]` |
| Exception Dashboard | Bidirectional | Mismatched records | Real-time | `[PRS-009]` |

**API Specification:**
```yaml
# eCertification Integration API [PRS-009]
GET /api/v1/ecert/validate/{certificateNumber}
  Response: CertificationStatus { valid: bool, expirationDate: date, name: string }

POST /api/v1/ecert/exceptions
  Request: ExceptionRecord { certNumber, safsName, ecertName, issueType }
  Response: ExceptionId
```

---

### 1.3 WINS (Washington Integrated Network System)

**Traces To:** `[SAFS-079]`

**Current State:** District users must navigate to WINS separately for student data.

**RFP Requirement:** `[SAFS-079]` - "Links to other OSPI data systems used by districts"

| Integration Point | Direction | Data | Frequency | Traces To |
|-------------------|-----------|------|-----------|-----------|
| Student Data | WINS → SAFS | Student counts, demographics | On-demand | `[SAFS-079]` |
| Navigation | SAFS → WINS | SSO redirect | User-initiated | `[SAFS-079]` |

---

### 1.4 Highly Capable Program

**Traces To:** `[SAFS-079]`

**Current State:** Separate data source referenced by districts.

**RFP Requirement:** `[SAFS-079]` - "integrate with WINS and Highly Capable data sources"

| Integration Point | Direction | Data | Frequency | Traces To |
|-------------------|-----------|------|-----------|-----------|
| HC Enrollment | HC System → SAFS | Highly Capable student counts | Monthly | `[SAFS-079]` |

---

## 2. External Systems

### 2.1 Workday/OneWA (Washington Enterprise Cloud)

**Traces To:** `[SAFS-003]`

**Current State:** No integration. Payment data handled via manual Excel crosswalk and email to Budget Office.

**RFP Requirement:** `[SAFS-003]` - Per RFP, system must integrate with Washington's enterprise cloud platform for finance and payments.

| Integration Point | Direction | Data | Frequency | Traces To |
|-------------------|-----------|------|-----------|-----------|
| Payment File | SAFS → Workday | Apportionment payment file | Monthly | `[SAFS-003]`, `[SAFS-106]` |
| Budget Codes | Workday → SAFS | Valid budget code list | Quarterly | `[SAFS-003]` |
| Payment Confirmation | Workday → SAFS | Payment status | Per payment | `[SAFS-003]` |

**API Specification:**
```yaml
# Workday/OneWA Integration API [SAFS-003]
POST /api/v1/workday/payments
  Request: PaymentBatch { payments: PaymentRecord[] }
  Response: BatchResult { batchId, status, paymentIds[] }

GET /api/v1/workday/payments/{batchId}/status
  Response: PaymentBatchStatus { batchId, payments: PaymentStatus[] }

GET /api/v1/workday/budgetcodes
  Response: BudgetCode[] { code, description, effectiveDate, isActive }
```

---

### 2.2 State Auditor's Office (SAO)

**Traces To:** `[SAFS-042]`

**Current State:** Year-end PDF extract sent manually. IT staff generate and send files.

**RFP Requirement:** `[SAFS-042]` - SAO must receive financial data for audit purposes.

| Integration Point | Direction | Data | Frequency | Traces To |
|-------------------|-----------|------|-----------|-----------|
| Financial Extract | SAFS → SAO | F-196 data, F-197 year-end | Annual | `[SAFS-042]` |
| Audit Findings | SAO → SAFS | Audit adjustments | As needed | `[SAFS-005]` |

**API Specification:**
```yaml
# SAO Integration API [SAFS-042]
POST /api/v1/sao/financial-extract
  Request: FinancialExtract { fiscalYear, districts: DistrictFinancial[] }
  Response: SubmissionReceipt { receiptId, timestamp }

GET /api/v1/sao/audit-findings/{fiscalYear}
  Response: AuditFinding[] { districtCode, findingType, amount, status }
```

---

## 3. Data Exchange Methods

### 3.1 District Data Submission

**Traces To:** `[SAFS-030]`, `[TEC-183]`, `[TEC-184]`, `[TEC-188]`

**RFP Requirement:** `[SAFS-030]` - "District users must be able to submit data via API, fixed-length files, or comma-delimited files via SFTP"

| Method | Format | Use Case | Traces To |
|--------|--------|----------|-----------|
| Web Portal | JSON/Form | Small districts, manual entry | `[PRS-004]` |
| API | JSON | Large districts, automated systems | `[SAFS-030]`, `[TEC-184]` |
| SFTP | CSV, fixed-length | Third-party accounting systems | `[SAFS-030]`, `[TEC-188]` |
| File Upload | CSV, Excel | Ad-hoc submissions | `[TEC-183]` |

---

### 3.2 ESD Batch Upload

**Traces To:** `[SAFS-036]`

**RFP Requirement:** `[SAFS-036]` - "ESDs or vendors can upload data for multiple districts in a single exchange"

| Method | Format | Use Case | Traces To |
|--------|--------|----------|-----------|
| Batch API | JSON array | Programmatic multi-district submission | `[SAFS-036]` |
| Batch File | CSV | Single file with district identifiers | `[SAFS-036]` |

---

### 3.3 Internal OSPI Data

**Traces To:** `[SAFS-080]`

**RFP Requirement:** `[SAFS-080]` - "API or MFT to RECEIVE information from internal OSPI systems"

| Method | Systems | Use Case | Traces To |
|--------|---------|----------|-----------|
| API | EDS, eCert, WINS | Real-time data exchange | `[SAFS-080]` |
| MFT | Legacy systems | Scheduled file transfers | `[SAFS-080]` |

---

## 4. Demo Integration Approach

For the demo, the following integrations will be **mocked** with realistic data flows:

| System | Demo Approach | Traces To |
|--------|---------------|-----------|
| EDS | Mock API returning sample personnel data | `[SAFS-114]` |
| eCert | Mock validation service | `[PRS-009]` |
| Workday/OneWA | Mock payment submission and confirmation | `[SAFS-003]` |
| SAO | Mock extract generation | `[SAFS-042]` |

**Demo Script Integration Points:**
- Section 1: Show API-based data submission `[SAFS-030]`
- Section 2: Show Workday payment file generation `[SAFS-003]`
- Section 3: Show API documentation portal with integration examples `[TEC-185]`

---

## 5. Integration Requirements Summary

| Req ID | Requirement Summary | Specification |
|--------|---------------------|---------------|
| `INT-001` | Centralized relational database | Single Azure SQL database serving all three sections |
| `INT-002` | Automated data ingestion and transformation | Automated API data pull from source systems with validation |
| `INT-003` | Maintained lookup/reference tables | School codes, district codes, ESD codes maintained from EDS |
| `INT-004` | SQL-based stored procedures | Converting Access queries to Azure SQL stored procedures |
| `INT-005` | Schema stability, 10-20 years historical data | Archive tables for historical data (6-10+ years) |
| `INT-006` | Support forecasting and trend analytics | Native functionality to share data with external systems via API |

---

*Extracted from SASQUATCH_Demo_Specifications.md v3.0*
*Document Version: 1.0*
*Created: 2026-01-12*
