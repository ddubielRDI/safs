---
name: verifier-phase7-validation-win
expert-role: QA Validation Integrity Verifier
purpose: phase-boundary verifier for phase7-validation-win (structural validation results, benchmark gap analysis, severity tagging, gap-to-requirement linkage)
created: 2026-05-20
---

# Verifier — Phase 7 Quality Validation & Gap Analysis

## When this runs

After phase7-validation-win reports done, BEFORE the next downstream consumer (Stage 7 / SVA-7) reads validation-results.json or GAP_ANALYSIS.md.

## Inputs (read in this order)

1. `{folder}/shared/validation-results.json` — primary output under verification (combined structural + gap data)
2. `{folder}/outputs/GAP_ANALYSIS.md` — human-readable companion deliverable
3. `{folder}/shared/requirements-normalized.json` — source for requirement-count benchmark math + cross-ref ID set
4. `{folder}/outputs/*.md` — file listing used to confirm `summary.total` matches actual emitted deliverable count
5. Any prior verifier report at `{folder}/shared/validation/verifier-phase7.json` (if a retry run)

## Verification Checks

### Check 1 — Both output files exist and parse correctly

**Criterion:** `shared/validation-results.json` exists AND parses as valid JSON. `outputs/GAP_ANALYSIS.md` exists AND is non-empty. JSON file size > 500 bytes; markdown file size > 512 bytes.

**Pass:** Both files present, JSON parses without error, sizes above floors.

**Fail:** Either file absent, JSON parse error, or either file under the size floor (indicates a stub or write-failure).

**Evidence to cite:** Actual file sizes (e.g., `validation-results.json = 12,847 bytes`, `GAP_ANALYSIS.md = 8,123 bytes`) and JSON parse status.

**Hard-rule reminder:** NEVER claim a file is missing without `Glob` verification first.

---

### Check 2 — UTF-8 round-trip with no mojibake

**Criterion:** Both files open cleanly with `encoding='utf-8'`. No mojibake sequences (`â€™`, `â€œ`, `â€`, `Ã©`, `Â`, `�`) present in either file. JSON written with `ensure_ascii=False` (verify by spot-checking non-ASCII chars survive as UTF-8, not as `\uXXXX` escapes when source contained them).

**Pass:** UTF-8 reads succeed; zero mojibake matches across both files.

**Fail:** Any mojibake sequence found, or UnicodeDecodeError on open with utf-8.

**Evidence to cite:** Grep result counts for each mojibake pattern in each file (e.g., `validation-results.json: 0 matches; GAP_ANALYSIS.md: 0 matches`).

---

### Check 3 — Schema fidelity to phase7 contract

**Criterion:** `validation-results.json` top-level keys include all of: `validated_at`, `files`, `summary`, `gap_analysis`. `summary` contains `total`, `passed`, `failed`, `warnings` (all integers, total >= passed + failed differing only by warning-bearing entries). `gap_analysis` contains `requirements`, `specifications`, `gaps`, `overall_score`.

**Pass:** All required keys present at all nesting levels; `summary.total >= summary.passed` and `summary.total >= summary.failed`.

**Fail:** Any required key absent, `summary` counts internally inconsistent (passed + failed > total), or `overall_score` not present.

**Evidence to cite:** List actual top-level keys found (e.g., `validation-results.json top-level keys = ['validated_at', 'files', 'summary', 'gap_analysis']`). Cite the specific missing key path.

---

### Check 4 — Structural validation results enumerated per file

**Criterion:** `validation-results["files"]` is a non-empty array. Each entry has `file`, `checks`, `passed` (bool), `errors`, `warnings`. Length of `files` must be >= 5 (at minimum the 5 phase-defined `VALIDATION_RULES` deliverables: REQUIREMENTS_CATALOG.md, ARCHITECTURE.md, SECURITY_REQUIREMENTS.md, EFFORT_ESTIMATION.md, TRACEABILITY.md). No silent omission of a deliverable.

**Pass:** `len(files) >= 5`; every entry has all required keys; every entry's `file` value is a basename string.

**Concern:** `len(files)` between 5 and 8 — log advisory (some optional deliverables not validated).

**Fail:** `len(files) < 5`, or any entry missing a required key, or any of the 5 mandatory deliverable filenames absent from the `file` field set.

**Evidence to cite:** `summary.total = N`, list of missing mandatory deliverables from the validated set (e.g., "TRACEABILITY.md not enumerated in files[].file").

---

### Check 5 — Benchmark gap analysis present with requirements + specifications + gaps

**Criterion:** `gap_analysis.requirements` is a dict containing `total`, `target`, `gap`, `meets_target`. `gap_analysis.specifications` is a dict with at least one entry (e.g., `architecture`, `security`, `demos`). `gap_analysis.gaps` is an array (may be empty). `gap_analysis.overall_score` is a float between 0 and 100 inclusive.

**Pass:** All four sub-fields present and well-typed; `overall_score` numeric and in range.

**Fail:** Any sub-field missing or wrong type; `overall_score` outside [0, 100] or non-numeric.

