---
name: verifier-phase8.3-technical-win
expert-role: Technical Volume Quality Verifier
purpose: phase-boundary verifier for phase8.3-technical-win (proven-capability callouts, eval factor callouts, CVD theme threading, tech-lifecycle validation, page-budget)
created: 2026-05-20
---

# Verifier — Phase 8.3 Technical Approach

## When this runs

After phase8.3-technical-win reports done, BEFORE Phase 8.4 (Business Solution) which depends on Technical's architecture choices and tech stack for solution decomposition.

## Inputs (read in this order)

1. `{folder}/outputs/bid-sections/03_TECHNICAL.md` — primary output under verification
2. `{folder}/shared/bid/POSITIONING_OUTPUT.json` — for win_themes, matched_projects, evaluator_messages, theme_eval_mapping
3. `{folder}/shared/EVALUATION_CRITERIA.json` — for eval factor ordering (content must be ordered by weight)
4. `{folder}/shared/tech-lifecycle-evidence.json` — for tech version currency check
5. `{folder}/outputs/ARCHITECTURE.md` — source for §3.4 persona vignettes ADR/Principle cross-reference
6. `{folder}/shared/UNIFIED_RTM.json` — for requirement ID cross-references in Proven Capability callouts

## Verification Checks

### Check 1 — File exists and meets minimum size

**Criterion:** `outputs/bid-sections/03_TECHNICAL.md` exists AND file size >= 15,360 bytes (15 KB minimum — phase target). Technical is the highest-weighted volume in most RFPs; small file = catastrophic underdelivery.

**Pass:** File exists, size >= 15,360 bytes.

**Fail:** File absent OR size < 15,360 bytes.

**Evidence to cite:** File path + actual size in bytes.

**Hard-rule reminder:** NEVER claim file is missing without `Glob` verification first.

---

### Check 2 — All 10 major sections present and ordered by eval weight

**Criterion:** Grep `^## ` returns >= 10 hits. Expected major sections: 1. Executive Technical Summary, 2. Understanding of Requirements, 3. Solution Architecture, 4. Integration and Interoperability, 5. Security Framework, 6. Quality Assurance, 7. Risk Management, 8. Implementation Methodology, 9. User Experience, 10. Continuous Improvement.

Cross-check: the first 3 sections (excluding Sec 1 Executive Summary) should map to the top-3 weighted eval factors in EVALUATION_CRITERIA.json. If "Security" is weight #1 in the RFP but Sec 5 (Security) appears before Sec 3 (Architecture) — that's correct ordering by weight, accept it.

**Pass:** All 10 sections present.

**Concern:** 8-9 sections (one or two merged).

**Fail:** < 8 sections.

**Evidence to cite:** Grep `^## ` count; list section headings found.

---

### Check 3 — Eval factor callout at every major section

**Criterion:** Grep `^> \*\*Evaluation Factor` returns >= 10 hits — one per major section. (Sec 1 may use `**Evaluation Factors Addressed:**` plural variant.)

**Pass:** >= 10 callouts.

**Concern:** 7-9 callouts.

**Fail:** < 7 callouts.

**Evidence to cite:** Grep count and the first 80 chars of first 3 matched lines.

---

### Check 4 — >= 2 themes per major section in CVD format

**Criterion:** Load `win_themes`. Split document by `^## ` boundaries (10 major sections). For each, count distinct themes appearing as `**[Theme Name]**` or `**Theme Name**`. Threshold: >= 2 per section, >= 50% in CVD (Claim-Vivid Detail) format with concrete evidence statement.

Additionally, every theme in `win_themes` must appear AT LEAST 3 times total across the document (phase quality checklist item 9 — "Every win theme appears >= 3 times").

**Pass:** All 10 sections have >= 2 themes AND every theme appears >= 3 times across document.

**Concern:** 8-9 sections meet bar OR some themes appear exactly 2 times.

**Fail:** 3+ sections have < 2 themes OR any theme appears 0 times.

**Evidence to cite:** Section-by-section theme count table; per-theme total count.

