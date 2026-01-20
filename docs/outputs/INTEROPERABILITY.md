# SASQUATCH Interoperability Specification

**SASQUATCH** - School Apportionment System for Quality, Accountability, Transparency, and Calculations Hub

**Document Version:** 1.0
**Last Updated:** 2026-01-19
**Classification:** Technical Specification

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Integration Architecture](#2-integration-architecture)
3. [API Specifications](#3-api-specifications)
4. [Data Exchange Standards](#4-data-exchange-standards)
5. [Authentication and Authorization](#5-authentication-and-authorization)
6. [Integration Patterns](#6-integration-patterns)
7. [Error Handling](#7-error-handling)
8. [Monitoring and Logging](#8-monitoring-and-logging)
9. [Appendices](#9-appendices)

---

## 1. Executive Summary

### 1.1 Purpose

This document defines the interoperability requirements, standards, and specifications for the SASQUATCH system, which replaces the legacy School Apportionment Financial System (SAFS). SASQUATCH serves as the central hub for Washington State's K-12 education funding calculations and distributions, processing over **$27.3 billion annually** to **380 educational entities**.

### 1.2 Scope

The interoperability specification covers:

- Integration with **18 internal OSPI systems**
- Data exchange with **external state agencies**
- Connectivity for **295 school districts** and **9 Educational Service Districts (ESDs)**
- Public-facing reporting interfaces
- Legislative and auditor access requirements

### 1.3 Key Stakeholders

| Stakeholder | Count | Primary Functions |
|-------------|-------|-------------------|
| School Districts | 295 | Data entry, budget submission, enrollment reporting |
| Educational Service Districts (ESDs) | 9 | Review, approval, multi-district support |
| OSPI Staff | ~50 | Administration, calculations, reporting |
| Third-Party Vendors | ~100+ | Fiscal management services for districts |
| Legislature | Variable | Scenario modeling, policy analysis |
| State Auditor's Office | 1 | Financial auditing, compliance |
| Public | Unlimited | Report viewing, transparency access |

### 1.4 Design Principles

1. **Minimal Disruption**: Maintain current import and data collection methods where practical
2. **Standards-Based**: Use industry-standard protocols and formats
3. **Security-First**: Implement defense-in-depth security architecture
4. **Audit Trail**: Complete traceability for all data exchanges
5. **Scalability**: Support peak processing periods (monthly apportionment cycles)
6. **Resilience**: Graceful degradation and recovery capabilities

---

## 2. Integration Architecture

### 2.1 High-Level Architecture

```
+------------------------------------------------------------------+
|                        SASQUATCH CORE                             |
|  +--------------------+  +------------------+  +----------------+ |
|  | Data Collection    |  | Calculations     |  | Reporting      | |
|  | & Review           |  | & Estimations    |  | Engine         | |
|  +--------------------+  +------------------+  +----------------+ |
|                              |                                    |
|  +----------------------------------------------------------+    |
|  |              Integration Services Layer                   |    |
|  |  +----------+  +----------+  +----------+  +----------+  |    |
|  |  | API      |  | MFT/SFTP |  | Event    |  | Batch    |  |    |
|  |  | Gateway  |  | Services |  | Bus      |  | Processor|  |    |
|  |  +----------+  +----------+  +----------+  +----------+  |    |
|  +----------------------------------------------------------+    |
+------------------------------------------------------------------+
         |              |              |              |
    +----+----+    +----+----+    +----+----+    +----+----+
    | Internal |    | External |    | District |    | Public  |
    | OSPI     |    | State    |    | Systems  |    | Access  |
    | Systems  |    | Agencies |    |          |    |         |
    +----------+    +----------+    +----------+    +---------+
```

### 2.2 Internal OSPI System Integrations

SASQUATCH integrates with 18 internal OSPI systems organized by functional domain:

#### 2.2.1 Enrollment and Student Data Systems

| System | Description | Data Flow | Integration Method |
|--------|-------------|-----------|-------------------|
| **CEDARS** | Comprehensive Education Data and Research System | Inbound | REST API, Batch |
| **P-223** | Enrollment Reporting | Bidirectional | Native Integration |
| **P-223H** | Enrollment Reporting History | Inbound | Batch |
| **P-223S** | Summer Enrollment | Bidirectional | Native Integration |
| **ALE** | Alternative Learning Experience | Bidirectional | API, File Upload |

#### 2.2.2 Financial and Budget Systems

| System | Description | Data Flow | Integration Method |
|--------|-------------|-----------|-------------------|
| **EDS** | Education Data System | Bidirectional | REST API, MFT |
| **F-195** | Budgeting and Accounting System | Bidirectional | Native Integration |
| **F-196** | Annual Financial Statement | Bidirectional | Native Integration |
| **F-197** | Cash File Report | Bidirectional | Native Integration |
| **F-200** | Budget Extension Statement | Bidirectional | Native Integration |
| **F-203** | Estimate for State Revenues | Bidirectional | Native Integration |
| **Apportionment** | Payment Calculations | Outbound | Native Integration |

#### 2.2.3 Program-Specific Systems

| System | Description | Data Flow | Integration Method |
|--------|-------------|-----------|-------------------|
| **iGrants** | Grant Management System | Inbound | REST API, MFT |
| **Grants Claims** | Grant Claims Processing | Inbound | REST API |
| **Transportation (STARS)** | Student Transportation Reporting | Inbound | MFT, Batch |
| **Child Nutrition Services (CNS)** | Food Program Data | Inbound | REST API |
| **Open Doors** | Youth Reengagement Program | Inbound | REST API |
| **Skill Center** | Vocational Skills Centers | Inbound | REST API |

#### 2.2.4 Staff and Personnel Systems

| System | Description | Data Flow | Integration Method |
|--------|-------------|-----------|-------------------|
| **S-275** | Personnel Reporting | Bidirectional | Native Integration |
| **eCertification** | Teacher Certification | Inbound | REST API |
| **EMS** | Employee Management System | Inbound | REST API |

#### 2.2.5 Special Programs

| System | Description | Data Flow | Integration Method |
|--------|-------------|-----------|-------------------|
| **SPED** | Special Education | Inbound | REST API |
| **Highly Capable** | Gifted Programs | Inbound | REST API |
| **Migrant** | Migrant Education | Inbound | REST API |
| **LAP** | Learning Assistance Program | Inbound | REST API |
| **Bilingual/LEP** | English Learner Programs | Inbound | REST API |
| **Voc Ed** | Vocational Education | Inbound | REST API |

### 2.3 External Agency Integrations

#### 2.3.1 State Auditor's Office (SAO)

| Interface | Purpose | Format | Frequency |
|-----------|---------|--------|-----------|
| Financial Extract | Year-end financial data | PDF, Data Extract | Annual |
| F-196 Reports | Expenditure reporting | PDF | Annual |
| F-197 Extract | Cash file data | Data Extract | Annual |
| Audit Support | On-demand queries | Secure Portal | As needed |

#### 2.3.2 Budget Office / AFRS Integration

| Interface | Purpose | Format | Frequency |
|-----------|---------|--------|-----------|
| Revenue Code Crosswalk | Map revenue to budget codes | CSV, API | Monthly |
| Apportionment Extract | Payment data for processing | Fixed-length, XML | Monthly |
| STAT Memo | Monthly payment summary | Document | Monthly |
| Year-End Report | Budget reconciliation | Report | Annual |

#### 2.3.3 Third-Party Vendor Systems

| Interface | Purpose | Format | Frequency |
|-----------|---------|--------|-----------|
| Budget Import | Third-party budget data | CSV, XLS, API | On-demand |
| Data Export | Financial data for vendors | CSV, XLS | On-demand |
| Read-Only Access | View district data | Secure Portal | Real-time |

### 2.4 District and ESD Connectivity

#### 2.4.1 Data Submission Channels

```
District/ESD Users
       |
       +---> Web Portal (Primary)
       |         |
       |         +---> Interactive Forms
       |         +---> File Upload (CSV, XLS, XML)
       |         +---> Direct Entry
       |
       +---> SFTP/MFT (Bulk)
       |         |
       |         +---> Fixed-length files
       |         +---> Comma-delimited files
       |
       +---> REST API (Automated)
                 |
                 +---> JSON payloads
                 +---> Programmatic access
```

#### 2.4.2 Multi-District Support

The system supports consolidated operations for:

- ESDs managing multiple districts
- Third-party vendors serving multiple clients
- Bulk submission capabilities (multiple districts per file)

---

## 3. API Specifications

### 3.1 API Design Standards

#### 3.1.1 General Requirements

- **Protocol**: HTTPS (TLS 1.3 minimum)
- **Style**: RESTful architecture
- **Format**: JSON (primary), XML (legacy support)
- **Versioning**: URI-based (e.g., `/api/v1/`)
- **Documentation**: OpenAPI 3.0 specification

#### 3.1.2 Base URL Structure

```
Production:  https://api.sasquatch.k12.wa.us/v1
Staging:     https://api-stage.sasquatch.k12.wa.us/v1
Sandbox:     https://api-sandbox.sasquatch.k12.wa.us/v1
```

### 3.2 Core API Endpoints

#### 3.2.1 District Management

```http
# List all districts
GET /api/v1/districts

# Get district details
GET /api/v1/districts/{districtId}

# Get district by ESD
GET /api/v1/esds/{esdId}/districts

# Get district submission status
GET /api/v1/districts/{districtId}/submissions/status
```

#### 3.2.2 Enrollment Data

```http
# Submit enrollment data (P-223)
POST /api/v1/districts/{districtId}/enrollment

# Get enrollment for period
GET /api/v1/districts/{districtId}/enrollment/{schoolYear}/{month}

# Submit enrollment revision
PUT /api/v1/districts/{districtId}/enrollment/{submissionId}

# Get enrollment validation results
GET /api/v1/districts/{districtId}/enrollment/{submissionId}/validations

# Lock/unlock enrollment submissions (OSPI only)
POST /api/v1/enrollment/lock
DELETE /api/v1/enrollment/lock
```

#### 3.2.3 Budget and Financial Data

```http
# Submit budget (F-195)
POST /api/v1/districts/{districtId}/budgets

# Get budget by fiscal year
GET /api/v1/districts/{districtId}/budgets/{fiscalYear}

# Submit budget extension (F-200)
POST /api/v1/districts/{districtId}/budgets/{fiscalYear}/extensions

# Submit revenue estimate (F-203)
POST /api/v1/districts/{districtId}/revenue-estimates

# Get financial statement (F-196)
GET /api/v1/districts/{districtId}/financial-statements/{fiscalYear}

# Submit financial statement revision
PUT /api/v1/districts/{districtId}/financial-statements/{fiscalYear}
```

#### 3.2.4 Personnel Data

```http
# Submit personnel data (S-275)
POST /api/v1/districts/{districtId}/personnel

# Get personnel summary
GET /api/v1/districts/{districtId}/personnel/{schoolYear}

# Get personnel by assignment type
GET /api/v1/districts/{districtId}/personnel/{schoolYear}/assignments

# Validate personnel data
POST /api/v1/districts/{districtId}/personnel/validate
```

#### 3.2.5 Apportionment and Calculations

```http
# Get apportionment for district
GET /api/v1/districts/{districtId}/apportionment/{fiscalYear}/{month}

# Get calculation details
GET /api/v1/calculations/{calculationId}

# Run sandbox calculation (authorized users)
POST /api/v1/sandbox/calculations

# Compare calculation scenarios
POST /api/v1/sandbox/calculations/compare
```

#### 3.2.6 Reports

```http
# List available reports
GET /api/v1/reports

# Generate report
POST /api/v1/reports/{reportType}

# Get report status
GET /api/v1/reports/jobs/{jobId}

# Download report
GET /api/v1/reports/jobs/{jobId}/download

# Get public reports
GET /api/v1/public/reports
```

#### 3.2.7 System Integration

```http
# Receive data from internal OSPI systems
POST /api/v1/integrations/{systemId}/ingest

# Send data to internal OSPI systems
POST /api/v1/integrations/{systemId}/export

# Get integration status
GET /api/v1/integrations/{systemId}/status

# Trigger sync with external system
POST /api/v1/integrations/{systemId}/sync
```

#### 3.2.8 Notifications

```http
# Get notifications for user
GET /api/v1/notifications

# Mark notification as read
PUT /api/v1/notifications/{notificationId}/read

# Subscribe to notification topic
POST /api/v1/notifications/subscriptions

# Get submission calendar
GET /api/v1/calendar/submissions
```

### 3.3 API Request/Response Examples

#### 3.3.1 Enrollment Submission Request

```json
POST /api/v1/districts/12345/enrollment
Content-Type: application/json
Authorization: Bearer {access_token}

{
  "schoolYear": "2026-27",
  "reportingMonth": "October",
  "submissionType": "Original",
  "schools": [
    {
      "schoolId": "4567",
      "enrollmentData": {
        "k12FTE": 450.5,
        "headcount": 475,
        "gradeLevel": {
          "K": 45,
          "1": 48,
          "2": 52,
          "3": 49,
          "4": 51,
          "5": 47,
          "6": 183
        },
        "specialPrograms": {
          "ale": 12.5,
          "skillCenter": 0,
          "openDoors": 3.0,
          "runningStart": 15.0
        }
      }
    }
  ],
  "certifications": {
    "submittedBy": "district.user@school.wa.us",
    "submissionDate": "2026-10-15T14:30:00Z",
    "attestation": true
  }
}
```

#### 3.3.2 Enrollment Submission Response

```json
{
  "submissionId": "ENR-2026-12345-10",
  "status": "RECEIVED",
  "validationStatus": "PENDING",
  "timestamp": "2026-10-15T14:30:05Z",
  "links": {
    "self": "/api/v1/districts/12345/enrollment/ENR-2026-12345-10",
    "validations": "/api/v1/districts/12345/enrollment/ENR-2026-12345-10/validations",
    "status": "/api/v1/districts/12345/submissions/status"
  }
}
```

#### 3.3.3 Validation Results Response

```json
{
  "submissionId": "ENR-2026-12345-10",
  "validationStatus": "WARNINGS",
  "validationResults": {
    "errors": [],
    "warnings": [
      {
        "code": "ENR-W-101",
        "severity": "WARNING",
        "field": "schools[0].enrollmentData.specialPrograms.ale",
        "message": "ALE FTE (12.5) differs from P-223 by more than 5%",
        "requiresComment": true
      }
    ],
    "info": [
      {
        "code": "ENR-I-001",
        "message": "Enrollment increased 3.2% from prior month"
      }
    ]
  },
  "canSubmit": true,
  "requiredActions": [
    {
      "type": "ADD_COMMENT",
      "field": "schools[0].enrollmentData.specialPrograms.ale",
      "description": "Provide explanation for ALE variance"
    }
  ]
}
```

### 3.4 API Rate Limits and Quotas

| User Type | Requests/Minute | Requests/Hour | Requests/Day |
|-----------|-----------------|---------------|--------------|
| District User | 60 | 1,000 | 10,000 |
| ESD User | 120 | 2,000 | 20,000 |
| OSPI Staff | 300 | 10,000 | 100,000 |
| System Integration | 600 | 20,000 | 200,000 |
| Public (Unauthenticated) | 10 | 100 | 1,000 |

### 3.5 Webhook Notifications

#### 3.5.1 Available Webhook Events

```json
{
  "events": [
    "submission.created",
    "submission.validated",
    "submission.approved",
    "submission.rejected",
    "submission.returned",
    "deadline.approaching",
    "deadline.passed",
    "system.lockPeriod.start",
    "system.lockPeriod.end",
    "calculation.completed",
    "report.generated"
  ]
}
```

#### 3.5.2 Webhook Payload Format

```json
{
  "eventId": "evt_abc123",
  "eventType": "submission.validated",
  "timestamp": "2026-10-15T14:35:00Z",
  "data": {
    "submissionId": "ENR-2026-12345-10",
    "districtId": "12345",
    "status": "VALIDATED_WITH_WARNINGS",
    "warningCount": 1
  },
  "links": {
    "resource": "/api/v1/districts/12345/enrollment/ENR-2026-12345-10"
  }
}
```

---

## 4. Data Exchange Standards

### 4.1 Supported File Formats

#### 4.1.1 Import Formats

| Format | Use Case | Validation | Max Size |
|--------|----------|------------|----------|
| **CSV** | Bulk data import | Schema validation | 100 MB |
| **Excel (.xlsx)** | User-friendly import | Template validation | 50 MB |
| **Excel (.xls)** | Legacy support | Template validation | 25 MB |
| **XML** | Structured data import | XSD validation | 100 MB |
| **Fixed-length** | Legacy system compat | Position validation | 100 MB |
| **JSON** | API submissions | JSON Schema | 10 MB |

#### 4.1.2 Export Formats

| Format | Use Case | Accessibility |
|--------|----------|---------------|
| **CSV** | Data analysis, spreadsheets | Universal |
| **Excel (.xlsx)** | Reporting, analysis | Standard |
| **PDF** | Official reports, ADA compliant | Standard |
| **XML** | System integration | Integration |
| **JSON** | API responses | Integration |

### 4.2 File Transfer Protocols

#### 4.2.1 SFTP/MFT Configuration

```yaml
SFTP Server Configuration:
  Host: sftp.sasquatch.k12.wa.us
  Port: 22
  Authentication: SSH Key (Required), Password (Optional 2FA)

Directory Structure:
  /inbound/{districtId}/
    /enrollment/
    /budget/
    /personnel/
    /financial/
  /outbound/{districtId}/
    /reports/
    /extracts/
    /confirmations/
  /archive/{districtId}/{year}/

File Naming Convention:
  {DistrictID}_{DataType}_{SchoolYear}_{Period}_{Timestamp}.{ext}
  Example: 12345_P223_2026-27_OCT_20261015143000.csv
```

#### 4.2.2 Managed File Transfer (MFT) Features

- Automatic file encryption (AES-256)
- Checksum validation (SHA-256)
- Retry logic (3 attempts with exponential backoff)
- Email notification on success/failure
- Scheduled pickup windows
- File quarantine for failed validations

### 4.3 Data Schemas

#### 4.3.1 Enrollment Data Schema (P-223)

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "title": "P223EnrollmentSubmission",
  "type": "object",
  "required": ["districtId", "schoolYear", "month", "schools"],
  "properties": {
    "districtId": {
      "type": "string",
      "pattern": "^[0-9]{5}$"
    },
    "schoolYear": {
      "type": "string",
      "pattern": "^[0-9]{4}-[0-9]{2}$"
    },
    "month": {
      "type": "string",
      "enum": ["September", "October", "November", "December",
               "January", "February", "March", "April", "May"]
    },
    "schools": {
      "type": "array",
      "items": {
        "$ref": "#/$defs/schoolEnrollment"
      }
    }
  },
  "$defs": {
    "schoolEnrollment": {
      "type": "object",
      "required": ["schoolId", "fte", "headcount"],
      "properties": {
        "schoolId": {"type": "string"},
        "fte": {"type": "number", "minimum": 0},
        "headcount": {"type": "integer", "minimum": 0},
        "gradeBreakdown": {
          "type": "object",
          "additionalProperties": {"type": "number"}
        },
        "programs": {
          "type": "object",
          "properties": {
            "ale": {"type": "number"},
            "skillCenter": {"type": "number"},
            "openDoors": {"type": "number"},
            "runningStart": {"type": "number"}
          }
        }
      }
    }
  }
}
```

#### 4.3.2 Budget Data Schema (F-195)

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "title": "F195BudgetSubmission",
  "type": "object",
  "required": ["districtId", "fiscalYear", "budgetType", "funds"],
  "properties": {
    "districtId": {"type": "string"},
    "fiscalYear": {"type": "string"},
    "budgetType": {
      "type": "string",
      "enum": ["ORIGINAL", "REVISION", "EXTENSION"]
    },
    "funds": {
      "type": "array",
      "items": {
        "$ref": "#/$defs/fundBudget"
      }
    }
  },
  "$defs": {
    "fundBudget": {
      "type": "object",
      "properties": {
        "fundCode": {"type": "string"},
        "fundName": {"type": "string"},
        "revenues": {
          "type": "array",
          "items": {"$ref": "#/$defs/lineItem"}
        },
        "expenditures": {
          "type": "array",
          "items": {"$ref": "#/$defs/lineItem"}
        },
        "beginningBalance": {"type": "number"},
        "endingBalance": {"type": "number"}
      }
    },
    "lineItem": {
      "type": "object",
      "properties": {
        "accountCode": {"type": "string"},
        "description": {"type": "string"},
        "amount": {"type": "number"}
      }
    }
  }
}
```

### 4.4 Code Crosswalks

#### 4.4.1 Revenue to Budget Code Mapping

The system maintains crosswalk tables for translating between:

- Apportionment revenue codes
- AFRS budget codes
- Program codes
- Activity codes
- Object codes

```sql
-- Example crosswalk structure
ApportionmentCode | AFRSCode | Description           | EffectiveDate | EndDate
------------------|----------|----------------------|---------------|--------
APR001           | 3100     | Basic Education      | 2020-07-01    | NULL
APR002           | 3121     | Special Ed Level 1   | 2020-07-01    | NULL
APR003           | 3141     | LEP/Bilingual        | 2020-07-01    | NULL
```

#### 4.4.2 District and School Codes

- 5-digit district codes (standardized statewide)
- 4-digit school codes (unique within district)
- ESD assignment mapping
- County code mapping

### 4.5 Data Validation Rules

#### 4.5.1 Validation Levels

| Level | Type | Behavior | User Action |
|-------|------|----------|-------------|
| **1** | Format | Blocks processing | Must correct before submission |
| **2** | Error | Blocks submission | Must correct before approval |
| **3** | Warning | Allows submission | Must provide explanation |
| **4** | Info | Informational | No action required |

#### 4.5.2 Cross-System Validations

```yaml
ALE-P223 Reconciliation:
  - ALE FTE must not exceed P-223 total FTE
  - ALE headcount categories must match P-223 breakdown
  - Discrepancy > 5% triggers warning

Personnel-Certification:
  - All certificated staff must have valid certification
  - Assignment codes must align with certification endorsements
  - FTE totals must reconcile across systems

Budget-Enrollment:
  - Revenue projections must align with enrollment projections
  - Staff ratios must be within statutory limits
  - Per-pupil calculations must use consistent headcounts
```

---

## 5. Authentication and Authorization

### 5.1 Identity Provider Integration

#### 5.1.1 Primary Authentication: Microsoft Entra ID

```yaml
Entra ID Configuration:
  Tenant: ospi.onmicrosoft.com
  Application ID: {registered-app-id}

Authentication Flows:
  - Authorization Code Flow (web applications)
  - Client Credentials Flow (system integrations)
  - Device Code Flow (CLI tools)

Token Configuration:
  Access Token Lifetime: 1 hour
  Refresh Token Lifetime: 24 hours
  ID Token Claims: email, name, groups, roles
```

#### 5.1.2 Federated Authentication for Districts

```yaml
Supported Identity Providers:
  - Microsoft Entra ID (primary)
  - Google Workspace
  - Clever
  - ClassLink

Federation Protocol: SAML 2.0, OIDC

District Onboarding:
  1. District IT registers federation configuration
  2. OSPI validates metadata
  3. User mapping configured (email domain matching)
  4. Test authentication verified
  5. Production access enabled
```

### 5.2 Authorization Model

#### 5.2.1 Role-Based Access Control (RBAC)

| Role | Scope | Permissions |
|------|-------|-------------|
| **District Data Entry** | Own District | Submit data, view own reports |
| **District Approver** | Own District | Submit, approve, view reports |
| **District Admin** | Own District | Full district access, user management |
| **ESD Reviewer** | Assigned Districts | Review, approve, return submissions |
| **ESD Admin** | Assigned Districts | Full ESD access, district oversight |
| **OSPI Reviewer** | Statewide | Review, approve, run calculations |
| **OSPI Admin** | Statewide | Full system access, configuration |
| **OSPI IT** | Statewide | System administration, integrations |
| **Vendor (Read)** | Assigned Districts | View data only |
| **Vendor (Write)** | Assigned Districts | Submit on behalf of district |
| **Auditor** | Statewide | Read-only access to all data |
| **Legislature** | Statewide | Sandbox access, scenario modeling |
| **Public** | Public data | View published reports only |

#### 5.2.2 Permission Matrix

```yaml
Permissions:
  ENROLLMENT_SUBMIT: [DistrictDataEntry, DistrictApprover, DistrictAdmin, VendorWrite]
  ENROLLMENT_APPROVE: [DistrictApprover, DistrictAdmin, ESDReviewer, ESDAdmin, OSPIReviewer]
  ENROLLMENT_VIEW: [All authenticated roles]

  BUDGET_SUBMIT: [DistrictDataEntry, DistrictApprover, DistrictAdmin, VendorWrite]
  BUDGET_APPROVE: [DistrictApprover, ESDReviewer, OSPIReviewer, OSPIAdmin]

  CALCULATION_RUN: [OSPIReviewer, OSPIAdmin]
  CALCULATION_MODIFY: [OSPIAdmin]

  SANDBOX_ACCESS: [Legislature, OSPIAdmin]

  SYSTEM_LOCK: [OSPIAdmin, OSPIIT]
  USER_MANAGE: [DistrictAdmin, ESDAdmin, OSPIAdmin]
```

### 5.3 API Authentication

#### 5.3.1 OAuth 2.0 Token Request

```http
POST /oauth2/token
Content-Type: application/x-www-form-urlencoded

grant_type=client_credentials
&client_id={client_id}
&client_secret={client_secret}
&scope=https://api.sasquatch.k12.wa.us/.default
```

#### 5.3.2 API Request with Bearer Token

```http
GET /api/v1/districts/12345/enrollment
Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...
X-Request-ID: req_abc123
X-Correlation-ID: corr_xyz789
```

### 5.4 Service Account Management

#### 5.4.1 System Integration Accounts

```yaml
Account Types:
  - Internal OSPI System
  - External State Agency
  - District Integration System
  - Third-Party Vendor System

Account Provisioning:
  1. Request submitted via service desk
  2. Security review completed
  3. Credentials generated (certificate-based preferred)
  4. IP allowlist configured (if applicable)
  5. Rate limits assigned
  6. Audit logging enabled

Credential Rotation:
  - Secrets: 90 days mandatory rotation
  - Certificates: 1 year validity, 30-day renewal window
  - API Keys: Annual rotation, immediate revocation capability
```

### 5.5 Multi-Factor Authentication

```yaml
MFA Requirements:
  - Required for: OSPI Staff, ESD Admins, District Admins
  - Optional for: District Data Entry users
  - Not applicable: Service accounts, Public access

Supported Methods:
  - Microsoft Authenticator (push notification)
  - TOTP (any authenticator app)
  - FIDO2 security keys
  - SMS (backup only, not recommended)
```

---

## 6. Integration Patterns

### 6.1 Synchronous Request-Response

#### 6.1.1 Use Cases

- Real-time data validation
- Status checks
- User authentication
- Small data retrievals

#### 6.1.2 Implementation Pattern

```
Client                    API Gateway               Backend Service
   |                           |                           |
   |--- POST /validate ------->|                           |
   |                           |--- Validate Request ----->|
   |                           |<-- Validation Result -----|
   |<-- 200 OK + Results ------|                           |
```

### 6.2 Asynchronous Processing

#### 6.2.1 Use Cases

- Large file uploads
- Report generation
- Batch calculations
- Cross-system synchronization

#### 6.2.2 Implementation Pattern

```
Client                    API Gateway               Queue           Worker
   |                           |                      |               |
   |--- POST /reports -------->|                      |               |
   |                           |--- Enqueue Job ----->|               |
   |<-- 202 Accepted + JobID --|                      |               |
   |                           |                      |<-- Dequeue ---|
   |                           |                      |               |
   |--- GET /jobs/{id} ------->|                      |--- Process -->|
   |<-- 200 IN_PROGRESS -------|                      |               |
   |                           |                      |<-- Complete --|
   |--- GET /jobs/{id} ------->|                      |               |
   |<-- 200 COMPLETED + URL ---|                      |               |
```

### 6.3 Event-Driven Integration

#### 6.3.1 Event Bus Architecture

```
+------------------+     +------------------+     +------------------+
| Source System    |---->| Event Bus        |---->| Target System    |
| (e.g., CEDARS)   |     | (Azure Service   |     | (SASQUATCH)      |
|                  |     |  Bus)            |     |                  |
+------------------+     +------------------+     +------------------+
                               |
                               +---->+------------------+
                                     | Archive/Audit    |
                                     | (Event Store)    |
                                     +------------------+
```

#### 6.3.2 Event Schema

```json
{
  "eventId": "evt_123456",
  "eventType": "enrollment.updated",
  "source": "cedars",
  "timestamp": "2026-10-15T14:30:00Z",
  "version": "1.0",
  "data": {
    "districtId": "12345",
    "schoolYear": "2026-27",
    "changeType": "UPDATE",
    "affectedRecords": 150
  },
  "metadata": {
    "correlationId": "corr_xyz789",
    "causationId": "evt_123455"
  }
}
```

### 6.4 Batch Processing

#### 6.4.1 Batch Job Types

| Job Type | Schedule | Processing Window | SLA |
|----------|----------|-------------------|-----|
| Enrollment Aggregation | Daily 2:00 AM | 2 hours | 99.5% |
| Calculation Run | Monthly 1st-5th | 8 hours | 99.9% |
| Report Generation | Daily 4:00 AM | 3 hours | 99.5% |
| SAO Extract | Annual August | 24 hours | 100% |
| Data Sync | Every 15 min | 5 minutes | 99% |

#### 6.4.2 Batch Processing Flow

```yaml
Batch Job Lifecycle:
  1. Scheduled trigger or manual initiation
  2. Input data collection and staging
  3. Pre-processing validation
  4. Main processing (parallelized where possible)
  5. Post-processing validation
  6. Output generation and delivery
  7. Notification and logging
  8. Cleanup of temporary resources

Error Handling:
  - Checkpoint/restart capability
  - Partial success tracking
  - Failed record isolation
  - Automatic retry (configurable)
  - Manual intervention workflow
```

### 6.5 Data Synchronization Patterns

#### 6.5.1 Incremental Sync

```yaml
Incremental Sync Strategy:
  Change Detection:
    - Timestamp-based (LastModifiedDate)
    - Version number tracking
    - Change Data Capture (CDC) where available

  Sync Process:
    1. Query changes since last sync timestamp
    2. Transform to target format
    3. Apply changes (upsert)
    4. Update sync checkpoint
    5. Log sync statistics

  Conflict Resolution:
    - Last-write-wins (default)
    - Source-system-wins (configurable)
    - Manual resolution queue
```

#### 6.5.2 Full Refresh

```yaml
Full Refresh Strategy:
  Use Cases:
    - Initial data load
    - Recovery from sync failures
    - Periodic reconciliation (weekly)

  Process:
    1. Extract full dataset from source
    2. Stage in temporary tables
    3. Validate record counts and checksums
    4. Swap tables (blue-green deployment)
    5. Archive previous data
    6. Update sync metadata
```

---

## 7. Error Handling

### 7.1 Error Response Format

#### 7.1.1 Standard Error Response

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "The submission contains validation errors",
    "details": [
      {
        "field": "schools[0].enrollmentData.fte",
        "code": "INVALID_VALUE",
        "message": "FTE cannot be negative",
        "value": -5.0,
        "constraint": "minimum: 0"
      }
    ],
    "requestId": "req_abc123",
    "timestamp": "2026-10-15T14:30:00Z",
    "helpUrl": "https://docs.sasquatch.k12.wa.us/errors/VALIDATION_ERROR"
  }
}
```

### 7.2 Error Categories

#### 7.2.1 HTTP Status Codes

| Code | Category | Description | Client Action |
|------|----------|-------------|---------------|
| 400 | Bad Request | Invalid request format/data | Fix request and retry |
| 401 | Unauthorized | Authentication required/failed | Authenticate and retry |
| 403 | Forbidden | Insufficient permissions | Contact administrator |
| 404 | Not Found | Resource does not exist | Verify resource ID |
| 409 | Conflict | Resource state conflict | Resolve conflict, retry |
| 422 | Unprocessable | Validation failed | Fix data errors, retry |
| 429 | Too Many Requests | Rate limit exceeded | Wait and retry |
| 500 | Internal Error | Server error | Retry with backoff |
| 502 | Bad Gateway | Upstream service error | Retry with backoff |
| 503 | Service Unavailable | Maintenance/overload | Retry later |
| 504 | Gateway Timeout | Upstream timeout | Retry with longer timeout |

### 7.3 Error Codes Reference

#### 7.3.1 Authentication Errors (AUTH-xxx)

| Code | Description | Resolution |
|------|-------------|------------|
| AUTH-001 | Token expired | Refresh token or re-authenticate |
| AUTH-002 | Invalid token | Obtain new token |
| AUTH-003 | Token signature invalid | Use correct signing key |
| AUTH-004 | MFA required | Complete MFA challenge |
| AUTH-005 | Session expired | Log in again |

#### 7.3.2 Validation Errors (VAL-xxx)

| Code | Description | Resolution |
|------|-------------|------------|
| VAL-001 | Required field missing | Provide required field |
| VAL-002 | Invalid format | Correct field format |
| VAL-003 | Value out of range | Use valid range |
| VAL-004 | Duplicate record | Remove duplicate |
| VAL-005 | Reference not found | Use valid reference |
| VAL-006 | Cross-field validation failed | Correct related fields |

#### 7.3.3 Business Rule Errors (BUS-xxx)

| Code | Description | Resolution |
|------|-------------|------------|
| BUS-001 | Submission window closed | Wait for next window |
| BUS-002 | System locked | Wait for unlock |
| BUS-003 | Approval required | Submit for approval |
| BUS-004 | Prerequisite not met | Complete prerequisite |
| BUS-005 | Limit exceeded | Reduce to within limits |

#### 7.3.4 Integration Errors (INT-xxx)

| Code | Description | Resolution |
|------|-------------|------------|
| INT-001 | External system unavailable | Retry later |
| INT-002 | Data sync failed | Check sync status |
| INT-003 | File transfer failed | Retry transfer |
| INT-004 | Format conversion failed | Check file format |
| INT-005 | Checksum mismatch | Re-upload file |

### 7.4 Retry Strategies

#### 7.4.1 Exponential Backoff

```python
retry_config = {
    "max_retries": 5,
    "initial_delay_ms": 1000,
    "max_delay_ms": 60000,
    "backoff_multiplier": 2.0,
    "jitter": True,
    "retryable_codes": [429, 500, 502, 503, 504]
}

# Delay calculation
delay = min(initial_delay * (multiplier ^ attempt), max_delay)
delay = delay + random(0, delay * 0.1)  # 10% jitter
```

#### 7.4.2 Circuit Breaker

```yaml
Circuit Breaker Configuration:
  Failure Threshold: 5 failures in 60 seconds
  Open Duration: 30 seconds
  Half-Open Requests: 3
  Success Threshold: 2 consecutive successes

States:
  CLOSED: Normal operation, requests pass through
  OPEN: All requests fail fast, no external calls
  HALF_OPEN: Limited requests to test recovery
```

### 7.5 Error Notifications

#### 7.5.1 User Notifications

```yaml
Notification Channels:
  - In-app notification (always)
  - Email (configurable)
  - Dashboard alert (for admins)

Notification Content:
  - Error summary
  - Affected submission/resource
  - Suggested resolution
  - Help documentation link
  - Support contact information
```

#### 7.5.2 System Alerts

```yaml
Alert Severity Levels:
  CRITICAL: Immediate response required (page on-call)
  HIGH: Response within 1 hour
  MEDIUM: Response within 4 hours
  LOW: Response within 24 hours

Alert Routing:
  - CRITICAL: PagerDuty + Email + Slack
  - HIGH: Email + Slack
  - MEDIUM: Slack + Dashboard
  - LOW: Dashboard only
```

---

## 8. Monitoring and Logging

### 8.1 Observability Architecture

```
+------------------+     +------------------+     +------------------+
| Application      |---->| Telemetry        |---->| Azure Monitor    |
| (SASQUATCH)      |     | Collector        |     | / App Insights   |
+------------------+     +------------------+     +------------------+
       |                                                   |
       |                                                   v
       |                                          +------------------+
       |                                          | Dashboards       |
       |                                          | & Alerts         |
       |                                          +------------------+
       |
       +---->+------------------+     +------------------+
             | Audit Log        |---->| Log Analytics    |
             | Collector        |     | Workspace        |
             +------------------+     +------------------+
                                              |
                                              v
                                     +------------------+
                                     | Compliance       |
                                     | Reporting        |
                                     +------------------+
```

### 8.2 Logging Standards

#### 8.2.1 Log Format

```json
{
  "timestamp": "2026-10-15T14:30:00.123Z",
  "level": "INFO",
  "service": "sasquatch-api",
  "instance": "api-prod-001",
  "correlationId": "corr_xyz789",
  "requestId": "req_abc123",
  "userId": "user@district.wa.us",
  "districtId": "12345",
  "action": "enrollment.submit",
  "resource": "/api/v1/districts/12345/enrollment",
  "method": "POST",
  "statusCode": 202,
  "durationMs": 145,
  "message": "Enrollment submission accepted",
  "metadata": {
    "schoolYear": "2026-27",
    "month": "October",
    "recordCount": 15
  }
}
```

#### 8.2.2 Log Levels

| Level | Description | Retention | Examples |
|-------|-------------|-----------|----------|
| DEBUG | Detailed debugging | 7 days | Variable values, loop iterations |
| INFO | Normal operations | 30 days | Request processing, completions |
| WARN | Potential issues | 90 days | Retries, degraded performance |
| ERROR | Failures | 1 year | Exceptions, validation failures |
| FATAL | Critical failures | 7 years | System crashes, data loss |
| AUDIT | Security events | 7 years | Logins, data access, changes |

### 8.3 Metrics and KPIs

#### 8.3.1 System Health Metrics

| Metric | Description | Target | Alert Threshold |
|--------|-------------|--------|-----------------|
| API Availability | Uptime percentage | 99.9% | < 99.5% |
| API Latency (P50) | Median response time | < 200ms | > 500ms |
| API Latency (P99) | 99th percentile | < 2s | > 5s |
| Error Rate | Failed requests | < 0.1% | > 1% |
| Throughput | Requests per second | Variable | Anomaly detection |

#### 8.3.2 Business Metrics

| Metric | Description | Target | Frequency |
|--------|-------------|--------|-----------|
| Submission Rate | Districts submitted | 100% by deadline | Daily during window |
| Validation Pass Rate | First-time pass | > 90% | Daily |
| Calculation Accuracy | Error-free calculations | 100% | Monthly |
| Report Generation Time | Time to generate | < 5 min | Per request |
| User Satisfaction | NPS score | > 70 | Quarterly |

#### 8.3.3 Integration Metrics

| Metric | Description | Target | Alert |
|--------|-------------|--------|-------|
| Sync Latency | Time for data to propagate | < 15 min | > 1 hour |
| Sync Success Rate | Successful syncs | > 99.9% | < 99% |
| Queue Depth | Messages waiting | < 1000 | > 10000 |
| File Transfer Success | Successful transfers | > 99.9% | < 99% |

### 8.4 Audit Trail Requirements

#### 8.4.1 Auditable Events

```yaml
Data Modification Events:
  - Record created
  - Record updated (before/after values)
  - Record deleted
  - Submission status changed
  - Approval granted/denied
  - Calculation run
  - Report generated

Security Events:
  - User login (success/failure)
  - Password change
  - Role assignment
  - Permission change
  - API key created/revoked
  - Session terminated

System Events:
  - Configuration change
  - System lock/unlock
  - Scheduled job execution
  - Integration sync
  - Error occurrence
```

#### 8.4.2 Audit Log Retention

| Category | Retention Period | Storage Tier |
|----------|------------------|--------------|
| Security events | 7 years | Hot (1 year) + Archive |
| Data modifications | 7 years | Hot (1 year) + Archive |
| Access logs | 2 years | Hot (90 days) + Archive |
| System events | 1 year | Hot (30 days) + Archive |
| Debug logs | 30 days | Hot only |

### 8.5 Dashboards and Reporting

#### 8.5.1 Operations Dashboard

```yaml
Dashboard Panels:
  - System Health Overview
    - Service status (green/yellow/red)
    - Current error rate
    - Active users
    - Request throughput

  - API Performance
    - Latency trends (P50, P95, P99)
    - Error rate by endpoint
    - Top slowest endpoints

  - Integration Status
    - Sync status by system
    - Queue depths
    - Failed transfers

  - Submission Status
    - Districts submitted today
    - Pending validations
    - Approval backlog
```

#### 8.5.2 Executive Dashboard

```yaml
Dashboard Panels:
  - Submission Compliance
    - % districts submitted by deadline
    - Outstanding submissions
    - ESD summary

  - Data Quality
    - Validation error trends
    - Common error types
    - Improvement metrics

  - System Performance
    - Availability this month
    - Major incidents
    - User satisfaction scores
```

### 8.6 Alerting Strategy

#### 8.6.1 Alert Definitions

```yaml
Critical Alerts:
  - name: "API Down"
    condition: availability < 99% for 5 minutes
    action: Page on-call, escalate after 15 minutes

  - name: "Database Connection Failure"
    condition: connection_errors > 10 in 1 minute
    action: Page on-call, auto-failover triggered

  - name: "Security Breach Detected"
    condition: anomalous_access_pattern detected
    action: Page security team, block suspicious IP

High Priority Alerts:
  - name: "High Error Rate"
    condition: error_rate > 5% for 5 minutes
    action: Email + Slack notification

  - name: "Integration Sync Failed"
    condition: sync_failures > 3 consecutive
    action: Email integration team

  - name: "Submission Deadline Approaching"
    condition: outstanding_submissions > 10 AND deadline < 24 hours
    action: Email SAFS team
```

---

## 9. Appendices

### 9.1 Glossary

| Term | Definition |
|------|------------|
| **ALE** | Alternative Learning Experience - Non-traditional education programs |
| **AFRS** | Agency Financial Reporting System - State budget system |
| **Apportionment** | The process of calculating and distributing state education funding |
| **CEDARS** | Comprehensive Education Data and Research System |
| **EDS** | Education Data System - OSPI's data platform |
| **ESD** | Educational Service District - Regional education agency |
| **FTE** | Full-Time Equivalent - Standard measure of student enrollment |
| **MFT** | Managed File Transfer - Secure file exchange protocol |
| **OSPI** | Office of Superintendent of Public Instruction |
| **P-223** | Primary enrollment reporting form |
| **S-275** | Personnel reporting form |
| **SAO** | State Auditor's Office |
| **SAFS** | School Apportionment Financial System (legacy) |
| **SASQUATCH** | School Apportionment System for Quality, Accountability, Transparency, and Calculations Hub |
| **SFTP** | Secure File Transfer Protocol |

### 9.2 Reference Documents

| Document | Location | Purpose |
|----------|----------|---------|
| RFP 2026-12 | OSPI Contracts | Original procurement document |
| Attachment A | RFP Exhibit | System requirements |
| Attachment B | RFP Exhibit | As-is workflow documentation |
| Attachment C | RFP Exhibit | Demonstration scenarios |
| WaTech Strategic Plan | wa.gov | State IT alignment requirements |
| FERPA Guidelines | ed.gov | Student privacy requirements |
| GAAP Standards | fasb.org | Financial reporting requirements |

### 9.3 System Contact Information

| System | Owner | Contact | Support Hours |
|--------|-------|---------|---------------|
| SASQUATCH | OSPI IT | sasquatch-support@k12.wa.us | 24/7 for P1 |
| CEDARS | OSPI Data | cedars@k12.wa.us | Business hours |
| EDS | OSPI IT | eds-support@k12.wa.us | Business hours |
| iGrants | OSPI Grants | igrants@k12.wa.us | Business hours |

### 9.4 Change Log

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-01-19 | SASQUATCH Team | Initial specification |

---

*This document is maintained by the OSPI SASQUATCH project team. For questions or updates, contact sasquatch-support@k12.wa.us.*