**Evidence to cite:** Print `gap_analysis.requirements.total / target / meets_target`, `len(gap_analysis.specifications)`, `len(gap_analysis.gaps)`, `gap_analysis.overall_score`.

---

### Check 6 — Every gap has area, severity (CRITICAL/HIGH/MEDIUM/LOW), description, recommendation, and (where possible) is tied to a specific requirement or spec

**Criterion:** For each entry in `gap_analysis.gaps`, all of `area`, `severity`, `description`, `recommendation` keys exist and have non-empty string values. `severity` value MUST be one of: `CRITICAL`, `HIGH`, `MEDIUM`, `LOW`. Each gap whose `area` references "Requirements" or names a specific spec document SHOULD include a citation in `description` (e.g., a benchmark figure, a count comparison, or a referenced spec file). No silent gap suppression: if `summary.failed > 0` OR `requirements.meets_target == false` OR any spec entry has `meets_target == false`, THEN `len(gaps) > 0`.

**Pass:** All gaps well-formed; severity in allowed set; gap count consistent with failed/missed-target signals upstream.

**Concern:** A gap entry lacks a recommendation OR severity is set but no requirement/spec citation appears in `description`. Log advisory.

**Fail:** Any gap missing a required field; any severity outside `{CRITICAL, HIGH, MEDIUM, LOW}`; OR silent suppression detected (failed/missed-target signals upstream but `len(gaps) == 0`).

**Evidence to cite:** For failing entries: index N, missing field name, or invalid severity value. For silent-suppression: `summary.failed = X, requirements.meets_target = false, len(gaps) = 0` (the contradiction itself).

---

### Check 7 — No `[:N]` truncation in deliverable strings and no `_Showing N of M_` row-cap notices in GAP_ANALYSIS.md

**Criterion:** Grep `GAP_ANALYSIS.md` for the literal pattern `_Showing ` — must return 0 matches. Grep `GAP_ANALYSIS.md` for the regex `\[:[0-9]+\]` (Python slice notation surviving into the deliverable) — must return 0 matches. Grep for em-dash mojibake (`—` rendered as `â€"`) — must return 0 matches. No table cells should end mid-word (terminal-cell check on the closing `|` of each row — last token before `|` must end on a word boundary or punctuation).

**Pass:** All three greps return 0; no mid-word terminations detected.

**Fail:** Any grep returns >0 matches OR any table row has a mid-word terminal cell.

**Evidence to cite:** Grep counts per pattern; file:line cite for any mid-word terminations found.

---

### Check 8 — Cross-reference integrity: requirement IDs in GAP_ANALYSIS resolve to requirements-normalized.json

**Criterion:** Extract all requirement-id-shaped tokens (regex `\b\d{3}[A-Z]{2,3}\b`) from `GAP_ANALYSIS.md`. Build set of canonical IDs from `requirements-normalized.json["requirements"][*].canonical_id`. Every extracted token MUST exist in the canonical set. Tolerance: 0 invalid references.

**Pass:** All referenced IDs resolve to a real requirement in normalized source.

**Fail:** Any orphan ID (referenced in markdown but not present in canonical set).

**Evidence to cite:** List up to 5 orphan IDs and their context line numbers in GAP_ANALYSIS.md.

---

### Check 9 — Cross-stage consistency: `summary.total` equals count of validated deliverables on disk

**Criterion:** `summary.total` must equal the count of entries in `files`, which in turn must equal the count of *.md files in `{folder}/outputs/` plus the count of JSON files that phase7's `JSON_VALIDATIONS` dict explicitly enumerates and that exist on disk.

**Pass:** Counts agree exactly.

**Fail:** Mismatch in either direction.

**Evidence to cite:** `summary.total = N`, `len(files) = M`, `count of {folder}/outputs/*.md = X`, `count of validated shared/*.json = Y`. Identify any deliverable on disk not in `files`, or vice versa.

---

## Disposition Logic

- **PASS:** Checks 1, 2, 3, 4, 5, 6, 8, 9 all pass AND Check 7 returns 0 across all patterns.
- **CONCERN:** Check 4 in advisory band (5 <= files < 8) OR Check 6 advisory (gaps missing recommendation citations but otherwise well-formed). Log + continue.
- **FAIL:** Any of Checks 1, 2, 3, 5, 7, 8, 9 fail OR Check 6 fails (invalid severity, silent gap suppression, or missing required gap field).

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase7-validation with the following targeted corrections:

[Check 1 fail] validation-results.json or GAP_ANALYSIS.md missing / undersized.
  Action: Re-run Step 3 (Merge Results) and Step 4b (Generate Gap Analysis Document).
  Verify the write_json and write_file calls completed without exception and that
  upstream req_assessment, spec_assessment, and gaps were populated before merge.

[Check 2 fail] Mojibake or non-UTF-8 bytes detected in {file}.
  Action: Ensure every open() in this phase uses encoding='utf-8' and every
  json.dump uses ensure_ascii=False. Re-emit the affected file from in-memory data,
  do NOT in-place "fix" mojibake (the underlying source is the bug, not the output).

