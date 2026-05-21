---
name: phase7d-scoring-win
expert-role: Bid Strategist
domain-expertise: Win probability, scoring models
skill: capture-strategist
sub-skill: bid-decision
---

# Phase 7d: Bid Scoring Model

## Purpose

Calculate pre-submission win probability score and identify disqualifiers.

## ⛔ HONEST-MATH DISCIPLINE (codified 2026-05-21 — MARS Phase 7d incident)

The default mechanical formula in this phase file (binary spec-coverage × gate-passed × no-Critical-reqs) systematically produces **false-confidence** scores on bids with high structural risk + PLANNED-not-ADDRESSED compliance status. The MARS 2026-05-21 first pass returned 88% win_probability when the upstream Phase 1.9 Go/No-Go was 58/100 and 13 Critical/HIGH structural risks all carried IDENTIFIED-only mitigations. The honest-math corrections below MUST be applied — they are not optional polish.

**Five mandatory corrections:**

1. **Alignment uses `chain_completeness.avg_score`, NOT binary specifications-list presence.** From `UNIFIED_RTM.verification.chain_completeness.avg_score`. Reflects that 0 of 1116 traceability chains may be `complete` and most are only `partial` — binary presence overstates alignment.

2. **Compliance distinguishes ADDRESSED vs PLANNED.** Weight `coverage_status=ADDRESSED` at 1.0 and `coverage_status=PLANNED` at 0.85. `PLANNED` means "planned response," not "proven coverage." Gate-passed compliance caps at 95% when most items are PLANNED, not 100%.

3. **Risk-mitigation applies a structural-risk overlay penalty.** For each Critical or HIGH structural risk (from `REQUIREMENT_RISKS.structural_risks[]`) whose `mitigation_strategies[*].status` are ALL `IDENTIFIED` (none `IMPLEMENTED`): subtract 6 pts. For each Critical/HIGH structural risk with NO mitigations at all: subtract 10 pts. Cap at 60-pt penalty.

4. **Win probability blends mechanical math with Go/No-Go anchor.** `win_probability = round(0.6 * mechanical_weighted_score + 0.4 * (0.6 * gonogo.overall_score + 0.4 * gonogo.assessment_areas.win_probability.score))`. The Go/No-Go anchor prevents the mechanical formula from drifting far above Lohfeld/Shipley assessment.

5. **All-IDENTIFIED-mitigation structural risks count as UN-mitigated for the critical-disqualifier check.** If ≥10 Critical/HIGH structural risks have all-IDENTIFIED mitigations, fire the `exhibit_k_security_failure` disqualifier and CLAMP win_probability to ≤15. Paper plans are not protection.

**Verification:** the next phase verifier (`verifier-phase7d-scoring-win.md`) should re-derive each factor score independently and compare against the producer's claim; if delta > 5 pts on any factor, FAIL the verifier.

## Inputs

- `{folder}/shared/EVALUATION_CRITERIA.json`
- `{folder}/shared/COMPLIANCE_MATRIX.json`
- `{folder}/shared/PERSONA_COVERAGE.json`
- `{folder}/shared/REQUIREMENT_RISKS.json`

## Required Outputs

- `{folder}/shared/WIN_SCORECARD.json`

## Instructions

### Step 1: Define Scoring Model

```python
SCORING_MODEL = {
    "alignment": {
        "weight": 0.30,
        "description": "Requirements coverage and explicit mapping"
    },
    "value": {
        "weight": 0.25,
        "description": "ROI metrics and value proposition"
    },
    "risk_mitigation": {
        "weight": 0.20,
        "description": "Risk identification and mitigation quality"
    },
    "compliance": {
        "weight": 0.15,
        "description": "Mandatory requirements coverage"
    },
    "presentation": {
        "weight": 0.10,
        "description": "Document quality and professionalism"
    }
}

DISQUALIFIERS = [
    "mandatory_item_missing",
    "page_limit_exceeded",
    "submission_deadline_missed",
    "required_certification_missing",
    "conflict_of_interest"
]
```

### Step 2: Score Alignment

