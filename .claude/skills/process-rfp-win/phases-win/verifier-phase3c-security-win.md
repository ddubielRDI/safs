---
name: verifier-phase3c-security-win
expert-role: Security Specification Verifier
purpose: phase-boundary verifier for phase3c-security-win (file size, compliance framework parity with Phase 1.7, OWASP/encryption/auth coverage, no boilerplate fluff)
created: 2026-05-20
---

# Verifier — Phase 3c Security Specifications

## When this runs

After phase3c-security-win reports done, BEFORE SVA-3 (Spec Validator gate).

## Inputs (read in this order)

1. `{folder}/outputs/SECURITY_REQUIREMENTS.md` — primary output under verification
2. `{folder}/shared/COMPLIANCE_MATRIX.json` — authoritative list of compliance frameworks the RFP requires (from Phase 1.7)
3. `{folder}/shared/requirements-normalized.json` — for cross-checking security-keyword requirements
4. `{folder}/shared/domain-context.json` — for `selected_domain` (drives primary compliance default)
5. Any prior SVA-3 report at `{folder}/shared/validation/sva3-spec.json` (if a retry run)

## Verification Checks

### Check 1 — File exists, ≥ 8 KB, UTF-8 round-trip clean

**Criterion:** `outputs/SECURITY_REQUIREMENTS.md` exists, size ≥ 8,192 bytes (8 KB), valid UTF-8 with no `�` / `Ã©` / `â€™` mojibake.

**Pass:** Exists, size ≥ 8,192, UTF-8 decodes cleanly.

**Fail:** Missing OR under 8,192 OR mojibake.

**Evidence to cite:** `os.path.getsize()` value; first 80 chars of any mojibake hit with offset.

**Hard-rule reminder:** NEVER claim file missing without `Glob` first.

---

### Check 2 — Compliance frameworks parity with COMPLIANCE_MATRIX.json (no extras, no omissions)

**Criterion:** Build set `RFP_FRAMEWORKS` from `COMPLIANCE_MATRIX.json` (every framework label cited as mandatory in the matrix — e.g., HIPAA, FERPA, FedRAMP, NIST 800-53, SOC 2, CIS Controls, FISMA, PCI DSS, GDPR, CCPA). Build set `DOC_FRAMEWORKS` from grepping SECURITY_REQUIREMENTS.md for each label. Verify: every framework in `RFP_FRAMEWORKS` appears in `DOC_FRAMEWORKS` (no omissions). Also verify no framework appears in `DOC_FRAMEWORKS` that isn't in `RFP_FRAMEWORKS` UNLESS it is in the `KNOWN_FOUNDATIONAL` set {`SOC 2`, `CIS Controls`, `OWASP`} (which apply by default).

**Pass:** No required framework missing; no extra non-foundational framework invented.

**Fail:** Any RFP-required framework absent from the doc OR any extra non-foundational framework invented (e.g., GDPR added for a US K-12 domain where RFP never mentioned it).

**Evidence to cite:** `RFP_FRAMEWORKS \ DOC_FRAMEWORKS` (missing set) and `DOC_FRAMEWORKS \ (RFP_FRAMEWORKS ∪ KNOWN_FOUNDATIONAL)` (extra set).

---

### Check 3 — OWASP Top 10 coverage present and complete

**Criterion:** Document contains an `OWASP Top 10` heading AND mentions all ten categories. Acceptable forms: `A01:` through `A10:` (2021 numbering), OR `A1:` through `A10:` (older 2017 numbering — accept if RFP explicitly cites that version, fail otherwise).

**Pass:** All ten OWASP categories present in current-numbering form.

**Concern:** Older 2017 numbering used WITHOUT RFP explicitly citing 2017.

**Fail:** OWASP section absent OR fewer than 10 categories listed.

**Evidence to cite:** Count of `A0[1-9]:` and `A10:` hits.

---

### Check 4 — Encryption + authentication + authorization sections present

**Criterion:** Document contains all of: `Encryption Standards` (or table referencing `AES-256` AND `TLS 1.3`), `Authentication Architecture` (or `Authentication` section with MFA reference), `Authorization Model` (or `Role-Based Access Control` / `RBAC`). Each section must have body > 200 chars.

**Pass:** All three sections present with non-trivial bodies.

**Fail:** Any of the three absent or stub-only.

**Evidence to cite:** Section heading + body byte count.

---

### Check 5 — Domain-appropriate compliance addressed

**Criterion:** Based on `domain-context.json:selected_domain`:
- `education` → grep `FERPA` must return ≥ 1 hit
- `healthcare` → grep `HIPAA` must return ≥ 1 hit
- `government` / `federal` / `state-government` / `local-government` → grep `NIST 800-53` OR `FedRAMP` OR `FISMA` must return ≥ 1 hit
- `financial` / `payments` → grep `PCI DSS` must return ≥ 1 hit
- `default` / generic → grep `SOC 2` must return ≥ 1 hit

**Pass:** Domain-appropriate compliance label appears.

**Fail:** Required label missing for the selected domain.

**Evidence to cite:** `selected_domain` + expected label + grep count (0).

---

### Check 6 — CIS Controls version + source URL cited (and current)

**Criterion:** Document contains `CIS Controls v8.1` (or current version at phase execution time) AND a `https://www.cisecurity.org/controls/` source URL AND a verification date within 90 days of today.

**Pass:** All three present and version is current.

**Concern:** Source URL present but verification date > 90 days old (stale).

**Fail:** CIS Controls cited without version OR without source URL.

**Evidence to cite:** Version string + URL + date.

---

### Check 7 — Each clause maps to a requirement (no boilerplate fluff)

