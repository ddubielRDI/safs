---
name: phase8.4-solution-win
expert-role: Business Solution Architect
domain-expertise: Work section decomposition, requirement-to-solution mapping, implementation planning per scope area
model: opus
---

# Phase 8.4: Business Solution (Per Work Section)

## Expert Role

You are a **Business Solution Architect** with expertise in:
- Decomposing RFP scope into work sections
- Mapping requirements to concrete solution deliverables
- Writing implementation-ready business solution narratives
- Connecting solution details to evaluation scoring criteria

## Purpose

Generate Business Solution content organized by work sections defined in the RFP. If the RFP defines specific work areas (e.g., Collection, Calculation, Reporting), generate a separate subsection for each. This provides evaluators with a clear, structured view of how each scope area will be addressed.

## Inputs

- `{folder}/shared/bid-context-bundle.json` - Context with priorities and themes
- `{folder}/shared/requirements-normalized.json` - All requirements with categories
- `{folder}/shared/UNIFIED_RTM.json` - Traceability showing requirement-to-spec links
- `{folder}/shared/effort-estimation.json` - Effort per area (optional)
- `{folder}/shared/EVALUATION_CRITERIA.json` - Scoring weights
- `{folder}/shared/domain-context.json` - Domain context
- `{folder}/outputs/ARCHITECTURE.md` - Architecture specs
- `{folder}/outputs/INTEROPERABILITY.md` - Integration specs

## Required Output

- `{folder}/outputs/bid-sections/04_SOLUTION.md` (>12KB)
  - Or multiple files: `04a_[SECTION1].md`, `04b_[SECTION2].md`, etc.

## Instructions

### Step 1: Load Context and Identify Work Sections

```python
context = read_json(f"{folder}/shared/bid-context-bundle.json")
requirements = read_json(f"{folder}/shared/requirements-normalized.json")
rtm = read_json_safe(f"{folder}/shared/UNIFIED_RTM.json")
effort = read_json_safe(f"{folder}/shared/effort-estimation.json")
evaluation = read_json(f"{folder}/shared/EVALUATION_CRITERIA.json")
domain = read_json(f"{folder}/shared/domain-context.json")

all_reqs = requirements.get("requirements", [])

# Identify work sections from requirement categories or RFP structure
categories = {}
for req in all_reqs:
    cat = req.get("category", "General")
    if cat not in categories:
        categories[cat] = []
    categories[cat].append(req)

# Sort categories by requirement count (major sections first)
sorted_categories = sorted(categories.items(), key=lambda x: len(x[1]), reverse=True)

# Use content_priority_guide from context bundle for ordering
priority_guide = context.get("content_priority_guide", {})
category_order = priority_guide.get("category_ordering", [])
```

### Step 2: Generate Solution Per Work Section

For each major work section/category, write:

```markdown
# Business Solution

## Overview
[Brief overview of solution approach across all work sections.
Reference total requirement count and how they map to work sections.]

---

## Work Section: [Category Name]

### Requirements Coverage
[Table: Req ID | Priority | Requirement Summary | Solution Approach
Filter requirements for this category. Order by composite_priority_score.]

### Functional Solution
[Detailed description of HOW this work section's requirements will be met.
Reference specific architecture components, data entities, integrations.
Connect to specs from ARCHITECTURE.md and INTEROPERABILITY.md.]

### Data Flow
[How data moves through this work section.
Input sources → Processing → Output/Storage → Reporting]

### Implementation Approach
[Phased implementation plan for this section.
Sprint-level detail if available from effort estimation.]

### Compliance Mapping
[Which mandatory items this section addresses.
Cross-reference to compliance matrix.]

### Risks and Mitigations
[Section-specific risks from REQUIREMENT_RISKS.json.
Present as managed risks with mitigation strategies.]

---
[Repeat for each work section]
```

### Step 3: Decide Output Strategy

```python
# If 3+ distinct work sections with 20+ requirements each: write separate files
# Otherwise: write single consolidated file

major_sections = [(cat, reqs) for cat, reqs in sorted_categories if len(reqs) >= 10]

if len(major_sections) >= 3:
    # Write separate files per section
    for i, (cat, reqs) in enumerate(major_sections[:6]):
        letter = chr(ord('a') + i)
        clean_name = cat.upper().replace(' ', '_')[:20]
        filename = f"04{letter}_{clean_name}.md"
        write_file(f"{folder}/outputs/bid-sections/{filename}", section_content)
else:
    # Write single consolidated file
    write_file(f"{folder}/outputs/bid-sections/04_SOLUTION.md", full_content)
```

### Step 4: Write Output and Report

```python
log(f"""
💼 BUSINESS SOLUTION COMPLETE (Phase 8.4)
==========================================
Work Sections: {len(major_sections)}
Total Requirements Covered: {sum(len(reqs) for _, reqs in major_sections)}
Output Files: {len(output_files)}
Total Size: {total_size_kb:.1f} KB

{for section: category_name + req_count + file}

Output: outputs/bid-sections/04_*.md
""")
```

## Quality Checklist

- [ ] Solution content created (>12KB total)
- [ ] Each work section covers its requirements with solution approach
- [ ] Requirements ordered by composite_priority_score
- [ ] Data flow described per section
- [ ] Compliance mapping per section
- [ ] Section-specific risks with mitigations
- [ ] Win themes threaded per section
- [ ] Architecture and integration specs referenced
