---
name: phase3g-risks-win
expert-role: Risk Analyst
domain-expertise: Risk assessment, mitigation strategies
skill: risk-analyst
---

# Phase 3g: Requirement Risk Assessment

## Purpose

Assess risks for each requirement and generate mitigation strategies.

## Inputs

- `{folder}/shared/requirements-normalized.json` - Normalized requirements
- `{folder}/shared/domain-context.json` - Domain context

## Required Outputs

- `{folder}/outputs/REQUIREMENT_RISKS.md` - Risk assessment document
- `{folder}/shared/REQUIREMENT_RISKS.json` - Machine-readable risk data

## Instructions

### Step 1: Load Requirements

```python
requirements = read_json(f"{folder}/shared/requirements-normalized.json")
domain_context = read_json(f"{folder}/shared/domain-context.json")

all_reqs = requirements.get("requirements", [])
```

### Step 2: Define Risk Categories

```python
RISK_CATEGORIES = {
    "PM": {
        "name": "Project Management",
        "indicators": ["schedule", "resource", "scope", "budget", "stakeholder", "communication"],
        "mitigations": [
            "Regular status reporting",
            "Change management process",
            "Resource buffer allocation",
            "Stakeholder alignment meetings"
        ]
    },
    "SOFTWARE": {
        "name": "Software/Technical",
        "indicators": ["integration", "performance", "scalability", "security", "complexity", "technology"],
        "mitigations": [
            "Proof of concept development",
            "Performance testing early",
            "Code reviews and static analysis",
            "Architecture review board"
        ]
    },
    "DOMAIN": {
        "name": "Domain/Business",
        "indicators": ["compliance", "regulation", "workflow", "data", "process", "user adoption"],
        "mitigations": [
            "Subject matter expert involvement",
            "User acceptance testing",
            "Compliance audit checkpoints",
            "Training program development"
        ]
    }
}
```

### Step 3: Assess Each Requirement

```python
def assess_requirement_risk(req):
    """Assess risk for a single requirement."""
    text_lower = req.get("text", "").lower()

    # Determine risk category
    category_scores = {}
    for cat_id, cat_info in RISK_CATEGORIES.items():
        score = sum(1 for ind in cat_info["indicators"] if ind in text_lower)
        category_scores[cat_id] = score

    primary_category = max(category_scores, key=category_scores.get)

    # Calculate risk factors
    risk_factors = []

    # Complexity risk
    if len(text_lower.split()) > 50:
        risk_factors.append("High complexity (long requirement)")

    if "integration" in text_lower or "interface" in text_lower:
        risk_factors.append("Integration dependency")

    if any(term in text_lower for term in ["real-time", "high availability", "99.9%"]):
        risk_factors.append("Stringent performance requirement")

    if any(term in text_lower for term in ["ferpa", "hipaa", "pci", "compliance"]):
        risk_factors.append("Regulatory compliance requirement")

    if any(term in text_lower for term in ["legacy", "migrate", "convert"]):
        risk_factors.append("Legacy system dependency")

    if any(term in text_lower for term in ["custom", "proprietary", "unique"]):
        risk_factors.append("Custom development required")

    # Calculate risk score (1-5)
    base_score = len(risk_factors)
    if req.get("priority") == "CRITICAL":
        base_score += 1
    risk_score = min(5, max(1, base_score))

    # Determine risk level
    if risk_score >= 4:
        risk_level = "HIGH"
    elif risk_score >= 2:
        risk_level = "MEDIUM"
    else:
        risk_level = "LOW"

    return {
        "category": primary_category,
        "category_name": RISK_CATEGORIES[primary_category]["name"],
        "risk_score": risk_score,
        "risk_level": risk_level,
        "risk_factors": risk_factors,
        "suggested_mitigations": RISK_CATEGORIES[primary_category]["mitigations"][:2]
    }

# Assess all requirements
for req in all_reqs:
    req["risk_assessment"] = assess_requirement_risk(req)
```

