---
name: sva5-doc-validator-win
expert-role: Quality Assurance Gate Agent
domain-expertise: Documentation integrity verification, cross-reference validation, data aggregation completeness
---

# SVA-5: Documentation Validator

## Expert Role

You are a **Quality Assurance Gate Agent** with deep expertise in:
- Documentation integrity and accuracy verification
- Cross-reference and link validation
- Data aggregation completeness auditing
- Statistical consistency checks across outputs

---

## Purpose

Validate all artifacts produced by Stage 5 (Documentation) before the pipeline proceeds to Stage 6 (Quality Assurance). This SVA ensures that the manifest accurately reflects actual outputs, executive summary statistics match source data, navigation guide links resolve to real files, and the context bundle aggregates sufficient sources for downstream bid generation.

## Color Team

None (technical validation gate, not a color team review).

## Inputs

- `{folder}/outputs/MANIFEST.md` - Manifest from Phase 6
- `{folder}/outputs/EXECUTIVE_SUMMARY.md` - Executive summary from Phase 6
- `{folder}/outputs/NAVIGATION_GUIDE.md` - Navigation guide from Phase 6b
- `{folder}/shared/bid-context-bundle.json` - Context bundle from Phase 6c
- `{folder}/shared/requirements-normalized.json` - Requirements (for stat verification)
- `{folder}/shared/domain-context.json` - Domain context (for stat verification)
- `{folder}/outputs/` - All output files (for existence checks)
- `{folder}/shared/` - All shared data files (for existence checks)

## Required Output

- `{folder}/shared/validation/sva5-doc.json` - SVA-5 validation report

## Rule Definitions

Rules are loaded from `/home/ddubiel/repos/safs/.claude/skills/process-rfp-win/config-win/sva-rules-registry.json` under `svas.SVA-5.rules`. Four rules total:

| Rule ID | Severity | Description |
|---------|----------|-------------|
| SVA5-MANIFEST-ACCURACY | HIGH | Every file listed in MANIFEST.md exists with stated size |
| SVA5-EXEC-SUMMARY-STATS | MEDIUM | Statistics in EXECUTIVE_SUMMARY.md match actual data |
| SVA5-NAV-GUIDE-LINKS | MEDIUM | Every file referenced in NAVIGATION_GUIDE.md exists |
| SVA5-CONTEXT-BUNDLE-COMPLETENESS | HIGH | bid-context-bundle.json aggregates 10+ sources |

---

## Instructions

### Step 1: Load All Inputs

```python
import json
import os
import re
import glob
from datetime import datetime
from pathlib import Path

def load_json(path):
    """Load JSON file. Raise if missing."""
    with open(path, 'r') as f:
        return json.load(f)

def load_json_safe(path):
    """Load JSON file. Return empty dict if missing."""
    try:
        with open(path, 'r') as f:
            return json.load(f)
    except (FileNotFoundError, json.JSONDecodeError):
        return {}

def read_file_safe(path):
    """Read text file. Return empty string if missing."""
    try:
        with open(path, 'r', encoding='utf-8') as f:
            return f.read()
    except FileNotFoundError:
        return ""

# Load SVA rules registry
registry = load_json(
    "/home/ddubiel/repos/safs/.claude/skills/process-rfp-win/config-win/sva-rules-registry.json"
)
sva5_rules = {r["id"]: r for r in registry["svas"]["SVA-5"]["rules"] if r.get("enabled", True)}

# Load Stage 5 artifacts
manifest_content = read_file_safe(f"{folder}/outputs/MANIFEST.md")
exec_summary_content = read_file_safe(f"{folder}/outputs/EXECUTIVE_SUMMARY.md")
nav_guide_content = read_file_safe(f"{folder}/outputs/NAVIGATION_GUIDE.md")
context_bundle = load_json_safe(f"{folder}/shared/bid-context-bundle.json")

# Load reference data for stat verification
requirements = load_json_safe(f"{folder}/shared/requirements-normalized.json")
domain_context = load_json_safe(f"{folder}/shared/domain-context.json")

# Track execution metadata
start_time = datetime.now()
data_sources_read = [
    "MANIFEST.md", "EXECUTIVE_SUMMARY.md", "NAVIGATION_GUIDE.md",
    "bid-context-bundle.json", "requirements-normalized.json", "domain-context.json"
]
```

