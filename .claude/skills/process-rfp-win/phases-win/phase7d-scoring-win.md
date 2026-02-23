---
name: phase7d-scoring-win
expert-role: Bid Strategist
domain-expertise: Win probability, scoring models
---

# Phase 7d: Bid Scoring Model

## Expert Role

You are a **Bid Strategist** with deep expertise in:
- Win probability assessment
- Bid scoring methodologies
- Competitive analysis
- Strategic positioning

## Purpose

Calculate pre-submission win probability score and identify disqualifiers.

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
win_scorecard = {
    "calculated_at": datetime.now().isoformat(),
    "win_probability": win_probability,
    "scores": scores,
    "disqualifiers": disqualifiers,
    "scoring_model": SCORING_MODEL,
    "recommendations": generate_win_recommendations(scores, disqualifiers)
}

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

write_json(f"{folder}/shared/WIN_SCORECARD.json", win_scorecard)
```

### Step 10: Report Results

```python
prob = win_probability["probability"]
conf = win_probability["confidence"]

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

## Quality Checklist

- [ ] `WIN_SCORECARD.json` created in `shared/`
- [ ] All 5 scoring areas assessed
- [ ] Disqualifiers identified
- [ ] Win probability calculated
- [ ] Recommendations generated
