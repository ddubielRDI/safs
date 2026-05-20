---
name: phase8d-diagrams-win
expert-role: Visual Design Engineer
domain-expertise: Mermaid, diagrams, visual communication
---

# Phase 8d: Diagram Rendering

## Expert Role

You are a **Visual Design Engineer** with deep expertise in:
- Mermaid diagram rendering
- Visual communication
- Diagram optimization
- Image quality management

## Purpose

Render all Mermaid diagrams (.mmd) to PNG images for PDF inclusion.

## Diagram Quality Criteria (MANDATORY)

Every diagram produced by this phase MUST be evaluator-readable. A diagram that renders successfully but doesn't communicate the architecture is a failure. NEW-V4-F12 added 2026-05-18 per user priority #3.

### Visual Standards
- **Font size:** Minimum 14pt for text in diagrams meant to be printed on Letter paper at 1:1 scale. Verify by checking that node labels are legible at 50% zoom.
- **Contrast:** WCAG 2.2 AA minimum (4.5:1 for text, 3:1 for graphical elements against background). Avoid pale-on-white or red-on-red combinations.
- **Color choices:** Use a palette that is colorblind-safe (Okabe-Ito or Wong palette). Do NOT rely on color alone to convey meaning — include text labels.
- **Aspect ratio:** Target 4:3 or 16:9 for landscape diagrams; 8.5:11 for portrait. Avoid extreme wide diagrams that get scaled down too small in PDF.

### Communication Standards
- **Title:** Every diagram has a descriptive title that names the system AND the view (e.g., "MARS Hosted SaaS — Logical Architecture (Component View)" not just "Architecture").
- **Legend:** If symbols/colors carry meaning, include a legend. No assumed conventions.
- **Labels:** Every node has a non-trivial label. "Module A / Module B" is unacceptable — name the actual component.
- **Annotations:** Highlight 1-3 key flows or decisions with callout boxes (e.g., "Tyler/NIC Oregon integration — server-redirect pattern per ADR-007").
- **Source citation:** Bottom-right corner watermark with "Source: ARCHITECTURE.md / ADR-{N} / {date}".

### Mermaid-Specific
- Use `classDef` to apply consistent styling per component category (data, service, gateway, external system).
- For sequence diagrams, label both message lines AND lifelines.
- For flowcharts, use decision diamonds with explicit yes/no branch labels — no orphan arrows.
- For Gantt charts, ensure critical path is highlighted in a distinct color and key milestones are labeled.

### Verification Step
After rendering each diagram:
1. Save a PNG at the target print resolution (300 DPI for Letter paper = 2550×3300 px).
2. Verify font legibility at 100% scale.
3. If any text is illegible, increase font_size and re-render — do not ship illegible diagrams.

### Diagram Embedding Discipline (MANDATORY — added 2026-05-19)

**Rendering a PNG is NOT enough.** The phase that produces the bid section
markdown (currently phase8.2, 8.3, 8.4, 8.4k, 8.6) MUST embed each rendered
diagram into the relevant bid section .md file before phase8e runs.

**Standard placement map** (binding contract between phase8d and the
authoring phases):

| Diagram | Embed in | Section |
|---------|----------|---------|
| `orgchart.png` | `02_MANAGEMENT.md` | after § "3.1 Staffing Plan" |
| `timeline.png` | `02_MANAGEMENT.md` | before § "6. Transition Plan" |
| `architecture.png` | `03_TECHNICAL.md` | inside § "3.1 Architecture Overview" |
| `data_model.png` | `03_TECHNICAL.md` | inside § "3.3 Data Architecture" |
| `integration_sequence.png` | `06_INTEGRATION.md` | inside § "2.1 Tyler / NIC Oregon Integration" |
| `risk_heatmap.png` | `04_RISK_REGISTER.md` | in § "Executive Summary" (Figure A) |

**Markdown syntax** (bid sections live in `outputs/bid-sections/`):

```markdown
**Figure N. <Title>** -- <one-sentence persuasive caption tying back to a
win theme; describes what the diagram ENABLES or PROVES>.

![Alt text describing the diagram contents for screen readers and a11y](../bid/<diagram>.png)
```

