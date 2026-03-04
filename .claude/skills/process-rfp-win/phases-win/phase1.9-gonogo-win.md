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

Assess the RFP opportunity across 7 weighted assessment areas using LLM narrative analysis with cited evidence. Produce a weighted score (0-100) and recommend GO, CONDITIONAL, or NO_GO. This gate runs at the end of Stage 1 (Document Intake) after all RFP documents have been parsed, domain detected, evaluation criteria extracted, compliance checked, and submission structure identified. The recommendation helps the team decide whether to invest resources in completing the full bid pipeline.

## Inputs

- `{folder}/shared/domain-context.json` - Industry, compliance frameworks
- `{folder}/shared/EVALUATION_CRITERIA.json` - Scoring methodology, factor weights
- `{folder}/shared/COMPLIANCE_MATRIX.json` - Mandatory items, compliance requirements
- `{folder}/shared/SUBMISSION_STRUCTURE.json` - Volume structure, submission requirements
- `{folder}/flattened/*.md` - Full RFP text for analysis
- Company profile: read from `/home/ddubiel/repos/safs/.claude/skills/process-rfp-win/config-win/company-profile.json`
- (Optional) bid-context-bundle if available from prior capture/intelligence activities

## Required Output

- `{folder}/shared/GO_NOGO_DECISION.json` (>1KB)

---

## Step 1: Load All Stage 1 Artifacts and Company Profile

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

Prepare reference data for the assessment:

```python
# Flatten company services (services is a DICT: {"data_and_ai": [...], ...})
services_dict = company.get("services", {})
all_services = [svc for cat in services_dict.values() for svc in cat]

# Locations are dicts with city/state, not flat strings
locations = company.get("locations", [])
location_summary = [f"{loc.get('city', '')}, {loc.get('state', '')}" for loc in locations if isinstance(loc, dict)]

industries = company.get("industries", [])
certifications = company.get("bid_defaults", {}).get("certifications", [])
past_performance = company.get("past_performance", {})
```

---

## Step 2: LLM Narrative Assessment

Read all inputs thoroughly. For EACH of the 7 assessment areas below, produce a narrative assessment with:
- **score** — integer 0-100 (no decimals, no qualitative labels — a number)
- **rationale** — narrative with cited evidence from the inputs
- **evidence** — list of specific citation strings
- **risks** — list of identified risks with evidence
- **mitigations** — list of proposed mitigations with evidence

The area **name** must be EXACTLY one of: "Strategic Fit", "Technical Capability", "Competitive Position", "Resource Availability", "Financial Viability", "Risk Assessment", "Win Probability".

### EVIDENCE REQUIREMENTS (MANDATORY)

For every assessment area:

1. NEVER ASSUME — if a capability, certification, or experience is not explicitly
   documented in the company profile or past projects, treat it as absent.
   Do not infer capabilities from company size or industry presence alone.

2. GROUND EVERY CLAIM — every claim in the rationale must be traceable to a specific input.
   Describe the evidence naturally without referencing internal file names:
   - GOOD: "GIS services include Enterprise Implementations, Data Management"
   - GOOD: "Evaluation criteria list Technical Approach at 40% weight"
   - GOOD: "Two active Texas DIR contracts: DIR-CPO-6036, DIR-CPO-6069"
   - BAD: "company-profile.json services.data_and_ai includes 'Data Engineering'"
   - BAD: "Past_Projects.md Company Intelligence shows..."
   - BAD: "EVALUATION_CRITERIA.json lists..."

   **NEVER include file names** (*.json, *.md) **in rationale, evidence, risks, or mitigations.**
   These outputs feed human-readable reports and PDFs. Internal file paths are meaningless
   to evaluators and reviewers.

3. GAPS ARE EVIDENCE TOO — if the inputs don't contain evidence for a criterion,
   say so explicitly: "No documented evidence for [X]"
   Score that criterion conservatively.

4. NO FILLER — do not pad rationales with generic statements like "the company
   is well-positioned" without citing what specifically positions them.

5. NO PARROTED VALUES — do not include raw field values as "evidence" when they
   add no insight. Example: "'Not applicable'" is not evidence. Instead, state what
   the absence means: "Qualifications-only submission -- no pricing risk."

### DEADLINE-NEUTRAL SCORING (MANDATORY)

**Deadline status MUST NOT penalize area scores.** RFPs may be evaluated:
- Retroactively for learning and portfolio analysis
- In anticipation of reissuance (common in government procurement)
- To establish baseline positioning for future similar opportunities

**Rules:**
1. If the submission deadline has passed, note it as a **risk observation** in the
   overall_risks array — do NOT reduce individual area scores because of it
2. Score every area as if the opportunity were still open (evaluate capability,
   fit, competition, resources on their merits)
