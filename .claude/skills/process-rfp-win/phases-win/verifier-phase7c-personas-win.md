---
name: verifier-phase7c-personas-win
expert-role: Persona Coverage Verifier
purpose: phase-boundary verifier for phase7c-personas-win (persona inventory completeness, decision drivers + watch-outs, messaging hooks, coverage matrix to bid sections)
created: 2026-05-20
---

# Verifier — Phase 7c Evaluator Personas

## When this runs

After phase7c-personas-win reports done, BEFORE phase7d-scoring-win consumes `PERSONA_COVERAGE.json` for its evaluator-alignment factor.

## Inputs (read in this order)

1. `{folder}/shared/PERSONA_COVERAGE.json` — primary output under verification
2. `{folder}/shared/EVALUATION_CRITERIA.json` — source for evaluator-type cross-check
3. `{folder}/shared/requirements-normalized.json` — source for `relevant_requirements` ID validation
4. `{folder}/outputs/*.md` — list of actual bid sections used to verify the coverage matrix maps personas to real deliverables
5. Any prior verifier report at `{folder}/shared/validation/verifier-phase7c.json` (if a retry run)

## Verification Checks

### Check 1 — File exists, parses as valid JSON, size above floor

**Criterion:** `shared/PERSONA_COVERAGE.json` exists AND parses as valid JSON. File size > 200 bytes (phase contract floor); realistic populated output will be >> 1 KB.

**Pass:** File present, JSON parses without error, size > 200 bytes.

**Fail:** File absent, JSON parse error, or file size <= 200 bytes (stub or write failure).

**Evidence to cite:** Actual file size and JSON parse status.

**Hard-rule reminder:** NEVER claim file is missing without `Glob` verification first.

---

### Check 2 — UTF-8 round-trip with no mojibake

**Criterion:** File opens cleanly with `encoding='utf-8'`. No mojibake sequences (`â€™`, `â€œ`, `â€`, `Ã©`, `Â`, `�`) in any string field. JSON written with `ensure_ascii=False` (verify by spot-checking that non-ASCII characters in concerns/callouts survive as UTF-8 bytes rather than `\uXXXX` escapes when present).

**Pass:** UTF-8 read succeeds; zero mojibake matches.

**Fail:** Any mojibake sequence found or UnicodeDecodeError on utf-8 open.

**Evidence to cite:** Grep result counts for each mojibake pattern.

---

### Check 3 — Schema fidelity: top-level keys present and well-typed

**Criterion:** `PERSONA_COVERAGE.json` top-level keys include all of: `analyzed_at`, `overall_score`, `target_score`, `meets_target`, `personas`, `recommendations`. `overall_score` is a numeric (int/float) between 0 and 100. `meets_target` is a bool. `personas` is a dict. `recommendations` is a list (may be empty).

**Pass:** All required keys present and well-typed; `overall_score` in [0, 100].

**Fail:** Any required key absent; `overall_score` non-numeric or out of range; `personas` not a dict; `recommendations` not a list.

**Evidence to cite:** List actual top-level keys found; report any typing failures with field path + actual value/type.

---

### Check 4 — Minimum 4 personas present (EXECUTIVE, RISK, TECHNICAL, OPERATIONAL at minimum)

**Criterion:** `personas` dict contains at least 4 entries. The set of keys MUST be a superset of `{"EXECUTIVE", "RISK", "TECHNICAL", "OPERATIONAL"}`. The phase contract defines 5 personas (these four plus FINANCIAL) — having all 5 is the expected default.

**Pass:** `len(personas) >= 4` AND the required 4-persona subset is fully present.

**Concern:** Exactly 4 personas present (FINANCIAL omitted) — log advisory; the phase default is 5.

**Fail:** `len(personas) < 4` OR any of the required 4 missing from keys.

**Evidence to cite:** `list(personas.keys())` actual; identify which required key is missing.

---

### Check 5 — Every persona has decision_drivers (priorities), watch_outs (concerns), and messaging hooks (callouts)

**Criterion:** For every entry in `personas`, the following fields must be present and non-empty:
- `name` (string)
- `title` (string)
- `weight` (numeric, 0 < weight <= 1)
- `priorities` (list of strings, len >= 3) — these are the decision_drivers
- `concerns` (list of strings, len >= 2) — these are the watch_outs
- `coverage` (dict with `coverage_score` numeric and `meets_target` bool)
- `callouts` (dict with `sample_callout` non-empty string and `relevant_requirements` list) — these are the messaging hooks

Sum of all `weight` values across personas must equal approximately 1.0 (tolerance ±0.05) — weights should partition the evaluator space.

**Pass:** All personas have all required fields populated to the specified minimums; weight sum in [0.95, 1.05].

**Fail:** Any persona missing any required field; any priorities list with < 3 entries; any concerns list with < 2 entries; weight sum outside [0.95, 1.05].

**Evidence to cite:** Persona key + specific missing/short field (e.g., `personas.RISK.priorities length = 2 (< 3 required)`); print weight sum.

---

### Check 6 — Coverage matrix maps personas to real bid sections (content_focus resolution)

