---
name: phase-addendum-win
expert-role: Requirements Change Manager
domain-expertise: Change impact analysis, requirements traceability, configuration management
---

# Phase: Addendum Integration

## Expert Role

You are a **Requirements Change Manager** with deep expertise in:
- Change impact analysis
- Requirements traceability and configuration management
- Delta detection and propagation
- Compliance verification after changes

## Purpose

Process RFP addenda and clarifications that modify requirements after initial processing. Detect deltas, flag impacted requirements and specifications, and propagate changes through the pipeline.

## Why This Phase Exists

**Problem Identified:**
- RFPs frequently release addenda that modify, add, or remove requirements
- No mechanism existed to handle mid-process requirement changes
- Risk of stale requirements in final bid causing compliance failures

**Solution:**
- Delta detection against existing requirements
- Impact analysis on specifications and traceability
- Automated flagging of affected documents
- Re-processing triggers for impacted phases

## When to Invoke

This phase is **NOT part of the standard pipeline sequence**. Invoke it when:
1. User provides an addendum document after initial processing
2. RFP issuer releases clarifications that change requirements
3. User identifies requirement changes that need propagation

**Invocation:**
```
/process-rfp-win addendum <path-to-addendum-document>
```

## Inputs

**Required:**
- `addendum_path` - Path to the addendum document (PDF, DOCX, or MD)
- `{folder}/shared/requirements-normalized.json` - Existing requirements baseline
- `{folder}/flattened/` - Existing flattened documents

**Optional:**
- `{folder}/outputs/TRACEABILITY.md` - Existing traceability matrix
- `{folder}/outputs/ARCHITECTURE.md` - Architecture spec (for impact checking)
- `{folder}/outputs/SECURITY_REQUIREMENTS.md` - Security spec
- `{folder}/outputs/INTEROPERABILITY.md` - Integration spec

## Required Outputs

- `{folder}/shared/addendum-analysis.json` - Delta analysis results
- `{folder}/shared/requirements-normalized.json` - Updated requirements (if changes found)
- `{folder}/shared/addendum-impact-report.md` - Human-readable impact report

## Instructions

### Step 1: Flatten Addendum Document

```python
import subprocess
from pathlib import Path
from datetime import datetime

def flatten_addendum(addendum_path, folder):
    """Convert addendum to markdown for analysis."""

    addendum_name = Path(addendum_path).stem
    output_path = f"{folder}/flattened/addendum_{addendum_name}.md"

    # Use markitdown for conversion (same as Phase 1)
    if addendum_path.endswith('.pdf'):
        result = subprocess.run(
            ['markitdown', addendum_path],
            capture_output=True,
            text=True
        )
        content = result.stdout
    elif addendum_path.endswith('.docx'):
        result = subprocess.run(
            ['markitdown', addendum_path],
            capture_output=True,
            text=True
        )
        content = result.stdout
    elif addendum_path.endswith('.md'):
        with open(addendum_path, 'r') as f:
            content = f.read()
    else:
        raise ValueError(f"Unsupported addendum format: {addendum_path}")

    # Write flattened content
    with open(output_path, 'w') as f:
        f.write(content)

    return output_path, content

addendum_md_path, addendum_content = flatten_addendum(addendum_path, folder)
log(f"Addendum flattened to: {addendum_md_path}")
```

### Step 2: Extract Addendum Requirements

```python
def extract_addendum_requirements(addendum_content):
    """Extract requirements from addendum text."""

    requirements = []

    # Look for requirement-like statements
    import re

    # Pattern 1: Numbered items with "shall/must/will"
    numbered_pattern = r'(\d+\.[\d.]*)\s*(.+?(?:shall|must|will|should).+?)(?=\n\d+\.|\n\n|$)'

    # Pattern 2: Bullet points with requirement verbs
    bullet_pattern = r'[•\-\*]\s*(.+?(?:shall|must|will|should).+?)(?=\n[•\-\*]|\n\n|$)'

    # Pattern 3: "The system shall/must" statements
    system_pattern = r'(?:The\s+)?(?:system|solution|contractor|vendor)\s+(?:shall|must|will)\s+(.+?)(?=\.|$)'

    # Extract using patterns
    for match in re.finditer(numbered_pattern, addendum_content, re.IGNORECASE | re.MULTILINE):
        requirements.append({
            "text": match.group(2).strip(),
            "source": "addendum",
            "reference": match.group(1),
            "change_type": "unknown"  # Will be determined in delta detection
        })

    for match in re.finditer(bullet_pattern, addendum_content, re.IGNORECASE | re.MULTILINE):
        requirements.append({
            "text": match.group(1).strip(),
            "source": "addendum",
            "reference": "bullet",
            "change_type": "unknown"
        })

    return requirements

addendum_requirements = extract_addendum_requirements(addendum_content)
log(f"Extracted {len(addendum_requirements)} potential requirements from addendum")
```

