---
name: phase1-flatten-win
expert-role: Document Processing Specialist
domain-expertise: PDF/DOCX/XLSX parsing, text extraction, OCR
skill: document-processor
---

# Phase 1: Document Flattening

---

## ⚠️ CRITICAL: PDF Handling Warning

**NEVER use Claude's native Read tool on PDF files.**
**NEVER use pdfplumber - it is BANNED from this skill.**

Claude's Read tool has an undocumented ~1MB limit for binary PDF files. This causes:
- "PDF too large" errors that **HALT ALL PROGRESS**
- Silent truncation of content
- Inconsistent failures that are unrecoverable

**pdfplumber is completely banned** because it invokes Claude's Read tool internally, triggering the same ~1MB limit failures.

**MANDATORY:** All PDFs must be converted using `markitdown` first.

## ⛔ ENCODING DISCIPLINE (codified 2026-05-20 — MARS U+FFFD propagation incident)

The MARS run on 2026-05-20 produced a flattened `Attachment-A-Sample-XaaS-Contract-MARS.md` containing 1,789 U+FFFD (`�`) replacement characters where the source PDF used curly apostrophes (U+2019 `'`), curly quotes (U+201C/D `"` `"`), and em-dashes (U+2014 `—`). The corruption propagated downstream into COMPLIANCE_MATRIX.json (Phase 1.7), domain-context.json (Phase 1.5 `?rm` for `firm`), and CLARIFYING_QUESTIONS.md (Phase 1.85 Q-I11 evidence snippet). Defense-in-depth scrubbing at every consumer phase is now codified in `skill-win.md` MOJIBAKE SCRUB, but **the proper fix lives here at the source.**

**Three mandatory rules:**

1. **Every read of an original document MUST use `encoding='utf-8', errors='strict'`.** Never use `errors='replace'` (which silently substitutes U+FFFD on any decode failure, producing the exact corruption pattern observed in MARS). Never use `errors='ignore'` (which silently drops bytes). `errors='strict'` raises on any decode failure — the right behavior because it forces the agent to diagnose and pick the correct codec rather than ship corrupted bytes downstream.

2. **Every write of a flattened .md MUST use `encoding='utf-8', newline='\n'`.** No platform-default encoding (Windows defaults to cp1252, which corrupts em dashes). No CRLF (breaks byte-level diffs across machines).

3. **Post-extraction U+FFFD check.** After `markitdown` produces the markdown text, count U+FFFD in the result. If `text.count('�') > 0`:
   - Log the count and the first 5 indices to `pipeline_metadata.flatten_audit[]`.
   - Attempt a re-extraction with an alternative tool (`pdftotext -layout`, PyMuPDF/`fitz` text mode, or OCR fallback) and keep whichever extraction has fewer U+FFFD.
   - If ALL extractors produce U+FFFD in the same positions, log the document as "upstream PDF data loss — unrecoverable" and continue. Downstream phases will see the audit entry and know to apply defense-in-depth scrubbing rather than blame their own logic.

```python
def safe_read(path):
    """Read text with UTF-8 + strict error handling. Raise on decode failure."""
    with open(path, "r", encoding="utf-8", errors="strict") as f:
        return f.read()

def safe_write_flatten(path, text):
    """Write flattened markdown with explicit UTF-8 + LF + a post-write audit."""
    ufffd_count = text.count("�")
    if ufffd_count > 0:
        # Surface the first 5 occurrence indices so re-extraction or downstream
        # consumers can act on the audit.
        indices = []
        idx = 0
        for _ in range(5):
            idx = text.find("�", idx + 1)
            if idx < 0:
                break
            indices.append(idx)
        log(f"⚠️  FLATTEN AUDIT: {path} contains {ufffd_count} U+FFFD chars at "
            f"indices {indices} — consider re-extracting with alt tool")
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write(text)
    return ufffd_count
```

**Why `errors='strict'` is non-negotiable:** Once U+FFFD is in the flattened markdown, the original character is **gone**. Every downstream phase that consumes that text must either guess (often wrong) or accept the corruption. A decode failure at flatten time, by contrast, lets the agent pick a better codec or fall back to OCR while the original bytes are still intact. The cost of a noisy strict-decode failure is a single retry; the cost of silent U+FFFD propagation is corruption spread across 5+ phases (as MARS demonstrated).

### Why markitdown?

