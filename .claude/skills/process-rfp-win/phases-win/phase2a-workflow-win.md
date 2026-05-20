---
name: phase2a-workflow-win
expert-role: Business Process Analyst
domain-expertise: Process flows, BPMN, workflow mapping
---

# Phase 2a: Workflow Extraction

## Expert Role

You are a **Business Process Analyst** with deep expertise in:
- Business process modeling (BPMN)
- Workflow mapping and analysis
- AS-IS vs TO-BE process documentation
- Actor identification and data flow analysis

## Purpose

Extract AS-IS workflow steps from RFP documents to generate workflow-derived requirements.

## Inputs

- `{folder}/flattened/*.md` - Flattened RFP documents
- `{folder}/shared/domain-context.json` - Domain context

## Required Outputs

- `{folder}/shared/workflow-extracted-reqs.json` - Workflow-derived requirement candidates

## Instructions

### Step 1: Load Documents

```python
import glob

flattened_files = glob.glob(f"{folder}/flattened/*.md")
combined_content = ""

for file_path in flattened_files:
    combined_content += read_file(file_path) + "\n\n"

domain_context = read_json(f"{folder}/shared/domain-context.json")
```

### Step 2: Identify Workflow Sections

## ⛔ PDF PAGE-NUMBER FOOTER REJECTION (codified 2026-05-20 — MARS Phase 2a incident)

PDF→markdown extractors emit page footers as standalone numeric lines (`7`, `8`, `9` etc., often followed by `Attachment H Detailed Requirements / Page | N`). If the workflow section-boundary detector treats any standalone line containing a digit as a potential section heading, these page footers PREMATURELY TERMINATE workflow extraction.

**MARS 2026-05-20 incident:** Phase 2a's first pass captured only 1 of 20 steps for section 3.1.1.1 because a `7` page-number footer line was treated as a section break. The fix: when checking whether a line is a section boundary, exclude lines that are (a) pure single/double-digit number, (b) followed within 3 lines by the literal "Page |" or "Attachment H" footer text, OR (c) lone digit on a line with no other content.

```python
PAGE_FOOTER_PATTERN = re.compile(r"^\s*\d{1,3}\s*$")
ORPHAN_LIST_MARKER = re.compile(r"^\s*\d{1,3}\.\s*$")  # naked "5." with no body

def fold_orphan_list_markers(steps):
    """Fold naked '8.' / '16.' steps into the NEXT step's description.

    Codified 2026-05-20 (MARS Phase 2a incident): PDF→markdown extractors
    frequently emit a numbered-list marker on its own line when the item
    body wraps to the following paragraph. Without folding, 77 of 850
    steps in the MARS run were captured as standalone marker-only steps,
    inflating step_count by ~9% and producing meaningless candidates.

    The fold preserves the marker as a prefix on the next step so the
    item number stays visible in the deliverable but doesn't generate
    a separate workflow-step record.
    """
    folded = []
    pending_marker = None
    for s in steps:
        text = (s.get("description") or s.get("text") or "").strip()
        if ORPHAN_LIST_MARKER.match(text):
            pending_marker = text
            continue
        if pending_marker:
            s["description"] = f"{pending_marker} {s.get('description', '')}".strip()
            pending_marker = None
        folded.append(s)
    return folded
```

Apply `fold_orphan_list_markers()` to the per-workflow `steps[]` list AFTER step extraction and BEFORE writing the final JSON. Verifier expects `requirement_candidates` count to fall by ~9% (the orphan-fragment portion).

```python
PAGE_FOOTER_CONTEXT = re.compile(r"Page\s*\|\s*\d+|Attachment\s+[A-Z]\s+(Detailed\s+Requirements|Cost\s+Proposal)", re.IGNORECASE)

def is_page_footer(line, surrounding_lines):
    """A standalone digit line is a PDF page footer (not a section break) when:
    - It matches PAGE_FOOTER_PATTERN AND
    - Within the next 3 lines there's either footer context text OR another digit-only line
    """
    if not PAGE_FOOTER_PATTERN.match(line):
        return False
    for next_line in surrounding_lines[:3]:
        if PAGE_FOOTER_CONTEXT.search(next_line):
            return True
        if PAGE_FOOTER_PATTERN.match(next_line):
            return True
    return False
```

Apply this check BEFORE evaluating any line as a section boundary candidate. After applying the filter, section 3.1.1.1 went from 1 step → 20 steps captured.

```python
WORKFLOW_PATTERNS = [
    r"(?:current|existing|as-is)\s+(?:process|workflow|procedure)",
    r"process\s+(?:flow|description|overview)",
    r"workflow\s+(?:diagram|description|steps)",
    r"(?:step|activity)\s+\d+",
    r"(?:input|output)\s+data",
    r"data\s+(?:flow|exchange)"
]

def find_workflow_sections(content):
    sections = []
    for pattern in WORKFLOW_PATTERNS:
        matches = re.finditer(pattern, content, re.IGNORECASE)
        for match in matches:
            start = max(0, match.start() - 200)
            end = min(len(content), match.end() + 3000)
            sections.append({
                "pattern": pattern,
                "context": content[start:end],
                "position": match.start()
            })
    return sections

workflow_sections = find_workflow_sections(combined_content)
```

