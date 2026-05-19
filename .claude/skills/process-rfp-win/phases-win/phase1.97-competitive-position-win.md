---
name: phase1.97-competitive-position-win
expert-role: Competitive Strategist
domain-expertise: Win/loss analysis, ghost strategy, incumbent vs challenger positioning
---

# Phase 1.97: Competitive Position (Stage 1, after 1.95 Intel)

## Purpose

Build `COMPETITIVE_POSITION.md` and `shared/bid/COMPETITIVE_POSITION.json` with an explicit **ghost strategy**, **pain-point map**, **switching-cost analysis**, and **win conditions** — a Stage 1 artifact that gives Stage 3 architects a competitive frame BEFORE they design.

Historically this analysis was buried in `phase8.0-positioning-win.md` (Stage 7), which meant architects designed in a competitive vacuum and bid authors back-fit positioning to architecture decisions already made. Pulling this to Stage 1 means architecture and competitive strategy converge from the start: ghost weaknesses inform component choices; switching-cost analysis informs transition planning; win conditions inform every Stage 3 spec.

## Expert Role

You are a **Competitive Strategist** with deep expertise in:

- Win/loss analysis (Shipley capture management methodology)
- Ghost strategy authoring (positioning AGAINST competitors without naming them)
- Incumbent-vs-challenger positioning (incumbency advantages and how to neutralize them)
- Pain-point mapping (buyer's documented frustrations → bidder's discriminating strengths)
- Switching-cost analysis (data migration, retraining, contract-overlap costs the buyer absorbs)

## Inputs

- `{folder}/shared/bid/CLIENT_INTELLIGENCE.json` — incumbent name, contract history, known pain points, decision-maker hints (produced by Phase 1.95)
- `{folder}/shared/GO_NOGO_DECISION.json` — score breakdown shows where we are strongest / weakest
- `{folder}/shared/EVALUATION_CRITERIA.json` — what factors will be scored (frame positioning to factor weights)
- `{folder}/shared/COMPLIANCE_MATRIX.json` — mandatory items the incumbent satisfies vs ones they don't
- `{folder}/shared/domain-context.json` — buyer type (government / commercial), procurement vehicle, contract length
- `${CLAUDE_SKILL_DIR}/config-win/company-profile.json` — bidder's discriminating strengths and reference past-performance

## Required Outputs

- `{folder}/outputs/COMPETITIVE_POSITION.md` (≥3 KB) — human-readable strategy document
- `{folder}/shared/bid/COMPETITIVE_POSITION.json` — machine-readable; consumed by phase3a, 3b, 3c, 3e, 3f, 3g (architects use it to weight design decisions), AND by phase8.0-positioning-win.md (which loads it as input instead of recomputing)

## Instructions

### Step 1: Load Inputs

```python
import os
from datetime import datetime

client_intel    = read_json_safe(f"{folder}/shared/bid/CLIENT_INTELLIGENCE.json") or {}
go_nogo         = read_json_safe(f"{folder}/shared/GO_NOGO_DECISION.json") or {}
evaluation      = read_json_safe(f"{folder}/shared/EVALUATION_CRITERIA.json") or {}
compliance      = read_json_safe(f"{folder}/shared/COMPLIANCE_MATRIX.json") or {}
domain_context  = read_json_safe(f"{folder}/shared/domain-context.json") or {}

skill_dir = os.environ.get("CLAUDE_SKILL_DIR") or os.path.dirname(os.path.abspath(__file__))
company_profile = read_json_safe(f"{skill_dir}/config-win/company-profile.json") or {}
```

### Step 2: Incumbent Profile

```python
incumbent = {
    "name":          client_intel.get("incumbent_name") or client_intel.get("current_vendor"),
    "contract_term": client_intel.get("incumbent_contract_term"),
    "known_issues":  client_intel.get("incumbent_pain_points", []) or client_intel.get("issues", []),
    "known_strengths": client_intel.get("incumbent_strengths", []),
    "rebid_signals": client_intel.get("rebid_signals", []),  # e.g., "RFP scope expanded", "new evaluation criteria"
}
has_incumbent = bool(incumbent["name"])
```

### Step 3: Build the Ghost Strategy