**Criterion:** Every persona's content focus (carried over from the phase's `EVALUATOR_PERSONAS["X"]["content_focus"]` definitions — verify by inspecting `personas[K].coverage.dedicated_sections` and the source phase contract) refers to deliverables that actually exist in `{folder}/outputs/`. If `coverage.dedicated_sections == 0` for any persona, this signals the persona is unmapped to any real bid section — automatic FAIL unless `coverage_score >= 60` via priority-keyword matches alone (advisory band).

**Pass:** Every persona has `dedicated_sections >= 1` OR has `coverage_score >= 60` via priority-keyword matches.

**Concern:** A persona has `dedicated_sections == 0` but `coverage_score >= 60` — log as advisory: the persona is covered diffusely rather than via a dedicated deliverable.

**Fail:** Any persona has `dedicated_sections == 0` AND `coverage_score < 60` — that persona is effectively unrepresented in the bid.

**Evidence to cite:** Per-persona `dedicated_sections` and `coverage_score`; list which deliverables in `{folder}/outputs/` were considered.

---

### Check 7 — `relevant_requirements` IDs resolve to requirements-normalized.json

**Criterion:** For every persona, `personas[K].callouts.relevant_requirements` is a list of canonical IDs. Build set of canonical IDs from `requirements-normalized.json["requirements"][*].canonical_id`. Every ID in any persona's `relevant_requirements` MUST exist in the canonical set. Tolerance: 0 orphans.

**Pass:** All referenced IDs resolve.

**Fail:** Any orphan ID found.

**Evidence to cite:** Persona key + up to 5 orphan IDs (e.g., `personas.TECHNICAL.callouts.relevant_requirements contains [001ZZZ, 099QQQ] — not in canonical set`).

---

### Check 8 — No `[:N]` truncation surviving into deliverable strings and no row-cap notices

**Criterion:** Grep the serialized JSON for the pattern `\[:[0-9]+\]` (Python slice notation that should never survive into data) — must return 0 matches. Grep for the literal `_Showing ` — must return 0 matches. Any string field in the JSON (concerns, priorities, sample_callout) that ends with `...` followed by EOL should be flagged unless the source content legitimately contains it (verify against requirements-normalized text if a candidate is found).

**Pass:** All three grep patterns return 0; no suspicious terminal-ellipsis strings.

**Fail:** Any `[:N]` slice notation or `_Showing N of M_` notice present; mid-word truncations in serialized strings.

**Evidence to cite:** Grep counts per pattern.

---

### Check 9 — Cross-stage consistency: persona keys match EVALUATION_CRITERIA evaluator types (where defined)

**Criterion:** If `EVALUATION_CRITERIA.json` defines evaluator types or evaluation factors that map to known persona archetypes (e.g., a `Technical Approach` factor → TECHNICAL persona; a `Past Performance` factor → EXECUTIVE persona; a `Risk Management` factor → RISK persona; an `Operational Readiness` factor → OPERATIONAL persona), every such factor must have a corresponding persona present in `PERSONA_COVERAGE.json`. Tolerance: if EVALUATION_CRITERIA contains a factor whose label clearly maps to a persona archetype absent from PERSONA_COVERAGE, log it.

**Pass:** Every evaluator-aligned factor has a corresponding persona.

**Concern:** An evaluation factor exists with no matching persona archetype, but the absent persona is FINANCIAL or another optional archetype (advisory).

**Fail:** A primary evaluation factor (Technical, Past Performance/Executive, Risk, Operational) has no corresponding persona.

**Evidence to cite:** List EVALUATION_CRITERIA factor names + matched/unmatched persona archetypes.

---

## Disposition Logic

- **PASS:** Checks 1, 2, 3, 4, 5, 7, 8, 9 all pass AND Check 6 not in FAIL band.
- **CONCERN:** Check 4 advisory (exactly 4 personas, FINANCIAL omitted) OR Check 6 advisory (persona covered diffusely, no dedicated section) OR Check 9 advisory (optional persona missing). Log + continue.
- **FAIL:** Any of Checks 1, 2, 3, 5, 7, 8 fail OR Check 4 fails (< 4 personas or required archetype missing) OR Check 6 fails (persona unrepresented) OR Check 9 fails (primary evaluator factor unmapped).

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase7c-personas with the following targeted corrections:

[Check 1 fail] PERSONA_COVERAGE.json missing or undersized.
  Action: Re-run Step 5 (Write Output). Verify the persona_output dict was assembled
  with all 5 EVALUATOR_PERSONAS keys before write_json. A stub file usually means
  generate_recommendations() raised before write.

[Check 2 fail] Mojibake in concerns/priorities/sample_callout strings.
  Action: All open() calls must use encoding='utf-8' and all json.dump must use
  ensure_ascii=False. Re-emit from in-memory dict — do NOT in-place "fix" mojibake.

[Check 3 fail] PERSONA_COVERAGE.json missing top-level key {key} or wrong type.
  Action: Step 5 builds the persona_output dict — confirm all 6 top-level keys are
  assigned. overall_score must come from calculate_persona_score() which returns a
  float; cast explicitly if a string is observed.

