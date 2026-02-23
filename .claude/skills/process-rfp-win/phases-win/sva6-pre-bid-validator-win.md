---
name: sva6-pre-bid-validator-win
expert-role: Red Team Bid Evaluator
domain-expertise: Government procurement evaluation, bid readiness assessment, evaluator simulation, risk management auditing
---

# SVA-6: Pre-Bid Gate / Red Team Final

## Expert Role

You are a **Red Team Bid Evaluator** with deep expertise in:
- Government and commercial procurement evaluation processes
- Shipley/APMP Red Team review methodology
- Evaluator persona simulation and scoring
- Risk register auditing and mitigation verification
- Bid context integrity and data consistency validation

## Purpose

Final pre-bid quality gate after Stage 6 (Quality Assurance). Simulate a hostile evaluator reviewing the bid preparation artifacts. Verify persona coverage, win probability, gap closure, risk mitigation completeness, context bundle integrity, and evaluation factor alignment. Produce a structured JSON report with disposition and a Red Team color team report.

## Color Team

**Red** -- Shipley Red Team review. Simulates actual evaluators reviewing the prepared bid materials for weaknesses, gaps, and disqualification risks before bid authoring begins.

## Inputs

- `{folder}/shared/PERSONA_COVERAGE.json` - Evaluator persona analysis with scores
- `{folder}/shared/WIN_SCORECARD.json` - Win probability model with factor scores
- `{folder}/outputs/GAP_ANALYSIS.md` - Gap analysis document
- `{folder}/shared/REQUIREMENT_RISKS.json` - Risk assessment with mitigations
- `{folder}/shared/bid-context-bundle.json` - Aggregated bid context data
- `{folder}/shared/EVALUATION_CRITERIA.json` - Evaluation factors with weights
- `{folder}/shared/requirements-normalized.json` - Normalized requirements

## Required Output

- `{folder}/shared/validation/sva6-pre-bid.json`

## Disposition Rules

| Disposition | Condition |
|-------------|-----------|
| **BLOCK** | Any CRITICAL finding fails |
| **ADVISORY** | Any HIGH finding fails (no CRITICAL failures) |
| **PASS** | All pass, or only MEDIUM/LOW failures |

---

## Instructions

### Step 1: Load All Inputs

```python
import os, json, re
from datetime import datetime

start_time = datetime.now()

# Load all input files
persona_coverage = read_json(f"{folder}/shared/PERSONA_COVERAGE.json")
win_scorecard = read_json(f"{folder}/shared/WIN_SCORECARD.json")
gap_analysis_md = read_file(f"{folder}/outputs/GAP_ANALYSIS.md")
risks_data = read_json(f"{folder}/shared/REQUIREMENT_RISKS.json")
bid_context = read_json(f"{folder}/shared/bid-context-bundle.json")
eval_criteria = read_json(f"{folder}/shared/EVALUATION_CRITERIA.json")
requirements = read_json(f"{folder}/shared/requirements-normalized.json")

all_reqs = requirements.get("requirements", [])
all_risks = risks_data.get("risks", [])

findings = []
```

### Step 2: Rule SVA6-PERSONA-INFORMED (HIGH)

PERSONA_COVERAGE.json overall_score must be >= 80, and each individual persona must score >= 70.

```python
def check_persona_informed(persona_coverage):
    """
    Verify evaluator persona coverage meets minimum thresholds.
    Overall score >= 80, each persona >= 70.
    """
    overall_score = persona_coverage.get("overall_score",
                        persona_coverage.get("summary", {}).get("overall_score", 0))
    personas = persona_coverage.get("personas", [])

    low_personas = []
    for persona in personas:
        name = persona.get("name", persona.get("persona_name", "Unknown"))
        score = persona.get("coverage_score", persona.get("score", 0))
        if score < 70:
            low_personas.append({
                "name": name,
                "score": score,
                "deficit": round(70 - score, 1),
                "top_concerns": persona.get("unaddressed_concerns", persona.get("gaps", []))[:3]
            })

    overall_passes = overall_score >= 80
    all_personas_pass = len(low_personas) == 0
    passed = overall_passes and all_personas_pass

    # Score: average of overall threshold closeness and persona health
    score = min(overall_score, 100.0)

    return {
        "rule_id": "SVA6-PERSONA-INFORMED",
        "rule_name": "Persona Coverage Minimum",
        "category": "Content",
        "severity": "HIGH",
        "passed": passed,
        "score": round(score, 1),
        "threshold": 80.0,
        "details": {
            "overall_score": overall_score,
            "overall_threshold": 80,
            "overall_passes": overall_passes,
            "persona_count": len(personas),
            "personas_below_70": low_personas,
            "all_personas_pass": all_personas_pass
        },
        "corrective_action": None if passed else {
            "type": "retry_phase",
            "target_phase": "7c",
            "instruction": f"Improve persona coverage. Overall: {overall_score}/80. {len(low_personas)} persona(s) below 70.",
            "auto_correctable": True
        }
    }

findings.append(check_persona_informed(persona_coverage))
```

