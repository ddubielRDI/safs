---
name: verifier-phase2b-normalize-win
expert-role: Requirements Normalization Verifier
purpose: phase-boundary verifier for phase2b-normalize-win (dedup reduction ratio, domain-axis collisions, source_ids preservation, priority/advisory caps)
created: 2026-05-20
---

# Verifier — Phase 2b Requirements Normalization

## When this runs

After phase2b-normalize-win reports done, BEFORE phase2c-catalog-win consumes `requirements-normalized.json`. This verifier exists because MARS round 1 had 30-40% near-duplicate inflation (dedup threshold too strict) AND a confirmed domain-axis collision regression where firm-desk-review variants were silently merged into entity-desk-review variants, leaving a functional scope gap.

## Inputs (read in this order)

1. `{folder}/shared/requirements-normalized.json` — primary output under verification
2. `{folder}/shared/requirements-raw.json` — source counts for reduction-ratio math
3. `{folder}/shared/sample-data-analysis.json` — confirms upstream Step 7c input flowed in
4. `{folder}/shared/workflow-extracted-reqs.json` — confirms workflow input flowed in
5. Prior verifier report at `{folder}/shared/validation/verifier-phase2b.json` (if a retry run)

## Verification Checks

### Check 1 — File exists and is valid JSON, schema-structurally valid

**Criterion:** `shared/requirements-normalized.json` exists AND parses as valid JSON AND contains top-level keys: `normalized_at`, `summary`, `category_distribution`, `priority_distribution`, `validation_summary`, `ambiguity_summary`, `ambiguous_requirements`, `requirements`, `invalid_requirements`. The `summary` block must contain `total_input`, `after_deduplication`, `duplicates_merged`, `valid_requirements`, `invalid_requirements`, `deduplication_rate`. File size > 1,024 bytes.

**Pass:** All keys present, JSON parses, size > 1,024 bytes.

**Fail:** File absent, JSON parse error, or any required key missing.

**Evidence to cite:** Specific missing key path. File size in bytes.

**Hard-rule reminder:** NEVER claim file is missing without `Glob` verification first.

---

### Check 2 — Dedup reduction ratio within sanity bounds (10% to 65%)

**Criterion:** `(summary.duplicates_merged / summary.total_input) * 100` must be between 10.0 and 65.0. Below 10% indicates dedup threshold too strict (prefix-normalization broken, near-duplicate inflation persists). Above 65% indicates over-aggressive merging (legitimate variants being collapsed).

Note: V3 widened the upper bound from 40% to 65% on 2026-05-20 because broader extraction patterns legitimately produce 50-60% overlap.

**Pass:** Reduction ratio between 10% and 65%.

**Concern:** Ratio between 7-10% OR between 60-65% — log advisory, may signal threshold tuning needed.

**Fail:** Ratio < 7% (likely V3-F4 prefix-normalization is broken) OR > 65% (over-aggressive merge).

**Evidence to cite:** `summary.total_input`, `summary.duplicates_merged`, `summary.deduplication_rate`, computed ratio.

---

### Check 3 — V3-F11: Domain-axis dedup protection (entity/firm, user/administrator, etc.)

**Criterion:** For each domain axis pair, count requirements in `requirements[]` containing each token. If both tokens appear in `flattened/*.md` source corpus, BOTH must appear in normalized requirements. Specifically:
- Count requirements with `\bentity\b` AND count with `\bfirm\b` — if grep shows both terms in flattened source, both counts MUST be > 0.
- Same check for `\buser\b` vs `\badministrator\b`, `\binternal\b` vs `\bexternal\b`, `\bapprove\b` vs `\breject\b`.

**Pass:** All domain axes that have both sides in flattened source also have both sides in normalized requirements.

**Fail:** Any axis with both sides in source but only one side surviving normalization — domain-axis dedup protection failed.

**Evidence to cite:** Per-axis: count in source, count in normalized requirements, axis name. If FAIL: which axis collapsed.

---

### Check 4 — source_ids preserved through dedup (V2-F1 fix verification)

**Criterion:** Every requirement in `requirements[]` (NOT `invalid_requirements[]`) that originated from `pattern_extraction` source has `source_ids[]` non-empty (length >= 1). AND: at least 5% of surviving requirements have `source_ids` length >= 2 (evidence of merge-time source-id union) — this validates that the V2-F1 fix actually fired.

