---
name: phase2-gonogo
expert-role: Bid Decision Analyst
domain-expertise: Bid/no-bid analysis, opportunity qualification, capture management, Shipley BD lifecycle
---

# Phase 2: Go/No-Go Scoring

**Expert Role:** Bid Decision Analyst

**Purpose:** Score the RFP opportunity across 5 dimensions (0-20 each, total 0-100) and recommend GO, CONDITIONAL, or NO-GO. Uses the combined RFP text and Phase 1 summary as inputs. Identical scoring rubric to the full pipeline's Phase 1.9.

**Inputs:**
- Combined RFP text (in memory from Phase 0)
- `{folder}/screen/rfp-summary.json` — Phase 1 metadata extraction
- Company profile: `config-win/company-profile.json`

**Required Output:**
- `{folder}/screen/go-nogo-score.json` (>1KB)

**Instructions:**

## Step 1: Load Inputs

```python
rfp_summary = read_json(f"{folder}/screen/rfp-summary.json")
company = read_json(COMPANY_PROFILE)
# combined_text available from Phase 0 (in memory)
```

## Step 2: Score Dimension 1 — Capability Match (0-20)

Assess how well RDI's services align with RFP scope.

```python
# Flatten company services (services is a DICT: {"data_and_ai": [...], ...})
services_dict = company.get("services", {})
rdi_services = [svc.lower() for cat in services_dict.values() for svc in cat]
rdi_industries = [i.lower() for i in company.get("industries", [])]

# Get scope keywords from Phase 1 summary
scope_keywords = [kw.lower() for kw in rfp_summary.get("scope_keywords", [])]
rfp_domain = (rfp_summary.get("industry_domain") or "").lower()

# Count service matches against scope keywords
matching_services = 0
total_keywords = max(len(scope_keywords), 1)
capability_evidence = []

for keyword in scope_keywords:
    for service in rdi_services:
        if keyword in service.lower() or service.lower() in keyword:
            matching_services += 1
            capability_evidence.append(f"Service match: '{service}' aligns with scope keyword '{keyword}'")
            break

# Industry alignment
industry_match = any(ind in rfp_domain for ind in rdi_industries) if rfp_domain else False
if industry_match:
    capability_evidence.append(f"Industry match: RDI serves '{rfp_domain}' domain")

# Also check mandatory requirements from RFP text
mandatory_reqs = rfp_summary.get("mandatory_requirements", [])
for req in mandatory_reqs:
    req_lower = req.lower()
    for service in rdi_services:
        if service.lower() in req_lower:
            matching_services += 1
            capability_evidence.append(f"Requirement match: '{service}' addresses '{req[:60]}'")
            break

match_ratio = matching_services / max(total_keywords + len(mandatory_reqs), 1)

# Scoring rubric
if match_ratio > 0.8 and industry_match:
    capability_score = 20
elif match_ratio > 0.6 and industry_match:
    capability_score = 15
elif match_ratio > 0.6 or industry_match:
    capability_score = 12
elif match_ratio > 0.4 or industry_match:
    capability_score = 10
elif match_ratio > 0.2:
    capability_score = 5
else:
    capability_score = 0

capability_rationale = f"{int(match_ratio*100)}% alignment ({matching_services} matches). Industry: {'Yes' if industry_match else 'No'}."
```

## Step 3: Score Dimension 2 — Competitive Position (0-20)

Assess evaluation criteria alignment, advantage/disadvantage signals, competition level, preference points, and COTS positioning.

