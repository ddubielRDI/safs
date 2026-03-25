---
name: phase4.5-themes
expert-role: Positioning Strategist
domain-expertise: Win theme development, competitive positioning, proposal strategy, Shipley/APMP best practices
skill: capture-strategist
sub-skill: competitive-positioning
---

# Phase 4.5: Preliminary Win Theme Derivation

**Purpose:** Derive 3–4 directional win themes from screening data so a GO result comes with positioning hints. These are preliminary — the full pipeline's Phase 8.0 generates production themes with evaluation factor mapping and CVD format.

**Inputs:**
- `{folder}/screen/rfp-summary.json` — Phase 1 output (scope keywords, industry domain, eval criteria)
- `{folder}/screen/go-nogo-score.json` — Phase 2 output (assessment areas with scores/weights/evidence, overall risks & mitigations)
- `{folder}/screen/client-intel-snapshot.json` — Phase 3 output (may not exist if --quick)
- `{folder}/screen/compliance-check.json` — Phase 4a output (compliance status counts)
- `{folder}/screen/past-projects-match.json` — Phase 4b output (matched projects, match quality)
- `config-win/company-profile.json` — Company capabilities, services, locations

**Required Output:**
- `{folder}/screen/preliminary-themes.json` (>1KB)

---

## Instructions

### Step 1: Load All Prior Phase Outputs

```python
rfp_summary = read_json(f"{folder}/screen/rfp-summary.json")
go_nogo = read_json(f"{folder}/screen/go-nogo-score.json")
compliance = read_json(f"{folder}/screen/compliance-check.json")
past_matches = read_json(f"{folder}/screen/past-projects-match.json")
company = read_json(COMPANY_PROFILE)

# Optional — may not exist if --quick mode
client_intel = read_json_safe(f"{folder}/screen/client-intel-snapshot.json")
intel_available = client_intel is not None and client_intel.get("status") == "complete"

# Extract key fields
rfp_domain = (rfp_summary.get("industry_domain") or "").lower()
scope_keywords = [kw.lower() for kw in rfp_summary.get("scope_keywords", [])]
eval_criteria = rfp_summary.get("evaluation_criteria", [])

# New Phase 1 enriched fields (backward-compatible)
buyer_priorities = rfp_summary.get("buyer_priorities", [])
required_technologies = rfp_summary.get("required_technologies", [])
matched_projects = past_matches.get("matched_projects", [])
match_quality = past_matches.get("match_quality", "weak")
compliance_summary = compliance.get("summary", {})
pass_count = compliance_summary.get("pass", 0)
# Helper to find assessment area by name prefix
def find_area(areas, name_prefix):
    for a in areas:
        if a.get("name", "").lower().startswith(name_prefix.lower()):
            return a
    return {}

assessment_areas = go_nogo.get("assessment_areas", [])
overall_risks = go_nogo.get("overall_risks", [])
overall_mitigations = go_nogo.get("overall_mitigations", [])
```

### Skill Integration: Capture-Strategist & Competitive-Positioning Framework Application (MANDATORY)

The **capture-strategist** and **competitive-positioning** sub-skill are loaded in context. Apply these frameworks throughout this phase:

**Discriminator Type Classification:** For each theme candidate, classify its discriminator type:
- **Hard Discriminator** — provable, measurable, verifiable (patent, certification, metric, tool). Example: "Esri Gold Partner since 1992" is hard; "experienced team" is soft.
- **Soft Discriminator** — qualitative, subjective, narrative-dependent. These are weaker and easier for competitors to claim.
- Per competitive-positioning sub-skill, prefer Hard discriminators; Soft discriminators need quantified evidence to be credible.

**APMP Theme Maturity Assessment:** Assess each theme's maturity level:
1. **Feature** — states a capability ("We have GIS expertise")
2. **Benefit** — connects capability to client value ("Our GIS expertise reduces implementation time")
3. **Proof** — provides evidence for the benefit ("In 3 similar projects, we reduced implementation time by 40%")
4. **Discriminator** — proves competitors can't match ("Only Esri Gold Partner in Alaska with 34 years of municipal GIS delivery")
- **Only select themes that reach at least "Proof" maturity** — they must have evidence from past projects, metrics, or certifications

**Ghost Strategy Element:** For each selected theme, identify what competitor weakness it exploits:
- What would a competitor need to match this? (Time, certification, experience, partnership)
- This informs Phase 8.0 ghost strategy in the full pipeline

**CVD Formula (Capability-Value-Differentiator):** Theme framings in Step 3b MUST follow this structure:
- **Capability:** What we can do (specific, named — not generic)
- **Value:** Why that matters to THIS evaluator (tied to buyer priority)
- **Differentiator:** Why competitors can't match it (ghost element)

