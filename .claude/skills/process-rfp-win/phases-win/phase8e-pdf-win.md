---
name: phase8e-pdf-win
expert-role: Publication Specialist
domain-expertise: Multi-file PDF assembly, typography, professional document layout, RFP submission packaging
skill: publication-specialist
---

# Phase 8e: Multi-File PDF Assembly

## Purpose

Assemble final bid submission as multiple named PDF files matching the RFP's required submission structure. Uses `SUBMISSION_STRUCTURE.json` to determine file count, naming, and content mapping.

## Inputs

- `{folder}/shared/SUBMISSION_STRUCTURE.json` - Required file structure and naming
- `{folder}/outputs/bid-sections/*.md` - All bid section markdown files
- `{folder}/outputs/bid/*.png` - Rendered diagrams
- `{folder}/outputs/*.md` - Supporting documents (EXECUTIVE_SUMMARY, TRACEABILITY, etc.)
- `${CLAUDE_SKILL_DIR}/config-win/pdf-theme.css` - **Chromium-based** PDF styling (used ONLY by `npx md-to-pdf` fallback path; safe to use `border`, `background-color`, full CSS)
- `PROFESSIONAL_CSS` / `COVER_CSS` / `VOL_HEADER_CSS` (defined inline in this phase file at "## PDF Theme (CSS)") - **fitz.Story-safe** styling used by `markdown_pdf` (the PRIMARY renderer); strips constructs that trigger ghost-fill / em-dash mojibake bugs
- `${CLAUDE_SKILL_DIR}/config-win/company-profile.json` - Company name for file naming

## Required Outputs

**All PDFs in `outputs/bid/`** — Named per RFP convention.

Default output set (overridden by SUBMISSION_STRUCTURE.json):
- `{folder}/outputs/bid/ResourceData_1_SUBMITTAL.pdf`
- `{folder}/outputs/bid/ResourceData_2_MANAGEMENT.pdf`
- `{folder}/outputs/bid/ResourceData_3_TECHNICAL.pdf`
- `{folder}/outputs/bid/ResourceData_4_SOLUTION.pdf`
- `{folder}/outputs/bid/ResourceData_5_FINANCIAL.pdf`
- `{folder}/outputs/bid/ResourceData_6_INTEGRATION.pdf`
- Plus any additional volumes from SUBMISSION_STRUCTURE.json

Also generates:
- `{folder}/outputs/bid/Draft_Bid.pdf` (consolidated single file for review)
- `{folder}/outputs/bid/EXECUTIVE_SUMMARY.pdf`
- `{folder}/outputs/bid/REQUIREMENTS_CATALOG.pdf`
- `{folder}/outputs/bid/TRACEABILITY_MATRIX.pdf`

## Instructions

### Step 1: Load Submission Structure and Discover Bid Sections

```python
import os
import glob
import re

submission = read_json_safe(f"{folder}/shared/SUBMISSION_STRUCTURE.json")
company = read_json("config-win/company-profile.json")

bidder_name = company.get("company_name", "ResourceData").replace(", Inc.", "").replace(" ", "")

# Discover all bid section files
bid_section_files = sorted(glob.glob(f"{folder}/outputs/bid-sections/*.md"))
legacy_bid_files = [
    f"{folder}/outputs/title-page.md",
    f"{folder}/outputs/solution.md",
    f"{folder}/outputs/timeline.md"
]

# Map bid section files to volume numbers
file_volume_map = {}
for bf in bid_section_files:
    fname = os.path.basename(bf)
    # Extract volume number from filename (e.g., "01_SUBMITTAL.md" -> 1)
    match = re.match(r'(\d+)[a-z]?_(.+)\.md', fname)
    if match:
        vol_num = int(match.group(1))
        vol_name = match.group(2)
        if vol_num not in file_volume_map:
            file_volume_map[vol_num] = []
        file_volume_map[vol_num].append({
            "path": bf,
            "name": vol_name,
            "filename": fname
        })
```

### Step 2: Build PDF Generation Plan

```python
pdf_plan = []

if submission:
    # Use SUBMISSION_STRUCTURE.json for naming and ordering
    volumes = submission.get("volumes", [])
    naming = submission.get("naming_convention", {})
    pattern = naming.get("pattern", "{Bidder}_{Volume}_{Title}.pdf")

    for vol in volumes:
        order = vol.get("order", 99)
        title = vol.get("title", "")
        title_clean = re.sub(r'[^A-Z0-9_]', '_', title.upper())[:30]
        output_file = vol.get("output_file", "")

        # Resolve filename using naming convention
        pdf_name = pattern.replace("{Bidder}", bidder_name) \
                          .replace("{Volume}", str(order)) \
                          .replace("{Title}", title_clean) \
                          .replace(" ", "_")
        if not pdf_name.endswith(".pdf"):
            pdf_name += ".pdf"

        # Find matching source markdown files
        source_files = []
        if order in file_volume_map:
            source_files = [f["path"] for f in file_volume_map[order]]
        elif output_file:
            full_path = f"{folder}/{output_file}"
            if os.path.exists(full_path):
                source_files = [full_path]

        pdf_plan.append({
            "volume": order,
            "title": title,
            "pdf_name": pdf_name,
            "source_files": source_files,
            "page_limit": vol.get("page_limit")
        })
else:
    # Default structure: one PDF per bid section file
    default_volumes = {
        1: ("SUBMITTAL", "Letter of Submittal"),
        2: ("MANAGEMENT", "Management Proposal"),
        3: ("TECHNICAL", "Technical Approach"),
        4: ("SOLUTION", "Business Solution"),
        5: ("FINANCIAL", "Financial Proposal"),
        6: ("INTEGRATION", "Technical Integration")
    }

    for vol_num, (name, title) in default_volumes.items():
        source_files = [f["path"] for f in file_volume_map.get(vol_num, [])]
        pdf_plan.append({
            "volume": vol_num,
            "title": title,
            "pdf_name": f"{bidder_name}_{vol_num}_{name}.pdf",
            "source_files": source_files,
            "page_limit": None
        })

# Add appendix volumes (always generated)
appendix_volumes = [
    ("REQUIREMENTS_REVIEW", "04_REQUIREMENTS_REVIEW.md", "Requirements Review"),
    ("RISK_REGISTER", "04_RISK_REGISTER.md", "Risk Register"),
]
for name, source, title in appendix_volumes:
    source_path = f"{folder}/outputs/bid-sections/{source}"
    if os.path.exists(source_path) and not any(name in p["pdf_name"] for p in pdf_plan):
        pdf_plan.append({
            "volume": 99,
            "title": title,
            "pdf_name": f"{bidder_name}_A_{name}.pdf",
            "source_files": [source_path],
            "page_limit": None
        })
```

