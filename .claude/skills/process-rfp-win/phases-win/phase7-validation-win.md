---
name: phase7-validation-win
expert-role: QA Engineer
domain-expertise: Quality assurance, validation rules, gap analysis, benchmark comparison
---

# Phase 7: Quality Validation & Gap Analysis

## Expert Role

You are a **QA Engineer** with deep expertise in:
- Quality assurance processes
- Validation rule implementation
- Output verification
- Defect identification
- Gap analysis methodologies
- Benchmark comparison
- Coverage assessment
- Quality metrics

## Purpose

Validate all generated outputs for quality and completeness (Part A), then compare against benchmarks to identify gaps (Part B).

## Inputs

- All `{folder}/outputs/*.md` files
- `{folder}/shared/*.json` files
- `{folder}/shared/requirements-normalized.json`

## Required Outputs

- `{folder}/shared/validation-results.json` (combined structural + gap data)
- `{folder}/outputs/GAP_ANALYSIS.md`

## Instructions

### Part A: Structural Validation

#### Step 1a: Define Validation Rules

```python
VALIDATION_RULES = {
    "REQUIREMENTS_CATALOG.md": {
        "min_size_kb": 10,
        "required_sections": ["Requirements", "Category"],
        "must_contain": ["canonical_id", "priority"]
    },
    "ARCHITECTURE.md": {
        "min_size_kb": 15,
        "required_sections": ["Architecture", "Technology Stack"],
        "must_contain": ["layer", "component"]
    },
    "SECURITY_REQUIREMENTS.md": {
        "min_size_kb": 8,
        "required_sections": ["Authentication", "Authorization"],
        "must_contain": ["security", "encryption"]
    },
    "EFFORT_ESTIMATION.md": {
        "min_size_kb": 8,
        "required_sections": ["Summary", "Estimates"],
        "must_contain": ["hours", "effort"]
    },
    "TRACEABILITY.md": {
        "min_size_kb": 10,
        "required_sections": ["Traceability Matrix"],
        "must_contain": ["Req ID", "Specification"]
    }
}
```

#### Step 2a: Validate Each Output

```python
import glob
import os

def validate_file(file_path, rules):
    """Validate a single file against rules."""
    results = {
        "file": os.path.basename(file_path),
        "checks": [],
        "passed": True,
        "errors": [],
        "warnings": []
    }

    # Check file exists
    if not os.path.exists(file_path):
        results["passed"] = False
        results["errors"].append("File does not exist")
        return results

    content = read_file(file_path)
    size_kb = os.path.getsize(file_path) / 1024

    # Size check
    min_size = rules.get("min_size_kb", 1)
    if size_kb >= min_size:
        results["checks"].append(f"✅ Size: {size_kb:.1f}KB >= {min_size}KB")
    else:
        results["passed"] = False
        results["errors"].append(f"Size too small: {size_kb:.1f}KB < {min_size}KB required")

    # Section checks
    for section in rules.get("required_sections", []):
        if section.lower() in content.lower():
            results["checks"].append(f"✅ Section '{section}' found")
        else:
            results["warnings"].append(f"Section '{section}' not found")

    # Content checks
    for term in rules.get("must_contain", []):
        if term.lower() in content.lower():
            results["checks"].append(f"✅ Contains '{term}'")
        else:
            results["warnings"].append(f"Missing expected term '{term}'")

    # Count errors
    if len(results["errors"]) > 0:
        results["passed"] = False

    return results

# Validate all outputs
validation_results = {
    "validated_at": datetime.now().isoformat(),
    "files": [],
    "summary": {
        "total": 0,
        "passed": 0,
        "failed": 0,
        "warnings": 0
    }
}

output_files = glob.glob(f"{folder}/outputs/*.md")
for file_path in output_files:
    file_name = os.path.basename(file_path)
    rules = VALIDATION_RULES.get(file_name, {"min_size_kb": 1})

    result = validate_file(file_path, rules)
    validation_results["files"].append(result)

    validation_results["summary"]["total"] += 1
    if result["passed"]:
        validation_results["summary"]["passed"] += 1
    else:
        validation_results["summary"]["failed"] += 1
    if result["warnings"]:
        validation_results["summary"]["warnings"] += len(result["warnings"])
```

#### Step 3a: Validate JSON Files

