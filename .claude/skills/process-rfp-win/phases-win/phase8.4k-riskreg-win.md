---
name: phase8.4k-riskreg-win
expert-role: Risk Analyst
domain-expertise: Risk register formatting, tabular risk presentation, mitigation tracking, risk categorization
skill: risk-analyst
---

# Phase 8.4k: Risk Register

## Purpose

Generate a formal Risk Register document in tabular format, organized by work section. This provides evaluators with a clear, structured view of identified risks and mitigation strategies — demonstrating proactive risk management.

## Inputs

- `{folder}/shared/REQUIREMENT_RISKS.json` - All risk assessments
- `{folder}/shared/UNIFIED_RTM.json` - Risk-to-requirement-to-bid traceability
- `{folder}/shared/requirements-normalized.json` - Requirements for context
- `{folder}/shared/domain-context.json` - Domain context

## Required Output

- `{folder}/outputs/bid-sections/04_RISK_REGISTER.md` (>5KB)

## Instructions

### Step 1: Load Risk Data

```python
risks_data = read_json(f"{folder}/shared/REQUIREMENT_RISKS.json")
rtm = read_json_safe(f"{folder}/shared/UNIFIED_RTM.json")
requirements = read_json(f"{folder}/shared/requirements-normalized.json")
domain = read_json(f"{folder}/shared/domain-context.json")

all_risks = risks_data.get("risks", [])
rtm_risks = rtm.get("entities", {}).get("risks", []) if rtm else []
```

### Step 2: Organize Risks by Category and Severity

```python
risk_categories = {}
for risk in all_risks:
    cat = risk.get("category", risk.get("risk_category", "General"))
    if cat not in risk_categories:
        risk_categories[cat] = []
    risk_categories[cat].append(risk)

# Sort within categories by severity
severity_order = {"CRITICAL": 0, "HIGH": 1, "MEDIUM": 2, "LOW": 3}
for cat in risk_categories:
    risk_categories[cat].sort(key=lambda r: severity_order.get(r.get("severity", "MEDIUM"), 2))

# Count by severity
severity_counts = {"CRITICAL": 0, "HIGH": 0, "MEDIUM": 0, "LOW": 0}
for risk in all_risks:
    sev = risk.get("severity", "MEDIUM")
    severity_counts[sev] = severity_counts.get(sev, 0) + 1
```

### Step 3: Generate Risk Register Document

```markdown
# Risk Register

## Executive Summary

| Severity | Count | Status |
|----------|-------|--------|
| CRITICAL | {critical} | All mitigated |
| HIGH | {high} | All mitigated |
| MEDIUM | {medium} | Monitored |
| LOW | {low} | Accepted |
| **Total** | **{total}** | |

## Risk Assessment Methodology

Resource Data employs a structured risk management approach:
- **Identification**: Systematic analysis of requirements, architecture, and integration points
- **Assessment**: Severity (impact x likelihood) using a 4-level scale
- **Mitigation**: Proactive strategies with assigned owners and verification criteria
- **Monitoring**: Ongoing risk tracking through project governance

---

## Risk Register by Category

### [Category Name]

| Risk ID | Description | Severity | Likelihood | Impact | Mitigation | Owner | Verification | Status |
|---------|-------------|----------|------------|--------|------------|-------|--------------|--------|
{for each risk in category: formatted row}

[Repeat for each category]

---

## Risk Mitigation Timeline

| Phase | Risks Addressed | Mitigation Activities |
|-------|----------------|----------------------|
| Discovery | {risks} | Initial risk validation, stakeholder review |
| Design | {risks} | Architecture risk mitigations, security review |
| Build | {risks} | Technical risk mitigations, integration testing |
| Test | {risks} | Performance risk validation, UAT |
| Deploy | {risks} | Deployment risk mitigations, rollback plans |

## Risk Monitoring and Reporting

- Weekly risk review in project status meetings
- Monthly risk register updates to steering committee
- Escalation triggers for severity changes
- Lessons learned captured in project knowledge base
```

### Step 4: Write Output

```python
write_file(f"{folder}/outputs/bid-sections/04_RISK_REGISTER.md", register_content)

log(f"""
⚠️ RISK REGISTER COMPLETE (Phase 8.4k)
========================================
Total Risks: {len(all_risks)}
Categories: {len(risk_categories)}
CRITICAL: {severity_counts["CRITICAL"]}
HIGH: {severity_counts["HIGH"]}
MEDIUM: {severity_counts["MEDIUM"]}
LOW: {severity_counts["LOW"]}

Output: outputs/bid-sections/04_RISK_REGISTER.md
""")
```

## Quality Checklist

- [ ] `04_RISK_REGISTER.md` created (>5KB)
- [ ] All risks from REQUIREMENT_RISKS.json included
- [ ] Organized by category with severity ordering
- [ ] Mitigation strategy for every HIGH/CRITICAL risk
- [ ] Owner role assigned for every HIGH/CRITICAL risk
- [ ] Verification criteria specified
- [ ] Risk methodology section included
- [ ] Timeline showing when risks are addressed