```python
# Check evaluation criteria from Phase 1
eval_criteria = rfp_summary.get("evaluation_criteria", [])
eval_method = rfp_summary.get("evaluation_method", "")

competitive_evidence = []

# Advantage signals in RFP text
advantage_keywords = ["innovative", "creative", "modern", "agile", "cloud", "digital transformation", "best value"]
disadvantage_keywords = ["incumbent", "existing contractor", "sole source", "set-aside", "8(a)", "hubzone"]

advantages_found = [kw for kw in advantage_keywords if kw in combined_text.lower()]
disadvantages_found = [kw for kw in disadvantage_keywords if kw in combined_text.lower()]

for adv in advantages_found:
    competitive_evidence.append(f"Advantage signal: '{adv}'")
for dis in disadvantages_found:
    competitive_evidence.append(f"Risk signal: '{dis}'")

# Set-aside check
set_aside = rfp_summary.get("set_aside")
if set_aside:
    competitive_evidence.append(f"Set-aside: {set_aside}")
    disadvantages_found.append(f"set-aside: {set_aside}")

# --- COMPETITION HEADWIND DETECTION ---
competition_headwinds = 0

# 1. Prior competition level — parse proposal counts from prior_rfp_history
prior_history = rfp_summary.get("prior_rfp_history", "")
if prior_history:
    proposal_count_match = re.search(r'(\d+)\s+proposal', str(prior_history), re.IGNORECASE)
    if proposal_count_match:
        prior_proposals = int(proposal_count_match.group(1))
        if prior_proposals >= 5:
            competition_headwinds += 1
            competitive_evidence.append(f"High competition: {prior_proposals} proposals in prior attempt")

# 2. Preference point eligibility — veteran-owned, small business preferences
pref_keywords = ["preference point", "veteran-owned", "small business preference",
                  "veteran owned", "small business set-aside"]
pref_found = False
for pk in pref_keywords:
    if pk in combined_text.lower():
        pref_found = True
        break
if pref_found:
    is_veteran_owned = company.get("bid_defaults", {}).get("veteran_owned", False)
    if not is_veteran_owned:
        competition_headwinds += 1
        competitive_evidence.append("Structural disadvantage: preference points available but RDI not eligible")

# 3. COTS/platform advantage detection — custom builders may be disadvantaged
cots_keywords = ["commercial off-the-shelf", "cots", "configurable platform", "low-code",
                  "no-code", "saas solution", "out of the box", "out-of-the-box"]
cots_found = any(kw in combined_text.lower() for kw in cots_keywords)
if cots_found:
    competition_headwinds += 1
    competitive_evidence.append("COTS/platform solutions accepted — custom builders may be disadvantaged")

# --- SCORING ---
advantage_net = len(advantages_found) - len(disadvantages_found) - competition_headwinds

if len(eval_criteria) > 0 and advantage_net >= 2:
    competitive_score = 15
elif advantage_net >= 0:
    competitive_score = 12
elif advantage_net >= -2:
    competitive_score = 10
elif advantage_net >= -4:
    competitive_score = 8
else:
    competitive_score = 5

competitive_rationale = (
    f"Eval criteria: {len(eval_criteria)} found. "
    f"Advantage signals: {len(advantages_found)}. Risk signals: {len(disadvantages_found)}. "
    f"Competition headwinds: {competition_headwinds}. Adjusted net: {advantage_net}."
)
```

## Step 4: Score Dimension 3 — Resource Availability (0-20)

Assess staffing requirements, certification gaps, scope size, and company capacity.

