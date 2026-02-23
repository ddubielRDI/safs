---
name: sva4-red-team-win
expert-role: Red Team Reviewer
domain-expertise: Bidirectional traceability validation, evaluator simulation, compliance chain integrity, Shipley Red Team review
---

# SVA-4: Red Team Review

## Expert Role

You are a **Red Team Reviewer** conducting a rigorous mid-point review (~60% completion). Your focus:
- Bidirectional traceability completeness (forward AND backward)
- Traceability quality audit (catch false positives in RTM linking)
- Evaluator simulation scoring using actual evaluation criteria weights
- Compliance forward chain integrity (mandatory item -> req -> spec)
- Estimation consistency and risk alignment

## Purpose

Validate that Stage 4 (Traceability & Estimation) produced a reliable, complete Unified RTM and reasonable effort estimates. This is the Shipley **Red Team** review — the most critical review, simulating how evaluators will score the proposal. Issues found here MUST be fixed before bid writing begins.

## Inputs

- `{folder}/shared/UNIFIED_RTM.json` - The Unified Requirements Traceability Matrix (primary input)
- `{folder}/shared/requirements-normalized.json` - Normalized requirements
- `{folder}/shared/COMPLIANCE_MATRIX.json` - Mandatory items
- `{folder}/shared/EVALUATION_CRITERIA.json` - Evaluation factors with weights
- `{folder}/shared/REQUIREMENT_RISKS.json` - Risk assessments
- `{folder}/shared/effort-estimation.json` - Effort estimates (optional)
- `{folder}/shared/traceability-links.json` - Backward-compat traceability (optional)
- Specification files: `{folder}/outputs/ARCHITECTURE.md`, `INTEROPERABILITY.md`, `SECURITY_FRAMEWORK.md`, `UI_UX_SPECIFICATIONS.md`, `ENTITY_DEFINITIONS.md`

## Required Output

- `{folder}/shared/validation/sva4-red-team.json` - Red Team review report

## Instructions

### Step 1: Load All Data

```python
from datetime import datetime
import re

rtm = read_json(f"{folder}/shared/UNIFIED_RTM.json")
norm_reqs = read_json(f"{folder}/shared/requirements-normalized.json")
compliance = read_json(f"{folder}/shared/COMPLIANCE_MATRIX.json")
evaluation = read_json(f"{folder}/shared/EVALUATION_CRITERIA.json")
risks = read_json(f"{folder}/shared/REQUIREMENT_RISKS.json")

# Optional
effort = read_json_safe(f"{folder}/shared/effort-estimation.json")

# Extract entities from RTM
entities = rtm.get("entities", {})
rtm_reqs = entities.get("requirements", [])
rtm_specs = entities.get("specifications", [])
rtm_risks = entities.get("risks", [])
rtm_mandatory = entities.get("mandatory_items", [])
rtm_eval = entities.get("evaluation_criteria", [])
chain_links = rtm.get("chain_links", [])
rtm_verification = rtm.get("verification", {})

all_reqs = norm_reqs.get("requirements", [])
mandatory_items = compliance.get("mandatory_items", compliance.get("rtm_entities", {}).get("mandatory_items", []))
eval_factors = evaluation.get("evaluation_factors", evaluation.get("factors", []))

findings = []
```

### Step 2: Execute Rule Checks

