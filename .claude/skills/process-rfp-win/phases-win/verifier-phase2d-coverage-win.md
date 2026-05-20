---
name: verifier-phase2d-coverage-win
expert-role: Workflow Coverage Gate Verifier
purpose: phase-boundary verifier for phase2d-coverage-win (BLOCKING GATE — 100% coverage requirement, gap-report fidelity, orphan workflow detection)
created: 2026-05-20
---

# Verifier — Phase 2d Workflow Coverage (BLOCKING GATE)

## When this runs

After phase2d-coverage-win reports done, BEFORE Phase 3 begins. Phase 2d is itself a BLOCKING GATE — this verifier exists to ensure the gate fired correctly (true 100% coverage, no silent vacuous-pass on zero input) and that the gap report has actionable fidelity (specific workflow_ids, not just counts).

## Inputs (read in this order)

1. `{folder}/shared/workflow-coverage.json` — primary output under verification
2. `{folder}/shared/workflow-extracted-reqs.json` — source of workflow candidates (count must match)
3. `{folder}/shared/requirements-normalized.json` — pool of requirements used for matching
4. Prior verifier report at `{folder}/shared/validation/verifier-phase2d.json` (if a retry run)

## Verification Checks

### Check 1 — File exists and is valid JSON, schema-structurally valid

**Criterion:** `shared/workflow-coverage.json` exists AND parses as valid JSON AND contains top-level keys: `analyzed_at`, `gate_status`, `summary`, `coverage_matrix`, `gaps`, `detailed_report`. The `gate_status` block must contain `passed` (bool), `coverage_percentage` (number), `required_percentage` (must equal 100), `gap_count` (int). The `summary` block must contain `total_workflow_items`, `matched`, `unmatched`, `coverage_percentage`, `quality_breakdown`, `gate_passed`, `zero_input_warning`.

**Pass:** All keys present, JSON parses, file size > 100 bytes.

**Fail:** File absent, JSON parse error, or any required key missing.

**Evidence to cite:** Specific missing key path. File size in bytes.

**Hard-rule reminder:** NEVER claim file is missing without `Glob` verification first.

---

### Check 2 — BLOCKING GATE: coverage_percentage = 100

**Criterion:** `gate_status.passed == true` AND `gate_status.coverage_percentage == 100` (exactly 100, not 99.9, not 99). This is THE blocking gate — if it didn't pass, Phase 3 must not run. EXCEPTION: if `summary.total_workflow_items == 0`, the gate vacuously passes by design (nothing to BLOCK on) — in that case Check 5 (zero-input warning) is the operative check.

**Pass:** `gate_passed == true` AND (`coverage_percentage == 100` OR `total_workflow_items == 0`).

**Fail:** `gate_passed == true` reported but `coverage_percentage < 100` (false-positive gate pass) OR `coverage_percentage == 100` but the matrix shows unmatched items (inconsistent reporting).

**Evidence to cite:** `gate_status.passed`, `gate_status.coverage_percentage`, `summary.matched`, `summary.unmatched`, `summary.total_workflow_items`.

---

### Check 3 — coverage_matrix count parity with upstream workflow candidates

**Criterion:** `len(coverage_matrix)` MUST equal `len(workflow-extracted-reqs.json.requirement_candidates)`. Tolerance: 0. Every workflow candidate from Phase 2a must appear in the coverage analysis.

**Pass:** Counts match exactly.

**Fail:** Mismatch — coverage analysis dropped or invented workflow items.

**Evidence to cite:** `coverage_matrix count = N`, `requirement_candidates count = M`. List up to 5 workflow_ids present in upstream but absent from coverage_matrix.

---

### Check 4 — Every workflow has a matched requirement OR a gap entry (no orphans)

**Criterion:** For each entry in `coverage_matrix`:
- If `matched == true`: `requirement_id` MUST be non-empty AND resolve to a canonical_id present in `requirements-normalized.json.requirements[].canonical_id`.
- If `matched == false`: a corresponding entry in `gaps[]` MUST exist with the same `workflow_id`.