```python
def validate_json_file(file_path, expected_keys):
    """Validate JSON file structure."""
    results = {
        "file": os.path.basename(file_path),
        "checks": [],
        "passed": True,
        "errors": []
    }

    try:
        data = read_json(file_path)
        results["checks"].append("✅ Valid JSON")

        for key in expected_keys:
            if key in data:
                results["checks"].append(f"✅ Key '{key}' present")
            else:
                results["errors"].append(f"Missing key '{key}'")
                results["passed"] = False

    except Exception as e:
        results["passed"] = False
        results["errors"].append(f"Invalid JSON: {str(e)}")

    return results

JSON_VALIDATIONS = {
    "requirements-normalized.json": ["requirements", "summary"],
    "domain-context.json": ["selected_domain", "confidence"],
    "COMPLIANCE_MATRIX.json": ["mandatory_items", "gate_status"],
    "EVALUATION_CRITERIA.json": ["selection_method", "evaluation_factors"]
}

for json_file, expected_keys in JSON_VALIDATIONS.items():
    file_path = f"{folder}/shared/{json_file}"
    if os.path.exists(file_path):
        result = validate_json_file(file_path, expected_keys)
        validation_results["files"].append(result)

        validation_results["summary"]["total"] += 1
        if result["passed"]:
            validation_results["summary"]["passed"] += 1
        else:
            validation_results["summary"]["failed"] += 1
```

#### Step 4a: Cross-Reference Validation

```python
def validate_cross_references(folder):
    """Validate cross-references between documents."""
    issues = []

    # Load requirements
    requirements = read_json(f"{folder}/shared/requirements-normalized.json")
    req_ids = set(r.get("canonical_id") for r in requirements.get("requirements", []))

    # Check traceability references valid requirements
    traceability_path = f"{folder}/outputs/TRACEABILITY.md"
    if os.path.exists(traceability_path):
        content = read_file(traceability_path)
        # Extract referenced IDs
        refs = re.findall(r'\[(\d{3}[A-Z]{2,3})\]', content)
        for ref in refs:
            if ref not in req_ids:
                issues.append(f"Invalid requirement reference in TRACEABILITY.md: {ref}")

    return issues

cross_ref_issues = validate_cross_references(folder)
if cross_ref_issues:
    validation_results["cross_reference_issues"] = cross_ref_issues
```

#### Step 5a: Report Structural Results

```python
summary = validation_results["summary"]

log(f"""
📋 Part A: Structural Validation Results
=========================================
Total Files: {summary["total"]}
  ✅ Passed: {summary["passed"]}
  ❌ Failed: {summary["failed"]}
  ⚠️ Warnings: {summary["warnings"]}
""")

for result in validation_results["files"]:
    status = "✅" if result["passed"] else "❌"
    log(f"{status} {result['file']}")
    for error in result.get("errors", []):
        log(f"   ❌ {error}")
    for warning in result.get("warnings", [])[:3]:
        log(f"   ⚠️ {warning}")
```

### Part B: Benchmark Gap Analysis

#### Step 1b: Define Benchmarks

```python
BENCHMARKS = {
    "requirements": {
        "target_count": 247,
        "critical_percent": 10,
        "high_percent": 30
    },
    "specifications": {
        "architecture_kb": 15,
        "security_kb": 8,
        "demos_count": 8
    },
    "traceability": {
        "coverage_percent": 100,
        "spec_mapping_percent": 95
    },
    "estimation": {
        "all_requirements_estimated": True,
        "ai_ratio_documented": True
    }
}
```

#### Step 2b: Assess Current State

```python
def assess_requirements(folder):
    """Assess requirements against benchmarks."""
    requirements = read_json(f"{folder}/shared/requirements-normalized.json")
    reqs = requirements.get("requirements", [])

    total = len(reqs)
    critical = sum(1 for r in reqs if r.get("priority") == "CRITICAL")
    high = sum(1 for r in reqs if r.get("priority") == "HIGH")

    return {
        "total": total,
        "target": BENCHMARKS["requirements"]["target_count"],
        "gap": max(0, BENCHMARKS["requirements"]["target_count"] - total),
        "critical_percent": critical / total * 100 if total > 0 else 0,
        "high_percent": high / total * 100 if total > 0 else 0,
        "meets_target": total >= BENCHMARKS["requirements"]["target_count"]
    }

def assess_specifications(folder):
    """Assess specification documents against benchmarks."""
    import os

    results = {}

    # Architecture size
    arch_path = f"{folder}/outputs/ARCHITECTURE.md"
    if os.path.exists(arch_path):
        size_kb = os.path.getsize(arch_path) / 1024
        results["architecture"] = {
            "size_kb": size_kb,
            "target_kb": BENCHMARKS["specifications"]["architecture_kb"],
            "meets_target": size_kb >= BENCHMARKS["specifications"]["architecture_kb"]
        }

    # Security size
    sec_path = f"{folder}/outputs/SECURITY_REQUIREMENTS.md"
    if os.path.exists(sec_path):
        size_kb = os.path.getsize(sec_path) / 1024
        results["security"] = {
            "size_kb": size_kb,
            "target_kb": BENCHMARKS["specifications"]["security_kb"],
            "meets_target": size_kb >= BENCHMARKS["specifications"]["security_kb"]
        }

    # Demo count
    demos_path = f"{folder}/outputs/DEMO_SCENARIOS.md"
    if os.path.exists(demos_path):
        content = read_file(demos_path)
        scenario_count = len(re.findall(r'## Scenario \d+', content))
        results["demos"] = {
            "count": scenario_count,
            "target": BENCHMARKS["specifications"]["demos_count"],
            "meets_target": scenario_count >= BENCHMARKS["specifications"]["demos_count"]
        }

    return results

req_assessment = assess_requirements(folder)
spec_assessment = assess_specifications(folder)
```

