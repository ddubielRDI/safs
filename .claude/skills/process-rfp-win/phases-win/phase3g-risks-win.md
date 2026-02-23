---
name: phase3g-risks-win
expert-role: Risk Analyst
domain-expertise: Risk assessment, mitigation strategies
---

# Phase 3g: Requirement Risk Assessment

## Expert Role

You are a **Risk Analyst** with deep expertise in:
- Risk identification and assessment
- Mitigation strategy development
- Risk categorization (PM, Software, Domain)
- Quantitative risk analysis

## Purpose

Assess risks for each requirement and generate mitigation strategies.

## Inputs

- `{folder}/shared/requirements-normalized.json` - Normalized requirements
- `{folder}/shared/domain-context.json` - Domain context

## Required Outputs

- `{folder}/outputs/REQUIREMENT_RISKS.md` - Risk assessment document
- `{folder}/shared/REQUIREMENT_RISKS.json` - Machine-readable risk data

## Instructions

### Step 1: Load Requirements

```python
requirements = read_json(f"{folder}/shared/requirements-normalized.json")
domain_context = read_json(f"{folder}/shared/domain-context.json")

all_reqs = requirements.get("requirements", [])
```

### Step 2: Define Risk Categories

```python
RISK_CATEGORIES = {
    "PM": {
        "name": "Project Management",
        "indicators": ["schedule", "resource", "scope", "budget", "stakeholder", "communication"],
        "mitigations": [
            "Regular status reporting",
            "Change management process",
            "Resource buffer allocation",
            "Stakeholder alignment meetings"
        ]
    },
    "SOFTWARE": {
        "name": "Software/Technical",
        "indicators": ["integration", "performance", "scalability", "security", "complexity", "technology"],
        "mitigations": [
            "Proof of concept development",
            "Performance testing early",
            "Code reviews and static analysis",
            "Architecture review board"
        ]
    },
    "DOMAIN": {
        "name": "Domain/Business",
        "indicators": ["compliance", "regulation", "workflow", "data", "process", "user adoption"],
        "mitigations": [
            "Subject matter expert involvement",
            "User acceptance testing",
            "Compliance audit checkpoints",
            "Training program development"
        ]
    }
}
```

### Step 3: Assess Each Requirement

```python
def assess_requirement_risk(req):
    """Assess risk for a single requirement."""
    text_lower = req.get("text", "").lower()

    # Determine risk category
    category_scores = {}
    for cat_id, cat_info in RISK_CATEGORIES.items():
        score = sum(1 for ind in cat_info["indicators"] if ind in text_lower)
        category_scores[cat_id] = score

    primary_category = max(category_scores, key=category_scores.get)

    # Calculate risk factors
    risk_factors = []

    # Complexity risk
    if len(text_lower.split()) > 50:
        risk_factors.append("High complexity (long requirement)")

    if "integration" in text_lower or "interface" in text_lower:
        risk_factors.append("Integration dependency")

    if any(term in text_lower for term in ["real-time", "high availability", "99.9%"]):
        risk_factors.append("Stringent performance requirement")

    if any(term in text_lower for term in ["ferpa", "hipaa", "pci", "compliance"]):
        risk_factors.append("Regulatory compliance requirement")

    if any(term in text_lower for term in ["legacy", "migrate", "convert"]):
        risk_factors.append("Legacy system dependency")

    if any(term in text_lower for term in ["custom", "proprietary", "unique"]):
        risk_factors.append("Custom development required")

    # Calculate risk score (1-5)
    base_score = len(risk_factors)
    if req.get("priority") == "CRITICAL":
        base_score += 1
    risk_score = min(5, max(1, base_score))

    # Determine risk level
    if risk_score >= 4:
        risk_level = "HIGH"
    elif risk_score >= 2:
        risk_level = "MEDIUM"
    else:
        risk_level = "LOW"

    return {
        "category": primary_category,
        "category_name": RISK_CATEGORIES[primary_category]["name"],
        "risk_score": risk_score,
        "risk_level": risk_level,
        "risk_factors": risk_factors,
        "suggested_mitigations": RISK_CATEGORIES[primary_category]["mitigations"][:2]
    }

# Assess all requirements
for req in all_reqs:
    req["risk_assessment"] = assess_requirement_risk(req)
```

### Step 4: Generate Risk Heat Map Data