**Anti-Pattern Guards:**
- "Strategy-free proposals" — themes without win strategy (just listing capabilities)
- "Incumbent complacency" — themes that assume past performance alone wins
- "Feature-dumping" — themes at Feature maturity level without Proof
- Generic themes interchangeable with any company

---

### Step 2: Build Candidate Theme Pool

Build candidates across 4 categories. Each candidate gets a score, evidence list, and rationale.

```python
candidates = []
```

#### Category 1: Domain Expertise Themes

```python
# Trigger: industry domain matches a known template
domain_templates = {
    "education": {
        "name": "K-12/Higher Education Domain Expertise",
        "keywords": ["education", "school", "student", "learning", "curriculum", "k-12", "higher education", "university"]
    },
    "healthcare": {
        "name": "Healthcare & Human Services Expertise",
        "keywords": ["healthcare", "health", "clinical", "patient", "hipaa", "medical", "ehr"]
    },
    "government": {
        "name": "Government Solutions & Compliance Expertise",
        "keywords": ["government", "federal", "state", "agency", "public sector", "compliance"]
    },
    "natural resources": {
        "name": "Natural Resources & Environmental Expertise",
        "keywords": ["natural resources", "environmental", "wildlife", "fisheries", "forestry", "conservation"]
    },
    "transportation": {
        "name": "Transportation & Infrastructure Expertise",
        "keywords": ["transportation", "transit", "infrastructure", "dot", "highway", "fleet"]
    }
}

if rfp_domain in domain_templates:
    template = domain_templates[rfp_domain]

    # Count domain projects from past matches
    domain_projects = [p for p in matched_projects if (p.get("industry") or "").lower() == rfp_domain]
    domain_count = len(domain_projects)

    # Score: 10 if 2+ domain projects, 5 if 1, 2 baseline
    if domain_count >= 2:
        score = 10
        confidence = "high"
    elif domain_count == 1:
        score = 5
        confidence = "medium"
    else:
        score = 2
        confidence = "low"

    evidence = []
    if domain_count > 0:
        evidence.append(f"{domain_count} past project(s) in {rfp_domain} domain")
        for dp in domain_projects[:2]:
            evidence.append(f"Project: {dp.get('title', 'Untitled')} (score: {dp.get('relevance_score', 0)})")
    if match_quality in ("strong", "moderate"):
        evidence.append(f"Overall match quality: {match_quality}")

    candidates.append({
        "name": template["name"],
        "category": "domain_expertise",
        "score": score,
        "confidence": confidence,
        "evidence": evidence,
        "rationale": f"RFP targets {rfp_domain} domain. {'Strong' if domain_count >= 2 else 'Some' if domain_count == 1 else 'Limited'} demonstrated experience in this sector."
    })
```

#### Category 1b: Consulting & Discovery Methodology Themes

```python
# Business Analysis / Stakeholder Discovery theme -- addresses RFPs that emphasize
# requirements gathering, stakeholder engagement, or discovery phases. This draws
# from the IT Consulting service line rather than only delivered GIS/IT systems.
discovery_keywords = ["stakeholder", "discovery", "requirements", "needs assessment",
                      "interviews", "workshops", "user research", "business analysis",
                      "gap analysis", "current state", "future state", "roadmap"]
disc_matches = [kw for kw in discovery_keywords if any(kw in sk for sk in scope_keywords)]
if disc_matches:
    # Also check buyer priorities for discovery-related priorities
    disc_priorities = [bp for bp in buyer_priorities
                       if any(kw in bp.get("name", "").lower() for kw in
                              ["stakeholder", "discovery", "requirements", "consulting"])]
    disc_evidence = [f"Scope keywords: {', '.join(disc_matches)}"]
    if disc_priorities:
        disc_evidence.append(f"Buyer priority: {disc_priorities[0].get('name', '')}")
    # Check company profile for consulting capabilities
    company_services = []
    for cat, svcs in company.get("services", {}).items():
        company_services.extend(svcs)
    consulting_svcs = [s for s in company_services if any(
        kw in s.lower() for kw in ["business analysis", "strategic planning", "consulting",
                                    "change management", "project management"])]
    if consulting_svcs:
        disc_evidence.append(f"RDI capabilities: {', '.join(consulting_svcs[:3])}")
    candidates.append({
        "name": "Structured Discovery & Stakeholder Engagement Methodology",
        "category": "domain_expertise",
        "score": min(len(disc_matches) * 2 + len(disc_priorities) * 3, 10),
        "confidence": "medium" if not disc_priorities else "high",
        "evidence": disc_evidence,
        "rationale": f"RFP scope includes {len(disc_matches)} discovery/consulting keywords, suggesting a requirements-driven engagement. RDI's Business Analysis and IT Strategic Planning capabilities address this need."
    })
```

#### Category 2: Technical Capability Themes

