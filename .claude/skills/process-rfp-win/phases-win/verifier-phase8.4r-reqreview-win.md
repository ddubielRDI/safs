---
name: verifier-phase8.4r-reqreview-win
expert-role: Compliance Matrix Quality Verifier
purpose: phase-boundary verifier for phase8.4r-reqreview-win (compliance matrix row completeness, mandatory item coverage, response code per row, no truncation, Attachment A/H format match)
created: 2026-05-20
---

# Verifier — Phase 8.4r Requirements Review Response

## When this runs

After phase8.4r-reqreview-win reports done, BEFORE Phase 8f (RTM Verification) which cross-references requirement-to-bid traceability and depends on every requirement having a response row.

## Inputs (read in this order)

1. `{folder}/outputs/bid-sections/04_REQUIREMENTS_REVIEW.md` — primary output under verification
2. `{folder}/shared/requirements-normalized.json` — for source requirement count
3. `{folder}/shared/COMPLIANCE_MATRIX.json` — for mandatory item coverage
4. `{folder}/shared/UNIFIED_RTM.json` — for linked_spec_ids / bid_section_ids cross-check
5. `{folder}/shared/SUBMISSION_STRUCTURE.json` — to detect Attachment A vs Attachment H vs other matrix format requirements

## Verification Checks

### Check 1 — File exists and meets minimum size

**Criterion:** `outputs/bid-sections/04_REQUIREMENTS_REVIEW.md` exists AND file size >= 5,120 bytes (5 KB minimum — phase target is 8 KB; for a multi-hundred-requirement matrix, anything under 5 KB is row-capped).

**Pass:** File exists, size >= 5,120 bytes.

**Fail:** File absent OR size < 5,120 bytes.

**Evidence to cite:** File path + actual size in bytes.

**Hard-rule reminder:** NEVER claim file is missing without `Glob` verification first.

---

### Check 2 — Row count matches source requirement count (no row capping)

**Criterion:** Count data rows in the Requirements Response Matrix table (lines matching `^\| ` excluding header and separator). Compare against `len(requirements-normalized["requirements"])`. Tolerance: 0 — every requirement must have exactly one response row (or, if RFP demands CRITICAL/HIGH only, the count must match that filtered subset exactly).

Per phase's ⛔ NO-TRUNCATION DISCIPLINE: "If 762 reqs are CRITICAL/HIGH, render 762 rows. The table will be long. That's correct."

**Pass:** Row count = source count exactly (or = CRITICAL+HIGH filter count exactly, if filter was applied per RFP rules).

**Concern:** Off by 1-3 rows (could be category headers counted as data rows).

**Fail:** Row count < 95% of source count — rows were capped.

**Evidence to cite:** Row count in document vs source count vs computed deviation %.

---

### Check 3 — ZERO row-cap notice patterns

**Criterion:** Scan entire file for `_Showing \d+ of \d+_` (case-insensitive). Zero occurrences. Per phase's discipline header: "NEVER emit '_Showing N of M_' notices. Show everything."

**Pass:** Zero matches.

**Fail:** One or more matches.

**Evidence to cite:** Line number + quoted notice text.

---

### Check 4 — Response code present for every row (Comply/Partial/Take Exception/Alternate)

**Criterion:** Every data row in the Requirements Response Matrix must have a non-empty Status (or response code) cell. Acceptable values: `COMPLIANT`, `COMPLY`, `PARTIAL`, `PARTIALLY COMPLY`, `NON-COMPLIANT`, `TAKE EXCEPTION`, `EXCEPTION`, `ALTERNATE`, `ALTERNATIVE`. Other values trigger FAIL (unknown disposition).

Cross-check: status counts in the Response Summary table must equal the row counts. E.g., if summary says "COMPLIANT: 500", then exactly 500 rows must show COMPLIANT.

**Pass:** Every row has a recognized response code; summary counts match actual row counts.

**Concern:** Summary counts off by 1-5 from actual counts (rounding or category-row counting issue).

**Fail:** Any row missing a status OR has an unrecognized status value OR summary counts differ from actuals by > 5.

**Evidence to cite:** Per malformed row: line + cell content. Summary mismatches: claimed vs actual.

---

### Check 5 — Mandatory items from COMPLIANCE_MATRIX appear in response matrix

**Criterion:** Load `mandatory_items` from COMPLIANCE_MATRIX.json. For each mandatory item, its linked requirement(s) (via `linked_requirement_ids` or text match) must appear in the response matrix with a recognized status.

A mandatory item linked to a requirement that has NO row in the response matrix = compliance gap that Phase 8f traceability audit will flag.

**Pass:** Every mandatory item is traceable to at least one response row.