- **Already installed** (v0.1.3) - no additional dependencies
- **No size limits** - handles large PDFs reliably
- **Multi-format support** - PDF, DOCX, XLSX, PPTX, images, and more
- **Better extraction** - uses pdfminer under the hood

### Fallback Chain

| Tier | Method | Use When |
|------|--------|----------|
| **Tier 0** | `markitdown` | Primary - handles all sizes, no limits |
| **Tier 1** | Claude Vision OCR | Scanned PDFs only (if markitdown returns empty) |
| **Tier 2** | Quarantine | Log error, move to `failed/`, continue other docs |

### Tool Summary

| Tool | Status | Reason |
|------|--------|--------|
| `pdfplumber` | **🚫 BANNED** | Causes ~1MB limit errors, halts progress |
| `markitdown` | **✅ MANDATORY** | No size limits, handles all formats |
| `openpyxl` | ✅ Allowed | XLSX fallback if markitdown unavailable |
| `python-docx` | ✅ Allowed | DOCX fallback if markitdown unavailable |

---

## Purpose

Convert all RFP documents from their native formats to structured markdown for downstream processing.

## Inputs

- `{folder}/original/` - Directory containing source documents
- `{folder}/shared/source-manifest.json` - List of documents to process

## Required Outputs

- `{folder}/flattened/*.md` - One markdown file per source document
- `{folder}/shared/flatten-results.json` - Processing results and metadata

## Instructions

### Step 1: Load Document List

```python
manifest = read_json(f"{folder}/shared/source-manifest.json")
documents = manifest["documents"]
```

### Step 2: Process Each Document by Type

For each document, apply the appropriate extraction method:

#### DOCX Files

```python
from docx import Document

def flatten_docx(path):
    doc = Document(path)
    content = []

    for para in doc.paragraphs:
        style = para.style.name
        text = para.text.strip()

        if not text:
            continue

        # Preserve heading structure
        if style.startswith('Heading'):
            level = int(style[-1]) if style[-1].isdigit() else 1
            content.append(f"{'#' * level} {text}")
        else:
            content.append(text)

    # Process tables
    for table in doc.tables:
        content.append(table_to_markdown(table))

    return '\n\n'.join(content)
```

#### XLSX Files

```python
import openpyxl

def flatten_xlsx(path):
    wb = openpyxl.load_workbook(path, data_only=True)
    content = []

    for sheet_name in wb.sheetnames:
        sheet = wb[sheet_name]
        content.append(f"## Sheet: {sheet_name}")

        # Extract as markdown table
        rows = list(sheet.iter_rows(values_only=True))
        if rows:
            content.append(rows_to_markdown_table(rows))

    return '\n\n'.join(content)
```

#### PDF Files

**⚠️ CRITICAL: NEVER use pdfplumber or Claude's Read tool on PDFs - they have a ~1MB limit that halts progress.**

```python
import subprocess
import os

def flatten_pdf(path):
    """
    Convert PDF using markitdown.

    CRITICAL: Never use pdfplumber or Claude's Read tool on PDFs.
    They have a ~1MB limit that causes "PDF too large" errors
    and halts all processing progress.
    """
    base_name = os.path.splitext(path)[0]
    output_path = f"{base_name}.md"

    # Tier 0: markitdown (primary - no size limits)
    try:
        result = subprocess.run(
            ['markitdown', path, '-o', output_path],
            capture_output=True, text=True, timeout=120
        )

        if result.returncode == 0 and os.path.exists(output_path):
            with open(output_path, 'r', encoding='utf-8') as f:
                content = f.read()
            if len(content) > 100:  # Sanity check for non-empty extraction
                return content
            # If content is too short, might be scanned PDF - try Tier 1
    except subprocess.TimeoutExpired:
        log(f"markitdown timeout for {path}")
    except Exception as e:
        log(f"markitdown failed for {path}: {e}")

    # Tier 1: Claude Vision OCR (for scanned PDFs only)
    # Only attempt if markitdown returned empty/minimal content
    # This uses Claude's vision capability on rendered pages, NOT the Read tool
    log(f"Attempting Vision OCR for scanned PDF: {path}")
    # [Vision OCR implementation - render pages as images, use Claude vision]

    # Tier 2: Quarantine - move to failed folder, continue processing other docs
    failed_dir = os.path.dirname(path).replace('/original', '/original/failed')
    os.makedirs(failed_dir, exist_ok=True)
    quarantine_path = os.path.join(failed_dir, os.path.basename(path))

    log(f"⚠️ PDF conversion failed, quarantining: {path} → {quarantine_path}")
    # Note: In actual execution, move the file and raise to skip this document
    raise ConversionError(f"PDF conversion failed after all tiers, quarantined: {quarantine_path}")
```

