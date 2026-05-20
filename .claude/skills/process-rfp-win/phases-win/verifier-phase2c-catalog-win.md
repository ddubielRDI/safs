---
name: verifier-phase2c-catalog-win
expert-role: Requirements Catalog Verifier
purpose: phase-boundary verifier for phase2c-catalog-win (catalog rendering completeness, table-cell truncation guard, RTM traceability preservation, no row caps)
created: 2026-05-20
---

# Verifier — Phase 2c Requirements Catalog

## When this runs

After phase2c-catalog-win reports done, BEFORE phase2d-coverage-win runs (and before any downstream phase reads REQUIREMENTS_CATALOG.md). This verifier exists because the catalog DOCX/MD is the human-facing deliverable AND because Phase 4 RTM reconstruction relies on REQUIREMENTS_CATALOG.json carrying source_ids, requirement_type, and parent_id forward (V2-F6 fix). Catalog is also the most common surface for the `_Showing N of M_` row-cap regression.

## Inputs (read in this order)

1. `{folder}/outputs/REQUIREMENTS_CATALOG.md` — primary human-facing deliverable
2. `{folder}/shared/REQUIREMENTS_CATALOG.json` — machine-readable companion
3. `{folder}/shared/requirements-normalized.json` — source of truth for catalog count parity
4. `{folder}/shared/domain-context.json` — confirms domain label flowed through
5. Prior verifier report at `{folder}/shared/validation/verifier-phase2c.json` (if a retry run)

## Verification Checks

### Check 1 — Both files exist and parse correctly

**Criterion:** `outputs/REQUIREMENTS_CATALOG.md` exists with size > 10,240 bytes (10 KB minimum — anything smaller indicates a near-empty render). AND `shared/REQUIREMENTS_CATALOG.json` exists, parses as valid JSON, with top-level keys: `generated_at`, `domain`, `total_requirements`, `categories`, `priority_summary`, `requirements`. Markdown file is UTF-8 readable.

**Pass:** Both files present, MD ≥10 KB, JSON parses, all top-level JSON keys present.

**Fail:** Either file absent OR MD <10 KB OR JSON parse error OR missing required keys.

**Evidence to cite:** Both file sizes in bytes. Missing key paths.

**Hard-rule reminder:** NEVER claim file is missing without `Glob` verification first.

---

### Check 2 — Catalog count parity with normalized requirements

**Criterion:** `REQUIREMENTS_CATALOG.json.total_requirements` MUST equal `len(REQUIREMENTS_CATALOG.json.requirements)` MUST equal `len(requirements-normalized.json.requirements)` (the valid requirements, not invalid_requirements). Tolerance: 0.

**Pass:** All three counts match exactly.

**Fail:** Any mismatch — catalog dropped, duplicated, or invented requirements.

**Evidence to cite:** All three counts. If mismatch: list up to 5 canonical_ids present in normalized but absent from catalog (or vice versa).

---

### Check 3 — RTM traceability fields preserved (V2-F6 fix)

**Criterion:** Every requirement in `REQUIREMENTS_CATALOG.json.requirements[]` has these fields populated (not missing, not null):
- `canonical_id` — non-empty string matching `\d{3,}[A-Z]{3}`
- `display_id` — non-empty string
- `text` — non-empty string
- `category` — non-empty string
- `priority` — value in {CRITICAL, HIGH, MEDIUM, LOW}
- `source_ids` — list, may be empty for non-pattern_extraction items but key MUST be present
- `requirement_type` — non-empty string (one of 7 valid types from Phase 2 Step 7b)

**Pass:** All requirements have all 7 fields populated.

**Fail:** Any requirement missing any of these fields. V2-F6 fix didn't fire OR catalog generator dropped fields.

**Evidence to cite:** Per missing field: count of offenders. List first 5 with `canonical_id`.

---

### Check 4 — REQUIREMENTS_CATALOG.md ≥10KB AND requirement count matches

**Criterion:** Catalog markdown contains all requirements visible. Heuristic: count lines starting with `| [` (table rows referencing display_id like `| [001APP] |`) — must equal or exceed `total_requirements`. Also count `#### [` headings (detailed view per requirement) — must also equal `total_requirements`.

**Pass:** Both counts (table rows + detailed headings) >= total_requirements.

**Concern:** Table-row count matches but detailed-heading count is lower — detailed section may have been truncated.

**Fail:** Table-row count < total_requirements — catalog rendering dropped rows.

**Evidence to cite:** Table-row count, detailed-heading count, expected total_requirements.

---

### Check 5 — No `_Showing N of M_` row-cap notices

**Criterion:** Grep `REQUIREMENTS_CATALOG.md` for any of: `_Showing \d+ of \d+_`, `Showing \d+ of \d+`, `... and \d+ more`, `\(\+\d+ more\)` AT THE TABLE/CATEGORY LEVEL. NOTE: the Quick Reference Index intentionally truncates priority lists with `(+N more)` per design — but a requirements TABLE must never carry such caps.