### Step 4: Generate Risk Heat Map Data

```python
def generate_heat_map(requirements):
    """Generate risk heat map data."""
    heat_map = {
        "HIGH": {"PM": 0, "SOFTWARE": 0, "DOMAIN": 0},
        "MEDIUM": {"PM": 0, "SOFTWARE": 0, "DOMAIN": 0},
        "LOW": {"PM": 0, "SOFTWARE": 0, "DOMAIN": 0}
    }

    for req in requirements:
        risk = req.get("risk_assessment", {})
        level = risk.get("risk_level", "LOW")
        category = risk.get("category", "SOFTWARE")
        heat_map[level][category] += 1

    return heat_map

heat_map = generate_heat_map(all_reqs)
```

### Step 5: Generate Risk Document

```python
def generate_risks_md(requirements, heat_map, domain):
    high_risk = [r for r in requirements if r.get("risk_assessment", {}).get("risk_level") == "HIGH"]
    medium_risk = [r for r in requirements if r.get("risk_assessment", {}).get("risk_level") == "MEDIUM"]
    low_risk = [r for r in requirements if r.get("risk_assessment", {}).get("risk_level") == "LOW"]

    doc = f"""# Requirement Risk Assessment

**Domain:** {domain}
**Generated:** {datetime.now().strftime('%Y-%m-%d %H:%M')}
**Total Requirements Assessed:** {len(requirements)}

---

## Executive Summary

| Risk Level | Count | Percentage |
|------------|-------|------------|
| HIGH | {len(high_risk)} | {len(high_risk)/len(requirements)*100:.1f}% |
| MEDIUM | {len(medium_risk)} | {len(medium_risk)/len(requirements)*100:.1f}% |
| LOW | {len(low_risk)} | {len(low_risk)/len(requirements)*100:.1f}% |

---

## Risk Heat Map

```
           │ PM    │ SOFTWARE │ DOMAIN │
    ───────┼───────┼──────────┼────────┤
    HIGH   │ {heat_map["HIGH"]["PM"]:^5} │ {heat_map["HIGH"]["SOFTWARE"]:^8} │ {heat_map["HIGH"]["DOMAIN"]:^6} │
    MEDIUM │ {heat_map["MEDIUM"]["PM"]:^5} │ {heat_map["MEDIUM"]["SOFTWARE"]:^8} │ {heat_map["MEDIUM"]["DOMAIN"]:^6} │
    LOW    │ {heat_map["LOW"]["PM"]:^5} │ {heat_map["LOW"]["SOFTWARE"]:^8} │ {heat_map["LOW"]["DOMAIN"]:^6} │