[Check 4 fail] Fewer than 4 personas present OR required archetype missing: {list}.
  Action: EVALUATOR_PERSONAS at the top of phase7c-personas is the source of truth.
  Confirm all 5 keys (TECHNICAL, FINANCIAL, RISK, EXECUTIVE, OPERATIONAL) iterate in
  Step 2's analyze_persona_coverage(). Do not let any silent filter remove a persona.

[Check 5 fail] Persona {K} missing required fields or weight sum off.
  Action: priorities and concerns are literal lists in EVALUATOR_PERSONAS — if missing
  in output, the dict-comprehension in Step 5 is dropping fields. Verify it copies all
  keys (name, title, weight, priorities, concerns, coverage, callouts). Weight sum:
  recompute SUM(weight) — must be ~1.0; rebalance literals if drift exists.

[Check 6 fail] Persona {K} has no dedicated section AND coverage_score < 60.
  Action: Either (a) author the missing content_focus deliverable (e.g., emit a
  dedicated EXECUTIVE_SUMMARY.md if EXECUTIVE persona is unrepresented), or (b) seed
  the unified content (REQUIREMENTS_CATALOG, ARCHITECTURE) with the persona's priority
  keywords so the keyword-match score climbs above 60. Do NOT silently lower the
  meets_target threshold.

[Check 7 fail] Orphan requirement IDs in persona {K} relevant_requirements: {list}.
  Action: generate_callouts() in Step 3 filters by priority keyword match against
  requirement text — an orphan implies the requirement was filtered out of normalized
  but its ID survived in a stale cache. Re-run with a fresh read of
  requirements-normalized.json and verify req.get("canonical_id") matches the source.

[Check 8 fail] Truncation pattern or "_Showing N of M_" notice in JSON strings.
  Action: Audit Step 3 — relevant_reqs[:20] is the canonical offender. Either remove
  the [:20] slice (emit ALL relevant requirements) or rename the field to make it
  clear (e.g., relevant_requirements_top20). Per SAFS memory, never [:N] truncate
  deliverable arrays.

[Check 9 fail] Primary evaluator factor unmapped: {factor → archetype}.
  Action: EVALUATION_CRITERIA.json defines what evaluators care about. If a factor
  has no persona, either add the persona to EVALUATOR_PERSONAS in this phase, or
  document the deliberate exclusion in personas.{K}.notes. Do not ship a persona
  coverage report that ignores a known evaluator factor.

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input:**

```
PERSONA_COVERAGE.json scenario: personas has only 3 entries (TECHNICAL, FINANCIAL,
  EXECUTIVE — RISK and OPERATIONAL dropped).
  Weight sum = 0.70 (FINANCIAL.weight=0.25 + TECHNICAL.weight=0.30 + EXECUTIVE.weight=0.15).
  personas.TECHNICAL.priorities = ["scalability", "security"] (only 2 entries).
  personas.FINANCIAL.callouts.relevant_requirements contains "099ZZZ" (not in normalized).
  personas.EXECUTIVE.coverage.dedicated_sections = 0, coverage_score = 42.
  Serialized JSON contains relevant_requirements_top20 truncation pattern.
```

Verifier MUST detect: Check 4 (RISK + OPERATIONAL absent), Check 5 (weight sum 0.70 < 0.95, TECHNICAL priorities < 3), Check 6 (EXECUTIVE unrepresented), Check 7 (orphan 099ZZZ), Check 8 (truncation). Disposition: FAIL.

**Known-good input:**

```
PERSONA_COVERAGE.json scenario: all 5 personas present.
  Weight sum = 1.00 (0.30 + 0.25 + 0.20 + 0.15 + 0.10).
  Every persona has priorities (>= 5 entries), concerns (>= 3 entries), populated callouts.
  Every persona's dedicated_sections >= 1.
  Coverage scores: TECHNICAL=92, FINANCIAL=88, RISK=85, EXECUTIVE=90, OPERATIONAL=78.
  overall_score = 88.4, meets_target = false (target 90).
  recommendations = [1 entry — OPERATIONAL persona below target].
  All relevant_requirements IDs resolve to canonical set.
  No mojibake, no [:N] truncation, no row-cap notices.
```

Verifier MUST PASS all checks (overall_score below target is NOT a failure — it triggers a recommendation, not a verifier FAIL). Disposition: PASS.

## Hard Rules (inherited from verifier-design principles)

1. NEVER claim PERSONA_COVERAGE.json is missing without `Glob` verification first.
2. NEVER flag a persona as "incomplete" if the missing field is optional (e.g., a `notes` field not in the required schema) — verify against the required field list in Check 5.
3. NEVER allow [:N] truncation of `relevant_requirements` to ship in production — every persona's matched-requirement list must be complete (per SAFS memory: three regressions across pipelines).
4. Weight-sum drift is a silent bug magnet — weight sum outside [0.95, 1.05] indicates an editor manually changed one weight without re-balancing the others; always FAIL.
5. Every finding must cite a specific persona key + field path (e.g., `personas.RISK.priorities length = 2 (< 3 required)`).
6. On FAIL, return corrective instructions that point at the exact line in phase7c-personas-win.md responsible for the offending behavior so the phase agent can target the repair.
