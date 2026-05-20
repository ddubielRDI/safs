---
name: verifier-phase8d-diagrams-win
expert-role: Diagram Embed Verifier
purpose: phase-boundary verifier for phase8d-diagrams-win (render completeness, embed presence, no orphans, file-size sanity, SVG-preferred check)
created: 2026-05-19
---

# Verifier — Phase 8d Diagram Rendering

## When this runs

After phase8d-diagrams-win reports done, BEFORE phase8e (PDF assembly) runs. This verifier exists specifically to catch the "diagrams render but never get embedded" bug that produced zero-diagram PDFs in MARS.

## Inputs (read in this order)

1. `{folder}/outputs/bid/figure-registry.json` — authoritative list of expected diagrams
2. `{folder}/outputs/bid/` directory listing (via `Glob("outputs/bid/*.png")` and `Glob("outputs/bid/*.svg")`) — what actually rendered
3. `{folder}/outputs/bid-sections/*.md` — all bid section markdown files (embed presence scan)
4. `{folder}/outputs/bid/*.mmd` — source blueprints (count: expected diagrams)

## Verification Checks

### Check 1 — Every figure in figure-registry.json has a rendered file

**Criterion:** For each entry in `figure-registry["figures"]`, check that the file referenced by `figures[N]["file"]` (and `figures[N]["file_svg"]` if present) exists on disk in `{folder}/outputs/bid/`.

**Pass:** All registry-listed files exist on disk.

**Fail:** Any registry-listed file is absent on disk. This means a blueprint was registered but rendering failed silently.

**Evidence to cite:** `figure-registry.json:figures[N].file = "{filename}"` + Glob result showing file absent.

**Hard-rule reminder:** NEVER claim a file is missing without `Glob("{folder}/outputs/bid/{filename}")` verification first.

---

### Check 2 — Every rendered PNG/SVG has at least one `![]()` embed in a bid section

**Criterion:** For each PNG and SVG file found in `{folder}/outputs/bid/`, search all `outputs/bid-sections/*.md` files for a markdown image reference containing that filename (e.g., `architecture.png` or `architecture.svg`). The reference may use relative path (`../bid/architecture.png`) or base name only.

**Pass:** Every rendered diagram file has at least one `![...](.../filename.ext)` or `![...](filename.ext)` reference in at least one bid section markdown file.

**Fail:** Any rendered diagram file has ZERO references in any bid section markdown. This is the orphaned-diagram bug that produced zero-diagram PDFs in MARS (2026-05-19).

**Evidence to cite:** `outputs/bid/{filename}` + "referenced in: [none]". List which bid sections were scanned.

---

### Check 3 — No orphaned PNGs or SVGs (rendered but not in figure-registry.json)

**Criterion:** For each PNG/SVG in `outputs/bid/`, verify it appears in `figure-registry["figures"]` (via `file` or `file_svg` field). Exclude non-diagram files (any file whose basename does not end in `.png` or `.svg`, and exclude `*.min.js`, `*.css`, etc.).

**Pass:** All rendered diagram files are registered in figure-registry.json.

**Concern (not Fail):** An unregistered PNG/SVG exists. This may be a debug render or interim artifact. Log as advisory — it won't break the PDF but should be cleaned up.

**Evidence to cite:** `outputs/bid/{filename}` not found in any `figure-registry.figures[*].file` or `.file_svg`.

---

### Check 4 — PNG file size sanity (not blank, not uncompressed bloat)

**Criterion:** For each PNG in `outputs/bid/`:
- `file_size > 5,120 bytes` (5 KB) — catches blank/failed renders (a blank white 1920×1080 PNG is ~12 KB, so < 5 KB means truly broken)
- `file_size < 1,048,576 bytes` (1 MB) — catches uncompressed bloat from `--scale 2` at 4K+

**Pass:** All PNGs are between 5 KB and 1 MB.

**Fail:** Any PNG is < 5 KB (blank/failed render) OR > 1 MB (bloat — will inflate PDF past portal limits).

**Evidence to cite:** `outputs/bid/{filename}: {size_bytes} bytes` + threshold that was violated.

---

### Check 5 — SVG preferred over PNG when both exist (file-size rationale)

**Criterion:** For each diagram where BOTH `{name}.svg` and `{name}.png` exist on disk, verify that the bid section embed references the SVG version (not the PNG). Per phase8d discipline: SVG is the primary embed format because fitz.Story compresses it to ~30 KB vs 15–27 MB for uncompressed PNG.

**Pass:** When SVG exists, the bid section markdown references the SVG (`.svg` extension in `![...](...)` path), not the PNG.

**Concern (not Fail):** Embed references PNG when SVG exists. Log as advisory with a note that this may cause PDF size bloat per the SVG-preferred discipline. No hard block — SVG preference is a quality optimization, not a correctness gate.

**Evidence to cite:** `outputs/bid/{name}.svg` exists BUT `bid-sections/{file}.md:line N` references `{name}.png` instead.

---

### Check 6 — SVG file size sanity

**Criterion:** For each SVG in `outputs/bid/`: `file_size < 307,200 bytes` (300 KB). Per phase8d SVG discipline, SVGs should be < 300 KB on disk. An SVG larger than 300 KB likely contains embedded raster data or is a malformed export.

**Pass:** All SVGs are < 300 KB.

**Concern:** Any SVG between 200 KB and 300 KB. Log as advisory.

