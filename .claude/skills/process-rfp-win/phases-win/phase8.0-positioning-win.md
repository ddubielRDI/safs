---
name: phase8.0-positioning-win
expert-role: Bid Strategist
domain-expertise: Strategic positioning, differentiators
---

# Phase 8.0: Strategic Positioning

## Expert Role

You are a **Bid Strategist** with deep expertise in:
- Strategic positioning
- Value proposition development
- Competitive differentiation
- Evaluator-focused messaging

## Purpose

Generate strategic positioning and evaluator-specific messages.

## Inputs

- `{folder}/shared/bid/CLIENT_INTELLIGENCE.json`
- `{folder}/shared/EVALUATION_CRITERIA.json`
- `{folder}/shared/PERSONA_COVERAGE.json`
- `{folder}/shared/domain-context.json`
- `{folder}/shared/requirements-normalized.json` - Requirements with tech stack
- `Past_Projects.md` - 28 case studies for auto-selection
- `config-win/company-profile.json` - Company data (override_projects list)

## Required Outputs

- `{folder}/shared/bid/POSITIONING_OUTPUT.json` — Contains:
  - `core_positioning` — Tagline, value proposition, themes, differentiators, proof points
  - `evaluator_messages` — Targeted messages per evaluator role (TECHNICAL, FINANCIAL, RISK, EXECUTIVE, OPERATIONAL)
  - `content_priority_order` — Ranked list of topics by combined eval+pain weight
  - `win_themes` — List of win theme names
  - `theme_eval_mapping` — Dict mapping each theme name to the list of eval factors it supports (with factor_id, factor_name, weight, alignment_score, relevance)
  - `section_theme_mandates` — Dict mapping bid section name to required themes and eval factors that must be addressed in that section
  - `key_messages` — Summary messages for executive summary, technical, management, pricing
  - `matched_projects` — Ranked past projects with scores and relevance statements
  - `match_metadata` — Statistics on project matching

## Instructions

### Step 0: Consult Historical Bid Outcomes

```python
# Step 0: Consult Historical Bid Outcomes
import os, json

outcomes_path = f"{skill_dir}/config-win/bid-outcomes.json"
historical_patterns = {}

if os.path.exists(outcomes_path):
    outcomes_data = read_json_safe(outcomes_path)
    outcomes = outcomes_data.get("outcomes", [])

    # Only analyze if 3+ completed outcomes exist
    completed = [o for o in outcomes if o.get("outcome") in ("win", "loss")]

    if len(completed) >= 3:
        # Calculate win rate by domain
        domain_stats = {}
        for o in completed:
            domain = o.get("domain", "unknown")
            if domain not in domain_stats:
                domain_stats[domain] = {"wins": 0, "losses": 0}
            if o["outcome"] == "win":
                domain_stats[domain]["wins"] += 1
            else:
                domain_stats[domain]["losses"] += 1

        # Identify effective themes (themes in wins vs losses)
        theme_wins = {}
        theme_losses = {}
        for o in completed:
            themes = o.get("themes_used", [])
            for t in themes:
                if o["outcome"] == "win":
                    theme_wins[t] = theme_wins.get(t, 0) + 1
                else:
                    theme_losses[t] = theme_losses.get(t, 0) + 1

        # Common weaknesses across losses
        all_weaknesses = []
        for o in completed:
            if o["outcome"] == "loss":
                all_weaknesses.extend(o.get("weaknesses_cited", []))

        historical_patterns = {
            "has_data": True,
            "total_bids": len(completed),
            "overall_win_rate": sum(1 for o in completed if o["outcome"] == "win") / len(completed),
            "domain_stats": domain_stats,
            "effective_themes": {t: {"wins": theme_wins.get(t, 0), "losses": theme_losses.get(t, 0)} for t in set(list(theme_wins.keys()) + list(theme_losses.keys()))},
            "recurring_weaknesses": list(set(all_weaknesses)),
            "advisory": f"Based on {len(completed)} past bids ({sum(1 for o in completed if o['outcome'] == 'win')} wins). Use as advisory input, not override."
        }
        log(f"Historical outcomes: {len(completed)} bids analyzed, {historical_patterns['overall_win_rate']:.0%} win rate")
    else:
        log(f"Historical outcomes: {len(completed)} completed bids (need 3+ for pattern analysis)")
        historical_patterns = {"has_data": False, "reason": f"Only {len(completed)} completed outcomes (need 3+)"}
else:
    log("Historical outcomes: No bid-outcomes.json found (first bid)")
    historical_patterns = {"has_data": False, "reason": "No historical data yet"}
```