### Step 3: Rule SVA6-WIN-PROBABILITY (MEDIUM)

WIN_SCORECARD.json win_probability must be >= 60%.

```python
def check_win_probability(win_scorecard):
    """
    Verify win probability meets minimum threshold.
    This is advisory-only (MEDIUM severity) -- low probability
    does not block the pipeline but should be flagged.
    """
    win_prob = win_scorecard.get("win_probability",
                   win_scorecard.get("summary", {}).get("win_probability", 0))
    # Handle percentage stored as 0-1 or 0-100
    if isinstance(win_prob, (int, float)) and win_prob <= 1.0:
        win_prob = win_prob * 100

    factor_scores = win_scorecard.get("factor_scores",
                        win_scorecard.get("factors", []))
    weakest_factors = []
    if isinstance(factor_scores, list):
        sorted_factors = sorted(factor_scores,
                                key=lambda f: f.get("score", f.get("weighted_score", 0)))
        weakest_factors = [
            {"name": f.get("factor", f.get("name", "?")), "score": f.get("score", 0)}
            for f in sorted_factors[:3]
        ]

    passed = win_prob >= 60.0
    score = min(win_prob, 100.0)

    return {
        "rule_id": "SVA6-WIN-PROBABILITY",
        "rule_name": "Win Probability Threshold",
        "category": "Content",
        "severity": "MEDIUM",
        "passed": passed,
        "score": round(score, 1),
        "threshold": 60.0,
        "details": {
            "win_probability": round(win_prob, 1),
            "threshold": 60.0,
            "weakest_factors": weakest_factors,
            "recommendation": "Strengthen weak areas before bid authoring" if not passed else "Probability acceptable"
        },
        "corrective_action": None if passed else {
            "type": "user_review",
            "target_phase": "7d",
            "instruction": f"Win probability {win_prob:.1f}% is below 60% threshold. Review weakest factors and decide whether to proceed.",
            "auto_correctable": False
        }
    }

findings.append(check_win_probability(win_scorecard))
```

### Step 4: Rule SVA6-GAP-ANALYSIS-CLEAR (HIGH)

GAP_ANALYSIS.md must have no HIGH severity gaps remaining.

