---
name: phase4-traceability-win
expert-role: Traceability Architect
domain-expertise: Requirements traceability, RTM construction, cross-entity linking, compliance chain verification
---

# Phase 4: Unified Requirements Traceability Matrix (RTM Builder)

## Expert Role

You are a **Traceability Architect** with deep expertise in:
- Requirements Traceability Matrix (RTM) construction
- Bidirectional cross-entity linking
- Compliance chain verification
- Evaluation weight inheritance and composite scoring
- Section-level specification mapping

## Purpose

Consolidate all entity stubs produced by upstream phases into a single **UNIFIED_RTM.json** — the master foreign-key table linking every entity across the full chain:

```
RFP Source → Mandatory Item → Requirement → Specification → Risk → Mitigation → Bid Section → Evidence
```

This phase also materializes `chain_links[]` — denormalized traceability chains per requirement — and computes `composite_priority_score` for content ordering in bid generation.

## Inputs

| Source Phase | File | Entities Consumed |
|-------------|------|-------------------|
| Phase 1.6 | `{folder}/shared/EVALUATION_CRITERIA.json` | `evaluation_factors[]` with `factor_id`, `points`, `subfactors[]` |
| Phase 1.7 | `{folder}/shared/COMPLIANCE_MATRIX.json` | `rtm_entities.rfp_sources[]`, `rtm_entities.mandatory_items[]` |
| Phase 2 | `{folder}/shared/requirements-raw.json` | `rtm_rfp_sources[]` (additional source refs) |
| Phase 2b | `{folder}/shared/requirements-normalized.json` | `requirements[]` with `source_ids[]`, priority, category |
| Phase 3a-3f | `{folder}/outputs/ARCHITECTURE.md` | Section headings for spec linking |
| Phase 3a-3f | `{folder}/outputs/INTEROPERABILITY.md` | Section headings for spec linking |
| Phase 3a-3f | `{folder}/outputs/SECURITY_REQUIREMENTS.md` | Section headings for spec linking |
| Phase 3a-3f | `{folder}/outputs/UI_SPECS.md` | Section headings for spec linking |
| Phase 3a-3f | `{folder}/outputs/ENTITY_DEFINITIONS.md` | Section headings for spec linking |
| Phase 3g | `{folder}/shared/REQUIREMENT_RISKS.json` | `rtm_risks[]` with `risk_id`, `mitigation_strategies[]` |
| Domain | `{folder}/shared/domain-context.json` | Domain context for keyword mapping |

## Required Outputs

- `{folder}/shared/UNIFIED_RTM.json` - The master RTM (conforms to `schemas/unified-rtm.schema.json`)
- `{folder}/outputs/TRACEABILITY.md` (>15KB) - Human-readable traceability document with bidirectional links
- `{folder}/shared/traceability-links.json` - Backward-compatible links for downstream phases

## Instructions

### Step 1: Load All Upstream Artifacts

```python
from datetime import datetime
import hashlib
import re
import glob

# Core data from upstream phases
evaluation_data = read_json(f"{folder}/shared/EVALUATION_CRITERIA.json")
compliance_data = read_json(f"{folder}/shared/COMPLIANCE_MATRIX.json")
requirements_raw = read_json(f"{folder}/shared/requirements-raw.json")
requirements_norm = read_json(f"{folder}/shared/requirements-normalized.json")
risks_data = read_json(f"{folder}/shared/REQUIREMENT_RISKS.json")
domain_context = read_json(f"{folder}/shared/domain-context.json")

# Spec markdown files for section-level linking
SPEC_FILES = {
    "ARCHITECTURE": {"file": "ARCHITECTURE.md", "spec_type": "ARCHITECTURE"},
    "INTEROPERABILITY": {"file": "INTEROPERABILITY.md", "spec_type": "INTEROPERABILITY"},
    "SECURITY": {"file": "SECURITY_REQUIREMENTS.md", "spec_type": "SECURITY"},
    "UI_UX": {"file": "UI_SPECS.md", "spec_type": "UI_UX"},
    "ENTITY": {"file": "ENTITY_DEFINITIONS.md", "spec_type": "ENTITY"},
    "RISK": {"file": "REQUIREMENT_RISKS.md", "spec_type": "RISK"}
}

spec_contents = {}
for key, info in SPEC_FILES.items():
    path = f"{folder}/outputs/{info['file']}"
    try:
        spec_contents[key] = read_file(path)
    except:
        spec_contents[key] = ""
        log(f"⚠️ Spec file not found: {info['file']}")

all_reqs = requirements_norm.get("requirements", [])
log(f"📦 Loaded {len(all_reqs)} requirements, "
    f"{len(compliance_data.get('mandatory_items', []))} mandatory items, "
    f"{len(risks_data.get('rtm_risks', []))} tracked risks, "
    f"{len(evaluation_data.get('evaluation_factors', []))} evaluation factors")
```

### Step 2: Consolidate RFP Sources

Merge `rfp_sources` from Phase 1.7 (compliance) and Phase 2 (requirements extraction) into a deduplicated list.

```python
# Start with compliance sources
rfp_sources = compliance_data.get("rtm_entities", {}).get("rfp_sources", [])
existing_source_ids = {s["source_id"] for s in rfp_sources}

# Add requirement-level sources from Phase 2
for src in requirements_raw.get("rtm_rfp_sources", []):
    if src["source_id"] not in existing_source_ids:
        rfp_sources.append(src)
        existing_source_ids.add(src["source_id"])

log(f"📎 Consolidated {len(rfp_sources)} RFP source references")
```

### Step 3: Build Mandatory Items with Requirement Links

