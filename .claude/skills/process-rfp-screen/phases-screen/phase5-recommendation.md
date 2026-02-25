---
name: phase5-recommendation
expert-role: Risk Analyst & Bid Decision Advisor
domain-expertise: Risk assessment, opportunity analysis, bid strategy, portfolio management
---

# Phase 5: Risk Assessment & Recommendation

**Expert Role:** Risk Analyst & Bid Decision Advisor

**Purpose:** Consolidate risks from all prior phases, check historical bid patterns, assess opportunities, and assemble the final recommendation. Produces both the risk assessment and the consolidated BID_SCREEN.json.

**Inputs:**
- `{folder}/screen/rfp-summary.json` — Phase 1 output
- `{folder}/screen/go-nogo-score.json` — Phase 2 output
- `{folder}/screen/client-intel-snapshot.json` — Phase 3 output (may not exist if --quick)
- `{folder}/screen/compliance-check.json` — Phase 4a output
- `{folder}/screen/past-projects-match.json` — Phase 4b output
- `config-win/bid-outcomes.json` — Historical bid data (optional)

**Required Outputs:**
- `{folder}/screen/risk-assessment.json` (>1KB)
- `{folder}/screen/BID_SCREEN.json` (>3KB) — Consolidated machine-readable data

---

## Instructions

### Step 1: Load All Prior Phase Outputs

```python
rfp_summary = read_json(f"{folder}/screen/rfp-summary.json")
go_nogo = read_json(f"{folder}/screen/go-nogo-score.json")

# Optional outputs
client_intel = read_json_safe(f"{folder}/screen/client-intel-snapshot.json")
compliance = read_json(f"{folder}/screen/compliance-check.json")
past_matches = read_json(f"{folder}/screen/past-projects-match.json")

# Phase 4.5 output (preliminary themes)
preliminary_themes = read_json_safe(f"{folder}/screen/preliminary-themes.json")
```

### Step 2: Consolidate Risks

```python
risks = []

# From Go/No-Go assessment area risks
for risk in go_nogo.get("overall_risks", []):
    risks.append({
        "risk": risk,
        "source": "go_nogo_scoring",
        "severity": "high" if go_nogo.get("overall_score", 0) < 40 else "medium",
        "category": "scoring"
    })

# From Compliance (Phase 4a)
for item in compliance.get("compliance_items", []):
    if item["status"] == "GAP":
        risks.append({
            "risk": f"Compliance gap: {item['requirement'][:100]}",
            "source": "compliance_check",
            "severity": "high",
            "category": "compliance"
        })
    elif item["status"] == "RISK":
        risks.append({
            "risk": f"Compliance risk: {item['requirement'][:100]}",
            "source": "compliance_check",
            "severity": "medium",
            "category": "compliance"
        })

# From Past Projects (Phase 4b)
match_quality = past_matches.get("match_quality", "weak")
if match_quality == "weak":
    risks.append({
        "risk": "Weak past project alignment — limited relevant experience to cite",
        "source": "past_project_matching",
        "severity": "medium",
        "category": "experience"
    })

# Timeline risk
# Helper to find assessment area by name
def find_area(areas, name_prefix):
    for a in areas:
        if a.get("name", "").lower().startswith(name_prefix.lower()):
            return a
    return {}

assessment_areas = go_nogo.get("assessment_areas", [])
resource_area = find_area(assessment_areas, "Resource Availability")
resource_score = resource_area.get("score", 0)
# Resource Availability now includes timeline feasibility assessment
if resource_score < 40:  # 40/100 equivalent to old 10/20 threshold
    risks.append({
        "risk": "Tight or unknown deadline — may not allow adequate proposal preparation",
        "source": "timeline_analysis",
        "severity": "high",
        "category": "timeline"
    })

# Dealbreaker assessment
dealbreakers = [r for r in risks if r["severity"] == "high" and r["category"] in ["compliance", "scoring"]]
has_dealbreaker = len(dealbreakers) > 0

# Cap at 8 risks, prioritized by severity
risks.sort(key=lambda r: {"high": 0, "medium": 1, "low": 2}[r["severity"]])
risks = risks[:8]
```

### Step 3: Check Historical Bid Patterns

```python
historical_context = {"has_data": False}

outcomes_path = f"{CONFIG_DIR}/bid-outcomes.json"
if os.path.exists(outcomes_path):
    outcomes_data = read_json_safe(outcomes_path)
    outcomes = outcomes_data.get("outcomes", [])
    completed = [o for o in outcomes if o.get("outcome") in ("win", "loss")]

    if len(completed) >= 3:
        rfp_domain = (rfp_summary.get("industry_domain") or "").lower()
        domain_wins = sum(1 for o in completed if o.get("domain", "").lower() == rfp_domain and o["outcome"] == "win")
        domain_total = sum(1 for o in completed if o.get("domain", "").lower() == rfp_domain)

        historical_context = {
            "has_data": True,
            "total_bids": len(completed),
            "overall_win_rate": sum(1 for o in completed if o["outcome"] == "win") / len(completed),
            "domain_win_rate": domain_wins / max(domain_total, 1) if domain_total > 0 else None,
            "domain_bids": domain_total,
            "advisory": f"Based on {len(completed)} past bids"
        }
```

### Step 4: Assess Opportunities

