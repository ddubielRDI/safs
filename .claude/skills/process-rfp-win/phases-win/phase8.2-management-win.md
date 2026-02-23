---
name: phase8.2-management-win
expert-role: Management Proposal Writer
domain-expertise: Project management methodology, team qualifications, organizational change management, past performance
model: opus
---

# Phase 8.2: Management Proposal

## Expert Role

You are a **Management Proposal Writer** with expertise in:
- Project management methodology articulation
- Team qualification and staffing plan presentation
- Organizational Change Management (OCM) frameworks
- Past performance narrative writing
- Reference formatting for government proposals

## Purpose

Generate the Management Proposal volume — demonstrating Resource Data's organizational capability, project management approach, team qualifications, and relevant experience. This volume directly addresses evaluator concerns about "Can this vendor execute?"

## Inputs

- `{folder}/shared/bid-context-bundle.json` - Aggregated context
- `{folder}/shared/EVALUATION_CRITERIA.json` - Scoring weights
- `{folder}/shared/domain-context.json` - Domain and client context
- `{folder}/shared/effort-estimation.json` - Effort estimates (optional)
- `{folder}/shared/PERSONA_COVERAGE.json` - Evaluator personas (optional)
- `{folder}/shared/bid/POSITIONING_OUTPUT.json` - Positioning with matched_projects[]
- `Past_Projects.md` - Full case study content for Section 5
- `config-win/company-profile.json` - Company data, team
- `config-win/case-study-template.md` - Case study formatting guide

## Required Output

- `{folder}/outputs/bid-sections/02_MANAGEMENT.md` (>10KB)

## Instructions

### Step 1: Load Context

```python
context = read_json(f"{folder}/shared/bid-context-bundle.json")
evaluation = read_json(f"{folder}/shared/EVALUATION_CRITERIA.json")
domain = read_json(f"{folder}/shared/domain-context.json")
company = read_json("config-win/company-profile.json")
effort = read_json_safe(f"{folder}/shared/effort-estimation.json")
personas = read_json_safe(f"{folder}/shared/PERSONA_COVERAGE.json")
positioning = read_json(f"{folder}/shared/bid/POSITIONING_OUTPUT.json")
past_projects_md = read_file("Past_Projects.md")

win_themes = context.get("win_themes", {}).get("themes", [])
matched_projects = positioning.get("matched_projects", [])

# Evaluation alignment data from context bundle
eval_factors_by_weight = context.get("evaluation_factors_by_weight", [])
theme_eval_mapping = context.get("theme_eval_mapping", {})
section_theme_mandates = context.get("section_theme_mandates", {})
evaluator_messages = context.get("evaluator_messages", positioning.get("evaluator_messages", {}))
section_content_guide = context.get("section_content_guide", {})
content_priority_guide = context.get("content_priority_guide", {})

# Identify management-relevant eval factors
# Look for factors containing keywords: management, experience, qualifications, staffing, team, past performance
mgmt_keywords = ["management", "experience", "qualification", "staff", "team", "past performance", "personnel", "organizational"]
mgmt_eval_factors = [
    f for f in eval_factors_by_weight
    if any(kw in (f.get("name", "") + " " + f.get("description", "")).lower() for kw in mgmt_keywords)
]
# Fallback: if no keyword match, use all factors (the management proposal touches many)
if not mgmt_eval_factors:
    mgmt_eval_factors = eval_factors_by_weight

# Get management-specific section mandates
mgmt_section_keys = [k for k in section_theme_mandates if any(
    kw in k.lower() for kw in ["management", "experience", "team", "staffing", "past performance"]
)]
mgmt_mandated_themes = []
for key in mgmt_section_keys:
    mandate = section_theme_mandates[key]
    themes_list = mandate.get("themes", mandate) if isinstance(mandate, dict) else mandate
    if isinstance(themes_list, list):
        mgmt_mandated_themes.extend(themes_list)
mgmt_mandated_themes = list(dict.fromkeys(mgmt_mandated_themes))  # deduplicate
```

### Step 1b: Determine Section Ordering by Eval Weight

