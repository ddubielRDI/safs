---
name: verifier-phase3b-interop-win
expert-role: Interoperability Specification Verifier
purpose: phase-boundary verifier for phase3b-interop-win (file size, protocol fidelity, source_id traceback, no fabricated endpoints)
created: 2026-05-20
---

# Verifier — Phase 3b Interoperability Specifications

## When this runs

After phase3b-interop-win reports done, BEFORE SVA-3 (Spec Validator gate).

## Inputs (read in this order)

1. `{folder}/outputs/INTEROPERABILITY.md` — primary output under verification
2. `{folder}/shared/requirements-normalized.json` — for source_id traceback + protocol-version override detection
3. `{folder}/shared/domain-context.json` — for `selected_domain` to validate domain-appropriate system inventory
4. Any prior SVA-3 report at `{folder}/shared/validation/sva3-spec.json` (if a retry run)

## Verification Checks

### Check 1 — File exists, ≥ 5 KB, UTF-8 round-trip clean

**Criterion:** `outputs/INTEROPERABILITY.md` exists, size ≥ 5,120 bytes (5 KB), valid UTF-8 with no `�` / `Ã©` / `â€™` mojibake.

**Pass:** Exists, size ≥ 5,120, UTF-8 decodes cleanly.

**Fail:** Missing OR under 5,120 OR mojibake.

**Evidence to cite:** `os.path.getsize()` value; first 80 chars of any mojibake hit with offset.

**Hard-rule reminder:** NEVER claim file missing without `Glob` first.

---

### Check 2 — External Systems Inventory table present and populated

**Criterion:** Document contains heading `External System Inventory` (or `External Systems`) AND a markdown table with at least one data row (header + separator + ≥ 1 row). Required columns: `System`, `Type`, `Protocol`.

**Pass:** Heading present, table parses, ≥ 1 data row with non-empty System/Type/Protocol cells.

**Fail:** Heading absent OR table missing OR all rows empty.

**Evidence to cite:** Heading line + table row count + first row contents.

---

### Check 3 — API/integration patterns enumerated

**Criterion:** Document contains `API Specifications` section AND at least one of `Inbound APIs` / `Outbound APIs` / `Integration Patterns`. Body must enumerate at least 2 integration patterns from the set {`REST`, `SOAP`, `GraphQL`, `EDI`, `HL7`, `FHIR`, `Message Queue`, `SFTP`, `Webhook`} (case-insensitive).

**Pass:** Sections present, ≥ 2 patterns enumerated.

**Fail:** Sections absent OR < 2 patterns named.

**Evidence to cite:** Patterns found + their headings/line numbers.

---

### Check 4 — Data exchange formats specified per integration

**Criterion:** For each row in the External System Inventory table, the `Protocol` cell is non-empty AND not equal to `TBD`/`N/A`/`?`. If RFP-detected protocol versions exist (per phase Step 2 `detected_versions` log line), those versions must appear in the table cells — e.g., if RFP cites `FHIR R5`, the table must contain `FHIR R5` (not the default `FHIR R4`).

**Pass:** All Protocol cells non-empty AND any detected RFP version overrides applied.

**Fail:** Empty / placeholder Protocol cells OR detected-version override missed (stale default retained when RFP cited newer).

**Evidence to cite:** Row + cell value + detected version (if any).

---

### Check 5 — Each integration has source_id traceback to an RFP requirement

**Criterion:** For each external system or API spec block, there must be at least one citation linking back to an RFP requirement — either an inline `req_id` (e.g., `INT-001`, `req_id: SEC-042`), or a section in the document explicitly mapping systems to requirements (a "Requirements Coverage" or "Source Requirements" subsection / table). Compute: at least 80% of system inventory rows have a traceback citation.

**Pass:** ≥ 80% of integrations cite a source requirement.

**Concern:** 60–79% — log advisory.

**Fail:** < 60% — too many fabricated integrations with no RFP basis.

**Evidence to cite:** Count of systems with vs without traceback citation; list up to 5 systems lacking citation.

---

### Check 6 — No fabricated third-party endpoints

**Criterion:** For each named third-party system in the inventory (e.g., `Tyler/NIC Oregon`, `Epic/Cerner`, `CEDARS`, `Stripe`), verify the name appears in either: (a) `requirements-normalized.json` requirement text, OR (b) `domain-context.json` `selected_domain` profile, OR (c) the default DOMAIN_SYSTEMS dict for the selected domain. Fabricated names (system in the inventory but NOT traceable to any input) are a fail.

**Pass:** Every named system traces to RFP text, domain context, or domain default.

**Fail:** Any system name appears only in INTEROPERABILITY.md with no upstream evidence.

**Evidence to cite:** System name + absence from all three traceback sources.

**Anti-false-positive rule:** Generic descriptors like `ERP System`, `CRM`, `Email Service` are domain defaults from the `default` profile — they pass automatically. Only flag specific vendor/product names with no traceback.

---

### Check 7 — Error handling + security sections present

