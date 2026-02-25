---
name: phase6-pdf
expert-role: Publication Specialist
domain-expertise: Document layout, PDF generation, professional formatting
---

# Phase 6: PDF Generation

**Expert Role:** Publication Specialist

**Purpose:** Generate a professional BID_SCREEN.md from consolidated data, then render as BID_SCREEN.pdf (~7-9 pages). This is the final human-readable deliverable.

**Inputs:**
- `{folder}/screen/BID_SCREEN.json` — Consolidated data from Phase 5

**Required Outputs:**
- `{folder}/screen/BID_SCREEN.md` (>5KB) — Markdown source
- `{folder}/screen/BID_SCREEN.pdf` (>10KB) — Final PDF

---

## CRITICAL CSS CONSTRAINTS (fitz.Story Renderer)

The `markdown_pdf` library uses PyMuPDF `fitz.Story` internally — an HTML4/CSS2 subset renderer with known rendering bugs:

- **NEVER use CSS `border` properties** — they render as thick filled rectangles
- **NEVER use `background-color` on ANY block element** (th, td, blockquote, pre, code) — fitz.Story ghost-fills: fills leak to fixed y-positions on every subsequent page
- **Safe properties:** color, font-*, padding, margin, text-align
- **`hr` MUST use:** `height: 0; color: #ffffff; background-color: #ffffff;`
- Distinguish elements via: bold/color for th, color+padding for blockquote, font-family for code/pre

---

## Instructions

### Step 1: Load Consolidated Data

```python
bid_screen = read_json(f"{folder}/screen/BID_SCREEN.json")
rfp = bid_screen["rfp_summary"]
gonogo = bid_screen["go_nogo_score"]
intel = bid_screen.get("client_intel", {})
compliance = bid_screen["compliance"]
projects = bid_screen["past_projects"]
risk = bid_screen["risk_assessment"]
themes = bid_screen.get("preliminary_themes", {})
```

### Step 2: Build BID_SCREEN.md

Build the markdown document section by section. All 8 sections must be populated (Intel section omitted if --quick mode, themes section omitted if no themes generated).

```python
from datetime import datetime

recommendation = bid_screen["recommendation"]
total_score = bid_screen["total_score"]

# Badge styling via text (no CSS needed — plain markdown)
badge = {
    "GO": "GO — Proceed to Full Pipeline",
    "CONDITIONAL": "CONDITIONAL — Review Risks Before Committing",
    "NO_GO": "NO-GO — Do Not Bid"
}

md = f"""# RFP Bid Screening Report

**Generated:** {datetime.now().strftime("%B %d, %Y at %I:%M %p")}
**Mode:** {"Quick" if bid_screen.get("screening_mode") == "quick" else "Full"}

---

## Recommendation

### {badge.get(recommendation, recommendation)}

**Score: {total_score}/100**

{bid_screen.get("rationale", "")}

---

## Quick Facts

| Field | Value |
|-------|-------|
| **Client** | {rfp.get("client_name", "Not identified")} |
| **RFP Number** | {rfp.get("rfp_number", "Not found")} |
| **Title** | {rfp.get("rfp_title", "Not found")} |
| **Deadline** | {rfp.get("submission_deadline", "Not found")} |
| **Estimated Value** | {rfp.get("estimated_value", "Not disclosed")} |
| **Contract Type** | {rfp.get("contract_type", "Not specified")} |
| **Domain** | {rfp.get("industry_domain", "Not classified")} |
| **Set-Aside** | {rfp.get("set_aside", "None")} |

---

## Go/No-Go Scorecard

| Area | Weight | Score | Weighted | Rating |
|------|--------|-------|----------|--------|
"""

# Build scorecard rows from assessment areas
areas = gonogo.get("assessment_areas", [])
for area in areas:
    name = area.get("name", "")
    weight = area.get("weight", 0)
    score = area.get("score", 0)
    weighted = score * weight
    weight_pct = f"{weight*100:.0f}%"
    rating = "Strong" if score >= 70 else "Moderate" if score >= 50 else "Weak"
    md += f"| **{name}** | {weight_pct} | {score} | {weighted:.1f} | {rating} |\n"

md += f"| **TOTAL** | **100%** | | **{total_score}** | **{recommendation}** |\n"

# Assessment Area Details
md += "\n### Assessment Area Details\n\n"
for area in areas:
    name = area.get("name", "")
    score = area.get("score", 0)
    weight_pct = f"{area.get('weight', 0)*100:.0f}%"
    rationale = area.get("rationale", "No rationale provided.")
    md += f"**{name} ({score}/100, weight: {weight_pct}):** {rationale}\n\n"

    # Show key evidence and risks
    evidence = area.get("evidence", [])
    if evidence:
        md += "Key evidence:\n"
        for e in evidence[:3]:
            md += f"- {e}\n"
        md += "\n"

    risks = area.get("risks", [])
    if risks:
        md += "Risks:\n"
        for r in risks[:2]:
            md += f"- {r}\n"
        md += "\n"

md += "\n---\n\n"
```

