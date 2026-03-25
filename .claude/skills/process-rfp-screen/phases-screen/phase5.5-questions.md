---
name: phase5.5-questions
expert-role: Senior Capture Manager & Requirements Analyst
domain-expertise: RFP analysis, requirements elicitation, Q&A period strategy, proposal capture management
skill: capture-strategist
sub-skill: competitive-positioning
---

# Phase 5.5: Clarifying Questions Generation

**Purpose:** Generate prioritized, RFP-specific clarifying questions by cross-referencing all prior phase findings against the raw RFP text. Output is professional enough to submit directly during a client Q&A period.

**Inputs:**
- `{folder}/screen/rfp-summary.json` — Phase 1 output
- `{folder}/screen/go-nogo-score.json` — Phase 2 output
- `{folder}/screen/client-intel-snapshot.json` — Phase 3 output (may not exist if --quick)
- `{folder}/screen/compliance-check.json` — Phase 4a output
- `{folder}/screen/past-projects-match.json` — Phase 4b output
- `{folder}/screen/preliminary-themes.json` — Phase 4.5 output
- `{folder}/screen/risk-assessment.json` — Phase 5 output
- `{folder}/screen/BID_SCREEN.json` — Phase 5 consolidated output
- Combined RFP text (already in memory from prior phases)

**Required Output:**
- `{folder}/screen/clarifying-questions.json` (>1KB)
- Updated `{folder}/screen/BID_SCREEN.json` with `clarifying_questions` key

---

## Instructions

### Step 1: Load All Prior Phase Outputs

```python
rfp_summary = read_json(f"{folder}/screen/rfp-summary.json")
go_nogo = read_json(f"{folder}/screen/go-nogo-score.json")
compliance = read_json(f"{folder}/screen/compliance-check.json")
past_matches = read_json(f"{folder}/screen/past-projects-match.json")
risk_assessment = read_json(f"{folder}/screen/risk-assessment.json")

# Optional outputs
client_intel = read_json_safe(f"{folder}/screen/client-intel-snapshot.json")
preliminary_themes = read_json_safe(f"{folder}/screen/preliminary-themes.json")

# Helper to find assessment area by name prefix
def find_area(areas, name_prefix):
    for a in areas:
        if a.get("name", "").lower().startswith(name_prefix.lower()):
            return a
    return {}

assessment_areas = go_nogo.get("assessment_areas", [])
```

### Step 2: Build Trigger Inventory (Deterministic Scan)

Scan all prior phase outputs for gaps, ambiguities, and unknowns. Each trigger maps to a question category and includes the source finding.

