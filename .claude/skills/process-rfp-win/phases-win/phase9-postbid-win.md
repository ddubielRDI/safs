---
name: phase9-postbid-win
expert-role: Bid Performance Analyst
domain-expertise: Win/loss analysis, competitive intelligence, proposal improvement
skill: capture-strategist
sub-skill: bid-decision
---

# Phase 9: Post-Bid Learning Loop

## Purpose

Log the outcome of a completed bid and generate a lessons-learned report. When enough historical data exists (3+ completed outcomes), analyze patterns to identify winning strategies and recurring weaknesses.

This phase can be invoked:
- **Standalone** — after a bid decision is received, independent of the main pipeline
- **As part of the pipeline** — as an optional post-pipeline step

**Consumer:** Phase 8.0 (Strategic Positioning) reads `bid-outcomes.json` in Step 0 and surfaces `debrief_insights` (high/low impact themes, swing factors) to inform positioning strategy when 3+ outcomes contain `structured_debrief` data.

## Inputs

- `{folder}/shared/bid-context-bundle.json` — Aggregated bid context (themes, domain, eval criteria)
- `{folder}/shared/bid/POSITIONING_OUTPUT.json` — Strategic positioning and win themes
- `{folder}/shared/validation/sva7-gold-team.json` — Gold team review score
- `{folder}/shared/WIN_SCORECARD.json` — Predicted win score
- `config-win/bid-outcomes.json` — Historical outcomes (append target)

## Required Outputs

- Updated `config-win/bid-outcomes.json` — New outcome entry appended
- `{folder}/outputs/BID_OUTCOME_REPORT.md` — Lessons learned report (min 2KB)

## Tagging Taxonomy

Use these predefined values for consistency across all outcome entries:

### Domain Values
| Value | Description |
|-------|-------------|
| `government-state` | State government agencies |
| `government-federal` | Federal government agencies |
| `government-local` | County, city, municipal governments |
| `commercial` | Private sector companies |
| `education` | K-12, higher education, education agencies |
| `healthcare` | Hospitals, health agencies, insurance |
| `nonprofit` | Non-profit organizations |

### RFP Type Values
| Value | Description |
|-------|-------------|
| `technology` | Software development, IT infrastructure, systems |
| `consulting` | Advisory, strategy, management consulting |
| `construction` | Construction management, facilities |
| `professional-services` | Staff augmentation, managed services |
| `staffing` | Temporary staffing, recruitment |
| `mixed` | Combination of multiple types |

## Instructions

### Step 1: Load Current Bid Context

```python
import os, json
from datetime import datetime

skill_dir = "/home/ddubiel/repos/safs/.claude/skills/process-rfp-win"

# Load bid context from this RFP
context_bundle = read_json_safe(f"{folder}/shared/bid-context-bundle.json")
positioning = read_json_safe(f"{folder}/shared/bid/POSITIONING_OUTPUT.json")
sva7_report = read_json_safe(f"{folder}/shared/validation/sva7-gold-team.json")
win_scorecard = read_json_safe(f"{folder}/shared/WIN_SCORECARD.json")

# Extract key data from context
rfp_name = ""
if context_bundle:
    rfp_name = context_bundle.get("rfp_name", context_bundle.get("project_name", "Unknown RFP"))

domain = ""
if context_bundle:
    domain_ctx = context_bundle.get("domain_context", {})
    domain = domain_ctx.get("selected_domain", "unknown")

themes_used = []
if positioning:
    themes_used = positioning.get("win_themes", [])

predicted_score = None
if win_scorecard:
    predicted_score = win_scorecard.get("total_score", win_scorecard.get("win_probability", None))

sva7_score = None
if sva7_report:
    sva7_score = sva7_report.get("summary", {}).get("overall_score", None)

pipeline_mode = "full"
if context_bundle:
    pipeline_mode = context_bundle.get("pipeline_mode", "full")

log(f"Bid context loaded: {rfp_name}")
log(f"  Domain: {domain}")
log(f"  Themes: {', '.join(themes_used)}")
log(f"  Predicted score: {predicted_score}")
log(f"  SVA-7 score: {sva7_score}")
```

