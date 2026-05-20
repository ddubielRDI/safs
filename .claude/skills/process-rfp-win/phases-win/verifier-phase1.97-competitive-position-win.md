---
name: verifier-phase1.97-competitive-position-win
expert-role: Competitive Position Integrity Verifier
purpose: phase-boundary verifier for phase1.97-competitive-position-win (ghost strategy + pain map + switching costs + win conditions traced to factors)
created: 2026-05-20
---

# Verifier — Phase 1.97 Competitive Position

## When this runs

After phase1.97-competitive-position-win reports done, at the end of Stage 1. Output feeds Phase 3a-3g architects (component choices weighted by ghost weaknesses) and Phase 8.0 positioning. A faulty competitive position cascades through every Stage 3 design decision and every Stage 7 bid narrative.

## Inputs (read in this order)

1. `{folder}/outputs/COMPETITIVE_POSITION.md` — human-readable strategy document
2. `{folder}/shared/bid/COMPETITIVE_POSITION.json` — machine-readable consumed downstream
3. `{folder}/shared/bid/CLIENT_INTELLIGENCE.json` — for incumbent + pain point traceability
4. `{folder}/shared/EVALUATION_CRITERIA.json` — for win-condition factor linking
5. `{folder}/shared/GO_NOGO_DECISION.json` — for score / recommendation cross-check

## Verification Checks

### Check 1 — Both output files exist with sufficient size

**Criterion:**
- `outputs/COMPETITIVE_POSITION.md` exists, size >= 3,072 bytes (per phase spec).
- `shared/bid/COMPETITIVE_POSITION.json` exists, parses as valid JSON, size >= 500 bytes.

**Pass:** Both files exist with their respective size thresholds met.

**Fail:** Either file absent OR under-size OR JSON invalid.

**Evidence to cite:** Actual sizes + JSON parse status.

---

### Check 2 — JSON has required top-level schema

**Criterion:** COMPETITIVE_POSITION.json top-level keys include: `generated_at`, `buyer`, `incumbent`, `ghost_strategy`, `pain_map`, `switching_costs`, `win_conditions`, `go_nogo_input`.

**Pass:** All 8 keys present.

**Fail:** Any key missing.

**Evidence to cite:** Actual top-level keys vs expected set.

---

### Check 3 — `ghost_strategy` has at least one entry when incumbent named

**Criterion:** If `incumbent` is non-null AND `incumbent.name` is non-empty, `ghost_strategy` array MUST have length >= 1. Greenfield (no incumbent) → empty ghost_strategy is acceptable but unusual; log CONCERN.

**Pass:** Incumbent present AND ghosts >= 1, OR no incumbent AND ghosts may be empty.

**Concern:** No incumbent AND ghosts empty — log advisory (greenfield positioning).

**Fail:** Incumbent named but ghost_strategy empty (positioning vs the incumbent was not built).

**Evidence to cite:** Incumbent name + ghosts count.

---

### Check 4 — Ghost statements target at least one named competitor (internally)

**Criterion:** Each entry in `ghost_strategy` MUST have `incumbent_weakness` (the internal-only reference to a real competitor weakness from CLIENT_INTELLIGENCE) AND `ghost_phrasing` (the proper-noun-stripped version that will appear in bid text). The `ghost_phrasing` MUST NOT contain the incumbent's proper noun.

**Pass:** Every ghost has both fields; ghost_phrasing is free of incumbent name.

**Fail:** Any ghost with empty `incumbent_weakness` OR `ghost_phrasing` still containing the incumbent's name.

**Evidence to cite:** Ghost index + ghost_phrasing snippet + incumbent name.

---

### Check 5 — No competitor proper nouns in COMPETITIVE_POSITION.md ghost section