```python
triggers = []

# --- Trigger Source 1: Phase 1 rfp-summary.json - Missing metadata ---
# Fields commonly containing "not disclosed" / "not specified" / "not found"
metadata_fields = {
    "estimated_value": "budget_pricing",
    "submission_deadline": "scope_boundaries",
    "contract_type": "budget_pricing",
    "period_of_performance": "scope_boundaries",
    "set_aside": "compliance_gaps",
    "questions_deadline": "evaluation_process"
}

for field, category in metadata_fields.items():
    value = str(rfp_summary.get(field, "")).lower()
    if not value or value == "none":
        continue  # Skip truly empty fields silently
    if any(phrase in value for phrase in ["not disclosed", "not specified", "not found", "unknown", "tbd", "n/a"]):
        triggers.append({
            "source": f"Phase 1: rfp-summary.{field}",
            "finding": f"{field.replace('_', ' ').title()} is '{rfp_summary.get(field, '')}'",
            "category": category,
            "severity": "high" if field in ("estimated_value", "period_of_performance") else "medium"
        })

# --- Trigger Source 2: Phase 1 mandatory_requirements - Vague patterns ---
vague_patterns = [
    "scalable", "flexible", "user-friendly", "robust", "modern",
    "comprehensive", "as needed", "various", "state-of-the-art",
    "best practices", "industry standard", "appropriate", "sufficient",
    "reasonable", "adequate"
]
mandatory_reqs = rfp_summary.get("mandatory_requirements", [])
for req in mandatory_reqs:
    req_lower = req.lower() if isinstance(req, str) else str(req).lower()
    matched_vague = [p for p in vague_patterns if p in req_lower]
    if matched_vague:
        triggers.append({
            "source": f"Phase 1: mandatory_requirements",
            "finding": f"Vague language in requirement: '{req[:100]}' (terms: {', '.join(matched_vague)})",
            "category": "scope_boundaries",
            "severity": "medium"
        })

# --- Trigger Source 3: Phase 1 required_technologies - Missing versions ---
required_techs = rfp_summary.get("required_technologies", [])
for tech in required_techs:
    tech_str = str(tech)
    # Check if technology lacks a version number (no digits or version patterns)
    import re
    has_version = bool(re.search(r'\d+\.?\d*', tech_str)) or any(
        v in tech_str.lower() for v in ["latest", "current", "lts"]
    )
    if not has_version and len(tech_str) > 2:
        triggers.append({
            "source": f"Phase 1: required_technologies",
            "finding": f"Technology '{tech_str}' specified without version/edition",
            "category": "technical_requirements",
            "severity": "medium"
        })

# --- Trigger Source 4: Phase 2 assessment_areas risks ---
for area in assessment_areas:
    area_name = area.get("name", "")
    area_risks = area.get("risks", [])
    for risk_text in area_risks:
        # Map area name to question category
        area_category_map = {
            "strategic fit": "scope_boundaries",
            "technical capability": "technical_requirements",
            "competitive position": "evaluation_process",
            "resource availability": "operational_context",
            "financial viability": "budget_pricing",
            "risk assessment": "scope_boundaries",
            "win probability": "evaluation_process"
        }
        category = area_category_map.get(area_name.lower(), "scope_boundaries")
        triggers.append({
            "source": f"Phase 2: {area_name} risks",
            "finding": str(risk_text)[:150],
            "category": category,
            "severity": "high" if area.get("score", 100) < 40 else "medium"
        })

# --- Trigger Source 5: Phase 4a compliance_items with RISK or PARTIAL ---
for item in compliance.get("compliance_items", []):
    status = item.get("status", "")
    if status in ("RISK", "PARTIAL"):
        triggers.append({
            "source": f"Phase 4a: compliance ({status})",
            "finding": f"Compliance {status}: {item.get('requirement', '')[:120]}",
            "category": "compliance_gaps",
            "severity": "high" if status == "RISK" else "medium"
        })

# --- Trigger Source 6: Phase 4b match_quality weak ---
match_quality = past_matches.get("match_quality", "")
if match_quality == "weak":
    triggers.append({
        "source": "Phase 4b: past-projects-match",
        "finding": "Weak past project match -- scope may not align with company strengths",
        "category": "scope_boundaries",
        "severity": "medium"
    })

# --- Trigger Source 7: Phase 4.5 uncovered buyer priorities ---
if preliminary_themes:
    buyer_coverage = preliminary_themes.get("buyer_priority_coverage", {})
    uncovered = buyer_coverage.get("uncovered", [])
    for priority_name in uncovered:
        triggers.append({
            "source": "Phase 4.5: buyer_priority_coverage.uncovered",
            "finding": f"HIGH buyer priority not addressed by any theme: '{priority_name}'",
            "category": "scope_boundaries",
            "severity": "high"
        })

# --- Trigger Source 8: High-point eval criteria without decomposed subfactors (Batch 3) ---
evaluation_model = rfp_summary.get("evaluation_model", {})
point_allocation = evaluation_model.get("point_allocation", [])
for criterion in point_allocation:
    criterion_points = criterion.get("points", 0)
    subfactors = criterion.get("subfactors", [])
    # Flag high-point criteria (>=200 points) that lack subfactors — these are opaque to bidders
    if criterion_points >= 200 and len(subfactors) == 0:
        triggers.append({
            "source": "Phase 1: evaluation_model.point_allocation",
            "finding": f"High-point criterion '{criterion.get('criterion', '')}' ({criterion_points} pts) has no decomposed subfactors -- evaluation basis is opaque",
            "category": "evaluation_process",
            "severity": "high"
        })
    elif criterion_points >= 100 and len(subfactors) <= 1:
        triggers.append({
            "source": "Phase 1: evaluation_model.point_allocation",
            "finding": f"Criterion '{criterion.get('criterion', '')}' ({criterion_points} pts) has minimal subfactor decomposition -- scoring basis unclear",
            "category": "evaluation_process",
            "severity": "medium"
        })

# --- Trigger Source 9: Tech gaps from Phase 4 tech_gap_analysis (Batch 3) ---
tech_gap = past_matches.get("tech_gap_analysis", {})
techs_without_coverage = tech_gap.get("technologies_without_coverage", [])
gap_severity = tech_gap.get("gap_severity", "none")
if techs_without_coverage:
    for tech_name in techs_without_coverage[:3]:  # Cap at 3 to avoid trigger flood
        triggers.append({
            "source": "Phase 4: tech_gap_analysis",
            "finding": f"Technology '{tech_name}' required by RFP has no past project coverage -- version/deployment expectations unclear",
            "category": "technical_requirements",
            "severity": "high" if gap_severity in ("medium", "high") else "medium"
        })

# --- Trigger Source 10: Phase 5 high-severity risks ---
for risk_item in risk_assessment.get("risks", []):
    if risk_item.get("severity") == "high":
        triggers.append({
            "source": f"Phase 5: risk-assessment ({risk_item.get('category', 'general')})",
            "finding": risk_item.get("risk", "")[:150],
            "category": {
                "compliance": "compliance_gaps",
                "scoring": "scope_boundaries",
                "experience": "scope_boundaries",
                "timeline": "operational_context"
            }.get(risk_item.get("category", ""), "scope_boundaries"),
            "severity": "high"
        })

# --- Trigger Source: Unknown incumbent ---
# When the competitive landscape shows incumbent is unknown, generate a strategic question
incumbent = client_intel.get("intelligence", {}).get("competitive_landscape", {}).get("incumbent", "Unknown") if client_intel else "Unknown"
if incumbent in ("Unknown", "unknown", None, ""):
    triggers.append({
        "source": "Phase 3: competitive_landscape",
        "finding": "Incumbent/predecessor vendor is unknown -- cannot assess competitive positioning or client satisfaction with prior work",
        "category": "evaluation_process",
        "severity": "high"
    })

# --- Trigger Source: Prior contract outcome ---
# Always valuable to understand what happened with the predecessor contract
if incumbent in ("Unknown", "unknown", None, ""):
    triggers.append({
        "source": "Phase 3: competitive_landscape",
        "finding": "Prior contract outcome unknown -- understanding client satisfaction with predecessor work informs positioning strategy",
        "category": "operational_context",
        "severity": "medium"
    })

log(f"  Trigger inventory: {len(triggers)} triggers found")
log(f"  By category: { {cat: sum(1 for t in triggers if t['category'] == cat) for cat in set(t['category'] for t in triggers)} }")
```