Link each mandatory item (SHALL/MUST) to the requirements that address it. Uses text similarity matching — a requirement addresses a mandatory item if their text shares significant overlap.

```python
mandatory_items = compliance_data.get("rtm_entities", {}).get("mandatory_items", [])

def text_similarity(text_a, text_b):
    """Compute word-overlap similarity between two texts."""
    words_a = set(text_a.lower().split())
    words_b = set(text_b.lower().split())
    if not words_a or not words_b:
        return 0.0
    intersection = words_a & words_b
    # Remove common stop words from intersection count
    stop_words = {"the", "a", "an", "is", "are", "be", "to", "of", "and", "in", "for", "with", "on", "at", "by", "from", "shall", "must", "will", "should", "may"}
    meaningful = intersection - stop_words
    denominator = min(len(words_a - stop_words), len(words_b - stop_words))
    if denominator == 0:
        return 0.0
    return len(meaningful) / denominator

# Link mandatory items → requirements
SIMILARITY_THRESHOLD = 0.35  # Tuned: above this = likely addresses the mandatory item
for m_item in mandatory_items:
    linked_req_ids = []
    m_text = m_item.get("text", "")

    for req in all_reqs:
        req_text = req.get("text", "")
        score = text_similarity(m_text, req_text)
        if score >= SIMILARITY_THRESHOLD:
            req_id = req.get("canonical_id", req.get("id", ""))
            linked_req_ids.append(req_id)

    m_item["linked_requirement_ids"] = linked_req_ids

    # Update coverage status based on links
    if linked_req_ids:
        m_item["coverage_status"] = "ADDRESSED"
    # Keep existing status if no links (PLANNED or GAP)

linked_count = sum(1 for m in mandatory_items if m.get("linked_requirement_ids"))
log(f"🔗 Linked {linked_count}/{len(mandatory_items)} mandatory items to requirements")
```

### Step 4: Build Requirements Entity with Mandatory + Source Links

Construct the RTM requirements array with all cross-references.

```python
# Build req_id → mandatory_item_ids reverse map
req_to_mandatory = {}
for m_item in mandatory_items:
    for req_id in m_item.get("linked_requirement_ids", []):
        if req_id not in req_to_mandatory:
            req_to_mandatory[req_id] = []
        req_to_mandatory[req_id].append(m_item["mandatory_id"])

rtm_requirements = []
for req in all_reqs:
    req_id = req.get("canonical_id", req.get("id", ""))
    rtm_req = {
        "req_id": req_id,
        "text": req.get("text", ""),
        "category": req.get("category", "GENERAL"),
        "priority": req.get("priority", "MEDIUM"),
        "requirement_type": req.get("type", "FUNCTIONAL"),
        "source_ids": req.get("source_ids", []),
        "mandatory_item_ids": req_to_mandatory.get(req_id, []),
        # Populated in Step 6 (evaluation linking)
        "evaluation_factor": None,
        "evaluation_weight_inherited": None,
        "composite_priority_score": None
    }
    rtm_requirements.append(rtm_req)

log(f"📋 Built {len(rtm_requirements)} RTM requirement entries")
```

### Step 5: Section-Level Specification Linking

Parse markdown headings from spec files to create section-level (not file-level) spec entities. Then link requirements to specific sections using keyword + category matching.

