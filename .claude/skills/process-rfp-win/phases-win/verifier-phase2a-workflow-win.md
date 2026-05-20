---
name: verifier-phase2a-workflow-win
expert-role: Workflow Extraction Verifier
purpose: phase-boundary verifier for phase2a-workflow-win (schema, workflow counts, actor/step integrity, source grounding)
created: 2026-05-20
---

# Verifier — Phase 2a Workflow Extraction

## When this runs

After phase2a-workflow-win reports done, BEFORE Phase 2 (requirements extraction) consumes `workflow-extracted-reqs.json`. A failure here cascades to phase2 (workflow merge), phase2d (coverage gate), and phase4 (RTM workflow links).

## Inputs (read in this order)

1. `{folder}/shared/workflow-extracted-reqs.json` — primary output under verification
2. `{folder}/shared/domain-context.json` — confirms the upstream Phase 1 input existed
3. `{folder}/flattened/*.md` — source documents to spot-check workflow grounding against
4. Any prior verifier report at `{folder}/shared/validation/verifier-phase2a.json` (if a retry run)

## Verification Checks

### Check 1 — File exists and is valid JSON, schema-structurally valid

**Criterion:** `shared/workflow-extracted-reqs.json` exists AND parses as valid JSON AND contains top-level keys: `extracted_at`, `summary`, `workflows`, `requirement_candidates`, `actor_data_flows`, `category_distribution`. The `summary` block must contain `total_workflows`, `table_workflows`, `narrative_workflows`, `requirement_candidates`, `unique_actors`, `unique_data_elements`.

**Pass:** All keys present, JSON parses without error, file size > 100 bytes.

**Fail:** File absent, JSON parse error, or any top-level / summary key missing.

**Evidence to cite:** Specific missing key path (e.g., `summary.unique_actors` absent), file size in bytes.

**Hard-rule reminder:** NEVER claim file is missing without `Glob` verification first.

---

### Check 2 — At least one workflow extracted

**Criterion:** `len(workflows) >= 1` AND `summary.total_workflows >= 1`. If zero workflows are extracted, downstream phase2d coverage gate vacuously passes and provides no defense against a misclassified input doc — surface this as a structural concern.

**Pass:** `total_workflows >= 1` AND at least one workflow has step content.

**Concern:** `total_workflows == 0` but `requirement_candidates == 0` consistently — log advisory: input documents may not be an RFP (cover letter, scope summary, etc.). Do not block, but propagate the warning to phase2d zero-input detector.

**Fail:** `workflows` array is missing, malformed, or `total_workflows` field present but disagrees with `len(workflows)`.

**Evidence to cite:** `summary.total_workflows = N`, `summary.table_workflows = X`, `summary.narrative_workflows = Y`, `len(workflows) = Z`.

---

### Check 3 — Every workflow has steps and the step count agrees with summary

**Criterion:** For each entry in `workflows`, the `step_count` field is an integer > 0. Sum of `step_count` across all workflows should reasonably explain `summary.requirement_candidates` (each step typically produces 0-1 candidates after the 20-char length filter in Step 5).

**Pass:** All workflows have `step_count >= 1`; aggregate step count is within 2x of `requirement_candidates` count.

**Concern:** Some workflows have `step_count == 0`. Log advisory.

**Fail:** Any workflow has missing `step_count` field OR aggregate step count is 5x larger than `requirement_candidates` (indicates Step 5 silently dropped most steps via the length filter or category filter).

**Evidence to cite:** List up to 5 workflow indices with `step_count == 0`. Print `sum(step_count) vs requirement_candidates`.

---

### Check 4 — Every requirement_candidate has actors AND category populated

**Criterion:** For each entry in `requirement_candidates`, `actors` is a list (may be empty if narrative extraction found none — but the key MUST be present) AND `category` is a non-empty string from the set {DATA_ENTRY, VALIDATION, PROCESSING, REPORTING, APPROVAL, NOTIFICATION, INTEGRATION, GENERAL}.

**Pass:** All candidates have `actors` key and a valid `category` value.

**Fail:** Any candidate missing `actors` key OR `category` is null/missing/empty string.

**Evidence to cite:** Count candidates with missing/null `category` (must be 0). List up to 5 candidate `id` values violating the rule.

---

### Check 5 — No orphan workflow_ids (forward-traceability check)

