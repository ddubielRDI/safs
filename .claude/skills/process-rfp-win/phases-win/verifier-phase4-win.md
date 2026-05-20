---
name: verifier-phase4-win
expert-role: RTM Integrity Verifier
purpose: phase-boundary verifier for phase4-traceability-win (schema, entity counts, coverage thresholds, chain integrity)
created: 2026-05-19
---

# Verifier — Phase 4 Traceability (UNIFIED_RTM)

## When this runs

After phase4-traceability-win reports done, BEFORE SVA-4 (Red Team gate).

## Inputs (read in this order)

1. `{folder}/shared/UNIFIED_RTM.json` — primary output under verification
2. `{folder}/shared/REQUIREMENT_RISKS.json` — cross-check: risk count parity
3. `{folder}/shared/requirements-normalized.json` — source requirement count for coverage math
4. `{folder}/shared/COMPLIANCE_MATRIX.json` — mandatory item count for waiver completeness check
5. Any prior SVA-4 report at `{folder}/shared/validation/sva4-red-team.json` (if a retry run)

## Verification Checks

### Check 1 — File exists and is valid JSON, schema-structurally valid

**Criterion:** `shared/UNIFIED_RTM.json` exists AND parses as valid JSON AND contains top-level keys: `meta`, `entities`, `chain_links`, `verification`. Under `entities`: keys `rfp_sources`, `mandatory_items`, `requirements`, `specifications`, `risks`, `bid_sections`, `evidence`, `evaluation_criteria` all present (may be empty arrays for bid_sections/evidence stubs at Phase 4 time).

**Pass:** All keys present, JSON parses without error.

**Fail:** File absent, JSON parse error, or any top-level / entities key missing.

**Evidence to cite:** Specific missing key path (e.g., `entities.evaluation_criteria` absent).

**Hard-rule reminder:** NEVER claim file is missing without `Glob` verification first.

---

### Check 2 — Requirement count matches normalized source

**Criterion:** `len(UNIFIED_RTM["entities"]["requirements"])` equals `len(requirements-normalized["requirements"])`. Tolerance: 0 (exact match — every requirement from normalization must appear in the RTM).

**Pass:** Counts match exactly.

**Fail:** RTM requirements count < normalized count (requirements were silently dropped during RTM build). RTM count > normalized count is also a FAIL (phantom requirements injected).

**Evidence to cite:** `UNIFIED_RTM.entities.requirements count = N` vs `requirements-normalized.requirements count = M`. List up to 5 `req_id` values present in normalized but absent from RTM.

---

### Check 3 — Forward spec coverage ≥ 95%

**Criterion:** `verification.forward_coverage.spec_coverage_pct >= 95.0`. This is the percentage of requirements that have at least one specification section link.

**Pass:** `spec_coverage_pct >= 95.0`.

**Concern:** `spec_coverage_pct` between 90.0 and 94.9 — log as advisory, do not block.

**Fail:** `spec_coverage_pct < 90.0` — too many requirements are floating without spec anchors; downstream bid authoring will produce ungrounded content.

**Evidence to cite:** `verification.forward_coverage.spec_coverage_pct = N%`, `requirements_with_specs = X / requirements_total = Y`.

---

### Check 4 — Backward spec coverage ≥ 90%

**Criterion:** For each specification section in `entities.specifications`, it must have `linked_requirement_ids` that is non-empty (at least one requirement links back to it), OR the spec section's `spec_id` matches a pattern in an `intentionally_unlinked` annotation. Compute: `(specs_with_at_least_one_req / total_specs) * 100 >= 90.0`.

**Pass:** ≥ 90% of spec sections have at least one requirement linkage.

**Concern:** 85–89.9%. Log advisory.

**Fail:** < 85%. Orphaned spec sections indicate the spec inventory was built in isolation from requirements.

**Evidence to cite:** List up to 10 `spec_id` values with empty `linked_requirement_ids`.

---

### Check 5 — Every WAIVED mandatory item has a non-empty unlinked_reason

**Criterion:** For each entry in `entities.mandatory_items` where `coverage_status` is NOT `"ADDRESSED"` (i.e., it is `"GAP"`, `"PLANNED"`, or `"WAIVED"`), the field `unlinked_reason` must be a non-empty string (not null, not `""`).

**Pass:** All non-ADDRESSED mandatory items have documented reasons.

**Fail:** Any mandatory item with status other than ADDRESSED has null or empty `unlinked_reason`. This creates a compliance gap that evaluators and SVA-7 will flag.

**Evidence to cite:** `entities.mandatory_items[N].mandatory_id`, `status`, `unlinked_reason` value (or null).

---

### Check 6 — Risk entity count matches REQUIREMENT_RISKS source

**Criterion:** `len(UNIFIED_RTM["entities"]["risks"])` should equal `len(REQUIREMENT_RISKS["rtm_risks"])` (or `len(REQUIREMENT_RISKS["risks"])` — check both keys as the source schema allows either). Tolerance: ±5% to account for deduplication, but zero risks dropped is the target.

**Pass:** Counts match within ±5%.

**Concern:** Difference > 5% but < 15%. Log discrepancy with counts — risks may have been deduplicated or merged.

**Fail:** Difference > 15%. Risks were silently dropped during RTM consolidation; SVA-7 traceability audit will fail.

**Evidence to cite:** `UNIFIED_RTM.entities.risks count = N` vs `REQUIREMENT_RISKS source count = M`.

