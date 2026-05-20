---
name: verifier-phase1.7-compliance-win
expert-role: Compliance Gate Integrity Verifier
purpose: phase-boundary verifier for phase1.7-compliance-win (mandatory item count plausibility, source linkage, gate status)
created: 2026-05-20
---

# Verifier — Phase 1.7 Compliance Matrix

## When this runs

After phase1.7-compliance-win reports done, BEFORE phase1.8-submission-win. Phase 1.7 is itself a BLOCKING GATE — this verifier validates that the gate's claim of "passed" or "failed" is structurally and quantitatively sound (correct item counts, every item sourced, frameworks grep-verifiable against RFP text).

## Inputs (read in this order)

1. `{folder}/shared/COMPLIANCE_MATRIX.json` — primary output under verification
2. `{folder}/shared/domain-context.json` — for compliance_frameworks cross-reference
3. `{folder}/flattened/*.md` — RFP text for framework citation verification

## Verification Checks

### Check 1 — File exists and is valid JSON, structurally complete

**Criterion:** `shared/COMPLIANCE_MATRIX.json` exists, parses as valid JSON, top-level keys include `extracted_at`, `gate_status`, `format_requirements`, `mandatory_items`, `category_summary`, `rtm_entities`. File size >= 500 bytes.

**Pass:** All required keys present, JSON parses, size threshold met.

**Fail:** File absent, parse error, any key missing, or size < 500 bytes.

**Evidence to cite:** Missing key path or parse error; actual file size.

---

### Check 2 — `gate_status` block has required fields

**Criterion:** `gate_status` is a dict containing: `passed` (bool), `total_mandatory` (int), `addressed` (int), `gaps` (int), `coverage_percentage` (float 0-100), `gap_items` (list).

**Pass:** All 6 fields present with correct types.

**Fail:** Any field missing or wrong type (e.g., `passed` is string "true" instead of bool).

**Evidence to cite:** Field name + actual value + actual type.

---

### Check 3 — `mandatory_items` count is plausible

**Criterion:** `len(mandatory_items) >= 20` for a real RFP. Compliance-heavy RFPs (Federal, state procurement, healthcare) typically have 50-500 mandatory items. Items below 20 indicate extraction failure or a stub document.

**Pass:** count >= 20.

**Concern:** count is in `[10, 19]` — log advisory; may be a small RFP or a sole-source vehicle. Allow but flag.

**Fail:** count < 10 (extraction broken).

**Alert (high):** count > 2,000 — likely over-extraction (every SHALL captured including sample-contract / form boilerplate). Phase should have applied scope filter. Mark CONCERN and recommend re-run with stricter pattern set.

**Evidence to cite:** Actual count of `mandatory_items` and breakdown by `source_type`.

---

### Check 4 — Every mandatory item has `source_ids`

**Criterion:** Every entry in `mandatory_items` has a non-empty `source_ids` list (linking to `rtm_entities.rfp_sources`). Each source_id MUST match pattern `^SRC-\d{3,}$` (3-or-more digits, zero-padded minimum).

**Why the open-ended digit width (codified 2026-05-20):** The previous regex `^SRC-\d{3}$` mathematically caps the addressable item space at 999. The MARS run with foundation-tier atomic-RFPAttH extraction produced 1,319 items (SRC-0001 through SRC-1319), and 320 of them legitimately have 4-digit IDs. Compliance-heavy government RFPs routinely cross 1,000 mandatory items — the regex must admit them.

**Pass:** All items have `source_ids` populated AND all IDs match the pattern.

**Fail:** Any item with empty `source_ids` OR any malformed source_id (downstream RTM build will silently drop these).

**Evidence to cite:** Count of items missing source_ids + index of first 5 offenders.

---

### Check 5 — `rtm_entities` populated

**Criterion:** `rtm_entities` dict contains `rfp_sources` (list, non-empty) and `mandatory_items` (list, length equal to top-level `mandatory_items`). Every entry in `rtm_entities.mandatory_items` MUST have keys: `mandatory_id`, `text`, `source_type`, `category`, `coverage_status`, `source_ids`, `linked_requirement_ids`, `linked_compliance_framework_ids`.

**Pass:** Both arrays present with matching schema; mandatory_items count matches.

**Fail:** Either array missing, empty, or schema-incomplete.

**Evidence to cite:** `len(rtm_entities.rfp_sources)`, `len(rtm_entities.mandatory_items)`, top-level `len(mandatory_items)`.

---

### Check 6 — Non-negotiable (high-priority) items list non-empty

