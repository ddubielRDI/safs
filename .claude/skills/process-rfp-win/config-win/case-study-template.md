# Case Study Template

## Purpose

This template defines the format for case studies in bid documents. Case studies are **auto-selected from `Past_Projects.md`** based on domain, industry, and technology match. Phase 8.0 (Strategic Positioning) scores and ranks all 28 projects, and downstream phases (8.1, 8.2, 8.3) automatically embed the best-matching projects using this template format.

## Auto-Selection Process

Case studies are auto-populated by the bid pipeline:

1. **Phase 8.0** reads `Past_Projects.md` and scores each project against the RFP's domain, industry, and technology stack
2. **`matched_projects[]`** in `POSITIONING_OUTPUT.json` contains the ranked results
3. **Phases 8.1, 8.2, 8.3** consume matched_projects and write full case studies using this template format
4. **No `[CASE STUDY PLACEHOLDER]` markers** should appear in final bid documents

If you need to manually add or replace a case study, follow this template.

---

## Case Study Structure

### Template

```markdown
**Case Study: [Project Name]**

**Client:** [Client Name and Industry]
**Project Duration:** [Start - End Date]
**Our Role:** [Prime Contractor / Subcontractor / Partner]

**Challenge:**
[2-3 sentences describing the client's challenge that mirrors challenges in the current RFP]

**Our Solution:**
[2-3 sentences describing what we delivered, with specific technologies/approaches]

**Results:**
| Metric | Achievement |
|--------|------------|
| [Metric 1] | [Quantified result] |
| [Metric 2] | [Quantified result] |
| [Metric 3] | [Quantified result] |

**Relevance to This RFP:**
[1-2 sentences explicitly connecting this experience to the current opportunity]

**Reference Available:** [Yes/No - Contact upon request]
```

---

## Example Case Studies by Context

### For Risk Mitigation Demonstrations

```markdown
**Case Study: State Education Data Migration**

**Client:** [State] Department of Education
**Project Duration:** January 2023 - August 2023
**Our Role:** Prime Contractor

**Challenge:**
The department needed to migrate 15 years of student enrollment data (2.3 million records) from a legacy mainframe system to a modern cloud platform with zero data loss and minimal downtime during the school enrollment period.

**Our Solution:**
We implemented a phased migration approach with automated validation at each stage. Our proprietary data reconciliation engine compared source and target records in real-time, flagging any discrepancies for immediate resolution. We executed the cutover during a 48-hour maintenance window using a blue-green deployment strategy.

**Results:**
| Metric | Achievement |
|--------|------------|
| Data Migration Accuracy | 99.997% (3 records required manual correction) |
| Downtime | 4 hours (vs. 48-hour window allocated) |
| Post-Migration Issues | Zero critical issues in first 90 days |

**Relevance to This RFP:**
This project demonstrates our proven ability to handle complex data migrations with stringent accuracy requirements—directly applicable to the data migration scope outlined in Section 4.3 of the current RFP.

**Reference Available:** Yes - Contact upon request
```

### For Technical Capability Demonstrations

```markdown
**Case Study: Real-Time Enrollment Dashboard**

**Client:** [County] School District (45,000 students)
**Project Duration:** March 2022 - September 2022
**Our Role:** Prime Contractor

**Challenge:**
The district required real-time visibility into enrollment counts across 87 schools to support dynamic resource allocation and comply with state reporting requirements. The existing system provided data with a 24-hour lag.

**Our Solution:**
We designed and implemented a cloud-native dashboard using React frontend with a GraphQL API backed by PostgreSQL and Redis caching. The architecture supports 500+ concurrent users with sub-second response times. Automated data feeds from the Student Information System refresh every 5 minutes.

**Results:**
| Metric | Achievement |
|--------|------------|
| Data Refresh Latency | 5 minutes (down from 24 hours) |
| Concurrent Users Supported | 500+ |
| Dashboard Response Time | < 1 second (95th percentile) |
| User Adoption | 92% of administrators within 30 days |

**Relevance to This RFP:**
Our dashboard architecture and real-time data processing approach directly addresses requirements [REQ-ID] and [REQ-ID] for enrollment monitoring capabilities.

**Reference Available:** Yes - Contact upon request
```

### For Compliance Demonstrations

```markdown
**Case Study: FERPA-Compliant Student Portal**

**Client:** [State] Department of Education
**Project Duration:** June 2021 - December 2021
**Our Role:** Prime Contractor

**Challenge:**
The department needed a parent/student portal that provided secure access to academic records while maintaining strict FERPA compliance. Previous audit findings had identified concerns about access logging and data encryption.

**Our Solution:**
We implemented a zero-trust architecture with role-based access control, comprehensive audit logging, and encryption at rest and in transit. The system includes automated FERPA compliance reporting and supports the department's annual security assessments.

**Results:**
| Metric | Achievement |
|--------|------------|
| FERPA Compliance Audit | Passed with zero findings |
| Unauthorized Access Attempts | Zero successful breaches |
| Audit Trail Coverage | 100% of data access events logged |
| Security Assessment Score | 98/100 (up from 72/100 on legacy system) |

**Relevance to This RFP:**
This project demonstrates our deep understanding of FERPA requirements and our ability to implement compliant solutions—directly relevant to the compliance requirements in Section 5 of this RFP.

**Reference Available:** Yes - Contact upon request
```

