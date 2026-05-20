---
name: verifier-phase8.2-management-win
expert-role: Management Volume Quality Verifier
purpose: phase-boundary verifier for phase8.2-management-win (case-study auto-population, eval factor callouts, CVD theme threading, 4-persona coverage, OCM presence, page-budget)
created: 2026-05-20
---

# Verifier — Phase 8.2 Management Proposal

## When this runs

After phase8.2-management-win reports done, BEFORE Phase 8.3 (Technical Approach) cross-references management content for team/staffing alignment.

## Inputs (read in this order)

1. `{folder}/outputs/bid-sections/02_MANAGEMENT.md` — primary output under verification
2. `{folder}/shared/bid/POSITIONING_OUTPUT.json` — for win_themes, matched_projects, theme_eval_mapping, section_theme_mandates
3. `{folder}/shared/EVALUATION_CRITERIA.json` — for management-related eval factor weights
4. `{folder}/shared/COMPLIANCE_MATRIX.json` — to detect OCM/training mandates
5. `Past_Projects.md` — to verify case-study client names/details match source
6. `config-win/company-profile.json` — for key_personnel and locations cross-reference

## Verification Checks

### Check 1 — File exists and meets minimum size

**Criterion:** `outputs/bid-sections/02_MANAGEMENT.md` exists AND file size >= 8,192 bytes (8 KB minimum — phase target is 10 KB; anything under 8 KB indicates missing sections).

**Pass:** File exists, size >= 8,192 bytes.

**Fail:** File absent OR size < 8,192 bytes.

**Evidence to cite:** File path + actual size in bytes.

**Hard-rule reminder:** NEVER claim file is missing without `Glob` verification first.

---

### Check 2 — Six major sections present

**Criterion:** All six `## N.` major section headings appear: Company Overview, Project Management Approach, Proposed Team, Organizational Change Management, Experience and Past Performance, Transition Plan (literal heading text may vary slightly — match on keyword in heading).

**Pass:** All 6 major section headings detected.

**Concern:** 5 of 6 (one section missing or merged).

**Fail:** 4 or fewer major sections present.

**Evidence to cite:** Grep `^## ` and list the headings found.

---

### Check 3 — Evaluation factor callout at every major section

**Criterion:** Grep `^> \*\*Evaluation Factor:\*\*` (or `**Evaluation Factors Addressed:**`) returns >= 6 hits (one per major section).

**Pass:** >= 6 eval factor callouts.

**Concern:** 4-5 callouts — log advisory.

**Fail:** < 4 callouts.

**Evidence to cite:** Grep count and the first 80 chars of each matched line.

---

### Check 4 — 3-5 real case studies auto-populated from Past_Projects.md

**Criterion:** Within Section 5 (Experience and Past Performance), count case studies. Each must have: a **Case Study** heading/marker, client name, project duration, Challenge section, Solution section, Results table, Relevance statement. Required count: >= 3, max 5.

Cross-check: client names cited in case studies must match clients from `matched_projects[0..4]` in POSITIONING_OUTPUT.json. At least the top-ranked matched_projects entry's client must appear as a case study client.

**Pass:** 3-5 case studies, each with all 7 elements, all clients trace to matched_projects.

**Concern:** Exactly 3 case studies but one is missing the Results table — log advisory.

**Fail:** < 3 case studies OR any case study uses fabricated client name (not in matched_projects).

**Evidence to cite:** Count of case studies; for each, list which of the 7 elements are present; flag any client name not matching matched_projects.

---

### Check 5 — `[CASE STUDY PLACEHOLDER]` markers absent

**Criterion:** Zero occurrences of `[CASE STUDY PLACEHOLDER]`, `[PROJECT PLACEHOLDER]`, or `[CLIENT PLACEHOLDER]` anywhere in the file.

**Pass:** Zero placeholder markers.

**Fail:** Any placeholder marker present.

**Evidence to cite:** Line number + quoted marker context.

---

### Check 6 — >= 2 themes per major section in CVD format

**Criterion:** Load `win_themes`. Split the document by `^## ` major-section boundaries. For each of the 6 major sections, count the number of distinct theme names that appear as `**[Theme Name]**` (or `**Theme Name**` plain bold) within that section's content range. Threshold: >= 2 per section.

Additionally, theme references should appear in CVD (Claim-Vivid Detail) format — a bold theme marker followed by a colon and a concrete evidence statement (not a generic claim). At least 50% of theme references must have evidence text within the same paragraph.

**Pass:** All 6 sections have >= 2 themes AND >= 50% are in CVD format.

