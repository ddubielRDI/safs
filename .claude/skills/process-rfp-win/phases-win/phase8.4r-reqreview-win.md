---
name: phase8.4r-reqreview-win
expert-role: Requirements Analyst
domain-expertise: Requirements response formatting, compliance matrix completion, tabular requirement responses
---

# Phase 8.4r: Requirements Review Response

## ⛔ NO-TRUNCATION DISCIPLINE (READ FIRST — BLOCKING)

**Render ALL CRITICAL/HIGH requirements (and ideally all 2,167 if size allows). Render FULL text per row.** Per SAFS memory (`feedback_screen_encoding_truncation.md`), the win pipeline regressed 2026-05-19 producing `_Showing 200 of 762_` row caps and mid-word `[:200]...` truncation in this very file's output. The rule:

- **NEVER `[:N]` slice requirement text, response prose, or any deliverable-content string.** Render every character of every requirement.
- **NEVER cap rows.** No `high_critical[:200]`, no `priority_filter` artificial limits. If 762 reqs are CRITICAL/HIGH, render 762 rows. The table will be long. That's correct.
- **NEVER emit "_Showing N of M_" notices.** Show everything.
- **`linked_spec_ids` and `bid_section_ids`** in any displayed cell render ALL entries (or join with commas) — not `[:3]` or `[:2]` head slices.

Pipelines produce FULL DATA. The PDF renderer handles pagination. Humans decide what to trim.

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
- `{folder}/outputs/bid-sections/04*.md` - Solution details (glob — matches 04a_SOLUTION_*.md, 04b_SOLUTION_*.md, 04_SOLUTION.md, etc.; V4-F11 fix 2026-05-18)

## Required Output

- `{folder}/outputs/bid-sections/04_REQUIREMENTS_REVIEW.md` (>8KB)

## Instructions

### Step 1: Load Data

```python
import glob

requirements = read_json(f"{folder}/shared/requirements-normalized.json")
rtm = read_json_safe(f"{folder}/shared/UNIFIED_RTM.json")
compliance = read_json(f"{folder}/shared/COMPLIANCE_MATRIX.json")

all_reqs = requirements.get("requirements", [])
rtm_reqs = rtm.get("entities", {}).get("requirements", []) if rtm else []
rtm_specs = rtm.get("entities", {}).get("specifications", []) if rtm else []

# V4-F11 fix 2026-05-18: load all 04*.md solution files via glob so split-file
# layouts (04a_SOLUTION_Architecture.md, 04b_SOLUTION_Implementation.md, etc.)
# are not silently missed. Previous singular 04_SOLUTION.md reference broke
# under any RFP that required multi-volume solution detail.
solution_md_files = sorted(glob.glob(f"{folder}/outputs/bid-sections/04*.md"))
solution_content = ""
for sf in solution_md_files:
    solution_content += read_file(sf) + "\n\n"
log(f"Loaded {len(solution_md_files)} solution file(s) for cross-reference: {[os.path.basename(f) for f in solution_md_files]}")
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
    # 2026-05-19 NO-TRUNCATION FIX: removed linked_spec_ids[:3] cap. Render ALL
    # linked specs for the requirement.
    if rtm_req and rtm_req.get("linked_spec_ids"):
        spec_names = []
        for sid in rtm_req["linked_spec_ids"]:
            for s in rtm_specs:
                if s.get("spec_id") == sid:
                    spec_names.append(s.get("title", sid))
                    break
        response = f"Addressed in: {', '.join(spec_names)}"
    else:
        response = "Addressed in Technical Approach"

    # Build bid reference
    # 2026-05-19 NO-TRUNCATION FIX: removed bid_section_ids[:2] cap.
    bid_ref = ""
    if rtm_req and rtm_req.get("bid_section_ids"):
        bid_ref = ", ".join(rtm_req["bid_section_ids"])

    response_rows.append({
        "req_id": req_id,
        "category": category,
        "priority": priority,
        # NO TRUNCATION — this is the evaluator-facing requirements review row.
        # Markdown / PDF table cells wrap natively; slicing produces visible cut-offs
        # in the bid submission. (Removed [:200] 2026-05-18.)
        "requirement": text,
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

## Quality Checklist (MANDATORY — report each by name with evidence)

The phase agent MUST verify each of the following BEFORE reporting completion. The agent's completion report MUST include a checklist-results block with:
- Item name (verbatim from below)
- PASS / FAIL / SKIPPED-WITH-REASON
- Evidence (file:line citation, grep result, file size, assertion that ran, etc.)

"All checks passed" without per-item evidence is NOT acceptable.

### Required output files
1. **04_REQUIREMENTS_REVIEW.md** exists at `{folder}/outputs/bid-sections/04_REQUIREMENTS_REVIEW.md` — evidence: `ls -la` showing size > 8,192 bytes

### Schema fidelity
2. **No `_Showing N of M_` row-cap notices** — evidence: grep "_Showing" in 04_REQUIREMENTS_REVIEW.md returned 0 matches
3. **No mid-word truncations** in requirement text cells — evidence: spot-check 5 random rows; confirm text is not cut off mid-word (no trailing `...` from a `[:200]` slice)
4. **`linked_spec_ids` and `bid_section_ids` cells render ALL entries** — no `[:3]` or `[:2]` head slices — evidence: confirm these fields were rendered in full per 2026-05-19 fix
5. No `[:N]` slicing applied to deliverable content strings — evidence: grep for `\[:[0-9]+\]` in production code paths returned 0 hits

### Cross-stage consistency
6. **Every requirement has a response row** — `len(response_rows)` equals `len(all_reqs)` — evidence: print both counts
7. **Compliance status determined for each requirement** — every row has one of COMPLIANT / PARTIAL / ALTERNATE / NON-COMPLIANT — evidence: count rows with null/missing status (must be 0)
8. **Summary statistics accurate** — `compliant_count + partial_count + non_compliant_count` equals total row count — evidence: print all three counts and their sum vs total
9. **Category breakdown included** — grep "Category" or "## Category" in document returned >= 1 hit — evidence: grep result

### Anti-regression rules (universal)
10. **UTF-8 encoding** on every `open()` call — evidence: search this phase's emitted scripts/code for `encoding='utf-8'` in every file-open
11. **ensure_ascii=False** on every `json.dump` call — evidence: same grep
12. **No empty `|  |` cells in requirement/response table** — evidence: grep for empty cells adjacent to requirement text returned 0 matches
13. **No mid-word table-cell truncations** — evidence: line-by-line cell-end check returned 0 hits

### Memory discipline
14. **Relevant SAFS memory entries reviewed and applied** — evidence: list which memory files were read and which rules were applicable (e.g., "NEVER cap rows with high_critical[:200] — rendered ALL CRITICAL/HIGH requirements per 2026-05-19 fix")
