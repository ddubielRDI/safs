---
name: phase1.8-submission-win
expert-role: Procurement Analyst
domain-expertise: RFP submission requirements, proposal packaging, government procurement formatting
skill: procurement-analyst
---

# Phase 1.8: Submission Structure Detection

## Purpose

Parse the flattened RFP documents to extract the exact submission structure required: what files/volumes to submit, naming conventions, page limits, and format requirements. This enables the pipeline to generate output files that match the evaluator's expectations exactly.

## Inputs

- `{folder}/flattened/*.md` - All flattened RFP documents
- `{folder}/shared/COMPLIANCE_MATRIX.json` - Mandatory items (may reference submission requirements)
- `{folder}/shared/domain-context.json` - Domain context

## Required Output

- `{folder}/shared/SUBMISSION_STRUCTURE.json` (>2KB)

## Instructions

### Step 1: Load Flattened RFP Documents

```python
import re
import glob

flattened_files = sorted(glob.glob(f"{folder}/flattened/*.md"))
combined_text = ""
for fp in flattened_files:
    combined_text += read_file(fp) + "\n\n"

compliance = read_json(f"{folder}/shared/COMPLIANCE_MATRIX.json")
domain = read_json(f"{folder}/shared/domain-context.json")
```

### Step 2: Extract Submission Instructions

Search for submission/packaging sections in the RFP text.

```python
# Common headings that contain submission structure
submission_patterns = [
    r'(?:submission|proposal)\s+(?:requirements?|instructions?|format|packaging|organization)',
    r'(?:volume|section|tab)\s+(?:structure|organization|format)',
    r'(?:required\s+)?(?:proposal\s+)?(?:contents?|components?|documents?)',
    r'(?:how\s+to\s+submit|submittal\s+requirements)',
    r'proposal\s+format\s+and\s+content'
]

# Find the section containing submission instructions
submission_section = ""
for pattern in submission_patterns:
    matches = list(re.finditer(pattern, combined_text, re.IGNORECASE))
    for match in matches:
        # Extract surrounding context (up to 5000 chars after the match)
        start = max(0, match.start() - 200)
        end = min(len(combined_text), match.end() + 5000)
        submission_section += combined_text[start:end] + "\n---\n"
```

### Step 3: Parse Required Volumes/Files

```python
volumes = []

# Pattern 1: Numbered volumes/sections (e.g., "Volume 1: Management Proposal")
vol_pattern = r'(?:volume|section|tab|part)\s*(\d+)[:\s\-]+([^\n]+)'
for match in re.finditer(vol_pattern, submission_section, re.IGNORECASE):
    volumes.append({
        "order": int(match.group(1)),
        "title": match.group(2).strip().rstrip('.'),
        "detection_method": "numbered_volume"
    })

# Pattern 2: Lettered submissions (e.g., "a. Letter of Submittal")
letter_pattern = r'(?:^|\n)\s*([a-z])[.)]\s+([A-Z][^\n]+)'
for match in re.finditer(letter_pattern, submission_section):
    order = ord(match.group(1)) - ord('a') + 1
    volumes.append({
        "order": order,
        "title": match.group(2).strip().rstrip('.'),
        "detection_method": "lettered_item"
    })

# Pattern 3: File naming conventions (e.g., "{Bidder}_1_SUBMITTAL.pdf")
naming_pattern = r'[{<]\s*(?:bidder|vendor|offeror)[}\s>]*[_\-]\d+[_\-]([A-Z_]+)\.(?:pdf|docx?)'
for match in re.finditer(naming_pattern, submission_section, re.IGNORECASE):
    volumes.append({
        "title": match.group(1).replace('_', ' ').title(),
        "filename_template": match.group(0),
        "detection_method": "filename_pattern"
    })

# Deduplicate by title similarity
seen_titles = set()
unique_volumes = []
for v in volumes:
    title_key = re.sub(r'\s+', ' ', v["title"].lower().strip())
    if title_key not in seen_titles:
        seen_titles.add(title_key)
        unique_volumes.append(v)

volumes = sorted(unique_volumes, key=lambda v: v.get("order", 99))
```