To distinguish: the `(+N more)` in Quick Reference is acceptable when it follows a list of canonical_ids in the Quick Reference Index section. It is unacceptable when it terminates a requirements TABLE or a category section.

**Pass:** Zero row-cap notices in category tables or detailed-view sections.

**Fail:** Any row-cap notice in a category table or detailed view.

**Evidence to cite:** Pattern, line number where found, surrounding context (3 lines).

---

### Check 6 — No mid-word truncation in table cells

**Criterion:** No table-cell content in `REQUIREMENTS_CATALOG.md` shows mid-word truncation. Specifically: for every line matching `| [` (a requirement row), the requirement text cell must NOT end with a hyphen, comma, or subordinator word (the/a/of/for/with/etc.) inside the cell (before the next `|`).

Per phase2c Step 2 (HUNT-B-003 fix), the only acceptable modification to requirement text is newline replacement and pipe escaping — NO truncation.

**Pass:** Zero mid-word truncations in table cells.

**Fail:** Any table cell whose requirement-text portion ends with hyphen/comma/subordinator.

**Evidence to cite:** First 3 offending lines with line numbers + the truncated text fragment.

---

### Check 7 — No embedded newlines in table cells (HUNT-B-003 fix)

**Criterion:** Grep for the regex `\|[^|]*\n[^|]` (a pipe followed by content, then a newline, then more content before the next pipe) — must return 0 matches. Embedded raw newlines break markdown table rendering.

**Pass:** Zero embedded newlines in table cells.

**Fail:** Any match — Step 2's newline-replacement was skipped or partially applied.

**Evidence to cite:** Line numbers of offending matches.

---

### Check 8 — Categories distribution reasonable (>3 categories)

**Criterion:** `REQUIREMENTS_CATALOG.json.categories` must contain more than 3 category keys with non-zero counts. If all requirements land in one or two categories (e.g., all TEC), the category inference in Phase 2 Step 7 likely defaulted everything to the fallback.

**Pass:** ≥4 categories with non-zero `count`.

**Concern:** 3 categories with non-zero count — log advisory, may be a small RFP.

**Fail:** ≤2 categories with non-zero count — categorization failed.

**Evidence to cite:** Print categories dict with counts. Flag the dominant category if >70% of total falls there.

---

### Check 9 — Table of Contents and Quick Reference Index present

**Criterion:** REQUIREMENTS_CATALOG.md MUST contain:
- A `## Table of Contents` section (grep `## Table of Contents` returns ≥1 hit)
- A category section per non-empty category (grep `^## [A-Z][A-Z][A-Z]:` returns ≥`len(categories)` hits)
- A Quick Reference Index (grep `Quick Reference Index` returns ≥1 hit)
- A `### By Priority` subsection (grep `### By Priority` returns ≥1 hit)

**Pass:** All four structural markers present.

**Fail:** Any missing — structural integrity broken.

**Evidence to cite:** Which markers were found (✓) vs missing (✗).

---

### Check 10 — UTF-8 round-trip and universal anti-regression