---

### Check 7 — Chain link integrity: ≥ 80% of chains have a non-null specification link

**Criterion:** For each entry in `chain_links`, `specifications` array must be non-empty (at least one spec_id). Compute: `(chains_with_spec / total_chains) * 100 >= 80.0`.

**Pass:** ≥ 80% of chains have at least one spec link.

**Concern:** 70–79.9%. Log as advisory.

**Fail:** < 70%. Too many requirements have no spec path — the traceability chain is broken at the spec link and bid authoring will produce content with no spec grounding.

**Evidence to cite:** `chain_links[N].chain_id` with `specifications = []` — list first 10 broken chains.

---

### Check 8 — Integrity hash present and non-trivial

**Criterion:** `meta.integrity_hash` is a non-empty string of length ≥ 32 characters (SHA-256 hex = 64 chars). This confirms the phase computed the hash over actual requirement text, not an empty or stub blob.

**Pass:** `integrity_hash` present and len >= 32.

**Fail:** Missing, empty, or < 32 chars (indicates hash was skipped or computed over empty content).

**Evidence to cite:** `meta.integrity_hash` value (first 16 chars for brevity).

---

## Disposition Logic

- **PASS:** Checks 1, 2, 5, 6, 8 pass AND Checks 3, 4, 7 meet threshold (not just CONCERN band).
- **CONCERN:** Any of Checks 3, 4, 7 fall in the advisory band. Log + continue to SVA-4.
- **FAIL:** Any of Checks 1, 2, 5, 6, 8 fail OR Checks 3, 4, 7 fall below their FAIL thresholds.

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase4-traceability with the following targeted corrections:

[Check 1 fail] UNIFIED_RTM.json missing or structurally invalid.
  Action: Re-run the full RTM build from Step 1. Verify all upstream files are present first.

[Check 2 fail] RTM requirements count {N} does not match normalized count {M}.
  Missing req_ids: {list}. Action: Audit Steps 4-5 of phase4 — ensure all_reqs is not filtered or sliced.

[Check 3 fail] Forward spec coverage {N}% < 90%.
  Action: Review SPEC_LINK_THRESHOLD in Step 5 — may need lowering. Also check fallback section
  in Step 5 is activating for unlinked requirements.

[Check 4 fail] Backward spec coverage {N}% < 85% — {count} orphan spec sections.
  Action: Review extract_spec_sections() — likely parsed headings from sections with no req keywords.
  Consider expanding CATEGORY_SPEC_AFFINITY or lowering SPEC_LINK_THRESHOLD.

[Check 5 fail] Mandatory items with no unlinked_reason: {list of mandatory_ids}.
  Action: For each GAP/PLANNED/WAIVED item, populate unlinked_reason with the specific
  compliance disposition (e.g., "Addressed by Section 3.4 of ARCHITECTURE.md per RFP §5.2.1").

[Check 6 fail] Risk count mismatch — RTM {N} vs source {M} (>{threshold}% deviation).
  Action: Audit Step 7 (consolidate risks) — check the filter that removes risks with no valid req_ids.
  Risks must be retained even if their linked_requirement_ids list becomes empty after filtering.

[Check 7 fail] Chain spec-link coverage {N}% < 70%.
  Action: Re-run Step 5 with a lower SPEC_LINK_THRESHOLD (try 2 instead of 3) and verify
  the fallback ARCHITECTURE section is linked to all otherwise-unlinked requirements.

[Check 8 fail] Integrity hash missing or degenerate.
  Action: Ensure Steps 11 encodes the full req_text_blob (verify no [:100] slice survives from old code).

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input:**

```
UNIFIED_RTM.json scenario: 2,100 requirements in RTM but requirements-normalized has 2,167.
  entities.risks has 40 entries, REQUIREMENT_RISKS has 281.
  verification.forward_coverage.spec_coverage_pct = 72.0
  meta.integrity_hash = ""
```

Verifier MUST detect: Check 2 (67 requirements dropped), Check 3 (72% < 90%), Check 6 (40 vs 281 is 86% deviation), Check 8 (empty hash). Disposition: FAIL.

**Known-good input:**

```
UNIFIED_RTM.json scenario: 2,167 requirements matching normalized.
  entities.risks has 277 entries vs 281 source (1.4% deviation — within ±5%).
  verification.forward_coverage.spec_coverage_pct = 96.8
  backward coverage: 143/150 spec sections have links (95.3%)
  chains with spec link: 2,089/2,167 = 96.4%
  meta.integrity_hash = "a3f4b2..." (64 chars)
  All mandatory items with non-ADDRESSED status have unlinked_reason populated.
```

Verifier MUST PASS all checks. Disposition: PASS.

## Hard Rules (inherited from verifier-design principles)

1. NEVER claim UNIFIED_RTM.json is missing without `Glob` verification first.
2. NEVER report a `unlinked_reason` as "missing" if the `coverage_status` is `"ADDRESSED"` — the field is only required for non-addressed items.
3. NEVER flag a spec section as an orphan if it has `linked_requirement_ids` populated — verify the actual array content, not just key presence.
4. Every finding must cite a specific field path + value or count (e.g., `entities.requirements count = 2,100 vs normalized count = 2,167`).
5. On FAIL, return corrective instructions with the specific numeric discrepancy so the phase agent can target the exact repair without re-running everything.