```python
opportunities = []

# From Go/No-Go mitigations and high-scoring area evidence
for mitigation in go_nogo.get("overall_mitigations", []):
    opportunities.append({"opportunity": mitigation, "source": "go_nogo_scoring"})

# High-scoring areas are opportunities
for area in go_nogo.get("assessment_areas", []):
    if area.get("score", 0) >= 70:
        opportunities.append({
            "opportunity": f"Strong {area['name']} (score: {area['score']}/100)",
            "source": "go_nogo_scoring"
        })

# Strong project matches
top_match = past_matches.get("matched_projects", [{}])[0] if past_matches.get("matched_projects") else {}
if top_match.get("relevance_score", 0) > 15:
    opportunities.append({
        "opportunity": f"Strong past performance: {top_match.get('title', '')} (score: {top_match.get('relevance_score')})",
        "source": "past_project_matching"
    })

# Client intel opportunities (if available)
if client_intel and client_intel.get("status") == "complete":
    tech_stack = client_intel.get("intelligence", {}).get("technology_stack", [])
    if tech_stack:
        opportunities.append({
            "opportunity": f"Client tech stack identified ({len(tech_stack)} technologies) — can tailor approach",
            "source": "client_intelligence"
        })
```

### Step 5: Generate Final Recommendation

```python
total_score = go_nogo.get("overall_score", 0)
recommendation = go_nogo.get("recommendation", "NO_GO")

# Generate rationale
if recommendation == "GO":
    rationale = f"Score {total_score}/100 meets GO threshold. "
    if opportunities:
        rationale += f"{len(opportunities)} competitive advantages identified. "
    if risks:
        rationale += f"{len(risks)} risks noted but manageable."
    next_steps = [
        f"Run full pipeline: /process-rfp-win {folder}",
        "Estimated full pipeline time: 3-4 hours",
        "Focus areas: " + ", ".join(go_nogo.get("overall_mitigations", [])[:3])
    ]
    # Append top preliminary theme names to next steps
    if preliminary_themes and preliminary_themes.get("themes"):
        top_theme_names = [t["name"] for t in preliminary_themes["themes"][:3]]
        next_steps.append("Preliminary win themes: " + "; ".join(top_theme_names))
elif recommendation == "CONDITIONAL":
    rationale = f"Score {total_score}/100 in CONDITIONAL range. "
    if dealbreakers:
        rationale += f"{len(dealbreakers)} potential dealbreaker(s) require resolution. "
    rationale += "Review risks before committing resources."
    next_steps = [
        "Resolve conditions: " + "; ".join(r["risk"][:60] for r in dealbreakers[:3]),
        f"If conditions resolved, run: /process-rfp-win {folder}",
        "Consider team capacity and competing bids"
    ]
else:  # NO_GO
    rationale = f"Score {total_score}/100 below threshold. "
    rationale += "; ".join(r["risk"][:60] for r in risks[:3])
    next_steps = [
        "Do not invest in full bid preparation",
        "Override available if strategic reasons exist",
        "Consider monitoring for future opportunities with this client"
    ]
```

### Step 6: Write Risk Assessment

```python
risk_assessment = {
    "phase": "5",
    "timestamp": datetime.now().isoformat(),
    "risks": risks,
    "risk_summary": {
        "total": len(risks),
        "high": sum(1 for r in risks if r["severity"] == "high"),
        "medium": sum(1 for r in risks if r["severity"] == "medium"),
        "low": sum(1 for r in risks if r["severity"] == "low")
    },
    "has_dealbreaker": has_dealbreaker,
    "dealbreakers": [r["risk"] for r in dealbreakers],
    "opportunities": opportunities,
    "historical_context": historical_context,
    "recommendation": recommendation,
    "total_score": total_score,
    "rationale": rationale,
    "next_steps": next_steps
}
write_json(f"{folder}/screen/risk-assessment.json", risk_assessment)
```

### Step 7: Write Consolidated BID_SCREEN.json

```python
bid_screen = {
    "generated_at": datetime.now().isoformat(),
    "screening_mode": "quick" if quick_mode else "full",
    "folder": folder,

    "recommendation": recommendation,
    "total_score": total_score,
    "rationale": rationale,

    "rfp_summary": rfp_summary,
    "go_nogo_score": go_nogo,
    "client_intel": client_intel if client_intel else {"status": "skipped"},
    "compliance": compliance,
    "past_projects": past_matches,
    "risk_assessment": risk_assessment,
    "preliminary_themes": preliminary_themes or {"status": "not_generated"},

    "next_steps": next_steps
}
write_json(f"{folder}/screen/BID_SCREEN.json", bid_screen)
```

### Step 8: Report

```
RISK ASSESSMENT & RECOMMENDATION (Phase 5)
============================================
Recommendation: {recommendation}
Score: {total_score}/100
Rationale: {rationale}

Risks: {len(risks)} ({high} high, {medium} medium)
Dealbreakers: {len(dealbreakers)}
Opportunities: {len(opportunities)}
Historical: {"Available" if historical_context["has_data"] else "No data"}

Next Steps:
{for step in next_steps: f"  - {step}"}

Outputs:
  screen/risk-assessment.json
  screen/BID_SCREEN.json
```

---

## Quality Checklist

- [ ] `risk-assessment.json` written (>1KB)
- [ ] `BID_SCREEN.json` written (>3KB) — consolidated from all phases
- [ ] Risks consolidated from all prior phases
- [ ] Dealbreaker assessment performed
- [ ] Historical bid patterns checked (if bid-outcomes.json exists with 3+ entries)
- [ ] Opportunities identified
- [ ] Next steps tailored to recommendation (GO/CONDITIONAL/NO-GO)