#### Step 3b: Identify Gaps

```python
def identify_gaps(req_assessment, spec_assessment):
    """Compile list of gaps."""
    gaps = []

    # Requirements gaps
    if not req_assessment["meets_target"]:
        gaps.append({
            "area": "Requirements",
            "severity": "HIGH" if req_assessment["gap"] > 50 else "MEDIUM",
            "description": f"Only {req_assessment['total']} requirements extracted (target: {req_assessment['target']})",
            "recommendation": "Review source documents for missed requirements, enable aggressive sub-item extraction"
        })

    # Specification gaps
    for spec_name, spec_data in spec_assessment.items():
        if not spec_data.get("meets_target", True):
            if "size_kb" in spec_data:
                gaps.append({
                    "area": f"Specification: {spec_name}",
                    "severity": "MEDIUM",
                    "description": f"Document is {spec_data['size_kb']:.1f}KB (target: {spec_data['target_kb']}KB)",
                    "recommendation": f"Expand {spec_name} section with more detail"
                })
            elif "count" in spec_data:
                gaps.append({
                    "area": f"Specification: {spec_name}",
                    "severity": "MEDIUM",
                    "description": f"Only {spec_data['count']} items (target: {spec_data['target']})",
                    "recommendation": f"Add more {spec_name} items"
                })

    return gaps

gaps = identify_gaps(req_assessment, spec_assessment)
```

#### Step 4b: Generate Gap Analysis Document

