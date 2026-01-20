# SASQUATCH Security Requirements Specification

**Document Version:** 1.0
**Last Updated:** 2026-01-19
**Classification:** Internal Use Only
**System:** School Apportionment System for Quality, Accountability, Transparency, and Calculations Hub (SASQUATCH)
**Owner:** Washington State Office of Superintendent of Public Instruction (OSPI)

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Compliance Requirements](#2-compliance-requirements)
3. [Authentication Architecture](#3-authentication-architecture)
4. [Authorization Model (RBAC)](#4-authorization-model-rbac)
5. [Data Protection](#5-data-protection)
6. [Audit and Logging](#6-audit-and-logging)
7. [Network Security](#7-network-security)
8. [Incident Response](#8-incident-response)
9. [Security Testing Requirements](#9-security-testing-requirements)
10. [Appendices](#10-appendices)

---

## 1. Executive Summary

### 1.1 Purpose

This document establishes the comprehensive security requirements for the SASQUATCH system, which replaces the legacy School Apportionment Financial System (SAFS). SASQUATCH processes and distributes over **$27.3 billion annually** in state and federal education funding to Washington State's 380 educational entities, making security a paramount concern.

### 1.2 Scope

These requirements apply to:
- All SASQUATCH system components (Data Collection, Data Calculation, Data Reporting)
- All user interfaces (web applications, APIs, reporting tools)
- All data stores (production, backup, archive, sandbox environments)
- All integration points with internal OSPI systems and external partners
- All personnel with system access (internal staff, contractors, external users)

### 1.3 System Context

| Attribute | Value |
|-----------|-------|
| **Annual Funding Processed** | $27,307,000,000+ |
| **Educational Entities Served** | 380 (295 School Districts + 9 ESDs + Tribal Compacts) |
| **Data Classification** | Category 4 - Confidential information requiring special handling |
| **Hosting Environment** | OSPI Azure Cloud |
| **External Authentication** | Secure Access Washington (SAW) |
| **Internal Authentication** | Azure Active Directory |

### 1.4 Security Objectives

1. **Confidentiality**: Protect sensitive student, personnel, and financial data from unauthorized disclosure
2. **Integrity**: Ensure accuracy and completeness of apportionment calculations and financial distributions
3. **Availability**: Maintain system availability for critical funding calculation and distribution cycles
4. **Accountability**: Provide complete audit trails for all data modifications and access
5. **Compliance**: Meet all state and federal regulatory requirements

### 1.5 Risk Context

The SASQUATCH system presents elevated security risks due to:
- High-value financial transactions ($27.3B annually)
- Sensitive student data protected by FERPA
- Sensitive personnel data (S-275 staffing reports)
- Public transparency requirements balanced against data protection
- Multi-stakeholder access (Districts, ESDs, OSPI, Legislature, State Auditor, Public)

---

## 2. Compliance Requirements

### 2.1 Washington Technology Solutions (WaTech) Standards

SASQUATCH must comply with all applicable WaTech security policies and standards:

#### 2.1.1 Securing IT Assets (Standard No. SEC-04-03-S)

| Requirement ID | Requirement | Priority |
|----------------|-------------|----------|
| SEC-001 | Implement WaTech security controls as defined in Policy 141.10 | CRITICAL |
| SEC-002 | Password validation must meet OSPI/WaTech standards (Policy 141.10, Section 6.2) | CRITICAL |
| SEC-003 | Maintain disaster recovery and continuity of operations plans per WaTech Policy 151.10 | CRITICAL |
| SEC-004 | Perform regular security updates and patches to address vulnerabilities | HIGH |
| SEC-005 | Conduct annual security audits and vulnerability assessments | HIGH |

#### 2.1.2 Data Categorization Requirements

Per WaTech guidance (watech.wa.gov/categorizing-data-state-agency), SASQUATCH data is classified as:

| Category | Level | Description | Handling Requirements |
|----------|-------|-------------|----------------------|
| **Category 4** | Confidential | Information requiring special handling | Encryption required, access logging, background checks for some personnel |

### 2.2 Federal Compliance Requirements

#### 2.2.1 FERPA (Family Educational Rights and Privacy Act)

| Requirement ID | Requirement | Applicability |
|----------------|-------------|---------------|
| FERPA-001 | Protect personally identifiable information (PII) of students | All student enrollment data |
| FERPA-002 | Limit disclosure of education records to authorized parties only | Enrollment reporting, P-223 data |
| FERPA-003 | Provide parents/eligible students access to their records upon request | District-level implementation |
| FERPA-004 | Maintain audit trail of all disclosures of PII | All student data access |
| FERPA-005 | Implement appropriate safeguards for electronic records | System-wide |

#### 2.2.2 Other Federal Requirements

| Regulation | Applicability | Requirements |
|------------|---------------|--------------|
| **Section 504 / ADA** | All public-facing interfaces | WCAG 2.0 AA compliance minimum |
| **Title VI / Title IX** | Non-discrimination in data collection | Equal access to system functionality |
| **PCI DSS** | If payment card data is processed | Secure payment processing (if applicable) |

### 2.3 State Compliance Requirements

#### 2.3.1 Washington State Laws

| Law/Code | Requirement | System Impact |
|----------|-------------|---------------|
| **RCW 42.56** | Public Records Act compliance | Public reports, redaction of confidential data |
| **RCW 28A** | Common School Provisions | Funding calculation accuracy and transparency |
| **WAC 392** | Educational funding rules | Calculation engine compliance |
| **RCW 43.105** | State IT policies | WaTech alignment |

#### 2.3.2 Address Confidentiality Program

| Requirement ID | Requirement | Priority |
|----------------|-------------|----------|
| ACP-001 | Support identification of individuals enrolled in WA Secretary of State Address Confidentiality Program | HIGH |
| ACP-002 | Automatically redact SSNs, birthdates, and addresses for ACP participants | CRITICAL |
| ACP-003 | Exclude ACP participant records from public-facing reports and data exports | CRITICAL |

### 2.4 Accessibility Requirements

#### 2.4.1 WCAG 2.0 Compliance

| Requirement ID | Requirement | Level |
|----------------|-------------|-------|
| ACC-001 | All web interfaces must comply with WCAG 2.0 Level AA minimum | CRITICAL |
| ACC-002 | Reports must be generated in ADA-compliant formats (accessible PDFs or interactive web reports) | HIGH |
| ACC-003 | Color contrast, keyboard navigation, and screen reader compatibility required | HIGH |
| ACC-004 | Alternative text for all images and charts | MEDIUM |
| ACC-005 | Form labels and error messages must be programmatically associated | HIGH |

---

## 3. Authentication Architecture

### 3.1 Authentication Standards

#### 3.1.1 Protocol Requirements

| Requirement ID | Requirement | Standard |
|----------------|-------------|----------|
| AUTH-001 | Solution must be SAML 2.0 compliant | CRITICAL |
| AUTH-002 | External users authenticate via Secure Access Washington (SAW) | CRITICAL |
| AUTH-003 | Internal OSPI staff authenticate via Azure Active Directory | CRITICAL |
| AUTH-004 | Support for Multi-Factor Authentication (MFA) | HIGH |
| AUTH-005 | Session timeout after period of inactivity (configurable, default 30 minutes) | HIGH |

#### 3.1.2 Password Policy (Per WaTech Policy 141.10, Section 6.2)

| Attribute | Requirement |
|-----------|-------------|
| **Minimum Length** | 12 characters |
| **Complexity** | Must include uppercase, lowercase, numbers, and special characters |
| **History** | Cannot reuse last 12 passwords |
| **Maximum Age** | 90 days (or as updated by WaTech policy) |
| **Lockout Threshold** | 5 failed attempts |
| **Lockout Duration** | 30 minutes or administrator unlock |

### 3.2 Identity Providers

#### 3.2.1 External Users (SAW Integration)

```
User Types:
- District Data Entry Staff
- District Administrators
- ESD Reviewers
- Legislature Members (read-only sandbox)
- Public Users (report viewers)
- State Auditor's Office

Authentication Flow:
User -> SASQUATCH -> SAW -> Identity Verification -> SAML Assertion -> SASQUATCH Session
```

| Requirement ID | Requirement | Priority |
|----------------|-------------|----------|
| SAW-001 | Integrate with SAW for all external user authentication | CRITICAL |
| SAW-002 | Accept and validate SAW SAML assertions | CRITICAL |
| SAW-003 | Map SAW identity attributes to SASQUATCH user profiles | HIGH |
| SAW-004 | Handle SAW session timeout and re-authentication gracefully | MEDIUM |

#### 3.2.2 Internal Users (Azure AD Integration)

```
User Types:
- OSPI Data Analysts
- OSPI Administrators
- OSPI SAFS IT Staff
- System Administrators

Authentication Flow:
OSPI Staff -> SASQUATCH -> Azure AD -> MFA Challenge -> OAuth/OIDC Token -> SASQUATCH Session
```

| Requirement ID | Requirement | Priority |
|----------------|-------------|----------|
| AAD-001 | Integrate with OSPI Azure Active Directory for internal authentication | CRITICAL |
| AAD-002 | Enforce MFA for all administrative accounts | CRITICAL |
| AAD-003 | Support Conditional Access policies | HIGH |
| AAD-004 | Synchronize user attributes from Azure AD | HIGH |

### 3.3 Service Account Management

| Requirement ID | Requirement | Priority |
|----------------|-------------|----------|
| SVC-001 | All service accounts must use managed identities where possible | HIGH |
| SVC-002 | Service account credentials must be stored in Azure Key Vault | CRITICAL |
| SVC-003 | Service accounts must have minimum required privileges | CRITICAL |
| SVC-004 | Regular rotation of service account credentials (90 days) | HIGH |
| SVC-005 | Audit logging of all service account usage | HIGH |

### 3.4 API Authentication

| Requirement ID | Requirement | Priority |
|----------------|-------------|----------|
| API-001 | All API endpoints must require authentication | CRITICAL |
| API-002 | Support OAuth 2.0 for API authentication | HIGH |
| API-003 | API keys must be rotatable without service interruption | MEDIUM |
| API-004 | Rate limiting on all API endpoints | HIGH |
| API-005 | API authentication events must be logged | HIGH |

---

## 4. Authorization Model (RBAC)

### 4.1 Role Hierarchy

```
                    ┌─────────────────────┐
                    │ System Administrator│
                    │    (Full Access)    │
                    └──────────┬──────────┘
                               │
              ┌────────────────┼────────────────┐
              │                │                │
    ┌─────────▼─────────┐ ┌────▼────┐ ┌────────▼────────┐
    │ OSPI Administrator│ │OSPI Data│ │ State Auditor   │
    │(System Config)    │ │ Analyst │ │ (Read-Only All) │
    └─────────┬─────────┘ └────┬────┘ └─────────────────┘
              │                │
              │    ┌───────────┴───────────┐
              │    │                       │
    ┌─────────▼────▼──┐           ┌────────▼────────┐
    │  ESD Reviewer   │           │Legislature User │
    │ (Regional View) │           │(Sandbox Only)   │
    └────────┬────────┘           └─────────────────┘
             │
    ┌────────▼────────┐
    │District Admin   │
    │(District Scope) │
    └────────┬────────┘
             │
    ┌────────▼────────┐
    │District Staff   │
    │(Data Entry)     │
    └─────────────────┘
             │
    ┌────────▼────────┐
    │  Public User    │
    │(Reports Only)   │
    └─────────────────┘
```

### 4.2 Role Definitions

#### 4.2.1 System Administrator

| Attribute | Value |
|-----------|-------|
| **Scope** | System-wide |
| **Data Access** | All data (production, sandbox, archive) |
| **Functions** | User management, system configuration, security settings, audit review |
| **Restrictions** | Cannot modify calculation results directly; changes require audit trail |

**Permissions:**
- Manage user accounts and role assignments
- Configure system parameters and constants
- Access security logs and audit trails
- Manage integration settings
- Perform system maintenance operations

#### 4.2.2 OSPI Administrator

| Attribute | Value |
|-----------|-------|
| **Scope** | All districts/ESDs |
| **Data Access** | All apportionment data |
| **Functions** | Calculation management, data validation, formula configuration |
| **Restrictions** | Cannot modify user security settings |

**Permissions:**
- Modify calculation formulae (with approval workflow)
- Lock/unlock data collection periods
- Override validation errors (with justification)
- Approve/reject district submissions
- Configure state constants
- Manage data collection calendars

#### 4.2.3 OSPI Data Analyst

| Attribute | Value |
|-----------|-------|
| **Scope** | All districts/ESDs |
| **Data Access** | All apportionment data (read), sandbox (read/write) |
| **Functions** | Data analysis, report generation, projections |
| **Restrictions** | Cannot modify production calculations or approve submissions |

**Permissions:**
- View all district data and calculations
- Generate ad-hoc reports
- Create and run sandbox scenarios
- Export data for analysis
- View audit trails for data changes

#### 4.2.4 ESD Reviewer

| Attribute | Value |
|-----------|-------|
| **Scope** | Assigned ESD region only |
| **Data Access** | Districts within assigned ESD |
| **Functions** | Review and validate district submissions |
| **Restrictions** | Cannot access data from other ESDs |

**Permissions:**
- View submissions from districts in assigned ESD
- Run validation checks on district data
- Add review comments and recommendations
- Forward submissions to OSPI for final approval
- Generate regional reports

#### 4.2.5 District Administrator

| Attribute | Value |
|-----------|-------|
| **Scope** | Assigned district only |
| **Data Access** | Own district data only |
| **Functions** | Manage district users, submit data, certify submissions |
| **Restrictions** | Cannot access other districts' data |

**Permissions:**
- Manage district-level user accounts
- Review and certify data submissions
- Access district-specific reports
- Use projection sandbox with district data
- View district historical data

#### 4.2.6 District Data Entry Staff

| Attribute | Value |
|-----------|-------|
| **Scope** | Assigned district only |
| **Data Access** | Own district data, assigned forms only |
| **Functions** | Enter and update district data |
| **Restrictions** | Cannot certify submissions or access sensitive reports |

**Permissions:**
- Enter data for assigned forms (enrollment, personnel, budget, etc.)
- Run validation checks on entered data
- View validation errors and warnings
- Save draft submissions
- View own submission history

#### 4.2.7 Legislature User

| Attribute | Value |
|-----------|-------|
| **Scope** | Sandbox environment only |
| **Data Access** | Aggregated/anonymized data, scenario modeling |
| **Functions** | Compare funding scenarios, model policy changes |
| **Restrictions** | No access to production data, cannot see individual student/staff records |

**Permissions:**
- Access sandbox calculation environment
- Create and compare funding scenarios
- View aggregated statewide reports
- Export scenario comparison results

#### 4.2.8 State Auditor's Office (SAO)

| Attribute | Value |
|-----------|-------|
| **Scope** | System-wide read access |
| **Data Access** | All production data, audit trails, historical data |
| **Functions** | Audit review, data verification |
| **Restrictions** | Read-only access; cannot modify any data |

**Permissions:**
- View all district submissions and calculations
- Download data extracts in SAO-compatible formats
- Access complete audit trails
- View archived historical data
- Generate audit reports

#### 4.2.9 Public User

| Attribute | Value |
|-----------|-------|
| **Scope** | Published public reports only |
| **Data Access** | Aggregated public reports with confidential data redacted |
| **Functions** | View and download public reports |
| **Restrictions** | No access to detailed data, PII, or internal reports |

**Permissions:**
- View published public reports
- Filter and drill down within public data
- Download public reports

### 4.3 Authorization Controls

#### 4.3.1 Data Scope Enforcement

| Requirement ID | Requirement | Priority |
|----------------|-------------|----------|
| AUTHZ-001 | All data queries must be filtered by user's authorized scope | CRITICAL |
| AUTHZ-002 | Row-level security must be enforced at database level | CRITICAL |
| AUTHZ-003 | API responses must only include data within user's scope | CRITICAL |
| AUTHZ-004 | Report generation must respect scope restrictions | HIGH |
| AUTHZ-005 | Export functions must limit data to authorized scope | HIGH |

#### 4.3.2 Function-Level Authorization

| Requirement ID | Requirement | Priority |
|----------------|-------------|----------|
| AUTHZ-010 | All system functions must check user permissions before execution | CRITICAL |
| AUTHZ-011 | Administrative functions must require elevated privileges | CRITICAL |
| AUTHZ-012 | Calculation modifications must require OSPI Administrator or higher | HIGH |
| AUTHZ-013 | User management functions must be restricted to appropriate roles | HIGH |
| AUTHZ-014 | Bulk operations must require additional authorization confirmation | MEDIUM |

#### 4.3.3 Approval Workflows

| Workflow | Participants | Steps |
|----------|--------------|-------|
| **Calculation Formula Change** | OSPI Admin -> Senior Admin -> System Admin | Propose -> Review -> Approve -> Implement |
| **District Data Submission** | District Staff -> District Admin -> ESD -> OSPI | Enter -> Certify -> Review -> Accept |
| **State Constant Update** | OSPI Admin -> Senior Admin | Propose -> Review -> Approve |
| **User Role Assignment** | Requester -> Approver (varies by role level) | Request -> Verify -> Approve |

### 4.4 Segregation of Duties

| Requirement ID | Requirement | Priority |
|----------------|-------------|----------|
| SOD-001 | Data entry and data approval must be performed by different users | CRITICAL |
| SOD-002 | Calculation configuration and calculation execution must be separated | HIGH |
| SOD-003 | User administration and security audit must be performed by different roles | HIGH |
| SOD-004 | No single user can complete end-to-end funding distribution without review | CRITICAL |

---

## 5. Data Protection

### 5.1 Encryption Requirements

#### 5.1.1 Encryption in Transit

| Requirement ID | Requirement | Standard |
|----------------|-------------|----------|
| ENC-001 | All web traffic must use HTTPS with TLS 1.2 or higher | CRITICAL |
| ENC-002 | SSL/TLS certificates must use minimum 2048-bit key length | CRITICAL |
| ENC-003 | API communications must be encrypted end-to-end | CRITICAL |
| ENC-004 | Database connections must use encrypted protocols | HIGH |
| ENC-005 | File transfers must use secure protocols (SFTP, HTTPS) | HIGH |

#### 5.1.2 Encryption at Rest

| Requirement ID | Requirement | Standard |
|----------------|-------------|----------|
| ENC-010 | All database storage must be encrypted (Azure Transparent Data Encryption) | CRITICAL |
| ENC-011 | Backup files must be encrypted | CRITICAL |
| ENC-012 | Archive data must remain encrypted | HIGH |
| ENC-013 | Encryption keys must be managed via Azure Key Vault | CRITICAL |
| ENC-014 | Log files containing sensitive data must be encrypted | HIGH |

### 5.2 Data Classification and Handling

#### 5.2.1 Data Categories

| Category | Examples | Handling Requirements |
|----------|----------|----------------------|
| **Category 4 - Confidential** | SSN, student PII, personnel data, individual salaries | Encrypted storage, access logging, need-to-know access |
| **Category 3 - Sensitive** | Aggregate enrollment, budget summaries | Role-based access, audit trail |
| **Category 2 - Internal** | Process documentation, internal reports | Internal access only |
| **Category 1 - Public** | Published reports, public statistics | May be publicly disclosed |

#### 5.2.2 Specific Data Protection Requirements

| Data Type | Classification | Protection Measures |
|-----------|---------------|---------------------|
| **Social Security Numbers (SSN)** | Category 4 | Encrypted, masked display (last 4 only), access logged |
| **Student Names** | Category 4 | FERPA protection, role-based access |
| **Staff Birthdates** | Category 4 | Encrypted, redacted for ACP participants |
| **Individual Salary Data** | Category 4 | Role-based access, aggregation for public reports |
| **District Enrollment Counts** | Category 2-3 | Generally public at aggregate level |
| **Apportionment Calculations** | Category 3 | Audit trail required, version control |

### 5.3 Data Masking and Redaction

#### 5.3.1 Automatic Redaction Requirements

| Requirement ID | Requirement | Priority |
|----------------|-------------|----------|
| MASK-001 | SSNs must be masked in UI displays (show only last 4 digits) | CRITICAL |
| MASK-002 | ACP participant records must be automatically excluded from public exports | CRITICAL |
| MASK-003 | Public reports must aggregate data to prevent individual identification | HIGH |
| MASK-004 | Small cell sizes (<10) must be suppressed in public reports to prevent re-identification | HIGH |
| MASK-005 | Birthdates must be masked or removed for ACP participants | CRITICAL |

#### 5.3.2 Address Confidentiality Program (ACP) Handling

| Requirement ID | Requirement | Priority |
|----------------|-------------|----------|
| ACP-010 | System must integrate with WA Secretary of State ACP database | CRITICAL |
| ACP-011 | ACP status must be checked before any public data disclosure | CRITICAL |
| ACP-012 | ACP records must be flagged and handled per confidentiality requirements | CRITICAL |
| ACP-013 | Audit trail required for all ACP record access | HIGH |

### 5.4 Data Retention and Disposal

#### 5.4.1 Retention Requirements

| Data Type | Retention Period | Justification |
|-----------|-----------------|---------------|
| **Apportionment Calculations** | 7 years minimum | State Auditor requirements |
| **District Submissions** | 7 years minimum | Audit and historical analysis |
| **Audit Logs** | 7 years minimum | Compliance and investigation |
| **User Activity Logs** | 3 years | Security analysis |
| **Temporary/Working Data** | Until process completion | System performance |
| **Backup Data** | Per backup rotation schedule | Disaster recovery |

#### 5.4.2 Secure Disposal Requirements

| Requirement ID | Requirement | Priority |
|----------------|-------------|----------|
| DISP-001 | Data beyond retention period must be securely deleted | HIGH |
| DISP-002 | Deletion must use secure erase methods (not simple delete) | HIGH |
| DISP-003 | Disposal actions must be logged with date, data type, and authorizing user | HIGH |
| DISP-004 | Physical media disposal must follow NIST 800-88 guidelines | MEDIUM |

### 5.5 Data Loss Prevention (DLP)

| Requirement ID | Requirement | Priority |
|----------------|-------------|----------|
| DLP-001 | Prevent bulk export of Category 4 data without authorization | HIGH |
| DLP-002 | Alert on unusual data access patterns (e.g., accessing many districts rapidly) | MEDIUM |
| DLP-003 | Block or log attempts to email Category 4 data externally | MEDIUM |
| DLP-004 | Watermark exported documents with user identity | LOW |

---

## 6. Audit and Logging

### 6.1 Audit Trail Requirements

#### 6.1.1 Auditable Events

| Event Category | Specific Events | Retention |
|----------------|-----------------|-----------|
| **Authentication** | Login success/failure, logout, session timeout, MFA challenges | 3 years |
| **Authorization** | Permission denied, role changes, privilege escalation | 7 years |
| **Data Access** | View of Category 4 data, bulk data exports, report generation | 7 years |
| **Data Modification** | Create, update, delete of any apportionment data | 7 years |
| **Calculation Changes** | Formula modifications, constant updates, validation rule changes | 7 years |
| **Submission Workflow** | Submission, certification, review, approval, rejection | 7 years |
| **Administrative Actions** | User management, configuration changes, system settings | 7 years |
| **Security Events** | Failed login attempts, account lockouts, suspicious activity | 7 years |

#### 6.1.2 Audit Record Content

Each audit record must contain:

| Field | Description | Required |
|-------|-------------|----------|
| **Timestamp** | UTC timestamp with millisecond precision | Yes |
| **User ID** | Authenticated user identifier | Yes |
| **User Role** | Role at time of action | Yes |
| **Source IP** | Originating IP address | Yes |
| **Action Type** | Category of action performed | Yes |
| **Target Object** | Entity affected (table, record, file) | Yes |
| **Object ID** | Identifier of affected object | Yes |
| **Previous Value** | Value before change (for modifications) | Conditional |
| **New Value** | Value after change (for modifications) | Conditional |
| **Result** | Success or failure of action | Yes |
| **Session ID** | User session identifier | Yes |
| **Correlation ID** | Request correlation for distributed tracing | Yes |

### 6.2 Specific Audit Requirements from RFP

| Requirement ID | Requirement | RFP Reference |
|----------------|-------------|---------------|
| AUD-001 | Audit trails must exist for any adjustments made to calculations | Demonstration Scenario A.3 |
| AUD-002 | Audit trails must exist for changes to state constants | Demonstration Scenario A.3 |
| AUD-003 | Audit trails must exist for changes to district data | Demonstration Scenario A.3 |
| AUD-004 | Provide hierarchical approval process and audit trail for internal user calculation changes | RFP Section A.5 |
| AUD-005 | Track and audit all data validation changes | RFP Section A.5 |
| AUD-006 | State Auditor's Office must have timely access to all audit data | Requirement 042SAFS |

### 6.3 Logging Architecture

#### 6.3.1 Log Categories

| Log Type | Purpose | Destination |
|----------|---------|-------------|
| **Application Logs** | Debugging, troubleshooting | Azure Log Analytics |
| **Security Logs** | Security monitoring, incident investigation | Azure Sentinel / SIEM |
| **Audit Logs** | Compliance, regulatory requirements | Dedicated audit store |
| **Performance Logs** | System health, capacity planning | Azure Monitor |
| **Transaction Logs** | Financial audit trail | Append-only audit database |

#### 6.3.2 Log Protection

| Requirement ID | Requirement | Priority |
|----------------|-------------|----------|
| LOG-001 | Audit logs must be tamper-evident (append-only) | CRITICAL |
| LOG-002 | Log access must be restricted to authorized personnel | CRITICAL |
| LOG-003 | Log modification or deletion must be prevented | CRITICAL |
| LOG-004 | Logs must be backed up separately from application data | HIGH |
| LOG-005 | Log integrity must be verifiable (checksums/signatures) | HIGH |

### 6.4 Traceability Requirements

| Requirement ID | Requirement | RFP Reference |
|----------------|-------------|---------------|
| TRACE-001 | System must provide ability to trace transactions through the system for debugging | Requirement 175TEC |
| TRACE-002 | Transaction tracing must support security breach investigation | Requirement 175TEC |
| TRACE-003 | Distributed tracing must correlate requests across system components | Technical Requirement |
| TRACE-004 | Trace data must be available for at least 90 days online | Operational Requirement |

### 6.5 Audit Reporting

| Report | Audience | Frequency |
|--------|----------|-----------|
| **Login Activity Summary** | Security Team | Daily |
| **Failed Authentication Report** | Security Team | Daily |
| **Data Access Report by User** | Compliance Team | Weekly |
| **Calculation Changes Report** | OSPI Leadership | Per occurrence |
| **Full Audit Extract for SAO** | State Auditor | On demand |
| **Privileged User Activity** | Security Team | Weekly |
| **Anomaly Detection Report** | Security Team | Real-time alerts |

---

## 7. Network Security

### 7.1 Network Architecture

#### 7.1.1 Azure Hosting Requirements

| Requirement ID | Requirement | Priority |
|----------------|-------------|----------|
| NET-001 | System must be hosted in OSPI Azure Cloud environment | CRITICAL |
| NET-002 | Azure Virtual Network must isolate SASQUATCH components | CRITICAL |
| NET-003 | Network Security Groups must restrict traffic to required paths only | CRITICAL |
| NET-004 | Azure DDoS Protection must be enabled | HIGH |
| NET-005 | Azure Firewall or equivalent must protect ingress/egress | HIGH |

#### 7.1.2 Network Segmentation

| Zone | Components | Access |
|------|------------|--------|
| **Public Zone** | Web Application Firewall, Load Balancer | Internet-facing |
| **Web Tier** | Web applications, API gateway | From WAF only |
| **Application Tier** | Business logic, calculation engine | From Web Tier only |
| **Data Tier** | Databases, file storage | From App Tier only |
| **Management Zone** | Admin interfaces, monitoring | VPN/Jump host only |

### 7.2 Web Application Security

#### 7.2.1 Web Application Firewall (WAF)

| Requirement ID | Requirement | Priority |
|----------------|-------------|----------|
| WAF-001 | Azure WAF must be deployed in front of all web applications | CRITICAL |
| WAF-002 | OWASP Core Rule Set must be enabled and kept current | CRITICAL |
| WAF-003 | Custom rules must block known attack patterns | HIGH |
| WAF-004 | WAF must log all blocked requests for analysis | HIGH |
| WAF-005 | Rate limiting must prevent brute force attacks | HIGH |

#### 7.2.2 OWASP Top 10 Protection

| Vulnerability | Mitigation Requirement |
|---------------|----------------------|
| **Injection** | Parameterized queries, input validation |
| **Broken Authentication** | Strong password policy, MFA, session management |
| **Sensitive Data Exposure** | Encryption, proper key management |
| **XML External Entities** | Disable DTD processing, validate XML |
| **Broken Access Control** | RBAC enforcement, principle of least privilege |
| **Security Misconfiguration** | Hardened configurations, regular audits |
| **Cross-Site Scripting (XSS)** | Output encoding, Content Security Policy |
| **Insecure Deserialization** | Validate serialized data, restrict types |
| **Components with Vulnerabilities** | Regular patching, dependency scanning |
| **Insufficient Logging** | Comprehensive audit logging (Section 6) |

### 7.3 API Security

| Requirement ID | Requirement | Priority |
|----------------|-------------|----------|
| API-010 | All APIs must use HTTPS only | CRITICAL |
| API-011 | API Gateway must validate and sanitize all inputs | CRITICAL |
| API-012 | Rate limiting must be enforced per user/client | HIGH |
| API-013 | API versioning must be implemented | MEDIUM |
| API-014 | CORS policies must restrict cross-origin requests | HIGH |
| API-015 | API responses must not leak internal system information | HIGH |

### 7.4 Integration Security

#### 7.4.1 District Data Transmission

| Requirement ID | Requirement | Priority |
|----------------|-------------|----------|
| INT-001 | District file uploads must use secure transport (HTTPS/SFTP) | CRITICAL |
| INT-002 | Uploaded files must be scanned for malware | HIGH |
| INT-003 | File type validation must prevent executable uploads | HIGH |
| INT-004 | API integrations must use OAuth 2.0 authentication | HIGH |

#### 7.4.2 Internal System Integration

| Requirement ID | Requirement | Priority |
|----------------|-------------|----------|
| INT-010 | Integration with 18 internal OSPI systems must use secure protocols | CRITICAL |
| INT-011 | Service-to-service authentication must use managed identities | HIGH |
| INT-012 | Integration traffic must stay within Azure Virtual Network when possible | HIGH |
| INT-013 | Integration credentials must be stored in Azure Key Vault | CRITICAL |

---

## 8. Incident Response

### 8.1 Incident Classification

| Severity | Definition | Examples | Response Time |
|----------|------------|----------|---------------|
| **Critical (P1)** | System unavailable or major security breach affecting funding operations | Data breach, ransomware, system-wide outage during payment cycle | 15 minutes |
| **High (P2)** | Significant security event or partial system impact | Unauthorized access detected, component failure, data integrity issue | 1 hour |
| **Medium (P3)** | Limited security event or minor functional impact | Suspicious activity, minor vulnerability, single-user issue | 4 hours |
| **Low (P4)** | Security observation requiring investigation | Policy violation, informational security alert | 24 hours |

### 8.2 Incident Response Procedures

#### 8.2.1 Response Phases

| Phase | Activities | Responsible Party |
|-------|------------|-------------------|
| **Detection** | Identify and validate incident | Security monitoring, automated alerts |
| **Containment** | Limit incident scope and prevent spread | Security Team, System Administrators |
| **Eradication** | Remove threat and close vulnerabilities | Security Team, Development Team |
| **Recovery** | Restore normal operations | Operations Team, Development Team |
| **Lessons Learned** | Document and improve processes | All parties, Incident Manager |

#### 8.2.2 Specific Incident Types

| Incident Type | Immediate Actions |
|---------------|-------------------|
| **Unauthorized Data Access** | Revoke access, preserve logs, notify privacy officer |
| **Ransomware/Malware** | Isolate affected systems, engage incident response, notify WaTech |
| **DDoS Attack** | Engage Azure DDoS protection, scale resources, notify WaTech |
| **Data Breach (Category 4)** | Notify OSPI leadership, WaTech, and AG office per state law |
| **Insider Threat** | Preserve evidence, revoke access, engage HR and legal |

### 8.3 Notification Requirements

#### 8.3.1 Internal Notifications

| Audience | Severity | Notification Timeline |
|----------|----------|----------------------|
| **Security Team** | All | Immediate (automated) |
| **System Administrators** | P1, P2 | Within 15 minutes |
| **OSPI IT Management** | P1, P2 | Within 1 hour |
| **OSPI Executive Leadership** | P1 | Within 2 hours |

#### 8.3.2 External Notifications

| Audience | Trigger | Timeline | Authority |
|----------|---------|----------|-----------|
| **WaTech Security Office** | P1 incidents, any suspected breach | Within 24 hours | State policy |
| **Attorney General** | Data breach affecting WA residents | Per RCW 19.255.010 | State law |
| **Affected Districts** | Data breach affecting their data | Within 48 hours | Best practice |
| **US Dept of Education** | FERPA breach | Within 72 hours | Federal requirement |

### 8.4 Evidence Preservation

| Requirement ID | Requirement | Priority |
|----------------|-------------|----------|
| IR-001 | All logs must be preserved from incident detection through resolution | CRITICAL |
| IR-002 | System state must be captured before any remediation actions | HIGH |
| IR-003 | Chain of custody must be maintained for all evidence | HIGH |
| IR-004 | Evidence must be stored in tamper-evident manner | HIGH |
| IR-005 | Legal hold capability must exist for ongoing investigations | MEDIUM |

### 8.5 Business Continuity

| Requirement ID | Requirement | Priority |
|----------------|-------------|----------|
| BC-001 | System must meet WaTech Policy 151.10 disaster recovery requirements | CRITICAL |
| BC-002 | Recovery Time Objective (RTO): 4 hours for critical functions | CRITICAL |
| BC-003 | Recovery Point Objective (RPO): 1 hour for transaction data | CRITICAL |
| BC-004 | Disaster recovery tests must be conducted annually | HIGH |
| BC-005 | Business continuity plan must be documented and maintained | HIGH |

---

## 9. Security Testing Requirements

### 9.1 Testing Schedule

| Test Type | Frequency | Performed By |
|-----------|-----------|--------------|
| **Vulnerability Scanning** | Monthly | Automated tools |
| **Penetration Testing** | Annually minimum | Third-party security firm |
| **Security Code Review** | Each release | Development team with security tools |
| **Configuration Review** | Quarterly | Security team |
| **Access Review** | Quarterly | System administrators |
| **Disaster Recovery Test** | Annually | Operations team |

### 9.2 Vulnerability Assessment

| Requirement ID | Requirement | RFP Reference |
|----------------|-------------|---------------|
| TEST-001 | Regular security audits and vulnerability assessments at least annually | Requirement 153TEC |
| TEST-002 | All identified vulnerabilities must be prioritized and remediated | Best practice |
| TEST-003 | Critical vulnerabilities must be remediated within 30 days | Best practice |
| TEST-004 | High vulnerabilities must be remediated within 90 days | Best practice |
| TEST-005 | Vulnerability scan results must be documented and tracked | Compliance |

### 9.3 Penetration Testing Requirements

| Aspect | Requirement |
|--------|-------------|
| **Scope** | All public-facing applications, APIs, and authenticated user functions |
| **Methodology** | OWASP Testing Guide, PTES, or equivalent |
| **Testing Firm** | Independent third party with relevant certifications (OSCP, CREST) |
| **Report** | Written report with findings, risk ratings, and remediation recommendations |
| **Retest** | Validation testing after critical/high findings are remediated |

### 9.4 Secure Development Practices

| Requirement ID | Requirement | Priority |
|----------------|-------------|----------|
| DEV-001 | Security requirements must be included in user stories | HIGH |
| DEV-002 | Static Application Security Testing (SAST) in CI/CD pipeline | HIGH |
| DEV-003 | Dynamic Application Security Testing (DAST) in staging environment | HIGH |
| DEV-004 | Dependency scanning for known vulnerabilities | HIGH |
| DEV-005 | Security review required for high-risk changes | CRITICAL |
| DEV-006 | Secrets must never be committed to source control | CRITICAL |

### 9.5 Security Update Management

| Requirement ID | Requirement | RFP Reference |
|----------------|-------------|---------------|
| PATCH-001 | Regular updates and patches to address security vulnerabilities | Requirement 161TEC |
| PATCH-002 | Critical security patches within 48 hours of release | Best practice |
| PATCH-003 | High security patches within 30 days | Best practice |
| PATCH-004 | Patch testing in non-production environment before deployment | Best practice |
| PATCH-005 | Emergency patching procedure for zero-day vulnerabilities | Best practice |

---

## 10. Appendices

### Appendix A: Requirement Traceability Matrix

| Security Requirement | RFP Requirement ID | Section |
|---------------------|-------------------|---------|
| SSL encryption 2048-bit minimum | 147TEC | 5.1.1 |
| Password validation per WaTech | 148TEC | 3.1.2 |
| SAML 2.0 compliance, SAW/Azure AD | 149TEC | 3.1.1 |
| Annual security audits | 153TEC | 9.1 |
| FERPA/HIPAA/PCI compliance | 154TEC | 2.2 |
| Data encryption transit/rest | 156TEC | 5.1 |
| Access controls per state standards | 157TEC | 4.0 |
| WaTech SEC-04-03-S compliance | 159TEC | 2.1.1 |
| Disaster planning per WaTech 151.10 | 160TEC | 8.5 |
| Security updates and patches | 161TEC | 9.5 |
| Transaction tracing capability | 175TEC | 6.4 |
| ACP participant protection | 117SAFS, 118SAFS | 5.3.2 |
| SAO data access | 042SAFS | 4.2.8 |
| Audit trails for calculations | Demo A.3 | 6.2 |
| Hierarchical approval and audit | RFP A.5 | 4.3.3 |

### Appendix B: Personnel Security Requirements

| Requirement | Applicability | Process |
|-------------|---------------|---------|
| **Building Access Badge** | All on-site contractors | Submit security forms to OSPI for clearance |
| **FBI Background Check** | Projects with particularly sensitive data | Fingerprints at State Patrol approved site |
| **OSPI O365 Account** | All project personnel | Temporary accounts, closed at contract end |
| **Security Training** | All system users | Annual security awareness training |
| **Confidentiality Agreement** | All contractors | Signed before project access |

### Appendix C: Glossary

| Term | Definition |
|------|------------|
| **ACP** | Address Confidentiality Program (WA Secretary of State) |
| **Category 4** | WaTech data classification for confidential information requiring special handling |
| **ESD** | Educational Service District |
| **FERPA** | Family Educational Rights and Privacy Act |
| **MFA** | Multi-Factor Authentication |
| **OSPI** | Office of Superintendent of Public Instruction |
| **PII** | Personally Identifiable Information |
| **RBAC** | Role-Based Access Control |
| **RTO** | Recovery Time Objective |
| **RPO** | Recovery Point Objective |
| **SAO** | State Auditor's Office |
| **SASQUATCH** | School Apportionment System for Quality, Accountability, Transparency, and Calculations Hub |
| **SAW** | Secure Access Washington |
| **SIEM** | Security Information and Event Management |
| **WAC** | Washington Administrative Code |
| **WaTech** | Washington Technology Solutions |

### Appendix D: Reference Documents

| Document | Source | Version |
|----------|--------|---------|
| WaTech Policy 141.10 - Securing IT Assets | watech.wa.gov | 2023-12 |
| WaTech Policy 151.10 - Disaster Recovery | watech.wa.gov | Current |
| WaTech Standard SEC-04-03-S | watech.wa.gov | Current |
| WaTech Data Categorization | watech.wa.gov/categorizing-data-state-agency | Current |
| OSPI RFP 2026-12 | OSPI | 2026 |
| Attachment A - System Requirements | OSPI RFP | 2026 |
| OWASP Testing Guide | owasp.org | v4.2 |
| NIST 800-88 - Media Sanitization | nist.gov | Rev 1 |
| FERPA Regulations | ed.gov | 34 CFR Part 99 |

---

**Document Control:**

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-01-19 | SASQUATCH Project Team | Initial version |

---

*This document is maintained as part of the SASQUATCH project documentation. Updates require approval from OSPI IT Security and the SASQUATCH Project Steering Committee.*