```python
def score_alignment(folder):
    """Score requirements alignment."""
    requirements = read_json(f"{folder}/shared/requirements-normalized.json")
    compliance = read_json(f"{folder}/shared/COMPLIANCE_MATRIX.json")

    reqs = requirements.get("requirements", [])
    mandatory = compliance.get("mandatory_items", [])

    # Requirement coverage
    total_reqs = len(reqs)
    with_specs = sum(1 for r in reqs if r.get("mapped_specs"))
    coverage_rate = with_specs / total_reqs if total_reqs > 0 else 0

    # Mandatory coverage
    addressed_mandatory = sum(
        1 for m in mandatory
        if m.get("coverage", {}).get("status") in ["PLANNED", "ADDRESSED"]
    )
    mandatory_rate = addressed_mandatory / len(mandatory) if mandatory else 1.0

    # Calculate score
    alignment_score = (coverage_rate * 0.6 + mandatory_rate * 0.4) * 100

    return {
        "score": round(alignment_score, 1),
        "total_requirements": total_reqs,
        "mapped_to_specs": with_specs,
        "mandatory_items": len(mandatory),
        "mandatory_addressed": addressed_mandatory
    }

alignment = score_alignment(folder)
```

### Step 3: Score Value Proposition

```python
def score_value(folder):
    """Score value proposition."""
    estimation = None
    estimation_path = f"{folder}/outputs/EFFORT_ESTIMATION.md"

    if os.path.exists(estimation_path):
        content = read_file(estimation_path)

        # Check for AI savings documentation
        has_ai_savings = "ai" in content.lower() and "savings" in content.lower()

        # Check for ROI discussion
        has_roi = "roi" in content.lower() or "return on investment" in content.lower()

        # Check for efficiency metrics
        has_efficiency = any(term in content.lower() for term in ["efficiency", "productivity", "faster"])

        score = 70  # Base score
        if has_ai_savings:
            score += 10
        if has_roi:
            score += 10
        if has_efficiency:
            score += 10

        return {
            "score": min(100, score),
            "has_ai_savings": has_ai_savings,
            "has_roi": has_roi,
            "has_efficiency": has_efficiency
        }

    return {"score": 50, "note": "Estimation document not found"}

value = score_value(folder)
```

### Step 4: Score Risk Mitigation

```python
def score_risk_mitigation(folder):
    """Score risk mitigation quality."""
    risks_path = f"{folder}/shared/REQUIREMENT_RISKS.json"

    if os.path.exists(risks_path):
        risks = read_json(risks_path)

        total_reqs = risks.get("summary", {}).get("total_assessed", 0)
        high_risk = risks.get("summary", {}).get("high_risk", 0)

        # Lower high-risk ratio is better
        high_risk_ratio = high_risk / total_reqs if total_reqs > 0 else 0.5

        # Check for mitigation strategies
        reqs_with_mitigation = sum(
            1 for r in risks.get("requirements", [])
            if r.get("risk", {}).get("suggested_mitigations")
        )
        mitigation_rate = reqs_with_mitigation / total_reqs if total_reqs > 0 else 0

        score = 100 - (high_risk_ratio * 30) + (mitigation_rate * 30)
        score = max(0, min(100, score))

        return {
            "score": round(score, 1),
            "high_risk_count": high_risk,
            "high_risk_ratio": round(high_risk_ratio * 100, 1),
            "mitigation_rate": round(mitigation_rate * 100, 1)
        }

    return {"score": 60, "note": "Risk assessment not found"}

risk_mitigation = score_risk_mitigation(folder)
```

### Step 5: Score Compliance

```python
def score_compliance(folder):
    """Score compliance coverage."""
    compliance = read_json(f"{folder}/shared/COMPLIANCE_MATRIX.json")

    gate_status = compliance.get("gate_status", {})
    passed = gate_status.get("passed", False)
    coverage_pct = gate_status.get("coverage_percentage", 0)

    if passed:
        score = 100
    else:
        score = coverage_pct

    return {
        "score": score,
        "gate_passed": passed,
        "coverage_percentage": coverage_pct,
        "gap_count": gate_status.get("gap_count", 0)
    }

compliance_score = score_compliance(folder)
```