3. If the submission deadline has passed, add this to overall_risks:
   "NOTE: Submission deadline has passed ({deadline}). Scores reflect capability
   assessment independent of deadline status."
4. The deadline status DOES appear in the final recommendation rationale as context,
   but the recommendation threshold (GO/CONDITIONAL/NO-GO) is based on merit scores only

---

### Area 1: Strategic Fit (Weight: 15%)

**What to evaluate:**
- Geographic proximity between company office locations and the RFP's place of performance or issuing agency state
- Industry alignment between company industries and the RFP domain
- Existing contracts in the same state or with the same agency (repeat client potential)
- Revenue significance relative to company size
- Growth opportunity in the target market

**Where to find evidence:**
- `company-profile.json` locations (list of dicts with city/state), industries
- `domain-context.json` industry, compliance frameworks, geographic indicators
- `combined_text` for geographic requirements, place of performance clauses, client/agency name

**Scoring guidance:**
- **80-100:** Office in the same state, strong industry alignment, existing client relationship, significant revenue opportunity
- **50-70:** Adjacent state or remote-eligible, partial industry overlap, new client in known sector
- **20-40:** No geographic presence, weak industry fit, small or unclear revenue
- **0-20:** Misaligned geography with on-site requirement, no industry relevance

**Do not assume — if evidence is not found in the inputs, score conservatively and note the gap.**

---

### Area 2: Technical Capability (Weight: 25%)

**What to evaluate:**
- Service alignment: how many RFP scope requirements and mandatory items match company services
- Domain expertise: relevant past project experience documented in company profile
- Technology stack overlap: does the company offer the specific technologies the RFP demands
- Mandatory requirement coverage: can each stated requirement be addressed by a documented capability
- Depth vs breadth: does the company have deep expertise or only tangential coverage

**Where to find evidence:**
- `company-profile.json` services (DICT — flatten all categories), past_performance
- `EVALUATION_CRITERIA.json` factors and scoring methodology
- `COMPLIANCE_MATRIX.json` mandatory_items and requirements
- `combined_text` for detailed technical requirements, SOW specifics

**Scoring guidance:**
- **80-100:** Strong match on 80%+ of requirements, documented past projects in same domain, all mandatory requirements addressable
- **50-70:** Moderate overlap (50-80%), some past experience, most requirements addressable with gaps noted
- **20-40:** Limited overlap (<50%), few relevant past projects, significant capability gaps
- **0-20:** Minimal alignment, no documented experience in the domain

**Do not assume — if evidence is not found in the inputs, score conservatively and note the gap.**

---

### Area 3: Competitive Position (Weight: 20%)

**What to evaluate:**
- Evaluation criteria alignment: how well company strengths map to stated evaluation factors and their weights
- Advantage signals: best value procurement, innovation emphasis, modern technology preference
- Disadvantage signals: incumbent contractor, set-aside restrictions, sole-source history
- Competition level: prior proposal counts, number of expected bidders, market saturation
- Preference points: veteran-owned, small business, HUBZone eligibility vs requirements
- COTS positioning: whether COTS/low-code is preferred (disadvantaging custom builders)

**Where to find evidence:**
- `EVALUATION_CRITERIA.json` factors, weights, evaluation methodology
- `company-profile.json` bid_defaults (veteran_owned, small_business, certifications)
- `COMPLIANCE_MATRIX.json` for set-aside or preference requirements
- `combined_text` for competition signals, preference language, incumbent references

**Scoring guidance:**
- **80-100:** Evaluation criteria favor company strengths, no set-aside barriers, few competitors expected, eligible for preference points
- **50-70:** Neutral competitive landscape, some evaluation criteria align, moderate competition
- **20-40:** Set-aside restrictions apply, incumbent advantage detected, high competition expected, preference points not available
- **0-20:** Strong incumbent, restrictive set-aside excludes company, COTS preference with no platform offering

**Do not assume — if evidence is not found in the inputs, score conservatively and note the gap.**

---

### Area 4: Resource Availability (Weight: 15%)

**What to evaluate:**
- Staffing capacity relative to personnel requirements (key personnel, team size)
- Personnel qualification requirements (resumes, CVs, specific roles demanded)
- Certification gaps: required certifications vs company's documented certifications
- Proposal volume and complexity: how much effort the response will require
- Timeline feasibility: is the proposal deadline achievable given scope

**Where to find evidence:**
- `company-profile.json` employees, bid_defaults.certifications
- `SUBMISSION_STRUCTURE.json` volumes, page limits, deliverable count
- `COMPLIANCE_MATRIX.json` mandatory_items for personnel and certification mandates
- `combined_text` for key personnel requirements, certification mandates, clearance requirements, proposal deadline