**Criterion:** In COMPETITIVE_POSITION.md, the `## Ghost Strategy` section MUST NOT contain the incumbent's proper noun in the ghost-phrased statements (those appear in `### {ghost_phrasing}` headers and the narrative body, NOT in the explicit `**Underlying competitor weakness (internal only):**` lines which are allowed to name the competitor as internal-only context).

**Pass:** Ghost-phrased headers and ghost narrative body have zero matches for incumbent proper noun.

**Fail:** Any incumbent-name leakage outside the explicitly internal "Underlying competitor weakness" lines.

**Evidence to cite:** Incumbent name + line numbers + section.

---

### Check 6 — `pain_map` is non-empty and each entry maps to an evaluation factor

**Criterion:** `pain_map` array length >= 1 (assuming Phase 1.95 produced pain_points). Every entry has `pain_point` (text), `evaluation_factor` (non-null factor name OR explicit "no factor match" rationale), `our_response`, `demonstrated_in`.

**Pass:** Length >= 1 AND each entry has all 4 keys with `evaluation_factor` non-null (or documented as no-match).

**Concern:** 1 of N pain points has null evaluation_factor without explanation — log advisory.

**Fail:** Multiple pain points without factor mapping (Step 4 _link_to_factor did not run).

**Evidence to cite:** Per pain-point: keys + evaluation_factor value.

---

### Check 7 — `switching_cost_analysis` present when incumbent exists

**Criterion:** If incumbent is non-null, `switching_costs` array length >= 1 (phase Step 5 emits 4 defaults when has_incumbent). If no incumbent, switching_costs may be empty.

**Pass:** Incumbent + switching_costs >= 1, OR no incumbent + empty acceptable.

**Fail:** Incumbent present but switching_costs empty (Step 5 skipped).

**Evidence to cite:** Incumbent flag + switching_costs count.

---

### Check 8 — `win_conditions` has 3-5 entries with required schema

**Criterion:** `win_conditions` length in `[3, 5]`. Every entry has `id` (matches `^WC-\d+$`), `condition` (text), `rationale` (text), `owner_sections` (list of bid section filenames).

**Pass:** Count in `[3, 5]` AND all entries have full schema.

**Concern:** Count is 6-7 — log advisory; phase spec says 3-5.

**Fail:** Count < 3 OR > 7 OR any entry missing keys.

**Evidence to cite:** Count + keys of win_conditions[0].

---

### Check 9 — Each win condition tied to an evaluation factor (directly or via ghost/pain)

**Criterion:** For each `win_condition`, verify it ties back to a known evaluation factor — either:
- (a) `condition` text mentions a factor name from EVALUATION_CRITERIA.json, OR
- (b) the win condition is WC-1 (canonically the top-factor win — phase Step 6 emits this from `factors_sorted[0]`), OR
- (c) the rationale references a ghost weakness or pain point that links to a factor.

At least 80% (rounded down) of win conditions must demonstrably tie to a factor.

**Pass:** >= 80% trace to a factor.

**Concern:** 60-79% — log advisory.

**Fail:** < 60% (win conditions floating untethered to evaluation criteria).

**Evidence to cite:** Per win condition: factor link trace.

---

### Check 10 — `go_nogo_input` block consistent with Phase 1.9 output

**Criterion:** `go_nogo_input.total_score` matches `GO_NOGO_DECISION.json.overall_score` exactly. `go_nogo_input.recommendation` matches `GO_NOGO_DECISION.json.recommendation` exactly.

**Pass:** Both fields match.

**Fail:** Either field differs from Phase 1.9 source.

**Evidence to cite:** Both values from each source.

---

### Check 11 — Buyer name consistent with jurisdiction_anchor and CLIENT_INTELLIGENCE

**Criterion:** `buyer.name` is non-empty AND consistent with `CLIENT_INTELLIGENCE.json.client_info.organization_name` (allow paraphrasing). Cross-check `domain-context.json.jurisdiction_anchor` — buyer should reference the same state/entity.

**Pass:** All three sources reference the same buyer (with paraphrasing tolerance).