### Step 3: Edge Case -- Empty Trigger Inventory

If no triggers found, write minimal output and exit early.

```python
if len(triggers) == 0:
    from datetime import datetime
    minimal_output = {
        "phase": "5.5",
        "phase_name": "Clarifying Questions",
        "timestamp": datetime.now().isoformat(),
        "rfp_number": rfp_summary.get("rfp_number", "Not found"),
        "client_name": rfp_summary.get("client_name", "Unknown"),
        "questions_deadline": rfp_summary.get("questions_deadline", "Not found"),
        "questions": [],
        "summary": {
            "total_questions": 0,
            "by_priority": {"HIGH": 0, "MEDIUM": 0, "LOW": 0},
            "by_category": {}
        },
        "trigger_count": 0,
        "note": "No ambiguities or gaps detected in prior phase outputs. RFP appears well-specified."
    }
    write_json(f"{folder}/screen/clarifying-questions.json", minimal_output)

    # Update BID_SCREEN.json
    bid_screen = read_json(f"{folder}/screen/BID_SCREEN.json")
    bid_screen["clarifying_questions"] = minimal_output
    write_json(f"{folder}/screen/BID_SCREEN.json", bid_screen)

    log("  No triggers found -- RFP appears well-specified. Minimal output written.")
    # Skip to report
    return
```