### Step 3: Load Existing Requirements Baseline

```python
import json

def load_requirements_baseline(folder):
    """Load existing normalized requirements as baseline."""

    baseline_path = f"{folder}/shared/requirements-normalized.json"

    with open(baseline_path, 'r') as f:
        baseline_data = json.load(f)

    return baseline_data

baseline = load_requirements_baseline(folder)
existing_requirements = baseline.get("requirements", [])
log(f"Loaded {len(existing_requirements)} existing requirements as baseline")
```

### Step 4: Detect Deltas (Changes, Additions, Removals)

```python
from difflib import SequenceMatcher

def similarity(a, b):
    """Calculate text similarity ratio."""
    return SequenceMatcher(None, a.lower(), b.lower()).ratio()

def detect_deltas(addendum_reqs, existing_reqs, threshold=0.75):
    """
    Detect changes between addendum and existing requirements.

    Returns:
    - modifications: Existing requirements that are being changed
    - additions: New requirements from addendum
    - clarifications: Requirements that clarify existing ones
    """

    modifications = []
    additions = []
    clarifications = []

    for addendum_req in addendum_reqs:
        addendum_text = addendum_req["text"]

        # Find best match in existing requirements
        best_match = None
        best_similarity = 0

        for existing in existing_reqs:
            existing_text = existing.get("text", "")
            sim = similarity(addendum_text, existing_text)

            if sim > best_similarity:
                best_similarity = sim
                best_match = existing

        if best_similarity > threshold:
            # This is a modification of existing requirement
            if best_similarity < 0.95:  # Not identical
                modifications.append({
                    "original": best_match,
                    "modified": addendum_req,
                    "similarity": best_similarity,
                    "change_description": f"Requirement modified (similarity: {best_similarity:.0%})"
                })
            else:
                # Nearly identical - likely a clarification
                clarifications.append({
                    "original": best_match,
                    "clarification": addendum_req,
                    "similarity": best_similarity
                })
        else:
            # This is a new requirement
            additions.append({
                "requirement": addendum_req,
                "closest_existing": best_match,
                "closest_similarity": best_similarity
            })

    return modifications, additions, clarifications

modifications, additions, clarifications = detect_deltas(
    addendum_requirements,
    existing_requirements
)

log(f"""
Delta Detection Results:
- Modifications: {len(modifications)}
- Additions: {len(additions)}
- Clarifications: {len(clarifications)}
""")
```

### Step 5: Analyze Impact on Specifications

```python
def analyze_spec_impact(modifications, additions, folder):
    """Determine which specifications are impacted by changes."""

    impacted_specs = {}

    # Category to spec mapping
    category_spec_map = {
        "SEC": ["SECURITY_REQUIREMENTS.md"],
        "INTG": ["INTEROPERABILITY.md"],
        "TEC": ["ARCHITECTURE.md"],
        "APP": ["ARCHITECTURE.md", "UI_SPECS.md"],
        "DATA": ["ENTITY_DEFINITIONS.md"],
        "UI": ["UI_SPECS.md"],
        "PERF": ["ARCHITECTURE.md"]
    }

    all_changes = modifications + additions

    for change in all_changes:
        req = change.get("modified", change.get("requirement", {}))
        category = req.get("category", "TEC")

        affected_specs = category_spec_map.get(category, ["ARCHITECTURE.md"])

        for spec in affected_specs:
            if spec not in impacted_specs:
                impacted_specs[spec] = []
            impacted_specs[spec].append({
                "requirement_text": req.get("text", "")[:100],
                "change_type": "modification" if "modified" in change else "addition",
                "category": category
            })

    # Check if specs exist
    existing_specs = []
    for spec_name in impacted_specs.keys():
        spec_path = f"{folder}/outputs/{spec_name}"
        if Path(spec_path).exists():
            existing_specs.append(spec_name)

    return impacted_specs, existing_specs

impacted_specs, existing_specs = analyze_spec_impact(modifications, additions, folder)
```

### Step 6: Analyze Traceability Impact

```python
def analyze_traceability_impact(modifications, folder):
    """Check impact on traceability matrix."""

    traceability_path = f"{folder}/outputs/TRACEABILITY.md"

    if not Path(traceability_path).exists():
        return {"exists": False, "impacted_entries": 0}

    with open(traceability_path, 'r') as f:
        traceability_content = f.read()

    impacted_entries = []

    for mod in modifications:
        original_id = mod["original"].get("canonical_id", "")
        if original_id and original_id in traceability_content:
            impacted_entries.append({
                "requirement_id": original_id,
                "reason": "Requirement text modified"
            })

    return {
        "exists": True,
        "impacted_entries": len(impacted_entries),
        "entries": impacted_entries[:20]  # Limit to 20
    }

traceability_impact = analyze_traceability_impact(modifications, folder)
```

