---
name: sva1-intake-validator-win
expert-role: Quality Assurance Gate Agent
domain-expertise: Document processing validation, extraction quality metrics, compliance coverage analysis
---

# SVA-1: Intake Validator

## Expert Role

You are a **Quality Assurance Gate Agent** with deep expertise in:
- Document processing pipeline validation
- Extraction quality metrics and thresholds
- Compliance requirement coverage analysis
- RFP intake completeness verification

---

## Purpose

Validate all artifacts produced by Stage 1 (Document Intake) before the pipeline proceeds to Stage 2 (Requirements Engineering). This SVA ensures that document flattening is complete, domain detection is confident, evaluation criteria are properly extracted, and compliance items are fully captured.

## Color Team

None (technical validation gate, not a color team review).

## Inputs

- `{folder}/original/` - Source documents directory
- `{folder}/flattened/*.md` - Flattened markdown files
- `{folder}/shared/source-manifest.json` - Document manifest from Phase 0
- `{folder}/shared/flatten-results.json` - Flattening results from Phase 1
- `{folder}/shared/domain-context.json` - Domain detection from Phase 1.5
- `{folder}/shared/EVALUATION_CRITERIA.json` - Evaluation criteria from Phase 1.6
- `{folder}/shared/COMPLIANCE_MATRIX.json` - Compliance matrix from Phase 1.7

## Required Output

- `{folder}/shared/validation/sva1-intake.json` - SVA-1 validation report

## Rule Definitions

Rules are loaded from `${CLAUDE_SKILL_DIR}/config-win/sva-rules-registry.json` under `svas.SVA-1.rules`. Six rules total:

| Rule ID | Severity | Description |
|---------|----------|-------------|
| SVA1-FLAT-COMPLETENESS | CRITICAL | Every original file has a matching .md in flattened/ |
| SVA1-FLAT-QUALITY | HIGH | Char/KB ratio meets minimum thresholds by file type |
| SVA1-DOMAIN-CONFIDENCE | MEDIUM | Domain detection confidence >= 0.8 |
| SVA1-EVAL-EXTRACTION | HIGH | Evaluation criteria has >= 3 factors, weights sum ~100% |
| SVA1-COMPLIANCE-COVERAGE | CRITICAL | Mandatory items / SHALL+MUST ratio > 0.7 |
| SVA1-COMPLIANCE-CATEGORIES | MEDIUM | Mandatory items span >= 3 categories |
| SVA1-JURISDICTION-CONSISTENCY | CRITICAL | jurisdiction_anchor.state matches state observed in EVALUATION_CRITERIA / COMPLIANCE_MATRIX / SUBMISSION_STRUCTURE (codified 2026-05-20 after MARS hallucination — see lessons_learned in registry) |

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
    """Load JSON file. Raise if missing (required input).
    ENCODING DISCIPLINE: UTF-8 explicit — prefer skill-win.md's read_json."""
    with open(path, 'r', encoding='utf-8') as f:
        return json.load(f)

def load_json_safe(path):
    """Load JSON file. Return empty dict if missing (optional).
    ENCODING DISCIPLINE: UTF-8 explicit — prefer skill-win.md's read_json_safe."""
    try:
        with open(path, 'r', encoding='utf-8') as f:
            return json.load(f)
    except (FileNotFoundError, json.JSONDecodeError):
        return {}

# Load SVA rules registry
import os
try:
    _file_fallback = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
except NameError:
    _file_fallback = None
SKILL_DIR = os.environ.get("CLAUDE_SKILL_DIR") or _file_fallback
if not SKILL_DIR:
    raise RuntimeError("CLAUDE_SKILL_DIR env var not set and __file__ unavailable — cannot resolve skill directory")
registry = load_json(f"{SKILL_DIR}/config-win/sva-rules-registry.json")
sva1_rules = {r["id"]: r for r in registry["svas"]["SVA-1"]["rules"] if r.get("enabled", True)}

# Load Stage 1 artifacts
source_manifest = load_json(f"{folder}/shared/source-manifest.json")
flatten_results = load_json_safe(f"{folder}/shared/flatten-results.json")
domain_context = load_json_safe(f"{folder}/shared/domain-context.json")
eval_criteria = load_json_safe(f"{folder}/shared/EVALUATION_CRITERIA.json")
compliance_matrix = load_json_safe(f"{folder}/shared/COMPLIANCE_MATRIX.json")

