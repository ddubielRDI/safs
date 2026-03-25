---
name: phase6-report
expert-role: Publication Specialist
domain-expertise: Document layout, DOCX generation, professional formatting
skill: publication-specialist
---

# Phase 6: Report Generation (DOCX)

**Purpose:** Generate a professional BID_SCREEN.docx from consolidated data using python-docx (~5-7 pages, three-tier structure). Also writes BID_SCREEN.md as a secondary reference output. This is the final human-readable deliverable.

**Inputs:**
- `{folder}/screen/BID_SCREEN.json` -- Consolidated data from Phase 5

**Required Outputs:**
- `{folder}/screen/BID_SCREEN.docx` (>30KB) -- Final DOCX report (primary deliverable)
- `{folder}/screen/BID_SCREEN.md` (>5KB) -- Markdown source (secondary reference)

---

## Instructions

### Step 1: Load Consolidated Data

```python
bid_screen = read_json(f"{folder}/screen/BID_SCREEN.json")
rfp = bid_screen["rfp_summary"]
gonogo = bid_screen["go_nogo_score"]
intel = bid_screen.get("client_intel", {})
compliance = bid_screen["compliance"]
projects = bid_screen["past_projects"]
risk = bid_screen["risk_assessment"]
themes = bid_screen.get("preliminary_themes", {})

# Phase 5.5 output (clarifying questions) -- loaded separately since it updates BID_SCREEN.json after Phase 5
clarifying_qs = bid_screen.get("clarifying_questions", {})
if not clarifying_qs:
    # Fallback: read directly from file if BID_SCREEN.json wasn't updated
    clarifying_qs = read_json_safe(f"{folder}/screen/clarifying-questions.json") or {}
```

### Step 2: Build BID_SCREEN.md

Build the markdown document section by section. All sections must be populated where data exists (Intel section omitted if --quick mode, themes section omitted if no themes generated, Evaluation Score Potential and Technology Intelligence omitted if intelligence layers absent).

```python
from datetime import datetime

recommendation = bid_screen["recommendation"]
total_score = bid_screen["total_score"]

# Tone-adapted report subtitle based on client communication style
client_tone = bid_screen.get("rfp_summary", {}).get("client_tone", {})
primary_style = client_tone.get("primary_style", "formal_bureaucratic")

if primary_style == "outcomes_focused":
    report_subtitle = "Opportunity Assessment -- Value Delivery Analysis"
elif primary_style == "innovation_driven":
    report_subtitle = "Opportunity Assessment -- Innovation & Transformation Potential"
elif primary_style == "compliance_heavy":
    report_subtitle = "Opportunity Assessment -- Compliance & Risk Readiness"
elif primary_style == "mission_driven":
    report_subtitle = "Opportunity Assessment -- Mission Alignment & Impact"
else:
    report_subtitle = "Opportunity Assessment"

# Badge styling via text (no CSS needed -- plain markdown)
badge = {
    "GO": "GO -- Proceed to Full Pipeline",
    "CONDITIONAL": "CONDITIONAL -- Review Risks Before Committing",
    "NO_GO": "NO-GO -- Do Not Bid"
}

md = f"""# RFP Bid Screening Report

*{report_subtitle}*

**Generated:** {datetime.now().strftime("%B %d, %Y at %I:%M %p")}
**Mode:** {"Quick" if bid_screen.get("screening_mode") == "quick" else "Full"}

---

## Recommendation

### {badge.get(recommendation, recommendation)}

**Score: {total_score}/100**

{bid_screen.get("rationale", "")}

---

## Quick Facts

| Field | Value |
|-------|-------|
| **Client** | {rfp.get("client_name", "Not identified")} |
| **RFP Number** | {rfp.get("rfp_number", "Not found")} |
| **Title** | {rfp.get("rfp_title", "Not found")} |
| **Deadline** | {rfp.get("submission_deadline", "Not found")} |
| **Estimated Value** | {rfp.get("estimated_value", "Not disclosed")} |
| **Contract Type** | {rfp.get("contract_type", "Not specified")} |
| **Domain** | {rfp.get("industry_domain", "Not classified")} |
| **Set-Aside** | {rfp.get("set_aside", "None")} |

---

## Go/No-Go Scorecard

| Area | Weight | Score | Weighted | Rating |
|------|--------|-------|----------|--------|
"""

# Build scorecard rows from assessment areas
areas = gonogo.get("assessment_areas", [])
for area in areas:
    name = area.get("name", "")
    weight = area.get("weight", 0)
    score = area.get("score", 0)
    weighted = score * weight
    weight_pct = f"{weight*100:.0f}%"
    rating = "Strong" if score >= 70 else "Moderate" if score >= 50 else "Weak"
    md += f"| **{name}** | {weight_pct} | {score} | {weighted:.1f} | {rating} |\n"

md += f"| **TOTAL** | **100%** | | **{total_score}** | **{recommendation}** |\n"

# Assessment Area Details
md += "\n### Assessment Area Details\n\n"
for area in areas:
    name = area.get("name", "")
    score = area.get("score", 0)
    weight_pct = f"{area.get('weight', 0)*100:.0f}%"
    rationale = area.get("rationale", "No rationale provided.")
    md += f"**{name} ({score}/100, weight: {weight_pct}):** {rationale}\n\n"

    # Show key evidence and risks
    evidence = area.get("evidence", [])
    if evidence:
        md += "Key evidence:\n"
        for e in evidence[:3]:
            md += f"- {e}\n"
        md += "\n"

    risks = area.get("risks", [])
    if risks:
        md += "Risks:\n"
        for r in risks[:2]:
            md += f"- {r}\n"
        md += "\n"

md += "\n---\n\n"

# Evaluation Score Potential (from evaluation_model intelligence layer)
eval_model = rfp.get("evaluation_model", {})
if eval_model and eval_model.get("point_allocation"):
    md += "## Evaluation Score Potential\n\n"
    md += f"**Evaluation Method:** {eval_model.get('evaluation_method_implications', 'Not specified')}\n"
    md += f"**Evaluator Profile:** {eval_model.get('evaluator_persona', 'Unknown').replace('_', ' ').title()}\n"
    md += f"**Technical-to-Price Ratio:** {eval_model.get('technical_to_price_ratio', 'Unknown')}\n\n"

    md += "| Criterion | Weight | Est. Points | Discriminator Potential |\n"
    md += "|-----------|--------|-------------|------------------------|\n"
    for criterion in eval_model["point_allocation"]:
        name = criterion.get("criterion", "")
        pct = criterion.get("pct", "")
        points = criterion.get("points", "")
        disc = criterion.get("discriminator_potential", "").title()
        md += f"| {name} | {pct} | {points} | {disc} |\n"
    md += "\n"

    md += "\n---\n\n"
```

#### Section: Buyer Priorities

