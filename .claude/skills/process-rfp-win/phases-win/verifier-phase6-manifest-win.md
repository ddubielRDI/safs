---
name: verifier-phase6-manifest-win
expert-role: Manifest / Executive Summary / Navigation Verifier
purpose: phase-boundary verifier for phase6-manifest-win (three-file presence, executive-summary integrity, company-stat traceability, navigation completeness)
created: 2026-05-20
---

# Verifier — Phase 6 Manifest / Executive Summary / Navigation Guide

## When this runs

After phase6-manifest-win reports done, BEFORE the next phase (typically phase6c-context-bundle or Stage 7 prep).

## Inputs (read in this order)

1. `{folder}/outputs/MANIFEST.md` — primary deliverable 1
2. `{folder}/outputs/EXECUTIVE_SUMMARY.md` — primary deliverable 2
3. `{folder}/outputs/NAVIGATION_GUIDE.md` — primary deliverable 3 (merged into Phase 6 on 2026-05-18)
4. `{folder}/shared/effort-estimation.json` — Investment Summary source of truth
5. `{folder}/shared/requirements-normalized.json` — requirement count cross-check
6. `{folder}/shared/domain-context.json` — domain field, persona context
7. `config-win/company-profile.json` — company statistic traceability source
8. `{folder}/outputs/*.md` — full file list for inventory + navigation cross-reference

## Verification Checks

### Check 1 — All three files exist, valid UTF-8, no mojibake

**Criterion:** All three files present: `MANIFEST.md`, `EXECUTIVE_SUMMARY.md`, `NAVIGATION_GUIDE.md` at `{folder}/outputs/`. Each opens as UTF-8 without `UnicodeDecodeError`. Grep across the three files for `Ã¢â‚¬`, `â€"`, `Â `, `�` returns 0 hits.

**Pass:** All three files exist AND decode cleanly AND no mojibake.

**Fail:** Any file absent, decode error, OR mojibake sequence present in any file.

**Evidence to cite:** Per-file existence boolean + size; mojibake line + file path if any.

**Hard-rule reminder:** NEVER claim file is missing without `Glob` verification first.

---

### Check 2 — File sizes meet thresholds

**Criterion:**
  - `MANIFEST.md` ≥ 2,048 bytes (2 KB)
  - `EXECUTIVE_SUMMARY.md` ≥ 2,048 bytes (2 KB)
  - `NAVIGATION_GUIDE.md` ≥ 3,072 bytes (3 KB)

**Pass:** All three sizes meet or exceed the thresholds.

**Concern:** Any file within 10% below threshold (e.g., 1,850–2,047 bytes for the 2KB files) — log advisory.

**Fail:** Any file below 90% of its threshold — content was truncated or template wasn't substituted.

**Evidence to cite:** `MANIFEST.md = N bytes`, `EXECUTIVE_SUMMARY.md = M bytes`, `NAVIGATION_GUIDE.md = K bytes`.

---

### Check 3 — MANIFEST.md inventories all output .md files

**Criterion:** Count of `.md` files referenced in MANIFEST.md "Output File Inventory" table is equal to (or greater than, in case of subdirectory inclusions) the count of `.md` files under `{folder}/outputs/` (via `glob.glob("{folder}/outputs/**/*.md", recursive=True)`). Tolerance: 0 (every file present on disk MUST be listed; extra entries are allowed if subdirs add structured-data files).

**Pass:** Listed count ≥ on-disk count AND no file on disk is missing from the inventory.

**Fail:** Any on-disk .md file not present in the inventory table.

**Evidence to cite:** `on_disk_count = N`, `inventory_count = M`, missing files list.

---

### Check 4 — Verification Checklist marks all 5 mandatory outputs as Present

**Criterion:** In MANIFEST.md "Verification Checklist" section, the 5 mandatory required_outputs (ARCHITECTURE.md, SECURITY_REQUIREMENTS.md, REQUIREMENTS_CATALOG.md, TRACEABILITY.md, EFFORT_ESTIMATION.md) each appear adjacent to a "✅ Present" marker. None should show "❌ Missing".

**Pass:** All 5 marked Present.