```python
resource_evidence = []

# Check for key personnel/cert requirements in RFP text
import re
personnel_patterns = [
    r'(?:key\s+personnel|project\s+manager|team\s+lead|senior\s+developer|architect)',
    r'(?:resume|curriculum\s+vitae|cv|qualifications?\s+of\s+staff)',
    r'(?:certif(?:ied|ication)|pmp|cissp|aws\s+certified|clearance|fedramp)'
]

personnel_refs = []
cert_refs = []
for pattern in personnel_patterns:
    matches = re.findall(pattern, combined_text, re.IGNORECASE)
    if matches:
        if 'certif' in pattern or 'pmp' in pattern or 'clearance' in pattern:
            cert_refs.extend(matches)
        else:
            personnel_refs.extend(matches)

resource_evidence.append(f"Personnel references: {len(set(personnel_refs))}")
resource_evidence.append(f"Certification references: {len(set(cert_refs))}")

# --- COMPANY SIZE BASELINE ---
# Larger companies have deeper bench strength; start from capacity baseline
employees = str(company.get("employees", ""))
if "200" in employees or (employees.isdigit() and int(employees) >= 200):
    resource_baseline = 15
    resource_evidence.append("Company size: 200+ employees — strong resource capacity")
elif "100" in employees or (employees.isdigit() and int(employees) >= 100):
    resource_baseline = 13
elif "50" in employees or (employees.isdigit() and int(employees) >= 50):
    resource_baseline = 12
else:
    resource_baseline = 10

# --- CERTIFICATION GAP (REVISED) ---
rdi_certs = company.get("bid_defaults", {}).get("certifications", [])
real_certs = [c for c in rdi_certs if isinstance(c, str) and "[USER INPUT" not in c]
certs_are_placeholder = len(real_certs) == 0 and len(rdi_certs) > 0

if certs_are_placeholder:
    # Placeholders mean "not yet documented" — treat as moderate risk, NOT zero certs
    cert_adjustment = -2
    resource_evidence.append("NOTE: Company certifications not yet documented (placeholder data) — moderate risk")
elif len(set(cert_refs)) == 0:
    # RFP doesn't require specific certs — no gap
    cert_adjustment = 0
else:
    cert_gap = len(set(cert_refs)) - len(real_certs)
    if cert_gap <= 0:
        cert_adjustment = 0
    elif cert_gap <= 2:
        cert_adjustment = -3
    else:
        cert_adjustment = -6
    resource_evidence.append(f"Cert gap: {max(cert_gap, 0)} (RFP refs: {len(set(cert_refs))}, company certs: {len(real_certs)})")

# --- PERSONNEL GAP ADJUSTMENT ---
# Few role references = simpler staffing; many = complex team required
unique_personnel_refs = len(set(personnel_refs))
if unique_personnel_refs > 5:
    personnel_adjustment = -3  # Many distinct roles required — heavy staffing demand
elif unique_personnel_refs > 3:
    personnel_adjustment = -2
elif unique_personnel_refs <= 1:
    personnel_adjustment = 1   # Simple staffing — well within capacity
else:
    personnel_adjustment = 0

resource_score = max(resource_baseline + cert_adjustment + personnel_adjustment, 3)
resource_score = min(resource_score, 20)

resource_rationale = (
    f"Baseline: {resource_baseline} (company size). "
    f"Cert adjustment: {cert_adjustment}. Personnel adjustment: {personnel_adjustment}. "
    f"Personnel refs: {unique_personnel_refs}. Final: {resource_score}."
)
```

## Step 5: Score Dimension 4 — Timeline Feasibility (0-20)

Assess proposal deadline and project duration.

```python
timeline_evidence = []
deadline = rfp_summary.get("submission_deadline")
pop = rfp_summary.get("period_of_performance")

if deadline:
    timeline_evidence.append(f"Deadline: {deadline}")
    # Try to parse days remaining
    # Default to moderate if parsing fails
    timeline_score = 15
    timeline_rationale = f"Deadline: {deadline}. Manual verification recommended."
else:
    timeline_score = 10
    timeline_evidence.append("No deadline detected — risk factor")
    timeline_rationale = "No deadline detected. Unknown timeline adds risk."

if pop:
    timeline_evidence.append(f"Period of performance: {pop}")
```

## Step 6: Score Dimension 5 — Strategic Alignment (0-20)

Assess geography (including state proximity), client relationship, growth industry match, state contract overlap, and revenue potential.