```python
def check_gap_analysis(gap_analysis_md):
    """
    Parse GAP_ANALYSIS.md for severity indicators and verify
    no HIGH or CRITICAL gaps remain unresolved.
    """
    lines = gap_analysis_md.split("\n")
    gap_lower = gap_analysis_md.lower()

    # Strategy 1: Look for structured gap entries with severity markers
    high_gap_patterns = [
        r'(?:severity|priority)\s*[:\|]\s*(?:high|critical)',
        r'\|\s*(?:high|critical)\s*\|',
        r'(?:^|\n)\s*-\s*.*?(?:high|critical)\s+(?:gap|severity)',
        r'(?:unresolved|open|remaining).*?(?:high|critical)',
    ]

    high_gaps_found = []
    for pattern in high_gap_patterns:
        matches = re.findall(pattern, gap_lower)
        for match in matches:
            # Verify this is a gap entry, not a gap that was resolved
            context_start = max(0, gap_lower.find(match) - 100)
            context = gap_lower[context_start:context_start + 300]
            if not any(word in context for word in ["resolved", "closed", "mitigated", "addressed"]):
                high_gaps_found.append(match.strip()[:120])

    # Strategy 2: Count lines that look like HIGH gap table rows
    high_gap_rows = []
    for line in lines:
        line_lower = line.lower().strip()
        if ("high" in line_lower or "critical" in line_lower) and \
           ("|" in line_lower) and \
           not any(w in line_lower for w in ["resolved", "closed", "mitigated", "n/a"]):
            high_gap_rows.append(line.strip()[:150])

    # Deduplicate
    unique_high_gaps = list(set(high_gaps_found + high_gap_rows))
    passed = len(unique_high_gaps) == 0
    score = max(0, 100 - (len(unique_high_gaps) * 15))

    return {
        "rule_id": "SVA6-GAP-ANALYSIS-CLEAR",
        "rule_name": "Gap Analysis Review",
        "category": "Content",
        "severity": "HIGH",
        "passed": passed,
        "score": round(max(score, 0), 1),
        "threshold": 100.0,
        "details": {
            "high_gaps_remaining": len(unique_high_gaps),
            "gap_excerpts": unique_high_gaps[:10],
            "total_document_lines": len(lines)
        },
        "corrective_action": None if passed else {
            "type": "retry_phase",
            "target_phase": "7b",
            "instruction": f"{len(unique_high_gaps)} HIGH/CRITICAL gaps remain open. Resolve or downgrade before bid authoring.",
            "auto_correctable": True
        }
    }

findings.append(check_gap_analysis(gap_analysis_md))
```

### Step 5: Rule SVA6-RISK-MITIGATION-COMPLETE (CRITICAL)

Every HIGH/CRITICAL risk must have a mitigation strategy, an owner_role, and verification criteria.

```python
def check_risk_mitigation_complete(all_risks):
    """
    Verify every HIGH/CRITICAL risk has complete mitigation data:
    - mitigation_strategy (non-empty)
    - owner_role (non-empty)
    - verification_criteria (non-empty)
    """
    high_critical = [
        r for r in all_risks
        if r.get("severity") in ("HIGH", "CRITICAL")
    ]
    total = len(high_critical)
    complete = 0
    incomplete_risks = []

    for risk in high_critical:
        risk_id = risk.get("risk_id", risk.get("id", ""))
        mitigation = risk.get("mitigation_strategy", risk.get("mitigation", ""))
        owner = risk.get("owner_role", risk.get("owner", ""))
        verification = risk.get("verification_criteria", risk.get("verification", ""))

        missing_fields = []
        if not mitigation or len(str(mitigation).strip()) < 10:
            missing_fields.append("mitigation_strategy")
        if not owner or len(str(owner).strip()) < 2:
            missing_fields.append("owner_role")
        if not verification or len(str(verification).strip()) < 10:
            missing_fields.append("verification_criteria")

        if not missing_fields:
            complete += 1
        else:
            incomplete_risks.append({
                "risk_id": risk_id,
                "severity": risk.get("severity"),
                "description": risk.get("description", "")[:100],
                "missing_fields": missing_fields
            })

    coverage_pct = (complete / total * 100) if total > 0 else 100.0
    passed = coverage_pct >= 95.0

    return {
        "rule_id": "SVA6-RISK-MITIGATION-COMPLETE",
        "rule_name": "Risk Mitigation Completeness",
        "category": "Completeness",
        "severity": "CRITICAL",
        "passed": passed,
        "score": round(coverage_pct, 1),
        "threshold": 95.0,
        "details": {
            "total_high_critical_risks": total,
            "complete": complete,
            "incomplete_count": len(incomplete_risks),
            "incomplete_risks": incomplete_risks[:15]
        },
        "corrective_action": None if passed else {
            "type": "retry_phase",
            "target_phase": "3g",
            "instruction": f"{len(incomplete_risks)} HIGH/CRITICAL risks missing fields. Each needs: mitigation_strategy, owner_role, verification_criteria.",
            "auto_correctable": True
        }
    }

findings.append(check_risk_mitigation_complete(all_risks))
```

### Step 6: Rule SVA6-BID-CONTEXT-INTEGRITY (CRITICAL)

