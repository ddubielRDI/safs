---
name: verifier-phase8.1-submittal-win
expert-role: Cover Letter Quality Verifier
purpose: phase-boundary verifier for phase8.1-submittal-win (NOSE formula presence, win-theme threading, EXECUTIVE persona integration, page-budget awareness)
created: 2026-05-20
---

# Verifier — Phase 8.1 Letter of Submittal

## When this runs

After phase8.1-submittal-win reports done, BEFORE Phase 8.2 (Management Proposal) reads the submittal for cross-references and tone consistency.

## Inputs (read in this order)

1. `{folder}/outputs/bid-sections/01_SUBMITTAL.md` — primary output under verification
2. `{folder}/shared/bid/POSITIONING_OUTPUT.json` — for win_themes, matched_projects, evaluator_messages, ghost_strategy carry-forward
3. `{folder}/shared/EVALUATION_CRITERIA.json` — for top-weighted factor preview
4. `{folder}/shared/COMPLIANCE_MATRIX.json` — for required certification/attestation coverage
5. `config-win/company-profile.json` — for items that should NOT carry `[USER INPUT REQUIRED]` (signatory, locations, etc.)

## Verification Checks

### Check 1 — File exists and meets minimum size

**Criterion:** `outputs/bid-sections/01_SUBMITTAL.md` exists AND file size >= 3,072 bytes (3 KB minimum — phase target is 5 KB but anything under 3 KB is a stub failure).

**Pass:** File exists, size >= 3,072 bytes.

**Fail:** File absent OR size < 3,072 bytes.

**Evidence to cite:** File path + actual size in bytes.

**Hard-rule reminder:** NEVER claim file is missing without `Glob` verification first.

---

### Check 2 — NOSE formula sections present

**Criterion:** Grep the document for the four NOSE elements as section headings or explicit framing. Required hits (case-insensitive, each must occur >= 1 time): "Understanding Your Needs" OR "Needs"; "Outcomes" OR "Delivering Outcomes"; "Solution" OR "Solution Approach"; "Evidence" OR "Why Resource Data" (the standard NOSE Evidence section heading).

**Pass:** All 4 NOSE markers found.

**Fail:** Any one of the 4 NOSE markers absent.

**Evidence to cite:** Per missing element: grep pattern used and result count.

---

### Check 3 — Evaluation factor preview/callout at top of letter

**Criterion:** Within the first 2,500 characters of the document, a callout or paragraph must reference the top-weighted evaluation factor(s) by name. Pattern: blockquote `> **Evaluation Factors Addressed:**` OR free-text mention of at least one top-3 factor name from EVALUATION_CRITERIA.json.

**Pass:** Top-weighted eval factor named within first 2,500 chars.

**Fail:** No eval-factor preview in the letter's opening.

**Evidence to cite:** First 2,500 chars searched; report the matching line or "no match".

---

### Check 4 — >= 3 distinct win themes referenced

**Criterion:** Load `win_themes` from POSITIONING_OUTPUT.json. Count how many distinct theme names appear as bold-bracketed markers `**[Theme Name]**` OR as bold-only `**Theme Name**` in the document. Threshold: 3 distinct themes.

**Pass:** >= 3 distinct themes from win_themes appear in the letter.

**Concern:** Exactly 2 distinct themes appear — log advisory.

**Fail:** Fewer than 2 distinct themes appear. The letter is not threading positioning.

**Evidence to cite:** List the themes from positioning; list which appeared in the letter (with line numbers).

---

### Check 5 — matched_projects evidence embedded (no `[CASE STUDY PLACEHOLDER]`)

**Criterion:** Zero occurrences of `[CASE STUDY PLACEHOLDER]`, `[PROJECT PLACEHOLDER]`, or similar TBD markers for case-study content. The "Why Resource Data" section must reference at least one client name from `matched_projects[0]` (top-ranked match).

**Pass:** Zero placeholder markers AND top-matched client name appears in document.

**Fail:** Any placeholder marker present OR top-matched client absent.

**Evidence to cite:** Quote any placeholder hit; OR report top-matched client name and whether grep found it.

---

### Check 6 — EXECUTIVE persona messaging applied

**Criterion:** Load `evaluator_messages.EXECUTIVE.headline` from POSITIONING_OUTPUT.json. Search the submittal for at least 3 words of the headline (or a paraphrased keyword cluster like "strategic partnership", "long-term success", "executive sponsor"). The "Our Solution Approach" or equivalent section must reflect EXECUTIVE-persona framing.