```python
# Include if buyer priorities were extracted in Phase 1
buyer_priorities = rfp.get("buyer_priorities", [])
if buyer_priorities:
    md += "## Buyer Priorities\n\n"
    md += "*Decision drivers identified from RFP repetition, emphasis, evaluation weight, "
    md += "and structural patterns (Shipley hot buttons / APMP Customer Intimacy).*\n\n"

    md += "| Priority | Importance | Evaluation Criterion | Coverage |\n"
    md += "|----------|------------|---------------------|----------|\n"

    # Determine coverage from go-nogo and theme data
    gonogo_coverage = gonogo.get("buyer_priority_coverage", {})
    theme_coverage = themes.get("buyer_priority_coverage", {}) if themes else {}
    addressed_list = gonogo_coverage.get("high_addressed_list", [])
    theme_covered = theme_coverage.get("covered", [])

    for bp in buyer_priorities:
        name = bp.get("name", "Unknown")
        importance = bp.get("importance", "?")
        eval_crit = bp.get("evaluation_criterion", "N/A")

        # Determine coverage status
        if name in addressed_list and name in theme_covered:
            coverage = "STRONG"
        elif name in addressed_list or name in theme_covered:
            coverage = "PARTIAL"
        elif importance == "HIGH":
            coverage = "GAP"
        else:
            coverage = "--"

        md += f"| **{name}** | {importance} | {eval_crit} | {coverage} |\n"

    md += "\n"

    # Show signal evidence for each priority
    md += "### Priority Details\n\n"
    for bp in buyer_priorities:
        name = bp.get("name", "Unknown")
        signal = bp.get("signal", "No signal detected.")
        linked = bp.get("linked_scope_keywords", [])
        md += f"**{name}** ({bp.get('importance', '?')})\n"
        md += f"*{signal}*\n"
        if linked:
            md += f"Linked keywords: {', '.join(linked)}\n"
        md += "\n"

    md += "\n---\n\n"

# Technology Intelligence (from tech_intelligence layer)
tech_intel = rfp.get("tech_intelligence", {})
if tech_intel and tech_intel.get("technology_stacks"):
    md += "## Technology Intelligence\n\n"

    # Stack overview
    for stack in tech_intel.get("technology_stacks", []):
        md += f"**{stack['stack_name']}** (Coherence: {stack.get('coherence', 'N/A')})\n"
        for comp in stack.get("components", []):
            version_str = f" v{comp['version']}" if comp.get("version") else ""
            md += f"- {comp['name']}{version_str} -- {comp.get('role', '')} ({comp.get('maturity', '')})\n"
        md += "\n"

    # RDI Alignment summary
    alignment = tech_intel.get("rdi_alignment", {})
    if alignment:
        md += "### RDI Technology Alignment\n\n"
        ratio = alignment.get('coverage_ratio', 0)
        pct = f"{int(ratio * 100)}%" if isinstance(ratio, (int, float)) else str(ratio)
        md += f"**RDI Technology Coverage:** {pct} of RFP-required technologies matched to documented RDI experience\n\n"
        if alignment.get("strong_match"):
            md += f"**Strong Match:** {', '.join(alignment['strong_match'][:6])}\n"
        if alignment.get("no_match"):
            md += f"**Gaps:** {', '.join(alignment['no_match'][:4])}\n"
        md += "\n"

    # Maturity Profile table
    maturity = tech_intel.get("maturity_profile", {})
    if maturity:
        md += "### Technology Maturity Profile\n\n"
        md += "| Maturity | Count |\n|----------|-------|\n"
        for level in ["established", "mature", "emerging", "declining"]:
            count = maturity.get(level, 0)
            if count > 0:
                md += f"| {level.title()} | {count} |\n"
        md += "\n"

    # Risk flags
    risk_flags = tech_intel.get("technology_risk_flags", [])
    if risk_flags:
        md += "### Technology Risk Flags\n\n"
        for flag in risk_flags:
            md += f"- {flag}\n"
        md += "\n"

    md += "\n---\n\n"
```

#### Section: Client Intelligence (conditional)

```python
# Only include if not --quick mode and data exists
if intel and intel.get("status") == "complete":
    intelligence = intel.get("intelligence", {})

    md += "## Client Intelligence\n\n"

    # Organization Profile -- populated by Phase 3 Category A search
    org = intelligence.get("organization_profile", {})
    if org and org.get("name"):
        md += "### Organization Profile\n\n"
        md += f"**Name:** {org.get('name', 'Unknown')}\n"
        md += f"**Industry:** {org.get('industry', 'Unknown')}\n"
        md += f"**Size:** {org.get('size', 'Unknown')}\n"
        md += f"**Headquarters:** {org.get('headquarters', 'Unknown')}\n"
        if org.get("governance"):
            md += f"**Governance:** {org.get('governance')}\n"
        if org.get("demographics"):
            md += f"**Demographics:** {org.get('demographics')}\n"
        if org.get("budget"):
            md += f"**Budget:** {org.get('budget')}\n"
        md += "\n"

    # Recent News
    news = intelligence.get("news", [])
    if news:
        md += "### Recent News\n\n"
        md += "| Date | Headline | Source |\n"
        md += "|------|----------|--------|\n"
        for item in news[:5]:
            if isinstance(item, dict):
                md += f"| {item.get('date', 'N/A')} | {item.get('headline', 'N/A')} | {item.get('source', 'N/A')} |\n"
            else:
                md += f"| N/A | {item} | N/A |\n"
        md += "\n"

    # Key Contacts / Leadership
    leadership = intelligence.get("leadership", [])
    if leadership:
        md += "### Key Contacts\n\n"
        for person in leadership[:5]:
            if isinstance(person, dict):
                md += f"- **{person.get('name', 'Unknown')}** -- {person.get('title', 'Unknown')}"
                if person.get("note"):
                    md += f" ({person.get('note')})"
                md += "\n"
            else:
                md += f"- {person}\n"
        md += "\n"

    # Technology Environment
    tech_stack = intelligence.get("technology_stack", [])
    if tech_stack:
        md += "### Technology Environment\n\n"
        for tech in tech_stack:
            if isinstance(tech, dict):
                md += f"- **{tech.get('technology', 'Unknown')}** ({tech.get('category', '')}) -- {tech.get('note', '')}\n"
            else:
                md += f"- {tech}\n"
        md += "\n"

    # Competitive Landscape -- populated by Phase 3 Category E search
    competitive = intelligence.get("competitive_landscape", {})
    if competitive and (competitive.get("incumbent") or competitive.get("known_competitors")):
        md += "### Competitive Landscape\n\n"
        md += f"**Incumbent:** {competitive.get('incumbent', 'Unknown')}\n"
        known = competitive.get("known_competitors", [])
        if known:
            md += f"**Known Competitors:** {', '.join(known)}\n"
        notes = competitive.get("notes", "")
        if notes:
            md += f"\n*{notes}*\n"
        md += "\n"

    md += "\n---\n\n"
```

#### Section: Compliance Quick-Check

```python
md += "## Compliance Quick-Check\n\n"

# Contract vehicles and existing relationships (enriched from Past_Projects.md)
contract_info = compliance.get("contract_vehicles", {})
matching_vehicles = contract_info.get("matching_rfp", [])
if matching_vehicles:
    md += f"**Existing Contract Vehicles:** {'; '.join(matching_vehicles)}\n\n"

existing_rel = compliance.get("existing_relationship", {})
if existing_rel.get("found"):
    md += f"**Existing Client Relationship:** {existing_rel.get('matched_client', 'Identified')}\n\n"

comp_partnerships = compliance.get("partnerships", [])
if comp_partnerships:
    md += f"**Technology Partnerships:** {'; '.join(comp_partnerships)}\n\n"

md += "| Requirement | Category | Status |\n"
md += "|-------------|----------|--------|\n"

compliance_items = compliance.get("compliance_items", [])
pass_count = 0
gap_count = 0
risk_count = 0

for item in compliance_items:
    status = item.get("status", "UNKNOWN")
    req = item.get("requirement", "")[:80]
    cat = item.get("category", "General")
    md += f"| {req} | {cat} | {status} |\n"

    if status == "PASS":
        pass_count += 1
    elif status == "GAP":
        gap_count += 1
    elif status == "RISK":
        risk_count += 1

md += f"\n**Summary:** {pass_count} PASS | {gap_count} GAP | {risk_count} RISK\n\n"
md += "\n---\n\n"
```

#### Section: Past Project Matches

```python
md += "## Past Project Matches\n\n"

matched = projects.get("matched_projects", [])

if matched:
    md += "### Top 5 Matches\n\n"
    md += "| Rank | Project | Industry | Score | Key Overlap |\n"
    md += "|------|---------|----------|-------|-------------|\n"

    for i, proj in enumerate(matched[:5], 1):
        title = proj.get("title", "Untitled")
        industry = proj.get("industry", "N/A")
        score = proj.get("relevance_score", 0)
        tech_matches = ", ".join(proj.get("technology_matches", [])[:3])
        md += f"| {i} | {title} | {industry} | {score} | {tech_matches} |\n"

    md += "\n### Detailed Match Analysis\n\n"

    for i, proj in enumerate(matched[:3], 1):
        md += f"#### {i}. {proj.get('title', 'Untitled')}\n\n"
        md += f"**Client:** {proj.get('client', 'N/A')}\n"
        md += f"**Industry:** {proj.get('industry', 'N/A')}\n"
        md += f"**Relevance Score:** {proj.get('relevance_score', 0)} ({proj.get('relevance_rating', 'N/A')})\n"

        if proj.get("team_size"):
            md += f"**Team Size:** {proj.get('team_size')}\n"
        if proj.get("timeline"):
            md += f"**Timeline:** {proj.get('timeline')}\n"

        tech = proj.get("technology_matches", []) or proj.get("technologies", [])
        if tech:
            md += f"**Technologies:** {', '.join(tech[:6])}\n"

        # Key outcomes (enriched from Past_Projects.md)
        outcomes = proj.get("key_outcomes", [])
        if outcomes:
            md += "**Key Outcomes:**\n"
            for outcome in outcomes[:3]:
                md += f"- {outcome}\n"

        metrics = proj.get("key_metrics", [])
        if metrics and not outcomes:
            md += f"**Key Metrics:** {'; '.join(metrics[:3])}\n"

        # Client quote (enriched from Past_Projects.md)
        quote = proj.get("quote_text")
        attribution = proj.get("quote_attribution")
        if quote:
            md += f"\n*\"{quote}\"*"
            if attribution:
                md += f" -- {attribution}"
            md += "\n"

        relevance = proj.get("relevance_statement", "")
        if relevance:
            md += f"\n**Relevance:** {relevance}\n"

        md += "\n"
else:
    md += "*No matching past projects found.*\n\n"

md += "\n---\n\n"
```

