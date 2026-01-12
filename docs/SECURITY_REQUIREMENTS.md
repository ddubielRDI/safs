# SASQUATCH System Security Requirements

**Document Version:** 1.0
**Date:** 2026-01-12
**Source:** RFP 2026-12 Attachment A - System Requirements V2

---

## Executive Summary

The SASQUATCH (State Apportionment System) handles sensitive financial and personnel data for Washington State's 295+ school districts, ESDs, and Tribal Compact schools. This document consolidates all cybersecurity requirements from the RFP documentation.

### Security Classification

| Data Category | Classification | Examples |
|--------------|----------------|----------|
| Personnel Data | **Category 3 - Confidential** | SSN, addresses, phone numbers, professional licenses |
| Student Enrollment | **Protected (FERPA)** | Headcounts, FTE, program enrollment |
| Financial Data | **Sensitive** | Budgets, apportionments, expenditures |
| System Credentials | **Confidential** | Passwords, session tokens, API keys |

### Threat Model Summary

- **External Threats**: Unauthorized access attempts, data interception, credential theft
- **Internal Threats**: Privilege escalation, unauthorized data access, audit trail manipulation
- **Privacy Risks**: PII exposure, confidentiality program violations, FERPA breaches
- **Compliance Risks**: WaTech policy violations, audit failures, regulatory penalties

---

## 1. Authentication & Identity Management

### 1.1 Core Authentication Requirements

| Req ID | Requirement | Priority |
|--------|-------------|----------|
| **146TEC** | Individual logins and passwords unique within the system | Required |
| **148TEC** | Password validation per OSPI/WaTech Policy 141.10, Section 6.2 | Required |
| **149TEC** | SAML 2.0 compliance; Secure Access Washington (SAW) for external auth; Azure AD for agency auth | Required |
| **152TEC** | User session timeout after configurable inactive period | Required |
| **006SAFS** | Modern login via Entra ID or federated authentication | Future State |

### 1.2 Authentication Architecture

```
External Users (Districts, ESDs, Vendors)
    |
    v
[Secure Access Washington (SAW)] -- SAML 2.0 --> [SASQUATCH]
                                                      ^
                                                      |
[Azure Active Directory] -- Agency Auth -------- OSPI Staff
```

### 1.3 Password Standards (WaTech Policy 141.10)

Reference: https://watech.wa.gov/sites/default/files/2023-12/141.10_SecuringITAssets_2023_12_Parts_Rescinded.pdf

- Minimum complexity requirements
- Password rotation policies
- Account lockout after failed attempts
- Multi-factor authentication where applicable

---

## 2. Encryption & Data Protection

### 2.1 Encryption Requirements

| Req ID | Requirement | Scope |
|--------|-------------|-------|
| **147TEC** | SSL encryption with minimum 2048-bit key length | Transport Layer |
| **156TEC** | Encrypt data in transit AND at rest; backup files must be encrypted | Full Stack |

### 2.2 Implementation Standards

| Layer | Standard | Minimum |
|-------|----------|---------|
| Transport | TLS | 1.2+ (recommend 1.3) |
| Data at Rest | AES | 256-bit |
| Backups | AES | 256-bit with key management |
| Certificates | RSA/ECDSA | 2048-bit RSA / 256-bit ECDSA |

### 2.3 Key Management

- Secure key storage (HSM or equivalent)
- Key rotation procedures
- Separation of encryption keys from encrypted data
- Backup key recovery procedures

---

## 3. Access Control & Authorization

### 3.1 Access Control Requirements

| Req ID | Requirement | Type |
|--------|-------------|------|
| **157TEC** | Access controls meeting/exceeding State of WA and industry standards | Framework |
| **004EXP** | Role-based administrative controls for OSPI staff | RBAC |
| **022SAFS** | Third-party vendor access aligned with served districts | Delegation |
| **037SAFS** | CRUD operations with mandatory audit trails | Audited Access |
| **042SAFS** | State Auditor's Office timely data access | Audit Access |

### 3.2 Role Hierarchy