### Step 6: Score Presentation

```python
def score_presentation(folder):
    """Score document quality and presentation."""
    import glob
    import os

    output_files = glob.glob(f"{folder}/outputs/*.md")

    # Check file existence and sizes
    required_docs = ["EXECUTIVE_SUMMARY.md", "REQUIREMENTS_CATALOG.md", "ARCHITECTURE.md"]
    docs_present = sum(1 for doc in required_docs if any(doc in f for f in output_files))

    # Check total content volume
    total_size = sum(os.path.getsize(f) for f in output_files)
    size_score = min(100, total_size / 1024 / 100 * 100)  # 100KB = 100%

    score = (docs_present / len(required_docs) * 50) + (size_score * 0.5)

    return {
        "score": round(min(100, score), 1),
        "required_docs_present": docs_present,
        "total_content_kb": round(total_size / 1024, 1)
    }

presentation = score_presentation(folder)
```

### Step 7: Check Disqualifiers

```python
def check_disqualifiers(folder):
    """Check for potential disqualifiers."""
    disqualifiers = []

    compliance = read_json(f"{folder}/shared/COMPLIANCE_MATRIX.json")

    # Mandatory item missing
    if not compliance.get("gate_status", {}).get("passed", True):
        gaps = compliance.get("gate_status", {}).get("gap_items", [])
        disqualifiers.append({
            "type": "mandatory_item_missing",
            "severity": "CRITICAL",
            "details": f"{len(gaps)} mandatory items unaddressed",
            "items": [g["text"][:50] for g in gaps[:3]]
        })

    # Format compliance
    format_reqs = compliance.get("format_requirements", {})
    if format_reqs.get("page_limit"):
        # Would need to check actual document pages
        pass

    return disqualifiers

disqualifiers = check_disqualifiers(folder)
```

### Step 8: Calculate Win Probability

```python
def calculate_win_probability(scores, disqualifiers):
    """Calculate overall win probability."""
    if disqualifiers:
        # Any critical disqualifier = low probability
        critical = [d for d in disqualifiers if d.get("severity") == "CRITICAL"]
        if critical:
            return {
                "probability": 10,
                "confidence": "LOW",
                "reason": "Critical disqualifiers present"
            }

    # Weighted score
    weighted_score = (
        scores["alignment"]["score"] * SCORING_MODEL["alignment"]["weight"] +
        scores["value"]["score"] * SCORING_MODEL["value"]["weight"] +
        scores["risk_mitigation"]["score"] * SCORING_MODEL["risk_mitigation"]["weight"] +
        scores["compliance"]["score"] * SCORING_MODEL["compliance"]["weight"] +
        scores["presentation"]["score"] * SCORING_MODEL["presentation"]["weight"]
    )

    # Convert to probability (scale and adjust)
    probability = min(95, max(5, weighted_score * 0.9))

    if probability >= 80:
        confidence = "HIGH"
    elif probability >= 60:
        confidence = "MEDIUM"
    else:
        confidence = "LOW"

    return {
        "probability": round(probability, 0),
        "confidence": confidence,
        "weighted_score": round(weighted_score, 1)
    }

scores = {
    "alignment": alignment,
    "value": value,
    "risk_mitigation": risk_mitigation,
    "compliance": compliance_score,
    "presentation": presentation
}

win_probability = calculate_win_probability(scores, disqualifiers)
```

### Step 9: Write Output

