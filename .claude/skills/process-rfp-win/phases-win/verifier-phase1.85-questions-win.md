---
name: verifier-phase1.85-questions-win
expert-role: Clarifying Questions Integrity Verifier
purpose: phase-boundary verifier for phase1.85-questions-win (CQ markdown, summary JSON, severity tiers, no competitive leakage)
created: 2026-05-20
---

# Verifier — Phase 1.85 Clarifying Questions

## When this runs

After phase1.85-questions-win reports done, BEFORE phase1.9-gonogo-win. Phase 1.9 reads `clarifying-questions-summary.json` to assess "questions deadline already passed" as a risk input. This verifier ensures the summary is well-formed AND the human-facing CQ document does not leak competitive position.

## Inputs (read in this order)

1. `{folder}/outputs/CLARIFYING_QUESTIONS.md` — human-readable markdown output
2. `{folder}/shared/clarifying-questions-summary.json` — machine-readable summary consumed by Phase 1.9
3. `{folder}/shared/SUBMISSION_STRUCTURE.json` — for deadline posture cross-check
4. `{folder}/shared/COMPLIANCE_MATRIX.json` — for mandatory-item source spot-checks

## Verification Checks

### Check 1 — CLARIFYING_QUESTIONS.md exists with sufficient size

**Criterion:** `outputs/CLARIFYING_QUESTIONS.md` exists, is UTF-8 readable, size >= 2,048 bytes (per phase spec). Empty or stub files indicate the questions pipeline did not actually run.

**Pass:** File exists, size >= 2,048 bytes, UTF-8 readable.

**Fail:** File absent, < 2,048 bytes, or encoding error on read.

**Evidence to cite:** Actual file size + first 200 bytes of content.

---

### Check 2 — Summary JSON exists and is valid

**Criterion:** `shared/clarifying-questions-summary.json` exists, parses as valid JSON, contains top-level keys: `generated_at`, `mode`, `deadline_status`, `deadline_date`, `totals`.

**Pass:** All keys present, JSON parses.

**Fail:** File absent, parse error, or any key missing.

**Evidence to cite:** Missing key path or parse-error line.

---

### Check 3 — `totals` block has all 4 severity tiers

**Criterion:** `totals` is a dict with keys: `critical`, `important`, `nice_to_have`, `all`. All values are non-negative integers. `critical + important + nice_to_have == all` (arithmetic consistency).

**Pass:** All 4 keys present with int >= 0 values AND arithmetic identity holds.

**Fail:** Key missing, negative value, or sum mismatch.

**Evidence to cite:** Print the `totals` block + recomputed sum vs `all`.

---

### Check 4 — `deadline_status` surfaced

**Criterion:** `deadline_status` field is a non-empty string with one of the recognized values: `"open"`, `"closing_soon"`, `"imminent"`, `"closed"`, `"expired"`, `"none"`, `"not_permitted"`, `"unknown"`. Cross-check: this should match `SUBMISSION_STRUCTURE.json.questions_deadline` (allowing canonicalization e.g., `closing_soon` ↔ `imminent`).

**Pass:** Value present and in allowed set AND consistent with SUBMISSION value.

**Concern:** Value is `"unknown"` — log advisory.

**Fail:** Value missing OR not in allowed set OR contradicts SUBMISSION (e.g., SUBMISSION says open, summary says closed).

**Evidence to cite:** Summary value + SUBMISSION value + reconciliation.

---

### Check 5 — `mode` derived correctly from deadline_status

**Criterion:** Per phase Step 2 logic:
- `closed` / `expired` → mode = `post_deadline`
- `closing_soon` / `imminent` → mode = `triage_critical`
- `none` / `not_permitted` → mode = `internal_only`
- otherwise → mode = `open`

**Pass:** Mode value consistent with deadline_status mapping.

**Fail:** Mode contradicts the deadline_status (e.g., status=closed but mode=open).

**Evidence to cite:** Both values + expected mapping.

---

### Check 6 — Every Critical question has source OR evidence_snippet

**Criterion:** In CLARIFYING_QUESTIONS.md, parse the `## Critical Questions` section. Every question entry MUST have either an `**RFP source:**` line OR an `**Evidence snippet:**` block. Unsupported critical questions are inadmissible — agencies will reject them as "research questions" not "clarification requests."