| Role Level | Users | Permissions |
|------------|-------|-------------|
| **System Admin** | OSPI IT Staff | Full system configuration, user management |
| **OSPI Staff** | Apportionment team | Run calculations, approve submissions, override values |
| **ESD Staff** | Regional reviewers | Review district submissions, run edits, approve/return |
| **District Staff** | Business managers | Submit data, view own district, run reports |
| **Vendor** | Third-party contractors | Delegated district access only |
| **Auditor** | SAO Staff | Read-only access to all data |

### 3.3 Third-Party Vendor Access Controls

Per requirement **022SAFS**:
- Vendors serve multiple districts under contract
- Access limited to authorized district records only
- Permissions must match served district permissions
- Audit logging of all vendor activity

---

## 4. Network Security

### 4.1 Perimeter Security

| Req ID | Requirement |
|--------|-------------|
| **150TEC** | Firewall with intrusion detection and prevention systems (IDS/IPS) |

### 4.2 Network Security Controls

- Web Application Firewall (WAF)
- DDoS protection
- Network segmentation between tiers
- VPN for administrative access
- IP allowlisting for SFTP connections

### 4.3 Secure Transmission Protocols

| Req ID | Requirement | Method |
|--------|-------------|--------|
| **030SAFS** | Secure data submission via APIs, SFTP | Inbound |
| **078SAFS** | Integration via MFTs, APIs, EIBs with best practices | Bidirectional |
| **080SAFS** | API/MFT for internal OSPI data | Internal |
| **188TEC** | SFTP alignment with OSPI framework | Legacy Support |

---

## 5. Audit Logging & Monitoring

### 5.1 Logging Requirements

| Req ID | Requirement | Category |
|--------|-------------|----------|
| **151TEC** | Log unauthorized login attempts: date/time, user ID, device, location | Security Events |
| **175TEC** | Transaction tracing for debugging and security breach investigation | Forensics |
| **172TEC** | Real-time monitoring: CPU, memory, disk, network | Operations |
| **173TEC** | Alerting to OSPI support team on issues | Incident Response |

### 5.2 Audit Trail Requirements

| Req ID | Requirement | Scope |
|--------|-------------|-------|
| **037SAFS** | Audit trails for all CRUD operations | Data Changes |
| **070SAFS** | Audit tracking for midyear changes to funds/codes/formulas | Configuration |
| **Demo A.3** | Audit trails for adjustments to calculations, constants, district data | Calculations |

### 5.3 Log Retention

- Security logs: Minimum 1 year
- Transaction logs: Per state retention schedule
- Audit trails: Duration of data retention + audit period

### 5.4 Security Audit Requirements

| Req ID | Requirement |
|--------|-------------|
| **153TEC** | Regular security audits and vulnerability assessments at least annually |

---

## 6. Privacy & PII Protection

### 6.1 Confidential Data Classification

Per **189TEC** and WaTech Category 3:

| Data Element | Classification | Handling |
|--------------|----------------|----------|
| Social Security Numbers | Category 3 - Confidential | Minimize collection, encrypt, redact |
| Residential Addresses | Category 3 - Confidential | Protect from disclosure |
| Personal Phone Numbers | Category 3 - Confidential | Protect from disclosure |
| Professional License Info | Category 3 - Confidential | Limited access |

**Enterprise Initiative**: Active effort to reduce SSN collection across OSPI systems.

### 6.2 Address Confidentiality Program

| Req ID | Requirement | Purpose |
|--------|-------------|---------|
| **117SAFS** | Identify individuals in confidentiality program | Detection |
| **118SAFS** | Redact PII for confidentiality program participants | Protection |
| **001PRS** | Integrate with confidentiality database; auto-redact before publication | Automation |

### 6.3 PII Handling Workflow

```
[Personnel Data Submission]
         |
         v
[System Validation] --> Check Confidentiality Program Database
         |
         v
[Flag Protected Records]
         |
         v
[Apply Redaction Rules] --> Before ANY public report generation
         |
         v
[Log Redaction Actions] --> Audit trail maintained
         |
         v
[Publish Report]
```

### 6.4 SSN Reconciliation