**Criterion:** Every `requirement_candidates[i].id` (e.g., `WF001`) is a unique, well-formed identifier matching the pattern `WF\d{3,}`. No duplicate IDs. The IDs that Phase 2 (`phase2-extract-win`) later references via `workflow_id` field on merged requirements must all originate here.

**Pass:** All candidate IDs match `WF\d{3,}`, all unique.

**Fail:** Duplicate IDs detected OR malformed IDs (e.g., empty string, wrong prefix). This breaks RTM workflow-to-requirement linkage in Phase 4.

**Evidence to cite:** Print first 3 candidate IDs as sample. Count duplicates. List any malformed IDs.

---

### Check 6 — Workflow content is grep-verifiable against flattened sources

**Criterion:** Sample 3 random `requirement_candidates`. For each, the `description` (or significant fragment thereof, ≥20 chars) must be locatable in at least one file under `{folder}/flattened/*.md` via case-insensitive substring match. This catches hallucinated content or content from a stale prior run.

**Pass:** All 3 sampled candidate descriptions are found in at least one flattened file.

**Concern:** 1 of 3 not found — may be a soft-wrap or whitespace artifact. Log advisory.

**Fail:** 2+ of 3 sampled descriptions not found in any flattened file. Phase 2a is emitting content not grounded in the source corpus.

**Evidence to cite:** Sample candidate IDs, the ≥20-char fragment used for lookup, and the source file where each was found (or "NOT FOUND").

---

### Check 7 — UTF-8 round-trip and no mojibake

**Criterion:** Open `workflow-extracted-reqs.json` with `encoding='utf-8'`, scan all string values for mojibake sentinels: `�` (replacement char), the byte sequence `Ã©` (UTF-8-decoded-as-Latin1 of é), the byte sequence `â€™` (UTF-8 right single quote misread). Also scan for em-dash mojibake `â€"`.

**Pass:** No mojibake sentinels found.

**Fail:** Any mojibake sentinel found in any string value. Phase emitted text with broken encoding.

**Evidence to cite:** First 3 string values containing the sentinel, with field path (e.g., `workflows[0].steps[3].description`).

---

### Check 8 — No `[:N]` truncation in deliverable strings

**Criterion:** No `description`, `step` text, `actor` name, or `data_element` field in the output is a deterministic prefix slice of the source. Specifically: no field ending mid-word in a hyphen, ending in subordinator words (`the`, `a`, `of`, `for`, `with`, etc.), or ending in a comma without a sentence terminator.

Note: Step 5 of phase2a-workflow-win has a defensible `[:500]` cap on `description` — this is acceptable for table-row workflow items that may include very long narrative text. Flag only if a 500-char cap clearly chopped mid-sentence content.

**Pass:** No mid-word truncation patterns detected in description/step fields.

**Concern:** Some descriptions are exactly 500 chars and end mid-sentence — log advisory for upstream Step 5 cap review.

**Fail:** Any description/step text ends with a hyphen, comma, or subordinator word AND is shorter than 500 chars (indicates extraction regex truncated mid-content, not the safety cap).

**Evidence to cite:** First 3 offending field paths with the suspect text.

---

### Check 9 — No `_Showing N of M_` row-cap notices

**Criterion:** This phase emits only JSON, but if any embedded markdown fragment (e.g., in a summary table that the agent later writes) contains the row-cap notice pattern `_Showing \d+ of \d+_`, flag it. This is the universal anti-regression check from the SAFS memory.

**Pass:** No row-cap notices in any string value.

**Fail:** Any row-cap notice found.

**Evidence to cite:** Field path and matched substring.

---

## Disposition Logic

- **PASS:** Checks 1, 2, 3, 4, 5 pass AND Checks 7, 8, 9 pass (no encoding/truncation regressions).
- **CONCERN:** Check 2 reports zero-input advisory OR Check 3/6/8 fall in the advisory band. Continue but log warning.
- **FAIL:** Any of Checks 1, 4, 5, 7, 9 fail OR Checks 3, 6, 8 fall below their FAIL thresholds.

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase2a-workflow-win with the following targeted corrections:

[Check 1 fail] workflow-extracted-reqs.json missing or structurally invalid.
  Action: Verify Step 7 of phase2a executed. Ensure write_json() was called with the
  required top-level keys (extracted_at, summary, workflows, requirement_candidates,
  actor_data_flows, category_distribution). Do NOT skip the summary subkeys.

