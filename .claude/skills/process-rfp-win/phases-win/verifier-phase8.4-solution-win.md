---
name: verifier-phase8.4-solution-win
expert-role: Business Solution Quality Verifier
purpose: phase-boundary verifier for phase8.4-solution-win (per-work-section file coverage, requirement-to-solution mapping, no truncation, no row caps)
created: 2026-05-20
---

# Verifier — Phase 8.4 Business Solution

## When this runs

After phase8.4-solution-win reports done, BEFORE Phase 8.4r (Requirements Review) which globs `04*.md` files for cross-referencing solution coverage.

## Inputs (read in this order)

1. `{folder}/outputs/bid-sections/04_SOLUTION.md` (single-file mode) OR `{folder}/outputs/bid-sections/04[a-f]_SOLUTION_*.md` (split-file mode) — primary outputs under verification
2. `{folder}/shared/SUBMISSION_STRUCTURE.json` — for required work sections (if specified by RFP)
3. `{folder}/shared/requirements-normalized.json` — for requirement count per category
4. `{folder}/shared/UNIFIED_RTM.json` — for requirement-to-spec linkage cross-check
5. `{folder}/shared/REQUIREMENT_RISKS.json` — for embedded risk-table mitigation source
6. `{folder}/shared/bid/POSITIONING_OUTPUT.json` — for win_themes coverage check
7. `config-win/company-profile.json` — to identify items that should NOT carry `[USER INPUT REQUIRED]`

## Verification Checks

### Check 1 — Solution file(s) exist and meet aggregate minimum size

**Criterion:** Glob `{folder}/outputs/bid-sections/04*.md`. At least one file matches. EXCLUDE `04_REQUIREMENTS_REVIEW.md` and `04_RISK_REGISTER.md` from the count (those are different phases). Aggregate file size of remaining 04*.md files >= 10,240 bytes (10 KB minimum; phase target is 12 KB).

**Pass:** >= 1 solution file exists; aggregate size >= 10,240 bytes.

**Fail:** No solution file matches the glob OR aggregate size < 10,240 bytes.

**Evidence to cite:** Glob result (file names + individual sizes); aggregate size.

**Hard-rule reminder:** NEVER claim file is missing without `Glob` verification first.

---

### Check 2 — Per-work-section coverage from SUBMISSION_STRUCTURE.json

**Criterion:** Load `SUBMISSION_STRUCTURE.json`. If it defines `work_sections[]` or equivalent, each named work section must be addressed by at least one 04*.md file. Match by: (a) filename includes a slug of the work section name (e.g., work section "Collection" -> `04a_SOLUTION_COLLECTION.md`), OR (b) one of the 04*.md files contains a `## Work Section: Collection` heading.

If SUBMISSION_STRUCTURE.json does NOT define explicit work sections, fall back to requirement categories: each category with >= 10 requirements should be addressed by at least one 04*.md section.

**Pass:** Every required work section has a corresponding file or section.

**Concern:** 1 missing work section out of 4+ — log advisory.

**Fail:** 2+ required work sections unaddressed.

**Evidence to cite:** List required work sections; for each, name the file or section that addresses it (or "missing").

---

### Check 3 — Each solution file >= 10 KB (when split-file mode)

**Criterion:** If split-file mode (3+ files matching `04[a-f]_SOLUTION_*.md`), each individual file must be >= 10,240 bytes. Phase rule: "at least one 04a_SOLUTION_*.md ≥10KB per work section in SUBMISSION_STRUCTURE.json".

If single-file mode (only `04_SOLUTION.md` exists), this check is satisfied if the single file is >= 12,288 bytes.

**Pass:** Every solution file meets its size minimum.

**Concern:** Single file ≥10 KB but <12 KB; OR 1 split file is 8-10 KB.

**Fail:** Any file < 8 KB (insufficient depth for any single work section).

**Evidence to cite:** Per-file size table.

---

### Check 4 — Requirements coverage table present per work section