### Step 3: Assemble Markdown Per Volume

```python
for plan in pdf_plan:
    if not plan["source_files"]:
        log(f"⚠️ No source files for volume: {plan['title']}")
        continue

    # Concatenate source files for this volume
    combined_md = ""
    for sf in plan["source_files"]:
        content = read_file(sf)
        combined_md += content + "\n\n---\n\n"

    # Write combined markdown
    md_path = f"{folder}/outputs/bid/{plan['pdf_name'].replace('.pdf', '.md')}"
    write_file(md_path, combined_md)
    plan["md_path"] = md_path
    plan["char_count"] = len(combined_md)
```

### Step 4: Also Assemble Consolidated Draft Bid

```python
# Single consolidated file for easy review
all_content = ""
for plan in sorted(pdf_plan, key=lambda p: p["volume"]):
    if plan.get("md_path") and os.path.exists(plan["md_path"]):
        all_content += f"# Volume {plan['volume']}: {plan['title']}\n\n"
        all_content += read_file(plan["md_path"])
        all_content += "\n\n---\n\n"

# Also include legacy sections if they exist and weren't already included
for legacy in legacy_bid_files:
    if os.path.exists(legacy):
        fname = os.path.basename(legacy)
        if not any(fname in str(p.get("source_files", [])) for p in pdf_plan):
            all_content += read_file(legacy) + "\n\n---\n\n"

write_file(f"{folder}/outputs/bid/Draft_Bid.md", all_content)
```

### Step 5: Generate PDFs

**⚠️ THIS STEP IS MANDATORY. The pipeline is NOT complete without PDF files.**

**Tool Priority:**
1. **Python `markdown_pdf`** (PRIMARY — always available, no external dependencies)
2. `npx md-to-pdf` (SECONDARY — requires Node.js, may not be installed)
3. `pandoc` (TERTIARY — requires system install)

**If ALL tools fail, the pipeline FAILS. Do NOT skip this step. Do NOT mark the pipeline as complete without PDFs.**

#### ⛔ Renderer–CSS Routing (CRITICAL — different renderers REQUIRE different CSS)

This phase has TWO active render paths, each with INCOMPATIBLE CSS expectations. Mixing them up produces silent quality regressions in evaluator-facing PDFs.

| Renderer | CSS to use | Rationale |
|---|---|---|
| `markdown_pdf` (PRIMARY, Python, fitz.Story-based) | `PROFESSIONAL_CSS` / `COVER_CSS` / `VOL_HEADER_CSS` — defined inline at "## PDF Theme (CSS)" below | fitz.Story has two known rendering bugs: `border-*` renders as solid filled rectangles overlapping text; `background-color` on block elements (th/td/blockquote/pre/code) causes ghost-fill bands across every subsequent page. The inline CSS strings strip these constructs and use only safe properties (color, font-*, padding, margin, text-align). |
| `npx md-to-pdf` (FALLBACK, Node.js, Chromium/Puppeteer-based) | `${CLAUDE_SKILL_DIR}/config-win/pdf-theme.css` (via `--config-file ${CLAUDE_SKILL_DIR}/config-win/md-to-pdf.config.js`) | Chromium renders full CSS correctly. The corporate `pdf-theme.css` uses `border-bottom` on headings, `background-color` on table rows, etc. — all of which are SAFE in Chromium and produce the intended corporate look. |
| `pandoc` (TERTIARY, system install) | pandoc-specific (LaTeX-style); CSS does NOT apply | If reached, manually craft pandoc styling or fall back to default. |

**Rules:**
1. **Never pass `pdf-theme.css` to `markdown_pdf`.** It will trigger ghost-fill bands and overlapping borders that ruin every subsequent page.
2. **Never pass `PROFESSIONAL_CSS` to `npx md-to-pdf`.** Chromium would render fine, but the styling is intentionally minimal — you'd lose the corporate look that `pdf-theme.css` provides.
3. **If the `npx md-to-pdf` fallback fires, pass `--config-file ${CLAUDE_SKILL_DIR}/config-win/md-to-pdf.config.js` explicitly.** md-to-pdf's default config discovery (look for `md-to-pdf.config.js` in cwd) WILL NOT find it because the config lives in `config-win/`, not the skill root. Without explicit config, the fallback renders with md-to-pdf's plain defaults — no corporate styling.
4. **Em-dash replacement is renderer-specific.** `markdown_pdf` (fitz.Story) requires `content.replace('—', '--')` before passing to `pdf.add_section(Section(...))` (see line 279 of this file). Chromium renders em-dashes correctly — DO NOT pre-replace for the `md-to-pdf` path or you lose typographic quality.

