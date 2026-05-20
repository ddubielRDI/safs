---
name: verifier-phase7d-scoring-win
expert-role: Win Scorecard Integrity Verifier
purpose: phase-boundary verifier for phase7d-scoring-win (scorecard schema, factor decomposition, evidence per factor, scalar win_probability per V5-F5, advisory disposition)
created: 2026-05-20
---

# Verifier — Phase 7d Bid Scoring Model

## When this runs

After phase7d-scoring-win reports done. This verifier is ADVISORY ONLY — its result does NOT gate downstream bid generation. The phase's own output is advisory (informs go/no-go judgement, does not block) and this verifier inherits that disposition: PASS or FAIL is logged but bid authoring proceeds.

## Inputs (read in this order)

1. `{folder}/shared/WIN_SCORECARD.json` — primary output under verification
2. `{folder}/shared/COMPLIANCE_MATRIX.json` — cross-check source for compliance factor + disqualifiers
3. `{folder}/shared/PERSONA_COVERAGE.json` — cross-check source for evaluator-alignment evidence
4. `{folder}/shared/REQUIREMENT_RISKS.json` — cross-check source for risk_mitigation factor evidence
5. `{folder}/shared/requirements-normalized.json` — cross-check source for alignment factor's `total_requirements` value
6. Any prior verifier report at `{folder}/shared/validation/verifier-phase7d.json` (if a retry run)

## Verification Checks

### Check 1 — File exists, parses as valid JSON, size above floor

**Criterion:** `shared/WIN_SCORECARD.json` exists AND parses as valid JSON. File size > 200 bytes (phase contract floor); realistic output will be >> 1 KB.

**Pass:** File present, JSON parses, size > 200 bytes.

**Fail:** File absent, JSON parse error, or size <= 200 bytes.

**Evidence to cite:** Actual file size and JSON parse status.

**Hard-rule reminder:** NEVER claim file is missing without `Glob` verification first.

---

### Check 2 — UTF-8 round-trip with no mojibake

**Criterion:** File opens cleanly with `encoding='utf-8'`. No mojibake sequences (`â€™`, `â€œ`, `â€`, `Ã©`, `Â`, `�`) in any string field (disqualifier details, recommendations, scoring_model descriptions). JSON written with `ensure_ascii=False`.

**Pass:** UTF-8 read succeeds; zero mojibake matches.

**Fail:** Any mojibake sequence found or UnicodeDecodeError on utf-8 open.

**Evidence to cite:** Grep counts per pattern.

---

### Check 3 — Schema fidelity: top-level keys present and well-typed

**Criterion:** `WIN_SCORECARD.json` top-level keys include all of: `calculated_at`, `win_probability`, `win_probability_detail`, `scores`, `disqualifiers`, `scoring_model`, `recommendations`. `disqualifiers` is a list (may be empty). `scores`, `scoring_model`, `win_probability_detail` are dicts. `recommendations` is a list.

**Pass:** All required keys present and well-typed.

**Fail:** Any required key absent; any field with wrong container type.

**Evidence to cite:** Actual top-level keys list; identify missing or wrong-typed key.

---

### Check 4 — `win_probability` is a SCALAR (V5-F5 fix)

**Criterion:** `type(win_scorecard["win_probability"])` is `int` or `float` (NOT a dict). The value is in [0, 100]. The full dict is preserved under `win_probability_detail` (which IS a dict with keys `probability`, `confidence`, `weighted_score`).

This check is critical: SVA-6 downstream reads `win_scorecard.get("win_probability", 0)` and expects a number. SAFS memory documents this as the V5-F5 fix — a prior regression where `win_probability` was a dict broke SVA-6 arithmetic.

**Pass:** `win_probability` is numeric and in [0, 100]; `win_probability_detail` is a dict with the 3 required keys.

**Fail:** `win_probability` is a dict, string, null, or out of range; OR `win_probability_detail` is absent or missing required sub-keys.

**Evidence to cite:** `type(win_probability) = <type>`, actual value; `win_probability_detail keys = [...]`.

---

### Check 5 — All 5 scoring factors decomposed in `scores` with score + evidence

