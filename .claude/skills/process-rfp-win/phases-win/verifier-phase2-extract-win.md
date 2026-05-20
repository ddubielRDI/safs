---
name: verifier-phase2-extract-win
expert-role: Requirements Extraction Verifier
purpose: phase-boundary verifier for phase2-extract-win (V3 extraction-quality gates — scope filter, soft-wrap, ToC chrome, quote-aware terminator, question-mark, section heading, contract-clause leakage)
created: 2026-05-20
---

# Verifier — Phase 2 Requirements Extraction (V3 quality gates)

## When this runs

After phase2-extract-win reports done, BEFORE phase2b-normalize-win runs. This verifier exists because MARS round 1 (catalog) had 18-28% truncations, 383 contract clauses misclassified as requirements, and 30-40% near-dup inflation. All six V3 fixes (scope filter, soft-wrap join, quote-aware terminator, ToC chrome rejection, question-mark filter, section-heading resolver) must be empirically verified on output, not assumed from code presence.

## Inputs (read in this order)

1. `{folder}/shared/requirements-raw.json` — primary output under verification
2. `{folder}/shared/sample-data-analysis.json` — Step 7c side-output (also required)
3. `{folder}/shared/workflow-extracted-reqs.json` — confirms upstream Phase 2a input flowed in
4. `{folder}/flattened/*.md` — source corpus for spot-checks
5. Prior verifier report at `{folder}/shared/validation/verifier-phase2-extract.json` (if a retry run)

## Verification Checks

### Check 1 — File exists and is valid JSON, schema-structurally valid

**Criterion:** `shared/requirements-raw.json` exists AND parses as valid JSON AND contains top-level keys: `extracted_at`, `summary`, `category_distribution`, `source_type_distribution`, `requirement_type_classification`, `requirements`, `rtm_rfp_sources`. The `summary` block must contain `total_requirements`, `from_pattern_extraction`, `from_sub_item_promotion`, `from_workflow_extraction`, `target_achieved`. File size > 1,024 bytes.

ALSO: `shared/sample-data-analysis.json` exists AND parses AND has `summary` + `field_definitions` + `entities` keys (Step 7c output).

**Pass:** Both files present, all keys present, JSON parses without error.

**Fail:** Either file absent, JSON parse error, or any required key missing.

**Evidence to cite:** Specific missing key path. File sizes for both.

**Hard-rule reminder:** NEVER claim file is missing without `Glob` verification first.

---

### Check 2 — V3-F1: Non-requirements docs excluded from extraction (contract-clause leakage defense)

**Criterion:** No requirement in `requirements[]` has a `source_file` matching the EXTRACTION_BLACKLIST_PATTERNS: sample, xaas-contract, disclosure-exemption-affidavit, proposer-information, reference-check, responsibility-inquiry, cost-proposal, vendor_pricing, vendor-pricing. AND no requirement text matches the boilerplate sentinels: `Contractor shall`, `Proposer certifies`, `indemnif`, `force majeure`, `Maximum Not-To-Exceed`.

**Pass:** Zero requirements from blacklisted source files AND zero requirements matching boilerplate sentinels.

**Fail:** Any requirement with `source_file` matching a blacklist pattern OR any requirement text matching a boilerplate sentinel.

**Evidence to cite:** Count of requirements per blacklisted source_file. List up to 5 offending `req.id` + `source_file` + first 100 chars of text. List up to 5 offending boilerplate matches with `req.id` + matched sentinel.

**Why this matters:** MARS round 1 ingested 383 contract clauses from Attachment-A as system requirements. This is the single highest-severity regression to defend against.

---

### Check 3 — V3-F2: Soft-wrap join applied (no mid-word truncation in requirement text)

**Criterion:** Count requirements whose `text` field ends with: `-` (hyphen), `,` (comma), OR a subordinator word from the set {the, a, an, or, and, to, of, for, with, by, from, in, on, at, as, if, that, which, such, including, associating, those}. The count MUST be 0.

**Pass:** Zero requirements end with hyphen/comma/subordinator.