**Pass:** All pattern_extraction requirements have non-empty source_ids AND ≥5% have multi-source citations.

**Concern:** All requirements have source_ids but <5% have multi-source — may indicate very few duplicates were actually merged (cross-reference with Check 2).

**Fail:** Any pattern_extraction requirement has empty/missing source_ids.

**Evidence to cite:** Count of requirements with empty source_ids (must be 0). Count and % with source_ids length >= 2.

---

### Check 5 — SHOULD-only requirements capped at MEDIUM priority (V2-F4 fix)

**Criterion:** For every requirement in `requirements[]` whose text contains `should` (case-insensitive) but does NOT contain any of `shall`, `must`, `required`, `mandatory`, the `priority` field MUST be `MEDIUM` or `LOW`, NEVER `CRITICAL` or `HIGH`.

**Pass:** Zero advisory-only requirements have CRITICAL or HIGH priority.

**Fail:** Any advisory-only requirement with CRITICAL or HIGH priority — V2-F4 fix didn't fire, compliance matrix will be inflated.

**Evidence to cite:** Count of offenders. List first 5 with `req.id`, priority, text excerpt.

---

### Check 6 — V3-F5: CRITICAL count <5% of total (sanity gate)

**Criterion:** `priority_distribution.CRITICAL / summary.valid_requirements < 0.05`. Most requirements should be HIGH, not CRITICAL. If >5% are CRITICAL, the priority algorithm is likely matching critical keywords from full_context instead of text only.

**Pass:** CRITICAL count <5% of valid requirements.

**Concern:** CRITICAL count between 5% and 8%. Log advisory.

**Fail:** CRITICAL count > 8% — priority over-flagging, V3-F5 fix not enforcing text-only matching.

**Evidence to cite:** CRITICAL count, total valid, percentage. Spot-check 3 CRITICAL items: confirm their text (not full_context) contains a critical keyword.

---

### Check 7 — No checklist questions / fragment-end / quote-internal in valid requirements

**Criterion:** Apply Phase 2 V3 quality checks again at this gate (last line of defense before catalog):
- Zero requirements in `requirements[]` end with `?` (after rstrip).
- Zero requirements in `requirements[]` end with hyphen, comma, or subordinator words.
- Zero requirements in `requirements[]` match `"[a-z]+$` (quote-internal truncation).

Such items belong in `invalid_requirements[]` only.

**Pass:** Zero offenders in valid `requirements[]`.

**Fail:** Any offenders — V3-F6/F8/F10 fixes failed to route to invalid.

**Evidence to cite:** Count per category. List first 3 of each with `req.id`.

---

### Check 8 — Length-gate validation (V3-F6)

**Criterion:** Any requirement in `requirements[]` with `len(text) < 30` MUST carry an explicit `validation.issues` entry containing the phrase "very short" OR "Too short". Items <20 chars should be in `invalid_requirements[]`.

**Pass:** Zero requirements <20 chars in `requirements[]`. Any 20-29 char requirement carries the warning flag.

**Fail:** Any <20-char requirement surviving in `requirements[]`, OR any 20-29-char requirement without the warning flag.

**Evidence to cite:** Count of <20-char items in `requirements[]`. Count of 20-29-char items missing the flag.

---

### Check 9 — Canonical IDs assigned and unique

**Criterion:** Every requirement in `requirements[]` has `canonical_id` matching pattern `\d{3,}[A-Z]{3}` (e.g., `001APP`, `024TEC`). All canonical_ids are unique across the full `requirements[]` array. Also has `display_id` populated.

**Pass:** All requirements have unique well-formed canonical_id and display_id.

**Fail:** Duplicate canonical_ids, missing canonical_id, or malformed canonical_id.

**Evidence to cite:** Count of duplicates (must be 0). Count of missing (must be 0). List up to 5 violators.

---

### Check 10 — UTF-8 round-trip and universal anti-regression

**Criterion:** Open `requirements-normalized.json` with `encoding='utf-8'`. Scan all string values for mojibake sentinels (`�`, `Ã©`, `â€™`, `â€"`) — count must be ≤5 (see codified band below). Scan for `_Showing \d+ of \d+_` row-cap notice — count must be 0. Scan for `[:N]` literal slicing leakage — count must be 0. EXCEPTION: `ambiguous_requirements[].text_preview` is a documented intentional truncation (HUNT-A-0003 fix); the field is named `_preview` precisely to mark this — verifier must NOT flag it.