#### Section: Preliminary Win Themes

```python
# Include if themes were generated (Phase 4.5)
theme_list = themes.get("themes", [])
if theme_list:
    md += "## Preliminary Win Themes\n\n"
    md += "*Directional positioning hints from screening data. Full pipeline generates "
    md += "production themes with evaluation factor mapping.*\n\n"

    # Check if themes have skill-enriched fields
    has_enriched_themes = any(t.get("discriminator_type") for t in theme_list)

    if has_enriched_themes:
        md += "| # | Theme | Category | Type | Maturity | Confidence |\n"
        md += "|---|-------|----------|------|----------|------------|\n"
        for t in theme_list:
            rank = t.get("rank", "")
            name = t.get("name", "Unnamed")
            category = t.get("category", "").replace("_", " ").title()
            disc_type = t.get("discriminator_type", "")
            maturity = t.get("maturity_level", "")
            confidence = t.get("confidence", "unknown").upper()
            md += f"| {rank} | **{name}** | {category} | {disc_type} | {maturity} | {confidence} |\n"
    else:
        md += "| # | Theme | Category | Confidence | Key Evidence |\n"
        md += "|---|-------|----------|------------|---------------|\n"
        for t in theme_list:
            rank = t.get("rank", "")
            name = t.get("name", "Unnamed")
            category = t.get("category", "").replace("_", " ").title()
            confidence = t.get("confidence", "unknown").upper()
            evidence_summary = "; ".join(t.get("evidence", [])[:2])[:120]
            md += f"| {rank} | **{name}** | {category} | {confidence} | {evidence_summary} |\n"

    md += "\n### Theme Details\n\n"
    for t in theme_list:
        rank = t.get("rank", "")
        name = t.get("name", "Unnamed")
        rationale = t.get("rationale", "No rationale provided.")
        evidence = t.get("evidence", [])

        md += f"**{rank}. {name}**\n"
        framing = t.get("framing", "")
        if framing:
            md += f"\n{framing}\n\n"
        md += f"*{rationale}*\n"

        # Skill-enriched fields (from competitive-positioning sub-skill)
        disc_type = t.get("discriminator_type")
        maturity = t.get("maturity_level")
        ghost = t.get("ghost_element")
        if disc_type or maturity:
            md += f"\n**Discriminator:** {disc_type or 'N/A'} | **Maturity:** {maturity or 'N/A'}\n"
        if ghost:
            md += f"**Competitive Edge:** {ghost}\n"

        # Evaluation alignment (from evaluation_model intelligence layer)
        eval_align = t.get("evaluation_alignment", {})
        if eval_align.get("mapped_criteria"):
            md += f"\n**Evaluation Impact:** ~{eval_align.get('estimated_point_contribution', 0)} estimated points "
            md += f"({eval_align.get('point_contribution_pct', 'N/A')})\n"
            criteria_names = [c["criterion"] for c in eval_align["mapped_criteria"][:3]]
            md += f"Mapped to: {', '.join(criteria_names)}\n"

        # Technology gaps linked to this theme
        tech_gaps = t.get("tech_gaps_to_address", [])
        if tech_gaps:
            md += f"**Technology Gaps to Address:** {', '.join(tech_gaps)}\n"

        if evidence:
            md += "Supporting evidence:\n"
            for e in evidence:
                md += f"- {e}\n"
        md += "\n"

    md += "\n---\n\n"
elif themes.get("status") != "not_generated":
    md += "## Preliminary Win Themes\n\n"
    md += "*No preliminary themes generated for this screening.*\n\n"
    md += "\n---\n\n"
```

#### Section: Clarifying Questions for Client

```python
# Include if clarifying questions were generated (Phase 5.5)
cq_questions = clarifying_qs.get("questions", [])
if cq_questions:
    md += "## Clarifying Questions for Client\n\n"
    md += "*Questions identified from screening analysis that would strengthen the "
    md += "proposal approach. Prioritized by impact on bid decision and technical strategy.*\n\n"

    # Questions deadline callout (only if extracted)
    cq_deadline = clarifying_qs.get("questions_deadline", "Not found")
    if cq_deadline and cq_deadline not in ("Not found", "Not specified", "N/A", "Unknown"):
        # Calculate days remaining if possible
        from datetime import datetime
        deadline_note = ""
        try:
            # Try common date formats
            for fmt in ("%Y-%m-%d", "%m/%d/%Y", "%B %d, %Y", "%b %d, %Y"):
                try:
                    deadline_dt = datetime.strptime(cq_deadline.strip(), fmt)
                    days_remaining = (deadline_dt - datetime.now()).days
                    if days_remaining < 0:
                        deadline_note = " -- PASSED"
                    elif days_remaining == 0:
                        deadline_note = " -- TODAY"
                    else:
                        deadline_note = f" -- {days_remaining} days remaining"
                    break
                except ValueError:
                    continue
        except Exception:
            pass
        md += f"**Questions Deadline: {cq_deadline}{deadline_note}**\n\n"

    # Summary line
    cq_summary = clarifying_qs.get("summary", {})
    by_priority = cq_summary.get("by_priority", {})
    high_ct = by_priority.get("HIGH", 0)
    med_ct = by_priority.get("MEDIUM", 0)
    low_ct = by_priority.get("LOW", 0)
    total_ct = cq_summary.get("total_questions", len(cq_questions))
    md += f"**{total_ct} Questions:** {high_ct} Critical | {med_ct} Important | {low_ct} Advisory\n\n"

    md += "\n### Question Details\n\n"

    # Group by priority for detailed display
    priority_labels = {
        "HIGH": "Critical Priority",
        "MEDIUM": "Important Priority",
        "LOW": "Advisory Priority"
    }

    for priority_level, label in priority_labels.items():
        priority_questions = [q for q in cq_questions if q.get("priority") == priority_level]
        if not priority_questions:
            continue

        md += f"#### {label}\n\n"
        for q in priority_questions:
            qid = q.get("id", "")
            q_text = q.get("question", "")
            rfp_ref = q.get("rfp_reference", "")
            impact = q.get("impact", "")
            related = q.get("related_finding", "")

            md += f"**{qid}.** {q_text}\n\n"
            if rfp_ref:
                md += f"- *Unclear RFP Language:* {rfp_ref}\n"
            if impact:
                md += f"- *Why Asked:* {impact}\n"
            if related:
                md += f"- *Screening Finding:* {related}\n"
            eval_target = q.get("evaluation_criterion_targeted")
            if eval_target:
                md += f"- *Evaluation Criterion:* {eval_target}\n"
            md += "\n"

    md += "\n---\n\n"
```

#### Section: Risks and Opportunities

```python
md += "## Risks and Opportunities\n\n"

# Risk Assessment
md += "### Risk Assessment\n\n"
risks = risk.get("risks", [])

# Render severity distribution if available (skill-enriched from Phase 5)
severity_dist = risk.get("severity_distribution", {})
if severity_dist:
    md += f"**Risk Profile:** {severity_dist.get('critical', 0)} Critical | "
    md += f"{severity_dist.get('high', 0)} High | "
    md += f"{severity_dist.get('moderate', 0)} Moderate | "
    md += f"{severity_dist.get('low', 0)} Low\n\n"

# Render risk correlations if present
risk_correlations = risk.get("risk_correlations", [])
if risk_correlations:
    md += "**Correlated Risks:** " + "; ".join(risk_correlations) + "\n\n"

if risks:
    # Use enriched table format if skill-informed fields are present
    has_enriched = any(r.get("severity_band") for r in risks)
    if has_enriched:
        md += "| # | Risk | Category | L | I | Severity | Response |\n"
        md += "|---|------|----------|---|---|----------|----------|\n"
        for i, r in enumerate(risks, 1):
            risk_desc = r.get("description_if_then", r.get("risk", ""))[:100]
            risk_cat = r.get("risk_category", r.get("category", "general"))
            likelihood = r.get("likelihood", "")
            impact = r.get("impact", "")
            severity_band = r.get("severity_band", r.get("severity", "unknown").upper())
            response = r.get("response_strategy", "")
            md += f"| {i} | {risk_desc} | {risk_cat} | {likelihood} | {impact} | {severity_band} | {response} |\n"
    else:
        md += "| # | Risk | Severity | Category |\n"
        md += "|---|------|----------|----------|\n"
        for i, r in enumerate(risks, 1):
            md += f"| {i} | {r.get('risk', '')} | {r.get('severity', 'unknown').upper()} | {r.get('category', 'general')} |\n"
    md += "\n"
else:
    md += "*No significant risks identified.*\n\n"

# Dealbreaker assessment
has_dealbreaker = risk.get("has_dealbreaker", False)
dealbreakers = risk.get("dealbreakers", [])
if has_dealbreaker:
    md += f"**Dealbreaker Assessment:** {len(dealbreakers)} potential dealbreaker(s) identified:\n"
    for db in dealbreakers:
        md += f"- {db}\n"
    md += "\n"
else:
    md += "**Dealbreaker Assessment:** No dealbreakers identified.\n\n"

# Competitive Advantages
md += "### Competitive Advantages\n\n"
opportunities = risk.get("opportunities", [])
if opportunities:
    for opp in opportunities:
        md += f"- {opp.get('opportunity', '')}\n"
    md += "\n"
else:
    md += "*No specific competitive advantages identified.*\n\n"

# Historical context
md += "### Historical Bid Context\n\n"
hist = risk.get("historical_context", {})
if hist.get("has_data"):
    md += f"**Total Past Bids:** {hist.get('total_bids', 0)}\n"
    win_rate = hist.get("overall_win_rate", 0)
    md += f"**Overall Win Rate:** {win_rate:.0%}\n"
    domain_rate = hist.get("domain_win_rate")
    if domain_rate is not None:
        md += f"**Domain Win Rate:** {domain_rate:.0%} ({hist.get('domain_bids', 0)} bids)\n"
    md += f"\n*{hist.get('advisory', '')}*\n\n"
else:
    md += "*No historical bid data available. Consider logging outcomes in bid-outcomes.json for future pattern analysis.*\n\n"

md += "\n---\n\n"
```