```

---

## High-Risk Requirements (Immediate Attention)

"""

    # 2026-05-19 NO-TRUNCATION FIX: removed high_risk[:15] cap and [:200] text truncation
    # per feedback_screen_encoding_truncation.md regression discipline. Pipelines produce
    # full data; humans decide what to trim.
    for req in high_risk:
        risk = req.get("risk_assessment", {})
        doc += f"""### {req.get('canonical_id', 'N/A')} - {risk.get('category_name')}

**Requirement:** {req.get('text', '')}

**Risk Score:** {risk.get('risk_score')}/5 ({risk.get('risk_level')})

**Risk Factors:**
"""
        for factor in risk.get("risk_factors", []):
            doc += f"- {factor}\n"

        doc += "\n**Recommended Mitigations:**\n"
        for mitigation in risk.get("suggested_mitigations", []):
            doc += f"- {mitigation}\n"

        doc += "\n---\n\n"

    doc += """
## Medium-Risk Requirements

| Req ID | Category | Score | Key Risk Factor |
|--------|----------|-------|-----------------|
"""

    # 2026-05-19 NO-TRUNCATION FIX: removed medium_risk[:20] cap and key_factor[:40] truncation
    for req in medium_risk:
        risk = req.get("risk_assessment", {})
        key_factor = risk.get("risk_factors", ["None"])[0] if risk.get("risk_factors") else "Standard complexity"
        doc += f"| {req.get('canonical_id', 'N/A')} | {risk.get('category')} | {risk.get('risk_score')}/5 | {key_factor} |\n"

    doc += """

---

## Risk Mitigation Strategy

### Project Management Risks
1. Implement weekly risk review meetings
2. Maintain risk register with ownership
3. Establish escalation procedures
4. Allocate contingency buffer (15-20%)

### Software/Technical Risks
1. Conduct proof of concepts for high-risk integrations
2. Implement continuous integration/testing
3. Schedule architecture reviews at key milestones
4. Plan for performance testing early

### Domain/Business Risks
1. Engage subject matter experts throughout
2. Schedule regular compliance checkpoints
3. Develop comprehensive training program
4. Plan for user acceptance testing phases

---

## Risk Monitoring Dashboard

### Key Risk Indicators (KRIs)
| Indicator | Threshold | Current | Status |
|-----------|-----------|---------|--------|
| High-risk requirement % | < 15% | {len(high_risk)/len(requirements)*100:.1f}% | {"🟢" if len(high_risk)/len(requirements) < 0.15 else "🔴"} |
| Unmitigated risks | 0 | TBD | ⚪ |
| Risk trend | Decreasing | TBD | ⚪ |

---

## Appendix: Full Risk Register

| Req ID | Category | Level | Score | Primary Factor |
|--------|----------|-------|-------|----------------|
"""

    for req in requirements:
        risk = req.get("risk_assessment", {})
        factor = risk.get("risk_factors", ["N/A"])[0] if risk.get("risk_factors") else "N/A"
        # 2026-05-19 NO-TRUNCATION FIX: removed factor[:30] truncation
        doc += f"| {req.get('canonical_id', 'N/A')} | {risk.get('category', 'N/A')} | {risk.get('risk_level', 'N/A')} | {risk.get('risk_score', 0)}/5 | {factor} |\n"

    return doc

domain = domain_context.get("selected_domain", "Generic")
risks_md = generate_risks_md(all_reqs, heat_map, domain)
write_file(f"{folder}/outputs/REQUIREMENT_RISKS.md", risks_md)
```

### Step 5b: Assign Stable RTM Risk IDs (NEW - for UNIFIED_RTM.json)

```python
## RTM CONTRIBUTION: Assign stable risk_id (RISK-001) and mitigation_id (MIT-001-01)
## Phase 4 will use these to link risks into the UNIFIED_RTM.json chain

risk_id_counter = 1
risks_for_rtm = []

# Consolidate risks by requirement - group related risks
for req in all_reqs:
    risk = req.get("risk_assessment", {})
    if risk.get("risk_level") in ["HIGH", "MEDIUM"]:  # Only track non-trivial risks
        risk_id = f"RISK-{risk_id_counter:03d}"
        req["risk_assessment"]["risk_id"] = risk_id

        # Assign mitigation IDs
        mitigations = []
        for mit_idx, mitigation_text in enumerate(risk.get("suggested_mitigations", []), 1):
            mitigation_id = f"MIT-{risk_id_counter:03d}-{mit_idx:02d}"
            mitigations.append({
                "mitigation_id": mitigation_id,
                "strategy": mitigation_text,
                "owner_role": infer_owner_role(risk.get("category", "SOFTWARE")),
                "timeline": "Phase-dependent",
                "status": "IDENTIFIED",
                "bid_location": None,  # Populated post-bid-authoring
                "evidence_ids": []
            })

        # 2026-05-19 NO-TRUNCATION FIX: removed [:80] cap on risk titles.
        # This was the ROOT CAUSE of mid-word truncation in Stage 7 bid docs
        # (e.g., "with a repor" → should be "with a reporting year"; "entit" → "entities").
        # Per feedback_screen_encoding_truncation.md: pipelines produce full data.
        risks_for_rtm.append({
            "risk_id": risk_id,
            "title": f"{risk.get('category_name', 'Risk')}: {req.get('text', '')}",
            "category": risk.get("category", "SOFTWARE"),
            "likelihood": min(5, risk.get("risk_score", 1)),
            "impact": min(5, max(1, risk.get("risk_score", 1) + 1)),  # Impact slightly higher than likelihood
            "risk_score": risk.get("risk_score", 1) * min(5, max(1, risk.get("risk_score", 1) + 1)),
            "risk_level": risk.get("risk_level"),
            "linked_requirement_ids": [req.get("canonical_id", req.get("id", ""))],
            "mitigation_strategies": mitigations
        })

        risk_id_counter += 1

def infer_owner_role(category):
    """Infer the responsible role based on risk category."""
    role_map = {
        "PM": "Program Manager",
        "SOFTWARE": "Technical Architect",
        "DOMAIN": "Business Analyst"
    }
    return role_map.get(category, "Program Manager")
```

