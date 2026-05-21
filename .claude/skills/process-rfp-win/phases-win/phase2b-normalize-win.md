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

# V3-F4 fix 2026-05-20: dedup threshold lowered AND prefix-normalization added.
# Prior threshold 0.85 + no prefix-stripping let three near-identical variants
# survive ("X", "be able to X", "Administrator shall be able to X"). Result was
# 11.5% dedup rate on a catalog that should have had 40%+ duplicate compression.
# Now: strip common procurement-language prefixes before similarity comparison,
# AND lower threshold to 0.78 to catch genuine variants without over-merging.
PROC_PREFIX_PATTERNS = [
    re.compile(r'^\s*(?:the\s+)?(?:system|application|solution|platform|vendor|contractor|proposer|administrator|user)\s+(?:shall|must|will|should)\s+(?:be\s+able\s+to\s+)?', re.IGNORECASE),
    re.compile(r'^\s*(?:shall|must|will)\s+(?:be\s+able\s+to\s+)?', re.IGNORECASE),
    re.compile(r'^\s*be\s+able\s+to\s+', re.IGNORECASE),
    re.compile(r'^\s*(?:allow|enable|permit)\s+(?:users?|administrators?)\s+to\s+', re.IGNORECASE),
]

def normalize_for_dedup(text):
    """Strip procurement-language prefixes and collapse whitespace for similarity comparison."""
    t = text.lower().strip()
    # Strip each known prefix once (longest-first ordering)
    for pat in PROC_PREFIX_PATTERNS:
        t = pat.sub('', t, count=1)
    # Collapse whitespace
    t = re.sub(r'\s+', ' ', t).strip()
    return t

def similarity(a, b):
    """Calculate text similarity ratio (computed on prefix-normalized forms)."""
    return SequenceMatcher(None, normalize_for_dedup(a), normalize_for_dedup(b)).ratio()

def domain_tokens(text):
    """Return the set of domain-distinguishing tokens present in text.
    V3-F11 fix 2026-05-20: parallel requirements that differ ONLY in domain
    object (entity vs firm, role vs permission, etc.) are NOT duplicates,
    even if the rest of the sentence is identical. The dedup ratio on
    "manage how often entities of a certain type appear" vs "manage how often
    certain firms appear" exceeds 0.78, so without this protection the firm
    variant is dropped, leaving a functional scope gap. Returns a set so
    we can compare presence as XOR.
    """
    tl = text.lower()
    found = set()
    # Each pair contains tokens that MUST be distinct between requirements
    domain_axes = [
        ("entity", "firm"),
        ("entities", "firms"),
        ("user", "administrator"),
        ("internal", "external"),
        ("read-only", "read-write"),
        ("approve", "reject"),
        ("draft", "submitted"),
        ("inbound", "outbound"),
        ("primary", "secondary"),
    ]
    for axis in domain_axes:
        for token in axis:
            if re.search(r'\b' + re.escape(token) + r'\b', tl):
                found.add(token)
    return found

