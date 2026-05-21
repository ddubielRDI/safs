---
name: sva2-pink-team-win
expert-role: Pink Team Reviewer
domain-expertise: Requirements completeness, deduplication quality, compliance mapping, Shipley Pink Team review
---

# SVA-2: Pink Team Review

## ⛔ SCHEMA-PATH CONTRACT (codified 2026-05-20 — MARS Pink-Team post-retry-3 finding)

When reading `compliance_matrix.mandatory_items[].linked_requirement_ids[]` for Phase 1.7 cross-stage trace audits, the AUTHORITATIVE path is:

```
compliance_matrix["rtm_entities"]["mandatory_items"][i]["linked_requirement_ids"]
```

NOT the top-level `compliance_matrix["mandatory_items"][i]["linked_requirement_ids"]`. The top-level array uses field names per Phase 1.7 Step 7 schema (`id`, `coverage`, etc.) and does NOT carry the linked_requirement_ids field. The RTM entity (Step 6b) uses canonical RTM names and IS where Phase 1.7's `backfill_linked_ids()` step writes the linkages.

SVA-2 implementations that read top-level mandatory_items will report 0% linkage even when 66%+ is populated at rtm_entities path. This is a latent SVA-2 skill bug surfaced by the MARS 2026-05-20 retry-3 cycle.

## Expert Role

You are a **Pink Team Reviewer** conducting the first formal review (~25% completion). Your focus:
- Requirements completeness and source coverage
- Deduplication quality (not too aggressive, not too loose)
- Compliance-to-requirement mapping integrity
- Strategy readiness for specification phase

## Purpose

Validate that Stage 2 (Requirements Engineering) produced a complete, well-structured requirements set ready for specification development. This is the Shipley **Pink Team** review — checking that the "storyboard" (requirements structure) is solid before investing in detailed specs.

## Inputs

- `{folder}/shared/requirements-raw.json` - Raw extracted requirements
- `{folder}/shared/requirements-normalized.json` - Normalized/deduped requirements
- `{folder}/shared/COMPLIANCE_MATRIX.json` - Mandatory items
- `{folder}/shared/EVALUATION_CRITERIA.json` - Evaluation factors
- `{folder}/shared/domain-context.json` - Domain context
- `{folder}/shared/workflow-extracted-reqs.json` - Workflow requirements (optional)
- `{folder}/shared/sample-data-analysis.json` - Sample data entities (optional)
- `{folder}/flattened/*.md` - Flattened RFP documents

## Required Output

- `{folder}/shared/validation/sva2-pink-team.json` - Pink Team review report

## Instructions

### Step 1: Load Data

```python
from datetime import datetime
import re
import glob

raw_reqs = read_json(f"{folder}/shared/requirements-raw.json")
norm_reqs = read_json(f"{folder}/shared/requirements-normalized.json")
compliance = read_json(f"{folder}/shared/COMPLIANCE_MATRIX.json")
evaluation = read_json(f"{folder}/shared/EVALUATION_CRITERIA.json")
domain = read_json(f"{folder}/shared/domain-context.json")

# Optional
workflow_reqs = read_json_safe(f"{folder}/shared/workflow-extracted-reqs.json")
sample_data = read_json_safe(f"{folder}/shared/sample-data-analysis.json")

# Load flattened docs for source coverage check
flattened_files = glob.glob(f"{folder}/flattened/*.md")
combined_rfp_text = ""
for fp in flattened_files:
    combined_rfp_text += read_file(fp) + "\n\n"

raw_count = len(raw_reqs.get("requirements", []))
norm_count = len(norm_reqs.get("requirements", []))
all_reqs = norm_reqs.get("requirements", [])
mandatory_items = compliance.get("mandatory_items", compliance.get("rtm_entities", {}).get("mandatory_items", []))

findings = []
```

### Step 2: Execute Rule Checks

