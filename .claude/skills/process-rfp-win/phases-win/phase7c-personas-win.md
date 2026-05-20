---
name: phase7c-personas-win
expert-role: UX Researcher
domain-expertise: Evaluator personas, stakeholder analysis
---

# Phase 7c: Evaluator Personas

## Expert Role

You are a **UX Researcher** with deep expertise in:
- Evaluator persona development
- Stakeholder analysis
- Content optimization for audiences
- User research methodologies

## Purpose

Generate evaluator personas and optimize content coverage for each persona type.

## Inputs

- `{folder}/shared/EVALUATION_CRITERIA.json`
- `{folder}/shared/requirements-normalized.json`
- `{folder}/shared/domain-context.json`

## Required Outputs

- `{folder}/shared/PERSONA_COVERAGE.json`

## Instructions

### Step 1: Define Evaluator Personas

```python
EVALUATOR_PERSONAS = {
    "TECHNICAL": {
        "name": "Technical Evaluator",
        "title": "IT Director / Solutions Architect",
        "priorities": ["scalability", "security", "integration", "architecture", "performance"],
        "concerns": ["Will this integrate with our systems?", "Is it secure?", "Can it scale?"],
        "weight": 0.30,
        "content_focus": ["ARCHITECTURE.md", "SECURITY_REQUIREMENTS.md", "INTEROPERABILITY.md"]
    },
    "FINANCIAL": {
        "name": "Financial Evaluator",
        "title": "CFO / Budget Analyst",
        "priorities": ["cost", "roi", "budget", "value", "efficiency"],
        "concerns": ["What's the total cost?", "What's the ROI?", "Hidden costs?"],
        "weight": 0.25,
        "content_focus": ["EFFORT_ESTIMATION.md", "Timeline sections", "Pricing"]
    },
    "RISK": {
        "name": "Risk Evaluator",
        "title": "Risk Manager / Compliance Officer",
        "priorities": ["compliance", "risk", "security", "audit", "mitigation"],
        "concerns": ["What are the risks?", "How are they mitigated?", "Compliance?"],
        "weight": 0.20,
        "content_focus": ["REQUIREMENT_RISKS.md", "SECURITY_REQUIREMENTS.md", "Compliance sections"]
    },
    "EXECUTIVE": {
        "name": "Executive Evaluator",
        "title": "CIO / Deputy Director",
        "priorities": ["strategic", "vision", "summary", "outcomes", "benefits"],
        "concerns": ["Will this achieve our goals?", "What's the business impact?"],
        "weight": 0.15,
        "content_focus": ["EXECUTIVE_SUMMARY.md", "Solution overview"]
    },
    "OPERATIONAL": {
        "name": "Operational Evaluator",
        "title": "Operations Manager / End User Representative",
        "priorities": ["usability", "workflow", "training", "support", "documentation"],
        "concerns": ["Is it user-friendly?", "How's the learning curve?", "Support?"],
        "weight": 0.10,
        "content_focus": ["UI_SPECS.md", "DEMO_SCENARIOS.md", "Training sections"]
    }
}
```

### Step 2: Analyze Content Coverage

```python
import glob

def analyze_persona_coverage(folder, personas):
    """Analyze how well content addresses each persona."""
    output_files = glob.glob(f"{folder}/outputs/*.md")

    # Combine all content
    all_content = ""
    for file_path in output_files:
        all_content += read_file(file_path) + "\n"

    all_content_lower = all_content.lower()

    coverage = {}
    for persona_id, persona in personas.items():
        # Count keyword matches
        priority_matches = sum(
            all_content_lower.count(priority.lower())
            for priority in persona["priorities"]
        )

        # Calculate coverage score (0-100)
        # More matches = higher score, capped at 100
        raw_score = min(100, priority_matches * 2)

        # Check for dedicated content sections
        dedicated_sections = sum(
            1 for doc in persona["content_focus"]
            if any(doc.lower() in f.lower() for f in output_files)
        )
        section_bonus = dedicated_sections * 10

        final_score = min(100, raw_score + section_bonus)

        coverage[persona_id] = {
            "persona": persona["name"],
            "priority_matches": priority_matches,
            "dedicated_sections": dedicated_sections,
            "coverage_score": final_score,
            "meets_target": final_score >= 90
        }

    return coverage

coverage = analyze_persona_coverage(folder, EVALUATOR_PERSONAS)
```

### Step 3: Generate Persona-Specific Callouts

```python
def generate_callouts(requirements, personas):
    """Generate persona-specific content callouts."""
    callouts = {}

    reqs = requirements.get("requirements", [])

    for persona_id, persona in personas.items():
        relevant_reqs = []
        for req in reqs:
            text_lower = req.get("text", "").lower()
            if any(p in text_lower for p in persona["priorities"]):
                relevant_reqs.append(req.get("canonical_id"))

        callouts[persona_id] = {
            "persona": persona["name"],
            "relevant_requirements": relevant_reqs[:20],
            "count": len(relevant_reqs),
            "sample_callout": generate_sample_callout(persona)
        }

    return callouts

def generate_sample_callout(persona):
    """Generate sample callout text for persona."""
    templates = {
        "TECHNICAL": "For our Technical Team: This solution leverages modern cloud architecture with proven scalability patterns.",
        "FINANCIAL": "Cost-Benefit Analysis: Our AI-assisted approach reduces development effort by 35%, delivering faster ROI.",
        "RISK": "Risk Mitigation: Comprehensive risk assessment identifies and addresses all critical concerns.",
        "EXECUTIVE": "Strategic Alignment: This solution directly supports your organization's digital transformation goals.",
        "OPERATIONAL": "User Experience: Intuitive interface design minimizes training requirements and maximizes adoption."
    }
    return templates.get(persona["name"].split()[0].upper(), "See detailed specifications.")

requirements = read_json(f"{folder}/shared/requirements-normalized.json")
callouts = generate_callouts(requirements, EVALUATOR_PERSONAS)
```