**Criterion:** Each work section (file or `## Work Section:` heading group) must contain a "Requirements Coverage" or equivalent table mapping Req IDs to Solution Approach. The table must list requirements filtered to that work section's category.

Cross-check: count of Req IDs in the requirements table for a sampled section should match the count of requirements in that category from `requirements-normalized.json` (tolerance ±5%, no row cap).

**Pass:** Every work section has a requirements coverage table; counts match normalized source within ±5%.

**Concern:** 1 section has requirements table but count is 10-20% under source — log advisory.

**Fail:** Any work section missing requirements coverage table OR row count > 20% under source (row capping).

**Evidence to cite:** Per section: requirements row count vs normalized-source count.

---

### Check 5 — Implementation plan / approach section present per work section

**Criterion:** Each work section must contain an "Implementation Approach" OR "Implementation Plan" OR "Functional Solution" subsection describing HOW the requirements will be met. Bare requirement-to-spec mapping is insufficient.

**Pass:** Every work section has an implementation-approach subsection >= 500 chars.

**Concern:** 1 section has implementation subsection < 500 chars (thin).

**Fail:** Any work section lacks an implementation-approach subsection.

**Evidence to cite:** Per section: subsection name + char count.

---

### Check 6 — Embedded risk tables have populated Mitigation cells

**Criterion:** Within any 04*.md file, locate embedded risk tables (rows with `^\| RISK-` prefix). For each row, the Mitigation cell must be non-empty (either populated text or the `[MITIGATION TBD]` placeholder — both are valid). Zero empty `|  |` cells in Mitigation column.

Cross-check: For a sample of 5+ risk rows, the Mitigation cell text must trace back to either `risk.mitigation_strategies[]` (array) OR `risk.mitigation_strategy` (singular) in REQUIREMENT_RISKS.json — Jaccard word overlap >= 0.4.

**Pass:** All Mitigation cells populated; sampled rows trace to source data.

**Concern:** 1-2 sampled rows have low overlap (< 0.4 but > 0.2).

**Fail:** Any empty `|  |` mitigation cell OR 3+ sampled rows have overlap < 0.2 (fabricated mitigations).

**Evidence to cite:** Per empty cell: line + row context. Per traceability failure: risk_id + cell text + source text + overlap score.

---

### Check 7 — No `[USER INPUT REQUIRED]` for company-profile-satisfiable items

**Criterion:** Scan all 04*.md solution files for `[USER INPUT REQUIRED]` markers. For each occurrence, check whether the requested data exists in `company-profile.json` (team_members, certifications, methodologies, office_locations, partnerships). If yes, that marker is a FAIL.

Items that legitimately remain `[USER INPUT REQUIRED]`: client-specific data (preferred deployment region, specific integration endpoints, RFP-Q&A clarifications, payment timing preferences).

**Pass:** All markers refer to genuinely user-supplied data.

**Fail:** One or more markers reference company-profile.json data.

**Evidence to cite:** Line + marker text + the company-profile.json field that should have been substituted.

---

### Check 8 — Win themes threaded per work section

**Criterion:** Load `win_themes` from POSITIONING_OUTPUT.json. For each work section (file or heading group), at least 1 distinct theme must appear as a bold marker (`**[Theme]**` or `**Theme**`). Phase quality checklist item 8: "Win themes threaded per section — at least 1 explicit theme reference per major work section".

**Pass:** Every work section has >= 1 theme reference.

**Concern:** 1 section lacks theme reference.

**Fail:** 2+ sections lack any theme reference.

**Evidence to cite:** Per section: theme(s) found or "no theme reference".

---

### Check 9 — Page-budget awareness (MARS 25-page cap)

**Criterion:** Aggregate char count across all 04*.md solution files. Solution volume target: 5-9 pages aggregate (15,000-27,000 chars). Within a 25-page total, solution is the second-largest volume after Technical.

**Pass:** 15,000-27,000 chars aggregate.

