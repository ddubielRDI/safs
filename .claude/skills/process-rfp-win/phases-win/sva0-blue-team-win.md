---
name: sva0-blue-team-win
expert-role: Strategic Readiness Reviewer
domain-expertise: Proposal strategy validation, Shipley Blue Team methodology, pre-authoring readiness assessment
---

# SVA-0: Blue Team — Strategic Readiness Gate

## Expert Role

You are a **Strategic Readiness Reviewer** conducting the Shipley Blue Team review. Your focus:
- Evaluation criteria coverage completeness
- Compliance matrix resolution
- Past performance strength and relevance
- Evaluator persona readiness
- Evidence library sufficiency
- Client intelligence completeness

## Purpose

Validate that all strategic inputs are ready before Phase 8 bid authoring begins. This is the Shipley **Blue Team** review — confirming that strategy and solution architecture are sound before investing in content generation. The Blue Team catches positioning gaps, missing evidence, and persona alignment issues early.

**Timing:** AFTER SVA-6 (Pre-Bid Validator), BEFORE Phase 8.0 (Positioning).
**Gate type:** BLOCKING — if BLOCK disposition, Phase 8 cannot proceed.

## Inputs

- `{folder}/shared/EVALUATION_CRITERIA.json` — Evaluation factors with weights
- `{folder}/shared/COMPLIANCE_MATRIX.json` — Mandatory items and resolution status
- `{folder}/shared/UNIFIED_RTM.json` — Requirements traceability matrix
- `{folder}/shared/bid/CLIENT_INTELLIGENCE.json` — Client and competitive intel (if exists)
- `{folder}/shared/PERSONA_COVERAGE.json` — Evaluator personas
- `{folder}/shared/REQUIREMENT_RISKS.json` — Risk assessments
- `{folder}/shared/domain-context.json` — Domain and industry context
- `config-win/evidence-library.json` — Evidence library with proof points
- `Past_Projects.md` — Past performance case studies
- All Phase 1-7 outputs

## Required Output

- `{folder}/shared/validation/sva0-blue-team.json` — Blue Team validation report (>1KB)
- `{folder}/outputs/BLUE_TEAM_READINESS.md` — Human-readable readiness summary

## Instructions

### Step 1: Load All Strategic Inputs

```python
from datetime import datetime
import os
import json

skill_dir = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

# Core inputs
eval_criteria = read_json(f"{folder}/shared/EVALUATION_CRITERIA.json")
compliance = read_json(f"{folder}/shared/COMPLIANCE_MATRIX.json")
rtm = read_json(f"{folder}/shared/UNIFIED_RTM.json")
personas = read_json_safe(f"{folder}/shared/PERSONA_COVERAGE.json")
risks = read_json_safe(f"{folder}/shared/REQUIREMENT_RISKS.json")
domain_context = read_json_safe(f"{folder}/shared/domain-context.json")

# Optional inputs
client_intel = read_json_safe(f"{folder}/shared/bid/CLIENT_INTELLIGENCE.json")

# Config inputs
evidence_library = read_json_safe(f"{skill_dir}/config-win/evidence-library.json")
past_projects_md = read_file_safe(f"{skill_dir}/Past_Projects.md")

log("Blue Team: All strategic inputs loaded")
```

### Step 2: Execute Validation Rules