```python
# Order management sections by evaluation factor weight.
# Map major management sections to the eval factors they address:
mgmt_section_eval_map = {
    "company_overview": ["experience", "qualification", "past performance", "organizational"],
    "project_management": ["management", "approach", "methodology", "governance"],
    "proposed_team": ["staff", "personnel", "team", "qualification", "key personnel"],
    "ocm": ["training", "change management", "transition", "knowledge transfer"],
    "past_performance": ["experience", "past performance", "reference"],
    "transition": ["transition", "implementation", "schedule"]
}

# For each section, find the highest-weight eval factor it addresses
# and order sections accordingly (highest weight first)
section_weights = {}
for section_key, keywords in mgmt_section_eval_map.items():
    max_weight = 0
    matched_factor = None
    for factor in eval_factors_by_weight:
        factor_text = (factor.get("name", "") + " " + factor.get("description", "")).lower()
        if any(kw in factor_text for kw in keywords):
            w = factor.get("weight_normalized", factor.get("weight", 0))
            if w > max_weight:
                max_weight = w
                matched_factor = factor
    section_weights[section_key] = {
        "weight": max_weight,
        "factor": matched_factor
    }

# Sort sections by weight descending
ordered_sections = sorted(section_weights.items(), key=lambda x: x[1]["weight"], reverse=True)
# This ordering guides the AI to emphasize the highest-weighted sections first
# and allocate proportionally more content to them
```

### Step 2: Generate Management Proposal

**CRITICAL: Evaluation-Driven Content Ordering and Theme Threading**

Before writing, review `section_content_guide` for any management-related sections. Each section below must:
1. Open with an **Evaluation Factor callout box** identifying the factor(s) addressed and their point value
2. Include **>= 2 win themes** per major section, using the format: `**[Theme Name]**: [evidence statement]`
3. Prioritize content by `content_priority_guide` — address highest composite-score requirements first
4. Integrate `evaluator_messages` messaging tailored to the primary evaluator persona for that section

**Evaluator Message Integration:** For each section, identify the most relevant evaluator persona (EXECUTIVE for Company Overview, OPERATIONAL for OCM, RISK for Governance) and weave in that persona's `key_message` and `proof_points` from `evaluator_messages`.

Write comprehensive management proposal with these sections (order sections by evaluation weight where the RFP structure permits):

