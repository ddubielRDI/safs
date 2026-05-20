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

**Phase8e (PDF assembly) caveats** — the rendering engine (fitz.Story via
markdown_pdf) cannot resolve `..` in image src paths. Phase8e's
`clean_markdown` MUST rewrite `../bid/foo.png` to `bid/foo.png` and set
`Section(root=outputs/)` so the PNG resolves. Without this, the PDF ships
with broken image links and zero embedded images.

**Failure mode being prevented:** previous MARS runs shipped PDFs with all
the PNGs sitting on disk but no markdown embeds. The bid PDF had zero
diagrams — a visible-to-evaluator quality defect.

**Verification:** before phase8e runs, assert that every PNG referenced in
`figure-registry.json` appears as an embed in at least one bid section .md
file. If not, fail loudly and surface which figures are unreferenced.


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
  -w 1920 \
  -H 1080 \
  --scale 2 \
  --backgroundColor white

# Verify output
ls -la "$BID_DIR/architecture.png"
```

### Step 4: Render Timeline/Gantt Chart

```bash
# Render Gantt chart (using absolute paths from skill directory).
# NEW-V4-F12 fix 2026-05-18: standardized output to 1920x1080 white background
# for consistent quality across all diagrams.
cd "$SKILL_DIR" && npx @mermaid-js/mermaid-cli \
  -i "$BID_DIR/timeline.mmd" \
  -o "$BID_DIR/timeline.png" \
  -b white \
  -w 1920 \
  -H 1080 \
  --scale 2 \
  --backgroundColor white

# Verify output
ls -la "$BID_DIR/timeline.png"
```

### Step 5: Render Org Chart

```bash
# Render org chart (using absolute paths from skill directory).
# NEW-V4-F12 fix 2026-05-18: standardized to 1920x1080 white background.
cd "$SKILL_DIR" && npx @mermaid-js/mermaid-cli \
  -i "$BID_DIR/orgchart.mmd" \
  -o "$BID_DIR/orgchart.png" \
  -b white \
  -w 1920 \
  -H 1080 \
  --scale 2 \
  --backgroundColor white

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

### Step 7: Verify All Diagrams

```python
import os

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
        results.append({
            "file": diagram,
            "status": "✅",
            "size_kb": size_kb
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
        log(f"  {r['status']} {r['file']}: {r['size_kb']:.1f} KB")
    else:
        log(f"  {r['status']} {r['file']}: {r.get('error', 'Unknown error')}")
```

### Step 8: Fallback Rendering (if CLI fails)

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

### Step 9: Quality Checks

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

### Step 10: Report Results

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

## ⛔ SVG-Preferred Rendering Discipline (BLOCKING — added 2026-05-19)

**Problem:** mmdc `--scale 2 -w 1920` renders PNG at ~3000px wide, which
fitz.Story embeds UNCOMPRESSED (no Filter in PDF stream). A single architecture
diagram expands to 15-27 MB uncompressed in the PDF. 6 diagrams = 85 MB of raw
raster data in Draft_Bid.pdf — exceeding all procurement portal upload limits.

**Rule:** ALWAYS render SVG as the PRIMARY format. mmdc supports SVG natively
(`-o foo.svg`). fitz.Story accepts SVG via `<img src="...svg">` and rasterises
it at page DPI (~96 DPI for Letter), producing a ~30 KB compressed image per
diagram. Keep PNG as FALLBACK only.

**Rendering order (MANDATORY for all 6 diagrams):**

```bash
# SVG first (primary for PDF embed — tiny, crisp, compresses well)
cd "$SKILL_DIR" && npx @mermaid-js/mermaid-cli \
  -i "$BID_DIR/foo.mmd" -o "$BID_DIR/foo.svg" \
  -b white --backgroundColor white

# PNG second (kept as fallback if SVG embed fails)
cd "$SKILL_DIR" && npx @mermaid-js/mermaid-cli \
  -i "$BID_DIR/foo.mmd" -o "$BID_DIR/foo.png" \
  -b white -w 1920 -H 1080 --scale 2 --backgroundColor white
```

**Markdown embed syntax:** reference SVG, not PNG:

```markdown
![Alt text](../bid/architecture.svg)
```

phase8e `clean_markdown()` MUST resolve SVG paths (rewrite `../bid/foo.svg`
to `bid/foo.svg`) identically to PNG paths, and check for SVG presence before
falling back to PNG.

**figure-registry.json:** track both `file_svg` and `file_png` per figure;
set `file` to the SVG path when available.

**Size verification:** each SVG should be <300 KB on disk. After PDF render
with `optimize=True`, each embedded image in the final PDF should be <200 KB
(PyMuPDF `doc.extract_image(xref)["image"]` length check).

**Cross-reference:** phase8e-pdf-win.md contains the `optimize=True` and
size-limit discipline. Together these two rules eliminate the 90 MB → <1 MB
PDF size regression.

## Rendering Commands Summary

**IMPORTANT: Always run from skill directory to avoid polluting RFP folder with package.json/package-lock.json**

```bash
# All rendering commands for reference. Render SVG FIRST (primary), PNG second (fallback).
# NEW-V4-F14 2026-05-19: SVG-preferred to prevent 90 MB PDF size regression.
# NEW-V4-F12 + V4-F9 fix 2026-05-18: fallback SKILL_DIR and consistent 1920x1080 output.
if [ -z "${CLAUDE_SKILL_DIR:-}" ]; then
    SKILL_DIR="C:/Resource Data/WSL/safs/.claude/skills/process-rfp-win"
else
    SKILL_DIR="${CLAUDE_SKILL_DIR}"
fi
BID_DIR="{folder}/outputs/bid"

# Architecture (flowchart) — SVG primary, PNG fallback
cd "$SKILL_DIR" && npx @mermaid-js/mermaid-cli -i "$BID_DIR/architecture.mmd" -o "$BID_DIR/architecture.svg" -b white --backgroundColor white
cd "$SKILL_DIR" && npx @mermaid-js/mermaid-cli -i "$BID_DIR/architecture.mmd" -o "$BID_DIR/architecture.png" -b white -w 1920 -H 1080 --scale 2 --backgroundColor white

# Timeline (Gantt) — SVG primary, PNG fallback
cd "$SKILL_DIR" && npx @mermaid-js/mermaid-cli -i "$BID_DIR/timeline.mmd" -o "$BID_DIR/timeline.svg" -b white --backgroundColor white
cd "$SKILL_DIR" && npx @mermaid-js/mermaid-cli -i "$BID_DIR/timeline.mmd" -o "$BID_DIR/timeline.png" -b white -w 1920 -H 1080 --scale 2 --backgroundColor white

# Org Chart (flowchart) — SVG primary, PNG fallback
cd "$SKILL_DIR" && npx @mermaid-js/mermaid-cli -i "$BID_DIR/orgchart.mmd" -o "$BID_DIR/orgchart.svg" -b white --backgroundColor white
cd "$SKILL_DIR" && npx @mermaid-js/mermaid-cli -i "$BID_DIR/orgchart.mmd" -o "$BID_DIR/orgchart.png" -b white -w 1920 -H 1080 --scale 2 --backgroundColor white
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
