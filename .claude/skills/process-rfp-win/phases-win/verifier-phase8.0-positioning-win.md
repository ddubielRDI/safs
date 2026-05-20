---
name: verifier-phase8.0-positioning-win
expert-role: Strategic Positioning Verifier
purpose: phase-boundary verifier for phase8.0-positioning-win (matched_projects integrity, theme-eval mapping completeness, evaluator messages, ghost strategy carry-forward)
created: 2026-05-20
---

# Verifier — Phase 8.0 Strategic Positioning

## When this runs

After phase8.0-positioning-win reports done, BEFORE Phase 8.1 (Letter of Submittal) consumes `POSITIONING_OUTPUT.json` as its primary source of themes, matched projects, and evaluator messages.

## Inputs (read in this order)

1. `{folder}/shared/bid/POSITIONING_OUTPUT.json` — primary output under verification
2. `{folder}/shared/EVALUATION_CRITERIA.json` — cross-check: theme-eval mapping must cover top-weighted factors
3. `{folder}/shared/PERSONA_COVERAGE.json` — cross-check: every persona must have an evaluator message
4. `Past_Projects.md` — for matched_projects cross-reference
5. `config-win/company-profile.json` — for `override_projects` honor check
6. `{folder}/shared/bid/COMPETITIVE_POSITION.json` (Phase 1.97) — for ghost-strategy carry-forward check

## Verification Checks

### Check 1 — File exists, parses as JSON, meets minimum size

**Criterion:** `shared/bid/POSITIONING_OUTPUT.json` exists AND parses as valid JSON AND file size >= 3,072 bytes (3 KB). A positioning output under 3 KB indicates a stub or a failed run.

**Pass:** File exists, `json.load` succeeds, `os.path.getsize(path) >= 3072`.

**Fail:** File absent OR parse error OR size < 3,072 bytes.

**Evidence to cite:** File path + actual size in bytes.

**Hard-rule reminder:** NEVER claim file is missing without `Glob` verification first.

---

### Check 2 — Top-level schema fidelity

**Criterion:** All of the following top-level keys are present: `core_positioning`, `evaluator_messages`, `content_priority_order`, `win_themes`, `theme_eval_mapping`, `section_theme_mandates`, `matched_projects`, `match_metadata`, `matched_evidence`, `ghost_strategy`. Under `core_positioning`: `tagline`, `value_proposition`, `themes`, `key_differentiators`, `proof_points`.

**Pass:** All keys present.

**Fail:** Any required top-level key missing OR `core_positioning` missing any required sub-key.

**Evidence to cite:** List actual top-level keys observed; name missing key(s) explicitly.

---

### Check 3 — matched_projects has 3+ ranked entries from Past_Projects.md

**Criterion:** `len(matched_projects) >= 3`. Every entry must have non-null `project_number`, `client`, `relevance_score`, `score_breakdown`, `rank`, and `relevance_statement`. The `_internal_source` field in `match_metadata` must equal `"Past_Projects.md"`.

**Pass:** 3+ entries AND every entry has all required fields AND match_metadata._internal_source == "Past_Projects.md".

**Concern:** Exactly 3 entries with all `relevance_score < 10` (weak matches across the board) — log advisory but don't fail; Phase 8.1/8.2/8.3 will note the warning.

**Fail:** Fewer than 3 entries OR any entry missing required field OR _internal_source absent/wrong.

**Evidence to cite:** Print `len(matched_projects)`, list first 3 ranks/clients/scores, and any missing field paths.

---

### Check 4 — theme_eval_mapping covers every win theme

**Criterion:** For every theme in `win_themes` (or `core_positioning.themes`), `theme_eval_mapping[theme_name]` exists AND is a non-empty list AND each entry has fields `factor_id`, `factor_name`, `weight`, `alignment_score`, `relevance`. Themes with zero mapped factors are FAIL — they cannot be threaded into bid sections.

**Pass:** Every theme has >= 1 mapped factor, all entries fully formed.

**Fail:** Any theme has empty factor list OR missing required field on any mapping entry.

**Evidence to cite:** List themes with empty mappings; for the first malformed entry, name the missing field.

---

### Check 5 — section_theme_mandates is populated

**Criterion:** `section_theme_mandates` is a dict with at least 3 section keys. Each section value must contain `themes` (list, >=1 entry), `eval_factors` (list, >=1 entry), and `is_primary_for` (list, may be empty). Empty `section_theme_mandates` means Phase 8.1-8.4 won't know which themes are mandated per section, which produces ungrounded bid sections.

**Pass:** >= 3 sections with populated themes + eval_factors.

**Fail:** `section_theme_mandates` empty OR any section has zero themes AND zero eval_factors.