#### Section: Final Recommendation

```python
md += "## Final Recommendation\n\n"
md += f"### {recommendation}\n\n"
md += f"**Score:** {total_score}/100\n"
md += f"**Rationale:** {bid_screen.get('rationale', '')}\n\n"

md += "### Next Steps\n\n"
next_steps = bid_screen.get("next_steps", [])
for step in next_steps:
    md += f"- {step}\n"

md += "\n---\n\n"
md += "*Report generated by RFP Screening Pipeline. This is an automated assessment -- human judgment should inform the final bid/no-bid decision.*\n"
```

### Step 3: Write BID_SCREEN.md

```python
write_file(f"{folder}/screen/BID_SCREEN.md", md)
```

### Step 4: Generate BID_SCREEN.docx

Three-tier document structure: Executive Brief (page 1, scannable in 2 minutes), Detailed Analysis (pages 2-5), Appendix (reference only).

```python
from docx import Document
from docx.shared import Pt, Inches, RGBColor, Emu
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_TABLE_ALIGNMENT
from docx.oxml.ns import qn
from docx.oxml import OxmlElement
import os

# --- Color constants ---
NAVY = RGBColor(0x00, 0x33, 0x66)
GREEN = RGBColor(0x00, 0x80, 0x00)
AMBER = RGBColor(0xC8, 0x96, 0x00)
RED = RGBColor(0xB4, 0x00, 0x00)
GRAY = RGBColor(0x64, 0x64, 0x64)
BLACK = RGBColor(0x1A, 0x1A, 0x1A)
WHITE = RGBColor(0xFF, 0xFF, 0xFF)

def recommendation_color(rec):
    """Return color for GO/CONDITIONAL/NO_GO."""
    if rec == "GO":
        return GREEN
    elif rec == "CONDITIONAL":
        return AMBER
    else:
        return RED

def scorecard_rating_color(score):
    """Return color for scorecard rating: green 75+, amber 60-74, red <60."""
    if score >= 75:
        return GREEN
    elif score >= 60:
        return AMBER
    else:
        return RED

def scorecard_rating_label(score):
    """Return rating label for scorecard: Exceptional/Strong/Adequate/Weak."""
    if score >= 90:
        return "Exceptional"
    elif score >= 75:
        return "Strong"
    elif score >= 60:
        return "Adequate"
    else:
        return "Weak"

def status_color(status):
    """Return color for PASS/GAP/RISK."""
    if status == "PASS":
        return GREEN
    elif status == "GAP":
        return RED
    elif status == "RISK":
        return AMBER
    else:
        return BLACK

def add_styled_heading(doc, text, level=1):
    """Add a heading with navy color and Calibri font."""
    sizes = {1: Pt(18), 2: Pt(13), 3: Pt(11)}
    h = doc.add_heading(text, level=level)
    for run in h.runs:
        run.font.color.rgb = NAVY
        run.font.name = "Calibri"
        run.font.size = sizes.get(level, Pt(11))
    return h

def add_body_paragraph(doc, text="", bold=False, italic=False, color=None, size=Pt(10)):
    """Add a body paragraph with standard formatting."""
    p = doc.add_paragraph()
    run = p.add_run(text)
    run.font.name = "Calibri"
    run.font.size = size
    run.bold = bold
    run.italic = italic
    if color:
        run.font.color.rgb = color
    return p

def add_key_value(doc, key, value, value_color=None, size=Pt(10)):
    """Add a 'Key: Value' paragraph with bold key and optional value color."""
    p = doc.add_paragraph()
    k = p.add_run(f"{key}: ")
    k.font.name = "Calibri"
    k.font.size = size
    k.bold = True
    k.font.color.rgb = NAVY
    v = p.add_run(str(value))
    v.font.name = "Calibri"
    v.font.size = size
    if value_color:
        v.font.color.rgb = value_color
    return p

def add_so_what(doc, text):
    """Add a 'So What' line: thin separator + italic navy text."""
    # Thin line separator via a gray paragraph
    sep = doc.add_paragraph()
    sep_run = sep.add_run("_" * 60)
    sep_run.font.name = "Calibri"
    sep_run.font.size = Pt(6)
    sep_run.font.color.rgb = GRAY
    # So What text
    p = doc.add_paragraph()
    label = p.add_run("So What: ")
    label.font.name = "Calibri"
    label.font.size = Pt(10)
    label.bold = True
    label.italic = True
    label.font.color.rgb = NAVY
    content = p.add_run(text)
    content.font.name = "Calibri"
    content.font.size = Pt(10)
    content.italic = True
    content.font.color.rgb = NAVY
    return p

def set_cell_text(cell, text, bold=False, color=None, size=Pt(9)):
    """Set cell text with formatting."""
    cell.text = ""
    p = cell.paragraphs[0]
    run = p.add_run(str(text))
    run.font.name = "Calibri"
    run.font.size = size
    run.bold = bold
    if color:
        run.font.color.rgb = color

def add_table_from_data(doc, headers, rows, style="Light Grid Accent 1"):
    """Add a formatted table. headers = list of strings, rows = list of lists."""
    table = doc.add_table(rows=1, cols=len(headers), style=style)
    table.alignment = WD_TABLE_ALIGNMENT.LEFT
    # Header row
    for i, h in enumerate(headers):
        set_cell_text(table.rows[0].cells[i], h, bold=True, color=NAVY, size=Pt(9))
    # Data rows
    for row_data in rows:
        row = table.add_row()
        for i, val in enumerate(row_data):
            if isinstance(val, tuple):
                # (text, color) tuple for colored cells
                set_cell_text(row.cells[i], val[0], color=val[1], size=Pt(9))
            else:
                set_cell_text(row.cells[i], str(val), size=Pt(9))
    return table

def add_bullet(doc, text, color=None, size=Pt(9)):
    """Add a bullet point with standard formatting."""
    bp = doc.add_paragraph(style="List Bullet")
    run = bp.add_run(text)
    run.font.name = "Calibri"
    run.font.size = size
    if color:
        run.font.color.rgb = color
    return bp

# === Build the Document ===
doc = Document()

# Set default font
style = doc.styles["Normal"]
font = style.font
font.name = "Calibri"
font.size = Pt(10)
font.color.rgb = BLACK

# Set document properties
doc.core_properties.title = f"RFP Bid Screening -- {rfp.get('client_name', 'Unknown')}"
doc.core_properties.author = "Resource Data, Inc."

# Pre-compute shared values
rec_color = recommendation_color(recommendation)
areas = gonogo.get("assessment_areas", [])
compliance_items = compliance.get("compliance_items", [])
pass_count = sum(1 for it in compliance_items if it.get("status") == "PASS")
gap_count = sum(1 for it in compliance_items if it.get("status") == "GAP")
risk_count = sum(1 for it in compliance_items if it.get("status") == "RISK")
total_compliance = len(compliance_items)
theme_list = themes.get("themes", [])
matched = projects.get("matched_projects", [])
risks_list = risk.get("risks", [])
cq_questions = clarifying_qs.get("questions", [])

# ============================================================
# TIER 1: EXECUTIVE BRIEF (page 1 -- scannable in 2 minutes)
# ============================================================

# --- Section 1: Recommendation Banner ---
add_styled_heading(doc, "RFP Bid Screening Report", level=1)
add_body_paragraph(doc, report_subtitle, color=GRAY, size=Pt(11))
add_body_paragraph(doc, f"Generated: {datetime.now().strftime('%B %d, %Y at %I:%M %p')} | Mode: {'Quick' if bid_screen.get('screening_mode') == 'quick' else 'Full'}", color=GRAY, size=Pt(9))

# Historical RFP banner -- FIRST thing when deadline_passed is true
deadline_passed = rfp.get("deadline_passed", False)
if deadline_passed:
    p = doc.add_paragraph()
    run = p.add_run(f"HISTORICAL SCREENING -- Deadline passed {rfp.get('submission_deadline', 'N/A')}. Analysis validates capability for similar future opportunities.")
    run.font.name = "Calibri"
    run.font.size = Pt(11)
    run.bold = True
    run.font.color.rgb = AMBER

# Large colored recommendation + score
rec_text = badge.get(recommendation, recommendation)
p = doc.add_paragraph()
run = p.add_run(rec_text)
run.font.name = "Calibri"
run.font.size = Pt(18)
run.bold = True
run.font.color.rgb = rec_color
score_run = p.add_run(f"  ({total_score}/100)")
score_run.font.name = "Calibri"
score_run.font.size = Pt(14)
score_run.bold = True
score_run.font.color.rgb = rec_color

# Strengths (3-4 bullets, strongest evidence only)
strengths = bid_screen.get("strengths", [])
if not strengths:
    # Fallback: extract from assessment areas with highest scores
    sorted_areas = sorted(areas, key=lambda a: a.get("score", 0), reverse=True)
    for a in sorted_areas[:4]:
        ev = a.get("evidence", [])
        if ev:
            strengths.append(ev[0])
        elif a.get("rationale"):
            strengths.append(f"{a.get('name', '')}: {a.get('rationale', '')[:100]}")
if strengths:
    add_body_paragraph(doc, "Strengths", bold=True, color=NAVY, size=Pt(10))
    for s in strengths[:4]:
        add_bullet(doc, s)

# Gaps/Risks (2-3 bullets, honest)
gaps = bid_screen.get("gaps", [])
if not gaps:
    # Fallback: extract from assessment areas with lowest scores or risk fields
    sorted_areas_weak = sorted(areas, key=lambda a: a.get("score", 0))
    for a in sorted_areas_weak[:3]:
        ar = a.get("risks", [])
        if ar:
            gaps.append(ar[0])
if gaps:
    add_body_paragraph(doc, "Gaps / Risks", bold=True, color=NAVY, size=Pt(10))
    for g in gaps[:3]:
        add_bullet(doc, g, color=AMBER)

# Decision change conditions
change_conditions = bid_screen.get("decision_change_conditions", "")
if change_conditions:
    add_body_paragraph(doc, f"What would change this decision: {change_conditions}", italic=True, color=GRAY, size=Pt(9))

# Next steps if GO
next_steps = bid_screen.get("next_steps", [])
if next_steps and recommendation in ("GO", "CONDITIONAL"):
    add_body_paragraph(doc, "Next Steps", bold=True, color=NAVY, size=Pt(10))
    for step in next_steps[:3]:
        add_bullet(doc, step)

# --- Section 2: Opportunity Snapshot (compact single-row table) ---
add_styled_heading(doc, "Opportunity Snapshot", level=2)
snapshot_headers = ["Client", "Value", "Deadline", "Eval Method", "Domain", "Set-Aside"]
eval_model = rfp.get("evaluation_model", {})
eval_method_short = eval_model.get("evaluation_method_implications", rfp.get("evaluation_method", "N/A"))
if len(str(eval_method_short)) > 30:
    eval_method_short = str(eval_method_short)[:30] + "..."
snapshot_row = [[
    rfp.get("client_name", "Not identified"),
    rfp.get("estimated_value", "Not disclosed"),
    rfp.get("submission_deadline", "Not found"),
    eval_method_short,
    rfp.get("industry_domain", "N/A"),
    rfp.get("set_aside", "None"),
]]
add_table_from_data(doc, snapshot_headers, snapshot_row)

# --- Section 3: Scorecard (compact -- no narrative here) ---
add_styled_heading(doc, "Go/No-Go Scorecard", level=2)
scorecard_rows = []
for area in areas:
    name = area.get("name", "")
    weight = area.get("weight", 0)
    score = area.get("score", 0)
    weighted = score * weight
    weight_pct = f"{weight*100:.0f}%"
    rating_label = scorecard_rating_label(score)
    scorecard_rows.append([
        name,
        weight_pct,
        str(score),
        f"{weighted:.1f}",
        (rating_label, scorecard_rating_color(score))
    ])
scorecard_rows.append([
    ("TOTAL", NAVY),
    ("100%", NAVY),
    "",
    (str(total_score), NAVY),
    (recommendation, rec_color)
])
add_table_from_data(doc, ["Area", "Weight", "Score", "Weighted", "Rating"], scorecard_rows)
add_body_paragraph(doc, "90+ Exceptional | 75-89 Strong | 60-74 Adequate | <60 Weak", color=GRAY, size=Pt(8))

# --- Section 4: Key Decision Factors ---
add_styled_heading(doc, "Key Decision Factors", level=2)

go_factors = bid_screen.get("go_factors", [])
if not go_factors:
    # Fallback: derive from top-scoring areas
    for a in sorted(areas, key=lambda x: x.get("score", 0), reverse=True)[:3]:
        ev = a.get("evidence", [])
        go_factors.append(ev[0] if ev else f"{a.get('name', '')}: scored {a.get('score', 0)}/100")
add_body_paragraph(doc, "What makes this a GO:", bold=True, color=NAVY, size=Pt(10))
for f in go_factors[:3]:
    add_bullet(doc, f)

change_factors = bid_screen.get("change_factors", [])
if not change_factors and change_conditions:
    change_factors = [change_conditions]
if change_factors:
    add_body_paragraph(doc, "What could change the decision:", bold=True, color=NAVY, size=Pt(10))
    for cf in change_factors[:2]:
        add_bullet(doc, cf, color=AMBER)

# ============================================================
# TIER 2: DETAILED ANALYSIS (pages 2-5)
# ============================================================

# --- Section 5: Win Themes ---
if theme_list:
    add_styled_heading(doc, "Win Themes", level=2)

    for t in theme_list:
        rank = t.get("rank", "")
        name = t.get("name", "Unnamed")

        # Theme name heading
        p = doc.add_paragraph()
        r = p.add_run(f"{rank}. {name}")
        r.font.name = "Calibri"
        r.font.size = Pt(11)
        r.bold = True
        r.font.color.rgb = NAVY

        # Discriminator type, maturity
        disc_type = t.get("discriminator_type", "")
        t_maturity = t.get("maturity_level", "")
        if disc_type or t_maturity:
            add_key_value(doc, "Discriminator", f"{disc_type or 'N/A'} ({t_maturity or 'N/A'})")

        # Framing (italic)
        framing = t.get("framing", "")
        if framing:
            add_body_paragraph(doc, framing, italic=True, color=BLACK, size=Pt(10))

        # Evaluation impact
        eval_align = t.get("evaluation_alignment", {})
        if eval_align.get("mapped_criteria"):
            pts = eval_align.get("estimated_point_contribution", 0)
            pct_val = eval_align.get("point_contribution_pct", "N/A")
            criteria_names = [c["criterion"] for c in eval_align["mapped_criteria"][:3]]
            add_key_value(doc, "Evaluation Impact", f"~{pts} points ({pct_val}) -- {', '.join(criteria_names)}")

        # Ghost element
        ghost = t.get("ghost_element")
        if ghost:
            add_key_value(doc, "Competitive Edge", ghost)

        # So What
        so_what = t.get("so_what", "")
        if not so_what:
            # Generate contextual So What from rationale
            so_what = t.get("rationale", "")[:120]
        if so_what:
            add_so_what(doc, so_what)

elif themes.get("status") != "not_generated":
    add_styled_heading(doc, "Win Themes", level=2)
    add_body_paragraph(doc, "No preliminary themes generated for this screening.", color=GRAY)

# --- Section 6: Compliance & Contract Vehicles (CONDENSED) ---
add_styled_heading(doc, "Compliance & Contract Vehicles", level=2)

# One bold summary line
compliance_summary = f"Compliance: {pass_count}/{total_compliance} PASS"
if gap_count == 0 and risk_count == 0:
    compliance_summary += " -- All requirements met"
else:
    compliance_summary += f" -- {gap_count} GAP, {risk_count} RISK"
add_body_paragraph(doc, compliance_summary, bold=True, color=GREEN if gap_count == 0 and risk_count == 0 else AMBER)

# If ANY gap/risk: show detailed table ONLY for non-PASS items
non_pass_items = [it for it in compliance_items if it.get("status") != "PASS"]
if non_pass_items:
    comp_rows = []
    for item in non_pass_items:
        s = item.get("status", "UNKNOWN")
        comp_rows.append([
            item.get("requirement", "")[:80],
            item.get("category", "General"),
            (s, status_color(s))
        ])
    add_table_from_data(doc, ["Requirement", "Category", "Status"], comp_rows)

# Contract vehicles
contract_info = compliance.get("contract_vehicles", {})
matching_vehicles = contract_info.get("matching_rfp", [])
if matching_vehicles:
    add_key_value(doc, "Contract Vehicles", "; ".join(matching_vehicles))

# Existing relationship
existing_rel = compliance.get("existing_relationship", {})
if existing_rel.get("found"):
    add_key_value(doc, "Existing Client", existing_rel.get("matched_client", "Identified"), value_color=GREEN)

# Partnerships/awards (bullets)
comp_partnerships = compliance.get("partnerships", [])
if comp_partnerships:
    add_key_value(doc, "Partnerships", "; ".join(comp_partnerships))

comp_awards = compliance.get("awards", [])
if comp_awards:
    add_key_value(doc, "Awards", "; ".join(comp_awards[:5]))

# So What
compliance_so_what = ""
if gap_count == 0 and risk_count == 0:
    compliance_so_what = "Full compliance -- no barriers to submission."
elif gap_count > 0:
    compliance_so_what = f"{gap_count} compliance gap(s) require resolution before submission."
else:
    compliance_so_what = f"{risk_count} risk item(s) should be addressed in proposal narrative."
add_so_what(doc, compliance_so_what)

# --- Section 7: Past Project Matches ---
add_styled_heading(doc, "Past Project Matches", level=2)

if matched:
    # Top 3: full detail
    for i, proj in enumerate(matched[:3], 1):
        p = doc.add_paragraph()
        r = p.add_run(f"{i}. {proj.get('title', 'Untitled')}")
        r.font.name = "Calibri"
        r.font.size = Pt(11)
        r.bold = True
        r.font.color.rgb = NAVY

        # Compact key-value line
        client = proj.get("client", "N/A")
        score_val = proj.get("relevance_score", 0)
        rating_val = proj.get("relevance_rating", "N/A")
        add_key_value(doc, "Client", f"{client} | Score: {score_val}/51 ({rating_val})")

        # Key outcomes
        outcomes = proj.get("key_outcomes", [])
        if outcomes:
            for outcome in outcomes[:3]:
                add_bullet(doc, outcome)

        # Quote
        quote = proj.get("quote_text")
        attribution = proj.get("quote_attribution")
        if quote:
            p = doc.add_paragraph()
            r = p.add_run(f'"{quote}"')
            r.font.name = "Calibri"
            r.font.size = Pt(10)
            r.italic = True
            if attribution:
                r2 = p.add_run(f" -- {attribution}")
                r2.font.name = "Calibri"
                r2.font.size = Pt(9)
                r2.font.color.rgb = GRAY

        # Relevance statement
        relevance = proj.get("relevance_statement", "")
        if relevance:
            add_body_paragraph(doc, relevance, color=GRAY, size=Pt(9))

    # Projects 4-5: one line each
    for i, proj in enumerate(matched[3:5], 4):
        client = proj.get("client", "N/A")
        title = proj.get("title", "Untitled")
        score_val = proj.get("relevance_score", 0)
        rating_val = proj.get("relevance_rating", "N/A")
        add_body_paragraph(doc, f"{i}. {client} -- {title} ({score_val}/51, {rating_val})", size=Pt(9))

    # Tech gap analysis
    tech_gap = projects.get("technology_gap_analysis", "")
    if tech_gap:
        add_key_value(doc, "Tech Gap Analysis", tech_gap)

    # So What
    proj_so_what = ""
    if matched:
        top_score = matched[0].get("relevance_score", 0)
        if top_score >= 35:
            proj_so_what = "Strong past performance portfolio directly supports this opportunity."
        elif top_score >= 20:
            proj_so_what = "Moderate past performance match -- proposal narrative must bridge experience gaps."
        else:
            proj_so_what = "Weak past performance alignment -- consider teaming or highlighting transferable skills."
    add_so_what(doc, proj_so_what)
else:
    add_body_paragraph(doc, "No matching past projects found.", color=GRAY)
    add_so_what(doc, "No past performance evidence available -- significant proposal weakness.")

# --- Section 8: Risks & Opportunities ---
add_styled_heading(doc, "Risks & Opportunities", level=2)

# Risk table: 3 columns (Severity, Risk, Mitigation)
if risks_list:
    risk_rows = []
    for r_item in risks_list:
        severity = r_item.get("severity_band", r_item.get("severity", "unknown")).upper()
        risk_desc = r_item.get("description_if_then", r_item.get("risk", ""))[:120]
        mitigation = r_item.get("response_strategy", r_item.get("mitigation", ""))[:100]
        sev_color = RED if severity in ("CRITICAL", "HIGH") else AMBER if severity == "MODERATE" else GRAY
        risk_rows.append([(severity, sev_color), risk_desc, mitigation])
    add_table_from_data(doc, ["Severity", "Risk", "Mitigation"], risk_rows)
else:
    add_body_paragraph(doc, "No significant risks identified.", color=GRAY)

# Dealbreaker line
has_dealbreaker = risk.get("has_dealbreaker", False)
dealbreakers = risk.get("dealbreakers", [])
if has_dealbreaker:
    for db in dealbreakers:
        add_bullet(doc, f"DEALBREAKER: {db}", color=RED)
else:
    add_body_paragraph(doc, "No dealbreakers identified.", bold=True, color=GREEN, size=Pt(10))

# Opportunities
opportunities = risk.get("opportunities", [])
if opportunities:
    add_body_paragraph(doc, "Opportunities", bold=True, color=NAVY, size=Pt(10))
    for opp in opportunities:
        add_bullet(doc, opp.get("opportunity", str(opp)) if isinstance(opp, dict) else str(opp))

# So What
risk_so_what = ""
if has_dealbreaker:
    risk_so_what = f"{len(dealbreakers)} dealbreaker(s) must be resolved before pursuit."
elif len(risks_list) == 0:
    risk_so_what = "Clean risk profile supports confident pursuit."
else:
    critical_count = sum(1 for r_item in risks_list if r_item.get("severity_band", r_item.get("severity", "")).upper() in ("CRITICAL", "HIGH"))
    if critical_count > 0:
        risk_so_what = f"{critical_count} high-severity risk(s) require mitigation plans in proposal."
    else:
        risk_so_what = "Manageable risk profile -- standard mitigation strategies apply."
add_so_what(doc, risk_so_what)

# --- Section 9: Clarifying Questions (restructured) ---
if cq_questions:
    add_styled_heading(doc, "Clarifying Questions", level=2)

    # Deadline callout
    cq_deadline = clarifying_qs.get("questions_deadline", "Not found")
    if cq_deadline and cq_deadline not in ("Not found", "Not specified", "N/A", "Unknown"):
        deadline_note = ""
        try:
            for fmt in ("%Y-%m-%d", "%m/%d/%Y", "%B %d, %Y", "%b %d, %Y"):
                try:
                    deadline_dt = datetime.strptime(cq_deadline.strip(), fmt)
                    days_remaining = (deadline_dt - datetime.now()).days
                    if days_remaining < 0:
                        deadline_note = " -- PASSED"
                    elif days_remaining == 0:
                        deadline_note = " -- TODAY"
                    else:
                        deadline_note = f" -- {days_remaining} days remaining"
                    break
                except ValueError:
                    continue
        except Exception:
            pass
        add_key_value(doc, "Questions Deadline", f"{cq_deadline}{deadline_note}", value_color=AMBER)

    # Summary line
    cq_summary = clarifying_qs.get("summary", {})
    by_priority = cq_summary.get("by_priority", {})
    high_ct = by_priority.get("HIGH", 0)
    med_ct = by_priority.get("MEDIUM", 0)
    low_ct = by_priority.get("LOW", 0)
    total_ct = cq_summary.get("total_questions", len(cq_questions))
    add_body_paragraph(doc, f"{total_ct} Questions: {high_ct} Critical | {med_ct} Important | {low_ct} Advisory", bold=True, color=NAVY)

    # Classify questions into subsections by question_type or fallback by priority
    pre_qual_qs = [q for q in cq_questions if q.get("question_type") in ("strategic", "relationship", "pre_qualification")]
    response_prep_qs = [q for q in cq_questions if q.get("question_type") in ("technical", "detailed", "response_preparation")]
    research_qs = [q for q in cq_questions if q.get("question_type") in ("research", "budget", "incumbent")]

    # Fallback: if no question_type field, split by priority
    if not pre_qual_qs and not response_prep_qs and not research_qs:
        pre_qual_qs = [q for q in cq_questions if q.get("priority") == "HIGH"][:3]
        assigned_ids = {id(q) for q in pre_qual_qs}
        research_qs = [q for q in cq_questions if q.get("priority") == "MEDIUM" and id(q) not in assigned_ids][:2]
        assigned_ids.update(id(q) for q in research_qs)
        response_prep_qs = [q for q in cq_questions if id(q) not in assigned_ids]

    def render_question(q, full_detail=False):
        """Render a single question. full_detail=True shows impact + eval criterion."""
        qid = q.get("id", "")
        q_text = q.get("question", "")

        if full_detail:
            # HIGH priority: full question + impact + eval criterion
            p = doc.add_paragraph()
            r = p.add_run(f"{qid}. ")
            r.font.name = "Calibri"
            r.font.size = Pt(10)
            r.bold = True
            r2 = p.add_run(q_text)
            r2.font.name = "Calibri"
            r2.font.size = Pt(10)

            impact = q.get("impact", "")
            if impact:
                add_bullet(doc, f"Impact: {impact}")

            eval_target = q.get("evaluation_criterion_targeted")
            if eval_target:
                add_bullet(doc, f"Evaluation Criterion: {eval_target}")
        else:
            # MEDIUM: one-line
            add_body_paragraph(doc, f"{qid}. {q_text}", size=Pt(9))

    if pre_qual_qs:
        add_styled_heading(doc, "For Pre-Qualification Conference", level=3)
        for q in pre_qual_qs:
            render_question(q, full_detail=(q.get("priority") == "HIGH"))

    if response_prep_qs:
        add_styled_heading(doc, "For Response Preparation", level=3)
        for q in response_prep_qs:
            render_question(q, full_detail=(q.get("priority") == "HIGH"))

    if research_qs:
        add_styled_heading(doc, "Research Offline", level=3)
        for q in research_qs:
            render_question(q, full_detail=False)

# --- Section 10: Client & Competitive Intelligence (MERGED) ---
intel_rendered = False
if intel and intel.get("status") == "complete":
    intelligence = intel.get("intelligence", {})
    org = intelligence.get("organization_profile", {})
    competitive = intelligence.get("competitive_landscape", {})
    tech_stack = intelligence.get("technology_stack", [])

    if org or competitive or tech_stack:
        intel_rendered = True
        add_styled_heading(doc, "Client & Competitive Intelligence", level=2)

        # Client profile (compact)
        if org and org.get("name"):
            parts = []
            if org.get("size"):
                parts.append(f"Size: {org['size']}")
            if org.get("demographics"):
                parts.append(f"Demographics: {org['demographics']}")
            if org.get("governance"):
                parts.append(f"Governance: {org['governance']}")
            if org.get("headquarters"):
                parts.append(f"HQ: {org['headquarters']}")
            if parts:
                add_body_paragraph(doc, f"{org.get('name', '')} -- {' | '.join(parts)}", bold=True, size=Pt(10))
            if org.get("budget"):
                add_key_value(doc, "Budget", org["budget"])

        # Technology Evolution (strategically critical)
        if tech_stack:
            tech_from = []
            tech_to = []
            for tech in tech_stack:
                if isinstance(tech, dict):
                    note = tech.get("note", "").lower()
                    tech_name = tech.get("technology", "Unknown")
                    if "legacy" in note or "previous" in note or "migrat" in note:
                        tech_from.append(tech_name)
                    elif "current" in note or "new" in note or "cloud" in note:
                        tech_to.append(tech_name)
            if tech_from or tech_to:
                from_str = ", ".join(tech_from) if tech_from else "on-premise/legacy"
                to_str = ", ".join(tech_to) if tech_to else "modern/cloud"
                add_key_value(doc, "Technology Evolution", f"{from_str} -> {to_str}")

        # Competitive Landscape
        if competitive and (competitive.get("incumbent") or competitive.get("known_competitors")):
            add_body_paragraph(doc, "Competitive Landscape:", bold=True, color=NAVY, size=Pt(10))
            if competitive.get("incumbent"):
                add_key_value(doc, "Incumbent", competitive["incumbent"])
            known = competitive.get("known_competitors", [])
            if known:
                add_key_value(doc, "Known Competitors", ", ".join(known))

            # Intelligence Required (unknowns)
            intel_gaps = []
            if not competitive.get("incumbent") or competitive.get("incumbent") == "Unknown":
                intel_gaps.append("Incumbent identity")
            if not known:
                intel_gaps.append("Competitor field")
            if not competitive.get("notes"):
                intel_gaps.append("Competitive strategy details")
            if intel_gaps:
                add_key_value(doc, "Intelligence Required", "; ".join(intel_gaps), value_color=AMBER)

            notes = competitive.get("notes", "")
            if notes:
                add_body_paragraph(doc, notes, color=GRAY, size=Pt(9))

        # So What
        intel_so_what = ""
        if competitive.get("incumbent") and competitive["incumbent"] != "Unknown":
            intel_so_what = f"Incumbent ({competitive['incumbent']}) advantage must be neutralized through differentiation."
        elif known:
            intel_so_what = f"{len(known)} known competitor(s) -- positioning strategy should exploit known weaknesses."
        else:
            intel_so_what = "Limited competitive intelligence -- invest in OSINT before proposal start."
        add_so_what(doc, intel_so_what)

# ============================================================
# TIER 3: APPENDIX (reference only -- 9pt font)
# ============================================================

# --- Appendix A: Buyer Priorities ---
buyer_priorities = rfp.get("buyer_priorities", [])
if buyer_priorities:
    add_styled_heading(doc, "Appendix A: Buyer Priorities", level=2)

    gonogo_coverage = gonogo.get("buyer_priority_coverage", {})
    theme_coverage = themes.get("buyer_priority_coverage", {}) if themes else {}
    addressed_list = gonogo_coverage.get("high_addressed_list", [])
    theme_covered = theme_coverage.get("covered", [])

    bp_rows = []
    for bp_item in buyer_priorities:
        name = bp_item.get("name", "Unknown")
        importance = bp_item.get("importance", "?")
        eval_crit = bp_item.get("evaluation_criterion", "N/A")
        signal = bp_item.get("signal", "")[:80]

        if name in addressed_list and name in theme_covered:
            coverage = "STRONG"
            cov_color = GREEN
        elif name in addressed_list or name in theme_covered:
            coverage = "PARTIAL"
            cov_color = AMBER
        elif importance == "HIGH":
            coverage = "GAP"
            cov_color = RED
        else:
            coverage = "--"
            cov_color = GRAY

        bp_rows.append([name, importance, signal, eval_crit, (coverage, cov_color)])
    add_table_from_data(doc, ["Priority", "Importance", "Signal/Evidence", "Eval Criterion", "Coverage"], bp_rows)

# --- Appendix B: Technology Intelligence ---
tech_intel = rfp.get("tech_intelligence", {})
if tech_intel and tech_intel.get("technology_stacks"):
    add_styled_heading(doc, "Appendix B: Technology Intelligence", level=2)

    # Stack overview
    for stack in tech_intel.get("technology_stacks", []):
        p = doc.add_paragraph()
        r = p.add_run(f"{stack['stack_name']}")
        r.font.name = "Calibri"
        r.font.size = Pt(9)
        r.bold = True
        r.font.color.rgb = NAVY
        r2 = p.add_run(f" (Coherence: {stack.get('coherence', 'N/A')})")
        r2.font.name = "Calibri"
        r2.font.size = Pt(8)
        r2.font.color.rgb = GRAY

        for comp in stack.get("components", []):
            version_str = f" v{comp['version']}" if comp.get("version") else ""
            add_bullet(doc, f"{comp['name']}{version_str} -- {comp.get('role', '')} ({comp.get('maturity', '')})", size=Pt(8))

    # RDI Alignment with three-tier coverage display
    alignment = tech_intel.get("rdi_alignment", {})
    if alignment:
        add_styled_heading(doc, "RDI Technology Alignment", level=3)

        # Three-tier coverage display
        platform_coverage = alignment.get("coverage_ratio", 0)
        pct = f"{int(platform_coverage * 100)}%" if isinstance(platform_coverage, (int, float)) else str(platform_coverage)

        # Version confidence based on unversioned count
        unversioned = tech_intel.get("unversioned_technologies", [])
        total_techs = sum(len(s.get("components", [])) for s in tech_intel.get("technology_stacks", []))
        if not unversioned or len(unversioned) == 0:
            version_conf = "High"
        elif len(unversioned) < total_techs / 2:
            version_conf = "Medium"
        else:
            version_conf = "Low"

        add_key_value(doc, "Platform Coverage", f"{pct} of required technologies matched", size=Pt(9))
        add_key_value(doc, "Version Confidence", f"{version_conf} ({len(unversioned)} of {total_techs} technologies unversioned in RFP)", size=Pt(9))

        if alignment.get("strong_match"):
            add_key_value(doc, "Strong Match", ", ".join(alignment["strong_match"][:6]), size=Pt(9))
        if alignment.get("no_match"):
            add_key_value(doc, "Gaps", ", ".join(alignment["no_match"][:4]), value_color=AMBER, size=Pt(9))

    # Maturity Profile
    maturity = tech_intel.get("maturity_profile", {})
    if maturity:
        add_styled_heading(doc, "Technology Maturity Profile", level=3)
        mat_rows = []
        for level_name in ["established", "mature", "emerging", "declining"]:
            count = maturity.get(level_name, 0)
            if count > 0:
                mat_rows.append([level_name.title(), str(count)])
        if mat_rows:
            add_table_from_data(doc, ["Maturity", "Count"], mat_rows)

    # Risk flags
    risk_flags = tech_intel.get("technology_risk_flags", [])
    if risk_flags:
        add_styled_heading(doc, "Technology Risk Flags", level=3)
        for flag in risk_flags:
            add_bullet(doc, flag, color=AMBER, size=Pt(8))

# --- Appendix C: Evaluation Score Potential ---
if eval_model and eval_model.get("point_allocation"):
    add_styled_heading(doc, "Appendix C: Evaluation Score Potential", level=2)

    add_key_value(doc, "Evaluation Method", eval_model.get("evaluation_method_implications", "Not specified"), size=Pt(9))
    add_key_value(doc, "Evaluator Profile", eval_model.get("evaluator_persona", "Unknown").replace("_", " ").title(), size=Pt(9))
    add_key_value(doc, "Technical-to-Price Ratio", eval_model.get("technical_to_price_ratio", "Unknown"), size=Pt(9))

    eval_rows = []
    for criterion in eval_model["point_allocation"]:
        eval_rows.append([
            criterion.get("criterion", ""),
            str(criterion.get("pct", "")),
            str(criterion.get("points", "")),
            criterion.get("discriminator_potential", "").title()
        ])
    add_table_from_data(doc, ["Criterion", "Weight", "Est. Points", "Discriminator Potential"], eval_rows)

# ============================================================
# Footer
# ============================================================
add_body_paragraph(doc, "Report generated by RFP Screening Pipeline. This is an automated assessment -- human judgment should inform the final bid/no-bid decision.", color=GRAY, size=Pt(8))

# === Save ===
doc.save(f"{folder}/screen/BID_SCREEN.docx")
```