**Fail:** Any requirement text ends with these terminators — indicates soft-wrap PDF line breaks were NOT joined before extraction regex ran.

**Evidence to cite:** Count of offending requirements. List first 5 with `req.id` + last 40 chars of text + which terminator class.

---

### Check 4 — V3-F8: Quote-aware terminator (no quote-internal truncation)

**Criterion:** No requirement text ends in a lowercase word immediately preceded by an opening quote without a matching close. Specifically: grep requirements[].text for the regex `"[a-z]+$` (a quote followed by lowercase letters at end of string). Count must be 0.

**Pass:** Zero quote-internal truncations.

**Fail:** Any match — Rule 3 sub-item terminator stopped at a quote-internal period (e.g., the text `the system shall maintain a "primary` was captured because the regex hit the period in `"primary."` and stopped there).

**Evidence to cite:** Count, then list first 3 offending `req.id` + last 50 chars showing the open-quote pattern.

---

### Check 5 — V3-F10: Question-mark filter (no checklist prompts as requirements)

**Criterion:** Zero requirements in `requirements[]` have a `text` field ending in `?` (after `rstrip()`).

**Pass:** Zero requirements end with question mark.

**Fail:** Any requirement ends with `?` — these are proposer self-evaluation checklist questions (e.g., "notify users of various actions?") that were captured by REQUIREMENT_PATTERNS but should be rejected.

**Evidence to cite:** Count, then list first 3 offending `req.id` + full text.

---

### Check 6 — V3-F9: ToC chrome filter on section field

**Criterion:** No requirement's `section` field contains 4+ consecutive dots (leader-dot pattern `\.{4,}`) OR a trailing page number (regex `\s+\d{1,4}\s*$`). AND `section` is not a generic fallback like "(unknown source file)" for more than 20% of requirements — at least 80% must have a real numbered section identifier.

**⛔ Scope narrowing (codified 2026-05-20 — V3-Q25, MARS Phase 2 retry-2 incident):** The 80% numbered-section bar applies ONLY to requirements whose `source` is `pattern_extraction` or `sub_item_promotion`. Requirements with `source == "workflow_extraction"` (merged from Phase 2a workflow-extracted-reqs.json) legitimately have NO RFP section heading — they're derived from workflow steps, not from numbered RFP sections. Including them in the denominator artificially depresses the ratio to ~49% even when the actual ToC-chrome filter is fully clean. Filter before computing:

```python
target_pop = [r for r in requirements if r.get("source") in ("pattern_extraction", "sub_item_promotion")]
numbered = [r for r in target_pop if re.match(r'\d+\.\d+', r.get('section', ''))]
ratio = len(numbered) / max(len(target_pop), 1)
```

**Pass:** Zero ToC-chrome sections AND ≥80% of `target_pop` (pattern + sub-item requirements) have a numbered section identifier matching `\d+\.\d+`.

**Concern:** 70-80% have numbered identifiers within `target_pop`. Log advisory.

**Fail:** Any section contains leader dots / page-number suffix OR <70% of `target_pop` requirements have a real numbered section.

**Evidence to cite:** Count of ToC-chrome matches. Count + % of requirements with `\d+\.\d+` section pattern. List first 3 offending sections.

---

### Check 7 — Source ID coverage and rfp_sources accumulation

**Criterion:** Every requirement with `source == "pattern_extraction"` has `source_ids[]` non-empty (length >= 1) AND `source_file` populated to a real filename (not "unknown"). Every requirement with `source == "sub_item_promotion"` may have inherited source_ids — check `parent_id` resolves to a requirement that itself has source_ids. AND `rtm_rfp_sources[]` has at least as many entries as the distinct source_ids referenced from requirements[].

**Pass:** All pattern_extraction requirements have non-empty source_ids and source_file != "unknown". rtm_rfp_sources accumulated.

**Fail:** Any pattern_extraction requirement missing source_ids, OR rtm_rfp_sources count is less than the union of source_ids referenced from requirements.

**Evidence to cite:** Count of pattern_extraction requirements with empty source_ids (must be 0). rtm_rfp_sources count vs distinct source_ids referenced.