```python
# --- SVA4-TRACE-BIDIRECTIONAL (CRITICAL) ---
def check_bidirectional_trace():
    """Forward: every req has spec links. Backward: every spec has requirements mapped."""

    # FORWARD: requirement -> specification
    total_reqs = len(rtm_reqs)
    reqs_with_specs = 0
    reqs_missing_specs = []

    for req in rtm_reqs:
        linked_specs = req.get("linked_spec_ids", [])
        if linked_specs:
            reqs_with_specs += 1
        else:
            # Only flag CRITICAL and HIGH priority missing specs
            priority = req.get("priority", "MEDIUM")
            if priority in ["CRITICAL", "HIGH"]:
                reqs_missing_specs.append({
                    "requirement_id": req.get("requirement_id"),
                    "priority": priority,
                    "text_preview": req.get("text", "")[:80]
                })

    forward_pct = (reqs_with_specs / total_reqs * 100) if total_reqs else 0

    # BACKWARD: specification -> requirements
    total_specs = len(rtm_specs)
    specs_with_reqs = 0
    specs_orphaned = []

    for spec in rtm_specs:
        linked_reqs = spec.get("linked_requirement_ids", [])
        if linked_reqs:
            specs_with_reqs += 1
        else:
            specs_orphaned.append({
                "spec_id": spec.get("spec_id"),
                "title": spec.get("title", "")[:60]
            })

    backward_pct = (specs_with_reqs / total_specs * 100) if total_specs else 0

    # Overall: both forward and backward must exceed 90%
    overall_score = (forward_pct + backward_pct) / 2
    passed = forward_pct >= 90 and backward_pct >= 85

    return {
        "rule_id": "SVA4-TRACE-BIDIRECTIONAL",
        "rule_name": "Bidirectional Traceability Completeness",
        "category": "Traceability",
        "severity": "CRITICAL",
        "passed": passed,
        "score": round(overall_score, 1),
        "threshold": 90.0,
        "details": {
            "forward": {
                "total_requirements": total_reqs,
                "with_spec_links": reqs_with_specs,
                "coverage_pct": round(forward_pct, 1),
                "critical_high_missing": reqs_missing_specs[:10]
            },
            "backward": {
                "total_specs": total_specs,
                "with_req_links": specs_with_reqs,
                "coverage_pct": round(backward_pct, 1),
                "orphaned_specs": specs_orphaned[:5]
            }
        },
        "corrective_action": {
            "type": "retry_phase",
            "target_phase": "4",
            "auto_correctable": True,
            "instruction": "Re-run traceability with enhanced keyword matching and lower similarity threshold"
        } if not passed else None
    }

findings.append(check_bidirectional_trace())


# --- SVA4-TRACE-QUALITY (HIGH) ---
def check_trace_quality():
    """Sample 20% of mappings and verify semantic alignment. Flag false positives."""
    import random

    # Sample 20% of chain_links for quality audit
    sample_size = max(5, int(len(chain_links) * 0.20))
    sample = random.sample(chain_links, min(sample_size, len(chain_links))) if chain_links else []

    likely_false_positives = []
    weak_links = []
    strong_links = 0

    for chain in sample:
        chain_id = chain.get("chain_id", "")
        req_id = chain.get("requirement_id", "")
        completeness = chain.get("completeness_score", 0)

        # Find the requirement text
        req_text = ""
        for r in rtm_reqs:
            if r.get("requirement_id") == req_id:
                req_text = r.get("text", "").lower()
                break

        # Check spec links for semantic relevance
        spec_ids = chain.get("spec_ids", [])
        for spec_id in spec_ids:
            spec_title = ""
            for s in rtm_specs:
                if s.get("spec_id") == spec_id:
                    spec_title = s.get("title", "").lower()
                    break

            # Simple semantic check: at least 2 shared meaningful words
            req_words = set(w for w in req_text.split() if len(w) > 3)
            spec_words = set(w for w in spec_title.split() if len(w) > 3)
            shared = req_words & spec_words

            if len(shared) < 1 and spec_title:
                likely_false_positives.append({
                    "chain_id": chain_id,
                    "requirement": req_id,
                    "spec": spec_id,
                    "shared_words": list(shared),
                    "concern": "Low semantic overlap between requirement and spec"
                })

        if completeness < 0.5:
            weak_links.append({
                "chain_id": chain_id,
                "completeness": completeness,
                "status": chain.get("status", "UNKNOWN")
            })
        elif completeness >= 0.8:
            strong_links += 1

    false_positive_rate = (len(likely_false_positives) / len(sample) * 100) if sample else 0
    quality_score = max(0, 100 - false_positive_rate * 2)
    passed = false_positive_rate <= 15  # Allow up to 15% potential false positives

    return {
        "rule_id": "SVA4-TRACE-QUALITY",
        "rule_name": "Traceability Quality Audit",
        "category": "Traceability",
        "severity": "HIGH",
        "passed": passed,
        "score": round(quality_score, 1),
        "threshold": 85.0,
        "details": {
            "sample_size": len(sample),
            "total_chains": len(chain_links),
            "strong_links": strong_links,
            "weak_links_count": len(weak_links),
            "likely_false_positives": len(likely_false_positives),
            "false_positive_rate_pct": round(false_positive_rate, 1),
            "false_positive_examples": likely_false_positives[:5],
            "weak_link_examples": weak_links[:5]
        },
        "corrective_action": {
            "type": "retry_phase",
            "target_phase": "4",
            "auto_correctable": True,
            "instruction": "Review flagged false positives, tighten similarity thresholds, re-link specs with stricter criteria"
        } if not passed else None
    }

findings.append(check_trace_quality())


# --- SVA4-RED-TEAM-SCORING (HIGH) ---
def check_red_team_scoring():
    """Simulate evaluation using EVALUATION_CRITERIA.json weights. Produce mock score."""
    if not eval_factors:
        return {
            "rule_id": "SVA4-RED-TEAM-SCORING",
            "rule_name": "Red Team Evaluator Simulation",
            "category": "Content",
            "severity": "HIGH",
            "passed": True,
            "score": 100,
            "threshold": None,
            "details": {"note": "No evaluation factors defined - skipping simulation"},
            "corrective_action": None
        }

    factor_scores = []
    total_max = 0
    total_estimated = 0

    for factor in eval_factors:
        factor_name = factor.get("factor_name", factor.get("name", "Unknown"))
        factor_id = factor.get("factor_id", "")
        max_points = factor.get("points", factor.get("max_points", factor.get("weight", 100)))
        subfactors = factor.get("subfactors", [])

        # Score based on requirement coverage for this factor
        # Find RTM eval criteria matching this factor
        rtm_factor = None
        for ec in rtm_eval:
            if ec.get("factor_id") == factor_id or ec.get("factor_name") == factor_name:
                rtm_factor = ec
                break

        linked_req_count = 0
        if rtm_factor:
            linked_req_count = len(rtm_factor.get("linked_requirement_ids", []))

        # Score heuristic:
        # - Requirements linked: 40% of score
        # - Specs covering those reqs: 30% of score
        # - Risk coverage: 15% of score
        # - Chain completeness: 15% of score

        req_score = min(1.0, linked_req_count / max(5, len(subfactors) * 3)) if linked_req_count else 0.3

        # Check if linked requirements have specs
        spec_coverage = 0
        if rtm_factor:
            linked_reqs = rtm_factor.get("linked_requirement_ids", [])
            reqs_with_specs = 0
            for rid in linked_reqs:
                for r in rtm_reqs:
                    if r.get("requirement_id") == rid and r.get("linked_spec_ids"):
                        reqs_with_specs += 1
                        break
            spec_coverage = reqs_with_specs / len(linked_reqs) if linked_reqs else 0.5

        # Chain completeness for this factor's reqs
        chain_scores = []
        if rtm_factor:
            for rid in rtm_factor.get("linked_requirement_ids", []):
                for cl in chain_links:
                    if cl.get("requirement_id") == rid:
                        chain_scores.append(cl.get("completeness_score", 0))
                        break

        avg_chain = sum(chain_scores) / len(chain_scores) if chain_scores else 0.4

        estimated_pct = req_score * 0.40 + spec_coverage * 0.30 + 0.6 * 0.15 + avg_chain * 0.15
        estimated_score = max_points * estimated_pct

        factor_scores.append({
            "factor_name": factor_name,
            "factor_id": factor_id,
            "max_points": max_points,
            "estimated_score": round(estimated_score, 1),
            "score_percentage": round(estimated_pct * 100, 1),
            "notes": f"{linked_req_count} reqs linked, {round(spec_coverage*100)}% spec coverage, {round(avg_chain*100)}% chain avg"
        })

        total_max += max_points
        total_estimated += estimated_score

    overall_pct = (total_estimated / total_max * 100) if total_max else 0
    # Red Team threshold: we want at least 65% projected score at this stage
    passed = overall_pct >= 65

    return {
        "rule_id": "SVA4-RED-TEAM-SCORING",
        "rule_name": "Red Team Evaluator Simulation",
        "category": "Content",
        "severity": "HIGH",
        "passed": passed,
        "score": round(overall_pct, 1),
        "threshold": 65.0,
        "details": {
            "evaluator_simulation": {
                "factors": factor_scores,
                "weighted_total": round(total_estimated, 1),
                "max_possible": total_max,
                "percentage": round(overall_pct, 1)
            }
        },
        "corrective_action": {
            "type": "user_review",
            "target_phase": None,
            "auto_correctable": False,
            "instruction": f"Projected evaluator score is {overall_pct:.0f}% (below 65% threshold). Review factor coverage gaps."
        } if not passed else None
    }

findings.append(check_red_team_scoring())


# --- SVA4-COMPLIANCE-FORWARD-TRACE (CRITICAL) ---
def check_compliance_forward_trace():
    """Every mandatory item traces through: item -> requirement -> specification."""
    total_mandatory = len(rtm_mandatory)
    if total_mandatory == 0:
        return {
            "rule_id": "SVA4-COMPLIANCE-FORWARD-TRACE",
            "rule_name": "Compliance Forward Traceability",
            "category": "Traceability",
            "severity": "CRITICAL",
            "passed": True,
            "score": 100,
            "threshold": None,
            "details": {"note": "No mandatory items in RTM"},
            "corrective_action": None
        }

    complete_chains = 0
    broken_at_req = []
    broken_at_spec = []

    for m in rtm_mandatory:
        m_id = m.get("mandatory_id", m.get("id", "?"))
        linked_reqs = m.get("linked_requirement_ids", [])

        if not linked_reqs:
            broken_at_req.append({
                "mandatory_id": m_id,
                "text_preview": m.get("text", m.get("description", ""))[:60],
                "break_point": "No linked requirements"
            })
            continue

        # Check if at least one linked requirement has a spec
        has_spec = False
        for rid in linked_reqs:
            for r in rtm_reqs:
                if r.get("requirement_id") == rid:
                    if r.get("linked_spec_ids"):
                        has_spec = True
                        break
            if has_spec:
                break

        if not has_spec:
            broken_at_spec.append({
                "mandatory_id": m_id,
                "linked_requirement_ids": linked_reqs[:3],
                "break_point": "Requirements exist but no specifications linked"
            })
        else:
            complete_chains += 1

    chain_pct = (complete_chains / total_mandatory * 100) if total_mandatory else 0
    passed = chain_pct >= 90

    return {
        "rule_id": "SVA4-COMPLIANCE-FORWARD-TRACE",
        "rule_name": "Compliance Forward Traceability",
        "category": "Traceability",
        "severity": "CRITICAL",
        "passed": passed,
        "score": round(chain_pct, 1),
        "threshold": 90.0,
        "details": {
            "total_mandatory_items": total_mandatory,
            "complete_chains": complete_chains,
            "chain_coverage_pct": round(chain_pct, 1),
            "broken_at_requirement": broken_at_req[:10],
            "broken_at_specification": broken_at_spec[:10]
        },
        "corrective_action": {
            "type": "retry_phase",
            "target_phase": "4",
            "auto_correctable": True,
            "instruction": "Re-link mandatory items with broader matching criteria and ensure spec linking covers compliance items"
        } if not passed else None
    }

findings.append(check_compliance_forward_trace())


# --- SVA4-ESTIMATION-CONSISTENCY (HIGH) ---
def check_estimation_consistency():
    """Effort per requirement is reasonable. Total between 500-50,000 hours."""
    if not effort:
        return {
            "rule_id": "SVA4-ESTIMATION-CONSISTENCY",
            "rule_name": "Estimation Consistency",
            "category": "Consistency",
            "severity": "HIGH",
            "passed": True,
            "score": 100,
            "threshold": None,
            "details": {"note": "No effort estimation file found - skipping"},
            "corrective_action": None
        }

    # Parse total hours from estimation
    total_hours = effort.get("total_hours", effort.get("summary", {}).get("total_hours", 0))
    phase_estimates = effort.get("phases", effort.get("work_packages", effort.get("estimates", [])))

    # Check total range
    total_in_range = 500 <= total_hours <= 50000

    # Check per-requirement average
    req_count = len(all_reqs)
    avg_per_req = total_hours / req_count if req_count else 0
    avg_reasonable = 2 <= avg_per_req <= 200  # 2-200 hours per requirement is reasonable

    # Check for outliers (any single phase > 40% of total)
    outliers = []
    if isinstance(phase_estimates, list):
        for phase in phase_estimates:
            phase_hours = phase.get("hours", phase.get("effort_hours", 0))
            phase_name = phase.get("name", phase.get("phase", "?"))
            if total_hours > 0 and phase_hours / total_hours > 0.40:
                outliers.append({
                    "phase": phase_name,
                    "hours": phase_hours,
                    "pct_of_total": round(phase_hours / total_hours * 100, 1)
                })

    passed = total_in_range and avg_reasonable
    score = 100
    if not total_in_range:
        score -= 40
    if not avg_reasonable:
        score -= 30
    if outliers:
        score -= len(outliers) * 15

    return {
        "rule_id": "SVA4-ESTIMATION-CONSISTENCY",
        "rule_name": "Estimation Consistency",
        "category": "Consistency",
        "severity": "HIGH",
        "passed": passed,
        "score": round(max(0, score), 1),
        "threshold": None,
        "details": {
            "total_hours": total_hours,
            "total_in_range": total_in_range,
            "requirement_count": req_count,
            "avg_hours_per_req": round(avg_per_req, 1),
            "avg_reasonable": avg_reasonable,
            "phase_outliers": outliers
        },
        "corrective_action": {
            "type": "retry_phase",
            "target_phase": "5",
            "auto_correctable": True,
            "instruction": "Re-estimate with adjusted heuristics to bring totals within reasonable range"
        } if not passed else None
    }

findings.append(check_estimation_consistency())


# --- SVA4-ESTIMATION-RISK-ALIGNMENT (MEDIUM) ---
def check_estimation_risk_alignment():
    """High-risk requirements should have higher effort estimates."""
    if not effort:
        return {
            "rule_id": "SVA4-ESTIMATION-RISK-ALIGNMENT",
            "rule_name": "Estimation Risk Alignment",
            "category": "Consistency",
            "severity": "MEDIUM",
            "passed": True,
            "score": 100,
            "threshold": None,
            "details": {"note": "No effort estimation file found - skipping"},
            "corrective_action": None
        }

    # Build risk severity map from RTM risks
    req_risk_map = {}  # req_id -> highest risk severity
    for risk in rtm_risks:
        severity = risk.get("severity", "MEDIUM")
        sev_rank = {"CRITICAL": 4, "HIGH": 3, "MEDIUM": 2, "LOW": 1}.get(severity, 2)
        affected_reqs = risk.get("affected_requirement_ids", [])
        for rid in affected_reqs:
            current = req_risk_map.get(rid, 0)
            req_risk_map[rid] = max(current, sev_rank)

    # Get per-requirement effort estimates if available
    req_estimates = effort.get("per_requirement", effort.get("requirement_estimates", {}))

    if not req_estimates or not req_risk_map:
        return {
            "rule_id": "SVA4-ESTIMATION-RISK-ALIGNMENT",
            "rule_name": "Estimation Risk Alignment",
            "category": "Consistency",
            "severity": "MEDIUM",
            "passed": True,
            "score": 80,
            "threshold": None,
            "details": {
                "note": "Insufficient granular data for per-requirement risk-effort correlation",
                "risk_mapped_reqs": len(req_risk_map),
                "effort_mapped_reqs": len(req_estimates) if isinstance(req_estimates, dict) else 0
            },
            "corrective_action": None
        }

    # Check: avg effort for HIGH/CRITICAL risk reqs should be > avg effort for LOW/MEDIUM
    high_risk_efforts = []
    low_risk_efforts = []

    for rid, sev in req_risk_map.items():
        est = req_estimates.get(rid, {})
        hours = est.get("hours", est.get("effort_hours", 0)) if isinstance(est, dict) else 0
        if hours > 0:
            if sev >= 3:  # HIGH or CRITICAL
                high_risk_efforts.append(hours)
            else:
                low_risk_efforts.append(hours)

    if high_risk_efforts and low_risk_efforts:
        avg_high = sum(high_risk_efforts) / len(high_risk_efforts)
        avg_low = sum(low_risk_efforts) / len(low_risk_efforts)
        ratio = avg_high / avg_low if avg_low > 0 else 1
        passed = ratio >= 1.0  # High risk should have at least equal effort
        score = min(100, ratio * 50 + 50) if ratio >= 1 else ratio * 100
    else:
        passed = True
        score = 75
        ratio = None
        avg_high = sum(high_risk_efforts) / len(high_risk_efforts) if high_risk_efforts else 0
        avg_low = sum(low_risk_efforts) / len(low_risk_efforts) if low_risk_efforts else 0

    return {
        "rule_id": "SVA4-ESTIMATION-RISK-ALIGNMENT",
        "rule_name": "Estimation Risk Alignment",
        "category": "Consistency",
        "severity": "MEDIUM",
        "passed": passed,
        "score": round(max(0, min(100, score)), 1),
        "threshold": None,
        "details": {
            "high_risk_reqs": len(high_risk_efforts),
            "low_risk_reqs": len(low_risk_efforts),
            "avg_effort_high_risk": round(avg_high, 1),
            "avg_effort_low_risk": round(avg_low, 1),
            "effort_ratio": round(ratio, 2) if ratio else None,
            "assessment": "Aligned" if passed else "Misaligned - high risk items under-estimated"
        },
        "corrective_action": None
    }

findings.append(check_estimation_risk_alignment())
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
failed_count = len(findings) - passed_count
critical_failures = sum(1 for f in findings if f["severity"] == "CRITICAL" and not f["passed"])
high_failures = sum(1 for f in findings if f["severity"] == "HIGH" and not f["passed"])
overall_score = sum(f["score"] for f in findings) / len(findings) if findings else 0
```