### Skill Integration: Capture-Strategist & Competitive-Positioning for Question Strategy (MANDATORY)

The **capture-strategist** and **competitive-positioning** sub-skill are loaded in context. Apply these frameworks to question generation:

**Competitive Positioning Lens:** Prioritize questions whose answers would create competitive advantage, not just reduce ambiguity. A capture strategist knows that Q&A questions are a positioning tool:
- Questions that demonstrate domain understanding signal competence to the evaluator
- Questions that expose areas where competitors are likely weak position your strengths
- Questions should show pre-RFP engagement understanding (customer intimacy)

**Shipley Q&A Period Strategy:** Questions should signal capability without revealing strategy:
- DO ask about scope details that show you understand the client's environment
- DO ask about evaluation process details that help you tailor the response
- DON'T ask questions that reveal you lack basic domain knowledge
- DON'T ask questions that tip your pricing or teaming strategy

**Ghost Strategy Awareness:** When framing questions, consider:
- Questions that highlight requirements where you have a unique advantage
- Questions that force the client to clarify requirements where incumbents may be coasting
- Questions that surface evaluation criteria favoring your differentiators

**Strategic Question Category:** Beyond the 8 technical categories, some questions should be flagged as "demonstrates_expertise" — questions that a domain expert would ask but a generalist wouldn't think to. These signal customer intimacy.

**Anti-Pattern Guard:** Check against "RFP-reactive capture" — questions should show strategic engagement, not just gap-filling.

---

### Step 4: LLM Synthesis -- Generate Questions

Pass the trigger inventory, RFP text, buyer priorities, and evaluation criteria to a single LLM call.