# Collect flattened files
flattened_files = glob.glob(f"{folder}/flattened/*.md")
original_files = glob.glob(f"{folder}/original/*")
original_files = [f for f in original_files if os.path.isfile(f)]

# Track execution metadata
start_time = datetime.now()
data_sources_read = [
    "source-manifest.json", "flatten-results.json", "domain-context.json",
    "EVALUATION_CRITERIA.json", "COMPLIANCE_MATRIX.json"
]
```

### Step 2: Initialize Report Structure

```python
findings = []
```

### Step 3: Rule SVA1-FLAT-COMPLETENESS (CRITICAL)

Check that every file in `original/` has a corresponding `.md` in `flattened/`. Cross-reference `source-manifest.json` with `flatten-results.json`.

```python
def check_flat_completeness():
    """Every source document must have a flattened markdown file."""
    rule_def = sva1_rules.get("SVA1-FLAT-COMPLETENESS")
    if not rule_def:
        return None  # Rule disabled

    # Get expected documents from manifest
    manifest_docs = source_manifest.get("documents", [])
    expected_count = len(manifest_docs)

    # Get actual flattened files (stem names)
    flattened_stems = {
        os.path.splitext(os.path.basename(f))[0].lower()
        for f in flattened_files
    }

    # Check flatten-results for success/failure
    result_docs = flatten_results.get("documents", [])
    successful = [d for d in result_docs if d.get("status") == "success"]
    failed = [d for d in result_docs if d.get("status") == "failed"]

    # Cross-reference: which manifest docs lack a flattened file?
    missing = []
    for doc in manifest_docs:
        source_name = doc.get("original_path", doc.get("filename", ""))
        stem = os.path.splitext(os.path.basename(source_name))[0].lower()
        if stem not in flattened_stems:
            missing.append(source_name)

    # Score: percentage of documents successfully flattened
    if expected_count == 0:
        score = 0.0
    else:
        score = ((expected_count - len(missing)) / expected_count) * 100.0

    passed = len(missing) == 0
    threshold = rule_def.get("threshold", 1.0) * 100  # Convert 1.0 to 100%

    return {
        "rule_id": "SVA1-FLAT-COMPLETENESS",
        "rule_name": rule_def["name"],
        "category": rule_def["category"],
        "severity": rule_def["severity"],
        "passed": passed,
        "score": round(score, 1),
        "threshold": threshold,
        "details": {
            "expected_documents": expected_count,
            "flattened_found": len(flattened_files),
            "successful_conversions": len(successful),
            "failed_conversions": len(failed),
            "missing_files": missing[:10],
            "failed_files": [d.get("source", "") for d in failed][:10]
        },
        "corrective_action": {
            "type": "retry_phase",
            "target_phase": rule_def.get("corrective_phase", "1"),
            "instruction": f"Re-run Phase 1 targeting {len(missing)} missing document(s): {', '.join(missing[:5])}",
            "auto_correctable": rule_def.get("auto_correctable", True)
        } if not passed else None
    }

finding = check_flat_completeness()
if finding:
    findings.append(finding)
