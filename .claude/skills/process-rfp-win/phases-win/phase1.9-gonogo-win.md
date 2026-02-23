---
name: phase1.9-gonogo-win
expert-role: Bid Decision Analyst
domain-expertise: Bid/no-bid analysis, opportunity qualification, capture management, Shipley BD lifecycle
---

# Phase 1.9: Go/No-Go Decision Gate

## Expert Role

You are a **Bid Decision Analyst** with expertise in:
- Bid/no-bid decision frameworks and opportunity qualification
- Capture management and Shipley business development lifecycle
- Win probability assessment and competitive positioning
- Resource planning and strategic portfolio management

## Purpose

Score the RFP opportunity across 5 dimensions (0-20 each, total 0-100) and recommend GO, CONDITIONAL, or NO-GO. This gate runs at the end of Stage 1 (Document Intake) after all RFP documents have been parsed, domain detected, evaluation criteria extracted, compliance checked, and submission structure identified. The recommendation helps the team decide whether to invest resources in completing the full bid pipeline.

## Inputs

- `{folder}/shared/domain-context.json` - Industry, compliance frameworks
- `{folder}/shared/EVALUATION_CRITERIA.json` - Scoring methodology, factor weights
- `{folder}/shared/COMPLIANCE_MATRIX.json` - Mandatory items, compliance requirements
- `{folder}/shared/SUBMISSION_STRUCTURE.json` - Volume structure, submission requirements
- `{folder}/flattened/*.md` - Full RFP text for analysis
- Company profile: read from `/home/ddubiel/repos/safs/.claude/skills/process-rfp-win/config-win/company-profile.json`

## Required Output

- `{folder}/shared/GO_NOGO_DECISION.json` (>1KB)

## Instructions

### Step 1: Load All Stage 1 Artifacts and Company Profile

```python
import json
import glob
from datetime import datetime

# Load Stage 1 outputs
domain = read_json(f"{folder}/shared/domain-context.json")
eval_criteria = read_json(f"{folder}/shared/EVALUATION_CRITERIA.json")
compliance = read_json(f"{folder}/shared/COMPLIANCE_MATRIX.json")
submission = read_json(f"{folder}/shared/SUBMISSION_STRUCTURE.json")

# Load full RFP text
flattened_files = sorted(glob.glob(f"{folder}/flattened/*.md"))
combined_text = ""
for fp in flattened_files:
    combined_text += read_file(fp) + "\n\n"

# Load company profile
company = read_json("/home/ddubiel/repos/safs/.claude/skills/process-rfp-win/config-win/company-profile.json")
```

### Step 2: Score Dimension 1 — Capability Match (0-20)

Assess how well RDI's services align with RFP requirements.

```python
capability_score = 0
capability_evidence = []

# Extract RFP scope/requirements from combined text
# Flatten company-profile.json services dict (categories → service names)
# services is a dict: {"data_and_ai": [...], "software_services": [...], ...}
services_dict = company.get("services", {})
rdi_services = [svc.lower() for cat in services_dict.values() for svc in cat]
rdi_industries = [i.lower() for i in company.get("industries", [])]
rfp_domain = domain.get("industry", "").lower()

# Service alignment: check each RFP requirement against RDI services
# Count matching vs total requirements from compliance matrix
mandatory_items = compliance.get("mandatory_items", [])
matching_services = 0
total_requirements = len(mandatory_items)

for item in mandatory_items:
    item_text = item.get("description", "").lower()
    for service in rdi_services:
        if service in item_text or any(keyword in item_text for keyword in service.split()):
            matching_services += 1
            capability_evidence.append(f"Service match: '{service}' aligns with '{item.get('description', '')[:80]}'")
            break

# Industry alignment
industry_match = any(ind in rfp_domain for ind in rdi_industries)
if industry_match:
    capability_evidence.append(f"Industry match: RDI serves '{rfp_domain}' domain")

# Scoring rubric
# 20 = perfect fit (>80% service match + industry match)
# 15 = strong fit (60-80% match + industry match)
# 10 = partial fit (40-60% match OR industry match without strong service alignment)
# 5 = stretch (20-40% match)
# 0 = no fit (<20% match, no industry alignment)
match_ratio = matching_services / max(total_requirements, 1)
if match_ratio > 0.8 and industry_match:
    capability_score = 20
elif match_ratio > 0.6 and industry_match:
    capability_score = 15
elif match_ratio > 0.4 or industry_match:
    capability_score = 10
elif match_ratio > 0.2:
    capability_score = 5
else:
    capability_score = 0

capability_rationale = f"{int(match_ratio*100)}% service alignment ({matching_services}/{total_requirements} requirements). Industry match: {'Yes' if industry_match else 'No'}."
```