```python
from datetime import datetime

# Gather context for the prompt
buyer_priorities = rfp_summary.get("buyer_priorities", [])
eval_criteria = rfp_summary.get("evaluation_criteria", [])
rfp_title = rfp_summary.get("rfp_title", "Unknown")
client_name = rfp_summary.get("client_name", "Unknown")
questions_deadline = rfp_summary.get("questions_deadline", "Not found")

# Format triggers for the prompt (group by category for clarity)
trigger_summary = ""
categories_seen = set()
for t in triggers:
    categories_seen.add(t["category"])
    trigger_summary += f"- [{t['severity'].upper()}] [{t['category']}] {t['finding']} (Source: {t['source']})\n"

# Load client tone for question framing (Batch 3)
client_tone = go_nogo.get("client_tone", {})
primary_style = client_tone.get("primary_style", "formal_bureaucratic")
mirroring_vocab = client_tone.get("mirroring_vocabulary", [])
adaptation_rules = client_tone.get("adaptation_rules", {})
formality_level = adaptation_rules.get("formality_level", "formal")
preferred_terms = adaptation_rules.get("preferred_terms", [])
avoid_terms = adaptation_rules.get("avoid_terms", [])

# Build the LLM prompt
question_prompt = f"""You are a Senior Capture Manager preparing clarifying questions for a client Q&A period.

## RFP Context
- Title: {rfp_title}
- Client: {client_name}
- Questions Deadline: {questions_deadline}

## Buyer Priorities (from RFP analysis)
{chr(10).join(f"- {bp.get('name', '')} ({bp.get('importance', '')}): {bp.get('signal', '')}" for bp in buyer_priorities[:8]) if buyer_priorities else "None extracted."}

## Evaluation Criteria
{chr(10).join(f"- {ec}" for ec in eval_criteria[:8]) if eval_criteria else "None extracted."}

## Gap & Ambiguity Triggers (from screening analysis)
{trigger_summary}

## Combined RFP Text (first 40,000 chars)
{combined_text[:40000] if 'combined_text' in dir() else rfp_summary.get('scope_summary', 'RFP text not available in memory.')}

---

## CLIENT TONE ADAPTATION (from Phase 1 analysis)
- Client communication style: {primary_style}
- Formality level: {formality_level}
- Mirror these client vocabulary terms where natural: {', '.join(mirroring_vocab[:10]) if mirroring_vocab else 'N/A'}
- Preferred terms: {', '.join(preferred_terms[:5]) if preferred_terms else 'N/A'}
- Terms to avoid: {', '.join(avoid_terms[:5]) if avoid_terms else 'N/A'}
- Match formality: {"Use formal, structured language with precise references" if formality_level == "formal" else "Use clear, direct language with a collaborative tone"}
- If the client's style is prescriptive/directive, frame questions as seeking clarification on specifics.
  If the client's style is collaborative, frame questions as seeking to understand partnership expectations.

## Your Task

Generate 8-15 clarifying questions based on the triggers above. Each question should be something a professional capture manager would submit during a formal Q&A period. Apply the CLIENT TONE ADAPTATION above to match the client's communication style.

## STRICT RULES

1. **Every question MUST reference a specific RFP section, requirement, or term** -- not vague references
2. **Never ask what is already clearly answered in the RFP text** -- you have the full text above
3. **Never ask boilerplate questions** such as:
   - Point of contact or submission address
   - Page limits or formatting requirements
   - Whether the deadline can be extended
   - Generic "can you provide more details on X"
   - Questions starting with "Can you clarify..."
4. **Professional tone** suitable for written submission to a government or corporate client
5. **Group related sub-questions** under a single item where natural (use semicolons or "specifically, ...")
6. **Each question must be actionable** -- the answer would change our bid approach, cost estimate, or technical design
7. **Prefer fewer, sharper questions** over padding to hit the count

## CATEGORIES (assign exactly one per question)
- technical_requirements: Technology versions, platform specifics, architecture constraints
- scope_boundaries: Scale, user counts, geographic scope, in/out of scope items
- design_ux: User experience expectations, accessibility, personas
- operational_context: Maintenance, SLAs, staff skill levels, transition requirements
- evaluation_process: Scoring rubric details, interview/demo process, evaluation timeline
- budget_pricing: Budget range, rate constraints, funding source, pricing structure
- compliance_gaps: Regulatory or legal requirements needing client confirmation
- data_integration: Existing systems, data schemas, migration paths, API requirements

## STRATEGIC QUESTION FRAMING (from capture-strategist skill)
Apply these lenses when crafting questions:
- Frame questions through COMPETITIVE POSITIONING lens -- prioritize questions whose answers create competitive advantage
- Reference APMP Customer Intimacy -- questions should demonstrate domain understanding to the evaluator
- Apply GHOST STRATEGY awareness -- questions that expose areas where competitors are likely weak should be prioritized
- Apply Shipley Q&A strategy -- questions should signal capability without revealing strategy
- Flag questions that "demonstrate expertise" -- questions a domain expert would ask but a generalist wouldn't
- Avoid "RFP-reactive" questions that merely fill gaps without strategic value

## PRIORITY LEVELS
- HIGH: Answer changes bid decision or fundamentally alters technical approach
- MEDIUM: Answer meaningfully affects proposal strategy, staffing, or cost estimate
- LOW: Answer improves proposal quality but does not change approach

## OUTPUT FORMAT (JSON array)

Return ONLY a valid JSON array. No markdown, no commentary.

[
  {{
    "question": "Professional question text referencing specific RFP content",
    "category": "one_of_the_8_categories",
    "priority": "HIGH|MEDIUM|LOW",
    "rfp_reference": "Specific section, page, or quoted requirement that prompted this",
    "impact": "What answering would clarify for the proposal team",
    "related_finding": "Which screening phase finding revealed this gap",
    "demonstrates_expertise": true/false,
    "strategic_value": "Brief note on competitive advantage this question creates (or 'informational' if purely gap-filling)",
    "evaluation_criterion_targeted": "Name of the evaluation criterion this question relates to (from evaluation_model), or null if not applicable"
  }}
]

Generate the questions now."""

# Single LLM call
raw_response = llm(question_prompt)

# Parse JSON from response
import json
# Strip markdown code fences if present
response_text = raw_response.strip()
if response_text.startswith("```"):
    response_text = response_text.split("\n", 1)[1] if "\n" in response_text else response_text[3:]
if response_text.endswith("```"):
    response_text = response_text[:-3]