```python
scope_text = " ".join(scope_keywords)

# Modernization theme
modernization_keywords = ["modernization", "migration", "cloud", "digital transformation", "legacy", "upgrade", "replacement", "transition"]
mod_matches = [kw for kw in modernization_keywords if any(kw in sk for sk in scope_keywords)]
if mod_matches:
    mod_evidence = [f"Scope keywords: {', '.join(mod_matches)}"]
    tech_area = find_area(assessment_areas, "Technical Capability")
    if tech_area.get("score", 0) >= 60:  # 60/100 equivalent to 12/20
        mod_evidence.append(f"Technical Capability score: {tech_area.get('score')}/100")
    candidates.append({
        "name": "Proven Modernization & Migration Methodology",
        "category": "technical_capability",
        "score": min(len(mod_matches) * 3, 12),
        "confidence": "high" if len(mod_matches) >= 3 else "medium",
        "evidence": mod_evidence,
        "rationale": f"RFP scope includes {len(mod_matches)} modernization-related keywords, suggesting a legacy-to-modern transformation opportunity."
    })

# Data/Analytics theme
data_keywords = ["data", "analytics", "ai", "machine learning", "database", "gis", "reporting", "business intelligence", "dashboard", "visualization"]
data_matches = [kw for kw in data_keywords if any(kw in sk for sk in scope_keywords)]
if data_matches:
    data_evidence = [f"Scope keywords: {', '.join(data_matches)}"]
    candidates.append({
        "name": "Data-Driven Solutions & Analytics Capability",
        "category": "technical_capability",
        "score": min(len(data_matches) * 3, 12),
        "confidence": "high" if len(data_matches) >= 3 else "medium",
        "evidence": data_evidence,
        "rationale": f"RFP scope includes {len(data_matches)} data/analytics keywords, indicating data management or analytical deliverables."
    })

# Security/Compliance theme
security_keywords = ["security", "cybersecurity", "compliance", "audit", "risk management", "authorization", "authentication", "encryption", "fedramp", "fisma"]
sec_matches = [kw for kw in security_keywords if any(kw in sk for sk in scope_keywords)]
if sec_matches or pass_count >= 3:
    sec_evidence = []
    if sec_matches:
        sec_evidence.append(f"Security scope keywords: {', '.join(sec_matches)}")
    if pass_count >= 3:
        sec_evidence.append(f"Compliance quick-check: {pass_count} PASS items")
    sec_score = min(len(sec_matches) * 2 + (3 if pass_count >= 3 else 0), 12)
    candidates.append({
        "name": "Security-First Architecture & Compliance Readiness",
        "category": "technical_capability",
        "score": sec_score,
        "confidence": "high" if sec_matches and pass_count >= 3 else "medium",
        "evidence": sec_evidence,
        "rationale": f"{'Security scope keywords detected' if sec_matches else ''}{' and ' if sec_matches and pass_count >= 3 else ''}{f'{pass_count} compliance items verified' if pass_count >= 3 else ''}."
    })

# Technology Depth theme (from tech_intelligence RDI alignment) — Batch 3
tech_intelligence = rfp_summary.get("tech_intelligence", {})
rdi_alignment = tech_intelligence.get("rdi_alignment", {})
strong_matches = rdi_alignment.get("strong_match", [])
# Filter to established/mature technologies with strong RDI match
established_strong = [t for t in strong_matches if isinstance(t, dict) and t.get("maturity") in ("established", "mature")]
if not established_strong and isinstance(strong_matches, list):
    # Fallback: treat as list of strings for backward compat
    established_strong = strong_matches if len(strong_matches) >= 2 else []

if len(established_strong) >= 2:
    tech_depth_evidence = [f"Strong RDI alignment on {len(established_strong)} established technologies"]
    for t in established_strong[:3]:
        if isinstance(t, dict):
            tech_depth_evidence.append(f"{t.get('name', 'Unknown')}: {t.get('rdi_capability', 'documented')}")
        else:
            tech_depth_evidence.append(f"{t}: strong match")

    # Annotate with tech gaps from Phase 4 past-projects-match
    tech_gaps = past_matches.get("tech_gap_analysis", {}).get("technologies_without_coverage", [])
    tech_gaps_note = ""
    if tech_gaps:
        tech_depth_evidence.append(f"Tech gaps to address in proposal: {', '.join(tech_gaps[:3])}")
        tech_gaps_note = f" Gaps to address: {', '.join(tech_gaps[:3])}."

    candidates.append({
        "name": "Technology Depth & Platform Expertise",
        "category": "technical_capability",
        "score": min(len(established_strong) * 3 + 2, 12),
        "confidence": "high" if len(established_strong) >= 3 else "medium",
        "evidence": tech_depth_evidence,
        "rationale": f"{len(established_strong)} established technologies in the RFP's required stack have strong RDI alignment, demonstrating deep platform expertise rather than superficial familiarity.{tech_gaps_note}",
        "tech_gaps_to_address": tech_gaps[:5]
    })
```