[Check 3 fail] validation-results.json missing top-level key {key} OR summary counts
  internally inconsistent (passed + failed > total).
  Action: Audit Step 5a (Report Structural Results) and Step 3 (Merge Results). The
  summary dict must be authored by aggregating per-file results, not from a hardcoded
  literal. Check that `validation_results["gap_analysis"]` is assigned BEFORE write_json.

[Check 4 fail] files[] has fewer than 5 entries or a mandatory deliverable name is
  absent: {list of missing}.
  Action: Step 2a iterates output_files via glob — verify the glob pattern matches
  what was actually produced. If a deliverable is genuinely absent from outputs/,
  the upstream phase (not phase7) is the root cause; report up.

[Check 5 fail] gap_analysis missing sub-field {requirements|specifications|gaps|overall_score}.
  Action: Step 3 (Merge Results) builds gap_analysis dict — verify all four assignments
  fire. overall_score must be returned as a float; cast via float() before write.

[Check 6 fail] Gap entries malformed — {N} gaps with invalid severity or missing
  recommendation, OR silent suppression detected (summary.failed = X but len(gaps) = 0).
  Action: identify_gaps() in Step 3b must emit a gap for every benchmark miss AND every
  validation failure. Severity vocabulary is locked to CRITICAL/HIGH/MEDIUM/LOW —
  remove any other values. Cross-check: if req_assessment["meets_target"] is False,
  a Requirements gap MUST be appended. If any spec entry has meets_target=False,
  a Specification: {name} gap MUST be appended.

[Check 7 fail] Truncation pattern or "_Showing N of M_" notice found in GAP_ANALYSIS.md.
  Action: Audit any list-rendering in Step 4b — replace `[:N]` slice operations with
  full enumeration (e.g., loop over all gap entries, not just gaps[:5]). Remove any
  "_Showing N of M_" or "..." row-cap notice. Phase7 deliverables must show ALL gaps.

[Check 8 fail] Orphan requirement ID {ID} referenced in GAP_ANALYSIS.md.
  Action: validate_cross_references() in Step 4a already detects this — escalate the
  cross_reference_issues array into a FAIL instead of silently logging. The phase
  must not emit GAP_ANALYSIS.md with broken citations.

[Check 9 fail] summary.total mismatches actual deliverable count.
  Action: glob in Step 2a may have excluded GAP_ANALYSIS.md itself (it is emitted by
  this same phase) — verify the glob runs BEFORE GAP_ANALYSIS.md is written, OR
  filter it out explicitly so the count remains stable.

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input:**

```
validation-results.json scenario: summary.total = 4 (TRACEABILITY.md missing from files[]).
  gap_analysis.requirements.meets_target = false (211/247) but gap_analysis.gaps = [].
  gap_analysis.gaps[0].severity = "MAJOR" (invalid value).
  GAP_ANALYSIS.md contains "_Showing 5 of 23 gaps_".
  GAP_ANALYSIS.md references [047SEC] which is absent from requirements-normalized.json.
```

Verifier MUST detect: Check 4 (TRACEABILITY.md missing), Check 6 (silent suppression + invalid severity), Check 7 (row-cap notice), Check 8 (orphan ID). Disposition: FAIL.

**Known-good input:**

```
validation-results.json scenario: summary.total = 7, all 5 mandatory deliverables present.
  summary.passed = 6, summary.failed = 1, summary.warnings = 3.
  gap_analysis.requirements.total = 247, target = 247, meets_target = true.
  gap_analysis.specifications has 3 entries, all meets_target = true.
  gap_analysis.gaps = [1 entry — derived from the 1 structural failure].
    gap[0].severity = "MEDIUM", recommendation populated, area cites a real deliverable.
  gap_analysis.overall_score = 87.3.
  GAP_ANALYSIS.md: 0 row-cap notices, 0 [:N] patterns, 0 mojibake, all req IDs resolve.
```

Verifier MUST PASS all checks. Disposition: PASS.

## Hard Rules (inherited from verifier-design principles)

1. NEVER claim a file is missing without `Glob` verification first.
2. NEVER report a gap as "missing severity" if the severity field is present and equals one of `{CRITICAL, HIGH, MEDIUM, LOW}` — verify actual content, not key absence.
3. NEVER allow `_Showing N of M_` notices or `[:N]` slicing to ship in deliverable markdown (per SAFS memory: three regressions documented across screen + win pipelines).
4. Silent gap suppression is the highest-severity failure mode for this phase — if upstream signals (failed validations, missed benchmarks) say gaps exist but `gap_analysis.gaps == []`, that is an automatic FAIL even if every other check passes.
5. Every finding must cite a specific field path + value or count (e.g., `gap_analysis.gaps[2].severity = "MAJOR" (invalid)`).
6. On FAIL, return corrective instructions with the specific field path or numeric discrepancy so the phase agent can target the exact repair without re-running everything.
