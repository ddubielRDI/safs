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

## Quality Checklist

- [ ] `workflow-extracted-reqs.json` created in `shared/`
- [ ] Workflows extracted from both tables and narrative text
- [ ] Requirement candidates generated with categories
- [ ] Actors identified for each workflow step
- [ ] Data elements extracted
- [ ] Actor-to-actor flows mapped
