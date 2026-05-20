---
name: phase1.7-compliance-win
expert-role: Compliance Officer
domain-expertise: Regulatory requirements, mandatory items, legal
skill: procurement-analyst
sub-skill: compliance-audit
---

# Phase 1.7: Compliance Gatekeeper (BLOCKING GATE)

## Purpose

Extract ALL mandatory requirements and validate 100% coverage. This is a **BLOCKING GATE** - the pipeline cannot proceed until all mandatory items are addressed.

## ⛔ MOJIBAKE SCRUB DISCIPLINE (codified 2026-05-20 — MARS incident)

The MARS run on 2026-05-20 produced a COMPLIANCE_MATRIX.json with **1,789 U+FFFD replacement characters** across 776 `mandatory_items[*].text` locations because Phase 0 flattened Attachment A with a lossy decoder. Phase 1.7 extracted faithfully but propagated the upstream corruption.

**Two mandatory protections:**

1. **Every text field extracted from flattened/* MUST pass through `scrub_mojibake(text, source_hint=...)`** (defined in `skill-win.md`, MOJIBAKE SCRUB section). Use the repaired string in the output, not the raw input. This applies to: `mandatory_items[].text`, `submission_required_attachments[].name`, `non_negotiable_items[].description` and `.action_required`, `rtm_entities.mandatory_items[].text`, AND any other text field extracted from flattened sources.

2. **Add a `pipeline_metadata.encoding_audit` block to the output JSON** with this exact schema:
   ```json
   {
     "pipeline_metadata": {
       "encoding_audit": {
         "scrubbed_total_chars": <int>,
         "items_with_scrubs": <int>,
         "items_with_unrepairable_residual": [
           {"path": "mandatory_items[27].text", "unrepairable_count": 1, "indices": [42]}
         ],
         "residual_total": <int>
       }
     }
   }
   ```
   Every unrepairable U+FFFD (or `?` mid-word) MUST be logged with its dotted field path and the character index inside that field. The verifier uses this audit to distinguish upstream PDF data-loss (acceptable) from scrub-heuristic misses (FAIL).

**Why the audit-not-block approach:** Some U+FFFD residuals reflect PDF glyph storage that has no recoverable source character (e.g., unmapped CIDs for bullet markers). Refusing to write the matrix on any residual would deadlock the pipeline on every real-world PDF. The audit gives the verifier the data it needs to PASS legitimate upstream-loss cases and FAIL avoidable misses.

**Defense-in-source:** Phase 0 flatten (`phase1-flatten-win.md`) is the proper fix point — it must use `errors='strict'` on every decode so encoding bugs fail loudly upstream. The Phase 1.7 scrub is defense-in-depth, not a substitute.

## ⛔ RETRY_HISTORY DISCIPLINE (codified 2026-05-20 — duplicate-append bug)

Each retry of Phase 1.7 MUST write **exactly one** record to `pipeline_metadata.retry_history[]` per retry, with this **canonical schema** (no field-name variants):

```json
{
  "retry_number": 2,
  "timestamp": "2026-05-20T20:15:41Z",
  "reason": "User-authorized RFPAttH atomic extraction",
  "fix_applied": "Re-walked flattened/RFPAttH... with sentence-level reflow + atomic SHALL clause extractor",
  "authorized_by": "user_override_via_AskUserQuestion",
  "codification_summary": "Codified atomic-vs-grouped extraction policy; widened verifier SRC-id regex"
}
```

**Idempotency contract:** before appending, check whether `pipeline_metadata.retry_history[]` already contains a record with the same `retry_number`. If yes, **replace-in-place** (overwrite that record); if no, append. Never append a 2nd record for the same retry_number.

**Why this discipline:** the MARS retry-2 (2026-05-20) revealed that prior retry-1 wrote `retry_at` (different field name) and the retry-2 producer wrote `timestamp` plus added/missing fields per pass, producing 4 entries when 2 were expected — a duplicate-append bug compounded with schema drift. Future readers (auditors, the SVA, future retry agents) need a clean ledger to determine "did this phase already retry for X reason."

```python
def append_retry_record(metadata, record):
    """Idempotent retry ledger writer. Replace-in-place by retry_number."""
    history = metadata.setdefault("retry_history", [])
    rnum = record["retry_number"]
    for i, existing in enumerate(history):
        if existing.get("retry_number") == rnum:
            history[i] = record   # overwrite
            return
    history.append(record)
```

## Inputs

- `{folder}/flattened/*.md` - Flattened RFP documents
- `{folder}/shared/domain-context.json` - Domain context with compliance terms

## Required Outputs

- `{folder}/shared/COMPLIANCE_MATRIX.json` - Mandatory items with coverage status

## BLOCKING GATE

**This phase MUST PASS before proceeding to Phase 2.**

Gate conditions:
- All mandatory items identified
- Each mandatory item has a planned response strategy
- No unaddressed "shall" or "must" requirements
- Format compliance verified (page limits, fonts, etc.)

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

### Step 2: Extract Mandatory Items

```python
MANDATORY_PATTERNS = [
    (r"shall\s+(?:be\s+)?(?:required\s+to\s+)?(.{20,200})", "SHALL"),
    (r"must\s+(?:be\s+)?(?:required\s+to\s+)?(.{20,200})", "MUST"),
    (r"required\s+to\s+(.{20,200})", "REQUIRED"),
    (r"mandatory[:\s]+(.{20,200})", "MANDATORY"),
    (r"offeror\s+shall\s+(.{20,200})", "OFFEROR_SHALL"),
    (r"contractor\s+shall\s+(.{20,200})", "CONTRACTOR_SHALL"),
    (r"vendor\s+shall\s+(.{20,200})", "VENDOR_SHALL"),
    (r"proposal\s+must\s+(?:include|contain|address)\s+(.{20,200})", "PROPOSAL_MUST")
]

mandatory_items = []
seen_text = set()

for pattern, source_type in MANDATORY_PATTERNS:
    matches = re.finditer(pattern, combined_content, re.IGNORECASE | re.DOTALL)
    for match in matches:
        text = match.group(1).strip()
        text = re.sub(r'\s+', ' ', text)  # Normalize whitespace

        # Deduplicate by similarity
        text_normalized = text.lower()[:50]
        if text_normalized not in seen_text:
            seen_text.add(text_normalized)
            mandatory_items.append({
                "id": f"M{len(mandatory_items)+1:03d}",
                "text": text[:300],  # Truncate long items
                "source_type": source_type,
                "full_context": match.group(0)[:500],
                "position": match.start()
            })
```

### Step 3: Categorize Mandatory Items

```python
CATEGORIES = {
    "TECHNICAL": ["system", "software", "hardware", "integration", "interface", "database", "security"],
    "PROCESS": ["workflow", "process", "procedure", "methodology", "approach"],
    "PERSONNEL": ["staff", "personnel", "team", "qualifications", "certification", "experience"],
    "COMPLIANCE": ["comply", "compliance", "regulation", "standard", "audit", "ferpa", "hipaa"],
    "DELIVERY": ["deliver", "milestone", "schedule", "timeline", "deadline", "due date"],
    "FORMAT": ["page", "font", "format", "submit", "submission", "copies", "electronic"]
}

def categorize_item(item):
    text_lower = item["text"].lower()
    for category, keywords in CATEGORIES.items():
        if any(kw in text_lower for kw in keywords):
            return category
    return "OTHER"

for item in mandatory_items:
    item["category"] = categorize_item(item)
```

### Step 4: Extract Format Requirements

```python
FORMAT_PATTERNS = [
    (r"page\s+limit[:\s]+(\d+)\s*pages?", "page_limit"),
    (r"maximum\s+(\d+)\s*pages?", "page_limit"),
    (r"not\s+(?:to\s+)?exceed\s+(\d+)\s*pages?", "page_limit"),
    (r"font\s+size[:\s]+(\d+)\s*(?:pt|point)?", "font_size"),
    (r"minimum\s+(\d+)\s*(?:pt|point)\s+font", "font_size"),
    (r"(\d+)\s*(?:pt|point)\s+font", "font_size"),
    (r"margin[s]?[:\s]+(\d+(?:\.\d+)?)\s*(?:inch|\")", "margins"),
    (r"submit(?:ted)?\s+(?:by|before|no\s+later\s+than)\s+([^\.]+)", "deadline"),
    (r"due\s+(?:date|by)[:\s]+([^\.]+)", "deadline")
]

format_requirements = {}
for pattern, req_type in FORMAT_PATTERNS:
    matches = re.finditer(pattern, combined_content, re.IGNORECASE)
    for match in matches:
        value = match.group(1).strip()
        if req_type not in format_requirements:
            format_requirements[req_type] = []
        format_requirements[req_type].append(value)

# Consolidate to most restrictive
format_summary = {}
if "page_limit" in format_requirements:
    format_summary["page_limit"] = min(int(p) for p in format_requirements["page_limit"] if p.isdigit())
if "font_size" in format_requirements:
    format_summary["font_size_min"] = max(int(f) for f in format_requirements["font_size"] if f.isdigit())
```

### Step 5: Assess Coverage Status

```python
def assess_coverage(item, domain_context):
    """Determine if mandatory item can be addressed.

    Status values: PLANNED, PARTIAL, ADDRESSED, GAP, WAIVED
    PARTIAL indicates capability partially meets requirement — standard compliance
    matrices use Compliant/Partial/Exception/Non-Compliant (FAR standard).
    """
    # Default: addressable (will be updated in later phases)
    return {
        "status": "PLANNED",
        "confidence": "medium",
        "response_strategy": "Will be addressed in bid response",
        "assigned_section": infer_section(item)
    }

def infer_section(item):
    """Infer which bid section should address this item."""
    category = item["category"]
    mapping = {
        "TECHNICAL": "Solution Description",
        "PROCESS": "Management Approach",
        "PERSONNEL": "Key Personnel",
        "COMPLIANCE": "Compliance Matrix",
        "DELIVERY": "Timeline",
        "FORMAT": "Submission Requirements",
        "OTHER": "Technical Approach"
    }
    return mapping.get(category, "Technical Approach")

for item in mandatory_items:
    item["coverage"] = assess_coverage(item, domain_context)
```

### Step 6: Calculate Gate Status

```python
def calculate_gate_status(mandatory_items):
    total = len(mandatory_items)
    addressed = sum(1 for item in mandatory_items if item["coverage"]["status"] in ["PLANNED", "ADDRESSED", "PARTIAL"])
    gaps = [item for item in mandatory_items if item["coverage"]["status"] == "GAP"]

    coverage_pct = (addressed / total * 100) if total > 0 else 100

    gate_passed = coverage_pct == 100

    return {
        "passed": gate_passed,
        "total_mandatory": total,
        "addressed": addressed,
        "gaps": len(gaps),
        "coverage_percentage": round(coverage_pct, 1),
        "gap_items": [{"id": g["id"], "text": g["text"][:100]} for g in gaps]
    }

gate_status = calculate_gate_status(mandatory_items)
```

### Step 6b: Generate RTM Entity Stubs (NEW - for UNIFIED_RTM.json)

```python
## RTM CONTRIBUTION: Generate rfp_sources[] and mandatory_items[] stubs
## These are early entries that Phase 4 will link into the full UNIFIED_RTM.json

rfp_sources = []
source_id_counter = 1

for item in mandatory_items:
    # Create an RFP source reference for each mandatory item location
    source_id = f"SRC-{source_id_counter:03d}"
    rfp_sources.append({
        "source_id": source_id,
        "document": "flattened/*.md",  # Will be refined by Phase 2 with specific file
        "section": item.get("full_context", "")[:100],
        "page_or_row": str(item.get("position", "")),
        "text_excerpt": item["text"][:500]
    })
    item["source_ids"] = [source_id]  # Link mandatory item to its source
    source_id_counter += 1

# Format mandatory items for RTM with coverage_status field
rtm_mandatory_items = []
for item in mandatory_items:
    rtm_mandatory_items.append({
        "mandatory_id": item["id"],  # M001, M002, etc.
        "text": item["text"],
        "source_type": item["source_type"],
        "category": item["category"],
        "linked_requirement_ids": [],  # Populated by Phase 4
        "linked_compliance_framework_ids": [],  # Populated by Phase 4
        "coverage_status": item["coverage"]["status"],  # PLANNED, ADDRESSED, GAP, WAIVED
        "bid_location": None,  # Populated post-bid-authoring
        "source_ids": item.get("source_ids", [])
    })
```

### Step 7: Write Output

```python
compliance_matrix = {
    "extracted_at": datetime.now().isoformat(),
    "gate_status": gate_status,
    "format_requirements": format_summary,
    "mandatory_items": [
        {
            "id": item["id"],
            "text": item["text"],
            "category": item["category"],
            "source_type": item["source_type"],
            "coverage": item["coverage"],
            "source_ids": item.get("source_ids", [])  # RTM: Link to rfp_sources
        }
        for item in mandatory_items
    ],
    "category_summary": {
        category: sum(1 for item in mandatory_items if item["category"] == category)
        for category in CATEGORIES.keys()
    },
    # RTM: Partial entity stubs for Phase 4 to consume
    "rtm_entities": {
        "rfp_sources": rfp_sources,
        "mandatory_items": rtm_mandatory_items
    }
}

write_json(f"{folder}/shared/COMPLIANCE_MATRIX.json", compliance_matrix)
```

### Step 8: Report Results

```
⛔ COMPLIANCE GATEKEEPER RESULTS
================================
Gate Status: {"PASSED ✅" if gate_passed else "FAILED ❌"}

Mandatory Items: {total}
  ✅ Addressed: {addressed}
  ❌ Gaps: {len(gaps)}
  Coverage: {coverage_pct}%

Category Breakdown:
| Category | Count |
|----------|-------|
{table rows}

Format Requirements:
  Page Limit: {format_summary.get("page_limit", "Not specified")}
  Font Size: {format_summary.get("font_size_min", "Not specified")}pt minimum

{if not gate_passed}
⚠️ BLOCKING GATE FAILED
The following mandatory items must be addressed:
{gap_items}

Options:
1. Address gaps and re-run this phase
2. User approval to proceed with known gaps
3. Abort pipeline
{endif}
```

## Gate Failure Handling

If gate fails, present options to user:

```python
if not gate_status["passed"]:
    options = [
        "Address gaps and retry",
        "Proceed with documented gaps (requires approval)",
        "Abort pipeline"
    ]

    # Log gaps for user review
    for gap in gate_status["gap_items"]:
        log(f"  ❌ {gap['id']}: {gap['text']}")

    # Require explicit user approval to proceed with gaps
    raise BlockingGateFailure(
        phase="1.7",
        condition="All mandatory items must be addressed",
        gaps=gate_status["gap_items"],
        options=options
    )
```

## Quality Checklist (MANDATORY — report each by name with evidence)

The phase agent MUST verify each of the following BEFORE reporting completion. The agent's completion report MUST include a checklist-results block with:
- Item name (verbatim from below)
- PASS / FAIL / SKIPPED-WITH-REASON
- Evidence (file:line citation, grep result, file size, assertion that ran, etc.)

"All checks passed" without per-item evidence is NOT acceptable.

### Required output files
1. **COMPLIANCE_MATRIX.json** exists at `{folder}/shared/COMPLIANCE_MATRIX.json` — evidence: `ls -la` size > 500 bytes and parses as valid JSON

### Schema fidelity
2. **COMPLIANCE_MATRIX.json top-level keys** include `extracted_at`, `gate_status`, `format_requirements`, `mandatory_items`, `category_summary`, `rtm_entities` — evidence: list actual top-level keys found
3. **gate_status** contains `passed` (bool), `total_mandatory`, `addressed`, `gaps`, `coverage_percentage` — evidence: print gate_status block values
4. **rtm_entities** contains `rfp_sources` and `mandatory_items` arrays — evidence: print lengths of both arrays
5. No `[:N]` slicing applied to deliverable content strings — evidence: grep for `\[:[0-9]+\]` in production code paths returned 0 hits

### Cross-stage consistency
6. **Every mandatory item linked to a requirement OR explicitly marked `WAIVED` with reason** — at phase-1.7 time all items begin as `PLANNED`; verify no item has `coverage.status = null` — evidence: count of items where status is null (must be 0)
7. **Category summary** totals equal `len(mandatory_items)` — evidence: `sum(category_summary.values())` vs len(mandatory_items)
8. **source_ids present** on every mandatory item in `rtm_entities.mandatory_items` — evidence: count items missing source_ids (must be 0)

### Anti-regression rules (universal)
9. **UTF-8 encoding** on every `open()` call — evidence: search this phase's emitted scripts/code for `encoding='utf-8'` in every file-open
10. **ensure_ascii=False** on every `json.dump` call — evidence: same grep
11. **No `_Showing N of M_` row-cap notices** in any deliverable markdown — evidence: grep returned 0 matches
12. **No empty `|  |` mitigation/cell patterns** in any deliverable table — evidence: grep returned 0 matches in cells with HIGH/MEDIUM/CRITICAL severity rows
13. **No mid-word table-cell truncations** — evidence: line-by-line cell-end check returned 0 hits

### Memory discipline
14. **Relevant SAFS memory entries reviewed and applied** — evidence: list which memory files were read and which rules were applicable
