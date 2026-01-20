# SASQUATCH RFP - Executive Summary

**RFP Number:** 2026-12
**Project:** School Apportionment Modernization
**Client:** Washington State Office of Superintendent of Public Instruction (OSPI)
**Prepared:** January 19, 2026

---

## Project Overview

Washington State OSPI is soliciting proposals to replace the legacy School Apportionment Financial System (SAFS) with a modern, cloud-based solution named **SASQUATCH** (School Apportionment System for Quality, Accountability, Transparency, and Calculations Hub).

The system manages the calculation and distribution of state education funding to Washington's 295 school districts, processing over $15 billion annually in apportionment payments.

---

## Key Project Parameters

| Parameter | Value |
|-----------|-------|
| **Budget** | $9,000,000 |
| **Timeline** | 24 months (July 2026 - June 2028) |
| **Domain** | K-12 Education (98% confidence) |
| **Legacy System** | SAFS (20+ years old) |
| **Districts Served** | 295 |
| **ESDs** | 9 Educational Service Districts |
| **Annual Funding Managed** | ~$15 billion |

---

## Scope Summary

### Three Work Sections

| Section | Description | % of Effort |
|---------|-------------|-------------|
| **1. Data Collection** | Enrollment, personnel, budget, and financial data input from districts | 35% |
| **2. Calculations** | Apportionment formulas, funding allocations, and adjustments | 40% |
| **3. Reporting** | Financial statements, compliance reports, and analytics | 25% |

### System Modernization Goals

1. **Replace Legacy Technology** - Move from aging mainframe-based system to modern cloud architecture
2. **Improve User Experience** - Intuitive web interface for district and OSPI users
3. **Enhance Data Quality** - Built-in validation, error checking, and data governance
4. **Increase Transparency** - Real-time access to funding calculations and status
5. **Ensure Compliance** - Meet state/federal audit and security requirements
6. **Enable Integration** - Connect with CEDARS, CORE, and other OSPI systems

---

## Requirements Analysis

| Metric | Count |
|--------|-------|
| Raw Requirements Extracted | 568 |
| Normalized Requirements (deduplicated) | 243 |
| Functional Requirements | 186 |
| Non-Functional Requirements | 57 |
| Critical Priority | 47 |
| High Priority | 89 |
| Medium Priority | 76 |
| Low Priority | 31 |

### Requirements by Work Section

| Section | Requirements | Critical | High |
|---------|--------------|----------|------|
| Data Collection | 98 | 19 | 38 |
| Calculations | 87 | 21 | 32 |
| Reporting | 58 | 7 | 19 |

---

## Proposed Architecture

### Technology Stack Recommendations

| Layer | Technology | Rationale |
|-------|------------|-----------|
| **Frontend** | React/Angular | Modern SPA, accessibility compliance |
| **Backend API** | .NET 8 / Node.js | Enterprise-grade, strong typing |
| **Database** | SQL Server / PostgreSQL | Complex calculations, ACID compliance |
| **Cloud Platform** | Azure Government | WA State preferred, FedRAMP compliance |
| **Integration** | REST APIs, SFTP | Compatibility with existing systems |

### System Components (18 Total)

| Category | Components |
|----------|------------|
| **User Interface** | District Portal, OSPI Admin Portal, Public Data Portal |
| **Data Management** | Enrollment Module, Personnel Module, Budget Module, Financial Statement Module |
| **Calculation Engine** | Apportionment Calculator, Formula Engine, Adjustment Processor |
| **Integration** | CEDARS Connector, CORE Connector, External API Gateway |
| **Reporting** | Report Generator, Analytics Dashboard, Audit Trail System |
| **Infrastructure** | Authentication/SSO, Notification Service, Document Management |

---

## Effort Estimation

### Summary

| Metric | Value |
|--------|-------|
| **Total Estimated Hours** | 20,480 |
| **AI Adjustment Factor** | 0.85 (15% productivity gain) |
| **Base Hours (before AI)** | 24,094 |
| **Team Size (Recommended)** | 12-15 FTEs |
| **Duration** | 24 months |

### Hours by Phase

| Phase | Hours | % of Total |
|-------|-------|------------|
| Discovery & Planning | 2,048 | 10% |
| Design & Architecture | 3,072 | 15% |
| Development | 9,216 | 45% |
| Testing & QA | 3,072 | 15% |
| Deployment & Training | 2,048 | 10% |
| Project Management | 1,024 | 5% |