```python
def extract_spec_sections(content, spec_key, spec_type):
    """Parse markdown headings into spec section entities."""
    sections = []
    if not content:
        return sections

    # Match ## and ### headings (skip # which is the document title)
    heading_pattern = r'^(#{2,3})\s+(.+)$'
    section_counter = 1

    for match in re.finditer(heading_pattern, content, re.MULTILINE):
        level = len(match.group(1))
        title = match.group(2).strip()

        # Skip generic headings
        if title.lower() in ["overview", "summary", "appendix", "references", "table of contents"]:
            continue

        # Generate anchor from title
        anchor = "#" + re.sub(r'[^a-z0-9\-]', '', title.lower().replace(" ", "-"))

        # Extract content under this heading (up to next same-level heading)
        heading_end = match.end()
        next_heading = re.search(r'^#{2,3}\s+', content[heading_end:], re.MULTILINE)
        section_text = content[heading_end:heading_end + next_heading.start()] if next_heading else content[heading_end:heading_end + 1000]

        # Build category abbreviation from section title words
        cat_abbrev = "".join(w[0].upper() for w in title.split()[:3] if w.isalpha())[:3]
        if not cat_abbrev:
            cat_abbrev = spec_key[:3]

        spec_id = f"SPEC-{spec_key[:4]}-{cat_abbrev}-{section_counter:02d}"

        sections.append({
            "spec_id": spec_id,
            "file": SPEC_FILES[spec_key]["file"],
            "section_anchor": anchor,
            "section_title": title,
            "linked_requirement_ids": [],  # Populated below
            "spec_type": spec_type,
            "_section_text": section_text[:500],  # Transient, used for matching only
            "_keywords": extract_keywords(title + " " + section_text[:300])
        })
        section_counter += 1

    return sections


def extract_keywords(text):
    """Extract meaningful keywords from text for matching."""
    stop_words = {"the", "a", "an", "is", "are", "be", "to", "of", "and", "in", "for", "with",
                  "on", "at", "by", "from", "this", "that", "it", "or", "as", "not", "all",
                  "each", "can", "will", "should", "may", "shall", "must", "such", "any"}
    words = set(re.findall(r'[a-z]{3,}', text.lower()))
    return words - stop_words


# Extract sections from all spec files
all_spec_sections = []
for spec_key, spec_info in SPEC_FILES.items():
    sections = extract_spec_sections(
        spec_contents.get(spec_key, ""),
        spec_key,
        spec_info["spec_type"]
    )
    all_spec_sections.extend(sections)

log(f"📐 Extracted {len(all_spec_sections)} spec sections across {len(SPEC_FILES)} spec files")

# CATEGORY → SPEC_TYPE affinity mapping
# Requirements in certain categories preferentially link to certain spec types
CATEGORY_SPEC_AFFINITY = {
    "ENROLLMENT": ["ARCHITECTURE", "UI_UX", "ENTITY"],
    "COLLECTION": ["ARCHITECTURE", "INTEROPERABILITY", "ENTITY"],
    "CALCULATION": ["ARCHITECTURE", "ENTITY", "SECURITY"],
    "REPORTING": ["ARCHITECTURE", "UI_UX", "INTEROPERABILITY"],
    "SECURITY": ["SECURITY", "ARCHITECTURE"],
    "INTEGRATION": ["INTEROPERABILITY", "ARCHITECTURE"],
    "DATA": ["ENTITY", "ARCHITECTURE"],
    "UI": ["UI_UX", "ARCHITECTURE"],
    "COMPLIANCE": ["SECURITY", "ARCHITECTURE"],
    "PERFORMANCE": ["ARCHITECTURE"],
    "GENERAL": ["ARCHITECTURE"]
}

# Link requirements to spec sections
SPEC_LINK_THRESHOLD = 3  # Minimum keyword overlap

for req in all_reqs:
    req_id = req.get("canonical_id", req.get("id", ""))
    req_keywords = extract_keywords(req.get("text", ""))
    req_category = req.get("category", "GENERAL").upper()

    # Get preferred spec types for this requirement's category
    preferred_types = CATEGORY_SPEC_AFFINITY.get(req_category, ["ARCHITECTURE"])

    best_matches = []
    for section in all_spec_sections:
        # Calculate keyword overlap
        overlap = len(req_keywords & section["_keywords"])

        # Boost score if spec type matches category affinity
        affinity_boost = 2 if section["spec_type"] in preferred_types else 0

        total_score = overlap + affinity_boost

        if total_score >= SPEC_LINK_THRESHOLD:
            best_matches.append((section, total_score))

    # Sort by score descending, take top 3 sections
    best_matches.sort(key=lambda x: x[1], reverse=True)
    for section, score in best_matches[:3]:
        if req_id not in section["linked_requirement_ids"]:
            section["linked_requirement_ids"].append(req_id)

# Fallback: requirements with no spec links get linked to ARCHITECTURE first section
architecture_sections = [s for s in all_spec_sections if s["spec_type"] == "ARCHITECTURE"]
if architecture_sections:
    fallback_section = architecture_sections[0]
    for req in all_reqs:
        req_id = req.get("canonical_id", req.get("id", ""))
        linked = any(req_id in s["linked_requirement_ids"] for s in all_spec_sections)
        if not linked:
            fallback_section["linked_requirement_ids"].append(req_id)

# Clean up transient fields
for section in all_spec_sections:
    section.pop("_section_text", None)
    section.pop("_keywords", None)

linked_to_specs = sum(1 for req in all_reqs
    if any(req.get("canonical_id", req.get("id", "")) in s["linked_requirement_ids"]
           for s in all_spec_sections))
log(f"🔗 Linked {linked_to_specs}/{len(all_reqs)} requirements to spec sections")
```

### Step 6: Evaluation Criteria Linking and Weight Inheritance

Map each requirement to an evaluation factor based on category/keywords, then compute composite_priority_score.

