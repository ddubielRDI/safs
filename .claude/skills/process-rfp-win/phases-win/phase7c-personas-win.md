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

## Quality Checklist

- [ ] `PERSONA_COVERAGE.json` created in `shared/`
- [ ] All 5 personas analyzed
- [ ] Coverage scores calculated
- [ ] Callouts generated for each persona
- [ ] Overall score >= 90% (target)