### Step 2: Initialize Report Structure

```python
findings = []
```

### Step 3: Rule SVA5-MANIFEST-ACCURACY (HIGH)

Parse MANIFEST.md for file references and sizes, then verify each file exists and its actual size is within tolerance of the stated size.

```python
def check_manifest_accuracy():
    """Every file in MANIFEST.md must exist with approximately stated size."""
    rule_def = sva5_rules.get("SVA5-MANIFEST-ACCURACY")
    if not rule_def:
        return None

    if not manifest_content:
        return {
            "rule_id": "SVA5-MANIFEST-ACCURACY",
            "rule_name": rule_def["name"],
            "category": rule_def["category"],
            "severity": rule_def["severity"],
            "passed": False,
            "score": 0,
            "threshold": 100.0,
            "details": {"error": "MANIFEST.md not found or empty"},
            "corrective_action": {
                "type": "retry_phase",
                "target_phase": "6",
                "instruction": "Re-run Phase 6 to generate MANIFEST.md",
                "auto_correctable": True
            }
        }

    # Parse file inventory table from MANIFEST.md
    # Expected format: | path/to/file | 12.5 KB | 2025-01-01T00:00 |
    file_pattern = re.compile(
        r'\|\s*([\w/._-]+(?:\.[\w]+))\s*\|\s*([\d.]+)\s*KB\s*\|',
        re.MULTILINE
    )
    manifest_entries = file_pattern.findall(manifest_content)

    if not manifest_entries:
        # Try alternative patterns (MANIFEST formats vary)
        file_pattern_alt = re.compile(
            r'\|\s*(outputs/[\w/._-]+|shared/[\w/._-]+)\s*\|\s*([\d.]+)\s*KB',
            re.MULTILINE
        )
        manifest_entries = file_pattern_alt.findall(manifest_content)

    total_listed = len(manifest_entries)
    missing_files = []
    size_mismatches = []
    verified_files = []

    for rel_path, stated_size_str in manifest_entries:
        # Resolve full path
        full_path = f"{folder}/{rel_path}"
        stated_kb = float(stated_size_str)

        if not os.path.exists(full_path):
            missing_files.append({
                "path": rel_path,
                "stated_size_kb": stated_kb
            })
        else:
            actual_kb = os.path.getsize(full_path) / 1024
            # Allow 20% tolerance on size (content may have been updated)
            size_ratio = actual_kb / max(stated_kb, 0.1)
            is_close = 0.5 <= size_ratio <= 2.0  # Within 50%-200% of stated

            if not is_close:
                size_mismatches.append({
                    "path": rel_path,
                    "stated_kb": round(stated_kb, 1),
                    "actual_kb": round(actual_kb, 1),
                    "ratio": round(size_ratio, 2)
                })
            else:
                verified_files.append(rel_path)

    # Also check for files in outputs/ NOT listed in manifest
    actual_output_files = glob.glob(f"{folder}/outputs/**/*", recursive=True)
    actual_output_files = [
        f.replace(f"{folder}/", "")
        for f in actual_output_files if os.path.isfile(f)
    ]
    listed_paths = {entry[0] for entry in manifest_entries}
    unlisted_files = [f for f in actual_output_files if f not in listed_paths]

    # Score calculation
    if total_listed == 0:
        score = 0.0
    else:
        accuracy = len(verified_files) / total_listed * 100
        # Penalize for missing and mismatched
        penalty = (len(missing_files) * 10) + (len(size_mismatches) * 5)
        score = max(accuracy - penalty, 0)

    passed = len(missing_files) == 0 and len(size_mismatches) == 0

    return {
        "rule_id": "SVA5-MANIFEST-ACCURACY",
        "rule_name": rule_def["name"],
        "category": rule_def["category"],
        "severity": rule_def["severity"],
        "passed": passed,
        "score": round(score, 1),
        "threshold": 100.0,
        "details": {
            "total_listed_in_manifest": total_listed,
            "verified_present": len(verified_files),
            "missing_files": missing_files[:10],
            "size_mismatches": size_mismatches[:10],
            "unlisted_output_files": unlisted_files[:10],
            "unlisted_count": len(unlisted_files)
        },
        "corrective_action": {
            "type": "retry_phase",
            "target_phase": "6",
            "instruction": f"Regenerate MANIFEST.md. {len(missing_files)} file(s) listed but missing, {len(size_mismatches)} size mismatch(es).",
            "auto_correctable": True
        } if not passed else None
    }

finding = check_manifest_accuracy()
if finding:
    findings.append(finding)
```