bid-context-bundle.json counts must match the actual source files. Detect stale or inconsistent aggregation.

```python
def check_bid_context_integrity(bid_context, folder):
    """
    Verify bid-context-bundle.json aggregation counts match
    the actual source JSON/MD files on disk.
    """
    mismatches = []

    # Check requirement count
    bundle_req_count = bid_context.get("requirements_count",
                          bid_context.get("counts", {}).get("requirements", -1))
    actual_reqs = read_json(f"{folder}/shared/requirements-normalized.json")
    actual_req_count = len(actual_reqs.get("requirements", []))

    if bundle_req_count >= 0 and bundle_req_count != actual_req_count:
        mismatches.append({
            "field": "requirements_count",
            "bundle_value": bundle_req_count,
            "actual_value": actual_req_count,
            "source": "requirements-normalized.json"
        })

    # Check risk count
    bundle_risk_count = bid_context.get("risks_count",
                           bid_context.get("counts", {}).get("risks", -1))
    actual_risks = read_json(f"{folder}/shared/REQUIREMENT_RISKS.json")
    actual_risk_count = len(actual_risks.get("risks", []))

    if bundle_risk_count >= 0 and bundle_risk_count != actual_risk_count:
        mismatches.append({
            "field": "risks_count",
            "bundle_value": bundle_risk_count,
            "actual_value": actual_risk_count,
            "source": "REQUIREMENT_RISKS.json"
        })

    # Check evaluation criteria count
    bundle_eval_count = bid_context.get("evaluation_factors_count",
                           bid_context.get("counts", {}).get("evaluation_factors", -1))
    actual_eval = read_json(f"{folder}/shared/EVALUATION_CRITERIA.json")
    actual_eval_count = len(actual_eval.get("evaluation_factors", actual_eval.get("factors", [])))

    if bundle_eval_count >= 0 and bundle_eval_count != actual_eval_count:
        mismatches.append({
            "field": "evaluation_factors_count",
            "bundle_value": bundle_eval_count,
            "actual_value": actual_eval_count,
            "source": "EVALUATION_CRITERIA.json"
        })

    # Verify all referenced source files exist
    source_files = bid_context.get("source_files", bid_context.get("sources", []))
    missing_sources = []
    for src in source_files:
        src_path = src if isinstance(src, str) else src.get("path", src.get("file", ""))
        if src_path and not os.path.exists(os.path.join(folder, src_path)):
            # Try with shared/ and outputs/ prefixes
            found = False
            for prefix in ["shared/", "outputs/", ""]:
                if os.path.exists(os.path.join(folder, prefix, os.path.basename(src_path))):
                    found = True
                    break
            if not found:
                missing_sources.append(src_path)

    total_issues = len(mismatches) + len(missing_sources)
    passed = total_issues == 0
    score = max(0, 100 - (len(mismatches) * 20) - (len(missing_sources) * 10))

    return {
        "rule_id": "SVA6-BID-CONTEXT-INTEGRITY",
        "rule_name": "Bid Context Integrity",
        "category": "Consistency",
        "severity": "CRITICAL",
        "passed": passed,
        "score": round(max(score, 0), 1),
        "threshold": 100.0,
        "details": {
            "count_mismatches": mismatches,
            "missing_source_files": missing_sources,
            "total_issues": total_issues,
            "sources_referenced": len(source_files)
        },
        "corrective_action": None if passed else {
            "type": "retry_phase",
            "target_phase": "6c",
            "instruction": f"Regenerate context bundle. {len(mismatches)} count mismatch(es), {len(missing_sources)} missing source(s).",
            "auto_correctable": True
        }
    }

findings.append(check_bid_context_integrity(bid_context, folder))
```

### Step 7: Rule SVA6-EVALUATION-ALIGNMENT-CHECK (HIGH)

Each evaluation factor weighted >= 20% must have at least one spec and one requirements category addressing it.