### Step 3: Score Dimension 2 — Competitive Position (0-20)

Assess evaluation criteria alignment, compliance burden, and advantage signals.

```python
competitive_score = 0
competitive_evidence = []

# Evaluation criteria alignment
# Check if RDI can score well on each evaluation factor
eval_factors = eval_criteria.get("factors", eval_criteria.get("criteria", []))
scoreable_factors = 0
total_factors = len(eval_factors)

for factor in eval_factors:
    factor_name = factor.get("name", factor.get("factor", "")).lower()
    # Check if factor aligns with RDI strengths
    strength_keywords = ["technical", "experience", "qualifications", "approach", "management", "understanding"]
    if any(kw in factor_name for kw in strength_keywords):
        scoreable_factors += 1
        competitive_evidence.append(f"Can score on: '{factor.get('name', factor.get('factor', ''))}'")

# Compliance burden assessment
mandatory_count = len(mandatory_items)
if mandatory_count < 10:
    compliance_burden = "low"
elif mandatory_count < 25:
    compliance_burden = "moderate"
else:
    compliance_burden = "high"
competitive_evidence.append(f"Compliance burden: {compliance_burden} ({mandatory_count} mandatory items)")

# Advantage signals from RFP text
advantage_keywords = ["innovative", "creative", "modern", "agile", "cloud", "digital transformation"]
disadvantage_keywords = ["incumbent", "existing contractor", "sole source", "set-aside", "8(a)", "hubzone"]

advantages_found = [kw for kw in advantage_keywords if kw in combined_text.lower()]
disadvantages_found = [kw for kw in disadvantage_keywords if kw in combined_text.lower()]

for adv in advantages_found:
    competitive_evidence.append(f"Advantage signal: '{adv}' mentioned in RFP")
for dis in disadvantages_found:
    competitive_evidence.append(f"Risk signal: '{dis}' mentioned in RFP")

# Scoring rubric
factor_ratio = scoreable_factors / max(total_factors, 1)
advantage_net = len(advantages_found) - len(disadvantages_found)

if factor_ratio > 0.7 and advantage_net >= 0:
    competitive_score = 20
elif factor_ratio > 0.5 and advantage_net >= 0:
    competitive_score = 15
elif factor_ratio > 0.3:
    competitive_score = 10
elif disadvantages_found:
    competitive_score = 5
else:
    competitive_score = 8

competitive_rationale = f"Can score on {int(factor_ratio*100)}% of evaluation factors. Compliance burden: {compliance_burden}. Net advantage signals: {advantage_net}."
```

### Step 4: Score Dimension 3 — Resource Availability (0-20)

Assess estimated effort vs RDI capacity, key personnel, and certifications.

