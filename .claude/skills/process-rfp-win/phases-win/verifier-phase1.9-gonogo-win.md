---
name: verifier-phase1.9-gonogo-win
expert-role: Go/No-Go Decision Integrity Verifier
purpose: phase-boundary verifier for phase1.9-gonogo-win (recommendation field, 7-area scoring, weight math, threshold logic, evidence)
created: 2026-05-20
---

# Verifier — Phase 1.9 Go/No-Go Decision

## When this runs

After phase1.9-gonogo-win reports done, BEFORE phase1.95-intel-win. The recommendation gates Phase 1.95's conditional execution. The decision JSON also feeds Phase 1.97 (competitive position), Phase 8.0 (positioning), and the executive summary. A faulty score here mis-directs whether the entire bid pipeline should run.

## Inputs (read in this order)

1. `{folder}/shared/GO_NOGO_DECISION.json` — primary output under verification
2. `{folder}/shared/domain-context.json` — for jurisdiction cross-check on rationale text
3. `{folder}/shared/EVALUATION_CRITERIA.json` — for criteria-alignment spot check
4. `{folder}/shared/COMPLIANCE_MATRIX.json` — for resource-availability rationale spot check
5. `{folder}/flattened/*.md` — RFP text for citation grounding

## Verification Checks

### Check 1 — File exists and is valid JSON, structurally complete

**Criterion:** `shared/GO_NOGO_DECISION.json` exists, parses as valid JSON, top-level keys include: `phase`, `recommendation`, `overall_score`, `threshold`, `assessment_areas`, `overall_risks`, `overall_mitigations`, `user_decision_required`. File size >= 1,024 bytes.

**Pass:** All required keys present, parses, size >= 1,024.

**Fail:** File absent, parse error, key missing, or size < 1,024.

**Evidence to cite:** Missing key path or actual file size.

---

### Check 2 — Field is `recommendation` NOT `decision`

**Criterion:** Top-level key MUST be `recommendation` (not `decision`). This is a long-standing SAFS convention documented in MEMORY.md. Grep for top-level `"decision":` returns 0 hits at the JSON-root level. `"recommendation":` returns exactly 1 hit at top-level.

**Pass:** `recommendation` key present at top-level; no top-level `decision` key.

**Fail:** `decision` key present at top-level (regression to deprecated schema) OR `recommendation` missing.

**Evidence to cite:** Grep results for both keys at top-level.

---

### Check 3 — `overall_score` is integer 0-100

**Criterion:** `overall_score` is numeric (preferably int after `round()`), >= 0, <= 100.

**Pass:** Value in `[0, 100]` and numeric.

**Fail:** Value outside range, null, or non-numeric (e.g., string "75/100").

**Evidence to cite:** Actual value + Python type.

---

### Check 4 — All 7 assessment areas present with exact names

**Criterion:** `assessment_areas` is a list of 7 entries with these exact `name` values (case-sensitive): `Strategic Fit`, `Technical Capability`, `Competitive Position`, `Resource Availability`, `Financial Viability`, `Risk Assessment`, `Win Probability`.

**Pass:** All 7 names present, no duplicates, no extras.

**Fail:** Any name missing, misspelled (e.g., "Strategic-Fit"), duplicated, or extra area added.

**Evidence to cite:** List of actual names found vs expected set.

---

### Check 5 — Weights match canonical distribution

**Criterion:** Each area has `weight` matching:
- Strategic Fit: 0.15
- Technical Capability: 0.25
- Competitive Position: 0.20
- Resource Availability: 0.15
- Financial Viability: 0.10
- Risk Assessment: 0.10
- Win Probability: 0.05

Sum MUST equal 1.0 (±0.001).

**Pass:** All 7 weights match canonical AND sum to 1.0.

**Fail:** Any weight wrong or sum != 1.0.

**Evidence to cite:** Per-area weight values + sum.

---

### Check 6 — Each area has evidence citations

**Criterion:** Every area MUST have a non-empty `evidence` list (list of strings, each >= 20 chars). Empty evidence indicates the LLM scored without grounding.

**Pass:** All 7 areas have `evidence` length >= 1 AND each entry >= 20 chars.

**Concern:** 1 of 7 areas has 1 short evidence entry — log advisory.

**Fail:** Any area with empty `evidence` OR all-entries-short evidence (< 20 chars each).

**Evidence to cite:** Per-area evidence count + average length.

---

### Check 7 — `overall_score` recomputes from area scores × weights

**Criterion:** Recompute: `expected = round(sum(area["score"] * area["weight"] for area in assessment_areas))`. Must equal `overall_score` exactly.

**Pass:** Recomputed value equals stored `overall_score`.

**Fail:** Mismatch (LLM hand-wrote a different number, or weight/score field was edited post-compute).

