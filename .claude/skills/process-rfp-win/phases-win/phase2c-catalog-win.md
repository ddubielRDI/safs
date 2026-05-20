---
name: phase2c-catalog-win
expert-role: Technical Writer
domain-expertise: Documentation structure, cataloging
---

# Phase 2c: Requirements Catalog

## Expert Role

You are a **Technical Writer** with deep expertise in:
- Documentation structure and organization
- Requirements cataloging and indexing
- Cross-reference creation
- Professional document formatting

## Purpose

Generate the REQUIREMENTS_CATALOG.md document with structured, indexed requirements.

## Inputs

- `{folder}/shared/requirements-normalized.json` - Normalized requirements
- `{folder}/shared/domain-context.json` - Domain context

## Required Outputs

- `{folder}/outputs/REQUIREMENTS_CATALOG.md` - Human-readable catalog
- `{folder}/shared/REQUIREMENTS_CATALOG.json` - Machine-readable catalog

## Instructions

### Step 1: Load Requirements

```python
requirements = read_json(f"{folder}/shared/requirements-normalized.json")
domain_context = read_json(f"{folder}/shared/domain-context.json")

valid_reqs = requirements.get("requirements", [])
```

### Step 2: Generate Catalog Structure

```python
def generate_catalog_md(requirements, domain_context):
    """Generate markdown requirements catalog."""
    domain = domain_context.get("selected_domain", "Generic")

    # Build document structure
    sections = []

    # Header
    sections.append(f"""# Requirements Catalog

**Domain:** {domain}
**Generated:** {datetime.now().strftime('%Y-%m-%d %H:%M')}
**Total Requirements:** {len(requirements)}

---

## Table of Contents

""")

    # Group by category
    categories = {}
    for req in requirements:
        cat = req.get("category", "OTHER")
        if cat not in categories:
            categories[cat] = []
        categories[cat].append(req)

    # TOC
    for cat in sorted(categories.keys()):
        count = len(categories[cat])
        sections.append(f"- [{cat}: {CATEGORY_NAMES.get(cat, cat)}](#cat-{cat.lower()}) ({count})\n")

    sections.append("\n---\n\n")

    # Category sections
    for cat in sorted(categories.keys()):
        reqs = categories[cat]
        cat_name = CATEGORY_NAMES.get(cat, cat)

        sections.append(f"""## {cat}: {cat_name} {{#cat-{cat.lower()}}}

**Count:** {len(reqs)}

| ID | Requirement | Priority | Source |
|----|-------------|----------|--------|
""")

        for req in sorted(reqs, key=lambda x: x.get("canonical_id", "")):
            req_id = req.get("display_id", req.get("id", "N/A"))
            # NO TRUNCATION on canonical requirement text. HUNT-B-003 fix 2026-05-18:
            # markdown table cells break on embedded newlines (renderer treats \n as
            # row separator) — replace newlines with space before pipe-escape. Order
            # matters: collapse whitespace BEFORE escaping pipes so \n→\\n doesn't
            # double-escape, and AFTER all transforms there are no raw newlines.
            text = req.get("text", "")
            text = text.replace("\r\n", " ").replace("\n", " ").replace("\r", " ")
            text = text.replace("|", "\\|")
            priority = req.get("priority", "MEDIUM")
            source = req.get("source", "manual")[:15]  # source field is a short identifier; truncation safe

            sections.append(f"| {req_id} | {text} | {priority} | {source} |\n")

        sections.append("\n")

        # Detailed view
        sections.append("### Detailed Requirements\n\n")
        for req in sorted(reqs, key=lambda x: x.get("canonical_id", "")):
            req_id = req.get("display_id", req.get("id", "N/A"))
            sections.append(f"""#### {req_id}

**Text:** {req.get("text", "")}

**Priority:** {req.get("priority", "MEDIUM")}
**Type:** {req.get("type", "UNKNOWN")}
**Source:** {req.get("source", "manual")}
**Validation Score:** {req.get("validation", {}).get("score", "N/A")}

---

""")

    return ''.join(sections)

CATEGORY_NAMES = {
    "APP": "Application/Data Collection",
    "ENR": "Enrollment",
    "BUD": "Budget/Financial",
    "STF": "Staff/Personnel",
    "RPT": "Reporting",
    "SEC": "Security/Compliance",
    "INT": "Integration/Interoperability",
    "UI": "User Interface",
    "TEC": "Technical/Infrastructure",
    "ADM": "Administration"
}

catalog_md = generate_catalog_md(valid_reqs, domain_context)
```

### Step 3: Generate Quick Reference Index

```python
def generate_index(requirements):
    """Generate requirement quick reference index."""
    index = []

    # By priority
    index.append("## Quick Reference Index\n\n")
    index.append("### By Priority\n\n")

    for priority in ["CRITICAL", "HIGH", "MEDIUM", "LOW"]:
        priority_reqs = [r for r in requirements if r.get("priority") == priority]
        if priority_reqs:
            index.append(f"**{priority}:** ")
            ids = [r.get("display_id", r.get("id")) for r in priority_reqs[:20]]
            index.append(", ".join(ids))
            if len(priority_reqs) > 20:
                index.append(f" (+{len(priority_reqs) - 20} more)")
            index.append("\n\n")

    # By type
    index.append("### By Type\n\n")
    types = set(r.get("type", "UNKNOWN") for r in requirements)
    for req_type in sorted(types):
        type_reqs = [r for r in requirements if r.get("type") == req_type]
        if type_reqs:
            index.append(f"**{req_type}:** {len(type_reqs)} requirements\n")

    return ''.join(index)

index_md = generate_index(valid_reqs)
catalog_md += "\n---\n\n" + index_md
```