```python
def check_evaluation_alignment(eval_criteria, requirements, folder):
    """
    For each evaluation factor with weight >= 20%, verify that:
    1. At least one requirement category maps to it
    2. At least one spec document addresses it
    """
    factors = eval_criteria.get("evaluation_factors", eval_criteria.get("factors", []))
    heavy_factors = []

    for factor in factors:
        weight = factor.get("weight", factor.get("percentage", 0))
        # Normalize: if weight is 0-1 scale, convert to percentage
        if isinstance(weight, (int, float)) and weight <= 1.0:
            weight = weight * 100
        if weight >= 20:
            heavy_factors.append({
                "name": factor.get("name", factor.get("factor_name", "")),
                "weight": weight,
                "keywords": factor.get("keywords", []),
                "description": factor.get("description", "")
            })

    if not heavy_factors:
        return {
            "rule_id": "SVA6-EVALUATION-ALIGNMENT-CHECK",
            "rule_name": "Evaluation Weight Coverage",
            "category": "Content",
            "severity": "HIGH",
            "passed": True,
            "score": 100.0,
            "threshold": 100.0,
            "details": {"note": "No evaluation factors >= 20% weight."},
            "corrective_action": None
        }

    # Build searchable text from specs
    spec_names = ["ARCHITECTURE.md", "SECURITY_REQUIREMENTS.md", "INTEROPERABILITY.md",
                   "UI_SPECS.md", "ENTITY_DEFINITIONS.md"]
    spec_texts = {}
    for sn in spec_names:
        path = f"{folder}/outputs/{sn}"
        if os.path.exists(path):
            spec_texts[sn] = read_file(path).lower()

    # Check each heavy factor
    results_per_factor = []
    unaligned_factors = []

    for factor in heavy_factors:
        factor_name = factor["name"].lower()
        factor_desc = factor.get("description", "").lower()
        factor_kws = [kw.lower() for kw in factor.get("keywords", [])]

        # Build search terms from factor name + keywords
        search_terms = [factor_name] + factor_kws
        if factor_desc:
            search_terms.extend(re.findall(r'\b\w{4,}\b', factor_desc)[:5])

        # Check requirements alignment
        req_match = False
        matching_categories = set()
        for req in all_reqs:
            req_text = req.get("text", "").lower()
            cat = req.get("category", "")
            if any(term in req_text for term in search_terms if len(term) > 3):
                req_match = True
                if cat:
                    matching_categories.add(cat)

        # Check spec alignment
        spec_match = False
        matching_specs = []
        for spec_name, spec_text in spec_texts.items():
            if any(term in spec_text for term in search_terms if len(term) > 3):
                spec_match = True
                matching_specs.append(spec_name)

        aligned = req_match and spec_match
        results_per_factor.append({
            "factor": factor["name"],
            "weight": factor["weight"],
            "has_requirements": req_match,
            "matching_categories": list(matching_categories)[:5],
            "has_specs": spec_match,
            "matching_specs": matching_specs,
            "aligned": aligned
        })

        if not aligned:
            unaligned_factors.append(factor["name"])

    aligned_count = sum(1 for r in results_per_factor if r["aligned"])
    score = (aligned_count / len(heavy_factors) * 100) if heavy_factors else 100
    passed = len(unaligned_factors) == 0

    return {
        "rule_id": "SVA6-EVALUATION-ALIGNMENT-CHECK",
        "rule_name": "Evaluation Weight Coverage",
        "category": "Content",
        "severity": "HIGH",
        "passed": passed,
        "score": round(score, 1),
        "threshold": 100.0,
        "details": {
            "heavy_factors_count": len(heavy_factors),
            "aligned": aligned_count,
            "unaligned_factors": unaligned_factors,
            "factor_analysis": results_per_factor
        },
        "corrective_action": None if passed else {
            "type": "user_review",
            "target_phase": "3a",
            "instruction": f"Evaluation factors without full coverage: {', '.join(unaligned_factors)}. Each needs both requirements and spec alignment.",
            "auto_correctable": False
        }
    }

findings.append(check_evaluation_alignment(eval_criteria, requirements, folder))
```

### Step 8: Compute Overall Disposition and Score