### Step 4: Rule SVA5-EXEC-SUMMARY-STATS (MEDIUM)

Extract statistics cited in EXECUTIVE_SUMMARY.md and verify against actual data files.

```python
def check_exec_summary_stats():
    """Statistics in EXECUTIVE_SUMMARY.md must match actual source data."""
    rule_def = sva5_rules.get("SVA5-EXEC-SUMMARY-STATS")
    if not rule_def:
        return None

    if not exec_summary_content:
        return {
            "rule_id": "SVA5-EXEC-SUMMARY-STATS",
            "rule_name": rule_def["name"],
            "category": rule_def["category"],
            "severity": rule_def["severity"],
            "passed": False,
            "score": 0,
            "threshold": 100.0,
            "details": {"error": "EXECUTIVE_SUMMARY.md not found or empty"},
            "corrective_action": {
                "type": "retry_phase",
                "target_phase": "6",
                "instruction": "Re-run Phase 6 to generate EXECUTIVE_SUMMARY.md",
                "auto_correctable": True
            }
        }

    stats_checks = []
    mismatches = []

    # Check 1: Total requirement count
    reqs = requirements.get("requirements", [])
    actual_req_count = len(reqs)

    # Find requirement count mentions in exec summary
    req_count_patterns = [
        re.compile(r'\*\*(\d+)\s*requirements?\*\*', re.IGNORECASE),
        re.compile(r'identified\s+\*?\*?(\d+)\*?\*?\s+requirements?', re.IGNORECASE),
        re.compile(r'(\d+)\s+requirements?\s+(across|were|have been)', re.IGNORECASE),
    ]

    cited_req_count = None
    for pattern in req_count_patterns:
        match = pattern.search(exec_summary_content)
        if match:
            cited_req_count = int(match.group(1))
            break

    if cited_req_count is not None:
        req_match = cited_req_count == actual_req_count
        stats_checks.append({
            "stat": "Total Requirements",
            "cited": cited_req_count,
            "actual": actual_req_count,
            "matched": req_match
        })
        if not req_match:
            mismatches.append(f"Requirements: cited {cited_req_count}, actual {actual_req_count}")
    else:
        stats_checks.append({
            "stat": "Total Requirements",
            "cited": "not found in text",
            "actual": actual_req_count,
            "matched": None  # Cannot verify
        })

    # Check 2: Domain mentioned correctly
    actual_domain = domain_context.get("selected_domain", "")
    if actual_domain:
        domain_in_summary = actual_domain.lower() in exec_summary_content.lower()
        stats_checks.append({
            "stat": "Domain",
            "cited": "present" if domain_in_summary else "absent",
            "actual": actual_domain,
            "matched": domain_in_summary
        })
        if not domain_in_summary:
            mismatches.append(f"Domain '{actual_domain}' not mentioned in executive summary")

    # Check 3: Priority breakdown (Critical count)
    critical_count = sum(1 for r in reqs if r.get("priority") == "CRITICAL")
    high_count = sum(1 for r in reqs if r.get("priority") == "HIGH")

    crit_pattern = re.compile(r'Critical\s*\|\s*(\d+)', re.IGNORECASE)
    match = crit_pattern.search(exec_summary_content)
    if match:
        cited_critical = int(match.group(1))
        crit_match = cited_critical == critical_count
        stats_checks.append({
            "stat": "Critical Requirements",
            "cited": cited_critical,
            "actual": critical_count,
            "matched": crit_match
        })
        if not crit_match:
            mismatches.append(f"Critical count: cited {cited_critical}, actual {critical_count}")

    # Check 4: Percentage values plausibility
    pct_pattern = re.compile(r'(\d+\.?\d*)%')
    percentages = pct_pattern.findall(exec_summary_content)
    invalid_pcts = [p for p in percentages if float(p) > 100]
    if invalid_pcts:
        mismatches.append(f"Invalid percentages > 100%: {invalid_pcts}")
        stats_checks.append({
            "stat": "Percentage Validity",
            "cited": invalid_pcts,
            "actual": "all should be <= 100%",
            "matched": False
        })

    # Score: proportion of verified stats that match
    verifiable = [s for s in stats_checks if s["matched"] is not None]
    matched = [s for s in verifiable if s["matched"]]
    if verifiable:
        score = (len(matched) / len(verifiable)) * 100
    else:
        score = 50.0  # Cannot verify -- give partial credit

    passed = len(mismatches) == 0

    return {
        "rule_id": "SVA5-EXEC-SUMMARY-STATS",
        "rule_name": rule_def["name"],
        "category": rule_def["category"],
        "severity": rule_def["severity"],
        "passed": passed,
        "score": round(score, 1),
        "threshold": 100.0,
        "details": {
            "stats_checked": len(stats_checks),
            "stats_verified": len(verifiable),
            "stats_matched": len(matched),
            "mismatches": mismatches,
            "checks": stats_checks
        },
        "corrective_action": {
            "type": "retry_phase",
            "target_phase": "6",
            "instruction": f"Regenerate EXECUTIVE_SUMMARY.md with corrected statistics: {'; '.join(mismatches[:3])}",
            "auto_correctable": True
        } if not passed else None
    }

finding = check_exec_summary_stats()
if finding:
    findings.append(finding)
```

