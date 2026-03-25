---
name: phase5-recommendation
expert-role: Risk Analyst & Bid Decision Advisor
domain-expertise: Risk assessment, opportunity analysis, bid strategy, portfolio management
skill: risk-analyst
---

# Phase 5: Risk Assessment & Recommendation

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

### Skill Integration: Risk-Analyst Framework Application (MANDATORY)

The **risk-analyst** skill is loaded in context. Apply these frameworks throughout this phase:

**Risk Taxonomy Classification:** Classify every risk into one of 6 categories:
- Technical — scope ambiguity, integration complexity, technology maturity
- Schedule — timeline compression, dependency chains, resource contention
- Cost — budget uncertainty, rate cap exposure, cost growth drivers
- Management — staffing gaps, governance structure, communication overhead
- External/Regulatory — compliance mandates, policy changes, political risk
- Past Performance — relevance gaps, reference risks, performance history

**5x5 Probability/Impact Matrix:** Score each risk with calibrated scales:
- Likelihood (1-5): 1=Rare, 2=Unlikely, 3=Possible, 4=Likely, 5=Almost Certain
- Consequence (1-5): 1=Negligible, 2=Minor, 3=Moderate, 4=Major, 5=Severe
- Severity = Likelihood x Consequence (1-25), banded: Low(1-4), Moderate(5-9), High(10-15), Critical(16-25)

**If/Then Risk Statement Format:** Rewrite every risk as:
"If [specific condition], then [measurable consequence]" — not vague descriptions.

**Response Strategy Assignment:** For each High/Critical risk, assign one of:
- Avoid — eliminate the threat entirely (change approach)
- Transfer — shift impact to a third party (insurance, subcontractor)
- Mitigate — reduce likelihood or consequence (specific actions)
- Accept — acknowledge and monitor (with trigger conditions)

**Residual Risk:** After mitigation, what risk remains? State explicitly.

**Anti-Pattern Guards (check output against these):**
- Vague risk statements without measurable consequences
- Mitigation theater — mitigations that sound good but don't reduce risk
- Probability anchoring — all risks scored "medium" without differentiation
- Copy-paste risks — generic statements not tied to THIS RFP's specifics

---

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

# From Phase 4 Tech Gap Analysis (Batch 3)
tech_gap = past_matches.get("tech_gap_analysis", {})
gap_severity = tech_gap.get("gap_severity", "none")
techs_without_coverage = tech_gap.get("technologies_without_coverage", [])
if gap_severity in ("medium", "high") and techs_without_coverage:
    risks.append({
        "risk": f"Technology gap: {len(techs_without_coverage)} RFP-required technolog{'y has' if len(techs_without_coverage) == 1 else 'ies have'} no past project coverage ({', '.join(techs_without_coverage[:3])})",
        "source": "tech_gap_analysis",
        "severity": "high" if gap_severity == "high" else "medium",
        "category": "experience"
    })
