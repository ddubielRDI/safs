---
name: phase2d-coverage-win
expert-role: QA Engineer
domain-expertise: Coverage analysis, gap detection
---

# Phase 2d: Coverage Validation (BLOCKING GATE)

## Expert Role

You are a **QA Engineer** with deep expertise in:
- Coverage analysis and metrics
- Gap detection and reporting
- Validation testing
- Quality assurance processes

## Purpose

Validate 100% coverage of workflow requirements. This is a **BLOCKING GATE** - the pipeline cannot proceed until coverage is verified.

## Inputs

- `{folder}/shared/workflow-extracted-reqs.json` - Workflow-derived candidates
- `{folder}/shared/requirements-normalized.json` - Normalized requirements

## Required Outputs

- `{folder}/shared/workflow-coverage.json` - Coverage analysis results

## BLOCKING GATE

**This phase MUST PASS before proceeding to Phase 3.**

Gate conditions:
- 100% of workflow candidates mapped to requirements
- All workflow steps have corresponding requirements
- No unmapped workflow items

## Instructions

### Step 1: Load Data

```python
workflow_reqs = read_json(f"{folder}/shared/workflow-extracted-reqs.json")
normalized_reqs = read_json(f"{folder}/shared/requirements-normalized.json")

workflow_candidates = workflow_reqs.get("requirement_candidates", [])
all_requirements = normalized_reqs.get("requirements", [])
```

### Step 2: Build Coverage Matrix

```python
from difflib import SequenceMatcher

def similarity(a, b):
    """Calculate text similarity."""
    return SequenceMatcher(None, a.lower(), b.lower()).ratio()

def find_best_match(workflow_item, requirements, threshold=0.6):
    """Find best matching requirement for workflow item."""
    best_match = None
    best_score = 0

    wf_text = workflow_item.get("description", "")

    for req in requirements:
        req_text = req.get("text", "")
        score = similarity(wf_text, req_text)

        if score > best_score:
            best_score = score
            best_match = req

    if best_score >= threshold:
        return {
            "matched": True,
            "requirement_id": best_match.get("canonical_id"),
            "similarity": round(best_score, 2),
            "match_quality": "HIGH" if best_score >= 0.8 else "MEDIUM"
        }
    else:
        return {
            "matched": False,
            "best_candidate": best_match.get("canonical_id") if best_match else None,
            "similarity": round(best_score, 2),
            "match_quality": "LOW"
        }

coverage_matrix = []
for wf_item in workflow_candidates:
    match = find_best_match(wf_item, all_requirements)
    coverage_matrix.append({
        "workflow_id": wf_item.get("id"),
        # V2-F7 fix 2026-05-18: store full workflow description. Previous [:100]
        # truncation in persisted JSON destroyed evidence for downstream phases;
        # log display below already truncates for readability.
        "workflow_text": wf_item.get("description", ""),
        "category": wf_item.get("category"),
        **match
    })
```

### Step 3: Calculate Coverage Metrics

```python
def calculate_coverage(matrix):
    """Calculate coverage statistics."""
    total = len(matrix)
    matched = sum(1 for item in matrix if item.get("matched"))
    unmatched = total - matched

    coverage_pct = (matched / total * 100) if total > 0 else 100

    # HUNT-B-008 fix 2026-05-18: zero-input case (no workflow candidates from
    # Phase 2a) was previously vacuously passing — a BLOCKING gate that always
    # passes on empty input provides no defense against document misclassification.
    # Surface a structural warning that downstream phases / human reviewers can act
    # on. The gate still vacuously passes by design (nothing to BLOCK on), but the
    # zero-input signal is explicit in the output.
    zero_input_warning = None
    if total == 0:
        zero_input_warning = (
            "WARNING: Phase 2d coverage check ran with ZERO workflow candidates "
            "from Phase 2a. Possible causes: (a) Phase 2a extraction failed, "
            "(b) input documents are not actually an RFP (cover letter, specification "
            "document, scope summary), (c) RFP uses non-standard requirement language "
            "that Phase 2a's extractor didn't recognize. Verify input documents before "
            "relying on downstream Stage 3-7 outputs."
        )
        log(f"⚠️  {zero_input_warning}")

    # Quality breakdown
    high_quality = sum(1 for item in matrix if item.get("match_quality") == "HIGH")
    medium_quality = sum(1 for item in matrix if item.get("match_quality") == "MEDIUM")
    low_quality = sum(1 for item in matrix if item.get("match_quality") == "LOW")

    return {
        "total_workflow_items": total,
        "matched": matched,
        "unmatched": unmatched,
        "coverage_percentage": round(coverage_pct, 1),
        "quality_breakdown": {
            "high": high_quality,
            "medium": medium_quality,
            "low": low_quality
        },
        "gate_passed": coverage_pct == 100,
        "zero_input_warning": zero_input_warning,
    }

coverage_stats = calculate_coverage(coverage_matrix)
```

### Step 4: Identify Gaps

```python
def identify_gaps(matrix):
    """Identify unmatched workflow items."""
    gaps = []
    for item in matrix:
        if not item.get("matched"):
            gaps.append({
                "workflow_id": item["workflow_id"],
                "description": item["workflow_text"],
                "category": item.get("category"),
                "suggested_action": "Create new requirement or improve matching"
            })
    return gaps

gaps = identify_gaps(coverage_matrix)
```

### Step 5: Generate Coverage Report