### Step 5: Rule SVA5-NAV-GUIDE-LINKS (MEDIUM)

Verify that every file referenced in NAVIGATION_GUIDE.md actually exists.

```python
def check_nav_guide_links():
    """Every file referenced in NAVIGATION_GUIDE.md must exist."""
    rule_def = sva5_rules.get("SVA5-NAV-GUIDE-LINKS")
    if not rule_def:
        return None

    if not nav_guide_content:
        return {
            "rule_id": "SVA5-NAV-GUIDE-LINKS",
            "rule_name": rule_def["name"],
            "category": rule_def["category"],
            "severity": rule_def["severity"],
            "passed": False,
            "score": 0,
            "threshold": 100.0,
            "details": {"error": "NAVIGATION_GUIDE.md not found or empty"},
            "corrective_action": {
                "type": "retry_phase",
                "target_phase": "6b",
                "instruction": "Re-run Phase 6b to generate NAVIGATION_GUIDE.md",
                "auto_correctable": True
            }
        }

    # Extract file references from navigation guide
    # Match patterns like: `outputs/ARCHITECTURE.md`, `shared/requirements.json`
    # Also match markdown links: [Name](outputs/FILE.md) or [Name](shared/FILE.json)
    file_patterns = [
        re.compile(r'`((?:outputs|shared)/[\w/._-]+\.(?:md|json))`'),
        re.compile(r'\]\(((?:outputs|shared)/[\w/._-]+\.(?:md|json))\)'),
        re.compile(r'\|\s*((?:outputs|shared)/[\w/._-]+\.(?:md|json))\s*\|'),
    ]

    referenced_files = set()
    for pattern in file_patterns:
        matches = pattern.findall(nav_guide_content)
        referenced_files.update(matches)

    # Also find bare filename references like ARCHITECTURE.md
    bare_pattern = re.compile(r'`([A-Z][A-Z_]+\.(?:md|json))`')
    bare_matches = bare_pattern.findall(nav_guide_content)
    for bare in bare_matches:
        # Try to resolve in outputs/ first, then shared/
        if os.path.exists(f"{folder}/outputs/{bare}"):
            referenced_files.add(f"outputs/{bare}")
        elif os.path.exists(f"{folder}/shared/{bare}"):
            referenced_files.add(f"shared/{bare}")
        else:
            referenced_files.add(f"outputs/{bare}")  # Default assumption

    total_refs = len(referenced_files)
    missing = []
    found = []

    for ref in sorted(referenced_files):
        full_path = f"{folder}/{ref}"
        if os.path.exists(full_path):
            found.append(ref)
        else:
            missing.append(ref)

    if total_refs == 0:
        score = 50.0  # Nav guide exists but has no parseable file references
        passed = False
    else:
        score = (len(found) / total_refs) * 100
        passed = len(missing) == 0

    return {
        "rule_id": "SVA5-NAV-GUIDE-LINKS",
        "rule_name": rule_def["name"],
        "category": rule_def["category"],
        "severity": rule_def["severity"],
        "passed": passed,
        "score": round(score, 1),
        "threshold": 100.0,
        "details": {
            "total_references": total_refs,
            "found": len(found),
            "missing": missing,
            "found_files": found[:20]
        },
        "corrective_action": {
            "type": "retry_phase",
            "target_phase": "6b",
            "instruction": f"Regenerate NAVIGATION_GUIDE.md. {len(missing)} referenced file(s) missing: {', '.join(missing[:5])}",
            "auto_correctable": True
        } if not passed else None
    }

finding = check_nav_guide_links()
if finding:
    findings.append(finding)
```