**Pass:** All Critical questions have source OR evidence.

**Concern:** 1 critical question missing both — log advisory.

**Fail:** > 1 critical question lacks both.

**Evidence to cite:** Critical question IDs + which lack support.

---

### Check 7 — Three severity tiers present in markdown

**Criterion:** CLARIFYING_QUESTIONS.md contains headers `## Critical Questions`, `## Important Questions`, `## Nice-to-have Questions` (or equivalent). If the tier is empty per `totals`, the header may be omitted; otherwise the header MUST be present.

**Pass:** Every non-zero tier has its corresponding header.

**Fail:** Non-zero tier missing its header (questions not properly bucketed).

**Evidence to cite:** Grep results for each header + tier totals.

---

### Check 8 — No competitive-position leakage in CQs

**Criterion:** CLARIFYING_QUESTIONS.md MUST NOT contain (outside the "Submission Guidance" section):
- `"win theme"`, `"win-theme"`
- `"discriminator"`, `"discriminating strength"`
- `"ghost"` (as in ghost strategy)
- `"competitive position"`, `"vs competitor"`, `"against the incumbent"`
- Specific competitor proper nouns (cross-ref against COMPETITIVE_POSITION if it exists)
- Bidder's own pricing intent ("our price will be...", "we plan to bid...")
- Proper nouns of the bidder (e.g., "Resource Data") in question text — questions should be 3rd-person neutral

**Pass:** Zero matches for leakage phrases outside "Submission Guidance" section.

**Fail:** Any leakage phrase found.

**Evidence to cite:** Line number + offending phrase + section it appears in.

---

### Check 9 — `phase1_9_input` field populated for Go/No-Go consumption

**Criterion:** Either the summary JSON OR the CQ markdown footer must surface a phase1_9_input block (or equivalent — the phase spec mentions Phase 1.9 reads totals + deadline status). Specifically, `clarifying-questions-summary.json` must have `totals.all` and `deadline_status` (these ARE the Phase 1.9 inputs).

**Pass:** Both fields exist and are non-null (totals.all is int, deadline_status is recognized string).

**Fail:** Either field missing or null.

**Evidence to cite:** Both values from the summary.

---

### Check 10 — Each critical question maps to a category

**Criterion:** Every Critical Question in the markdown MUST have its category header (e.g., `### Q-C01: Definition`, `### Q-C02: Acceptance criterion`). Allowed categories: `Definition`, `Quantification`, `Obligation level`, `Acceptance criterion`, `Evaluation`, `Conflict`, or similar one-word/two-word tags.

**Pass:** Every question has a recognizable category in its heading.

**Fail:** Critical questions without categories (raw question text only).

**Evidence to cite:** Question IDs without categories.

---

### Check 11 — Anti-truncation / anti-row-cap / encoding integrity

**Criterion:** Neither file contains `_Showing N of M_` notices, mid-word `[:N]` truncations, or mojibake (`â€"`, etc.).

**Pass:** Zero matches in both files.

**Fail:** Any match.

**Evidence to cite:** File + line + offending content.

---

## Disposition Logic

- **PASS:** Checks 1, 2, 3, 5, 7, 8, 9, 10, 11 pass AND Check 4 not in advisory band AND Check 6 not in advisory band.
- **CONCERN:** Check 4 == "unknown" OR Check 6 has 1 unsupported critical. Log + continue.
- **FAIL:** Any of Checks 1, 2, 3, 5, 7, 8, 9, 10, 11 fail OR Check 4 contradicts SUBMISSION OR Check 6 has > 1 unsupported critical.

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase1.85-questions-win with the following targeted corrections:

[Check 1 fail] CLARIFYING_QUESTIONS.md missing or under-size.
  Action: Re-run from Step 1. Verify all 4 input artifacts (COMPLIANCE_MATRIX,
  SUBMISSION_STRUCTURE, EVALUATION_CRITERIA, domain-context) exist first.

[Check 2 fail] clarifying-questions-summary.json missing or invalid.
  Action: Audit Step 6. Ensure write_json is called with all 5 required keys.

