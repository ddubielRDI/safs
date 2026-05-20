---
name: verifier-phase6c-context-bundle-win
expert-role: Bid Context Bundle Verifier
purpose: phase-boundary verifier for phase6c-context-bundle-win (schema completeness, RTM-sourced coverage, entity-list integrity, no truncation)
created: 2026-05-20
---

# Verifier — Phase 6c Bid Context Bundle (bid-context-bundle.json)

## When this runs

After phase6c-context-bundle-win reports done, BEFORE Stage 7 (bid authoring) begins. Stage 7 reads this bundle as its primary context input — bundle defects propagate into every bid section.

## Inputs (read in this order)

1. `{folder}/shared/bid-context-bundle.json` — primary output under verification
2. `{folder}/shared/UNIFIED_RTM.json` — RTM-sourced coverage cross-check (Phase 4)
3. `{folder}/shared/requirements-normalized.json` — requirement count + critical-req text fidelity
4. `{folder}/shared/REQUIREMENT_RISKS.json` — rtm_risks count + risk_level enum check
5. `{folder}/shared/EVALUATION_CRITERIA.json` — evaluation_factors + bid_section_mapping source
6. `{folder}/shared/COMPLIANCE_MATRIX.json` — mandatory_items count
7. `{folder}/shared/bid/CLIENT_INTELLIGENCE.json` — competitive position source
8. `{folder}/shared/bid/POSITIONING_OUTPUT.json` (optional; absent before Phase 8.0)

## Verification Checks

### Check 1 — File exists, parses, ≥ 10 KB, valid UTF-8

**Criterion:** `{folder}/shared/bid-context-bundle.json` exists AND `stat --printf %s` >= 10,240 bytes AND `json.load(open(path, encoding='utf-8'))` returns a dict without exception AND the JSON contains no `\u00XX` escape sequences for common UTF-8 characters (em dash, smart quotes, accented letters) — confirming `ensure_ascii=False` was applied.

**Pass:** All four sub-conditions hold.

**Fail:** File absent, size < 10 KB, JSON parse error, OR `—` / `’` / `“` escapes present in payload.

**Evidence to cite:** File size, parse error message if any, first escape sequence found.

**Hard-rule reminder:** NEVER claim file is missing without `Glob` verification first.

---

### Check 2 — Top-level schema fidelity (all required keys present)

**Criterion:** The parsed JSON contains all of these top-level keys (per phase6c-context-bundle-win.md Step 10 assemble_context_bundle):
  - `meta` (with sub-keys: `generated_at`, `domain`, `sources_included`, `source_count`, `bundle_version`)
  - `requirements_summary`
  - `risk_highlights`
  - `evaluation_alignment`
  - `evaluation_to_bid_mapping`
  - `evaluation_factors_by_weight`
  - `theme_eval_mapping`
  - `section_theme_mandates`
  - `evaluator_messages`
  - `section_content_guide`
  - `competitive_position`
  - `compliance_achievements`
  - `win_themes`
  - `personas`
  - `win_probability`
  - `domain_context`
  - `content_priority_guide`
  - `bid_author_instructions`

**Pass:** All 18 top-level keys present.

**Fail:** Any key missing.

**Evidence to cite:** List of missing top-level keys.

---

### Check 3 — coverage_claim sourced from RTM (not fabricated)

**Criterion:** `requirements_summary.coverage_source` is exactly one of:
  - `"rtm.verification.forward_coverage.bid_coverage_pct"` (RTM was present and computation succeeded), OR
  - `"unavailable"` (RTM absent or its verification block incomplete).

The string must NEVER be a fabricated source label like `"computed from requirements"`, `"estimated"`, or empty. If `requirements_summary.bid_coverage_pct` is non-null, then `coverage_source` MUST be the `"rtm.verification..."` variant (cannot have a percentage without an authoritative source).

**Pass:** `coverage_source` is one of the two allowed strings AND consistency holds between source and pct.

**Fail:** Any other source value, OR pct populated with source="unavailable".

**Evidence to cite:** `requirements_summary.coverage_source = "..."`, `bid_coverage_pct = X` (or null).

---

### Check 4 — Requirement count parity (no truncation, no drop)

**Criterion:** `requirements_summary.total` equals `len(requirements-normalized.requirements)`. Tolerance: 0 (exact match).

**Pass:** Counts match exactly.

**Concern:** Difference of 1–2 (could be a deduplication artifact) — log advisory.

**Fail:** Difference > 2 — requirements were silently dropped during bundle assembly.

**Evidence to cite:** `requirements_summary.total = N` vs `requirements-normalized count = M`.

---

### Check 5 — critical_requirements text NOT truncated