**Fail:** Any of the 5 marked Missing — upstream phase failure that Phase 6 papered over.

**Evidence to cite:** Per-file marker found ("✅ Present" or "❌ Missing"); list of any missing.

---

### Check 5 — Executive Summary Investment Summary cites BOTH traditional and AI-assisted figures

**Criterion:** EXECUTIVE_SUMMARY.md "Investment Summary" section contains BOTH:
  (a) A row with the literal phrase "Traditional Baseline" (or "Traditional, BEFORE savings") referencing the `summary.total_hours` figure from `shared/effort-estimation.json`, AND
  (b) A row with the literal phrase "AI-Assisted" (or "AFTER ... savings") referencing the `summary.ai_assisted_hours` figure.
Both figures must reconcile to ±1 hour with the values in `shared/effort-estimation.json`.

**Pass:** Both rows present, both reconcile.

**Fail:** Either row absent, OR a single ambiguous "Total Hours" row collapses both figures, OR figures disagree with json source.

**Evidence to cite:** Row text + numeric value found vs json source value.

**Hard-rule reminder:** NEVER label the Traditional baseline as "(AI-assisted savings applied)" — that's the figure BEFORE savings. Per phase6-manifest-win.md Step 4a rule 2.

---

### Check 6 — Executive Summary surfaces win themes

**Criterion:** EXECUTIVE_SUMMARY.md contains a section heading or table that references "win theme" OR "themes" OR explicit theme names from `shared/bid/POSITIONING_OUTPUT.json` (if it exists; otherwise the Recommended Approach narrative substitutes). At minimum, a "Recommended Approach" or "Solution Architecture" section must list concrete differentiator bullets — not a generic 4-line template.

**Pass:** ≥ 1 explicit theme reference OR a substantive Recommended Approach section (≥ 4 distinct bullets, none of which are the verbatim template defaults like "Modern cloud-native architecture").

**Concern:** Generic 4-bullet template found verbatim — log advisory; not a hard fail because Phase 8.0 may not have run yet.

**Fail:** No themes AND no substantive Recommended Approach section.

**Evidence to cite:** Theme names listed OR bullet text found.

---

### Check 7 — No fabricated company stats (every stat traces to company-profile.json)

**Criterion:** Identify all numeric/quantified claims in EXECUTIVE_SUMMARY.md that purport to be ABOUT THE BIDDER (e.g., "20+ years of experience", "150+ employees", "98% on-time delivery", certifications like "SOC 2 Type II", "ISO 27001"). For each such claim, verify it appears in or is derivable from `config-win/company-profile.json` (years_in_business, employee_count, past_performance metrics, certifications array, etc.). Claims about the SOLUTION (e.g., "the system will support 10,000 users") are OUT OF SCOPE for this check — only bidder-identity stats.

**Pass:** Every bidder-identity stat traces to a company-profile.json field.

**Fail:** Any bidder-stat fabricated (not in company-profile.json). Common offenders: invented employee counts, made-up certifications, fictional client counts.

**Evidence to cite:** Claim text + asserted value + company-profile.json field path (or "NOT FOUND").

---

### Check 8 — Navigation Guide links to all major deliverables

**Criterion:** NAVIGATION_GUIDE.md contains references (filename mentions) for every .md file in `{folder}/outputs/` of canonical category — at minimum: REQUIREMENTS_CATALOG.md, ARCHITECTURE.md, SECURITY_REQUIREMENTS.md, TRACEABILITY.md, EFFORT_ESTIMATION.md, EXECUTIVE_SUMMARY.md, REQUIREMENT_RISKS.md (if exists), COMPETITIVE_POSITION.md (if exists), DIAGRAM_BLUEPRINTS.md (if exists). The Quick Start table must contain ≥ 6 rows. The Use Case Guides section must contain ≥ 5 `###` sub-headings.

**Pass:** All canonical files referenced, Quick Start ≥ 6 rows, Use Case Guides ≥ 5 headings.