#### Category 3: Organizational Strength Themes

```python
# Geographic/Regional theme
strategic_area = find_area(assessment_areas, "Strategic Fit")
strategic_score = strategic_area.get("score", 0)
strategic_rationale = strategic_area.get("rationale", "")

if strategic_score >= 60 or "geographic" in strategic_rationale.lower() or "proximity" in strategic_rationale.lower():
    geo_evidence = [f"Strategic Fit score: {strategic_score}/100"]
    if "geographic" in strategic_rationale.lower() or "proximity" in strategic_rationale.lower():
        geo_evidence.append("Geographic/regional proximity detected")

    # Check company locations
    locations = company.get("locations", [])
    if locations:
        location_strs = []
        for loc in locations[:3]:
            if isinstance(loc, dict):
                location_strs.append(f"{loc.get('city', '')}, {loc.get('state', '')}")
            else:
                location_strs.append(str(loc))
        geo_evidence.append(f"Office locations: {'; '.join(location_strs)}")

    candidates.append({
        "name": "Regional Presence & Local Responsiveness",
        "category": "organizational_strength",
        "score": min(strategic_score // 10, 10),  # Normalize from 0-100 range to theme scoring
        "confidence": "high" if strategic_score >= 75 else "medium",
        "evidence": geo_evidence,
        "rationale": "Strong strategic alignment with geographic and/or jurisdictional proximity to the client."
    })

# Proven Results theme
projects_with_metrics = [p for p in matched_projects if p.get("key_metrics")]
if projects_with_metrics:
    results_evidence = [f"{len(projects_with_metrics)} matched project(s) with quantified results"]
    for pm in projects_with_metrics[:2]:
        metrics_preview = pm.get("key_metrics", [])[:2]
        results_evidence.append(f"{pm.get('title', 'Untitled')}: {'; '.join(str(m) for m in metrics_preview)}")

    candidates.append({
        "name": "Measurable Outcomes & Proven Track Record",
        "category": "organizational_strength",
        "score": min(len(projects_with_metrics) * 4, 10),
        "confidence": "high" if len(projects_with_metrics) >= 2 else "medium",
        "evidence": results_evidence,
        "rationale": f"{len(projects_with_metrics)} past projects with quantified results demonstrate ability to deliver measurable outcomes."
    })

# Partnership/Longevity — enriched from Past_Projects.md Company Intelligence
company_name = company.get("company_name", "Resource Data, Inc.")
years = company.get("years_in_business", 39)
employees = company.get("employees", "200+")

# Gather enriched evidence from past-projects-match.json (parsed from Past_Projects.md)
partnerships = past_matches.get("partnerships", [])
awards = past_matches.get("awards", [])
contract_vehicles = past_matches.get("contract_vehicles_in_state", [])
existing_rel = past_matches.get("existing_relationship", {})
total_known_clients = past_matches.get("total_known_clients", 0)

longevity_evidence = [
    f"{company_name}: {years} years in business, employee-owned",
    f"{employees} employees, 1,000+ projects completed, 100+ clients served"
]
longevity_score = 5  # Elevated baseline due to enriched evidence

if partnerships:
    longevity_evidence.append(f"Technology partnerships: {'; '.join(partnerships[:3])}")
    longevity_score += 2
if awards:
    longevity_evidence.append(f"Awards: {'; '.join(awards[:2])}")
    longevity_score += 1
if existing_rel.get("found"):
    longevity_evidence.append(f"Existing client relationship: {existing_rel.get('matched_client', '')}")
    longevity_score += 2
if contract_vehicles:
    longevity_evidence.append(f"Contract vehicles in RFP state: {'; '.join(contract_vehicles[:2])}")
    longevity_score += 2

candidates.append({
    "name": "Established Partnership & Organizational Stability",
    "category": "organizational_strength",
    "score": min(longevity_score, 12),
    "confidence": "high" if longevity_score >= 8 else "medium" if longevity_score >= 5 else "low",
    "evidence": longevity_evidence,
    "rationale": f"Employee-owned organization with {years} years of continuous operation, {total_known_clients}+ documented client relationships, and deep technology partnerships. {'Existing relationship with the client strengthens this theme.' if existing_rel.get('found') else 'Always-available baseline theme.'}"
})

# Award-Winning Solutions theme — only if awards are present and relevant
if awards and len(awards) >= 2:
    candidates.append({
        "name": "Award-Winning Quality & Innovation",
        "category": "organizational_strength",
        "score": min(len(awards) * 2, 8),
        "confidence": "medium",
        "evidence": [f"Award: {a}" for a in awards[:3]],
        "rationale": f"{len(awards)} documented awards including Esri Partner of the Year and Special Achievement in GIS recognitions demonstrate consistent delivery excellence."
    })
```

