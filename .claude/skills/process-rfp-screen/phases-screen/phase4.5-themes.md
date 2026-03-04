---
name: phase4.5-themes
expert-role: Positioning Strategist
domain-expertise: Win theme development, competitive positioning, proposal strategy, Shipley/APMP best practices
---

# Phase 4.5: Preliminary Win Theme Derivation

**Expert Role:** Positioning Strategist

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

# Partnership/Longevity — always-on baseline
company_name = company.get("company_name", "Resource Data, Inc.")
years = company.get("years_in_business", 39)
employees = company.get("employees", "200+")
candidates.append({
    "name": "Established Partnership & Organizational Stability",
    "category": "organizational_strength",
    "score": 3,  # Low priority baseline
    "confidence": "low",
    "evidence": [
        f"{company_name}: {years} years in business",
        f"{employees} employees"
    ],
    "rationale": f"Long-standing organization with {years} years of continuous operation and a deep bench. Always-available baseline theme."
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

### Step 3b: Generate Compelling Framing

For each selected theme, synthesize its evidence + rationale into a 1–2 sentence evaluator-facing pitch. This framing connects RDI's specific capabilities to the client's stated needs — it is the "elevator pitch" for the theme.

```python
# Build context for framing generation
rfp_title = rfp_summary.get("rfp_title", "")
client_name = rfp_summary.get("client_name", "Unknown")
scope_summary = ", ".join(scope_keywords[:8])

for theme in selected:
    # Gather all available context for this theme
    theme_evidence = theme.get("evidence", [])
    theme_rationale = theme.get("rationale", "")
    theme_name = theme.get("name", "")

    # LLM prompt: generate compelling framing
    framing_prompt = f"""Generate a 1-2 sentence compelling framing for this win theme.

RFP: "{rfp_title}" for {client_name}
Scope keywords: {scope_summary}
Theme: {theme_name}
Rationale: {theme_rationale}
Evidence: {'; '.join(theme_evidence)}
Matched projects: {', '.join(p.get('title', '') for p in matched_projects[:5])}

Requirements:
- Use SPECIFIC evidence (project names, metrics, numbers) — not generic claims
- Address the CLIENT'S need, not just RDI's capability (connect to the RFP scope)
- 1-2 sentences max, persuasive professional tone
- Reference matched projects and concrete deliverables where possible
- Do NOT use filler phrases like "uniquely positioned" or "best-in-class"

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

    # Check each HIGH priority for coverage
    covered = []
    uncovered = []
    for priority in high_priorities:
        priority_name = priority.get("name", "")
        linked_kws = [kw.lower() for kw in priority.get("linked_scope_keywords", [])]

        # A priority is "covered" if any of its linked keywords appear in theme keywords
        is_covered = any(
            any(kw in tk for tk in theme_keywords)
            for kw in linked_kws
        ) if linked_kws else False

        # Also check if the priority name itself is referenced
        if not is_covered:
            is_covered = any(
                word in " ".join(theme_keywords)
                for word in priority_name.lower().split()
                if len(word) > 3  # Skip short words like "and", "for"
            )

        if is_covered:
            covered.append(priority_name)
        else:
            uncovered.append(priority_name)

    buyer_priority_coverage = {
        "covered": covered,
        "uncovered": uncovered,
        "coverage_ratio": f"{len(covered)}/{len(high_priorities)}"
    }

    if uncovered:
        log(f"  Buyer Priority Coverage: {len(covered)}/{len(high_priorities)} HIGH priorities addressed")
        log(f"  THEME GAPS — full pipeline should address:")
        for u in uncovered:
            log(f"    - {u}")
else:
    buyer_priority_coverage = {"covered": [], "uncovered": [], "coverage_ratio": "N/A"}
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
    "methodology": "Rule-based theme derivation from screening data. Categories: domain_expertise, technical_capability, organizational_strength, client_alignment. Max 2 per category, top 3-4 selected by score.",
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
