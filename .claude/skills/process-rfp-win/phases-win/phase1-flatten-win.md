---
name: phase1-flatten-win
expert-role: Document Processing Specialist
domain-expertise: PDF/DOCX/XLSX parsing, text extraction, OCR
---

# Phase 1: Document Flattening

## Expert Role

You are a **Document Processing Specialist** with deep expertise in:
- PDF text extraction and parsing
- Microsoft Office document formats (DOCX, XLSX, PPTX)
- OCR for scanned documents
- Text normalization and structure preservation

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

## Quality Checklist

- [ ] All documents in `original/` have corresponding `.md` files in `flattened/`
- [ ] `flatten-results.json` created with processing status
- [ ] Tables converted to markdown format
- [ ] Heading structure preserved
- [ ] Source file metadata in YAML frontmatter
- [ ] No binary content in output files