### For Integration Demonstrations

```markdown
**Case Study: Multi-System Integration Hub**

**Client:** [Regional] Education Service Agency
**Project Duration:** February 2022 - July 2022
**Our Role:** Prime Contractor

**Challenge:**
The agency needed to integrate data from 12 different district Student Information Systems, 3 state reporting systems, and 5 third-party applications. Each system had different data formats, APIs, and update schedules.

**Our Solution:**
We implemented a canonical data model with adapters for each source system. The integration hub uses Apache Kafka for reliable message delivery and supports both real-time and batch processing modes. Automated monitoring alerts the team to any data quality issues.

**Results:**
| Metric | Achievement |
|--------|------------|
| Systems Integrated | 20 systems (12 SIS, 3 state, 5 third-party) |
| Data Processing Volume | 1.2 million records daily |
| Integration Uptime | 99.9% over 18 months |
| Manual Data Entry Reduction | 85% |

**Relevance to This RFP:**
Our integration architecture and experience with state education systems (including [System Name] mentioned in Section 3.2) directly supports the interoperability requirements in this RFP.

**Reference Available:** Yes - Contact upon request
```

---

## Placement Guidelines

### Where to Place Case Studies

1. **Executive Summary (title-page.md)**
   - 1 high-impact case study
   - Focus: Overall capability and client satisfaction
   - Length: Condensed (5-7 sentences)

2. **Solution Description (solution.md)**
   - 2-3 case studies
   - Focus: Technical capabilities, risk mitigation, specific features
   - Length: Full template format

3. **Timeline (timeline.md)**
   - 1 case study
   - Focus: On-time delivery, methodology success
   - Length: Condensed with emphasis on schedule adherence

### Selection Criteria

Choose case studies that:

1. **Match the domain** - Same industry (education, government, healthcare)
2. **Match the scale** - Similar number of users, records, or complexity
3. **Demonstrate specific capabilities** - Directly relevant to RFP requirements
4. **Have quantified results** - Concrete metrics, not just descriptions
5. **Are recent** - Within last 3 years preferred
6. **Have referenceable clients** - Available for reference calls

---

## Legacy Placeholder Markers (Deprecated)

**These markers are NO LONGER used.** The pipeline now auto-embeds real case studies.
If you encounter any of these in bid output, it indicates a pipeline error — the phase
failed to read `matched_projects[]` from `POSITIONING_OUTPUT.json`.

```
[CASE STUDY PLACEHOLDER: ...] ← SHOULD NOT APPEAR in final output
```

If found, re-run the relevant phase (8.1, 8.2, or 8.3) ensuring `POSITIONING_OUTPUT.json`
exists and contains `matched_projects[]`.

---

## User Instructions (Post-Generation Review)

Case studies are now auto-populated from `Past_Projects.md`. After bid generation:

1. **Review auto-selected case studies** for appropriateness to this specific RFP
2. **Verify client willingness** to provide reference if requested by the evaluating agency
3. **Optionally swap** with a better-matching project from `Past_Projects.md` if you know of one
4. **Update `[REQ-ID]` references** with actual requirement IDs from the bid's REQUIREMENTS_CATALOG.md
5. **Verify contact details** marked with `[VERIFY:]` — confirm these are current
6. **Check for confidentiality** — ensure no confidential client information is disclosed without permission

## Adding New Projects to Past_Projects.md

When Resource Data completes a new project worth citing in future bids:

1. Open `Past_Projects.md` in the repository root
2. Add a new `## Project N:` section following the existing format
3. Include: Client, Industry, Description, Technologies, Timeline, Team Size, Outcomes/Metrics
4. Add quantified results in a metrics table (these score higher in auto-matching)
5. Include a client quote/testimonial if available (provides +2 scoring bonus)
6. The next bid pipeline run will automatically include the new project in scoring

---

## Quality Checklist

Before finalizing bid with case studies:

- [ ] Zero `[CASE STUDY PLACEHOLDER]` markers remain in bid sections
- [ ] Case studies auto-populated from Past_Projects.md (not example/template data)
- [ ] Each case study has quantified results (metrics table)
- [ ] Each case study has explicit "Relevance to This RFP" statement
- [ ] No confidential client information disclosed without permission
- [ ] Reference availability confirmed with clients (check `[VERIFY:]` markers)
- [ ] Case studies span different capability areas (not all the same type)
- [ ] At least 3 distinct case studies across bid sections (submittal, management, technical)
