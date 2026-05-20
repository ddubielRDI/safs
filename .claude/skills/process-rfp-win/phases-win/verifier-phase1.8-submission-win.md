---
name: verifier-phase1.8-submission-win
expert-role: Submission Structure Integrity Verifier
purpose: phase-boundary verifier for phase1.8-submission-win (volume count, deadlines, naming, file format, phase mappings)
created: 2026-05-20
---

# Verifier — Phase 1.8 Submission Structure

## When this runs

After phase1.8-submission-win reports done, BEFORE phase1.85-questions-win. Phase 1.85 consumes the `questions_deadline` posture from this output; Phase 1.9 consumes deadlines for risk scoring; Phase 8.x consumes volume names + file_format for output assembly. A faulty submission structure cascades into mis-named output files and missed deadlines.

## Inputs (read in this order)

1. `{folder}/shared/SUBMISSION_STRUCTURE.json` — primary output under verification
2. `{folder}/shared/COMPLIANCE_MATRIX.json` — for cross-reference with format_requirements
3. `{folder}/flattened/*.md` — RFP text for deadline + volume grep-verification

## Verification Checks

### Check 1 — File exists and is valid JSON, structurally complete

**Criterion:** `shared/SUBMISSION_STRUCTURE.json` exists, parses as valid JSON, top-level keys include `detected_at`, `detection_confidence`, `volumes`, `format_requirements`, `naming_convention`, `assembly_instructions`. File size >= 2,048 bytes (per phase spec).

**Pass:** All required keys present, parses, size >= 2,048.

**Fail:** File absent, parse error, key missing, or size < 2,048.

**Evidence to cite:** Missing key path or actual file size.

---

### Check 2 — At least 3 volumes detected

**Criterion:** `len(volumes) >= 3`. Real RFPs typically require 3-7 volumes (technical, management, cost, plus letter / qualifications / certifications).

**Pass:** Length >= 3.

**Concern:** Length == 2 — log advisory; may be a simple Q-only or LPTA RFP.