### Step 1: Load Intelligence

```python
client_intel = read_json(f"{folder}/shared/bid/CLIENT_INTELLIGENCE.json")
eval_criteria = read_json(f"{folder}/shared/EVALUATION_CRITERIA.json")
personas = read_json(f"{folder}/shared/PERSONA_COVERAGE.json")
domain_context = read_json(f"{folder}/shared/domain-context.json")
requirements = read_json_safe(f"{folder}/shared/requirements-normalized.json")
past_projects_md = read_file("Past_Projects.md")
company = read_json("config-win/company-profile.json")
```

### Step 1b: Match Past Projects to RFP

Read `Past_Projects.md` and score each project against the RFP's domain, industry, technology stack, and requirements. Output a ranked `matched_projects[]` array for downstream phases (8.1, 8.2, 8.3) to consume.

```python
def select_matching_projects(past_projects_md, domain_context, requirements, company):
    """Score and rank past projects by relevance to this RFP.

    Scoring dimensions:
    - Industry match:    exact = 10pts, related = 5pts
    - Technology overlap: 3pts per matching technology
    - Quantified metrics: 5pts if project has a metrics/results table
    - Client quote:       2pts if project includes a testimonial
    - Recency:           5pts (<3 years), 3pts (3-5 years), 1pt (>5 years)
    - Scale/impact:      3pts for large-scale projects ($1M+ or 1000+ users)

    Returns top 5-8 ranked projects with score breakdowns.
    """

    # Extract RFP context for matching
    rfp_domain = domain_context.get("selected_domain", "default")
    rfp_industry = domain_context.get("industry", domain_context.get("sector", ""))
    rfp_client_type = domain_context.get("client_type", "")  # e.g., "state_government"

    # Build tech stack from requirements
    rfp_tech_stack = []
    if requirements:
        all_reqs = requirements.get("requirements", [])
        for req in all_reqs:
            techs = req.get("technologies", req.get("tech_stack", []))
            if isinstance(techs, list):
                rfp_tech_stack.extend(techs)
            keywords = req.get("keywords", [])
            if isinstance(keywords, list):
                rfp_tech_stack.extend(keywords)
    rfp_tech_stack = list(set(t.lower().strip() for t in rfp_tech_stack if t))

    # Related industry mapping for partial matches
    related_industries = {
        "government": ["education", "transportation", "utilities", "natural resources"],
        "education": ["government"],
        "healthcare": ["government"],
        "natural resources": ["government", "oil & gas", "fisheries", "mining"],
        "oil & gas": ["natural resources", "mining", "manufacturing"],
        "fisheries": ["natural resources", "government"],
        "transportation": ["government", "manufacturing"],
        "utilities": ["government", "manufacturing"],
        "manufacturing": ["oil & gas", "utilities"],
        "mining": ["natural resources", "oil & gas"]
    }

    # Parse Past_Projects.md into individual project entries
    # Each project starts with "## Project N:" heading
    # Extract: project_number, client, industry, description, technologies,
    #          outcomes/metrics, quotes, timeline
    projects = parse_past_projects(past_projects_md)

    scored_projects = []
    for project in projects:
        score = 0
        breakdown = {}

        # --- Industry Match (max 10pts) ---
        project_industry = project.get("industry", "").lower()
        rfp_industry_lower = rfp_industry.lower() if rfp_industry else rfp_domain.lower()

        if project_industry == rfp_industry_lower:
            breakdown["industry"] = 10
        elif project_industry in related_industries.get(rfp_industry_lower, []):
            breakdown["industry"] = 5
        else:
            breakdown["industry"] = 0
        score += breakdown["industry"]

        # --- Technology Overlap (3pts per match, max 15pts) ---
        project_techs = [t.lower() for t in project.get("technologies", [])]
        tech_matches = []
        for rfp_tech in rfp_tech_stack:
            for proj_tech in project_techs:
                if rfp_tech in proj_tech or proj_tech in rfp_tech:
                    tech_matches.append(rfp_tech)
                    break
        tech_score = min(len(tech_matches) * 3, 15)
        breakdown["technology"] = {"score": tech_score, "matches": tech_matches[:5]}
        score += tech_score

        # --- Quantified Metrics (5pts) ---
        has_metrics = project.get("has_metrics_table", False)
        breakdown["metrics"] = 5 if has_metrics else 0
        score += breakdown["metrics"]

        # --- Client Quote (2pts) ---
        has_quote = project.get("has_quote", False)
        breakdown["quote"] = 2 if has_quote else 0
        score += breakdown["quote"]

        # --- Recency (max 5pts) ---
        # Parse from timeline field if available
        recency_score = 3  # default: moderate recency
        timeline = project.get("timeline", "")
        if "2024" in timeline or "2025" in timeline or "2026" in timeline:
            recency_score = 5
        elif "2022" in timeline or "2023" in timeline:
            recency_score = 4
        elif "2020" in timeline or "2021" in timeline:
            recency_score = 3
        elif "2018" in timeline or "2019" in timeline:
            recency_score = 2
        else:
            recency_score = 1
        breakdown["recency"] = recency_score
        score += recency_score

        # --- Scale/Impact (3pts) ---
        scale_score = 0
        description = project.get("description", "").lower()
        outcomes = project.get("outcomes", "").lower()
        if any(kw in description + outcomes for kw in ["enterprise", "statewide", "million", "$1m", "1,000"]):
            scale_score = 3
        elif any(kw in description + outcomes for kw in ["department", "agency", "organization"]):
            scale_score = 1
        breakdown["scale"] = scale_score
        score += scale_score

        scored_projects.append({
            "project_number": project.get("project_number"),
            "client": project.get("client", ""),
            "industry": project.get("industry", ""),
            "title": project.get("title", ""),
            "relevance_score": score,
            "score_breakdown": breakdown,
            "technologies": project.get("technologies", []),
            "key_metrics": project.get("key_metrics", []),
            "timeline": project.get("timeline", ""),
            "description_summary": project.get("description", "")[:300],
            "has_metrics_table": has_metrics,
            "has_quote": has_quote,
            "relevance_statement": ""  # Populated below
        })

    # Sort by score descending
    scored_projects.sort(key=lambda p: p["relevance_score"], reverse=True)

    # Check for force-included override projects from company-profile.json
    overrides = company.get("past_performance", {}).get("override_projects", [])
    if overrides:
        override_entries = [p for p in scored_projects if p["project_number"] in overrides]
        non_override = [p for p in scored_projects if p["project_number"] not in overrides]
        # Force overrides to top, then append ranked non-overrides
        scored_projects = override_entries + non_override

    # Cap at top 8, ensure diversity (max 3 from same industry)
    final_projects = []
    industry_counts = {}
    for p in scored_projects:
        ind = p["industry"].lower()
        if industry_counts.get(ind, 0) >= 3:
            continue
        final_projects.append(p)
        industry_counts[ind] = industry_counts.get(ind, 0) + 1
        if len(final_projects) >= 8:
            break

    # Generate relevance statements for top matches
    for i, p in enumerate(final_projects):
        tech_matches = p["score_breakdown"].get("technology", {}).get("matches", [])
        p["rank"] = i + 1
        p["relevance_statement"] = (
            f"Ranked #{i+1} match (score: {p['relevance_score']}). "
            f"Industry: {p['industry']}. "
            + (f"Technology overlap: {', '.join(tech_matches[:3])}. " if tech_matches else "")
            + (f"Key result: {p['key_metrics'][0]}. " if p.get('key_metrics') else "")
        )

    # Zero-match fallback handling
    fallback_warning = None

    # Edge case: no projects parsed from Past_Projects.md
    if not final_projects:
        fallback_warning = (
            "CRITICAL: No projects could be parsed from Past_Projects.md. "
            "Verify the file exists and follows the expected ## Project N: format. "
            "Downstream phases will use generic company capability statements instead."
        )
        return [], fallback_warning

    # Warn if fewer than 3 projects scored >10
    strong_matches = [p for p in final_projects if p["relevance_score"] > 10]
    if len(strong_matches) < 3:
        fallback_warning = (
            f"Only {len(strong_matches)} strong matches found (score >10). "
            f"Top {len(final_projects)} projects included. Consider adding manual "
            f"override_projects in company-profile.json for this bid."
        )

    return final_projects, fallback_warning

matched_projects, match_warning = select_matching_projects(
    past_projects_md, domain_context, requirements, company
)
```