```python
eval_factors = evaluation_data.get("evaluation_factors", [])

# Build evaluation factor entity for RTM
rtm_eval_criteria = []
for factor in eval_factors:
    rtm_factor = {
        "factor_id": factor.get("factor_id", ""),
        "name": factor.get("name", ""),
        "points": factor.get("points", 0),
        "weight_percent": factor.get("weight_normalized", 0),
        "subfactors": [],
        "linked_requirement_ids": [],
        "linked_bid_section_ids": []  # Populated post-bid-authoring
    }

    for sf in factor.get("subfactors", []):
        rtm_factor["subfactors"].append({
            "subfactor_id": sf.get("subfactor_id", ""),
            "name": sf.get("name", ""),
            "points": sf.get("points", 0),
            "linked_requirement_ids": [],
            "bid_sections_serving": []
        })

    rtm_eval_criteria.append(rtm_factor)

# Map requirement categories to evaluation factors
CATEGORY_TO_EVAL_FACTOR = {
    "ENROLLMENT": "Technical Approach",
    "COLLECTION": "Technical Approach",
    "CALCULATION": "Technical Approach",
    "REPORTING": "Technical Approach",
    "SECURITY": "Technical Approach",
    "INTEGRATION": "Technical Approach",
    "DATA": "Technical Approach",
    "UI": "Technical Approach",
    "COMPLIANCE": "Corporate Experience",
    "PERFORMANCE": "Technical Approach",
    "MANAGEMENT": "Management Approach",
    "PERSONNEL": "Key Personnel",
    "STAFFING": "Key Personnel",
    "EXPERIENCE": "Past Performance",
    "REFERENCES": "Past Performance",
    "COST": "Price/Cost",
    "PRICING": "Price/Cost",
    "GENERAL": "Technical Approach"
}

# Build factor name → factor object lookup
factor_lookup = {f["name"]: f for f in rtm_eval_criteria}

# Link requirements to evaluation factors
for rtm_req in rtm_requirements:
    req_category = rtm_req["category"].upper()
    eval_factor_name = CATEGORY_TO_EVAL_FACTOR.get(req_category, "Technical Approach")

    factor = factor_lookup.get(eval_factor_name)
    if factor:
        rtm_req["evaluation_factor"] = eval_factor_name
        rtm_req["evaluation_weight_inherited"] = factor["points"]
        if rtm_req["req_id"] not in factor["linked_requirement_ids"]:
            factor["linked_requirement_ids"].append(rtm_req["req_id"])
    else:
        # Default to first factor if named factor not found
        if rtm_eval_criteria:
            first_factor = rtm_eval_criteria[0]
            rtm_req["evaluation_factor"] = first_factor["name"]
            rtm_req["evaluation_weight_inherited"] = first_factor["points"]
            if rtm_req["req_id"] not in first_factor["linked_requirement_ids"]:
                first_factor["linked_requirement_ids"].append(rtm_req["req_id"])

# Compute composite_priority_score
PRIORITY_BONUS = {"CRITICAL": 100, "HIGH": 50, "MEDIUM": 25, "LOW": 10}
MANDATORY_BONUS = 100
RISK_BONUS = {"HIGH": 50, "MEDIUM": 25, "LOW": 0, "CRITICAL": 75}

# Build req_id → risk_level lookup
risk_level_lookup = {}
for risk in risks_data.get("rtm_risks", []):
    for linked_req in risk.get("linked_requirement_ids", []):
        existing_level = risk_level_lookup.get(linked_req, "LOW")
        new_level = risk.get("risk_level", "LOW")
        # Keep the higher risk level
        level_order = {"LOW": 0, "MEDIUM": 1, "HIGH": 2, "CRITICAL": 3}
        if level_order.get(new_level, 0) > level_order.get(existing_level, 0):
            risk_level_lookup[linked_req] = new_level

for rtm_req in rtm_requirements:
    eval_weight = rtm_req.get("evaluation_weight_inherited") or 0
    priority_bonus = PRIORITY_BONUS.get(rtm_req["priority"], 10)
    mandatory_bonus = MANDATORY_BONUS if rtm_req["mandatory_item_ids"] else 0
    risk_level = risk_level_lookup.get(rtm_req["req_id"], "LOW")
    risk_bonus = RISK_BONUS.get(risk_level, 0)

    rtm_req["composite_priority_score"] = eval_weight + priority_bonus + mandatory_bonus + risk_bonus

log(f"📊 Computed composite_priority_score for {len(rtm_requirements)} requirements")
log(f"   Top score: {max(r['composite_priority_score'] for r in rtm_requirements)}, "
    f"Avg: {sum(r['composite_priority_score'] for r in rtm_requirements) / len(rtm_requirements):.0f}")
```

### Step 7: Consolidate Risks Entity

Import risk stubs from Phase 3g into RTM format.

```python
rtm_risks = risks_data.get("rtm_risks", [])

# Ensure all risks have proper structure
for risk in rtm_risks:
    # Verify linked_requirement_ids exist in our requirements
    valid_req_ids = {r["req_id"] for r in rtm_requirements}
    risk["linked_requirement_ids"] = [
        rid for rid in risk.get("linked_requirement_ids", [])
        if rid in valid_req_ids
    ]

    for mitigation in risk.get("mitigation_strategies", []):
        if "bid_location" not in mitigation:
            mitigation["bid_location"] = None
        if "evidence_ids" not in mitigation:
            mitigation["evidence_ids"] = []

log(f"⚠️ Consolidated {len(rtm_risks)} risks with {sum(len(r.get('mitigation_strategies', [])) for r in rtm_risks)} mitigations")
```

### Step 8: Initialize Bid Sections and Evidence (Stubs)

These are populated post-bid-authoring by Phase 8 and Phase 8f. We create empty arrays now.

```python
# bid_sections[] - populated by Phase 8 after bid files are written
rtm_bid_sections = []

# evidence[] - populated by Phase 8 as proof artifacts are identified
rtm_evidence = []
```

### Step 9: Materialize Chain Links

For every requirement, build a complete traceability chain showing its path through all entities. Compute completeness_score per chain.