### Hours by Work Section

| Section | Hours | % of Total |
|---------|-------|------------|
| Data Collection | 7,168 | 35% |
| Calculations | 8,192 | 40% |
| Reporting | 5,120 | 25% |

---

## Demo Requirements

The RFP requires vendors to demonstrate the system using **Tumwater School District (34033)** data. Sixteen (16) demonstration scenarios have been defined:

| Category | Scenarios | Duration |
|----------|-----------|----------|
| Data Collection | 6 | 90 minutes |
| Calculations | 5 | 75 minutes |
| Reporting | 3 | 45 minutes |
| Administration | 2 | 30 minutes |

**Demo Data Source:** OSPI SAFS Public Data Files (https://ospi.k12.wa.us/safs-data-files)

---

## Integration Requirements

### Internal OSPI Systems

| System | Integration Type | Priority |
|--------|------------------|----------|
| CEDARS (Student Data) | Bidirectional API | Critical |
| CORE (State Reporting) | Outbound API | High |
| iGrants (Grant Management) | Inbound API | High |
| EDS (Personnel) | Inbound Data | Medium |

### External Interfaces

| Interface | Method | Frequency |
|-----------|--------|-----------|
| District Data Uploads | SFTP/Web Upload | Daily/Monthly |
| State Treasury | Secure File Transfer | Monthly |
| Federal DOE Reporting | API/File Export | Annual |
| Public Data Portal | Read-only API | Real-time |

---

## Security & Compliance

### Compliance Requirements

| Standard | Requirement |
|----------|-------------|
| **FERPA** | Student data privacy protection |
| **FISMA** | Federal security controls |
| **WA State OCIO** | State security standards |
| **SOC 2 Type II** | Service organization controls |
| **WCAG 2.1 AA** | Accessibility compliance |

### Security Controls

- Multi-factor authentication (MFA)
- Role-based access control (RBAC)
- Data encryption (at-rest and in-transit)
- Comprehensive audit logging
- Automated vulnerability scanning
- Annual penetration testing

---

## Risk Factors

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Legacy data migration complexity | High | High | Phased migration with parallel operation |
| Calculation validation | Medium | Critical | Side-by-side comparison with SAFS |
| User adoption | Medium | Medium | Comprehensive training program |
| Integration dependencies | Medium | High | Early integration testing |
| Timeline compression | Low | High | Agile methodology with MVP approach |

---

## Evaluation Criteria

| Criterion | Weight |
|-----------|--------|
| Technical Approach | 30% |
| Prior Experience | 25% |
| Cost Proposal | 25% |
| Demo Performance | 20% |

---

## Key Dates

| Milestone | Date |
|-----------|------|
| RFP Released | January 8, 2026 |
| Questions Due | February 15, 2026 |
| Proposals Due | March 15, 2026 |
| Vendor Demos | April 1-30, 2026 |
| Award Announcement | May 15, 2026 |
| Contract Execution | June 30, 2026 |
| Project Kickoff | July 1, 2026 |
| Go-Live | June 30, 2028 |

---

## Recommendation

Based on comprehensive analysis of RFP 2026-12, this project represents a significant but achievable modernization effort. Key success factors include:

1. **Strong Requirements Coverage** - 243 well-defined requirements with 100% traceability
2. **Clear Architecture Path** - Modern cloud-native design with proven technologies
3. **Realistic Timeline** - 24 months aligns with complexity and phased approach
4. **Adequate Budget** - $9M budget supports estimated 20,480 hours of effort
5. **Defined Success Criteria** - 16 demo scenarios provide objective evaluation basis

**Proceed with proposal development.**

---

## Deliverables Generated

This analysis produced the following deliverables:

| Document | Purpose |
|----------|---------|
| REQUIREMENTS_CATALOG.md | Complete requirements specification |
| MODULAR_ARCHITECTURE.md | System design and component architecture |
| INTEROPERABILITY.md | Integration specifications |
| SECURITY_REQUIREMENTS.md | Security and compliance requirements |
| DEMO_SCENARIOS.md | Demonstration scripts and test data |
| TRACEABILITY.md | Requirements-to-components mapping |
| EFFORT_ESTIMATION.md | Work breakdown and resource planning |
| MANIFEST.md | Complete file inventory |
| EXECUTIVE_SUMMARY.md | This document |

---

*Prepared for proposal development team review*
*Generated: January 19, 2026*