A **ghost** is a competitor whose weaknesses we describe in our bid WITHOUT naming them. The ghost paragraph reads "Some legacy systems require batch-only data transfer..." not "ACME Inc. requires batch-only data transfer". The evaluator recognizes whom we are describing without us creating libel risk.

```python
ghosts = []
for issue in incumbent["known_issues"]:
    issue_text = issue.get("text") if isinstance(issue, dict) else str(issue)
    # Convert specific complaint into a ghost-form positioning
    ghosts.append({
        "incumbent_weakness": issue_text,
        "ghost_phrasing": _ghost_phrase(issue_text),
        "our_counter": _company_counter(issue_text, company_profile),
        "evaluation_factor_link": _link_to_factor(issue_text, evaluation),
    })


def _ghost_phrase(weakness: str) -> str:
    """Turn 'ACME's slow batch transfer' into 'Some legacy systems require batch-only data transfer'."""
    weakness = weakness.strip()
    if weakness.lower().startswith("acme") or weakness.lower().startswith(incumbent["name"].lower() if incumbent["name"] else ""):
        # Strip the proper noun
        weakness = weakness.split(" ", 1)[1] if " " in weakness else weakness
    return f"Some legacy systems {weakness}"


def _company_counter(weakness: str, profile: dict) -> str:
    """Pull a discriminating strength from company-profile that addresses the weakness."""
    strengths = (profile.get("discriminating_strengths") or
                 profile.get("differentiators") or [])
    # Naive keyword overlap; agents may refine
    weakness_words = set(weakness.lower().split())
    best = None
    best_score = 0
    for s in strengths:
        s_text = s if isinstance(s, str) else s.get("text", "")
        overlap = len(weakness_words & set(s_text.lower().split()))
        if overlap > best_score:
            best = s_text
            best_score = overlap
    return best or "(populate with a verified company strength)"


def _link_to_factor(weakness: str, evaluation: dict) -> str | None:
    """Identify which evaluation factor a ghost statement helps win."""
    factors = evaluation.get("evaluation_factors") or evaluation.get("factors") or []
    weakness_lower = weakness.lower()
    for f in factors:
        name = (f.get("factor_name") or f.get("name") or "").lower()
        if any(tok in weakness_lower for tok in name.split()):
            return f.get("factor_name") or f.get("name")
    return None
```

### Step 4: Pain-Point Map

For every documented buyer pain point in `CLIENT_INTELLIGENCE.json`, map it to:
1. An evaluation criterion it relates to
2. A discriminating strength of ours that resolves it
3. The bid section where we'll demonstrate that resolution

```python
pain_map = []
for pp in client_intel.get("pain_points", []):
    pp_text = pp.get("text") if isinstance(pp, dict) else str(pp)
    pain_map.append({
        "pain_point": pp_text,
        "evaluation_factor": _link_to_factor(pp_text, evaluation),
        "our_response": _company_counter(pp_text, company_profile),
        "demonstrated_in": _suggest_bid_section(pp_text),
    })


def _suggest_bid_section(pp_text: str) -> str:
    pp_l = pp_text.lower()
    if any(k in pp_l for k in ("price", "cost", "budget")):
        return "05_FINANCIAL.md"
    if any(k in pp_l for k in ("integrat", "interop", "api")):
        return "06_INTEGRATION.md"
    if any(k in pp_l for k in ("team", "staff", "support", "communication")):
        return "02_MANAGEMENT.md"
    if any(k in pp_l for k in ("schedule", "delivery", "deadline", "risk")):
        return "02_MANAGEMENT.md or 03_TECHNICAL.md"
    return "03_TECHNICAL.md"
```

### Step 5: Switching-Cost Analysis