def deduplicate_requirements(requirements, threshold=0.78):
    """Remove duplicate requirements based on prefix-normalized text similarity."""
    unique = []
    merged_count = 0

    for req in requirements:
        is_duplicate = False
        req_norm = normalize_for_dedup(req["text"])
        req_tokens = domain_tokens(req["text"])
        # Fast-path: exact match on normalized form
        for existing in unique:
            existing_norm = normalize_for_dedup(existing["text"])
            existing_tokens = domain_tokens(existing["text"])
            # V3-F11: domain-axis protection — if both items contain a different
            # token from the same axis (e.g., one has "entity", the other "firm"),
            # they are NOT duplicates regardless of body similarity.
            domain_conflict = False
            for axis in [("entity", "firm"), ("entities", "firms"),
                         ("user", "administrator"), ("internal", "external"),
                         ("approve", "reject"), ("draft", "submitted"),
                         ("inbound", "outbound"), ("primary", "secondary")]:
                a_in_req = axis[0] in req_tokens
                b_in_req = axis[1] in req_tokens
                a_in_ex = axis[0] in existing_tokens
                b_in_ex = axis[1] in existing_tokens
                # XOR: req has one side, existing has the other
                if (a_in_req and b_in_ex and not a_in_ex and not b_in_req) or \
                   (b_in_req and a_in_ex and not b_in_ex and not a_in_req):
                    domain_conflict = True
                    break
            if domain_conflict:
                continue  # don't even check similarity
            if req_norm == existing_norm:
                is_duplicate = True
            elif req_norm and existing_norm and SequenceMatcher(None, req_norm, existing_norm).ratio() > threshold:
                is_duplicate = True
            if is_duplicate:
                merged_count += 1
                if req.get("source") != existing.get("source"):
                    if "merged_sources" not in existing:
                        existing["merged_sources"] = [existing.get("source")]
                    existing["merged_sources"].append(req.get("source"))
                # V2-F1 fix 2026-05-18: also merge source_ids so RTM traceability
                # survives dedup. Without this, requirements that appeared in
                # multiple flattened docs lost all but one source citation.
                incoming_ids = req.get("source_ids", [])
                if incoming_ids:
                    existing.setdefault("source_ids", []).extend(
                        sid for sid in incoming_ids if sid not in existing.get("source_ids", [])
                    )
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

    # Check minimum length (V3-F6 fix 2026-05-20: stricter thresholds)
    # Fragments under 30 chars cannot be meaningful procurement requirements.
    # Examples we previously passed at score 85: "continuously monitor" (20),
    # "generate secure, tamper-" (24), "be able to generate a PDF version" (33).
    # The first two should be INVALID; the third should be flagged for review.
    if len(text) < 20:
        issues.append("Too short (< 20 chars) -- invalid fragment")
        score -= 40
    elif len(text) < 30:
        issues.append("Very short (< 30 chars) -- likely truncated")
        score -= 25

    # V3-F10 fix 2026-05-20: question-terminated text is a checklist question,
    # NOT a system requirement. The MARS RFP contains proposer self-evaluation
    # prompts like "notify users of various actions?" which are captured by the
    # extraction patterns but should be rejected here (score forces invalid).
    if text.rstrip().endswith("?"):
        issues.append("Ends with '?' -- checklist prompt, not a requirement")
        score -= 50

    # Fragment-end detection: text ending in a hyphen, comma, or open
    # subordinator strongly suggests mid-sentence truncation.
    text_stripped = text.rstrip()
    if text_stripped:
        last_char = text_stripped[-1]
        if last_char in ('-', ','):
            issues.append("Ends mid-sentence (hyphen/comma terminator) -- truncated extraction")
            score -= 20
        # Trailing subordinator words
        last_word = text_stripped.split()[-1].lower() if text_stripped.split() else ""
        if last_word in {"the", "a", "an", "or", "and", "to", "of", "for", "with", "by",
                         "from", "in", "on", "at", "as", "if", "that", "which", "such",
                         "including", "associating", "those"}:
            issues.append(f"Ends with subordinator '{last_word}' -- truncated extraction")
            score -= 20

    # Check for testable verb.
    # V2-F4 fix 2026-05-18: split into mandatory and advisory. Conflating SHALL
    # and SHOULD lets advisory-only requirements pass validation, then they
    # bubble up to CRITICAL/HIGH priority and inflate the compliance matrix.
    # Now: advisory requirements pass validation but are explicitly flagged
    # and reduced in score so reviewers see them.
    text_lower = text.lower()
    mandatory_verbs = ["shall", "must", "will"]
    advisory_verbs = ["should"]
    has_mandatory = any(verb in text_lower for verb in mandatory_verbs)
    has_advisory = any(verb in text_lower for verb in advisory_verbs)
    if not has_mandatory and not has_advisory:
        issues.append("Missing testable verb (shall/must/will/should)")
        score -= 15
    elif has_advisory and not has_mandatory:
        issues.append("Advisory requirement — verify priority cap")
        score -= 10

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
            # SCHEMA NOTE (HUNT-A-0003 doc fix 2026-05-18): the field is `text_preview`,
            # NOT `text`. Display-only preview for human-readable ambiguity list. Full
            # canonical text is preserved on the source requirement object in the
            # parent `requirements[]` array (look up by `id` if needed). The
            # `_preview` suffix is intentional and the value is intentionally truncated
            # to ~100 chars to keep the review list scannable. Do not rename without
            # auditing every consumer of `requirements-normalized.json.ambiguous_requirements[]`.
            "text_preview": req.get("text", "")[:100] + ("..." if len(req.get("text", "")) > 100 else ""),
            "ambiguity_score": req["ambiguity_analysis"]["ambiguity_score"],
            "review_priority": req["ambiguity_analysis"]["review_priority"],
            "issues": [a["category"] for a in req["ambiguity_analysis"]["ambiguities"]]
        })