**Concern:** 4-5 sections meet the bar; 1-2 are thin.

**Fail:** 3 or more sections have < 2 themes.

**Evidence to cite:** Section-by-section table: section name | theme count | theme names found.

---

### Check 7 — All 4 personas referenced (EXECUTIVE, RISK, TECHNICAL, OPERATIONAL)

**Criterion:** Document must show evidence of all 4 evaluator personas being addressed:
- EXECUTIVE: Section 1 (Company Overview) — partnership, strategic, long-term language OR EXECUTIVE headline match
- RISK: Section 2 (PM Approach) — governance, risk management, escalation language OR RISK headline match
- TECHNICAL: Section 3 (Proposed Team) — qualifications, certifications, technical depth language OR TECHNICAL headline match
- OPERATIONAL: Section 4 (OCM) — user-centered, training, adoption language OR OPERATIONAL headline match

For each persona, search the relevant section for keywords OR for a >= 3-word match against the persona's headline from POSITIONING_OUTPUT.json.evaluator_messages.

**Pass:** All 4 personas detectable in their assigned sections.

**Concern:** 3 of 4 — log advisory.

**Fail:** 2 or fewer personas detectable.

**Evidence to cite:** For each persona: section name + matched keyword or headline phrase OR "no match".

---

### Check 8 — OCM section addresses RFP OCM/training demands (if present)

**Criterion:** Scan COMPLIANCE_MATRIX.json `mandatory_items` for entries whose text contains keywords: `training`, `change management`, `OCM`, `knowledge transfer`, `transition`, `user adoption`. If at least one such mandate exists, Section 4 (OCM) must address it — its content must reference at least one of: stakeholder analysis, training plan, knowledge transfer plan.

**Pass:** Either no OCM mandates exist (vacuous PASS) OR Section 4 covers each detected OCM theme.

**Concern:** OCM mandates exist; Section 4 covers some but not all.

**Fail:** OCM mandates exist; Section 4 missing OR has < 500 chars OR has generic boilerplate with no specific OCM coverage.

**Evidence to cite:** List OCM-related mandatory item IDs; for each, report whether Section 4 addresses it (grep result).

---

### Check 9 — Page-budget awareness (MARS 25-page cap)

**Criterion:** Estimate pages by char/3,000. Management section target: 4-7 pages (12,000-21,000 chars). Within a 25-page total, this volume can claim a meaningful share but must not crowd Technical (Phase 8.3).

**Pass:** 12,000-21,000 chars (4-7 pages).

**Concern:** 8,000-12,000 (thin) OR 21,000-27,000 (heavy).

**Fail:** > 27,000 chars (> 9 pages — consumes > 36% of 25-page budget). OR < 8,000 chars.

**Evidence to cite:** Actual char count, estimated page count.

---

### Check 10 — Universal regression patterns

**Criterion:** Five sub-checks:
(a) UTF-8 decode clean; no mojibake (`Ã`, `â€`, `Â `, `�`).
(b) Zero `_Showing \d+ of \d+_` row-cap notices.
(c) Zero `[:N]` truncation patterns visible in deliverable strings.
(d) Zero em-dash chars (`—` U+2014).
(e) Zero internal file references (`.json`, `.md`, `Past_Projects`, `POSITIONING_OUTPUT`).

**Pass:** All five sub-checks pass.

**Fail:** Any sub-check has 1+ violation.

**Evidence to cite:** Per violation: line + first 80 chars quoted.

---

## Disposition Logic

- **PASS:** All 10 checks pass.
- **CONCERN:** Checks 2, 3, 6, 7, 8, or 9 in advisory band; all others pass.
- **FAIL:** Any of Checks 1, 4, 5, 10 fail OR Checks 2/3/6/7/8/9 fall below FAIL threshold.

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase8.2-management with the following targeted corrections:

[Check 1 fail] 02_MANAGEMENT.md missing or < 8 KB ({actual_size} bytes).
  Action: Re-run phase. Confirm positioning + compliance + company files loaded;
  matched_projects has 3+ entries (Phase 8.0 dependency).

[Check 2 fail] Missing major section(s): {list}.
  Action: Section template in Step 2 must render all 6 ## sections. Confirm
  the AI didn't truncate after Section 4 or merge Section 5 into Section 1.

[Check 3 fail] Only {N} eval factor callouts (need 6).
  Action: Every ## section must open with `> **Evaluation Factor:** ...` callout.
  Step 2 instructions are explicit — inject callouts during section authoring,
  not as a post-hoc pass.