response_text = response_text.strip()
if response_text.startswith("json"):
    response_text = response_text[4:].strip()

questions_raw = json.loads(response_text)
```

### Step 5: Post-Validation & Quality Enforcement

```python
VALID_CATEGORIES = {
    "technical_requirements", "scope_boundaries", "design_ux",
    "operational_context", "evaluation_process", "budget_pricing",
    "compliance_gaps", "data_integration"
}
VALID_PRIORITIES = {"HIGH", "MEDIUM", "LOW"}
BOILERPLATE_PHRASES = [
    "point of contact", "submission address", "page limit",
    "formatting requirement", "extend the deadline", "can you clarify",
    "please provide more details", "could you elaborate"
]

validated_questions = []
category_counts = {}

for q in questions_raw:
    # Skip if missing required fields
    if not all(k in q for k in ("question", "category", "priority")):
        continue

    question_text = q["question"].strip()

    # Reject questions < 30 chars
    if len(question_text) < 30:
        continue

    # Reject boilerplate phrases
    if any(bp in question_text.lower() for bp in BOILERPLATE_PHRASES):
        continue

    # Normalize category
    category = q["category"].strip().lower()
    if category not in VALID_CATEGORIES:
        # Try to map close matches
        category = "scope_boundaries"  # Default fallback

    # Normalize priority
    priority = q["priority"].strip().upper()
    if priority not in VALID_PRIORITIES:
        priority = "MEDIUM"

    # Enforce max 4 per category
    if category_counts.get(category, 0) >= 4:
        continue

    validated_questions.append({
        "question": question_text,
        "category": category,
        "priority": priority,
        "rfp_reference": q.get("rfp_reference", "General RFP review"),
        "impact": q.get("impact", ""),
        "related_finding": q.get("related_finding", ""),
        "demonstrates_expertise": q.get("demonstrates_expertise", False),
        "strategic_value": q.get("strategic_value", "informational"),
        "evaluation_criterion_targeted": q.get("evaluation_criterion_targeted")  # From evaluation_model (Batch 3)
    })
    category_counts[category] = category_counts.get(category, 0) + 1

# Enforce priority caps: max 5 HIGH
high_count = sum(1 for q in validated_questions if q["priority"] == "HIGH")
if high_count > 5:
    # Demote excess HIGH to MEDIUM (keep first 5 HIGHs)
    high_seen = 0
    for q in validated_questions:
        if q["priority"] == "HIGH":
            high_seen += 1
            if high_seen > 5:
                q["priority"] = "MEDIUM"

# Enforce min 1 HIGH (promote highest-impact MEDIUM if needed)
high_count = sum(1 for q in validated_questions if q["priority"] == "HIGH")
if high_count == 0 and validated_questions:
    validated_questions[0]["priority"] = "HIGH"

# Enforce total 8-15 range
if len(validated_questions) > 15:
    # Keep up to 15, prioritizing HIGH > MEDIUM > LOW
    priority_order = {"HIGH": 0, "MEDIUM": 1, "LOW": 2}
    validated_questions.sort(key=lambda q: priority_order.get(q["priority"], 1))
    validated_questions = validated_questions[:15]

# Assign IDs
for i, q in enumerate(validated_questions, 1):
    q["id"] = f"Q{i}"

# Check min 3 categories covered
categories_covered = len(set(q["category"] for q in validated_questions))

log(f"  Validated: {len(validated_questions)} questions across {categories_covered} categories")
if len(validated_questions) < 8:
    log(f"  NOTE: Only {len(validated_questions)} questions passed validation (target: 8-15)")
if categories_covered < 3:
    log(f"  NOTE: Only {categories_covered} categories covered (target: 3+)")
```

### Step 6: Build Summary Statistics

```python
by_priority = {
    "HIGH": sum(1 for q in validated_questions if q["priority"] == "HIGH"),
    "MEDIUM": sum(1 for q in validated_questions if q["priority"] == "MEDIUM"),
    "LOW": sum(1 for q in validated_questions if q["priority"] == "LOW")
}

by_category = {}
for q in validated_questions:
    cat = q["category"]
    by_category[cat] = by_category.get(cat, 0) + 1