**Concern:** 27,000-35,000 chars (heavy but bounded) OR 12,000-15,000 (thin).

**Fail:** > 35,000 chars (consumes > 11 pages of 25-page cap) OR < 10,000 chars (under-developed).

**Evidence to cite:** Per-file char counts + aggregate + estimated pages.

---

### Check 10 — Universal regression patterns

**Criterion:** Five sub-checks applied to every 04*.md solution file:
(a) UTF-8 decode clean; no mojibake.
(b) Zero `_Showing \d+ of \d+_` row-cap notices — anywhere.
(c) Zero `[:N]` truncation patterns in deliverable strings (no mid-word cuts at line/cell ends). PHASE-SPECIFIC: zero patterns matching `cat_risks[:5]`, `sorted_reqs[:30]`, `mandatory_in_cat[:10]` in any visible/inferred form.
(d) Zero em-dash chars (`—` U+2014).
(e) Zero internal file references.

**Pass:** All sub-checks pass.

**Fail:** Any sub-check has 1+ violation.

**Evidence to cite:** Per violation: file + line + first 80 chars quoted.

---

## Disposition Logic

- **PASS:** All 10 checks pass.
- **CONCERN:** Checks 2, 3, 4, 5, 8, or 9 in advisory band; all others pass.
- **FAIL:** Any of Checks 1, 6, 7, 10 fail OR Checks 2/3/4/5/8/9 fall below FAIL threshold.

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase8.4-solution with the following targeted corrections:

[Check 1 fail] No 04*.md solution file present OR aggregate < 10 KB.
  Action: Verify Step 3 (Decide Output Strategy) — single-file vs split-file logic.
  Confirm requirements-normalized.json has requirements (the per-category iteration
  needs source data). Confirm output directory exists.

[Check 2 fail] Missing work section(s): {list}.
  Action: Load SUBMISSION_STRUCTURE.json work_sections (if defined). Iterate and
  ensure each produces either a dedicated 04a/b/c file OR a `## Work Section:`
  heading group. If SUBMISSION_STRUCTURE doesn't define work_sections, fall back
  to requirement categories with >= 10 reqs.

[Check 3 fail] File(s) under size: {list with sizes}.
  Action: Each work section needs Functional Solution + Data Flow + Implementation
  Approach + Compliance Mapping + Risks subsections. Expand thin sections.
  Do NOT inflate with placeholders — pull from ARCHITECTURE.md, INTEROPERABILITY.md,
  and requirements-normalized.json.

[Check 4 fail] Requirements coverage table issue: {description}.
  Action: For each work section, filter requirements-normalized to that category
  and render ALL rows. NO `[:N]` slicing. NO row caps. NO "_Showing N of M_".
  Per SAFS memory (encoding+truncation discipline), this is a recurring regression
  vector.

[Check 5 fail] Implementation subsection missing/thin in: {list}.
  Action: Per Step 2 template, each work section must include the full subsection
  set. The Implementation Approach subsection should describe phased delivery
  with reference to effort-estimation.json sprint structure (if available).

[Check 6 fail] Empty mitigation cells OR fabricated mitigations.
  Action: For each risk row, populate Mitigation from BOTH `mitigation_strategies`
  (array) AND `mitigation_strategy` (singular) — try both fields in REQUIREMENT_RISKS.json.
  Use `[MITIGATION TBD]` ONLY when source data is genuinely empty.

[Check 7 fail] [USER INPUT REQUIRED] for company-profile items: {list}.
  Action: Substitute from company-profile.json for: team_members, certifications,
  methodologies, office_locations, partnerships.

[Check 8 fail] Work sections lacking theme references: {list}.
  Action: Each work section must include >= 1 theme in `**[Theme]**: <evidence>`
  CVD format. Use section_theme_mandates from POSITIONING_OUTPUT.json to identify
  which themes apply to which sections.

[Check 9 fail] Solution volume is {N} chars aggregate ({pages} pages).
  Action: For >35K: consolidate split files OR trim non-essential Data Flow
  descriptions. For <10K: each work section needs more depth.