**Pass:** EXECUTIVE headline keywords (>= 3 words) detected OR an EXECUTIVE proof-point phrase appears.

**Concern:** Generic executive language present but no specific match to positioning's EXECUTIVE headline — log advisory.

**Fail:** Zero EXECUTIVE-persona content detectable.

**Evidence to cite:** Positioning's EXECUTIVE headline text; matched substring or "no match" with first 300 chars of the solution section.

---

### Check 7 — No `[USER INPUT REQUIRED]` for company-profile-satisfiable items

**Criterion:** Scan for `[USER INPUT REQUIRED` markers. For each occurrence, check whether the requested information exists in `config-win/company-profile.json` (signatory name/title/email, primary location address/city/state/zip/phone, website, years_in_business, employees). If yes, that marker is a FAIL — the phase should have substituted the company-profile value.

**Pass:** All `[USER INPUT REQUIRED]` markers refer to genuinely user-supplied data (procurement officer name, RFP-specific dates, etc.) NOT items in company-profile.json.

**Fail:** One or more markers reference items that company-profile.json can satisfy.

**Evidence to cite:** Line number + quoted marker context + the company-profile.json field that should have been substituted.

---

### Check 8 — Compliance certifications represented

**Criterion:** Load `mandatory_items` from COMPLIANCE_MATRIX.json. Filter to items whose text contains a certification keyword (`certify`, `attest`, `represent`, `declare`, `affirm`, `warrant`). For each, the certification text (or a clearly paraphrased version) must appear in the "Certifications and Representations" section of the submittal.

**Pass:** All certification mandatory items have corresponding text in the certifications section. If no certifications were mandated by the RFP, PASS by vacuity.

**Concern:** 1 of N certifications absent — log advisory.

**Fail:** 2 or more required certifications absent from the letter.

**Evidence to cite:** List mandatory certification IDs; for each, report whether keywords appear in the certifications section.

---

### Check 9 — Page-budget awareness (MARS-style 25-page cap)

**Criterion:** Estimate page count: characters / 3,000 (typical for proposal-style markdown with tables). For the cover letter alone, target is 1-2 pages (3,000-6,000 chars). If the submittal alone exceeds 8,000 chars, it consumes too much of the 25-page proposal budget.

**Pass:** Length 3,000-8,000 chars (approx 1-2.5 pages).

**Concern:** 8,000-12,000 chars — log advisory; consider trimming before final assembly.

**Fail:** > 12,000 chars (consumes > 4 pages of the 25-page budget). OR < 2,000 chars (insubstantial).

**Evidence to cite:** Actual character count, estimated page count.

---

### Check 10 — Universal regression patterns (UTF-8, no `[:N]`, no `_Showing N of M_`)

**Criterion:** Three sub-checks combined:
(a) File decodes as UTF-8; zero mojibake patterns (`Ã`, `â€`, `Â `, `�`).
(b) Zero `_Showing \d+ of \d+_` row-cap notices.
(c) Zero `[:N]` truncation patterns visible in deliverable strings (no mid-word cuts at line/cell ends).
(d) Zero em-dash characters (`—` U+2014) — submittal must use `--` per fitz.Story rule.
(e) Zero internal file references (`.json`, `.md`, `Past_Projects`).

**Pass:** All five sub-checks pass.

**Fail:** Any sub-check has 1+ violation.

**Evidence to cite:** For each violation: line number + quoted offending content.

---

## Disposition Logic

- **PASS:** All 10 checks pass.
- **CONCERN:** Checks 4, 6, 8, or 9 in advisory band; all others pass.
- **FAIL:** Any of Checks 1, 2, 3, 5, 7, 10 fail OR Checks 4/6/8/9 fall below FAIL threshold.

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase8.1-submittal with the following targeted corrections:

[Check 1 fail] 01_SUBMITTAL.md missing or < 3 KB ({actual_size} bytes).
  Action: Re-run full phase. Verify positioning + compliance + company files loaded.

[Check 2 fail] Missing NOSE section(s): {list}.
  Action: Step 3 letter template must include four ## headings mapping to N-O-S-E.
  Confirm "Understanding Your Needs", "Delivering Outcomes That Matter",
  "Our Solution Approach", "Why Resource Data" all rendered.

[Check 3 fail] No evaluation factor preview in opening.
  Action: The blockquote callout `> **Evaluation Factors Addressed:**` at the top
  of the letter is mandatory. Pull top-3 factors from eval_factors_by_weight
  in the context bundle and emit the callout.