### Step 4: Extract Page Limits

```python
# Search for page limit specifications
page_limit_pattern = r'(?:(?:not?\s+(?:to\s+)?exceed|maximum|max|limit(?:ed)?\s+to|shall\s+not\s+exceed)\s+(\d+)\s+(?:pages?|pp?))'
general_limits = re.findall(page_limit_pattern, submission_section, re.IGNORECASE)

# Per-volume page limits
for i, vol in enumerate(volumes):
    vol_title = vol["title"].lower()
    # Search near the volume title for page limits
    for match in re.finditer(re.escape(vol["title"]), submission_section, re.IGNORECASE):
        nearby = submission_section[match.start():match.start()+500]
        limit_match = re.search(r'(\d+)\s*(?:pages?|pp?)\s*(?:max|limit|maximum)?', nearby, re.IGNORECASE)
        if limit_match:
            volumes[i]["page_limit"] = int(limit_match.group(1))

# Overall page limit
overall_limit = None
if general_limits:
    overall_limit = max(int(l) for l in general_limits)
```

### Step 5: Extract Format Requirements

```python
format_requirements = {
    "file_format": "PDF",  # Default
    "font": None,
    "font_size": None,
    "margins": None,
    "line_spacing": None,
    "paper_size": "Letter"
}

# Font requirements
font_pattern = r'(?:font|typeface)[:\s]+([A-Z][a-z]+(?:\s+[A-Z][a-z]+)*)'
font_match = re.search(font_pattern, submission_section)
if font_match:
    format_requirements["font"] = font_match.group(1)

# Font size
size_pattern = r'(\d{1,2})\s*(?:pt|point)\s*(?:font|type|minimum)'
size_match = re.search(size_pattern, submission_section, re.IGNORECASE)
if size_match:
    format_requirements["font_size"] = f"{size_match.group(1)}pt"

# Margins
margin_pattern = r'(?:margins?)[:\s]+(\d+(?:\.\d+)?)\s*(?:inch|in|")'
margin_match = re.search(margin_pattern, submission_section, re.IGNORECASE)
if margin_match:
    format_requirements["margins"] = f"{margin_match.group(1)}in"

# Line spacing
spacing_pattern = r'(?:single|double|1\.5|1\.0)\s*(?:spac(?:ed|ing))'
spacing_match = re.search(spacing_pattern, submission_section, re.IGNORECASE)
if spacing_match:
    format_requirements["line_spacing"] = spacing_match.group(0).strip()

# File format
if re.search(r'\.docx?\b', submission_section, re.IGNORECASE):
    format_requirements["file_format"] = "DOCX"
```

### Step 6: Extract Naming Convention

```python
naming_convention = {
    "pattern": "{Bidder}_{Volume}_{Title}.pdf",
    "bidder_placeholder": "{Bidder}",
    "detected": False
}

# Look for naming patterns
name_patterns = [
    r'(?:name|label|title)\s+(?:each\s+)?(?:file|document|volume)\s+(?:as|using)[:\s]+([^\n]+)',
    r'(?:file\s+)?naming\s+convention[:\s]+([^\n]+)',
    r'[{<](?:bidder|vendor)[}>][_\-].*\.(?:pdf|docx?)'
]

for pattern in name_patterns:
    match = re.search(pattern, submission_section, re.IGNORECASE)
    if match:
        naming_convention["pattern"] = match.group(0).strip() if match.lastindex == 0 else match.group(1).strip()
        naming_convention["detected"] = True
        break
```

### Step 7: Build Default Structure (if detection found minimal results)