---

### Check 5 — Tech-stack EOL Date column present and versions validated

**Criterion:** Within Sec 3.2 (Technology Stack), the technology table must contain an "EOL Date" or "End-of-Life" column. Every row must have a non-empty EOL Date cell (not "TBD", not empty).

Cross-check: for each technology version cited in the stack, look up `tech-lifecycle-evidence.json` and verify:
(a) The version matches that component's `recommended_version`.
(b) The cited EOL date is >= (proposal date + contract length + 2 years), per memory's `.NET version discipline` rule.
(c) NO ".NET 8" (EOL Nov 2026), NO ".NET 9" (STS), unless the contract is <6 months.

**Pass:** EOL column present, all rows populated, all versions match evidence file, all dates pass lifecycle math.

**Concern:** EOL column present, all populated, but 1 version is .NET 9 STS — log advisory if contract <12 months.

**Fail:** EOL column missing OR any row has empty EOL OR ".NET 8" referenced for a multi-year contract OR any version mismatches tech-lifecycle-evidence.json `recommended_version`.

**Evidence to cite:** Quote tech-stack table rows; for each FAIL, name the technology + version + EOL + reason.

---

### Check 6 — Proven Capability callouts present, citing different projects

**Criterion:** Grep `^> \*\*Proven Capability` returns >= 4 hits (covering Architecture, Integration, Security, Quality, UX — at least 4 of 6 expected sections). Each callout must cite a client name and a quantified result. Each callout must cite a DIFFERENT project (no repeats).

**Pass:** >= 4 distinct Proven Capability callouts, each citing a unique matched_projects client.

**Concern:** 3 callouts OR 4 callouts but with one repeat.

**Fail:** < 3 callouts OR 2+ callouts cite the same project.

**Evidence to cite:** List each callout with its cited client name + project number; flag duplicates.

---

### Check 7 — §3.4 Real-World Use Cases — >= 3 persona vignettes

**Criterion:** Grep `^#### Use Case` returns >= 3 hits. Each Use Case heading must include a named persona with a domain-specific role/location (NOT "Generic Filer", "Sample User", etc.).

For each Use Case, grep the content range until next `####` or `##` for `> \*\*Why ` blockquotes — count must be >= 3 per vignette. Each `> **Why` block must contain BOTH `ADR-` or `Principle #` AND `*Rejected:` within the same blockquote group.

**Pass:** >= 3 Use Cases, each with named-domain persona, each with >= 3 Why-this-design callouts containing both ADR/Principle and Rejected alternative.

**Concern:** 3 Use Cases but one persona is generic (e.g., "Filer" without name) — log advisory.

**Fail:** < 3 Use Cases OR any Use Case has < 3 Why callouts OR any Why callout lacks ADR/Principle OR lacks Rejected alternative.

**Evidence to cite:** List Use Case headings; per Use Case: Why-callout count, ADR/Principle citation presence, Rejected alternative presence.

---

### Check 8 — Content ordered by eval weight (highest-weighted addressed first/most)

**Criterion:** Identify the top-3 eval factors by weight in EVALUATION_CRITERIA.json. Measure approximate section length (character count) for each major section. The sections mapped to top-3 factors should collectively account for >= 50% of total document length.

Additionally, Sec 1 (Executive Technical Summary) callout should preview the top-3 factors by name.

**Pass:** Top-3-factor sections >= 50% of length AND Sec 1 callout names top-3.

**Concern:** Top-3 sections 40-49% of length OR Sec 1 callout names top-2.

**Fail:** Top-3-factor sections < 40% of length — content allocation misaligned with evaluation.

**Evidence to cite:** Top-3 factor names with weights; section char counts; computed percentage; Sec 1 callout text.

---

### Check 9 — Page-budget awareness (MARS 25-page cap)

**Criterion:** Estimate pages by char/3,000. Technical volume target: 10-15 pages (30,000-45,000 chars). Within a 25-page total, Technical claims the largest single share but must not exceed 15 pages or it crowds out other required sections.