Each embed has THREE required elements:
1. A bold "Figure N. <Title>" caption ABOVE the image, with a persuasive
   action-caption sentence per the guidelines below.
2. The `![]()` markdown image syntax with a real descriptive alt-text
   (not "diagram" or "architecture diagram" — name the components and
   what the viewer learns from it).
3. A blank line after for proper markdown rendering.

**Phase8e (PDF assembly) caveats — TWO failure modes:**

1. **Path resolution.** The rendering engine (fitz.Story via markdown_pdf)
   cannot resolve `..` in image src paths. Phase8e MUST rewrite `../bid/foo.png`
   to `./foo.png` AND set `Section(root=outputs/bid)` so the PNG resolves.
   Without this, the PDF ships with broken image links and zero embedded images.

2. **Page-DPI downsample (BLOCKING — added 2026-05-20).** fitz.Story embeds
   raster images at the layout-box pixel DPI (~96 DPI = page size 816x1056
   for Letter). When the source PNG is much larger (e.g., 3052x1766 from
   `mmdc --scale 2 -w 1920`), fitz.Story DOWNSAMPLES it to ~816x1056 before
   embedding. The downsample destroys text antialiasing: small label glyphs
   merge with each other and with box outlines, producing massive BLACK BLOBS
   instead of legible labels. This is invisible in the .png file on disk
   (which looks perfect) but destroys the PDF deliverable.

   **Fix:** render PNGs at ≤1400px wide. This is the maximum that fitz.Story
   embeds without downsampling, and preserves all text labels legibly.
   Use `-w 1400` and DROP `--scale 2` from mmdc invocations. See Step 3 below.

   **Failure signature (catch with PyMuPDF QA in phase8e):** if any embedded
   image has width <900px when the source on disk is >2000px, downsampling
   has occurred and the diagram is destroyed. Phase8e MUST assert source-vs-
   embedded width parity per figure.

**Failure mode 1 (path) being prevented:** previous MARS runs shipped PDFs
with all the PNGs sitting on disk but no markdown embeds. The bid PDF had
zero diagrams — a visible-to-evaluator quality defect.

**Failure mode 2 (downsample) being prevented:** rfp-mars 2026-05-19 run
shipped a Technical PDF where Figure 3 (MARS Logical Architecture) rendered
as an unreadable black silhouette — root cause was 3052x1766 PNG downsampled
by fitz.Story to 816x1056. User flagged with a "WTF?" screenshot. Reproducible
fix: pre-resize PNGs to ≤1400px wide before phase8e runs.

**Verification (BOTH gates MANDATORY):**
- Before phase8e runs, assert every PNG referenced in `figure-registry.json`
  appears as an embed in at least one bid section .md file.
- After phase8e renders, open each volume PDF via PyMuPDF; for every
  embedded image, assert `image.width >= 1200` (within 15% of 1400px source).
  If downsampling detected, fail the phase.


## Inputs

- `{folder}/outputs/bid/*.mmd` - Mermaid source files

## Required Outputs

- `{folder}/outputs/bid/architecture.png`
- `{folder}/outputs/bid/timeline.png`
- `{folder}/outputs/bid/orgchart.png`
- `{folder}/outputs/bid/figure-registry.json`

## Related Optional Output: Interactive Architecture Demo HTML

A complementary, evaluator-facing HTML deliverable can be produced at `{folder}/outputs/ARCHITECTURE_DEMO.html`. Authorship lives in `phase3h-diagrams-win.md` (the blueprint phase), NOT here — that phase already owns "what should the architecture diagrams say". This phase remains focused on rendering the mermaid blueprints to PNG for the PDF bid.

If the interactive HTML demo is desired:

- See `phase3h-diagrams-win.md` § "Interactive HTML demo (optional output)" for the full requirements: dual-audience toggle (Executive | Technical), D3 + dagre layered layout, bezier edges, label-collision discipline, white pill backgrounds, halo strokes, live RFP-requirement traceability per entity (must consume `shared/UNIFIED_RTM.json`), coverage banner per view.
- The HTML is a SIBLING to `TRACEABILITY_EXPLORER.html` — they cross-link (each architecture entity in the HTML links to `TRACEABILITY_EXPLORER.html#req-{ID}` anchors).
- Producer scripts live in `outputs/_arch_demo_v2_builder.py` + `_arch_demo_v2.js` + `_arch_demo_v2.css`; verifier in `_verify_arch_demo.js` (headless Playwright run that asserts zero label collisions).
- Build artifacts (underscored files) are NOT included in the bid PDF.

## Instructions

**CRITICAL: Action Caption Guidelines**
Every diagram MUST have a persuasive action caption that:
1. States what the diagram ENABLES or PROVES (not just what it shows)
2. References at least one win theme where natural
3. Uses active voice: "Our architecture ensures..." not "The architecture is..."
4. Is 1-2 sentences maximum

### Step 1: Verify Mermaid CLI Available

```bash
# Check if mermaid-cli is available
npx @mermaid-js/mermaid-cli --version

# If not installed, it will auto-install via npx
```

### Step 2: Create Mermaid Configuration

The config files in `config-win/mermaid-themes/` define colorful, professional themes.

```python
# Configuration for architecture diagrams
architecture_config = {
    "theme": "base",
    "themeVariables": {
        "primaryColor": "#003366",
        "primaryTextColor": "#ffffff",
        "primaryBorderColor": "#002244",
        "lineColor": "#607d8b",
        "secondaryColor": "#4a90a4",
        "tertiaryColor": "#2e7d32"
    },
    "flowchart": {
        "curve": "basis",
        "padding": 20
    }
}

# Configuration for Gantt charts
gantt_config = {
    "theme": "base",
    "themeVariables": {
        "primaryColor": "#003366",
        "primaryTextColor": "#ffffff",
        "primaryBorderColor": "#002244",
        "lineColor": "#607d8b",
        "taskBkgColor": "#4a90a4",
        "taskTextColor": "#ffffff",
        "doneTaskBkgColor": "#2e7d32"
    },
    "gantt": {
        "barHeight": 30,
        "fontSize": 14,
        "sectionFontSize": 16
    }
}
```

### Step 3: Render Architecture Diagram

**CRITICAL: Run npx from skill directory to avoid polluting RFP folder with package.json**

```bash
# V4-F9 fix 2026-05-18: CLAUDE_SKILL_DIR is set by Claude Code at skill load
# time, but bash subshells and re-invocations sometimes don't inherit it.
# Fall back to the absolute skill path so this phase never silently fails
# with "cd: : No such file or directory" when the env var is empty.
if [ -z "${CLAUDE_SKILL_DIR:-}" ]; then
    SKILL_DIR="C:/Resource Data/WSL/safs/.claude/skills/process-rfp-win"
else
    SKILL_DIR="${CLAUDE_SKILL_DIR}"
fi
if [ ! -d "$SKILL_DIR" ]; then
    echo "ERROR: SKILL_DIR not set or directory missing"
    exit 1
fi
BID_DIR="{folder}/outputs/bid"

# Render architecture diagram (using absolute paths)
cd "$SKILL_DIR" && npx @mermaid-js/mermaid-cli \
  -i "$BID_DIR/architecture.mmd" \
  -o "$BID_DIR/architecture.png" \
  -b white \
  -w 1400 \
  -H 800 \
  --backgroundColor white
  # NOTE: --scale 2 REMOVED 2026-05-20. fitz.Story downsamples raster
  # images larger than ~1500px wide to page-DPI (816x1056) which destroys
  # text labels into black blobs. 1400px wide is the maximum safe size
  # that embeds without downsampling. See phase8e § "PDF Size Discipline".

# Verify output
ls -la "$BID_DIR/architecture.png"
```

### Step 4: Render Timeline/Gantt Chart