#### Category 4: Client Alignment Themes (requires Phase 3 intel)

```python
if intel_available:
    intelligence = client_intel.get("intelligence", {})

    # Technology Alignment theme
    client_tech = intelligence.get("technology_stack", [])
    if client_tech:
        tech_overlap = [t for t in client_tech if any(kw in t.lower() for kw in scope_keywords)]
        if tech_overlap:
            candidates.append({
                "name": "Technology Stack Alignment & Integration Expertise",
                "category": "client_alignment",
                "score": min(len(tech_overlap) * 3, 10),
                "confidence": "high" if len(tech_overlap) >= 3 else "medium",
                "evidence": [
                    f"Client technologies: {', '.join(client_tech[:5])}",
                    f"Overlap with RFP scope: {', '.join(tech_overlap[:3])}"
                ],
                "rationale": f"{len(tech_overlap)} client technologies overlap with RFP scope keywords, indicating alignment between existing infrastructure and proposed solution."
            })

    # Strategic Initiative Alignment theme
    strategic_initiatives = intelligence.get("strategic_initiatives", [])
    if strategic_initiatives:
        candidates.append({
            "name": "Strategic Initiative Alignment",
            "category": "client_alignment",
            "score": min(len(strategic_initiatives) * 3, 9),
            "confidence": "medium",
            "evidence": [
                f"Client strategic initiatives: {', '.join(str(si) for si in strategic_initiatives[:3])}"
            ],
            "rationale": f"Client has {len(strategic_initiatives)} strategic initiative(s) that align with the proposed scope of work."
        })
```

### Step 3: Score and Select Themes

```python
# Sort candidates by score descending
candidates.sort(key=lambda c: c["score"], reverse=True)

# Apply category diversity constraint: max 2 per category
selected = []
category_counts = {}

for candidate in candidates:
    cat = candidate["category"]
    if category_counts.get(cat, 0) >= 2:
        continue
    selected.append(candidate)
    category_counts[cat] = category_counts.get(cat, 0) + 1
    if len(selected) >= 4:
        break

# Ensure at least 3 themes if candidates exist
if len(selected) < 3 and len(candidates) > len(selected):
    for candidate in candidates:
        if candidate not in selected:
            selected.append(candidate)
            if len(selected) >= 3:
                break

# Assign final rank
for i, theme in enumerate(selected, 1):
    theme["rank"] = i
```

### Step 3a: Skill-Informed Theme Enrichment (MANDATORY)

After selecting themes algorithmically, enrich each with skill-mandated structured fields.

```python
for theme in selected:
    evidence = theme.get("evidence", [])
    has_metrics = any(
        any(c.isdigit() for c in str(e)) for e in evidence
    )
    has_named_project = any("project" in str(e).lower() for e in evidence)
    has_certification = any(
        kw in str(evidence).lower()
        for kw in ["partner", "certified", "award", "gold", "accredited"]
    )

    # 1. Discriminator type classification
    if has_certification or has_metrics:
        theme["discriminator_type"] = "Hard"
    else:
        theme["discriminator_type"] = "Soft"

    # 2. APMP maturity assessment
    if has_certification and has_metrics and has_named_project:
        theme["maturity_level"] = "Discriminator"
    elif has_named_project and (has_metrics or has_certification):
        theme["maturity_level"] = "Proof"
    elif has_named_project or has_metrics:
        theme["maturity_level"] = "Benefit"
    else:
        theme["maturity_level"] = "Feature"

    # 3. Ghost strategy element
    #    What would a competitor need to match this theme?
    ghost_element = ""
    if has_certification:
        ghost_element = "Requires equivalent partnership/certification (years to obtain)"
    elif has_metrics:
        ghost_element = "Requires comparable quantified project outcomes (cannot be fabricated)"
    elif theme.get("category") == "organizational_strength":
        ghost_element = "Requires equivalent organizational tenure and track record"
    else:
        ghost_element = "Competitor could potentially claim similar capability"
    theme["ghost_element"] = ghost_element

# Log maturity assessment
for theme in selected:
    log(f"  Theme #{theme['rank']}: {theme['name']}")
    log(f"    Discriminator: {theme['discriminator_type']} | Maturity: {theme['maturity_level']} | Ghost: {theme['ghost_element'][:60]}")

# Warn if any selected theme is below Proof maturity
feature_themes = [t for t in selected if t.get("maturity_level") == "Feature"]
if feature_themes:
    log(f"  WARNING: {len(feature_themes)} theme(s) at Feature maturity — need evidence to reach Proof level")
```

### Step 3b: Generate Compelling Framing