**Pass:** 30,000-45,000 chars (10-15 pages).

**Concern:** 25,000-30,000 (thin for highest-weighted volume) OR 45,000-50,000 (heavy).

**Fail:** > 50,000 chars (> 17 pages — exceeds 68% of 25-page cap). OR < 20,000 chars (severely under-developed).

**Evidence to cite:** Actual char count, estimated page count.

---

### Check 10 — Universal regression patterns

**Criterion:** Five sub-checks:
(a) UTF-8 decode clean; no mojibake.
(b) Zero `_Showing \d+ of \d+_` row-cap notices.
(c) Zero `[:N]` truncation patterns in deliverable strings (no mid-word cuts at line/cell ends).
(d) Zero em-dash chars (`—` U+2014) — fitz.Story mojibake guard.
(e) Zero internal file references (`.json`, `.md`, `Past_Projects`, `ARCHITECTURE.md`, `POSITIONING_OUTPUT`).

**Pass:** All five sub-checks pass.

**Fail:** Any sub-check has 1+ violation.

**Evidence to cite:** Per violation: line number + first 80 chars quoted.

---

## Disposition Logic

- **PASS:** All 10 checks pass.
- **CONCERN:** Checks 2, 3, 4, 6, 7, 8, or 9 in advisory band; all others pass.
- **FAIL:** Any of Checks 1, 5, 10 fail (Check 5 is CRITICAL — .NET 8 in a multi-year bid is an automatic credibility hit per memory) OR Checks 2/3/4/6/7/8/9 fall below FAIL threshold.

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase8.3-technical with the following targeted corrections:

[Check 1 fail] 03_TECHNICAL.md missing or < 15 KB ({actual_size} bytes).
  Action: Re-run phase. The phase ingests 10+ source files (ARCHITECTURE.md,
  INTEROPERABILITY.md, SECURITY_FRAMEWORK.md, etc.). Confirm all loaded.

[Check 2 fail] Missing section(s): {list}.
  Action: Step 3 template specifies all 10 ## sections. Confirm the AI didn't
  stop generation early (token budget exhausted). Use the streaming approach:
  generate each section independently if needed.

[Check 3 fail] Only {N} eval factor callouts (need 10).
  Action: Each ## section must open with `> **Evaluation Factor:** ...`. Inject
  during section authoring, not as a post-hoc pass.

[Check 4 fail] Sections with < 2 themes: {list}. OR theme(s) absent: {list}.
  Action: Every theme from win_themes must appear >= 3 times across document.
  Use the theme_coverage counter from Step 4 BEFORE writing the final file.

[Check 5 fail — CRITICAL] Tech-stack issue: {description}.
  Action: Read tech-lifecycle-evidence.json. The Step 3.2 EOL discipline is
  non-negotiable. For 2026+ contracts:
    - REJECT .NET 8 (EOL Nov 2026) — use .NET 10 LTS (Nov 2028 EOL)
    - REJECT .NET 9 STS for any contract > 6 months
    - Verify EOL date columns are populated with web-searched values
  Per SAFS memory: "Government evaluators CHECK these dates. Getting this
  wrong is an automatic credibility hit."

[Check 6 fail] Proven Capability callouts: {N} found, {duplicates} repeats.
  Action: section_project_mapping (Step 2) must assign a UNIQUE matched_projects
  entry to each section. The used_projects set in Step 3b prevents repeats —
  confirm that logic ran.

