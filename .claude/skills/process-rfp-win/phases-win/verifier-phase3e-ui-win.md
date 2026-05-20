---
name: verifier-phase3e-ui-win
expert-role: UI/UX Specification Verifier
purpose: phase-boundary verifier for phase3e-ui-win (file existence, WCAG version fidelity to RFP, persona/screen flow coverage, responsive design, no fabricated screens)
created: 2026-05-20
---

# Verifier — Phase 3e UI/UX Specifications

## When this runs

After phase3e-ui-win reports done, BEFORE SVA-3 (Spec Validator gate).

## Inputs (read in this order)

1. `{folder}/outputs/UI_SPECS.md` — primary output under verification
2. `{folder}/shared/requirements-normalized.json` — for WCAG version detection + UI-keyword traceback
3. `{folder}/shared/domain-context.json` — for `selected_domain` and persona inference
4. Any prior SVA-3 report at `{folder}/shared/validation/sva3-spec.json` (if a retry run)

## Verification Checks

### Check 1 — File exists, ≥ 1 KB, UTF-8 round-trip clean

**Criterion:** `outputs/UI_SPECS.md` exists, size ≥ 1,024 bytes, valid UTF-8 with no `�` / `Ã©` / `â€™` mojibake.

**Pass:** Exists, size ≥ 1,024, UTF-8 decodes cleanly.

**Fail:** Missing OR under 1,024 OR mojibake.

**Evidence to cite:** `os.path.getsize()` value; first 80 chars of any mojibake hit with offset.

**Hard-rule reminder:** NEVER claim file missing without `Glob` first.

---

### Check 2 — Accessibility section present with WCAG cited

**Criterion:** Document contains heading `Accessibility Requirements` (or `Accessibility`) AND mentions `WCAG` with a version (2.0, 2.1, or 2.2) AND conformance level (`AA` or `AAA`).

**Pass:** Heading present, WCAG version + AA/AAA cited.

**Fail:** Heading absent OR WCAG missing OR version-without-level (e.g., `WCAG` alone is insufficient).

**Evidence to cite:** Heading line + WCAG version string + level.

---

### Check 3 — WCAG version matches RFP mandate (no downgrade, no fabricated upgrade)

**Criterion:** Scan `requirements-normalized.json` requirement text + full_context for explicit WCAG version mentions:
- If `wcag 2.2` / `wcag2.2` present → UI_SPECS.md MUST cite WCAG 2.2
- If `wcag 2.1` / `wcag2.1` present (and 2.2 absent) → UI_SPECS.md MUST cite WCAG 2.1
- If `section 508` present (and no explicit 2.x) → UI_SPECS.md MUST cite WCAG 2.2 with Section 508 note
- If nothing explicit → UI_SPECS.md MUST default to WCAG 2.2 (current baseline per phase Step 1b)

A downgrade (RFP cites 2.2 but doc cites 2.1) is a FAIL. A non-justified upgrade (RFP cites 2.1 but doc cites 2.2 without rationale note) is a CONCERN.

**Pass:** Cited version matches RFP mandate (or default 2.2 if no RFP mandate).

**Concern:** Doc upgrades above RFP mandate without rationale (e.g., 2.1 → 2.2 with no Section 508 note).

**Fail:** Doc downgrades below RFP mandate.

**Evidence to cite:** RFP-detected version + UI_SPECS cited version + rationale note presence.

---

### Check 4 — Source URL with verification date cited

**Criterion:** Document contains `https://www.w3.org/TR/WCAG2` URL (any 2.x variant) AND a verification date in YYYY-MM-DD form within 365 days of today.

**Pass:** URL present, date within 365 days.

**Concern:** URL present but date > 365 days old (stale).

**Fail:** URL absent.

**Evidence to cite:** URL string + date + delta in days.

---

### Check 5 — WCAG 2.2 new Success Criteria included when version == 2.2

**Criterion:** If cited WCAG version is 2.2, the document must contain at least 3 of the 9 new SC from WCAG 2.2: `2.4.11`, `2.4.12`, `2.4.13`, `2.5.7`, `2.5.8`, `3.2.6`, `3.3.7`, `3.3.8`, `3.3.9`.

**Pass:** ≥ 3 new SC referenced (or SKIP if WCAG version is 2.1/2.0 per RFP mandate).

**Fail:** WCAG 2.2 cited but fewer than 3 new SC present (template did not render version-specific block).

**Evidence to cite:** Count of SC identifiers found.

---

### Check 6 — Persona-based screen flows / Screen Specifications present

**Criterion:** Document contains `Screen Specifications` section (or `Screen Flows` / `User Flows`) with at least 3 numbered screens (`Screen 1`, `Screen 2`, `Screen 3` or `### Screen N`). Each screen block must include at least one of: `Components`, `User Actions`, `Validation Rules`.

**Pass:** Section present, ≥ 3 screen blocks, each with substantive subsections.

**Fail:** Section absent OR < 3 screens OR screens with no subsections (stub-only).

**Evidence to cite:** Screen count + subsection presence per screen.

---

### Check 7 — Responsive / mobile design addressed when RFP requires it

**Criterion:** Scan `requirements-normalized.json` text for keywords: `mobile`, `responsive`, `tablet`, `touch`, `phone`. If ≥ 1 hit, UI_SPECS.md MUST contain a `Responsive Design` (or `Mobile Considerations` / `Breakpoints`) section with body > 100 chars.

**Pass:** Either RFP doesn't require responsive (no keyword hits) OR section present with non-trivial body.

**Fail:** RFP requires responsive AND section absent or stub-only.