```python
resource_score = 0
resource_evidence = []

# Estimate effort from submission structure
volume_count = len(submission.get("volumes", []))
overall_page_limit = submission.get("overall_page_limit")

# Check for key personnel requirements in RFP text
personnel_patterns = [
    r'(?:key\s+personnel|project\s+manager|team\s+lead|senior\s+developer|architect)',
    r'(?:resume|curriculum\s+vitae|cv|qualifications?\s+of\s+staff)',
    r'(?:certif(?:ied|ication)|pmp|cissp|aws\s+certified|clearance)'
]

import re
personnel_requirements = []
certification_requirements = []
for pattern in personnel_patterns:
    matches = re.findall(pattern, combined_text, re.IGNORECASE)
    if matches:
        if 'certif' in pattern or 'pmp' in pattern:
            certification_requirements.extend(matches)
        else:
            personnel_requirements.extend(matches)

resource_evidence.append(f"Submission volumes: {volume_count}")
if overall_page_limit:
    resource_evidence.append(f"Overall page limit: {overall_page_limit}")
if personnel_requirements:
    resource_evidence.append(f"Key personnel references: {len(set(personnel_requirements))}")
if certification_requirements:
    resource_evidence.append(f"Certification requirements: {', '.join(set(certification_requirements))}")

# Check company profile for team/staff info
rdi_certs = company.get("certifications", [])
cert_gap = [c for c in set(certification_requirements) if not any(rc.lower() in c.lower() for rc in rdi_certs)]
if cert_gap:
    resource_evidence.append(f"Certification gaps: {cert_gap}")

# Scoring rubric
# 20 = fully staffed, no cert gaps, manageable scope
# 10 = some gaps but workable
# 0 = major gaps, excessive scope
if not cert_gap and volume_count <= 6:
    resource_score = 20
elif not cert_gap and volume_count <= 10:
    resource_score = 15
elif len(cert_gap) <= 1:
    resource_score = 10
elif len(cert_gap) <= 3:
    resource_score = 5
else:
    resource_score = 0

resource_rationale = f"{volume_count} volumes to produce. Personnel references: {len(set(personnel_requirements))}. Certification gaps: {len(cert_gap)}."
```

### Step 5: Score Dimension 4 — Timeline Feasibility (0-20)

Assess proposal deadline and project timeline.

```python
timeline_score = 0
timeline_evidence = []

# Search for submission deadline in RFP text
deadline_patterns = [
    r'(?:due\s+(?:date|by)|deadline|submit(?:ted)?\s+(?:by|before|no\s+later\s+than))[:\s]+([^\n]{10,60})',
    r'(?:proposals?\s+(?:must\s+be\s+)?(?:received|submitted)\s+(?:by|before|no\s+later))[:\s]+([^\n]{10,60})',
    r'(\w+\s+\d{1,2},?\s+\d{4}(?:\s+(?:at\s+)?\d{1,2}:\d{2}\s*(?:am|pm|[AP]\.?M\.?)?)?)'
]

deadline_text = None
for pattern in deadline_patterns:
    match = re.search(pattern, combined_text, re.IGNORECASE)
    if match:
        deadline_text = match.group(1).strip()
        timeline_evidence.append(f"Submission deadline: {deadline_text}")
        break

if not deadline_text:
    timeline_evidence.append("No explicit deadline detected — verify manually")

# Search for project duration/timeline
duration_patterns = [
    r'(?:contract|project|period\s+of\s+performance|term)[:\s]+(\d+)\s*(?:month|year|week)',
    r'(\d+)\s*(?:month|year|week)\s*(?:contract|period|term|project|duration)',
    r'(?:start|begin(?:ning)?|commence)[:\s]+([^\n]{10,60})'
]

project_duration = None
for pattern in duration_patterns:
    match = re.search(pattern, combined_text, re.IGNORECASE)
    if match:
        project_duration = match.group(0).strip()
        timeline_evidence.append(f"Project duration: {project_duration}")
        break

# Scoring rubric
# 20 = comfortable timeline (>3 weeks to deadline, reasonable project scope)
# 15 = adequate (2-3 weeks to deadline)
# 10 = tight but doable (1-2 weeks to deadline)
# 5 = very tight (<1 week to deadline)
# 0 = impossible (past deadline or <2 days)
# Since we cannot reliably parse all date formats, default to moderate
if deadline_text:
    timeline_score = 15  # Default to "adequate" when deadline exists
    timeline_rationale = f"Deadline detected: {deadline_text}. Verify timeline feasibility manually."
else:
    timeline_score = 10  # Unknown deadline adds risk
    timeline_rationale = "No deadline detected in RFP text. Manual verification required."

timeline_evidence.append("NOTE: Timeline score should be manually verified against actual calendar")
```

### Step 6: Score Dimension 5 — Strategic Alignment (0-20)

Assess industry growth, geographic proximity, client relationship, and revenue potential.