**Why this routing is documented here, not in `config-win/pdf-theme.css`:** the CSS file is correct AS-IS for its intended renderer (Chromium). The bug is at the renderer-selection boundary, not in either CSS file. This block exists so that an executing agent picking a fallback path doesn't unintentionally pair the wrong CSS with the wrong renderer.



```python
import os, re, sys
from markdown_pdf import MarkdownPdf, Section

BID_DIR = f"{folder}/outputs/bid-sections"
OUT_DIR = f"{folder}/outputs/bid"
os.makedirs(OUT_DIR, exist_ok=True)

company = read_json("config-win/company-profile.json")
bidder_name = company.get("company_name", "ResourceData").replace(", Inc.", "").replace(" ", "")

# Define volumes from bid section files
VOLUMES = [
    (1, "01_SUBMITTAL.md", "SUBMITTAL", "Letter of Submittal"),
    (2, "02_MANAGEMENT.md", "MANAGEMENT", "Management Proposal"),
    (3, "03_TECHNICAL.md", "TECHNICAL", "Technical Approach"),
    (4, "04_SOLUTION.md", "SOLUTION", "Business Solution"),
    (5, "05_FINANCIAL.md", "FINANCIAL", "Financial Proposal"),
    (6, "06_INTEGRATION.md", "INTEGRATION", "Technical Integration"),
]

APPENDICES = [
    ("A", "04_REQUIREMENTS_REVIEW.md", "REQUIREMENTS_REVIEW", "Requirements Review"),
    ("B", "04_RISK_REGISTER.md", "RISK_REGISTER", "Risk Register"),
]

def clean_markdown(content):
    """Clean markdown for PDF rendering. Strips HTML, anchors, and editorial markers.

    Source .md files are NOT modified -- cleaning happens at render time only.
    """
    # Remove HTML comments
    content = re.sub(r'<!--.*?-->', '', content, flags=re.DOTALL)
    # Strip internal anchor links: [text](#anchor) -> text
    content = re.sub(r'\[([^\]]+)\]\(#[^)]*\)', r'\1', content)
    # Ensure blank line before headings
    content = re.sub(r'([^\n])\n(#{1,6} )', r'\1\n\n\2', content)

    # --- Editorial marker stripping (PDF only) ---
    # 1. [USER INPUT REQUIRED: ...] or [USER INPUT REQUIRED] -> [___]
    content = re.sub(r'\[USER INPUT REQUIRED:?[^\]]*\]', '[___]', content)
    # 2. **[Theme Name]** -> empty (bold-wrapped win-theme bracket markers).
    # V4-F2 fix 2026-05-18: prior regex `r'\*\*\[[\w\s-]+\]\*\*\s*'` was too broad
    # and stripped legitimate citations like `**[Attachment A]**` or `**[Exhibit 3]**`
    # that evaluators rely on. Now: whitelist known authoring marker labels so
    # only true theme/ghost/proof markers are removed.
    known_markers = ["Win Theme", "Discriminator", "Theme", "Ghost", "Pain Point", "Proof Point"]
    marker_pattern = re.compile(
        r'\*\*\[(?:' + '|'.join(re.escape(m) for m in known_markers) + r')[:\s][^\]]*\]\*\*\s*',
        re.IGNORECASE
    )
    marker_count_before = len(marker_pattern.findall(content))
    content = marker_pattern.sub('', content)
    log(f"  clean_markdown: stripped {marker_count_before} authoring markers")
    # 3. **WIN THEME -- Name:** -> empty (Management-style theme callouts)
    content = re.sub(r'\*\*WIN THEME\s*--\s*[^:]+:\*\*\s*', '', content)
    # 4. > **REVIEW REQUIRED:** ... -> empty (process-note blockquotes)
    content = re.sub(r'^>\s*\*\*REVIEW REQUIRED:\*\*.*$', '', content, flags=re.MULTILINE)

    # --- Internal reference stripping (PDF only) ---
    # 5. Strip internal file references that should never appear in deliverables
    #    Patterns: "company-profile.json field.subfield", "Past_Projects.md Section",
    #    "EVALUATION_CRITERIA.json", "evidence-library.json", etc.
    content = re.sub(r'(?:company-profile\.json|Past_Projects\.md|rfp-summary\.json|'
                     r'bid-outcomes\.json|evidence-library\.json|domain-context\.json|'
                     r'EVALUATION_CRITERIA\.json|COMPLIANCE_MATRIX\.json|'
                     r'SUBMISSION_STRUCTURE\.json|POSITIONING_OUTPUT\.json|'
                     r'bid-context-bundle\.json|PERSONA_COVERAGE\.json)'
                     r'[\s\w.\[\]()]*', '', content)
    # 6. Clean up artifacts from reference stripping (dangling "per ", "from ", etc.)
    content = re.sub(r'\b(per|from|in|via|see)\s*\.\s', '. ', content)
    content = re.sub(r'\(\s*\)', '', content)  # empty parens
    content = re.sub(r'  +', ' ', content)  # double spaces

    # --- Em dash replacement (fitz.Story cannot render U+2014) ---
    # 7. Replace em dashes with -- to prevent mojibake in PDF
    content = content.replace('\u2014', '--')
    content = content.replace('\u2013', '--')  # en dash too

    return content


def validate_content(content, source_name="unknown"):
    """Scan cleaned markdown for remaining editorial artifacts before PDF render.
    Returns dict of findings (empty = clean).
    """
    findings = {}
    unfilled = re.findall(r'\[(?:USER INPUT|INSERT|PLACEHOLDER|TBD|TODO)[^\]]*\]', content, re.IGNORECASE)
    if unfilled:
        findings["unfilled_markers"] = unfilled
    themes = re.findall(r'\*\*\[[\w\s-]+\]\*\*', content)
    if themes:
        findings["theme_markers"] = themes
    win_themes = re.findall(r'\*\*WIN THEME\s*--\s*[^:]+:\*\*', content)
    if win_themes:
        findings["win_theme_callouts"] = win_themes
    process_notes = re.findall(r'^>\s*\*\*(?:REVIEW REQUIRED|NOTE TO AUTHOR|INTERNAL|EDITOR)[^\n]*', content, re.MULTILINE)
    if process_notes:
        findings["process_notes"] = process_notes
    for keyword in ["TODO", "TBD", "PLACEHOLDER", "FIXME", "DRAFT - For Internal"]:
        hits = re.findall(r'^(?!```).{0,200}\b' + re.escape(keyword) + r'\b', content, re.MULTILINE | re.IGNORECASE)
        if hits:
            findings.setdefault("keywords", {})[keyword] = len(hits)
    total = sum(len(v) if isinstance(v, list) else sum(v.values()) if isinstance(v, dict) else 0 for v in findings.values())
    if total > 0:
        log(f"[!] CONTENT VALIDATION [{source_name}]: {total} artifact(s) found")
    else:
        log(f"[OK] CONTENT VALIDATION [{source_name}]: clean")
    return findings