```python
def build_chain_links(requirements, mandatory_items, spec_sections, risks, bid_sections, evidence):
    """Materialize denormalized chain_links[] - one per requirement."""

    chains = []

    # Build reverse lookups
    req_to_specs = {}
    for spec in spec_sections:
        for rid in spec.get("linked_requirement_ids", []):
            if rid not in req_to_specs:
                req_to_specs[rid] = []
            req_to_specs[rid].append(spec["spec_id"])

    req_to_risks = {}
    req_to_mitigations = {}
    for risk in risks:
        for rid in risk.get("linked_requirement_ids", []):
            if rid not in req_to_risks:
                req_to_risks[rid] = []
            req_to_risks[rid].append(risk["risk_id"])
            for mit in risk.get("mitigation_strategies", []):
                if rid not in req_to_mitigations:
                    req_to_mitigations[rid] = []
                req_to_mitigations[rid].append(mit["mitigation_id"])

    req_to_bid = {}
    for bs in bid_sections:
        for rid in bs.get("linked_requirement_ids", []):
            if rid not in req_to_bid:
                req_to_bid[rid] = []
            req_to_bid[rid].append(bs["bid_section_id"])

    req_to_evidence = {}
    for ev in evidence:
        for rid in ev.get("linked_requirement_ids", []):
            if rid not in req_to_evidence:
                req_to_evidence[rid] = []
            req_to_evidence[rid].append(ev["evidence_id"])

    for req in requirements:
        req_id = req["req_id"]
        chain_id = f"CHAIN-{req_id}"

        # Gather all linked entities
        source_ids = req.get("source_ids", [])
        mandatory_ids = req.get("mandatory_item_ids", [])
        spec_ids = req_to_specs.get(req_id, [])
        risk_ids = req_to_risks.get(req_id, [])
        mitigation_ids = req_to_mitigations.get(req_id, [])
        bid_ids = req_to_bid.get(req_id, [])
        evidence_ids = req_to_evidence.get(req_id, [])

        # Compute completeness score (0.0 to 1.0)
        # Scoring weights per link type:
        # rfp_source: 0.10, mandatory: 0.05 (optional), spec: 0.25, risk: 0.10 (optional),
        # bid_section: 0.35, evidence: 0.15
        score = 0.0
        missing = []

        if source_ids:
            score += 0.10
        else:
            missing.append("rfp_source")

        # Mandatory link is optional (not all reqs trace to mandatory items)
        if mandatory_ids:
            score += 0.05

        if spec_ids:
            score += 0.25
        else:
            missing.append("specification")

        # Risk link is optional (not all reqs have risks)
        if risk_ids:
            score += 0.10

        if bid_ids:
            score += 0.35
        else:
            missing.append("bid_section")

        if evidence_ids:
            score += 0.15
        else:
            missing.append("evidence")

        # Determine status
        if score >= 0.70:
            status = "COMPLETE"
        elif score >= 0.35:
            status = "PARTIAL"
        else:
            status = "BROKEN"

        chains.append({
            "chain_id": chain_id,
            "rfp_source": source_ids[0] if source_ids else None,
            "mandatory_items": mandatory_ids,
            "requirement": req_id,
            "specifications": spec_ids,
            "risks": risk_ids,
            "mitigations": mitigation_ids,
            "bid_sections": bid_ids,
            "evidence": evidence_ids,
            "evaluation_factor": req.get("evaluation_factor"),
            "status": status,
            "completeness_score": round(score, 2),
            "missing_links": missing
        })

    return chains


chain_links = build_chain_links(
    rtm_requirements, mandatory_items, all_spec_sections,
    rtm_risks, rtm_bid_sections, rtm_evidence
)

# Report chain statistics
complete = sum(1 for c in chain_links if c["status"] == "COMPLETE")
partial = sum(1 for c in chain_links if c["status"] == "PARTIAL")
broken = sum(1 for c in chain_links if c["status"] == "BROKEN")
avg_score = sum(c["completeness_score"] for c in chain_links) / len(chain_links) if chain_links else 0

log(f"⛓️ Materialized {len(chain_links)} chains: "
    f"✅ {complete} COMPLETE, ⚠️ {partial} PARTIAL, ❌ {broken} BROKEN")
log(f"   Average completeness: {avg_score:.2f}")
```

### Step 10: Compute Initial Verification Metrics

Populate the `verification{}` section with coverage metrics (Phase 8f will update these post-bid).

```python
def compute_verification(requirements, specs, risks, bid_sections, mandatory_items, chains):
    """Compute initial verification metrics."""

    total_reqs = len(requirements)

    # Forward coverage
    reqs_with_specs = sum(1 for r in requirements
        if any(r["req_id"] in s.get("linked_requirement_ids", []) for s in specs))
    reqs_with_bid = sum(1 for r in requirements
        if any(r["req_id"] in bs.get("linked_requirement_ids", []) for bs in bid_sections))
    reqs_with_risks = sum(1 for r in requirements
        if any(r["req_id"] in risk.get("linked_requirement_ids", []) for risk in risks))

    # Backward coverage
    bid_with_reqs = sum(1 for bs in bid_sections if bs.get("linked_requirement_ids"))
    orphaned_bid = sum(1 for bs in bid_sections if not bs.get("linked_requirement_ids"))

    # Chain completeness
    complete_chains = sum(1 for c in chains if c["status"] == "COMPLETE")
    partial_chains = sum(1 for c in chains if c["status"] == "PARTIAL")
    broken_chains = sum(1 for c in chains if c["status"] == "BROKEN")
    avg_completeness = sum(c["completeness_score"] for c in chains) / len(chains) if chains else 0

    # Compliance alignment
    mandatory_in_bid = sum(1 for m in mandatory_items if m.get("bid_location"))
    mandatory_total = len(mandatory_items)

    return {
        "last_run": datetime.now().isoformat(),
        "run_by_phase": "phase4-traceability",
        "forward_coverage": {
            "requirements_with_specs": reqs_with_specs,
            "requirements_with_bid_sections": reqs_with_bid,
            "requirements_with_risks": reqs_with_risks,
            "requirements_total": total_reqs,
            "spec_coverage_pct": round(reqs_with_specs / total_reqs * 100, 1) if total_reqs else 0,
            "bid_coverage_pct": round(reqs_with_bid / total_reqs * 100, 1) if total_reqs else 0
        },
        "backward_coverage": {
            "bid_sections_with_requirements": bid_with_reqs,
            "bid_sections_total": len(bid_sections),
            "orphaned_bid_content": orphaned_bid
        },
        "chain_completeness": {
            "complete_chains": complete_chains,
            "partial_chains": partial_chains,
            "broken_chains": broken_chains,
            "avg_completeness_score": round(avg_completeness, 3)
        },
        "compliance_alignment": {
            "mandatory_items_in_bid": mandatory_in_bid,
            "mandatory_items_total": mandatory_total,
            "coverage_pct": round(mandatory_in_bid / mandatory_total * 100, 1) if mandatory_total else 0
        },
        "query_results": []  # Populated by Phase 8f
    }


verification = compute_verification(
    rtm_requirements, all_spec_sections, rtm_risks,
    rtm_bid_sections, mandatory_items, chain_links
)
```

### Step 11: Compute Integrity Hash and Write UNIFIED_RTM.json