```python
def generate_gap_analysis_md(req_assessment, spec_assessment, gaps):
    doc = f"""# Gap Analysis

**Generated:** {datetime.now().strftime('%Y-%m-%d %H:%M')}

---

## Executive Summary

This analysis compares pipeline outputs against established benchmarks to identify areas for improvement.

**Overall Score:** {calculate_overall_score(req_assessment, spec_assessment, gaps)}%

---

## Requirements Analysis

| Metric | Current | Target | Status |
|--------|---------|--------|--------|
| Total Requirements | {req_assessment["total"]} | {req_assessment["target"]} | {"✅" if req_assessment["meets_target"] else "❌"} |
| Critical % | {req_assessment["critical_percent"]:.1f}% | ~10% | {"✅" if 5 <= req_assessment["critical_percent"] <= 15 else "⚠️"} |
| High % | {req_assessment["high_percent"]:.1f}% | ~30% | {"✅" if 20 <= req_assessment["high_percent"] <= 40 else "⚠️"} |

---

## Specification Analysis

| Document | Current | Target | Status |
|----------|---------|--------|--------|
"""

    for spec_name, spec_data in spec_assessment.items():
        if "size_kb" in spec_data:
            current = f"{spec_data['size_kb']:.1f}KB"
            target = f"{spec_data['target_kb']}KB"
        elif "count" in spec_data:
            current = str(spec_data['count'])
            target = str(spec_data['target'])
        else:
            current = "N/A"
            target = "N/A"

        status = "✅" if spec_data.get("meets_target", True) else "❌"
        doc += f"| {spec_name.title()} | {current} | {target} | {status} |\n"

    doc += """

---

## Identified Gaps

"""

    if not gaps:
        doc += "**No significant gaps identified.** All benchmarks met.\n"
    else:
        doc += "| Area | Severity | Description |\n"
        doc += "|------|----------|-------------|\n"
        for gap in gaps:
            doc += f"| {gap['area']} | {gap['severity']} | {gap['description'][:60]}... |\n"

        doc += "\n### Gap Details and Recommendations\n\n"
        for i, gap in enumerate(gaps, 1):
            doc += f"""#### Gap {i}: {gap['area']}

**Severity:** {gap['severity']}
**Description:** {gap['description']}
**Recommendation:** {gap['recommendation']}

---

"""

    doc += """
## Benchmark Definitions

| Benchmark | Value | Rationale |
|-----------|-------|-----------|
| Requirement Count | 247+ | Industry average for enterprise RFP |
| Architecture Doc | 15KB+ | Comprehensive architecture coverage |
| Security Doc | 8KB+ | Complete security specifications |
| Demo Scenarios | 8+ | Key workflow coverage |
| Traceability | 100% | Full requirement coverage |

---

## Improvement Recommendations

1. **If requirements < 247:** Review sub-item extraction rules
2. **If docs undersized:** Add more detail, examples, diagrams
3. **If demos < 8:** Extract additional workflow scenarios
4. **If traceability < 100%:** Map orphan requirements

---

## Quality Score Breakdown

| Area | Weight | Score | Weighted |
|------|--------|-------|----------|
| Requirements | 30% | {min(100, req_assessment['total']/req_assessment['target']*100):.0f}% | {min(100, req_assessment['total']/req_assessment['target']*100)*0.3:.0f}% |
| Specifications | 40% | {calculate_spec_score(spec_assessment):.0f}% | {calculate_spec_score(spec_assessment)*0.4:.0f}% |
| Gaps | 30% | {max(0, 100 - len(gaps)*20):.0f}% | {max(0, 100 - len(gaps)*20)*0.3:.0f}% |
| **Total** | 100% | | **{calculate_overall_score(req_assessment, spec_assessment, gaps):.0f}%** |
"""

    return doc


def calculate_spec_score(spec_assessment):
    """Calculate specification score."""
    if not spec_assessment:
        return 100
    met = sum(1 for s in spec_assessment.values() if s.get("meets_target", True))
    return met / len(spec_assessment) * 100


def calculate_overall_score(req_assessment, spec_assessment, gaps):
    """Calculate overall quality score."""
    req_score = min(100, req_assessment['total']/req_assessment['target']*100) * 0.3
    spec_score = calculate_spec_score(spec_assessment) * 0.4
    gap_score = max(0, 100 - len(gaps)*20) * 0.3
    return req_score + spec_score + gap_score

gap_analysis_md = generate_gap_analysis_md(req_assessment, spec_assessment, gaps)
write_file(f"{folder}/outputs/GAP_ANALYSIS.md", gap_analysis_md)
```

### Step 3: Merge Results

```python
# Merge Part A structural results with Part B gap data
validation_results["gap_analysis"] = {
    "requirements": req_assessment,
    "specifications": spec_assessment,
    "gaps": gaps,
    "overall_score": calculate_overall_score(req_assessment, spec_assessment, gaps)
}

# Write combined results
write_json(f"{folder}/shared/validation-results.json", validation_results)

# Also write human-readable gap analysis
write_file(f"{folder}/outputs/GAP_ANALYSIS.md", gap_analysis_md)
```

### Step 4: Combined Report

```python
log(f"""
📋 Phase 7: Quality Validation & Gap Analysis - COMPLETE
=========================================================

Part A - Structural Validation:
  Total Files: {validation_results["summary"]["total"]}
  ✅ Passed: {validation_results["summary"]["passed"]}
  ❌ Failed: {validation_results["summary"]["failed"]}
  ⚠️ Warnings: {validation_results["summary"]["warnings"]}

Part B - Gap Analysis:
  Requirements: {req_assessment["total"]}/{req_assessment["target"]} ({"MET" if req_assessment["meets_target"] else "GAP"})
  Gaps Found: {len(gaps)}
  Overall Score: {calculate_overall_score(req_assessment, spec_assessment, gaps):.0f}%

Outputs:
  → {folder}/shared/validation-results.json (combined structural + gap data)
  → {folder}/outputs/GAP_ANALYSIS.md
""")
```

## Quality Checklist

- [ ] `validation-results.json` created in `shared/` (with gap_analysis data merged)
- [ ] `GAP_ANALYSIS.md` created in `outputs/`
- [ ] All output files validated (structural)
- [ ] Size requirements checked
- [ ] Required sections verified
- [ ] JSON files validated
- [ ] Cross-references checked
- [ ] Requirements assessed against benchmarks
- [ ] Specifications assessed against benchmarks
- [ ] Gaps identified with recommendations
- [ ] Overall score calculated