log(f"Flagged {len(ambiguous_requirements)} requirements for ambiguity review")
```

### Step 5: Assign Canonical IDs

**⛔ CATEGORY → 3-CHAR CODE MAPPING (codified 2026-05-20 — MARS Phase 2b incident):**

Phase 2 (extract) emits ~29 raw category values, some are legacy 3-char codes (TEC, APP, RPT, BUD, STF, SEC, INT, ADM) and some are full-word categories (FILING, USER_MANAGEMENT, DASHBOARD, REVIEW, etc.). The canonical_id format is `{NNNCAT}` where `CAT` MUST be a 3-char uppercase code; passing the full word through produces malformed IDs like `001USER_MANAGEMENT` that fail verifier Check 9 and break downstream RTM linkage.

The MARS retry-1 run produced 410/1,116 (36.7%) malformed IDs because `assign_canonical_ids()` used the raw category verbatim. Codified fix: a deterministic CATEGORY_TO_CODE table with a logged-warning fallback to "TEC" for unmapped values.

```python
# Codified 2026-05-20 — full-word category → 3-char code mapping.
# When adding new categories upstream in phase2-extract, add their 3-char code here.
CATEGORY_TO_CODE = {
    # 3-char codes pass through unchanged (legacy categories from phase2-extract)
    "TEC": "TEC", "APP": "APP", "RPT": "RPT", "BUD": "BUD",
    "STF": "STF", "SEC": "SEC", "INT": "INT", "ADM": "ADM",
    # Full-word categories from MARS round-2 raw distribution
    "FILING": "FIL", "DASHBOARD": "DSH", "CONFIGURATION": "CFG",
    "REVIEW": "REV", "UI": "UIX", "REQUEST": "REQ",
    "USER_MANAGEMENT": "USR", "MANAGEMENT": "MGT", "NOTIFICATION": "NTF",
    "EXTERNAL_OPS": "EXT", "PAYMENT": "PAY", "REPORTING": "RPT",
    "INTERNAL_OPS": "INO", "RECORDS_MANAGEMENT": "REC",
    "DOCUMENT_GENERATION": "DOC", "DATA_UPLOAD": "UPL",
    "AUDIT": "AUD", "MIGRATION": "MIG", "SEARCH": "SCH",
    "AUTOMATION": "AUT", "INTEGRATION": "ITG",
}
DEFAULT_CODE = "TEC"

def assign_canonical_ids(requirements, domain_context):
    """Assign canonical IDs in {NNNCAT} format with deterministic 3-char code mapping."""
    category_counters = {}
    unmapped_seen = set()

    for req in requirements:
        raw_cat = req.get("category", DEFAULT_CODE)
        code = CATEGORY_TO_CODE.get(raw_cat, DEFAULT_CODE)
        if code == DEFAULT_CODE and raw_cat not in CATEGORY_TO_CODE:
            if raw_cat not in unmapped_seen:
                log(f"  ⚠️  Unmapped category {raw_cat!r} → defaulted to {DEFAULT_CODE}; "
                    f"add to CATEGORY_TO_CODE in phase2b-normalize-win.md")
                unmapped_seen.add(raw_cat)

        if code not in category_counters:
            category_counters[code] = 1
        else:
            category_counters[code] += 1

        # Format: 001APP, 002USR, 003FIL — always 3-char suffix
        canonical_id = f"{category_counters[code]:03d}{code}"
        req["canonical_id"] = canonical_id
        req["display_id"] = f"[{canonical_id}]"
        req["category_code"] = code   # preserved for downstream consumers
        req["category_raw"] = raw_cat # original Phase 2 category preserved for audit

    return requirements