**Criterion:** Document contains `Error Handling` section (or `Retry Strategy` / `Error Response`) AND `Security Considerations` section (or `Authentication` / `Data Protection`). Each must have non-trivial body content (> 100 chars between heading and next heading).

**Pass:** Both sections present with body content.

**Fail:** Either section missing or stub-only.

**Evidence to cite:** Section heading + body byte count.

---

### Check 8 — No truncation artifacts, no row-cap notices

**Criterion:** Grep for `_Showing N of M_`, empty `|  |` table cells, mid-word ellipsis (e.g., `protoco...`), and `[:N]` slice notation in deliverable prose. All counts must be 0.

**Pass:** Zero hits across all patterns.

**Fail:** Any hit.

**Evidence to cite:** Pattern + line number + snippet.

---

## Disposition Logic

- **PASS:** Checks 1–4, 6–8 pass AND Check 5 not in FAIL band.
- **CONCERN:** Check 5 in advisory band (60–79%). Log + continue to SVA-3.
- **FAIL:** Any of Checks 1–4, 6–8 fail OR Check 5 < 60%.

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase3b-interop-win with these targeted corrections:

[Check 1 fail] INTEROPERABILITY.md missing or under 5KB (actual: {N} bytes).
  Action: Re-run Step 3 (generate_interop_md). Verify integration_reqs is non-empty
  and all generator subfunctions returned strings (not None / empty).

[Check 2 fail] External System Inventory table missing or empty.
  Action: Re-run Step 2 — confirm DOMAIN_SYSTEMS[domain] is populated and the
  generation loop iterates external_systems. Print external_systems before render.

[Check 3 fail] API Specifications section missing or < 2 patterns.
  Action: Confirm Integration Patterns block emits REST + Message Queue + Batch minimum.

[Check 4 fail] Protocol cells empty / RFP-detected version override missed: {list}.
  Action: Re-run Step 2 protocol-version scan loop. Confirm detected_versions dict is
  built and each external_systems entry has protocol_detected_from_rfp applied where
  the label matches (HL7 v2, HL7 FHIR, NCPDP SCRIPT, X12).

[Check 5 fail] Integrations without source_id traceback: {list}.
  Action: For each system in the inventory, add a Requirements Coverage subsection that
  cites the req_id(s) driving that integration. If a system has no RFP basis,
  remove it from the inventory — do not fabricate.

[Check 6 fail] Fabricated third-party endpoint(s): {list}.
  Action: Remove vendor/product names not in requirements-normalized.json, domain-context.json,
  or DOMAIN_SYSTEMS[domain]. Pipelines must never invent specific integrations.

[Check 7 fail] Error Handling or Security section missing/stub.
  Action: Re-run generate_error_handling_section and generate_security_section.

[Check 8 fail] Truncation artifacts: {pattern + line}.
  Action: Remove [:N] slicing per feedback_screen_encoding_truncation.md discipline.

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input:**

```
INTEROPERABILITY.md is 3 KB.
External System Inventory table absent.
Contains "Acme Payment Gateway" — name not in requirements text, not in domain context,
  not in DOMAIN_SYSTEMS["healthcare"] default profile (selected domain is healthcare).
RFP text says "FHIR R5 required" but table shows "HL7 FHIR R4" (default not overridden).
No Error Handling section.
```

Verifier MUST detect: Check 1 (under 5KB), Check 2 (no inventory table), Check 4 (FHIR version override missed), Check 6 (Acme Payment Gateway fabricated), Check 7 (no Error Handling). Disposition: FAIL.

**Known-good input:**

```
INTEROPERABILITY.md is 8 KB.
External System Inventory has 6 rows: Epic/Cerner, Lab Systems, Pharmacy, Insurance,
  plus 2 RFP-cited custom integrations with req_id traceback.
RFP cited "FHIR R5" → table shows "HL7 FHIR R5" (override applied).
All systems traceable to RFP text or DOMAIN_SYSTEMS["healthcare"].
Error Handling, Security Considerations, API Specifications sections all populated.
No truncation artifacts.
```

Verifier MUST PASS all checks. Disposition: PASS.

## Hard Rules (inherited from verifier-design principles)

1. NEVER claim INTEROPERABILITY.md is missing without `Glob` verification first.
2. NEVER flag a system as fabricated if its name is in `DOMAIN_SYSTEMS[selected_domain]` default profile OR appears verbatim in `requirements-normalized.json` requirement text (Check 6 anti-false-positive).
3. NEVER flag a Protocol cell as "empty" if it contains a valid protocol name even if version qualifier is absent — only flag truly empty / `TBD` / `?` placeholders.
4. Every finding must cite specific row + cell value + INTEROPERABILITY.md line number (e.g., `row 4: Epic/Cerner | EHR | HL7 FHIR R4` + `RFP detected FHIR R5 — override missed`).
5. On FAIL, return corrective instructions with the specific system name or protocol so the phase agent can target the repair without re-running everything.
