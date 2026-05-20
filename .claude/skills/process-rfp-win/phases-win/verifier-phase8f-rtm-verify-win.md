---
name: verifier-phase8f-rtm-verify-win
expert-role: RTM Verification Auditor
purpose: phase-boundary verifier for phase8f-rtm-verify-win (14 query execution, coverage metrics computation, UNIFIED_RTM verification{} update, no broken trace links surviving)
created: 2026-05-20
---

# Verifier — Phase 8f RTM Verification

## When this runs

After phase8f-rtm-verify-win reports done, BEFORE SVA-7 (Gold Team gate). This is the final traceability gate — verifies that Phase 8f actually ran 14 queries and updated the RTM, not that it merely emitted a report file.

## Inputs (read in this order)

1. `{folder}/outputs/RTM_REPORT.md` — primary human-readable output under verification
2. `{folder}/shared/UNIFIED_RTM.json` — verify `verification{}` section was populated
3. `{folder}/shared/EVALUATION_CRITERIA.json` — for evaluation alignment cross-check
4. `{folder}/shared/COMPLIANCE_MATRIX.json` — for mandatory item count consistency
5. Prior RTM verification (if any retry): `{folder}/shared/UNIFIED_RTM.json` chain_version before this run

## Verification Checks

### Check 1 — Both output files exist and meet minimum sizes

**Criterion:** Two files required:
(a) `outputs/RTM_REPORT.md` exists AND size >= 5,120 bytes (5 KB minimum — phase target).
(b) `shared/UNIFIED_RTM.json` exists AND parses as JSON AND has a top-level `verification` key that is a dict (not null, not empty).

**Pass:** Both files exist; RTM_REPORT.md >= 5 KB; UNIFIED_RTM.json has populated `verification` dict.

**Fail:** Either file absent OR RTM_REPORT.md < 5 KB OR UNIFIED_RTM.json missing `verification` OR `verification` is empty/null.

**Evidence to cite:** File paths + sizes + top-level keys of `verification` dict.

**Hard-rule reminder:** NEVER claim file is missing without `Glob` verification first.

---

### Check 2 — All 14 verification queries executed

**Criterion:** `UNIFIED_RTM.verification.query_results` is a list with `len() == 14`. The query_ids present must be exactly: F1, F2, F3, F4, F5, B1, B2, B3, B4, C1, C2, C3, C4, C5.

**Pass:** Length is 14 AND set of query_ids equals the required 14.

**Fail:** Length != 14 OR any required query_id missing.

**Evidence to cite:** Print `[q["query_id"] for q in query_results]`; list missing IDs.

---

### Check 3 — Every query result has the required fields

**Criterion:** Each entry in `query_results` must have non-null: `query_id`, `query_name`, `direction`, `passed` (bool), `score` (number), `details` (string).

`direction` must be one of: `forward`, `backward`, `chain`.

**Pass:** All 14 entries have all 6 required fields with valid types.

**Fail:** Any entry missing any field OR `direction` value is non-standard.

**Evidence to cite:** Per malformed entry: query_id + missing/invalid field.

---

### Check 4 — Coverage metrics computed (req-to-bid-section, mandatory-to-response, risk-to-mitigation)

**Criterion:** `UNIFIED_RTM.verification` must contain all three coverage sections with required sub-fields:

(a) `forward_coverage`: `requirements_with_specs`, `requirements_with_bid_sections`, `requirements_with_risks`, `requirements_total`, `spec_coverage_pct`, `bid_coverage_pct` — all present, numeric.

(b) `backward_coverage`: `bid_sections_with_requirements`, `bid_sections_total`, `orphaned_bid_content` — all present, numeric.

(c) `chain_completeness`: `complete_chains`, `partial_chains`, `broken_chains`, `avg_completeness_score` — all present, numeric.

(d) `compliance_alignment`: `mandatory_items_in_bid`, `mandatory_items_total`, `coverage_pct` — all present, numeric.

**Pass:** All four sections fully populated with numeric values.

**Fail:** Any section absent OR any required sub-field missing or non-numeric.

**Evidence to cite:** Print full `verification` keys + per-missing-field path.

---

### Check 5 — RTM_REPORT.md has Executive Summary + Coverage Dashboard + Query Results table

**Criterion:** Grep RTM_REPORT.md for:
(a) `^## Executive Summary` — required heading.
(b) `^## Coverage Dashboard` — required heading.
(c) `^## Verification Query Results` (or similar) — required.
(d) Three query-results tables grouped by direction: forward, backward, chain. Each table must list query rows with ID, Score, Status columns.

**Pass:** All 4 required structural elements present.

**Concern:** 3 of 4 present.

**Fail:** < 3 of 4 present OR the per-direction query tables are missing.

**Evidence to cite:** Per missing element: grep result.

---

### Check 6 — Failed queries have specific Recommended Actions

