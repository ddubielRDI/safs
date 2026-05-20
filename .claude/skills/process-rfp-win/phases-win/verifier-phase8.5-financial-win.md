---
name: verifier-phase8.5-financial-win
expert-role: Cost Proposal Quality Verifier
purpose: phase-boundary verifier for phase8.5-financial-win (rate-table integrity, rate-source attribution, market-fallback application, cost-eval alignment, TCO presence, payment schedule)
created: 2026-05-20
---

# Verifier — Phase 8.5 Financial Proposal

## When this runs

After phase8.5-financial-win reports done, BEFORE Phase 8.6 (Integration Plan) and Phase 8f (RTM Verification) which expect a complete bid section set.

## Inputs (read in this order)

1. `{folder}/outputs/bid-sections/05_FINANCIAL.md` — primary output under verification
2. `{folder}/shared/effort-estimation.json` — for total_hours and per-phase breakdown
3. `{folder}/shared/EVALUATION_CRITERIA.json` — to detect cost factor weight (LPTA vs Best Value)
4. `config-win/company-profile.json` — for `financial_defaults.labor_rates` and `market_rate_defaults`
5. `{folder}/shared/domain-context.json` — for contract duration / multi-year detection

## Verification Checks

### Check 1 — File exists and meets minimum size

**Criterion:** `outputs/bid-sections/05_FINANCIAL.md` exists AND file size >= 5,120 bytes (5 KB minimum — phase target).

**Pass:** File exists, size >= 5,120 bytes.

**Fail:** File absent OR size < 5,120 bytes.

**Evidence to cite:** File path + actual size in bytes.

**Hard-rule reminder:** NEVER claim file is missing without `Glob` verification first.

---

### Check 2 — Cost summary table present with all 6 line items

**Criterion:** Section 1.1 (Total Cost Overview) must contain a table with these rows: Direct Labor, Overhead (with %), Other Direct Costs, Subtotal, Profit (with %), **Total Proposed Cost** (bold). All amounts must be non-null (may be `$0` if unpopulated, but cells must exist).

**Pass:** All 6 rows present.

**Fail:** Cost summary table missing OR any required row absent.

**Evidence to cite:** Grep "Total Proposed Cost"; list rows found in cost summary table.

---

### Check 3 — Labor rate schedule with Rate Source column

**Criterion:** Section 2 (Labor Rate Schedule) must contain a table with columns: Role, Hourly Rate, **Rate Source**, GSA Range, Estimated Hours, Extended Cost.

For each row, Rate Source cell must contain one of: `COMPANY RATE`, `MARKET DEFAULT`, `UNPOPULATED`. Any other value or empty cell is FAIL.

**Pass:** Table present with all 6 columns; every row's Rate Source is one of the 3 allowed values.

**Fail:** Table missing OR any column absent OR any row has invalid Rate Source.

**Evidence to cite:** Column header row; sample row content; any malformed Rate Source values.

---

### Check 4 — Market rate fallback applied where company rates = $0

**Criterion:** Load `company.financial_defaults.labor_rates` and `company.market_rate_defaults.labor_rates`. For each role where company rate = $0 or absent AND market default rate > $0, the document must show `MARKET DEFAULT` as the Rate Source for that role.

If company rate > $0, document must show `COMPANY RATE`. If both are $0 or absent, document must show `UNPOPULATED` and a corresponding ACTION REQUIRED marker.

**Pass:** Fallback logic correctly applied per Step 1's normalization algorithm.

**Concern:** Fallback applied for most roles; 1 role shows wrong attribution.

**Fail:** 2+ roles show wrong Rate Source attribution vs company-profile.json source data.

**Evidence to cite:** Per failing role: name + company rate + market rate + claimed Rate Source.

---

### Check 5 — Cost evaluation alignment (cost factor weight respected)

**Criterion:** Load `evaluation_factors` from EVALUATION_CRITERIA.json. Find a factor with `cost` / `price` / `financial` / `budget` keyword. Note its weight.

Section 5.1 (Cost Evaluation Alignment) must:
(a) Reference the cost factor weight by number/percentage.
(b) Indicate strategy: LPTA emphasis (if cost is sole/dominant factor) vs Best Value emphasis (if cost is one of several).

**Pass:** Cost weight referenced AND strategy stated.

**Concern:** Cost weight referenced but strategy is vague.

**Fail:** No cost weight reference in Section 5.1 OR Section 5.1 absent.

**Evidence to cite:** Section 5.1 first 500 chars; cost factor name + weight from EVALUATION_CRITERIA.

---

### Check 6 — Total Cost of Ownership (TCO) section present