```python
findings = []

# ============================================================
# RULE 1: SVA0-EVAL-CRITERIA-COVERAGE (CRITICAL)
# Every evaluation factor must have supporting evidence/approach
# ============================================================
eval_factors = eval_criteria.get("evaluation_factors", [])
rtm_entities = rtm.get("entities", {})
rtm_evidence = rtm_entities.get("evidence", [])
rtm_bid_sections = rtm_entities.get("bid_sections", [])

unsupported_factors = []
for factor in eval_factors:
    factor_name = factor.get("name", "")
    factor_id = factor.get("factor_id", factor.get("id", factor_name))

    # Check if any RTM evidence or bid section references this factor
    has_evidence = any(
        factor_name.lower() in str(e).lower() or factor_id in str(e)
        for e in rtm_evidence
    )
    has_bid_section = any(
        factor_name.lower() in str(bs).lower() or factor_id in str(bs)
        for bs in rtm_bid_sections
    )

    if not has_evidence and not has_bid_section:
        unsupported_factors.append(factor_name)

rule1_passed = len(unsupported_factors) == 0
findings.append({
    "rule_id": "SVA0-EVAL-CRITERIA-COVERAGE",
    "rule_name": "Evaluation Criteria Coverage",
    "severity": "CRITICAL",
    "passed": rule1_passed,
    "details": (
        "All evaluation factors have supporting evidence/approach in RTM"
        if rule1_passed
        else f"{len(unsupported_factors)} evaluation factor(s) lack supporting evidence: {', '.join(unsupported_factors)}"
    ),
    "corrective_action": {
        "auto_correctable": False,
        "instruction": "Review UNIFIED_RTM.json and ensure each evaluation factor maps to evidence or bid section content"
    } if not rule1_passed else None
})

# ============================================================
# RULE 2: SVA0-COMPLIANCE-GATE-CLEAR (CRITICAL)
# No unresolved BLOCK items in compliance matrix
# ============================================================
mandatory_items = compliance.get("mandatory_items",
    compliance.get("rtm_entities", {}).get("mandatory_items", []))

unresolved_blocks = []
for item in mandatory_items:
    status = item.get("status", item.get("resolution", "")).upper()
    if status in ("BLOCK", "UNRESOLVED", "FAIL", "NOT_MET"):
        unresolved_blocks.append(item.get("item", item.get("description", "Unknown")))

rule2_passed = len(unresolved_blocks) == 0
findings.append({
    "rule_id": "SVA0-COMPLIANCE-GATE-CLEAR",
    "rule_name": "Compliance Gate Clear",
    "severity": "CRITICAL",
    "passed": rule2_passed,
    "details": (
        "All mandatory compliance items resolved"
        if rule2_passed
        else f"{len(unresolved_blocks)} unresolved BLOCK item(s): {', '.join(unresolved_blocks[:5])}"
    ),
    "corrective_action": {
        "auto_correctable": False,
        "target_phase": "1.7",
        "instruction": "Resolve compliance BLOCK items before proceeding to bid authoring"
    } if not rule2_passed else None
})

# ============================================================
# RULE 3: SVA0-PAST-PERFORMANCE-STRENGTH (HIGH)
# >=3 matched projects with relevance_score >15
# ============================================================
# Parse past projects to count strong matches
# This uses the same parsing logic as Phase 8.0 Step 1b
projects = parse_past_projects(past_projects_md) if past_projects_md else []

# Quick relevance scoring against current domain
rfp_domain = (domain_context or {}).get("selected_domain", "default")
rfp_industry = (domain_context or {}).get("industry", "")

strong_matches = 0
for project in projects:
    score = 0
    project_industry = project.get("industry", "").lower()
    if project_industry == rfp_industry.lower():
        score += 10
    if project.get("has_metrics_table"):
        score += 5
    if project.get("technologies"):
        score += min(len(project["technologies"]) * 2, 8)
    if score > 15:
        strong_matches += 1

rule3_passed = strong_matches >= 3
findings.append({
    "rule_id": "SVA0-PAST-PERFORMANCE-STRENGTH",
    "rule_name": "Past Performance Strength",
    "severity": "HIGH",
    "passed": rule3_passed,
    "details": (
        f"{strong_matches} strong past performance matches (score >15)"
        if rule3_passed
        else f"Only {strong_matches} strong matches found (need >=3 with score >15). Consider adding override_projects in company-profile.json."
    ),
    "corrective_action": {
        "auto_correctable": False,
        "instruction": "Review Past_Projects.md for relevance to this RFP domain. Add override_projects to company-profile.json if needed."
    } if not rule3_passed else None
})

# ============================================================
# RULE 4: SVA0-PERSONA-READINESS (HIGH)
# >=4/5 evaluator personas defined with pain points and success metrics
# ============================================================
persona_list = []
if personas:
    persona_list = personas.get("personas", personas.get("evaluators", []))

personas_ready = 0
personas_incomplete = []
for persona in persona_list:
    has_pain_points = bool(persona.get("pain_points", persona.get("concerns", [])))
    has_metrics = bool(persona.get("success_metrics", persona.get("priorities", [])))
    if has_pain_points and has_metrics:
        personas_ready += 1
    else:
        missing = []
        if not has_pain_points:
            missing.append("pain_points")
        if not has_metrics:
            missing.append("success_metrics")
        personas_incomplete.append(f"{persona.get('role', 'Unknown')} (missing: {', '.join(missing)})")

rule4_passed = personas_ready >= 4
findings.append({
    "rule_id": "SVA0-PERSONA-READINESS",
    "rule_name": "Evaluator Persona Readiness",
    "severity": "HIGH",
    "passed": rule4_passed,
    "details": (
        f"{personas_ready}/{len(persona_list)} personas fully defined with pain points and success metrics"
        if rule4_passed
        else f"Only {personas_ready}/{len(persona_list)} personas complete. Incomplete: {'; '.join(personas_incomplete[:3])}"
    ),
    "corrective_action": {
        "auto_correctable": True,
        "target_phase": "7c",
        "instruction": "Re-run persona generation to ensure pain_points and success_metrics populated for all evaluator roles"
    } if not rule4_passed else None
})

# ============================================================
# RULE 5: SVA0-EVIDENCE-SUFFICIENCY (HIGH)
# Evidence library tag-overlap covers >=60% of bid sections
# ============================================================
evidence_items = []
if evidence_library:
    for category, items in evidence_library.get("categories", {}).items():
        for item in items:
            item_text = str(item.get("statement", item.get("certification", item.get("quote", ""))))
            if "[USER INPUT" not in item_text:
                evidence_items.append(item)

# Count bid sections from RTM
bid_sections = rtm_entities.get("bid_sections", [])
total_sections = len(bid_sections) if bid_sections else 1

# Check evidence coverage by comparing evidence tags to section names
sections_covered = set()
for item in evidence_items:
    tags = [t.lower() for t in item.get("tags", [])]
    for section in bid_sections:
        section_name = section.get("name", section.get("title", str(section))).lower()
        if any(tag in section_name for tag in tags):
            sections_covered.add(section_name)

coverage_pct = len(sections_covered) / max(total_sections, 1)
rule5_passed = coverage_pct >= 0.60
findings.append({
    "rule_id": "SVA0-EVIDENCE-SUFFICIENCY",
    "rule_name": "Evidence Library Sufficiency",
    "severity": "HIGH",
    "passed": rule5_passed,
    "details": (
        f"Evidence covers {coverage_pct:.0%} of bid sections ({len(sections_covered)}/{total_sections})"
        if rule5_passed
        else f"Evidence covers only {coverage_pct:.0%} of bid sections ({len(sections_covered)}/{total_sections}). Need >=60%."
    ),
    "corrective_action": {
        "auto_correctable": False,
        "instruction": "Populate evidence-library.json with proof points covering more bid sections"
    } if not rule5_passed else None
})

# ============================================================
# RULE 6: SVA0-INTELLIGENCE-COMPLETENESS (MEDIUM)
# Client intel exists and includes tech stack + competitive landscape
# ============================================================
has_intel = client_intel is not None
has_tech_stack = False
has_competitive = False

if client_intel:
    intel_data = client_intel.get("intelligence", client_intel)
    has_tech_stack = bool(intel_data.get("technology_environment", intel_data.get("tech_stack")))
    competitive = intel_data.get("competitive_landscape", {})
    has_competitive = bool(competitive.get("incumbent") or competitive.get("known_competitors"))

rule6_passed = has_intel and has_tech_stack and has_competitive
findings.append({
    "rule_id": "SVA0-INTELLIGENCE-COMPLETENESS",
    "rule_name": "Intelligence Completeness",
    "severity": "MEDIUM",
    "passed": rule6_passed,
    "details": (
        "Client intelligence includes tech stack and competitive landscape"
        if rule6_passed
        else f"Intelligence gaps: {'no CLIENT_INTELLIGENCE.json' if not has_intel else ', '.join(filter(None, ['missing tech stack' if not has_tech_stack else '', 'missing competitive landscape' if not has_competitive else '']))}"
    ),
    "corrective_action": {
        "auto_correctable": True,
        "target_phase": "1.95",
        "instruction": "Re-run client intelligence phase to populate technology environment and competitive landscape"
    } if not rule6_passed else None
})

log(f"Blue Team: {len(findings)} rules evaluated")
```