### Step 3: Extract Process Tables

```python
def extract_process_tables(content):
    """Extract workflow information from markdown tables."""
    workflows = []

    # Find markdown tables
    table_pattern = r'\|[^\n]+\|\n\|[\-\s\|]+\|\n(?:\|[^\n]+\|\n)+'
    tables = re.findall(table_pattern, content)

    for table in tables:
        rows = table.strip().split('\n')
        if len(rows) < 3:
            continue

        # Parse header
        header = [cell.strip() for cell in rows[0].split('|')[1:-1]]

        # Check if this looks like a workflow table
        workflow_indicators = ['step', 'activity', 'action', 'process', 'task', 'input', 'output', 'actor', 'role']
        header_lower = [h.lower() for h in header]

        if any(ind in ' '.join(header_lower) for ind in workflow_indicators):
            # Parse data rows
            data_rows = []
            for row in rows[2:]:  # Skip header and separator
                cells = [cell.strip() for cell in row.split('|')[1:-1]]
                if len(cells) == len(header):
                    data_rows.append(dict(zip(header, cells)))

            if data_rows:
                workflows.append({
                    "type": "table",
                    "header": header,
                    "steps": data_rows
                })

    return workflows

process_tables = extract_process_tables(combined_content)
```

### Step 4: Extract Narrative Workflows

```python
def extract_narrative_workflows(content):
    """Extract workflow steps from narrative text."""
    workflows = []

    # Numbered steps pattern
    step_pattern = r'(?:step\s+)?(\d+)[\.:\)]\s*([^\n]+(?:\n(?!\d+[\.:\)])[^\n]+)*)'
    matches = re.findall(step_pattern, content, re.IGNORECASE)

    if matches:
        steps = []
        for step_num, step_text in matches:
            steps.append({
                "step_number": int(step_num),
                "description": step_text.strip(),
                "actors": extract_actors(step_text),
                "data_elements": extract_data_elements(step_text)
            })

        if steps:
            workflows.append({
                "type": "narrative",
                "steps": steps
            })

    return workflows

def extract_actors(text):
    """Extract actor mentions from text."""
    actor_patterns = [
        r'\b(?:user|administrator|manager|clerk|officer|staff|student|teacher|parent)\b',
        r'\b(?:system|application|database|api|service)\b',
        r'\b(?:district|school|department|agency)\b'
    ]

    actors = []
    for pattern in actor_patterns:
        matches = re.findall(pattern, text, re.IGNORECASE)
        actors.extend(matches)

    return list(set(actors))

def extract_data_elements(text):
    """Extract data element mentions from text."""
    data_patterns = [
        r'\b(?:form|report|file|document|record|data|information)\b',
        r'\b[A-Z][a-z]+(?:ID|Code|Number|Date|Name|Status)\b'
    ]

    elements = []
    for pattern in data_patterns:
        matches = re.findall(pattern, text, re.IGNORECASE)
        elements.extend(matches)

    return list(set(elements))

narrative_workflows = extract_narrative_workflows(combined_content)
```

### Step 5: Generate Requirement Candidates

```python
def generate_requirements(workflows):
    """Convert workflow steps to requirement candidates."""
    candidates = []
    req_id = 1

    for workflow in workflows:
        for step in workflow.get("steps", []):
            # Generate functional requirement
            if workflow["type"] == "table":
                description = step.get("Activity") or step.get("Step") or step.get("Action") or str(step)
            else:
                description = step.get("description", "")

            if len(description) > 20:  # Skip trivial steps
                candidates.append({
                    "id": f"WF{req_id:03d}",
                    "source": "workflow",
                    "source_type": workflow["type"],
                    "description": description[:500],
                    "actors": step.get("actors", []) if isinstance(step, dict) else extract_actors(description),
                    "data_elements": step.get("data_elements", []) if isinstance(step, dict) else extract_data_elements(description),
                    "category": infer_category(description),
                    "priority": "medium"
                })
                req_id += 1

    return candidates

def infer_category(description):
    """Infer requirement category from description."""
    desc_lower = description.lower()

    categories = {
        "DATA_ENTRY": ["enter", "input", "create", "add", "submit"],
        "VALIDATION": ["validate", "verify", "check", "confirm"],
        "PROCESSING": ["process", "calculate", "generate", "compute"],
        "REPORTING": ["report", "export", "print", "display"],
        "APPROVAL": ["approve", "reject", "review", "authorize"],
        "NOTIFICATION": ["notify", "alert", "email", "message"],
        "INTEGRATION": ["send", "receive", "interface", "api", "integrate"]
    }

    for category, keywords in categories.items():
        if any(kw in desc_lower for kw in keywords):
            return category

    return "GENERAL"

all_workflows = process_tables + narrative_workflows
requirement_candidates = generate_requirements(all_workflows)
```