**Parsing `Past_Projects.md`:** The AI executing this phase must read the full `Past_Projects.md` file and parse each project entry (delimited by `## Project N:` headings) into structured objects with these fields:
- `project_number` (int): The project number from the heading
- `client` (str): Client name
- `industry` (str): Industry category
- `title` (str): Project name/title
- `description` (str): Full project description
- `technologies` (list[str]): Technologies mentioned
- `timeline` (str): Duration/dates
- `outcomes` (str): Results/outcomes text
- `key_metrics` (list[str]): Quantified metrics (e.g., "99.9% uptime", "40% cost reduction")
- `has_metrics_table` (bool): Whether project has a structured metrics/results table
- `has_quote` (bool): Whether project includes a client quote/testimonial

### Step 1c: Match Evidence Library to RFP

Load the evidence library and match proof points to the RFP's domain keywords and evaluation criteria.

```python
# Load evidence library
evidence_library = read_json_safe("config-win/evidence-library.json")

# Match evidence to domain keywords from requirements
matched_evidence = []
if evidence_library:
    domain_keywords = set()
    # Extract keywords from domain context, requirements, and evaluation criteria
    domain_kw = domain_context.get("selected_domain", {})
    if isinstance(domain_kw, dict):
        domain_kw = domain_kw.get("keywords", [])
    elif isinstance(domain_kw, str):
        domain_kw = [domain_kw]
    domain_keywords.update([k.lower() for k in domain_kw])

    # Add evaluation factor keywords
    eval_factors = eval_criteria.get("evaluation_factors", [])
    for factor in eval_factors:
        factor_text = (factor.get("name", "") + " " + factor.get("description", "")).lower()
        domain_keywords.update(factor_text.split())

    # Score each evidence item by tag overlap with domain keywords
    for category, items in evidence_library.get("categories", {}).items():
        for item in items:
            # Skip unpopulated items
            item_text = item.get("statement", item.get("certification", item.get("quote", item.get("award", ""))))
            if "[USER INPUT" in str(item_text):
                continue

            tags = set(t.lower() for t in item.get("tags", []))
            overlap = tags.intersection(domain_keywords)
            if overlap or category in ["metrics", "differentiators"]:  # Always include metrics and differentiators
                matched_evidence.append({
                    "id": item.get("id"),
                    "category": category,
                    "content": item_text,
                    "tags": item.get("tags", []),
                    "relevance_tags": list(overlap),
                    "relevance_score": len(overlap)
                })

    # Sort by relevance
    matched_evidence.sort(key=lambda x: x["relevance_score"], reverse=True)

    # Fallback: if fewer than 3 matches, include all pre-populated metrics and differentiators
    if len(matched_evidence) < 3 and evidence_library:
        for category in ["metrics", "differentiators"]:
            for item in evidence_library.get("categories", {}).get(category, []):
                item_text = item.get("statement", "")
                if "[USER INPUT" in str(item_text):
                    continue
                if not any(e["id"] == item.get("id") for e in matched_evidence):
                    matched_evidence.append({
                        "id": item.get("id"),
                        "category": category,
                        "content": item_text,
                        "tags": item.get("tags", []),
                        "relevance_tags": [],
                        "relevance_score": 0  # fallback — no keyword match but included for minimum coverage
                    })
        log(f"Evidence fallback: padded to {len(matched_evidence)} items (minimum coverage)")
```