**Criterion:** Open both files with `encoding='utf-8'`. Scan all content for mojibake sentinels (`�`, `Ã©`, `â€™`, `â€"`) — count must be 0 (excluding occurrences inside string-escaped fields where they're intentional, which is essentially nowhere). Scan for `[:N]` literal slicing leakage in deliverable strings — count must be 0.

**Pass:** Zero hits on sentinels.

**Fail:** Any sentinel found.

**Evidence to cite:** Sentinel pattern, count, first 3 line numbers / field paths.

---

## Disposition Logic

- **PASS:** Checks 1, 2, 3, 4, 5, 6, 7, 9, 10 all pass AND Check 8 is PASS or CONCERN.
- **CONCERN:** Check 4 has table-row match but detailed-section lower OR Check 8 has only 3 categories. Log advisory.
- **FAIL:** Any of Checks 1, 2, 3, 5, 6, 7, 9, 10 fail OR Check 4 has table-row count < total OR Check 8 ≤ 2 categories.

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase2c-catalog-win with the following targeted corrections:

[Check 1 fail] Catalog files missing or undersized.
  Action: Confirm Step 4 (write_file MD) AND Step 5 (write_json JSON) both executed.
  10 KB minimum for MD ensures it's not a stub. If <10 KB, requirements list was empty
  or generate_catalog_md returned a near-empty string.

[Check 2 fail] Catalog count {N} does not match normalized count {M}.
  Action: Audit Step 1 — confirm `valid_reqs = requirements.get("requirements", [])`
  is reading the right key. Phase 2b emits valid items under "requirements", invalid
  items under "invalid_requirements". Catalog must source from "requirements" only.

[Check 3 fail] {N} requirements in catalog missing required fields (V2-F6 fix didn't fire).
  Action: Audit Step 5 json_catalog requirements list-comp — confirm ALL these keys are
  emitted:
    canonical_id, display_id, text, category, priority, type, source,
    validation_score, requirement_type, source_ids, parent_id
  The V2-F6 fix added the last three (requirement_type, source_ids, parent_id). Without
  them, Phase 4 RTM cannot rebuild traceability chains from the catalog.

[Check 4 fail] Catalog markdown missing rows. Table-row count {N} < total {M}.
  Action: Audit Step 2 generate_catalog_md — confirm the inner loop `for req in sorted(reqs, ...)`
  emits one row per requirement. If categories dict is built incorrectly, some
  requirements may be excluded from a category and silently dropped.

[Check 5 fail] Row-cap notice found in catalog tables/detailed view.
  Action: Audit Step 2 — confirm NO `[:N]` slicing on the requirements iterable,
  NO `if i < 20: ... else: break` loops. The catalog must render every requirement,
  every row, every detailed section. Reference SAFS memory:
  feedback_screen_encoding_truncation.md — this is a recurring regression.

[Check 6 fail] {N} mid-word table-cell truncations.
  Action: Audit Step 2 — confirm `text = req.get("text", "")` is followed only by
  newline replacement and pipe escaping. There must be NO truncation. If truncations
  are surviving, the upstream phase2b validate_requirement failed to route them
  to invalid_requirements — re-run phase2b verifier first.

[Check 7 fail] Embedded newline in table cell.
  Action: Audit Step 2 — confirm HUNT-B-003 fix lines:
    text = text.replace("\r\n", " ").replace("\n", " ").replace("\r", " ")
    text = text.replace("|", "\\|")
  Order matters: newline replace BEFORE pipe escape, BOTH before the row template.

[Check 8 fail] Only {N} categories have non-zero count.
  Action: Audit upstream Phase 2 Step 7 (categorize_requirement) — if all requirements
  land in TEC (the fallback), the CATEGORIES dict keywords don't match the source corpus.
  Consider expanding keyword lists per category. NOT a phase2c fix — escalate to phase2.

[Check 9 fail] Missing TOC / category sections / Quick Reference / By Priority.
  Action: Audit Step 2 generate_catalog_md — confirm the four structural blocks are
  unconditionally emitted: TOC, per-category ##, Quick Reference Index, By Priority.

[Check 10 fail] Mojibake / [:N] leakage in deliverables.
  Action: Universal anti-regression. Audit every open() for encoding='utf-8',
  every write_file/write_json for UTF-8 enforcement.

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input:**

```
REQUIREMENTS_CATALOG.md scenario:
  File size: 8,400 bytes  (< 10 KB threshold)
  287 normalized requirements expected
  Table-row count in MD: 100
  Line 142: "| [101TEC] | The system shall maintain a "primary | HIGH | manual |"
    (quote-internal truncation surviving from phase2)
  Line 287: "_Showing 100 of 287 requirements — see full JSON_"  (ROW-CAP REGRESSION)
  Quick Reference Index section: MISSING
  Categories with non-zero count: TEC (250), APP (37) — only 2 categories
  REQUIREMENTS_CATALOG.json.requirements[0] missing source_ids, requirement_type, parent_id
```

Verifier MUST detect: Check 1 (8.4 KB < 10 KB), Check 2 (100 vs 287), Check 3 (missing V2-F6 fields), Check 4 (100 < 287), Check 5 (row-cap notice), Check 8 (only 2 categories), Check 9 (Quick Reference missing). Disposition: FAIL.

**Known-good input:**

```
REQUIREMENTS_CATALOG.md scenario:
  File size: 184,200 bytes
  287 requirements, 287 table rows, 287 detailed sections
  All 8 categories (APP, ENR, BUD, STF, RPT, SEC, INT, UI, TEC, ADM) have non-zero counts
  No row-cap notices, no mid-word truncations, no embedded newlines
  TOC present, all 8 category sections present, Quick Reference Index present, By Priority present
  REQUIREMENTS_CATALOG.json: all 287 requirements have canonical_id, display_id, text,
    category, priority, source_ids, requirement_type. parent_id populated where applicable.
  No mojibake; no [:N] leakage.
```

Verifier MUST PASS all checks. Disposition: PASS.

## Hard Rules (inherited from verifier-design principles)

1. NEVER claim REQUIREMENTS_CATALOG.md or .json is missing without `Glob` verification first.
2. NEVER flag `(+N more)` in the Quick Reference Index as a row-cap regression — that's documented design for the index priority list ONLY. Row caps in category tables ARE regressions.
3. NEVER flag the `source` field's `[:15]` slicing as a `[:N]` violation — `source` is a documented short-identifier field, not a deliverable string.
4. Every finding must cite a specific line number in the MD OR JSON field path + value.
5. On FAIL, return corrective instructions referencing the specific Step / fix ID (V2-F6, HUNT-B-003) so the agent can locate the exact code block to repair.
6. The 10 KB minimum size gate (Check 1) is a fast detector for near-empty renders — never skip it even if requirement-count parity looks correct in the JSON.