# V4-F3 fix 2026-05-18: rfp_title was previously a NameError — no upstream block
# in this phase loaded the domain context or initialized the variable. Cover-page
# generation and per-volume header rendering both reference rfp_title, so the
# phase would crash on first PDF. Load it now from domain-context.json with a
# graceful placeholder fallback.
domain = read_json(f"{folder}/shared/domain-context.json")
rfp_title = domain.get("rfp_title", domain.get("project_name", "[RFP Title]"))

# --- 5a: Generate consolidated Draft_Bid.pdf ---
pdf = MarkdownPdf(toc_level=2)
pdf.meta["title"] = f"Draft Bid - {rfp_title}"
pdf.meta["author"] = company.get("company_name", "Resource Data, Inc.")

# Cover page — NO draft watermark, address from company profile
company_addr = company.get("address", "")
if not company_addr:
    # Fallback: build from HQ location in company profile
    hq = company.get("headquarters", {})
    company_addr = f"{hq.get('street', '')} {hq.get('city', '')}, {hq.get('state', '')} {hq.get('zip', '')}".strip()

cover = f"""
# {rfp_title}

## Technical and Business Proposal

---

### Submitted by

## {company.get("company_name", "Resource Data, Inc.")}

**{company_addr}**

---

**Date:** {datetime.now().strftime("%B %d, %Y")}

---
"""
pdf.add_section(Section(cover, toc=False))

# Add all volumes (with content validation)
all_validation = {}
for vol_num, filename, tag, title in VOLUMES:
    filepath = os.path.join(BID_DIR, filename)
    if os.path.exists(filepath):
        content = clean_markdown(read_file(filepath))
        findings = validate_content(content, filename)
        if findings:
            all_validation[filename] = findings
        pdf.add_section(Section(f"\n\n---\n\n# Volume {vol_num}: {title}\n\n" + content))

for app_id, filename, tag, title in APPENDICES:
    filepath = os.path.join(BID_DIR, filename)
    if os.path.exists(filepath):
        content = clean_markdown(read_file(filepath))
        findings = validate_content(content, filename)
        if findings:
            all_validation[filename] = findings
        pdf.add_section(Section(f"\n\n---\n\n# Appendix {app_id}: {title}\n\n" + content))

pdf.save(os.path.join(OUT_DIR, "Draft_Bid.pdf"))

# --- 5b: Generate individual volume PDFs ---
for vol_num, filename, tag, title in VOLUMES + [(a[0], a[1], a[2], a[3]) for a in APPENDICES]:
    filepath = os.path.join(BID_DIR, filename)
    if not os.path.exists(filepath):
        continue
    content = clean_markdown(read_file(filepath))
    validate_content(content, filename)  # Log any remaining artifacts
    vol_pdf = MarkdownPdf(toc_level=2)
    vol_pdf.meta["title"] = f"{title} - {rfp_title}"
    vol_pdf.meta["author"] = company.get("company_name", "Resource Data, Inc.")
    header = f"**{company.get('company_name', 'Resource Data, Inc.')} | {rfp_title}**\n\n---\n\n"
    vol_pdf.add_section(Section(header + content))
    pdf_name = f"{bidder_name}_{vol_num}_{tag}.pdf"
    vol_pdf.save(os.path.join(OUT_DIR, pdf_name))

# --- 5c: Generate supporting PDFs ---
for md_name, pdf_name in [
    ("EXECUTIVE_SUMMARY.md", "EXECUTIVE_SUMMARY.pdf"),
    ("REQUIREMENTS_CATALOG.md", "REQUIREMENTS_CATALOG.pdf"),
    ("TRACEABILITY.md", "TRACEABILITY_MATRIX.pdf"),
]:
    md_path = f"{folder}/outputs/{md_name}"
    if os.path.exists(md_path):
        content = clean_markdown(read_file(md_path))
        validate_content(content, md_name)  # Log any remaining artifacts
        s_pdf = MarkdownPdf(toc_level=2)
        s_pdf.meta["title"] = md_name.replace(".md", "").replace("_", " ")
        s_pdf.add_section(Section(content))
        s_pdf.save(os.path.join(OUT_DIR, pdf_name))