```python
# Frame the buyer's perceived switching cost — the obstacles that make incumbency sticky.
# Each entry: cost type, magnitude estimate, our mitigation plan.
switching_costs = []
if has_incumbent:
    switching_costs.extend([
        {
            "cost_type": "Data migration",
            "magnitude": "MEDIUM",
            "buyer_perceives": "Time and risk of migrating historical data from the legacy system.",
            "our_mitigation": "Phase-gated migration plan with parallel-run period (see 03_TECHNICAL.md §Migration). Reference-able past project where we executed equivalent migration.",
        },
        {
            "cost_type": "Re-training",
            "magnitude": "LOW-MEDIUM",
            "buyer_perceives": "Staff productivity dip during cutover.",
            "our_mitigation": "Familiar UX paradigms; embedded change-management resource; documented training plan (see 02_MANAGEMENT.md §Change Management).",
        },
        {
            "cost_type": "Contract overlap",
            "magnitude": "LOW",
            "buyer_perceives": "Cost of running two systems during cutover.",
            "our_mitigation": "Tight cutover plan minimizes overlap to days, not months. Pricing absorbs short overlap (see 05_FINANCIAL.md §Cutover).",
        },
        {
            "cost_type": "Knowledge transfer",
            "magnitude": "LOW",
            "buyer_perceives": "Loss of incumbent's tribal knowledge.",
            "our_mitigation": "Knowledge-capture sprints in Discovery phase; documented hand-off plan; option to retain key incumbent personnel as subs if compliant.",
        },
    ])
```

### Step 6: Win Conditions

Distill the position into 3-5 **win conditions** — measurable strategic plays the bid MUST execute. Every Stage 3 spec and Stage 7 bid section should be traceable back to at least one win condition.

```python
score = (go_nogo or {}).get("total_score", 0)
recommendation = (go_nogo or {}).get("recommendation", "NO_GO")

win_conditions = []

# WC-1: Top eval factor win
top_factor = None
factors_sorted = sorted(
    (evaluation.get("evaluation_factors") or evaluation.get("factors") or []),
    key=lambda f: (f.get("weight") or f.get("points") or 0),
    reverse=True,
)
if factors_sorted:
    top_factor = factors_sorted[0].get("factor_name") or factors_sorted[0].get("name")
if top_factor:
    win_conditions.append({
        "id": "WC-1",
        "condition": f"Win on \"{top_factor}\"",
        "rationale": f"This is the highest-weight evaluation factor. Bid sections covering this factor MUST be the strongest in the response.",
        "owner_sections": ["03_TECHNICAL.md", "04_SOLUTION_*.md"],
    })

# WC-2: Neutralize incumbency / counter ghost weaknesses
if has_incumbent and ghosts:
    win_conditions.append({
        "id": "WC-2",
        "condition": f"Neutralize incumbency — surface every ghost weakness through our discriminating strengths",
        "rationale": "Incumbent loss requires giving the evaluator clear, evidence-backed reasons to switch.",
        "owner_sections": ["01_SUBMITTAL.md", "02_MANAGEMENT.md", "03_TECHNICAL.md"],
    })

# WC-3: Past performance proximity
win_conditions.append({
    "id": "WC-3",
    "condition": "Saturate references with proximity to this buyer's domain + scale",
    "rationale": "Evaluator confidence in delivery is highest when past-performance is near-identical in domain, scale, and contract type. Use Past_Projects.md auto-selection.",
    "owner_sections": ["02_MANAGEMENT.md (3-5 case studies)", "03_TECHNICAL.md (Proven Capability callouts)"],
})

# WC-4: Risk discipline
win_conditions.append({
    "id": "WC-4",
    "condition": "Visible, mitigated risk register — under-promise on risk, over-deliver on mitigation",
    "rationale": "Evaluators reward bidders who name HIGH risks WITH credible mitigations more than bidders who claim no risk.",
    "owner_sections": ["04_RISK_REGISTER.md", "REQUIREMENT_RISKS.md"],
})

# WC-5: Pricing honesty
win_conditions.append({
    "id": "WC-5",
    "condition": "Price to win without lowballing — match the buyer's stated budget envelope; justify deltas",
    "rationale": "Buyers detect lowballing and discount it. Match the budget signals; over-justify any premium with TCO evidence.",
    "owner_sections": ["05_FINANCIAL.md"],
})
```

### Step 7: Write Outputs