```python
def generate_heat_map(requirements):
    """Generate risk heat map data."""
    heat_map = {
        "HIGH": {"PM": 0, "SOFTWARE": 0, "DOMAIN": 0},
        "MEDIUM": {"PM": 0, "SOFTWARE": 0, "DOMAIN": 0},
        "LOW": {"PM": 0, "SOFTWARE": 0, "DOMAIN": 0}
    }

    for req in requirements:
        risk = req.get("risk_assessment", {})
        level = risk.get("risk_level", "LOW")
        category = risk.get("category", "SOFTWARE")
        heat_map[level][category] += 1

    return heat_map

heat_map = generate_heat_map(all_reqs)
```

### Step 5: Generate Risk Document

```python
def generate_risks_md(requirements, heat_map, domain):
    high_risk = [r for r in requirements if r.get("risk_assessment", {}).get("risk_level") == "HIGH"]
    medium_risk = [r for r in requirements if r.get("risk_assessment", {}).get("risk_level") == "MEDIUM"]
    low_risk = [r for r in requirements if r.get("risk_assessment", {}).get("risk_level") == "LOW"]

    doc = f"""# Requirement Risk Assessment

**Domain:** {domain}
**Generated:** {datetime.now().strftime('%Y-%m-%d %H:%M')}
**Total Requirements Assessed:** {len(requirements)}

---

## Executive Summary

| Risk Level | Count | Percentage |
|------------|-------|------------|
| HIGH | {len(high_risk)} | {len(high_risk)/len(requirements)*100:.1f}% |
| MEDIUM | {len(medium_risk)} | {len(medium_risk)/len(requirements)*100:.1f}% |
| LOW | {len(low_risk)} | {len(low_risk)/len(requirements)*100:.1f}% |

---

## Risk Heat Map

```
           │ PM    │ SOFTWARE │ DOMAIN │
    ───────┼───────┼──────────┼────────┤
    HIGH   │ {heat_map["HIGH"]["PM"]:^5} │ {heat_map["HIGH"]["SOFTWARE"]:^8} │ {heat_map["HIGH"]["DOMAIN"]:^6} │
    MEDIUM │ {heat_map["MEDIUM"]["PM"]:^5} │ {heat_map["MEDIUM"]["SOFTWARE"]:^8} │ {heat_map["MEDIUM"]["DOMAIN"]:^6} │
    LOW    │ {heat_map["LOW"]["PM"]:^5} │ {heat_map["LOW"]["SOFTWARE"]:^8} │ {heat_map["LOW"]["DOMAIN"]:^6} │
```

---

## High-Risk Requirements (Immediate Attention)

"""

    for req in high_risk[:15]:
        risk = req.get("risk_assessment", {})
        doc += f"""### {req.get('canonical_id', 'N/A')} - {risk.get('category_name')}

**Requirement:** {req.get('text', '')[:200]}...

**Risk Score:** {risk.get('risk_score')}/5 ({risk.get('risk_level')})

**Risk Factors:**
"""
        for factor in risk.get("risk_factors", []):
            doc += f"- {factor}\n"

        doc += "\n**Recommended Mitigations:**\n"
        for mitigation in risk.get("suggested_mitigations", []):
            doc += f"- {mitigation}\n"

        doc += "\n---\n\n"

    doc += """
## Medium-Risk Requirements

| Req ID | Category | Score | Key Risk Factor |
|--------|----------|-------|-----------------|
"""

    for req in medium_risk[:20]:
        risk = req.get("risk_assessment", {})
        key_factor = risk.get("risk_factors", ["None"])[0] if risk.get("risk_factors") else "Standard complexity"
        doc += f"| {req.get('canonical_id', 'N/A')} | {risk.get('category')} | {risk.get('risk_score')}/5 | {key_factor[:40]} |\n"

    doc += """

---

## Risk Mitigation Strategy

### Project Management Risks
1. Implement weekly risk review meetings
2. Maintain risk register with ownership
3. Establish escalation procedures
4. Allocate contingency buffer (15-20%)

### Software/Technical Risks
1. Conduct proof of concepts for high-risk integrations
2. Implement continuous integration/testing
3. Schedule architecture reviews at key milestones
4. Plan for performance testing early

### Domain/Business Risks
1. Engage subject matter experts throughout
2. Schedule regular compliance checkpoints
3. Develop comprehensive training program
4. Plan for user acceptance testing phases

---

## Risk Monitoring Dashboard

### Key Risk Indicators (KRIs)
| Indicator | Threshold | Current | Status |
|-----------|-----------|---------|--------|
| High-risk requirement % | < 15% | {len(high_risk)/len(requirements)*100:.1f}% | {"🟢" if len(high_risk)/len(requirements) < 0.15 else "🔴"} |
| Unmitigated risks | 0 | TBD | ⚪ |
| Risk trend | Decreasing | TBD | ⚪ |

---

## Appendix: Full Risk Register

| Req ID | Category | Level | Score | Primary Factor |
|--------|----------|-------|-------|----------------|
"""

    for req in requirements:
        risk = req.get("risk_assessment", {})
        factor = risk.get("risk_factors", ["N/A"])[0] if risk.get("risk_factors") else "N/A"
        doc += f"| {req.get('canonical_id', 'N/A')} | {risk.get('category', 'N/A')} | {risk.get('risk_level', 'N/A')} | {risk.get('risk_score', 0)}/5 | {factor[:30]} |\n"

    return doc

domain = domain_context.get("selected_domain", "Generic")
risks_md = generate_risks_md(all_reqs, heat_map, domain)
write_file(f"{folder}/outputs/REQUIREMENT_RISKS.md", risks_md)
```

