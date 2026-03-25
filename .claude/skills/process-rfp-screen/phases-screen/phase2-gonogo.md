---
name: phase2-gonogo
expert-role: Bid Decision Analyst
domain-expertise: Bid/no-bid analysis, opportunity qualification, capture management, Shipley BD lifecycle
skill: capture-strategist
sub-skill: bid-decision
---

# Phase 2: Go/No-Go Scoring

**Purpose:** Assess the RFP opportunity across 7 weighted assessment areas using LLM narrative analysis with cited evidence. Produce a weighted score (0-100) and recommend GO, CONDITIONAL, or NO_GO.

**Inputs:**
- Combined RFP text (in memory from Phase 0)
- `{folder}/screen/rfp-summary.json` — Phase 1 metadata extraction
- Company profile: `config-win/company-profile.json`
- `Past_Projects.md` — 35 case studies + Government Contracts + Additional Known Clients + Company Intelligence (for Strategic Fit, Competitive Position, and Financial Viability evidence)

**Required Output:**
- `{folder}/screen/go-nogo-score.json` (>1KB)

---

## Step 1: Load Inputs

```python
rfp_summary = read_json(f"{folder}/screen/rfp-summary.json")
company = read_json(COMPANY_PROFILE)
# combined_text available from Phase 0 (in memory)

# New Phase 1 enriched fields (backward-compatible — may not exist on older runs)
buyer_priorities = rfp_summary.get("buyer_priorities", [])
required_technologies = rfp_summary.get("required_technologies") or rfp_summary.get("scope_keywords", [])
evaluation_subfactors = rfp_summary.get("evaluation_subfactors", [])
high_priorities = [p for p in buyer_priorities if p.get("importance") == "HIGH"]

# Phase 1 intelligence layers (Batch 3 — backward-compatible with empty dict defaults)
client_tone = rfp_summary.get("client_tone", {})
tech_intelligence = rfp_summary.get("tech_intelligence", {})
evaluation_model = rfp_summary.get("evaluation_model", {})
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

## Skill Integration: Capture-Strategist & Bid-Decision Framework Application (MANDATORY)

The **capture-strategist** and **bid-decision** sub-skill are loaded in context. Apply these frameworks as you assess each area:

**Shipley Phase 2 (Opportunity Assessment) Gate Criteria:**
- Is the opportunity real? (funded, defined scope, legitimate procurement)
- Can we compete? (technical capability, past performance, competitive position)
- Can we win? (discriminators, customer access, price competitiveness)
- Is it worth winning? (strategic fit, profitability, resource investment)

**APMP's 5 Opportunity Assessment Dimensions:**
Use as an overlay lens on each assessment area:
1. Customer — Do we know the customer? Do they know us?
2. Opportunity — Is the deal real, funded, and winnable?
3. Solution — Can we deliver what they need?
4. Competitive — Who else is bidding? Can we differentiate?
5. Team — Do we have the people and bandwidth?

**Lohfeld's 12 KPIs** — specifically assess these where data exists:
- Customer access level, requirements understanding, solution maturity
- Competitive intelligence depth, team readiness, price competitiveness
- Past performance relevance, incumbent relationship, key personnel availability

**Cognitive Bias Check (from bid-decision sub-skill):**
After completing all 7 assessments, briefly check for these 6 biases:
1. Sunk cost — "We've already invested time analyzing this" should not inflate scores
2. Optimism bias — Are scores based on evidence or hope?
3. Anchoring — Is the first impression dominating the assessment?
4. Confirmation bias — Are we seeking evidence that confirms GO while ignoring red flags?
5. Availability bias — Are we overweighting recent wins/losses?
6. Groupthink — Would a contrarian view change the recommendation?

Include a brief `bias_assessment` note in the output indicating whether any bias was detected.

---

## Step 2: LLM Narrative Assessment

Read all three inputs thoroughly. For EACH of the 7 assessment areas below, produce a narrative assessment with:
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
   - GOOD: "TCEQ GeoTAM project demonstrates dashboard delivery"
   - GOOD: "Two active Texas DIR contracts: DIR-CPO-6036, DIR-CPO-6069"
   - BAD: "company-profile.json services.data_and_ai includes 'Data Engineering'"
   - BAD: "Past_Projects.md Company Intelligence shows..."
   - BAD: "rfp-summary.json scope_keywords include..."

   **NEVER include file names** (*.json, *.md) **in rationale, evidence, risks, or mitigations.**
   These outputs feed human-readable PDF reports. Internal file paths are meaningless
   to reviewers.

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
5. **Historical RFP value:** Screening past-deadline RFPs is valuable for portfolio
   analysis, pipeline calibration, and positioning for similar future opportunities.
   The GO/NO-GO recommendation indicates whether the firm SHOULD HAVE bid (or should
   pursue similar opportunities), not whether it CAN still bid.

---

### SCORING ANCHORS (MANDATORY -- reference when assigning every score)

| Range | Rating | Definition |
|-------|--------|-----------|
| 90-100 | Exceptional | Near-zero gaps, hard discriminators with verifiable proof, demonstrably superior to likely competitors |
| 75-89 | Strong | Clear advantages with minor gaps that have mitigations, competitive on most dimensions |
| 60-74 | Adequate | Meets requirements without standout advantages, competitive but not differentiated |
| 40-59 | Weak | Notable gaps or competitive disadvantages, significant mitigation required |
| 0-39 | Critical | Disqualifying gaps or fundamental misalignment |

**Calibration mandate:** Before assigning any score above 85, explicitly state:
1. What would need to be true for this area to score 100
2. What specific gap or uncertainty prevents the higher score
This prevents optimism bias and forces evidence-based scoring.

### SCORE CEILING RULE

When key information is unknown, affected area scores are CAPPED at 85:
- **Competitive field unknown** → Competitive Position capped at 85 (cannot claim advantage without knowing competitors)
- **Contract value undisclosed** → Financial Viability capped at 85 (cannot assess fit without knowing scale)
- **Incumbent unknown** → Win Probability capped at 85 (cannot estimate win chance without knowing competitive landscape)
- **Key personnel availability unconfirmed** → Resource Availability capped at 85

The ceiling does NOT apply when the information IS known. It ensures scores reflect actual knowledge, not assumptions.

---

### Area 1: Strategic Fit (Weight: 15%)

**What to evaluate:**
- Geographic proximity between company office locations and the RFP's place of performance or issuing agency state
- Industry alignment between company industries and the RFP domain
- Existing contracts in the same state or with the same agency (repeat client potential)
- **Existing contract vehicles in the RFP's state** (GSA Schedule 70, state ITPS/TOPS/MNSITE/MPSA/DIR contracts — see Government Contracts Summary in Past_Projects.md)
- **Existing client relationships** — check if the RFP client appears in Past_Projects.md (35 case studies + 26 Additional Known Clients)
- Revenue significance relative to company size (~$36.1M estimated annual revenue per Company Intelligence section)
- Growth opportunity in the target market

**Where to find evidence:**
- `company-profile.json` locations (list of dicts with city/state), industries
- `rfp-summary.json` client_name, client_location, industry_domain, estimated_value
- `combined_text` for geographic requirements, place of performance clauses
- `Past_Projects.md` Government Contracts Summary (Federal/State/Local contract vehicles), Additional Known Clients section, Company Intelligence financial indicators

**Scoring guidance:**
- **80-100:** Office in the same state, strong industry alignment, existing client relationship, significant revenue opportunity
- **50-70:** Adjacent state or remote-eligible, partial industry overlap, new client in known sector
- **20-40:** No geographic presence, weak industry fit, small or unclear revenue
- **0-20:** Misaligned geography with on-site requirement, no industry relevance

**Do not assume — if evidence is not found in the inputs, score conservatively and note the gap.**

---

### Area 2: Technical Capability (Weight: 25%)

**What to evaluate:**
- Service alignment: how many RFP scope keywords and mandatory requirements match company services
- Domain expertise: relevant past project experience documented in company profile
- Technology stack overlap: does the company offer the specific technologies the RFP demands
- Mandatory requirement coverage: can each stated requirement be addressed by a documented capability
- Depth vs breadth: does the company have deep expertise or only tangential coverage
- **Buyer priority coverage:** For each buyer priority tagged HIGH, assess if the company has direct documented capability. Each unaddressable HIGH priority is a ceiling-reducer (~15 pts from 100)
- **Tech maturity modifiers (from `tech_intelligence`):** Apply +5 for each established technology with strong RDI alignment (cite specific technology names and `rdi_alignment` fields). Apply -5 per unmatched emerging technology (cap at -15 total). Example: "ArcGIS Enterprise has strong RDI alignment via Esri Gold Partnership (+5)" or "Terraform has no documented RDI experience (-5)".
- **Stack coherence check:** If `tech_intelligence.stack_coherence_score` < 0.5, note as risk — the client may not fully understand their own technology needs, which increases scope ambiguity and delivery risk.

**Where to find evidence:**
- `company-profile.json` services (DICT — flatten all categories), past_performance
- `rfp-summary.json` scope_keywords, required_technologies (prefer over scope_keywords), mandatory_requirements, buyer_priorities
- `rfp-summary.json` tech_intelligence.rdi_alignment, tech_intelligence.stack_coherence_score
- `combined_text` for detailed technical requirements, SOW specifics

**Scoring guidance:**
- **80-100:** Strong match on 80%+ of scope keywords, documented past projects in same domain, all mandatory requirements addressable
- **50-70:** Moderate overlap (50-80%), some past experience, most requirements addressable with gaps noted
- **20-40:** Limited overlap (<50%), few relevant past projects, significant capability gaps
- **0-20:** Minimal alignment, no documented experience in the domain

**Do not assume — if evidence is not found in the inputs, score conservatively and note the gap.**

---

### Area 3: Competitive Position (Weight: 20%)

**What to evaluate:**
- Evaluation criteria alignment: how well company strengths map to stated evaluation factors
- Advantage signals: best value procurement, innovation emphasis, modern technology preference
- Disadvantage signals: incumbent contractor, set-aside restrictions, sole-source history
- Competition level: prior proposal counts, number of expected bidders, market saturation
- Preference points: veteran-owned, small business, HUBZone eligibility vs requirements
- COTS positioning: whether COTS/low-code is preferred (disadvantaging custom builders)
- **Buyer priority differentiation:** HIGH buyer priorities where the company has a differentiator = significant advantage. Where the company merely meets the bar = neutral. Where there's a gap = disadvantage
- **Awards and recognition** — Multiple Esri Partner of the Year and SAG Awards, Top Workplaces #48 (2025) provide competitive differentiation evidence (see Company Intelligence in Past_Projects.md)
- **Technology partnerships** — Esri Gold Partner since 1992 (34 years), Snowflake Services Partner, Databricks Consulting Services Partner — strong differentiators for relevant technology stacks
- **Evaluation point alignment (from `evaluation_model`):** Map RDI's strongest capabilities to the highest-point evaluation criteria from `evaluation_model.point_allocation`. For each assessment area, add a `point_alignment` output field listing which high-point criteria the company can address and estimated point capture potential. Example: if "Technical Approach" is worth 600/1000 points and RDI has strong alignment, note this as a major competitive advantage.

**Where to find evidence:**
- `rfp-summary.json` evaluation_criteria, evaluation_method, set_aside, prior_rfp_history
- `rfp-summary.json` evaluation_model.point_allocation (criteria names, point values, decomposition)
- `company-profile.json` bid_defaults (veteran_owned, small_business, certifications)
- `combined_text` for competition signals, preference language, incumbent references
- `Past_Projects.md` Company Intelligence section: Awards & Recognition, Certifications & Partnerships

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
- `rfp-summary.json` submission_deadline, period_of_performance, mandatory_requirements
- `combined_text` for key personnel requirements, certification mandates, proposal page limits, clearance requirements

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
- `rfp-summary.json` estimated_value, contract_type, pricing_structure
- `company-profile.json` employees, revenue (if available), bid_defaults
- `Past_Projects.md` Company Intelligence: ~$36.1M estimated annual revenue (2025), ~200 employees, employee-owned
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
- `rfp-summary.json` mandatory_requirements, compliance_requirements, technology_requirements
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

**Apply Pwin Estimation Methodology from bid-decision sub-skill:**
The score for this area should reflect a calibrated win probability assessment, not just a gestalt feeling. The bid-decision sub-skill defines 4 Pwin levels:
- Level 1 (Subjective): Gut feel based on overall assessment — acceptable for screening
- Level 2 (Factor-Based): Weighted assessment of key factors (what this phase does)
- Level 3 (Comparative): Benchmarked against similar past bids (if bid-outcomes.json has data)
- Level 4 (Historical Calibration): Statistical calibration against outcome data (future capability)

For this screening, target Level 2 (Factor-Based) and Level 3 if historical data exists.

**What to evaluate:**
- Estimated realistic win chance given all other factors
- Structural advantages: existing relationship, geographic presence, unique capability
- Structural disadvantages: incumbent lock, set-aside exclusion, late market entry
- Teaming opportunities: could a teaming arrangement improve the bid
- Overall competitive dynamics based on all signals gathered
- **Buyer priority balance:** Count HIGH buyer priorities where company has strong evidence vs gaps. More addressed = higher win probability
- **Shipley competitive assessment:** Apply the "Can we win?" gate criterion — name 3+ discriminators or explain why fewer suffice

**Where to find evidence:**
- Synthesize findings from all other assessment areas
- `rfp-summary.json` prior_rfp_history, set_aside, evaluation_method
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

# VERIFICATION: Ensure overall_score matches weighted sum
calculated_score = round(sum(area["score"] * area["weight"] for area in assessment_areas))
if calculated_score != overall_score:
    log(f"  WARNING: Score mismatch -- calculated {calculated_score} vs stated {overall_score}. Using calculated value.")
    overall_score = calculated_score
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

## Step 5: Write Output

Write `{folder}/screen/go-nogo-score.json` with this schema:

```python
go_nogo = {
    "phase": "2",
    "phase_name": "Go/No-Go Scoring",
    "timestamp": datetime.now().isoformat(),
    "rfp_number": rfp_summary.get("rfp_number", ""),
    "rfp_title": rfp_summary.get("rfp_title", ""),
    "issuing_agency": rfp_summary.get("client_name", ""),
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
            "name": "Strategic Fit",
            "weight": 0.15,
            "score": strategic_fit_score,
            "rationale": "Detailed narrative with SPECIFIC evidence citations...",
            "evidence": ["cite 1", "cite 2"],
            "risks": ["risk with evidence"],
            "mitigations": ["mitigation with evidence"]
        },
        # ... repeat for all 7 areas
    ],
    "recommended_work_sections": [],
    "overall_risks": ["aggregated risk strings"],
    "overall_mitigations": ["aggregated mitigation strings"],
    "override_allowed": True,
    "user_decision_required": user_decision_required,

    # Buyer priority coverage tracking (from Phase 1 enrichment)
    "buyer_priority_coverage": buyer_priority_coverage,

    # Skill-informed strategic assessment (capture-strategist + bid-decision)
    "shipley_gate_assessment": {
        "opportunity_real": True,  # Is this a funded, legitimate procurement?
        "can_compete": True,  # Do we have the capability?
        "can_win": True,  # Do we have discriminators?
        "worth_winning": True,  # Is it strategically valuable?
        "discriminators_identified": [],  # 3+ discriminators named
        "conditions_for_go": []  # Gaps that need resolution
    },
    "bias_assessment": "",  # Brief note on whether cognitive biases were detected during scoring

    # Phase 1 intelligence layer pass-through (for downstream consumption by Phases 4.5, 5, 5.5)
    "client_tone": client_tone,  # Pass through from rfp-summary.json for theme/question tone adaptation
}