### Step 7: Update Requirements Baseline

```python
def update_requirements_baseline(baseline, modifications, additions, folder):
    """Update the requirements-normalized.json with changes."""

    existing_reqs = baseline.get("requirements", [])
    updated_reqs = []

    # Process existing requirements
    for req in existing_reqs:
        req_id = req.get("canonical_id", "")

        # Check if this requirement was modified
        modified = False
        for mod in modifications:
            if mod["original"].get("canonical_id") == req_id:
                # Update with modified version
                updated_req = req.copy()
                updated_req["text"] = mod["modified"]["text"]
                updated_req["modified_by_addendum"] = True
                updated_req["modification_date"] = datetime.now().isoformat()
                updated_req["original_text"] = req["text"]
                updated_reqs.append(updated_req)
                modified = True
                break

        if not modified:
            updated_reqs.append(req)

    # Add new requirements
    existing_categories = set(r.get("category", "TEC") for r in existing_reqs)
    category_counters = {cat: sum(1 for r in existing_reqs if r.get("category") == cat) for cat in existing_categories}

    for addition in additions:
        new_req = addition["requirement"]
        category = new_req.get("category", "TEC")

        # Assign new canonical ID
        if category not in category_counters:
            category_counters[category] = 0
        category_counters[category] += 1

        new_req["canonical_id"] = f"{category_counters[category]:03d}{category}"
        new_req["display_id"] = f"[{new_req['canonical_id']}]"
        new_req["added_by_addendum"] = True
        new_req["addition_date"] = datetime.now().isoformat()
        new_req["priority"] = "HIGH"  # New requirements from addenda are typically important

        updated_reqs.append(new_req)

    # Update baseline
    baseline["requirements"] = updated_reqs
    baseline["addendum_processed"] = {
        "date": datetime.now().isoformat(),
        "modifications": len(modifications),
        "additions": len(additions)
    }
    baseline["summary"]["total_input"] = len(updated_reqs)
    baseline["summary"]["valid_requirements"] = len(updated_reqs)

    # Write updated baseline
    output_path = f"{folder}/shared/requirements-normalized.json"
    with open(output_path, 'w') as f:
        json.dump(baseline, f, indent=2)

    return baseline, output_path

if modifications or additions:
    updated_baseline, updated_path = update_requirements_baseline(
        baseline, modifications, additions, folder
    )
    log(f"Updated requirements baseline: {updated_path}")
else:
    log("No changes to requirements baseline needed")
```

### Step 8: Generate Impact Report

```python
def generate_impact_report(folder, modifications, additions, clarifications, impacted_specs, traceability_impact):
    """Generate human-readable impact report."""

    report = f"""# Addendum Impact Report

Generated: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}

## Summary

| Change Type | Count |
|-------------|-------|
| Modifications | {len(modifications)} |
| Additions | {len(additions)} |
| Clarifications | {len(clarifications)} |

## Modifications to Existing Requirements

"""

    if modifications:
        for i, mod in enumerate(modifications, 1):
            original = mod["original"]
            modified = mod["modified"]
            report += f"""### {i}. [{original.get('canonical_id', 'N/A')}]

**Original:** {original.get('text', '')[:200]}...

**Modified:** {modified.get('text', '')[:200]}...

**Similarity:** {mod['similarity']:.0%}

---

"""
    else:
        report += "*No modifications to existing requirements*\n\n"

    report += """## New Requirements Added

"""

    if additions:
        for i, add in enumerate(additions, 1):
            req = add["requirement"]
            report += f"""### {i}. New Requirement

**Text:** {req.get('text', '')[:200]}...

**Closest Existing:** {add.get('closest_similarity', 0):.0%} similar to existing requirement

---

"""
    else:
        report += "*No new requirements added*\n\n"

    report += """## Impacted Specifications

| Specification | Impact Count | Action Required |
|---------------|--------------|-----------------|
"""

    for spec_name, impacts in impacted_specs.items():
        action = "Re-run phase" if spec_name in existing_specs else "Generate spec"
        report += f"| {spec_name} | {len(impacts)} | {action} |\n"

    report += f"""
## Traceability Impact

- **Traceability Matrix Exists:** {'Yes' if traceability_impact.get('exists') else 'No'}
- **Impacted Entries:** {traceability_impact.get('impacted_entries', 0)}

## Recommended Actions

"""

    if modifications or additions:
        report += """1. **Review changes above** - Verify addendum interpretation is correct
2. **Re-run impacted specification phases:**
"""
        for spec in existing_specs:
            phase_map = {
                "ARCHITECTURE.md": "Phase 3a",
                "SECURITY_REQUIREMENTS.md": "Phase 3c",
                "INTEROPERABILITY.md": "Phase 3b",
                "UI_SPECS.md": "Phase 3e",
                "ENTITY_DEFINITIONS.md": "Phase 3f"
            }
            report += f"   - {phase_map.get(spec, spec)}\n"

        report += """3. **Re-run traceability phase** (Phase 4)
4. **Re-run estimation phase** (Phase 5) if effort may change
5. **Re-generate bid documents** (Stage 7)

"""
    else:
        report += """*No significant changes detected. No action required.*

"""

    report += """## Verification Checklist

- [ ] All modifications reviewed and confirmed
- [ ] New requirements categorized correctly
- [ ] Impacted specifications regenerated
- [ ] Traceability matrix updated
- [ ] Bid documents reflect changes

"""

    # Write report
    report_path = f"{folder}/shared/addendum-impact-report.md"
    with open(report_path, 'w') as f:
        f.write(report)

    return report_path

report_path = generate_impact_report(
    folder, modifications, additions, clarifications,
    impacted_specs, traceability_impact
)
log(f"Impact report written to: {report_path}")
```