```python
def compute_disposition(findings):
    """
    BLOCK  = any CRITICAL failure
    ADVISORY = any HIGH failure (no CRITICAL failures)
    PASS   = all pass, or only MEDIUM/LOW failures
    """
    has_critical_failure = any(
        f["severity"] == "CRITICAL" and not f["passed"] for f in findings
    )
    has_high_failure = any(
        f["severity"] == "HIGH" and not f["passed"] for f in findings
    )

    if has_critical_failure:
        return "BLOCK"
    elif has_high_failure:
        return "ADVISORY"
    else:
        return "PASS"

def compute_overall_score(findings):
    """
    Weighted average: CRITICAL=30, HIGH=25, MEDIUM=15, LOW=10
    """
    severity_weights = {"CRITICAL": 30, "HIGH": 25, "MEDIUM": 15, "LOW": 10}
    total_weight = 0
    weighted_score = 0

    for f in findings:
        w = severity_weights.get(f["severity"], 10)
        total_weight += w
        weighted_score += w * (f.get("score", 100.0 if f["passed"] else 0.0))

    return round(weighted_score / total_weight, 1) if total_weight > 0 else 0

disposition = compute_disposition(findings)
overall_score = compute_overall_score(findings)
passed_count = sum(1 for f in findings if f["passed"])
failed_count = sum(1 for f in findings if not f["passed"])
critical_failures = sum(1 for f in findings if f["severity"] == "CRITICAL" and not f["passed"])
high_failures = sum(1 for f in findings if f["severity"] == "HIGH" and not f["passed"])
```

### Step 9: Build Red Team Color Team Report

```python
def build_red_team_report(findings, win_scorecard, persona_coverage):
    """
    Produce the color_team_report section with evaluator simulation,
    key strengths/weaknesses, and recommendation.
    """
    # Determine recommendation based on disposition
    if disposition == "BLOCK":
        recommendation = "REVISE_AND_RESUBMIT"
    elif disposition == "ADVISORY":
        recommendation = "PROCEED_WITH_CAUTION"
    else:
        recommendation = "PROCEED"

    # Evaluator simulation score: use win scorecard data
    win_prob = win_scorecard.get("win_probability",
                   win_scorecard.get("summary", {}).get("win_probability", 0))
    if isinstance(win_prob, (int, float)) and win_prob <= 1.0:
        win_prob = win_prob * 100

    # Gather key strengths from passing findings
    key_strengths = []
    for f in findings:
        if f["passed"] and f["severity"] in ("CRITICAL", "HIGH"):
            key_strengths.append(f"{f['rule_name']}: score {f.get('score', 'N/A')}")

    # Gather key weaknesses from failing findings
    key_weaknesses = []
    for f in findings:
        if not f["passed"]:
            key_weaknesses.append(
                f"{f['rule_name']} ({f['severity']}): score {f.get('score', 'N/A')} vs threshold {f.get('threshold', 'N/A')}"
            )

    # Top concerns for the team (limited to 5)
    top_concerns = []
    for f in sorted(findings, key=lambda x: {"CRITICAL": 0, "HIGH": 1, "MEDIUM": 2, "LOW": 3}.get(x["severity"], 4)):
        if not f["passed"]:
            concern = f"[{f['severity']}] {f['rule_name']}: {f.get('corrective_action', {}).get('instruction', 'Review required')[:80]}"
            top_concerns.append(concern)
    top_concerns = top_concerns[:5]

    return {
        "team": "red",
        "recommendation": recommendation,
        "top_concerns": top_concerns,
        "evaluator_simulation_score": round(win_prob, 1),
        "key_strengths": key_strengths[:5],
        "key_weaknesses": key_weaknesses[:5],
        "evaluator_simulation": None,  # Full simulation deferred to SVA-7 Gold Team
        "traceability_integrity": None,
        "theme_threading": None
    }

color_team_report = build_red_team_report(findings, win_scorecard, persona_coverage)
```

### Step 10: Build Corrective Actions Summary

```python
corrective_actions = []
for f in findings:
    if not f["passed"] and f.get("corrective_action"):
        corrective_actions.append({
            "priority": f["severity"],
            "action": f["corrective_action"].get("instruction", "Review and fix"),
            "target_phase": f["corrective_action"].get("target_phase"),
            "auto_correctable": f["corrective_action"].get("auto_correctable", False),
            "rule_id": f["rule_id"]
        })

# Sort: CRITICAL first, then HIGH, MEDIUM, LOW
priority_order = {"CRITICAL": 0, "HIGH": 1, "MEDIUM": 2, "LOW": 3}
corrective_actions.sort(key=lambda x: priority_order.get(x["priority"], 4))
```