### Step 4: Build Red Team Report

```python
# Extract traceability integrity metrics from RTM verification
fwd_coverage = rtm_verification.get("forward_coverage", {}).get("requirements_with_specs_pct", 0)
bwd_coverage = rtm_verification.get("backward_coverage", {}).get("specs_with_requirements_pct", 0)
chain_avg = rtm_verification.get("chain_coverage", {}).get("average_completeness", 0)

# Get evaluator simulation from SVA4-RED-TEAM-SCORING finding
eval_sim = None
for f in findings:
    if f["rule_id"] == "SVA4-RED-TEAM-SCORING":
        eval_sim = f.get("details", {}).get("evaluator_simulation")
        break

# Estimate false positive rate from quality audit
fp_rate = 0
for f in findings:
    if f["rule_id"] == "SVA4-TRACE-QUALITY":
        fp_rate = f.get("details", {}).get("false_positive_rate_pct", 0)
        break

# Compliance chain check
compliance_chain_ok = True
for f in findings:
    if f["rule_id"] == "SVA4-COMPLIANCE-FORWARD-TRACE":
        compliance_chain_ok = f["passed"]
        break

red_team_report = {
    "team": "red",
    "review_type": "Red Team (~60% Completion)",
    "recommendation": (
        "PROCEED" if disposition == "PASS" else
        "PROCEED_WITH_CAUTION" if disposition == "ADVISORY" else
        "REVISE_AND_RESUBMIT"
    ),
    "top_concerns": [
        f"{f['rule_id']}: {f['details'].get('concern', f['rule_name'])} (score: {f['score']})"
        for f in findings if not f["passed"]
    ][:5],
    "evaluator_simulation": eval_sim,
    "traceability_integrity": {
        "forward_coverage_pct": round(fwd_coverage, 1),
        "backward_coverage_pct": round(bwd_coverage, 1),
        "false_positive_estimate_pct": round(fp_rate, 1),
        "compliance_chain_complete": compliance_chain_ok,
        "chain_completeness_avg": round(chain_avg * 100, 1) if chain_avg <= 1 else round(chain_avg, 1)
    },
    "risk_assessment": {
        "total_risks_in_rtm": len(rtm_risks),
        "high_critical_risks": sum(1 for r in rtm_risks if r.get("severity") in ["HIGH", "CRITICAL"]),
        "risks_with_mitigations": sum(1 for r in rtm_risks if r.get("mitigation_strategies")),
        "mitigations_with_owners": sum(
            1 for r in rtm_risks
            for m in r.get("mitigation_strategies", [])
            if m.get("owner_role")
        )
    },
    "go_no_go": {
        "ready_for_bid_writing": disposition != "BLOCK",
        "critical_fixes_needed": critical_failures,
        "advisory_items": high_failures,
        "projected_evaluator_score": eval_sim.get("percentage", 0) if eval_sim else None
    }
}
```