```bash
# Render Gantt chart (using absolute paths from skill directory).
# NEW-V4-F12 fix 2026-05-18: standardized white background for consistency.
# NEW 2026-05-20: width 1400px max (no --scale 2) -- fitz.Story downsample fix.
cd "$SKILL_DIR" && npx @mermaid-js/mermaid-cli \
  -i "$BID_DIR/timeline.mmd" \
  -o "$BID_DIR/timeline.png" \
  -b white \
  -w 1400 \
  -H 800 \
  --backgroundColor white
  # NOTE: --scale 2 REMOVED 2026-05-20. fitz.Story downsamples raster
  # images larger than ~1500px wide to page-DPI (816x1056) which destroys
  # text labels into black blobs. 1400px wide is the maximum safe size
  # that embeds without downsampling. See phase8e § "PDF Size Discipline".

# Verify output
ls -la "$BID_DIR/timeline.png"
```

### Step 5: Render Org Chart

```bash
# Render org chart (using absolute paths from skill directory).
# NEW-V4-F12 fix 2026-05-18: standardized white background.
# NEW 2026-05-20: width 1400px max (no --scale 2) -- fitz.Story downsample fix.
cd "$SKILL_DIR" && npx @mermaid-js/mermaid-cli \
  -i "$BID_DIR/orgchart.mmd" \
  -o "$BID_DIR/orgchart.png" \
  -b white \
  -w 1400 \
  -H 800 \
  --backgroundColor white
  # NOTE: --scale 2 REMOVED 2026-05-20. fitz.Story downsamples raster
  # images larger than ~1500px wide to page-DPI (816x1056) which destroys
  # text labels into black blobs. 1400px wide is the maximum safe size
  # that embeds without downsampling. See phase8e § "PDF Size Discipline".

# Verify output
ls -la "$BID_DIR/orgchart.png"
```

### Step 6: Generate Figure Registry with Action Captions

```python
import os
import json
from datetime import datetime

bid_dir = f"{folder}/outputs/bid"

# Load context for caption generation
context_bundle = read_json_safe(f"{folder}/shared/bid-context-bundle.json")
win_themes = context_bundle.get("win_themes", {}).get("themes", []) if context_bundle else []

# Read each .mmd source to understand diagram content
architecture_mmd = read_file_safe(f"{bid_dir}/architecture.mmd")
timeline_mmd = read_file_safe(f"{bid_dir}/timeline.mmd")
orgchart_mmd = read_file_safe(f"{bid_dir}/orgchart.mmd")

# Build figure registry with persuasive action captions
# IMPORTANT: Captions must be PERSUASIVE (what this enables/proves), not DESCRIPTIVE (what this shows)
# Good: "Figure 1: Our layered architecture ensures security compliance while accelerating deployment by isolating concerns"
# Bad: "Figure 1: System Architecture Diagram"

figure_registry = {
    "generated_at": datetime.now().isoformat(),
    "figures": []
}

figure_num = 0
diagram_configs = [
    {
        "file": "architecture.png",
        "source": "architecture.mmd",
        "type": "architecture",
        "section_ref": "03_TECHNICAL.md",
        "caption_prompt": "Describe how this architecture design enables the proposed solution's key benefits. Reference a win theme if applicable.",
        "mmd_content": architecture_mmd
    },
    {
        "file": "timeline.png",
        "source": "timeline.mmd",
        "type": "timeline",
        "section_ref": "02_MANAGEMENT.md",
        "caption_prompt": "Describe how this timeline demonstrates efficient delivery and risk-aware scheduling.",
        "mmd_content": timeline_mmd
    },
    {
        "file": "orgchart.png",
        "source": "orgchart.mmd",
        "type": "orgchart",
        "section_ref": "02_MANAGEMENT.md",
        "caption_prompt": "Describe how this team structure ensures accountability and brings the right expertise to each phase.",
        "mmd_content": orgchart_mmd
    }
]

for config in diagram_configs:
    png_path = f"{bid_dir}/{config['file']}"
    if os.path.exists(png_path):
        figure_num += 1

        # Generate persuasive action caption from diagram content
        # The AI executing this phase will craft the caption using:
        # 1. The Mermaid source content (what the diagram shows)
        # 2. Win themes from context bundle (what to emphasize)
        # 3. The caption_prompt (guidance on tone)

        figure = {
            "number": figure_num,
            "file": config["file"],
            "source_file": config["source"],
            "type": config["type"],
            "section_ref": config["section_ref"],
            "caption": f"Figure {figure_num}: [GENERATE: Analyze {config['source']} content and write a persuasive action caption. {config['caption_prompt']}]",
            "alt_text": f"[GENERATE: Accessible description of {config['type']} diagram for screen readers]",
            "cross_reference": f"See Figure {figure_num} in Section {config['section_ref'].replace('.md', '').replace('_', ' ')}"
        }
        figure_registry["figures"].append(figure)

# CRITICAL: Replace all [GENERATE:] markers with actual captions NOW.
# The AI executing this phase MUST write real persuasive captions, NOT leave markers.
for fig in figure_registry["figures"]:
    if "[GENERATE:" in fig.get("caption", ""):
        # AI: Replace this with your actual persuasive action caption based on
        # the diagram's .mmd content and win themes from bid-context-bundle.json.
        # This marker should NOT appear in the final output.
        pass  # AI replaces caption here
    if "[GENERATE:" in fig.get("alt_text", ""):
        pass  # AI replaces alt_text here

# Validation: ensure no unsubstituted markers remain
unsubstituted = [f["file"] for f in figure_registry["figures"]
                 if "[GENERATE:" in f.get("caption", "") or "[GENERATE:" in f.get("alt_text", "")]
if unsubstituted:
    log(f"⚠️ WARNING: {len(unsubstituted)} figures still have [GENERATE:] markers: {', '.join(unsubstituted)}")
    log("   AI must replace these with actual persuasive captions before writing.")

# Write figure registry
write_json(f"{bid_dir}/figure-registry.json", figure_registry)
log(f"Figure registry: {len(figure_registry['figures'])} figures with action captions")
```