```python
# --- SVA2-REQ-SOURCE-COVERAGE (CRITICAL) ---
def check_source_coverage():
    """Every RFP section with SHALL/MUST should have requirements traced to it."""
    # Find all SHALL/MUST locations in flattened text
    shall_must_pattern = r'(?:shall|must)\s+(?:be\s+)?(?:required\s+to\s+)?'
    shall_locations = list(re.finditer(shall_must_pattern, combined_rfp_text, re.IGNORECASE))
    total_shall = len(shall_locations)

    # Count how many req source_ids reference locations
    reqs_with_sources = sum(1 for r in all_reqs if r.get("source_ids") or r.get("source_location"))

    # Score: ratio of requirements with source tracking
    if norm_count > 0:
        source_ratio = reqs_with_sources / norm_count
    else:
        source_ratio = 0

    # Also check: do we have at least 1 req per 2 SHALL/MUST occurrences?
    coverage_ratio = norm_count / max(total_shall, 1)

    passed = source_ratio >= 0.8 and coverage_ratio >= 0.3
    score = min(100, source_ratio * 60 + coverage_ratio * 40)

    return {
        "rule_id": "SVA2-REQ-SOURCE-COVERAGE",
        "severity": "CRITICAL",
        "passed": passed,
        "score": round(score, 1),
        "threshold": 80.0,
        "details": {
            "shall_must_occurrences": total_shall,
            "requirements_extracted": norm_count,
            "reqs_with_source_tracking": reqs_with_sources,
            "source_tracking_ratio": round(source_ratio, 2),
            "extraction_coverage_ratio": round(coverage_ratio, 2)
        },
        "corrective_action": {
            "type": "retry_phase",
            "target_phase": "2",
            "auto_correctable": True
        } if not passed else None
    }

findings.append(check_source_coverage())


# --- SVA2-REQ-DEDUP-QUALITY (HIGH) ---
def check_dedup_quality():
    """Reduction ratio (raw to normalized) should be 10-40%."""
    if raw_count == 0:
        return {
            "rule_id": "SVA2-REQ-DEDUP-QUALITY", "severity": "HIGH",
            "passed": False, "score": 0, "threshold": None,
            "details": {"error": "No raw requirements found"},
            "corrective_action": {"type": "retry_phase", "target_phase": "2", "auto_correctable": True}
        }

    reduction = 1 - (norm_count / raw_count)
    in_range = 0.10 <= reduction <= 0.40

    # Score peaks at 25% reduction
    if in_range:
        score = 100 - abs(reduction - 0.25) * 200
    elif reduction < 0.10:
        score = max(0, 50 + reduction * 500)  # Below range
    else:
        score = max(0, 50 - (reduction - 0.40) * 200)  # Above range

    return {
        "rule_id": "SVA2-REQ-DEDUP-QUALITY",
        "severity": "HIGH",
        "passed": in_range,
        "score": round(max(0, min(100, score)), 1),
        "threshold": "10-40% reduction",
        "details": {
            "raw_count": raw_count,
            "normalized_count": norm_count,
            "reduction_ratio": round(reduction, 3),
            "assessment": "OK" if in_range else ("Under-deduped" if reduction < 0.10 else "Over-merged")
        },
        "corrective_action": {
            "type": "retry_phase",
            "target_phase": "2b",
            "auto_correctable": True
        } if not in_range else None
    }

findings.append(check_dedup_quality())


# --- SVA2-REQ-PRIORITY-DIST (MEDIUM) ---
def check_priority_distribution():
    """CRITICAL 5-15%, HIGH 20-35%, MEDIUM 35-55%, LOW 10-25%."""
    dist = {"CRITICAL": 0, "HIGH": 0, "MEDIUM": 0, "LOW": 0}
    for req in all_reqs:
        p = req.get("priority", "MEDIUM")
        dist[p] = dist.get(p, 0) + 1

    pcts = {k: v / norm_count * 100 if norm_count else 0 for k, v in dist.items()}

    RANGES = {
        "CRITICAL": (5, 15),
        "HIGH": (20, 35),
        "MEDIUM": (35, 55),
        "LOW": (10, 25)
    }

    violations = []
    for level, (lo, hi) in RANGES.items():
        pct = pcts[level]
        if pct < lo or pct > hi:
            violations.append(f"{level}: {pct:.1f}% (expected {lo}-{hi}%)")

    score = max(0, 100 - len(violations) * 25)
    return {
        "rule_id": "SVA2-REQ-PRIORITY-DIST",
        "severity": "MEDIUM",
        "passed": len(violations) <= 1,
        "score": round(score, 1),
        "threshold": "Within ranges",
        "details": {
            "distribution": {k: {"count": dist[k], "pct": round(pcts[k], 1)} for k in dist},
            "violations": violations
        },
        "corrective_action": None
    }

findings.append(check_priority_distribution())


# --- SVA2-REQ-CATEGORY-COVERAGE (HIGH) ---
def check_category_coverage():
    """Requirements span at least 5 categories."""
    categories = set()
    for req in all_reqs:
        cat = req.get("category", "OTHER")
        categories.add(cat)

    frameworks = domain.get("compliance_frameworks", [])
    framework_coverage = []
    for fw in frameworks:
        fw_lower = fw.lower()
        has_req = any(fw_lower in req.get("text", "").lower() for req in all_reqs)
        framework_coverage.append({"framework": fw, "has_requirement": has_req})

    cat_count = len(categories)
    passed = cat_count >= 5
    score = min(100, cat_count / 5 * 100)

    return {
        "rule_id": "SVA2-REQ-CATEGORY-COVERAGE",
        "severity": "HIGH",
        "passed": passed,
        "score": round(score, 1),
        "threshold": 5,
        "details": {
            "category_count": cat_count,
            "categories": sorted(categories),
            "framework_coverage": framework_coverage
        },
        "corrective_action": {
            "type": "retry_phase", "target_phase": "2", "auto_correctable": True
        } if not passed else None
    }

findings.append(check_category_coverage())


# --- SVA2-COMPLIANCE-REQ-MAPPING (CRITICAL) ---
def check_compliance_mapping():
    """Every mandatory item maps to at least one requirement."""
    total = len(mandatory_items)
    if total == 0:
        return {
            "rule_id": "SVA2-COMPLIANCE-REQ-MAPPING", "severity": "CRITICAL",
            "passed": True, "score": 100, "threshold": 100,
            "details": {"note": "No mandatory items to check"},
            "corrective_action": None
        }

    mapped = 0
    unmapped = []
    for m in mandatory_items:
        linked = m.get("linked_requirement_ids", [])
        # Also check coverage_status
        status = m.get("coverage_status", m.get("coverage", {}).get("status", ""))
        if linked or status in ["ADDRESSED", "PLANNED"]:
            mapped += 1
        else:
            unmapped.append(m.get("mandatory_id", m.get("id", "?")))

    coverage_pct = mapped / total * 100
    passed = coverage_pct >= 90

    return {
        "rule_id": "SVA2-COMPLIANCE-REQ-MAPPING",
        "severity": "CRITICAL",
        "passed": passed,
        "score": round(coverage_pct, 1),
        "threshold": 90.0,
        "details": {
            "total_mandatory": total,
            "mapped_to_requirements": mapped,
            "unmapped": unmapped[:10],
            "coverage_pct": round(coverage_pct, 1)
        },
        "corrective_action": {
            "type": "retry_phase", "target_phase": "2", "auto_correctable": True
        } if not passed else None
    }

findings.append(check_compliance_mapping())


# --- SVA2-WORKFLOW-SCHEMA-CONTRACT (CRITICAL) ---
# HUNT-C-0006 fix 2026-05-18: workflow-extracted-reqs.json is consumed by phase3d
# (demo skill) which reads specific keys (category_distribution as DICT,
# requirement_candidates[] as list-of-dicts with .category field). SVA-2
# previously did NOT verify these keys exist — a structurally wrong JSON file
# with correct text-content alignment would pass SVA-2 while crashing phase3d.
def check_workflow_schema_contract():
    """Verify workflow-extracted-reqs.json has the schema fields downstream consumers expect."""
    if not workflow_reqs:
        return {
            "rule_id": "SVA2-WORKFLOW-SCHEMA-CONTRACT", "severity": "CRITICAL",
            "passed": True, "score": 100, "threshold": None,
            "details": {"note": "No workflow_reqs loaded — schema check skipped"},
            "corrective_action": None
        }
    missing_keys = []
    if "category_distribution" not in workflow_reqs:
        missing_keys.append("category_distribution (DICT — required by phase3d demo skill)")
    elif not isinstance(workflow_reqs.get("category_distribution"), dict):
        missing_keys.append(f"category_distribution (wrong type: expected DICT, got {type(workflow_reqs.get('category_distribution')).__name__})")
    if "requirement_candidates" not in workflow_reqs:
        missing_keys.append("requirement_candidates (LIST — required by phase3d demo skill)")
    elif not isinstance(workflow_reqs.get("requirement_candidates"), list):
        missing_keys.append(f"requirement_candidates (wrong type: expected LIST, got {type(workflow_reqs.get('requirement_candidates')).__name__})")
    else:
        # Spot-check first 3 items for required .category field
        sample = workflow_reqs["requirement_candidates"][:3]
        items_missing_cat = [i for i, item in enumerate(sample) if not isinstance(item, dict) or "category" not in item]
        if items_missing_cat:
            missing_keys.append(f"requirement_candidates[].category (missing on items {items_missing_cat} of first 3 sampled)")

    passed = len(missing_keys) == 0
    return {
        "rule_id": "SVA2-WORKFLOW-SCHEMA-CONTRACT",
        "severity": "CRITICAL",
        "passed": passed,
        "score": 100 if passed else 0,
        "threshold": None,
        "details": {
            "missing_or_invalid_keys": missing_keys,
            "consumer_skill": "phase3d-demos-win.md (currently restoring; demos pipeline)"
        },
        "corrective_action": {
            "type": "retry_phase", "target_phase": "2a", "auto_correctable": True,
            "instruction": f"Phase 2a must emit workflow-extracted-reqs.json with: {missing_keys}"
        } if not passed else None
    }

findings.append(check_workflow_schema_contract())


# --- SVA2-WORKFLOW-ALIGNMENT (CRITICAL) ---
def check_workflow_alignment():
    """Verify workflow requirements align with extracted requirements."""
    if not workflow_reqs:
        return {
            "rule_id": "SVA2-WORKFLOW-ALIGNMENT", "severity": "CRITICAL",
            "passed": True, "score": 100, "threshold": None,
            "details": {"note": "No workflow requirements to validate"},
            "corrective_action": None
        }

    # V2-F5 fix: Phase 2a writes the list under `requirement_candidates`, not
    # `requirements` / `workflow_requirements`. Read the canonical key first;
    # legacy keys preserved as fallback so this gate keeps working on older runs.
    wf_reqs = workflow_reqs.get(
        "requirement_candidates",
        workflow_reqs.get("requirements", workflow_reqs.get("workflow_requirements", []))
    )
    if not wf_reqs:
        return {
            "rule_id": "SVA2-WORKFLOW-ALIGNMENT", "severity": "CRITICAL",
            "passed": True, "score": 100, "threshold": None,
            "details": {"note": "Workflow file exists but has no requirements"},
            "corrective_action": None
        }

    # Check alignment: each workflow req should match at least one normalized req
    aligned = 0
    for wf in wf_reqs:
        # V2-F5 fix: Phase 2a candidate items use `description` as the text field;
        # `text`/`requirement` retained as fallbacks for legacy runs.
        wf_text = wf.get("description", wf.get("text", wf.get("requirement", ""))).lower()[:100]
        # Simple check: any normalized req shares significant words
        for req in all_reqs:
            req_text = req.get("text", "").lower()
            shared_words = set(wf_text.split()) & set(req_text.split())
            if len(shared_words) >= 3:
                aligned += 1
                break

    ratio = aligned / len(wf_reqs) if wf_reqs else 1
    passed = ratio >= 0.7

    return {
        "rule_id": "SVA2-WORKFLOW-ALIGNMENT",
        "severity": "CRITICAL",
        "passed": passed,
        "score": round(ratio * 100, 1),
        "threshold": 70.0,
        "details": {
            "workflow_requirements": len(wf_reqs),
            "aligned_to_normalized": aligned,
            "alignment_ratio": round(ratio, 2)
        },
        "corrective_action": {
            "type": "retry_phase", "target_phase": "2", "auto_correctable": True
        } if not passed else None
    }

findings.append(check_workflow_alignment())


# --- SVA2-SAMPLE-DATA-INTEGRATION (MEDIUM) ---
def check_sample_data():
    """Data entities from sample data appear in requirements."""
    if not sample_data:
        return {
            "rule_id": "SVA2-SAMPLE-DATA-INTEGRATION", "severity": "MEDIUM",
            "passed": True, "score": 100, "threshold": None,
            "details": {"note": "No sample data analysis available"},
            "corrective_action": None
        }

    entities = sample_data.get("entities", sample_data.get("data_entities", []))
    if not entities:
        # HUNT-C-0007 fix 2026-05-18: previously returned PASS score=100 vacuously
        # when entities[] was empty, silently masking the case where phase2.5 found
        # no entities OR the file was structurally empty. Now: emit a structural
        # warning at score 50 (passes the >=50% threshold but visible in report).
        # Distinguish the legitimate-empty case (no spreadsheets in RFP) from a
        # likely extraction failure by checking field_definitions presence.
        has_field_defs = bool(sample_data.get("field_definitions"))
        spreadsheet_count = len(sample_data.get("spreadsheet_analysis", []))
        note = (
            f"Empty entities[]. spreadsheet_analysis count={spreadsheet_count}, "
            f"field_definitions={'present' if has_field_defs else 'absent'}. "
            "If RFP genuinely has no sample data attachments, this is expected. "
            "If RFP has .xlsx/.csv attachments but entities is empty, phase2.5 may have failed."
        )
        # If there ARE spreadsheets but no entities, that's a likely extraction bug — fail.
        passed = spreadsheet_count == 0
        return {
            "rule_id": "SVA2-SAMPLE-DATA-INTEGRATION", "severity": "MEDIUM",
            "passed": passed,
            "score": 100 if passed else 30,
            "threshold": 50.0,
            "details": {"note": note, "spreadsheet_count": spreadsheet_count},
            "corrective_action": {
                "type": "retry_phase", "target_phase": "2.5", "auto_correctable": True,
                "instruction": "Spreadsheets present but no entities extracted — re-run phase 2.5"
            } if not passed else None
        }

    all_req_text = " ".join(r.get("text", "").lower() for r in all_reqs)
    found = 0
    for entity in entities:
        name = entity.get("name", entity.get("entity", "")).lower()
        if name and name in all_req_text:
            found += 1

    ratio = found / len(entities) if entities else 1
    return {
        "rule_id": "SVA2-SAMPLE-DATA-INTEGRATION",
        "severity": "MEDIUM",
        "passed": ratio >= 0.5,
        "score": round(ratio * 100, 1),
        "threshold": 50.0,
        "details": {
            "total_entities": len(entities),
            "found_in_requirements": found,
            "ratio": round(ratio, 2)
        },
        "corrective_action": None
    }

findings.append(check_sample_data())


# --- SVA2-DELIVERABLE-NO-ROW-CAPS (HIGH) ---
def check_deliverable_no_row_caps():
    """
    HIGH: Catch _Showing N of M_ row-cap notices and empty mitigation cells (|  |)
    in outputs/*.md files before the bid enters specification phase.

    Two patterns are fatal at the evaluator-PDF level:
    1. _Showing N of M_ — a table renderer pagination notice exposed as prose,
       e.g., '_Showing 15 of 281 risks_'. Signals the table generator capped rows
       before writing all content.
    2. Empty mitigation cell: a pipe-delimited row containing a risk-severity label
       (HIGH/MEDIUM/CRITICAL/LOW) and a cell that is entirely whitespace between
       pipes. In a risk register, this exposes an unfilled Mitigation column.

    Counterfactual: would have caught '_Showing 15 of 281 risks_' and '_Showing 200
    of 762_' visible in evaluator PDFs during the 2026-05-18 rfp-mars run, and the
    empty '|  |' mitigation column for every risk row in 04_RISK_REGISTER.md.
    """
    import re as _re
    import glob as _glob

    ROW_CAP_PATTERN = _re.compile(r"_Showing\s+\d+\s+of\s+\d+")
    # Empty cell = two or more spaces (or just a single space) between two pipes,
    # on a line that also contains a risk-severity token.
    EMPTY_CELL_PATTERN = _re.compile(r"\|\s{1,}\|")
    SEVERITY_TOKEN = _re.compile(r"\|\s*(?:HIGH|MEDIUM|CRITICAL|LOW)\s*\|", _re.IGNORECASE)

    row_cap_hits = []
    empty_mit_hits = []

    scan_dirs = [
        f"{folder}/outputs/bid-sections",
        f"{folder}/outputs",
    ]
    files_scanned = 0

    for scan_dir in scan_dirs:
        for fpath in _glob.glob(f"{scan_dir}/*.md"):
            files_scanned += 1
            try:
                with open(fpath, "r", encoding="utf-8") as _fh:
                    lines = _fh.readlines()
            except (OSError, UnicodeDecodeError):
                continue
            fname = os.path.basename(fpath)
            for line_no, line in enumerate(lines, 1):
                if ROW_CAP_PATTERN.search(line):
                    row_cap_hits.append({
                        "file": fname,
                        "line_no": line_no,
                        "text": line.strip()[:120]
                    })
                if EMPTY_CELL_PATTERN.search(line) and SEVERITY_TOKEN.search(line):
                    empty_mit_hits.append({
                        "file": fname,
                        "line_no": line_no,
                        "text": line.strip()[:120]
                    })

    total_violations = len(row_cap_hits) + len(empty_mit_hits)
    passed = total_violations == 0
    score = 100.0 if passed else max(0.0, 100.0 - total_violations * 10)

    return {
        "rule_id": "SVA2-DELIVERABLE-NO-ROW-CAPS",
        "rule_name": "Deliverable Table Row Cap Check",
        "category": "Content Quality",
        "severity": "HIGH",
        "passed": passed,
        "score": round(score, 1),
        "threshold": 0,
        "details": {
            "files_scanned": files_scanned,
            "row_cap_violations": len(row_cap_hits),
            "empty_mitigation_violations": len(empty_mit_hits),
            "row_cap_hits": row_cap_hits[:10],
            "empty_mitigation_hits": empty_mit_hits[:10]
        },
        "corrective_action": None if passed else {
            "type": "retry_phase",
            "target_phase": "8",
            "auto_correctable": True,
            "instruction": (
                f"{'_Showing N of M_ row-cap notice in ' + str(len(row_cap_hits)) + ' location(s) — re-run table generation without row limits. ' if row_cap_hits else ''}"
                f"{'Empty mitigation cell (|  |) in ' + str(len(empty_mit_hits)) + ' risk-severity row(s) — re-run risk register generation to populate Mitigation column. ' if empty_mit_hits else ''}"
            ).strip()
        }
    }

findings.append(check_deliverable_no_row_caps())


# --- SVA2-STRATEGY-READINESS (HIGH) ---
def check_strategy_readiness():
    """Minimum: 50+ reqs, 3+ categories, eval criteria, compliance gate passed."""
    checks = {
        "sufficient_requirements": norm_count >= 50,
        "category_diversity": len(set(r.get("category", "") for r in all_reqs)) >= 3,
        "evaluation_criteria_present": bool(evaluation.get("evaluation_factors", evaluation.get("factors"))),
        "compliance_gate_passed": compliance.get("gate_status", {}).get("passed", True)
    }

    passed_checks = sum(1 for v in checks.values() if v)
    all_passed = all(checks.values())
    score = passed_checks / len(checks) * 100

    return {
        "rule_id": "SVA2-STRATEGY-READINESS",
        "severity": "HIGH",
        "passed": all_passed,
        "score": round(score, 1),
        "threshold": 100.0,
        "details": checks,
        "corrective_action": {
            "type": "user_review", "target_phase": None, "auto_correctable": False
        } if not all_passed else None
    }

findings.append(check_strategy_readiness())
```