### Step 4: Calculate Overall Persona Score

```python
def calculate_persona_score(coverage, personas):
    """Calculate weighted persona coverage score."""
    total_score = 0
    for persona_id, data in coverage.items():
        weight = personas[persona_id]["weight"]
        total_score += data["coverage_score"] * weight

    return round(total_score, 1)

overall_score = calculate_persona_score(coverage, EVALUATOR_PERSONAS)
```

### Step 5: Write Output

```python
persona_output = {
    "analyzed_at": datetime.now().isoformat(),
    "overall_score": overall_score,
    "target_score": 90,
    "meets_target": overall_score >= 90,
    "personas": {
        persona_id: {
            "name": EVALUATOR_PERSONAS[persona_id]["name"],
            "title": EVALUATOR_PERSONAS[persona_id]["title"],
            "weight": EVALUATOR_PERSONAS[persona_id]["weight"],
            "priorities": EVALUATOR_PERSONAS[persona_id]["priorities"],
            "concerns": EVALUATOR_PERSONAS[persona_id]["concerns"],
            "coverage": coverage[persona_id],
            "callouts": callouts[persona_id]
        }
        for persona_id in EVALUATOR_PERSONAS.keys()
    },
    "recommendations": generate_recommendations(coverage)
}

def generate_recommendations(coverage):
    """Generate recommendations for improving coverage."""
    recs = []
    for persona_id, data in coverage.items():
        if not data["meets_target"]:
            recs.append({
                "persona": data["persona"],
                "current_score": data["coverage_score"],
                "recommendation": f"Add more content addressing {EVALUATOR_PERSONAS[persona_id]['priorities'][:3]}"
            })
    return recs

write_json(f"{folder}/shared/PERSONA_COVERAGE.json", persona_output)
```

### Step 6: Report Results

```python
log(f"""
👥 Evaluator Persona Analysis Complete
======================================
Overall Score: {overall_score}% {"✅" if overall_score >= 90 else "⚠️"}
Target: 90%+

Persona Coverage:
""")

for persona_id, data in coverage.items():
    status = "✅" if data["meets_target"] else "⚠️"
    log(f"  {status} {EVALUATOR_PERSONAS[persona_id]['name']}: {data['coverage_score']}%")
```

## Quality Checklist (MANDATORY — report each by name with evidence)

The phase agent MUST verify each of the following BEFORE reporting completion. The agent's completion report MUST include a checklist-results block with:
- Item name (verbatim from below)
- PASS / FAIL / SKIPPED-WITH-REASON
- Evidence (file:line citation, grep result, file size, assertion that ran, etc.)

"All checks passed" without per-item evidence is NOT acceptable.

### Required output files
1. **PERSONA_COVERAGE.json** exists at `{folder}/shared/PERSONA_COVERAGE.json` — evidence: `ls -la` size > 200 bytes and parses as valid JSON

### Schema fidelity
2. **PERSONA_COVERAGE.json top-level keys** include `analyzed_at`, `overall_score`, `target_score`, `meets_target`, `personas`, `recommendations` — evidence: list actual top-level keys found
3. **All 5 personas present** (TECHNICAL, FINANCIAL, RISK, EXECUTIVE, OPERATIONAL) — evidence: print `list(personas.keys())`
4. **Every persona entry** has `name`, `title`, `weight`, `coverage`, `callouts` — evidence: print key set of personas["TECHNICAL"]
5. No `[:N]` slicing applied to deliverable content strings — evidence: grep for `\[:[0-9]+\]` in production code paths returned 0 hits

### Cross-stage consistency
6. **Coverage scores calculated** for all 5 personas — evidence: print `{k: v["coverage"]["coverage_score"] for k, v in personas.items()}`
7. **Overall score reported** — evidence: print `overall_score` (any value; note if < 90% target with recommendation)
8. **Recommendations generated** for personas below target — evidence: print `len(recommendations)` and confirm non-empty entries for personas with coverage_score < 90

### Anti-regression rules (universal)
9. **UTF-8 encoding** on every `open()` call — evidence: search this phase's emitted scripts/code for `encoding='utf-8'` in every file-open
10. **ensure_ascii=False** on every `json.dump` call — evidence: same grep
11. **No `_Showing N of M_` row-cap notices** in any deliverable markdown — evidence: grep returned 0 matches
12. **No empty `|  |` mitigation/cell patterns** in any deliverable table — evidence: grep returned 0 matches
13. **No mid-word table-cell truncations** — evidence: line-by-line cell-end check returned 0 hits

### Memory discipline
14. **Relevant SAFS memory entries reviewed and applied** — evidence: list which memory files were read and which rules were applicable
