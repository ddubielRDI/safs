---
name: verifier-phase1.6-evaluation-win
expert-role: Evaluation Criteria Integrity Verifier
purpose: phase-boundary verifier for phase1.6-evaluation-win (factor count, weight math, citations, selection method)
created: 2026-05-20
---

# Verifier — Phase 1.6 Evaluation Criteria

## When this runs

After phase1.6-evaluation-win reports done, BEFORE phase1.7-compliance-win is invoked. Evaluation criteria drive bid structure, page allocation, and win-theme placement — incorrect weights cascade into mis-sized sections and lost points.

## Inputs (read in this order)

1. `{folder}/shared/EVALUATION_CRITERIA.json` — primary output under verification
2. `{folder}/shared/domain-context.json` — for jurisdiction cross-check
3. `{folder}/flattened/*.md` — RFP text for citation-grounding spot checks

## Verification Checks

### Check 1 — File exists and is valid JSON, structurally complete

**Criterion:** `shared/EVALUATION_CRITERIA.json` exists, parses as valid JSON, contains top-level keys: `extracted_at`, `selection_method`, `evaluation_factors`, `total_weight`, `recommendations`, `bid_structure_guidance`, `section_order_recommendations`. File size >= 500 bytes.

**Pass:** All keys present, JSON parses, size threshold met.

**Fail:** File absent, parse error, any required top-level key missing, or size < 500 bytes.

**Evidence to cite:** Missing key path or parse-error message; actual file size.

---

### Check 2 — At least 3 evaluation factors detected

**Criterion:** `len(evaluation_factors) >= 3`. Real RFPs always specify at least Technical, Past Performance, and Price (or equivalents). Fewer than 3 factors indicates extraction failure or a stub RFP.

**Pass:** `evaluation_factors` length >= 3.

**Concern:** Exactly 2 factors — log advisory; may be a simple LPTA RFP. Allow but flag.

**Fail:** `evaluation_factors` length < 2 (extraction failed).

**Evidence to cite:** Actual length of `evaluation_factors` array and list of factor names.

---

### Check 3 — Every factor has required schema keys

**Criterion:** Every entry in `evaluation_factors` has these keys: `factor_id`, `name`, `points`, `weight_normalized`, `detected_in_rfp`. The `factor_id` MUST match pattern `^EVAL-\d{2}$` (e.g., `EVAL-01`).

**Pass:** All factors have all required keys; all factor_ids match the pattern.

**Fail:** Any factor missing a required key OR any factor_id with invalid pattern.

**Evidence to cite:** Index of offending factor + which key is missing or malformed.

---

### Check 4 — Weights sum to 100 within ±10 tolerance

**Criterion:** `sum(f["weight_normalized"] for f in evaluation_factors)` must be in `[90, 110]`. The phase normalizes to 100, but rounding + inferred-weight blending allows small drift; ±10 is the published tolerance.

**Pass:** Sum is in `[90, 110]`.

**Concern:** Sum is in `[85, 89]` or `[111, 115]` — log advisory; downstream page allocation will be slightly off but recoverable.

**Fail:** Sum is < 85 or > 115 (normalize step skipped or factor weights nonsense).

**Evidence to cite:** Print actual sum value and per-factor weight breakdown.

---

### Check 5 — Each factor has explicit weight source OR is marked inferred

**Criterion:** Every factor MUST have EITHER `weight_explicit` populated (non-null integer) OR `weight_inferred` populated (non-null integer) — exactly one of these two. Both null = factor weight is fictional (a worse-than-inferred state). Both populated = phase logic bug.

**Pass:** Every factor has exactly one of `weight_explicit` or `weight_inferred` populated.

**Fail:** Any factor with BOTH null OR BOTH populated.

**Evidence to cite:** Factor index, factor name, both weight values (explicit and inferred).

---

### Check 6 — Selection method is one of allowed values

