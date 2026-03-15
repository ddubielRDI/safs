---
name: phase0-intake
expert-role: Document Processing Specialist
domain-expertise: PDF/DOCX/XLSX parsing, text extraction, document organization
skill: document-processor
---

# Phase 0: Folder Setup & Document Intake

## Purpose

Validate folder exists, create `screen/` subdirectory, discover source documents, convert to markdown, and assemble combined text for analysis by downstream phases.

## Inputs

- `{folder}` -- User-provided folder path containing RFP documents
- Documents may be in folder root or `original/` subdirectory

## Required Outputs

- `{folder}/screen/source-manifest.json` -- List of documents found with conversion status
- Combined text stored in memory for downstream phases

## Instructions

### Step 1: Validate and Setup

```python
import os

# Confirm folder exists
if not os.path.exists(folder):
    error(f"ABORT: Folder does not exist: {folder}")
    halt()

# Create screen/ output directory
screen_dir = f"{folder}/screen"
os.makedirs(screen_dir, exist_ok=True)

# Check for existing screen outputs and warn if overwriting
existing_outputs = [f for f in os.listdir(screen_dir) if os.path.isfile(f"{screen_dir}/{f}")]
if existing_outputs:
    log(f"  WARNING: screen/ already contains {len(existing_outputs)} file(s) -- outputs will be overwritten")
    for f in existing_outputs:
        log(f"    - {f}")

log(f"  Folder validated: {folder}")
log(f"  Output directory: {screen_dir}")
```

### Step 2: Discover Source Documents

```python
DOCUMENT_EXTENSIONS = ['.pdf', '.docx', '.xlsx', '.doc', '.xls']

def discover_documents(folder):
    """Search folder root and original/ for RFP documents."""
    documents = []
    search_dirs = [folder]

    # Also search original/ if it exists
    original_dir = f"{folder}/original"
    if os.path.exists(original_dir) and os.path.isdir(original_dir):
        search_dirs.append(original_dir)

    for search_dir in search_dirs:
        for filename in os.listdir(search_dir):
            filepath = f"{search_dir}/{filename}"
            if not os.path.isfile(filepath):
                continue
            ext = os.path.splitext(filename)[1].lower()
            if ext in DOCUMENT_EXTENSIONS:
                documents.append({
                    "filename": filename,
                    "path": filepath,
                    "extension": ext,
                    "size_bytes": os.path.getsize(filepath)
                })

    return documents

documents = discover_documents(folder)

if not documents:
    error("ABORT: No RFP documents found (.pdf, .docx, .xlsx, .doc, .xls)")
    error(f"  Searched: {folder} and {folder}/original/")
    halt()

# Sort by priority: largest PDF first (likely main RFP body),
# then remaining PDFs by size, then other formats by size
def sort_priority(doc):
    is_pdf = 1 if doc["extension"] == ".pdf" else 0
    return (-is_pdf, -doc["size_bytes"])

documents.sort(key=sort_priority)

log(f"  Discovered {len(documents)} document(s):")
for doc in documents:
    size_kb = doc["size_bytes"] / 1024
    log(f"    - {doc['filename']} ({size_kb:.1f} KB) [{doc['extension']}]")
```

### Step 3: Convert Documents to Markdown

```python
import subprocess

SCAN_LIMIT = 80000  # Max chars for combined text

converted_texts = []
success_count = 0
failed_count = 0

for doc in documents:
    filename = doc["filename"]
    filepath = doc["path"]
    ext = doc["extension"]

    log(f"  Converting: {filename}")

    # Primary method: markitdown via Bash
    conversion_method = "markitdown"
    converted_text = None

    try:
        result = subprocess.run(
            ["markitdown", filepath],
            capture_output=True, text=True, timeout=120
        )
        if result.returncode == 0 and len(result.stdout.strip()) > 10:
            converted_text = result.stdout.strip()
            log(f"    markitdown: {len(converted_text)} chars extracted")
        else:
            raise RuntimeError(f"markitdown returned empty or failed (rc={result.returncode})")
    except Exception as e:
        log(f"    markitdown failed: {e}")

        # Fallback for DOCX
        if ext in [".docx", ".doc"] and converted_text is None:
            conversion_method = "python-docx"
            try:
                # python-docx fallback
                from docx import Document as DocxDocument
                docx_doc = DocxDocument(filepath)
                paragraphs = [p.text for p in docx_doc.paragraphs if p.text.strip()]
                converted_text = "\n\n".join(paragraphs)
                log(f"    python-docx fallback: {len(converted_text)} chars extracted")
            except Exception as e2:
                log(f"    python-docx fallback failed: {e2}")

        # Fallback for XLSX
        elif ext in [".xlsx", ".xls"] and converted_text is None:
            conversion_method = "openpyxl"
            try:
                import openpyxl
                wb = openpyxl.load_workbook(filepath, read_only=True, data_only=True)
                lines = []
                for sheet_name in wb.sheetnames:
                    ws = wb[sheet_name]
                    lines.append(f"## Sheet: {sheet_name}\n")
                    for row in ws.iter_rows(values_only=True):
                        row_text = " | ".join(str(cell) if cell is not None else "" for cell in row)
                        if row_text.strip().replace("|", "").strip():
                            lines.append(row_text)
                wb.close()
                converted_text = "\n".join(lines)
                log(f"    openpyxl fallback: {len(converted_text)} chars extracted")
            except Exception as e2:
                log(f"    openpyxl fallback failed: {e2}")

    # Record result
    if converted_text and len(converted_text) > 10:
        doc["conversion_status"] = "success"
        doc["converted_chars"] = len(converted_text)
        doc["conversion_method"] = conversion_method
        converted_texts.append(converted_text)
        success_count += 1
    else:
        doc["conversion_status"] = "failed"
        doc["converted_chars"] = 0
        doc["conversion_method"] = "failed"
        failed_count += 1
        log(f"    FAILED: Could not extract text from {filename}")
```