**Concern:** Paraphrasing departs notably — log advisory.

**Fail:** Buyer name contradicts CLIENT_INTELLIGENCE or anchor (different entity entirely — e.g., wrong state).

**Evidence to cite:** Three values + reconciliation.

---

### Check 12 — Anti-truncation / anti-row-cap / encoding integrity

**Criterion:** Neither file contains `_Showing N of M_` notices, mid-word `[:N]` truncations, or mojibake (`â€"`, etc.). Markdown tables in pain_map / switching_costs sections do NOT show empty `|  |` cells in HIGH/MEDIUM severity rows.

**Pass:** Zero matches.

**Fail:** Any match.

**Evidence to cite:** File + line + offending content.

---

## Disposition Logic

- **PASS:** Checks 1, 2, 4, 5, 7, 8, 10, 11, 12 pass AND Checks 3, 6, 9 not in advisory band.
- **CONCERN:** Greenfield with no ghosts (Check 3) OR 1 unmapped pain point (Check 6) OR Check 9 in advisory band OR Check 11 paraphrasing-departs. Log + continue to Stage 2.
- **FAIL:** Any of Checks 1, 2, 4, 5, 7, 8, 10, 11, 12 fail OR Checks 3/6/9 hard-fail.

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase1.97-competitive-position-win with the following targeted corrections:

[Check 1 fail] COMPETITIVE_POSITION.md or .json missing/under-size.
  Action: Re-run from Step 1. Verify Phase 1.95 CLIENT_INTELLIGENCE.json exists.
  Under-size MD usually means empty sections; ensure ghosts/pain_map/switching_costs/
  win_conditions are populated before write.

[Check 2 fail] JSON schema incomplete.
  Action: Audit Step 7. position_obj template must include all 8 keys.

[Check 3 fail] Incumbent named but no ghosts.
  Action: Re-run Step 3. The for loop over incumbent.known_issues must produce a ghost
  per issue. If known_issues was empty, supplement from CLIENT_INTELLIGENCE.pain_points
  attributed to the incumbent.

[Check 4 fail] Ghost retains incumbent proper noun.
  Offending: {list}. Action: Re-run Step 3 _ghost_phrase function. The proper-noun
  stripping must handle compound names and case variations. Verify by post-grep on
  each ghost_phrasing for the incumbent name.

[Check 5 fail] Incumbent name leaks into COMPETITIVE_POSITION.md ghost section.
  Lines: {list}. Action: After Step 7 markdown emit, run a sanity grep that asserts
  the incumbent name does NOT appear under ## Ghost Strategy section headers/bodies
  (only under "Underlying competitor weakness (internal only):" lines).

[Check 6 fail] Pain points without evaluation_factor mapping.
  Offending: {list}. Action: Re-run Step 4 _link_to_factor. If a pain point cannot
  be tied to a factor, set evaluation_factor=null AND populate a no_factor_reason
  field (e.g., "Pain point relates to internal operations, not scored externally").

[Check 7 fail] Incumbent named but switching_costs empty.
  Action: Re-run Step 5. The 4 default switching_costs (Data migration, Re-training,
  Contract overlap, Knowledge transfer) must be appended when has_incumbent=True.

[Check 8 fail] win_conditions count out of [3, 5] or schema incomplete.
  Action: Re-run Step 6. WC-1 through WC-5 are emitted conditionally; verify the
  if/conditions all trigger. Trim if > 5 (combine similar conditions).

[Check 9 fail] Win conditions floating from evaluation factors.
  Failing: {list}. Action: Re-run Step 6 with explicit factor linking — every
  win_condition.condition string should mention a factor name OR map via ghost/pain.

[Check 10 fail] go_nogo_input drifts from Phase 1.9.
  Stored: {x}, Phase 1.9: {y}. Action: Re-read GO_NOGO_DECISION.json in Step 1;
  copy total_score and recommendation verbatim — do not re-compute or paraphrase.