### Step 2: Map Win Themes to Evaluation Factors

Map each win theme to the evaluation factors it supports. This creates explicit traceability from themes → eval factors → bid sections. The function is defined here and called after themes are generated in Step 3.

```python
def build_theme_eval_mapping(themes, eval_criteria):
    """Map each win theme to the evaluation factors it addresses.

    Uses keyword/intent alignment between theme names and factor
    descriptions, subfactors, and names to create a traceable link.

    Returns dict: theme_name → list of mapped eval factors.
    """
    eval_factors = eval_criteria.get("evaluation_factors", [])
    theme_eval_mapping = {}

    for theme in themes:
        theme_lower = theme.lower() if isinstance(theme, str) else theme.get("name", "").lower()
        theme_name = theme if isinstance(theme, str) else theme.get("name", "")
        mapped_factors = []

        for factor in eval_factors:
            factor_name = factor.get("name", "")
            factor_desc = factor.get("description", "")
            subfactors = factor.get("subfactors", [])
            subfactor_text = " ".join(
                sf.get("name", "") + " " + sf.get("description", "")
                for sf in subfactors if isinstance(sf, dict)
            ).lower()

            # Check alignment: theme keywords appear in factor name,
            # description, or subfactors (bidirectional check)
            factor_text = (factor_name + " " + factor_desc + " " + subfactor_text).lower()
            theme_words = [w for w in theme_lower.split() if len(w) > 3]

            alignment_score = sum(1 for w in theme_words if w in factor_text)
            if alignment_score > 0 or theme_lower in factor_text:
                mapped_factors.append({
                    "factor_id": factor.get("factor_id", factor.get("id", factor_name)),
                    "factor_name": factor_name,
                    "weight": factor.get("weight_normalized", 0),
                    "alignment_score": alignment_score,
                    "relevance": f"Theme '{theme_name}' supports '{factor_name}' — "
                                 f"{alignment_score} keyword alignments detected"
                })

        # Sort by weight descending so highest-impact factors appear first
        mapped_factors.sort(key=lambda f: f["weight"], reverse=True)
        theme_eval_mapping[theme_name] = mapped_factors

    return theme_eval_mapping
```

