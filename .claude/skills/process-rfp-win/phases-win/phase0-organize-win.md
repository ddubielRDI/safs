---
name: phase0-organize-win
expert-role: DevOps Engineer
domain-expertise: File systems, directory structures, automation
---

# Phase 0: Folder Organization

## Expert Role

You are a **DevOps Engineer** with deep expertise in:
- File systems and directory structures
- Automation and scripting
- Document organization workflows
- Path handling and validation

## Purpose

Create the standard folder structure and organize RFP documents for processing.

**⚠️ CRITICAL — MOVE, NOT COPY:** All source documents (.docx, .xlsx, .pdf, etc.) MUST be **MOVED** out of the root RFP folder into `original/`. This means `shutil.move()` or copy + delete. After this phase completes, **ZERO document files may remain in the root folder**. If any remain, this phase has FAILED. Verify with a directory listing before reporting success.

## Inputs

- `{folder}` - User-provided folder path containing RFP documents

## Required Outputs

- `{folder}/original/` - Directory containing all source documents
- `{folder}/flattened/` - Directory for converted markdown files
- `{folder}/shared/` - Directory for intermediate JSON files
- `{folder}/shared/validation/` - Directory for SVA validation reports (SVA-1 through SVA-7)
- `{folder}/outputs/` - Directory for final deliverables
- `{folder}/outputs/bid/` - Directory for final rendered PDFs and images
- `{folder}/outputs/bid-sections/` - Directory for multi-file bid section markdown files

## Instructions

### Step 1: Validate Input Folder

```bash
# Verify folder exists
if [ ! -d "{folder}" ]; then
    echo "ERROR: Folder does not exist: {folder}"
    exit 1
fi

# Check write permissions
if [ ! -w "{folder}" ]; then
    echo "WARNING: Folder is read-only. Consider using a writable location."
fi
```

### Step 1b: Security Validation

```python
import os
import re

def validate_path_security(folder_path):
    """Validate folder path for security risks."""
    issues = []

    # 1. Reject path traversal patterns (check for ".." as a path segment, not substring)
    # This avoids false positives on legitimate folder names like "my..folder"
    path_parts = folder_path.replace("\\", "/").split("/")
    if ".." in path_parts:
        issues.append("BLOCKED: Path contains '..' path segment (directory traversal)")

    # 2. Reject null bytes and control characters
    if "\x00" in folder_path or any(ord(c) < 32 for c in folder_path if c not in ('\n', '\r', '\t')):
        issues.append("BLOCKED: Path contains null bytes or control characters")

    # 3. Reject shell metacharacters in folder name (not path separators)
    folder_name = os.path.basename(folder_path)
    dangerous_chars = re.findall(r'[;&|`$(){}!<>]', folder_name)
    if dangerous_chars:
        issues.append(f"BLOCKED: Folder name contains shell metacharacters: {''.join(set(dangerous_chars))}")

    # 4. Reject executable extensions in folder name
    executable_exts = ['.exe', '.bat', '.sh', '.ps1', '.cmd', '.com', '.msi', '.vbs', '.js']
    if any(folder_name.lower().endswith(ext) for ext in executable_exts):
        issues.append(f"BLOCKED: Folder name has executable extension")

    # 5. Resolve to absolute path and verify it's a real directory
    resolved = os.path.realpath(folder_path)
    if resolved != os.path.abspath(folder_path):
        # Path contains symlinks — log warning but allow
        log(f"  Security note: Path resolves through symlink: {folder_path} -> {resolved}")

    return issues

# Run security validation
security_issues = validate_path_security(folder)

if security_issues:
    for issue in security_issues:
        log(f"  🛡️ {issue}")
    raise RuntimeError(f"PHASE 0 SECURITY GATE FAILED: {len(security_issues)} issue(s) detected. Path: {folder}")