**Evidence to cite:** Per-area score, weight, weighted contribution; total recomputed; stored value; delta.

---

### Check 8 — Recommendation follows threshold rules

**Criterion:**
- `overall_score >= 50` → recommendation = `"GO"`
- `overall_score in [40, 49]` → recommendation = `"CONDITIONAL"`
- `overall_score < 40` → recommendation = `"NO_GO"` (underscore form, not "NO-GO")

The `threshold` block in the JSON MUST contain `go: 50, conditional: 40, no_go: 0` (or equivalent — the integers may live in different sub-key names but the threshold values must be present).

**Pass:** Threshold + recommendation + score all consistent.

**Fail:** Mismatch (e.g., score=45 but recommendation="GO", or recommendation="NO-GO" with hyphen instead of "NO_GO").

**Evidence to cite:** Score + recommendation + threshold values + expected recommendation per rules.

---

### Check 9 — `risk_factors` / `overall_risks` enumerated for non-GO recommendations

**Criterion:** If recommendation is `"CONDITIONAL"` or `"NO_GO"`, `overall_risks` MUST be a non-empty list with at least 2 entries. Each entry should be a string or dict with descriptive content (>= 20 chars). For `"GO"` recommendations, an empty `overall_risks` list is allowed but unusual — log a CONCERN if zero risks on a GO.

**Pass:** Non-GO has >= 2 substantive risks; GO has any.

**Concern:** GO with 0 risks — log advisory (unrealistic).

**Fail:** CONDITIONAL or NO_GO with empty or single-entry overall_risks.

**Evidence to cite:** Recommendation + risks list length + risk entries.

---

### Check 10 — Rationales don't reference internal file names

**Criterion:** Per the phase's EVIDENCE REQUIREMENTS rule, no `rationale`, `evidence`, `risks`, or `mitigations` string should contain internal file references like `company-profile.json`, `EVALUATION_CRITERIA.json`, `Past_Projects.md`, `COMPLIANCE_MATRIX.json`, etc. Use natural-language descriptions instead.

**Pass:** Zero file-name references in narrative fields.

**Fail:** Any file-name reference in rationale/evidence/risks/mitigations text.

**Evidence to cite:** Area name + field + offending string snippet.

---

### Check 11 — Cross-stage jurisdiction consistency in rationale

**Criterion:** If any area's `rationale` mentions a state name, it MUST match `domain-context.json.jurisdiction_anchor.state.value`. This catches the MARS-style drift where 1.5 said Oregon but 1.9 free-styled "Alaska Department of Administration" into the Strategic Fit rationale.

**Pass:** No state mentioned OR state matches anchor.

**Fail:** State mentioned contradicts anchor.

**Evidence to cite:** Anchor state + contradictory state token + area + field + snippet.

---

### Check 12 — `services` was loaded as DICT (not list) — implicit via rationale check

**Criterion:** Spot-check Technical Capability rationale. If it mentions specific service categories that exist in `company-profile.json.services` (a DICT keyed by category), the rationale should reference services natively (without category-level brackets that would only appear if loaded as a list). This is a soft check — primarily a sanity proxy. Allow PASS if no service-related text exists.

**Pass:** Service references are natural OR no services referenced.

**Concern:** Service text suggests services was treated as a flat list (rare, hard to detect deterministically) — log advisory.

**Fail:** N/A — this check produces CONCERN at most.

**Evidence to cite:** Snippet of services-mentioning text.

---

### Check 13 — Anti-truncation / anti-row-cap / encoding integrity

**Criterion:** No string value contains `_Showing N of M_` notices, mid-word `[:N]` truncations, or mojibake (`â€"`, etc.).

**Pass:** Zero matches.

**Fail:** Any match.

**Evidence to cite:** Path + offending value.

---

## Disposition Logic

- **PASS:** Checks 1, 2, 3, 4, 5, 7, 8, 10, 11, 13 pass AND Check 6 not in advisory band AND Check 9 not in advisory band.
- **CONCERN:** Check 6 1-of-7 short OR Check 9 GO-with-zero-risks OR Check 12 sanity hint. Log + continue to Phase 1.95.
- **FAIL:** Any of Checks 1, 2, 3, 4, 5, 7, 8, 10, 11, 13 fail OR Check 6 multi-area empty evidence OR Check 9 non-GO with insufficient risks.

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase1.9-gonogo-win with the following targeted corrections:

[Check 1 fail] GO_NOGO_DECISION.json missing or under-size.
  Action: Re-run from Step 1. Verify all 4 Stage 1 inputs (domain-context, EVAL, COMPLIANCE,
  SUBMISSION) plus company-profile.json exist.