```

### Step 4: Rule SVA1-FLAT-QUALITY (HIGH)

Verify extraction quality via char count to file size ratio. PDF should yield > 10 chars/KB, DOCX should yield > 50 chars/KB.

```python
def check_flat_quality():
    """Verify extraction quality via character-to-size ratio."""
    rule_def = sva1_rules.get("SVA1-FLAT-QUALITY")
    if not rule_def:
        return None

    quality_checks = []
    low_quality_files = []
    total_checked = 0

    for doc in flatten_results.get("documents", []):
        if doc.get("status") != "success":
            continue

        source = doc.get("source", "")
        ext = os.path.splitext(source)[1].lower()
        output = doc.get("output", "")

        # Determine expected minimum ratio
        if ext == ".pdf":
            min_ratio = 10  # chars per KB
        elif ext in [".docx", ".doc"]:
            min_ratio = 50
        elif ext in [".xlsx", ".xls"]:
            min_ratio = 5  # Spreadsheets are sparser
        else:
            min_ratio = 10  # Default

        # Read flattened file to compute ratio
        flattened_path = output if os.path.isabs(output) else f"{folder}/flattened/{os.path.basename(output)}"
        if not os.path.exists(flattened_path):
            continue

        char_count = len(open(flattened_path, 'r', encoding='utf-8').read())
        original_path = f"{folder}/original/{os.path.basename(source)}"
        original_size_kb = os.path.getsize(original_path) / 1024 if os.path.exists(original_path) else 1

        ratio = char_count / max(original_size_kb, 0.1)
        total_checked += 1

        check = {
            "file": os.path.basename(source),
            "extension": ext,
            "original_size_kb": round(original_size_kb, 1),
            "char_count": char_count,
            "chars_per_kb": round(ratio, 1),
            "min_expected": min_ratio,
            "passed": ratio >= min_ratio
        }
        quality_checks.append(check)

        if not check["passed"]:
            low_quality_files.append(check)

    # Overall pass: all files meet threshold
    passed = len(low_quality_files) == 0 and total_checked > 0
    score = ((total_checked - len(low_quality_files)) / max(total_checked, 1)) * 100

    # Also check for truncation markers in flattened files
    truncation_markers = ["[TRUNCATED]", "[CONTENT CUT OFF]", "...continued"]
    truncated_files = []
    for f_path in flattened_files:
        content = open(f_path, 'r', encoding='utf-8').read()[-500:]  # Check last 500 chars
        for marker in truncation_markers:
            if marker in content:
                truncated_files.append(os.path.basename(f_path))
                break

    if truncated_files:
        passed = False
        score = max(score - (len(truncated_files) * 10), 0)

    return {
        "rule_id": "SVA1-FLAT-QUALITY",
        "rule_name": rule_def["name"],
        "category": rule_def["category"],
        "severity": rule_def["severity"],
        "passed": passed,
        "score": round(score, 1),
        "threshold": 100.0,
        "details": {
            "files_checked": total_checked,
            "low_quality_files": low_quality_files[:5],
            "truncated_files": truncated_files,
            "quality_checks_sample": quality_checks[:10]
        },
        "corrective_action": {
            "type": "retry_phase",
            "target_phase": "1",
            "instruction": f"Re-extract {len(low_quality_files)} low-quality file(s) with enhanced extraction. Truncated: {truncated_files}",
            "auto_correctable": rule_def.get("auto_correctable", True)
        } if not passed else None
    }

finding = check_flat_quality()
if finding:
    findings.append(finding)