log("  🛡️ Security: Path validated — no traversal or injection patterns detected")
```

### Step 2: Create Directory Structure

```bash
mkdir -p "{folder}/original"
mkdir -p "{folder}/flattened"
mkdir -p "{folder}/shared"
mkdir -p "{folder}/shared/validation"
mkdir -p "{folder}/outputs"
mkdir -p "{folder}/outputs/bid"
mkdir -p "{folder}/outputs/bid-sections"
```

### Step 3: Identify Documents to Move

Recursively find all documents (up to 5 levels deep):

```python
DOCUMENT_EXTENSIONS = ['.docx', '.xlsx', '.xls', '.pdf', '.doc', '.pptx', '.ppt', '.txt', '.rtf']
EXCLUDED_DIRS = ['original', 'flattened', 'shared', 'outputs', '.git', 'node_modules', '__pycache__']
MAX_DEPTH = 5

def find_documents(path, depth=0):
    """Recursively find all documents."""
    if depth > MAX_DEPTH:
        return []

    found = []
    for item in listdir(path):
        item_path = f"{path}/{item}"

        if isfile(item_path) and item.lower().endswith(tuple(DOCUMENT_EXTENSIONS)):
            found.append({
                "absolute_path": item_path,
                "original_relative_path": item_path.replace(folder + "/", ""),
                "filename": item,
                "depth": depth
            })
        elif isdir(item_path) and item not in EXCLUDED_DIRS:
            found.extend(find_documents(item_path, depth + 1))

    return found

documents = find_documents(folder)
```

### Step 4: MOVE Documents to original/ (NOT copy — MOVE)

**⚠️ USE `shutil.move()` — this is a MOVE operation, not a copy. The source file MUST NOT exist after this step.**

```python
import shutil
import os

for doc in documents:
    source = doc["absolute_path"]
    dest = f"{folder}/original/{doc['filename']}"

    # Handle filename conflicts
    if exists(dest):
        base, ext = os.path.splitext(doc['filename'])
        dest = f"{folder}/original/{base}_{doc['depth']}{ext}"

    # MOVE the file — source is removed automatically
    shutil.move(source, dest)
    log(f"  MOVED: {doc['original_relative_path']} -> original/{os.path.basename(dest)}")

    # Verify source is gone
    if os.path.exists(source):
        os.remove(source)
        log(f"  ⚠️ Force-deleted lingering source: {source}")
```

### Step 5: VERIFY Root Folder is Clean (HARD GATE — phase fails if not clean)

**⚠️ THIS IS A GATE. If ANY document files remain in the root folder, this phase has FAILED. Do NOT proceed to Phase 1.**

```python
# Scan root folder (not subdirectories) for any remaining document files
remaining = []
for item in os.listdir(folder):
    item_path = f"{folder}/{item}"
    if os.path.isfile(item_path) and item.lower().endswith(tuple(DOCUMENT_EXTENSIONS)):
        remaining.append(item)

if remaining:
    # Force-move any stragglers
    for doc in remaining:
        source = f"{folder}/{doc}"
        dest = f"{folder}/original/{doc}"
        if not os.path.exists(dest):
            shutil.move(source, dest)
        else:
            os.remove(source)
        log(f"  ⚠️ Force-moved straggler: {doc}")

    # Re-verify
    still_remaining = [f for f in os.listdir(folder)
                       if os.path.isfile(f"{folder}/{f}")
                       and f.lower().endswith(tuple(DOCUMENT_EXTENSIONS))]
    if still_remaining:
        raise RuntimeError(f"PHASE 0 FAILED: {len(still_remaining)} documents still in root: {still_remaining}")

log("✅ Root folder clean — all documents moved to original/")

# Also remove any stray npm files that may have been created by previous runs
NPM_JUNK_FILES = ['package.json', 'package-lock.json', 'node_modules']
for junk in NPM_JUNK_FILES:
    junk_path = f"{folder}/{junk}"
    if os.path.exists(junk_path):
        if os.path.isdir(junk_path):
            shutil.rmtree(junk_path)
            log(f"  Removed npm directory: {junk}")
        else:
            os.remove(junk_path)
            log(f"  Removed npm file: {junk}")
