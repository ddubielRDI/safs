---
name: verifier-phase8.6-integration-win
expert-role: Integration Plan Quality Verifier
purpose: phase-boundary verifier for phase8.6-integration-win (multi-vendor approach, interop tie-back, SLAs/handoffs, no fabricated endpoints, no truncation)
created: 2026-05-20
---

# Verifier — Phase 8.6 Technical Integration Plan

## When this runs

After phase8.6-integration-win reports done, BEFORE Phase 8f (RTM Verification) which expects the integration plan to address integration-flagged requirements.

## Inputs (read in this order)

1. `{folder}/outputs/bid-sections/06_INTEGRATION.md` — primary output under verification
2. `{folder}/outputs/INTEROPERABILITY.md` (Phase 3b) — for interop fact source (endpoints, protocols, data formats)
3. `{folder}/outputs/ARCHITECTURE.md` — for integration architecture cross-reference
4. `{folder}/shared/requirements-normalized.json` — for integration-related requirement subset
5. `{folder}/shared/REQUIREMENT_RISKS.json` — for integration risk table source
6. `{folder}/shared/domain-context.json` — for known client environment / vendor list

## Verification Checks

### Check 1 — File exists and meets minimum size

**Criterion:** `outputs/bid-sections/06_INTEGRATION.md` exists AND file size >= 5,120 bytes (5 KB minimum — phase target is 8 KB; 5 KB is the verifier floor below which sections are clearly missing).

**Pass:** File exists, size >= 5,120 bytes.

**Fail:** File absent OR size < 5,120 bytes.

**Evidence to cite:** File path + actual size in bytes.

**Hard-rule reminder:** NEVER claim file is missing without `Glob` verification first.

---

### Check 2 — All 8 major sections present

**Criterion:** Grep `^## ` returns >= 8 hits. Expected: 1. Integration Overview, 2. External System Integrations, 3. Data Migration Plan, 4. API Management, 5. Multi-Vendor Coordination, 6. Integration Testing, 7. Integration Risks and Mitigations, 8. Ongoing Integration Support.

**Pass:** All 8 major sections present.

**Concern:** 6-7 sections (one or two merged).

**Fail:** < 6 sections.

**Evidence to cite:** Grep result; list headings.

---

### Check 3 — Multi-vendor integration approach addressed (Section 5)

**Criterion:** Section 5 (Multi-Vendor Coordination) must contain:
(a) Vendor Touchpoints subsection describing coordination approach
(b) Responsibility Matrix table with columns: Integration, Our Responsibility, Client Responsibility, Vendor Responsibility (or similar handoff structure)

**Pass:** Section 5 present with both subsections; responsibility matrix has >= 3 rows.

**Concern:** Section 5 present but responsibility matrix has < 3 rows.

**Fail:** Section 5 absent OR responsibility matrix absent.

**Evidence to cite:** Section 5 content excerpt; responsibility matrix row count.

---

### Check 4 — Interoperability tied back to Phase 3b INTEROPERABILITY.md

**Criterion:** Section 1 (Integration Overview) and Section 2 (External System Integrations) must derive content from `outputs/INTEROPERABILITY.md`. Verify by:
(a) Loading INTEROPERABILITY.md and extracting external system names / protocol mentions.
(b) Verifying that at least 50% of those system names appear in 06_INTEGRATION.md.

**Pass:** >= 50% overlap between INTEROPERABILITY.md systems and 06_INTEGRATION.md content.

**Concern:** 30-49% overlap.

**Fail:** < 30% overlap (integration plan ignored interop specs).

**Evidence to cite:** List external systems in INTEROPERABILITY.md; report which appear in 06_INTEGRATION.

---

### Check 5 — SLAs and handoffs defined for external integrations

**Criterion:** Section 2 (External System Integrations) per-system blocks must include SLA references: latency targets, availability requirements, error-handling policies. For each external system, at least one SLA element must be specified.

**Pass:** Every external system block has >= 1 SLA element.

**Concern:** 1-2 system blocks missing SLA details.

**Fail:** 3+ system blocks lack any SLA details.

**Evidence to cite:** Per system: SLA element found or "no SLA reference".

---

### Check 6 — No fabricated 3rd-party endpoints

**Criterion:** Scan Section 2 content for endpoint references (e.g., `https://...`, `api.X.com/v1/...`, IP addresses, port numbers). For each, verify:
(a) The endpoint is described as illustrative/sample (e.g., "endpoint format: https://[client-vendor]/api/v1/...") OR
(b) The endpoint exists in INTEROPERABILITY.md or domain-context.json's known systems list