Furthermore: `len(gaps) == summary.unmatched`.

**Pass:** All matched entries point to valid canonical_ids. All unmatched entries have a gap entry. Gap count equals unmatched count.

**Fail:** Any matched entry with a phantom canonical_id (not found in normalized requirements). Any unmatched entry without a gap entry. Gap count != unmatched count.

**Evidence to cite:** Count of phantom canonical_ids referenced. Count of unmatched-without-gap. List up to 5 offenders.

---

### Check 5 — Zero-input warning surfaced when total = 0 (HUNT-B-008 fix)

**Criterion:** If `summary.total_workflow_items == 0`, `summary.zero_input_warning` MUST be non-null and contain the documented HUNT-B-008 warning text (start with "WARNING: Phase 2d coverage check ran with ZERO workflow candidates"). If `total_workflow_items > 0`, `zero_input_warning` MUST be null.

**Pass:** Warning correctly surfaced when input is zero; correctly null otherwise.

**Fail:** Total is 0 but warning is null/missing/empty (vacuous-pass regression) OR total > 0 but warning is populated (false positive).

**Evidence to cite:** `total_workflow_items`, `zero_input_warning` field value (or null).

---

### Check 6 — Gap report includes specific workflow_ids and description text

**Criterion:** Each entry in `gaps[]` has populated fields: `workflow_id` (non-empty), `description` (non-empty, no `[:N]` truncation — full text per V2-F7 fix), `category` (may be null only if upstream had no category), `suggested_action` (non-empty string).

**Pass:** All gap entries have workflow_id + description + suggested_action populated. No `[:N]` truncation marker visible in description.

**Fail:** Any gap entry missing workflow_id or description. Any description ending mid-word in `...` (display truncation leaked into persisted JSON) OR ending in hyphen/comma/subordinator (suggests upstream description was truncated before reaching here).

**Evidence to cite:** Per missing field: count of offenders. List first 3 with workflow_id.

---

### Check 7 — workflow_text not [:100] truncated (V2-F7 fix verification)

**Criterion:** Inspect `coverage_matrix[].workflow_text` for evidence of `[:100]` truncation. Specifically: count entries where `len(workflow_text) == 100` AND the 100th character is mid-word (no whitespace boundary). Such entries indicate the V2-F7 fix didn't fire and display truncation leaked into the persisted JSON.

**Pass:** Zero entries with exactly 100-char workflow_text ending mid-word.

**Concern:** Some entries are exactly 100 chars but end at a word boundary — may be coincidence, log advisory.

**Fail:** Any entry with exactly 100 chars ending mid-word.

**Evidence to cite:** Count of suspicious entries. List first 3 workflow_ids with their workflow_text last-20-chars.

---

### Check 8 — Quality breakdown sums consistently

**Criterion:** `summary.quality_breakdown.high + summary.quality_breakdown.medium + summary.quality_breakdown.low == summary.total_workflow_items`. AND `summary.matched == high + medium` (LOW is the unmatched bucket below the 0.6 threshold).

**Pass:** Sums match.

**Fail:** Sums inconsistent — quality classification logic has a bug.

**Evidence to cite:** Print the three quality counts, matched, unmatched, total.

---

### Check 9 — detailed_report has category_coverage with reasonable categories

**Criterion:** `detailed_report.category_coverage` has at least one category entry. Each entry has `total`, `matched`, `percentage` fields. If `summary.total_workflow_items > 0`, then at least one category in `category_coverage` must have `total > 0`.

**Pass:** Non-empty category_coverage with consistent fields.

**Fail:** Empty category_coverage when total > 0, OR malformed entries (missing total/matched/percentage).

**Evidence to cite:** Number of categories with total > 0, sample category entry.

---

### Check 10 — UTF-8 round-trip and universal anti-regression