**Criterion:** For each query in `query_results` where `passed == False`, the RTM_REPORT.md must contain a "Recommended Action" block (or equivalent corrective guidance) for that query_id. Generic "review failed items" without specific guidance = FAIL for that query.

**Pass:** Every failed query has a specific Recommended Action block.

**Concern:** 1 failed query has generic action; others specific.

**Fail:** 2+ failed queries have generic or absent action blocks.

**Evidence to cite:** List failed query_ids; per failure: action block presence + first 200 chars.

---

### Check 7 — Overall disposition reflects query pass rate

**Criterion:** RTM_REPORT.md must surface an "Overall Status" at the top with value one of: `PASS`, `ADVISORY`, `FAIL`.

Cross-check the disposition against query pass rate:
- PASS: all 14 queries passed (pass_count == 14)
- ADVISORY: pass_count between 10-13 (>= 70%)
- FAIL: pass_count < 10

The displayed disposition must align with the actual pass count.

**Pass:** Disposition value present AND matches computed pass rate.

**Fail:** Disposition absent OR contradicts pass rate (e.g., shows PASS when 3 queries failed).

**Evidence to cite:** Disposition value from report + actual pass count + expected disposition.

---

### Check 8 — No broken trace links surviving (chain integrity)

**Criterion:** After Phase 8f runs, the RTM should have surfaced and reported on broken chains. Verify:
(a) `verification.chain_completeness.broken_chains` is a number (not null).
(b) If broken_chains > 0, RTM_REPORT.md must include a "Chain Gap Analysis" or "Most Common Missing Links" section showing what's broken.
(c) `verification.chain_completeness.avg_completeness_score` is computed (numeric).

A non-zero `broken_chains` count is acceptable — broken chains exist and Phase 8f surfaced them. What's NOT acceptable: broken_chains > 0 but RTM_REPORT.md has no gap analysis (Phase 8f detected but didn't report).

**Pass:** Counts populated; if broken_chains > 0, gap analysis is in the report.

**Fail:** Counts absent OR broken_chains > 0 with no gap analysis section.

**Evidence to cite:** Count values + gap-analysis section presence.

---

### Check 9 — UNIFIED_RTM.json chain_version incremented

**Criterion:** `UNIFIED_RTM.meta.chain_version` must be > the previous version (Phase 8f appends to verification, so it should bump the version). Also `UNIFIED_RTM.meta.last_updated_by_phase` should equal `"phase8f-rtm-verify"`.

**Pass:** chain_version incremented AND last_updated_by_phase set correctly.

**Concern:** last_updated_by_phase set but chain_version did not increment (idempotent re-run scenario).

**Fail:** last_updated_by_phase missing OR not equal to "phase8f-rtm-verify".

**Evidence to cite:** Current `meta.chain_version` + `meta.last_updated_by_phase`.

---

### Check 10 — Universal regression patterns

**Criterion:** Three sub-checks across both RTM_REPORT.md and UNIFIED_RTM.json:
(a) UTF-8 decode clean on both files; no mojibake patterns.
(b) Zero `_Showing \d+ of \d+_` row-cap notices in RTM_REPORT.md.
(c) Zero `[:N]` truncation patterns in any displayed list within RTM_REPORT.md (the phase emits `unlinked[:10]` in details strings — that is acceptable IF the count is shown alongside, e.g., "Unlinked: REQ-001, REQ-002 (+45 more)"). Bare slicing without count surfacing = FAIL.

**Pass:** All sub-checks pass.

**Fail:** Any sub-check has 1+ violation.

**Evidence to cite:** Per violation: file + line + first 80 chars quoted.

---

## Disposition Logic

- **PASS:** All 10 checks pass.
- **CONCERN:** Checks 5, 6, or 9 in advisory band; all others pass.
- **FAIL:** Any of Checks 1, 2, 3, 4, 7, 8, 10 fail OR Checks 5/6/9 fall below FAIL threshold.

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase8f-rtm-verify with the following targeted corrections:

[Check 1 fail] RTM_REPORT.md absent/small OR UNIFIED_RTM.verification missing.
  Action: Re-run phase. The phase must complete BOTH writes:
    - write_file(RTM_REPORT.md)
    - write_json(UNIFIED_RTM.json) with verification dict populated
  Confirm both completed.

[Check 2 fail] {N} queries executed (need 14). Missing IDs: {list}.
  Action: Step 3-5 define 14 query functions (F1-F5, B1-B4, C1-C5). Each must
  append to query_results. Confirm no query function returned early or threw
  an exception that was swallowed.

[Check 3 fail] Query result(s) malformed: {list of query_ids with missing fields}.
  Action: Each query function must return a dict with all 6 required fields.
  Audit run_f1 through run_c5 — common missed field: `direction`.

[Check 4 fail] Coverage metrics incomplete: {list of missing sub-fields}.
  Action: Step 6 (Update RTM Verification Section) assembles the four coverage
  sections. Confirm all formula computations ran (don't shortcut on empty
  upstream data — emit 0 with denominator preservation).