**⛔ U+FFFD threshold (codified 2026-05-20 — MARS Phase 2b retry-2 incident, aligns Phase 2b verifier with Phase 1.7 / Phase 2 / Phase 2b-retry-1 verifier readings):**

Phase 1.7 / Phase 2 / Phase 2b-retry-1 verifiers all applied the ≤5 PASS-WITH-CONCERN band per the standing `skill-win.md` MOJIBAKE SCRUB policy. The original Phase 2b verifier criterion was zero-tolerance, which broke verifier reading consistency across the pipeline. This clarification re-aligns Phase 2b with the standing band:

- **PASS:** `_Showing N of M_` = 0 AND `[:N]` literal = 0 AND U+FFFD count = 0
- **PASS-WITH-CONCERN:** `_Showing N of M_` = 0 AND `[:N]` literal = 0 AND 0 < U+FFFD count ≤ 5 AND every residual is logged in `pipeline_metadata.encoding_audit[].items_with_unrepairable_residual[]` (or equivalent audit trail) with field path + character index. The CONCERN-band reading is the documented escape valve for upstream Phase 0 PDF-extraction artifacts that the producer's scrub helpers genuinely cannot disambiguate. Cite the audit log entries as evidence.
- **FAIL:** Any `_Showing N of M_` notice OR any `[:N]` literal OR U+FFFD count > 5 OR any U+FFFD residual NOT logged in the audit.

**Pass:** Sentinel counts within the codified bands above (with concerns logged as advisory).

**Fail:** Sentinel counts outside the codified bands above OR audit log incomplete.

**Evidence to cite:** Sentinel pattern, count, first 3 field paths, audit-log presence/absence for each residual (excluding documented `text_preview` field).

---

## Disposition Logic

- **PASS:** Checks 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 all pass (with concerns logged as advisory).
- **CONCERN:** Check 2 or 6 in advisory band, OR Check 4 has <5% multi-source. Continue but log warning.
- **FAIL:** Any of Checks 1, 3, 5, 7, 8, 9, 10 fail OR Check 2 outside [7%, 65%] OR Check 6 >8%.

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase2b-normalize-win with the following targeted corrections:

[Check 1 fail] requirements-normalized.json missing or invalid.
  Action: Verify Step 8 (write normalized) executed AND wrote all required top-level keys.

[Check 2 fail] Dedup ratio {X}% outside [10%, 65%].
  If <10%: V3-F4 prefix-normalization is broken. Audit normalize_for_dedup() — confirm
    PROC_PREFIX_PATTERNS strips "the system shall", "the application shall", "be able to",
    etc. before SequenceMatcher.ratio() runs. Confirm threshold = 0.78 (not 0.85).
  If >65%: dedup over-aggressive. Raise threshold to 0.82 or audit domain-axis protection.

[Check 3 fail] Domain-axis collapse: {axis} survives only on one side.
  Action: Audit Step 3 deduplicate_requirements() — confirm domain_conflict check uses
  the XOR pattern (a_in_req and b_in_ex and not a_in_ex and not b_in_req) OR
  (b_in_req and a_in_ex and not b_in_ex and not a_in_req). Without XOR, parallel
  requirements get merged. Confirm all 9 domain_axes pairs are checked.

[Check 4 fail] {N} pattern_extraction requirements lack source_ids OR <5% multi-source.
  Action: Audit Step 3 dedup loop — confirm the V2-F1 merge block runs:
    incoming_ids = req.get("source_ids", [])
    if incoming_ids:
        existing.setdefault("source_ids", []).extend(...)
  This block MUST fire on every duplicate detection, not just when sources differ.

[Check 5 fail] {N} SHOULD-only requirements have CRITICAL/HIGH priority.
  Action: Audit Step 6 assign_priority() — confirm is_advisory_only check runs FIRST,
  before critical_keywords check. The order matters: advisory items must return MEDIUM
  before falling through to keyword matching.

[Check 6 fail] CRITICAL count {X}% > 8% — over-flagging.
  Action: Audit Step 6 — V3-F5 fix requires text_lower = req["text"].lower() ONLY,
  NOT req.get("full_context", ""). Adjacent context bleeding into priority assignment
  inflates CRITICAL count when nearby sentences mention "security" or "required".