Fabricated endpoints (specific URLs not from source files) are a credibility failure — evaluators can verify these.

**Pass:** All endpoints are either explicitly illustrative OR traceable to source files.

**Fail:** Any concrete endpoint (with `.com`/`.gov`/`.org` domain) that is NOT illustrative and NOT in source files.

**Evidence to cite:** Per fabricated endpoint: line + URL + check against INTEROPERABILITY.md.

---

### Check 7 — Integration Risks table has populated mitigations (no empty cells, no truncations)

**Criterion:** Section 7 (Integration Risks and Mitigations) must contain a table with columns: Risk, Severity, Mitigation. Per phase ⛔ NO-TRUNCATION DISCIPLINE:
(a) Render ALL applicable integration risks (no `integration_risks[:6]` cap).
(b) Render FULL descriptions and mitigations (no `desc[:220]` or `mit_text[:200]` cuts).
(c) Mitigation cells populated from BOTH `mitigation_strategies` array AND `mitigation_strategy` singular field.
(d) Zero empty `|  |` cells in Mitigation column.

Cross-check: count `integration_risks` from REQUIREMENT_RISKS.json (filtered by integration keywords). Row count in 06_INTEGRATION risk table should match this count (±5%).

**Pass:** All criteria met.

**Concern:** Row count off by 5-15% from source.

**Fail:** Row count > 15% under source OR any empty mitigation OR any mid-word truncation.

**Evidence to cite:** Row count vs source count; line + content for empty cells or truncations.

---

### Check 8 — All integration requirements addressed

**Criterion:** Filter `requirements-normalized.json` for requirements with integration keywords (integrat*, interface, api, import, export, migrate, connect, interop, exchange, sync) OR category in (integration, interoperability, data exchange).

For a sample of 3 high-priority integration requirements, verify their content/concepts appear somewhere in 06_INTEGRATION.md (grep for req_id OR keyword match).

**Pass:** All 3 sampled integration requirements traceable to document content.

**Concern:** 1 of 3 sampled requirements absent.

**Fail:** 2 or 3 of sampled requirements absent.

**Evidence to cite:** Per requirement: req_id + sampled text + match found in document or "no match".

---

### Check 9 — Page-budget awareness (MARS 25-page cap)

**Criterion:** Estimate pages by char/3,000. Integration volume target: 2-4 pages (6,000-12,000 chars).

**Pass:** 6,000-12,000 chars (2-4 pages).

**Concern:** 5,000-6,000 (thin) OR 12,000-15,000 (heavy).

**Fail:** > 15,000 chars (> 5 pages) OR < 4,000 chars.

**Evidence to cite:** Actual char count, estimated page count.

---

### Check 10 — Universal regression patterns

**Criterion:** Five sub-checks:
(a) UTF-8 decode clean; no mojibake.
(b) Zero `_Showing \d+ of \d+_` row-cap notices.
(c) Zero `[:N]` truncation patterns in deliverable strings — specifically zero `integration_risks[:6]`, `desc[:220]`, `mit_text[:200]` artifacts per 2026-05-19 fix.
(d) Zero em-dash chars (`—` U+2014).
(e) Zero empty `|  |` cells in any table.

**Pass:** All sub-checks pass.

**Fail:** Any sub-check has 1+ violation.

**Evidence to cite:** Per violation: line + first 80 chars quoted.

---

## Disposition Logic

- **PASS:** All 10 checks pass.
- **CONCERN:** Checks 2, 3, 4, 5, 8, or 9 in advisory band; all others pass.
- **FAIL:** Any of Checks 1, 6, 7, 10 fail OR Checks 2/3/4/5/8/9 fall below FAIL threshold.

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase8.6-integration with the following targeted corrections:

[Check 1 fail] 06_INTEGRATION.md missing or < 5 KB ({actual_size} bytes).
  Action: Re-run phase. Confirm INTEROPERABILITY.md and ARCHITECTURE.md loaded
  (these are the primary content sources).

[Check 2 fail] Missing section(s): {list}.
  Action: Step 2 template specifies all 8 ## sections. Confirm the AI rendered
  each — common drop: Section 5 (Multi-Vendor Coordination) gets merged into
  Section 2 if there's only one vendor.

[Check 3 fail] Multi-vendor coordination thin/missing.
  Action: Even single-vendor scenarios require Section 5's Responsibility Matrix
  with our/client/vendor split per integration. This is a non-negotiable evaluator
  expectation in government RFPs.