### Step 3: Calculate Disposition

```python
critical_failures = [f for f in findings if f["severity"] == "CRITICAL" and not f["passed"]]
high_failures = [f for f in findings if f["severity"] == "HIGH" and not f["passed"]]
medium_failures = [f for f in findings if f["severity"] == "MEDIUM" and not f["passed"]]

# Disposition logic:
# PASS: All CRITICAL pass + <=1 HIGH failure
# ADVISORY: All CRITICAL pass + 2+ HIGH failures
# BLOCK: Any CRITICAL failure
if critical_failures:
    disposition = "BLOCK"
elif len(high_failures) >= 2:
    disposition = "ADVISORY"
else:
    disposition = "PASS"

# Calculate score (0-100)
total_rules = len(findings)
passed_rules = sum(1 for f in findings if f["passed"])
severity_weights = {"CRITICAL": 30, "HIGH": 15, "MEDIUM": 10}

max_score = sum(severity_weights.get(f["severity"], 10) for f in findings)
earned_score = sum(severity_weights.get(f["severity"], 10) for f in findings if f["passed"])
overall_score = round((earned_score / max_score) * 100) if max_score > 0 else 0

log(f"Blue Team disposition: {disposition} (score: {overall_score}/100)")
log(f"  CRITICAL: {len(critical_failures)} failures")
log(f"  HIGH: {len(high_failures)} failures")
log(f"  MEDIUM: {len(medium_failures)} failures")
```