```python
def generate_win_recommendations(scores, disqualifiers):
    """Generate recommendations to improve win probability."""
    recs = []

    if disqualifiers:
        recs.append({
            "priority": "CRITICAL",
            "action": "Address all disqualifiers before submission"
        })

    # Find lowest scoring area
    min_score_area = min(scores.items(), key=lambda x: x[1]["score"])
    if min_score_area[1]["score"] < 80:
        recs.append({
            "priority": "HIGH",
            "action": f"Improve {min_score_area[0]} score (currently {min_score_area[1]['score']}%)"
        })

    return recs

# V5-F5 fix: flatten win_probability to a top-level scalar for SVA-6 compatibility.
# win_probability is the calculated dict from Step 8 (probability/confidence/weighted_score).
# Downstream SVA-6 reads win_scorecard.get("win_probability", 0) and expects a number.
win_probability_detail = win_probability  # full dict preserved under a separate key
win_probability_scalar = win_probability_detail.get("probability", 0)

win_scorecard = {
    "calculated_at": datetime.now().isoformat(),
    "win_probability": win_probability_scalar,
    "win_probability_detail": win_probability_detail,
    "scores": scores,
    "disqualifiers": disqualifiers,
    "scoring_model": SCORING_MODEL,
    "recommendations": generate_win_recommendations(scores, disqualifiers)
}

write_json(f"{folder}/shared/WIN_SCORECARD.json", win_scorecard)
```

### Step 10: Report Results

```python
# Note: post-V5-F5 fix, win_probability_detail carries the full dict.
prob = win_probability_detail["probability"]
conf = win_probability_detail["confidence"]

log(f"""
🎯 Bid Scoring Model Complete
=============================
Win Probability: {prob}% ({conf} confidence)

Score Breakdown:
  Alignment:      {alignment["score"]}% (weight: 30%)
  Value:          {value["score"]}% (weight: 25%)
  Risk Mitigation:{risk_mitigation["score"]}% (weight: 20%)
  Compliance:     {compliance_score["score"]}% (weight: 15%)
  Presentation:   {presentation["score"]}% (weight: 10%)

Disqualifiers: {len(disqualifiers)}
""")
```

## Quality Checklist (MANDATORY — report each by name with evidence)

The phase agent MUST verify each of the following BEFORE reporting completion. The agent's completion report MUST include a checklist-results block with:
- Item name (verbatim from below)
- PASS / FAIL / SKIPPED-WITH-REASON
- Evidence (file:line citation, grep result, file size, assertion that ran, etc.)

"All checks passed" without per-item evidence is NOT acceptable.

### Required output files
1. **WIN_SCORECARD.json** exists at `{folder}/shared/WIN_SCORECARD.json` — evidence: `ls -la` size > 200 bytes and parses as valid JSON

### Schema fidelity
2. **WIN_SCORECARD.json top-level keys** include `calculated_at`, `win_probability`, `win_probability_detail`, `scores`, `disqualifiers`, `scoring_model`, `recommendations` — evidence: list actual top-level keys found
3. **win_probability** is a scalar number (not a dict) — evidence: confirm `type(win_scorecard["win_probability"])` is int or float (V5-F5 fix); print actual value
4. **All 5 scoring areas present** in `scores`: alignment, value, risk_mitigation, compliance, presentation — evidence: print `list(scores.keys())`
5. **Every score entry** has a `score` key (float 0-100) — evidence: print `{k: v.get("score") for k, v in scores.items()}`
6. No `[:N]` slicing applied to deliverable content strings — evidence: grep for `\[:[0-9]+\]` in production code paths returned 0 hits

### Cross-stage consistency
7. **Disqualifiers identified** — evidence: print `len(disqualifiers)` and any CRITICAL disqualifier names (zero is acceptable if none apply)
8. **Recommendations generated** — evidence: print `len(recommendations)` and confirm entries for any score < 80

### Anti-regression rules (universal)
9. **UTF-8 encoding** on every `open()` call — evidence: search this phase's emitted scripts/code for `encoding='utf-8'` in every file-open
10. **ensure_ascii=False** on every `json.dump` call — evidence: same grep
11. **No `_Showing N of M_` row-cap notices** in any deliverable markdown — evidence: grep returned 0 matches
12. **No empty `|  |` mitigation/cell patterns** in any deliverable table — evidence: grep returned 0 matches
13. **No mid-word table-cell truncations** — evidence: line-by-line cell-end check returned 0 hits

### Memory discipline
14. **Relevant SAFS memory entries reviewed and applied** — evidence: list which memory files were read and which rules were applicable (e.g., "win_probability is a scalar not dict — V5-F5 fix applied; win_probability_detail preserves full dict")