---

### Check 8 — Target requirement count (advisory, not hard-block)

**Criterion:** `summary.total_requirements >= 247` (legacy target) OR `summary.total_requirements >= 50` (minimum credible RFP signal). The 247 target is a stretch goal documented in the phase file; the 50-floor is a structural integrity check.

**Pass:** Total >= 247.

**Concern:** Total between 50 and 246. Log as advisory; phase4 traceability and phase2d coverage will run, but call out the smaller catalog so downstream phases set expectations correctly.

**Fail:** Total < 50 — almost certainly an extraction failure (regex misfire, blacklist over-broad, or source-doc misclassification).

**Evidence to cite:** `summary.total_requirements`, `summary.from_pattern_extraction`, `summary.from_sub_item_promotion`, `summary.from_workflow_extraction`.

---

### Check 9 — Every requirement type-classified (Step 7b output)

**Criterion:** Every requirement in `requirements[]` has `requirement_type` set to one of the 7 valid types: FUNCTIONAL, NON_FUNCTIONAL, CONSTRAINT, INTERFACE, SECURITY, COMPLIANCE, DATA. Also has `type_confidence` in {high, medium, low}.

**Pass:** All requirements have a valid `requirement_type` value AND `type_confidence` is set.

**Fail:** Any requirement with null/missing `requirement_type` or value outside the 7-set.

**Evidence to cite:** Count of missing/invalid. List first 5 offending `req.id`.

---

### Check 10 — UTF-8 round-trip and universal anti-regression

**Criterion:** Open `requirements-raw.json` with `encoding='utf-8'`. Scan all string values for mojibake sentinels (`�`, `Ã©`, `â€™`, `â€"`) — count must be 0. Scan all string values for `_Showing \d+ of \d+_` row-cap notice — count must be 0. Scan for `[:N]` literal slicing leakage (e.g., `[:100]` appearing as a string inside emitted content) — count must be 0.

**Pass:** Zero hits on all three sentinels.

**Fail:** Any sentinel found.

**Evidence to cite:** Sentinel pattern, count, first 3 field paths.

---

## Disposition Logic

- **PASS:** Checks 1, 2, 3, 4, 5, 6, 7, 9, 10 all pass AND Check 8 is PASS or CONCERN.
- **CONCERN:** Check 8 is CONCERN (50-246 requirements) OR Check 6 is in 70-80% advisory band. Continue but log warning.
- **FAIL:** Any of Checks 1, 2, 3, 4, 5, 7, 9, 10 fail OR Check 6 below 70% OR Check 8 below 50.

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase2-extract-win with the following targeted corrections:

[Check 1 fail] requirements-raw.json or sample-data-analysis.json missing or invalid.
  Action: Verify Step 8 (write requirements-raw.json) AND Step 7c (write sample-data-analysis.json)
  both executed. Both files MUST exist before phase2b runs.

[Check 2 fail] Contract-clause / boilerplate leakage. {N} requirements from {blacklisted source}.
  Action: Re-audit Step 1 EXTRACTION_BLACKLIST_PATTERNS. Confirm is_requirement_source()
  is called on every file before adding to combined_content. The exclusion list MUST include:
  sample, xaas-contract, disclosure-exemption-affidavit, proposer-information,
  reference-check, responsibility-inquiry, cost-proposal, vendor_pricing, vendor-pricing.
  Print log of EXCLUDED files to confirm gate worked.

[Check 3 fail] {N} requirements end in hyphen/comma/subordinator (truncation).
  Action: Audit Step 1 collapse_soft_wraps() — confirm it runs BEFORE Step 3 regex extraction.
  Print pre/post char counts. If count delta is near 0, soft-wrap isn't joining.
  Verify Rule 3 sub-item terminator does NOT include `\n` in its terminator class
  (V3-F2 fix removed it).

