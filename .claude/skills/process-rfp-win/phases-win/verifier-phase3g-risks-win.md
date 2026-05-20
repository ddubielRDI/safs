---
name: verifier-phase3g-risks-win
expert-role: Requirement Risk Verifier
purpose: phase-boundary verifier for phase3g-risks-win (MD + JSON existence, schema fidelity, likelihood/impact/mitigation/owner completeness, requirement traceback, reasonable count)
created: 2026-05-20
---

# Verifier — Phase 3g Requirement Risk Assessment

## When this runs

After phase3g-risks-win reports done, BEFORE SVA-3 (Spec Validator gate). Risks feed Phase 4 RTM, so verifier-phase4 also performs a parity check downstream — this verifier owns the structural integrity at source.

## Inputs (read in this order)

1. `{folder}/outputs/REQUIREMENT_RISKS.md` — human-readable risk document
2. `{folder}/shared/REQUIREMENT_RISKS.json` — machine-readable risk data (consumed by Phase 4)
3. `{folder}/shared/requirements-normalized.json` — for requirement-count baseline + canonical_id traceback
4. `{folder}/shared/tech-lifecycle-evidence.json` — for structural-risk framework-version cross-check
5. `{folder}/outputs/ARCHITECTURE.md` — for ADR consistency on structural risks (e.g., .NET version)
6. Any prior SVA-3 report at `{folder}/shared/validation/sva3-spec.json` (if a retry run)

## Verification Checks

### Check 1 — Both files exist, MD ≥ 1 KB, JSON valid

**Criterion:** `outputs/REQUIREMENT_RISKS.md` exists with size ≥ 1,024 bytes AND `shared/REQUIREMENT_RISKS.json` exists with size ≥ 500 bytes AND parses as valid JSON AND contains top-level keys: `assessed_at`, `summary`, `heat_map`, `requirements`, `rtm_risks`. All UTF-8 clean (no `�` / `Ã©`).

**Pass:** Both files present, sizes met, JSON valid with all 5 top-level keys.

**Fail:** Either file missing, undersize, JSON parse error, or any top-level key missing.

**Evidence to cite:** File paths + sizes + missing key path(s).

**Hard-rule reminder:** NEVER claim file missing without `Glob` verification first.

---

### Check 2 — rtm_risks schema fidelity (every entry has required fields)

**Criterion:** For each entry in `REQUIREMENT_RISKS.json:rtm_risks`, verify all of these keys are present AND non-null: `risk_id`, `title`, `category`, `likelihood`, `impact`, `risk_level`, `linked_requirement_ids`, `mitigation_strategies`. Furthermore: `likelihood` and `impact` must be integers 1–5; `risk_level` must be one of `HIGH`, `MEDIUM`, `LOW`, `CRITICAL`; `linked_requirement_ids` must be a non-empty array; `mitigation_strategies` must be a non-empty array.

**Pass:** All entries pass the schema check.

**Fail:** Any entry has a missing/null key OR likelihood/impact out of range OR empty arrays.

**Evidence to cite:** First failing `risk_id` + the failing field name + value.

---

### Check 3 — Every mitigation has strategy + owner + timeline

**Criterion:** For each `rtm_risks[*].mitigation_strategies[*]` entry, verify: `mitigation_id` non-empty, `strategy` non-empty string (> 10 chars), `owner_role` non-empty string, `timeline` non-empty string. Empty/null mitigations are a fail. Generic placeholder values like `TBD`, `?`, `N/A` are a fail.

**Pass:** Every mitigation has all four fields non-empty and non-placeholder.

**Fail:** Any mitigation has empty/placeholder field.

**Evidence to cite:** Risk_id + mitigation_id + empty field name.

---

### Check 4 — Risks tied back to requirements (no orphan risks)

**Criterion:** For each `rtm_risks[*].linked_requirement_ids`, verify every ID in the array exists in `requirements-normalized.json:requirements[*].canonical_id`. Tolerance: 0 — every linked_requirement_id must match a real canonical_id.

**Pass:** All linked_requirement_ids resolve.

**Fail:** Any rtm_risks entry contains a linked_requirement_id that doesn't exist in the normalized requirements.

**Evidence to cite:** First failing risk_id + the unresolved req_id.

---

### Check 5 — Risk count is reasonable (typical 30–150)

