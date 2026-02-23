---
name: phase2-extract-win
expert-role: Requirements Engineer
domain-expertise: Requirements elicitation, traceability, requirement classification
---

# Phase 2: Requirements Extraction

## Expert Role

You are a **Requirements Engineer** with deep expertise in:
- Requirements elicitation and documentation
- Functional vs non-functional requirement classification
- Requirement decomposition and sub-item extraction
- Traceability and dependency mapping
- Requirement type taxonomy (IEEE/ISO standards)

## Purpose

Extract ALL requirements from RFP documents with aggressive sub-item extraction. Target: 247+ requirements.

## Inputs

- `{folder}/flattened/*.md` - Flattened RFP documents
- `{folder}/shared/domain-context.json` - Domain context
- `{folder}/shared/workflow-extracted-reqs.json` - Workflow-derived candidates

## Required Outputs

- `{folder}/shared/requirements-raw.json` - Extracted requirements

## Target: 247+ Requirements

Previous pipelines extracted only 158 requirements. Apply aggressive extraction rules.

## Instructions

### Step 1: Load Documents

```python
import glob

flattened_files = glob.glob(f"{folder}/flattened/*.md")
combined_content = ""

for file_path in flattened_files:
    combined_content += read_file(file_path) + "\n\n"

domain_context = read_json(f"{folder}/shared/domain-context.json")
workflow_reqs = read_json(f"{folder}/shared/workflow-extracted-reqs.json")
```

### Step 2: Define Extraction Patterns

```python
REQUIREMENT_PATTERNS = [
    # Explicit requirements
    (r"(?:the\s+)?system\s+(?:shall|must|will)\s+(.{20,300})", "SYSTEM"),
    (r"(?:the\s+)?application\s+(?:shall|must|will)\s+(.{20,300})", "APPLICATION"),
    (r"(?:the\s+)?solution\s+(?:shall|must|will)\s+(.{20,300})", "SOLUTION"),

    # User-facing requirements
    (r"(?:users?\s+(?:shall|must|will)\s+be\s+able\s+to)\s+(.{20,300})", "USER"),
    (r"(?:allow\s+(?:users?|administrators?)\s+to)\s+(.{20,300})", "USER"),
    (r"(?:enable\s+(?:users?|administrators?)\s+to)\s+(.{20,300})", "USER"),

    # Support/provide patterns
    (r"(?:shall|must|will)\s+(?:support|provide|include|enable|allow)\s+(.{20,300})", "CAPABILITY"),

    # Data requirements
    (r"(?:data\s+(?:shall|must|will))\s+(.{20,300})", "DATA"),
    (r"(?:(?:store|capture|maintain|track)\s+(?:the\s+)?following)\s*[:\n](.{20,500})", "DATA"),

    # Interface requirements
    (r"(?:interface\s+(?:shall|must|will))\s+(.{20,300})", "INTERFACE"),
    (r"(?:integrate\s+with)\s+(.{20,200})", "INTERFACE"),

    # Performance requirements
    (r"(?:response\s+time|performance|throughput)\s+(?:shall|must|will)\s+(.{20,200})", "PERFORMANCE"),

    # Security requirements
    (r"(?:security|authentication|authorization)\s+(?:shall|must|will)\s+(.{20,300})", "SECURITY")
]
```

### Step 3: Extract Primary Requirements

```python
requirements = []
seen_text = set()
req_id = 1

for pattern, req_type in REQUIREMENT_PATTERNS:
    matches = re.finditer(pattern, combined_content, re.IGNORECASE | re.DOTALL)
    for match in matches:
        text = match.group(1).strip()
        text = re.sub(r'\s+', ' ', text)

        # Normalize for deduplication
        text_normalized = text.lower()[:50]
        if text_normalized not in seen_text:
            seen_text.add(text_normalized)
            requirements.append({
                "id": f"REQ{req_id:03d}",
                "text": text[:500],
                "type": req_type,
                "source": "pattern_extraction",
                "full_context": match.group(0)[:600],
                "position": match.start(),
                "sub_items": []
            })
            req_id += 1
```

### Step 4: Aggressive Sub-Item Extraction

Apply rules to extract sub-requirements:

```python
def extract_sub_items(requirement):
    """Extract sub-items from a requirement."""
    text = requirement["text"]
    sub_items = []

    # Rule 1: Numbered sub-items (1.1, a), i.)
    numbered_pattern = r'(?:^|\n)\s*(?:(\d+\.\d+)|([a-z]\))|(\([a-z]\))|(\d+\))|([ivx]+\.))\s*(.{15,200})'
    matches = re.findall(numbered_pattern, text, re.IGNORECASE)
    for match in matches:
        item_text = match[-1].strip()
        if len(item_text) > 15:
            sub_items.append({
                "text": item_text,
                "rule": "numbered_sub_item"
            })

    # Rule 2: Bulleted items
    bullet_pattern = r'(?:^|\n)\s*[-•*]\s*(.{15,200})'
    bullets = re.findall(bullet_pattern, text)
    for bullet in bullets:
        if len(bullet.strip()) > 15:
            sub_items.append({
                "text": bullet.strip(),
                "rule": "bulleted_item"
            })

    # Rule 3: Items with distinct SHALL/MUST
    shall_pattern = r'(?:shall|must|will)\s+(.{15,150}?)(?:\.|\n|$)'
    shalls = re.findall(shall_pattern, text, re.IGNORECASE)
    if len(shalls) > 1:  # Multiple SHALL statements in one requirement
        for shall in shalls[1:]:  # Skip first (it's the main requirement)
            sub_items.append({
                "text": shall.strip(),
                "rule": "distinct_shall"
            })

    # Rule 4: "Including but not limited to" lists
    including_pattern = r'including(?:\s+but\s+not\s+limited\s+to)?[:\s]+(.{20,500})'
    including_match = re.search(including_pattern, text, re.IGNORECASE)
    if including_match:
        items = re.split(r'[,;](?:\s*and)?', including_match.group(1))
        for item in items:
            item = item.strip()
            if len(item) > 15:
                sub_items.append({
                    "text": item,
                    "rule": "including_list"
                })

    # Rule 5: "Following" lists
    following_pattern = r'following[:\s]+(.{20,500})'
    following_match = re.search(following_pattern, text, re.IGNORECASE)
    if following_match:
        items = re.split(r'[,;](?:\s*and)?', following_match.group(1))
        for item in items:
            item = item.strip()
            if len(item) > 15:
                sub_items.append({
                    "text": item,
                    "rule": "following_list"
                })

    return sub_items

# Apply sub-item extraction to all requirements
for req in requirements:
    req["sub_items"] = extract_sub_items(req)
```

### Step 5: Promote Sub-Items to Requirements

```python
def should_promote(sub_item, parent_req):
    """Determine if sub-item should become standalone requirement."""
    text = sub_item["text"]

    # Rule: Independently testable (>15 words)
    word_count = len(text.split())
    if word_count >= 15:
        return True

    # Rule: Contains action verb
    action_verbs = ["display", "validate", "calculate", "generate", "store", "retrieve", "update", "delete", "export", "import"]
    if any(verb in text.lower() for verb in action_verbs):
        return True

    # Rule: References different system component
    components = ["database", "api", "interface", "report", "form", "dashboard", "module"]
    if any(comp in text.lower() for comp in components):
        return True

    return False

# Promote qualifying sub-items
promoted_reqs = []
for req in requirements:
    for sub_item in req["sub_items"]:
        if should_promote(sub_item, req):
            promoted_reqs.append({
                "id": f"REQ{req_id:03d}",
                "text": sub_item["text"],
                "type": req["type"],
                "source": "sub_item_promotion",
                "parent_id": req["id"],
                "promotion_rule": sub_item["rule"],
                "sub_items": []
            })
            req_id += 1

requirements.extend(promoted_reqs)
```

### Step 6: Merge Workflow Requirements

```python
# Add workflow-derived requirements
workflow_candidates = workflow_reqs.get("requirement_candidates", [])
for wf_req in workflow_candidates:
    # Check for duplicates
    text_normalized = wf_req["description"].lower()[:50]
    if text_normalized not in seen_text:
        seen_text.add(text_normalized)
        requirements.append({
            "id": f"REQ{req_id:03d}",
            "text": wf_req["description"],
            "type": "WORKFLOW",
            "source": "workflow_extraction",
            "workflow_id": wf_req["id"],
            "actors": wf_req.get("actors", []),
            "category": wf_req.get("category"),
            "sub_items": []
        })
        req_id += 1
```

### Step 7: Categorize Requirements

```python
CATEGORIES = {
    "APP": ["application", "data collection", "form", "screen"],
    "ENR": ["enrollment", "student", "registration"],
    "BUD": ["budget", "financial", "fund", "expenditure"],
    "STF": ["staff", "personnel", "employee", "certificate"],
    "RPT": ["report", "export", "output", "print"],
    "SEC": ["security", "authentication", "authorization", "audit"],
    "INT": ["integration", "interface", "api", "import", "export"],
    "UI": ["user interface", "screen", "display", "dashboard"],
    "TEC": ["technical", "infrastructure", "database", "performance"],
    "ADM": ["administration", "configuration", "setup", "maintenance"]
}

def categorize_requirement(req):
    text_lower = req["text"].lower()
    for category, keywords in CATEGORIES.items():
        if any(kw in text_lower for kw in keywords):
            return category
    return "TEC"  # Default to technical

for req in requirements:
    if "category" not in req:
        req["category"] = categorize_requirement(req)
```