### Step 2: Collect Outcome Data from User

**[USER INPUT REQUIRED]** — Prompt the user for the following fields. Use the tagging taxonomy above for domain and rfp_type values.

```python
# Present current context and ask for outcome data
log("""
========================================
POST-BID OUTCOME COLLECTION
========================================

Please provide the following information about the bid outcome:

[USER INPUT REQUIRED: bid_id]
  Example: RFP-2026-001
  A unique identifier for this bid

[USER INPUT REQUIRED: outcome]
  One of: win, loss, no-decision, pending
  The final result of the bid

[USER INPUT REQUIRED: score_received]
  Numeric score from evaluators (or null if not disclosed)

[USER INPUT REQUIRED: evaluator_feedback]
  Direct quotes or summary of evaluator feedback
  What did they say about our proposal?

[USER INPUT REQUIRED: strengths_cited]
  List of strengths evaluators highlighted
  Example: ["Strong technical approach", "Relevant experience"]

[USER INPUT REQUIRED: weaknesses_cited]
  List of weaknesses evaluators flagged
  Example: ["Pricing above average", "Limited local presence"]

[USER INPUT REQUIRED: competitive_position]
  Number of bidders and our rank if known
  Example: "3 bidders, ranked 1st" or "5 bidders, rank unknown"

[USER INPUT REQUIRED: lessons_learned]
  Free text: What should we do differently next time?

[USER INPUT REQUIRED: decision_announced_date]
  Date the award decision was announced (e.g., 2026-03-15)
  Leave blank if not yet announced

[USER INPUT REQUIRED: swing_factors]
  What were the 2-3 most important factors in the outcome?
  Example: "pricing was 15% below competition", "incumbent relationship"

[USER INPUT REQUIRED: winner_info] (if loss)
  Who won the contract and what were their strengths?
  Example: "Acme Corp — strong local presence, lower pricing"

[USER INPUT REQUIRED: evaluator_quotes]
  Any specific evaluator quotes or written feedback about our proposal?
  Example: "Technical approach was thorough but pricing raised concerns"
""")

# Wait for user response and build outcome entry
# The executing agent should pause and collect user responses
```

### Step 3: Build Outcome Entry