For each selected theme, synthesize its evidence + rationale into a 1–2 sentence evaluator-facing pitch. This framing connects RDI's specific capabilities to the client's stated needs — it is the "elevator pitch" for the theme.

```python
# Build context for framing generation
rfp_title = rfp_summary.get("rfp_title", "")
client_name = rfp_summary.get("client_name", "Unknown")
scope_summary = ", ".join(scope_keywords[:8])

# Load client tone from go-nogo pass-through for tone adaptation (Batch 3)
client_tone = go_nogo.get("client_tone", {})
primary_style = client_tone.get("primary_style", "formal_bureaucratic")
mirroring_vocab = client_tone.get("mirroring_vocabulary", [])
adaptation_rules = client_tone.get("adaptation_rules", {})
preferred_terms = adaptation_rules.get("preferred_terms", [])
avoid_terms = adaptation_rules.get("avoid_terms", [])
formality_level = adaptation_rules.get("formality_level", "formal")

for theme in selected:
    # Gather all available context for this theme
    theme_evidence = theme.get("evidence", [])
    theme_rationale = theme.get("rationale", "")
    theme_name = theme.get("name", "")

    # LLM prompt: generate CVD-structured compelling framing
    # Per competitive-positioning sub-skill, use Capability-Value-Differentiator formula
    discriminator_type = theme.get("discriminator_type", "Soft")
    maturity_level = theme.get("maturity_level", "Feature")
    ghost_element = theme.get("ghost_element", "")

    framing_prompt = f"""Generate a 1-2 sentence compelling framing for this win theme using the CVD (Capability-Value-Differentiator) formula.

RFP: "{rfp_title}" for {client_name}
Scope keywords: {scope_summary}
Theme: {theme_name}
Rationale: {theme_rationale}
Evidence: {'; '.join(theme_evidence)}
Matched projects: {', '.join(p.get('title', '') for p in matched_projects[:5])}
Discriminator type: {discriminator_type} ({"provable/measurable" if discriminator_type == "Hard" else "qualitative — strengthen with evidence"})
Maturity level: {maturity_level} (target: Proof or Discriminator)
Ghost element: {ghost_element}

CVD FORMULA (MANDATORY structure):
- CAPABILITY: What we can do (specific, named — not generic)
- VALUE: Why that matters to THIS evaluator (tied to buyer priority from RFP)
- DIFFERENTIATOR: Why competitors can't match it (reference the ghost element)

TONE ADAPTATION (from client tone analysis):
- Client communication style: {primary_style}
- Formality level: {formality_level}
- Mirror these client vocabulary terms where natural: {', '.join(mirroring_vocab[:10]) if mirroring_vocab else 'N/A'}
- Preferred terms to use: {', '.join(preferred_terms[:5]) if preferred_terms else 'N/A'}
- Terms to avoid: {', '.join(avoid_terms[:5]) if avoid_terms else 'N/A'}
- Match the client's register -- if the RFP is prescriptive and directive, be direct and confident;
  if the RFP is collaborative, emphasize partnership and co-creation language.

Requirements:
- Use SPECIFIC evidence (project names, metrics, numbers) — not generic claims
- Address the CLIENT'S need, not just RDI's capability (connect to the RFP scope)
- 1-2 sentences max, persuasive professional tone adapted to client style above
- Reference matched projects and concrete deliverables where possible
- Do NOT use filler phrases like "uniquely positioned" or "best-in-class"
- The framing should implicitly contain all three CVD elements in natural prose

Return ONLY the framing text, no labels or quotes."""

    framing = llm(framing_prompt).strip()
    theme["framing"] = framing
```

### Step 3c: Buyer Priority Coverage Check

After selecting themes, verify that the selected themes collectively address all HIGH buyer priorities. This signals to the full pipeline where theme gaps exist.