### Step 5: QA Check

```python
import os

docx_path = f"{folder}/screen/BID_SCREEN.docx"
md_path = f"{folder}/screen/BID_SCREEN.md"

# Check markdown
if os.path.exists(md_path):
    md_size = os.path.getsize(md_path) / 1024
    log(f"BID_SCREEN.md: {md_size:.1f}KB")
    if md_size < 5:
        log(f"WARNING: BID_SCREEN.md only {md_size:.1f}KB -- expected >5KB")
else:
    log("ERROR: BID_SCREEN.md not generated")

# Check DOCX
if os.path.exists(docx_path):
    size_kb = os.path.getsize(docx_path) / 1024
    if size_kb < 30:
        log(f"WARNING: BID_SCREEN.docx only {size_kb:.1f}KB -- may be incomplete")
        # Retry once
        doc.save(f"{folder}/screen/BID_SCREEN.docx")
        size_kb = os.path.getsize(docx_path) / 1024

    log(f"BID_SCREEN.docx: {size_kb:.1f}KB")
    if size_kb < 30:
        log("ERROR: BID_SCREEN.docx still below 30KB after retry")
else:
    log("ERROR: BID_SCREEN.docx not generated")
    log("Attempting retry with python-docx...")
    try:
        doc.save(f"{folder}/screen/BID_SCREEN.docx")
        size_kb = os.path.getsize(docx_path) / 1024
        log(f"Retry succeeded: {size_kb:.1f}KB")
    except Exception as e:
        log(f"Retry failed: {e}")
        log("Fallback: BID_SCREEN.json is still available as machine-readable output")
```