```python
def generate_coverage_report(matrix, stats, gaps):
    """Generate detailed coverage report."""
    report = {
        "category_coverage": {},
        "requirement_usage": {},
        "unmapped_by_category": {}
    }

    # Coverage by workflow category
    categories = set(item.get("category") for item in matrix)
    for category in categories:
        cat_items = [item for item in matrix if item.get("category") == category]
        cat_matched = sum(1 for item in cat_items if item.get("matched"))
        report["category_coverage"][category] = {
            "total": len(cat_items),
            "matched": cat_matched,
            "percentage": round(cat_matched / len(cat_items) * 100, 1) if cat_items else 100
        }

    # Requirement usage (how many workflow items each req covers)
    for item in matrix:
        if item.get("matched"):
            req_id = item.get("requirement_id")
            if req_id not in report["requirement_usage"]:
                report["requirement_usage"][req_id] = 0
            report["requirement_usage"][req_id] += 1

    # Gaps by category
    for gap in gaps:
        cat = gap.get("category", "OTHER")
        if cat not in report["unmapped_by_category"]:
            report["unmapped_by_category"][cat] = []
        report["unmapped_by_category"][cat].append(gap["workflow_id"])

    return report

detailed_report = generate_coverage_report(coverage_matrix, coverage_stats, gaps)
```

### Step 6: Write Output

```python
coverage_output = {
    "analyzed_at": datetime.now().isoformat(),
    "gate_status": {
        "passed": coverage_stats["gate_passed"],
        "coverage_percentage": coverage_stats["coverage_percentage"],
        "required_percentage": 100,
        "gap_count": len(gaps)
    },
    "summary": coverage_stats,
    "coverage_matrix": coverage_matrix,
    "gaps": gaps,
    "detailed_report": detailed_report
}

write_json(f"{folder}/shared/workflow-coverage.json", coverage_output)
```

### Step 7: Handle Gate Status

```python
if not coverage_stats["gate_passed"]:
    # BLOCKING GATE FAILED
    log(f"\033[91m⛔ WORKFLOW COVERAGE GATE FAILED\033[0m")
    log(f"Coverage: {coverage_stats['coverage_percentage']}% (100% required)")
    log(f"Gaps: {len(gaps)} workflow items unmatched")
    log("")
    log("Unmatched Items:")
    for gap in gaps[:10]:
        # Display-only truncation (60 chars). Full text preserved in workflow-coverage.json.
        desc = gap.get('description', '')
        log(f"  ❌ {gap['workflow_id']}: {desc[:60]}{'...' if len(desc) > 60 else ''}")

    if len(gaps) > 10:
        log(f"  ... and {len(gaps) - 10} more")

    log("")
    log("Options:")
    log("  1. Create requirements for unmapped workflow items")
    log("  2. Adjust matching threshold")
    log("  3. User approval to proceed with gaps")

    raise BlockingGateFailure(
        phase="2d",
        condition="100% workflow coverage required",
        gaps=gaps
    )
else:
    log("✅ Workflow Coverage Gate PASSED (100%)")
```

### Step 8: Report Results

```
📊 Workflow Coverage Analysis Complete
=======================================
Gate Status: {"PASSED ✅" if gate_passed else "FAILED ❌"}

Coverage: {coverage_percentage}%
  Matched: {matched}/{total}
  Unmatched: {unmatched}

Quality Breakdown:
  HIGH (>80% match): {high}
  MEDIUM (60-80%): {medium}
  LOW (<60%): {low}

Coverage by Category:
| Category | Total | Matched | % |
|----------|-------|---------|---|
{table rows}

{if gaps}
Gaps Identified:
{gap_list}
{endif}
```

## Quality Checklist (MANDATORY — report each by name with evidence)

The phase agent MUST verify each of the following BEFORE reporting completion. The agent's completion report MUST include a checklist-results block with:
- Item name (verbatim from below)
- PASS / FAIL / SKIPPED-WITH-REASON
- Evidence (file:line citation, grep result, file size, assertion that ran, etc.)

"All checks passed" without per-item evidence is NOT acceptable.

### Required output files
1. **workflow-coverage.json** exists at `{folder}/shared/workflow-coverage.json` — evidence: `ls -la` size > 100 bytes and parses as valid JSON

### Schema fidelity
2. **workflow-coverage.json top-level keys** include `analyzed_at`, `gate_status`, `summary`, `coverage_matrix`, `gaps`, `detailed_report` — evidence: list actual top-level keys found
3. **gate_status** contains `passed` (bool), `coverage_percentage`, `required_percentage`, `gap_count` — evidence: print gate_status block
4. **coverage_matrix** entry count matches workflow-extracted-reqs.json requirement_candidates count — evidence: print both lengths
5. No `[:N]` slicing applied to deliverable content strings — evidence: grep for `\[:[0-9]+\]` in production code paths returned 0 hits; confirm workflow_text is stored FULL (no truncation per V2-F7 fix)

### Cross-stage consistency
6. **Zero-input warning surfaced** if workflow-extracted-reqs.json had 0 candidates — evidence: print `summary.zero_input_warning` value (null = no warning = normal)
7. **Gaps documented** for all unmatched workflow items — evidence: `len(gaps)` equals `summary.unmatched`; each gap has `workflow_id`, `description`, `suggested_action`

### Anti-regression rules (universal)
8. **UTF-8 encoding** on every `open()` call — evidence: search this phase's emitted scripts/code for `encoding='utf-8'` in every file-open
9. **ensure_ascii=False** on every `json.dump` call — evidence: same grep
10. **No `_Showing N of M_` row-cap notices** in any deliverable markdown — evidence: grep returned 0 matches
11. **No empty `|  |` mitigation/cell patterns** in any deliverable table — evidence: grep returned 0 matches
12. **No mid-word table-cell truncations** — evidence: line-by-line cell-end check returned 0 hits

### Memory discipline
13. **Relevant SAFS memory entries reviewed and applied** — evidence: list which memory files were read and which rules were applicable