**Criterion:** At least 1 mandatory item has `source_type` in `{"SHALL", "MUST", "MANDATORY", "OFFEROR_SHALL", "CONTRACTOR_SHALL", "PROPOSAL_MUST"}`. Real RFPs always have at least a handful of these strict-obligation items.

**Pass:** >= 1 strict-obligation item.

**Fail:** Zero strict-obligation items (every item came from looser patterns like "required to" — likely an over-broad match that picked up non-mandatory language).

**Evidence to cite:** Count by `source_type` (group-by histogram).

---

### Check 7 — `coverage_status` field present on every item, valid value

**Criterion:** Every `mandatory_items` entry has `coverage.status` (or `rtm_entities.mandatory_items[N].coverage_status`) in the allowed set `{"PLANNED", "PARTIAL", "ADDRESSED", "GAP", "WAIVED"}`. No null values. At Phase 1.7 boundary, status is typically `PLANNED`.

**Pass:** Every item has a valid coverage_status.

**Fail:** Any null status OR value outside the allowed set.

**Evidence to cite:** Status counts (group-by histogram) + index of any null offenders.

---

### Check 8 — `coverage_percentage` is computed and arithmetically consistent

**Criterion:** `gate_status.coverage_percentage` ≈ `(gate_status.addressed / gate_status.total_mandatory) * 100`, within ±0.5 rounding. Also: `gate_status.total_mandatory == len(mandatory_items)` and `gate_status.addressed + gate_status.gaps == gate_status.total_mandatory` (gap-vs-addressed accounting).

**Pass:** All three arithmetic identities hold.

**Fail:** Any identity violated.

**Evidence to cite:** Print the three values + recomputed expected values + deltas.

---

### Check 9 — `compliance_frameworks` reflects actual RFP language

**Criterion:** If `domain-context.json` lists compliance frameworks (HIPAA, FERPA, FedRAMP, Section 508, GASB, etc.) and they appear in `COMPLIANCE_MATRIX.json` (in mandatory item text, format_requirements, or a frameworks list), each framework name MUST be grep-verifiable in flattened text. Spot-check 3 frameworks if present.

**Pass:** All sampled frameworks grep-verify.

**Concern:** 1 of 3 sampled fails to grep — log advisory; may be inferred from domain context.

**Fail:** Multiple frameworks claimed but absent from RFP text (hallucinated compliance scope).

**Evidence to cite:** Framework name + grep result count + claim location.

---

### Check 10 — `category_summary` totals equal `len(mandatory_items)`

**Criterion:** `sum(category_summary.values()) == len(mandatory_items)`. The category histogram must account for every item exactly once.

**Pass:** Sums match exactly.

**Fail:** Mismatch (categorization dropped or double-counted items).

**Evidence to cite:** `sum(category_summary.values())` vs `len(mandatory_items)`.

---

### Check 11 — Anti-truncation / anti-row-cap / encoding integrity

**Criterion:** No string value in the JSON contains `_Showing N of M_` patterns, mid-word `[:N]` truncations, or mojibake (`â€"`, etc.). Item `text` fields MUST NOT end with a truncation token; phase already caps at 300 chars but this is a content truncation, not a `_Showing_` row cap.