```python
# Check: do selected themes collectively reference all HIGH buyer priorities?
high_priorities = [p for p in buyer_priorities if p.get("importance") == "HIGH"]

if high_priorities:
    # Build set of all keywords referenced by selected themes
    theme_keywords = set()
    for theme in selected:
        # Gather keywords from theme evidence and name
        theme_name_lower = theme.get("name", "").lower()
        theme_keywords.add(theme_name_lower)
        for ev in theme.get("evidence", []):
            theme_keywords.update(ev.lower().split())

    # For each buyer priority, check if ANY selected theme's buyer_priority_mapping includes it
    covered = []
    partial = []
    uncovered = []
    for bp in high_priorities:
        bp_name = bp.get("name", "")
        linked_kws = [kw.lower() for kw in bp.get("linked_scope_keywords", [])]

        # Check direct theme mapping: linked keywords appear in theme keywords
        directly_covered = any(
            any(kw in tk for tk in theme_keywords)
            for kw in linked_kws
        ) if linked_kws else False

        # Also check if the priority name itself is directly referenced
        if not directly_covered:
            directly_covered = any(
                word in " ".join(theme_keywords)
                for word in bp_name.lower().split()
                if len(word) > 3  # Skip short words like "and", "for"
            )

        # Check if any theme evidence mentions the priority keywords (tangential coverage)
        tangentially_covered = any(
            bp_name.lower().split()[0] in " ".join(t.get("evidence", [])).lower()
            for t in selected
        ) if not directly_covered else False

        if directly_covered:
            coverage_status = "Covered"
            covered.append(bp_name)
        elif tangentially_covered:
            coverage_status = "Partial"
            partial.append(bp_name)
        else:
            coverage_status = "Uncovered"
            uncovered.append(bp_name)

    buyer_priority_coverage = {
        "covered": covered,
        "partial": partial,
        "uncovered": uncovered,
        "coverage_ratio": f"{len(covered)}/{len(high_priorities)}",
        "partial_count": len(partial)
    }

    if uncovered or partial:
        log(f"  Buyer Priority Coverage: {len(covered)}/{len(high_priorities)} HIGH priorities fully covered, {len(partial)} partial")
        if partial:
            log(f"  PARTIAL — themes tangentially address but lack dedicated evidence:")
            for p in partial:
                log(f"    - {p}")
        if uncovered:
            log(f"  THEME GAPS — full pipeline should address:")
            for u in uncovered:
                log(f"    - {u}")
else:
    buyer_priority_coverage = {"covered": [], "partial": [], "uncovered": [], "coverage_ratio": "N/A", "partial_count": 0}
```

### Step 3d: Evaluation Point Mapping (Batch 3)

For each selected theme, map it to evaluation criteria from `evaluation_model` and estimate point contribution. This provides visibility into which themes drive the most evaluation points and identifies orphan themes with zero alignment.

```python
evaluation_model = rfp_summary.get("evaluation_model", {})
point_allocation = evaluation_model.get("point_allocation", [])
total_available_points = sum(c.get("points", 0) for c in point_allocation) or 1000

if point_allocation:
    for theme in selected:
        theme_name_lower = theme.get("name", "").lower()
        theme_evidence_text = " ".join(str(e).lower() for e in theme.get("evidence", []))
        theme_rationale_lower = theme.get("rationale", "").lower()
        theme_text = f"{theme_name_lower} {theme_evidence_text} {theme_rationale_lower}"

        mapped_criteria = []
        for criterion in point_allocation:
            criterion_name = criterion.get("criterion", "")
            criterion_name_lower = criterion_name.lower()
            criterion_points = criterion.get("points", 0)
            subfactors = criterion.get("subfactors", [])

            # Check for keyword overlap between theme and criterion
            criterion_words = set(criterion_name_lower.split()) - {"the", "and", "or", "for", "a", "an", "in", "of", "to"}
            theme_words = set(theme_text.split()) - {"the", "and", "or", "for", "a", "an", "in", "of", "to"}
            overlap = criterion_words & theme_words

            # Also check subfactor alignment
            subfactor_overlap = []
            for sf in subfactors:
                sf_name = (sf.get("name", "") if isinstance(sf, dict) else str(sf)).lower()
                if any(w in theme_text for w in sf_name.split() if len(w) > 3):
                    subfactor_overlap.append(sf_name)

            if len(overlap) >= 1 or subfactor_overlap:
                # Determine alignment strength
                if len(overlap) >= 3 or (len(overlap) >= 1 and subfactor_overlap):
                    alignment_strength = "strong"
                elif len(overlap) >= 1 or subfactor_overlap:
                    alignment_strength = "moderate"
                else:
                    alignment_strength = "weak"

                mapped_criteria.append({
                    "criterion": criterion_name,
                    "points": criterion_points,
                    "alignment_strength": alignment_strength,
                    "keyword_overlap": list(overlap)[:5],
                    "subfactor_overlap": subfactor_overlap[:3]
                })

        # Estimate point contribution based on alignment strength
        estimated_contribution = 0
        for mc in mapped_criteria:
            strength_multiplier = {"strong": 0.4, "moderate": 0.2, "weak": 0.1}.get(mc["alignment_strength"], 0.1)
            estimated_contribution += mc["points"] * strength_multiplier

        theme["evaluation_alignment"] = {
            "mapped_criteria": mapped_criteria,
            "estimated_point_contribution": round(estimated_contribution),
            "point_contribution_pct": f"{(estimated_contribution / total_available_points) * 100:.1f}%"
        }

        # Log warning for themes with zero alignment
        if not mapped_criteria:
            log(f"  WARNING: Theme #{theme.get('rank', '?')} '{theme.get('name', '')}' has ZERO evaluation criteria alignment")

    # Compute aggregate theme point coverage
    total_theme_points = sum(
        t.get("evaluation_alignment", {}).get("estimated_point_contribution", 0)
        for t in selected
    )
    log(f"  Evaluation point coverage: ~{total_theme_points}/{total_available_points} points ({(total_theme_points / total_available_points) * 100:.1f}%)")
else:
    # No evaluation model available — skip point mapping
    for theme in selected:
        theme["evaluation_alignment"] = {
            "mapped_criteria": [],
            "estimated_point_contribution": 0,
            "point_contribution_pct": "N/A (no evaluation model)"
        }
    total_theme_points = 0
```