#### Section: Client Intelligence (conditional)

```python
# Only include if not --quick mode and data exists
if intel and intel.get("status") == "complete":
    intelligence = intel.get("intelligence", {})

    md += "## Client Intelligence\n\n"
    md += "### Organization Profile\n\n"

    org = intelligence.get("organization_profile", {})
    md += f"**Name:** {org.get('name', 'Unknown')}\n"
    md += f"**Industry:** {org.get('industry', 'Unknown')}\n"
    md += f"**Size:** {org.get('size', 'Unknown')}\n"
    md += f"**Headquarters:** {org.get('headquarters', 'Unknown')}\n\n"

    # Recent News
    news = intelligence.get("news", [])
    if news:
        md += "### Recent News\n\n"
        md += "| Date | Headline | Source |\n"
        md += "|------|----------|--------|\n"
        for item in news[:5]:
            md += f"| {item.get('date', 'N/A')} | {item.get('headline', 'N/A')} | {item.get('source', 'N/A')} |\n"
        md += "\n"

    # Technology Environment
    tech_stack = intelligence.get("technology_stack", [])
    if tech_stack:
        md += "### Technology Environment\n\n"
        for tech in tech_stack:
            md += f"- {tech}\n"
        md += "\n"

    # Competitive Landscape
    competitive = intelligence.get("competitive_landscape", {})
    if competitive:
        md += "### Competitive Landscape\n\n"
        md += f"**Incumbent:** {competitive.get('incumbent', 'Unknown')}\n"
        known = competitive.get("known_competitors", [])
        if known:
            md += f"**Known Competitors:** {', '.join(known)}\n"
        md += "\n"

    md += "\n---\n\n"
```

#### Section: Compliance Quick-Check

```python
md += "## Compliance Quick-Check\n\n"
md += "| Requirement | Category | Status |\n"
md += "|-------------|----------|--------|\n"

compliance_items = compliance.get("compliance_items", [])
pass_count = 0
gap_count = 0
risk_count = 0

for item in compliance_items:
    status = item.get("status", "UNKNOWN")
    req = item.get("requirement", "")[:80]
    cat = item.get("category", "General")
    md += f"| {req} | {cat} | {status} |\n"

    if status == "PASS":
        pass_count += 1
    elif status == "GAP":
        gap_count += 1
    elif status == "RISK":
        risk_count += 1

md += f"\n**Summary:** {pass_count} PASS | {gap_count} GAP | {risk_count} RISK\n\n"
md += "\n---\n\n"
```

#### Section: Past Project Matches

