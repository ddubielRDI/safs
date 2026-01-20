# SASQUATCH RFP Processing - File Manifest

**RFP:** 2026-12 School Apportionment Modernization
**Client:** Washington State Office of Superintendent of Public Instruction (OSPI)
**Generated:** January 19, 2026
**Pipeline Version:** 1.0

---

## Directory Structure

```
/home/ddubiel/repos/safs/docs/
|
+-- original/                    # Source RFP documents (input)
|   +-- rfp_2026-12_sasquatch_apportionment.pdf
|   +-- RFP_2026-12_Addendum_01.pdf
|   +-- Attachment_A_Sasquatch_System_RequirementsV2.xlsx
|   +-- attachment_b_high-level_as-is_workflows.pdf
|   +-- attachment_c_sasquatch_rfp_demonstration_scenarios.docx
|
+-- flattened/                   # Converted markdown (intermediate)
|   +-- rfp_2026-12_sasquatch_apportionment.md
|   +-- RFP_2026-12_Addendum_01.md
|   +-- Attachment_A_Sasquatch_System_RequirementsV2.md
|   +-- attachment_b_high-level_as-is_workflows.md
|   +-- attachment_c_sasquatch_rfp_demonstration_scenarios.md
|
+-- shared/                      # Processing artifacts (intermediate)
|   +-- domain-detection.json
|   +-- requirements.json
|   +-- requirements-normalized.json
|   +-- sample-data-mappings.json
|   +-- progress.json
|   +-- errors.json
|   +-- gaps.json
|   +-- assumptions.json
|
+-- outputs/                     # Deliverables (output)
    +-- MANIFEST.md              (this file)
    +-- EXECUTIVE_SUMMARY.md
    +-- REQUIREMENTS_CATALOG.md
    +-- MODULAR_ARCHITECTURE.md
    +-- INTEROPERABILITY.md
    +-- SECURITY_REQUIREMENTS.md
    +-- DEMO_SCENARIOS.md
    +-- TRACEABILITY.md
    +-- EFFORT_ESTIMATION.md
```

---

## Input Documents

| File | Size | Description |
|------|------|-------------|
| `rfp_2026-12_sasquatch_apportionment.pdf` | 1.2 MB | Main RFP document with scope, timeline, and evaluation criteria |
| `RFP_2026-12_Addendum_01.pdf` | 143 KB | Addendum with Q&A responses and clarifications |
| `Attachment_A_Sasquatch_System_RequirementsV2.xlsx` | 104 KB | Detailed system requirements spreadsheet (568 rows) |
| `attachment_b_high-level_as-is_workflows.pdf` | 1.6 MB | Current SAFS workflow diagrams and process documentation |
| `attachment_c_sasquatch_rfp_demonstration_scenarios.docx` | 34 KB | Demo scenarios for vendor evaluation |

**Total Input Size:** ~3.1 MB (5 documents)

---

## Intermediate Files

### Converted Markdown (flattened/)

| File | Size | Source |
|------|------|--------|
| `rfp_2026-12_sasquatch_apportionment.md` | 236 KB | Main RFP |
| `RFP_2026-12_Addendum_01.md` | 14 KB | Addendum |
| `Attachment_A_Sasquatch_System_RequirementsV2.md` | 390 KB | Requirements spreadsheet |
| `attachment_b_high-level_as-is_workflows.md` | 40 KB | Workflow diagrams |
| `attachment_c_sasquatch_rfp_demonstration_scenarios.md` | 6 KB | Demo scenarios |

### Processing Artifacts (shared/)

| File | Size | Purpose |
|------|------|---------|
| `domain-detection.json` | 2.3 KB | Domain classification results (Education: 98% confidence) |
| `requirements.json` | 548 KB | Raw extracted requirements (568 items) |
| `requirements-normalized.json` | 251 KB | Deduplicated/normalized requirements (243 items) |
| `sample-data-mappings.json` | 1.4 KB | Demo data source mappings |
| `progress.json` | 383 B | Pipeline execution progress |
| `errors.json` | 59 B | Processing errors (empty - no errors) |
| `gaps.json` | 57 B | Identified gaps (empty - none detected) |
| `assumptions.json` | 64 B | Documented assumptions |