### Step 2b: Build Section-Theme Mandates

Generate explicit mandates for which themes MUST appear in which bid section, driven by eval factor weights and `bid_section_mapping` from `EVALUATION_CRITERIA.json`.

```python
def build_section_theme_mandates(eval_criteria):
    """Generate section → required themes + eval factors mandates.

    Uses bid_section_mapping (or EVALUATION_TO_BID_MAPPING) from
    EVALUATION_CRITERIA.json to determine which themes must appear
    in which proposal sections.

    Returns dict: section_name → { themes: [], eval_factors: [] }
    """
    bid_mapping = eval_criteria.get(
        "bid_section_mapping",
        eval_criteria.get("EVALUATION_TO_BID_MAPPING", {})
    )

    section_theme_mandates = {}

    for factor_name, mapping in bid_mapping.items():
        if not isinstance(mapping, dict):
            continue

        primary_section = mapping.get("primary_section", "")
        themes_for_factor = mapping.get("win_themes", [])
        supporting_sections = mapping.get("supporting_sections", [])

        # Add to primary section
        if primary_section:
            if primary_section not in section_theme_mandates:
                section_theme_mandates[primary_section] = {
                    "themes": [],
                    "eval_factors": [],
                    "is_primary_for": []
                }
            section_theme_mandates[primary_section]["themes"].extend(themes_for_factor)
            section_theme_mandates[primary_section]["eval_factors"].append(factor_name)
            section_theme_mandates[primary_section]["is_primary_for"].append(factor_name)

        # Add to supporting sections (themes should echo here too)
        for sup_section in supporting_sections:
            if sup_section not in section_theme_mandates:
                section_theme_mandates[sup_section] = {
                    "themes": [],
                    "eval_factors": [],
                    "is_primary_for": []
                }
            section_theme_mandates[sup_section]["themes"].extend(themes_for_factor)
            if factor_name not in section_theme_mandates[sup_section]["eval_factors"]:
                section_theme_mandates[sup_section]["eval_factors"].append(factor_name)

    # Deduplicate themes per section
    for section in section_theme_mandates:
        section_theme_mandates[section]["themes"] = list(
            dict.fromkeys(section_theme_mandates[section]["themes"])
        )

    return section_theme_mandates
```