unique_requirements = assign_canonical_ids(unique_requirements, domain_context)
```

Verifier Check 9 enforces `^\d{3,}[A-Z]{3}$` — every canonical_id must have exactly 3 trailing uppercase letters. If a new category is added upstream and not mapped, the warning fires AND the ID falls back to `TEC` so downstream isn't broken; the warning is the signal to update the table.

### Step 6: Assign Priorities

```python
def assign_priority(req):
    """Assign priority based on requirement characteristics.

    V2-F4 fix 2026-05-18: SHOULD-only requirements (advisory) MUST cap at MEDIUM.
    Previously, "should" was conflated with SHALL keywords, sending advisory items
    to CRITICAL/HIGH and inflating the mandatory compliance matrix.

    V3-F5 fix 2026-05-20: priority keyword check restricted to the REQUIREMENT TEXT
    (req['text']) only. Prior implementation incidentally checked surrounding
    context, which flagged routine requirements as CRITICAL when an adjacent
    sentence happened to mention "security" or "required". Also: fragments under
    30 chars cannot be reliably classified — cap them at MEDIUM until the
    extraction issue is fixed upstream.
    """
    text_lower = req["text"].lower()
    text_clean = text_lower.strip()

    # Length-gate: very short fragments cannot be classified reliably
    if len(text_clean) < 30:
        return "MEDIUM"

    # Advisory check FIRST — SHOULD without SHALL/MUST/required/mandatory caps at MEDIUM
    is_advisory_only = (
        "should" in text_lower
        and not any(m in text_lower for m in ["shall", "must", "required", "mandatory"])
    )
    if is_advisory_only:
        return "MEDIUM"  # Ceiling for advisory requirements

    # CRITICAL only when the requirement itself contains a strong critical signal.
    # "must" alone is too common (every "system must" is critical?) -> require
    # multi-signal: a critical keyword AND a strong action/domain marker.
    #
    # ⛔ TWO-SIGNAL GATE (codified 2026-05-20 — MARS Phase 2b incident):
    # The simple "any-keyword-matches" rule over-flagged 7.0% of requirements as
    # CRITICAL in the MARS run because single bland keywords (e.g., "compliance",
    # "audit trail", "encryption") appear in routine requirements without true
    # critical character. The verifier's CRITICAL <5% target was breached.
    #
    # Tightened logic: CRITICAL fires only when EITHER (a) the text contains a
    # STRONG single signal AND no ambiguating qualifier, OR (b) a PROHIBITION
    # ("shall not"/"must not"/"prohibited") PAIRED with a domain-critical context
    # token (access/transmit/disclos/credential/audit/encrypt/regulatory/pii/etc.).
    # Tightened MARS run from 7.0% → 1.44% CRITICAL — within the verifier's <5% band.
    STRONG_SINGLE_CRITICAL = {
        "security breach", "data loss", "tamper-proof", "tamper proof",
        "non-compliance", "mandatory compliance"
    }
    PROHIBITION_TOKENS = {"shall not", "must not", "prohibited"}
    CRITICAL_CONTEXT_TOKENS = {
        "access", "transmit", "disclos", "credential", "audit", "encrypt",
        "regulatory", "pii", "phi", "ssn", "password", "private key",
        "personally identifiable", "sensitive data"
    }

    has_strong_single = any(term in text_lower for term in STRONG_SINGLE_CRITICAL)
    has_prohibition = any(term in text_lower for term in PROHIBITION_TOKENS)
    has_critical_context = any(term in text_lower for term in CRITICAL_CONTEXT_TOKENS)

    if has_strong_single or (has_prohibition and has_critical_context):
        return "CRITICAL"

    # HIGH band requires more than just "shall" — that token appears in virtually
    # every system requirement, so a bare "shall" gate dumps 87%+ into HIGH
    # (the MARS 2026-05-20 Pink-Team finding: priority signal collapsed because
    # 87.7% landed in HIGH). Tightened logic: HIGH requires "shall"/"must" AND
    # a foundational-capability anchor token (functional domain that drives the
    # bid outcome).
    HIGH_ANCHOR_TOKENS = {
        # Core functional categories that drive evaluation factors
        "submit", "approve", "reject", "process payment", "authenticate", "authoriz",
        "calculate", "validate", "verify", "report", "generate report",
        "publish", "expose", "integrate", "synchroniz", "import", "export",
        "migrate", "convert", "load",
        # Data integrity / persistence
        "store", "persist", "retain", "purge", "back up", "backup", "restore",
        "version", "history", "audit log",
        # Workflow drivers
        "notify", "alert", "escalate", "route", "assign", "track", "monitor",
        # Compliance-adjacent (without triggering the CRITICAL critical-context)
        "comply with", "conform to", "adhere to", "meet the requirement",
        # Performance / SLA
        "respond within", "process within", "complete within", "uptime", "availability",
    }
    text_words = set(text_lower.split())
    has_mandate = any(t in text_lower for t in ("shall", "must", "required", "mandatory"))
    has_high_anchor = any(t in text_lower for t in HIGH_ANCHOR_TOKENS)

    # Codification 2026-05-20 (Pink-Team PINK-RES-02 + PINK-RES-03 — for next run):
    # Single-anchor HIGH gate still over-populates HIGH (44.8% of MARS items).
    # Tighten by requiring ≥2 distinct HIGH_ANCHOR_TOKENS, OR a HIGH_ANCHOR_TOKEN
    # PLUS explicit category-code in {SEC, INT, RPT, FIL} (the foundational
    # capability families). Plus ADD explicit CRITICAL anchors so security/SLA
    # items don't get buried in HIGH:
    #   STRONG_SINGLE_CRITICAL should also include: encrypt, multi-factor,
    #   MFA, SOC 2, FedRAMP, CJIS, IRS 1075, uptime, RTO, RPO, no overseas,
    #   FBI NCIC, 24-hour breach notification.
    # AND: phase2-extract-win.md classify_requirement_type() KEYWORD_MAP should
    # add common functional verbs to reduce UNCLASSIFIED bucket: populate,
    # determine, allow, display, send, generate, track, capture, validate,
    # enforce, calculate, route, notify.
    # These refinements are queued for next pipeline run — current MARS run
    # uses the existing single-anchor gate (PASS at SVA-2 with 1 MEDIUM rule fail).
    if has_mandate and has_high_anchor:
        return "HIGH"

    # Low indicators
    low_keywords = ["optional", "nice to have", "could", "may", "consider"]
    if any(term in text_lower for term in low_keywords):
        return "LOW"

    # MEDIUM band: mandate language WITHOUT a foundational-capability anchor.
    # Typical pattern: "the system shall display X", "the user shall be able
    # to view Y" — important to honor but not bid-fate-determining like
    # "the system shall calculate the total of all Actual column values".
    # This is the residual bucket that was over-collapsing into HIGH.
    if has_mandate:
        return "MEDIUM"

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