### Step 5c: Structural / Programmatic Risks (RFP-specific, MANDATORY)

Pipelines that produce a 5-year-or-longer engagement with technology-stack
choices MUST also emit a structural-risk register alongside the requirement-
derived risks. These are the bid-execution risks that cannot be extracted from
a single requirement -- runtime LTS coverage, mandatory-partner dependencies,
peak-season capacity, legacy-system migration cutover, statutory compliance
deadlines, identity-realm isolation, tenant-isolation correctness.

**Discipline rule (added 2026-05-19 after MARS regression):** Structural risk
descriptions and mitigations MUST be **derived from authoritative pipeline
artifacts** -- not hardcoded from the agent's memory of older tech-stack
choices. Specifically:

1. **Read `shared/tech-lifecycle-evidence.json`** (Phase 3a tech-stack output)
   for the actual runtime versions chosen, their GA dates, their Microsoft /
   vendor EOL dates, and any documented in-contract migration plan.
2. **Read `outputs/ARCHITECTURE.md`** for the Architecture Decision Records
   (ADRs) -- in particular ADR-005 (or equivalent) for the runtime LTS
   selection and the documented migration ladder.
3. **Cross-check** that the structural risk's framework version, EOL date,
   and mitigation strategy match what the architecture actually proposes.

If the architecture says ".NET 10 LTS at go-live with documented ladder to
.NET 12 LTS year 3 and .NET 14 LTS year 5," the risk register says exactly
that -- not ".NET 8 LTS EOL" (a stale prior-cycle baseline) and not ".NET 9
LTS" (a STS release wrongly labelled LTS). **Never hardcode framework
version numbers in this phase file or in stage3g_risks.py.** Read the
chosen runtime from the architecture artifacts and propagate it.

Failure mode being prevented: SVA-4 auto-corrected the architecture to a
new runtime baseline, but stage3g_risks.py was re-run with stale hardcoded
structural-risk text that mentioned the old runtime. The result was a bid
PDF whose Risk Register contradicted its own Architecture spec -- a fatal
evaluator-visible inconsistency.

**Verification:** before stage3g writes RISKS.json, assert that for any
structural risk whose title or description mentions a framework version
(`.NET N`, `Java N`, `Node N`, etc.), that version appears in
`tech-lifecycle-evidence.json` as a current or planned baseline. If it
does not, raise a clear error and fail the phase.

### Step 6: Write JSON Output