**Scoring guidance:**
- **80-100:** Company size supports staffing needs, all certifications documented, reasonable proposal timeline
- **50-70:** Adequate staffing likely, minor certification gaps, tight but feasible timeline
- **20-40:** Staffing strain likely, significant certification gaps, very tight deadline
- **0-20:** Insufficient capacity, critical certifications missing, deadline likely infeasible

**Note on placeholder data:** If company certifications contain "[USER INPUT" placeholders, treat as "not yet documented" and score with moderate caution — not as zero certifications, but not as confirmed either.

**Do not assume — if evidence is not found in the inputs, score conservatively and note the gap.**

---

### Area 5: Financial Viability (Weight: 10%)

**What to evaluate:**
- Contract value relative to company size and revenue (is this appropriately scaled)
- Cost formula or pricing structure requirements and their impact
- Budget constraints, ceiling prices, or rate caps mentioned in the RFP
- Payment terms (net-30, milestone-based, cost-reimbursable)
- Indirect cost rate limitations or audit requirements

**Where to find evidence:**
- `EVALUATION_CRITERIA.json` for price/cost factor weight and evaluation method
- `company-profile.json` employees, revenue (if available), bid_defaults
- `combined_text` for pricing clauses, rate ceilings, payment schedules, cost accounting standards

**Scoring guidance:**
- **80-100:** Contract value well-aligned with company capacity, favorable payment terms, no restrictive rate caps
- **50-70:** Value is manageable, standard payment terms, some pricing constraints to work around
- **20-40:** Value too large or too small for company, restrictive rate caps, unfavorable payment terms
- **0-20:** Severe financial mismatch, cost accounting requirements company cannot meet

**Do not assume — if evidence is not found in the inputs, score conservatively and note the gap.**

---

### Area 6: Risk Assessment (Weight: 10%)

**What to evaluate:**
- Technical complexity: scope ambiguity, integration requirements, legacy system dependencies
- Compliance obligations: FedRAMP, FISMA, HIPAA, Section 508, state-specific regulations
- Political risk: controversial project, agency leadership changes, funding uncertainty
- Integration burden: number of external systems, data migration, interoperability requirements
- Data sensitivity: PII, PHI, CUI, classified information handling requirements

**Where to find evidence:**
- `COMPLIANCE_MATRIX.json` mandatory_items for compliance and security requirements
- `domain-context.json` compliance_frameworks for regulatory context
- `combined_text` for compliance clauses, security requirements, integration specifications, data handling rules
- `company-profile.json` for relevant compliance certifications or clearances

**Scoring guidance (NOTE: higher = LOWER risk = better):**
- **80-100:** Straightforward scope, minimal compliance burden, low integration complexity, standard data handling
- **50-70:** Moderate complexity, manageable compliance requirements, some integration work
- **20-40:** High complexity, significant compliance burden, extensive integration, sensitive data
- **0-20:** Extreme complexity, compliance requirements company cannot meet, critical security clearances needed

**Do not assume — if evidence is not found in the inputs, score conservatively and note the gap.**

---

### Area 7: Win Probability (Weight: 5%)

**What to evaluate:**
- Estimated realistic win chance given all other factors
- Structural advantages: existing relationship, geographic presence, unique capability
- Structural disadvantages: incumbent lock, set-aside exclusion, late market entry
- Teaming opportunities: could a teaming arrangement improve the bid
- Overall competitive dynamics based on all signals gathered

**Where to find evidence:**
- Synthesize findings from all other assessment areas
- `EVALUATION_CRITERIA.json` evaluation methodology and factor weights
- `COMPLIANCE_MATRIX.json` for set-aside or eligibility restrictions
- `combined_text` for teaming clauses, joint venture provisions, subcontracting goals

**Scoring guidance:**
- **80-100:** Multiple structural advantages, weak or no incumbent, high capability match, favorable evaluation criteria
- **50-70:** Reasonable chance with solid proposal, no decisive disadvantages, competitive field
- **20-40:** Significant headwinds but not disqualifying, teaming could help
- **0-20:** Structural barriers make winning very unlikely without extraordinary circumstances

**Do not assume — if evidence is not found in the inputs, score conservatively and note the gap.**

---

## Step 3: Compute Weighted Total

After generating all 7 narrative assessments with scores:

```python
weights = {
    "Strategic Fit": 0.15,
    "Technical Capability": 0.25,
    "Competitive Position": 0.20,
    "Resource Availability": 0.15,
    "Financial Viability": 0.10,
    "Risk Assessment": 0.10,
    "Win Probability": 0.05,
}

# assessment_areas is the list of 7 area dicts built from the LLM analysis above
overall_score = round(sum(area["score"] * area["weight"] for area in assessment_areas))
```

---

## Step 4: Apply Thresholds

```python
if overall_score >= 50:
    recommendation = "GO"
    user_decision_required = False
elif overall_score >= 40:
    recommendation = "CONDITIONAL"
    user_decision_required = True
else:
    recommendation = "NO_GO"
    user_decision_required = True
```

