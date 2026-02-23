---
name: sva2-pink-team-win
expert-role: Pink Team Reviewer
domain-expertise: Requirements completeness, deduplication quality, compliance mapping, Shipley Pink Team review
---

# SVA-2: Pink Team Review

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

    wf_reqs = workflow_reqs.get("requirements", workflow_reqs.get("workflow_requirements", []))
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
        wf_text = wf.get("text", wf.get("requirement", "")).lower()[:100]
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
        return {
            "rule_id": "SVA2-SAMPLE-DATA-INTEGRATION", "severity": "MEDIUM",
            "passed": True, "score": 100, "threshold": None,
            "details": {"note": "No data entities found in sample data"},
            "corrective_action": None
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
- [ ] All 8 rules evaluated
- [ ] Disposition correctly calculated (BLOCK/ADVISORY/PASS)
- [ ] Pink Team report includes storyboard assessment
- [ ] Compliance-to-requirement mapping verified
- [ ] Corrective actions specified for failed rules