**Evidence to cite:** RFP keyword hits + section presence + body byte count.

---

### Check 8 — No fabricated screen names beyond RFP scope

**Criterion:** For each named screen in the Screen Specifications section, the screen's purpose/requirement citation must trace to a `canonical_id` in `requirements-normalized.json`. Compute: at least 70% of screens have a valid req_id reference (either in the screen heading like `UI-001` or in the Requirements Coverage appendix).

**Pass:** ≥ 70% of screens cite a real requirement.

**Concern:** 50–69% — log advisory.

**Fail:** < 50% — too many fabricated screens.

**Evidence to cite:** Count of screens with valid req_id vs without; list up to 5 unciteable.

**Anti-false-positive rule:** Generic templates like `Dashboard Template`, `List View Template`, `Form Template` are design-system patterns, not "screens" in scope — exclude them from the count.

---

### Check 9 — No truncation artifacts, no row-cap notices

**Criterion:** Grep for `_Showing N of M_`, empty `|  |` table cells in coverage tables, mid-word ellipsis, and `[:N]` slice notation. All counts must be 0.

**Pass:** Zero hits across all patterns.

**Fail:** Any hit.

**Evidence to cite:** Pattern + line number + snippet.

---

## Disposition Logic

- **PASS:** Checks 1–4, 6, 7, 9 pass AND Checks 5, 8 not in FAIL band.
- **CONCERN:** Check 3 / 4 / 8 in advisory band only. Log + continue to SVA-3.
- **FAIL:** Any of Checks 1, 2, 5, 6, 7, 9 fail OR Check 3 / 4 / 8 below FAIL threshold.

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase3e-ui-win with these targeted corrections:

[Check 1 fail] UI_SPECS.md missing or under 1KB.
  Action: Re-run Step 2 generate_ui_specs — verify ui_reqs is non-empty and generator emitted body.

[Check 2 fail] Accessibility section / WCAG cite missing.
  Action: Confirm Step 1b detected the WCAG version (default 2.2 AA) and Step 2
  emitted the Accessibility Requirements section.

[Check 3 fail] WCAG version mismatch with RFP mandate.
  RFP detected: {version}. UI_SPECS cites: {cited}.
  Action: Re-run Step 1b — confirm wcag_version variable is set per the detection logic
  and substituted into the template.

[Check 4 fail] WCAG source URL or verification date missing.
  Action: Confirm Step 2 generator emits "https://www.w3.org/TR/WCAG{version_compact}/"
  and current-date marker.

[Check 5 fail] WCAG 2.2 cited but new SC missing (< 3 of 9).
  Action: Re-run Step 1b — confirm wcag22_operable_sc and wcag22_understandable_sc strings
  are concatenated into the template when wcag_version == "2.2".

[Check 6 fail] Screen Specifications missing / < 3 screens.
  Action: Re-run Step 2 screen-generation loop — verify ui_reqs has ≥ 3 entries.
  Check Step 1 keyword filter; if too narrow, broaden ui_keywords list.

[Check 7 fail] Responsive section missing despite RFP requiring it.
  Action: Re-emit generate_ui_specs Responsive Design block — confirm breakpoints table
  and Mobile Considerations subsection emitted.

[Check 8 fail] Fabricated screens: {list}.
  Action: For each screen without req_id traceback, remove the screen or add a
  Requirements Coverage row citing the actual canonical_id.

[Check 9 fail] Truncation artifacts: {pattern + line}.
  Action: Remove [:200], [:N] slicing per feedback_screen_encoding_truncation.md discipline.

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input:**

```
UI_SPECS.md is 0.5 KB.
RFP cites "WCAG 2.2 AA" — UI_SPECS cites "WCAG 2.1 AA" (downgrade).
Accessibility section missing the WCAG 2.2 new SC (2.5.8, 3.3.8).
RFP mentions "mobile" + "responsive" — UI_SPECS has no Responsive Design section.
Screen Specifications has 8 screens, only 2 cite a real req_id (6 fabricated).
```

Verifier MUST detect: Check 1 (under 1KB), Check 3 (downgrade), Check 5 (no new 2.2 SC), Check 7 (missing Responsive), Check 8 (< 50% screens cite). Disposition: FAIL.

**Known-good input:**

```
UI_SPECS.md is 14 KB.
WCAG 2.2 AA cited, source URL + 2026-05-20 verification date.
New SC 2.4.11, 2.5.8, 3.3.8, 3.3.9 all present.
Screen Specifications has 15 screens, 13 (87%) cite valid canonical_ids.
Responsive Design section with breakpoints table + Mobile Considerations.
No [:N] truncation.
```

Verifier MUST PASS all checks. Disposition: PASS.

## Hard Rules (inherited from verifier-design principles)

1. NEVER claim UI_SPECS.md is missing without `Glob` verification first.
2. NEVER flag generic design-system templates (`Dashboard Template`, `List View Template`, `Form Template`) as fabricated screens — they are wireframe patterns, not requirement-scoped screens (Check 8 anti-false-positive).
3. NEVER report WCAG version as wrong without first reading the RFP detection logic outputs from Step 1b — the detected version may legitimately default to 2.2 when no explicit RFP citation exists.
4. Every finding must cite specific WCAG version + UI_SPECS.md line + RFP detection source (e.g., `requirements-normalized.json req_id REQ-042: "system must conform to WCAG 2.2 AA"` + `UI_SPECS.md:240 "WCAG 2.1 AA"` = downgrade).
5. On FAIL, return corrective instructions naming the specific section / WCAG version / screen count so the phase agent can target the repair surgically.