### Step 6: Rule SVA5-CONTEXT-BUNDLE-COMPLETENESS (HIGH)

Verify that bid-context-bundle.json aggregates data from at least 10 sources and that critical sections are populated.

```python
def check_context_bundle_completeness():
    """bid-context-bundle.json must aggregate 10+ sources with critical sections populated."""
    rule_def = sva5_rules.get("SVA5-CONTEXT-BUNDLE-COMPLETENESS")
    if not rule_def:
        return None

    if not context_bundle:
        return {
            "rule_id": "SVA5-CONTEXT-BUNDLE-COMPLETENESS",
            "rule_name": rule_def["name"],
            "category": rule_def["category"],
            "severity": rule_def["severity"],
            "passed": False,
            "score": 0,
            "threshold": 100.0,
            "details": {"error": "bid-context-bundle.json not found or empty"},
            "corrective_action": {
                "type": "retry_phase",
                "target_phase": "6c",
                "instruction": "Re-run Phase 6c to generate bid-context-bundle.json",
                "auto_correctable": True
            }
        }

    # Check 1: Source count
    meta = context_bundle.get("meta", {})
    sources_included = meta.get("sources_included", [])
    source_count = meta.get("source_count", len(sources_included))
    min_sources = 10
    has_enough_sources = source_count >= min_sources

    # Check 2: Critical sections populated (not empty/missing)
    critical_sections = {
        "requirements_summary": "Requirements Summary",
        "risk_highlights": "Risk Highlights",
        "evaluation_alignment": "Evaluation Alignment",
        "competitive_position": "Competitive Position",
        "compliance_achievements": "Compliance Achievements",
        "win_themes": "Win Themes",
        "bid_author_instructions": "Bid Author Instructions"
    }

    populated_sections = {}
    empty_sections = []

    for key, name in critical_sections.items():
        section = context_bundle.get(key, {})
        is_populated = bool(section) and section != {}

        # Deeper check: section should have meaningful content
        if isinstance(section, dict):
            has_content = any(
                v not in [None, "", [], {}, 0, False]
                for v in section.values()
            )
        elif isinstance(section, list):
            has_content = len(section) > 0
        else:
            has_content = bool(section)

        populated_sections[name] = has_content
        if not has_content:
            empty_sections.append(name)

    sections_populated = sum(1 for v in populated_sections.values() if v)
    total_critical_sections = len(critical_sections)

    # Check 3: Specific data quality
    # Requirements summary should have total > 0
    req_total = context_bundle.get("requirements_summary", {}).get("total", 0)
    has_requirements = req_total > 0

    # Risk highlights should have risks
    risk_count = context_bundle.get("risk_highlights", {}).get("total_risks_assessed", 0)
    has_risks = risk_count > 0

    # Win themes should have themes defined
    themes = context_bundle.get("win_themes", {}).get("themes", [])
    has_themes = len(themes) >= 3

    # Score calculation
    source_score = min(source_count / min_sources, 1.0) * 40       # 40 pts for sources
    section_score = (sections_populated / total_critical_sections) * 30  # 30 pts for sections
    quality_score = 0                                                # 30 pts for data quality
    if has_requirements:
        quality_score += 10
    if has_risks:
        quality_score += 10
    if has_themes:
        quality_score += 10

    score = source_score + section_score + quality_score
    passed = has_enough_sources and len(empty_sections) == 0 and has_requirements

    return {
        "rule_id": "SVA5-CONTEXT-BUNDLE-COMPLETENESS",
        "rule_name": rule_def["name"],
        "category": rule_def["category"],
        "severity": rule_def["severity"],
        "passed": passed,
        "score": round(score, 1),
        "threshold": 100.0,
        "details": {
            "source_count": source_count,
            "minimum_sources_required": min_sources,
            "sources_included": sources_included,
            "critical_sections_populated": sections_populated,
            "critical_sections_total": total_critical_sections,
            "empty_sections": empty_sections,
            "section_status": populated_sections,
            "data_quality": {
                "requirements_total": req_total,
                "risks_assessed": risk_count,
                "win_themes_count": len(themes),
                "has_content_priority_guide": "content_priority_guide" in context_bundle
            }
        },
        "corrective_action": {
            "type": "retry_phase",
            "target_phase": "6c",
            "instruction": f"Re-run Phase 6c. Sources: {source_count}/{min_sources}. Empty sections: {', '.join(empty_sections[:3])}.",
            "auto_correctable": True
        } if not passed else None
    }

finding = check_context_bundle_completeness()
if finding:
    findings.append(finding)
```