[Check 4 fail] {N} requirements have quote-internal truncation.
  Action: Audit Step 4 Rule 3 shall_pattern — must use the V3-F8 lookahead:
  `(?:[.;](?![\"\'])(?=\s|$)|\n\n|$)`. The (?![\"\']) negative lookahead is mandatory.

[Check 5 fail] {N} requirements end with '?'.
  Action: Add filter in Step 3: after text capture, `if text.rstrip().endswith("?"): continue`.
  These are proposer checklist questions, not requirements.

[Check 6 fail] ToC chrome in section field OR <80% have numbered sections.
  Action: Audit Step 7d find_section_heading() — confirm TOC_CHROME_RE filter rejects
  candidates with 4+ leader dots or trailing page numbers. If <80% have numbered IDs,
  the file-content cache may be missing data or the SECTION_HEADING_PATTERNS list is
  too narrow.

[Check 7 fail] {N} pattern_extraction requirements lack source_ids.
  Action: Audit Step 7d — confirm the source_id_counter loop assigns source_ids[]
  to every requirement, not just those whose source_file was found. The fallback
  branch must still emit a SRC-NNN entry pointing to "unknown".

[Check 8 concern/fail] Total {N} requirements (< target).
  Action: If <50: investigate regex misfire — print REQUIREMENT_PATTERNS hit counts
  per pattern. Possible that EXTRACTION_BLACKLIST is over-broad and excluded the
  RFP body itself.

[Check 9 fail] {N} requirements lack requirement_type classification.
  Action: Audit Step 7b — confirm classify_requirement_type() is called on EVERY
  requirement, including workflow-merged ones added in Step 6.

[Check 10 fail] Mojibake / row-cap / [:N] leakage found.
  Action: Universal anti-regression — audit every open() for encoding='utf-8',
  every json.dump for ensure_ascii=False. Reference SAFS memory:
  feedback_screen_encoding_truncation.md.

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input (modeled on MARS round-1 regression):**

```
requirements-raw.json scenario:
  summary.total_requirements = 487  (looks healthy but contaminated)
  Of these, 383 have source_file = "Attachment-A-Sample-XaaS-Contract-Final.md"
    and text starting with "Contractor shall" or containing "indemnif"
  18 requirements text ends with "tamper-", "secure," or "the"  (soft-wrap truncation)
  6 requirements text ends with `?` ("notify users of various actions?")
  41 requirements section field reads "10.7 Annual Report Filing Decision Tree ........  42"
  3 requirements text ends with `"primary` (quote-internal truncation)
```

Verifier MUST detect: Check 2 (383 from blacklisted source), Check 3 (18 truncations), Check 4 (3 quote-internal), Check 5 (6 question marks), Check 6 (41 ToC chrome). Disposition: FAIL.

**Known-good input:**

```
requirements-raw.json scenario:
  summary.total_requirements = 264  (above 247 target)
  source_files: all reference RFP body + Att-H + workflow source (no blacklisted files)
  No requirement text ends with hyphen/comma/subordinator/?
  No quote-internal truncation
  92% of requirements have section = "3.X.Y Title" pattern
  All requirements have requirement_type set; 187 high confidence, 65 medium, 12 low
  rtm_rfp_sources has 264 entries matching source_ids referenced from requirements
  sample-data-analysis.json present with 4 spreadsheets, 47 entities, 312 field definitions
  No mojibake, no _Showing N of M_, no [:N] leakage.
```

Verifier MUST PASS all checks. Disposition: PASS.

## Hard Rules (inherited from verifier-design principles)

1. NEVER claim either output file is missing without `Glob` verification first.
2. NEVER skip Check 2 (contract-clause leakage) — this is the single highest-impact V3 regression and the entire reason this verifier exists.
3. NEVER report a requirement as "truncated" without quoting the last 40 chars of its text so the agent can pinpoint the failure mode.
4. Every finding must cite a specific `req.id` + field name + value (e.g., `requirements[12].source_file = "Attachment-A-Sample-XaaS-Contract.md"`).
5. On FAIL, return corrective instructions with the specific count and check ID so the phase agent can target the exact V3 fix without re-running everything.
6. Source-file blacklist verification (Check 2) takes precedence over count targets (Check 8) — a "healthy" count of 487 is worse than a low count of 100 if the 487 includes contract-clause contamination.
