---
name: phase5-estimation-win
expert-role: Project Estimator
domain-expertise: Effort estimation, resource planning, AI ratios
---

# Phase 5: Effort Estimation

## Expert Role

You are a **Project Estimator** with deep expertise in:
- Software effort estimation techniques
- Resource planning and allocation
- AI-assisted development ratios
- Agile story point estimation

## Purpose

Generate effort estimates with AI assistance ratios for all requirements.

## Inputs

- `{folder}/shared/requirements-normalized.json`
- `{folder}/shared/REQUIREMENT_RISKS.json`
- `{folder}/shared/domain-context.json`

## Required Outputs

- `{folder}/outputs/EFFORT_ESTIMATION.md` (>8KB)

## Instructions

### Step 1: Load Requirements

```python
requirements = read_json(f"{folder}/shared/requirements-normalized.json")
risks = read_json(f"{folder}/shared/REQUIREMENT_RISKS.json")
domain_context = read_json(f"{folder}/shared/domain-context.json")

all_reqs = requirements.get("requirements", [])

# V3-F1 fix: build risk_map as req_id → risk_level (string) via reverse lookup
# from rtm_risks[].linked_requirement_ids. The prior implementation read a
# non-existent `risks.requirements[]` shape; the canonical structure (per
# unified-rtm schema) is `risks.rtm_risks[]` with each risk carrying a list of
# linked_requirement_ids. We keep the highest severity per requirement so a
# requirement linked to both a HIGH and a LOW risk inherits HIGH.
risk_map = {}
RISK_LEVEL_ORDER = ["LOW", "MEDIUM", "HIGH", "CRITICAL"]
for r in risks.get("rtm_risks", []):
    risk_level = r.get("risk_level", "MEDIUM")
    for req_id in r.get("linked_requirement_ids", []):
        existing = risk_map.get(req_id, "LOW")
        if RISK_LEVEL_ORDER.index(risk_level) > RISK_LEVEL_ORDER.index(existing):
            risk_map[req_id] = risk_level
```

### Step 2: Define Estimation Parameters

```python
# Base effort by category (in hours)
CATEGORY_BASE_EFFORT = {
    "APP": 16,   # Application features
    "ENR": 24,   # Complex enrollment logic
    "BUD": 32,   # Financial calculations
    "STF": 16,   # Staff management
    "RPT": 20,   # Reporting
    "SEC": 24,   # Security features
    "INT": 40,   # Integration (complex)
    "UI": 12,    # UI components
    "TEC": 20,   # Technical infrastructure
    "ADM": 16    # Administration
}

# Risk multipliers
# V3-F1 fix: include CRITICAL — rtm_risks emit CRITICAL/HIGH/MEDIUM/LOW.
RISK_MULTIPLIERS = {
    "CRITICAL": 1.75,
    "HIGH": 1.5,
    "MEDIUM": 1.2,
    "LOW": 1.0
}

# AI assistance ratios (% of effort AI can assist with)
AI_ASSISTANCE_RATIOS = {
    "CODE_GENERATION": 0.40,    # 40% of coding
    "TESTING": 0.50,            # 50% of testing
    "DOCUMENTATION": 0.60,      # 60% of documentation
    "CODE_REVIEW": 0.30,        # 30% of reviews
    "DESIGN": 0.20              # 20% of design
}
```

### Step 3: Estimate Each Requirement