[Check 2 fail] Top-level key is "decision" not "recommendation".
  Action: Schema regression — rename the field in Step 5 write_json. SAFS convention is
  "recommendation"; "decision" is deprecated.

[Check 3 fail] overall_score out of range or non-numeric.
  Actual: {value}. Action: Re-run Step 3 compute. Ensure round() returns int and that
  the sum-of-weights is correct (sum to 1.0, not 100).

[Check 4 fail] Assessment area names wrong.
  Missing/misspelled: {list}. Action: Audit Step 2 — the area names are CASE-SENSITIVE
  and must match the exact 7 listed. Use a constant tuple to enforce.

[Check 5 fail] Weights don't match canonical distribution.
  Action: Re-run Step 3 with the canonical weights dict. Do not let LLM override weights.

[Check 6 fail] Areas with empty evidence.
  Failing areas: {list}. Action: Re-run Step 2 LLM assessment. Evidence is MANDATORY —
  the LLM must cite specific inputs. Reject any area dict with empty evidence and re-prompt.

[Check 7 fail] overall_score doesn't recompute from area scores × weights.
  Stored: {x}, Recomputed: {y}. Action: Audit Step 3 — sum loop must iterate ALL 7 areas
  exactly once. Verify round() applied once at the end, not per-area.

[Check 8 fail] Recommendation doesn't match threshold rules.
  Score: {x}, Recommendation: {y}, Expected: {z}. Action: Audit Step 4 — the if/elif chain
  is closed. NO_GO uses underscore form (not "NO-GO"); CONDITIONAL is exact spelling.

[Check 9 fail] Non-GO with insufficient risks.
  Recommendation: {x}, risks count: {y}. Action: Step 2 LLM assessment must populate risks
  for areas scoring < 70. Step 5 aggregates them — verify the extend loop runs.

[Check 10 fail] File names in rationale/evidence text.
  Offending: {list}. Action: Step 2 EVIDENCE REQUIREMENTS rule #2 — natural-language only.
  Re-prompt the LLM with explicit prohibition; reject draft area dicts containing *.json
  or *.md tokens in narrative fields.

[Check 11 fail] State token in rationale contradicts jurisdiction_anchor.
  Anchor: {x}, contradictory: {y} in area {z}. Action: Cross-stage drift. Re-read
  domain-context.json before generating each area. Add an explicit check after Step 2.

[Check 13 fail] Row-cap, truncation, or mojibake.
  Action: Ensure encoding='utf-8' on every write, ensure_ascii=False on json.dump.

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input:**

```
GO_NOGO_DECISION.json scenario:
  top-level key is "decision" (regression).
  assessment_areas has 6 entries — "Win Probability" missing.
  Strategic Fit weight is 0.20 (should be 0.15); sum of weights = 1.05.
  overall_score stored = 65; recompute from weights = 61 (delta -4).
  Recommendation = "GO" but score = 45 (should be CONDITIONAL).
  Technical Capability rationale = "Per company-profile.json services.data_and_ai,
    we have strong GIS capability" (file name leakage).
  Strategic Fit rationale mentions "Alaska Department of Administration" but
    jurisdiction_anchor.state = "Oregon".
  Strategic Fit evidence = [].
```

Verifier MUST detect: Check 2 (decision regression), Check 4 (Win Probability missing), Check 5 (weight wrong + sum wrong), Check 7 (recompute mismatch), Check 8 (threshold violation), Check 10 (file name leakage), Check 11 (state contradiction), Check 6 (empty evidence). Disposition: FAIL.

**Known-good input:**

```
GO_NOGO_DECISION.json scenario:
  Top-level "recommendation" present, no "decision".
  All 7 areas with canonical weights, sum = 1.0.
  Each area has score 30-95, evidence list with 3-6 cited inputs (>20 chars each).
  overall_score = 62; recompute = 62 (exact match).
  Recommendation = "GO" (62 >= 50).
  Rationales use natural language; no *.json / *.md tokens.
  Strategic Fit mentions "Oregon" matching anchor.
  overall_risks has 4 entries.
```

Verifier MUST PASS all checks. Disposition: PASS.

## Hard Rules

1. NEVER claim GO_NOGO_DECISION.json is missing without `Glob` verification first.
2. NEVER accept "decision" as a top-level key — this is a documented SAFS regression (MEMORY.md).
3. NEVER skip the score recompute — LLM-generated scores frequently don't match weight*area math; this is the primary integrity check.
4. NEVER ignore state-token drift; jurisdiction enforcement is universal across Stage 1.
5. Every finding must cite a specific field path + value + expected value (e.g., `assessment_areas[2].weight = 0.20 but canonical = 0.15`).
6. On FAIL, return corrective instructions targeting the specific Step number (Step 2 area names, Step 3 score compute, Step 4 threshold, Step 5 write) for surgical repair.