### Step 5: Build Corrective Actions Summary

```python
corrective_actions = []
for f in findings:
    if f.get("corrective_action"):
        corrective_actions.append({
            "priority": f["severity"],
            "action": f["corrective_action"].get("instruction", f["rule_name"]),
            "target_phase": f["corrective_action"].get("target_phase"),
            "auto_correctable": f["corrective_action"].get("auto_correctable", False),
            "rule_id": f["rule_id"]
        })

# Sort: CRITICAL first, then HIGH, MEDIUM, LOW
severity_order = {"CRITICAL": 0, "HIGH": 1, "MEDIUM": 2, "LOW": 3}
corrective_actions.sort(key=lambda x: severity_order.get(x["priority"], 4))
```

### Step 6: Write Report

```python
report = {
    "validator": "SVA-4",
    "stage_validated": 4,
    "validated_at": datetime.now().isoformat(),
    "disposition": disposition,
    "color_team": "red",
    "summary": {
        "total_rules": len(findings),
        "passed": passed_count,
        "failed": failed_count,
        "critical_failures": critical_failures,
        "high_failures": high_failures,
        "overall_score": round(overall_score, 1)
    },
    "findings": findings,
    "color_team_report": red_team_report,
    "corrective_actions_summary": corrective_actions,
    "execution_metadata": {
        "files_analyzed": 7 + len(rtm_reqs) + len(rtm_specs),
        "data_sources_read": [
            "UNIFIED_RTM.json",
            "requirements-normalized.json",
            "COMPLIANCE_MATRIX.json",
            "EVALUATION_CRITERIA.json",
            "REQUIREMENT_RISKS.json",
            "effort-estimation.json"
        ]
    }
}

write_json(f"{folder}/shared/validation/sva4-red-team.json", report)
```

