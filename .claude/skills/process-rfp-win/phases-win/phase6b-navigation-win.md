---
name: phase6b-navigation-win
expert-role: Technical Writer
domain-expertise: User guides, navigation aids
---

# Phase 6b: Navigation Guide

## Expert Role

You are a **Technical Writer** with deep expertise in:
- User guide creation
- Document navigation design
- Cross-reference systems
- Information architecture

## Purpose

Generate NAVIGATION_GUIDE.md to help users find information quickly.

## Inputs

- All `{folder}/outputs/*.md` files
- `{folder}/shared/requirements-normalized.json`

## Required Outputs

- `{folder}/outputs/NAVIGATION_GUIDE.md`

## Instructions

### Step 1: Catalog All Documents

```python
import glob

output_files = glob.glob(f"{folder}/outputs/*.md")
documents = []

for file_path in output_files:
    name = os.path.basename(file_path)
    content = read_file(file_path)
    word_count = len(content.split())
    documents.append({
        "name": name,
        "path": file_path,
        "word_count": word_count,
        "sections": extract_sections(content)
    })

def extract_sections(content):
    """Extract H2 sections from markdown."""
    sections = re.findall(r'^## (.+)$', content, re.MULTILINE)
    return sections[:10]
```

### Step 2: Define Use Cases

```python
USE_CASES = [
    {
        "title": "Understanding the Requirements",
        "description": "Get a comprehensive view of all RFP requirements",
        "documents": ["REQUIREMENTS_CATALOG.md", "TRACEABILITY.md"],
        "key_sections": ["Requirements by Category", "Full Traceability Matrix"]
    },
    {
        "title": "Technical Architecture Review",
        "description": "Review the proposed technical solution",
        "documents": ["ARCHITECTURE.md", "SECURITY_REQUIREMENTS.md", "INTEROPERABILITY.md"],
        "key_sections": ["Architecture Overview", "Technology Stack", "Security Architecture"]
    },
    {
        "title": "Estimating Project Effort",
        "description": "Understand effort estimates and resource needs",
        "documents": ["EFFORT_ESTIMATION.md", "REQUIREMENT_RISKS.md"],
        "key_sections": ["Executive Summary", "Resource Plan", "Risk Summary"]
    },
    {
        "title": "Preparing the Bid Response",
        "description": "Gather information for bid document sections",
        "documents": ["EXECUTIVE_SUMMARY.md", "TRACEABILITY.md", "DEMO_SCENARIOS.md"],
        "key_sections": ["Key Findings", "Bid Section Coverage", "Demo Scenarios"]
    },
    {
        "title": "Validating Compliance",
        "description": "Ensure all mandatory requirements are addressed",
        "documents": ["REQUIREMENTS_CATALOG.md", "SECURITY_REQUIREMENTS.md"],
        "key_sections": ["Critical Requirements", "Compliance Requirements"]
    }
]
```

### Step 3: Generate Navigation Guide

```python
def generate_navigation_md(documents, use_cases):
    doc = f"""# Navigation Guide

**Generated:** {datetime.now().strftime('%Y-%m-%d %H:%M')}

This guide helps you quickly find information across all generated documents.

---

## Quick Start

| I want to... | Go to... |
|--------------|----------|
| See all requirements | REQUIREMENTS_CATALOG.md |
| Review architecture | ARCHITECTURE.md |
| Check security specs | SECURITY_REQUIREMENTS.md |
| View estimates | EFFORT_ESTIMATION.md |
| Find risks | REQUIREMENT_RISKS.md |
| Prepare bid content | Draft_Bid.md (in /bid/) |

---

## Document Overview

| Document | Purpose | Sections |
|----------|---------|----------|
"""

    for doc_info in documents:
        sections = len(doc_info["sections"])
        doc += f"| {doc_info['name']} | [See below] | {sections} sections |\n"

    doc += """

---

## Use Case Guides

"""

    for uc in use_cases:
        doc += f"""### {uc["title"]}

**Goal:** {uc["description"]}

**Primary Documents:**
"""
        for doc_name in uc["documents"]:
            doc += f"- {doc_name}\n"

        doc += "\n**Key Sections:**\n"
        for section in uc["key_sections"]:
            doc += f"- {section}\n"

        doc += "\n---\n\n"

    # Document details
    doc += """## Document Details

"""

    for doc_info in sorted(documents, key=lambda x: x["name"]):
        doc += f"""### {doc_info["name"]}

**Word Count:** ~{doc_info["word_count"]:,}

**Sections:**
"""
        for section in doc_info["sections"]:
            doc += f"- {section}\n"

        doc += "\n"

    # Cross-reference guide
    doc += """
---

## Cross-Reference Guide

### Finding Requirements by Topic

| Topic | Documents | Search Terms |
|-------|-----------|--------------|
| Security | SECURITY_REQUIREMENTS.md, REQUIREMENTS_CATALOG.md | SEC, security, auth |
| Integration | INTEROPERABILITY.md, ARCHITECTURE.md | INT, api, interface |
| UI/UX | UI_SPECS.md, DEMO_SCENARIOS.md | UI, screen, display |
| Data | ENTITY_DEFINITIONS.md, REQUIREMENTS_CATALOG.md | data, entity, field |
| Reports | REQUIREMENTS_CATALOG.md | RPT, report, export |

### Tracing Requirements

To trace a requirement through all documents:

1. **Start:** REQUIREMENTS_CATALOG.md - Find the requirement ID
2. **Specs:** TRACEABILITY.md - See which specs address it
3. **Risks:** REQUIREMENT_RISKS.md - Check risk level
4. **Estimate:** EFFORT_ESTIMATION.md - Find effort estimate
5. **Bid:** Check bid section mapping in TRACEABILITY.md

---

## Search Tips

### Finding Specific Requirements

Use Ctrl+F (or Cmd+F) with these patterns:
- Requirement ID: `[001APP]`, `[015SEC]`
- Category code: `APP`, `SEC`, `INT`, `UI`
- Priority: `CRITICAL`, `HIGH`

### Finding Related Information

Documents are linked by:
- Requirement IDs (e.g., `[001APP]`)
- Section cross-references
- Category codes

---

## Document Map

```
EXECUTIVE_SUMMARY.md (Start Here)
├── REQUIREMENTS_CATALOG.md
│   ├── TRACEABILITY.md
│   └── REQUIREMENT_RISKS.md
├── ARCHITECTURE.md
│   ├── SECURITY_REQUIREMENTS.md
│   ├── INTEROPERABILITY.md
│   └── UI_SPECS.md
├── EFFORT_ESTIMATION.md
│   └── REQUIREMENT_RISKS.md
└── bid/Draft_Bid.md (Final Deliverable)
    ├── All above documents
    └── DEMO_SCENARIOS.md
```

---

## Need Help?

If you can't find what you need:
1. Check MANIFEST.md for complete file list
2. Search for keywords across all files
3. Review TRACEABILITY.md for requirement mappings
"""

    return doc

navigation_md = generate_navigation_md(documents, USE_CASES)
write_file(f"{folder}/outputs/NAVIGATION_GUIDE.md", navigation_md)
```

## Quality Checklist

- [ ] `NAVIGATION_GUIDE.md` created in `outputs/`
- [ ] All documents cataloged
- [ ] 5+ use cases defined
- [ ] Cross-reference guide included
- [ ] Document map provided