elif gap_severity == "low" and techs_without_coverage:
    risks.append({
        "risk": f"Minor technology gap: {', '.join(techs_without_coverage[:2])} lack past project coverage",
        "source": "tech_gap_analysis",
        "severity": "low",
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

### Step 2b: Skill-Informed Risk Enrichment (MANDATORY)

Apply the risk-analyst framework to enrich each consolidated risk. This transforms flat risk lists into structured, actionable risk assessments.

```python
# Enrich each risk with skill-mandated structured fields
for risk_item in risks:
    risk_text = risk_item.get("risk", "")

    # 1. Classify into 6-category taxonomy (from skill)
    #    Map existing "category" to the skill's taxonomy
    category_map = {
        "scoring": "Management",
        "compliance": "External/Regulatory",
        "experience": "Past Performance",
        "timeline": "Schedule",
    }
    risk_item["risk_category"] = category_map.get(
        risk_item.get("category", ""), "Technical"
    )

    # 2. Score with calibrated 5x5 matrix
    #    Use the risk context to assign probability and impact
    #    Defaults based on original severity; LLM should refine
    severity_defaults = {
        "high": {"likelihood": 4, "impact": 4},
        "medium": {"likelihood": 3, "impact": 3},
        "low": {"likelihood": 2, "impact": 2},
    }
    defaults = severity_defaults.get(risk_item.get("severity", "medium"), {"likelihood": 3, "impact": 3})
    risk_item["likelihood"] = defaults["likelihood"]
    risk_item["impact"] = defaults["impact"]
    risk_item["severity_score"] = risk_item["likelihood"] * risk_item["impact"]

    # Band severity score
    ss = risk_item["severity_score"]
    if ss >= 16:
        risk_item["severity_band"] = "Critical"
    elif ss >= 10:
        risk_item["severity_band"] = "High"
    elif ss >= 5:
        risk_item["severity_band"] = "Moderate"
    else:
        risk_item["severity_band"] = "Low"

    # 3. Rewrite risk in If/Then format (LLM should produce these directly,
    #    but provide the structure for algorithmic risks)
    if not risk_text.lower().startswith("if "):
        risk_item["description_if_then"] = f"If {risk_text.lower()}, then proposal competitiveness or delivery capability may be compromised"
    else:
        risk_item["description_if_then"] = risk_text

    # 4. Assign response strategy for High/Critical
    if risk_item["severity_band"] in ("High", "Critical"):
        risk_item["response_strategy"] = "Mitigate"  # Default; LLM should refine
        risk_item["mitigation_plan"] = ""  # LLM fills with specific actions
        risk_item["residual_risk"] = ""  # What remains after mitigation
    else:
        risk_item["response_strategy"] = "Accept"
        risk_item["mitigation_plan"] = "Monitor during proposal development"
        risk_item["residual_risk"] = "Low — accepted risk within tolerance"

# Compute severity distribution
severity_distribution = {
    "critical": sum(1 for r in risks if r.get("severity_band") == "Critical"),
    "high": sum(1 for r in risks if r.get("severity_band") == "High"),
    "moderate": sum(1 for r in risks if r.get("severity_band") == "Moderate"),
    "low": sum(1 for r in risks if r.get("severity_band") == "Low"),
}

# Check for correlated risks (risks in same category that compound)
risk_correlations = []
from collections import Counter
category_counts = Counter(r.get("risk_category", "") for r in risks)
for cat, count in category_counts.items():
    if count >= 2:
        risk_correlations.append(
            f"{count} risks in {cat} category may compound each other"
        )
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

### Step 4: Assess Opportunities (Risk-Opportunity Duality)

Structure opportunities as the inverse of managed risks — each mitigated risk reveals a competitive advantage. Per risk-analyst skill, opportunities emerge when the organization's risk management capability exceeds what competitors can demonstrate.

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

# Contract vehicle advantage (from Phase 4 enrichment)
matching_vehicles = compliance.get("contract_vehicles", {}).get("matching_rfp", [])
if matching_vehicles:
    opportunities.append({
        "opportunity": f"Existing contract vehicle(s) in RFP state: {'; '.join(matching_vehicles[:2])}",
        "source": "contract_vehicles"
    })

# Existing client relationship (from Phase 4 enrichment)
existing_rel = compliance.get("existing_relationship", {})
if existing_rel.get("found"):
    opportunities.append({
        "opportunity": f"Existing client relationship: {existing_rel.get('matched_client', 'identified')} — incumbent knowledge advantage",
        "source": "existing_relationship"
    })

# Partnership differentiation (from Phase 4 enrichment)
partnerships = compliance.get("partnerships", [])
if partnerships:
    opportunities.append({
        "opportunity": f"Technology partnerships: {'; '.join(partnerships[:3])}",
        "source": "partnerships"
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

### Step 5: Generate Final Recommendation (Strategic Narrative)

Per the risk-analyst skill, the recommendation rationale must be a strategic narrative, not a mechanical score report. Frame the recommendation through a risk appetite lens: what is the risk-adjusted value of pursuing this opportunity?

```python
total_score = go_nogo.get("overall_score", 0)
recommendation = go_nogo.get("recommendation", "NO_GO")

# Generate STRATEGIC NARRATIVE rationale (MANDATORY: must name specific differentiators)
# A BD director reading only this rationale should understand WHY, not just the score.
# Structure: (1) Risk severity distribution, (2) Risk correlations, (3) Risk-adjusted assessment

# Gather evidence for rationale synthesis
strongest_area = max(assessment_areas, key=lambda a: a.get("score", 0)) if assessment_areas else {}
weakest_area = min(assessment_areas, key=lambda a: a.get("score", 0)) if assessment_areas else {}
buyer_coverage = go_nogo.get("buyer_priority_coverage", {})
high_addressed = buyer_coverage.get("high_addressed", 0)
high_total = buyer_coverage.get("high_total", 0)
high_gaps = buyer_coverage.get("high_gaps", [])
top_project = past_matches.get("matched_projects", [{}])[0] if past_matches.get("matched_projects") else {}
match_quality_str = past_matches.get("match_quality", "unknown")
matching_vehicles = compliance.get("contract_vehicles", {}).get("matching_rfp", [])
partnerships = compliance.get("partnerships", [])
existing_rel = compliance.get("existing_relationship", {})

# Load intelligence layers for recommendation enrichment (Batch 3)
client_tone = go_nogo.get("client_tone", {})
primary_style = client_tone.get("primary_style", "formal_bureaucratic")

# Evaluation point coverage from Phase 4.5 themes
eval_point_coverage = (preliminary_themes or {}).get("evaluation_point_coverage", {})
total_theme_points = eval_point_coverage.get("total_theme_points", 0)
total_available_points = eval_point_coverage.get("total_available_points", "unknown")

# Tone-adapted rationale framing prefix
tone_framing = {
    "formal_bureaucratic": "",  # Default — no special framing
    "technical_precise": "Technical assessment: ",
    "collaborative_partnership": "Partnership opportunity assessment: ",
    "results_oriented": "Results-focused assessment: ",
    "innovation_forward": "Innovation opportunity assessment: ",
}.get(primary_style, "")

if recommendation == "GO":
    # Build a rationale that names 3+ specific differentiators
    rationale = f"{tone_framing}Score {total_score}/100 meets GO threshold. "

    # Evaluation point coverage (Batch 3)
    if total_theme_points and total_available_points != "unknown":
        rationale += f"Themes address ~{total_theme_points}/{total_available_points} evaluation points. "

    # Top differentiator from strongest assessment area
    if strongest_area:
        rationale += f"{strongest_area.get('name', '')} scored {strongest_area.get('score', 0)}/100"
        # Extract first evidence item for specificity
        evidence = strongest_area.get("evidence", [])
        if evidence:
            rationale += f" -- {evidence[0]}. "
        else:
            rationale += ". "

    # Buyer priority coverage
    if high_total > 0:
        rationale += f"All {high_addressed}/{high_total} HIGH buyer priorities addressed. " if high_addressed == high_total else f"{high_addressed}/{high_total} HIGH buyer priorities addressed"
        if high_gaps:
            rationale += f" (gaps: {', '.join(high_gaps)}). "
        else:
            rationale += " "

    # Past performance strength
    if top_project and top_project.get("relevance_score", 0) > 15:
        rationale += f"Past performance match quality: {match_quality_str} (top match: {top_project.get('title', 'N/A')}, score {top_project.get('relevance_score', 0)}). "

    # Contract vehicles / partnerships
    if matching_vehicles:
        rationale += f"Existing contract vehicles in RFP state: {'; '.join(matching_vehicles[:2])}. "
    if partnerships:
        rationale += f"{partnerships[0]}. "

    # Risk distribution summary (skill-enriched)
    rationale += f"Risk profile: {severity_distribution.get('critical', 0)} Critical, "
    rationale += f"{severity_distribution.get('high', 0)} High, "
    rationale += f"{severity_distribution.get('moderate', 0)} Moderate, "
    rationale += f"{severity_distribution.get('low', 0)} Low. "
    if risk_correlations:
        rationale += f"Correlated risks: {'; '.join(risk_correlations)}. "
    if severity_distribution.get("critical", 0) == 0 and severity_distribution.get("high", 0) == 0:
        rationale += "No critical or high-severity risks -- favorable risk posture. "
    elif severity_distribution.get("critical", 0) > 0:
        rationale += "Critical risks require immediate mitigation before bid commitment. "
    else:
        rationale += f"{severity_distribution.get('high', 0)} high-severity risk(s) require attention. "

    # Existing relationship note
    if existing_rel.get("found"):
        rationale += f" Existing client relationship: {existing_rel.get('matched_client', 'identified')}."

    next_steps = [
        f"Run full pipeline: /process-rfp-win {folder}",
        "Estimated full pipeline time: 3-4 hours",
        "Focus areas: " + ", ".join(go_nogo.get("overall_mitigations", [])[:3])
    ]
    # Append top preliminary theme names to next steps
    if preliminary_themes and preliminary_themes.get("themes"):
        top_theme_names = [t["name"] for t in preliminary_themes["themes"][:3]]
        next_steps.append("Preliminary win themes: " + "; ".join(top_theme_names))

    # Append uncovered buyer priorities as focus areas for the full pipeline
    theme_coverage = (preliminary_themes or {}).get("buyer_priority_coverage", {})
    gonogo_coverage = go_nogo.get("buyer_priority_coverage", {})
    uncovered = theme_coverage.get("uncovered", []) or gonogo_coverage.get("high_gaps", [])
    if uncovered:
        next_steps.append("Focus areas for full pipeline (uncovered HIGH buyer priorities): " + "; ".join(uncovered))

elif recommendation == "CONDITIONAL":
    rationale = f"Score {total_score}/100 in CONDITIONAL range. "
    if weakest_area:
        rationale += f"Weakest area: {weakest_area.get('name', '')} ({weakest_area.get('score', 0)}/100). "
    if dealbreakers:
        rationale += f"{len(dealbreakers)} potential dealbreaker(s) require resolution: {'; '.join(d[:60] for d in dealbreakers[:2])}. "
    if high_gaps:
        rationale += f"Unaddressed HIGH buyer priorities: {', '.join(high_gaps)}. "
    rationale += "Review risks before committing resources."
    next_steps = [
        "Resolve conditions: " + "; ".join(r["risk"][:60] for r in dealbreakers[:3]),
        f"If conditions resolved, run: /process-rfp-win {folder}",
        "Consider team capacity and competing bids"
    ]
else:  # NO_GO
    rationale = f"Score {total_score}/100 below threshold. "
    if weakest_area:
        rationale += f"Critical weakness: {weakest_area.get('name', '')} ({weakest_area.get('score', 0)}/100). "
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
    "risks": risks,  # Each risk now includes: risk_category, likelihood, impact, severity_score, severity_band, description_if_then, response_strategy, mitigation_plan, residual_risk
    "risk_summary": {
        "total": len(risks),
        "high": sum(1 for r in risks if r["severity"] == "high"),
        "medium": sum(1 for r in risks if r["severity"] == "medium"),
        "low": sum(1 for r in risks if r["severity"] == "low")
    },
    "severity_distribution": severity_distribution,  # {critical, high, moderate, low} from 5x5 matrix
    "risk_correlations": risk_correlations,  # Compounding risks in same category
    "aggregate_exposure": f"{severity_distribution.get('critical', 0)} critical, {severity_distribution.get('high', 0)} high risks across {len(set(r.get('risk_category', '') for r in risks))} categories",
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
- [ ] Uncovered HIGH buyer priorities included in next_steps (if GO recommendation)
- [ ] buyer_priority_coverage from both go-nogo-score.json and preliminary-themes.json consulted

### Intelligence Layer Integration Quality Checks (Batch 3)
- [ ] Tech gap risks from Phase 4 `tech_gap_analysis` added to risk consolidation
- [ ] Gap severity (none/low/medium/high) correctly mapped to risk severity levels
- [ ] Evaluation point coverage included in GO rationale ("Themes address ~X/Y points")
- [ ] Tone-adapted rationale framing applied based on `client_tone.primary_style`
- [ ] `client_tone` and `evaluation_point_coverage` loaded from upstream phase outputs

### Skill Integration Quality Checks (risk-analyst)
- [ ] Every risk classified into 6-category taxonomy (Technical/Schedule/Cost/Management/External-Regulatory/Past Performance)
- [ ] Every risk scored with 5x5 probability/impact matrix (likelihood 1-5, impact 1-5)
- [ ] Severity banded from severity_score: Low(1-4), Moderate(5-9), High(10-15), Critical(16-25)
- [ ] Every risk has description_if_then in "If [condition], then [consequence]" format
- [ ] High/Critical risks have response_strategy (Avoid/Transfer/Mitigate/Accept) and residual_risk
- [ ] severity_distribution included in output (critical/high/moderate/low counts)
- [ ] risk_correlations identified (compounding risks in same category)
- [ ] Recommendation rationale is strategic narrative, not mechanical score report
- [ ] **Anti-pattern check:** No vague risk statements, no mitigation theater, no probability anchoring, no copy-paste risks