### Step 7: Enforce PDF-Safe Width (MANDATORY — added 2026-05-20)

**⚠️ BLOCKING.** After mmdc renders, ANY PNG wider than 1500px MUST be
resized to 1400px wide (preserving aspect ratio) with LANCZOS resampling.
This is the maximum width fitz.Story embeds without downsampling — see
the failure-mode docs at the top of this phase file. Older mmdc settings
or alternate renderers may still produce wider PNGs even after Step 3-5
were updated; this step is the universal safety net.

```python
from PIL import Image
import os, shutil

MAX_PDF_SAFE_WIDTH = 1400
bid_dir = f"{folder}/outputs/bid"

diagrams_to_check = [
    "architecture.png", "data_model.png", "orgchart.png",
    "timeline.png", "risk_heatmap.png", "integration_sequence.png",
]

for diagram in diagrams_to_check:
    path = os.path.join(bid_dir, diagram)
    if not os.path.exists(path):
        continue
    with Image.open(path) as src:
        w, h = src.size
    if w <= MAX_PDF_SAFE_WIDTH:
        log(f"  {diagram}: {w}x{h} (already PDF-safe)")
        continue
    # Preserve the high-res original as .hires.png (for non-PDF uses like web)
    hires = path.replace(".png", ".hires.png")
    if not os.path.exists(hires):
        shutil.copy(path, hires)
    ratio = MAX_PDF_SAFE_WIDTH / w
    new_h = int(h * ratio)
    with Image.open(hires) as img:
        resized = img.resize((MAX_PDF_SAFE_WIDTH, new_h), Image.LANCZOS)
        resized.save(path, "PNG", optimize=True)
    log(f"  {diagram}: {w}x{h} -> {MAX_PDF_SAFE_WIDTH}x{new_h} (PDF-safe)")
```

### Step 8: Verify All Diagrams