```python
strategic_evidence = []

# --- GEOGRAPHIC MATCHING (ENHANCED with state adjacency) ---
# Locations is a list of dicts: [{"city": "Anchorage", "state": "AK", ...}]
rdi_location_objs = company.get("locations", [])
rdi_locations_lower = []
rdi_state_abbrs = []
for loc in rdi_location_objs:
    if isinstance(loc, dict):
        for field in ["city", "state"]:
            val = loc.get(field, "").lower().strip()
            if val:
                rdi_locations_lower.append(val)
        state_val = loc.get("state", "").lower().strip()
        if state_val:
            rdi_state_abbrs.append(state_val)

geographic_match = False

# Direct city/state matching (existing logic)
location_patterns = [
    r'(?:location|state|city|office|headquarter|based\s+in)[:\s]+([^\n]{5,50})',
    r'(?:alaska|anchorage|juneau|oregon|portland|idaho|boise|texas|houston)',
]
for pattern in location_patterns:
    matches = re.findall(pattern, combined_text, re.IGNORECASE)
    for match in matches:
        match_lower = match.lower() if isinstance(match, str) else match
        if any(loc in match_lower for loc in rdi_locations_lower):
            geographic_match = True
            strategic_evidence.append(f"Geographic match: '{match}'")
            break

# State adjacency — neighboring states count as proximity
# Map of state abbreviations to their neighbors
adjacent_states = {
    "or": ["wa", "id", "ca", "nv"],
    "ak": [],  # No land borders, but AK agencies often work with OR/WA vendors
    "id": ["wa", "or", "mt", "wy", "nv", "ut"],
    "tx": ["nm", "ok", "ar", "la"]
}

# Extract RFP state from client location, agency name, or text signals
# Common state abbreviation/name mapping for detection
state_name_to_abbr = {
    "washington": "wa", "oregon": "or", "idaho": "id", "alaska": "ak",
    "texas": "tx", "montana": "mt", "california": "ca", "nevada": "nv",
    "wyoming": "wy", "utah": "ut", "new mexico": "nm", "oklahoma": "ok",
    "arkansas": "ar", "louisiana": "la", "hawaii": "hi", "colorado": "co"
}
rfp_state_abbr = ""
# Check rfp_summary for client state
client_location = rfp_summary.get("client_location", "")
client_name = rfp_summary.get("client_name", "")
rfp_state_text = f"{client_location} {client_name}".lower()

# Try abbreviation match first (e.g., "WA" in "State of WA")
for full_name, abbr in state_name_to_abbr.items():
    if full_name in rfp_state_text or full_name in combined_text.lower()[:2000]:
        rfp_state_abbr = abbr
        break
if not rfp_state_abbr:
    # Try 2-letter abbreviations in client_location
    import re as _re
    state_match = _re.search(r'\b([A-Z]{2})\b', client_location)
    if state_match:
        rfp_state_abbr = state_match.group(1).lower()

if not geographic_match and rfp_state_abbr:
    for rdi_state in rdi_state_abbrs:
        # Direct state match
        if rfp_state_abbr == rdi_state:
            geographic_match = True
            strategic_evidence.append(f"State match: RDI has office in {rdi_state.upper()}")
            break
        # Adjacent state match
        if rfp_state_abbr in adjacent_states.get(rdi_state, []):
            geographic_match = True
            strategic_evidence.append(
                f"Adjacent state proximity: RDI {rdi_state.upper()} office neighbors RFP state {rfp_state_abbr.upper()}"
            )
            break

# --- STATE CONTRACT OVERLAP ---
# Check if RDI has existing contracts in the RFP's state (strong strategic signal)
state_contract_match = False
if rfp_state_abbr:
    # Check Past_Projects.md references or company profile for state contracts
    # Look for state name in rfp_summary contract/agency references
    state_full = [name for name, abbr in state_name_to_abbr.items() if abbr == rfp_state_abbr]
    state_search_terms = [rfp_state_abbr, rfp_state_abbr.upper()] + state_full
    # Scan combined_text for references to existing contracts in that state
    # Also check if client_name contains state agency patterns
    state_agency_patterns = [f"state of {rfp_state_abbr}", f"{rfp_state_abbr} department",
                             f"{rfp_state_abbr} office of", f"{rfp_state_abbr} state"]
    if state_full:
        state_agency_patterns.extend([
            f"state of {state_full[0]}", f"{state_full[0]} department",
            f"{state_full[0]} office of"
        ])
    for pattern in state_agency_patterns:
        if pattern in combined_text.lower():
            state_contract_match = True
            strategic_evidence.append(f"State agency RFP: matches RDI's operating state/region ({rfp_state_abbr.upper()})")
            break

# --- GROWTH INDUSTRY MATCH ---
rdi_industries = [i.lower() for i in company.get("industries", [])]
rfp_domain = (rfp_summary.get("industry_domain") or "").lower()
growth_match = False
if rfp_domain:
    growth_match = any(ind in rfp_domain for ind in rdi_industries)
    if growth_match:
        strategic_evidence.append(f"Growth industry match: RFP domain '{rfp_domain}' aligns with RDI industry focus")

# --- REVENUE POTENTIAL ---
estimated_value = rfp_summary.get("estimated_value")
if estimated_value:
    strategic_evidence.append(f"Estimated value: {estimated_value}")

# --- REPEAT CLIENT CHECK ---
repeat_client = False
if client_name:
    # Check if client_name appears in past_performance references or known clients
    past_perf = company.get("past_performance", {})
    past_source = str(past_perf.get("source_file", ""))
    # Simple heuristic: if RFP client name keywords appear in combined context
    # (Full check would read Past_Projects.md — here we flag as partial match)
    client_keywords = [w.lower() for w in client_name.split() if len(w) > 3]
    # Check if any non-trivial client keywords appear in the company's known references
    if any(kw in str(past_perf).lower() for kw in client_keywords if kw not in ["state", "department", "office"]):
        repeat_client = True
        strategic_evidence.append(f"Potential repeat client: '{client_name}'")

# --- SCORING ---
strategic_factors = sum([
    geographic_match,
    growth_match or state_contract_match,
    bool(estimated_value),
    repeat_client
])

if strategic_factors >= 3:
    strategic_score = 18
elif strategic_factors >= 2:
    strategic_score = 15
elif strategic_factors == 1:
    strategic_score = 10
else:
    strategic_score = 5  # Baseline — new opportunities still have value

strategic_rationale = (
    f"Geographic: {'Yes' if geographic_match else 'No'}. "
    f"Industry match: {'Yes' if growth_match else 'No'}. "
    f"State contract overlap: {'Yes' if state_contract_match else 'No'}. "
    f"Value disclosed: {'Yes' if estimated_value else 'No'}. "
    f"Repeat client: {'Yes' if repeat_client else 'TBD'}. "
    f"Strategic factors: {strategic_factors}."
)
```