### Step 3: Calculate Disposition

```python
def calculate_disposition(findings):
    has_critical_fail = any(f["severity"] == "CRITICAL" and not f["passed"] for f in findings)
    has_high_fail = any(f["severity"] == "HIGH" and not f["passed"] for f in findings)

    if has_critical_fail:
        return "BLOCK"
    elif has_high_fail:
        return "ADVISORY"
    else:
        return "PASS"

disposition = calculate_disposition(findings)
passed_count = sum(1 for f in findings if f["passed"])
total_count = len(findings)
overall_score = sum(f["score"] for f in findings) / total_count if total_count else 0
```

### Step 4: Build Pink Team Report

```python
# Pink Team specific analysis
pink_team_report = {
    "review_type": "Pink Team (~25% Completion)",
    "storyboard_assessment": {
        "requirements_foundation": "SOLID" if norm_count >= 50 else "WEAK",
        "compliance_coverage": f"{sum(1 for m in mandatory_items if m.get('linked_requirement_ids') or m.get('coverage_status') == 'ADDRESSED')}/{len(mandatory_items)} mandatory items linked",
        "category_breadth": f"{len(set(r.get('category', '') for r in all_reqs))} categories",
        "priority_balance": "Balanced" if findings[2]["passed"] else "Skewed"
    },
    "readiness_for_specs": disposition != "BLOCK",
    "key_strengths": [
        f["rule_id"] for f in findings if f["passed"] and f["severity"] in ["CRITICAL", "HIGH"]
    ],
    "key_concerns": [
        {"rule": f["rule_id"], "score": f["score"], "severity": f["severity"]}
        for f in findings if not f["passed"]
    ],
    "recommendation": (
        "Requirements foundation is solid. Proceed to Stage 3 (Specifications)."
        if disposition == "PASS" else
        "Requirements have gaps. Review concerns before proceeding."
        if disposition == "ADVISORY" else
        "Critical requirements gaps detected. Must address before specifications."
    )
}
```