```python
def estimate_requirement(req, risk_map):
    """Estimate effort for a single requirement."""
    category = req.get("category", "TEC")
    req_id = req.get("canonical_id", "N/A")

    # Base effort
    base_hours = CATEGORY_BASE_EFFORT.get(category, 20)

    # Apply complexity factors
    text = req.get("text", "")
    complexity_factor = 1.0

    # Complexity indicators
    if len(text.split()) > 50:
        complexity_factor += 0.2
    if "integration" in text.lower():
        complexity_factor += 0.3
    if any(term in text.lower() for term in ["real-time", "batch", "async"]):
        complexity_factor += 0.2
    if any(term in text.lower() for term in ["report", "export", "dashboard"]):
        complexity_factor += 0.15

    # Apply risk multiplier
    # V3-F1 fix: risk_map values are now risk-level strings (not dicts).
    risk_level = risk_map.get(req_id, "MEDIUM")
    risk_multiplier = RISK_MULTIPLIERS.get(risk_level, 1.2)

    # Calculate total effort
    total_hours = base_hours * complexity_factor * risk_multiplier

    # Calculate AI-assisted savings
    ai_savings = total_hours * 0.35  # Average AI assistance

    return {
        "base_hours": base_hours,
        "complexity_factor": round(complexity_factor, 2),
        "risk_multiplier": risk_multiplier,
        "total_hours": round(total_hours, 1),
        "ai_assisted_hours": round(total_hours - ai_savings, 1),
        "ai_savings_hours": round(ai_savings, 1),
        "ai_savings_percent": 35
    }

for req in all_reqs:
    req["estimation"] = estimate_requirement(req, risk_map)
```

### Step 4: Generate Summary Statistics

```python
def derive_team_size(domain_context, total_hours):
    """V3-F6 fix: team size is no longer hardcoded.

    Resolution order:
    1. `domain_context.estimation.team_size_recommended` (operator-set, wins)
    2. Compute from total_hours using a sensible scale-tier heuristic:
         <= 2,000 hrs  → 2 FTE
         <= 10,000 hrs → 4 FTE
         <= 25,000 hrs → 6 FTE
         <= 50,000 hrs → 8 FTE
         else          → 12 FTE
       These tiers approximate a 6-12 month delivery window per scale band.
    3. The breakdown by role lives in the Resource Plan section below; this
       function returns the headline FTE count used for duration math.
    """
    override = (domain_context or {}).get("estimation", {}).get("team_size_recommended")
    if isinstance(override, (int, float)) and override > 0:
        return int(override), "domain-context override"

    if total_hours <= 2000:
        return 2, "scale-tier: <=2k hrs"
    elif total_hours <= 10000:
        return 4, "scale-tier: 2k-10k hrs"
    elif total_hours <= 25000:
        return 6, "scale-tier: 10k-25k hrs"
    elif total_hours <= 50000:
        return 8, "scale-tier: 25k-50k hrs"
    return 12, "scale-tier: >50k hrs"


def calculate_summary(requirements, domain_context):
    """Calculate project-wide estimation summary."""
    total_hours = sum(r["estimation"]["total_hours"] for r in requirements)
    ai_assisted_hours = sum(r["estimation"]["ai_assisted_hours"] for r in requirements)
    ai_savings = total_hours - ai_assisted_hours

    # By category
    by_category = {}
    for req in requirements:
        cat = req.get("category", "OTHER")
        if cat not in by_category:
            by_category[cat] = {"hours": 0, "count": 0}
        by_category[cat]["hours"] += req["estimation"]["total_hours"]
        by_category[cat]["count"] += 1

    # V3-F6 fix: team size derived from scale tier or domain-context override.
    team_size, team_size_source = derive_team_size(domain_context, total_hours)
    # Duration uses the AI-assisted hour total (more realistic given AI ratios)
    # divided by (FTE × 40 hrs/week). Falls back to total_hours if AI hours are 0.
    effective_hours = ai_assisted_hours if ai_assisted_hours > 0 else total_hours
    estimated_weeks = effective_hours / (team_size * 40) if team_size else 0

    return {
        "total_requirements": len(requirements),
        "total_hours": round(total_hours, 0),
        "ai_assisted_hours": round(ai_assisted_hours, 0),
        "ai_savings_hours": round(ai_savings, 0),
        "ai_savings_percent": round(ai_savings / total_hours * 100, 1) if total_hours > 0 else 0,
        "by_category": by_category,
        "estimated_duration_weeks": round(estimated_weeks, 1),
        "team_size_recommended": team_size,
        "team_size_source": team_size_source
    }

summary = calculate_summary(all_reqs, domain_context)
```

### Step 5: Generate Effort Document