**Criterion:** Section 5.3 (Total Cost of Ownership) must exist with content addressing: implementation cost, maintenance cost, support cost, training cost. For multi-year contracts (detect via `domain-context.json` contract duration field, or via 3yr/5yr language), a 3-year AND/OR 5-year TCO comparison must appear.

**Pass:** TCO section present; multi-year contracts have multi-year comparison.

**Concern:** TCO section present but lacks multi-year comparison for a multi-year contract.

**Fail:** TCO section absent entirely.

**Evidence to cite:** Grep "Total Cost of Ownership" or "TCO"; section content excerpt.

---

### Check 7 — Payment schedule auto-generated from effort phases

**Criterion:** Section 6 (Payment Schedule) must contain a table with columns: Milestone, Key Deliverable, Payment %, Cumulative %. Number of milestone rows >= 3 (single-phase projects are atypical).

Cross-check: if `effort-estimation.json` has N phases, the payment schedule should have approximately N milestones. Cumulative % at last milestone <= 100%, with a "Project Completion" row reaching 100%.

**Pass:** Payment schedule table present with >= 3 milestones; cumulative reaches 100% at end.

**Concern:** Schedule present but milestone count differs from phase count by 2+ (manual override).

**Fail:** Section 6 absent OR < 3 milestones OR cumulative % does not reach 100%.

**Evidence to cite:** Milestone count from doc vs phase count from effort-estimation; cumulative % at last row.

---

### Check 8 — UNPOPULATED rates flagged with ACTION REQUIRED markers

**Criterion:** For every role with Rate Source = `UNPOPULATED`, the document must contain an "ACTION REQUIRED" or "[USER INPUT REQUIRED" marker mentioning that role's name (within a reasonable proximity — within the same section).

Additionally, if any roles use `MARKET DEFAULT`, a "REVIEW REQUIRED" or equivalent marker must appear noting market-default usage.

**Pass:** Every UNPOPULATED role has ACTION REQUIRED marker; if MARKET DEFAULTs used, REVIEW REQUIRED marker present.

**Concern:** Markers present but not paired with specific role names.

**Fail:** UNPOPULATED roles lack any flagging marker — submission risk.

**Evidence to cite:** List UNPOPULATED roles; grep for ACTION REQUIRED in document; report coverage.

---

### Check 9 — Page-budget awareness (MARS 25-page cap)

**Criterion:** Estimate pages by char/3,000. Financial volume target: 2-4 pages (6,000-12,000 chars).

**Pass:** 6,000-12,000 chars (2-4 pages).

**Concern:** 5,000-6,000 (thin) OR 12,000-15,000 (heavy).

**Fail:** > 15,000 chars (> 5 pages) OR < 4,000 chars.

**Evidence to cite:** Actual char count, estimated page count.

---

### Check 10 — Universal regression patterns

**Criterion:** Five sub-checks:
(a) UTF-8 decode clean; no mojibake.
(b) Zero `_Showing \d+ of \d+_` row-cap notices.
(c) Zero `[:N]` truncation patterns.
(d) Zero em-dash chars (`—` U+2014).
(e) Zero empty `|  |` cells in rate or cost tables where adjacent cells contain values.

**Pass:** All sub-checks pass.

**Fail:** Any sub-check has 1+ violation.

**Evidence to cite:** Per violation: line + first 80 chars quoted.

---

## Disposition Logic

- **PASS:** All 10 checks pass.
- **CONCERN:** Checks 4, 5, 6, 7, 8, or 9 in advisory band; all others pass.
- **FAIL:** Any of Checks 1, 2, 3, 10 fail OR Checks 4/5/6/7/8/9 fall below FAIL threshold.

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase8.5-financial with the following targeted corrections:

[Check 1 fail] 05_FINANCIAL.md missing or < 5 KB ({actual_size} bytes).
  Action: Re-run phase. Confirm effort-estimation.json loaded with phase data
  AND company-profile.json has financial_defaults populated.

[Check 2 fail] Cost summary missing rows: {list}.
  Action: Step 3 Section 1.1 template renders all 6 rows. Confirm labor_total,
  overhead, odc, subtotal, profit, total are all computed (even if $0).

[Check 3 fail] Labor rate schedule issue: {description}.
  Action: Step 1's rate normalization loop builds labor_rates with Rate Source
  values from {COMPANY RATE, MARKET DEFAULT, UNPOPULATED}. Confirm no roles
  fall through to a 4th unrecognized value.

[Check 4 fail] Rate fallback misapplied for: {list of roles}.
  Action: Step 1's `if c_rate > 0` check is the fallback gate. Audit role-by-role:
  for each role where company rate = 0 but market rate > 0, the document MUST
  show MARKET DEFAULT.

