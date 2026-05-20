---
name: verifier-phase3h-diagrams-win
expert-role: Diagram Blueprint Verifier
purpose: phase-boundary verifier for phase3h-diagrams-win (MD + JSON existence, six mandatory blueprints, font/contrast/palette discipline, source citations, mermaid validity)
created: 2026-05-20
---

# Verifier — Phase 3h Diagram Blueprints

## When this runs

After phase3h-diagrams-win reports done, BEFORE SVA-3 (Spec Validator gate). Blueprints are consumed by Phase 8d, so any quality miss here costs an entire stage 7 rerun — verify aggressively.

## Inputs (read in this order)

1. `{folder}/outputs/DIAGRAM_BLUEPRINTS.md` — human-readable blueprint document
2. `{folder}/shared/diagram-blueprints.json` — machine-readable blueprint set (consumed by Phase 8d)
3. `{folder}/outputs/ARCHITECTURE.md`, `INTEROPERABILITY.md`, `SECURITY_REQUIREMENTS.md` — citation targets
4. `{folder}/outputs/ENTITY_DEFINITIONS.md` — citation target for data model diagram
5. `{folder}/shared/REQUIREMENT_RISKS.json` — citation source for risk heat map
6. `{folder}/shared/tech-lifecycle-evidence.json` — citation source for tech labels on architecture
7. Any prior SVA-3 report at `{folder}/shared/validation/sva3-spec.json` (if a retry run)

## Verification Checks

### Check 1 — Both files exist, MD ≥ 5 KB, JSON valid

**Criterion:** `outputs/DIAGRAM_BLUEPRINTS.md` exists with size ≥ 5,120 bytes AND `shared/diagram-blueprints.json` exists with size ≥ 500 bytes AND parses as valid JSON AND contains top-level keys: `generated_at`, `palette`, `theme_init_directive`, `blueprints`. All UTF-8 clean (no `�` / `Ã©` / `â€™`).

**Pass:** Both files present, sizes met, JSON valid with all 4 top-level keys.

**Fail:** Either file missing, undersize, JSON parse error, or any top-level key missing.

**Evidence to cite:** File paths + sizes + missing key path(s).

**Hard-rule reminder:** NEVER claim file missing without `Glob` verification first.

---

### Check 2 — All six mandatory diagrams blueprinted

**Criterion:** `diagram-blueprints.json:blueprints` array contains entries with `name` matching ALL of: `architecture` (logical architecture), `integration_sequence` (sequence/integration), `data_model` (ER), `timeline` (Gantt with critical path), `orgchart` (org chart), `risk_heatmap` (risk heat map). Tolerance: 0 — all six must be present by exact name OR by clearly-equivalent name (e.g., `logical_architecture` for `architecture` is acceptable).

**Pass:** All six diagrams blueprinted.

**Fail:** Any of the six missing.

**Evidence to cite:** List of `[bp["name"] for bp in blueprints]` + missing names.

---

### Check 3 — Every blueprint has required schema fields

**Criterion:** For each blueprint, verify all of these keys are present AND non-empty: `name`, `view`, `mermaid`, `source_citation`, `intent`, `caption`, `alt_text`, `render_target`, `render_source`, `accessibility`. Under `accessibility`: `wcag_contrast_ratio`, `colorblind_safe_palette`, `font_size_pt`.

**Pass:** All blueprints pass schema.

**Fail:** Any blueprint missing a required field OR field is null/empty string.

**Evidence to cite:** Blueprint `name` + missing field path.

---

### Check 4 — Font size ≥ 14pt declared per diagram

**Criterion:** For each blueprint's `mermaid` source, verify the `%%{init:` theme directive contains `'fontSize': '14px'` (or higher). Also verify `accessibility.font_size_pt >= 14`. Both must agree.

**Pass:** All blueprints declare font ≥ 14pt in both places.

**Fail:** Any blueprint has font < 14pt OR theme directive missing fontSize declaration.

**Evidence to cite:** Blueprint name + mermaid `init` substring + accessibility.font_size_pt.

---

### Check 5 — WCAG-AA contrast palette declared (Okabe-Ito or equivalent)

**Criterion:** Top-level `palette` field present in JSON with at least 8 named colors AND every blueprint's `accessibility.colorblind_safe_palette` equals `"Okabe-Ito"` or a recognized equivalent (`"Wong"`, `"IBM Design Library"`, `"Color Universal Design"`). AND `accessibility.wcag_contrast_ratio >= 4.5`.

**Pass:** Palette declared + every blueprint cites it + contrast ≥ 4.5.

**Fail:** Palette missing OR any blueprint cites unrecognized palette OR contrast < 4.5.

**Evidence to cite:** Top-level palette keys + per-blueprint palette name + contrast ratio.

---

### Check 6 — Descriptive titles (system + view, not just type)