```python
def generate_estimation_md(requirements, summary, domain):
    doc = f"""# Effort Estimation

**Domain:** {domain}
**Generated:** {datetime.now().strftime('%Y-%m-%d %H:%M')}
**Requirements Estimated:** {summary["total_requirements"]}

---

## Executive Summary

| Metric | Value |
|--------|-------|
| Total Requirements | {summary["total_requirements"]} |
| Total Effort (Traditional) | {summary["total_hours"]:,.0f} hours |
| Total Effort (AI-Assisted) | {summary["ai_assisted_hours"]:,.0f} hours |
| AI Savings | {summary["ai_savings_hours"]:,.0f} hours ({summary["ai_savings_percent"]}%) |
| Recommended Team Size | {summary["team_size_recommended"]} developers |
| Estimated Duration | {summary["estimated_duration_weeks"]} weeks |

---

## AI Assistance Impact

### Efficiency Gains by Activity

| Activity | Traditional | AI-Assisted | Savings |
|----------|-------------|-------------|---------|
| Code Generation | 100% | 60% | 40% |
| Testing | 100% | 50% | 50% |
| Documentation | 100% | 40% | 60% |
| Code Review | 100% | 70% | 30% |
| Design | 100% | 80% | 20% |

### AI Tools Leveraged
- GitHub Copilot for code generation
- Claude Code for architecture review
- Automated test generation
- Documentation generators

---

## Effort by Category

| Category | Requirements | Hours | % of Total |
|----------|--------------|-------|------------|
"""

    total_hours = summary["total_hours"]
    for cat, data in sorted(summary["by_category"].items(), key=lambda x: x[1]["hours"], reverse=True):
        pct = data["hours"] / total_hours * 100 if total_hours > 0 else 0
        doc += f"| {cat} | {data['count']} | {data['hours']:.0f} | {pct:.1f}% |\n"

    doc += f"""

---

## Detailed Estimates

### High-Effort Requirements (>40 hours)

| Req ID | Category | Base | Complexity | Risk | Total | AI-Assisted |
|--------|----------|------|------------|------|-------|-------------|
"""

    # V4-F6 fix 2026-05-20: NEVER cap deliverable table rows. Prior code applied
    # [:15] and [:20] slices, silently dropping qualifying requirements. Emit all.
    high_effort = [r for r in requirements if r["estimation"]["total_hours"] > 40]
    for req in sorted(high_effort, key=lambda x: x["estimation"]["total_hours"], reverse=True):
        est = req["estimation"]
        doc += f"| {req.get('canonical_id', 'N/A')} | {req.get('category', 'N/A')} | {est['base_hours']} | {est['complexity_factor']}x | {est['risk_multiplier']}x | {est['total_hours']} | {est['ai_assisted_hours']} |\n"

    doc += """

### Medium-Effort Requirements (16-40 hours)

| Req ID | Category | Total Hours | AI-Assisted |
|--------|----------|-------------|-------------|
"""

    medium_effort = [r for r in requirements if 16 <= r["estimation"]["total_hours"] <= 40]
    for req in sorted(medium_effort, key=lambda x: x["estimation"]["total_hours"], reverse=True):
        est = req["estimation"]
        doc += f"| {req.get('canonical_id', 'N/A')} | {req.get('category', 'N/A')} | {est['total_hours']} | {est['ai_assisted_hours']} |\n"

    doc += """

---

## Resource Plan

### Recommended Team Composition

| Role | Count | Responsibility |
|------|-------|---------------|
| Tech Lead | 1 | Architecture, code review |
| Senior Developer | 2 | Core development |
| Developer | 2 | Feature development |
| QA Engineer | 1 | Testing, automation |
| BA/PM | 0.5 | Requirements, coordination |

### Phase Allocation

| Phase | Duration | Focus |
|-------|----------|-------|
| Discovery | 2 weeks | Requirements refinement |
| Design | 3 weeks | Architecture, technical design |
| Development | 12 weeks | Core implementation |
| Testing | 4 weeks | QA, UAT |
| Deployment | 2 weeks | Go-live preparation |

---

## Assumptions

1. Team has relevant domain experience
2. Development environment available
3. Requirements stable after discovery
4. AI tools available (Copilot, Claude)
5. Standard 40-hour work weeks

### Items Requiring Pre-Submission Verification

[USER INPUT REQUIRED] Verify team composition and labor rates against company GL before submission. Market-rate defaults sourced from GSA MAS IT Category (verify staleness).

[USER INPUT REQUIRED] Confirm AI-tool licensing costs for AI-assisted hours (currently estimated at 35% savings — verify achievable per RDI engineering plan).

[USER INPUT REQUIRED] Insurance / bonding requirements per RFP Section [cite] if total > $500K.

---

## Risk Adjustments

High-risk requirements include 50% buffer
Medium-risk requirements include 20% buffer

---

## Appendix: Full Estimation Table

| Req ID | Category | Priority | Risk | Hours | AI Hours |
|--------|----------|----------|------|-------|----------|
"""

    for req in requirements:
        est = req["estimation"]
        # V3-F1 fix: risk_map values are risk-level strings.
        risk = risk_map.get(req.get("canonical_id"), "N/A")
        doc += f"| {req.get('canonical_id', 'N/A')} | {req.get('category', 'N/A')} | {req.get('priority', 'N/A')} | {risk} | {est['total_hours']} | {est['ai_assisted_hours']} |\n"

    return doc

domain = domain_context.get("selected_domain", "Generic")
estimation_md = generate_estimation_md(all_reqs, summary, domain)
write_file(f"{folder}/outputs/EFFORT_ESTIMATION.md", estimation_md)
```