### Step 5b: Assign Stable RTM Risk IDs (NEW - for UNIFIED_RTM.json)

```python
## RTM CONTRIBUTION: Assign stable risk_id (RISK-001) and mitigation_id (MIT-001-01)
## Phase 4 will use these to link risks into the UNIFIED_RTM.json chain

risk_id_counter = 1
risks_for_rtm = []

# Consolidate risks by requirement - group related risks
for req in all_reqs:
    risk = req.get("risk_assessment", {})
    if risk.get("risk_level") in ["HIGH", "MEDIUM"]:  # Only track non-trivial risks
        risk_id = f"RISK-{risk_id_counter:03d}"
        req["risk_assessment"]["risk_id"] = risk_id

        # Assign mitigation IDs
        mitigations = []
        for mit_idx, mitigation_text in enumerate(risk.get("suggested_mitigations", []), 1):
            mitigation_id = f"MIT-{risk_id_counter:03d}-{mit_idx:02d}"
            mitigations.append({
                "mitigation_id": mitigation_id,
                "strategy": mitigation_text,
                "owner_role": infer_owner_role(risk.get("category", "SOFTWARE")),
                "timeline": "Phase-dependent",
                "status": "IDENTIFIED",
                "bid_location": None,  # Populated post-bid-authoring
                "evidence_ids": []
            })

        risks_for_rtm.append({
            "risk_id": risk_id,
            "title": f"{risk.get('category_name', 'Risk')}: {req.get('text', '')[:80]}",
            "category": risk.get("category", "SOFTWARE"),
            "likelihood": min(5, risk.get("risk_score", 1)),
            "impact": min(5, max(1, risk.get("risk_score", 1) + 1)),  # Impact slightly higher than likelihood
            "risk_score": risk.get("risk_score", 1) * min(5, max(1, risk.get("risk_score", 1) + 1)),
            "risk_level": risk.get("risk_level"),
            "linked_requirement_ids": [req.get("canonical_id", req.get("id", ""))],
            "mitigation_strategies": mitigations
        })

        risk_id_counter += 1

def infer_owner_role(category):
    """Infer the responsible role based on risk category."""
    role_map = {
        "PM": "Program Manager",
        "SOFTWARE": "Technical Architect",
        "DOMAIN": "Business Analyst"
    }
    return role_map.get(category, "Program Manager")
```

### Step 6: Write JSON Output

```python
risks_json = {
    "assessed_at": datetime.now().isoformat(),
    "summary": {
        "total_assessed": len(all_reqs),
        "high_risk": len([r for r in all_reqs if r.get("risk_assessment", {}).get("risk_level") == "HIGH"]),
        "medium_risk": len([r for r in all_reqs if r.get("risk_assessment", {}).get("risk_level") == "MEDIUM"]),
        "low_risk": len([r for r in all_reqs if r.get("risk_assessment", {}).get("risk_level") == "LOW"]),
        "total_risks_tracked": len(risks_for_rtm),
        "total_mitigations": sum(len(r["mitigation_strategies"]) for r in risks_for_rtm)
    },
    "heat_map": heat_map,
    "requirements": [
        {
            "id": req.get("canonical_id"),
            "text": req.get("text"),
            "risk": req.get("risk_assessment")
        }
        for req in all_reqs
    ],
    # RTM: Structured risk entities for Phase 4 to consume
    "rtm_risks": risks_for_rtm
}

write_json(f"{folder}/shared/REQUIREMENT_RISKS.json", risks_json)
```

## Quality Checklist

- [ ] `REQUIREMENT_RISKS.md` created in `outputs/`
- [ ] `REQUIREMENT_RISKS.json` created in `shared/`
- [ ] 70%+ requirements have risk assessments
- [ ] Heat map generated
- [ ] High-risk items have mitigation strategies
- [ ] Risk categories properly assigned