[Check 11 fail] Buyer name contradicts CLIENT_INTELLIGENCE or anchor.
  Action: Re-read CLIENT_INTELLIGENCE.json.client_info.organization_name in Step 7
  and use it directly. Do not free-style buyer.name.

[Check 12 fail] Row-cap, truncation, or mojibake.
  Action: Ensure encoding='utf-8' on every write, ensure_ascii=False on json.dump.
  Pain-map and switching-cost table cells must not be truncated with [:120]/[:80]
  in deliverable strings — those are presentation caps, not slice notation literals.

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input:**

```
COMPETITIVE_POSITION.json scenario:
  incumbent.name = "Tyler Technologies" (named).
  ghost_strategy = [] (empty — Step 3 was skipped).
  pain_map[0].evaluation_factor = null (no factor mapping).
  switching_costs = [] (incumbent present but no costs).
  win_conditions = [{id: "WC-1", condition: "Win the bid"}] — count=1, generic, no factor tie.
  go_nogo_input.total_score = 70, recommendation = "GO".
  GO_NOGO_DECISION.json.overall_score = 62 (mismatch).

COMPETITIVE_POSITION.md scenario:
  Size = 2,500 bytes (under 3,072 threshold).
  Under ## Ghost Strategy: "Tyler Technologies' batch transfer is slow..." (proper noun
    leaked into ghost statement body).
  buyer.name = "City of Salem" but CLIENT_INTELLIGENCE says "Oregon Secretary of State".
```

Verifier MUST detect: Check 1 (under-size MD), Check 3 (incumbent + zero ghosts), Check 5 (proper noun in ghost section), Check 6 (null factor mapping), Check 7 (no switching costs), Check 8 (count=1 < 3 + no schema), Check 10 (score drift), Check 11 (buyer contradicts intel). Disposition: FAIL.

**Known-good input:**

```
COMPETITIVE_POSITION.md: 4,800 bytes, well-formed sections.
COMPETITIVE_POSITION.json scenario:
  incumbent.name = "Tyler Technologies".
  ghost_strategy has 3 entries, each with incumbent_weakness + ghost_phrasing free
    of "Tyler" (e.g., "Some legacy systems require batch-only data transfer").
  pain_map has 5 entries, every entry tied to a factor.
  switching_costs has 4 entries (the canonical defaults).
  win_conditions has 5 entries: WC-1 references top factor "Technical Approach";
    WC-2 references neutralizing incumbency; WC-3 past performance; WC-4 risk;
    WC-5 pricing.
  go_nogo_input.total_score = 62 (matches Phase 1.9).
  buyer.name = "Oregon Secretary of State, Audits Division" (matches intel + anchor).
  COMPETITIVE_POSITION.md ## Ghost Strategy has zero "Tyler" mentions outside the
    explicit "Underlying competitor weakness (internal only):" lines.
```

Verifier MUST PASS all checks. Disposition: PASS.

## Hard Rules

1. NEVER claim COMPETITIVE_POSITION files are missing without `Glob` verification first.
2. NEVER accept ghost statements containing the incumbent proper noun in the ghost-phrased body — ghost strategy is by definition unnamed.
3. NEVER accept buyer/incumbent/score drift between this file and its upstream sources (CLIENT_INTELLIGENCE, GO_NOGO_DECISION, jurisdiction_anchor) — Stage 1 ends here and everything downstream relies on consistency.
4. NEVER accept win conditions that don't trace to evaluation factors — un-tethered conditions are aspirational, not strategic.
5. Every finding must cite a specific JSON path or markdown line + offending value + expected value.
6. On FAIL, return corrective instructions targeting the specific Step number (Step 3 ghosts, Step 4 pain_map, Step 5 switching_costs, Step 6 win_conditions, Step 7 outputs) for surgical repair.