## Quality Checklist (MANDATORY — report each by name with evidence)

The phase agent MUST verify each of the following BEFORE reporting completion. The agent's completion report MUST include a checklist-results block with:
- Item name (verbatim from below)
- PASS / FAIL / SKIPPED-WITH-REASON
- Evidence (file:line citation, grep result, file size, assertion that ran, etc.)

"All checks passed" without per-item evidence is NOT acceptable.

### Required output files
1. **EFFORT_ESTIMATION.md** exists at `{folder}/outputs/EFFORT_ESTIMATION.md` — evidence: `ls -la` showing size > 8,192 bytes (8 KB)

### Schema fidelity
2. **All requirements have estimates** — every requirement in all_reqs has an `estimation` dict — evidence: count requirements missing `estimation` key (must be 0)
3. **AI assistance ratios calculated** — every estimation has `ai_savings_percent` and `ai_assisted_hours` — evidence: spot-check estimation of requirements[0]
4. **Category breakdown included** in EFFORT_ESTIMATION.md — grep "Category" or "Effort by Category" returned >= 1 hit — evidence: grep result
5. No `[:N]` slicing applied to deliverable content strings — evidence: grep for `\[:[0-9]+\]` in production code paths returned 0 hits

### Cross-stage consistency
6. **Risk multiplier lookup uses `rtm_risks[].linked_requirement_ids` reverse-walk** (not the broken `risks.requirements[]` path) — evidence: confirm risk_map was built from `risks.get("rtm_risks", [])` with iteration over `linked_requirement_ids` — print first 3 keys from risk_map
7. **Team size derived from `domain_context.estimation.team_size_recommended` OR scale heuristic — NOT hardcoded** — evidence: print `team_size_recommended` value and `team_size_source` string confirming derivation method
8. **Assumptions section present** in EFFORT_ESTIMATION.md — grep "Assumptions" returned >= 1 hit — evidence: grep result

### Anti-regression rules (universal)
9. **UTF-8 encoding** on every `open()` call — evidence: search this phase's emitted scripts/code for `encoding='utf-8'` in every file-open
10. **ensure_ascii=False** on every `json.dump` call — evidence: same grep
11. **No `_Showing N of M_` row-cap notices** in any deliverable markdown — evidence: grep returned 0 matches
12. **No empty `|  |` mitigation/cell patterns** in any deliverable table — evidence: grep returned 0 matches
13. **No mid-word table-cell truncations** — evidence: line-by-line cell-end check returned 0 hits

### Memory discipline
14. **Relevant SAFS memory entries reviewed and applied** — evidence: list which memory files were read and which rules were applicable (e.g., "risk_map built from rtm_risks[].linked_requirement_ids as per V3-F1 fix; team_size from scale-tier heuristic as per V3-F6 fix")
