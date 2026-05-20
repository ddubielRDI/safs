---
name: verifier-phase5-estimation-win
expert-role: Effort Estimation Verifier
purpose: phase-boundary verifier for phase5-estimation-win (file presence, role coverage, sanity ratios, requirement parity, schema fidelity)
created: 2026-05-20
---

# Verifier — Phase 5 Effort Estimation (EFFORT_ESTIMATION.md)

## When this runs

After phase5-estimation-win reports done, BEFORE the Stage 5 SVA (or Phase 6 manifest if no Stage 5 SVA is configured).

## Inputs (read in this order)

1. `{folder}/outputs/EFFORT_ESTIMATION.md` — primary deliverable under verification
2. `{folder}/shared/effort-estimation.json` — structured estimation data (used by Phase 6 Investment Summary)
3. `{folder}/shared/requirements-normalized.json` — requirement count parity check
4. `{folder}/shared/UNIFIED_RTM.json` — Phase 4 RTM requirement count cross-check
5. `{folder}/shared/REQUIREMENT_RISKS.json` — risk-level enum parity (CRITICAL/HIGH/MEDIUM/LOW)
6. `{folder}/shared/domain-context.json` — `estimation.team_size_recommended` override source

## Verification Checks

### Check 1 — File exists, ≥ 8 KB, valid UTF-8, no mojibake

**Criterion:** `outputs/EFFORT_ESTIMATION.md` exists AND `stat --printf %s` >= 8192 bytes AND the file opens cleanly as UTF-8 (no `UnicodeDecodeError`) AND no mojibake sequences present (grep for `Ã¢â‚¬`, `â€"`, `Â `, `�` returns 0 hits).

**Pass:** All four sub-conditions hold.

**Fail:** File absent, size < 8192 bytes, decode error, OR any mojibake sequence found.

**Evidence to cite:** File size in bytes; first mojibake sequence + line number if any.

**Hard-rule reminder:** NEVER claim file is missing without `Glob` verification first.

---

### Check 2 — Total hours and cost computed (non-zero, schema-bound)

**Criterion:** `shared/effort-estimation.json` exists, parses, and `summary.total_hours > 0` AND `summary.ai_assisted_hours > 0` AND `summary.ai_savings_percent > 0`. EFFORT_ESTIMATION.md Executive Summary table cites both "Total Effort (Traditional)" AND "Total Effort (AI-Assisted)" rows with numeric values that match `effort-estimation.json` to ±1 hour.

**Pass:** All values present, positive, and md/json reconcile.

**Fail:** Any field zero, missing, OR md figures disagree with json source.

**Evidence to cite:** `summary.total_hours = N`, `summary.ai_assisted_hours = M`, md Executive Summary row text.

---

### Check 3 — Effort broken down by category AND by role

**Criterion:** EFFORT_ESTIMATION.md contains BOTH:
  (a) "Effort by Category" section with a table of category → hours rows (≥ 5 categories listed), AND
  (b) "Resource Plan" / "Recommended Team Composition" section listing role-level allocation with at minimum the canonical role categories (PM/BA, Dev/Senior Developer, QA Engineer, Tech Lead, plus DevOps and Security if the requirement set warrants them).

**Pass:** Both sections present, category table has ≥ 5 rows, role table covers the mandatory role set.

**Concern:** Category table has 3–4 rows (likely small RFP) — log advisory, don't block.

**Fail:** Either section missing OR fewer than 3 categories OR missing two or more of the mandatory roles (PM, BA, Dev, QA, DevOps, Security — note Tech Lead/Senior Dev can collapse into "Dev").

**Evidence to cite:** List of categories found; list of roles found; list of missing canonical roles.

---

### Check 4 — AI-assistance ratio in sane band (1.3x – 3.0x)

**Criterion:** Compute `ratio = summary.total_hours / summary.ai_assisted_hours`. Must satisfy `1.3 <= ratio <= 3.0`. This bounds the AI savings claim: at 0% savings ratio=1.0 (no value), at >67% savings ratio>3.0 (unrealistic). Sane band ~23%–67% savings translates to ratio 1.3–3.0.

**Pass:** `1.3 <= ratio <= 3.0`.

**Concern:** `1.2 <= ratio < 1.3` OR `3.0 < ratio <= 3.3` — log advisory.

**Fail:** `ratio < 1.2` (claim too modest, AI assistance not meaningful) OR `ratio > 3.3` (claim implausible, will be rejected at evaluator review).

**Evidence to cite:** `total_hours = N`, `ai_assisted_hours = M`, `ratio = N/M = X.XX`, `ai_savings_percent = P%`.

---

### Check 5 — Requirement coverage matches Phase 4 RTM count

**Criterion:** Count of unique `Req ID` values in EFFORT_ESTIMATION.md "Appendix: Full Estimation Table" equals `len(UNIFIED_RTM.entities.requirements)` (preferred) OR `len(requirements-normalized.requirements)` if RTM unavailable. Tolerance: 0 (exact match).

**Pass:** Counts match exactly.

**Concern:** Difference of 1–3 requirements (likely a parser quirk or trailing-row issue) — log with the specific delta.

