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

## Inputs

- `{folder}/outputs/bid/*.mmd` - Mermaid source files

## Required Outputs

- `{folder}/outputs/bid/architecture.png`
- `{folder}/outputs/bid/timeline.png`
- `{folder}/outputs/bid/orgchart.png`
- `{folder}/outputs/bid/figure-registry.json`

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
# SKILL_DIR contains npm cache - keeps RFP folder clean
SKILL_DIR="/home/ddubiel/repos/safs/.claude/skills/process-rfp-win"
BID_DIR="{folder}/outputs/bid"

# Render architecture diagram (using absolute paths)
cd "$SKILL_DIR" && npx @mermaid-js/mermaid-cli \
  -i "$BID_DIR/architecture.mmd" \
  -o "$BID_DIR/architecture.png" \
  -b white \
  -w 1200 \
  --scale 2

# Verify output
ls -la "$BID_DIR/architecture.png"
```

### Step 4: Render Timeline/Gantt Chart

```bash
# Render Gantt chart (using absolute paths from skill directory)
cd "$SKILL_DIR" && npx @mermaid-js/mermaid-cli \
  -i "$BID_DIR/timeline.mmd" \
  -o "$BID_DIR/timeline.png" \
  -b white \
  -w 1400 \
  --scale 2

# Verify output
ls -la "$BID_DIR/timeline.png"
```

### Step 5: Render Org Chart

```bash
# Render org chart (using absolute paths from skill directory)
cd "$SKILL_DIR" && npx @mermaid-js/mermaid-cli \
  -i "$BID_DIR/orgchart.mmd" \
  -o "$BID_DIR/orgchart.png" \
  -b white \
  -w 1000 \
  --scale 2

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

## Rendering Commands Summary

**IMPORTANT: Always run from skill directory to avoid polluting RFP folder with package.json/package-lock.json**

```bash
# All rendering commands for reference:
SKILL_DIR="/home/ddubiel/repos/safs/.claude/skills/process-rfp-win"
BID_DIR="{folder}/outputs/bid"

# Architecture (flowchart)
cd "$SKILL_DIR" && npx @mermaid-js/mermaid-cli -i "$BID_DIR/architecture.mmd" -o "$BID_DIR/architecture.png" -b white -w 1200 --scale 2

# Timeline (Gantt)
cd "$SKILL_DIR" && npx @mermaid-js/mermaid-cli -i "$BID_DIR/timeline.mmd" -o "$BID_DIR/timeline.png" -b white -w 1400 --scale 2

# Org Chart (flowchart)
cd "$SKILL_DIR" && npx @mermaid-js/mermaid-cli -i "$BID_DIR/orgchart.mmd" -o "$BID_DIR/orgchart.png" -b white -w 1000 --scale 2
```

## Quality Checklist

- [ ] `architecture.png` created (>10KB)
- [ ] `timeline.png` created (>10KB)
- [ ] `orgchart.png` created (>5KB)
- [ ] All diagrams render clearly
- [ ] No raw Mermaid code in final bid document
- [ ] `figure-registry.json` created with action captions for all rendered diagrams
- [ ] Each caption is persuasive (action-oriented), not merely descriptive
- [ ] Figure numbers are sequential (1, 2, 3...)
- [ ] Section cross-references point to correct bid sections