```

**Fallback (if markdown_pdf is not installed):**

```bash
pip install markdown_pdf
# Then re-run the Python code above
```

**Second fallback (npx md-to-pdf):**

V4-F1 fix 2026-05-18: the fallback now passes `--config-file` explicitly so the
Chromium renderer uses the corporate `pdf-theme.css` defined alongside the
skill. Without this flag md-to-pdf cannot locate the config (it lives in
`config-win/`, not the skill root) and falls back to plain unstyled defaults.
Removed redundant `--pdf-options` — page format and margins are already declared
in the config file, and duplicating them caused override ambiguity.

```bash
SKILL_DIR="{skills_dir}"
BID_DIR="{folder}/outputs/bid"

for md_file in "$BID_DIR"/*.md; do
    pdf_file="${md_file%.md}.pdf"
    cd "$SKILL_DIR" && npx md-to-pdf "$md_file" -o "$pdf_file" \
        --config-file "${SKILL_DIR}/config-win/md-to-pdf.config.js"
done
```

### Step 6: Verify PDF Outputs

```python
results = []
total_pages_est = 0

for plan in pdf_plan:
    pdf_path = f"{folder}/outputs/bid/{plan['pdf_name']}"

    if os.path.exists(pdf_path):
        size_kb = os.path.getsize(pdf_path) / 1024
        est_pages = plan.get("char_count", 0) / 3000  # ~3000 chars per page
        total_pages_est += est_pages

        # Check page limit if specified
        within_limit = True
        if plan.get("page_limit") and est_pages > plan["page_limit"]:
            within_limit = False

        results.append({
            "volume": plan["volume"],
            "title": plan["title"],
            "pdf_name": plan["pdf_name"],
            "status": "✅",
            "size_kb": round(size_kb, 1),
            "est_pages": round(est_pages),
            "page_limit": plan.get("page_limit"),
            "within_limit": within_limit
        })
    else:
        results.append({
            "volume": plan["volume"],
            "title": plan["title"],
            "pdf_name": plan["pdf_name"],
            "status": "❌",
            "issue": "PDF not generated"
        })

# Also check consolidated and supporting PDFs
for pdf_name in ["Draft_Bid.pdf", "EXECUTIVE_SUMMARY.pdf", "REQUIREMENTS_CATALOG.pdf", "TRACEABILITY_MATRIX.pdf"]:
    path = f"{folder}/outputs/bid/{pdf_name}"
    if os.path.exists(path):
        size_kb = os.path.getsize(path) / 1024
        results.append({
            "volume": "-",
            "title": pdf_name.replace(".pdf", "").replace("_", " "),
            "pdf_name": pdf_name,
            "status": "✅",
            "size_kb": round(size_kb, 1)
        })
```

### Step 6b: Post-Generation QA Validation (MANDATORY)

**⚠️ NEVER skip this step. The entire point of the pipeline is to produce professional PDFs. Validating them is non-negotiable.**

Use PyMuPDF (`fitz`) to programmatically inspect every generated PDF for quality issues. This catches problems that are invisible in markdown but obvious when a human opens the PDF.

```python
import fitz  # PyMuPDF

qa_issues = []
pdf_files = [f for f in os.listdir(OUT_DIR) if f.endswith(".pdf")]

for pdf_name in sorted(pdf_files):
    pdf_path = os.path.join(OUT_DIR, pdf_name)
    try:
        doc = fitz.open(pdf_path)
    except Exception as e:
        qa_issues.append((pdf_name, "CORRUPT", f"Cannot open PDF: {e}"))
        continue

    # QA-1: File not empty
    if doc.page_count == 0:
        qa_issues.append((pdf_name, "EMPTY", "PDF has 0 pages"))
        doc.close()
        continue

    # QA-2: Minimum file size (corrupt/blank PDFs are tiny)
    size_kb = os.path.getsize(pdf_path) / 1024
    if size_kb < 10:
        qa_issues.append((pdf_name, "TOO_SMALL", f"Only {size_kb:.1f} KB — likely corrupt"))

    has_colored_headers = False
    blank_pages = 0

    for page_idx in range(doc.page_count):
        page = doc[page_idx]
        text = page.get_text().strip()

        # QA-3: Detect blank pages (< 20 chars of text)
        if len(text) < 20:
            blank_pages += 1

        # QA-4: Verify CSS is applied — look for navy fill rectangles (table headers)
        drawings = page.get_drawings()
        for d in drawings:
            if d.get("fill"):
                fill = d["fill"]
                if isinstance(fill, tuple) and len(fill) >= 3:
                    # Navy blue ~(0, 0.15, 0.33)
                    if fill[0] < 0.05 and fill[1] < 0.2 and fill[2] > 0.2:
                        has_colored_headers = True

    if blank_pages > 0:
        qa_issues.append((pdf_name, "BLANK_PAGES", f"{blank_pages} blank page(s)"))

    # Only flag missing headers on substantial documents (> 3 pages should have tables)
    if not has_colored_headers and doc.page_count > 3:
        qa_issues.append((pdf_name, "NO_STYLED_HEADERS",
                         "No colored table headers detected — CSS may not be applied"))

    doc.close()

# Report QA results
log(f"QA Validation: Checked {len(pdf_files)} PDFs")
if qa_issues:
    log(f"⚠️ Found {len(qa_issues)} QA issues:")
    for pdf_name, issue_type, desc in qa_issues:
        log(f"  [{issue_type}] {pdf_name}: {desc}")
    # CORRUPT or EMPTY issues are blockers — fail the phase
    blockers = [q for q in qa_issues if q[1] in ("CORRUPT", "EMPTY", "TOO_SMALL")]
    if blockers:
        raise RuntimeError(f"QA BLOCKED: {len(blockers)} critical PDF issues found")
else:
    log("✅ ALL QA CHECKS PASSED — PDFs are professional quality")
```

**QA checks performed:**

| Check | What It Detects | Severity |
|-------|-----------------|----------|
| QA-1: Empty PDF | 0-page documents from generation failures | BLOCKER |
| QA-2: File size | Corrupt/blank PDFs under 10KB | BLOCKER |
| QA-3: Blank pages | Pages with no text content | WARNING |
| QA-4: Styled headers | Missing CSS (no colored table headers) | WARNING |

**BLOCKER issues** cause the phase to fail. **WARNING issues** are reported but don't block.

### Step 7: Generate Submission Checklist

```python
# List what the user needs to submit
checklist = "## Submission Checklist\n\n"
checklist += "| # | File | Size | Pages | Limit | Status |\n"
checklist += "|---|------|------|-------|-------|--------|\n"

for r in sorted(results, key=lambda x: str(x.get("volume", 99))):
    limit_str = str(r.get("page_limit", "-"))
    within = "✅" if r.get("within_limit", True) else "⚠️ OVER"
    pages = str(r.get("est_pages", "-"))
    checklist += f"| {r.get('volume', '-')} | {r['pdf_name']} | {r.get('size_kb', 'N/A')} KB | {pages} | {limit_str} | {r['status']} {within} |\n"

# Report user input markers remaining
all_md_content = ""
for bf in bid_section_files:
    all_md_content += read_file(bf)
user_markers = re.findall(r'\[USER INPUT REQUIRED[^\]]*\]', all_md_content)

if user_markers:
    checklist += f"\n### Remaining User Input Required ({len(user_markers)} items)\n\n"
    for marker in set(user_markers):
        checklist += f"- {marker}\n"
```

### Step 8: Report Results

```python
success_count = sum(1 for r in results if r["status"] == "✅")
volume_count = sum(1 for r in results if r.get("volume") != "-" and r["status"] == "✅")

log(f"""
📦 MULTI-FILE PDF ASSEMBLY COMPLETE (Phase 8e)
================================================
Submission Files: {volume_count} volumes generated
Supporting Files: {success_count - volume_count} supplementary PDFs
Total Estimated Pages: {total_pages_est:.0f}
User Input Items Remaining: {len(user_markers)}

{checklist}

Submission Directory: {folder}/outputs/bid/
Consolidated Review: {folder}/outputs/bid/Draft_Bid.pdf
""")

# Write assembly report
assembly_report = {
    "assembled_at": datetime.now().isoformat(),
    "bidder_name": bidder_name,
    "submission_structure_used": submission is not None,
    "volumes_generated": volume_count,
    "pdf_results": results,
    "content_validation": all_validation,  # Editorial artifact findings (empty = clean)
    "user_input_remaining": len(user_markers),
    "total_estimated_pages": round(total_pages_est)
}
write_json(f"{folder}/outputs/bid/assembly-report.json", assembly_report)
```

## PDF Theme (CSS)

**MANDATORY: All PDFs MUST use this professional CSS theme via the `user_css` parameter.**
Pass this CSS to every `pdf.add_section(Section(...), user_css=PROFESSIONAL_CSS)` call.

**⚠️ CRITICAL CSS CONSTRAINT:** `fitz.Story` (the rendering engine inside `markdown_pdf`) has two rendering bugs. (1) CSS `border` properties render as thick filled rectangles that overlap text. **NEVER** use any `border-*` property. (2) `background-color` on block elements (th, td, blockquote, pre, code) causes **ghost fills** — the colored rectangle leaks to the same y-position on every subsequent page, creating visible tinted bands across the entire document. **NEVER** use `background-color` on any element except `hr` (invisible spacer at height:0). Safe properties: `color`, `font-*`, `padding`, `margin`, `text-align`. See [PyMuPDF #3041](https://github.com/pymupdf/PyMuPDF/discussions/3041).

```python
PROFESSIONAL_CSS = """
body {
    font-family: 'Segoe UI', 'Helvetica Neue', Arial, sans-serif;
    font-size: 10.5pt;
    line-height: 1.55;
    color: #1a1a1a;
}

/* Headings — color + size hierarchy only, NO borders (fitz.Story renders borders as thick filled rectangles) */
h1 { color: #002855; font-size: 22pt; font-weight: 700; margin-top: 28px; margin-bottom: 14px; }
h2 { color: #002855; font-size: 16pt; font-weight: 600; margin-top: 22px; margin-bottom: 10px; }
h3 { color: #2b6695; font-size: 13pt; font-weight: 600; margin-top: 18px; margin-bottom: 8px; }
h4 { color: #3a7ca5; font-size: 11.5pt; font-weight: 600; margin-top: 14px; margin-bottom: 6px; }

/* Tables — NO background-color on ANY table element (fitz.Story ghost-fills at page breaks) */
/* Table headers distinguished by bold navy text only */
table { border-collapse: collapse; width: 100%; margin: 12px 0; font-size: 8.5pt; word-wrap: break-word; table-layout: auto; }
th { color: #002855; font-weight: 700; padding: 7px 6px; text-align: left; font-size: 8.5pt; }
td { padding: 5px 6px; vertical-align: top; word-wrap: break-word; overflow-wrap: break-word; }
td strong { color: #002855; }

/* Callout boxes — NO background-color (ghost-fills on every page), use color+padding only */
blockquote { padding: 12px 16px; margin: 14px 0; font-style: normal; color: #1a3a5c; }
blockquote strong { color: #002855; }

/* Code — NO background-color (ghost-fills), padding only */
pre { padding: 12px; font-family: 'Consolas', 'Courier New', monospace; font-size: 9pt; line-height: 1.4; white-space: pre-wrap; }
code { font-family: 'Consolas', 'Courier New', monospace; font-size: 9pt; padding: 1px 4px; }

/* HR — invisible spacer only, NO border-top (renders as thick navy bar) */
hr { height: 0; margin: 20px 0; color: #ffffff; background-color: #ffffff; }

ul, ol { margin: 8px 0; padding-left: 24px; }
li { margin-bottom: 4px; }
a { color: #2b6695; text-decoration: none; }
strong { color: #1a2a3a; }
p { margin: 6px 0; }
"""
```

**Cover page CSS** (centered, large fonts for title page):

```python
COVER_CSS = """
body { font-family: 'Segoe UI', Arial, sans-serif; color: #002855; text-align: center; }
h1 { color: #002855; font-size: 28pt; font-weight: 700; margin-top: 100px; margin-bottom: 8px; }
h2 { color: #3a7ca5; font-size: 18pt; font-weight: 500; margin-bottom: 20px; }
h3 { color: #002855; font-size: 14pt; font-weight: 600; margin-top: 30px; }
hr { height: 0; margin: 25px auto; color: #ffffff; background-color: #ffffff; }
p { font-size: 11pt; color: #333; }
em { font-size: 9pt; color: #666; }
"""
```

**Volume header CSS** (compact running header):

```python
VOL_HEADER_CSS = """
body { font-family: 'Segoe UI', Arial, sans-serif; color: #002855; }
h1 { color: #002855; font-size: 11pt; font-weight: 600; margin-bottom: 0; }
hr { height: 0; margin: 4px 0 12px 0; color: #ffffff; background-color: #ffffff; }
"""
```

## ⛔ Wide-table HEAD-repeat Discipline (BLOCKING — added 2026-05-19)

**Problem:** fitz.Story (PyMuPDF 1.26.x) does NOT implement
`display: table-header-group` for repeated column headers on page breaks
(confirmed empirically). A 10-column risk register or 7-column requirements
review rendered as a single markdown table has headers ONLY on page 1.
An evaluator on page 30 cannot tell which column is which — an immediate
quality defect.

**Rule:** For ANY section whose markdown contains a table with >=7 columns,
convert the wide table to CHUNKED HTML (multiple mini-tables each with its
own `<thead>`) BEFORE passing to fitz.Story. This is the ONLY reliable
mechanism.

**Calibration (risk register, landscape Letter-L, 7.5pt font):**
- `rows_per_chunk=3` → headers on 45/48 pages (94% data-page coverage)
- `rows_per_chunk=12` → headers on only 23/52 pages (chunks span 2+ pages, FAIL)
- **Use `rows_per_chunk=3` for 10-column tables, `rows_per_chunk=4` for 7-column tables**

**Implementation pattern (add to `clean_markdown_wide_tables()`):**

```python
def _build_chunked_html(headers, rows, rows_per_chunk=3):
    """Convert parsed table into chunked HTML with repeated <thead> per chunk."""
    header_html = "".join(f"<th>{h}</th>" for h in headers)
    chunks = []
    for start in range(0, len(rows), rows_per_chunk):
        chunk = rows[start:start + rows_per_chunk]
        html = f"<table>\n<thead><tr>{header_html}</tr></thead>\n<tbody>\n"
        for row in chunk:
            html += "<tr>" + "".join(f"<td>{c}</td>" for c in row) + "</tr>\n"
        html += "</tbody></table>\n"
        chunks.append(html)
    return "".join(chunks)

def convert_wide_tables_to_chunked_html(content, min_cols=7, rows_per_chunk=3):
    """Replace wide markdown tables (>=min_cols) with chunked HTML (thead per chunk)."""
    lines = content.split("\n")
    output = []
    i = 0
    while i < len(lines):
        line = lines[i]
        stripped = line.strip()
        if (stripped.startswith("|")
                and stripped.count("|") >= (min_cols + 1)
                and not re.match(r"^\s*\|[\s\-:|]+\|", line)
                and i + 1 < len(lines)
                and re.match(r"^\s*\|[\s\-:|]+\|", lines[i + 1])):
            headers, rows, end_idx = _parse_md_table(lines, i)
            if headers and rows and len(headers) >= min_cols:
                output.append(_build_chunked_html(headers, rows, rows_per_chunk))
                i = end_idx
                continue
        output.append(line)
        i += 1
    return "\n".join(output)
```

**Apply to these sections:**
- `04_RISK_REGISTER.md` (10 cols) — use `rows_per_chunk=3`, `paper_size="Letter-L"`
- `04_REQUIREMENTS_REVIEW.md` (7 cols) — use `rows_per_chunk=3`

**Cross-reference:** phase8.4k-riskreg-win.md contains the landscape-orientation
discipline. The thead-repeat discipline here is the complementary fix for
the header-visibility problem.

**Verification (MANDATORY):** After render, open risk register PDF via PyMuPDF.
Check that >=90% of data pages (pages with >200 chars of text) contain ALL of:
`{"Risk ID", "RTM Risk ID", "Description", "Severity", "Likelihood", "Impact",
"Mitigation", "Owner", "Verification", "Status"}` — normalize ligatures
(ﬁ→fi, ﬂ→fl) before string matching. If fewer than 90% of data pages pass,
reduce `rows_per_chunk` and re-render.

## ⛔ PDF Size Discipline (BLOCKING — added 2026-05-19)

**Problem:** fitz.Story embeds PNG images as UNCOMPRESSED raw raster bitmaps.
A mermaid PNG rendered at `--scale 2 -w 1920` produces ~3000px wide images
which expand to 15-27 MB each UNCOMPRESSED in the PDF. A 6-diagram bid
produces a 90 MB Draft_Bid.pdf — exceeding procurement portal upload limits
(typically 10-25 MB).

**Fix (two-part, both MANDATORY):**

1. **Pass `optimize=True` to ALL `MarkdownPdf()` constructors.** This calls
   `doc.ez_save()` instead of `doc.save()`, enabling deflate compression on
   image streams. Brings 41 MB TECHNICAL.pdf → 0.20 MB (200x compression).
   ```python
   pdf = MarkdownPdf(toc_level=2, optimize=True)  # MANDATORY
   ```

2. **Prefer SVG over PNG for mermaid embeds.** mmdc supports SVG output via
   `-o foo.svg`. SVGs are vector (10-250 KB each), render crisp at page DPI
   when fitz rasterises them, and compress to ~30 KB in the final PDF.
   - Render both: `mmdc -i foo.mmd -o foo.svg` AND `mmdc -i foo.mmd -o foo.png`
   - Reference SVG in markdown: `![alt](../bid/foo.svg)` instead of `.png`
   - Keep PNG on disk as fallback if SVG embed fails
   - See phase8d-diagrams-win.md for SVG rendering guidance

**Size targets (MANDATORY):**
- `Draft_Bid.pdf`: < 25 MB (portal limit)
- Individual volume PDFs: < 15 MB each
- Risk register and appendix PDFs: < 15 MB each

**Verification (MANDATORY):** After render, `os.path.getsize(pdf_path)` all
PDFs in `outputs/bid/`. Any file >15 MB is a blocker. `Draft_Bid.pdf` >25 MB
is a blocker.

## ⚠️ Quality Checklist (ALL MANDATORY — pipeline fails if ANY unchecked)

## Quality Checklist (MANDATORY — report each by name with evidence)

The phase agent MUST verify each of the following BEFORE reporting completion. The agent's completion report MUST include a checklist-results block with:
- Item name (verbatim from below)
- PASS / FAIL / SKIPPED-WITH-REASON
- Evidence (file:line citation, grep result, file size, assertion that ran, etc.)

"All checks passed" without per-item evidence is NOT acceptable.

### Required output files
1. **Draft_Bid.pdf** exists in `outputs/bid/` — evidence: `ls -la {folder}/outputs/bid/Draft_Bid.pdf` showing size > 10,240 bytes
2. **6+ individual volume PDFs** exist in `outputs/bid/` (one per bid section) — evidence: `ls {folder}/outputs/bid/*.pdf | wc -l` >= 7 (including Draft_Bid.pdf)
3. **EXECUTIVE_SUMMARY.pdf** exists in `outputs/bid/` — evidence: `ls -la` showing size > 10,240 bytes

### Schema fidelity
4. **All PDFs are > 10KB (not empty/corrupt)** — evidence: for each PDF in outputs/bid/, print name and size; flag any < 10,240 bytes as FAIL
5. **Every PDF in `outputs/bid/` < 25 MB** (procurement portal cap) — evidence: `ls -la outputs/bid/*.pdf` showing no file exceeds 26,214,400 bytes
6. **Assembly report written (`assembly-report.json`)** — evidence: `ls -la {folder}/outputs/bid/assembly-report.json` size > 100 bytes
7. No `[:N]` slicing applied to deliverable content strings — evidence: grep for `\[:[0-9]+\]` in production code paths returned 0 hits

### Cross-stage consistency
8. **Risk register PDF orientation = landscape** — evidence: open 04_RISK_REGISTER PDF with PyMuPDF: `assert page.rect.width > page.rect.height` for every page; also confirm "Owner", "Verification", "Status" strings all appear in rendered text
9. **Every PDF's image count matches figure-registry expected count** — evidence: for each volume PDF, use PyMuPDF to count embedded images vs figure-registry.json figure count for that section; report any mismatch
10. **QA Validation passed (Step 6b)** — no BLOCKER issues (CORRUPT, EMPTY, TOO_SMALL) — evidence: print qa_issues list; must be empty for BLOCKER-severity items
11. **Content validation passed** — zero editorial artifacts ([USER INPUT REQUIRED], **[Theme]**, **WIN THEME**, **REVIEW REQUIRED**, TODO/TBD/PLACEHOLDER) in deliverable content — evidence: grep each pattern across all bid-section .md files; report any found

### Anti-regression rules (universal)
12. **UTF-8 encoding** on every `open()` call — evidence: search this phase's emitted scripts/code for `encoding='utf-8'` in every file-open
13. **ensure_ascii=False** on every `json.dump` call — evidence: same grep
14. **No ghost fills** — CSS has zero `background-color` on block elements (th, td, blockquote, pre, code) — evidence: grep `background-color` in PROFESSIONAL_CSS applied to this phase's fitz.Story sections returned 0 matches
15. **No `_Showing N of M_` row-cap notices** in any deliverable markdown — evidence: grep returned 0 matches across all bid-section input files
16. **No mid-word table-cell truncations** — evidence: line-by-line cell-end check returned 0 hits

### Memory discipline
17. **Relevant SAFS memory entries reviewed and applied** — evidence: list which memory files were read and which rules were applicable (e.g., "fitz.Story CSS — NEVER background-color on blocks, NEVER em dashes — applied; landscape for Risk Register — applied")

**⚠️ THIS PHASE IS NON-NEGOTIABLE. The entire pipeline exists to produce PDF deliverables for humans.**
**If this phase fails or is skipped, the pipeline output is INCOMPLETE and UNUSABLE.**
**Markdown files are intermediate artifacts. PDFs are the deliverable.**