[Check 7 fail] {N} question-marks / fragment-ends / quote-internals in valid catalog.
  Action: Audit Step 4 validate_requirement() — confirm fragment-end detection (last_char
  in '-,'), subordinator check, and ends-with-? check all return score < 70 to route
  to invalid_requirements[]. Confirm Step 8 filter (`if r["validation"]["valid"]`) is
  applied.

[Check 8 fail] {N} short fragments in valid requirements without warning flag.
  Action: Audit validate_requirement() length check thresholds — must be:
    < 20: score -= 40, "Too short -- invalid fragment"
    < 30: score -= 25, "Very short -- likely truncated"
  Items <20 should score <70 -> invalid. Items 20-29 should be flagged.

[Check 9 fail] Duplicate or malformed canonical_ids.
  Action: Audit Step 5 assign_canonical_ids() — confirm category_counters is a single
  shared dict, not re-initialized per-requirement. Confirm format string is
  f"{counter:03d}{category}" (3-digit zero-padded numeric prefix + 3-char category).

[Check 10 fail] Mojibake / row-cap / [:N] leakage.
  Action: Universal anti-regression. Audit open() calls for encoding='utf-8', json.dump
  for ensure_ascii=False. Do NOT flag ambiguous_requirements[].text_preview — that's
  intentional and documented.

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input (modeled on pre-V3 MARS regression):**

```
requirements-normalized.json scenario:
  summary.total_input = 487
  summary.duplicates_merged = 56
  summary.deduplication_rate = "11.5%"  (below 10% threshold post-correction)
  Of valid requirements:
    23 contain "should" only and have priority = HIGH
    8 contain "should" only and have priority = CRITICAL
    87 are flagged CRITICAL (20% of valid count)
    3 end with `?`, 6 end with hyphen
  domain axes: source has both "entity" (134) and "firm" (52); normalized has
    "entity" (89) but "firm" (0) — firm-desk-review variants merged into entity ones.
  29 pattern_extraction requirements have empty source_ids[]
```

Verifier MUST detect: Check 2 (11.5% below 10% sanity gate when V3 threshold should give 30-50%; in this case OUTPUT IS PARTIAL FAIL — re-tune triggered), Check 3 (firm collapsed), Check 4 (29 missing source_ids), Check 5 (31 SHOULD-only with HIGH/CRITICAL), Check 6 (20% CRITICAL), Check 7 (3+6 invalid items in valid). Disposition: FAIL.

**Known-good input:**

```
requirements-normalized.json scenario:
  summary.total_input = 487
  summary.duplicates_merged = 178
  summary.deduplication_rate = "36.6%"  (in [10%, 65%] band)
  summary.valid_requirements = 287, invalid = 22
  Of valid requirements:
    No "should"-only items have CRITICAL or HIGH priority
    9 are CRITICAL (3.1% — within sanity)
    No question marks, no fragment-ends, no quote-internals in valid[]
    All have canonical_id matching \d{3,}[A-Z]{3}, all unique
    23% have source_ids length >= 2 (V2-F1 fix verified firing)
  domain axes: "entity" (78) AND "firm" (31) both present in normalized
  No mojibake; no _Showing N of M_; no [:N] leakage (text_preview field correctly excluded).
```

Verifier MUST PASS all checks. Disposition: PASS.

## Hard Rules (inherited from verifier-design principles)

1. NEVER claim requirements-normalized.json is missing without `Glob` verification first.
2. NEVER flag `ambiguous_requirements[].text_preview` as a `[:N]` regression — it is documented intentional truncation (HUNT-A-0003 fix).
3. NEVER report a domain-axis collapse without confirming the missing token actually appears in flattened source — false positives waste retry budget.
4. Every finding must cite a specific `req.id` + field name + value (e.g., `requirements[42].priority = "CRITICAL"` with text excerpt confirming SHOULD-only).
5. On FAIL, return corrective instructions referencing the specific V3 fix ID (V3-F4, V3-F5, V3-F11) so the agent can locate the exact code block to repair.
6. Domain-axis check (Check 3) is the highest-impact regression — never skip even on a green-looking output. The firm-desk-review collapse was invisible at summary level.