### Step 4: Assemble Combined Text

```python
# Concatenate all converted markdown in priority order (already sorted)
combined_text_raw = "\n\n---\n\n".join(converted_texts)
total_raw_chars = len(combined_text_raw)

# Apply SCAN_LIMIT
truncated = total_raw_chars > SCAN_LIMIT
if truncated:
    combined_text = combined_text_raw[:SCAN_LIMIT]
    log(f"  Combined text: {total_raw_chars} chars -> truncated to {SCAN_LIMIT} chars (SCAN_LIMIT)")
else:
    combined_text = combined_text_raw
    log(f"  Combined text: {total_raw_chars} chars (within SCAN_LIMIT)")

# Minimum content check
if len(combined_text.strip()) < 500:
    error(f"ABORT: Combined text is only {len(combined_text.strip())} chars -- insufficient content for screening")
    error("  Need at least 500 chars of extractable text to perform meaningful analysis")
    halt()

# Store combined_text in memory for downstream phases
log(f"  Combined text ready for analysis: {len(combined_text)} chars")
```

### Step 5: Write Source Manifest

```python
import json
from datetime import datetime

manifest = {
    "phase": "0",
    "timestamp": datetime.now().isoformat(),
    "folder": folder,
    "documents": [
        {
            "filename": doc["filename"],
            "path": doc["path"],
            "extension": doc["extension"],
            "size_bytes": doc["size_bytes"],
            "conversion_status": doc["conversion_status"],
            "converted_chars": doc["converted_chars"],
            "conversion_method": doc["conversion_method"]
        }
        for doc in documents
    ],
    "total_documents": len(documents),
    "successful_conversions": success_count,
    "failed_conversions": failed_count,
    "combined_text_chars": len(combined_text),
    "scan_limit": SCAN_LIMIT,
    "truncated": truncated
}

manifest_path = f"{folder}/screen/source-manifest.json"
with open(manifest_path, "w") as f:
    json.dump(manifest, f, indent=2)

log(f"  Written: screen/source-manifest.json")
```

### Step 6: Error Handling

```python
# ALL conversions failed -> ABORT
if success_count == 0:
    error("ABORT: All document conversions failed. Cannot proceed with screening.")
    for doc in documents:
        error(f"  - {doc['filename']}: {doc['conversion_method']}")
    halt()

# SOME conversions failed -> WARN, continue
if failed_count > 0:
    log(f"  WARNING: {failed_count} of {len(documents)} conversions failed -- continuing with available text")
    for doc in documents:
        if doc["conversion_status"] == "failed":
            log(f"    FAILED: {doc['filename']}")

# Combined text too short (already checked in Step 4, but re-verify after assembly)
if len(combined_text.strip()) < 500:
    error("ABORT: Insufficient text content for screening (<500 chars)")
    halt()
```

### Step 7: Report

```
DOCUMENT INTAKE COMPLETE (Phase 0)
===================================
Documents found: {total_documents}
Converted: {success_count} | Failed: {failed_count}
Combined text: {combined_text_chars} chars {("(truncated from " + str(total_raw_chars) + ")") if truncated else "(within limit)"}
Output: screen/source-manifest.json
```

## Quality Checklist

- [ ] `screen/` directory created
- [ ] All documents in folder root AND `original/` discovered
- [ ] Each document converted via markitdown with fallback (python-docx for DOCX, openpyxl for XLSX)
- [ ] `source-manifest.json` written with per-document status
- [ ] Combined text assembled in priority order (largest PDF first)
- [ ] SCAN_LIMIT applied (80,000 chars)
- [ ] ABORT if all conversions fail or text < 500 chars