**Criterion:** Open `workflow-coverage.json` with `encoding='utf-8'`. Scan all string values for mojibake sentinels (`�`, `Ã©`, `â€™`, `â€"`) — count must be 0. Scan for `_Showing \d+ of \d+_` row-cap notice — count must be 0. Confirm no `[:N]` literal slicing leakage on persisted strings (the V2-F7 fix removed `[:100]` from workflow_text — verify).

**Pass:** Zero hits on sentinels.

**Fail:** Any sentinel found.

**Evidence to cite:** Sentinel pattern, count, first 3 field paths.

---

## Disposition Logic

- **PASS:** Checks 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 all pass.
- **CONCERN:** Check 7 has 100-char entries at word boundary (coincidental); Check 5 fired correctly with zero input. Log advisory but continue.
- **FAIL:** Any of Checks 1, 2, 3, 4, 5, 6, 8, 9, 10 fail OR Check 7 has mid-word 100-char truncations.

**Special disposition:** If Check 2 reports coverage_percentage < 100 AND `gate_passed == false` AND gap report is populated correctly, the verifier's disposition is **PASS** (the BLOCKING GATE correctly fired and prevented downstream phases from running). The verifier's job is to confirm the gate behaved correctly, not to circumvent it.

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase2d-coverage-win with the following targeted corrections:

[Check 1 fail] workflow-coverage.json missing or invalid.
  Action: Verify Step 6 executed and wrote all required top-level keys. The phase
  may have halted at Step 7 BlockingGateFailure WITHOUT writing the file — confirm
  Step 6 runs unconditionally BEFORE Step 7 gate check.

[Check 2 fail] Inconsistent gate-status reporting.
  Action: Audit Step 7 — gate_passed must derive directly from coverage_percentage == 100.
  Confirm calculate_coverage() returns gate_passed = (coverage_pct == 100). If
  gate_passed = true but coverage < 100, there's a calculation bug.

[Check 3 fail] coverage_matrix count {N} != workflow_candidates count {M}.
  Action: Audit Step 2 build-loop — confirm every wf_item from workflow_candidates
  appends exactly one entry to coverage_matrix. No filtering, no skipping.

[Check 4 fail] {N} matched entries point to phantom canonical_ids OR {M} unmatched
  entries lack gap entries.
  Action for phantom IDs: confirm find_best_match() returns canonical_id from the
    actual requirements pool, not a derivative or hash.
  Action for missing gaps: confirm Step 4 identify_gaps() iterates ALL coverage_matrix
    entries with matched==False, no early exit.

[Check 5 fail] Zero-input warning regression.
  Action: Audit Step 3 calculate_coverage() — confirm the if total == 0 branch sets
  zero_input_warning to the documented HUNT-B-008 text. Confirm summary block carries
  zero_input_warning forward (not stripped by a downstream serializer).

[Check 6 fail] Gap entries missing fields or truncated descriptions.
  Action: Audit Step 4 identify_gaps() — confirm all 4 fields populated. Confirm
  description sources from item["workflow_text"] (which itself is full per V2-F7),
  not from a stale truncated source.

[Check 7 fail] workflow_text [:100] truncation leaked.
  Action: Audit Step 2 — confirm V2-F7 fix is in place:
    "workflow_text": wf_item.get("description", "")   # full text, no [:100]
  Display-only [:60] truncation in Step 7 log() is acceptable; persisted JSON must
  carry full text.

[Check 8 fail] Quality breakdown sums inconsistent.
  Action: Audit Step 3 — confirm quality_breakdown counts iterate the same coverage_matrix
  used for matched/unmatched counts.

[Check 9 fail] detailed_report.category_coverage empty.
  Action: Audit Step 5 generate_coverage_report() — confirm the `for category in
  set(item.get("category") for item in matrix)` loop emits entries when matrix is
  non-empty. If categories are all None, upstream Phase 2a didn't populate them
  — escalate to phase2a verifier.

[Check 10 fail] Mojibake / row-cap / [:N] leakage.
  Action: Universal anti-regression. Audit open() calls for encoding='utf-8',
  json.dump for ensure_ascii=False. Reference SAFS memory:
  feedback_screen_encoding_truncation.md.