[Check 2 fail] workflows array empty or malformed.
  Action: Re-audit Step 2 (find_workflow_sections) and Steps 3-4 (table + narrative
  extraction). Confirm the WORKFLOW_PATTERNS regex set matched at least one section
  in combined_content. If 0 matches: input documents may not contain workflow language —
  verify the input is an RFP body, not a cover letter or scope summary.

[Check 3 fail] step_count missing or aggregate count >> requirement_candidates.
  Action: Audit Step 5 (generate_requirements) — the `if len(description) > 20` filter
  may be dropping too many steps. Lower threshold to 15 if table-extracted steps are
  consistently short. Also verify Step 7 builds the summary `workflows[]` block with
  correct step_count from len(w.get("steps", [])).

[Check 4 fail] requirement_candidates missing actors or category.
  Action: Verify Step 5 sets both keys unconditionally. The `step.get("actors", [])`
  default must yield [] not None. infer_category() must always return a string.

[Check 5 fail] Duplicate or malformed candidate IDs.
  Action: Audit Step 5 — req_id counter must increment monotonically across BOTH
  table and narrative workflows. If two workflow types both start at WF001, you have
  a shared-counter bug. Confirm req_id = 1 is initialized once before the outer loop.

[Check 6 fail] Sampled descriptions not found in flattened source.
  Action: Confirm Step 1 read from {folder}/flattened/*.md (not a cached/stale dir).
  Verify combined_content was rebuilt for this run. Suspect a stale run if descriptions
  reference content from a different RFP.

[Check 7 fail] Mojibake in JSON output.
  Action: Audit every open() call in phase2a Python code — must use encoding='utf-8'.
  Audit json.dump() — must use ensure_ascii=False. The mojibake came from somewhere
  in the I/O chain; trace via the field path in Evidence.

[Check 8 fail] [:N] truncation in descriptions.
  Action: Step 5 has `description[:500]` — confirm this is the only truncation point
  AND it does NOT trigger for descriptions naturally <500 chars. Investigate whether
  a regex group is capturing only a prefix of the actual step text.

[Check 9 fail] Row-cap notice found.
  Action: Universal anti-regression — search emitted code for "_Showing" and remove.
  Reference SAFS memory: feedback_screen_encoding_truncation.md.

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input:**

```
workflow-extracted-reqs.json scenario:
  summary.total_workflows = 3
  workflows[].step_count = [12, 8, 5]  (aggregate 25)
  requirement_candidates count = 2  (95% dropped)
  requirement_candidates[0].id = "WF001", [1].id = "WF001"  (duplicate)
  requirement_candidates[0].category = null
  workflows[0].steps[2].description = "the system shall generate secure, tamper-"
```

Verifier MUST detect: Check 3 (25 steps -> 2 candidates is 12x ratio), Check 4 (null category), Check 5 (duplicate WF001), Check 8 (description ends mid-word in hyphen). Disposition: FAIL.

**Known-good input:**

```
workflow-extracted-reqs.json scenario:
  summary.total_workflows = 4 (2 table + 2 narrative)
  workflows[].step_count = [15, 10, 8, 12]  (aggregate 45)
  requirement_candidates count = 38  (light filtering for sub-20-char steps)
  All candidate IDs are unique WF001..WF038
  All candidates have actors[] (may be empty list) and a non-null category
  Sampled 3 descriptions: all found in flattened/RFP-body.md or flattened/Att-H.md
  No mojibake; no [:N] mid-word truncation; no _Showing N of M_ patterns.
```

Verifier MUST PASS all checks. Disposition: PASS.

## Hard Rules (inherited from verifier-design principles)

1. NEVER claim workflow-extracted-reqs.json is missing without `Glob` verification first.
2. NEVER flag zero-input as FAIL — it is a CONCERN advisory that must propagate to phase2d, not a phase2a halt condition.
3. NEVER report a candidate as malformed without citing its `id` and the specific field that failed validation.
4. Every finding must cite a specific JSON field path + value or count (e.g., `requirement_candidates[7].category = null`).
5. On FAIL, return corrective instructions with the specific numeric discrepancy so the phase agent can target the exact repair without re-running everything.
6. Source-grounding spot-check (Check 6) is the most important defense against stale/cached/hallucinated output — never skip it.