[Check 10 fail — TRUNCATION/REGRESSION] {pattern type} at {file}:{line}.
  Action: This phase's ⛔ NO-TRUNCATION DISCIPLINE header is a HARD GATE.
  Remove all [:N] slices on description, text, mitigation, requirement strings.
  Remove all row caps. Remove all "_Showing N of M_" notices. Strip em dashes.

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input:**

```
Solution files:
  04a_SOLUTION_COLLECTION.md: 5,200 bytes
  04b_SOLUTION_CALCULATION.md: 6,800 bytes
  04c_SOLUTION_REPORTING.md: 4,100 bytes
  04_SOLUTION.md: absent (split-file mode)
  Aggregate: 16,100 bytes — meets aggregate min, but each file under 10 KB.
SUBMISSION_STRUCTURE.json defines work_sections: [Collection, Calculation, Reporting, Audit].
  04*.md files address only 3 of 4 — "Audit" missing.
Requirements Coverage table in COLLECTION shows "_Showing 30 of 218_".
Risk table in CALCULATION has 4 of 18 mitigation cells empty.
4 [USER INPUT REQUIRED] markers — 3 reference team_members from company-profile.
2 em-dash chars in CALCULATION section.
Win themes: only "Right-Sized Partner" appears in CALCULATION; other 3 themes absent across all files.
```

Verifier MUST detect: Check 2 (Audit missing), Check 3 (each file < 10 KB), Check 4 ("_Showing N of M_" + row cap), Check 6 (empty mitigation cells), Check 7 (company-profile items), Check 8 (multiple work sections lack themes), Check 10 (em dashes + truncation pattern). Disposition: FAIL.

**Known-good input:**

```
Solution files:
  04a_SOLUTION_COLLECTION.md: 11,200 bytes
  04b_SOLUTION_CALCULATION.md: 12,800 bytes
  04c_SOLUTION_REPORTING.md: 10,600 bytes
  04d_SOLUTION_AUDIT.md: 10,400 bytes
  Aggregate: 45,000 bytes (~15 pages).
All 4 work sections from SUBMISSION_STRUCTURE.json covered.
Each file has: Requirements Coverage (ALL rows, no caps), Functional Solution,
  Data Flow, Implementation Approach, Compliance Mapping, Risks subsections.
Embedded risk tables: all mitigation cells populated from source data.
[USER INPUT REQUIRED] markers: 2 total (deployment region preference, integration
  endpoint URLs) — neither satisfiable by company-profile.
Every work section has >= 1 theme in CVD format.
No "_Showing N of M_"; no `[:N]` slices; no em dashes; no file references.
```

Hmm — aggregate 45,000 chars is ~15 pages, which exceeds Check 9's 27,000-char "pass" band. Adjusted for Auth's split-file Audit volume case, Check 9 disposition = CONCERN. All other checks pass.

Verifier disposition: PASS with Check 9 CONCERN advisory.

## Hard Rules (inherited from verifier-design principles)

1. NEVER claim solution files are missing without `Glob` verification first — they may use any `04[a-f]_SOLUTION_*.md` pattern.
2. NEVER count `04_REQUIREMENTS_REVIEW.md` (Phase 8.4r) or `04_RISK_REGISTER.md` (Phase 8.4k) toward this phase's checks — they are separate phases with separate verifiers.
3. NEVER flag `[MITIGATION TBD]` as an empty mitigation cell — it is the prescribed placeholder per phase rule.
4. NEVER fail Check 2 if SUBMISSION_STRUCTURE.json genuinely doesn't define work_sections and requirement categories all have < 10 reqs — accept consolidated single-file output.
5. NEVER apply Check 9's strict >35K FAIL when work sections are 4+ and the RFP scope is genuinely broad — log CONCERN instead. Page-budget judgment must account for RFP scope reality.
6. Every finding must cite file + line number + first 80 chars of offending content (or section + measurement).