**Criterion:** Document contains a `Security Requirements Matrix` (or `Compliance Checklist` / `Requirements Coverage`) table that maps each security clause to one or more `req_id`s from `requirements-normalized.json`. At least 70% of rows must have a non-empty `req_id` cell that exists in the normalized requirements set.

**Pass:** ≥ 70% of matrix rows have valid req_id citations.

**Concern:** 50–69% — log advisory.

**Fail:** < 50% — too much boilerplate not tied to RFP requirements.

**Evidence to cite:** Count of rows with vs without valid req_id; list up to 5 rows with missing/invalid citations.

---

### Check 8 — No truncation artifacts, no row-cap notices

**Criterion:** Grep for `_Showing N of M_`, empty `|  |` table cells in Security Requirements Matrix, mid-word ellipsis, and `[:N]` slice notation. All counts must be 0.

**Pass:** Zero hits across all patterns.

**Fail:** Any hit.

**Evidence to cite:** Pattern + line number + snippet.

---

## Disposition Logic

- **PASS:** Checks 1, 2, 4, 5, 8 pass AND Checks 3, 6, 7 not in FAIL band.
- **CONCERN:** Check 3 / 6 / 7 in advisory band only. Log + continue to SVA-3.
- **FAIL:** Any of Checks 1, 2, 4, 5, 8 fail OR Check 3 / 6 / 7 below FAIL threshold.

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase3c-security-win with these targeted corrections:

[Check 1 fail] SECURITY_REQUIREMENTS.md missing or under 8KB (actual: {N} bytes).
  Action: Re-run Step 3 generate_security_md — verify every sub-generator returned a
  populated string. Check for silent exception swallowing.

[Check 2 fail] Compliance framework parity broken.
  Missing from doc: {list}. Extra invented: {list}.
  Action: Re-build the compliance section by iterating COMPLIANCE_MATRIX.json mandatory_items.
  Do not add frameworks unless they are foundational (SOC 2, CIS Controls, OWASP) or
  appear in the RFP's COMPLIANCE_MATRIX.json.

[Check 3 fail] OWASP Top 10 incomplete: only {N}/10 categories present.
  Action: Re-emit all ten A01..A10 (2021) mitigations. If RFP cites a different version,
  cite that version explicitly with source URL.

[Check 4 fail] Encryption / Authentication / Authorization section(s) missing.
  Action: Confirm Step 3 emitted each section. Verify AES-256 in table, MFA in
  Authentication section, RBAC in Authorization section.

[Check 5 fail] Domain-appropriate compliance label missing.
  selected_domain = {domain}. Expected label: {label}. Action: Add the domain-required
  compliance section per Step 2 DOMAIN_COMPLIANCE dict.

[Check 6 fail] CIS Controls citation incomplete: {what is missing}.
  Action: Cite CIS Controls current version with cisecurity.org source URL and
  fetched_at date < 90 days old.

[Check 7 fail] Security Requirements Matrix has too many uncited rows ({N}% have valid req_id).
  Action: Re-build the matrix by iterating security_reqs (from Step 1 keyword filter).
  Every row's req_id cell must reference an actual canonical_id from requirements-normalized.json.

[Check 8 fail] Truncation artifacts: {pattern + line}.
  Action: Remove [:50], [:N] slicing per feedback_screen_encoding_truncation.md discipline.

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input:**

```
SECURITY_REQUIREMENTS.md is 6 KB.
COMPLIANCE_MATRIX.json mandates HIPAA, NIST 800-53, FedRAMP Moderate.
SECURITY_REQUIREMENTS.md contains: HIPAA + GDPR + SOC 2 (NIST 800-53 and FedRAMP missing,
  GDPR invented for a US healthcare domain).
OWASP section has only A01, A02, A03 (7 categories missing).
No Encryption Standards section.
Security Requirements Matrix has 30 rows; 24 of them have req_id = "N/A" or empty.
```

Verifier MUST detect: Check 1 (under 8KB), Check 2 (NIST/FedRAMP missing, GDPR extra), Check 3 (only 3/10 OWASP), Check 4 (no Encryption section), Check 7 (only 6/30 = 20% rows cited). Disposition: FAIL.

**Known-good input:**

```
SECURITY_REQUIREMENTS.md is 14 KB.
Compliance frameworks: HIPAA, NIST 800-53, FedRAMP Moderate, SOC 2, CIS Controls v8.1.
OWASP A01..A10 all present.
Encryption Standards, Authentication Architecture, Authorization Model all populated.
CIS Controls v8.1 cited with https://www.cisecurity.org/controls/ + 2026-05-19 verification date.
Security Requirements Matrix has 45 rows; 38 (84%) have valid req_id citations.
```

Verifier MUST PASS all checks. Disposition: PASS.

## Hard Rules (inherited from verifier-design principles)

1. NEVER claim SECURITY_REQUIREMENTS.md is missing without `Glob` verification first.
2. NEVER flag a foundational framework (SOC 2, CIS Controls, OWASP) as "invented" — they apply by default (Check 2 anti-false-positive).
3. NEVER flag an OWASP category as missing if it appears in a different numbering convention that the RFP explicitly cited (e.g., 2017 numbering when RFP cites 2017).
4. Every finding must cite the specific framework label + SECURITY_REQUIREMENTS.md line number + COMPLIANCE_MATRIX.json source entry (e.g., `COMPLIANCE_MATRIX.json:mandatory_items[7] = "FedRAMP Moderate"` + `grep "FedRAMP" SECURITY_REQUIREMENTS.md = 0 hits`).
5. On FAIL, return corrective instructions naming the specific framework / section / row count so the phase agent can target the repair surgically.