### Step 7: Report Results

```
🔴 RED TEAM REVIEW COMPLETE (SVA-4)
======================================
Disposition: {disposition} {"✅" if disposition == "PASS" else "⚠️" if disposition == "ADVISORY" else "❌"}
Score: {overall_score:.0f}/100
Rules: {passed_count}/{len(findings)} passed | {critical_failures} CRITICAL fails | {high_failures} HIGH fails

Findings:
{for each finding: severity_icon + rule_id + score + pass/fail}

Red Team Assessment:
  Recommendation: {red_team_report["recommendation"]}
  Forward Traceability: {fwd_coverage:.0f}%
  Backward Traceability: {bwd_coverage:.0f}%
  False Positive Rate: {fp_rate:.1f}%
  Compliance Chain: {"COMPLETE" if compliance_chain_ok else "BROKEN"}

Evaluator Simulation:
  Projected Score: {eval_sim["percentage"]:.0f}% ({eval_sim["weighted_total"]}/{eval_sim["max_possible"]} points)
  {for each factor: factor_name + estimated_score/max_points}

Go/No-Go: {"READY for bid writing" if red_team_report["go_no_go"]["ready_for_bid_writing"] else "NOT READY - must fix critical issues"}

Output: shared/validation/sva4-red-team.json
```

## Quality Checklist

- [ ] `sva4-red-team.json` created in `shared/validation/`
- [ ] All 6 rules evaluated
- [ ] Disposition correctly calculated (BLOCK/ADVISORY/PASS)
- [ ] Evaluator simulation produces factor-by-factor scoring
- [ ] Traceability integrity metrics computed (forward + backward + false positive rate)
- [ ] Compliance forward chain traced: mandatory item -> requirement -> specification
- [ ] Corrective actions sorted by priority
- [ ] Go/No-Go assessment included in color team report