**Criterion:** For each blueprint, `view` field must contain BOTH a system/project reference AND a view descriptor. Reject generic titles like `"Architecture"`, `"Sequence Diagram"`, `"Gantt"`. Accept titles like `"MARS Hosted SaaS — Logical Architecture (Component View)"`, `"Citizen Licensing — Sequence"`, `"Implementation Timeline — Gantt with Critical Path"`. Heuristic: `view` must contain at least one em-dash (`—`) or colon (`:`) separator AND length ≥ 20 chars.

**Pass:** All blueprints have descriptive titles.

**Concern:** Title meets length but lacks separator (advisory).

**Fail:** Any title < 20 chars or matches a generic pattern.

**Evidence to cite:** Blueprint name + view string.

---

### Check 7 — Source citations reference real files

**Criterion:** For each blueprint, `source_citation` must reference at least one filename matching pattern `[A-Z_][A-Z0-9_]*\.(md|json)`. For each cited filename, verify it exists in either `{folder}/outputs/` or `{folder}/shared/` OR is on the `EXPECTED_LATER` allowlist {`02_MANAGEMENT.md`, `UNIFIED_RTM.json`, `EFFORT_ESTIMATION.md`} (these are produced after Phase 3h).

**Pass:** Every citation either resolves on disk or is in the EXPECTED_LATER allowlist.

**Concern:** Cited file is EXPECTED_LATER (Phase 8d will re-check) — log advisory.

**Fail:** Cited file is neither on disk nor in the EXPECTED_LATER allowlist (fabricated citation).

**Evidence to cite:** Blueprint name + citation string + missing filename + disposition.

---

### Check 8 — Mermaid sources are syntactically valid

**Criterion:** For each blueprint, `mermaid` source must pass `mmdc --validate` (mermaid-cli parse-only check). If the phase ran the validation step (Step 4) and recorded results, read them; otherwise re-run validation here using `npx @mermaid-js/mermaid-cli -i <tmpfile> --validate`.

**Pass:** All blueprints parse cleanly.

**Fail:** Any blueprint fails mermaid parse.

**Evidence to cite:** Blueprint name + mmdc stderr first 200 chars.

---

### Check 9 — Gantt timeline has critical path tasks flagged

**Criterion:** The `timeline` blueprint's `mermaid` source must contain at least one `:crit,` (or `crit ` keyword) on a task line. This confirms the gantt diagram explicitly marks critical-path tasks.

**Pass:** ≥ 1 `crit` keyword in timeline source.

**Fail:** No `crit` keyword in timeline source — critical path not flagged.

**Evidence to cite:** grep count of `crit` in timeline mermaid block.

---

### Check 10 — Persuasive captions + screen-reader alt-text on every diagram

**Criterion:** For each blueprint, `caption` length ≥ 40 chars AND contains a persuasive verb/phrase (`ensures`, `accelerates`, `reduces`, `protects`, `enables`, `delivers`, `demonstrates`, `clarifies`, `our`, `we have`). AND `alt_text` length ≥ 50 chars (descriptive enough for a screen reader).

**Pass:** All blueprints have persuasive captions AND descriptive alt-text.

**Concern:** Caption meets length but no persuasive verb (advisory — descriptive captions are weaker but acceptable).

**Fail:** Caption < 40 chars OR alt_text < 50 chars.

**Evidence to cite:** Blueprint name + caption length + alt_text length + verb match.

---

### Check 11 — No truncation artifacts, no row-cap notices

**Criterion:** Grep DIAGRAM_BLUEPRINTS.md for `_Showing N of M_`, mid-word ellipsis, `[:N]` slice notation. NOTE: Phase 3h does use `[:40]` on risk titles inside the heat-map mermaid label generation (line `title = (r.get("title") or r.get("name") or rid).replace('"', "'")[:40]`) — this is acceptable inside mermaid labels (mermaid breaks on long labels), but flag if it appears in the persuasive `caption` or `alt_text` fields.

**Pass:** Zero hits in deliverable prose fields (caption / alt_text / intent / source_citation).

**Fail:** Any hit in deliverable prose fields.

**Evidence to cite:** Pattern + blueprint name + field name + snippet.

---

## Disposition Logic

- **PASS:** Checks 1–5, 7–11 pass AND Check 6 not in FAIL band.
- **CONCERN:** Check 6 / 7 / 10 in advisory band only. Log + continue to SVA-3.
- **FAIL:** Any of Checks 1–5, 8, 9, 11 fail OR Check 6 / 7 / 10 below FAIL threshold.

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase3h-diagrams-win with these targeted corrections:

[Check 1 fail] DIAGRAM_BLUEPRINTS.md or diagram-blueprints.json missing/invalid.
  Action: Re-run Step 6 (write JSON) AND Step 7 (write MD). Confirm both writes complete.

