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
risk_map = {r["id"]: r["risk"] for r in risks.get("requirements", [])}
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
RISK_MULTIPLIERS = {
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
    risk_level = risk_map.get(req_id, {}).get("risk_level", "MEDIUM")
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
def calculate_summary(requirements):
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

    return {
        "total_requirements": len(requirements),
        "total_hours": round(total_hours, 0),
        "ai_assisted_hours": round(ai_assisted_hours, 0),
        "ai_savings_hours": round(ai_savings, 0),
        "ai_savings_percent": round(ai_savings / total_hours * 100, 1) if total_hours > 0 else 0,
        "by_category": by_category,
        "estimated_duration_weeks": round(total_hours / 40 / 3, 1),  # 3 developers
        "team_size_recommended": 3
    }

summary = calculate_summary(all_reqs)
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

    high_effort = [r for r in requirements if r["estimation"]["total_hours"] > 40]
    for req in sorted(high_effort, key=lambda x: x["estimation"]["total_hours"], reverse=True)[:15]:
        est = req["estimation"]
        doc += f"| {req.get('canonical_id', 'N/A')} | {req.get('category', 'N/A')} | {est['base_hours']} | {est['complexity_factor']}x | {est['risk_multiplier']}x | {est['total_hours']} | {est['ai_assisted_hours']} |\n"

    doc += """

### Medium-Effort Requirements (16-40 hours)

| Req ID | Category | Total Hours | AI-Assisted |
|--------|----------|-------------|-------------|
"""

    medium_effort = [r for r in requirements if 16 <= r["estimation"]["total_hours"] <= 40]
    for req in sorted(medium_effort, key=lambda x: x["estimation"]["total_hours"], reverse=True)[:20]:
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
        risk = risk_map.get(req.get("canonical_id"), {}).get("risk_level", "N/A")
        doc += f"| {req.get('canonical_id', 'N/A')} | {req.get('category', 'N/A')} | {req.get('priority', 'N/A')} | {risk} | {est['total_hours']} | {est['ai_assisted_hours']} |\n"

    return doc

domain = domain_context.get("selected_domain", "Generic")
estimation_md = generate_estimation_md(all_reqs, summary, domain)
write_file(f"{folder}/outputs/EFFORT_ESTIMATION.md", estimation_md)
```

## Quality Checklist

- [ ] `EFFORT_ESTIMATION.md` created (>8KB)
- [ ] All requirements have estimates
- [ ] AI assistance ratios calculated
- [ ] Category breakdown included
- [ ] Resource plan provided
- [ ] Assumptions documented