### Step 4: Write Validation Report

```python
report = {
    "sva_id": "SVA-0",
    "sva_name": "Blue Team — Strategic Readiness Gate",
    "generated_at": datetime.now().isoformat(),
    "disposition": disposition,
    "summary": {
        "overall_score": overall_score,
        "total_rules": total_rules,
        "passed": passed_rules,
        "failed": total_rules - passed_rules,
        "critical_failures": len(critical_failures),
        "high_failures": len(high_failures),
        "medium_failures": len(medium_failures)
    },
    "findings": findings,
    "color_team_report": {
        "team": "Blue",
        "methodology": "Shipley Blue Team — Strategy and solution validation before writing",
        "top_concerns": [
            f["details"] for f in findings if not f["passed"]
        ],
        "recommendation": (
            "All strategic inputs validated. Proceed to bid authoring."
            if disposition == "PASS"
            else "Address findings before proceeding to bid authoring."
            if disposition == "ADVISORY"
            else "CRITICAL gaps must be resolved before bid authoring can begin."
        )
    }
}

write_json(f"{folder}/shared/validation/sva0-blue-team.json", report)
```

### Step 5: Generate BLUE_TEAM_READINESS.md

```python
md = []
md.append("# Blue Team Readiness Report")
md.append("")
md.append(f"**Generated:** {datetime.now().strftime('%Y-%m-%d %H:%M')}")
md.append(f"**Disposition:** **{disposition}**")
md.append(f"**Score:** {overall_score}/100")
md.append("")

md.append("## Validation Results")
md.append("")
md.append("| Rule | Severity | Status | Details |")
md.append("|------|----------|--------|---------|")
for f in findings:
    status = "PASS" if f["passed"] else "FAIL"
    icon = "✅" if f["passed"] else ("❌" if f["severity"] == "CRITICAL" else "⚠️")
    md.append(f"| {f['rule_id']} | {f['severity']} | {icon} {status} | {f['details'][:100]} |")
md.append("")

# Failed findings detail
failed = [f for f in findings if not f["passed"]]
if failed:
    md.append("## Required Actions")
    md.append("")
    for i, f in enumerate(failed, 1):
        md.append(f"### {i}. {f['rule_name']} ({f['severity']})")
        md.append("")
        md.append(f"**Issue:** {f['details']}")
        md.append("")
        if f.get("corrective_action"):
            action = f["corrective_action"]
            auto = "Auto-correctable" if action.get("auto_correctable") else "Manual"
            md.append(f"**Action ({auto}):** {action.get('instruction', 'Review and fix manually')}")
            if action.get("target_phase"):
                md.append(f"**Target Phase:** {action['target_phase']}")
        md.append("")

md.append("---")
md.append("")
md.append("## Disposition Guide")
md.append("")
md.append("- **PASS**: All strategic inputs validated. Proceed to Phase 8.0 (Positioning).")
md.append("- **ADVISORY**: Minor gaps detected. Proceed with caution — address findings during bid authoring.")
md.append("- **BLOCK**: Critical gaps found. Must resolve before Phase 8 can begin.")
md.append("")

report_content = "\n".join(md)
write_file(f"{folder}/outputs/BLUE_TEAM_READINESS.md", report_content)
log(f"Blue Team readiness report: {folder}/outputs/BLUE_TEAM_READINESS.md")
```

### Step 6: Report Results

```python
log(f"""
{'='*60}
🔵 BLUE TEAM — STRATEGIC READINESS GATE
{'='*60}

Disposition: {disposition} (score: {overall_score}/100)

Rule Results:
{chr(10).join(f"  {'✅' if f['passed'] else '❌'} {f['rule_id']}: {f['details'][:80]}" for f in findings)}

{'Proceeding to Phase 8.0 (Positioning)...' if disposition != 'BLOCK' else 'BLOCKED — resolve critical findings before bid authoring.'}

Outputs:
  Report: {folder}/shared/validation/sva0-blue-team.json
  Summary: {folder}/outputs/BLUE_TEAM_READINESS.md
""")
```

## Quality Checklist

- [ ] All 6 validation rules executed with correct severity levels
- [ ] `sva0-blue-team.json` written to `shared/validation/` with findings array and disposition
- [ ] `BLUE_TEAM_READINESS.md` generated in `outputs/` with human-readable summary
- [ ] Disposition logic: BLOCK on CRITICAL failure, ADVISORY on 2+ HIGH failures, PASS otherwise
- [ ] Each finding includes rule_id, severity, passed, details, and corrective_action
- [ ] Auto-correctable findings reference target_phase for retry