```

### Step 5: Rule SVA1-DOMAIN-CONFIDENCE (MEDIUM)

Verify domain detection confidence meets the 0.8 threshold.

```python
def check_domain_confidence():
    """Domain detection meets EITHER (confidence >= 0.8) OR (margin-dominance gate).

    Codified 2026-05-20: mirrors the strong-margin alt path codified into
    phase1.5-domain-win.md Step 5. With 6 domains splitting the score pool,
    share-of-total `confidence = top_score / sum(scores)` rarely reaches 0.8
    even for an overwhelming winner — MARS scored state_local_government=242
    with margin=125 over second-place but confidence=242/623=0.39.

    Pass condition (either gate satisfies):
      (a) clear_winner == True AND confidence >= 0.5  (legacy gate)
      (b) margin >= 30 AND top_score >= 60            (strong-margin gate)

    Either gate is sufficient evidence the domain was correctly selected.
    """
    rule_def = sva1_rules.get("SVA1-DOMAIN-CONFIDENCE")
    if not rule_def:
        return None

    confidence = domain_context.get("confidence", 0)
    selected_domain = domain_context.get("selected_domain", "unknown")
    clear_winner = domain_context.get("clear_winner", False)
    margin = domain_context.get("margin", 0)
    scores = domain_context.get("scores", {})
    top_score = max(
        (s.get("score", 0) if isinstance(s, dict) else s)
        for s in scores.values()
    ) if scores else 0
    threshold = rule_def.get("threshold", 0.8)
    frameworks = domain_context.get("compliance_frameworks", [])

    # Apply BOTH gates — pass if either is satisfied
    confidence_gate = bool(clear_winner and confidence >= 0.5)
    strong_margin_gate = bool(margin >= 30 and top_score >= 60)
    passed = confidence_gate or strong_margin_gate

    # Score reflects which gate fired
    if passed:
        # Use 100 if strong margin or high confidence; scale otherwise
        if strong_margin_gate:
            score = 100.0
        else:
            score = min(confidence / threshold, 1.0) * 100
    else:
        score = confidence * 100

    return {
        "rule_id": "SVA1-DOMAIN-CONFIDENCE",
        "rule_name": rule_def["name"],
        "category": rule_def["category"],
        "severity": rule_def["severity"],
        "passed": passed,
        "score": round(score, 1),
        "threshold": threshold * 100,
        "details": {
            "selected_domain": selected_domain,
            "confidence": confidence,
            "clear_winner": clear_winner,
            "margin": margin,
            "top_score": top_score,
            "confidence_gate_passed": confidence_gate,
            "strong_margin_gate_passed": strong_margin_gate,
            "gate_used": "strong_margin" if strong_margin_gate else ("confidence" if confidence_gate else "none"),
            "compliance_frameworks_detected": frameworks,
            "framework_count": len(frameworks)
        },
        "corrective_action": {
            "type": "user_review",
            "target_phase": "1.5",
            "instruction": (
                f"Domain detected as '{selected_domain}' with confidence={confidence:.0%}, "
                f"margin={margin}, top_score={top_score}. Neither gate satisfied "
                f"(need: clear_winner+conf>=0.5 OR margin>=30+top>=60). "
                f"User should verify domain classification."
            ),
            "auto_correctable": False
        } if not passed else None
    }

finding = check_domain_confidence()
if finding:
    findings.append(finding)
```

### Step 6: Rule SVA1-EVAL-EXTRACTION (HIGH)

Verify evaluation criteria extraction: >= 3 factors, weights sum to approximately 100%.

```python
def check_eval_extraction():
    """Evaluation criteria must have >= 3 factors with weights summing to ~100%."""
    rule_def = sva1_rules.get("SVA1-EVAL-EXTRACTION")
    if not rule_def:
        return None

    factors = eval_criteria.get("factors", eval_criteria.get("evaluation_factors", []))
    factor_count = len(factors)
    has_enough_factors = factor_count >= 3

    # Compute weight sum
    weights = []
    explicit_weight_count = 0
    for f in factors:
        w = f.get("weight", f.get("points", 0))
        weights.append(w)
        if f.get("source") != "inferred" and f.get("inferred") is not True:
            explicit_weight_count += 1

    weight_sum = sum(weights)

    # Weights should sum to approximately 100 (allow 90-110 tolerance)
    weights_valid = 90 <= weight_sum <= 110 if weight_sum > 0 else False

    # At least one factor must have an explicit (non-inferred) weight
    has_explicit = explicit_weight_count >= 1

    passed = has_enough_factors and weights_valid and has_explicit

    # Score: combination of factor count, weight validity, and explicit count
    sub_scores = []
    sub_scores.append(min(factor_count / 3.0, 1.0) * 40)   # 40 pts for factor count
    sub_scores.append(40 if weights_valid else 0)            # 40 pts for weight validity
    sub_scores.append(20 if has_explicit else 0)             # 20 pts for explicit weight
    score = sum(sub_scores)

    return {
        "rule_id": "SVA1-EVAL-EXTRACTION",
        "rule_name": rule_def["name"],
        "category": rule_def["category"],
        "severity": rule_def["severity"],
        "passed": passed,
        "score": round(score, 1),
        "threshold": 100.0,
        "details": {
            "factor_count": factor_count,
            "minimum_required": 3,
            "weight_sum": round(weight_sum, 1),
            "weight_sum_valid": weights_valid,
            "explicit_weight_count": explicit_weight_count,
            "evaluation_method": eval_criteria.get("evaluation_method",
                                  eval_criteria.get("selection_method", "unknown")),
            "factors_preview": [
                {"name": f.get("name", f.get("factor")), "weight": f.get("weight", f.get("points"))}
                for f in factors[:8]
            ]
        },
        "corrective_action": {
            "type": "retry_phase",
            "target_phase": rule_def.get("corrective_phase", "1.6"),
            "instruction": f"Re-extract evaluation criteria. Current: {factor_count} factors, weight sum={weight_sum:.0f}%, explicit={explicit_weight_count}.",
            "auto_correctable": rule_def.get("auto_correctable", True)
        } if not passed else None
    }

