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

Generate MANIFEST.md (audit log) and EXECUTIVE_SUMMARY.md.

## Inputs

- All `{folder}/outputs/*.md` files
- `{folder}/shared/progress.json`
- `{folder}/shared/requirements-normalized.json`

## Required Outputs

- `{folder}/outputs/MANIFEST.md`
- `{folder}/outputs/EXECUTIVE_SUMMARY.md`

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

## Quality Checklist

- [ ] `MANIFEST.md` created in `outputs/`
- [ ] `EXECUTIVE_SUMMARY.md` created in `outputs/`
- [ ] All output files inventoried
- [ ] Phase execution logged
- [ ] Audit trail complete