---

## Output Deliverables

| File | Size | Description | Audience |
|------|------|-------------|----------|
| **MANIFEST.md** | ~8 KB | Complete file listing (this document) | Technical team |
| **EXECUTIVE_SUMMARY.md** | ~12 KB | High-level summary for decision makers | Executives, PM |
| **REQUIREMENTS_CATALOG.md** | 166 KB | Complete requirements with categorization, priorities, and validation rules | Business Analysts, Developers |
| **MODULAR_ARCHITECTURE.md** | 71 KB | System architecture, component design, technology stack | Architects, Tech Leads |
| **INTEROPERABILITY.md** | 43 KB | Integration requirements, APIs, data exchange formats | Integration Engineers |
| **SECURITY_REQUIREMENTS.md** | 42 KB | Security controls, compliance requirements, data protection | Security Team, Compliance |
| **DEMO_SCENARIOS.md** | 56 KB | 16 detailed demonstration scripts with test data | Demo Team, QA |
| **TRACEABILITY.md** | 49 KB | Requirement-to-component mapping matrix | QA, Project Management |
| **EFFORT_ESTIMATION.md** | 39 KB | Work breakdown, hours, and resource allocation | PM, Estimators |

**Total Output Size:** ~486 KB (9 documents)

---

## Key Metrics Summary

| Metric | Value |
|--------|-------|
| Source Documents | 5 |
| Raw Requirements Extracted | 568 |
| Normalized Requirements | 243 |
| Deduplication Reduction | 57% |
| Work Sections | 3 (Collection, Calculation, Reporting) |
| System Components | 18 |
| Requirement Traceability | 100% |
| Demo Scenarios | 16 |
| Estimated Hours (AI-adjusted) | 20,480 |
| Budget Alignment | $9M |

---

## Domain Detection Results

| Domain | Confidence Score |
|--------|------------------|
| **Education** | **98%** (selected) |
| Government | 45% |
| Finance | 35% |
| Healthcare | 5% |
| eCommerce | 2% |

**Key Terminology Detected:**
- SAFS: School Apportionment Financial System (legacy)
- SASQUATCH: School Apportionment System for Quality, Accountability, Transparency, and Calculations Hub
- OSPI: Office of Superintendent of Public Instruction
- ESD: Educational Service District
- Apportionment: State education funding distribution process

---

## File Dependencies

```
Input (original/)
    |
    v
Conversion (flattened/) <-- convert-docs.py
    |
    v
Analysis (shared/)
    |
    +-- domain-detection.json
    +-- requirements.json --> requirements-normalized.json
    +-- sample-data-mappings.json
    |
    v
Generation (outputs/)
    |
    +-- REQUIREMENTS_CATALOG.md      <-- requirements-normalized.json
    +-- MODULAR_ARCHITECTURE.md      <-- requirements-normalized.json
    +-- INTEROPERABILITY.md          <-- requirements-normalized.json
    +-- SECURITY_REQUIREMENTS.md     <-- requirements-normalized.json
    +-- DEMO_SCENARIOS.md            <-- sample-data-mappings.json
    +-- TRACEABILITY.md              <-- all requirements + architecture
    +-- EFFORT_ESTIMATION.md         <-- all above
    +-- EXECUTIVE_SUMMARY.md         <-- aggregated metrics
    +-- MANIFEST.md                  <-- file inventory
```

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2026-01-19 | Initial generation from RFP 2026-12 |

---

## Notes

1. **No Processing Errors:** The `errors.json` file is empty, indicating all documents were successfully processed.

2. **Demo Data Source:** No sample data was included with the RFP. Demo scenarios reference publicly available SAFS data files from `https://ospi.k12.wa.us/safs-data-files` using Tumwater School District (34033) as the reference district per Attachment C.

3. **Requirement Deduplication:** 568 raw requirements were normalized to 243 after removing duplicates and consolidating related items (57% reduction).

4. **Traceability:** All 243 normalized requirements are mapped to system components with 100% coverage.

---

*Generated by RFP Processing Pipeline v1.0*
