---
name: phase6-manifest-win
expert-role: Technical Writer
domain-expertise: Documentation, audit trails
---

# Phase 6: Manifest Generation

## Expert Role

You are a **Technical Writer** with deep expertise in:
- Documentation structure
- Audit trail creation
- Executive summaries
- Technical documentation standards

## Purpose

Generate MANIFEST.md (audit log), EXECUTIVE_SUMMARY.md, and NAVIGATION_GUIDE.md.

NAVIGATION_GUIDE.md generation merged into this phase 2026-05-18 (formerly Phase 6b).

## Inputs

- All `{folder}/outputs/*.md` files
- `{folder}/shared/progress.json`
- `{folder}/shared/requirements-normalized.json`

## Required Outputs

- `{folder}/outputs/MANIFEST.md`
- `{folder}/outputs/EXECUTIVE_SUMMARY.md`
- `{folder}/outputs/NAVIGATION_GUIDE.md` (merged 2026-05-18 from former Phase 6b)

## Instructions

### Step 1: Inventory All Outputs

```python
import glob
import os

output_files = glob.glob(f"{folder}/outputs/**/*", recursive=True)
shared_files = glob.glob(f"{folder}/shared/*.json")

file_inventory = []
for file_path in output_files:
    if os.path.isfile(file_path):
        stat = os.stat(file_path)
        file_inventory.append({
            "path": file_path.replace(folder + "/", ""),
            "size_bytes": stat.st_size,
            "modified": datetime.fromtimestamp(stat.st_mtime).isoformat()
        })
```

### Step 2: Load Summary Data

```python
requirements = read_json(f"{folder}/shared/requirements-normalized.json")
progress = read_json(f"{folder}/shared/progress.json") if exists(f"{folder}/shared/progress.json") else {}
domain_context = read_json(f"{folder}/shared/domain-context.json")

req_count = len(requirements.get("requirements", []))
domain = domain_context.get("selected_domain", "Generic")
```

### Step 3: Generate Manifest

```python
def generate_manifest(file_inventory, progress, domain, req_count):
    doc = f"""# Processing Manifest

**Domain:** {domain}
**Generated:** {datetime.now().strftime('%Y-%m-%d %H:%M')}
**Pipeline Version:** WIN Edition 1.0

---

## Pipeline Summary

| Metric | Value |
|--------|-------|
| Start Time | {progress.get("pipeline_start", "N/A")} |
| Total Requirements | {req_count} |
| Output Files | {len(file_inventory)} |
| Domain Detected | {domain} |

---

## Phase Execution Log

| Phase | Status | Duration | Message |
|-------|--------|----------|---------|
"""

    phases = progress.get("phases", {})
    for phase_id, phase_data in sorted(phases.items()):
        status = phase_data.get("status", "unknown")
        status_icon = "✅" if status == "completed" else "❌" if status == "failed" else "⏳"
        message = phase_data.get("message", "")[:50]
        doc += f"| {phase_id} | {status_icon} {status} | - | {message} |\n"

    doc += """

---

## Output File Inventory

| File | Size | Modified |
|------|------|----------|
"""

    for file_info in sorted(file_inventory, key=lambda x: x["path"]):
        size_kb = file_info["size_bytes"] / 1024
        doc += f"| {file_info['path']} | {size_kb:.1f} KB | {file_info['modified'][:19]} |\n"

    doc += """

---

## Verification Checklist

| Output | Status |
|--------|--------|
"""

    required_outputs = [
        "ARCHITECTURE.md",
        "SECURITY_REQUIREMENTS.md",
        "REQUIREMENTS_CATALOG.md",
        "TRACEABILITY.md",
        "EFFORT_ESTIMATION.md"
    ]

    for output in required_outputs:
        exists_in_inventory = any(output in f["path"] for f in file_inventory)
        status = "✅ Present" if exists_in_inventory else "❌ Missing"
        doc += f"| {output} | {status} |\n"

    doc += """

---

## Audit Trail

This manifest serves as the audit trail for the RFP processing pipeline.

### Processing Environment
- Pipeline: process-rfp-win (Mayor Orchestrator)
- Date: """ + datetime.now().strftime('%Y-%m-%d %H:%M:%S') + """

### Data Lineage
1. Source documents → `original/`
2. Flattened content → `flattened/`
3. Intermediate data → `shared/`
4. Final outputs → `outputs/`

---

## Integrity Verification

To verify output integrity:
```bash
# Check all files exist
ls -la {folder}/outputs/