### Step 6: Report

```
REPORT GENERATION (Phase 6)
============================
BID_SCREEN.md: {md_size}KB
BID_SCREEN.docx: {size_kb}KB

Primary deliverable: {folder}/screen/BID_SCREEN.docx
Secondary reference: {folder}/screen/BID_SCREEN.md
Machine-readable: {folder}/screen/BID_SCREEN.json
```

---

## Quality Checklist

### Document Structure
- [ ] `BID_SCREEN.md` written (>5KB)
- [ ] `BID_SCREEN.docx` generated (>30KB)
- [ ] Three-tier structure: Executive Brief (page 1) -> Detailed Analysis (pages 2-5) -> Appendix (reference)
- [ ] QA check passed (file exists, > 30KB)
- [ ] If DOCX fails, BID_SCREEN.json still available as fallback

### TIER 1: Executive Brief (page 1)
- [ ] Section 1: Recommendation banner with large colored text + score
- [ ] Historical RFP banner FIRST when deadline_passed == true
- [ ] Strengths (3-4 bullets), Gaps/Risks (2-3 bullets), decision change conditions, next steps
- [ ] Section 2: Opportunity Snapshot as compact single-row table (Client|Value|Deadline|Eval Method|Domain|Set-Aside)
- [ ] Section 3: Scorecard compact -- 7 rows + total, rating color-coded (green 75+, amber 60-74, red <60)
- [ ] Scoring anchor legend below table ("90+ Exceptional | 75-89 Strong | 60-74 Adequate | <60 Weak")
- [ ] Section 4: Key Decision Factors -- GO factors + change conditions
- [ ] Executive brief designed to fit one printed page (minimal spacing, compact tables)