Max 1 retry. On second FAIL, escalate to human with this verifier report.

NOTE: This verifier does NOT recommend lowering the 100% gate threshold. The gate's
purpose is to surface coverage problems early. If coverage genuinely cannot reach
100%, the user must either (a) create new requirements for unmapped workflow items,
(b) adjust the matching threshold in find_best_match (currently 0.6), or (c) grant
explicit override approval — but THIS verifier does not enable that decision.
```

## Self-Test Cases

**Known-bad input (vacuous-pass regression):**

```
workflow-coverage.json scenario:
  summary.total_workflow_items = 0
  summary.zero_input_warning = null  (REGRESSION: HUNT-B-008 fix didn't fire)
  gate_status.passed = true
  gate_status.coverage_percentage = 100  (vacuously)
  coverage_matrix = []
  gaps = []
```

Verifier MUST detect: Check 5 (zero_input_warning missing on zero-total). Disposition: FAIL. (The vacuous pass is "correct" in pure math terms, but the missing warning means downstream phases lose the signal that the RFP may be misclassified.)

**Known-bad input (false-positive gate):**

```
workflow-coverage.json scenario:
  summary.total_workflow_items = 38
  summary.matched = 35, summary.unmatched = 3
  summary.coverage_percentage = 92.1
  gate_status.passed = true  (BUG — should be false)
  gate_status.coverage_percentage = 92.1
  gaps = [3 entries]
  coverage_matrix entries: WF005, WF012, WF027 have workflow_text exactly 100 chars
    ending mid-word ("the system shall validate input data submitted by the user including")
```

Verifier MUST detect: Check 2 (gate_passed=true but coverage<100), Check 7 ([:100] truncation regression). Disposition: FAIL.

**Known-good input:**

```
workflow-coverage.json scenario:
  summary.total_workflow_items = 38
  summary.matched = 38, summary.unmatched = 0
  summary.coverage_percentage = 100
  summary.quality_breakdown = {high: 24, medium: 14, low: 0}
  gate_status.passed = true, coverage_percentage = 100
  coverage_matrix has 38 entries, all with non-truncated workflow_text
  gaps = []
  detailed_report.category_coverage has 6 categories with consistent fields
  zero_input_warning = null (correctly, since total > 0)
  No mojibake, no row-caps, no [:N] leakage.
```

Verifier MUST PASS all checks. Disposition: PASS.

**Known-good input (correctly-failed gate):**

```
workflow-coverage.json scenario:
  summary.total_workflow_items = 38
  summary.matched = 35, summary.unmatched = 3
  summary.coverage_percentage = 92.1
  gate_status.passed = false  (correctly)
  gaps = [
    {workflow_id: "WF014", description: "...", suggested_action: "Create new requirement..."},
    {workflow_id: "WF023", description: "...", suggested_action: "..."},
    {workflow_id: "WF036", description: "...", suggested_action: "..."}
  ]
  All gap entries have full description, no truncation.
```

Verifier disposition: **PASS** (the BLOCKING GATE correctly fired; verifier confirms the gate behaved per spec). Phase 3 should not run until user resolves the gaps.

## Hard Rules (inherited from verifier-design principles)

1. NEVER claim workflow-coverage.json is missing without `Glob` verification first.
2. NEVER report a phase2d gate FAIL as a verifier FAIL — the verifier's job is to confirm the gate behaved correctly. A correctly-fired gate (gate_passed=false with full gap report) is a PASS disposition for this verifier.
3. NEVER skip Check 5 (zero-input warning) even when total > 0 — verify the warning is correctly null in that case too.
4. NEVER suggest lowering the 100% threshold in corrective instructions. That decision belongs to the user, not to a verifier.
5. Every finding must cite a specific field path + value (e.g., `gate_status.passed = true but coverage_percentage = 92.1`).
6. On FAIL, return corrective instructions with the specific check ID and field path so the agent can target the exact repair without re-running everything.
7. The matched-to-canonical-id verification (Check 4) is the most important defense against phantom matches — never skip it.