```python
import os
from PIL import Image

required_diagrams = [
    "architecture.png",
    "timeline.png",
    "orgchart.png"
]

bid_dir = f"{folder}/outputs/bid"
results = []

for diagram in required_diagrams:
    path = f"{bid_dir}/{diagram}"
    if os.path.exists(path):
        size_kb = os.path.getsize(path) / 1024
        with Image.open(path) as img:
            w, h = img.size
        # PDF-safe width gate (BLOCKING)
        if w > 1500:
            results.append({
                "file": diagram,
                "status": "❌",
                "error": f"Width {w}px exceeds 1500px PDF-safe limit -- fitz.Story will downsample and destroy text labels. Re-run Step 7."
            })
            continue
        results.append({
            "file": diagram,
            "status": "✅",
            "size_kb": size_kb,
            "dimensions": f"{w}x{h}"
        })
    else:
        results.append({
            "file": diagram,
            "status": "❌",
            "error": "File not generated"
        })

# Report results
for r in results:
    if r["status"] == "✅":
        log(f"  {r['status']} {r['file']}: {r['size_kb']:.1f} KB, {r['dimensions']}")
    else:
        log(f"  {r['status']} {r['file']}: {r.get('error', 'Unknown error')}")

# Hard fail on any diagram with width violation
failures = [r for r in results if r["status"] == "❌"]
if failures:
    raise RuntimeError(f"Phase 8d FAILED: {len(failures)} diagram(s) failed PDF-safe checks. See log above.")
```

### Step 9: Fallback Rendering (if CLI fails)

**Note:** this step runs ONLY if Step 1 (CLI availability check) failed. Step 8
(Verify All Diagrams) hard-fails if any required diagram is missing; the
fallback below is the safety net BEFORE Step 8 runs, used only when mmdc is
genuinely unavailable. In normal operation Steps 3-5 produce the diagrams and
Step 8 verifies them.

If Mermaid CLI fails, provide alternative:

```python
def create_fallback_placeholder(diagram_name, folder):
    """Create placeholder if rendering fails."""
    placeholder_content = f"""
    [Diagram: {diagram_name}]

    Note: Mermaid CLI rendering was unavailable.
    Please render manually using:
    - https://mermaid.live/
    - VS Code Mermaid Preview extension
    - mermaid-cli npm package

    Source file: {folder}/outputs/bid/{diagram_name.replace('.png', '.mmd')}
    """
    return placeholder_content
```

### Step 10: Quality Checks

```python
def verify_diagram_quality(folder):
    """Verify diagram quality meets standards."""
    issues = []

    for diagram in required_diagrams:
        path = f"{folder}/outputs/bid/{diagram}"

        if not os.path.exists(path):
            issues.append(f"Missing: {diagram}")
            continue

        size_kb = os.path.getsize(path) / 1024

        # Minimum size check (10KB expected for quality diagram)
        if diagram == "architecture.png" and size_kb < 10:
            issues.append(f"{diagram} too small ({size_kb:.1f}KB < 10KB)")
        elif size_kb < 5:
            issues.append(f"{diagram} too small ({size_kb:.1f}KB < 5KB)")

    return issues

quality_issues = verify_diagram_quality(folder)
if quality_issues:
    log("⚠️ Quality issues detected:")
    for issue in quality_issues:
        log(f"  - {issue}")
```

### Step 11: Report Results

```python
log(f"""
🎨 Diagram Rendering Complete
=============================
Diagrams rendered: {sum(1 for r in results if r['status'] == '✅')}/3
Figure registry: {len(figure_registry['figures'])} figures with action captions

| Diagram | Status | Size |
|---------|--------|------|
| architecture.png | {results[0]['status']} | {results[0].get('size_kb', 0):.1f} KB |
| timeline.png | {results[1]['status']} | {results[1].get('size_kb', 0):.1f} KB |
| orgchart.png | {results[2]['status']} | {results[2].get('size_kb', 0):.1f} KB |

Output directory: {folder}/outputs/bid/
""")
```

## ⛔ Rendering Discipline (BLOCKING — REVISED 2026-05-20)

**REVISION NOTE (2026-05-20):** the prior version of this section claimed
SVG embeds avoid the size/quality problem. Empirical testing on PyMuPDF
1.26.7 + markdown_pdf 1.3.5 PROVED THIS WRONG. fitz.Story rasterises SVG
to PNG at the page-DPI layout box — identical to PNG handling — producing
the SAME black-blob artifact when the rasterised result is too small for
text antialiasing. SVG embed is NOT a quality fix. Keeping PNG as the
primary format AT THE CORRECT WIDTH is the actual fix.