finding = check_eval_extraction()
if finding:
    findings.append(finding)
```

### Step 7: Rule SVA1-COMPLIANCE-COVERAGE (CRITICAL)

Verify the ratio of mandatory items found in COMPLIANCE_MATRIX.json to SHALL/MUST keyword occurrences in flattened text exceeds 0.7.

```python
def check_compliance_coverage():
    """Mandatory items / SHALL+MUST distinct-clause ratio > 0.7.

    Codified 2026-05-20: implements the 2026-03-03 lessons_learned guidance that
    was captured but never wired in. Raw keyword count over-inflates the
    denominator because SHALL/MUST appear in:
      - section headers (each section title may contain "SHALL")
      - regulatory quotes from cited statutes (e.g., a clause that quotes
        ORS 279B and itself contains 5x "shall")
      - compound restatements (one obligation restated 3-4 times across a clause)
      - definition lists ("'Shall' means ...")

    The fixed denominator is DISTINCT CLAUSE-LEVEL keyword occurrences:
      - one keyword per sentence (collapse multiples within the same sentence)
      - exclude sentences that are pure header text (all-caps, < 8 words, no period)
      - exclude sentences inside a `>` blockquote (regulatory quote indicator)
      - exclude sentences in definition lists (line starts with `**X**` or `"X" means`)

    MARS 2026-05-20 raw count = 1543; clause-dedup count = ~760-850.
    """
    rule_def = sva1_rules.get("SVA1-COMPLIANCE-COVERAGE")
    if not rule_def:
        return None

    mandatory_items = compliance_matrix.get("mandatory_items", [])
    mandatory_count = len(mandatory_items)

    keyword_pattern = re.compile(r'\b(SHALL|MUST|REQUIRED|MANDATORY)\b', re.IGNORECASE)
    sentence_split = re.compile(r'(?<=[\.\?\!])\s+')
    header_check = re.compile(r'^\s*#{1,6}\s+|^[A-Z][A-Z0-9\s\-]{6,}$')
    blockquote_check = re.compile(r'^\s*>\s+')
    definition_check = re.compile(r'^\s*[\*"][^*"]{1,40}[\*"]\s+(means|refers to|is defined as)', re.IGNORECASE)

    # Clause-deduped count + raw count (for diagnostics)
    raw_keyword_count = 0
    clause_dedup_count = 0
    for f_path in flattened_files:
        content = open(f_path, 'r', encoding='utf-8').read()
        raw_keyword_count += len(keyword_pattern.findall(content))
        # Per-line scan so we can apply line-level filters before sentence-splitting
        for line in content.split("\n"):
            if not line.strip():
                continue
            if header_check.match(line):
                continue  # skip pure headers (count once via sentence split below if mixed)
            if blockquote_check.match(line):
                continue  # skip regulatory quotes
            if definition_check.match(line):
                continue  # skip definition list entries
            # Sentence-level: 1 keyword per sentence regardless of repetition
            for sent in sentence_split.split(line):
                if keyword_pattern.search(sent):
                    clause_dedup_count += 1

    # Threshold check uses CLAUSE-DEDUP count
    threshold = rule_def.get("threshold", 0.7)
    if clause_dedup_count == 0:
        ratio = 1.0 if mandatory_count == 0 else 0.0
    else:
        ratio = mandatory_count / clause_dedup_count

    passed = ratio >= threshold
    score = min(ratio / threshold, 1.0) * 100

    return {
        "rule_id": "SVA1-COMPLIANCE-COVERAGE",
        "rule_name": rule_def["name"],
        "category": rule_def["category"],
        "severity": rule_def["severity"],
        "passed": passed,
        "score": round(score, 1),
        "threshold": threshold * 100,
        "details": {
            "mandatory_items_found": mandatory_count,
            "shall_must_occurrences_raw": raw_keyword_count,
            "shall_must_occurrences_clause_dedup": clause_dedup_count,
            "denominator_used": "clause_dedup (per 2026-03-03 lessons_learned)",
            "ratio": round(ratio, 3),
            "threshold_ratio": threshold,
            "gate_status": compliance_matrix.get("gate_status", "unknown"),
            "sample_mandatory_items": [
                item.get("description", item.get("item", ""))[:100]
                for item in mandatory_items[:5]
            ]
        },
        "corrective_action": {
            "type": "retry_phase",
            "target_phase": rule_def.get("corrective_phase", "1.7"),
            "instruction": (
                f"Re-run compliance extraction. Found {mandatory_count} items vs "
                f"{clause_dedup_count} clause-deduped SHALL/MUST occurrences "
                f"(raw={raw_keyword_count}; ratio={ratio:.2f}, need >{threshold})."
            ),
            "auto_correctable": rule_def.get("auto_correctable", True)
        } if not passed else None
    }