[Check 3 fail] totals arithmetic broken.
  totals={value}, sum={computed}. Action: Re-run Step 4-5. The by_sev dict groups
  candidates; verify the iteration is exhaustive and severity values match the
  3-tier closed set.

[Check 4 fail] deadline_status missing or contradicts SUBMISSION.
  Summary: {x}, SUBMISSION: {y}. Action: Re-read SUBMISSION_STRUCTURE.json
  questions_deadline field in Step 2 — DO NOT override or re-interpret.

[Check 5 fail] mode doesn't match deadline_status.
  Action: Re-run Step 2 mode resolution logic. The if/elif chain is closed; ensure
  no early-exit bypasses it.

[Check 6 fail] Critical questions lack source/evidence.
  Failing IDs: {list}. Action: In Step 3 pattern scans, every candidate must populate
  EITHER evidence_snippet OR source_section. Patterns that produce neither (e.g.,
  Pattern E for unweighted factors) should default source_section to a known artifact.

[Check 7 fail] Severity tier headers missing.
  Action: Re-run Step 5 markdown generation. The `for sev in ("Critical", "Important",
  "Nice-to-have")` loop emits headers only when bucket is non-empty — verify the totals
  count agrees.

[Check 8 fail] Competitive position leakage.
  Offending phrases: {list}. Action: Add a post-generation sanity grep in Step 5 that
  rejects any draft_question containing leakage tokens. Re-run with the filter.

[Check 9 fail] phase1_9_input fields missing.
  Action: Ensure Step 6 write_json includes totals + deadline_status at minimum.

[Check 10 fail] Critical questions lack categories.
  Action: Re-run Step 3. Every candidate dict has a "category" field — ensure Step 5
  emits it in the heading line `### Q-{sev[0]}{i:02d}: {c['category']}`.

[Check 11 fail] Row-cap, truncation, or mojibake.
  Action: Ensure encoding='utf-8' on every write, ensure_ascii=False on json.dump.

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input:**

```
CLARIFYING_QUESTIONS.md scenario: 1,800 bytes (under-size), 8 critical questions.
  3 critical questions are raw text without category headers.
  2 critical questions lack both source and evidence snippet.
  Question Q-C04 contains phrase "Our win theme is reliability" (leakage).
  Question Q-C06 mentions "vs the incumbent ACME Corp" (competitive leakage).
  No `## Important Questions` header despite totals.important = 12.

summary.json scenario: totals = {critical: 8, important: 12, nice_to_have: 5, all: 24}.
  Arithmetic: 8 + 12 + 5 = 25 != 24 (mismatch).
  deadline_status absent.
  mode = "open" (default) but SUBMISSION.questions_deadline = "closed".
```

Verifier MUST detect: Check 1 (under-size), Check 3 (sum mismatch), Check 4 (absent + contradicts SUBMISSION), Check 5 (mode wrong), Check 6 (2 unsupported critical), Check 7 (Important header missing), Check 8 (2 leakage phrases), Check 10 (3 uncategorized). Disposition: FAIL.

**Known-good input:**

```
CLARIFYING_QUESTIONS.md: 4,200 bytes; opens with deadline line.
  Critical (5), Important (8), Nice-to-have (3) — all with categories + source/evidence.
  No leakage phrases anywhere outside "Submission Guidance".

summary.json: totals = {critical: 5, important: 8, nice_to_have: 3, all: 16} (5+8+3=16).
  deadline_status = "closing_soon" matching SUBMISSION value.
  mode = "triage_critical" (correctly derived).
  generated_at, deadline_date present.
```

Verifier MUST PASS all checks. Disposition: PASS.

## Hard Rules

1. NEVER claim CLARIFYING_QUESTIONS.md is missing without `Glob` verification first.
2. NEVER accept a deadline_status that contradicts SUBMISSION_STRUCTURE — Phase 1.85 reads from Phase 1.8 only; drift here is a phase logic bug.
3. NEVER ignore competitive-position leakage — questions are submitted to the procuring agency; bidder strategy in a question is a hard reject by procurement.
4. Every finding must cite a specific line number (markdown) or field path (JSON) + offending content snippet.
5. On FAIL, return corrective instructions targeting the specific Step number (Step 2 mode, Step 3 patterns, Step 5 markdown gen, Step 6 summary write) for surgical repair.