**Evidence to cite:** Print number of sections and the keys; list any section with empty themes list.

---

### Check 6 — evaluator_messages covers all 5 personas

**Criterion:** `evaluator_messages` contains keys `TECHNICAL`, `FINANCIAL`, `RISK`, `EXECUTIVE`, `OPERATIONAL`. Each value must have non-empty `headline`, `key_message`, `proof_points` (>= 1 entry).

**Pass:** All 5 personas present with all 3 fields populated.

**Concern:** 4 of 5 personas populated (one missing) — log advisory; downstream bid sections that rely on the missing persona may produce generic content.

**Fail:** 3 or fewer personas populated, OR any persona missing `headline` or `key_message`.

**Evidence to cite:** Print `list(evaluator_messages.keys())`; for any missing persona, name it explicitly.

---

### Check 7 — Ghost strategy carried forward from Phase 1.97 (not re-invented)

**Criterion:** `ghost_strategy.competitive_context.incumbent` must match `COMPETITIVE_POSITION.json.incumbent` (if Phase 1.97 produced that file). `ghost_strategy.ghost_phrases` is a list (may be empty if no Phase 1.97 file exists, but if Phase 1.97 produced phrases, the count here must equal or exceed Phase 1.97's count — no silent drop).

**Pass:** `incumbent` matches OR Phase 1.97 file absent (first-bid scenario). Phrase count >= Phase 1.97 count.

**Concern:** Phase 1.97 file present but incumbent mismatch — log advisory; phase may have re-invented strategy.

**Fail:** Phase 1.97 file present AND phrase count dropped (e.g., 1.97 had 8 phrases, 8.0 has 2).

**Evidence to cite:** Print `incumbent` from both files; print phrase counts from both.

---

### Check 8 — Positioning aligned to top-weighted eval factors

**Criterion:** Of the top-3 highest-weighted factors in `EVALUATION_CRITERIA.json`, at least 2 must appear as targets in `theme_eval_mapping` (i.e., at least 2 themes map to factors with `factor_name` matching one of the top-3 weighted factor names). This ensures positioning emphasizes what evaluators score most heavily.

**Pass:** 2+ of the top-3 weighted factors are referenced in theme_eval_mapping.

**Concern:** Exactly 1 of top-3 referenced — log advisory.

**Fail:** Zero of the top-3 weighted factors referenced in theme_eval_mapping. Positioning is misaligned with evaluation.

**Evidence to cite:** List top-3 factor names with weights; list which appear in any theme's mapping.

---

### Check 9 — UTF-8 round-trip, no mojibake

**Criterion:** File opens cleanly as UTF-8. No `Ã`, `â€`, `Â `, or replacement-character (`�`) patterns appear in the deserialized strings (tagline, value_proposition, evaluator headlines, ghost phrases).

**Pass:** UTF-8 decode succeeds AND zero mojibake patterns in any narrative string.

**Fail:** Decode error OR any mojibake substring found.

**Evidence to cite:** Field path + first 80 chars of the offending string, quoted.

---

### Check 10 — No `[:N]` truncation in deliverable strings

**Criterion:** Scan `tagline`, `value_proposition`, every `relevance_statement` in matched_projects, every `key_message` in evaluator_messages, and every ghost_phrase/counter_narrative for content ending mid-word (lowercase letter immediately followed by end-of-string, with no terminal punctuation). Apply the same disambiguation rule as the risk-register verifier (hyphenated line-wrap is NOT truncation).

**Pass:** Zero mid-word truncations.

**Fail:** Any deliverable string ends mid-word without hyphen + continuation.

**Evidence to cite:** Field path + offending string ending (last 50 chars).

---

## Disposition Logic

- **PASS:** All checks pass.
- **CONCERN:** Checks 3, 6, 7, or 8 in advisory band; all other checks pass.
- **FAIL:** Any of Checks 1, 2, 4, 5, 9, 10 fail OR Checks 3/6/7/8 fall below FAIL threshold.

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase8.0-positioning with the following targeted corrections:

[Check 1 fail] POSITIONING_OUTPUT.json missing or < 3KB ({actual_size} bytes).
  Action: Re-run the full phase from Step 1. Confirm Past_Projects.md is readable
  and company-profile.json loads cleanly.

[Check 2 fail] Missing top-level key(s): {list}.
  Action: Re-run Step 6 (Write Output) — ensure the positioning_output dict assembles
  all 10 required top-level keys. Do NOT short-circuit by skipping evaluator_messages
  or matched_evidence assembly.

[Check 3 fail] matched_projects has {N} entries (need 3+).
  Action: Confirm select_matching_projects() returned a non-empty list. If Past_Projects.md
  has <3 strong matches, the function should still return all available + set match_warning.
  Verify the function isn't filtering with [:N] slice or industry_counts >= 3 too aggressively.

[Check 4 fail] Themes with empty factor mapping: {list}.
  Action: build_theme_eval_mapping() did not find keyword overlap. Lower the
  `len(w) > 3` threshold in theme_words extraction OR ensure eval_factors have
  populated descriptions/subfactors. A theme like "Innovation" must map to at
  least one factor — if no overlap, broaden the alignment heuristic.

[Check 5 fail] section_theme_mandates empty or under-populated.
  Action: build_section_theme_mandates() needs `bid_section_mapping` or
  `EVALUATION_TO_BID_MAPPING` in EVALUATION_CRITERIA.json. If absent in source,
  derive defaults from eval_factor names by mapping factor -> primary_section
  (e.g., "Technical Approach" -> "03_TECHNICAL").

[Check 6 fail] Evaluator messages missing personas: {list}.
  Action: generate_evaluator_messages() iterates over a fixed 5-template dict.
  Confirm the loop ran to completion and didn't break early on a `weight` lookup error.

[Check 7 fail] Ghost strategy diverged from Phase 1.97 (incumbent mismatch or phrase drop).
  Action: Step 1d must load Phase 1.97's COMPETITIVE_POSITION.json (if it exists)
  and merge — not replace — its ghost_phrases and counter_narratives.

[Check 8 fail] Top-3 eval factors not referenced in theme_eval_mapping.
  Action: positioning themes ("Innovation", "Efficiency", "Reliability", "Partnership"
  in the default block) don't naturally align to factor names like "Technical Capability"
  or "Past Performance". Either rename themes to align with actual eval factor language
  OR enrich theme keywords during build_theme_eval_mapping.

[Check 9 fail] Mojibake found at {field_path}: "{snippet}".
  Action: Every open() and json.dump must use encoding='utf-8' and ensure_ascii=False.
  Audit all file writes in this phase.

[Check 10 fail] Mid-word truncation at {field_path}: "{ending}".
  Action: Remove any [:N] slice on tagline, value_proposition, relevance_statement,
  ghost_phrase, counter_narrative. Render full strings.

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input:**

```
POSITIONING_OUTPUT.json scenario: file size 2,100 bytes, matched_projects has 1 entry.
  theme_eval_mapping has 4 themes but 2 themes map to empty factor lists.
  evaluator_messages contains only TECHNICAL and EXECUTIVE.
  Phase 1.97 produced 6 ghost_phrases; this file has 0.
  tagline ends "Technology Solutions That Drive Resul"
```

Verifier MUST detect: Check 1 (under 3 KB), Check 3 (1 < 3 entries), Check 4 (2 themes with empty mappings), Check 6 (3 of 5 personas missing — FAIL band), Check 7 (phrases dropped from 6 to 0), Check 10 (truncated tagline). Disposition: FAIL.

**Known-good input:**

```
POSITIONING_OUTPUT.json: 12,400 bytes.
  matched_projects: 6 entries, ranks 1-6, scores 28-14, all have score_breakdown.
  win_themes: ["Right-Sized Partner", "Domain Expertise", "Predictable Delivery", "Risk-Managed Innovation"]
    — every theme maps to >= 2 eval factors in theme_eval_mapping.
  section_theme_mandates: 6 sections, each with 2+ themes and 1+ eval_factor.
  evaluator_messages: all 5 personas populated with headline + key_message + 3 proof_points.
  ghost_strategy: 7 ghost_phrases (Phase 1.97 had 6 — one added), incumbent matches Phase 1.97.
  Top-3 eval factors ("Technical Approach", "Past Performance", "Management") all appear in theme_eval_mapping.
  All strings end with terminal punctuation; no mojibake.
```

Verifier MUST PASS all checks. Disposition: PASS.

## Hard Rules (inherited from verifier-design principles)

1. NEVER claim POSITIONING_OUTPUT.json is missing without `Glob` verification first.
2. NEVER flag a theme as "unmapped" without first confirming `theme_eval_mapping[theme_name]` is the array used — some phases nest under `core_positioning.theme_eval_mapping`; check both paths.
3. NEVER fail Check 7 when Phase 1.97's COMPETITIVE_POSITION.json file genuinely doesn't exist — first-bid scenarios have no carry-forward source.
4. NEVER fail Check 8 on RFPs where evaluation factors are described in totally generic language ("Approach", "Qualifications") with no subfactor detail — log CONCERN instead.
5. Every finding must cite a specific field path + value (e.g., `evaluator_messages.OPERATIONAL absent`, NOT "operational message missing").