finding = check_compliance_coverage()
if finding:
    findings.append(finding)
```

### Step 8: Rule SVA1-COMPLIANCE-CATEGORIES (MEDIUM)

Verify that mandatory items span at least 3 different categories.

```python
def check_compliance_categories():
    """Mandatory items must span >= 3 categories."""
    rule_def = sva1_rules.get("SVA1-COMPLIANCE-CATEGORIES")
    if not rule_def:
        return None

    mandatory_items = compliance_matrix.get("mandatory_items", [])
    threshold = rule_def.get("threshold", 3)

    # Extract unique categories
    categories = set()
    category_counts = {}
    for item in mandatory_items:
        cat = item.get("category", item.get("type", "UNCATEGORIZED"))
        categories.add(cat)
        category_counts[cat] = category_counts.get(cat, 0) + 1

    unique_count = len(categories)
    passed = unique_count >= threshold
    score = min(unique_count / threshold, 1.0) * 100

    return {
        "rule_id": "SVA1-COMPLIANCE-CATEGORIES",
        "rule_name": rule_def["name"],
        "category": rule_def["category"],
        "severity": rule_def["severity"],
        "passed": passed,
        "score": round(score, 1),
        "threshold": float(threshold),
        "details": {
            "unique_categories": unique_count,
            "minimum_required": threshold,
            "category_distribution": category_counts,
            "categories_found": sorted(list(categories))
        },
        "corrective_action": {
            "type": "user_review",
            "target_phase": "1.7",
            "instruction": f"Only {unique_count} compliance categories found (need {threshold}). Review if RFP truly has narrow compliance scope.",
            "auto_correctable": False
        } if not passed else None
    }

finding = check_compliance_categories()
if finding:
    findings.append(finding)