| Req ID | Requirement |
|--------|-------------|
| **009PRS** | Automated reconciliation for SSN mismatches between S-275 and eCertification |

- Exception dashboard for conflicts
- Route to appropriate OSPI team
- Near real-time synchronization

---

## 7. Regulatory Compliance

### 7.1 Compliance Requirements

| Req ID | Regulations | Applicability |
|--------|-------------|---------------|
| **154TEC** | PCI-DSS, HIPAA, FERPA | All applicable data |
| **155TEC** | GDPR, CCPA | As applicable |
| **158TEC** | WA State data retention/deletion policies | All records |
| **159TEC** | WaTech SEC-04-03-S (Securing IT Assets) | System-wide |

### 7.2 Washington State Standards

| Standard | Requirement ID | Focus Area |
|----------|---------------|------------|
| **WaTech Policy 141.10** | 148TEC | Password and authentication |
| **WaTech Policy 151.10** | 160TEC | Disaster recovery / COOP |
| **WaTech SEC-04-03-S** | 159TEC | Securing IT assets |
| **WaTech Category 3** | 189TEC | Confidential data handling |

### 7.3 FERPA Compliance

The system processes:
- Student enrollment data (headcounts, FTEs)
- Student program participation (ALE, Running Start, etc.)
- School-level aggregated data

FERPA protections:
- Authorized access only
- Audit trails on data access
- Minimum necessary principle
- No unauthorized disclosure

---

## 8. Disaster Recovery & Business Continuity

### 8.1 DR Requirements

| Req ID | Requirement | Target |
|--------|-------------|--------|
| **160TEC** | DR and COOP plans per WaTech Policy 151.10 | Compliance |
| **167TEC** | Reliable data protection/retrieval after failure | Availability |
| **168TEC** | Recovery Point Objective < 30 minutes (single point failure) | RPO |
| **169TEC** | Recovery Point Objective < 5 minutes (data loss) | RPO |
| **176TEC** | Reliable backup and restoration services | Operations |

### 8.2 Availability Requirements

| Req ID | Requirement |
|--------|-------------|
| **165TEC** | 99.99% uptime per month, excluding scheduled maintenance |

**Calculation**: 99.99% = ~4.3 minutes downtime/month maximum

### 8.3 Backup Requirements

- Encrypted backups (per 156TEC)
- Geo-redundant storage
- Regular backup testing
- Documented restoration procedures
- Backup integrity verification

---

## 9. Security Maintenance

### 9.1 Patch Management

| Req ID | Requirement |
|--------|-------------|
| **161TEC** | Regular updates and patches for security vulnerabilities |
| **171TEC** | Timely support and maintenance for security, reliability, functionality |

### 9.2 Security Update Process

1. Monitor for security advisories
2. Assess vulnerability impact
3. Test patches in sandbox
4. Schedule maintenance window (coordinated with OSPI)
5. Deploy patches
6. Verify successful application
7. Document changes

---

## 10. Data Integrity Controls

### 10.1 Input Validation

| Req ID | Requirement | Control |
|--------|-------------|---------|
| **012SAFS** | Integer field validation (digits only, auto-format commas) | Format |
| **013SAFS** | Decimal field validation (precision limits) | Format |
| **032SAFS** | Human-readable error messages for validation failures | UX |

### 10.2 Data Locking

| Req ID | Requirement | Purpose |
|--------|-------------|---------|
| **005ENR/007SAFS** | Lock data during processing cycles (up to 3 days/month) | Calculation Integrity |
| **Demo A.2** | Lock for monthly calculation and annual audit purposes | Integrity |

### 10.3 Calculated Field Protection

| Req ID | Requirement |
|--------|-------------|
| **014SAFS** | Display calculated fields as read-only with visual distinction |

### 10.4 Concurrent Access Control

| Req ID | Requirement |
|--------|-------------|
| **021SAFS** | Alert users when another modifies concurrently viewed data |

### 10.5 Version Control

| Req ID | Requirement | Scope |
|--------|-------------|-------|
| **091-092SAFS** | Maintain original + revised versions of F-196 | Audit |
| **035SAFS** | Retain 3+ years history; up to 25 years for some data | Archive |