```python
position_obj = {
    "generated_at": datetime.now().isoformat(),
    "buyer": {
        "name": (client_intel.get("buyer_name") or
                 client_intel.get("organization") or
                 domain_context.get("buyer", "(unknown)")),
        "type": domain_context.get("buyer_type"),
        "contract_years": domain_context.get("contract_years"),
    },
    "incumbent": incumbent if has_incumbent else None,
    "ghost_strategy": ghosts,
    "pain_map": pain_map,
    "switching_costs": switching_costs,
    "win_conditions": win_conditions,
    "go_nogo_input": {
        "total_score": score,
        "recommendation": recommendation,
    },
}
write_json(f"{folder}/shared/bid/COMPETITIVE_POSITION.json", position_obj)

# Human-readable
lines = []
lines.append(f"# Competitive Position\n")
lines.append(f"**Generated:** {datetime.now().strftime('%Y-%m-%d %H:%M')}\n\n")
lines.append(f"**Buyer:** {position_obj['buyer']['name']}\n")
lines.append(f"**Contract length:** {position_obj['buyer'].get('contract_years', 'unknown')} years\n")
lines.append(f"**Go/No-Go recommendation:** {recommendation} ({score}/100)\n\n---\n\n")

if has_incumbent:
    lines.append(f"## Incumbent: {incumbent['name']}\n\n")
    lines.append(f"**Contract term:** {incumbent['contract_term']}\n\n")
    lines.append("**Documented pain points:**\n")
    for pp in incumbent["known_issues"]:
        pp_text = pp.get("text") if isinstance(pp, dict) else str(pp)
        lines.append(f"- {pp_text}\n")
    lines.append("\n")
else:
    lines.append("## No documented incumbent\n\nGreenfield positioning — focus on aspirational win conditions and reference proximity.\n\n")

lines.append("## Ghost Strategy\n\nGhost statements are positioning AGAINST competitor weaknesses WITHOUT naming them.\n\n")
for g in ghosts:
    lines.append(f"### {g['ghost_phrasing']}\n")
    lines.append(f"**Underlying competitor weakness (internal only):** {g['incumbent_weakness']}\n\n")
    lines.append(f"**Our counter:** {g['our_counter']}\n\n")
    if g.get("evaluation_factor_link"):
        lines.append(f"**Links to evaluation factor:** {g['evaluation_factor_link']}\n\n")
    lines.append("---\n\n")

lines.append("## Pain-Point Map\n\n")
lines.append("| Pain point | Evaluation factor | Our response | Demonstrated in |\n")
lines.append("|------------|-------------------|--------------|-----------------|\n")
for pm in pain_map:
    lines.append(f"| {pm['pain_point'][:80]} | {pm.get('evaluation_factor') or '—'} | {pm['our_response'][:120]} | {pm['demonstrated_in']} |\n")
lines.append("\n")

if switching_costs:
    lines.append("## Switching-Cost Analysis\n\n")
    lines.append("| Cost type | Magnitude | Buyer perceives | Our mitigation |\n")
    lines.append("|-----------|-----------|-----------------|----------------|\n")
    for sc in switching_costs:
        lines.append(f"| {sc['cost_type']} | {sc['magnitude']} | {sc['buyer_perceives']} | {sc['our_mitigation']} |\n")
    lines.append("\n")

lines.append("## Win Conditions\n\n")
for wc in win_conditions:
    lines.append(f"### {wc['id']}: {wc['condition']}\n\n")
    lines.append(f"**Rationale:** {wc['rationale']}\n\n")
    lines.append(f"**Owner sections:** {', '.join(wc['owner_sections'])}\n\n---\n\n")

write_file(f"{folder}/outputs/COMPETITIVE_POSITION.md", "".join(lines))
log(f"COMPETITIVE_POSITION written: {len(ghosts)} ghosts, {len(pain_map)} pain points, {len(win_conditions)} win conditions")
```

### Step 8: Report

```
🎯 Competitive Position Authored
================================
Incumbent:        {incumbent name or 'None'}
Ghost statements: {len(ghosts)}
Pain points:      {len(pain_map)}
Switching costs:  {len(switching_costs)}
Win conditions:   {len(win_conditions)}

Outputs:
  - outputs/COMPETITIVE_POSITION.md
  - shared/bid/COMPETITIVE_POSITION.json (consumed by phase3a–3g + phase8.0)
```

## Quality Checklist

- [ ] `outputs/COMPETITIVE_POSITION.md` >= 3 KB
- [ ] `shared/bid/COMPETITIVE_POSITION.json` written
- [ ] If incumbent known, ≥1 ghost statement per documented incumbent pain point
- [ ] Pain-point map links each pain to an evaluation factor + bid section
- [ ] Switching-cost analysis present when incumbent exists
- [ ] 3-5 named win conditions, each with owner sections
- [ ] No proper-noun naming of competitors in the GHOST sections (legal-risk gate)