**Criterion:** `len(rtm_risks)` is between 30 and 150 inclusive. Below 30 indicates risks were under-assessed; above 150 indicates over-fragmentation (each requirement should not necessarily produce its own risk — group similar risks).

**Pass:** `30 <= len(rtm_risks) <= 150`.

**Concern:** Count in 15–29 OR 151–250 — log advisory; large RFPs can legitimately exceed 150.

**Fail:** Count < 15 OR > 250 — clearly broken.

**Evidence to cite:** `len(rtm_risks)`, `requirements-normalized count`, ratio.

---

### Check 6 — Summary heat_map totals match rtm_risks counts

**Criterion:** `summary.high_risk + summary.medium_risk + summary.low_risk` equals `summary.total_assessed`. AND `summary.total_risks_tracked` equals `len(rtm_risks)`. AND `heat_map["HIGH"]["PM"] + heat_map["HIGH"]["SOFTWARE"] + heat_map["HIGH"]["DOMAIN"]` equals `summary.high_risk`.

**Pass:** All three totals reconcile.

**Fail:** Any discrepancy.

**Evidence to cite:** Each total + the computed delta.

---

### Check 7 — Structural risk framework versions match tech-lifecycle-evidence

**Criterion:** For each `rtm_risks[*]` entry whose `title` or `category` contains a framework version pattern (`.NET \d+`, `Java \d+`, `Node \d+`, `React \d+`, `Angular \d+`), verify that version appears in `tech-lifecycle-evidence.json:components[*].recommended_version`. Per phase Step 5c discipline: structural risks must NOT reference stale framework versions.

**Pass:** Every framework-version reference in risk titles matches the evidence file.

**Fail:** Any risk title cites `.NET 8` / `.NET 9` (or similar stale version) when evidence file has `.NET 10` — indicates hardcoded structural risk text bypassed the architecture refresh.

**Evidence to cite:** Risk_id + title fragment + version + absence in evidence components.

**Hard-rule reminder:** From MEMORY.md — .NET 8 LTS EOL Nov 2026, .NET 9 STS, .NET 10 LTS is the multi-year target.

---

### Check 8 — REQUIREMENT_RISKS.md row count approximately matches rtm_risks

**Criterion:** Count `###` headings (HIGH-risk detail blocks) + Medium-risk table rows + Full Risk Register table rows in REQUIREMENT_RISKS.md. The Full Risk Register total should equal `summary.total_assessed` (the requirements count, not rtm_risks). Tolerance: ±5%.

**Pass:** Counts within ±5%.

**Concern:** Within ±10% — log advisory.

**Fail:** Beyond ±10% — rendering loop dropped or duplicated risks.

**Evidence to cite:** MD row count + JSON count + delta %.

---

### Check 9 — No truncation artifacts, no row-cap notices

**Criterion:** Grep REQUIREMENT_RISKS.md for `_Showing N of M_`, empty `|  |` Mitigation cells on HIGH/CRITICAL rows, mid-word ellipsis (e.g., `complian...`), `[:N]` slice notation, AND `high_risk[:15]` / `medium_risk[:20]` / `[:80]` / `[:30]` / `[:40]` / `[:200]` patterns in any source. All counts must be 0 per the 2026-05-19 no-truncation fix.

**Pass:** Zero hits across all patterns.

**Fail:** Any hit.

**Evidence to cite:** Pattern + line number + snippet.

---

## Disposition Logic

- **PASS:** Checks 1, 2, 3, 4, 6, 7, 9 pass AND Checks 5, 8 not in FAIL band.
- **CONCERN:** Check 5 / 8 in advisory band only. Log + continue to SVA-3.
- **FAIL:** Any of Checks 1, 2, 3, 4, 6, 7, 9 fail OR Check 5 / 8 below FAIL threshold.

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase3g-risks-win with these targeted corrections:

[Check 1 fail] REQUIREMENT_RISKS.md or REQUIREMENT_RISKS.json missing/invalid.
  Action: Re-run Step 5 generate_risks_md AND Step 6 write_json. Confirm both
  output paths are written; check for silent exception in heat_map generation.

[Check 2 fail] rtm_risks schema broken at risk_id {ID}: {field} = {value}.
  Action: Re-run Step 5b — confirm assess_requirement_risk populates likelihood (1-5),
  impact (1-5), and that mitigation_strategies is built from suggested_mitigations.