**Problem 1 (quality):** mmdc `--scale 2 -w 1920` produces ~3800px wide
PNGs. When fitz.Story embeds these, it downsamples to page-DPI (~816x1056)
and text labels merge into black blobs. The PNG on disk is fine; the PDF
embed is destroyed. Reproduced on rfp-mars 2026-05-19 run.

**Problem 2 (size):** uncompressed raster streams in the PDF can exceed
procurement portal upload limits (typically 25 MB). Mitigated by
`MarkdownPdf(optimize=True)` which enables deflate compression. With
`optimize=True` plus 1400px-wide PNGs, each embedded image is ~30-60 KB.

**Rule (combines both fixes):**

1. **Render PNG at `-w 1400` (no `--scale 2`).** This is the maximum width
   fitz.Story embeds without downsampling. Resulting PNG is ~1400x... on
   disk, ~60-150 KB per file.
2. **Pass `optimize=True` to every `MarkdownPdf()` constructor in phase8e.**
   This enables stream compression (deflate). 6 diagrams at 1400px wide
   compress to <1 MB total in the final PDF.
3. **Reference PNG (not SVG) in bid-section .md files.** SVG embed is
   NOT a fix — it suffers the same downsample artifact. Phase 8e
   `clean_markdown()` rewrites `../bid/foo.png` to `./foo.png` so that
   `Section(root=BID_OUT_ABS)` resolves them.

**Rendering command (single PNG per diagram, no SVG primary):**

```bash
cd "$SKILL_DIR" && npx @mermaid-js/mermaid-cli \
  -i "$BID_DIR/foo.mmd" -o "$BID_DIR/foo.png" \
  -b white -w 1400 --backgroundColor white
```

SVGs MAY still be rendered as an optional secondary artifact (useful for
the interactive ARCHITECTURE_DEMO.html, vector printing, web embedding).
They are NOT used by phase8e PDF assembly.

**Markdown embed syntax (use PNG):**

```markdown
![Alt text describing components](../bid/architecture.png)
```

**figure-registry.json:** track `file` as the PNG path. Optional `file_svg`
field for the secondary SVG (when rendered).

**Size verification:** each PNG should be <250 KB on disk. After PDF render
with `optimize=True`, each embedded image in the final PDF should be <200 KB
(PyMuPDF `doc.extract_image(xref)["image"]` length check). Total final
PDF size <15 MB per volume, <25 MB for Draft_Bid.pdf.

**Cross-reference:** phase8e-pdf-win.md contains the `optimize=True` and
size-limit discipline. Together these two rules eliminate the 90 MB → <1 MB
PDF size regression.

## Rendering Commands Summary

**IMPORTANT: Always run from skill directory to avoid polluting RFP folder with package.json/package-lock.json**

```bash
# All rendering commands for reference. Render SVG FIRST (primary), PNG second (fallback).
# NEW-V4-F14 2026-05-19: SVG-preferred to prevent 90 MB PDF size regression.
# NEW-V4-F12 + V4-F9 fix 2026-05-18: fallback SKILL_DIR + consistent output.
# NEW 2026-05-20: PNG width capped at 1400px (no --scale 2) — fitz.Story
# downsamples larger images to ~816x1056 and destroys text labels. See
# failure-mode docs at top of this file.
if [ -z "${CLAUDE_SKILL_DIR:-}" ]; then
    SKILL_DIR="C:/Resource Data/WSL/safs/.claude/skills/process-rfp-win"
else
    SKILL_DIR="${CLAUDE_SKILL_DIR}"
fi
BID_DIR="{folder}/outputs/bid"

# Architecture (flowchart) — SVG primary, PNG fallback
cd "$SKILL_DIR" && npx @mermaid-js/mermaid-cli -i "$BID_DIR/architecture.mmd" -o "$BID_DIR/architecture.svg" -b white --backgroundColor white
cd "$SKILL_DIR" && npx @mermaid-js/mermaid-cli -i "$BID_DIR/architecture.mmd" -o "$BID_DIR/architecture.png" -b white -w 1400 --backgroundColor white

# Timeline (Gantt) — SVG primary, PNG fallback
cd "$SKILL_DIR" && npx @mermaid-js/mermaid-cli -i "$BID_DIR/timeline.mmd" -o "$BID_DIR/timeline.svg" -b white --backgroundColor white
cd "$SKILL_DIR" && npx @mermaid-js/mermaid-cli -i "$BID_DIR/timeline.mmd" -o "$BID_DIR/timeline.png" -b white -w 1400 --backgroundColor white

# Org Chart (flowchart) — SVG primary, PNG fallback
cd "$SKILL_DIR" && npx @mermaid-js/mermaid-cli -i "$BID_DIR/orgchart.mmd" -o "$BID_DIR/orgchart.svg" -b white --backgroundColor white
cd "$SKILL_DIR" && npx @mermaid-js/mermaid-cli -i "$BID_DIR/orgchart.mmd" -o "$BID_DIR/orgchart.png" -b white -w 1400 --backgroundColor white
```