```python
strategic_score = 0
strategic_evidence = []

# Geographic proximity — RDI offices in AK, OR, ID, TX
# locations is a list of dicts: [{"city": "Anchorage", "state": "AK", ...}, ...]
rdi_location_objs = company.get("locations", [])
rdi_locations_lower = []
for loc in rdi_location_objs:
    if isinstance(loc, dict):
        for field in ["city", "state"]:
            val = loc.get(field, "").lower().strip()
            if val:
                rdi_locations_lower.append(val)
    else:
        rdi_locations_lower.append(str(loc).lower())

# Check RFP for location references
location_patterns = [
    r'(?:location|state|city|office|headquarter|based\s+in)[:\s]+([^\n]{5,50})',
    r'(?:alaska|anchorage|fairbanks|juneau|oregon|portland|salem|idaho|boise|texas|austin|houston)',
]

geographic_match = False
for pattern in location_patterns:
    matches = re.findall(pattern, combined_text, re.IGNORECASE)
    for match in matches:
        match_lower = match.lower() if isinstance(match, str) else match
        if any(loc in match_lower for loc in rdi_locations_lower):
            geographic_match = True
            strategic_evidence.append(f"Geographic proximity: '{match}' near RDI office")
            break

# Industry growth area
growth_industries = company.get("growth_areas", company.get("target_industries", []))
growth_match = any(g.lower() in rfp_domain for g in growth_industries) if growth_industries else False
if growth_match:
    strategic_evidence.append(f"Growth industry: '{rfp_domain}' is a target area")

# Client relationship
client_name_patterns = [
    r'(?:department|agency|authority|commission|bureau|office\s+of)[:\s]+([^\n]{5,80})',
    r'(?:issued\s+by|prepared\s+for|on\s+behalf\s+of)[:\s]+([^\n]{5,80})'
]
client_name = None
for pattern in client_name_patterns:
    match = re.search(pattern, combined_text, re.IGNORECASE)
    if match:
        client_name = match.group(1).strip()
        strategic_evidence.append(f"Client: {client_name}")
        break

# Check past performance for repeat client
past_clients = company.get("past_performance", {}).get("clients", [])
repeat_client = False
if client_name and past_clients:
    for pc in past_clients:
        if isinstance(pc, str) and pc.lower() in client_name.lower():
            repeat_client = True
            strategic_evidence.append(f"Repeat client: '{pc}' found in past performance")
            break

# Revenue signals
revenue_patterns = [
    r'(?:budget|estimated\s+(?:cost|value)|not\s+to\s+exceed|ceiling)[:\s]*\$?([\d,]+(?:\.\d{2})?)',
    r'\$([\d,]+(?:\.\d{2})?)\s*(?:million|M|thousand|K)?'
]
estimated_value = None
for pattern in revenue_patterns:
    match = re.search(pattern, combined_text, re.IGNORECASE)
    if match:
        estimated_value = match.group(0).strip()
        strategic_evidence.append(f"Estimated value: {estimated_value}")
        break

# Scoring rubric
# 20 = high strategic value (growth area + geographic + repeat client)
# 15 = strong (2 of 3 strategic factors)
# 10 = moderate (1 strategic factor)
# 5 = low (no clear strategic alignment but no negatives)
# 0 = no strategic value
strategic_factors = sum([geographic_match, growth_match, repeat_client])

if strategic_factors >= 3:
    strategic_score = 20
elif strategic_factors == 2:
    strategic_score = 15
elif strategic_factors == 1:
    strategic_score = 10
else:
    strategic_score = 5  # Neutral — new opportunities still have baseline value

strategic_rationale = f"Strategic factors met: {strategic_factors}/3 (geographic: {'Yes' if geographic_match else 'No'}, growth area: {'Yes' if growth_match else 'No'}, repeat client: {'Yes' if repeat_client else 'No'})."
```

### Step 7: Calculate Total Score and Recommendation