**Criterion:** Per phase6c-context-bundle-win.md Step 2 (2026-05-18 fix), `requirements_summary.critical_requirements[*].text` must contain the FULL requirement text — no `[:200]` slice. For each entry, find the same requirement in `requirements-normalized.requirements[]` (by `canonical_id` matching `id`) and string-compare: bundle.text MUST equal source.text exactly (or, if source is empty, bundle.text may be empty).

**Pass:** Every critical_requirements entry has text matching the source verbatim (or both empty).

**Fail:** Any entry has truncated text (bundle text length < source text length AND source > 200 chars — the classic [:200] slice signature).

**Evidence to cite:** Up to 3 offending entries with `id`, `bundle text length`, `source text length`.

---

### Check 6 — risk_highlights reads `rtm_risks` (not the empty `risks` key)

**Criterion:** `risk_highlights.total_risks_assessed` > 0 if `REQUIREMENT_RISKS.json` contains a non-empty `rtm_risks[]` array. Per HUNT-A-0002 fix, the broken pattern reads `risks_data.get("risks", [])` (always empty) and produces `total_risks_assessed = 0` while source has hundreds. AND `risk_highlights.by_risk_level` contains all four enum values (CRITICAL, HIGH, MEDIUM, LOW) — confirming the schema-correct `risk_level` field was read, not the broken `severity` field.

**Pass:** `total_risks_assessed > 0` when source `rtm_risks` is non-empty AND `by_risk_level` has all four keys.

**Fail:** `total_risks_assessed == 0` while source `rtm_risks` has > 0 entries, OR `by_risk_level` missing any enum key, OR a `by_severity` field appears instead (regression marker).

**Evidence to cite:** `risk_highlights.total_risks_assessed = N`, `source rtm_risks count = M`, `by_risk_level keys = [...]`.

---

### Check 7 — top_10_risks structure complete

**Criterion:** `risk_highlights.top_10_risks` is a list of length `min(10, total_risks_assessed)`. Each entry contains: `id`, `risk_level` (one of CRITICAL/HIGH/MEDIUM/LOW), and `mitigation_strategies` as an ARRAY (per UNIFIED_RTM schema — NOT a single `mitigation` dict). The list must be sorted with CRITICAL/HIGH first (sort verified by walking entries: at no point does a higher-priority level follow a lower-priority level).

**Pass:** Length correct, every entry has required fields, sort order intact.

**Fail:** Wrong length, missing field, sort violation, OR `mitigation_strategies` appears as a dict rather than an array.

**Evidence to cite:** Length found; first malformed entry; first sort violation index.

---

### Check 8 — bid_section_mapping, eval_factors_by_weight, theme_eval_mapping all populated AND not truncated

**Criterion:**
  - `evaluation_to_bid_mapping` is a dict with > 0 keys (matches `EVALUATION_CRITERIA.bid_section_mapping` from Phase 1.6).
  - `evaluation_factors_by_weight` is a list, sorted DESCENDING by `weight_normalized` (or `weight` if normalized absent). Verify: `factors[0]["weight_normalized"] >= factors[1]["weight_normalized"]` for all adjacent pairs.
  - `theme_eval_mapping` is a dict — may be empty `{}` if POSITIONING_OUTPUT.json is absent at execution time (per V5-F7 note), but if non-empty must contain at least one theme key with `evaluation_factors` sub-list.
  - `section_theme_mandates` is a dict, same rule as theme_eval_mapping.
  - `section_content_guide` is a dict with > 0 section keys, each with sub-keys: `evaluation_factors`, `emphasis_notes`, `mandated_themes`, `top_requirements`.

**Pass:** All five conditions hold per the relaxation rules.

**Concern:** `theme_eval_mapping` and `section_theme_mandates` are empty AND `POSITIONING_OUTPUT.json` does not exist on disk — log advisory only (this is the documented V5-F7 sequencing situation).

**Fail:** `evaluation_to_bid_mapping` empty, OR `evaluation_factors_by_weight` not sorted descending, OR `section_content_guide` empty, OR any section_content_guide entry missing the required sub-keys.

**Evidence to cite:** Key counts; sort-violation index; first malformed section_content_guide entry.

---

### Check 9 — meta.sources_included not truncated, source_count consistent

**Criterion:** `meta.sources_included` is a list of source filenames. `meta.source_count` equals `len(meta.sources_included)` exactly. The list should contain at least: `requirements-normalized.json`, `REQUIREMENT_RISKS.json`, `EVALUATION_CRITERIA.json`, `COMPLIANCE_MATRIX.json`, `CLIENT_INTELLIGENCE.json`, `domain-context.json`, `UNIFIED_RTM.json` (the 7 required sources). If any required source file exists on disk but is absent from `sources_included`, that's a silent drop.