## Quality Checklist (MANDATORY — report each by name with evidence)

The phase agent MUST verify each of the following BEFORE reporting completion. The agent's completion report MUST include a checklist-results block with:
- Item name (verbatim from below)
- PASS / FAIL / SKIPPED-WITH-REASON
- Evidence (file:line citation, grep result, file size, assertion that ran, etc.)

"All checks passed" without per-item evidence is NOT acceptable.

### Required output files
1. **Every blueprint in `diagram-blueprints.json` has corresponding rendered file** — for each blueprint, its `render_target` path exists in `outputs/bid/` — evidence: for each blueprint name, confirm `ls {folder}/outputs/bid/{name}.svg` OR `{name}.png` succeeds; list any missing
2. **figure-registry.json** exists at `{folder}/outputs/bid/figure-registry.json` — evidence: `ls -la` size > 200 bytes and parses as valid JSON
3. **architecture.png** (or .svg) created (>10KB for png, >1KB for svg) — evidence: `ls -la` with size confirmation
4. **timeline.png** (or .svg) created (>10KB for png, >1KB for svg) — evidence: `ls -la` with size confirmation
5. **orgchart.png** (or .svg) created (>5KB for png, >1KB for svg) — evidence: `ls -la` with size confirmation

### Schema fidelity
6. **Every rendered PNG/SVG has `![]()` reference** in some bid section — evidence: grep `!\[.*\](.*architecture\.\(png\|svg\))` across all bid-sections/*.md returned >= 1 hit; same for timeline and orgchart
7. **figure-registry.json figures array** contains one entry per rendered diagram — evidence: print `len(figure_registry["figures"])`
8. **Each caption is persuasive (action-oriented)** — no `[GENERATE:` markers remain — evidence: grep "[GENERATE:" in figure-registry.json returned 0 matches
9. No `[:N]` slicing applied to deliverable content strings — evidence: grep for `\[:[0-9]+\]` in production code paths returned 0 hits

### Cross-stage consistency
10. **PDF size compliance — SVG primary format used** — evidence: confirm mmdc output is .svg (primary) not .png (fallback) to prevent 90 MB PDF regression; print the formats actually generated
11. **Diagram Quality Criteria met** — >= 14pt fonts, WCAG 2.2 AA contrast, colorblind-safe palette — evidence: confirm mermaid source contains `fontSize: '14px'` and Okabe-Ito hex colors (e.g., #0072B2) in classDef blocks

### Anti-regression rules (universal)
12. **UTF-8 encoding** on every `open()` call — evidence: search this phase's emitted scripts/code for `encoding='utf-8'` in every file-open
13. **ensure_ascii=False** on every `json.dump` call — evidence: same grep
14. **No `_Showing N of M_` row-cap notices** in any deliverable markdown — evidence: grep returned 0 matches
15. **No empty `|  |` mitigation/cell patterns** in any deliverable table — evidence: grep returned 0 matches
16. **No mid-word table-cell truncations** — evidence: line-by-line cell-end check returned 0 hits

### Memory discipline
17. **Relevant SAFS memory entries reviewed and applied** — evidence: list which memory files were read and which rules were applicable (e.g., "SVG primary / PNG fallback — prevents 90 MB PDF regression per NEW-V4-F14")