[Check 4 fail] Interop tie-back gap: {N}% overlap.
  Action: Section 1.2 and Section 2 must enumerate every external system from
  INTEROPERABILITY.md. Re-parse INTEROPERABILITY.md for system names and ensure
  each appears at least once in 06_INTEGRATION.

[Check 5 fail] SLAs missing for: {list of systems}.
  Action: Section 2 per-system template requires Latency + Availability + Error
  Handling at minimum. If real SLAs aren't known, use "[USER INPUT REQUIRED:
  client to specify SLA targets]" — never silently omit.

[Check 6 fail] Fabricated endpoint(s): {list}.
  Action: Replace specific URLs with illustrative format ("https://[client-vendor]/...")
  or remove. Government evaluators verify endpoints — fabrication is a credibility
  hit.

[Check 7 fail] Integration Risks table issue: {description}.
  Action: Per phase ⛔ NO-TRUNCATION discipline:
    - Render ALL integration_risks (remove any [:6] or [:N] cap)
    - Mitigation from BOTH `mitigation_strategies` array AND `mitigation_strategy` singular
    - No `desc[:220]` or `mit_text[:200]` truncations

[Check 8 fail] Integration requirements unaddressed: {list of req_ids}.
  Action: For each unaddressed requirement, add a sentence/paragraph in Section 1
  or Section 2 that covers it. Use req_id as a comment or in a "Requirements
  Addressed" callout.

[Check 9 fail] Integration volume is {N} chars ({pages} pages).
  Action: For >15K: trim Section 6 (Testing) and Section 8 (Ongoing Support).
  For <4K: expand Section 2 per-system blocks with full SLA + protocol detail.

[Check 10 fail — TRUNCATION/REGRESSION] {pattern type} at line {N}.
  Action: This phase's 2026-05-19 regression vector is well-documented. Strip
  every [:N] slice. Remove every row cap. Confirm full strings render.

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input:**

```
06_INTEGRATION.md: 4,200 bytes.
  Sections present: Integration Overview, External Systems, Data Migration, API Management, Testing.
  Missing: Multi-Vendor Coordination, Risks/Mitigations, Ongoing Support (3 missing).
  Section 2 references 3 external systems; INTEROPERABILITY.md identifies 9 systems
    (33% overlap — fails Check 4).
  Endpoints cited: "https://api.example.com/v1/payment" — not in INTEROPERABILITY.md.
  Risk table: 4 rows shown, source has 18 (78% under — clearly capped).
  2 mitigation cells empty.
  6 of 9 integration requirements traceable to content.
```

Verifier MUST detect: Check 1 (< 5 KB), Check 2 (3 of 8 missing), Check 3 (Section 5 absent), Check 4 (33% overlap < 50%), Check 6 (fabricated endpoint), Check 7 (row cap + empty mitigations), Check 10 (truncation pattern). Disposition: FAIL.

**Known-good input:**

```
06_INTEGRATION.md: 9,400 bytes.
  All 8 major sections present.
  Section 5 (Multi-Vendor Coordination) has 4-row Responsibility Matrix.
  INTEROPERABILITY.md identifies 7 external systems; 6 of 7 (86%) appear in document.
  Section 2 per-system blocks all have Latency + Availability + Error Handling.
  No fabricated endpoints — 3 illustrative formats used, all clearly marked.
  Section 7 Integration Risks table: 18 rows (matches source 18), all mitigations
    populated from `mitigation_strategies` array OR `mitigation_strategy` singular.
  3 sampled integration requirements all traceable to document content.
  No em dashes, no row caps, no truncation, UTF-8 clean.
```

Verifier MUST PASS all checks. Disposition: PASS.

## Hard Rules (inherited from verifier-design principles)

1. NEVER claim 06_INTEGRATION.md is missing without `Glob` verification first.
2. NEVER flag a placeholder endpoint format like `https://[client-vendor]/api/v1/...` as a fabricated endpoint — the bracket placeholders make it illustrative.
3. NEVER fail Check 3 (multi-vendor) for an RFP that genuinely has zero external vendors in scope — the Responsibility Matrix can be Our / Client only (2 columns) in that case.
4. NEVER flag an empty Mitigation cell that contains `[MITIGATION TBD]` — that is the prescribed placeholder.
5. NEVER fail Check 8 when integration requirements are genuinely few (< 5 in normalized requirements) — adjust sample size accordingly.
6. Every finding must cite line number + first 80 chars of offending content (or system/endpoint names for Checks 4, 6).