### Step 7b: Classify Requirement Type (NEW)

Classify each requirement by type according to IEEE/ISO taxonomy for improved downstream processing:

```python
REQUIREMENT_TYPES = {
    "FUNCTIONAL": {
        "description": "What the system shall do",
        "indicators": [
            "display", "calculate", "generate", "store", "retrieve", "update",
            "delete", "validate", "process", "convert", "export", "import",
            "submit", "approve", "reject", "create", "modify", "search",
            "filter", "sort", "print", "email", "notify"
        ],
        "patterns": [
            r"(?:shall|must|will)\s+(?:display|show|present)",
            r"(?:shall|must|will)\s+(?:calculate|compute|determine)",
            r"(?:shall|must|will)\s+(?:generate|produce|create)",
            r"(?:shall|must|will)\s+(?:allow|enable|permit)\s+(?:users?|administrators?)\s+to",
            r"(?:shall|must|will)\s+(?:store|save|maintain|track|record)",
            r"(?:shall|must|will)\s+(?:retrieve|fetch|get|query|search)",
            r"(?:shall|must|will)\s+(?:send|transmit|email|notify)"
        ]
    },
    "NON_FUNCTIONAL": {
        "description": "How well the system performs",
        "sub_types": ["PERFORMANCE", "USABILITY", "RELIABILITY", "AVAILABILITY"],
        "indicators": [
            "response time", "throughput", "latency", "concurrent",
            "uptime", "availability", "reliability", "scalable",
            "user-friendly", "intuitive", "accessible", "usable",
            "maintainable", "portable", "testable"
        ],
        "patterns": [
            r"(?:response\s+time|latency)\s+(?:shall|must|will)",
            r"(?:shall|must|will)\s+(?:support|handle)\s+\d+\s+(?:concurrent|simultaneous)",
            r"(?:shall|must|will)\s+be\s+(?:available|accessible)\s+\d+",
            r"(?:shall|must|will)\s+(?:scale|support\s+growth)",
            r"uptime\s+(?:of|at\s+least)\s+\d+"
        ]
    },
    "CONSTRAINT": {
        "description": "Limitations on solution design",
        "indicators": [
            "limited to", "must use", "cannot", "shall not", "must not",
            "restricted to", "only", "exclusively", "required to use",
            "compatible with", "compliant with", "based on"
        ],
        "patterns": [
            r"(?:shall|must)\s+(?:not|never)",
            r"(?:shall|must)\s+be\s+(?:limited|restricted)\s+to",
            r"(?:shall|must)\s+(?:use|utilize|employ)\s+(?:only|exclusively)",
            r"(?:shall|must)\s+be\s+(?:compatible|compliant)\s+with",
            r"(?:shall|must)\s+(?:run|operate|execute)\s+on"
        ]
    },
    "INTERFACE": {
        "description": "External system interactions",
        "indicators": [
            "interface", "integrate", "api", "import", "export",
            "exchange", "transmit", "receive", "connect", "communicate",
            "interoperate", "edi", "hl7", "fhir", "rest", "soap", "sftp"
        ],
        "patterns": [
            r"(?:shall|must|will)\s+(?:interface|integrate|connect)\s+with",
            r"(?:shall|must|will)\s+(?:import|export)\s+(?:data|files?|records?)",
            r"(?:shall|must|will)\s+(?:send|receive|exchange)\s+(?:data|messages?|files?)",
            r"(?:shall|must|will)\s+(?:provide|expose|consume)\s+(?:an?\s+)?api"
        ]
    },
    "SECURITY": {
        "description": "Security and access control",
        "indicators": [
            "security", "authentication", "authorization", "access control",
            "role-based", "permission", "encrypt", "audit", "log",
            "password", "credential", "token", "session", "ssl", "tls",
            "vulnerability", "penetration", "secure"
        ],
        "patterns": [
            r"(?:shall|must|will)\s+(?:authenticate|authorize|verify)",
            r"(?:shall|must|will)\s+(?:encrypt|protect|secure)",
            r"(?:shall|must|will)\s+(?:log|audit|track)\s+(?:all\s+)?(?:access|changes?|actions?)",
            r"(?:shall|must|will)\s+(?:restrict|limit|control)\s+access",
            r"role-based\s+access"
        ]
    },
    "COMPLIANCE": {
        "description": "Regulatory and legal requirements",
        "indicators": [
            "comply", "compliance", "regulation", "regulatory", "statutory",
            "legal", "ferpa", "hipaa", "ada", "section 508", "wcag",
            "gdpr", "sox", "pci", "fisma", "nist", "fips"
        ],
        "patterns": [
            r"(?:shall|must|will)\s+(?:comply|conform)\s+(?:with|to)",
            r"(?:in\s+)?(?:compliance|accordance)\s+with",
            r"(?:ferpa|hipaa|ada|wcag|gdpr|sox|pci|fisma|nist)",
            r"(?:shall|must|will)\s+meet\s+(?:all\s+)?(?:regulatory|legal|statutory)"
        ]
    },
    "DATA": {
        "description": "Data storage and management",
        "indicators": [
            "store", "database", "field", "record", "data type",
            "validation", "format", "schema", "entity", "table",
            "attribute", "archive", "retention", "backup"
        ],
        "patterns": [
            r"(?:shall|must|will)\s+(?:store|maintain|persist)\s+(?:the\s+)?(?:following\s+)?(?:data|information|records?)",
            r"data\s+(?:shall|must|will)\s+be\s+(?:retained|archived|backed\s+up)",
            r"(?:shall|must|will)\s+validate\s+(?:that\s+)?(?:data|input|fields?)"
        ]
    }
}

def classify_requirement_type(req):
    """
    Classify requirement by type using indicators and patterns.
    Returns primary type and confidence score.
    """
    text = req.get("text", "").lower()
    full_context = req.get("full_context", "").lower()
    combined_text = text + " " + full_context

    scores = {}

    for req_type, config in REQUIREMENT_TYPES.items():
        score = 0

        # Check indicators (1 point each)
        indicators = config.get("indicators", [])
        for indicator in indicators:
            if indicator in combined_text:
                score += 1

        # Check patterns (3 points each - stronger signal)
        patterns = config.get("patterns", [])
        for pattern in patterns:
            if re.search(pattern, combined_text, re.IGNORECASE):
                score += 3

        scores[req_type] = score

    # Determine primary type
    if not scores or max(scores.values()) == 0:
        return {
            "primary_type": "FUNCTIONAL",  # Default
            "confidence": "low",
            "scores": scores
        }

    max_score = max(scores.values())
    primary_type = max(scores.keys(), key=lambda k: scores[k])

    # Determine confidence
    if max_score >= 6:
        confidence = "high"
    elif max_score >= 3:
        confidence = "medium"
    else:
        confidence = "low"

    # Check for secondary type (if another type scores > 50% of primary)
    secondary_type = None
    for req_type, score in scores.items():
        if req_type != primary_type and score >= max_score * 0.5 and score >= 2:
            secondary_type = req_type

    return {
        "primary_type": primary_type,
        "secondary_type": secondary_type,
        "confidence": confidence,
        "scores": scores
    }

# Apply type classification to all requirements
for req in requirements:
    type_result = classify_requirement_type(req)
    req["requirement_type"] = type_result["primary_type"]
    req["requirement_type_secondary"] = type_result["secondary_type"]
    req["type_confidence"] = type_result["confidence"]
    req["type_scores"] = type_result["scores"]
```