[Check 7 fail] §3.4 Real-World Use Cases deficient: {description}.
  Action: §3.4 requirements (from Step 3 instructions):
    - >= 3 Use Cases with `#### Use Case N -- [Persona], [role]` heading
    - Each persona named + domain-specific role/location (e.g., "Maria, City Clerk
      for Sisters, Oregon" not "Generic Filer")
    - Each Use Case has >= 3 `> **Why [design]?**` callouts
    - Each Why callout names ADR-NNN or Principle #N AND has `*Rejected: alt*`
    - 300-450 words per vignette
  Re-run §3.4 generation with the template.

[Check 8 fail] Content allocation misaligned with eval weights.
  Action: Sort eval_factors_by_weight. Allocate proportional content to the
  top-3 sections. If "Past Performance" is 25% weight, Section 5 should be
  ~25% of document length.

[Check 9 fail] Technical volume is {N} chars ({pages} pages).
  Action: For >50K: trim Sec 10 (Continuous Improvement) and Sec 1 Exec Summary
  duplication. For <20K: expand Sec 3 Architecture and Sec 5 Security — these
  carry the most evaluator weight.

[Check 10 fail] Regression pattern {type} at line {N}.
  Action: Strip em dashes (replace with --), remove file mentions, eliminate
  [:N] slices, fix UTF-8 mojibake.

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input:**

```
03_TECHNICAL.md: 11,000 bytes.
  8 of 10 major sections present (Sec 9 UX and Sec 10 Continuous Improvement missing).
  Eval factor callouts: 5 of 10.
  Win themes (4 total): 2 themes appear >= 3 times; 2 themes appear 0 times.
  Tech stack table: ".NET 8.0 LTS | EOL: Nov 2026" — for a 3-year contract starting 2026.
  Proven Capability callouts: 5 found, but 2 cite Mat-Su Borough (duplicate).
  §3.4 Real-World Use Cases section absent entirely.
  3 em-dash chars present.
  "As documented in Past_Projects.md..." appears in Sec 5.
```

Verifier MUST detect: Check 1 (< 15 KB), Check 2 (8 of 10), Check 3 (5 of 10), Check 4 (themes absent), Check 5 (CRITICAL .NET 8 for 3yr contract), Check 6 (duplicate project), Check 7 (§3.4 absent), Check 10 (em dashes + file ref). Disposition: FAIL.

**Known-good input:**

```
03_TECHNICAL.md: 38,000 bytes (~13 pages).
  All 10 major sections present, ordered by eval weight.
  Eval factor callouts: 10 (one per section).
  All 4 win themes appear 4-6 times each, in CVD format.
  Tech stack table:
    | .NET 10 | EOL: Nov 2028 | Latest LTS, covers contract + maintenance |
    | React 19 | EOL: Q3 2027 | Current LTS, 2+ years remaining |
    | PostgreSQL 17 | EOL: Nov 2029 | GA, under active support |
    — all versions match tech-lifecycle-evidence.json recommended_version.
  Proven Capability callouts: 5, all distinct projects from matched_projects.
  §3.4 has 4 Use Cases:
    Use Case 1 -- Maria, City Clerk for Sisters, Oregon
    Use Case 2 -- David, Public Records Searcher
    Use Case 3 -- Rachel, Internal Auditor, Oregon Secretary of State
    Use Case 4 -- Tom, External API Consumer (county registrar)
    — each has 3+ `> **Why ...?**` callouts with ADR-NNN + *Rejected: alt*.
  Top-3 eval factors collectively occupy 18,400 chars (48% of doc).
  No em dashes, no file references, UTF-8 clean.
```

Verifier MUST PASS all checks. Disposition: PASS.

## Hard Rules (inherited from verifier-design principles)

1. NEVER claim 03_TECHNICAL.md is missing without `Glob` verification first.
2. NEVER pass Check 5 when ".NET 8" or ".NET 9" appears in a tech stack for a multi-year contract — this is the highest-cost error per memory and is non-negotiable regardless of EOL date in the table.
3. NEVER fail Check 7 if §3.4 is explicitly waived in EVALUATION_CRITERIA.json (some RFPs disallow persona vignettes) — log VACUOUS PASS instead.
4. NEVER count a generic theme reference (e.g., "innovation is important") toward Check 4 — only bold-bracketed `**[Theme]**` or `**Theme**` markers count.
5. NEVER apply Check 8's >= 50% threshold strictly when eval weights are uniform (every factor weighted equally) — log PASS with note.
6. Every finding must cite line number + first 80 chars of offending content (or section name + measurement for Checks 4, 8, 9).