### Step 4: Write Markdown Output

```python
write_file(f"{folder}/outputs/REQUIREMENTS_CATALOG.md", catalog_md)
```

### Step 5: Generate JSON Catalog

```python
json_catalog = {
    "generated_at": datetime.now().isoformat(),
    "domain": domain_context.get("selected_domain"),
    "total_requirements": len(valid_reqs),
    "categories": {
        cat: {
            "name": CATEGORY_NAMES.get(cat, cat),
            "count": len([r for r in valid_reqs if r.get("category") == cat]),
            "requirement_ids": [r.get("canonical_id") for r in valid_reqs if r.get("category") == cat]
        }
        for cat in sorted(set(r.get("category", "OTHER") for r in valid_reqs))
    },
    "priority_summary": {
        priority: len([r for r in valid_reqs if r.get("priority") == priority])
        for priority in ["CRITICAL", "HIGH", "MEDIUM", "LOW"]
    },
    "requirements": [
        {
            "canonical_id": r.get("canonical_id"),
            "display_id": r.get("display_id"),
            "text": r.get("text"),
            "category": r.get("category"),
            "priority": r.get("priority"),
            "type": r.get("type"),
            "source": r.get("source"),
            "validation_score": r.get("validation", {}).get("score"),
            # V2-F6 fix 2026-05-18: preserve RTM traceability fields. Previously
            # catalog dropped requirement_type/source_ids/parent_id, breaking
            # Phase 4 traceability chain rebuild from this artifact.
            "requirement_type": r.get("requirement_type"),
            "source_ids": r.get("source_ids", []),
            "parent_id": r.get("parent_id"),
        }
        for r in valid_reqs
    ]
}

write_json(f"{folder}/shared/REQUIREMENTS_CATALOG.json", json_catalog)
```

### Step 6: Report Results

```
📚 Requirements Catalog Generated
==================================
Total Requirements: {len(valid_reqs)}

Output Files:
  ✅ REQUIREMENTS_CATALOG.md ({size} KB)
  ✅ REQUIREMENTS_CATALOG.json ({size} KB)

Category Summary:
| Category | Name | Count |
|----------|------|-------|
{table rows}

Priority Summary:
  CRITICAL: {n}
  HIGH: {n}
  MEDIUM: {n}
  LOW: {n}
```

## Quality Checklist (MANDATORY — report each by name with evidence)

The phase agent MUST verify each of the following BEFORE reporting completion. The agent's completion report MUST include a checklist-results block with:
- Item name (verbatim from below)
- PASS / FAIL / SKIPPED-WITH-REASON
- Evidence (file:line citation, grep result, file size, assertion that ran, etc.)

"All checks passed" without per-item evidence is NOT acceptable.

### Required output files
1. **REQUIREMENTS_CATALOG.md** exists at `{folder}/outputs/REQUIREMENTS_CATALOG.md` — evidence: `ls -la` size > 10,240 bytes
2. **REQUIREMENTS_CATALOG.json** exists at `{folder}/shared/REQUIREMENTS_CATALOG.json` — evidence: `ls -la` size > 1,024 bytes and parses as valid JSON

### Schema fidelity
3. **REQUIREMENTS_CATALOG.json top-level keys** include `generated_at`, `domain`, `total_requirements`, `categories`, `priority_summary`, `requirements` — evidence: list actual top-level keys found
4. **Every requirement in catalog `requirements[]`** has `canonical_id`, `display_id`, `text`, `category`, `priority`, `source_ids`, `requirement_type` — evidence: print key set of requirements[0]
5. No `[:N]` slicing applied to deliverable content strings — evidence: grep for `\[:[0-9]+\]` in production code paths returned 0 hits

### Cross-stage consistency
6. **Table of contents generated** in REQUIREMENTS_CATALOG.md — evidence: grep for "Table of Contents" or `## Table of Contents` returned >= 1 hit
7. **Requirements grouped by category** — each category appears as a `##` section — evidence: grep `^## [A-Z][A-Z][A-Z]:` returned >= 1 hit per category in category_distribution
8. **Quick reference index included** — evidence: grep for "Quick Reference Index" or "By Priority" in REQUIREMENTS_CATALOG.md returned >= 1 hit
9. **No embedded newlines in table cells** — markdown table rows must not contain raw `\n` — evidence: grep `\|[^|]*\n[^|]` in REQUIREMENTS_CATALOG.md returned 0 matches

### Anti-regression rules (universal)
10. **UTF-8 encoding** on every `open()` call — evidence: search this phase's emitted scripts/code for `encoding='utf-8'` in every file-open
11. **ensure_ascii=False** on every `json.dump` call — evidence: same grep
12. **No `_Showing N of M_` row-cap notices** in any deliverable markdown — evidence: grep returned 0 matches
13. **No empty `|  |` mitigation/cell patterns** in any deliverable table — evidence: grep returned 0 matches
14. **No mid-word table-cell truncations** — evidence: line-by-line cell-end check returned 0 hits

### Memory discipline
15. **Relevant SAFS memory entries reviewed and applied** — evidence: list which memory files were read and which rules were applicable