---

## 11. Secure Integration

### 11.1 External System Integration

| Req ID | System | Security Requirement |
|--------|--------|---------------------|
| **003SAFS** | One Washington (Workday) | OneWA interface specifications |
| **078SAFS** | External systems | MFT, API, EIB with best practices |
| **114SAFS** | EDS and eCert | Secure data sharing |

### 11.2 API Security

- OAuth 2.0 / API key authentication
- Rate limiting
- Input validation
- Encrypted transport (TLS)
- Audit logging of API calls

### 11.3 SFTP Security

- Key-based authentication preferred
- IP allowlisting
- Encrypted file transfers
- Automated virus scanning
- File integrity verification

---

## 12. Hosting & Infrastructure Security

### 12.1 Hosting Requirements

| Req ID | Requirement |
|--------|-------------|
| **179TEC** | SaaS or cloud-hosted on OSPI-approved platform |
| **186TEC** | Microsoft Azure Cloud preferred (not required) |

### 12.2 Cloud Security Controls

If Azure-hosted:
- Azure Security Center
- Azure DDoS Protection
- Azure Firewall
- Azure Key Vault for secrets
- Azure AD integration
- Geographic data residency (US)

### 12.3 Data Ownership

| Req ID | Requirement |
|--------|-------------|
| **187TEC** | OSPI retains ownership of data AND system code |

---

## Compliance Checklist

### Pre-Deployment Security Validation

- [ ] SSL/TLS certificates installed (2048-bit minimum)
- [ ] Encryption at rest enabled
- [ ] SAML 2.0 integration with SAW tested
- [ ] Azure AD integration configured
- [ ] IDS/IPS deployed and configured
- [ ] Audit logging enabled and verified
- [ ] Backup encryption validated
- [ ] DR procedures documented and tested
- [ ] Vulnerability assessment completed
- [ ] Penetration testing performed
- [ ] FERPA compliance verified
- [ ] WaTech SEC-04-03-S compliance documented
- [ ] Address Confidentiality Program integration tested
- [ ] Role-based access controls configured

### Ongoing Security Operations

- [ ] Annual security audits (153TEC)
- [ ] Regular vulnerability assessments
- [ ] Patch management process active
- [ ] Security monitoring operational
- [ ] Incident response procedures documented
- [ ] Backup restoration testing (quarterly)
- [ ] Access reviews (periodic)
- [ ] Audit log reviews (ongoing)

---

## Appendix A: Requirement Cross-Reference

| Category | Requirement IDs |
|----------|-----------------|
| Authentication | 146TEC, 148TEC, 149TEC, 152TEC, 006SAFS |
| Encryption | 147TEC, 156TEC |
| Access Control | 157TEC, 004EXP, 022SAFS, 037SAFS, 042SAFS |
| Network Security | 150TEC, 030SAFS, 078SAFS, 080SAFS |
| Audit/Logging | 151TEC, 153TEC, 175TEC, 172TEC, 173TEC |
| Privacy/PII | 189TEC, 117SAFS, 118SAFS, 001PRS, 009PRS |
| Compliance | 154TEC, 155TEC, 158TEC, 159TEC |
| DR/BC | 160TEC, 165TEC, 167TEC, 168TEC, 169TEC, 176TEC |
| Maintenance | 161TEC, 171TEC |
| Data Integrity | 012SAFS, 013SAFS, 014SAFS, 021SAFS, 005ENR, 007SAFS |
| Hosting | 179TEC, 186TEC, 187TEC |

---

## Appendix B: WaTech Policy References

| Policy | Title | URL |
|--------|-------|-----|
| 141.10 | Securing IT Assets | https://watech.wa.gov/policies/141.10 |
| 151.10 | Disaster Recovery | https://watech.wa.gov/policies/151.10 |
| SEC-04-03-S | IT Asset Security Standard | https://watech.wa.gov/standards |
| Category 3 | Data Classification | https://watech.wa.gov/categorizing-data-state-agency |

---

*Document generated from RFP 2026-12 Attachment A analysis*