```python
def build_structured_debrief(user_input, context):
    """Build structured debrief data from user input and bid context.

    Captures decision timeline, theme performance, swing factors,
    and competitive debrief data for pattern analysis.
    Optional — returns empty dict if no debrief data available.
    """
    debrief = {}

    # Decision timeline
    decision_date = user_input.get("decision_announced_date", "")
    submit_date = context.get("date_submitted", datetime.now().strftime("%Y-%m-%d"))
    days_to_decision = None
    if decision_date and submit_date:
        try:
            from datetime import datetime as dt
            d1 = dt.strptime(submit_date, "%Y-%m-%d")
            d2 = dt.strptime(decision_date, "%Y-%m-%d")
            days_to_decision = (d2 - d1).days
        except (ValueError, TypeError):
            pass

    debrief["decision_timeline"] = {
        "date_submitted": submit_date,
        "decision_announced": decision_date,
        "days_to_decision": days_to_decision
    }

    # Standard debrief questions (Lohfeld/APMP best practice)
    debrief["debrief_questions"] = [
        "Were there specific weaknesses that affected our overall score?",
        "How did our technical approach align with evaluation criteria?",
        "Did our pricing raise cost realism or reasonableness concerns?",
        "Were there requirements we failed to address adequately?",
        "Were there areas of our proposal that lacked clarity?",
        "What specific considerations determined the selection?"
    ]
    debrief["debrief_responses"] = {}  # User fills post-debrief meeting

    # Theme performance assessment
    theme_performance = []
    themes_used = context.get("themes_used", [])
    strengths = user_input.get("strengths_cited", [])
    weaknesses = user_input.get("weaknesses_cited", [])
    evaluator_quotes = user_input.get("evaluator_quotes", "")

    for theme in themes_used:
        theme_lower = theme.lower()
        # Determine impact based on whether theme appears in strengths vs weaknesses
        in_strengths = any(theme_lower in s.lower() for s in strengths)
        in_weaknesses = any(theme_lower in w.lower() for w in weaknesses)

        if in_strengths and not in_weaknesses:
            impact = "positive"
        elif in_weaknesses and not in_strengths:
            impact = "negative"
        else:
            impact = "neutral"

        theme_performance.append({
            "theme": theme,
            "evaluator_impact": impact,
            "evidence_from_feedback": (
                next((s for s in strengths if theme_lower in s.lower()), "") or
                next((w for w in weaknesses if theme_lower in w.lower()), "") or
                "No specific feedback on this theme"
            )
        })
    debrief["theme_performance"] = theme_performance

    # Swing factors
    swing_factors = []
    swing_input = user_input.get("swing_factors", "")
    if swing_input:
        # Parse user's swing factor input into structured entries
        factors = [f.strip() for f in swing_input.split(",") if f.strip()]
        for factor in factors:
            outcome = user_input.get("outcome", "")
            swing_factors.append({
                "factor": factor,
                "direction": "for_us" if outcome == "win" else "against_us",
                "addressable": True,  # Default — user can override in debrief
                "lesson": ""  # Populated during debrief review
            })
    debrief["swing_factors"] = swing_factors

    # Competitive debrief
    winner_info = user_input.get("winner_info", "")
    competitive_debrief = {
        "winner": "",
        "winner_strengths": [],
        "our_relative_position": "equivalent",
        "price_competitiveness": "at"
    }
    if winner_info and user_input.get("outcome") == "loss":
        # Parse winner info: "Company — strengths"
        parts = winner_info.split("—") if "—" in winner_info else winner_info.split("-")
        if len(parts) >= 1:
            competitive_debrief["winner"] = parts[0].strip()
        if len(parts) >= 2:
            competitive_debrief["winner_strengths"] = [s.strip() for s in parts[1].split(",")]
        competitive_debrief["our_relative_position"] = "weaker"
    elif user_input.get("outcome") == "win":
        competitive_debrief["our_relative_position"] = "stronger"
    debrief["competitive_debrief"] = competitive_debrief

    # Evaluator quotes
    if evaluator_quotes:
        debrief["evaluator_quotes"] = evaluator_quotes

    return debrief


def build_outcome_entry(user_input, context):
    """Build a structured outcome entry from user input and bid context."""

    entry = {
        "bid_id": user_input["bid_id"],
        "rfp_name": context["rfp_name"],
        "domain": context["domain"],
        "rfp_type": user_input.get("rfp_type", "technology"),
        "date_submitted": datetime.now().strftime("%Y-%m-%d"),
        "outcome": user_input["outcome"],
        "score_received": user_input.get("score_received"),
        "our_score_predicted": context["predicted_score"],
        "themes_used": context["themes_used"],
        "strengths_cited": user_input.get("strengths_cited", []),
        "weaknesses_cited": user_input.get("weaknesses_cited", []),
        "evaluator_feedback": user_input.get("evaluator_feedback", ""),
        "competitive_position": user_input.get("competitive_position", ""),
        "lessons_learned": user_input.get("lessons_learned", ""),
        "pipeline_mode": context["pipeline_mode"],
        "sva7_score": context["sva7_score"],
        "structured_debrief": build_structured_debrief(user_input, context)
    }

    return entry

outcome_entry = build_outcome_entry(user_input, {
    "rfp_name": rfp_name,
    "domain": domain,
    "predicted_score": predicted_score,
    "themes_used": themes_used,
    "pipeline_mode": pipeline_mode,
    "sva7_score": sva7_score
})
```

### Step 4: Append to bid-outcomes.json