```python
# Compute integrity hash from requirements text (for change detection)
req_text_blob = "|".join(r["req_id"] + ":" + r["text"][:100] for r in rtm_requirements)
integrity_hash = hashlib.sha256(req_text_blob.encode()).hexdigest()

# Assemble the full RTM
unified_rtm = {
    "meta": {
        "generated_at": datetime.now().isoformat(),
        "rfp_id": domain_context.get("rfp_id", "UNKNOWN"),
        "rfp_title": domain_context.get("rfp_title", domain_context.get("selected_domain", "Unknown RFP")),
        "last_updated_by_phase": "phase4-traceability",
        "chain_version": 1,
        "integrity_hash": integrity_hash
    },
    "entities": {
        "rfp_sources": rfp_sources,
        "mandatory_items": mandatory_items,
        "requirements": rtm_requirements,
        "specifications": all_spec_sections,
        "risks": rtm_risks,
        "bid_sections": rtm_bid_sections,
        "evidence": rtm_evidence,
        "evaluation_criteria": rtm_eval_criteria
    },
    "chain_links": chain_links,
    "verification": verification
}

write_json(f"{folder}/shared/UNIFIED_RTM.json", unified_rtm)
log(f"✅ Wrote UNIFIED_RTM.json ({len(rtm_requirements)} reqs, {len(all_spec_sections)} specs, "
    f"{len(rtm_risks)} risks, {len(chain_links)} chains)")
```

### Step 12: Generate Backward-Compatible traceability-links.json

For downstream phases that still consume the old format.

```python
# Build forward/backward links in the legacy format
forward_links = {}
backward_links = {
    "specifications": {},
    "bid_sections": {},
    "risks": {},
    "rfp_sources": {}
}

# Build spec lookup: req_id → spec files
req_to_spec_files = {}
for spec in all_spec_sections:
    for rid in spec.get("linked_requirement_ids", []):
        if rid not in req_to_spec_files:
            req_to_spec_files[rid] = set()
        req_to_spec_files[rid].add(spec["file"])

# Build risk lookup
risk_lookup = {}
for risk in rtm_risks:
    for rid in risk.get("linked_requirement_ids", []):
        risk_lookup[rid] = {
            "risk_id": risk["risk_id"],
            "risk_level": risk["risk_level"]
        }

for req in rtm_requirements:
    req_id = req["req_id"]
    spec_files = list(req_to_spec_files.get(req_id, ["ARCHITECTURE.md"]))
    risk_info = risk_lookup.get(req_id, {})

    forward_links[req_id] = {
        "requirement_text": req["text"][:200],
        "category": req["category"],
        "priority": req["priority"],
        "rfp_source": req["source_ids"][0] if req["source_ids"] else "Unknown",
        "specifications": spec_files,
        "bid_section": "Technical Approach",  # Updated post-bid-authoring
        "risk_id": risk_info.get("risk_id"),
        "risk_level": risk_info.get("risk_level", "N/A"),
        "composite_priority_score": req["composite_priority_score"]
    }

    # Backward: spec → reqs
    for spec_file in spec_files:
        if spec_file not in backward_links["specifications"]:
            backward_links["specifications"][spec_file] = []
        backward_links["specifications"][spec_file].append(req_id)

    # Backward: risk → reqs
    if risk_info.get("risk_id"):
        rid = risk_info["risk_id"]
        if rid not in backward_links["risks"]:
            backward_links["risks"][rid] = []
        backward_links["risks"][rid].append(req_id)

    # Backward: source → reqs
    for src_id in req.get("source_ids", []):
        if src_id not in backward_links["rfp_sources"]:
            backward_links["rfp_sources"][src_id] = []
        backward_links["rfp_sources"][src_id].append(req_id)

traceability_links = {
    "generated_at": datetime.now().isoformat(),
    "forward_links": forward_links,
    "backward_links": backward_links,
    "statistics": {
        "total_requirements": len(forward_links),
        "specifications_covered": len(backward_links["specifications"]),
        "bid_sections_covered": len(backward_links.get("bid_sections", {})),
        "rfp_sources": len(backward_links["rfp_sources"]),
        "chain_completeness": {
            "complete": complete,
            "partial": partial,
            "broken": broken,
            "avg_score": round(avg_score, 3)
        }
    }
}

write_json(f"{folder}/shared/traceability-links.json", traceability_links)
```

### Step 13: Generate Enhanced TRACEABILITY.md