**Concern:** 1-2 mandatory items lack response rows.

**Fail:** 3+ mandatory items unaddressed in the response matrix.

**Evidence to cite:** List mandatory_ids without response coverage; report linked requirement IDs.

---

### Check 6 — `linked_spec_ids` and `bid_section_ids` render in full (no `[:N]` truncation)

**Criterion:** Per phase fix 2026-05-19: "`linked_spec_ids` and `bid_section_ids` in any displayed cell render ALL entries (or join with commas) — not `[:3]` or `[:2]` head slices."

Sample 5 rows where the source RTM entry has 4+ `linked_spec_ids`. Verify the response matrix Reference/Spec column renders all of them (joined by commas).

Sample 5 rows where the source RTM entry has 3+ `bid_section_ids`. Verify the Bid Reference column renders all of them.

**Pass:** Sampled rows render all linked entries.

**Fail:** Any sampled row drops linked entries (e.g., source has 6 specs, row shows 3).

**Evidence to cite:** Per failing row: req_id + source linked count + rendered count.

---

### Check 7 — No mid-word truncation in requirement text cells

**Criterion:** Per phase fix 2026-05-18: "NEVER [:N] slice requirement text". Sample 10 random rows. For each, the Requirement column cell must not end mid-word (no `[a-z]\s*\|` cell-end pattern with `...` trailing).

Disambiguation: hyphenated line-wrap (`tamper-` continuing on next line as `proof`) is NOT a truncation — apply the same rule as the risk-register verifier.

**Pass:** Zero genuine mid-word truncations in sampled rows.

**Fail:** Any sampled row has a truncated requirement cell.

**Evidence to cite:** Line + cell content (last 80 chars) + check for hyphenation continuation.

---

### Check 8 — Attachment A/H format compliance (if RFP demands)

**Criterion:** Check `SUBMISSION_STRUCTURE.json` for any mention of "Attachment A", "Attachment H", or a specific requirements-matrix format requirement. If found:
- Attachment A typical format: Req ID | Description | Compliance | Comments
- Attachment H typical format: Req ID | Section | Page Reference | Response

The response matrix columns should align with the demanded attachment format. If the RFP specifies "Comply / Partial / Take Exception" verbiage, those exact phrases (not "COMPLIANT / PARTIAL / NON-COMPLIANT") should appear.

**Pass:** Matrix format aligns with RFP's required attachment format (or no specific format demanded — generic matrix accepted).

**Concern:** Format generally aligns but uses different terminology (COMPLIANT vs Comply) — log advisory.

**Fail:** RFP demands a specific attachment format and response matrix uses fundamentally different columns/verbiage.

**Evidence to cite:** SUBMISSION_STRUCTURE.json relevant excerpt; response matrix column header line.

---

### Check 9 — Category summary present and accurate

**Criterion:** The document must contain a "Category Summary" or "Category Breakdown" section with per-category compliance rates. Each category from `requirements-normalized` should have an entry showing total + compliance breakdown.

**Pass:** Category summary section present; rates match per-category row counts.

**Concern:** Section present but rates off by > 5%.

**Fail:** Category summary absent.

**Evidence to cite:** Grep for category summary heading; per-category rate accuracy check.

---

### Check 10 — Universal regression patterns

**Criterion:** Five sub-checks:
(a) UTF-8 decode clean; no mojibake.
(b) Zero `_Showing \d+ of \d+_` notices (also covered in Check 3, restate for completeness).
(c) Zero `[:N]` truncation patterns in deliverable strings.
(d) Zero em-dash chars (`—` U+2014).
(e) Zero empty `|  |` patterns in data rows where adjacent cells contain text (indicates skipped column).

**Pass:** All sub-checks pass.

**Fail:** Any sub-check has 1+ violation.

**Evidence to cite:** Per violation: line + first 80 chars quoted.

---

## Disposition Logic

- **PASS:** All 10 checks pass.
- **CONCERN:** Checks 4, 5, 8, or 9 in advisory band; all others pass.
- **FAIL:** Any of Checks 1, 2, 3, 6, 7, 10 fail OR Checks 4/5/8/9 fall below FAIL threshold.

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase8.4r-reqreview with the following targeted corrections:

[Check 1 fail] 04_REQUIREMENTS_REVIEW.md missing or < 5 KB ({actual_size} bytes).
  Action: Re-run. Confirm requirements-normalized.json has populated requirements list
  AND COMPLIANCE_MATRIX.json loaded.

[Check 2 fail] Row count {N} is {pct}% below source count {M}.
  Action: CRITICAL — find any [:N] slice or `high_critical[:200]` cap on the
  iteration. Step 2 must iterate ALL requirements (or the documented
  CRITICAL+HIGH filter set), with zero per-priority or per-category caps.