**Pass:** Zero matches for `_Showing_` patterns and zero mojibake. (300-char `text` cap is allowed but verify it's a clean cut, not mid-word with `[:300]` literal.)

**Fail:** Any literal `_Showing N of M_` notice OR any `[:\d+]` literal in deliverable strings OR mojibake found.

**Evidence to cite:** Path + offending value snippet.

---

## Disposition Logic

- **PASS:** Checks 1, 2, 4, 5, 6, 7, 8, 10, 11 pass AND Check 3 not in advisory/alert band AND Check 9 meets threshold.
- **CONCERN:** Check 3 in `[10, 19]` OR Check 3 > 2,000 (over-extraction alert) OR Check 9 single-sample miss. Log + continue or recommend re-run.
- **FAIL:** Any of Checks 1, 2, 4, 5, 6, 7, 8, 10, 11 fail OR Check 3 < 10 OR Check 9 multi-sample miss.

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase1.7-compliance-win with the following targeted corrections:

[Check 1 fail] COMPLIANCE_MATRIX.json missing or invalid.
  Action: Re-run from Step 1. Verify flattened/*.md and domain-context.json exist first.

[Check 2 fail] gate_status missing fields or wrong types.
  Action: Audit Step 6 calculate_gate_status. Ensure return dict has all 6 fields with
  correct types (passed=bool, counts=int, coverage_percentage=float).

[Check 3 fail] Mandatory item count < 10.
  Action: Audit Step 2 MANDATORY_PATTERNS. Likely cause: regex compilation issue or the
  RFP uses non-standard obligation phrasing (e.g., "is to" or "will be"). Add patterns,
  then re-run.

[Check 3 alert] Mandatory item count > 2,000 — over-extraction.
  Action: Apply scope filter — exclude items matched inside sample contract clauses,
  bidder form templates, or ToC chrome (refer to MARS round-1 catalog incident,
  feedback_requirements_extraction_quality.md). Tighten MANDATORY_PATTERNS or add a
  pre-filter on file basename (skip *_sample_contract.md, *_form_*.md).

[Check 4 fail] Items missing source_ids.
  Offending count: {N}. Action: Audit Step 6b — the source_id_counter loop must assign
  a source_id to EVERY mandatory item; verify no early-exit in the loop body.

[Check 5 fail] rtm_entities missing or schema-incomplete.
  Action: Re-run Step 6b in full. The rtm_mandatory_items list-comp must include every
  documented field; do not skip linked_compliance_framework_ids (it can be []).

[Check 6 fail] Zero strict-obligation items.
  Action: Audit Step 2 — the SHALL / MUST patterns must be the primary matchers.
  Likely cause: regex order placed weaker patterns first and they consumed the matches.
  Reorder MANDATORY_PATTERNS so SHALL/MUST patterns run before "required to" / generic.

[Check 7 fail] Items with invalid coverage_status.
  Offending: {list}. Action: Audit Step 5 assess_coverage default — every item should
  return status="PLANNED" at Phase 1.7 time. Null status indicates the assess_coverage
  function returned None somewhere; add explicit default.

[Check 8 fail] gate_status arithmetic inconsistent.
  Action: Re-run Step 6 calculate_gate_status. Verify total_mandatory == len(items),
  addressed + gaps == total_mandatory, coverage_percentage == (addressed/total)*100.

[Check 9 fail] Compliance frameworks claimed but not in RFP text.
  Offending: {list}. Action: Either remove the framework claim OR grep-verify it before
  adding. Hallucinated frameworks come from domain-profile defaults — apply them only
  if the RFP text confirms.

[Check 10 fail] Category summary doesn't match item count.
  Sum: {x}, items: {y}. Action: Audit Step 3 categorize_item — every item must return
  a valid category (default "OTHER"); ensure OTHER is in the CATEGORIES.keys() iteration
  for the summary.

[Check 11 fail] Row-cap, truncation, or mojibake.
  Action: Ensure encoding='utf-8' and ensure_ascii=False on all I/O. The 300-char
  text cap in Step 2 should be a clean .strip() cut, not literal "[:300]" in the
  output string.

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input (MARS round-1 over-extraction):**

```
COMPLIANCE_MATRIX.json scenario:
  mandatory_items has 2,847 entries (alert — way over 2,000).
  ~383 of them are clauses from a sample contract attached as Exhibit C.
  ~140 items have source_ids=[] (silent drop).
  gate_status.total_mandatory=2,847 but gate_status.addressed + gate_status.gaps = 2,705.
  category_summary sums to 2,830 (17 items lost to category function).
  Item M2843.text ends in literal "[:300]" (slice notation leaked into output).
  Frameworks list includes "FedRAMP High" but no grep hits in flattened text.
```

Verifier MUST detect: Check 3 (alert: over-extraction), Check 4 (140 missing source_ids), Check 8 (arithmetic mismatch), Check 9 (FedRAMP hallucinated), Check 10 (category sum mismatch), Check 11 ([:300] literal). Disposition: FAIL.

**Known-good input:**

```
COMPLIANCE_MATRIX.json scenario:
  mandatory_items has 287 entries.
  All 287 have non-empty source_ids matching SRC-\d{3,}.
  gate_status: total_mandatory=287, addressed=287, gaps=0, coverage_percentage=100.0,
    passed=true.
  Source_type distribution: SHALL=180, MUST=70, MANDATORY=15, REQUIRED=22.
  category_summary sums to 287.
  All compliance_frameworks grep-verify in flattened text.
  No truncation literals, no mojibake.
```

Verifier MUST PASS all checks. Disposition: PASS.

## Hard Rules

1. NEVER claim COMPLIANCE_MATRIX.json is missing without `Glob` verification first.
2. NEVER accept a compliance framework name without grep-verifying it appears in the RFP text.
3. NEVER ignore the > 2,000 item alert — over-extraction is functionally as bad as under-extraction (downstream RTM and bid sections become unactionable).
4. Every finding must cite a specific field path + value + recomputed expected (e.g., `gate_status.coverage_percentage = 88.2 but addressed/total = 287/287 = 100.0`).
5. On FAIL, return corrective instructions targeting the specific step number (Step 2 patterns, Step 5 assess_coverage, Step 6 gate math) for surgical repair.