```python
outcomes_path = f"{skill_dir}/config-win/bid-outcomes.json"

if os.path.exists(outcomes_path):
    outcomes_data = read_json(outcomes_path)
else:
    outcomes_data = {"version": "1.0", "outcomes": []}

# Check for duplicate bid_id
existing_ids = [o.get("bid_id") for o in outcomes_data.get("outcomes", [])]
if outcome_entry["bid_id"] in existing_ids:
    log(f"WARNING: bid_id '{outcome_entry['bid_id']}' already exists. Updating existing entry.")
    outcomes_data["outcomes"] = [
        o for o in outcomes_data["outcomes"]
        if o.get("bid_id") != outcome_entry["bid_id"]
    ]

outcomes_data["outcomes"].append(outcome_entry)
outcomes_data["last_updated"] = datetime.now().isoformat()

write_json(outcomes_path, outcomes_data)
log(f"Outcome logged: {outcome_entry['bid_id']} ({outcome_entry['outcome']})")
```

### Step 5: Analyze Historical Patterns (if 3+ outcomes)

```python
def analyze_patterns(outcomes):
    """Analyze bid outcome patterns when sufficient data exists."""
    completed = [o for o in outcomes if o.get("outcome") in ("win", "loss")]

    if len(completed) < 3:
        return None

    analysis = {
        "total_completed": len(completed),
        "wins": sum(1 for o in completed if o["outcome"] == "win"),
        "losses": sum(1 for o in completed if o["outcome"] == "loss"),
        "overall_win_rate": 0,
        "domain_performance": {},
        "theme_effectiveness": {},
        "recurring_weaknesses": [],
        "prediction_accuracy": [],
        "recommendations": []
    }

    analysis["overall_win_rate"] = analysis["wins"] / analysis["total_completed"]

    # --- Win rate by domain ---
    for o in completed:
        d = o.get("domain", "unknown")
        if d not in analysis["domain_performance"]:
            analysis["domain_performance"][d] = {"wins": 0, "losses": 0, "total": 0}
        analysis["domain_performance"][d]["total"] += 1
        if o["outcome"] == "win":
            analysis["domain_performance"][d]["wins"] += 1
        else:
            analysis["domain_performance"][d]["losses"] += 1

    for d in analysis["domain_performance"]:
        stats = analysis["domain_performance"][d]
        stats["win_rate"] = stats["wins"] / stats["total"] if stats["total"] > 0 else 0

    # --- Theme effectiveness ---
    theme_wins = {}
    theme_losses = {}
    for o in completed:
        for theme in o.get("themes_used", []):
            if o["outcome"] == "win":
                theme_wins[theme] = theme_wins.get(theme, 0) + 1
            else:
                theme_losses[theme] = theme_losses.get(theme, 0) + 1

    all_themes = set(list(theme_wins.keys()) + list(theme_losses.keys()))
    for theme in all_themes:
        w = theme_wins.get(theme, 0)
        l = theme_losses.get(theme, 0)
        total = w + l
        analysis["theme_effectiveness"][theme] = {
            "wins": w,
            "losses": l,
            "total": total,
            "win_rate": w / total if total > 0 else 0
        }

    # --- Recurring weaknesses ---
    weakness_counts = {}
    for o in completed:
        if o["outcome"] == "loss":
            for w in o.get("weaknesses_cited", []):
                w_lower = w.lower().strip()
                weakness_counts[w_lower] = weakness_counts.get(w_lower, 0) + 1

    analysis["recurring_weaknesses"] = sorted(
        [{"weakness": w, "occurrences": c} for w, c in weakness_counts.items()],
        key=lambda x: x["occurrences"],
        reverse=True
    )

    # --- Prediction accuracy (predicted vs actual) ---
    for o in completed:
        predicted = o.get("our_score_predicted")
        actual = o.get("score_received")
        if predicted is not None and actual is not None:
            analysis["prediction_accuracy"].append({
                "bid_id": o["bid_id"],
                "predicted": predicted,
                "actual": actual,
                "delta": actual - predicted
            })

    # --- Generate recommendations ---
    if analysis["overall_win_rate"] < 0.5:
        analysis["recommendations"].append(
            "Win rate below 50%. Review bid/no-bid criteria to pursue higher-probability opportunities."
        )

    # Recommend effective themes
    effective_themes = [
        t for t, stats in analysis["theme_effectiveness"].items()
        if stats["win_rate"] >= 0.6 and stats["total"] >= 2
    ]
    if effective_themes:
        analysis["recommendations"].append(
            f"High-performing themes (60%+ win rate): {', '.join(effective_themes)}. Prioritize these in future bids."
        )

    # Flag recurring weaknesses
    frequent_weaknesses = [
        w["weakness"] for w in analysis["recurring_weaknesses"]
        if w["occurrences"] >= 2
    ]
    if frequent_weaknesses:
        analysis["recommendations"].append(
            f"Recurring weaknesses across losses: {'; '.join(frequent_weaknesses)}. Develop mitigation strategies."
        )

    # Best-performing domains
    best_domains = [
        d for d, stats in analysis["domain_performance"].items()
        if stats["win_rate"] >= 0.6 and stats["total"] >= 2
    ]
    if best_domains:
        analysis["recommendations"].append(
            f"Strongest domains: {', '.join(best_domains)}. Consider focusing pursuit efforts here."
        )

    # --- Debrief insights (when 3+ outcomes have structured_debrief) ---
    debriefed = [o for o in completed if o.get("structured_debrief")]
    if len(debriefed) >= 3:
        debrief_insights = {
            "high_impact_themes": [],
            "low_impact_themes": [],
            "recurring_swing_factors": [],
            "avg_days_to_decision": 0,
            "competitor_win_rates": {}
        }

        # Identify high/low impact themes across debriefs
        theme_impacts = {}  # theme -> {positive: N, negative: N, neutral: N}
        for o in debriefed:
            for tp in o["structured_debrief"].get("theme_performance", []):
                theme = tp["theme"]
                impact = tp["evaluator_impact"]
                if theme not in theme_impacts:
                    theme_impacts[theme] = {"positive": 0, "negative": 0, "neutral": 0}
                theme_impacts[theme][impact] = theme_impacts[theme].get(impact, 0) + 1

        for theme, impacts in theme_impacts.items():
            total = sum(impacts.values())
            if total >= 2:
                pos_rate = impacts["positive"] / total
                neg_rate = impacts["negative"] / total
                if pos_rate >= 0.70:
                    debrief_insights["high_impact_themes"].append(
                        {"theme": theme, "positive_rate": round(pos_rate, 2), "occurrences": total}
                    )
                if neg_rate >= 0.50:
                    debrief_insights["low_impact_themes"].append(
                        {"theme": theme, "negative_rate": round(neg_rate, 2), "occurrences": total}
                    )

        # Recurring swing factors
        swing_counts = {}
        for o in debriefed:
            for sf in o["structured_debrief"].get("swing_factors", []):
                factor = sf["factor"].lower().strip()
                swing_counts[factor] = swing_counts.get(factor, 0) + 1
        debrief_insights["recurring_swing_factors"] = [
            {"factor": f, "occurrences": c}
            for f, c in sorted(swing_counts.items(), key=lambda x: x[1], reverse=True)
            if c >= 2
        ]

        # Average days to decision
        days_list = [
            o["structured_debrief"]["decision_timeline"]["days_to_decision"]
            for o in debriefed
            if o["structured_debrief"].get("decision_timeline", {}).get("days_to_decision") is not None
        ]
        if days_list:
            debrief_insights["avg_days_to_decision"] = round(sum(days_list) / len(days_list), 1)

        # Competitor win rates (our record vs specific winners)
        for o in debriefed:
            if o.get("outcome") == "loss":
                winner = o["structured_debrief"].get("competitive_debrief", {}).get("winner", "")
                if winner:
                    winner_lower = winner.lower().strip()
                    if winner_lower not in debrief_insights["competitor_win_rates"]:
                        debrief_insights["competitor_win_rates"][winner_lower] = {"losses_to": 0}
                    debrief_insights["competitor_win_rates"][winner_lower]["losses_to"] += 1

        analysis["debrief_insights"] = debrief_insights

    return analysis

# Run analysis
all_outcomes = outcomes_data.get("outcomes", [])
patterns = analyze_patterns(all_outcomes)
```