# Build buyer_priority_coverage by assessing each HIGH priority against company capabilities
high_priorities = [p for p in buyer_priorities if p.get("importance") == "HIGH"]
addressed = []
gaps = []

for priority in high_priorities:
    priority_name = priority.get("name", "")
    linked_kws = [kw.lower() for kw in priority.get("linked_scope_keywords", [])]

    # Check if any linked keyword appears in company services or capabilities
    company_match = False
    for kw in linked_kws:
        for svc in all_services:
            if kw in svc.lower() or svc.lower() in kw:
                company_match = True
                break
        if company_match:
            break

    if company_match:
        addressed.append(priority_name)
    else:
        gaps.append(priority_name)

buyer_priority_coverage = {
    "high_total": len(high_priorities),
    "high_addressed": len(addressed),
    "high_addressed_list": addressed,
    "high_gaps": gaps
}

go_nogo["buyer_priority_coverage"] = buyer_priority_coverage

write_json(f"{folder}/screen/go-nogo-score.json", go_nogo)
```

---

## Step 6: Report

```
GO/NO-GO SCORING (Phase 2)
===========================
Recommendation: {recommendation}
Overall Score: {overall_score}/100

  Area                    Weight   Score   Weighted
  ─────────────────────   ──────   ─────   ────────
  Strategic Fit           15%      {s1}    {s1*0.15:.1f}
  Technical Capability    25%      {s2}    {s2*0.25:.1f}
  Competitive Position    20%      {s3}    {s3*0.20:.1f}
  Resource Availability   15%      {s4}    {s4*0.15:.1f}
  Financial Viability     10%      {s5}    {s5*0.10:.1f}
  Risk Assessment         10%      {s6}    {s6*0.10:.1f}
  Win Probability          5%      {s7}    {s7*0.05:.1f}
  ─────────────────────   ──────   ─────   ────────
  TOTAL                   100%             {overall_score}