```python
if len(volumes) < 3:
    # Generate default structure based on common government RFP patterns
    default_volumes = [
        {"order": 1, "title": "Letter of Submittal", "required": True, "content_type": "cover_letter"},
        {"order": 2, "title": "Management Proposal", "required": True, "content_type": "management"},
        {"order": 3, "title": "Technical Approach", "required": True, "content_type": "technical"},
        {"order": 4, "title": "Business Solution", "required": True, "content_type": "solution"},
        {"order": 5, "title": "Cost Proposal", "required": True, "content_type": "financial"}
    ]

    # Merge detected with defaults
    detected_titles = {v["title"].lower() for v in volumes}
    for dv in default_volumes:
        if dv["title"].lower() not in detected_titles:
            dv["detection_method"] = "default_template"
            volumes.append(dv)

    volumes = sorted(volumes, key=lambda v: v.get("order", 99))
```

### Step 8: Assign Content Mapping

```python
# Map each volume to pipeline output phases
content_mapping = {
    "letter": {"phase": "8.1", "output_pattern": "01_SUBMITTAL.md"},
    "submittal": {"phase": "8.1", "output_pattern": "01_SUBMITTAL.md"},
    "management": {"phase": "8.2", "output_pattern": "02_MANAGEMENT.md"},
    "technical": {"phase": "8.3", "output_pattern": "03_TECHNICAL.md"},
    "solution": {"phase": "8.4", "output_pattern": "04_SOLUTION.md"},
    "business": {"phase": "8.4", "output_pattern": "04_SOLUTION.md"},
    "requirements": {"phase": "8.4r", "output_pattern": "04_REQUIREMENTS_REVIEW.md"},
    "risk": {"phase": "8.4k", "output_pattern": "04_RISK_REGISTER.md"},
    "cost": {"phase": "8.5", "output_pattern": "05_FINANCIAL.md"},
    "financial": {"phase": "8.5", "output_pattern": "05_FINANCIAL.md"},
    "price": {"phase": "8.5", "output_pattern": "05_FINANCIAL.md"},
    "integration": {"phase": "8.6", "output_pattern": "06_INTEGRATION.md"}
}

for i, vol in enumerate(volumes):
    title_lower = vol["title"].lower()
    for keyword, mapping in content_mapping.items():
        if keyword in title_lower:
            volumes[i]["mapped_phase"] = mapping["phase"]
            volumes[i]["output_file"] = f"outputs/bid-sections/{mapping['output_pattern']}"
            break
```

### Step 9: Write SUBMISSION_STRUCTURE.json

```python
structure = {
    "detected_at": datetime.now().isoformat(),
    "detection_confidence": "high" if any(v.get("detection_method") != "default_template" for v in volumes) else "low",
    "source_sections_analyzed": len(flattened_files),

    "volumes": volumes,

    "format_requirements": format_requirements,
    "naming_convention": naming_convention,
    "overall_page_limit": overall_limit,

    "assembly_instructions": {
        "output_directory": "outputs/bid/",
        "source_directory": "outputs/bid-sections/",
        "assembly_order": [v["title"] for v in volumes],
        "bidder_name": "Resource Data, Inc."
    }
}

write_json(f"{folder}/shared/SUBMISSION_STRUCTURE.json", structure)
```

### Step 10: Report Results

```
📋 SUBMISSION STRUCTURE DETECTED (Phase 1.8)
=============================================
Confidence: {detection_confidence}
Volumes: {len(volumes)}

Required Submissions:
{for each volume: order + title + page_limit + mapped_phase}

Format Requirements:
  File Format: {file_format}
  Font: {font or "Not specified"}
  Page Size: {paper_size}
  Margins: {margins or "Not specified"}

Naming: {naming_convention["pattern"]}

Output: shared/SUBMISSION_STRUCTURE.json
```

## Quality Checklist

- [ ] `SUBMISSION_STRUCTURE.json` written (>2KB)
- [ ] At least 3 volumes identified
- [ ] Page limits extracted where specified
- [ ] Format requirements captured
- [ ] Naming convention detected or defaulted
- [ ] Each volume mapped to a pipeline phase where possible