**Pass:** `source_count == len(sources_included)` AND all 7 required sources present (assuming they exist on disk).

**Fail:** Count mismatch, OR any of the 7 required-and-on-disk sources missing from the list.

**Evidence to cite:** `source_count = N`, `len(sources_included) = M`, missing required sources list.

---

### Check 10 — No `[:N]` truncation in deliverable string fields, no row-cap notices, no entity-list truncation

**Criterion:** Searching the JSON for telltale truncation signatures:
  - No string field ending with `...` (mid-sentence ellipsis) where the source has longer text.
  - `competitive_position.our_differentiators` length matches source `positioning.core_positioning.key_differentiators` (up to the documented `[:6]` cap — that's a list cap, not a string cap, and is intentional).
  - `compliance_achievements.mandatory_items.unaddressed` length matches actual unaddressed count in COMPLIANCE_MATRIX (up to the documented `[:5]` cap is allowed BUT the totals (`total`, `addressed`) must be the FULL counts, not capped).
  - `requirements_summary.critical_requirements` length is `min(20, source_critical_count)` — the `[:20]` LIST cap is documented and intentional. NOT a violation if applied as a list cap; IS a violation if the underlying `text` field is sliced.

**Pass:** All four sub-conditions hold.

**Fail:** Any string field truncated mid-content (not at a documented LIST cap boundary).

**Evidence to cite:** Field path + truncation marker found.

---

### Check 11 — content_priority_guide.available reflects RTM presence honestly

**Criterion:** If `UNIFIED_RTM.json` exists on disk AND has `entities.requirements[]` non-empty, THEN `content_priority_guide.available` must be `true` AND `content_priority_guide.top_30_requirements` must be a list of length `min(30, len(rtm_requirements))`. If RTM is absent or empty, `available` must be `false` with an explanatory `note` field.

**Pass:** `available` flag matches RTM-on-disk reality AND if true, the top_30 list is sized correctly.

**Fail:** `available = false` while RTM is present and non-empty (regression of HUNT-A-0001 ordering fix), OR `available = true` while RTM is absent (impossible state).

**Evidence to cite:** `content_priority_guide.available`; on-disk RTM presence + req count; `top_30_requirements` length.

---

### Check 12 — section_content_guide.top_requirements uses correct field name

**Criterion:** Per phase6c-context-bundle-win.md Step 4b.5 (HUNT-A-0004 fix), the field is `text` (NOT `text_preview`). However Step 4c builds section_content_guide and at the time of writing (per phase file lines 470-490) still appends `text_preview` keys when distributing requirements to sections. Verify: every entry in `section_content_guide[*].top_requirements[*]` contains either consistently `text` OR consistently `text_preview` across all sections. Mixed usage indicates a partial migration / regression.

**Pass:** Field name is consistent across all top_requirements entries.

**Concern:** Field is `text_preview` everywhere (legacy name) — log advisory; downstream Stage 7 consumers must handle either.

**Fail:** Mixed `text` and `text_preview` keys across different sections — schema regression.

**Evidence to cite:** Field name distribution across sections.

---

## Disposition Logic

- **PASS:** Checks 1, 2, 3, 4, 5, 6, 7, 9, 10, 11 pass AND Check 8 meets threshold (not just CONCERN) AND Check 12 not in FAIL.
- **CONCERN:** Any of Checks 4, 8, 10, 12 fall in their advisory band. Log + continue to Stage 7.
- **FAIL:** Any of Checks 1, 2, 3, 5, 6, 7, 9, 11 fail OR Check 4 difference > 2 OR Check 8 fails non-relaxation conditions OR Check 12 mixed usage.

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase6c-context-bundle-win with the following targeted corrections:

[Check 1] Missing/undersize/\u-escapes: re-run Step 11 — open(..., encoding='utf-8') AND
  json.dump(..., ensure_ascii=False). The — escape signature means ensure_ascii defaulted to True.

[Check 2] Missing top-level keys {list}: audit Step 10 assemble_context_bundle — every section
  must be assigned into the bundle dict before write.

[Check 3] coverage_source = "{value}" fabricated: re-run Step 2 — only two values permitted
  ("rtm.verification.forward_coverage.bid_coverage_pct" OR "unavailable"). NEVER fabricate a pct.

[Check 4] Requirement count mismatch ({M-N} reqs dropped): Step 2 `reqs = .get("requirements") or []`
  must NOT be followed by any filter or slice.

[Check 5] critical_requirements[*].text truncated at ids {list}: remove `[:200]` slice (2026-05-18 fix).
  The `[:20]` list cap is fine; the string slice is the bug.

[Check 6] risk_highlights.total_risks_assessed = 0 while source = {M}: apply HUNT-A-0002 fix —
  read `risks_data.get("rtm_risks", [])`, field names `risk_level` not `severity`,
  `mitigation_strategies[]` not `mitigation` dict.

[Check 7] top_10_risks broken or unsorted: confirm Step 3 sort + 6 required fields + array form.

[Check 8] eval_to_bid_mapping empty / factors unsorted / section_content_guide malformed:
  Steps 4b/4b.5/4c. If eval_to_bid_mapping empty, escalate to Phase 1.6 upstream defect.
  sorted(reverse=True) on factors. Every section dict needs all 4 sub-keys.

[Check 9] sources_included count mismatch: Step 10 sources.append() chain must run for every
  loaded source; source_count assigned AFTER appends.

[Check 10] String truncation at {path}: remove mid-content slice. List caps are documented;
  string slices ([:200], [:150]) are violations.

[Check 11] content_priority_guide.available = false while RTM present: Step 4b.5 (relocated
  per HUNT-A-0001) must run BEFORE Step 4c. Confirm unified_rtm = load_json_safe(...) picked up file.

[Check 12] Mixed text / text_preview field naming: standardize on one (Step 4b.5 uses `text`,
  Step 4c uses `text_preview` — pick one and update the phase file).

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input:**

```
bid-context-bundle.json scenario:
  meta.source_count = 8 but sources_included has 7 entries.
  requirements_summary.total = 1,840 but requirements-normalized has 2,167 (327 dropped).
  requirements_summary.coverage_source = "estimated" (fabricated).
  requirements_summary.critical_requirements[0].text length = 200 chars; source = 583 chars ([:200] slice).
  risk_highlights.total_risks_assessed = 0 but REQUIREMENT_RISKS.rtm_risks has 281 entries.
  risk_highlights uses `by_severity` key (legacy) instead of `by_risk_level`.
  content_priority_guide.available = false but UNIFIED_RTM.json exists with 2,167 reqs.
  evaluation_factors_by_weight: factors[0]["weight"]=20, factors[1]["weight"]=35 (NOT descending).
  No — escapes — UTF-8 OK.
```

Verifier MUST detect: Check 3 (fabricated source), Check 4 (327 dropped), Check 5 ([:200] slice),
Check 6 (rtm_risks broken read + by_severity legacy), Check 9 (count mismatch), Check 8 (factors
not sorted), Check 11 (available=false while RTM present). Disposition: FAIL.

**Known-good input:**

```
bid-context-bundle.json scenario:
  meta.source_count = 12, sources_included has 12 entries including all 7 required.
  requirements_summary.total = 2,167 matching normalized.
  requirements_summary.coverage_source = "rtm.verification.forward_coverage.bid_coverage_pct",
    bid_coverage_pct = 96.4.
  critical_requirements[0].text length = 583 chars matching source verbatim.
  risk_highlights.total_risks_assessed = 281, by_risk_level = {CRITICAL: 18, HIGH: 62, MEDIUM: 168, LOW: 33}.
  top_10_risks: 10 entries, all with mitigation_strategies as array, sorted CRITICAL→HIGH→...
  evaluation_to_bid_mapping has 9 section keys (matching Phase 1.6 output).
  evaluation_factors_by_weight: 6 factors, sorted descending by weight_normalized.
  section_content_guide has 9 sections, each with all 4 required sub-keys.
  content_priority_guide.available = true, top_30_requirements has 30 entries.
  No truncation signatures, no \u-escapes.
```

Verifier MUST PASS all checks. Disposition: PASS.

## Hard Rules (inherited from verifier-design principles)

1. NEVER claim bid-context-bundle.json is missing without `Glob` verification first.
2. NEVER flag `theme_eval_mapping` or `section_theme_mandates` as empty as a FAIL if `shared/bid/POSITIONING_OUTPUT.json` does not exist on disk — that's the documented V5-F7 phase-ordering window; emit CONCERN only.
3. NEVER flag list caps (`[:6]`, `[:5]`, `[:20]`) as truncation — those are documented intentional list caps. ONLY string-level slices (`text[:200]`) are violations.
4. NEVER report risk-count mismatch without distinguishing the `risks` key (empty by design) from the `rtm_risks` key (the authoritative source per HUNT-A-0002).
5. NEVER fail the bundle on `\u00XX` escapes for genuinely-non-printable control characters — only on the common UTF-8 punctuation/letters (em dash U+2014, smart quotes U+2018-U+201D, accented Latin U+00C0-U+017F).
6. Every finding must cite a specific JSON field path + value or count (e.g., `risk_highlights.total_risks_assessed = 0 vs source rtm_risks count = 281`).
7. On FAIL, return corrective instructions tied to the specific Step number of phase6c-context-bundle-win that owns the defect.
