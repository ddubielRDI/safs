---
name: phase2b-normalize-win
expert-role: Requirements Engineer
domain-expertise: Deduplication, normalization, validation
---

# Phase 2b: Normalize Requirements

## Expert Role

You are a **Requirements Engineer** with deep expertise in:
- Requirements deduplication and merging
- Quality validation and completeness checking
- Requirement normalization and standardization
- Priority assignment and categorization

## Purpose

Deduplicate, validate, and normalize extracted requirements. Merge requirements from multiple sources.

## Inputs

- `{folder}/shared/requirements-raw.json` - Raw extracted requirements
- `{folder}/shared/sample-data-analysis.json` - Data-derived requirements
- `{folder}/shared/workflow-extracted-reqs.json` - Workflow requirements
- `{folder}/shared/domain-context.json` - Domain context

## Required Outputs

- `{folder}/shared/requirements-normalized.json` - Validated, deduplicated requirements

## Instructions

### Step 1: Load All Requirements

```python
raw_reqs = read_json(f"{folder}/shared/requirements-raw.json")
sample_data = read_json(f"{folder}/shared/sample-data-analysis.json")
domain_context = read_json(f"{folder}/shared/domain-context.json")

all_requirements = raw_reqs.get("requirements", [])
```

### Step 2: Generate Data-Derived Requirements

```python
def generate_data_requirements(sample_data):
    """Generate requirements from data analysis."""
    data_reqs = []

    for field in sample_data.get("field_definitions", []):
        # Validation requirements
        for rule in field.get("validation_rules", []):
            data_reqs.append({
                "text": f"The system shall validate {field['field_name']} according to rule: {rule}",
                "type": "DATA_VALIDATION",
                "source": "data_analysis",
                "field": field["field_name"],
                "category": "TEC"
            })

        # Storage requirements
        if field["data_type"] in ["TEXT", "NUMERIC", "DATE"]:
            data_reqs.append({
                "text": f"The system shall store {field['field_name']} as {field['data_type']} type",
                "type": "DATA_STORAGE",
                "source": "data_analysis",
                "field": field["field_name"],
                "category": "TEC"
            })

    # Entity requirements
    for entity in sample_data.get("entities", []):
        data_reqs.append({
            "text": f"The system shall maintain {entity['name']} entity with {entity['field_count']} attributes",
            "type": "DATA_ENTITY",
            "source": "data_analysis",
            "entity": entity["name"],
            "category": "TEC"
        })

        if entity.get("foreign_keys"):
            data_reqs.append({
                "text": f"The system shall enforce referential integrity for {entity['name']} relationships: {', '.join(entity['foreign_keys'])}",
                "type": "DATA_INTEGRITY",
                "source": "data_analysis",
                "category": "TEC"
            })

    return data_reqs

data_reqs = generate_data_requirements(sample_data)
all_requirements.extend(data_reqs)
```

### Step 3: Deduplicate Requirements

```python
from difflib import SequenceMatcher

def similarity(a, b):
    """Calculate text similarity ratio."""
    return SequenceMatcher(None, a.lower(), b.lower()).ratio()

def deduplicate_requirements(requirements, threshold=0.85):
    """Remove duplicate requirements based on text similarity."""
    unique = []
    merged_count = 0

    for req in requirements:
        is_duplicate = False
        for existing in unique:
            if similarity(req["text"], existing["text"]) > threshold:
                is_duplicate = True
                merged_count += 1
                # Merge sources if different
                if req.get("source") != existing.get("source"):
                    if "merged_sources" not in existing:
                        existing["merged_sources"] = [existing.get("source")]
                    existing["merged_sources"].append(req.get("source"))
                break

        if not is_duplicate:
            unique.append(req)

    return unique, merged_count

unique_requirements, merged_count = deduplicate_requirements(all_requirements)
```

### Step 4: Validate Requirements Quality

```python
def validate_requirement(req):
    """Validate requirement quality and completeness."""
    issues = []
    score = 100

    text = req.get("text", "")

    # Check minimum length
    if len(text) < 20:
        issues.append("Too short (< 20 chars)")
        score -= 20

    # Check for testable verb
    testable_verbs = ["shall", "must", "will", "should"]
    if not any(verb in text.lower() for verb in testable_verbs):
        issues.append("Missing testable verb (shall/must/will)")
        score -= 15

    # Check for ambiguous terms
    ambiguous = ["appropriate", "reasonable", "adequate", "etc", "as needed", "user-friendly"]
    if any(term in text.lower() for term in ambiguous):
        issues.append("Contains ambiguous terms")
        score -= 10

    # Check for measurability
    measurable = ["within", "less than", "at least", "maximum", "minimum", "percentage"]
    has_measurable = any(term in text.lower() for term in measurable)

    # Performance requirements should be measurable
    if req.get("type") == "PERFORMANCE" and not has_measurable:
        issues.append("Performance requirement lacks measurable criteria")
        score -= 15

    return {
        "valid": score >= 70,
        "score": score,
        "issues": issues
    }

for req in unique_requirements:
    req["validation"] = validate_requirement(req)
```