```markdown
# Management Proposal

## 1. Company Overview

> **Evaluation Factor:** {mgmt_eval_factors[0].get("name", "Experience & Qualifications")}
> ({mgmt_eval_factors[0].get("weight", mgmt_eval_factors[0].get("points", "N/A"))} points)
> This section addresses: organizational capability, relevant experience, corporate stability

### 1.1 About Resource Data, Inc.
[Company history (est. 1986), mission, core philosophy.
{employees} professionals across {len(locations)} offices.
Emphasize stability, longevity, and commitment.
**Theme integration:** Weave in >= 1 win theme with evidence.
**Evaluator message:** Use EXECUTIVE evaluator_message headline and proof points.]

### 1.2 Organizational Structure
[Office locations and leadership. Regional presence.
Show proximity to client if applicable.]

### 1.3 Relevant Capabilities
[Services aligned with RFP requirements.
Map company capabilities to evaluation criteria.
**Content priority:** Address highest-scored requirements from content_priority_guide
that fall under this section's evaluation factor.]

## 2. Project Management Approach

> **Evaluation Factor:** {factor_name_for_management} ({weight} points)
> This section addresses: {subfactors_list}
> **Mandated themes:** {mgmt_mandated_themes_for_this_section}

### 2.1 Methodology
[Project management framework: Agile/Scrum with governance overlays.
Describe ceremonies, cadence, reporting structure.
Tailor to domain (government = more governance, reporting).
**Theme integration:** Invoke >= 1 theme, e.g., "Through our commitment to
**[Theme Name]**, we employ..."
**Evaluator message:** Use RISK evaluator_message to frame governance rigor.]

### 2.2 Governance Structure
[Project governance: steering committee, change control board,
escalation paths, decision authority matrix.]

### 2.3 Communication Plan
[Status reporting cadence, stakeholder communication matrix,
tools and platforms, meeting schedule.]

### 2.4 Quality Management
[Quality gates, peer review process, deliverable acceptance criteria,
continuous improvement mechanisms.
**Theme integration:** Invoke >= 1 theme tied to quality/reliability.]

## 3. Proposed Team

> **Evaluation Factor:** {factor_name_for_staffing} ({weight} points)
> This section addresses: key personnel qualifications, staffing adequacy, team experience

### 3.1 Staffing Plan
[Table: Role | Name | Qualifications | Allocation | Location
Populate from company-profile.json key_personnel.
Use [USER INPUT REQUIRED] for specific names if not populated.]

### 3.2 Key Personnel Qualifications
[For each key role: experience summary, relevant certifications,
past project highlights. Reference domain expertise.
**Theme integration:** Connect each person's qualifications to a win theme.]

### 3.3 Staffing Flexibility
[How Resource Data scales team up/down.
Access to 200+ professionals. Multi-office resource pool.
Subcontractor strategy if applicable.]

## 4. Organizational Change Management

> **Evaluation Factor:** {factor_name_for_ocm_or_training} ({weight} points)
> This section addresses: training approach, knowledge transfer, user adoption

### 4.1 OCM Approach
[Resource Data's OCM methodology. Stakeholder analysis,
communication planning, training strategy, resistance management.
**Evaluator message:** Use OPERATIONAL evaluator_message to frame user-centered approach.]

### 4.2 Training Plan
[User training approach: admin training, end-user training,
train-the-trainer model. Training materials and delivery methods.
**Theme integration:** Invoke >= 1 theme tied to user empowerment/adoption.]

### 4.3 Knowledge Transfer
[Documentation strategy, operational handoff plan,
ensuring client self-sufficiency post-project.]

## 5. Experience and Past Performance

### 5.1 Relevant Experience

**AUTO-POPULATED FROM Past_Projects.md via matched_projects[].**

Read matched_projects from POSITIONING_OUTPUT.json. For the top 3-5 matches,
write a **full case study** for each using the case-study-template format.
For each matched project:

1. Look up the project_number in Past_Projects.md to get full details
2. Write the case study in this format:

**Case Study: [Project Title]**

**Client:** [Client Name] — [Industry]
**Project Duration:** [Timeline from Past_Projects.md]
**Our Role:** Prime Contractor

**Challenge:**
[2-3 sentences from the project description describing the client's challenge.
Frame the challenge to mirror challenges in the current RFP.]

**Our Solution:**
[2-3 sentences describing what Resource Data delivered, with specific
technologies and approaches. Pull from the Technologies section.]

**Results:**
| Metric | Achievement |
|--------|------------|
| [Metric from outcomes] | [Quantified result] |
| [Metric from outcomes] | [Quantified result] |
| [Metric from outcomes] | [Quantified result] |

**Relevance to This RFP:**
[1-2 sentences connecting this project's outcomes to specific RFP evaluation
criteria. Reference the matched project's relevance_statement and map to
evaluation factors from EVALUATION_CRITERIA.json.]

**Reference Available:** Yes — Contact upon request

**IMPORTANT:** Do NOT use `[CASE STUDY PLACEHOLDER]` markers. Always write the
full case study from Past_Projects.md data. If matched_projects has fewer than
3 entries, use all available matches and note that additional references are
available upon request.

### 5.2 References

| Reference | Organization | Industry | Services Provided |
|-----------|-------------|----------|-------------------|
[For each of the top 3-5 matched projects, create a reference row using
the client name and industry from Past_Projects.md. Note that specific
contact details (name, phone, email) require user confirmation before
submission — mark with [VERIFY: Contact details for {client}].]

Additional known clients from Past_Projects.md are available upon request.

## 6. Transition Plan

### 6.1 Project Initiation
[First 30-60-90 day plan. Kickoff, discovery, baseline establishment.]

### 6.2 Knowledge Acquisition
[How team ramps up on client systems, processes, data.]

### 6.3 Transition-Out Plan
[End of contract transition: documentation, knowledge transfer,
data migration, system handoff procedures.]
```

### Step 3: Thread Win Themes and Evaluation Alignment