**Criterion:** `scores` dict contains all 5 keys: `alignment`, `value`, `risk_mitigation`, `compliance`, `presentation`. Every entry has a `score` field (numeric, 0 <= score <= 100). Every entry has at least one evidence-bearing field beyond `score`:
- `alignment` → `total_requirements`, `mapped_to_specs`, `mandatory_items`, `mandatory_addressed`
- `value` → `has_ai_savings`, `has_roi`, `has_efficiency` (or `note` if estimation absent)
- `risk_mitigation` → `high_risk_count`, `high_risk_ratio`, `mitigation_rate` (or `note` if assessment absent)
- `compliance` → `gate_passed`, `coverage_percentage`, `gap_count`
- `presentation` → `required_docs_present`, `total_content_kb`

**Pass:** All 5 factors present, all have `score` in [0, 100], all have ≥ 1 evidence field as listed above.

**Concern:** A factor has only the `note` fallback (e.g., estimation or risk assessment file absent) — log advisory but do not FAIL.

**Fail:** Any of the 5 factors absent; any `score` non-numeric or out of [0, 100]; any factor with no evidence field at all.

**Evidence to cite:** Print `{k: list(v.keys()) for k, v in scores.items()}` and the actual score per factor.

---

### Check 6 — Factor evidence cross-references actual source data

**Criterion:** Evidence values in `scores` must match the source files they were derived from:
- `scores.alignment.total_requirements` == `len(requirements-normalized.requirements)` (tolerance 0)
- `scores.alignment.mandatory_items` == `len(COMPLIANCE_MATRIX.mandatory_items)` (tolerance 0)
- `scores.compliance.gate_passed` == `COMPLIANCE_MATRIX.gate_status.passed`
- `scores.compliance.coverage_percentage` matches `COMPLIANCE_MATRIX.gate_status.coverage_percentage` (tolerance ±0.5)
- `scores.risk_mitigation.high_risk_count` matches `REQUIREMENT_RISKS.summary.high_risk` (tolerance 0) when REQUIREMENT_RISKS is present

**Pass:** All cross-references agree within their tolerances.

**Concern:** A source file (REQUIREMENT_RISKS, etc.) is absent and the corresponding score uses the documented fallback (`note` field). Log advisory.

**Fail:** Any cross-reference disagrees beyond tolerance — indicates the scoring functions read stale or inconsistent data.

**Evidence to cite:** Side-by-side comparison of scorecard value vs source value for each mismatch.

---

### Check 7 — `scoring_model` weights are present and sum to ~1.0

**Criterion:** `scoring_model` dict contains entries for all 5 factors (alignment, value, risk_mitigation, compliance, presentation), each with a `weight` numeric field and a `description` string field. Sum of weights must be in [0.95, 1.05].

Computed: 0.30 + 0.25 + 0.20 + 0.15 + 0.10 = 1.00 (the canonical default).

**Pass:** All 5 model entries present; weight sum in [0.95, 1.05]; every entry has a non-empty description.

**Fail:** Any model entry missing; weight sum outside band; any description empty.

**Evidence to cite:** Per-factor weight + computed sum.

---

### Check 8 — Disqualifiers well-formed when present; recommendations correlate with scores