### TIER 2: Detailed Analysis (pages 2-5)
- [ ] Section 5: Win Themes -- per theme: name, discriminator type, maturity, framing, eval impact, ghost, "So What"
- [ ] Section 6: Compliance CONDENSED -- one bold summary line, detailed table ONLY for non-PASS items
- [ ] Section 6: Contract vehicles, partnerships, awards after compliance
- [ ] Section 7: Past Projects -- top 3 full detail, projects 4-5 one-line summary
- [ ] Section 8: Risks -- 3-column table (Severity, Risk, Mitigation), dealbreaker line, opportunities
- [ ] Section 9: Clarifying Questions split into: Pre-Qualification Conference / Response Preparation / Research Offline
- [ ] Section 9: HIGH priority = full question + impact + eval criterion; MEDIUM = one line
- [ ] Section 10: Client & Competitive Intelligence MERGED -- client profile, tech evolution, competitive landscape
- [ ] All Tier 2 sections end with "So What" line (italic, navy, preceded by thin separator)

### TIER 3: Appendix (reference only)
- [ ] Appendix A: Buyer Priorities (detailed table with signal/evidence)
- [ ] Appendix B: Technology Intelligence (stacks, maturity, alignment detail)
- [ ] Appendix B: Three-tier coverage display (Platform Coverage + Version Confidence)
- [ ] Appendix C: Evaluation Score Potential (method, evaluator persona, point allocation)
- [ ] Appendix sections use 9pt font to signal reference-only status

### Style Rules
- [ ] Font: Calibri, 10pt body, headings in navy (#003366)
- [ ] H1=18pt, H2=13pt, H3=11pt
- [ ] Tables use 'Light Grid Accent 1' style
- [ ] Colors: GREEN (#008000), AMBER (#C89600), RED (#B40000), NAVY (#003366), GRAY (#646464)
- [ ] All decimal ratios rendered as percentages with contextual labels
- [ ] Scorecard renders 7 weighted assessment areas (not 5 equal dimensions)
- [ ] "So What" lines: italic, navy, preceded by thin line separator
- [ ] Tone-adapted report subtitle renders based on client_tone.primary_style (5 variants)

### Backward Compatibility
- [ ] Falls back gracefully when enriched fields not present (themes, risks, questions)
- [ ] If --quick mode, intel section omitted
- [ ] Risk table handles both enriched and simple risk formats
- [ ] Clarifying questions fall back to priority-based grouping when question_type absent
- [ ] Clarifying questions include evaluation_criterion_targeted
- [ ] All new sections omitted gracefully when data absent (`.get()` with defaults)