### Step 4b: Flag Ambiguous Requirements for Review

```python
# Comprehensive ambiguity detection patterns
AMBIGUITY_PATTERNS = {
    "vague_terms": {
        "terms": ["appropriate", "reasonable", "adequate", "sufficient", "suitable",
                  "timely", "promptly", "as needed", "as required", "etc", "etc.",
                  "user-friendly", "intuitive", "easy to use", "flexible", "robust",
                  "seamless", "efficient", "effective", "proper", "good", "best"],
        "severity": "HIGH",
        "guidance": "Replace with specific, measurable criteria"
    },
    "undefined_scope": {
        "terms": ["all applicable", "various", "multiple", "several", "many",
                  "some", "certain", "relevant", "necessary", "related"],
        "severity": "MEDIUM",
        "guidance": "Define explicit scope or enumerate specific items"
    },
    "subjective_criteria": {
        "terms": ["acceptable", "satisfactory", "preferred", "desirable",
                  "standard", "normal", "typical", "usual", "common"],
        "severity": "MEDIUM",
        "guidance": "Define objective acceptance criteria"
    },
    "temporal_ambiguity": {
        "terms": ["quickly", "soon", "fast", "rapid", "slow", "periodically",
                  "regularly", "frequently", "occasionally", "when possible"],
        "severity": "HIGH",
        "guidance": "Specify exact time constraints (e.g., 'within 5 seconds')"
    },
    "quantity_ambiguity": {
        "terms": ["large", "small", "minimal", "maximum", "significant",
                  "considerable", "substantial", "most", "few", "many"],
        "severity": "MEDIUM",
        "guidance": "Specify exact quantities or ranges"
    },
    "conditional_ambiguity": {
        "terms": ["if possible", "where applicable", "as appropriate",
                  "when feasible", "if necessary", "as determined"],
        "severity": "HIGH",
        "guidance": "Define specific conditions or remove conditionality"
    }
}

def detect_ambiguities(req):
    """Detect and categorize ambiguities in a requirement."""
    text = req.get("text", "").lower()
    ambiguities = []

    for category, config in AMBIGUITY_PATTERNS.items():
        matched_terms = [term for term in config["terms"] if term in text]
        if matched_terms:
            ambiguities.append({
                "category": category,
                "matched_terms": matched_terms,
                "severity": config["severity"],
                "guidance": config["guidance"]
            })

    # Calculate ambiguity score (0 = clear, 100 = very ambiguous)
    ambiguity_score = min(100, len(ambiguities) * 20 +
                         sum(10 for a in ambiguities if a["severity"] == "HIGH"))

    return {
        "ambiguities": ambiguities,
        "ambiguity_score": ambiguity_score,
        "needs_clarification": ambiguity_score >= 40,
        "review_priority": "HIGH" if ambiguity_score >= 60 else
                          "MEDIUM" if ambiguity_score >= 40 else "LOW"
    }

# Apply ambiguity detection to all requirements
ambiguous_requirements = []
for req in unique_requirements:
    req["ambiguity_analysis"] = detect_ambiguities(req)
    if req["ambiguity_analysis"]["needs_clarification"]:
        ambiguous_requirements.append({
            "id": req.get("canonical_id", "N/A"),
            "text": req.get("text", "")[:100] + "...",
            "ambiguity_score": req["ambiguity_analysis"]["ambiguity_score"],
            "review_priority": req["ambiguity_analysis"]["review_priority"],
            "issues": [a["category"] for a in req["ambiguity_analysis"]["ambiguities"]]
        })

log(f"Flagged {len(ambiguous_requirements)} requirements for ambiguity review")
```

### Step 5: Assign Canonical IDs

```python
def assign_canonical_ids(requirements, domain_context):
    """Assign canonical IDs in {NNNCAT} format."""
    category_counters = {}

    for req in requirements:
        category = req.get("category", "TEC")

        if category not in category_counters:
            category_counters[category] = 1
        else:
            category_counters[category] += 1

        # Format: 001APP, 002APP, etc.
        canonical_id = f"{category_counters[category]:03d}{category}"
        req["canonical_id"] = canonical_id
        req["display_id"] = f"[{canonical_id}]"

    return requirements

unique_requirements = assign_canonical_ids(unique_requirements, domain_context)
```

### Step 6: Assign Priorities

```python
def assign_priority(req):
    """Assign priority based on requirement characteristics."""
    text_lower = req["text"].lower()

    # Critical indicators
    critical = ["mandatory", "required", "must", "critical", "essential", "compliance", "security"]
    if any(term in text_lower for term in critical):
        return "CRITICAL"

    # High indicators
    high = ["shall", "important", "core", "primary"]
    if any(term in text_lower for term in high):
        return "HIGH"

    # Low indicators
    low = ["optional", "nice to have", "could", "may", "consider"]
    if any(term in text_lower for term in low):
        return "LOW"

    return "MEDIUM"

for req in unique_requirements:
    req["priority"] = assign_priority(req)
```