```python
md += "## Past Project Matches\n\n"

matched = projects.get("matched_projects", [])

if matched:
    md += "### Top 5 Matches\n\n"
    md += "| Rank | Project | Industry | Score | Key Overlap |\n"
    md += "|------|---------|----------|-------|-------------|\n"

    for i, proj in enumerate(matched[:5], 1):
        title = proj.get("title", "Untitled")
        industry = proj.get("industry", "N/A")
        score = proj.get("relevance_score", 0)
        tech_matches = ", ".join(proj.get("technology_matches", [])[:3])
        md += f"| {i} | {title} | {industry} | {score} | {tech_matches} |\n"

    md += "\n### Detailed Match Analysis\n\n"

    for i, proj in enumerate(matched[:3], 1):
        md += f"#### {i}. {proj.get('title', 'Untitled')}\n\n"
        md += f"**Client:** {proj.get('client', 'N/A')}\n"
        md += f"**Industry:** {proj.get('industry', 'N/A')}\n"
        md += f"**Relevance Score:** {proj.get('relevance_score', 0)}\n"

        tech = proj.get("technology_matches", [])
        if tech:
            md += f"**Technology Overlap:** {', '.join(tech)}\n"

        metrics = proj.get("key_metrics", [])
        if metrics:
            md += f"**Key Metrics:** {'; '.join(metrics[:3])}\n"

        relevance = proj.get("relevance_statement", "")
        if relevance:
            md += f"**Relevance:** {relevance}\n"

        md += "\n"
else:
    md += "*No matching past projects found.*\n\n"

md += "\n---\n\n"
```

#### Section: Preliminary Win Themes

```python
# Include if themes were generated (Phase 4.5)
theme_list = themes.get("themes", [])
if theme_list:
    md += "## Preliminary Win Themes\n\n"
    md += "*Directional positioning hints from screening data. Full pipeline generates "
    md += "production themes with evaluation factor mapping.*\n\n"

    md += "| # | Theme | Category | Confidence | Key Evidence |\n"
    md += "|---|-------|----------|------------|---------------|\n"

    for t in theme_list:
        rank = t.get("rank", "")
        name = t.get("name", "Unnamed")
        category = t.get("category", "").replace("_", " ").title()
        confidence = t.get("confidence", "unknown").upper()
        evidence_summary = "; ".join(t.get("evidence", [])[:2])[:120]
        md += f"| {rank} | **{name}** | {category} | {confidence} | {evidence_summary} |\n"

    md += "\n### Theme Details\n\n"
    for t in theme_list:
        rank = t.get("rank", "")
        name = t.get("name", "Unnamed")
        rationale = t.get("rationale", "No rationale provided.")
        evidence = t.get("evidence", [])

        md += f"**{rank}. {name}**\n"
        md += f"*{rationale}*\n"
        if evidence:
            md += "Supporting evidence:\n"
            for e in evidence:
                md += f"- {e}\n"
        md += "\n"

    md += "\n---\n\n"
elif themes.get("status") != "not_generated":
    md += "## Preliminary Win Themes\n\n"
    md += "*No preliminary themes generated for this screening.*\n\n"
    md += "\n---\n\n"
```

#### Section: Risks and Opportunities