**Fail:** Any SVG > 300 KB. Report and recommend re-rendering without embedded rasters.

**Evidence to cite:** `outputs/bid/{filename}.svg: {size_bytes} bytes`.

---

### Check 7 — figure-registry.json has no unsubstituted [GENERATE:] markers

**Criterion:** Scan all `caption` and `alt_text` fields in `figure-registry["figures"]` for the literal string `[GENERATE:`. Zero occurrences required. This marker indicates the phase agent left template placeholders instead of writing actual persuasive captions.

**Pass:** Zero `[GENERATE:` strings found in any figure's caption or alt_text.

**Fail:** One or more figures have unsubstituted markers.

**Evidence to cite:** `figure-registry.json:figures[N].file = "{filename}"` + `caption = "[GENERATE: ...]"` (first 80 chars quoted).

---

## Disposition Logic

- **PASS:** Checks 1, 2, 4, 7 pass (Checks 3, 5, 6 are CONCERN-level only).
- **CONCERN:** Any of Checks 3, 5, 6 trigger. Log advisory + continue to phase8e.
- **FAIL:** Any of Checks 1, 2, 4, 7 fail. Block phase8e and re-dispatch phase8d.

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase8d-diagrams with the following targeted corrections:

[Check 1 fail] Registered diagram files not found on disk: {list}.
  Action: Re-run rendering for these specific diagrams. Verify the .mmd source file exists first
  at {folder}/outputs/bid/{name}.mmd. If .mmd missing, check phase3h-diagrams-win outputs.

[Check 2 fail] Rendered diagrams with no embed in any bid section: {list}.
  Action: CRITICAL — this is the orphaned-diagram bug. For each orphaned diagram:
    - Locate its section_ref from figure-registry.json (e.g., "03_TECHNICAL.md")
    - Add the embed at the prescribed location per phase8d standard placement map:
      architecture.png → 03_TECHNICAL.md §3.1 Architecture Overview
      timeline.png → 02_MANAGEMENT.md before §6. Transition Plan
      orgchart.png → 02_MANAGEMENT.md after §3.1 Staffing Plan
      data_model.png → 03_TECHNICAL.md §3.3 Data Architecture
      integration_sequence.png → 06_INTEGRATION.md §2.1
      risk_heatmap.png → 04_RISK_REGISTER.md §Executive Summary
    - Syntax: ![Alt text describing diagram](../bid/{filename})
    - Include Figure N. <persuasive caption> ABOVE the image.

[Check 4 fail - blank] PNG < 5 KB (blank render): {list}.
  Action: Re-run mmdc for these diagrams. Verify .mmd source is not empty. Check npx available.

[Check 4 fail - bloat] PNG > 1 MB (uncompressed bloat): {list}.
  Action: Prefer SVG render for PDF embedding. If PNG required, reduce --scale to 1 and -w to 1280.

[Check 7 fail] Unsubstituted [GENERATE:] markers in figure-registry: {list of filenames}.
  Action: For each figure, analyze its .mmd source content + win themes from bid-context-bundle.json
  and write a real persuasive action caption. Replace all [GENERATE:] markers with actual text.

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input:**

```
figure-registry.json: 3 figures (architecture.png, timeline.png, orgchart.png).
outputs/bid/: architecture.png (350 KB), timeline.png (280 KB). orgchart.png ABSENT.
outputs/bid-sections/ scan: 0 references to architecture.png, timeline.png, or orgchart.png in any .md file.
figure-registry captions: all contain "[GENERATE:" markers.
```

Verifier MUST detect: Check 1 (orgchart.png absent), Check 2 (architecture.png and timeline.png orphaned — 0 embeds), Check 7 (unsubstituted markers). Disposition: FAIL.

**Known-good input:**

```
figure-registry.json: 3 figures registered.
outputs/bid/: architecture.svg (45 KB), architecture.png (380 KB),
              timeline.svg (38 KB), timeline.png (290 KB),
              orgchart.svg (22 KB), orgchart.png (180 KB).
outputs/bid-sections/03_TECHNICAL.md: contains "![System architecture...](../bid/architecture.svg)" at line 84.
outputs/bid-sections/02_MANAGEMENT.md: contains "![Project timeline...](../bid/timeline.svg)" at line 156,
                                        "![Org chart...](../bid/orgchart.svg)" at line 72.
figure-registry.json: all captions are real persuasive text (no [GENERATE: markers]).
```

Verifier MUST PASS Checks 1, 2, 4, 7. Checks 3 (no orphans), 5 (SVG preferred — confirmed), 6 (all SVGs < 300 KB). Disposition: PASS.

## Hard Rules (inherited from verifier-design principles)

1. NEVER claim a PNG or SVG is missing without a `Glob("{folder}/outputs/bid/{filename}")` call confirming absence.
2. NEVER report a line-wrapped image path across two lines as a missing embed — search for the filename token, not the full path syntax, when scanning bid sections.
3. NEVER flag a file as an orphan in Check 3 without verifying it does not appear in `figure-registry.figures[*].file` AND `figure-registry.figures[*].file_svg` (both fields).
4. Every finding must cite the specific file path + either the line number (for embed presence checks) or file size in bytes (for size checks).
5. On FAIL for Check 2 (orphaned embed), the corrective instructions must name the exact target bid section and section heading per the phase8d standard placement map — no vague "add the embed somewhere".