### Step 4: Write Output

```python
from datetime import datetime

preliminary_themes = {
    "phase": "4.5",
    "timestamp": datetime.now().isoformat(),
    "intel_available": intel_available,
    "total_candidates_evaluated": len(candidates),
    "themes_selected": len(selected),
    "category_distribution": category_counts,
    "themes": selected,
    "buyer_priority_coverage": buyer_priority_coverage,
    "evaluation_point_coverage": {
        "total_theme_points": total_theme_points,
        "total_available_points": total_available_points if point_allocation else "unknown",
        "coverage_pct": f"{(total_theme_points / total_available_points) * 100:.1f}%" if point_allocation else "N/A"
    },
    "client_tone_applied": bool(client_tone),
    "methodology": "Rule-based theme derivation from screening data. Categories: domain_expertise, technical_capability, organizational_strength, client_alignment. Max 2 per category, top 3-4 selected by score. Batch 3: evaluation point mapping, client tone adaptation, tech intelligence integration.",
    "note": "These are preliminary positioning hints. The full /process-rfp-win pipeline generates production themes with evaluation factor mapping, CVD format, and section-theme mandates."
}
write_json(f"{folder}/screen/preliminary-themes.json", preliminary_themes)
```

### Step 5: Report

```
PRELIMINARY WIN THEMES (Phase 4.5)
====================================
Intel Available: {intel_available}
Candidates Evaluated: {len(candidates)}
Themes Selected: {len(selected)}
Category Distribution: {category_counts}

Selected Themes:
{for each in selected:
  f"  #{rank}. {name} ({category}, {confidence} confidence, score: {score})"}

Output:
  screen/preliminary-themes.json
```

---

## Quality Checklist

- [ ] `preliminary-themes.json` written (>1KB)
- [ ] 3–4 themes selected with category diversity (max 2 per category)
- [ ] Each theme has: name, category, confidence, score, evidence[], rationale, framing
- [ ] Category 4 (client alignment) gracefully handles missing intel (zero candidates, not errors)
- [ ] Company profile loaded — services flattened (DICT not list) if used
- [ ] `intel_available` field correctly reflects Phase 3 status
- [ ] Methodology note included explaining these are preliminary, not production themes
- [ ] All prior phase outputs loaded and referenced (not re-analyzed from scratch)
- [ ] buyer_priorities loaded from rfp-summary.json (backward-compatible if absent)
- [ ] Buyer priority coverage check performed: HIGH priorities vs theme keywords
- [ ] buyer_priority_coverage included in output (covered, uncovered, coverage_ratio)
- [ ] Uncovered HIGH priorities flagged as "Theme gap — full pipeline should address"

### Intelligence Layer Integration Quality Checks (Batch 3)
- [ ] `client_tone` loaded from go-nogo-score.json pass-through
- [ ] TONE ADAPTATION instructions injected into framing prompt (style, vocabulary, preferred/avoid terms)
- [ ] `tech_intelligence.rdi_alignment` used to generate "Technology Depth" theme candidate (when 2+ established strong matches)
- [ ] Tech gaps from Phase 4 `tech_gap_analysis` annotated on relevant themes as `tech_gaps_to_address`
- [ ] Step 3d evaluation point mapping completed for all selected themes
- [ ] Each theme has `evaluation_alignment` with mapped_criteria, estimated_point_contribution, point_contribution_pct
- [ ] Themes with ZERO evaluation criteria alignment logged as warnings
- [ ] `evaluation_point_coverage` summary included in output JSON
- [ ] `client_tone_applied` flag included in output

### Skill Integration Quality Checks (capture-strategist + competitive-positioning)
- [ ] Each theme has discriminator_type: "Hard" (provable) or "Soft" (qualitative)
- [ ] Each theme has maturity_level: Feature/Benefit/Proof/Discriminator (target Proof+)
- [ ] Feature-level themes flagged with warning — need evidence to reach Proof
- [ ] Each theme has ghost_element: what competitor weakness this exploits
- [ ] Framing prompts use CVD formula (Capability-Value-Differentiator)
- [ ] Framing references specific evidence, not generic claims
- [ ] **Anti-pattern check:** No strategy-free themes, no incumbent complacency, no feature-dumping, no generic interchangeable themes