```

### Step 5b: Clean npm artifacts from RFP folder

**CRITICAL: npm tools should NEVER create package.json in the RFP folder.**

```python
# Remove any npm artifacts that shouldn't be in the RFP folder
# These are created if npx is run from within the RFP folder (incorrect)
NPM_ARTIFACTS = ['package.json', 'package-lock.json', 'node_modules']

for artifact in NPM_ARTIFACTS:
    # Check root folder
    root_path = f"{folder}/{artifact}"
    if os.path.exists(root_path):
        if os.path.isdir(root_path):
            shutil.rmtree(root_path)
        else:
            os.remove(root_path)
        log(f"⚠️ Removed stray npm artifact from root: {artifact}")

    # Check outputs folder
    outputs_path = f"{folder}/outputs/{artifact}"
    if os.path.exists(outputs_path):
        if os.path.isdir(outputs_path):
            shutil.rmtree(outputs_path)
        else:
            os.remove(outputs_path)
        log(f"⚠️ Removed stray npm artifact from outputs: {artifact}")

    # Check bid folder
    bid_path = f"{folder}/outputs/bid/{artifact}"
    if os.path.exists(bid_path):
        if os.path.isdir(bid_path):
            shutil.rmtree(bid_path)
        else:
            os.remove(bid_path)
        log(f"⚠️ Removed stray npm artifact from bid: {artifact}")
```

### Step 5c: Generate .gitignore for RFP folder

```python
# Prevent accidental git commit of RFP processing artifacts
gitignore_path = f"{folder}/.gitignore"

if not os.path.exists(gitignore_path):
    gitignore_content = """# Auto-generated by process-rfp-win pipeline
# Prevents accidental commit of RFP processing artifacts

# Processing directories
shared/
flattened/
original/

# Output artifacts
outputs/bid/*.pdf
outputs/bid/*.png
outputs/bid/*.mmd

# npm artifacts (should not be here but just in case)
node_modules/
package.json
package-lock.json

# OS files
.DS_Store
Thumbs.db
"""
    write_file(gitignore_path, gitignore_content)
    log("  🛡️ Generated .gitignore to prevent accidental commits")
else:
    log("  .gitignore already exists — skipping generation")
```

### Step 6: Create Source Manifest

```python
manifest = {
    "organized_at": datetime.now().isoformat(),
    "source_folder": folder,
    "documents": [
        {
            "original_path": doc["original_relative_path"],
            "current_path": f"original/{doc['filename']}",
            "depth_found": doc["depth"]
        }
        for doc in documents
    ],
    "document_count": len(documents)
}

write_json(f"{folder}/shared/source-manifest.json", manifest)
```

### Step 7: Report Results

```
📂 Folder Organization Complete
================================
Source folder: {folder}
Documents found: {len(documents)}
  - Root level: {count at depth 0}
  - Nested: {count at depth > 0}

Created directories:
  ✅ original/            ({len(documents)} documents)
  ✅ flattened/           (ready for converted markdown)
  ✅ shared/              (ready for intermediate files)
  ✅ shared/validation/   (ready for SVA validation reports)
  ✅ outputs/             (ready for deliverables)
  ✅ outputs/bid/         (ready for final PDFs and images)
  ✅ outputs/bid-sections/ (ready for multi-file bid section markdown)
```

## Quality Checklist

- [ ] Security validation passed (no path traversal, no shell metacharacters)
- [ ] All 8 directories exist (original, flattened, shared, shared/validation, outputs, outputs/bid, outputs/bid-sections)
- [ ] All documents MOVED to `original/` (copy + delete)
- [ ] **CRITICAL: No document files (.docx, .xlsx, .pdf, etc.) remain in root folder**
- [ ] **CRITICAL: No npm artifacts (package.json, package-lock.json, node_modules) in RFP folder**
- [ ] `.gitignore` generated or verified in RFP folder
- [ ] `source-manifest.json` created in `shared/`
- [ ] Filename conflicts resolved with depth suffix
- [ ] Verification step confirmed root folder is clean
