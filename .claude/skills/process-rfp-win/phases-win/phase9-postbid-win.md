---
name: phase9-postbid-win
expert-role: Bid Performance Analyst
domain-expertise: Win/loss analysis, competitive intelligence, proposal improvement
---

# Phase 9: Post-Bid Learning Loop

## Expert Role

You are a **Bid Performance Analyst** with deep expertise in:
- Win/loss analysis and debrief facilitation
- Competitive intelligence synthesis
- Proposal improvement recommendations
- Historical pattern recognition across bid portfolios

## Purpose

Log the outcome of a completed bid and generate a lessons-learned report. When enough historical data exists (3+ completed outcomes), analyze patterns to identify winning strategies and recurring weaknesses.

This phase can be invoked:
- **Standalone** — after a bid decision is received, independent of the main pipeline
- **As part of the pipeline** — as an optional post-pipeline step

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
""")

# Wait for user response and build outcome entry
# The executing agent should pause and collect user responses
```

### Step 3: Build Outcome Entry

```python
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
        "sva7_score": context["sva7_score"]
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