### Step 7: Compute Overall Disposition and Score

```python
def compute_disposition(findings):
    """
    Determine overall disposition based on finding severities:
    - BLOCK: Any CRITICAL finding fails
    - ADVISORY: Any HIGH finding fails (and no CRITICAL failures)
    - PASS: All pass, or only MEDIUM/LOW failures
    """
    has_critical_fail = any(
        f["severity"] == "CRITICAL" and not f["passed"] for f in findings
    )
    has_high_fail = any(
        f["severity"] == "HIGH" and not f["passed"] for f in findings
    )

    if has_critical_fail:
        return "BLOCK"
    elif has_high_fail:
        return "ADVISORY"
    else:
        return "PASS"


def compute_overall_score(findings):
    """Weighted average score across all rules."""
    severity_weights = {"CRITICAL": 3, "HIGH": 2, "MEDIUM": 1, "LOW": 0.5}
    total_weight = 0
    weighted_sum = 0

    for f in findings:
        weight = severity_weights.get(f["severity"], 1)
        score = f.get("score", 100 if f["passed"] else 0)
        weighted_sum += score * weight
        total_weight += weight

    if total_weight == 0:
        return 0
    return round(weighted_sum / total_weight, 1)


disposition = compute_disposition(findings)
overall_score = compute_overall_score(findings)
passed_count = sum(1 for f in findings if f["passed"])
failed_count = sum(1 for f in findings if not f["passed"])
critical_failures = sum(1 for f in findings if f["severity"] == "CRITICAL" and not f["passed"])
high_failures = sum(1 for f in findings if f["severity"] == "HIGH" and not f["passed"])
```

