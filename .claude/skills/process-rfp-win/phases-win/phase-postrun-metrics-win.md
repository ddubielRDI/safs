# Phase: Post-Run Metrics Report

---
name: Post-Run Metrics Report
description: Generate human-readable pipeline metrics report from aggregated run data
expert_role: Pipeline Analytics Engineer
domain_expertise: Pipeline telemetry, performance analysis, trend detection, optimization recommendations
---

## Expert Role

You are a **Pipeline Analytics Engineer** specializing in CI/CD pipeline telemetry, performance analysis, and operational optimization. You analyze execution data to identify bottlenecks, failure patterns, and improvement opportunities.

## Purpose

Generate a comprehensive, human-readable metrics report from the persistent `pipeline-metrics.json` file. This report provides insights into pipeline health, performance trends, and optimization opportunities across all recorded runs.

## Inputs

| Input | Path | Required |
|-------|------|----------|
| Pipeline Metrics | `config-win/pipeline-metrics.json` | YES |

**Relative to skill directory:** `/home/ddubiel/repos/safs/.claude/skills/process-rfp-win/`

## Output

| Output | Path |
|--------|------|
| Metrics Report | `{folder}/outputs/PIPELINE_METRICS.md` |

If no `{folder}` is provided, output to the skill directory: `config-win/PIPELINE_METRICS.md`

## Instructions

### Step 1: Load Metrics Data

```python
metrics_file = "/home/ddubiel/repos/safs/.claude/skills/process-rfp-win/config-win/pipeline-metrics.json"
metrics = read_json(metrics_file)

if not metrics.get("runs") or len(metrics["runs"]) == 0:
    log("No pipeline runs recorded yet. Nothing to report.")
    write_file(output_path, "# Pipeline Metrics Report\n\nNo pipeline runs recorded yet.\n")
    return
```

### Step 2: Generate Report Sections

Generate the following report in Markdown format:

#### Section 1: Run History Table

```markdown
## Run History

| # | Date | Mode | Duration (min) | Success | Phases | Retries |
|---|------|------|----------------|---------|--------|---------|
| 1 | 2026-02-21 | full | 28.3 | YES | 46 | 0 |
| 2 | 2026-02-22 | sprint | 12.1 | YES | 14 | 2 |
```

- Extract from `metrics.runs[]`
- Format dates as `YYYY-MM-DD`
- Show `total_duration_minutes` rounded to 1 decimal
- Show `success` as YES/NO
- Show `phase_count` and `total_retries`

#### Section 2: Phase Timing Analysis

```markdown
## Phase Timing Analysis

### Average Duration per Phase (seconds)

| Phase | Avg Duration | Runs | Trend |
|-------|-------------|------|-------|
| 8.3 (Technical Approach) | 145.2s | 5 | -- |
| 8.4 (Business Solution) | 132.8s | 5 | -- |
| 1 (Document Flattening) | 98.4s | 5 | -- |

### Top 5 Slowest Phases
1. Phase 8.3 (Technical Approach): avg 145.2s
2. Phase 8.4 (Business Solution): avg 132.8s
3. ...
```

- Use `metrics.aggregates.phase_avg_durations`
- Sort by average duration descending
- Show top 5 slowest
- If 5+ runs: calculate trend (improving/degrading/stable) by comparing first-half avg vs second-half avg

#### Section 3: SVA Gate Analysis

```markdown
## SVA Gate Analysis

| SVA Gate | Runs | Pass | Advisory | Block | Pass Rate |
|----------|------|------|----------|-------|-----------|
| SVA-1 | 5 | 4 | 1 | 0 | 80% |
| SVA-7 | 5 | 3 | 2 | 0 | 60% |
```

- Aggregate SVA dispositions from `run.sva_results` across all runs
- Calculate pass rate, advisory rate, block rate per SVA

#### Section 4: Failure Analysis

```markdown
## Failure Analysis

### Phase Failure Rates

| Phase | Failure Rate | Total Runs | Failures |
|-------|-------------|------------|----------|
| 8e (PDF Assembly) | 20% | 5 | 1 |
| 3a (Architecture) | 10% | 5 | 0.5 |

### Common Failure Points
- Phase 8e: PDF library dependency issues (1 occurrence)
- Phase 1: Large document conversion timeouts (1 occurrence)
```

- Use `metrics.aggregates.phase_failure_rates`
- Sort by failure rate descending
- Only show phases with failure_rate > 0
- If no failures: "No phase failures recorded."

#### Section 5: Optimization Recommendations

Generate data-driven recommendations based on the metrics:

```markdown
## Optimization Recommendations

Based on {N} pipeline runs:

1. **Bottleneck: Phase 8.3** - Averaging 145.2s. Consider splitting into sub-phases or reducing context window requirements.
2. **SVA-7 Advisory Rate: 40%** - Gold Team review frequently produces advisories. Review SVA-7 rule thresholds.
3. **Sprint Mode Efficiency** - Sprint runs average 12.1min vs 28.3min for full (57% faster). Consider sprint for routine renewals.
```

Rules for recommendations:
- If any phase averages > 120s: flag as bottleneck
- If any SVA has block rate > 20%: flag as reliability concern
- If sprint vs full time ratio > 50%: note sprint efficiency
- If failure rate > 15% for any phase: flag for investigation
- If total retries / total phases > 5%: flag retry frequency

#### Section 6: Trend Analysis (5+ runs only)

```markdown
## Trend Analysis

Pipeline performance over {N} runs:

- **Total Duration Trend:** Improving (first 3 avg: 30.2min, last 3 avg: 25.1min, -17%)
- **Phase Failure Trend:** Stable (0.5 failures/run avg)
- **SVA Pass Rate Trend:** Improving (first 3: 85%, last 3: 95%)
```

- Only generate if 5+ runs exist
- Compare first half of runs vs second half
- Report: improving (>10% better), degrading (>10% worse), or stable

### Step 3: Assemble and Write Report

```python
report = f"""# Pipeline Metrics Report

**Generated:** {datetime.now().strftime('%Y-%m-%d %H:%M')}
**Pipeline Version:** {metrics.get('version', '1.0')}
**Total Runs Analyzed:** {metrics['aggregates']['total_runs']}
**Full Runs:** {metrics['aggregates']['total_full_runs']} | **Sprint Runs:** {metrics['aggregates']['total_sprint_runs']}
**Average Duration:** {metrics['aggregates']['avg_total_duration_minutes']} minutes

---

{run_history_section}

---

{timing_analysis_section}

---

{sva_analysis_section}

---

{failure_analysis_section}

---

{recommendations_section}

---

{trend_section if len(metrics['runs']) >= 5 else '## Trend Analysis\\n\\nInsufficient data (need 5+ runs for trend analysis).'}
"""

write_file(output_path, report)
log(f"Pipeline metrics report written to {output_path}")
```

## Invocation

This phase is invoked on-demand (not part of the standard pipeline). Users can request it after any pipeline run:

```
"Generate a pipeline metrics report"
"Show me pipeline performance data"
"How is the pipeline performing?"
```

The orchestrator can also invoke it automatically after every N runs (configurable).