**Fail:** Difference > 3 requirements — Phase 5 silently dropped requirements OR injected phantoms.

**Evidence to cite:** `EFFORT_ESTIMATION.md Appendix req count = N` vs `UNIFIED_RTM count = M` vs `normalized count = K`. List up to 5 missing `req_id` values.

---

### Check 6 — Risk multiplier reverse-walk applied (rtm_risks-based, not empty)

**Criterion:** Confirm `shared/effort-estimation.json` records risk-level distribution OR the markdown's Appendix shows non-uniform Risk column values (i.e., NOT every row showing the same risk level — that's the symptom of the broken `risks.requirements[]` lookup returning empty). At least 10% of rows must have a non-default ("MEDIUM") risk value if `REQUIREMENT_RISKS.rtm_risks` has > 50 entries.

**Pass:** Risk distribution non-degenerate (≥ 2 distinct risk levels represented, ≥ 10% of rows non-MEDIUM when ≥ 50 rtm_risks exist).

**Fail:** Every Appendix row shows "MEDIUM" (or every row shows the same level) when REQUIREMENT_RISKS has > 50 rtm_risks — confirms the V3-F1 risk_map build is broken.

**Evidence to cite:** Distinct risk-level values found in Appendix; count of non-MEDIUM rows / total; `REQUIREMENT_RISKS.rtm_risks` count.

---

### Check 7 — Team size derived from override or scale tier (not hardcoded sentinel)

**Criterion:** `summary.team_size_recommended` is present AND `summary.team_size_source` is one of: `"domain-context override"`, `"scale-tier: <=2k hrs"`, `"scale-tier: 2k-10k hrs"`, `"scale-tier: 10k-25k hrs"`, `"scale-tier: 25k-50k hrs"`, `"scale-tier: >50k hrs"`. The chosen tier must match `summary.total_hours` per the heuristic in phase5-estimation-win.md Step 4 `derive_team_size`.

**Pass:** Source string present AND matches the tier the total_hours fall into.

**Fail:** Source field missing, OR source is empty/literal "5" or "10" (hardcoded), OR scale-tier source disagrees with total_hours band.

**Evidence to cite:** `team_size_recommended = N`, `team_size_source = "..."`, `total_hours = X` and the expected tier.

---

### Check 8 — No `[:N]` truncation, no row-cap notices, no `_Showing N of M_`

**Criterion:** In EFFORT_ESTIMATION.md, grep returns 0 hits for `_Showing [0-9]+ of [0-9]+_` AND 0 hits for mid-word table-cell truncations (cell ending with `...` mid-word) AND 0 hits for `\[:[0-9]+\]` literal text. The High-Effort table must contain ALL requirements > 40 hours (no `[:15]` or `[:20]` slice survived).

**Pass:** All four patterns produce 0 hits AND high-effort row count matches the actual count from json.

**Fail:** Any pattern found OR high-effort table count < actual count from json.

**Evidence to cite:** First offending line + context, OR `high_effort_md_rows = N` vs `high_effort_json_count = M`.

---

### Check 9 — Cost ladder reconciles (Direct → Overhead → Profit → Total)

**Criterion:** If `summary.cost_estimate` is present in effort-estimation.json AND EFFORT_ESTIMATION.md or downstream EXECUTIVE_SUMMARY.md cites cost figures, verify: `direct_labor_usd ≈ ai_assisted_hours * blended_rate_usd_per_hr` (±$10), AND `with_overhead_usd ≈ direct_labor_usd * 1.15` (±$10), AND `total_cost_usd ≈ with_overhead_usd * 1.10` (±$10).

**Pass:** All three reconciliations within tolerance.

**Concern:** One step off by >$10 but <$100 — likely rounding accumulation.

**Fail:** Any step off by >$100 — cost math wrong; Phase 6 Investment Summary will compound the error.

**Evidence to cite:** Each computed expected value vs the field value, with delta.

---

### Check 10 — JSON output schema fidelity (UTF-8, ensure_ascii=False, structure)

**Criterion:** `shared/effort-estimation.json` parses as JSON, top-level keys include at minimum `summary` and `requirements` (or `estimates`). Re-encoding the parsed dict and string-comparing key paths to original must show no `\u00XX`-escaped non-ASCII (i.e., `ensure_ascii=False` was honored — em dashes / accented chars survive as literal UTF-8). The `summary` dict contains: `total_requirements`, `total_hours`, `ai_assisted_hours`, `ai_savings_hours`, `ai_savings_percent`, `by_category`, `estimated_duration_weeks`, `team_size_recommended`, `team_size_source`.

**Pass:** Parses cleanly, all summary keys present, no `\u00XX` escapes for common chars.

**Fail:** Parse error, any required summary key missing, OR JSON file contains `—` (escaped em dash) — indicates ensure_ascii=True regression.

**Evidence to cite:** Missing key path(s); first escape sequence found if any.

---

## Disposition Logic