**Fail:** Length < 2 (detection failed; default template should have kicked in to Step 7 — verify why it didn't).

**Evidence to cite:** Actual volume count + list of volume titles.

---

### Check 3 — Every volume has `title` + `order`

**Criterion:** Every entry in `volumes` has non-empty `title` (string) AND `order` (int >= 1). Duplicate orders are allowed only when one is from default template and one is detected.

**Pass:** All volumes have both keys, no nulls.

**Fail:** Any volume with missing or null `title` or `order`.

**Evidence to cite:** Index of offending volumes + their actual keys.

---

### Check 4 — `page_limit` captured where RFP specifies

**Criterion:** If `flattened/*.md` contains the phrase "page limit" or "not to exceed N pages" (case-insensitive, grep), then `overall_page_limit` field MUST be non-null OR at least one volume MUST have `page_limit` populated.

**Pass:** Either condition met when RFP mentions page limits, OR RFP does not mention page limits.

**Fail:** RFP mentions page limits but neither `overall_page_limit` nor any per-volume `page_limit` is populated.

**Evidence to cite:** Grep result (count of page-limit mentions in flattened text) + `overall_page_limit` value + per-volume page_limit values.

---

### Check 5 — `due_date` / submission deadline present and parseable

**Criterion:** The submission deadline (either `assembly_instructions.due_date`, `format_requirements.deadline`, or a deadline field at top-level) MUST be present in some form. If present, it should be parseable as an ISO date or a clear free-form date string (e.g., "January 15, 2026 at 5:00 PM PST"). NULL is acceptable ONLY if grep of flattened text confirms no due date is published.

**Pass:** Deadline present AND parseable as date, OR null with grep-verified absence.

**Concern:** Deadline present as free-form text that doesn't match common date formats — log advisory; downstream code may fail to parse.

**Fail:** RFP text grep-shows a "due by" / "submit no later than" string but the output has no deadline anywhere.

**Evidence to cite:** Deadline field path + value + grep results from flattened text.

---

### Check 6 — `questions_deadline` posture surfaced

**Criterion:** A `questions_deadline` field MUST be present (top-level OR inside an `assembly_instructions` block OR a `submission_schedule` block). Acceptable values: `"open"`, `"closing_soon"`, `"closed"`, `"expired"`, `"none"`, `"not_permitted"`, `"unknown"`. Phase 1.85 reads this — null here breaks Phase 1.85 mode resolution.

**Pass:** Field present with one of the allowed values.

**Concern:** Field present as `"unknown"` — log advisory; Phase 1.85 will fall back to "open" mode.

**Fail:** Field absent.

**Evidence to cite:** Field path + value (or "absent").

---

### Check 7 — `naming_convention.pattern` present

**Criterion:** `naming_convention` is a dict with `pattern` key populated as a non-empty string (may be the default `"{Bidder}_{Volume}_{Title}.pdf"` if RFP didn't specify). The `detected` flag indicates whether it was extracted from RFP text vs default.

**Pass:** `naming_convention.pattern` non-null and non-empty.

**Fail:** Field missing or empty.

**Evidence to cite:** Print `naming_convention` dict.

---

### Check 8 — `file_format` specified

**Criterion:** `format_requirements.file_format` is one of `{"PDF", "DOCX", "DOC", "ZIP", "RFP_PORTAL"}` or similar (a real file-format token, not free text). Default "PDF" is acceptable.

**Pass:** Value is a recognized format token.

**Fail:** Value is null, free-text, or empty.

**Evidence to cite:** Actual `file_format` value.

---

### Check 9 — `mapped_phase` populated for content volumes

**Criterion:** Volumes whose title implies content authorship (containing tokens: "technical", "management", "solution", "cost", "financial", "price", "submittal", "letter", "qualifications", "approach") MUST have `mapped_phase` populated. Volumes that are pure forms / certifications / attachments MAY have `mapped_phase` null. Compute coverage: `mapped / content_titled_volumes >= 0.80`.

**Pass:** >= 80% of content-titled volumes have mapped_phase set.

**Concern:** 60-79% — log advisory.

**Fail:** < 60% (content_mapping dict in Step 8 not catching the volume keywords).

**Evidence to cite:** For each volume, title + mapped_phase value + classification (content/form).

---

### Check 10 — Volume titles grep-verify in flattened text (anti-hallucination)

**Criterion:** For each volume where `detection_method != "default_template"`, at least 70% must have their `title` grep-verifiable in flattened text (case-insensitive, with word-boundary tolerance for stemming — e.g., "Management Proposal" matches "Management Proposals").

**Pass:** >= 70% grep-verify.

**Concern:** 50-69% — log advisory; may be name-paraphrasing.

**Fail:** < 50% grep-verify (volume names hallucinated).

**Evidence to cite:** For each detected volume: title + grep result count.

---

### Check 11 — Cross-stage consistency with COMPLIANCE_MATRIX format_requirements

**Criterion:** If `COMPLIANCE_MATRIX.json.format_requirements.page_limit` exists, it should equal `SUBMISSION_STRUCTURE.json.overall_page_limit` (or be a per-volume limit). Allow ±0 for exact match, or null on the SUBMISSION side if COMPLIANCE captured the only mention.

**Pass:** Values match OR SUBMISSION has more specific per-volume limits while COMPLIANCE has overall.

**Concern:** Values differ by > 5% — log advisory.

**Fail:** Values directly contradict (e.g., COMPLIANCE says 50 pages, SUBMISSION says 200) — one of the phases mis-parsed.

**Evidence to cite:** Both values + reconciliation note.

---

### Check 12 — Anti-truncation / anti-row-cap / encoding integrity

**Criterion:** No string value contains `_Showing N of M_` notices, mid-word `[:N]` truncations, or mojibake (`â€"`, etc.).

**Pass:** Zero matches.

**Fail:** Any match.

**Evidence to cite:** Path + offending value.

---

## Disposition Logic

- **PASS:** Checks 1, 3, 6, 7, 8, 11, 12 pass AND Check 2 not in advisory band AND Checks 4, 5, 9, 10 meet threshold.
- **CONCERN:** Check 2 == 2 volumes OR Check 5 has unparseable free-text deadline OR Check 6 == "unknown" OR Checks 9/10 in advisory band OR Check 11 small drift. Log + continue.
- **FAIL:** Any of Checks 1, 3, 7, 8, 12 fail OR Check 2 < 2 OR Check 4 missed when RFP mentions limits OR Check 5 missed deadline OR Check 6 absent OR Checks 9/10 below FAIL threshold OR Check 11 direct contradiction.

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase1.8-submission-win with the following targeted corrections:

[Check 1 fail] SUBMISSION_STRUCTURE.json missing, invalid, or under-size.
  Action: Re-run from Step 1. Verify flattened/*.md and COMPLIANCE_MATRIX.json exist.
  Size < 2,048 → ensure all required fields are populated, not just a stub.

[Check 2 fail] Fewer than 2 volumes detected, default template did not trigger.
  Action: Verify Step 7 default-template fallback runs when `len(volumes) < 3` and that
  it merges (does not replace) detected volumes.

[Check 3 fail] Volumes missing title or order.
  Action: Audit Step 3 — every regex append must set both `title` and `order`. The
  numbered_volume + lettered_item paths set order; the filename_pattern path may not;
  add order assignment there.

[Check 4 fail] Page limits in RFP but not captured.
  Action: Audit Step 4 page_limit_pattern. Likely the RFP uses an unmatched phrasing
  (e.g., "limited to N pages", "shall be no more than N pages"). Extend the pattern set.

[Check 5 fail] Submission deadline missing despite RFP mention.
  Action: Audit deadline extraction. Add a fallback Step that grep-searches for
  "due", "submit by", "no later than" and captures the trailing date phrase.

[Check 6 fail] questions_deadline field absent.
  Action: Add explicit questions_deadline detection step. Search flattened text for
  "questions deadline", "Q&A deadline", "submit questions by" — set status accordingly.

[Check 7 fail] naming_convention.pattern missing or empty.
  Action: Audit Step 6. Ensure the default value is always written even when no pattern
  is detected (`{Bidder}_{Volume}_{Title}.pdf` per the phase spec).

[Check 8 fail] file_format not a recognized token.
  Action: Audit Step 5. Default to "PDF" if no other format detected. Free-text values
  should be canonicalized to the closed set.

[Check 9 fail] Content volumes lack mapped_phase.
  Failing volumes: {list}. Action: Audit Step 8 content_mapping dict. Add missing
  title-keyword entries; ensure title_lower scan iterates the full mapping.

[Check 10 fail] Volume titles don't grep-verify.
  Failing volumes: {list}. Action: This is hallucination — titles were generated from
  default template even when RFP didn't reference them. Tighten detection-method
  attribution so default_template volumes are correctly tagged and excluded from
  grep-verification expectations.

[Check 11 fail] Page limit conflict with COMPLIANCE_MATRIX.
  COMPLIANCE: {x}, SUBMISSION: {y}. Action: Reconcile by re-reading the cited RFP
  sections. Determine which phase mis-parsed (likely the broader pattern picked up
  a different limit).

[Check 12 fail] Row-cap, truncation, or mojibake.
  Action: Ensure encoding='utf-8' and ensure_ascii=False on all I/O.

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input:**

```
SUBMISSION_STRUCTURE.json scenario:
  volumes has 2 entries (just "Cover Letter" and "Technical Proposal").
  questions_deadline field absent entirely.
  overall_page_limit = null, no per-volume page_limit, yet flattened text contains
    "Technical Proposal shall not exceed 100 pages" at line 412.
  file_format = "best_effort_pdf" (free text, not a token).
  3 of 5 "detected" volumes have titles that don't appear in flattened text
    (filled by default template but tagged "lettered_item").
  COMPLIANCE_MATRIX.format_requirements.page_limit = 100; SUBMISSION has none.
```

Verifier MUST detect: Check 2 (concern — 2 volumes), Check 4 (page limit in text but not captured), Check 6 (questions_deadline absent), Check 8 (free-text format), Check 10 (titles don't grep-verify), Check 11 (page-limit conflict). Disposition: FAIL.

**Known-good input:**

```
SUBMISSION_STRUCTURE.json scenario:
  volumes has 5 entries: Letter of Submittal (1), Management Proposal (2),
    Technical Approach (3), Business Solution (4), Cost Proposal (5).
  All titles grep-verify in flattened/*.md.
  overall_page_limit = 100; volume "Technical Approach" page_limit = 60.
  due_date = "2026-06-30T17:00:00-08:00" (ISO parseable).
  questions_deadline = "closing_soon" with date "2026-06-10".
  naming_convention.pattern = "RDI_{volume}_{title}.pdf", detected=true.
  file_format = "PDF".
  All 5 volumes have mapped_phase set (8.1 through 8.5).
  COMPLIANCE_MATRIX.format_requirements.page_limit = 100 (matches).
```

Verifier MUST PASS all checks. Disposition: PASS.

## Hard Rules

1. NEVER claim SUBMISSION_STRUCTURE.json is missing without `Glob` verification first.
2. NEVER accept volume titles without spot-grep-verifying them in flattened text (default-template volumes are excused but must be tagged as such).
3. NEVER ignore page-limit conflicts between COMPLIANCE_MATRIX and SUBMISSION_STRUCTURE — one phase mis-parsed and downstream phases will use the wrong number.
4. Every finding must cite a specific field path + value + grep result count.
5. On FAIL, return corrective instructions targeting the specific Step number (Step 3 volume parsing, Step 4 page limits, Step 6 naming, Step 8 phase mapping) for surgical repair.