### Step 7: Group by Category

```python
def group_by_category(requirements):
    """Group requirements by category."""
    grouped = {}
    for req in requirements:
        category = req.get("category", "OTHER")
        if category not in grouped:
            grouped[category] = []
        grouped[category].append(req)
    return grouped

grouped_requirements = group_by_category(unique_requirements)
```

### Step 8: Write Output

```python
# Filter to valid requirements only
valid_requirements = [r for r in unique_requirements if r["validation"]["valid"]]
invalid_requirements = [r for r in unique_requirements if not r["validation"]["valid"]]

normalized_output = {
    "normalized_at": datetime.now().isoformat(),
    "summary": {
        "total_input": len(all_requirements),
        "after_deduplication": len(unique_requirements),
        "duplicates_merged": merged_count,
        "valid_requirements": len(valid_requirements),
        "invalid_requirements": len(invalid_requirements),
        "deduplication_rate": f"{(merged_count / len(all_requirements) * 100):.1f}%"
    },
    "category_distribution": {
        category: len(reqs)
        for category, reqs in grouped_requirements.items()
    },
    "priority_distribution": {
        priority: sum(1 for r in valid_requirements if r["priority"] == priority)
        for priority in ["CRITICAL", "HIGH", "MEDIUM", "LOW"]
    },
    "validation_summary": {
        "avg_score": sum(r["validation"]["score"] for r in unique_requirements) / len(unique_requirements),
        "common_issues": count_common_issues(unique_requirements)
    },
    "ambiguity_summary": {
        "total_flagged": len(ambiguous_requirements),
        "high_priority": sum(1 for r in ambiguous_requirements if r["review_priority"] == "HIGH"),
        "medium_priority": sum(1 for r in ambiguous_requirements if r["review_priority"] == "MEDIUM"),
        "common_issues": count_ambiguity_issues(ambiguous_requirements)
    },
    "ambiguous_requirements": ambiguous_requirements,
    "requirements": valid_requirements,
    "invalid_requirements": invalid_requirements
}

def count_ambiguity_issues(ambiguous_reqs):
    """Count frequency of ambiguity categories."""
    issue_counts = {}
    for req in ambiguous_reqs:
        for issue in req.get("issues", []):
            issue_counts[issue] = issue_counts.get(issue, 0) + 1
    return dict(sorted(issue_counts.items(), key=lambda x: x[1], reverse=True))

write_json(f"{folder}/shared/requirements-normalized.json", normalized_output)

def count_common_issues(requirements):
    """Count frequency of validation issues."""
    issue_counts = {}
    for req in requirements:
        for issue in req["validation"]["issues"]:
            issue_counts[issue] = issue_counts.get(issue, 0) + 1
    return dict(sorted(issue_counts.items(), key=lambda x: x[1], reverse=True)[:10])
```

### Step 9: Report Results

```
📋 Requirements Normalization Complete
======================================
Input Requirements: {total_input}
After Deduplication: {after_dedup}
  Duplicates Merged: {merged_count} ({rate}%)

Valid Requirements: {valid} ✅
Invalid Requirements: {invalid} ⚠️

Priority Distribution:
| Priority | Count | % |
|----------|-------|---|
| CRITICAL | {n} | {%} |
| HIGH | {n} | {%} |
| MEDIUM | {n} | {%} |
| LOW | {n} | {%} |

Category Distribution:
| Category | Count |
|----------|-------|
{table rows}

Common Validation Issues:
{issues_list}

⚠️ Ambiguity Analysis:
======================
Total Flagged for Review: {ambiguous_count}
  HIGH Priority: {high_priority} (needs immediate clarification)
  MEDIUM Priority: {medium_priority} (review recommended)

Common Ambiguity Patterns:
| Pattern | Count | Guidance |
|---------|-------|----------|
| vague_terms | {n} | Replace with specific, measurable criteria |
| temporal_ambiguity | {n} | Specify exact time constraints |
| conditional_ambiguity | {n} | Define specific conditions |
| undefined_scope | {n} | Enumerate specific items |

➡️ Recommendation: Review ambiguous_requirements in output JSON and seek RFP clarifications before finalizing specs.
```

## Quality Checklist

- [ ] `requirements-normalized.json` created in `shared/`
- [ ] Duplicates merged (5%+ reduction expected)
- [ ] All requirements have canonical IDs
- [ ] All requirements have priorities
- [ ] Validation scores calculated
- [ ] Invalid requirements documented separately
- [ ] Ambiguity analysis completed for all requirements
- [ ] High-priority ambiguous requirements flagged for user review