### Step 3: Define Core Positioning

```python
def develop_positioning(domain, client_intel, eval_criteria):
    """Develop core positioning strategy."""
    positioning = {
        "tagline": "",
        "value_proposition": "",
        "key_differentiators": [],
        "themes": [],
        "proof_points": []
    }

    # Domain-specific positioning
    if domain == "education":
        positioning["tagline"] = "Modern K-12 Solutions Built for Student Success"
        positioning["value_proposition"] = (
            "We deliver modern, FERPA-compliant education technology solutions "
            "that streamline operations, improve data quality, and enable "
            "better outcomes for students and educators."
        )
        positioning["themes"] = [
            "Student-Centered Design",
            "Compliance Without Complexity",
            "Data-Driven Decision Making",
            "Seamless State Reporting"
        ]

    elif domain == "healthcare":
        positioning["tagline"] = "Healthcare Technology That Puts Patients First"
        positioning["value_proposition"] = (
            "We deliver HIPAA-compliant healthcare solutions that improve "
            "care coordination, reduce administrative burden, and enhance "
            "patient outcomes."
        )
        positioning["themes"] = [
            "Patient-Centered Care",
            "Clinical Efficiency",
            "Interoperability",
            "Compliance & Security"
        ]

    else:
        positioning["tagline"] = "Technology Solutions That Drive Results"
        positioning["value_proposition"] = (
            "We deliver modern, scalable technology solutions that transform "
            "operations, reduce costs, and enable organizational success."
        )
        positioning["themes"] = [
            "Innovation",
            "Efficiency",
            "Reliability",
            "Partnership"
        ]

    # Key differentiators from intelligence
    leverage_points = client_intel.get("leverage_points", [])
    positioning["key_differentiators"] = [
        {
            "differentiator": lp["point"],
            "evidence": lp["evidence"],
            "competitive_advantage": "Unique capability"
        }
        for lp in leverage_points[:5]
    ]

    # Proof points — populated from matched past projects
    positioning["proof_points"] = []
    if matched_projects:
        for mp in matched_projects[:4]:
            if mp.get("key_metrics"):
                positioning["proof_points"].append(
                    f"{mp['client']}: {mp['key_metrics'][0]}"
                )
            else:
                positioning["proof_points"].append(
                    f"Proven delivery for {mp['client']} ({mp['industry']})"
                )
    # Always include general capability proof points
    positioning["proof_points"].extend([
        "39 years of continuous technology delivery since 1986",
        "200+ professionals across 5 offices"
    ])

    return positioning

domain = domain_context.get("selected_domain", "default")
positioning = develop_positioning(domain, client_intel, eval_criteria)

# Build theme-to-eval-factor mapping
theme_eval_mapping = build_theme_eval_mapping(positioning["themes"], eval_criteria)

# Build section-to-theme mandates
section_theme_mandates = build_section_theme_mandates(eval_criteria)
```

### Step 4: Generate Evaluator Messages