**Criterion:** `selection_method.type` is one of `{"Best Value", "LPTA", "Quality-Based", "Trade-off"}` (note: "Trade-off" is a synonym used by some RFPs but the phase canonicalizes to "Best Value"; allow "Trade-off" defensively).

**Pass:** `selection_method.type` ∈ allowed set.

**Fail:** Value is null, free-text, or outside the allowed set.

**Evidence to cite:** Actual `selection_method.type` value and the allowed set.

---

### Check 7 — Every factor has rfp_section citation OR is marked inferred

**Criterion:** For every factor where `detected_in_rfp == true`, there must be evidence that the factor name appears in the actual RFP text. Spot-check: grep `flattened/*.md` for the factor name (case-insensitive). At least 80% of `detected_in_rfp==true` factors must grep-verify.

**Pass:** >= 80% of detected factors grep-verify in flattened text.

**Concern:** 60-79% verify — log advisory.

**Fail:** < 60% verify (factor names hallucinated; extraction did not actually read the RFP).

**Evidence to cite:** List each factor, grep result count, verification status.

---

### Check 8 — Jurisdiction tokens consistent with state of issuance

**Criterion:** Cross-reference `domain-context.json.jurisdiction_anchor.state.value`. If `EVALUATION_CRITERIA.json` mentions a state name in any free-text field (e.g., `selection_method.description`, factor names, recommendations), it must match the anchor state. This catches the MARS-style drift where 1.5 said Oregon but 1.6 free-styled a different state.

**Pass:** No state mentioned, OR mentioned state matches anchor.

**Fail:** State mentioned that contradicts the anchor (e.g., anchor=Oregon but evaluation text references Alaska/Texas).

**Evidence to cite:** Anchor state, contradictory state token, field path where the contradiction appeared.

---

### Check 9 — Bid structure guidance maps to 3 standard bid sections

**Criterion:** `bid_structure_guidance` dict has keys for all of: `title-page.md`, `solution.md`, `timeline.md`. Each value has `sections`, `total_weight`, `page_allocation`, `guidance`.

**Pass:** All 3 section keys present with full sub-schema.

**Fail:** Any of the 3 keys missing or missing sub-fields.

**Evidence to cite:** List actual keys in `bid_structure_guidance`.

---

### Check 10 — Anti-truncation / anti-row-cap discipline

**Criterion:** No string value in the JSON contains the literal sequence `_Showing N of M_` (or any `_Showing \d+ of \d+_` pattern). No string value shows mid-word truncation (ends with `[:` followed by digits and `]`). No mojibake (`â€"`, `Ã©`, etc.).

**Pass:** Zero matches for any pattern.

**Fail:** Any match in any string value.

**Evidence to cite:** Path + value of the offending string.

---

## Disposition Logic

- **PASS:** Checks 1, 3, 5, 6, 9, 10 pass AND Check 2 not in advisory band AND Checks 4, 7, 8 meet threshold.
- **CONCERN:** Check 2 has exactly 2 factors, OR Check 4 in `[85, 89]` or `[111, 115]`, OR Check 7 in 60-79% band. Log + continue.
- **FAIL:** Any of Checks 1, 3, 5, 6, 8, 9, 10 fail OR Check 2 with < 2 factors OR Check 4 outside `[85, 115]` OR Check 7 < 60%.

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase1.6-evaluation-win with the following targeted corrections:

[Check 1 fail] EVALUATION_CRITERIA.json missing or invalid.
  Action: Re-run from Step 1. Verify flattened/*.md and domain-context.json exist first.

[Check 2 fail] Fewer than 2 evaluation factors detected.
  Action: Audit Step 4 (extract_factors). Likely cause: SECTION_PATTERNS in Step 2 did
  not match the RFP's evaluation section header phrasing. Add the missing phrasing to
  SECTION_PATTERNS, then re-run.

[Check 3 fail] Factor schema missing keys.
  Offending factors: {list}. Action: Ensure Step 7 builds each factor dict with the full
  schema. The factor_id pattern is "EVAL-{N:02d}" — verify the zfill.

[Check 4 fail] Weights sum {N} outside [85, 115].
  Action: Re-run Step 6 normalize_weights. Verify the total computation divides by
  sum-of-detected-weights, not by 100. Inferred weights must come from Step 5
  defaults dict, not be left null.

[Check 5 fail] Factors with ambiguous weight source.
  Offending factors: {list}. Action: For each factor, set EXACTLY ONE of weight_explicit
  or weight_inferred. If detected from RFP text → weight_explicit. If from
  DEFAULT_WEIGHTS dict → weight_inferred.

[Check 6 fail] selection_method.type out of allowed set.
  Actual: {value}. Action: Re-run Step 3 detect_selection_method. SELECTION_METHODS dict
  has only LPTA, Best Value, Quality-Based — extend if a new RFP variant appeared.

[Check 7 fail] Factor names don't grep-verify in flattened text.
  Failing factors: {list}. Action: The factors are likely hallucinated from
  COMMON_FACTORS without verifying RFP mentions. Add a grep-presence check to Step 4
  before appending each factor.

[Check 8 fail] State token contradicts jurisdiction_anchor.
  Anchor state: {x}; contradictory token: {y} at {path}. Action: This is a cross-stage
  drift — Phase 1.6 must not introduce new state names. Re-read domain-context.json
  jurisdiction_anchor and ensure free-text uses the same state.

[Check 9 fail] bid_structure_guidance missing standard section keys.
  Action: Re-run Step 8b generate_bid_structure_guidance — the dict is seeded with all
  3 keys; ensure no key was renamed.

[Check 10 fail] Row-cap, truncation, or mojibake detected.
  Action: Audit Step 7 write_json — ensure encoding='utf-8' and ensure_ascii=False.
  No [:N] slicing on deliverable strings (factor names, descriptions, rationales).

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input:**

```
EVALUATION_CRITERIA.json scenario:
  evaluation_factors has 4 entries.
  Sum of weight_normalized = 75 (factors did not normalize).
  Factor "Technical Approach" has both weight_explicit=40 AND weight_inferred=40.
  selection_method.type = "Hybrid" (not in allowed set).
  Factor "Past Performance" appears nowhere in flattened text (hallucinated).
  Free text in recommendations mentions "Alaska procurement code" but anchor=Oregon.
```

Verifier MUST detect: Check 4 (sum=75 < 85), Check 5 (both populated), Check 6 (Hybrid invalid), Check 7 (Past Performance not in RFP), Check 8 (Alaska in Oregon RFP). Disposition: FAIL.

**Known-good input:**

```
EVALUATION_CRITERIA.json scenario:
  evaluation_factors has 5 entries: Technical Approach (40), Past Performance (25),
    Price/Cost (20), Management (10), Corporate Experience (5). Sum = 100.
  Each factor has EXACTLY ONE of weight_explicit/weight_inferred populated.
  selection_method.type = "Best Value".
  All 5 factor names grep-verify in flattened/*.md.
  No state tokens in free text (or they match anchor=Oregon).
  bid_structure_guidance has all 3 standard keys.
```

Verifier MUST PASS all checks. Disposition: PASS.

## Hard Rules

1. NEVER claim EVALUATION_CRITERIA.json is missing without `Glob` verification first.
2. NEVER accept factor names without spot-grep-verifying them in flattened text — hallucinated factors are the most common Phase 1.6 failure.
3. NEVER ignore cross-stage state-token drift; jurisdiction integrity is enforced at every Stage 1 phase boundary.
4. Every finding must cite a specific field path + value (e.g., `evaluation_factors[2].weight_inferred = null AND weight_explicit = null`).
5. On FAIL, return corrective instructions with the specific factor index or sum value so the phase agent can target the exact repair without re-running the entire pipeline.