## Quality Checklist (MANDATORY — report each by name with evidence)

The phase agent MUST verify each of the following BEFORE reporting completion. The agent's completion report MUST include a checklist-results block with:
- Item name (verbatim from below)
- PASS / FAIL / SKIPPED-WITH-REASON
- Evidence (file:line citation, grep result, file size, assertion that ran, etc.)

"All checks passed" without per-item evidence is NOT acceptable.

### Required output files
1. **requirements-normalized.json** exists at `{folder}/shared/requirements-normalized.json` — evidence: `ls -la` size > 1,024 bytes and parses as valid JSON

### Schema fidelity
2. **requirements-normalized.json top-level keys** include `normalized_at`, `summary`, `requirements`, `invalid_requirements`, `ambiguous_requirements` — evidence: list actual top-level keys found
3. **Every requirement in `requirements[]`** has `canonical_id`, `priority`, `validation`, `ambiguity_analysis` — evidence: print key set of requirements[0]
4. No `[:N]` slicing applied to deliverable content strings — evidence: grep for `\[:[0-9]+\]` in production code paths returned 0 hits

### Cross-stage consistency
5. **Every normalized requirement preserves `source_ids[]` from its raw input(s)** — dedup merges must union source_ids from all merged duplicates — evidence: count requirements with empty source_ids (must be 0 for requirements from pattern_extraction)
6. **Dedup reduction ratio between 10% and 65%** (sanity bounds — V3 widened 2026-05-20: broader extraction patterns produce higher overlap, 50-60% legitimate) — if reduction < 10%, dedup threshold too strict; if > 65%, likely over-aggressive — evidence: print `summary.deduplication_rate` and flag if outside bounds.
7. **SHOULD-only requirements capped at MEDIUM priority** — no requirement whose text contains "should" (without "shall"/"must") has priority CRITICAL or HIGH — evidence: grep `requirements[]` for advisory-only items with CRITICAL/HIGH priority, count must be 0
8. **source_ids preserved through dedup** — when two requirements are merged, the surviving entry has source_ids from both merged items — evidence: spot-check one merged requirement to confirm source_ids length >= 2