### Step 9: Save Analysis Results

```python
def save_analysis_results(folder, modifications, additions, clarifications, impacted_specs, traceability_impact):
    """Save structured analysis results as JSON."""

    analysis = {
        "analyzed_at": datetime.now().isoformat(),
        "addendum_path": addendum_path,
        "summary": {
            "modifications": len(modifications),
            "additions": len(additions),
            "clarifications": len(clarifications),
            "specs_impacted": len(impacted_specs)
        },
        "modifications": [
            {
                "original_id": m["original"].get("canonical_id"),
                "original_text": m["original"].get("text", "")[:200],
                "modified_text": m["modified"].get("text", "")[:200],
                "similarity": m["similarity"]
            }
            for m in modifications
        ],
        "additions": [
            {
                "text": a["requirement"].get("text", "")[:200],
                "category": a["requirement"].get("category", "TEC"),
                "closest_existing_similarity": a.get("closest_similarity", 0)
            }
            for a in additions
        ],
        "impacted_specs": impacted_specs,
        "traceability_impact": traceability_impact,
        "recommended_phases_to_rerun": list(set(
            phase for spec in impacted_specs.keys()
            for phase in {
                "ARCHITECTURE.md": ["3a"],
                "SECURITY_REQUIREMENTS.md": ["3c"],
                "INTEROPERABILITY.md": ["3b"],
                "UI_SPECS.md": ["3e"],
                "ENTITY_DEFINITIONS.md": ["3f"]
            }.get(spec, [])
        )) + (["4"] if traceability_impact.get("impacted_entries", 0) > 0 else [])
    }

    output_path = f"{folder}/shared/addendum-analysis.json"
    with open(output_path, 'w') as f:
        json.dump(analysis, f, indent=2)

    return output_path

analysis_path = save_analysis_results(
    folder, modifications, additions, clarifications,
    impacted_specs, traceability_impact
)
```

### Step 10: Report Results

```python
log(f"""
📋 Addendum Integration Complete
================================
Addendum: {addendum_path}

📊 Changes Detected:
   Modifications: {len(modifications)}
   Additions: {len(additions)}
   Clarifications: {len(clarifications)}

📄 Impacted Specifications:
{chr(10).join(f"   • {spec}: {len(impacts)} requirements" for spec, impacts in impacted_specs.items())}

📁 Outputs:
   • Analysis: {analysis_path}
   • Report: {report_path}
   • Updated Requirements: {folder}/shared/requirements-normalized.json

⚠️ Next Steps:
   1. Review {report_path}
   2. Re-run impacted phases: {', '.join(impacted_specs.keys()) if impacted_specs else 'None'}
   3. Regenerate bid documents
""")
```

## Quality Checklist

- [ ] Addendum document flattened to markdown
- [ ] Requirements extracted from addendum
- [ ] Deltas detected (modifications, additions, clarifications)
- [ ] Impact on specifications analyzed
- [ ] Impact on traceability analyzed
- [ ] Requirements baseline updated (if changes found)
- [ ] Human-readable impact report generated
- [ ] Structured analysis JSON saved
- [ ] Recommended re-run phases identified

## Error Handling

**If addendum cannot be parsed:**
```
ERROR: Unable to parse addendum document
- Check document format (PDF, DOCX, MD supported)
- Ensure document is not password-protected
- Try converting manually and providing .md file
```

**If no requirements detected:**
```
WARNING: No requirements detected in addendum
- Addendum may contain only clarifications or administrative changes
- Review manually to confirm no requirement changes
```

**If baseline not found:**
```
ERROR: Requirements baseline not found
- Run standard pipeline first before processing addendum
- Ensure {folder}/shared/requirements-normalized.json exists
```