[Check 5 fail] Cost evaluation alignment thin/missing.
  Action: Section 5.1 must extract cost_factor weight from EVALUATION_CRITERIA
  and state strategy (LPTA if cost is sole/dominant, Best Value otherwise).

[Check 6 fail] TCO section missing OR multi-year comparison absent.
  Action: Section 5.3 must address implementation + maintenance + support + training.
  For 3+ year contracts, include "3-Year TCO" and/or "5-Year TCO" comparison
  table (auto-detect contract duration from domain-context).

[Check 7 fail] Payment schedule issue: {description}.
  Action: Section 6's milestones must be auto-generated from effort.phases
  iteration in Step 3. If only N=1-2 milestones, expand by splitting larger
  phases. Confirm cumulative_pct reaches 100% via the Project Completion row.

[Check 8 fail] UNPOPULATED roles lack ACTION REQUIRED markers.
  Action: Step 4's warning template assembly must run for every UNPOPULATED role.
  Surface the warning block at the top of the document with role names listed.

[Check 9 fail] Financial volume is {N} chars ({pages} pages).
  Action: For >15K: trim Section 4.2 (Key Assumptions) and Section 5.2 (Cost
  Efficiency) — these are narrative-heavy. For <4K: each section needs basic
  content (Cost Summary, Labor Rates, Phase Breakdown are mandatory).

[Check 10 fail] Regression pattern {type} at line {N}.
  Action: Strip em dashes, eliminate [:N] slices, fix empty cells.

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input:**

```
05_FINANCIAL.md: 3,800 bytes.
  Cost summary table present but missing the Overhead row (jumps from Direct Labor to ODC).
  Labor rate schedule: 6 roles. Roles "Project Manager" and "Senior Developer" have
    company rate = 0 in company-profile, market rate = $145, $175 respectively.
    Document shows Rate Source = "UNPOPULATED" for both (should be MARKET DEFAULT).
  Section 5.1 (Cost Evaluation Alignment) absent.
  Section 5.3 (TCO) absent.
  Section 6 (Payment Schedule) shows only 1 milestone with cumulative 100%.
  No ACTION REQUIRED markers for the 2 genuinely UNPOPULATED roles (PM, BA).
  2 em-dash chars in Section 4.
```

Verifier MUST detect: Check 1 (< 5 KB), Check 2 (Overhead row missing), Check 4 (2 roles show wrong Rate Source attribution), Check 5 (Section 5.1 absent), Check 6 (TCO absent), Check 7 (< 3 milestones), Check 8 (UNPOPULATED roles unflagged), Check 10 (em dashes). Disposition: FAIL.

**Known-good input:**

```
05_FINANCIAL.md: 8,400 bytes.
  Cost summary table complete: 6 rows including bold Total Proposed Cost.
  Labor rate schedule: 8 roles, all 6 columns present.
    5 roles use COMPANY RATE, 2 use MARKET DEFAULT (with REVIEW REQUIRED marker),
    1 uses UNPOPULATED (with ACTION REQUIRED marker naming the role).
  Section 5.1 references cost weight: "Cost represents 20% of the evaluation
    score. Our pricing strategy optimizes for Best Value..."
  Section 5.3 TCO present with 5-Year TCO comparison (contract is 5-year IDIQ).
  Section 6 Payment Schedule has 5 milestones from effort-estimation.json's
    5 phases, cumulative reaches 100% at Project Completion row.
  ACTION REQUIRED + REVIEW REQUIRED markers all present with role names.
  No em dashes, no row caps, UTF-8 clean.
```

Verifier MUST PASS all checks. Disposition: PASS.

## Hard Rules (inherited from verifier-design principles)

1. NEVER claim 05_FINANCIAL.md is missing without `Glob` verification first.
2. NEVER flag a $0 Direct Labor total as "missing" if `effort-estimation.json` is genuinely empty (zero phases produced) — the file should still render with $0 totals + a prominent warning marker.
3. NEVER fail Check 4 when a role appears in company-profile.financial_defaults.labor_rates but with `hourly_rate` keyed differently (e.g., `rate_per_hour`) — apply the normalize_role function logic before declaring mismatch.
4. NEVER fail Check 6 for an RFP genuinely shorter than 1 year (TCO is less meaningful for short engagements) — log CONCERN instead.
5. NEVER fail Check 7 when `effort-estimation.json` is genuinely absent (`read_json_safe` returned empty) — the phase template inserts a "[USER INPUT REQUIRED: payment schedule]" placeholder which is acceptable in this fail-soft scenario.
6. Every finding must cite line number + first 80 chars of offending content (or role name + rate values for Check 4).