summary = {
    "total_questions": len(validated_questions),
    "by_priority": by_priority,
    "by_category": by_category
}
```

### Step 7: Write Output

```python
output = {
    "phase": "5.5",
    "phase_name": "Clarifying Questions",
    "timestamp": datetime.now().isoformat(),
    "rfp_number": rfp_summary.get("rfp_number", "Not found"),
    "client_name": client_name,
    "questions_deadline": questions_deadline,
    "questions": validated_questions,
    "summary": summary,
    "trigger_count": len(triggers),
    "categories_covered": categories_covered,
    "methodology": "Two-step: deterministic trigger scan of prior phase outputs, then single LLM synthesis with post-validation. Quality rules: 8-15 questions, max 5 HIGH, min 1 HIGH, max 4 per category, min 3 categories, reject <30 chars and boilerplate."
}
write_json(f"{folder}/screen/clarifying-questions.json", output)
```

### Step 8: Update BID_SCREEN.json

Re-read the consolidated file and add the clarifying questions data.

```python
bid_screen = read_json(f"{folder}/screen/BID_SCREEN.json")
bid_screen["clarifying_questions"] = output
write_json(f"{folder}/screen/BID_SCREEN.json", bid_screen)
```

### Step 9: Report

```
CLARIFYING QUESTIONS (Phase 5.5)
==================================
Triggers Scanned: {len(triggers)}
Questions Generated: {len(validated_questions)}
Categories Covered: {categories_covered}
Priority Breakdown: {by_priority['HIGH']} HIGH | {by_priority['MEDIUM']} MEDIUM | {by_priority['LOW']} LOW

Questions:
{for each in validated_questions:
  f"  {q['id']}. [{q['priority']}] [{q['category']}] {q['question'][:80]}..."}

Outputs:
  screen/clarifying-questions.json
  screen/BID_SCREEN.json (updated with clarifying_questions key)
```

---

## Quality Checklist

- [ ] `clarifying-questions.json` written (>1KB unless 0 questions)
- [ ] `BID_SCREEN.json` updated with `clarifying_questions` key
- [ ] All 8 prior phase outputs loaded (graceful handling if optional ones missing)
- [ ] Trigger inventory built deterministically from prior outputs (not re-analyzing RFP)
- [ ] Single LLM call for question synthesis (not multiple calls)
- [ ] Post-validation applied: min 30 chars, no boilerplate, valid categories/priorities
- [ ] Priority caps enforced: max 5 HIGH, min 1 HIGH (if any questions exist)
- [ ] Category caps enforced: max 4 per category
- [ ] Total questions in 8-15 range (logged if outside range)
- [ ] Min 3 categories covered (logged if fewer)
- [ ] Each question has: id, question, category, priority, rfp_reference, impact, related_finding, evaluation_criterion_targeted
- [ ] Empty trigger inventory handled gracefully (0 questions, minimal output)
- [ ] Questions reference specific RFP content (not generic)
- [ ] Professional tone suitable for client Q&A submission

### Intelligence Layer Integration Quality Checks (Batch 3)
- [ ] Trigger Source 8: High-point eval criteria without subfactors detected from `evaluation_model.point_allocation`
- [ ] Trigger Source 9: Tech gaps from Phase 4 `tech_gap_analysis` added as triggers
- [ ] `client_tone` loaded from go-nogo-score.json and injected into LLM prompt
- [ ] CLIENT TONE ADAPTATION section included in prompt (style, vocabulary, formality, preferred/avoid terms)
- [ ] `evaluation_criterion_targeted` field present per validated question
- [ ] Questions reflect client tone adaptation (formality level, vocabulary mirroring)

### Skill Integration Quality Checks (capture-strategist + competitive-positioning)
- [ ] Questions framed through competitive positioning lens (not just gap-filling)
- [ ] At least 1-2 questions flagged as demonstrates_expertise = true
- [ ] strategic_value field populated for each question
- [ ] Questions signal capability without revealing strategy (Shipley Q&A discipline)
- [ ] **Anti-pattern check:** No "RFP-reactive capture" — questions show strategic engagement