- **PASS:** Checks 1, 2, 3, 5, 7, 8, 10 all pass AND Checks 4, 6, 9 meet threshold (not just CONCERN band).
- **CONCERN:** Any of Checks 3, 4, 5, 9 fall in the advisory band. Log + continue to next stage.
- **FAIL:** Any of Checks 1, 2, 6, 7, 8, 10 fail OR Checks 4, 5, 9 fall below their FAIL thresholds.

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase5-estimation-win with the following targeted corrections:

[Check 1] EFFORT_ESTIMATION.md missing/undersize/mojibake: if absent re-run Step 5
  (generate_estimation_md + write_file). Undersize → audit Step 5 string-build for premature
  early-return. Mojibake → enforce encoding='utf-8' + ensure_ascii=False.

[Check 2] Total hours / cost zero OR md disagrees with json: audit Step 4 calculate_summary —
  confirm sum() over r["estimation"]["total_hours"] is computed BEFORE doc generation. Re-run
  Phase 5 from Step 3.

[Check 3] Missing role categories {list}: expand Step 5 Resource Plan template to include all 6
  canonical roles (PM, BA, Dev, QA, DevOps, Security).

[Check 4] AI ratio {ratio} out of band [1.3, 3.0]: if < 1.3 increase AI_ASSISTANCE_RATIOS;
  if > 3.0 reduce per-activity ratios. Re-run Phase 5 from Step 3 after editing constants.

[Check 5] Appendix row count {N} != RTM count {M}, missing {list}: audit Step 3 — `for req in
  all_reqs` must not be filtered or sliced. Confirm Step 5 Appendix loop is unsliced.

[Check 6] All Appendix rows show identical risk level — risk_map empty: re-read Step 1 (V3-F1
  fix). risk_map MUST be built from `risks.get("rtm_risks", [])` with reverse-walk over
  `linked_requirement_ids`. Verify `len(risk_map) > 0` after Step 1.

[Check 7] team_size_source missing/sentinel: re-run Step 4 derive_team_size(domain_context,
  total_hours) and confirm BOTH return values (count + source) are captured into summary.

[Check 8] Truncation / row-cap at {line}: remove [:15], [:20], [:N] slices from Step 5 doc
  loops. Per V4-F6 fix 2026-05-20 — never cap deliverable table rows.

[Check 9] Cost ladder off by ${delta} at {step}: confirm ladder = direct_labor * 1.15 * 1.10
  (NOT 1.25 flat). Recompute persisted values in effort-estimation.json before Phase 6 reads.

[Check 10] JSON schema regression / \u-escapes: ensure json.dump uses ensure_ascii=False AND
  dict contains all 9 required summary keys.

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input:**

```
EFFORT_ESTIMATION.md = 4,200 bytes. summary.total_hours = 56,360.
summary.ai_assisted_hours = 56,360 (ratio = 1.0, no savings applied).
Appendix has 1,840 rows but UNIFIED_RTM has 2,167 requirements (327 dropped).
Every Appendix row shows "MEDIUM" risk; REQUIREMENT_RISKS.rtm_risks has 281 entries.
team_size_source field is absent. team_size_recommended = 5 (literal hardcode).
```

Verifier MUST detect: Check 1 (size < 8KB), Check 4 (ratio 1.0 < 1.3), Check 5 (327 reqs dropped),
Check 6 (uniform MEDIUM with 281 source risks), Check 7 (source missing, sentinel value).
Disposition: FAIL.

**Known-good input:**

```
EFFORT_ESTIMATION.md = 38,400 bytes. summary.total_hours = 56,360.
summary.ai_assisted_hours = 36,624 (ratio = 1.54, savings_percent = 35.0).
Appendix has 2,167 rows matching UNIFIED_RTM.
Risk column shows CRITICAL/HIGH/MEDIUM/LOW with 8% / 22% / 60% / 10% distribution.
team_size_source = "scale-tier: 25k-50k hrs", team_size_recommended = 8.
cost_estimate: direct_labor = $5,859,840, with_overhead = $6,738,816, total_cost = $7,412,698.
Reconciliation: 36,624 × $160 = $5,859,840 ✓; × 1.15 = $6,738,816 ✓; × 1.10 = $7,412,698 ✓.
All 6 canonical roles (PM, BA, Dev, QA, DevOps, Security) listed in Resource Plan.
```

Verifier MUST PASS all checks. Disposition: PASS.

## Hard Rules (inherited from verifier-design principles)

1. NEVER claim EFFORT_ESTIMATION.md is missing without `Glob` verification first.
2. NEVER flag a "MEDIUM" risk distribution as broken if `REQUIREMENT_RISKS.rtm_risks` has < 10 entries — small RFPs legitimately end up uniform.
3. NEVER flag the AI-ratio band on a project where total_hours < 500 (rounding distortion dominates).
4. Every finding must cite a specific field path + value (e.g., `summary.team_size_source = null`).
5. On FAIL, return corrective instructions tied to the specific Step number of phase5-estimation-win that owns the defect, so the phase agent can target the repair without re-running everything.
6. Tech Lead and Senior Developer count as ONE "Dev" role for the canonical-role check — don't fail a plan that uses either label.