### Step 6: Generate BID_OUTCOME_REPORT.md

```python
def generate_report(outcome_entry, patterns, all_outcomes):
    """Generate the post-bid outcome report."""

    report = []
    report.append("# Bid Outcome Report")
    report.append("")
    report.append(f"**Generated:** {datetime.now().strftime('%Y-%m-%d %H:%M')}")
    report.append(f"**Bid ID:** {outcome_entry['bid_id']}")
    report.append(f"**RFP:** {outcome_entry['rfp_name']}")
    report.append("")

    # --- This Bid Summary ---
    report.append("## This Bid Summary")
    report.append("")
    report.append(f"| Field | Value |")
    report.append(f"|-------|-------|")
    report.append(f"| Outcome | **{outcome_entry['outcome'].upper()}** |")
    report.append(f"| Domain | {outcome_entry['domain']} |")
    report.append(f"| RFP Type | {outcome_entry['rfp_type']} |")
    report.append(f"| Score Received | {outcome_entry.get('score_received', 'Not disclosed')} |")
    report.append(f"| Predicted Score | {outcome_entry.get('our_score_predicted', 'N/A')} |")
    report.append(f"| SVA-7 Score | {outcome_entry.get('sva7_score', 'N/A')} |")
    report.append(f"| Pipeline Mode | {outcome_entry['pipeline_mode']} |")
    report.append(f"| Competitive Position | {outcome_entry.get('competitive_position', 'Unknown')} |")
    report.append("")

    # Themes used
    report.append("### Win Themes Used")
    report.append("")
    for theme in outcome_entry.get("themes_used", []):
        report.append(f"- {theme}")
    report.append("")

    # Evaluator feedback
    report.append("### Evaluator Feedback")
    report.append("")
    if outcome_entry.get("evaluator_feedback"):
        report.append(f"> {outcome_entry['evaluator_feedback']}")
    else:
        report.append("*No evaluator feedback recorded.*")
    report.append("")

    # Strengths
    report.append("### Strengths Cited")
    report.append("")
    for s in outcome_entry.get("strengths_cited", []):
        report.append(f"- {s}")
    if not outcome_entry.get("strengths_cited"):
        report.append("*None recorded.*")
    report.append("")

    # Weaknesses
    report.append("### Weaknesses Cited")
    report.append("")
    for w in outcome_entry.get("weaknesses_cited", []):
        report.append(f"- {w}")
    if not outcome_entry.get("weaknesses_cited"):
        report.append("*None recorded.*")
    report.append("")

    # Lessons learned
    report.append("### Lessons Learned")
    report.append("")
    if outcome_entry.get("lessons_learned"):
        report.append(outcome_entry["lessons_learned"])
    else:
        report.append("*No lessons recorded.*")
    report.append("")

    # --- Historical Patterns (if available) ---
    report.append("---")
    report.append("")
    report.append("## Historical Patterns")
    report.append("")

    completed_count = sum(1 for o in all_outcomes if o.get("outcome") in ("win", "loss"))

    if patterns is None:
        report.append(f"*Insufficient data for pattern analysis. {completed_count} completed outcome(s) recorded (need 3+).*")
        report.append("")
        report.append("Continue logging bid outcomes after each decision to build the pattern database.")
    else:
        report.append(f"Based on **{patterns['total_completed']}** completed bids ({patterns['wins']} wins, {patterns['losses']} losses).")
        report.append("")

        # Overall win rate
        report.append(f"**Overall Win Rate:** {patterns['overall_win_rate']:.0%}")
        report.append("")

        # Domain performance table
        report.append("### Performance by Domain")
        report.append("")
        report.append("| Domain | Wins | Losses | Win Rate |")
        report.append("|--------|------|--------|----------|")
        for domain, stats in sorted(patterns["domain_performance"].items(),
                                     key=lambda x: x[1]["win_rate"], reverse=True):
            report.append(f"| {domain} | {stats['wins']} | {stats['losses']} | {stats['win_rate']:.0%} |")
        report.append("")

        # Theme effectiveness table
        report.append("### Theme Effectiveness")
        report.append("")
        report.append("| Theme | In Wins | In Losses | Win Rate |")
        report.append("|-------|---------|-----------|----------|")
        for theme, stats in sorted(patterns["theme_effectiveness"].items(),
                                    key=lambda x: x[1]["win_rate"], reverse=True):
            report.append(f"| {theme} | {stats['wins']} | {stats['losses']} | {stats['win_rate']:.0%} |")
        report.append("")

        # Recurring weaknesses
        if patterns["recurring_weaknesses"]:
            report.append("### Recurring Weaknesses")
            report.append("")
            report.append("| Weakness | Occurrences |")
            report.append("|----------|-------------|")
            for w in patterns["recurring_weaknesses"][:10]:
                report.append(f"| {w['weakness']} | {w['occurrences']} |")
            report.append("")

        # Prediction accuracy
        if patterns["prediction_accuracy"]:
            report.append("### Prediction Accuracy")
            report.append("")
            report.append("| Bid ID | Predicted | Actual | Delta |")
            report.append("|--------|----------|--------|-------|")
            for pa in patterns["prediction_accuracy"]:
                delta_sign = "+" if pa["delta"] >= 0 else ""
                report.append(f"| {pa['bid_id']} | {pa['predicted']} | {pa['actual']} | {delta_sign}{pa['delta']} |")
            report.append("")

        # Recommendations
        if patterns["recommendations"]:
            report.append("### Recommendations for Next Bid")
            report.append("")
            for i, rec in enumerate(patterns["recommendations"], 1):
                report.append(f"{i}. {rec}")
            report.append("")

    # --- Structured Debrief Sections (if this bid has debrief data) ---
    debrief = outcome_entry.get("structured_debrief", {})
    if debrief:
        report.append("---")
        report.append("")
        report.append("## Structured Debrief")
        report.append("")

        # Decision Timeline
        timeline = debrief.get("decision_timeline", {})
        if timeline.get("decision_announced") or timeline.get("days_to_decision"):
            report.append("### Decision Timeline")
            report.append("")
            report.append(f"| Milestone | Date |")
            report.append(f"|-----------|------|")
            report.append(f"| Submitted | {timeline.get('date_submitted', 'N/A')} |")
            report.append(f"| Decision | {timeline.get('decision_announced', 'N/A')} |")
            if timeline.get("days_to_decision") is not None:
                report.append(f"| Duration | {timeline['days_to_decision']} days |")
            report.append("")

        # Theme Performance Assessment
        theme_perf = debrief.get("theme_performance", [])
        if theme_perf:
            report.append("### Theme Performance Assessment")
            report.append("")
            report.append("| Theme | Impact | Evidence |")
            report.append("|-------|--------|----------|")
            for tp in theme_perf:
                impact_icon = {"positive": "✅", "negative": "❌", "neutral": "➖"}.get(tp["evaluator_impact"], "➖")
                report.append(f"| {tp['theme']} | {impact_icon} {tp['evaluator_impact']} | {tp.get('evidence_from_feedback', '')[:80]} |")
            report.append("")

        # Swing Factors
        swing_factors = debrief.get("swing_factors", [])
        if swing_factors:
            report.append("### Swing Factors")
            report.append("")
            report.append("| Factor | Direction | Addressable |")
            report.append("|--------|-----------|-------------|")
            for sf in swing_factors:
                direction_icon = "✅" if sf["direction"] == "for_us" else "❌"
                report.append(f"| {sf['factor']} | {direction_icon} {sf['direction']} | {'Yes' if sf.get('addressable') else 'No'} |")
            report.append("")

        # Competitive Position
        comp = debrief.get("competitive_debrief", {})
        if comp.get("winner") or comp.get("our_relative_position") != "equivalent":
            report.append("### Competitive Position")
            report.append("")
            if comp.get("winner"):
                report.append(f"**Winner:** {comp['winner']}")
                if comp.get("winner_strengths"):
                    report.append(f"**Winner Strengths:** {', '.join(comp['winner_strengths'])}")
            report.append(f"**Our Position:** {comp.get('our_relative_position', 'Unknown')}")
            report.append(f"**Price:** {comp.get('price_competitiveness', 'Unknown')}")
            report.append("")

    # --- Debrief Synthesis (if 3+ outcomes with debrief data) ---
    if patterns and patterns.get("debrief_insights"):
        insights = patterns["debrief_insights"]
        report.append("---")
        report.append("")
        report.append("## Debrief Synthesis (Cross-Bid Patterns)")
        report.append("")

        if insights.get("high_impact_themes"):
            report.append("### High-Impact Themes (>=70% positive)")
            report.append("")
            for t in insights["high_impact_themes"]:
                report.append(f"- **{t['theme']}** — {t['positive_rate']:.0%} positive ({t['occurrences']} bids)")
            report.append("")

        if insights.get("low_impact_themes"):
            report.append("### Low-Impact Themes (>=50% negative)")
            report.append("")
            for t in insights["low_impact_themes"]:
                report.append(f"- **{t['theme']}** — {t['negative_rate']:.0%} negative ({t['occurrences']} bids)")
            report.append("")

        if insights.get("recurring_swing_factors"):
            report.append("### Recurring Swing Factors")
            report.append("")
            for sf in insights["recurring_swing_factors"]:
                report.append(f"- **{sf['factor']}** — appeared in {sf['occurrences']} bids")
            report.append("")

        if insights.get("avg_days_to_decision"):
            report.append(f"**Average Decision Timeline:** {insights['avg_days_to_decision']} days")
            report.append("")

        if insights.get("competitor_win_rates"):
            report.append("### Competitor Trends")
            report.append("")
            report.append("| Competitor | Losses To |")
            report.append("|-----------|-----------|")
            for comp_name, stats in insights["competitor_win_rates"].items():
                report.append(f"| {comp_name} | {stats['losses_to']} |")
            report.append("")

    return "\n".join(report)

report_content = generate_report(outcome_entry, patterns, all_outcomes)
write_file(f"{folder}/outputs/BID_OUTCOME_REPORT.md", report_content)
log(f"Outcome report generated: {folder}/outputs/BID_OUTCOME_REPORT.md")
```