```

### Step 8.5: Rule SVA1-JURISDICTION-CONSISTENCY (CRITICAL)

Cross-validate jurisdiction across Phase 1.5, 1.6, 1.7, 1.8 outputs. Catches Phase 1.5 hallucination where a fabricated state name silently propagates into the rest of Stage 1. Codified 2026-05-20 — MARS RFP incident (Phase 1.5 wrote "Alaska" for an unambiguously Oregon RFP; downstream phases all read "Oregon" because they were forced to cite RFP sections).

```python
def check_jurisdiction_consistency():
    """Cross-validate jurisdiction across all Stage 1 outputs.

    The canonical anchor is domain-context.json::jurisdiction_anchor.state.
    Each downstream output must reference the same state name when its content
    contains a jurisdiction token (Oregon, Washington, California, etc.) or a
    state-specific statute prefix (ORS/OAR, RCW/WAC, Cal., Tex., AS, etc.)."""
    rule_def = sva1_rules.get("SVA1-JURISDICTION-CONSISTENCY")
    if not rule_def:
        return None

    # Submission structure may not be loaded above — load it now.
    submission_structure = load_json_safe(f"{folder}/shared/SUBMISSION_STRUCTURE.json")

    # 1) Read the canonical anchor from Phase 1.5
    anchor = domain_context.get("jurisdiction_anchor", {}) if isinstance(domain_context, dict) else {}
    anchor_state = (anchor.get("state") or {}).get("value")
    anchor_citation = (anchor.get("state") or {}).get("citation")

    # 2) Define a small state-prefix→state map so we can resolve statute prefixes.
    STATUTE_PREFIX_TO_STATE = {
        "ORS": "Oregon", "OAR": "Oregon",
        "RCW": "Washington", "WAC": "Washington",
        "Cal.": "California", "Cal ": "California",
        "Tex.": "Texas",
        "Fla.": "Florida",
        "N.Y.": "New York",
        "Ohio Rev": "Ohio",
        "Ill. Comp": "Illinois",
        "AS": "Alaska",
    }
    # Inverse — state to its allowed statute prefixes (used for contradiction check)
    STATE_TO_PREFIXES = {}
    for prefix, state in STATUTE_PREFIX_TO_STATE.items():
        STATE_TO_PREFIXES.setdefault(state, []).append(prefix)

    KNOWN_STATES = sorted(set(STATUTE_PREFIX_TO_STATE.values()) | {
        "Oregon", "Washington", "California", "Texas", "New York", "Florida",
        "Alaska", "Arizona", "Colorado", "Idaho", "Montana", "Nevada", "Utah",
        "Wyoming", "Hawaii", "Massachusetts", "Connecticut", "Maine", "Rhode Island",
        "Vermont", "New Hampshire", "New Jersey", "Pennsylvania", "Ohio", "Michigan",
        "Indiana", "Illinois", "Wisconsin", "Minnesota", "Iowa", "Missouri", "Kansas",
        "Nebraska", "North Dakota", "South Dakota", "Oklahoma", "Arkansas",
        "Louisiana", "Mississippi", "Alabama", "Georgia", "South Carolina",
        "North Carolina", "Tennessee", "Kentucky", "West Virginia", "Virginia",
        "Maryland", "Delaware", "New Mexico"
    }, key=len, reverse=True)

    def scan_doc_for_state(doc, source_label):
        """Return (state_name_or_None, evidence_snippet, prefix_seen) for the first
        state token or statute prefix found in the document's serialized form."""
        if not doc:
            return (None, None, None)
        # Stringify the doc so we can pattern-match against any text field.
        # ensure_ascii=False so accented chars round-trip (defensive)
        text = json.dumps(doc, ensure_ascii=False)
        # Prefer statute prefixes (very specific) before generic "State of X"
        for prefix, state in STATUTE_PREFIX_TO_STATE.items():
            if re.search(rf"\b{re.escape(prefix)}\s*[\d§\.]", text):
                return (state, f"statute prefix '{prefix}' in {source_label}", prefix)
        # Then look for "State of X" phrasing
        m = re.search(rf"State of ({'|'.join(re.escape(s) for s in KNOWN_STATES)})", text)
        if m:
            return (m.group(1), f"'State of {m.group(1)}' in {source_label}", None)
        # Last fallback — bare state name
        for state in KNOWN_STATES:
            if re.search(rf"\b{re.escape(state)}\b", text):
                return (state, f"'{state}' token in {source_label}", None)
        return (None, None, None)

    eval_state, eval_evidence, _ = scan_doc_for_state(eval_criteria, "EVALUATION_CRITERIA.json")
    comp_state, comp_evidence, _ = scan_doc_for_state(compliance_matrix, "COMPLIANCE_MATRIX.json")
    sub_state, sub_evidence, _ = scan_doc_for_state(submission_structure, "SUBMISSION_STRUCTURE.json")

    observed = {
        "phase_1.5_anchor": anchor_state,
        "phase_1.6_eval": eval_state,
        "phase_1.7_compliance": comp_state,
        "phase_1.8_submission": sub_state,
    }
    evidence = {
        "phase_1.5_anchor": anchor_citation,
        "phase_1.6_eval": eval_evidence,
        "phase_1.7_compliance": comp_evidence,
        "phase_1.8_submission": sub_evidence,
    }

    # Determine the "majority" state from phases 1.6/1.7/1.8 (they read RFP with citations
    # — more trustworthy than 1.5 which historically hallucinates when domain is unclear).
    downstream = [s for s in (eval_state, comp_state, sub_state) if s]
    majority = None
    if downstream:
        from collections import Counter
        c = Counter(downstream)
        majority, _count = c.most_common(1)[0]

    # Disagreements: every non-null observation must equal anchor (if anchor set) AND majority.
    non_null = {k: v for k, v in observed.items() if v}
    distinct = set(non_null.values())
    consistent = (len(distinct) <= 1)

    # Anchor vs majority — separate check because anchor hallucination is the main risk.
    anchor_disagrees_with_majority = (
        anchor_state is not None
        and majority is not None
        and anchor_state != majority
    )

    passed = consistent and not anchor_disagrees_with_majority
    # Score: 100 if all agree, 50 if anchor disagrees with majority,
    # 0 if downstream phases themselves disagree.
    if not non_null:
        score = 0.0
    elif anchor_disagrees_with_majority:
        score = 0.0
    elif not consistent:
        score = 25.0
    else:
        score = 100.0

    instruction = None
    if anchor_disagrees_with_majority:
        instruction = (
            f"Phase 1.5 jurisdiction_anchor.state='{anchor_state}' but phases 1.6/1.7/1.8 "
            f"consistently observed '{majority}'. Re-run Phase 1.5 with corrective guidance: "
            f"the RFP is for the State of {majority} — confirm via grep against flattened/* "
            f"and rewrite jurisdiction_anchor with grep-cited evidence. Do NOT add ad-hoc "
            f"*_domain_hints blocks (forbidden by Rule J3 in phase1.5-domain-win.md)."
        )
    elif not consistent and downstream:
        instruction = (
            f"Stage 1 outputs disagree on jurisdiction: {observed}. Re-read the RFP and "
            f"establish the canonical state in Phase 1.5 first, then re-run downstream "
            f"phases as needed."
        )

    return {
        "rule_id": "SVA1-JURISDICTION-CONSISTENCY",
        "rule_name": rule_def["name"],
        "category": rule_def["category"],
        "severity": rule_def["severity"],
        "passed": passed,
        "score": score,
        "threshold": 100.0,
        "details": {
            "observed_by_phase": observed,
            "evidence_by_phase": evidence,
            "majority_downstream_state": majority,
            "anchor_disagrees_with_majority": anchor_disagrees_with_majority,
            "downstream_consistent": consistent
        },
        "corrective_action": ({
            "type": "retry_phase",
            "target_phase": rule_def.get("corrective_phase", "1.5"),
            "instruction": instruction,
            "auto_correctable": rule_def.get("auto_correctable", True)
        } if not passed else None)
    }