# Verify file sizes
du -sh {folder}/outputs/*
```
"""

    return doc

manifest_md = generate_manifest(file_inventory, progress, domain, req_count)
write_file(f"{folder}/outputs/MANIFEST.md", manifest_md)
```

### Step 4: Generate Executive Summary

```python
def generate_executive_summary(domain, req_count, requirements, domain_context):
    # Calculate key metrics
    reqs = requirements.get("requirements", [])
    critical = sum(1 for r in reqs if r.get("priority") == "CRITICAL")
    high = sum(1 for r in reqs if r.get("priority") == "HIGH")

    doc = f"""# Executive Summary

**RFP Analysis for {domain} Solution**
**Generated:** {datetime.now().strftime('%Y-%m-%d %H:%M')}

---

## Overview

This document summarizes the comprehensive analysis of the Request for Proposal (RFP) for a {domain} solution. The analysis identified **{req_count} requirements** across multiple functional areas.

---

## Key Findings

### Requirements Summary

| Priority | Count | Percentage |
|----------|-------|------------|
| Critical | {critical} | {critical/req_count*100:.1f}% |
| High | {high} | {high/req_count*100:.1f}% |
| Medium | {req_count - critical - high} | {(req_count - critical - high)/req_count*100:.1f}% |

### Domain Analysis

The RFP is classified as a **{domain}** solution with the following characteristics:
"""

    # Add domain-specific content
    if domain == "education":
        doc += """
- K-12 education management focus
- FERPA compliance requirements
- State reporting integration (CEDARS/CEISDARS)
- Multi-district support
"""
    elif domain == "healthcare":
        doc += """
- Healthcare delivery management
- HIPAA compliance requirements
- HL7/FHIR integration needs
- Patient data protection
"""
    else:
        doc += """
- Enterprise solution requirements
- Standard compliance needs
- Integration requirements
- Data management focus
"""

    doc += """

---

## Recommended Approach

### Solution Architecture
- Modern cloud-native architecture
- Scalable microservices design
- API-first development approach
- Mobile-responsive user interface

### Implementation Strategy
1. **Discovery Phase** (2 weeks)
   - Requirements refinement
   - Stakeholder alignment

2. **Design Phase** (3 weeks)
   - Architecture finalization
   - Technical design documentation

3. **Development Phase** (12 weeks)
   - Iterative development
   - Continuous integration

4. **Deployment Phase** (2 weeks)
   - User acceptance testing
   - Production deployment

---

## Risk Summary

Key risks identified and mitigation strategies:

| Risk Area | Impact | Mitigation |
|-----------|--------|------------|
| Integration complexity | High | Early POC development |
| Compliance requirements | High | Dedicated compliance reviews |
| Timeline constraints | Medium | Agile methodology |
| Resource availability | Medium | Team backup planning |

---

## Investment Summary

Detailed estimates are provided in the Effort Estimation document. Key figures:

- **Total Effort:** See EFFORT_ESTIMATION.md
- **Recommended Team:** 5-6 FTE
- **AI-Assisted Savings:** ~35% efficiency gain

---

## Next Steps

1. Review detailed specifications
2. Clarify open questions
3. Schedule kickoff meeting
4. Finalize team composition
5. Begin discovery phase

---

## Document References

| Document | Purpose |
|----------|---------|
| REQUIREMENTS_CATALOG.md | Full requirements listing |
| ARCHITECTURE.md | Technical architecture |
| SECURITY_REQUIREMENTS.md | Security specifications |
| TRACEABILITY.md | Requirements traceability |
| EFFORT_ESTIMATION.md | Detailed estimates |
| NAVIGATION_GUIDE.md | Document navigation |
"""

    return doc

exec_summary = generate_executive_summary(domain, req_count, requirements, domain_context)
write_file(f"{folder}/outputs/EXECUTIVE_SUMMARY.md", exec_summary)
```

### Step N — Navigation Guide Generation (merged 2026-05-18 from former Phase 6b)

Generate `NAVIGATION_GUIDE.md` so users (and SVA-5 rule `SVA5-NAV-GUIDE-LINKS`)
can locate any artifact quickly. Output path is unchanged from the prior
Phase 6b so all downstream consumers (sva5-doc-validator-win.md, manifest
references) continue to work without edits.

```python
import glob
import re as _re
from datetime import datetime as _dt

output_files = glob.glob(f"{folder}/outputs/*.md")


def _extract_sections(content):
    """Pull the first 10 H2 section headings."""
    return _re.findall(r'^## (.+)$', content, _re.MULTILINE)[:10]


documents = []
for fp in output_files:
    content = read_file(fp)
    documents.append({
        "name": os.path.basename(fp),
        "path": fp,
        "word_count": len(content.split()),
        "sections": _extract_sections(content),
    })


USE_CASES = [
    {
        "title": "Understanding the Requirements",
        "description": "Get a comprehensive view of all RFP requirements",
        "documents": ["REQUIREMENTS_CATALOG.md", "TRACEABILITY.md"],
        "key_sections": ["Requirements by Category", "Full Traceability Matrix"],
    },
    {
        "title": "Technical Architecture Review",
        "description": "Review the proposed technical solution",
        "documents": ["ARCHITECTURE.md", "SECURITY_REQUIREMENTS.md", "INTEROPERABILITY.md"],
        "key_sections": ["Architecture Overview", "Technology Stack", "Security Architecture"],
    },
    {
        "title": "Estimating Project Effort",
        "description": "Understand effort estimates and resource needs",
        "documents": ["EFFORT_ESTIMATION.md", "REQUIREMENT_RISKS.md"],
        "key_sections": ["Executive Summary", "Resource Plan", "Risk Summary"],
    },
    {
        "title": "Preparing the Bid Response",
        "description": "Gather information for bid document sections",
        "documents": ["EXECUTIVE_SUMMARY.md", "TRACEABILITY.md", "DIAGRAM_BLUEPRINTS.md"],
        "key_sections": ["Key Findings", "Bid Section Coverage", "Diagram Blueprints"],
    },
    {
        "title": "Validating Compliance",
        "description": "Ensure all mandatory requirements are addressed",
        "documents": ["REQUIREMENTS_CATALOG.md", "SECURITY_REQUIREMENTS.md", "COMPETITIVE_POSITION.md"],
        "key_sections": ["Critical Requirements", "Compliance Requirements", "Competitive Position"],
    },
]


def generate_navigation_md(documents, use_cases):
    doc = f"""# Navigation Guide

**Generated:** {_dt.now().strftime('%Y-%m-%d %H:%M')}

This guide helps you quickly find information across all generated documents.

---

## Quick Start

| I want to... | Go to... |
|--------------|----------|
| See all requirements | REQUIREMENTS_CATALOG.md |
| Review architecture | ARCHITECTURE.md |
| Check security specs | SECURITY_REQUIREMENTS.md |
| View estimates | EFFORT_ESTIMATION.md |
| Find risks | REQUIREMENT_RISKS.md |
| See clarifying questions | CLARIFYING_QUESTIONS.md |
| See competitive position | COMPETITIVE_POSITION.md |
| See diagram blueprints | DIAGRAM_BLUEPRINTS.md |
| Prepare bid content | bid-sections/*.md and bid/*.pdf |

---

## Document Overview

| Document | Sections |
|----------|----------|
"""
    for d in documents:
        doc += f"| {d['name']} | {len(d['sections'])} sections |\n"

    doc += "\n---\n\n## Use Case Guides\n\n"
    for uc in use_cases:
        doc += f"### {uc['title']}\n\n**Goal:** {uc['description']}\n\n**Primary Documents:**\n"
        for dn in uc["documents"]:
            doc += f"- {dn}\n"
        doc += "\n**Key Sections:**\n"
        for s in uc["key_sections"]:
            doc += f"- {s}\n"
        doc += "\n---\n\n"

    doc += "## Document Details\n\n"
    for d in sorted(documents, key=lambda x: x["name"]):
        doc += f"### {d['name']}\n\n**Word Count:** ~{d['word_count']:,}\n\n**Sections:**\n"
        for s in d["sections"]:
            doc += f"- {s}\n"
        doc += "\n"

    doc += """
---

## Cross-Reference Guide

### Finding Requirements by Topic

| Topic | Documents | Search Terms |
|-------|-----------|--------------|
| Security | SECURITY_REQUIREMENTS.md, REQUIREMENTS_CATALOG.md | SEC, security, auth |
| Integration | INTEROPERABILITY.md, ARCHITECTURE.md | INT, api, interface |
| UI/UX | UI_SPECS.md | UI, screen, display |
| Data | ENTITY_DEFINITIONS.md, REQUIREMENTS_CATALOG.md | data, entity, field |

### Tracing Requirements

1. **Start:** REQUIREMENTS_CATALOG.md — Find the requirement ID
2. **Specs:** TRACEABILITY.md — See which specs address it
3. **Risks:** REQUIREMENT_RISKS.md — Check risk level
4. **Estimate:** EFFORT_ESTIMATION.md — Find effort estimate
5. **Bid:** Check bid section mapping in TRACEABILITY.md

---

## Need Help?

1. Check MANIFEST.md for the complete file list
2. Search keywords across all files
3. Review TRACEABILITY.md for requirement mappings
"""
    return doc


navigation_md = generate_navigation_md(documents, USE_CASES)
write_file(f"{folder}/outputs/NAVIGATION_GUIDE.md", navigation_md)
log(f"NAVIGATION_GUIDE.md generated ({len(documents)} documents cataloged)")
```

## Quality Checklist

- [ ] `MANIFEST.md` created in `outputs/`
- [ ] `EXECUTIVE_SUMMARY.md` created in `outputs/`
- [ ] `NAVIGATION_GUIDE.md` created in `outputs/` (merged 2026-05-18 from former Phase 6b)
- [ ] All output files inventoried
- [ ] Phase execution logged
- [ ] Audit trail complete