[Check 4 fail] Only {N} case studies (need 3+) OR fabricated client name "{name}".
  Action: Iterate matched_projects[:5]. For each, look up project_number in
  Past_Projects.md and render the full Challenge/Solution/Results template.
  Never use a client name not in matched_projects.

[Check 5 fail] Placeholder marker found at line {N}.
  Action: Remove every [CASE STUDY PLACEHOLDER] / [PROJECT PLACEHOLDER]. Replace
  with the corresponding matched_projects case study or omit the section if
  matched_projects has fewer than 3 entries (note "additional references
  available upon request").

[Check 6 fail] Sections with < 2 themes: {list}.
  Action: Each major section must reference >= 2 themes from win_themes in
  **[Theme Name]**: <evidence> CVD format. The Step 3 theme-coverage loop is
  not optional — verify before writing.

[Check 7 fail] Personas missing: {list}.
  Action: Section-to-persona mapping (from Step 3 instructions):
    Sec 1 -> EXECUTIVE, Sec 2 -> RISK, Sec 3 -> TECHNICAL, Sec 4 -> OPERATIONAL.
  Inject each persona's headline + key_message + 1+ proof_point from
  evaluator_messages into its assigned section.

[Check 8 fail] OCM mandates {list} not addressed in Section 4.
  Action: Section 4 must enumerate: (1) OCM methodology, (2) training plan
  with admin/end-user/train-the-trainer breakdown, (3) knowledge transfer plan.
  Each detected OCM mandate must map to one of these subsections.

[Check 9 fail] Management volume is {N} chars ({pages} pages).
  Action: For >27K chars: trim Section 6 (Transition) and Section 1.3 (Relevant
  Capabilities) — these are duplicative with other volumes. For <8K: each
  section needs more depth; do not cut sections, expand them.

[Check 10 fail] Regression pattern {type} at line {N}.
  Action: Strip em dashes, file references, [:N] slices.

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input:**

```
02_MANAGEMENT.md: 6,200 bytes.
  Sections present: Company Overview, PM Approach, Past Performance, Transition (4 of 6 — missing Team and OCM).
  Eval factor callouts: 2 (in Sec 1 and Sec 2 only).
  Case studies: 2, both reference real matched_projects clients.
  "Section 4: [CASE STUDY PLACEHOLDER]" line present.
  EXECUTIVE persona language in Sec 1; no RISK/TECHNICAL/OPERATIONAL detectable.
  COMPLIANCE_MATRIX has 3 training-related mandatory items; no OCM section to address them.
```

Verifier MUST detect: Check 1 (< 8 KB), Check 2 (4 of 6), Check 3 (2 of 6 callouts), Check 4 (only 2 case studies), Check 5 (placeholder), Check 7 (3 of 4 personas missing — FAIL), Check 8 (OCM mandates unaddressed). Disposition: FAIL.

**Known-good input:**

```
02_MANAGEMENT.md: 14,800 bytes.
  All 6 major sections present with eval factor callouts at each.
  Case studies: 4 (Mat-Su Borough, Oregon OLDC, Alaska DMV, Idaho DEQ) — all from
    matched_projects, all 7 elements present, Results tables populated.
  No [CASE STUDY PLACEHOLDER] markers.
  6 of 6 sections have >= 2 themes; 80% in CVD format with evidence statements.
  All 4 personas detectable in their assigned sections (EXECUTIVE in Sec 1,
    RISK in Sec 2, TECHNICAL in Sec 3, OPERATIONAL in Sec 4).
  OCM section (Sec 4) addresses all 3 training-related mandatory items.
  14.8 KB = ~5 pages, within 4-7 page budget.
  UTF-8 clean, no em dashes, no file references.
```

Verifier MUST PASS all checks. Disposition: PASS.

## Hard Rules (inherited from verifier-design principles)

1. NEVER claim 02_MANAGEMENT.md is missing without `Glob` verification first.
2. NEVER flag a case study as "fabricated client" without confirming the client name is genuinely absent from matched_projects AND from `config-win/known-clients.json` (or equivalent list of pre-approved client name references).
3. NEVER fail Check 8 (OCM coverage) when the RFP itself has zero OCM mandatory items — vacuous pass is correct.
4. NEVER flag heading text strictly — "Organizational Change Management" and "OCM Approach" both satisfy Section 4 detection; "Project Management Approach" and "Project Management" both satisfy Section 2.
5. Every finding must cite line number + first 80 chars of offending content (or section name + theme count for Check 6).