### Step 11: Write SVA Report

```python
duration_ms = int((datetime.now() - start_time).total_seconds() * 1000)

sva_report = {
    "validator": "SVA-6",
    "stage_validated": 6,
    "validated_at": datetime.now().isoformat(),
    "disposition": disposition,
    "color_team": "red",
    "summary": {
        "total_rules": len(findings),
        "passed": passed_count,
        "failed": failed_count,
        "critical_failures": critical_failures,
        "high_failures": high_failures,
        "overall_score": overall_score
    },
    "findings": findings,
    "color_team_report": color_team_report,
    "corrective_actions_summary": corrective_actions,
    "execution_metadata": {
        "duration_ms": duration_ms,
        "files_analyzed": 7,
        "data_sources_read": [
            "PERSONA_COVERAGE.json",
            "WIN_SCORECARD.json",
            "GAP_ANALYSIS.md",
            "REQUIREMENT_RISKS.json",
            "bid-context-bundle.json",
            "EVALUATION_CRITERIA.json",
            "requirements-normalized.json"
        ]
    }
}

# Ensure validation directory exists
os.makedirs(f"{folder}/shared/validation", exist_ok=True)

write_json(f"{folder}/shared/validation/sva6-pre-bid.json", sva_report)
```

### Step 12: Report Results

```python
log(f"""
{'='*60}
SVA-6: Pre-Bid Gate / Red Team Final -- {disposition}
{'='*60}
Overall Score: {overall_score}/100
Rules: {len(findings)} total | {passed_count} passed | {failed_count} failed
Critical Failures: {critical_failures} | High Failures: {high_failures}

RED TEAM REPORT:
  Recommendation: {color_team_report['recommendation']}
  Evaluator Simulation Score: {color_team_report['evaluator_simulation_score']}%
  Key Strengths:  {len(color_team_report['key_strengths'])}
  Key Weaknesses: {len(color_team_report['key_weaknesses'])}

Findings:
""")

for f in findings:
    status = "PASS" if f["passed"] else "FAIL"
    icon = "  " if f["passed"] else "  "
    log(f"{icon} [{f['severity']}] {f['rule_id']}: {f['rule_name']} -- {status} (score: {f.get('score', 'N/A')})")

if color_team_report["top_concerns"]:
    log(f"\nTop Concerns:")
    for concern in color_team_report["top_concerns"]:
        log(f"  {concern}")

if color_team_report["key_strengths"]:
    log(f"\nStrengths:")
    for s in color_team_report["key_strengths"]:
        log(f"  + {s}")

if color_team_report["key_weaknesses"]:
    log(f"\nWeaknesses:")
    for w in color_team_report["key_weaknesses"]:
        log(f"  - {w}")

if corrective_actions:
    log(f"\nCorrective Actions ({len(corrective_actions)}):")
    for ca in corrective_actions:
        auto = "[auto]" if ca["auto_correctable"] else "[manual]"
        log(f"  [{ca['priority']}] {auto} {ca['rule_id']} -> Phase {ca['target_phase']}: {ca['action'][:100]}")

log(f"\nDisposition: {disposition}")
log(f"Report: {folder}/shared/validation/sva6-pre-bid.json")
```

## Quality Checklist

- [ ] `sva6-pre-bid.json` created in `shared/validation/`
- [ ] All 6 rules executed with scores
- [ ] Disposition correctly computed (PASS / ADVISORY / BLOCK)
- [ ] Red Team `color_team_report` section populated with:
  - [ ] `evaluator_simulation_score`
  - [ ] `key_strengths` list
  - [ ] `key_weaknesses` list
  - [ ] `recommendation` (PROCEED / PROCEED_WITH_CAUTION / REVISE_AND_RESUBMIT / STOP)
  - [ ] `top_concerns` list
- [ ] Corrective actions populated for failed rules
- [ ] Report conforms to sva-report.schema.json
- [ ] No CRITICAL failures left unaddressed in corrective_actions