## Step 7: Calculate Total and Recommendation

```python
total_score = capability_score + competitive_score + resource_score + timeline_score + strategic_score

if total_score >= 50:
    recommendation = "GO"
elif total_score >= 40:
    recommendation = "CONDITIONAL"
else:
    recommendation = "NO_GO"

# Compile risk/opportunity factors
risk_factors = []
opportunity_factors = []

if capability_score < 10:
    risk_factors.append("Low capability alignment")
if competitive_score < 10:
    risk_factors.append("Weak competitive positioning")
if resource_score < 10:
    risk_factors.append("Resource/certification gaps")
if timeline_score < 10:
    risk_factors.append("Timeline concerns")
if strategic_score < 10:
    risk_factors.append("Low strategic value")
if set_aside:
    risk_factors.append(f"Set-aside restriction: {set_aside}")

if capability_score >= 15:
    opportunity_factors.append("Strong service alignment")
if geographic_match:
    opportunity_factors.append("Geographic proximity")
for adv in advantages_found:
    opportunity_factors.append(f"Innovation signal: '{adv}'")
```

## Step 8: Write Output

```python
go_nogo = {
    "phase": "2",
    "phase_name": "Go/No-Go Scoring",
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
    "override_allowed": True
}
write_json(f"{folder}/screen/go-nogo-score.json", go_nogo)
```

## Step 9: Report

```
GO/NO-GO SCORING (Phase 2)
===========================
Recommendation: {recommendation}
Total Score: {total_score}/100

  Capability Match:     {capability_score}/20
  Competitive Position: {competitive_score}/20
  Resource Availability:{resource_score}/20
  Timeline Feasibility: {timeline_score}/20
  Strategic Alignment:  {strategic_score}/20

Thresholds: GO >= 50 | CONDITIONAL 40-49 | NO-GO < 40

Risks: {len(risk_factors)}
Opportunities: {len(opportunity_factors)}
Output: screen/go-nogo-score.json
```

---

## Quality Checklist

- [ ] `go-nogo-score.json` written (>1KB)
- [ ] All 5 dimensions scored with rationale and evidence
- [ ] Company profile loaded — services flattened correctly (DICT not list)
- [ ] Locations handled as dicts with city/state
- [ ] Recommendation follows thresholds (GO >= 50, CONDITIONAL 40-49, NO-GO < 40)
- [ ] Risk and opportunity factors documented
- [ ] Output field is "recommendation" (not "decision")