[Check 3 fail] Mitigation missing strategy/owner/timeline at {risk_id}/{mitigation_id}.
  Action: Re-run Step 5b mitigation loop — confirm infer_owner_role returns a non-null
  role AND timeline default is set (e.g., "Phase-dependent"). No placeholder TBD values.

[Check 4 fail] Orphan linked_requirement_id at {risk_id}: {req_id} not in normalized.
  Action: Re-run Step 3 assess_requirement_risk — confirm req.get("canonical_id")
  is captured BEFORE filtering. Risks must be retained even if their requirement was
  later filtered out, with a clear note explaining.

[Check 5 fail] Risk count {N} below 15 or above 250 (unreasonable).
  Action: Audit Step 3 risk-level assignment — base_score thresholds may be too strict
  (too few HIGH/MEDIUM risks) or too loose (every req producing a HIGH).

[Check 6 fail] Heat map totals don't reconcile.
  high+med+low = {sum}, total_assessed = {total}, delta = {N}.
  Action: Confirm Step 4 generate_heat_map loops all requirements (no filter/slice).

[Check 7 fail] Stale framework version in structural risk: {risk_id} cites {version},
  evidence file has {evidence_version}.
  Action: Re-read tech-lifecycle-evidence.json AND ARCHITECTURE.md ADR section.
  Update Step 5c structural-risk text to match the actual chosen runtime.
  Per MEMORY.md: never hardcode framework versions in risk text.

[Check 8 fail] REQUIREMENT_RISKS.md row count diverges from rtm_risks by > 10%.
  Action: Confirm generate_risks_md's Full Risk Register loop iterates ALL requirements
  (the 2026-05-19 fix removed all [:N] slices — verify they're absent).

[Check 9 fail] Truncation artifacts: {pattern + line}.
  Action: Remove [:15], [:20], [:80], [:30], [:40], [:200] slicing per the 2026-05-19
  no-truncation fix in this phase. Pipelines emit full data.

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input:**

```
REQUIREMENT_RISKS.json has 12 rtm_risks (below 30 threshold).
risks[3].mitigation_strategies = [] (empty).
risks[7].linked_requirement_ids = ["REQ-9999"] (not in requirements-normalized).
risks[2].title = "Runtime risk: .NET 8 EOL Nov 2026" — evidence file has .NET 10 only.
Heat map totals: HIGH 5 + MEDIUM 4 + LOW 0 = 9, but total_assessed = 47 (divergence).
REQUIREMENT_RISKS.md uses high_risk[:15] cap.
```

Verifier MUST detect: Check 2 (empty mitigation_strategies at risks[3]), Check 4 (orphan REQ-9999), Check 5 (count = 12 < 15), Check 6 (heat map total 9 ≠ 47), Check 7 (.NET 8 stale), Check 9 ([:15] slice). Disposition: FAIL.

**Known-good input:**

```
REQUIREMENT_RISKS.json has 87 rtm_risks (within 30-150 band).
Every risk has likelihood ∈ [1,5], impact ∈ [1,5], non-empty mitigation_strategies.
Every linked_requirement_id resolves to a real canonical_id.
Structural risks reference .NET 10 LTS — matches evidence file.
heat_map totals: HIGH 18 + MEDIUM 41 + LOW 28 = 87 ✓
summary.high_risk = 18 ✓
REQUIREMENT_RISKS.md row counts match within ±2%.
No [:N] truncation.
```

Verifier MUST PASS all checks. Disposition: PASS.

## Hard Rules (inherited from verifier-design principles)

1. NEVER claim REQUIREMENT_RISKS.md or .json is missing without `Glob` verification first.
2. NEVER flag a mitigation as "placeholder" if the value is a complete sentence > 10 chars even if it sounds boilerplate — only flag literal `TBD`/`?`/`N/A`/empty (Check 3 anti-false-positive).
3. NEVER report Check 5 as FAIL for a small-scope RFP that legitimately has < 30 risks — verify the requirements-normalized count first; if < 100 requirements, a risk count of 15–29 is acceptable as CONCERN not FAIL.
4. Every finding must cite specific risk_id + field path + value + source (e.g., `rtm_risks[14].linked_requirement_ids[0] = "REQ-9999"` + `requirements-normalized.json:requirements has no canonical_id "REQ-9999"`).
5. On FAIL, return corrective instructions naming the specific risk_id / field / version so the phase agent can target the repair surgically without re-running everything.