finding = check_jurisdiction_consistency()
if finding:
    findings.append(finding)
```

### Step 9: Compute Overall Disposition and Score

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

### Step 10: Build Corrective Actions Summary

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

### Step 11: Assemble and Write Report

```python
end_time = datetime.now()

report = {
    "validator": "SVA-1",
    "stage_validated": 1,
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
        "files_analyzed": len(flattened_files) + len(original_files),
        "data_sources_read": data_sources_read
    }
}

# Ensure validation directory exists
Path(f"{folder}/shared/validation").mkdir(parents=True, exist_ok=True)

# Write report (UTF-8 + ensure_ascii=False — see skill-win.md helpers).
output_path = f"{folder}/shared/validation/sva1-intake.json"
with open(output_path, 'w', encoding='utf-8', newline='\n') as f:
    json.dump(report, f, indent=2, ensure_ascii=False)
```

### Step 12: Report Results

```python
# Disposition emoji and color
disp_icon = {
    "PASS": "✅",
    "ADVISORY": "⚠️",
    "BLOCK": "⛔"
}

log(f"""
{'='*60}
{disp_icon.get(disposition, '?')} SVA-1: Intake Validator - {disposition}
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

- [ ] `sva1-intake.json` written to `shared/validation/`
- [ ] All 6 rules from registry evaluated
- [ ] Disposition correctly computed (BLOCK if any CRITICAL fails)
- [ ] Each finding includes score, threshold, details, and corrective_action
- [ ] Corrective actions sorted by severity
- [ ] Report conforms to `sva-report.schema.json`
- [ ] Execution metadata recorded (duration, files analyzed, sources read)