[Check 2 fail] Missing mandatory diagram(s): {list}.
  Action: Re-run Step 3 — confirm blueprints.append() called for all six diagrams
  (architecture, integration_sequence, data_model, timeline, orgchart, risk_heatmap).

[Check 3 fail] Blueprint {name} missing field: {field_path}.
  Action: Re-run make_blueprint helper — confirm all required kwargs are passed
  (name, view, mermaid_src, source_citation, intent, caption, alt_text).

[Check 4 fail] Blueprint {name} has font < 14pt.
  Action: Re-run Step 2 THEME_INIT directive — ensure 'fontSize': '14px' is included
  AND make_blueprint sets accessibility.font_size_pt = 14.

[Check 5 fail] Palette or contrast issue.
  Action: Confirm Okabe-Ito PALETTE dict is the source for all classDef colors and
  accessibility.colorblind_safe_palette = "Okabe-Ito" + wcag_contrast_ratio = 4.5.

[Check 6 fail] Blueprint {name} has generic view title: "{title}".
  Action: Edit the view string to include both the system/project name AND the
  diagram type (e.g., "MARS — Risk Heat Map by Severity").

[Check 7 fail] Blueprint {name} cites non-existent file: {filename}.
  Action: Either produce the cited file before Phase 3h OR add to EXPECTED_LATER
  allowlist OR change the citation to a file that exists.

[Check 8 fail] Mermaid parse failure: {name} — {stderr}.
  Action: Fix mermaid syntax. Re-run Step 4 mmdc --validate to confirm before continuing.

[Check 9 fail] Timeline missing `crit` keyword.
  Action: Re-emit gantt_mermaid with at least one `:crit,` task on the critical path
  (e.g., Architecture sign-off, Core platform, Cutover).

[Check 10 fail] Blueprint {name}: caption / alt_text too short.
  Action: Re-emit make_blueprint with caption ≥ 40 chars (persuasive verb)
  AND alt_text ≥ 50 chars (screen-reader-descriptive).

[Check 11 fail] Truncation artifacts in deliverable prose: {pattern + blueprint + field}.
  Action: Remove [:N] slicing from caption / alt_text / intent / source_citation fields.
  Mermaid label slicing inside heat-map nodes is acceptable; prose slicing is not.

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input:**

```
DIAGRAM_BLUEPRINTS.md is 3 KB.
diagram-blueprints.json has 4 blueprints: architecture, integration_sequence,
  data_model, timeline (orgchart and risk_heatmap missing).
architecture blueprint's accessibility.font_size_pt = 11.
timeline mermaid source has no `:crit,` task markers.
data_model blueprint's caption = "ER diagram" (10 chars, no persuasive verb).
architecture blueprint cites "FOOBAR.md" (does not exist on disk, not in allowlist).
```

Verifier MUST detect: Check 1 (under 5KB), Check 2 (orgchart + risk_heatmap missing), Check 4 (font 11 < 14), Check 6 (caption 10 chars), Check 7 (FOOBAR.md fabricated), Check 9 (no crit in timeline), Check 10 (caption < 40 chars). Disposition: FAIL.

**Known-good input:**

```
DIAGRAM_BLUEPRINTS.md is 11 KB.
diagram-blueprints.json has all 6 mandatory blueprints.
Every blueprint has font_size_pt = 14, palette = "Okabe-Ito", wcag_contrast_ratio = 4.5.
Every view title has em-dash + ≥ 20 chars.
Every source_citation resolves on disk or is in EXPECTED_LATER allowlist.
All 6 mermaid sources pass mmdc --validate.
Timeline has 4 `:crit,` tasks (Architecture sign-off, Core platform, Tyler integration, Cutover).
All captions ≥ 40 chars with persuasive verbs.
All alt_texts ≥ 50 chars and descriptive.
```

Verifier MUST PASS all checks. Disposition: PASS.

## Hard Rules (inherited from verifier-design principles)

1. NEVER claim DIAGRAM_BLUEPRINTS.md or diagram-blueprints.json is missing without `Glob` verification first.
2. NEVER flag the `[:40]` label-truncation inside heat-map mermaid node labels as a violation — mermaid label sizing is a rendering concern, not a deliverable-content concern. Only flag `[:N]` in caption / alt_text / intent / source_citation prose fields (Check 11 anti-false-positive).
3. NEVER flag a `source_citation` referencing `02_MANAGEMENT.md`, `UNIFIED_RTM.json`, or `EFFORT_ESTIMATION.md` as fabricated — these are produced after Phase 3h, so the citation is forward-looking (Check 7 EXPECTED_LATER allowlist).
4. Every finding must cite specific blueprint name + field path + value (e.g., `blueprints[3].name = "data_model"` + `caption = "ER diagram" (10 chars, expected ≥ 40)`).
5. On FAIL, return corrective instructions naming the specific blueprint name / field / threshold so the phase agent can target the repair surgically without re-running upstream phases.