### Step 3: Normalize and Clean Text

```python
def normalize_text(content):
    """Clean and normalize extracted text."""
    # Fix common OCR issues
    content = content.replace('|', 'I')  # Common OCR mistake
    content = re.sub(r'\s+', ' ', content)  # Collapse whitespace
    content = re.sub(r'\n{3,}', '\n\n', content)  # Max 2 newlines

    # Preserve structure markers
    content = re.sub(r'^(\d+\.)', r'\n\1', content, flags=re.MULTILINE)

    return content.strip()
```

### Step 4: Write Output Files

```python
results = {
    "processed_at": datetime.now().isoformat(),
    "documents": [],
    "summary": {"success": 0, "partial": 0, "failed": 0}
}

for doc in documents:
    source_path = f"{folder}/original/{os.path.basename(doc['current_path'])}"
    ext = os.path.splitext(source_path)[1].lower()
    base_name = os.path.splitext(os.path.basename(source_path))[0]
    output_path = f"{folder}/flattened/{base_name}.md"

    try:
        if ext == '.docx':
            content = flatten_docx(source_path)
        elif ext in ['.xlsx', '.xls']:
            content = flatten_xlsx(source_path)
        elif ext == '.pdf':
            content = flatten_pdf(source_path)
        else:
            content = read_file(source_path)

        # Add source header
        header = f"""---
source: {doc['original_path']}
processed: {datetime.now().isoformat()}
---

"""
        write_file(output_path, header + normalize_text(content))

        results["documents"].append({
            "source": doc['original_path'],
            "output": output_path,
            "status": "success",
            "size_bytes": len(content)
        })
        results["summary"]["success"] += 1

    except Exception as e:
        results["documents"].append({
            "source": doc['original_path'],
            "status": "failed",
            "error": str(e)
        })
        results["summary"]["failed"] += 1

write_json(f"{folder}/shared/flatten-results.json", results)
```

### Step 5: Report Results

```
📄 Document Flattening Complete
================================
Total documents: {len(documents)}
  ✅ Success: {results["summary"]["success"]}
  ⚠️ Partial: {results["summary"]["partial"]}
  ❌ Failed:  {results["summary"]["failed"]}

Output directory: {folder}/flattened/
```

## Table Conversion Utilities

```python
def table_to_markdown(table):
    """Convert table data to markdown format."""
    if not table or not table[0]:
        return ""

    # Header row
    header = table[0]
    separator = ['---'] * len(header)
    rows = table[1:]

    lines = [
        '| ' + ' | '.join(str(cell or '') for cell in header) + ' |',
        '| ' + ' | '.join(separator) + ' |'
    ]

    for row in rows:
        lines.append('| ' + ' | '.join(str(cell or '') for cell in row) + ' |')

    return '\n'.join(lines)
```

## Mid-Word Reconstruction Discipline

**Problem:** PDF-to-markdown extraction via markitdown/pdfminer produces line breaks at arbitrary positions. When a line ends with a hyphenated word (e.g., `tamper-\n`) the extraction preserves the newline, leaving the text fragment `tamper-` as a standalone string. When Phase 2 extracts requirement text, it ingests the truncated value and propagates it to all downstream consumers (RISKS.json, UNIFIED_RTM.json, bid sections, PDFs).

**Root cause:** markitdown follows PDF text flow, which breaks lines at display boundaries. Hyphenated compounds (`tamper-proof`, `multi-tenant`) and sentence-ending fragments (`preventive mea\nsures`) appear split across lines.

**MANDATORY: Apply mid-word reconstruction BEFORE writing to `flattened/*.md`**

Add this post-processing step immediately after `markitdown` output is read:

```python
import re

def reconstruct_midword_breaks(text):
    """
    Join lines where a word is split by PDF line-break hyphenation or
    arbitrary line breaks mid-sentence.

    Two patterns:
    1. Hyphenated line-end: "word-\nmore"  → "word-more"
       A line ending with lowercase letters + hyphen, followed by a line
       starting with lowercase letters = hyphenated compound broken across lines.
    2. Bare mid-word break: "preventive mea\nsures" → "preventive measures"
       A line ending with lowercase letters (no punctuation), followed by a
       line starting with lowercase letters (no space, suggesting continuation).

    SAFE guard: Only join when both halves together form a plausible word.
    Do NOT join when the next line starts a new sentence (uppercase) or is
    a numbered list item or heading.
    """
    # Pattern 1: hyphen at line end + continuation on next line
    # "tamper-\nproof" → "tamper-proof"
    text = re.sub(r'([a-z]+-)\n([a-z])', r'\1\2', text)

    # Pattern 2: bare lowercase line end + lowercase line start (mid-word split)
    # "preventive mea\nsures" → "preventive measures"
    # Only join if the next line starts with 2+ lowercase chars (word continuation)
    # and the current line ends with 2+ lowercase chars (not a complete word ending)
    text = re.sub(r'([a-z]{2,})\n([a-z]{2,})', lambda m: (
        m.group(1) + m.group(2)  # join: mid-word split
        if not m.group(2)[0].isupper()
        else m.group(1) + '\n' + m.group(2)  # keep: next line is new sentence
    ), text)

    return text
```

**Apply in `flatten_pdf` after reading markitdown output:**

```python
if len(content) > 100:
    content = reconstruct_midword_breaks(content)  # REQUIRED: fix PDF line-break artifacts
    return content
```

**Verification gate:** After writing all `flattened/*.md` files, run this check:

```python
import re, glob

issues = []
for fpath in glob.glob(f"{folder}/flattened/*.md"):
    with open(fpath, encoding='utf-8') as f:
        lines = f.readlines()
    for i, line in enumerate(lines, 1):
        stripped = line.rstrip('\n')
        # Flag: line ends with lowercase+hyphen (should have been joined)
        if re.search(r'[a-z]-$', stripped):
            issues.append(f"{fpath}:{i}: hyphen-end detected: {stripped[-40:]}")

if issues:
    log(f"WARNING: {len(issues)} mid-word break artifacts after reconstruction:")
    for issue in issues[:10]:
        log(f"  {issue}")
else:
    log("Mid-word reconstruction: CLEAN (0 hyphen-end artifacts)")
```

## Quality Checklist (MANDATORY — report each by name with evidence)

The phase agent MUST verify each of the following BEFORE reporting completion. The agent's completion report MUST include a checklist-results block with:
- Item name (verbatim from below)
- PASS / FAIL / SKIPPED-WITH-REASON
- Evidence (file:line citation, grep result, file size, assertion that ran, etc.)

"All checks passed" without per-item evidence is NOT acceptable.

### Required output files
1. **flatten-results.json** exists at `{folder}/shared/flatten-results.json` — evidence: `ls -la` size > 200 bytes and parses as valid JSON
2. **Flattened .md files** — every document in `{folder}/original/` (not in failed/) has a corresponding `.md` in `{folder}/flattened/` — evidence: count comparison `ls original/ | grep -v failed` vs `ls flattened/*.md`

### Schema fidelity
3. **flatten-results.json top-level keys** include `processed_at`, `documents`, `summary` — evidence: list actual top-level keys found
4. **summary** contains `success`, `partial`, `failed` counts — evidence: print summary block values
5. No `[:N]` slicing applied to deliverable content strings — evidence: grep for `\[:[0-9]+\]` in production code paths returned 0 hits

### Cross-stage consistency
6. **Mid-word reconstruction applied** — zero hyphen-end artifacts in flattened/*.md — evidence: grep `[a-z]-$` across all flattened files returned 0 matches (or list the count with file:line for any remaining)
7. **YAML frontmatter present** in every .md file (lines 1-5 contain `---` and `source:`) — evidence: spot-check first 5 lines of 3 random .md files

### Anti-regression rules (universal)
8. **UTF-8 encoding** on every `open()` call — evidence: search this phase's emitted scripts/code for `encoding='utf-8'` in every file-open
9. **ensure_ascii=False** on every `json.dump` call — evidence: same grep
10. **No `_Showing N of M_` row-cap notices** in any deliverable markdown — evidence: grep returned 0 matches
11. **No empty `|  |` mitigation/cell patterns** in any deliverable table — evidence: grep returned 0 matches in cells with HIGH/MEDIUM/CRITICAL severity rows
12. **No mid-word table-cell truncations** — evidence: line-by-line cell-end check returned 0 hits

### Memory discipline
13. **Relevant SAFS memory entries reviewed and applied** — evidence: list which memory files were read and which rules were applicable (e.g., "pdfplumber BANNED — used markitdown Tier 0 as mandated")