Thresholds: GO >= 50 | CONDITIONAL 40-49 | NO-GO < 40

Buyer Priority Coverage:
  HIGH priorities: {high_total} total, {high_addressed} addressed, {len(high_gaps)} gaps
  Gaps: {', '.join(high_gaps) or "None"}

Top Risks:
{bullet list of overall_risks}

Top Mitigations:
{bullet list of overall_mitigations}

Output: screen/go-nogo-score.json
```

---

## Quality Checklist

- [ ] `go-nogo-score.json` written (>1KB)
- [ ] All 7 assessment areas scored with rationale, evidence, risks, mitigations
- [ ] Company profile loaded — services flattened correctly (DICT not list)
- [ ] Locations handled as dicts with city/state
- [ ] Every rationale cites specific evidence from inputs
- [ ] No unsupported claims or assumptions
- [ ] overall_score = sum(score * weight) for all 7 areas, rounded
- [ ] Recommendation follows thresholds (GO >= 50, CONDITIONAL 40-49, NO-GO < 40)
- [ ] Output field is "recommendation" (not "decision")
- [ ] buyer_priorities and required_technologies loaded from rfp-summary.json (with fallback)
- [ ] buyer_priority_coverage computed: high_total, high_addressed, high_gaps
- [ ] HIGH buyer priorities referenced in Technical Capability, Competitive Position, and Win Probability narratives

### Intelligence Layer Integration Quality Checks (Batch 3)
- [ ] `client_tone`, `tech_intelligence`, `evaluation_model` loaded from rfp-summary.json with `.get()` defaults
- [ ] Tech maturity modifiers applied in Area 2 (+5 strong RDI match, -5 unmatched emerging, cap -15)
- [ ] Stack coherence score < 0.5 flagged as risk in Area 2 if present
- [ ] Evaluation point alignment assessed in Area 3 using `evaluation_model.point_allocation`
- [ ] `point_alignment` output field included per Competitive Position area
- [ ] `client_tone` passed through to go-nogo-score.json output for downstream phases

### Skill Integration Quality Checks (capture-strategist + bid-decision)
- [ ] Shipley Phase 2 gate criteria assessed (opportunity_real, can_compete, can_win, worth_winning)
- [ ] 3+ discriminators identified and named in shipley_gate_assessment
- [ ] APMP 5-dimension overlay applied as lens across assessment areas
- [ ] Lohfeld KPIs referenced where data available (customer access, solution maturity, team readiness)
- [ ] Area 7 uses Pwin estimation methodology (Level 2+ factor-based, Level 3 if historical data)
- [ ] Cognitive bias check performed: sunk cost, optimism, anchoring, confirmation, availability, groupthink
- [ ] bias_assessment field populated with brief finding
- [ ] **Anti-pattern check:** No "strategy-free proposals," no "spray and pray," no "ignoring evaluation methodology"