```python
total_score = capability_score + competitive_score + resource_score + timeline_score + strategic_score

# Determine recommendation
if total_score >= 50:
    recommendation = "GO"
    user_decision_required = False
elif total_score >= 40:
    recommendation = "CONDITIONAL"
    user_decision_required = True
else:
    recommendation = "NO_GO"
    user_decision_required = True  # User can always override

# Compile risk and opportunity factors
risk_factors = []
opportunity_factors = []

if capability_score < 10:
    risk_factors.append("Low capability alignment with RFP requirements")
if competitive_score < 10:
    risk_factors.append("Weak competitive positioning")
if resource_score < 10:
    risk_factors.append("Resource gaps or certification shortfalls")
if timeline_score < 10:
    risk_factors.append("Timeline concerns — tight or unknown deadline")
if strategic_score < 10:
    risk_factors.append("Low strategic value for RDI portfolio")

if capability_score >= 15:
    opportunity_factors.append("Strong service alignment with RFP scope")
if competitive_score >= 15:
    opportunity_factors.append("Favorable competitive signals")
if geographic_match:
    opportunity_factors.append("Geographic proximity to project location")
if repeat_client:
    opportunity_factors.append("Existing relationship with client")
if growth_match:
    opportunity_factors.append("Aligns with RDI growth strategy")

for dis in disadvantages_found:
    risk_factors.append(f"Competitive risk: '{dis}' detected in RFP")
for adv in advantages_found:
    opportunity_factors.append(f"Innovation signal: '{adv}' mentioned in RFP")
```

### Step 8: Write GO_NOGO_DECISION.json

```python
decision = {
    "phase": "1.9",
    "phase_name": "Go/No-Go Decision Gate",
    "timestamp": datetime.now().isoformat(),
    "recommendation": recommendation,
    "total_score": total_score,
    "threshold": {
        "go": 50,
        "conditional": 40,
        "no_go": 0
    },
    "dimensions": {
        "capability_match": {
            "score": capability_score,
            "max": 20,
            "rationale": capability_rationale,
            "evidence": capability_evidence
        },
        "competitive_position": {
            "score": competitive_score,
            "max": 20,
            "rationale": competitive_rationale,
            "evidence": competitive_evidence
        },
        "resource_availability": {
            "score": resource_score,
            "max": 20,
            "rationale": resource_rationale,
            "evidence": resource_evidence
        },
        "timeline_feasibility": {
            "score": timeline_score,
            "max": 20,
            "rationale": timeline_rationale,
            "evidence": timeline_evidence
        },
        "strategic_alignment": {
            "score": strategic_score,
            "max": 20,
            "rationale": strategic_rationale,
            "evidence": strategic_evidence
        }
    },
    "risk_factors": risk_factors,
    "opportunity_factors": opportunity_factors,
    "override_allowed": True,
    "user_decision_required": user_decision_required
}

write_json(f"{folder}/shared/GO_NOGO_DECISION.json", decision)
```

### Step 9: Report Results

```
GO/NO-GO DECISION GATE (Phase 1.9)
=============================================
Recommendation: {recommendation}
Total Score: {total_score}/100

Dimension Scores:
  Capability Match:     {capability_score}/20  — {capability_rationale}
  Competitive Position: {competitive_score}/20  — {competitive_rationale}
  Resource Availability:{resource_score}/20  — {resource_rationale}
  Timeline Feasibility: {timeline_score}/20  — {timeline_rationale}
  Strategic Alignment:  {strategic_score}/20  — {strategic_rationale}

Thresholds: GO >= 50 | CONDITIONAL 40-49 | NO-GO < 40

Risk Factors:
{for each risk: "  - " + risk}

Opportunity Factors:
{for each opportunity: "  - " + opportunity}

{if recommendation == "CONDITIONAL":
  "ACTION REQUIRED: Score falls in CONDITIONAL range. Please review the analysis above and decide whether to proceed (GO) or stop (NO-GO). Reply with your decision."
}
{if recommendation == "NO_GO":
  "RECOMMENDATION: Do not bid. Override is available — reply GO to proceed anyway."
}

Output: shared/GO_NOGO_DECISION.json
```

## Quality Checklist

- [ ] `GO_NOGO_DECISION.json` written (>1KB)
- [ ] All 5 dimensions scored with rationale and evidence
- [ ] Company profile loaded and services compared against RFP requirements
- [ ] Risk factors and opportunity factors identified
- [ ] Recommendation follows threshold rules (GO >= 50, CONDITIONAL 40-49, NO-GO < 40)
- [ ] User decision solicited for CONDITIONAL and NO-GO recommendations
- [ ] Timeline score includes manual verification note