```python
md += "## Risks and Opportunities\n\n"

# Risk Assessment
md += "### Risk Assessment\n\n"
risks = risk.get("risks", [])

if risks:
    md += "| # | Risk | Severity | Category |\n"
    md += "|---|------|----------|----------|\n"
    for i, r in enumerate(risks, 1):
        md += f"| {i} | {r.get('risk', '')} | {r.get('severity', 'unknown').upper()} | {r.get('category', 'general')} |\n"
    md += "\n"
else:
    md += "*No significant risks identified.*\n\n"

# Dealbreaker assessment
has_dealbreaker = risk.get("has_dealbreaker", False)
dealbreakers = risk.get("dealbreakers", [])
if has_dealbreaker:
    md += f"**Dealbreaker Assessment:** {len(dealbreakers)} potential dealbreaker(s) identified:\n"
    for db in dealbreakers:
        md += f"- {db}\n"
    md += "\n"
else:
    md += "**Dealbreaker Assessment:** No dealbreakers identified.\n\n"

# Competitive Advantages
md += "### Competitive Advantages\n\n"
opportunities = risk.get("opportunities", [])
if opportunities:
    for opp in opportunities:
        md += f"- {opp.get('opportunity', '')}\n"
    md += "\n"
else:
    md += "*No specific competitive advantages identified.*\n\n"

# Historical context
md += "### Historical Bid Context\n\n"
hist = risk.get("historical_context", {})
if hist.get("has_data"):
    md += f"**Total Past Bids:** {hist.get('total_bids', 0)}\n"
    win_rate = hist.get("overall_win_rate", 0)
    md += f"**Overall Win Rate:** {win_rate:.0%}\n"
    domain_rate = hist.get("domain_win_rate")
    if domain_rate is not None:
        md += f"**Domain Win Rate:** {domain_rate:.0%} ({hist.get('domain_bids', 0)} bids)\n"
    md += f"\n*{hist.get('advisory', '')}*\n\n"
else:
    md += "*No historical bid data available. Consider logging outcomes in bid-outcomes.json for future pattern analysis.*\n\n"

md += "\n---\n\n"
```

#### Section: Final Recommendation

```python
md += "## Final Recommendation\n\n"
md += f"### {recommendation}\n\n"
md += f"**Score:** {total_score}/100\n"
md += f"**Rationale:** {bid_screen.get('rationale', '')}\n\n"

md += "### Next Steps\n\n"
next_steps = bid_screen.get("next_steps", [])
for step in next_steps:
    md += f"- {step}\n"

md += "\n---\n\n"
md += "*Report generated by RFP Screening Pipeline. This is an automated assessment — human judgment should inform the final bid/no-bid decision.*\n"
```

### Step 3: Write BID_SCREEN.md

```python
write_file(f"{folder}/screen/BID_SCREEN.md", md)
```

### Step 4: Generate BID_SCREEN.pdf

```python
from markdown_pdf import MarkdownPdf, Section

# PROFESSIONAL_CSS — fitz.Story safe (NO border, NO background-color on block elements)
PROFESSIONAL_CSS = """
body {
    font-family: 'Segoe UI', 'Helvetica Neue', Arial, sans-serif;
    font-size: 10.5pt;
    line-height: 1.55;
    color: #1a1a1a;
}
h1 { color: #002855; font-size: 22pt; font-weight: 700; margin-top: 28px; margin-bottom: 14px; }
h2 { color: #002855; font-size: 16pt; font-weight: 600; margin-top: 22px; margin-bottom: 10px; }
h3 { color: #2b6695; font-size: 13pt; font-weight: 600; margin-top: 18px; margin-bottom: 8px; }
h4 { color: #3a7ca5; font-size: 11.5pt; font-weight: 600; margin-top: 14px; margin-bottom: 6px; }
table { border-collapse: collapse; width: 100%; margin: 12px 0; font-size: 8.5pt; word-wrap: break-word; table-layout: auto; }
th { color: #002855; font-weight: 700; padding: 7px 6px; text-align: left; font-size: 8.5pt; }
td { padding: 5px 6px; vertical-align: top; word-wrap: break-word; overflow-wrap: break-word; }
td strong { color: #002855; }
blockquote { padding: 12px 16px; margin: 14px 0; font-style: normal; color: #1a3a5c; }
blockquote strong { color: #002855; }
pre { padding: 12px; font-family: 'Consolas', 'Courier New', monospace; font-size: 9pt; line-height: 1.4; white-space: pre-wrap; }
code { font-family: 'Consolas', 'Courier New', monospace; font-size: 9pt; padding: 1px 4px; }
hr { height: 0; margin: 20px 0; color: #ffffff; background-color: #ffffff; }
ul, ol { margin: 8px 0; padding-left: 24px; }
li { margin-bottom: 4px; }
a { color: #2b6695; text-decoration: none; }
strong { color: #1a2a3a; }
p { margin: 6px 0; }
"""

pdf = MarkdownPdf(toc_level=2)
pdf.meta["title"] = f"RFP Bid Screening — {rfp.get('client_name', 'Unknown')}"
pdf.meta["author"] = "Resource Data, Inc."

# Read the markdown we just wrote
md_content = read_file(f"{folder}/screen/BID_SCREEN.md")
pdf.add_section(Section(md_content, toc=True), user_css=PROFESSIONAL_CSS)
pdf.save(f"{folder}/screen/BID_SCREEN.pdf")
```