### Step 7: Report Results

```python
log(f"""
========================================
POST-BID LEARNING LOOP COMPLETE
========================================

Bid: {outcome_entry['bid_id']} — {outcome_entry['rfp_name']}
Outcome: {outcome_entry['outcome'].upper()}
Score: {outcome_entry.get('score_received', 'Not disclosed')} (predicted: {outcome_entry.get('our_score_predicted', 'N/A')})

Historical data:
  Total outcomes logged: {len(all_outcomes)}
  Completed (win/loss): {sum(1 for o in all_outcomes if o.get('outcome') in ('win', 'loss'))}
  Pattern analysis: {'Available' if patterns else 'Need 3+ completed outcomes'}

Outputs:
  Updated: config-win/bid-outcomes.json
  Created: {folder}/outputs/BID_OUTCOME_REPORT.md
""")
```

## Quality Checklist

- [ ] User prompted for all required outcome data with [USER INPUT REQUIRED] markers
- [ ] Outcome entry uses valid taxonomy values (domain, rfp_type, outcome)
- [ ] Bid context loaded from existing pipeline artifacts
- [ ] Duplicate bid_id check prevents double-counting
- [ ] Outcome appended to `config-win/bid-outcomes.json`
- [ ] Pattern analysis runs when 3+ completed outcomes exist
- [ ] Analysis covers: domain performance, theme effectiveness, recurring weaknesses, prediction accuracy
- [ ] `BID_OUTCOME_REPORT.md` generated with this bid summary and historical patterns
- [ ] Recommendations generated based on pattern data
- [ ] Report is at least 2KB
- [ ] `structured_debrief` populated in outcome entry (decision timeline, theme performance, swing factors)
- [ ] Debrief questions include all 6 standard industry questions
- [ ] Theme performance assessment maps each win theme to evaluator impact
- [ ] Swing factors captured with direction and addressability
- [ ] Competitive debrief captures winner info (if loss)
- [ ] Debrief insights generated when 3+ outcomes have structured_debrief data
- [ ] Backward compatible: existing outcomes without structured_debrief still load/process normally