```python
risks_json = {
    "assessed_at": datetime.now().isoformat(),
    "summary": {
        "total_assessed": len(all_reqs),
        "high_risk": len([r for r in all_reqs if r.get("risk_assessment", {}).get("risk_level") == "HIGH"]),
        "medium_risk": len([r for r in all_reqs if r.get("risk_assessment", {}).get("risk_level") == "MEDIUM"]),
        "low_risk": len([r for r in all_reqs if r.get("risk_assessment", {}).get("risk_level") == "LOW"]),
        "total_risks_tracked": len(risks_for_rtm),
        "total_mitigations": sum(len(r["mitigation_strategies"]) for r in risks_for_rtm)
    },
    "heat_map": heat_map,
    "requirements": [
        {
            "id": req.get("canonical_id"),
            "text": req.get("text"),
            "risk": req.get("risk_assessment")
        }
        for req in all_reqs
    ],
    # RTM: Structured risk entities for Phase 4 to consume
    "rtm_risks": risks_for_rtm
}

write_json(f"{folder}/shared/REQUIREMENT_RISKS.json", risks_json)
```

## Quality Checklist (MANDATORY — report each by name with evidence)

The phase agent MUST verify each of the following BEFORE reporting completion. The agent's completion report MUST include a checklist-results block with:
- Item name (verbatim from below)
- PASS / FAIL / SKIPPED-WITH-REASON
- Evidence (file:line citation, grep result, file size, assertion that ran, etc.)

"All checks passed" without per-item evidence is NOT acceptable.

### Required output files
1. **REQUIREMENT_RISKS.md** exists at `{folder}/outputs/REQUIREMENT_RISKS.md` — evidence: `ls -la` size > 1,024 bytes
2. **REQUIREMENT_RISKS.json** exists at `{folder}/shared/REQUIREMENT_RISKS.json` — evidence: `ls -la` size > 500 bytes and parses as valid JSON

### Schema fidelity
3. **REQUIREMENT_RISKS.json top-level keys** include `assessed_at`, `summary`, `heat_map`, `requirements`, `rtm_risks` — evidence: list actual top-level keys found
4. **Every rtm_risks entry** has `risk_id`, `title`, `category`, `likelihood`, `impact`, `risk_level`, `linked_requirement_ids`, `mitigation_strategies` — evidence: print key set of rtm_risks[0]
5. No `[:N]` slicing applied to deliverable content strings — evidence: grep for `\[:[0-9]+\]` in production code paths returned 0 hits; confirm NO `risks[:15]`, `high_risk[:15]`, `factor[:30]` slicing anywhere (confirmed removed per 2026-05-19 fix)

### Cross-stage consistency
6. **Risk register row count matches `RISKS.json` count ±5%** — evidence: print `summary.total_risks_tracked` vs `len(rtm_risks)` (must match); also confirm REQUIREMENT_RISKS.md row count approximately equals rtm_risks count
7. **Every risk row's Mitigation cell traces to source `mitigation_strategy` (singular) OR `mitigation_strategies` (array)** — evidence: count rtm_risks entries with empty mitigation_strategies array (must be 0 for HIGH/CRITICAL risks)
8. **Framework version in structural risks matches tech-lifecycle-evidence.json** — any structural risk mentioning a framework version (`.NET N`, `Java N`) must reference the same version as tech-lifecycle-evidence.json — evidence: grep structural risk titles for version patterns and confirm match
9. **No `_Showing N of M_` notices** in REQUIREMENT_RISKS.md — evidence: grep "_Showing" in file returned 0 matches
10. **No empty Mitigation cells** in tables — evidence: grep `\|[[:space:]]*\|` in HIGH/CRITICAL severity rows returned 0 matches

### Anti-regression rules (universal)
11. **UTF-8 encoding** on every `open()` call — evidence: search this phase's emitted scripts/code for `encoding='utf-8'` in every file-open
12. **ensure_ascii=False** on every `json.dump` call — evidence: same grep
13. **No mid-word table-cell truncations** — evidence: line-by-line cell-end check returned 0 hits

### Memory discipline
14. **Relevant SAFS memory entries reviewed and applied** — evidence: list which memory files were read and which rules were applicable (e.g., "structural risks sourced from tech-lifecycle-evidence.json — never hardcoded runtime versions")