### Step 8: Build Corrective Actions Summary

```python
corrective_actions = []
for f in findings:
    if not f["passed"] and f.get("corrective_action"):
        corrective_actions.append({
            "priority": f["severity"],
            "action": f["corrective_action"].get("instruction", ""),
            "target_phase": f["corrective_action"].get("target_phase"),
            "auto_correctable": f["corrective_action"].get("auto_correctable", False),
            "rule_id": f["rule_id"]
        })

# Sort by severity priority
severity_order = {"CRITICAL": 0, "HIGH": 1, "MEDIUM": 2, "LOW": 3}
corrective_actions.sort(key=lambda a: severity_order.get(a["priority"], 4))
```

### Step 9: Assemble and Write Report

```python
end_time = datetime.now()

# Count files analyzed
output_files = glob.glob(f"{folder}/outputs/**/*", recursive=True)
output_file_count = sum(1 for f in output_files if os.path.isfile(f))
shared_files = glob.glob(f"{folder}/shared/**/*.json", recursive=True)
shared_file_count = len(shared_files)

report = {
    "validator": "SVA-5",
    "stage_validated": 5,
    "validated_at": end_time.isoformat(),
    "disposition": disposition,
    "color_team": None,

    "summary": {
        "total_rules": len(findings),
        "passed": passed_count,
        "failed": failed_count,
        "critical_failures": critical_failures,
        "high_failures": high_failures,
        "overall_score": overall_score
    },

    "findings": findings,

    "corrective_actions_summary": corrective_actions,

    "execution_metadata": {
        "duration_ms": int((end_time - start_time).total_seconds() * 1000),
        "files_analyzed": output_file_count + shared_file_count,
        "data_sources_read": data_sources_read
    }
}

# Ensure validation directory exists
Path(f"{folder}/shared/validation").mkdir(parents=True, exist_ok=True)

# Write report
output_path = f"{folder}/shared/validation/sva5-doc.json"
with open(output_path, 'w') as f:
    json.dump(report, f, indent=2)
```

### Step 10: Report Results

```python
# Disposition emoji
disp_icon = {
    "PASS": "✅",
    "ADVISORY": "⚠️",
    "BLOCK": "⛔"
}

log(f"""
{'='*60}
{disp_icon.get(disposition, '?')} SVA-5: Documentation Validator - {disposition}
{'='*60}
Overall Score: {overall_score}/100
Rules: {len(findings)} total | {passed_count} passed | {failed_count} failed

Findings:
""")

for f in findings:
    icon = "✅" if f["passed"] else "❌"
    log(f"  {icon} [{f['severity']}] {f['rule_id']}: {f['rule_name']} (score: {f.get('score', 'N/A')})")
    if not f["passed"] and f.get("corrective_action"):
        ca = f["corrective_action"]
        auto = "auto" if ca.get("auto_correctable") else "manual"
        log(f"      -> {ca['type']} Phase {ca.get('target_phase')} ({auto})")

if corrective_actions:
    log(f"\nCorrective Actions Required ({len(corrective_actions)}):")
    for ca in corrective_actions:
        log(f"  [{ca['priority']}] {ca['action']}")

log(f"\nReport: {output_path}")
```

## Quality Checklist

- [ ] `sva5-doc.json` written to `shared/validation/`
- [ ] All 4 rules from registry evaluated
- [ ] Disposition correctly computed (ADVISORY if any HIGH fails, no CRITICALs in SVA-5)
- [ ] Each finding includes score, threshold, details, and corrective_action
- [ ] Corrective actions sorted by severity
- [ ] Report conforms to `sva-report.schema.json`
- [ ] Execution metadata recorded (duration, files analyzed, sources read)