---

## Step 5: Write GO_NOGO_DECISION.json

Write `{folder}/shared/GO_NOGO_DECISION.json` with this schema:

```python
# Extract RFP identification from available inputs
rfp_number = domain.get("rfp_number", "")
rfp_title = domain.get("rfp_title", "")
issuing_agency = domain.get("issuing_agency", domain.get("client_name", ""))

# If not in domain-context, check eval_criteria or compliance for RFP metadata
if not rfp_number:
    rfp_number = eval_criteria.get("rfp_number", compliance.get("rfp_number", ""))
if not rfp_title:
    rfp_title = eval_criteria.get("rfp_title", compliance.get("rfp_title", ""))
if not issuing_agency:
    issuing_agency = eval_criteria.get("issuing_agency", compliance.get("issuing_agency", ""))

# Identify recommended work sections if RFP has separable lots/sections
# Populate from SUBMISSION_STRUCTURE.json or COMPLIANCE_MATRIX.json if applicable
recommended_work_sections = []  # e.g., ["Section A - IT Services", "Section C - Data Analytics"]

# Aggregate risks and mitigations across all areas
overall_risks = []
overall_mitigations = []
for area in assessment_areas:
    overall_risks.extend(area.get("risks", []))
    overall_mitigations.extend(area.get("mitigations", []))

decision = {
    "phase": "1.9",
    "phase_name": "Go/No-Go Decision Gate",
    "timestamp": datetime.now().isoformat(),
    "rfp_number": rfp_number,
    "rfp_title": rfp_title,
    "issuing_agency": issuing_agency,
    "bidding_company": "Resource Data, Inc.",
    "recommendation": recommendation,
    "overall_score": overall_score,
    "threshold": {
        "go": 50,
        "conditional": 40,
        "no_go": 0
    },
    "assessment_areas": [
        {
            "name": area["name"],
            "weight": area["weight"],
            "score": area["score"],
            "rationale": area["rationale"],
            "evidence": area["evidence"],
            "risks": area["risks"],
            "mitigations": area["mitigations"]
        }
        for area in assessment_areas
    ],
    "recommended_work_sections": recommended_work_sections,
    "overall_risks": overall_risks,
    "overall_mitigations": overall_mitigations,
    "override_allowed": True,
    "user_decision_required": user_decision_required
}

write_json(f"{folder}/shared/GO_NOGO_DECISION.json", decision)
```

---

## Step 6: Report

```
GO/NO-GO DECISION GATE (Phase 1.9)
=============================================
Recommendation: {recommendation}
Overall Score: {overall_score}/100

  Area                    Weight   Score   Weighted
  ---------------------   ------   -----   --------
  Strategic Fit           15%      {s1}    {s1*0.15:.1f}
  Technical Capability    25%      {s2}    {s2*0.25:.1f}
  Competitive Position    20%      {s3}    {s3*0.20:.1f}
  Resource Availability   15%      {s4}    {s4*0.15:.1f}
  Financial Viability     10%      {s5}    {s5*0.10:.1f}
  Risk Assessment         10%      {s6}    {s6*0.10:.1f}
  Win Probability          5%      {s7}    {s7*0.05:.1f}
  ---------------------   ------   -----   --------
  TOTAL                   100%             {overall_score}

Thresholds: GO >= 50 | CONDITIONAL 40-49 | NO-GO < 40

Top Risks:
{bullet list of overall_risks}

Top Mitigations:
{bullet list of overall_mitigations}

{if recommended_work_sections:
  "Recommended Work Sections:"
  {bullet list of recommended_work_sections}
}

{if recommendation == "CONDITIONAL":
  "ACTION REQUIRED: Score falls in CONDITIONAL range. Please review the analysis above and decide whether to proceed (GO) or stop (NO-GO). Reply with your decision."
}
{if recommendation == "NO_GO":
  "ACTION REQUIRED: Score falls below the GO threshold. Recommendation is to not bid. Override is available — reply GO to proceed anyway."
}

Output: shared/GO_NOGO_DECISION.json
```

---

## Quality Checklist

- [ ] `GO_NOGO_DECISION.json` written (>1KB)
- [ ] All 7 assessment areas scored with rationale, evidence, risks, mitigations
- [ ] Company profile loaded and services compared against RFP requirements
- [ ] Every rationale cites specific evidence from inputs
- [ ] No unsupported claims or assumptions
- [ ] overall_score = sum(score * weight) for all 7 areas, rounded
- [ ] Recommendation follows threshold rules (GO >= 50, CONDITIONAL 40-49, NO-GO < 40)
- [ ] User decision solicited for CONDITIONAL and NO-GO recommendations
- [ ] Output field is "recommendation" (not "decision")