### V3 normalization-quality gates (added 2026-05-20)
9. **Prefix-normalization applied before similarity comparison** — evidence: confirm `normalize_for_dedup()` is called on BOTH sides of every similarity check; without prefix-strip, "X" vs "be able to X" vs "Administrator shall be able to X" survive as 3 distinct entries.
10. **Domain-axis dedup protection** — entity/firm, user/administrator, internal/external, etc. parallel requirements MUST survive dedup separately. Evidence: count requirements containing `\bentity\b` and `\bfirm\b` in `requirements[]`; both counts must be > 0 if both terms appear in `flattened/`. The previously-confirmed gap (firm-desk-review variants dropped) must not recur.
11. **No checklist questions in valid catalog** — evidence: count requirements whose text ends with `?` in the `requirements[]` (not `invalid_requirements[]`); must be 0.
12. **No fragment-end requirements** — evidence: count requirements whose text ends with hyphen, comma, or subordinator (the/a/or/and/to/etc.); must be 0 in `requirements[]`. Such items belong in `invalid_requirements[]` only.
13. **Priority assignment uses req text only, NOT full_context** — evidence: spot-check 5 CRITICAL items, confirm critical-keyword (`mandatory`, `compliance with`, `security breach`, etc.) appears in `text` field, not just `full_context`.
14. **CRITICAL count <5% of total** (sanity: most requirements are HIGH, not CRITICAL) — evidence: print CRITICAL count vs total; if CRITICAL > 5%, priority algorithm is over-flagging.
15. **Length-gate validation** — any text <30 chars must either be in `invalid_requirements[]` OR carry an explicit `validation.issues` entry flagging it as "very short". Evidence: count requirements with `len(text) < 30` in `requirements[]`; if > 0, each must have the flag.

### Anti-regression rules (universal)
16. **UTF-8 encoding** on every `open()` call — evidence: search this phase's emitted scripts/code for `encoding='utf-8'` in every file-open
17. **ensure_ascii=False** on every `json.dump` call — evidence: same grep
18. **No `_Showing N of M_` row-cap notices** in any deliverable markdown — evidence: grep returned 0 matches
19. **No empty `|  |` mitigation/cell patterns** in any deliverable table — evidence: grep returned 0 matches
20. **No mid-word table-cell truncations** — evidence: line-by-line cell-end check returned 0 hits

### Memory discipline
21. **Relevant SAFS memory entries reviewed and applied** — evidence: list which memory files were read and which rules were applicable. MUST include:
    - `feedback_codify_in_skills.md` — every fix in the phase file + checklist, not single-shot
    - `feedback_requirements_extraction_quality.md` — V3 normalization discipline (prefix-normalization, domain-axis, question-mark filter)
    - schema note: `ambiguous_requirements[].text_preview` is a display-only field — not renamed