### Step 7c: Generate Type Distribution Summary

```python
def summarize_type_distribution(requirements):
    """Generate summary of requirement type distribution."""
    type_counts = {}
    confidence_counts = {"high": 0, "medium": 0, "low": 0}

    for req in requirements:
        req_type = req.get("requirement_type", "FUNCTIONAL")
        confidence = req.get("type_confidence", "low")

        type_counts[req_type] = type_counts.get(req_type, 0) + 1
        confidence_counts[confidence] = confidence_counts.get(confidence, 0) + 1

    return {
        "type_distribution": type_counts,
        "confidence_distribution": confidence_counts,
        "functional_count": type_counts.get("FUNCTIONAL", 0),
        "non_functional_count": sum(
            type_counts.get(t, 0) for t in ["NON_FUNCTIONAL", "PERFORMANCE", "USABILITY"]
        ),
        "constraint_count": type_counts.get("CONSTRAINT", 0),
        "interface_count": type_counts.get("INTERFACE", 0),
        "security_count": type_counts.get("SECURITY", 0),
        "compliance_count": type_counts.get("COMPLIANCE", 0),
        "data_count": type_counts.get("DATA", 0)
    }

type_summary = summarize_type_distribution(requirements)
log(f"""
Requirement Type Classification Complete:
- Functional: {type_summary['functional_count']}
- Non-Functional: {type_summary['non_functional_count']}
- Interface: {type_summary['interface_count']}
- Security: {type_summary['security_count']}
- Compliance: {type_summary['compliance_count']}
- Constraint: {type_summary['constraint_count']}
- Data: {type_summary['data_count']}

Classification Confidence:
- High: {type_summary['confidence_distribution']['high']}
- Medium: {type_summary['confidence_distribution']['medium']}
- Low: {type_summary['confidence_distribution']['low']}
""")
```