```python
# MANDATORY: Verify theme and eval alignment before finalizing

# 1. Insert evaluation factor callout boxes at each major section header
#    Format: > **Evaluation Factor**: {factor_name} ({weight} points)
#    Pull from eval_factors_by_weight and match to section topic

# 2. Verify win theme threading: each major section (1-6) must reference
#    at least 2 win themes with explicit callout format:
#    **[Theme Name]**: [evidence statement connecting to this section]

# 3. Check section_theme_mandates for any themes REQUIRED in management sections
#    These are non-negotiable — if a theme is mandated, it MUST appear

# 4. Integrate evaluator_messages for persona-tailored content:
#    - Section 1 (Company Overview): EXECUTIVE persona
#    - Section 2 (PM Approach): RISK persona
#    - Section 3 (Team): TECHNICAL persona
#    - Section 4 (OCM): OPERATIONAL persona
#    - Section 5 (Past Performance): EXECUTIVE + RISK personas
#    - Section 6 (Transition): OPERATIONAL + RISK personas

# 5. Use content_priority_guide to ensure highest-impact requirements
#    are addressed first within each section

theme_coverage = {}
for theme in win_themes:
    t = theme if isinstance(theme, str) else theme.get("theme", "")
    theme_coverage[t] = 0  # Count references per theme

# After writing, verify:
# - Every theme appears >= 2 times across the document
# - Mandated themes appear in their mandated sections
# - Each section has an eval factor callout box
```

### Step 4: Write Output

```python
write_file(f"{folder}/outputs/bid-sections/02_MANAGEMENT.md", management_content)

size_kb = len(management_content) / 1024
user_input_markers = management_content.count("[USER INPUT REQUIRED")

case_study_count = min(len(matched_projects), 5)

# Count eval factor callout boxes and win theme references
eval_callout_count = management_content.count("**Evaluation Factor:**")
theme_ref_count = sum(
    management_content.count(f"**{t if isinstance(t, str) else t.get('theme', '')}**")
    for t in win_themes
)

log(f"""
👥 MANAGEMENT PROPOSAL COMPLETE (Phase 8.2)
=============================================
Size: {size_kb:.1f} KB
Sections: 6 major sections (ordered by eval weight)
Team Members: {len(company.get("key_personnel", {}).get("team_members", []))}
Case Studies (auto-populated): {case_study_count} from Past_Projects.md
Top Match: {matched_projects[0]['client'] if matched_projects else 'None'} (score: {matched_projects[0]['relevance_score'] if matched_projects else 'N/A'})
User Input Markers: {user_input_markers}
Eval Factor Callout Boxes: {eval_callout_count}
Win Theme References: {theme_ref_count}
Mandated Themes: {len(mgmt_mandated_themes)} required, verified present
Evaluator Personas Integrated: {len([p for p in evaluator_messages if p.upper() in ['EXECUTIVE', 'TECHNICAL_EVALUATOR', 'PROGRAM_MANAGER', 'RISK', 'OPERATIONAL']])}

Output: outputs/bid-sections/02_MANAGEMENT.md
""")
```

## Quality Checklist

- [ ] `02_MANAGEMENT.md` created (>10KB)
- [ ] Company overview populated from company-profile.json
- [ ] Project management methodology described with governance
- [ ] Staffing plan with key personnel (or USER INPUT markers)
- [ ] OCM approach with training plan
- [ ] Section 5 auto-populated with 3-5 real case studies from Past_Projects.md
- [ ] Each case study has: Client, Duration, Challenge, Solution, Results table, Relevance statement
- [ ] No `[CASE STUDY PLACEHOLDER]` or `[USER INPUT REQUIRED]` in Section 5 (except contact verification)
- [ ] References table populated from matched project clients
- [ ] Transition plan (in and out)
- [ ] Win themes threaded throughout (>= 2 themes per major section)
- [ ] Evaluation factor callout box at every major section header (Sections 1-6)
- [ ] Sections ordered by evaluation weight where RFP structure permits
- [ ] `section_theme_mandates` checked — all mandated themes present in required sections
- [ ] `evaluator_messages` integrated: EXECUTIVE (S1), RISK (S2), TECHNICAL (S3), OPERATIONAL (S4)
- [ ] `content_priority_guide` used to prioritize highest-impact requirements first
- [ ] Theme coverage verified: every theme appears >= 2 times across document