[Check 4 fail] Only {N} distinct themes referenced (need 3+).
  Action: Iterate through win_themes and confirm each appears in bold-bracket
  format in the Outcomes section. Do not rely on Step 3's free-text generation
  to weave themes — explicitly inject them.

[Check 5 fail] Case study placeholder found OR top-matched client absent.
  Action: Read matched_projects[0] from POSITIONING_OUTPUT.json. Look up its
  project_number in Past_Projects.md. Write the 5-7 sentence condensed case
  study inline. Never emit "[CASE STUDY PLACEHOLDER]".

[Check 6 fail] EXECUTIVE persona content absent.
  Action: Step 1 loads evaluator_messages.EXECUTIVE. Step 3 must inject the
  headline as a callout AND weave key_message + 2 proof_points into the
  "Our Solution Approach" section.

[Check 7 fail] [USER INPUT REQUIRED] for company-profile data: {list}.
  Action: Substitute from company-profile.json:
    - {bid_defaults.authorized_signatory.name/title/email}
    - {primary_location.address/city/state/zip/phone}
    - {company.website}, {company.years_in_business}, {company.employees}
  Only retain markers for procurement officer info, dates, RFP-specific blanks.

[Check 8 fail] Missing certifications: {list of mandatory_ids}.
  Action: Step 4 filter loop must catch ALL certify/attest/represent/declare/
  affirm/warrant keywords. Re-render the Certifications section with the full
  list.

[Check 9 fail] Letter is {N} chars (target 3,000-8,000 for 1-2 pages).
  Action: If >12K, trim non-essential prose; the cover letter must not consume
  >2 pages of the 25-page total. If <2K, expand each NOSE section with the
  context already loaded — no new research needed.

[Check 10 fail] Regression pattern: {pattern type} at line {N}.
  Action: Strip em dashes (replace with --), remove file-name mentions
  (Past_Projects.md, .json), eliminate any [:N] slice on persuasive prose.

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input:**

```
01_SUBMITTAL.md content:
  File size: 2,400 bytes.
  Only "Understanding Your Needs" and "Why Resource Data" headings present.
  No `> **Evaluation Factors Addressed:**` callout in first 2,500 chars.
  Win themes: 1 of 4 themes appears.
  "Past Performance — [CASE STUDY PLACEHOLDER] — strong delivery record"
  "**Email** | [USER INPUT REQUIRED]" (signatory email is in company-profile.json)
  Letter contains "As documented in Past_Projects.md project #7..."
  One em dash present in solution section.
```

Verifier MUST detect: Check 1 (< 3 KB), Check 2 (2 of 4 NOSE elements), Check 3 (no callout), Check 4 (1 of 4 themes — FAIL), Check 5 (placeholder), Check 7 (signatory email is in company-profile), Check 10 (em dash + Past_Projects.md mention). Disposition: FAIL.

**Known-good input:**

```
01_SUBMITTAL.md: 5,800 bytes.
  All 4 NOSE headings present.
  Opening callout: "> **Evaluation Factors Addressed:** Technical Approach (35 pts),
    Past Performance (25 pts), Management (20 pts)"
  4 of 4 win themes referenced with **[Theme Name]** format in Outcomes section.
  "Why Resource Data" cites Mat-Su Borough (top matched_projects entry) with
    20-year partnership detail and quantified result.
  EXECUTIVE headline phrase "Strategic Partnership for Success" appears in
    Solution section.
  [USER INPUT REQUIRED] only on Procurement Officer name and date — no items
    satisfiable by company-profile.json.
  All 3 mandatory certifications (FAR 52.203-2 anti-collusion, debarment,
    drug-free workplace) appear in Certifications section.
  No em dashes; no file-name references; UTF-8 clean.
```

Verifier MUST PASS all checks. Disposition: PASS.

## Hard Rules (inherited from verifier-design principles)

1. NEVER claim 01_SUBMITTAL.md is missing without `Glob` verification first.
2. NEVER flag `[USER INPUT REQUIRED: Procurement Officer name]` or `[USER INPUT REQUIRED: Date]` as a failure — those genuinely require user input that company-profile.json cannot satisfy.
3. NEVER flag a NOSE heading variant — accept "Understanding Your Needs" as the Needs heading; accept "Delivering Outcomes That Matter" as Outcomes; accept "Why Resource Data" as Evidence. The literal words N/O/S/E need not be heading text.
4. NEVER flag a theme as "absent" without grepping both `**[Theme]**` (bracketed) and `**Theme**` (plain bold) formats — the phase template allows either.
5. Every finding must cite line number + first 80 chars of the offending content (or the company-profile.json field path for Check 7).