```python
def generate_traceability_md(rtm):
    """Generate comprehensive human-readable traceability document."""
    entities = rtm["entities"]
    reqs = entities["requirements"]
    specs = entities["specifications"]
    risks = entities["risks"]
    mandatory = entities["mandatory_items"]
    eval_criteria = entities["evaluation_criteria"]
    chains = rtm["chain_links"]
    verify = rtm["verification"]

    domain = rtm["meta"].get("rfp_title", "Unknown")

    # Sort requirements by composite_priority_score (highest first)
    sorted_reqs = sorted(reqs, key=lambda r: r.get("composite_priority_score", 0), reverse=True)

    doc = f"""# Unified Traceability Matrix

**RFP:** {domain}
**Generated:** {datetime.now().strftime('%Y-%m-%d %H:%M')}
**Requirements Traced:** {len(reqs)}
**Chain Version:** {rtm["meta"]["chain_version"]}

---

## Executive Summary

| Metric | Value |
|--------|-------|
| Total Requirements | {len(reqs)} |
| Mandatory Items | {len(mandatory)} |
| Specification Sections | {len(specs)} |
| Risks Tracked | {len(risks)} |
| Evaluation Factors | {len(eval_criteria)} |
| Chain Completeness | {verify["chain_completeness"]["avg_completeness_score"]:.1%} |
| Complete Chains | {verify["chain_completeness"]["complete_chains"]} |
| Partial Chains | {verify["chain_completeness"]["partial_chains"]} |
| Broken Chains | {verify["chain_completeness"]["broken_chains"]} |

---

## Chain Status Distribution

| Status | Count | Percentage |
|--------|-------|------------|
| COMPLETE | {verify["chain_completeness"]["complete_chains"]} | {verify["chain_completeness"]["complete_chains"]/len(chains)*100:.1f}% |
| PARTIAL | {verify["chain_completeness"]["partial_chains"]} | {verify["chain_completeness"]["partial_chains"]/len(chains)*100:.1f}% |
| BROKEN | {verify["chain_completeness"]["broken_chains"]} | {verify["chain_completeness"]["broken_chains"]/len(chains)*100:.1f}% |

---

## Evaluation Weight Distribution

| Factor | Points | Weight | Requirements Linked |
|--------|--------|--------|-------------------|
"""

    for factor in eval_criteria:
        doc += f"| {factor['name']} | {factor['points']} | {factor.get('weight_percent', 0)}% | {len(factor.get('linked_requirement_ids', []))} |\n"

    doc += f"""
---

## Composite Priority Scoring

Top 20 requirements by `composite_priority_score` (drives bid content ordering):

| Rank | Req ID | Category | Priority | Score | Eval Factor | Mandatory? | Risk? |
|------|--------|----------|----------|-------|-------------|------------|-------|
"""

    for rank, req in enumerate(sorted_reqs[:20], 1):
        is_mandatory = "YES" if req.get("mandatory_item_ids") else "-"
        has_risk = risk_level_lookup.get(req["req_id"], "-")
        doc += (f"| {rank} | {req['req_id']} | {req['category']} | {req['priority']} | "
                f"{req['composite_priority_score']} | {req.get('evaluation_factor', 'N/A')} | "
                f"{is_mandatory} | {has_risk} |\n")

    doc += f"""
---

## Mandatory Item Coverage

| Mandatory ID | Category | Source | Linked Reqs | Status |
|-------------|----------|--------|-------------|--------|
"""

    for m in mandatory:
        linked = len(m.get("linked_requirement_ids", []))
        status = m.get("coverage_status", "UNKNOWN")
        status_icon = "✅" if status == "ADDRESSED" else "⚠️" if status == "PLANNED" else "❌"
        doc += f"| {m['mandatory_id']} | {m['category']} | {m['source_type']} | {linked} | {status_icon} {status} |\n"

    addressed = sum(1 for m in mandatory if m.get("coverage_status") == "ADDRESSED")
    doc += f"\n**Coverage: {addressed}/{len(mandatory)} mandatory items linked to requirements ({addressed/len(mandatory)*100:.1f}%)**\n"

    doc += """
---

## Forward Traceability Matrix

| Req ID | Category | Priority | Score | → Specs | → Risks | → Bid Section |
|--------|----------|----------|-------|---------|---------|---------------|
"""

    # Build quick lookups for the table
    req_specs_map = {}
    for spec in specs:
        for rid in spec.get("linked_requirement_ids", []):
            if rid not in req_specs_map:
                req_specs_map[rid] = []
            req_specs_map[rid].append(spec["file"])

    req_risks_map = {}
    for risk in risks:
        for rid in risk.get("linked_requirement_ids", []):
            req_risks_map[rid] = risk["risk_level"]

    for req in sorted_reqs:
        req_id = req["req_id"]
        spec_files = set(req_specs_map.get(req_id, []))
        spec_str = ", ".join(sorted(spec_files))[:30] or "N/A"
        risk_str = req_risks_map.get(req_id, "-")
        doc += (f"| {req_id} | {req['category']} | {req['priority']} | "
                f"{req.get('composite_priority_score', 0)} | {spec_str} | {risk_str} | TBD |\n")

    doc += """
---

## Backward Traceability

### By Specification Document

| Spec File | Type | Sections | Requirements |
|-----------|------|----------|-------------|
"""

    spec_by_file = {}
    for spec in specs:
        f = spec["file"]
        if f not in spec_by_file:
            spec_by_file[f] = {"type": spec["spec_type"], "sections": 0, "reqs": set()}
        spec_by_file[f]["sections"] += 1
        spec_by_file[f]["reqs"].update(spec.get("linked_requirement_ids", []))

    for f, info in sorted(spec_by_file.items(), key=lambda x: len(x[1]["reqs"]), reverse=True):
        doc += f"| {f} | {info['type']} | {info['sections']} | {len(info['reqs'])} |\n"

    doc += """

### By Risk Level

| Risk Level | Count | Requirements Affected |
|------------|-------|--------------------|
"""

    risk_by_level = {"CRITICAL": 0, "HIGH": 0, "MEDIUM": 0, "LOW": 0}
    reqs_by_risk = {"CRITICAL": set(), "HIGH": set(), "MEDIUM": set(), "LOW": set()}
    for risk in risks:
        level = risk.get("risk_level", "LOW")
        if level in risk_by_level:
            risk_by_level[level] += 1
            reqs_by_risk[level].update(risk.get("linked_requirement_ids", []))

    for level in ["CRITICAL", "HIGH", "MEDIUM", "LOW"]:
        doc += f"| {level} | {risk_by_level[level]} | {len(reqs_by_risk[level])} |\n"

    doc += f"""
---

## Chain Integrity Analysis

### Broken Chains (Require Attention)

"""

    broken_chains = [c for c in chains if c["status"] == "BROKEN"]
    if broken_chains:
        doc += f"⚠️ **{len(broken_chains)} broken chains detected:**\n\n"
        doc += "| Chain | Requirement | Score | Missing Links |\n"
        doc += "|-------|-------------|-------|---------------|\n"
        for chain in broken_chains[:30]:
            missing = ", ".join(chain.get("missing_links", []))
            doc += f"| {chain['chain_id']} | {chain['requirement']} | {chain['completeness_score']:.2f} | {missing} |\n"
    else:
        doc += "✅ No broken chains.\n"

    doc += """

### Partial Chains

"""

    partial_chains = [c for c in chains if c["status"] == "PARTIAL"]
    if partial_chains:
        doc += f"| Chain | Score | Missing |\n"
        doc += f"|-------|-------|--------|\n"
        for chain in partial_chains[:20]:
            missing = ", ".join(chain.get("missing_links", []))
            doc += f"| {chain['chain_id']} | {chain['completeness_score']:.2f} | {missing} |\n"

    doc += f"""
---

## Specification Section Detail

"""

    for spec in sorted(specs, key=lambda s: len(s.get("linked_requirement_ids", [])), reverse=True)[:30]:
        linked = len(spec.get("linked_requirement_ids", []))
        preview = ", ".join(spec.get("linked_requirement_ids", [])[:5])
        if linked > 5:
            preview += f" (+{linked - 5} more)"
        doc += f"### {spec['spec_id']}: {spec.get('section_title', 'N/A')}\n\n"
        doc += f"- **File:** {spec['file']}\n"
        doc += f"- **Type:** {spec['spec_type']}\n"
        doc += f"- **Requirements:** {linked} ({preview})\n\n"

    doc += f"""
---

## Appendix: Full Requirements Register (by Composite Score)

| Req ID | Cat | Pri | Score | Sources | Mandatory | Specs | Risks | Chain |
|--------|-----|-----|-------|---------|-----------|-------|-------|-------|
"""

    for req in sorted_reqs:
        rid = req["req_id"]
        chain_entry = next((c for c in chains if c["requirement"] == rid), None)
        chain_status = chain_entry["status"] if chain_entry else "N/A"
        chain_icon = "✅" if chain_status == "COMPLETE" else "⚠️" if chain_status == "PARTIAL" else "❌"
        src_count = len(req.get("source_ids", []))
        mand_count = len(req.get("mandatory_item_ids", []))
        spec_count = len(req_specs_map.get(rid, []))
        risk_str = req_risks_map.get(rid, "-")

        doc += (f"| {rid} | {req['category'][:4]} | {req['priority'][:3]} | "
                f"{req.get('composite_priority_score', 0)} | {src_count} | {mand_count} | "
                f"{spec_count} | {risk_str} | {chain_icon} |\n")

    return doc


traceability_md = generate_traceability_md(unified_rtm)
write_file(f"{folder}/outputs/TRACEABILITY.md", traceability_md)
```