### Step 5: QA Check

```python
import os

pdf_path = f"{folder}/screen/BID_SCREEN.pdf"
md_path = f"{folder}/screen/BID_SCREEN.md"

# Check markdown
if os.path.exists(md_path):
    md_size = os.path.getsize(md_path) / 1024
    log(f"BID_SCREEN.md: {md_size:.1f}KB")
    if md_size < 5:
        log(f"WARNING: BID_SCREEN.md only {md_size:.1f}KB — expected >5KB")
else:
    log("ERROR: BID_SCREEN.md not generated")

# Check PDF
if os.path.exists(pdf_path):
    size_kb = os.path.getsize(pdf_path) / 1024
    if size_kb < 10:
        log(f"WARNING: BID_SCREEN.pdf only {size_kb:.1f}KB — may be corrupt")
        # Retry once
        pdf.save(f"{folder}/screen/BID_SCREEN.pdf")
        size_kb = os.path.getsize(pdf_path) / 1024

    # PyMuPDF page count check
    try:
        import fitz
        doc = fitz.open(pdf_path)
        page_count = doc.page_count
        doc.close()
        log(f"BID_SCREEN.pdf: {size_kb:.1f}KB, {page_count} pages")
        if page_count == 0:
            log("ERROR: PDF has 0 pages")
    except ImportError:
        log(f"BID_SCREEN.pdf: {size_kb:.1f}KB (page count unavailable — fitz not installed)")
else:
    log("ERROR: BID_SCREEN.pdf not generated")
    log("Attempting retry with markdown_pdf...")
    # Retry: re-read markdown and regenerate
    try:
        md_content = read_file(f"{folder}/screen/BID_SCREEN.md")
        pdf2 = MarkdownPdf(toc_level=2)
        pdf2.meta["title"] = f"RFP Bid Screening — {rfp.get('client_name', 'Unknown')}"
        pdf2.meta["author"] = "Resource Data, Inc."
        pdf2.add_section(Section(md_content, toc=True), user_css=PROFESSIONAL_CSS)
        pdf2.save(f"{folder}/screen/BID_SCREEN.pdf")
        log("Retry succeeded")
    except Exception as e:
        log(f"Retry failed: {e}")
        log("Fallback: BID_SCREEN.json is still available as machine-readable output")
```

### Step 6: Report

```
PDF GENERATION (Phase 6)
=========================
BID_SCREEN.md: {md_size}KB
BID_SCREEN.pdf: {pdf_size}KB, {page_count} pages

Primary deliverable: {folder}/screen/BID_SCREEN.pdf
Machine-readable: {folder}/screen/BID_SCREEN.json
```

---

## Quality Checklist

- [ ] `BID_SCREEN.md` written (>5KB)
- [ ] `BID_SCREEN.pdf` generated (>10KB)
- [ ] PDF has > 0 pages
- [ ] No ghost fills — CSS has ZERO `background-color` on block elements (th, td, blockquote, pre, code)
- [ ] No CSS `border` properties used anywhere
- [ ] `hr` uses `height: 0; color: #ffffff; background-color: #ffffff;`
- [ ] All 8 sections populated (Cover, Scorecard, Intel, Compliance, Projects, Themes, Risks, Recommendation)
- [ ] Scorecard renders 7 weighted assessment areas (not 5 equal dimensions)
- [ ] If --quick mode, intel section omitted
- [ ] QA check passed (file exists, > 10KB, page count > 0)
- [ ] If PDF fails, BID_SCREEN.json still available as fallback