### Step 7d: Generate RTM Source IDs per Requirement (NEW - for UNIFIED_RTM.json)

```python
## RTM CONTRIBUTION: Link each requirement to its RFP source location
## Phase 4 will use these source_ids to build full traceability chains

# Load existing rfp_sources from compliance phase (if available)
compliance_data = read_json(f"{folder}/shared/COMPLIANCE_MATRIX.json")
existing_sources = compliance_data.get("rtm_entities", {}).get("rfp_sources", [])
source_id_counter = len(existing_sources) + 1

# Map each requirement to the flattened file where it was found
for req in requirements:
    position = req.get("position", 0)
    source_file = None

    # Determine which flattened file this requirement came from
    # by tracking cumulative character offsets
    cumulative_offset = 0
    for file_path in flattened_files:
        file_content = read_file(file_path)
        file_len = len(file_content)
        if cumulative_offset <= position < cumulative_offset + file_len:
            source_file = os.path.basename(file_path)
            break
        cumulative_offset += file_len + 2  # +2 for "\n\n" separator

    # Create source reference
    source_id = f"SRC-{source_id_counter:03d}"
    req["source_ids"] = [source_id]

    # Add to rfp_sources list for Phase 4
    existing_sources.append({
        "source_id": source_id,
        "document": source_file or "unknown",
        "section": req.get("full_context", "")[:100],
        "page_or_row": "",
        "text_excerpt": req.get("text", "")[:500]
    })
    source_id_counter += 1

# For promoted sub-items, inherit parent's source_ids
for req in requirements:
    if req.get("source") == "sub_item_promotion" and req.get("parent_id"):
        parent = next((r for r in requirements if r["id"] == req["parent_id"]), None)
        if parent and parent.get("source_ids"):
            req["source_ids"] = parent["source_ids"]  # Inherit parent's source
```

### Step 8: Write Output

```python
requirements_output = {
    "extracted_at": datetime.now().isoformat(),
    "summary": {
        "total_requirements": len(requirements),
        "from_pattern_extraction": sum(1 for r in requirements if r["source"] == "pattern_extraction"),
        "from_sub_item_promotion": sum(1 for r in requirements if r["source"] == "sub_item_promotion"),
        "from_workflow_extraction": sum(1 for r in requirements if r["source"] == "workflow_extraction"),
        "target_achieved": len(requirements) >= 247
    },
    "category_distribution": {
        category: sum(1 for r in requirements if r.get("category") == category)
        for category in CATEGORIES.keys()
    },
    "source_type_distribution": {
        req_type: sum(1 for r in requirements if r.get("type") == req_type)
        for req_type in set(r.get("type") for r in requirements)
    },
    "requirement_type_classification": type_summary,
    "requirements": requirements,
    # RTM: Accumulated rfp_sources for Phase 4
    "rtm_rfp_sources": existing_sources
}

write_json(f"{folder}/shared/requirements-raw.json", requirements_output)
```

### Step 9: Report Results

```
📋 Requirements Extraction Complete
====================================
Total Requirements: {len(requirements)} {"✅" if len(requirements) >= 247 else "⚠️"}
  Target: 247+

Source Breakdown:
  - Pattern extraction: {from_pattern}
  - Sub-item promotion: {from_sub_item}
  - Workflow extraction: {from_workflow}

Category Distribution:
| Category | Count | % |
|----------|-------|---|
{table rows}

Type Distribution:
{type_table}
```

## Quality Checklist

- [ ] `requirements-raw.json` created in `shared/`
- [ ] Target of 247+ requirements achieved
- [ ] Sub-items extracted and promoted
- [ ] Workflow requirements merged
- [ ] All requirements categorized (APP, ENR, BUD, etc.)
- [ ] All requirements type-classified (FUNCTIONAL, NON_FUNCTIONAL, INTERFACE, SECURITY, COMPLIANCE, CONSTRAINT, DATA)
- [ ] Type classification confidence distribution logged
- [ ] Source type distribution logged