### Step 14: Report Results

```
🔗 UNIFIED RTM CONSTRUCTION COMPLETE
======================================
Phase: 4 - Traceability Matrix (RTM Builder)

Entity Counts:
| Entity | Count |
|--------|-------|
| RFP Sources | {len(rfp_sources)} |
| Mandatory Items | {len(mandatory_items)} |
| Requirements | {len(rtm_requirements)} |
| Spec Sections | {len(all_spec_sections)} |
| Risks | {len(rtm_risks)} |
| Evaluation Criteria | {len(rtm_eval_criteria)} |
| Chain Links | {len(chain_links)} |

Chain Integrity:
  ✅ COMPLETE: {complete} ({complete/len(chain_links)*100:.1f}%)
  ⚠️ PARTIAL:  {partial} ({partial/len(chain_links)*100:.1f}%)
  ❌ BROKEN:   {broken} ({broken/len(chain_links)*100:.1f}%)
  Average Completeness: {avg_score:.2f}

Coverage Metrics:
  Spec Coverage: {verification["forward_coverage"]["spec_coverage_pct"]}%
  Mandatory Items Linked: {sum(1 for m in mandatory_items if m.get("linked_requirement_ids"))}/{len(mandatory_items)}

Composite Priority Score:
  Max: {max(r["composite_priority_score"] for r in rtm_requirements)}
  Avg: {sum(r["composite_priority_score"] for r in rtm_requirements) / len(rtm_requirements):.0f}
  Min: {min(r["composite_priority_score"] for r in rtm_requirements)}

Files Written:
  ✅ shared/UNIFIED_RTM.json (master RTM)
  ✅ outputs/TRACEABILITY.md (human-readable)
  ✅ shared/traceability-links.json (backward-compatible)

NOTE: bid_sections[] and evidence[] are empty stubs.
      Phase 8 (bid authoring) will populate bid_sections[].
      Phase 8f (RTM verification) will run the 14 verification queries.
```

## Quality Checklist

- [ ] `UNIFIED_RTM.json` created in `shared/` (conforms to `schemas/unified-rtm.schema.json`)
- [ ] `TRACEABILITY.md` created in `outputs/` (>15KB)
- [ ] `traceability-links.json` created in `shared/` (backward-compatible)
- [ ] All 8 entity arrays populated: rfp_sources, mandatory_items, requirements, specifications, risks, bid_sections (stub), evidence (stub), evaluation_criteria
- [ ] Mandatory items linked to requirements (text similarity >= 0.35)
- [ ] Requirements linked to spec sections (section-level, not file-level)
- [ ] Evaluation weight inheritance computed (composite_priority_score)
- [ ] chain_links[] materialized for every requirement with completeness_score
- [ ] Verification metrics computed (forward coverage, chain completeness)
- [ ] Integrity hash computed for change detection
- [ ] Broken chains identified and reported
- [ ] Top requirements by composite score highlighted in TRACEABILITY.md