[Check 3 fail] Row-cap notice found at line {N}: "{quoted text}".
  Action: Remove the "_Showing N of M_" entirely. Per phase ⛔ discipline:
  "NEVER emit '_Showing N of M_' notices. Show everything."

[Check 4 fail] {N} rows missing status OR summary mismatch by {delta}.
  Action: Step 2 `compliance_status` derivation must run for every row.
  After table render, recount each status category and update Response Summary
  with the recounted values (don't rely on accumulator counters that may have
  desynced).

[Check 5 fail] {N} mandatory items lack response coverage: {list}.
  Action: For each, identify its linked_requirement_ids (or do a text-similarity
  match if linkage absent). Ensure each linked requirement appears in the
  response matrix as a row.

[Check 6 fail] linked_spec_ids / bid_section_ids truncated in row(s): {list}.
  Action: Remove ALL `[:3]` and `[:2]` head slices. Render with `', '.join(rtm_req[...])`
  for both fields. Per 2026-05-19 fix.

[Check 7 fail] Mid-word truncation in requirement text at row(s): {list}.
  Action: Step 2's "requirement": text assignment must use the FULL text string.
  Remove any [:200] or [:N] slice. Per 2026-05-18 fix.

[Check 8 fail] Format mismatch with RFP Attachment {A/H}.
  Action: Re-read SUBMISSION_STRUCTURE.json. Adopt the column structure and
  verbiage from the demanded attachment. If RFP says "Comply", use "Comply"
  not "COMPLIANT".

[Check 9 fail] Category summary missing.
  Action: After building response_rows, group by category and compute compliance
  rates. Render as a sub-table.

[Check 10 fail] Regression pattern {type} at line {N}.
  Action: Strip em dashes, eliminate [:N] slices, fix empty cells, remove
  row caps.

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input:**

```
04_REQUIREMENTS_REVIEW.md content:
  File size: 4,200 bytes.
  Row count: 200 of 762 source requirements (74% under).
  Line 18: "_Showing 200 of 762 requirements_"
  35 rows have empty Status cells.
  Row REQ-0245 ends "Sys must enable rapid reporting of fina..."
  RFP demands Attachment H format with "Comply / Partial / Take Exception" verbiage;
    matrix uses "COMPLIANT / PARTIAL / NON-COMPLIANT" instead.
  Bid Reference column shows "BS-001, BS-002" but RTM source has 5 bid_section_ids.
  3 mandatory items have no corresponding response rows.
```

Verifier MUST detect: Check 1 (< 5 KB), Check 2 (200 vs 762 = 74% under), Check 3 (row-cap notice), Check 4 (empty status cells), Check 5 (3 unaddressed mandatories), Check 6 (truncated bid_section_ids), Check 7 (mid-word "fina..."), Check 8 (format mismatch). Disposition: FAIL.

**Known-good input:**

```
04_REQUIREMENTS_REVIEW.md: 124 KB (renders all 2,167 requirements per RFP).
  Row count = 2,167 = source count exactly.
  No "_Showing N of M_" patterns anywhere.
  Every row has a recognized response code (Comply / Partial / Take Exception /
    Alternate — matches RFP Attachment H verbiage).
  Summary table counts match per-row counts to the unit.
  All 47 mandatory items from COMPLIANCE_MATRIX have linked requirement(s)
    appearing in the matrix.
  Sampled 5 rows with 4+ linked_spec_ids: all show full comma-joined list.
  Sampled 10 rows for mid-word truncation: zero issues.
  Category summary present with rates matching per-category counts.
  UTF-8 clean, no em dashes, no empty cells.
```

Verifier MUST PASS all checks. Disposition: PASS.

## Hard Rules (inherited from verifier-design principles)

1. NEVER claim 04_REQUIREMENTS_REVIEW.md is missing without `Glob` verification first.
2. NEVER flag a small file size when the RFP genuinely has < 50 requirements — small RFPs produce small matrices legitimately. Adjust Check 1 expectation to `size >= max(5120, requirement_count * 80)` heuristic.
3. NEVER flag a tamper-\nproof line-wrap as truncation in Check 7 — apply the hyphenation continuation rule.
4. NEVER fail Check 8 if SUBMISSION_STRUCTURE.json doesn't reference any specific attachment format — generic matrix passes vacuously.
5. NEVER flag the table separator row (`|---|---|---|`) as a missing-status row in Check 4 — it's a header artifact, not a data row.
6. Every finding must cite line number + first 80 chars of offending row (or req_id + measurement for Checks 5, 6).