**Criterion:**
- For each entry in `disqualifiers`: `type` (string), `severity` (must be `CRITICAL`, `HIGH`, `MEDIUM`, or `LOW`), `details` (non-empty string), `items` (list, may be empty).
- If `len(disqualifiers) > 0` AND any has `severity == "CRITICAL"`, then `win_probability <= 15` (the phase contracts this in Step 8's calculate_win_probability — critical disqualifier => probability=10).
- For each factor in `scores` with `score < 80`, there should be a corresponding entry in `recommendations` referencing that factor (per phase Step 9 generate_win_recommendations() which appends a HIGH-priority rec for the lowest-scoring area when < 80).

**Pass:** All disqualifiers well-formed with valid severity vocabulary; if critical disqualifier present, win_probability <= 15; recommendations cover factors with score < 80.

**Concern:** A factor with `score < 80` has no matching recommendation but is not the lowest-scoring factor (the phase only appends one HIGH-priority rec — for the minimum). Log advisory.

**Fail:** Any disqualifier missing a required field or with invalid severity; critical disqualifier present but win_probability > 15; the lowest-scoring factor < 80 has no matching recommendation.

**Evidence to cite:** Per-disqualifier index + missing/invalid field; cite the critical→probability inconsistency directly (e.g., `disqualifiers[0].severity = "CRITICAL" but win_probability = 67`).

---

### Check 9 — Advisory disposition acknowledged: this verifier does NOT gate bid generation

**Criterion:** The verifier output must explicitly state `disposition_advisory_only: true` in its report. The phase contract for phase7d declares the output is advisory (informs go/no-go, does not block); the verifier must inherit this and never raise a FAIL that blocks downstream phases.

**Pass:** Verifier report contains `disposition_advisory_only: true`.

**Fail:** Not a content check — this is a meta-check; if the verifier report omits the advisory flag, fix the verifier wrapper, not phase7d.

**Evidence to cite:** Statement of advisory mode in verifier report header.

---

### Check 10 — No `[:N]` truncation and no row-cap notices

**Criterion:** Grep serialized JSON for `\[:[0-9]+\]` — must return 0 matches. Grep for `_Showing ` — must return 0 matches. Note: phase7d Step 7 uses `gaps[:3]` for disqualifier items list and `g["text"][:50]` for truncation; verify these truncations live in code only and do not surface as visible truncation patterns in the deliverable strings (e.g., a disqualifier item string ending in `...` mid-word).

**Pass:** All greps return 0 matches; no mid-word terminal-ellipsis strings.

**Fail:** Any `[:N]` slice notation literal or row-cap notice in serialized output; mid-word truncation in disqualifier `items`.

**Evidence to cite:** Grep counts; example offending string if found.

---

## Disposition Logic

- **PASS (advisory):** Checks 1, 2, 3, 4, 5, 7, 8, 9, 10 all pass AND Check 6 not in FAIL band.
- **CONCERN (advisory):** Check 5 advisory (fallback `note` used) OR Check 6 advisory (source file absent) OR Check 8 advisory (sub-threshold factor not in recs but not the minimum). Log + continue.
- **FAIL (advisory — log but do not block):** Any of Checks 1, 2, 3, 4, 5, 7, 10 fail OR Check 6 fails (data mismatch) OR Check 8 fails (malformed disqualifier or critical/probability inconsistency).

**IMPORTANT:** Per the phase contract ("advisory output (does not gate bid generation)"), even a FAIL disposition here does NOT block downstream phases. The verifier logs the failure and recommends remediation for the next pipeline run, but bid authoring proceeds. The verifier report must carry `disposition_advisory_only: true`.

## Corrective Instructions on FAIL

```
VERIFIER FAIL (ADVISORY — bid generation will continue) — Re-run phase7d-scoring with
the following targeted corrections:

[Check 1 fail] WIN_SCORECARD.json missing or undersized.
  Action: Re-run Step 9 (Write Output). Verify Steps 2-8 all populated their dicts
  (alignment, value, risk_mitigation, compliance_score, presentation, disqualifiers,
  win_probability) before assembly. A stub file usually means an exception in
  generate_win_recommendations().

[Check 2 fail] Mojibake in disqualifier details or recommendations strings.
  Action: All open() must use encoding='utf-8', all json.dump must use ensure_ascii=False.
  Re-emit from in-memory dict — do NOT in-place fix.

[Check 3 fail] WIN_SCORECARD.json missing top-level key {key}.
  Action: Step 9 builds win_scorecard with 7 top-level keys — verify all are assigned
  before write_json. calculated_at often dropped if datetime import is missing.

[Check 4 fail] win_probability is a dict (regression — V5-F5 fix reverted).
  Action: Per SAFS memory and the phase Step 9 comment "V5-F5 fix: flatten
  win_probability to a top-level scalar for SVA-6 compatibility", win_probability MUST
  be the scalar `win_probability_detail.get("probability", 0)`, NOT the full dict.
  Restore the two-key pattern: win_probability = scalar, win_probability_detail = dict.

[Check 5 fail] scores missing factor {K} or evidence field absent.
  Action: Steps 2-6 each return a dict with required evidence fields. If a factor's
  source file is missing, the phase falls back to a {score, note} dict — verify the
  fallback is intentional. Never silently drop a factor from scores.

[Check 6 fail] scorecard evidence disagrees with source — {field path}: scorecard={X}
  vs source={Y}.
  Action: The scoring functions in Steps 2-6 read source files via read_json. Verify
  no stale in-memory cache is being read — each function should re-read its source
  files. Confirm requirements-normalized.json hasn't been re-written between alignment
  scoring and write_json.

[Check 7 fail] SCORING_MODEL weight sum {N} outside [0.95, 1.05].
  Action: SCORING_MODEL is defined at the top of Step 1. Weights are literals:
  alignment=0.30, value=0.25, risk_mitigation=0.20, compliance=0.15, presentation=0.10
  = 1.00. If a value was edited, restore canonical balance.

[Check 8 fail] Disqualifier malformed OR critical disqualifier present but
  win_probability = {N} > 15.
  Action: calculate_win_probability() in Step 8 short-circuits to probability=10 when
  any critical disqualifier exists. Verify the short-circuit is reachable and that
  win_probability_detail.probability is what flows into the scalar win_probability.

[Check 10 fail] Truncation pattern in serialized scorecard.
  Action: Audit Step 7's `gaps[:3]` and `g["text"][:50]` truncations — these affect
  the disqualifier `items` list and item strings. Either drop the [:3] cap (emit all
  gap items) OR rename the field to make the truncation explicit (e.g., items_sample
  rather than items). Per SAFS memory, never silent [:N] in deliverables.

Max 1 retry. On second FAIL, escalate to human with this verifier report (but bid
generation continues regardless — this verifier is advisory).
```

## Self-Test Cases

**Known-bad input:**

```
WIN_SCORECARD.json scenario: win_probability = {"probability": 67, "confidence": "MEDIUM",
  "weighted_score": 74.4} (full dict — V5-F5 regression).
  scores missing "presentation" key entirely (Step 6 raised, was silently swallowed).
  scores.alignment.total_requirements = 200 but requirements-normalized has 247.
  disqualifiers = [{type: "page_limit_exceeded", severity: "BLOCKING", ...}] (invalid severity).
  SCORING_MODEL.value.weight = 0.50 (others summing to 1.25 total).
```

Verifier MUST detect: Check 4 (win_probability is dict), Check 5 (presentation missing), Check 6 (200 vs 247), Check 8 (severity "BLOCKING" not in vocabulary), Check 7 (weight sum 1.25). Disposition: FAIL (advisory).

**Known-good input:**

```
WIN_SCORECARD.json scenario: win_probability = 72 (scalar).
  win_probability_detail = {probability: 72, confidence: "MEDIUM", weighted_score: 80.0}.
  scores has all 5 factors, every score in [0, 100], every entry has ≥ 1 evidence field.
  scores.alignment.total_requirements = 247 (matches normalized).
  scores.compliance.gate_passed = true, matches COMPLIANCE_MATRIX.gate_status.passed.
  SCORING_MODEL weight sum = 1.00.
  disqualifiers = [] (no critical issues).
  recommendations = [1 entry — HIGH priority for risk_mitigation at score 68 (lowest)].
  No mojibake, no [:N] truncation in serialized output.
```

Verifier MUST PASS all checks. Disposition: PASS (advisory).

## Hard Rules (inherited from verifier-design principles)

1. NEVER claim WIN_SCORECARD.json is missing without `Glob` verification first.
2. NEVER raise a FAIL that blocks downstream — this phase is advisory by contract; the verifier inherits that disposition. Report failures, log them, but do not gate bid generation.
3. The V5-F5 scalar/dict split for `win_probability` is non-negotiable — SAFS memory documents this as a downstream-breaking regression. Always verify `type(win_probability)` is numeric.
4. NEVER allow `[:N]` truncation patterns to surface in disqualifier item strings or recommendation strings (per SAFS memory: three regressions).
5. Cross-reference checks (Check 6) catch stale-cache bugs — if scorecard numbers disagree with source files, the scoring function is reading from memory before the source was updated; always FAIL.
6. Every finding must cite a specific field path + value or count (e.g., `win_probability type = dict, expected numeric`).
7. On FAIL, return corrective instructions with the specific phase step (e.g., "Step 9 V5-F5 fix") so the phase agent can target the exact line responsible.