### Step 6: Map Actor Data Flows

```python
def map_actor_flows(candidates):
    """Create actor-to-actor data flow map."""
    flows = []

    for i, candidate in enumerate(candidates):
        if i > 0:
            prev = candidates[i-1]
            # Infer flow from previous step
            if prev.get("actors") and candidate.get("actors"):
                flows.append({
                    "from_step": prev["id"],
                    "to_step": candidate["id"],
                    "from_actors": prev["actors"],
                    "to_actors": candidate["actors"],
                    "data_exchanged": list(set(prev.get("data_elements", []) + candidate.get("data_elements", [])))
                })

    return flows

actor_flows = map_actor_flows(requirement_candidates)
```

### Step 7: Write Output

```python
workflow_output = {
    "extracted_at": datetime.now().isoformat(),
    "summary": {
        "total_workflows": len(all_workflows),
        "table_workflows": len(process_tables),
        "narrative_workflows": len(narrative_workflows),
        "requirement_candidates": len(requirement_candidates),
        "unique_actors": list(set(
            actor
            for candidate in requirement_candidates
            for actor in candidate.get("actors", [])
        )),
        "unique_data_elements": list(set(
            elem
            for candidate in requirement_candidates
            for elem in candidate.get("data_elements", [])
        ))[:50]
    },
    "workflows": [
        {
            "type": w["type"],
            "step_count": len(w.get("steps", [])),
            "header": w.get("header")
        }
        for w in all_workflows
    ],
    "requirement_candidates": requirement_candidates,
    "actor_data_flows": actor_flows,
    "category_distribution": {
        category: sum(1 for c in requirement_candidates if c.get("category") == category)
        for category in set(c.get("category") for c in requirement_candidates)
    }
}

write_json(f"{folder}/shared/workflow-extracted-reqs.json", workflow_output)
```

### Step 8: Report Results

```
📊 Workflow Extraction Complete
================================
Total Workflows Found: {len(all_workflows)}
  - Table-based: {len(process_tables)}
  - Narrative: {len(narrative_workflows)}

Requirement Candidates Generated: {len(requirement_candidates)}

Category Distribution:
| Category | Count |
|----------|-------|
{table rows}

Unique Actors: {len(unique_actors)}
  {actor_list}

Actor Data Flows: {len(actor_flows)} identified
```

## Quality Checklist (MANDATORY — report each by name with evidence)

The phase agent MUST verify each of the following BEFORE reporting completion. The agent's completion report MUST include a checklist-results block with:
- Item name (verbatim from below)
- PASS / FAIL / SKIPPED-WITH-REASON
- Evidence (file:line citation, grep result, file size, assertion that ran, etc.)

"All checks passed" without per-item evidence is NOT acceptable.

### Required output files
1. **workflow-extracted-reqs.json** exists at `{folder}/shared/workflow-extracted-reqs.json` — evidence: `ls -la` size > 100 bytes and parses as valid JSON

### Schema fidelity
2. **workflow-extracted-reqs.json top-level keys** include `extracted_at`, `summary`, `workflows`, `requirement_candidates`, `actor_data_flows`, `category_distribution` — evidence: list actual top-level keys found
3. **summary** contains `total_workflows`, `requirement_candidates`, `unique_actors`, `unique_data_elements` — evidence: print summary block
4. No `[:N]` slicing applied to deliverable content strings — evidence: grep for `\[:[0-9]+\]` in production code paths returned 0 hits

### Cross-stage consistency
5. **Workflows extracted from both tables and narrative** — evidence: print `summary.table_workflows` and `summary.narrative_workflows` (both should be >= 0; if both are 0 and requirement_candidates is 0 this may indicate a zero-input structural warning)
6. **Every requirement_candidate has a category** — evidence: count entries with missing/null category (must be 0)
7. **Actor_data_flows populated** when multiple workflow candidates exist — evidence: print `len(actor_data_flows)` vs `len(requirement_candidates) - 1` (should be approximately equal when candidates > 1)

### Anti-regression rules (universal)
8. **UTF-8 encoding** on every `open()` call — evidence: search this phase's emitted scripts/code for `encoding='utf-8'` in every file-open
9. **ensure_ascii=False** on every `json.dump` call — evidence: same grep
10. **No `_Showing N of M_` row-cap notices** in any deliverable markdown — evidence: grep returned 0 matches
11. **No empty `|  |` mitigation/cell patterns** in any deliverable table — evidence: grep returned 0 matches
12. **No mid-word table-cell truncations** — evidence: line-by-line cell-end check returned 0 hits

### Memory discipline
13. **Relevant SAFS memory entries reviewed and applied** — evidence: list which memory files were read and which rules were applicable