```python
def generate_evaluator_messages(personas, positioning, eval_criteria):
    """Generate targeted messages for each evaluator role."""
    messages = {}

    evaluator_templates = {
        "TECHNICAL": {
            "headline": "Architecture Built for Scale and Security",
            "key_message": (
                "Our modern, cloud-native architecture leverages proven "
                "technologies and industry best practices to deliver a "
                "scalable, secure, and maintainable solution."
            ),
            "proof_points": [
                "Microservices architecture for flexibility",
                "Zero-trust security model",
                "API-first design for integration"
            ]
        },
        "FINANCIAL": {
            "headline": "Maximizing Value, Minimizing Risk",
            "key_message": (
                "Our AI-assisted development approach delivers 35% efficiency "
                "gains, reducing project costs while accelerating delivery "
                "and improving quality."
            ),
            "proof_points": [
                "35% cost savings through AI efficiency",
                "Predictable fixed-price delivery",
                "Reduced long-term maintenance costs"
            ]
        },
        "RISK": {
            "headline": "Comprehensive Risk Management",
            "key_message": (
                "We identify, assess, and mitigate risks proactively with "
                "documented mitigation strategies for every identified risk, "
                "ensuring project success."
            ),
            "proof_points": [
                "Per-requirement risk assessment",
                "Documented mitigation strategies",
                "Compliance verification checkpoints"
            ]
        },
        "EXECUTIVE": {
            "headline": "Strategic Partnership for Success",
            "key_message": (
                "We're not just a vendor—we're a strategic partner committed "
                "to your organization's long-term success through modern "
                "technology and dedicated support."
            ),
            "proof_points": [
                "Executive sponsor engagement",
                "Strategic alignment reviews",
                "Continuous improvement commitment"
            ]
        },
        "OPERATIONAL": {
            "headline": "Designed for Real Users",
            "key_message": (
                "Our user-centered design approach ensures intuitive interfaces, "
                "minimal training requirements, and high user adoption rates "
                "from day one."
            ),
            "proof_points": [
                "Intuitive, modern UI",
                "Comprehensive training program",
                "Responsive support team"
            ]
        }
    }

    for role, template in evaluator_templates.items():
        # Adjust weight based on evaluation criteria
        eval_factors = eval_criteria.get("evaluation_factors", [])
        weight = 0.2  # Default

        for factor in eval_factors:
            if role.lower() in factor.get("name", "").lower():
                weight = factor.get("weight_normalized", 20) / 100

        messages[role] = {
            "role": role,
            "weight": weight,
            "headline": template["headline"],
            "key_message": template["key_message"],
            "proof_points": template["proof_points"],
            "call_to_action": f"See detailed {role.lower()} specifications in our proposal."
        }

    return messages

evaluator_messages = generate_evaluator_messages(personas, positioning, eval_criteria)
```

### Step 5: Determine Content Priority Order

```python
def determine_content_priority(eval_criteria, client_intel):
    """Determine content priority based on evaluation weights and pain points."""
    priorities = []

    # Get evaluation weights
    eval_factors = eval_criteria.get("evaluation_factors", [])
    for factor in eval_factors:
        priorities.append({
            "topic": factor.get("name"),
            "eval_weight": factor.get("weight_normalized", 10) / 100,
            "pain_weight": 0.2,  # Default pain weight
            "combined_score": 0
        })

    # Adjust for pain points from intelligence
    pain_points = client_intel.get("intelligence", {}).get("challenges", [])
    if pain_points:
        for priority in priorities:
            topic_lower = priority["topic"].lower()
            for pain in pain_points if isinstance(pain_points, list) else []:
                if isinstance(pain, str) and topic_lower in pain.lower():
                    priority["pain_weight"] = 0.4  # Higher pain weight

    # Calculate combined score (60% eval + 40% pain)
    for priority in priorities:
        priority["combined_score"] = (
            priority["eval_weight"] * 0.6 +
            priority["pain_weight"] * 0.4
        )

    # Sort by combined score
    priorities.sort(key=lambda x: x["combined_score"], reverse=True)

    return priorities

content_priority = determine_content_priority(eval_criteria, client_intel)
```

### Step 6: Write Output

