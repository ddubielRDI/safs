---
name: phase8.4-solution-win
expert-role: Business Solution Architect
domain-expertise: Work section decomposition, requirement-to-solution mapping, implementation planning per scope area
model: opus
---

# Phase 8.4: Business Solution (Per Work Section)

## ⛔ NO-TRUNCATION DISCIPLINE (READ FIRST — BLOCKING)

**Render ALL rows in every requirement and risk table. Render FULL text.** Per SAFS memory (`feedback_screen_encoding_truncation.md`), the win pipeline regressed 2026-05-19 producing mid-word truncation in solution-section risk and requirement tables ("would manifes" / "halt payments and cre" / "in ma..."). The rule:

- **NEVER `[:N]` slice description, text, mitigation, or any deliverable-content string.** Full text always.
- **NEVER cap rows** with `cat_risks[:5]`, `sorted_reqs[:30]`, `mandatory_in_cat[:10]`, or any per-category limit. Render ALL.
- **NEVER emit "_Showing N of M_" notices.** Hide nothing.
- **Mitigation cells** in any embedded risk table MUST be populated from BOTH `mitigation_strategies` (array, structural risks) AND `mitigation_strategy` (singular string, req-level risks). Use `[MITIGATION TBD]` only when source data is genuinely empty — never leave the cell empty `|  |`.

Pipelines produce FULL DATA. Humans decide what to trim. Not the agent.

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

## Quality Checklist (MANDATORY — report each by name with evidence)

The phase agent MUST verify each of the following BEFORE reporting completion. The agent's completion report MUST include a checklist-results block with:
- Item name (verbatim from below)
- PASS / FAIL / SKIPPED-WITH-REASON
- Evidence (file:line citation, grep result, file size, assertion that ran, etc.)

"All checks passed" without per-item evidence is NOT acceptable.

### Required output files
1. **Solution content** exists at `{folder}/outputs/bid-sections/04_SOLUTION.md` (single file) OR at `{folder}/outputs/bid-sections/04[a-f]_*.md` (multiple files) — evidence: `ls -la outputs/bid-sections/04*.md` showing total size > 12,288 bytes

### Schema fidelity
2. **Each work section covers its requirements with solution approach** — grep "### Requirements Coverage" or equivalent per-section table returned >= 1 hit per major category — evidence: count matches
3. **No `_Showing N of M_` row-cap notices** in any work section — evidence: grep "_Showing" in 04*.md returned 0 matches
4. **No empty Mitigation cells** in any embedded risk table — evidence: grep `\|[[:space:]]*\|` in HIGH/CRITICAL rows returned 0 matches
5. No `[:N]` slicing applied to deliverable content strings — evidence: grep for `\[:[0-9]+\]` in production code paths returned 0 hits; confirm NO `cat_risks[:5]`, `sorted_reqs[:30]`, `mandatory_in_cat[:10]` patterns

### Cross-stage consistency
6. **Requirements ordered by composite_priority_score** within each section — evidence: confirm the requirements table in at least one section lists higher-score items before lower-score items (spot-check)
7. **Section-specific risks with mitigations present** — every embedded risk table has its Mitigation column populated from `mitigation_strategies` array OR `mitigation_strategy` singular — evidence: count empty mitigation cells (must be 0)
8. **Win themes threaded per section** — at least 1 explicit theme reference per major work section — evidence: spot-check 2 sections for theme callout

### Anti-regression rules (universal)
9. **UTF-8 encoding** on every `open()` call — evidence: search this phase's emitted scripts/code for `encoding='utf-8'` in every file-open
10. **ensure_ascii=False** on every `json.dump` call — evidence: same grep
11. **No mid-word table-cell truncations** — evidence: line-by-line cell-end check returned 0 hits

### Memory discipline
12. **Relevant SAFS memory entries reviewed and applied** — evidence: list which memory files were read and which rules were applicable (e.g., "NEVER `[:N]` slice deliverable strings — rendered ALL rows and FULL text per 2026-05-19 discipline")