### Step 5: Write Report

```python
report = {
    "validator": "SVA-2",
    "stage_validated": 2,
    "validated_at": datetime.now().isoformat(),
    "disposition": disposition,
    "color_team": "pink",
    "summary": {
        "total_rules": total_count,
        "passed": passed_count,
        "failed": total_count - passed_count,
        "overall_score": round(overall_score, 1)
    },
    "findings": findings,
    "color_team_report": pink_team_report
}

write_json(f"{folder}/shared/validation/sva2-pink-team.json", report)
```

### Step 6: Report Results

```
🩷 PINK TEAM REVIEW COMPLETE (SVA-2)
======================================
Disposition: {disposition} {"✅" if disposition == "PASS" else "⚠️" if disposition == "ADVISORY" else "❌"}
Score: {overall_score:.0f}/100
Rules: {passed_count}/{total_count} passed

Findings:
{for each finding: icon + rule_id + score}

Pink Team Assessment:
  Requirements Foundation: {storyboard_assessment}
  Ready for Specs: {"YES" if readiness else "NO"}

{recommendation}

Output: shared/validation/sva2-pink-team.json
```

## Quality Checklist

- [ ] `sva2-pink-team.json` created in `shared/validation/`
- [ ] All 9 rules evaluated (includes SVA2-DELIVERABLE-NO-ROW-CAPS added 2026-05-19)
- [ ] Disposition correctly calculated (BLOCK/ADVISORY/PASS)
- [ ] Pink Team report includes storyboard assessment
- [ ] Compliance-to-requirement mapping verified
- [ ] Corrective actions specified for failed rules
- [ ] SVA2-DELIVERABLE-NO-ROW-CAPS checked for _Showing N of M_ and empty |  | mitigation cells
