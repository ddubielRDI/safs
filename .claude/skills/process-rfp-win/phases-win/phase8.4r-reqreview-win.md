---
name: phase8.4r-reqreview-win
expert-role: Requirements Analyst
domain-expertise: Requirements response formatting, compliance matrix completion, tabular requirement responses
---

# Phase 8.4r: Requirements Review Response

## Expert Role

You are a **Requirements Analyst** with expertise in:
- Structured requirements response formatting
- Government RFP compliance matrix completion
- Tabular requirement-by-requirement responses
- Cross-referencing solutions to individual requirements

## Purpose

Generate a tabular requirements review response. Many RFPs include an "Attachment A" or requirements matrix that bidders must complete. This phase generates that response — showing each requirement and how the solution addresses it.

## Inputs

- `{folder}/shared/requirements-normalized.json` - All requirements
- `{folder}/shared/UNIFIED_RTM.json` - Traceability (requirement → spec → bid section links)
- `{folder}/shared/COMPLIANCE_MATRIX.json` - Mandatory items
- `{folder}/outputs/bid-sections/03_TECHNICAL.md` - Technical approach (for cross-references)
- `{folder}/outputs/bid-sections/04_SOLUTION.md` - Solution details (for cross-references)

## Required Output

- `{folder}/outputs/bid-sections/04_REQUIREMENTS_REVIEW.md` (>8KB)

## Instructions

### Step 1: Load Data

```python
requirements = read_json(f"{folder}/shared/requirements-normalized.json")
rtm = read_json_safe(f"{folder}/shared/UNIFIED_RTM.json")
compliance = read_json(f"{folder}/shared/COMPLIANCE_MATRIX.json")

all_reqs = requirements.get("requirements", [])
rtm_reqs = rtm.get("entities", {}).get("requirements", []) if rtm else []
rtm_specs = rtm.get("entities", {}).get("specifications", []) if rtm else []
```

### Step 2: Build Response Matrix

```python
# For each requirement, determine:
# - Compliance status: COMPLIANT / PARTIAL / ALTERNATE / NON-COMPLIANT
# - Response: Brief description of how addressed
# - Reference: Where in bid it's addressed (section + page)

response_rows = []

for req in all_reqs:
    req_id = req.get("requirement_id", req.get("id", ""))
    text = req.get("text", "")
    priority = req.get("priority", "MEDIUM")
    category = req.get("category", "General")

    # Find RTM data for this requirement
    rtm_req = None
    for r in rtm_reqs:
        if r.get("requirement_id") == req_id:
            rtm_req = r
            break

    # Determine compliance status
    has_specs = bool(rtm_req and rtm_req.get("linked_spec_ids"))
    compliance_status = "COMPLIANT" if has_specs else "PARTIAL"

    # Build response text
    if rtm_req and rtm_req.get("linked_spec_ids"):
        spec_names = []
        for sid in rtm_req["linked_spec_ids"][:3]:
            for s in rtm_specs:
                if s.get("spec_id") == sid:
                    spec_names.append(s.get("title", sid))
                    break
        response = f"Addressed in: {', '.join(spec_names)}"
    else:
        response = "Addressed in Technical Approach"

    # Build bid reference
    bid_ref = ""
    if rtm_req and rtm_req.get("bid_section_ids"):
        bid_ref = ", ".join(rtm_req["bid_section_ids"][:2])

    response_rows.append({
        "req_id": req_id,
        "category": category,
        "priority": priority,
        "requirement": text[:200],
        "status": compliance_status,
        "response": response,
        "bid_reference": bid_ref
    })
```

### Step 3: Generate Requirements Response Document

```markdown
# Requirements Review Response

## Response Summary

| Metric | Count |
|--------|-------|
| Total Requirements | {len(all_reqs)} |
| COMPLIANT | {compliant_count} |
| PARTIAL | {partial_count} |
| NON-COMPLIANT | {non_compliant_count} |
| Compliance Rate | {compliance_pct}% |

## Requirements Response Matrix

| Req ID | Category | Priority | Requirement | Status | Response | Bid Reference |
|--------|----------|----------|-------------|--------|----------|---------------|
{for each row: formatted table row}

## Category Summary

{for each category: count + compliance rate}

## Notes

- COMPLIANT: Fully addressed in proposed solution with specification coverage
- PARTIAL: Addressed conceptually; detailed design in implementation phase
- NON-COMPLIANT: Cannot be addressed; alternative approach proposed
```

### Step 4: Write Output

```python
write_file(f"{folder}/outputs/bid-sections/04_REQUIREMENTS_REVIEW.md", review_content)

log(f"""
📋 REQUIREMENTS REVIEW COMPLETE (Phase 8.4r)
=============================================
Total Requirements: {len(all_reqs)}
COMPLIANT: {compliant_count} ({compliant_pct:.0f}%)
PARTIAL: {partial_count}
Categories Covered: {len(categories)}

Output: outputs/bid-sections/04_REQUIREMENTS_REVIEW.md
""")
```

## Quality Checklist

- [ ] `04_REQUIREMENTS_REVIEW.md` created (>8KB)
- [ ] Every requirement has a response row
- [ ] Compliance status determined for each requirement
- [ ] Bid section references populated from RTM where available
- [ ] Summary statistics accurate
- [ ] Category breakdown included