[Check 5 fail] RTM_REPORT.md structural element(s) missing: {list}.
  Action: generate_rtm_report() in Step 7 must emit Executive Summary,
  Coverage Dashboard, Verification Query Results sections. Confirm the function
  didn't return early on a missing data branch.

[Check 6 fail] Failed queries lack specific recommended actions: {list}.
  Action: Step 7's per-query-id recommendation generator has branches for F1, F2,
  F3, F4, C1, C2 explicitly. Extend with branches for F5, B1-B4, C3, C4, C5.

[Check 7 fail] Disposition mismatch: displays {X}, computed {Y}.
  Action: Step 7's `overall` calc: PASS if passed == total, ADVISORY if passed
  >= total * 0.7, FAIL otherwise. Confirm the calc ran on the actual pass count
  not a stale value.

[Check 8 fail] Broken chains exist but no gap analysis in report.
  Action: Step 7's "Most Common Missing Links" section must run whenever
  chain_completeness.broken_chains > 0. Confirm the missing_counts aggregation
  produces a table.

[Check 9 fail] chain_version not incremented OR last_updated_by_phase wrong.
  Action: Step 6 must set rtm["meta"]["last_updated_by_phase"] = "phase8f-rtm-verify"
  AND rtm["meta"]["chain_version"] = current + 1.

[Check 10 fail] Regression pattern {type} at {file}:{line}.
  Action: Strip em dashes, fix UTF-8 encoding errors, ensure (+N more) count
  surfaces for any [:N] slice in details strings.

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input:**

```
RTM_REPORT.md: 3,800 bytes.
  No "Coverage Dashboard" section heading.
  "Overall Status: PASS" displayed at top.
  Only 11 query rows in query result tables.
UNIFIED_RTM.json:
  verification.query_results has 11 entries (F1-F5, B1-B3, C1-C3 — missing B4, C4, C5).
  verification.chain_completeness.broken_chains = 47 but RTM_REPORT.md has no
    "Chain Gap Analysis" or "Missing Links" section.
  3 query results lack the `direction` field.
  Two failed queries (F3 and C2) have only "review failed items" generic action.
  meta.chain_version unchanged from prior version.
  meta.last_updated_by_phase = "phase4-traceability" (stale, didn't update).
```

Verifier MUST detect: Check 1 (< 5 KB), Check 2 (11 of 14), Check 3 (3 entries missing `direction`), Check 5 (Coverage Dashboard absent), Check 6 (2 generic actions), Check 7 (claims PASS with 3+ failures), Check 8 (47 broken chains, no gap section), Check 9 (chain_version unchanged + wrong last_updated_by_phase). Disposition: FAIL.

**Known-good input:**

```
RTM_REPORT.md: 18,400 bytes.
  All structural elements: Executive Summary, Coverage Dashboard, Verification
    Query Results with 3 grouped tables (forward, backward, chain).
  Overall Status: ADVISORY (11 of 14 queries passed — between thresholds).
  Per failed query (F3, B4, C3): specific Recommended Action blocks with
    targeted guidance.
  Chain Gap Analysis present (broken_chains = 12); missing-links table shows
    top patterns ("missing bid_section" 60%, "missing evidence" 32%).
UNIFIED_RTM.json:
  verification.query_results: 14 entries with all required fields.
  verification.forward_coverage.spec_coverage_pct = 96.4
  verification.backward_coverage.orphaned_bid_content = 0
  verification.chain_completeness.broken_chains = 12 (surfaced in report).
  verification.compliance_alignment.coverage_pct = 97.0
  meta.chain_version incremented from 3 to 4.
  meta.last_updated_by_phase = "phase8f-rtm-verify".
  UTF-8 clean, no row caps, all details strings have "+N more" suffixes where [:10] applied.
```

Verifier MUST PASS all checks. Disposition: PASS.

## Hard Rules (inherited from verifier-design principles)

1. NEVER claim RTM_REPORT.md or UNIFIED_RTM.json is missing without `Glob` verification first.
2. NEVER fail Check 8 (gap analysis) when `broken_chains == 0` — vacuous pass is correct.
3. NEVER count a `details: "Unlinked: REQ-001, REQ-002, REQ-003 (+45 more)"` string as a truncation in Check 10c — the count surfacing makes the slice acceptable. Bare `[:10]` without count surfacing IS a violation.
4. NEVER flag a missing `direction` field if the entry has a non-standard field name (e.g., `query_direction`) — accept reasonable alias and log CONCERN.
5. NEVER fail Check 9 on a re-run that uses the same chain_version (idempotency-aware execution) — confirm the run is genuinely a re-execution (timestamp check on `last_run`) before flagging.
6. Every finding must cite specific file + line/key path + value (e.g., `verification.query_results[7].direction = null`, NOT "some query has problem").