**Concern:** 1–2 canonical files missing from references (likely upstream didn't produce them) — log advisory.

**Fail:** ≥ 3 canonical files missing OR Quick Start < 5 rows OR Use Case Guides < 4 headings.

**Evidence to cite:** Missing filename references; Quick Start row count; Use Case heading count.

---

### Check 9 — Pipeline Summary metrics reconcile

**Criterion:** MANIFEST.md "Pipeline Summary" table has rows for: Start Time (not "N/A" unless progress.json was genuinely absent), Total Requirements (must equal `len(requirements-normalized.requirements)`), Output Files (must equal on-disk file count from Check 3), Domain Detected (must equal `domain_context.selected_domain`).

**Pass:** All four values present AND Total Requirements + Output Files + Domain reconcile with their sources.

**Concern:** Start Time is "N/A" — acceptable only if `shared/progress.json` does not exist.

**Fail:** Any of the three reconcilable values disagrees with source.

**Evidence to cite:** Per-field manifest value vs source value, with delta where numeric.

---

### Check 10 — No `[:N]` truncation, no row-cap notices, no `_Showing N of M_` across all three files

**Criterion:** In all three deliverables, grep returns 0 hits for `_Showing [0-9]+ of [0-9]+_` AND 0 hits for `\[:[0-9]+\]` literal text AND 0 hits for mid-word table-cell truncations (cell ending with `...` mid-word).

**Pass:** All three patterns produce 0 hits in all three files.

**Fail:** Any pattern found in any file.

**Evidence to cite:** Offending file + line + matched text.

---

### Check 11 — Filename tokens in tables are backtick-consistent

**Criterion:** Per phase6-manifest-win.md Step 4a rule 6: every filename token appearing in any markdown table cell across all three files MUST be backtick-wrapped consistently within that cell. No mixing of `` `EXECUTIVE_SUMMARY.md` `` and bare `EXECUTIVE_SUMMARY.md` in the same cell (causes PDF render `+ +` artifacts at Phase 8e).

**Pass:** Every cell containing a `.md`/`.json`/`.pdf` token is uniformly wrapped or uniformly bare (cell-internal consistency).

**Concern:** Inconsistency limited to NAVIGATION_GUIDE.md "Document Overview" table where the column header IS the filename — log advisory.

**Fail:** Mixed wrapping within the same cell anywhere in EXECUTIVE_SUMMARY.md (the file Phase 8e PDF-renders most often).

**Evidence to cite:** File + line + offending cell text.

---

## Disposition Logic

- **PASS:** Checks 1, 2, 3, 4, 5, 7, 9, 10, 11 pass AND Checks 6, 8 meet threshold (not just CONCERN).
- **CONCERN:** Any of Checks 2, 6, 8, 9, 11 fall in their advisory band. Log + continue to next stage.
- **FAIL:** Any of Checks 1, 3, 4, 5, 7, 10 fail OR Checks 6, 8 fall below their FAIL thresholds.

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase6-manifest-win with the following targeted corrections:

[Check 1] One deliverable missing/undecodable/mojibake: confirm Phase 6 Steps 3, 4, and Step N
  all executed. Mojibake fix: enforce encoding='utf-8' on every open().

[Check 2] {file} below size threshold: inspect the f-string template in Step 3 (manifest),
  Step 4 (exec_summary), or Step N (nav) for premature truncation or empty-loop body.

[Check 3] On-disk files not in inventory {list}: Step 1 glob — ensure `recursive=True`.
  Inventory may be missing files in subdirs (bid-sections/, bid/).

[Check 4] Mandatory outputs marked Missing {list}: upstream phase failed silently. DO NOT
  proceed past Phase 6 — re-run the upstream phase first, then Phase 6.

[Check 5] Investment Summary collapses both figures or figures disagree: apply Step 4a rules
  1-5 precisely. Both labeled rows REQUIRED. Recompute from shared/effort-estimation.json.

[Check 6] No win themes / generic template only: if shared/bid/POSITIONING_OUTPUT.json exists,
  source themes from there. If Phase 8.0 has not run, escalate as phase-ordering issue.

[Check 7] Fabricated company stat {claim}: replace with sourced value from company-profile.json
  OR remove the claim. NEVER invent bidder-identity numbers — evaluator due-diligence will reject.

[Check 8] Navigation Guide missing refs / Quick Start rows < 6 / Use Cases < 5: Step N USE_CASES
  list ships with 5 entries — confirm none filtered out. Files genuinely absent = Phase 7/8 issue.

[Check 9] Pipeline Summary {field} disagrees with source: Step 2 must read correct shared/ files
  and the f-string must substitute the right variables.

[Check 10] Truncation pattern at {file}:{line}: remove `[:N]` slice; emit full content. Per
  "Encoding + Truncation Discipline" memory — three regressions logged; do not introduce a fourth.

[Check 11] Mixed filename backtick wrapping at {file}:{line}: standardize on backtick-wrapped form.
  Phase 8e strips bare filenames as internal references — leaves `+ +` artifacts.

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input:**

```
MANIFEST.md = 1,200 bytes (template barely substituted, Output Files row says "N/A").
EXECUTIVE_SUMMARY.md = 1,900 bytes; Investment Summary has ONE row "Total Hours: 56,360
  (35% AI-assisted savings applied)". Claims "150+ employees" but company-profile.json
  has employee_count = 35.
NAVIGATION_GUIDE.md exists at 2,800 bytes; Quick Start has 3 rows; Use Case Guides has 2 headings.
MANIFEST.md Verification Checklist shows ARCHITECTURE.md = "❌ Missing".
ARCHITECTURE.md is actually present on disk — manifest read it as missing because the inventory
  glob was non-recursive.
```

Verifier MUST detect: Check 2 (NAV under 3KB; MANIFEST under 2KB), Check 4 (ARCHITECTURE marked
Missing), Check 5 (single collapsed row + misleading label), Check 7 (150+ employees fabricated),
Check 8 (Quick Start 3 < 6, Use Cases 2 < 5), Check 9 (Output Files = "N/A").
Disposition: FAIL.

**Known-good input:**

```
MANIFEST.md = 6,400 bytes; lists 27 .md files; Verification Checklist shows all 5 mandatories Present;
  Pipeline Summary reconciles (Total Requirements = 2,167 matching normalized.json; Domain = "education").
EXECUTIVE_SUMMARY.md = 12,800 bytes. Investment Summary has TWO labeled rows:
  "Total Effort (Traditional Baseline, BEFORE savings): 56,360 hr"
  "Total Effort (AI-Assisted, AFTER 35% savings): 36,624 hr"
  Direct Labor = $5,859,840 (= 36,624 × $160). Total Cost = $7,412,698 (reconciles to direct × 1.15 × 1.10).
  Win themes section lists 4 themes from POSITIONING_OUTPUT.json by name.
  All bidder-identity stats trace to company-profile.json.
NAVIGATION_GUIDE.md = 9,200 bytes; Quick Start has 9 rows; Use Case Guides has 5 ### headings;
  references all 12 canonical deliverable files.
All filename tokens in tables are backtick-wrapped consistently.
No [:N] slices, no _Showing N of M_, no mojibake.
```

Verifier MUST PASS all checks. Disposition: PASS.

## Hard Rules (inherited from verifier-design principles)

1. NEVER claim any of the three deliverables is missing without `Glob` verification first.
2. NEVER flag a bidder-identity stat as fabricated if it appears with the same value in `config-win/company-profile.json` — check ALL nested paths (past_performance.*, certifications[], locations[], etc.), not just top-level.
3. NEVER fail Check 5 if `shared/effort-estimation.json` is absent — that's a Phase 5 verifier concern; flag as upstream dependency missing instead.
4. NEVER fail Check 6 (win themes) if `shared/bid/POSITIONING_OUTPUT.json` doesn't exist AND the pipeline-ordering note in phase6-manifest-win.md indicates Phase 8.0 runs later — log as CONCERN only.
5. Every finding must cite a specific file + line + claimed value (e.g., `EXECUTIVE_SUMMARY.md:142 — "150+ employees" not in company-profile.json`).
6. On FAIL, return corrective instructions tied to the specific Step number of phase6-manifest-win that owns the defect.
7. Tech Lead / Senior Developer collapse (from Phase 5) is OUT OF SCOPE for this verifier — only Phase 6 deliverables are checked.