```python
positioning_output = {
    "generated_at": datetime.now().isoformat(),
    "core_positioning": positioning,
    "evaluator_messages": evaluator_messages,
    "content_priority_order": [
        {
            "rank": i + 1,
            "topic": p["topic"],
            "eval_weight": p["eval_weight"],
            "pain_weight": p["pain_weight"],
            "combined_score": round(p["combined_score"], 3)
        }
        for i, p in enumerate(content_priority[:10])
    ],
    "win_themes": positioning["themes"],
    "theme_eval_mapping": theme_eval_mapping,
    "section_theme_mandates": section_theme_mandates,
    "key_messages": {
        "executive_summary": positioning["value_proposition"],
        "technical_approach": evaluator_messages.get("TECHNICAL", {}).get("key_message"),
        "management_approach": evaluator_messages.get("EXECUTIVE", {}).get("key_message"),
        "pricing_rationale": evaluator_messages.get("FINANCIAL", {}).get("key_message")
    },
    "matched_projects": [
        {
            "rank": mp["rank"],
            "project_number": mp["project_number"],
            "client": mp["client"],
            "industry": mp["industry"],
            "title": mp["title"],
            "relevance_score": mp["relevance_score"],
            "score_breakdown": mp["score_breakdown"],
            "technologies": mp["technologies"],
            "key_metrics": mp["key_metrics"],
            "timeline": mp["timeline"],
            "description_summary": mp["description_summary"],
            "has_metrics_table": mp["has_metrics_table"],
            "has_quote": mp["has_quote"],
            "relevance_statement": mp["relevance_statement"]
        }
        for mp in matched_projects
    ],
    "match_metadata": {
        "total_projects_evaluated": len(scored_projects) if 'scored_projects' in dir() else 0,
        "projects_selected": len(matched_projects),
        "match_warning": match_warning,
        "source_file": "Past_Projects.md",
        "rfp_domain": domain_context.get("selected_domain", "default"),
        "rfp_industry": domain_context.get("industry", "")
    },
    "historical_patterns": historical_patterns,
    "matched_evidence": matched_evidence,
    "evidence_summary": {
        "total_available": sum(len(items) for items in evidence_library.get("categories", {}).values()) if evidence_library else 0,
        "populated": len([e for e in matched_evidence]),
        "unpopulated": sum(1 for cat in evidence_library.get("categories", {}).values() for item in cat if "[USER INPUT" in str(item)) if evidence_library else 0
    }
}

write_json(f"{folder}/shared/bid/POSITIONING_OUTPUT.json", positioning_output)
```

### Step 7: Report Results

```python
log(f"""
🎯 Strategic Positioning Complete
=================================
Tagline: {positioning["tagline"]}

Win Themes:
{chr(10).join(f"  • {theme}" for theme in positioning["themes"])}

Key Differentiators:
{chr(10).join(f"  • {d['differentiator']}" for d in positioning["key_differentiators"][:3])}

Content Priority (Top 5):
{chr(10).join(f"  {i+1}. {p['topic']} ({p['combined_score']:.1%})" for i, p in enumerate(content_priority[:5]))}

📋 Past Performance Matching:
  Projects Evaluated: {positioning_output["match_metadata"]["total_projects_evaluated"]}
  Projects Selected: {len(matched_projects)}
  Top Matches:
{chr(10).join(f"  #{mp['rank']}. {mp['client']} ({mp['industry']}) — Score: {mp['relevance_score']}" for mp in matched_projects[:5])}
{f"  ⚠️ WARNING: {match_warning}" if match_warning else "  ✅ Strong matches found"}

🔗 Theme-Eval Mapping:
{chr(10).join(f"  • {theme} → {len(factors)} eval factors" for theme, factors in theme_eval_mapping.items())}

📋 Section-Theme Mandates:
{chr(10).join(f"  • {section}: {len(info['themes'])} themes, {len(info['eval_factors'])} eval factors" for section, info in section_theme_mandates.items())}

Output: {folder}/shared/bid/POSITIONING_OUTPUT.json
""")
```

## Quality Checklist

- [ ] `POSITIONING_OUTPUT.json` created in `shared/bid/`
- [ ] Core positioning defined (tagline, value prop)
- [ ] 5 evaluator messages generated
- [ ] Content priority determined
- [ ] Win themes established
- [ ] `theme_eval_mapping` links every theme to >= 1 eval factor
- [ ] `section_theme_mandates` covers all major bid sections
- [ ] `evaluator_messages` aligned with themes
- [ ] `Past_Projects.md` parsed and scored against RFP domain/industry/technology
- [ ] `matched_projects[]` included in output with 5-8 ranked projects
- [ ] Each matched project has score_breakdown, key_metrics, relevance_statement
- [ ] Override projects from company-profile.json applied (if any)
- [ ] Zero-match fallback handled (warning if <3 strong matches)
- [ ] `matched_evidence` populated from `config-win/evidence-library.json`
- [ ] `evidence_summary` includes total_available, populated, unpopulated counts
