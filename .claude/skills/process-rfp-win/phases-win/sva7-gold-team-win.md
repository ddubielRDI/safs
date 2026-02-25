---
name: sva7-gold-team-win
expert-role: Gold Team Reviewer
domain-expertise: Final bid quality review, win theme threading, compliance verification, evaluator persona simulation, Shipley Gold Team review
---

# SVA-7: Gold Team Review

## Expert Role

You are a **Gold Team Reviewer** conducting the final quality gate (~95% completion). Your focus:
- Win theme semantic threading (themes appear with evidence, not just as headings)
- Risk-to-bid traceability (every HIGH risk mitigation verified in bid text)
- Compliance-to-bid verification (every mandatory item addressed in bid)
- Cross-document ID integrity (no phantom references)
- Evaluator persona satisfaction simulation
- Format compliance (page limits, structure)
- Statistical consistency between bid text and source data

## Purpose

Validate that Stage 7 (Bid Generation) produced a complete, compliant, evaluator-ready bid submission. This is the Shipley **Gold Team** review — the last chance to catch issues before submission. The Gold Team simulates the actual evaluation process and scores the bid as evaluators would.

## Inputs

- `{folder}/shared/UNIFIED_RTM.json` - Unified RTM with bid_sections populated
- `{folder}/shared/COMPLIANCE_MATRIX.json` - Mandatory items
- `{folder}/shared/EVALUATION_CRITERIA.json` - Evaluation factors with weights
- `{folder}/shared/REQUIREMENT_RISKS.json` - Risk assessments
- `{folder}/shared/PERSONA_COVERAGE.json` - Evaluator personas (optional)
- `{folder}/shared/CLIENT_INTELLIGENCE.json` - Client intelligence (optional)
- `{folder}/shared/bid-context-bundle.json` - Context bundle with win themes
- `{folder}/shared/SUBMISSION_STRUCTURE.json` - Submission structure (optional)
- `{folder}/outputs/bid-sections/*.md` - All bid section markdown files
- `{folder}/outputs/*.md` - Legacy bid output files

## Required Output

- `{folder}/shared/validation/sva7-gold-team.json` - Gold Team review report
- `{folder}/outputs/GOLD_TEAM_CHECKLIST.md` - Human review checklist for Gold Team sign-off

## Instructions

### Step 1: Load All Data

```python
from datetime import datetime
import re
import glob
import os

rtm = read_json(f"{folder}/shared/UNIFIED_RTM.json")
compliance = read_json(f"{folder}/shared/COMPLIANCE_MATRIX.json")
evaluation = read_json(f"{folder}/shared/EVALUATION_CRITERIA.json")
risks = read_json(f"{folder}/shared/REQUIREMENT_RISKS.json")
context_bundle = read_json(f"{folder}/shared/bid-context-bundle.json")

# Optional
personas = read_json_safe(f"{folder}/shared/PERSONA_COVERAGE.json")
client_intel = read_json_safe(f"{folder}/shared/CLIENT_INTELLIGENCE.json")
submission_structure = read_json_safe(f"{folder}/shared/SUBMISSION_STRUCTURE.json")

# Extract entities from RTM
entities = rtm.get("entities", {})
rtm_reqs = entities.get("requirements", [])
rtm_risks = entities.get("risks", [])
rtm_mandatory = entities.get("mandatory_items", [])
rtm_bid_sections = entities.get("bid_sections", [])
rtm_evidence = entities.get("evidence", [])
chain_links = rtm.get("chain_links", [])

mandatory_items = compliance.get("mandatory_items", compliance.get("rtm_entities", {}).get("mandatory_items", []))
eval_factors = evaluation.get("evaluation_factors", evaluation.get("factors", []))

# Load all bid section markdown files
bid_files = glob.glob(f"{folder}/outputs/bid-sections/*.md") + glob.glob(f"{folder}/outputs/Draft_Bid*.md")
bid_texts = {}
combined_bid_text = ""
for bf in bid_files:
    fname = os.path.basename(bf)
    text = read_file(bf)
    bid_texts[fname] = text
    combined_bid_text += text + "\n\n"

# Extract win themes from context bundle
win_themes = context_bundle.get("win_themes", context_bundle.get("strategic_themes", []))
if isinstance(win_themes, list) and win_themes and isinstance(win_themes[0], dict):
    theme_names = [t.get("theme", t.get("name", "")) for t in win_themes]
else:
    theme_names = win_themes if isinstance(win_themes, list) else []

findings = []
```

### Step 2: Execute Rule Checks

```python
# --- SVA7-THEME-THREADING-DEPTH (HIGH) ---
def check_theme_threading():
    """Each win theme appears in context with evidence, not just headings."""
    if not theme_names:
        return {
            "rule_id": "SVA7-THEME-THREADING-DEPTH",
            "rule_name": "Win Theme Semantic Threading",
            "category": "Content",
            "severity": "HIGH",
            "passed": True,
            "score": 100,
            "threshold": None,
            "details": {"note": "No win themes defined - skipping"},
            "corrective_action": None
        }

    theme_results = []
    total_bid_sections = len(bid_texts)

    # Define major bid sections for theme coverage check
    major_section_prefixes = ["01_SUBMITTAL", "02_MANAGEMENT", "03_TECHNICAL", "04a_SOLUTION"]
    major_bid_files = {
        fname: text for fname, text in bid_texts.items()
        if any(fname.upper().startswith(p) or p in fname.upper() for p in major_section_prefixes)
    }
    total_major_sections = max(len(major_bid_files), 1)

    # Load POSITIONING_OUTPUT.json for theme_eval_mapping
    positioning = read_json_safe(f"{folder}/shared/POSITIONING_OUTPUT.json")
    theme_eval_mapping = positioning.get("theme_eval_mapping", {}) if positioning else {}

    for theme in theme_names:
        if not theme:
            continue
        theme_lower = theme.lower()
        # Extract key phrases (3+ word segments)
        theme_words = [w for w in theme_lower.split() if len(w) > 2]

        sections_present = 0
        sections_with_depth = 0
        major_sections_present = 0

        for fname, text in bid_texts.items():
            text_lower = text.lower()

            # Check for theme presence: at least 2 theme keywords in this section
            keyword_hits = sum(1 for w in theme_words if w in text_lower)
            if keyword_hits < 2:
                continue
            sections_present += 1

            # Track presence in major bid sections specifically
            if fname in major_bid_files:
                major_sections_present += 1

            # Check for depth: theme keywords within 500 chars of evidence words
            evidence_markers = ["demonstrated", "proven", "delivered", "achieved",
                                "experience", "successfully", "track record",
                                "case study", "years of", "expertise in",
                                "example", "project", "client", "implemented"]

            # Find theme mentions and check nearby evidence
            for i, w in enumerate(theme_words):
                positions = [m.start() for m in re.finditer(re.escape(w), text_lower)]
                for pos in positions:
                    nearby_text = text_lower[max(0, pos-250):pos+250]
                    evidence_found = any(em in nearby_text for em in evidence_markers)
                    if evidence_found:
                        sections_with_depth += 1
                        break
                if sections_with_depth > 0:
                    break

        major_coverage_pct = (major_sections_present / total_major_sections * 100) if total_major_sections else 0
        depth = "deep" if sections_with_depth >= 2 else "moderate" if sections_with_depth >= 1 else "surface"
        theme_results.append({
            "theme": theme,
            "sections_present": sections_present,
            "sections_total": total_bid_sections,
            "major_sections_present": major_sections_present,
            "major_sections_total": total_major_sections,
            "major_coverage_pct": round(major_coverage_pct, 1),
            "sections_with_evidence": sections_with_depth,
            "depth": depth
        })

    # Check for eval factor callout boxes in management, technical, and solution sections
    eval_callout_pattern = r'>\s*\*?\*?Evaluation Factor\*?\*?:'
    eval_callout_files = {}
    eval_callout_missing = []
    for fname, text in bid_texts.items():
        fname_upper = fname.upper()
        if any(p in fname_upper for p in ["02_MANAGEMENT", "03_TECHNICAL", "04a_SOLUTION"]):
            callout_count = len(re.findall(eval_callout_pattern, text, re.IGNORECASE))
            eval_callout_files[fname] = callout_count
            if callout_count == 0:
                eval_callout_missing.append(fname)

    # Check theme-to-eval-factor linkage
    has_theme_eval_mapping = bool(theme_eval_mapping)

    # Score: themes should appear in 60%+ of sections with at least moderate depth
    themes_with_depth = sum(1 for t in theme_results if t["depth"] in ["moderate", "deep"])
    theme_score = (themes_with_depth / len(theme_results) * 100) if theme_results else 0

    # Check: themes appearing in >=50% of major sections
    themes_in_majority = sum(1 for t in theme_results if t["major_coverage_pct"] >= 50)
    themes_absent = [t["theme"] for t in theme_results if t["sections_present"] == 0]

    # Also check: average section presence
    avg_presence = (
        sum(t["sections_present"] for t in theme_results) / len(theme_results)
        if theme_results else 0
    )
    presence_ratio = avg_presence / total_bid_sections if total_bid_sections else 0

    overall_score = theme_score * 0.6 + min(100, presence_ratio * 100) * 0.4

    # BLOCK if any theme is completely absent from all bid sections
    has_absent_theme = len(themes_absent) > 0
    # Pass requires: good depth, reasonable presence, all themes in >=50% major sections, eval callouts present
    all_themes_in_majority = themes_in_majority == len(theme_results) if theme_results else True
    eval_callouts_present = len(eval_callout_missing) == 0

    passed = (theme_score >= 60 and presence_ratio >= 0.3
              and not has_absent_theme
              and all_themes_in_majority
              and eval_callouts_present)

    advisory_notes = []
    if not all_themes_in_majority and not has_absent_theme:
        weak_themes = [t["theme"] for t in theme_results if t["major_coverage_pct"] < 50 and t["sections_present"] > 0]
        if weak_themes:
            advisory_notes.append(f"Weak threading (<50% major sections): {', '.join(weak_themes[:3])}")
    if not eval_callouts_present:
        advisory_notes.append(f"Eval factor callout boxes missing in: {', '.join(eval_callout_missing)}")
    if not has_theme_eval_mapping:
        advisory_notes.append("POSITIONING_OUTPUT.json missing theme_eval_mapping linkage")

    return {
        "rule_id": "SVA7-THEME-THREADING-DEPTH",
        "rule_name": "Win Theme Semantic Threading",
        "category": "Content",
        "severity": "HIGH",
        "passed": passed,
        "score": round(overall_score, 1),
        "threshold": 60.0,
        "details": {
            "total_themes": len(theme_results),
            "themes_with_depth": themes_with_depth,
            "themes_in_majority_sections": themes_in_majority,
            "themes_absent_from_all_sections": themes_absent,
            "avg_section_presence": round(avg_presence, 1),
            "eval_callout_boxes": eval_callout_files,
            "eval_callout_missing": eval_callout_missing,
            "has_theme_eval_mapping": has_theme_eval_mapping,
            "advisory_notes": advisory_notes,
            "theme_analysis": theme_results
        },
        "corrective_action": {
            "type": "supplement_phase",
            "target_phase": "8",
            "auto_correctable": False,
            "instruction": (
                f"{'BLOCK: Themes completely absent from bid: ' + ', '.join(themes_absent) + '. ' if themes_absent else ''}"
                "Strengthen win theme threading: add evidence-backed theme statements to each bid section. "
                f"{'Add eval factor callout boxes (> **Evaluation Factor**:) to: ' + ', '.join(eval_callout_missing) + '. ' if eval_callout_missing else ''}"
                f"{'Ensure each theme appears in >=50% of major sections (01_SUBMITTAL, 02_MANAGEMENT, 03_TECHNICAL, 04a_SOLUTION_*). ' if not all_themes_in_majority else ''}"
            ).strip()
        } if not passed else None
    }

findings.append(check_theme_threading())


# --- SVA7-RISK-BID-INTEGRATION (CRITICAL) ---
def check_risk_bid_integration():
    """Every HIGH risk mitigation verified present in bid text."""
    high_critical_risks = [
        r for r in rtm_risks
        if r.get("severity") in ["HIGH", "CRITICAL"]
    ]

    if not high_critical_risks:
        return {
            "rule_id": "SVA7-RISK-BID-INTEGRATION",
            "rule_name": "Risk-to-Bid Traceability",
            "category": "Traceability",
            "severity": "CRITICAL",
            "passed": True,
            "score": 100,
            "threshold": None,
            "details": {"note": "No HIGH/CRITICAL risks to verify"},
            "corrective_action": None
        }

    verified = 0
    unverified = []

    for risk in high_critical_risks:
        risk_id = risk.get("risk_id", "?")
        mitigations = risk.get("mitigation_strategies", [])

        risk_addressed = False
        for mit in mitigations:
            mit_text = mit.get("description", mit.get("strategy", "")).lower()
            bid_loc = mit.get("bid_location", {})

            # Check 1: Does the RTM show bid_location populated?
            if bid_loc and bid_loc.get("section"):
                risk_addressed = True
                break

            # Check 2: Search for mitigation keywords in bid text
            if mit_text:
                key_phrases = [p.strip() for p in mit_text.split(",") if len(p.strip()) > 10]
                if not key_phrases:
                    key_phrases = [mit_text[:50]]

                for phrase in key_phrases[:3]:
                    # Extract meaningful words from phrase
                    words = [w for w in phrase.split() if len(w) > 3][:4]
                    if words and all(w in combined_bid_text.lower() for w in words):
                        risk_addressed = True
                        break
            if risk_addressed:
                break

        if risk_addressed:
            verified += 1
        else:
            unverified.append({
                "risk_id": risk_id,
                "severity": risk.get("severity"),
                "description": risk.get("description", "")[:80],
                "mitigation_count": len(mitigations)
            })

    coverage_pct = (verified / len(high_critical_risks) * 100) if high_critical_risks else 100
    passed = coverage_pct >= 80

    return {
        "rule_id": "SVA7-RISK-BID-INTEGRATION",
        "rule_name": "Risk-to-Bid Traceability",
        "category": "Traceability",
        "severity": "CRITICAL",
        "passed": passed,
        "score": round(coverage_pct, 1),
        "threshold": 80.0,
        "details": {
            "total_high_critical_risks": len(high_critical_risks),
            "verified_in_bid": verified,
            "coverage_pct": round(coverage_pct, 1),
            "unverified_risks": unverified[:10]
        },
        "corrective_action": {
            "type": "supplement_phase",
            "target_phase": "8",
            "auto_correctable": False,
            "instruction": f"Add risk mitigation content for {len(unverified)} unverified HIGH/CRITICAL risks to bid sections"
        } if not passed else None
    }

findings.append(check_risk_bid_integration())


# --- SVA7-COMPLIANCE-BID-COVERAGE (CRITICAL) ---
def check_compliance_bid_coverage():
    """Every mandatory item addressed in bid. Full chain verified."""
    total = len(rtm_mandatory)
    if total == 0:
        total = len(mandatory_items)

    items_to_check = rtm_mandatory if rtm_mandatory else mandatory_items
    if not items_to_check:
        return {
            "rule_id": "SVA7-COMPLIANCE-BID-COVERAGE",
            "rule_name": "Compliance-to-Bid Verification",
            "category": "Traceability",
            "severity": "CRITICAL",
            "passed": True,
            "score": 100,
            "threshold": None,
            "details": {"note": "No mandatory items to verify"},
            "corrective_action": None
        }

    addressed = 0
    not_addressed = []

    for m in items_to_check:
        m_id = m.get("mandatory_id", m.get("id", "?"))
        m_text = m.get("text", m.get("description", "")).lower()
        bid_loc = m.get("bid_location", {})

        found_in_bid = False

        # Check 1: RTM bid_location populated
        if bid_loc and bid_loc.get("section"):
            found_in_bid = True

        # Check 2: Search bid text for mandatory item reference
        if not found_in_bid:
            # Check for mandatory ID reference
            if m_id in combined_bid_text:
                found_in_bid = True

        # Check 3: Key phrase matching from mandatory item text
        if not found_in_bid and m_text:
            words = [w for w in m_text.split() if len(w) > 4][:5]
            if len(words) >= 3 and sum(1 for w in words if w in combined_bid_text.lower()) >= 3:
                found_in_bid = True

        if found_in_bid:
            addressed += 1
        else:
            not_addressed.append({
                "mandatory_id": m_id,
                "text_preview": m_text[:80],
                "priority": m.get("priority", "UNKNOWN")
            })

    coverage_pct = (addressed / len(items_to_check) * 100) if items_to_check else 100
    passed = coverage_pct >= 95

    return {
        "rule_id": "SVA7-COMPLIANCE-BID-COVERAGE",
        "rule_name": "Compliance-to-Bid Verification",
        "category": "Traceability",
        "severity": "CRITICAL",
        "passed": passed,
        "score": round(coverage_pct, 1),
        "threshold": 95.0,
        "details": {
            "total_mandatory_items": len(items_to_check),
            "addressed_in_bid": addressed,
            "coverage_pct": round(coverage_pct, 1),
            "not_addressed": not_addressed[:15]
        },
        "corrective_action": {
            "type": "supplement_phase",
            "target_phase": "8",
            "auto_correctable": False,
            "instruction": f"Add compliance coverage for {len(not_addressed)} mandatory items missing from bid"
        } if not passed else None
    }

findings.append(check_compliance_bid_coverage())


# --- SVA7-CROSS-DOC-ID-INTEGRITY (HIGH) ---
def check_cross_doc_ids():
    """All IDs referenced in bid sections exist in source JSON. No phantom references."""
    # Build valid ID sets from RTM
    valid_req_ids = set(r.get("requirement_id", "") for r in rtm_reqs)
    valid_risk_ids = set(r.get("risk_id", "") for r in rtm_risks)
    valid_spec_ids = set(s.get("spec_id", "") for s in entities.get("specifications", []))
    valid_mandatory_ids = set(m.get("mandatory_id", "") for m in rtm_mandatory)

    # Common ID patterns in bid text
    phantom_refs = []
    id_patterns = [
        (r'\b(\d{3}[A-Z]{3})\b', valid_req_ids, "requirement"),          # 001ENR format
        (r'\b(RISK-\d{3})\b', valid_risk_ids, "risk"),                     # RISK-001 format
        (r'\b(M\d{3})\b', valid_mandatory_ids, "mandatory"),               # M001 format
        (r'\b(SPEC-[A-Z]+-[A-Z]+-\d{2})\b', valid_spec_ids, "spec"),     # SPEC-ARCH-SEC-01 format
    ]

    total_refs = 0
    valid_refs = 0

    for pattern, valid_set, id_type in id_patterns:
        matches = re.findall(pattern, combined_bid_text)
        for match in matches:
            total_refs += 1
            if match in valid_set:
                valid_refs += 1
            else:
                phantom_refs.append({
                    "id": match,
                    "type": id_type,
                    "context": "Found in bid text but not in source data"
                })

    integrity_pct = (valid_refs / total_refs * 100) if total_refs else 100
    passed = integrity_pct >= 95 or len(phantom_refs) <= 3

    return {
        "rule_id": "SVA7-CROSS-DOC-ID-INTEGRITY",
        "rule_name": "Cross-Document ID Integrity",
        "category": "Consistency",
        "severity": "HIGH",
        "passed": passed,
        "score": round(integrity_pct, 1),
        "threshold": 95.0,
        "details": {
            "total_id_references": total_refs,
            "valid_references": valid_refs,
            "integrity_pct": round(integrity_pct, 1),
            "phantom_references": phantom_refs[:10],
            "valid_id_counts": {
                "requirements": len(valid_req_ids),
                "risks": len(valid_risk_ids),
                "specifications": len(valid_spec_ids),
                "mandatory": len(valid_mandatory_ids)
            }
        },
        "corrective_action": {
            "type": "user_review",
            "target_phase": None,
            "auto_correctable": False,
            "instruction": f"Found {len(phantom_refs)} phantom ID references in bid text that don't exist in source data"
        } if not passed else None
    }

findings.append(check_cross_doc_ids())


# --- SVA7-PERSONA-SATISFACTION (HIGH) ---
def check_persona_satisfaction():
    """Each evaluator persona's top 3 concerns addressed in bid."""
    if not personas:
        return {
            "rule_id": "SVA7-PERSONA-SATISFACTION",
            "rule_name": "Evaluator Persona Satisfaction",
            "category": "Content",
            "severity": "HIGH",
            "passed": True,
            "score": 80,
            "threshold": None,
            "details": {"note": "No persona coverage data available - advisory score assigned"},
            "corrective_action": None
        }

    persona_list = personas.get("personas", personas.get("evaluator_personas", []))
    overall_persona_score = personas.get("overall_score", 0)

    persona_results = []
    for persona in persona_list:
        name = persona.get("name", persona.get("role", "Unknown"))
        concerns = persona.get("top_concerns", persona.get("priorities", []))
        coverage = persona.get("coverage_score", 0)

        # Check top 3 concerns in bid text
        concerns_addressed = 0
        concern_details = []
        for concern in concerns[:3]:
            concern_text = concern if isinstance(concern, str) else concern.get("concern", concern.get("text", ""))
            concern_lower = concern_text.lower()
            words = [w for w in concern_lower.split() if len(w) > 3][:4]

            found = False
            if words:
                found = sum(1 for w in words if w in combined_bid_text.lower()) >= len(words) * 0.6

            if found:
                concerns_addressed += 1
            concern_details.append({
                "concern": concern_text[:60],
                "addressed": found
            })

        persona_results.append({
            "persona": name,
            "coverage_score": coverage,
            "concerns_checked": len(concern_details),
            "concerns_addressed": concerns_addressed,
            "concern_details": concern_details
        })

    # Score: average concern coverage across personas
    avg_concern_coverage = 0
    if persona_results:
        ratios = [
            p["concerns_addressed"] / max(p["concerns_checked"], 1)
            for p in persona_results
        ]
        avg_concern_coverage = sum(ratios) / len(ratios) * 100

    score = avg_concern_coverage * 0.6 + overall_persona_score * 0.4
    passed = score >= 70 and overall_persona_score >= 75

    return {
        "rule_id": "SVA7-PERSONA-SATISFACTION",
        "rule_name": "Evaluator Persona Satisfaction",
        "category": "Content",
        "severity": "HIGH",
        "passed": passed,
        "score": round(score, 1),
        "threshold": 70.0,
        "details": {
            "overall_persona_score": overall_persona_score,
            "avg_concern_coverage_pct": round(avg_concern_coverage, 1),
            "persona_results": persona_results
        },
        "corrective_action": {
            "type": "user_review",
            "target_phase": None,
            "auto_correctable": False,
            "instruction": "Review persona coverage gaps and strengthen bid content addressing evaluator priorities"
        } if not passed else None
    }

findings.append(check_persona_satisfaction())


# --- SVA7-FORMAT-COMPLIANCE (HIGH) ---
def check_format_compliance():
    """Page count estimate within limits. Submission structure requirements met."""
    # Estimate page count: ~3000 chars per page (standard proposal formatting)
    total_chars = len(combined_bid_text)
    estimated_pages = total_chars / 3000

    # Check against submission structure limits
    page_limit = None
    format_issues = []

    if submission_structure:
        volumes = submission_structure.get("volumes", submission_structure.get("required_files", []))
        total_page_limit = 0
        for vol in volumes:
            limit = vol.get("page_limit", 0)
            if limit:
                total_page_limit += limit
        if total_page_limit > 0:
            page_limit = total_page_limit

        # Check required file structure
        required_files = [v.get("filename", v.get("name", "")) for v in volumes if v.get("required", True)]
        for rf in required_files:
            # Check if a matching bid section exists
            rf_base = rf.replace(".pdf", "").replace(".PDF", "").lower()
            found = any(rf_base in fname.lower() for fname in bid_texts.keys())
            if not found:
                format_issues.append(f"Required file '{rf}' has no matching bid section")

    within_limit = True
    if page_limit and estimated_pages > page_limit:
        within_limit = False
        format_issues.append(f"Estimated {estimated_pages:.0f} pages exceeds limit of {page_limit}")

    # Check bid section count (should have at least 3 sections for a valid bid)
    if len(bid_texts) < 3:
        format_issues.append(f"Only {len(bid_texts)} bid sections found (expected 3+)")

    score = 100 - len(format_issues) * 20
    passed = len(format_issues) <= 1

    return {
        "rule_id": "SVA7-FORMAT-COMPLIANCE",
        "rule_name": "Format Compliance",
        "category": "Consistency",
        "severity": "HIGH",
        "passed": passed,
        "score": round(max(0, score), 1),
        "threshold": None,
        "details": {
            "total_bid_chars": total_chars,
            "estimated_pages": round(estimated_pages, 0),
            "page_limit": page_limit,
            "within_page_limit": within_limit,
            "bid_section_count": len(bid_texts),
            "format_issues": format_issues,
            "submission_structure_available": submission_structure is not None
        },
        "corrective_action": {
            "type": "user_review",
            "target_phase": None,
            "auto_correctable": False,
            "instruction": f"Format issues detected: {'; '.join(format_issues[:3])}"
        } if not passed else None
    }

findings.append(check_format_compliance())


# --- SVA7-CONSISTENCY-CHECK (MEDIUM) ---
def check_statistic_consistency():
    """Numbers cited in bid match source data."""
    inconsistencies = []

    # Check requirement count mentions
    actual_req_count = len(rtm_reqs)
    req_count_pattern = r'(\d+)\s+(?:requirements?|reqs?)\b'
    for match in re.finditer(req_count_pattern, combined_bid_text, re.IGNORECASE):
        cited_count = int(match.group(1))
        if cited_count > 10 and abs(cited_count - actual_req_count) > actual_req_count * 0.15:
            inconsistencies.append({
                "type": "requirement_count",
                "cited": cited_count,
                "actual": actual_req_count,
                "context": combined_bid_text[max(0,match.start()-30):match.end()+30][:80]
            })

    # Check risk count mentions
    actual_risk_count = len(rtm_risks)
    risk_count_pattern = r'(\d+)\s+(?:risks?|risk items?)\b'
    for match in re.finditer(risk_count_pattern, combined_bid_text, re.IGNORECASE):
        cited_count = int(match.group(1))
        if cited_count > 3 and abs(cited_count - actual_risk_count) > actual_risk_count * 0.20:
            inconsistencies.append({
                "type": "risk_count",
                "cited": cited_count,
                "actual": actual_risk_count,
                "context": combined_bid_text[max(0,match.start()-30):match.end()+30][:80]
            })

    # Check coverage percentage mentions
    pct_pattern = r'(\d{2,3})%\s+(?:coverage|compliance|traceability|requirements?\s+coverage)'
    for match in re.finditer(pct_pattern, combined_bid_text, re.IGNORECASE):
        cited_pct = int(match.group(1))
        if cited_pct > 100:
            inconsistencies.append({
                "type": "percentage_over_100",
                "cited": f"{cited_pct}%",
                "context": combined_bid_text[max(0,match.start()-30):match.end()+30][:80]
            })

    score = max(0, 100 - len(inconsistencies) * 15)
    passed = len(inconsistencies) <= 2

    return {
        "rule_id": "SVA7-CONSISTENCY-CHECK",
        "rule_name": "Statistic Consistency",
        "category": "Consistency",
        "severity": "MEDIUM",
        "passed": passed,
        "score": round(score, 1),
        "threshold": None,
        "details": {
            "inconsistencies_found": len(inconsistencies),
            "inconsistencies": inconsistencies[:10],
            "source_counts": {
                "requirements": actual_req_count,
                "risks": actual_risk_count,
                "mandatory_items": len(rtm_mandatory),
                "specifications": len(entities.get("specifications", []))
            }
        },
        "corrective_action": None
    }

findings.append(check_statistic_consistency())


# --- SVA7-COMPETITIVE-CONTRAST (MEDIUM) ---
def check_competitive_contrast():
    """Solution includes competitive positioning. Client intelligence referenced."""
    contrast_markers = [
        "unlike", "differentiate", "advantage", "unique", "proprietary",
        "competitive", "distinguish", "superior", "proven track record",
        "other vendors", "alternative", "our approach", "we uniquely",
        "sets us apart", "compared to", "while others"
    ]

    # Count contrast language in bid
    contrast_count = 0
    for marker in contrast_markers:
        contrast_count += combined_bid_text.lower().count(marker)

    # Check if client intelligence insights appear
    intel_referenced = False
    if client_intel:
        incumbent = client_intel.get("incumbent", client_intel.get("current_vendor", ""))
        pain_points = client_intel.get("pain_points", client_intel.get("challenges", []))

        if incumbent and incumbent.lower() in combined_bid_text.lower():
            intel_referenced = True
        for pp in pain_points[:5]:
            pp_text = pp if isinstance(pp, str) else pp.get("text", "")
            words = [w for w in pp_text.lower().split() if len(w) > 4][:3]
            if words and all(w in combined_bid_text.lower() for w in words):
                intel_referenced = True
                break

    # Score: contrast language should appear regularly
    # ~1 contrast per 2 pages = reasonable
    estimated_pages = max(1, len(combined_bid_text) / 3000)
    contrast_density = contrast_count / estimated_pages

    score = min(100, contrast_density * 25 + (25 if intel_referenced else 0))
    passed = contrast_count >= 5

    return {
        "rule_id": "SVA7-COMPETITIVE-CONTRAST",
        "rule_name": "Competitive Positioning Presence",
        "category": "Content",
        "severity": "MEDIUM",
        "passed": passed,
        "score": round(score, 1),
        "threshold": None,
        "details": {
            "contrast_statements": contrast_count,
            "contrast_per_page": round(contrast_density, 1),
            "client_intelligence_referenced": intel_referenced,
            "client_intel_available": client_intel is not None
        },
        "corrective_action": {
            "type": "supplement_phase",
            "target_phase": "8",
            "auto_correctable": False,
            "instruction": "Add competitive differentiation language and client intelligence references to bid sections"
        } if not passed else None
    }

findings.append(check_competitive_contrast())


# --- SVA7-CASE-STUDY-VALIDATION (HIGH) ---
def check_case_study_presence():
    """Verify real case studies are embedded in bid sections (not placeholders)."""
    # Check for forbidden placeholder markers
    placeholder_count = combined_bid_text.count("[CASE STUDY PLACEHOLDER")

    # Check for real case study indicators
    # Look for structured case study content: "Case Study:" headers,
    # metrics tables with "Metric" and "Achievement", and relevance statements
    case_study_headers = len(re.findall(r'\*?\*?Case Study:?\*?\*?', combined_bid_text, re.IGNORECASE))

    # Look for metrics tables (pipe-delimited with Metric/Achievement or similar headers)
    metrics_tables = len(re.findall(
        r'\|\s*Metric\s*\|.*\|', combined_bid_text, re.IGNORECASE
    ))

    # Look for "Relevance to This RFP" statements
    relevance_statements = len(re.findall(
        r'Relevance to This RFP', combined_bid_text, re.IGNORECASE
    ))

    # Look for "Proven Capability" callout boxes
    proven_capability_callouts = len(re.findall(
        r'>\s*\*?\*?Proven Capability\*?\*?:', combined_bid_text, re.IGNORECASE
    ))

    # Look for real client names (not placeholder patterns)
    # Real case studies reference actual clients, not "[CLIENT NAME]" or "[USER INPUT REQUIRED]"
    client_reference_pattern = r'(?:Client|client):\s*(?!\[)[A-Z][A-Za-z\s&,.]+'
    real_client_refs = len(re.findall(client_reference_pattern, combined_bid_text))

    # Score calculation
    total_case_studies = case_study_headers + proven_capability_callouts
    has_minimum_case_studies = total_case_studies >= 3
    has_metrics = metrics_tables >= 2
    has_relevance = relevance_statements >= 2
    no_placeholders = placeholder_count == 0

    score_components = {
        "no_placeholders": 30 if no_placeholders else 0,
        "case_study_count": min(30, total_case_studies * 10),
        "metrics_tables": min(20, metrics_tables * 10),
        "relevance_statements": min(10, relevance_statements * 5),
        "proven_capability_callouts": min(10, proven_capability_callouts * 5)
    }
    score = sum(score_components.values())

    passed = has_minimum_case_studies and no_placeholders

    return {
        "rule_id": "SVA7-CASE-STUDY-VALIDATION",
        "rule_name": "Past Performance Case Study Validation",
        "category": "Content",
        "severity": "HIGH",
        "passed": passed,
        "score": round(score, 1),
        "threshold": 60.0,
        "details": {
            "placeholder_markers_found": placeholder_count,
            "case_study_headers": case_study_headers,
            "proven_capability_callouts": proven_capability_callouts,
            "total_case_studies": total_case_studies,
            "metrics_tables": metrics_tables,
            "relevance_statements": relevance_statements,
            "real_client_references": real_client_refs,
            "score_breakdown": score_components
        },
        "corrective_action": {
            "type": "supplement_phase",
            "target_phase": "8",
            "auto_correctable": False,
            "instruction": (
                f"{'Found ' + str(placeholder_count) + ' [CASE STUDY PLACEHOLDER] markers — must be replaced with real case studies from Past_Projects.md. ' if placeholder_count > 0 else ''}"
                f"{'Only ' + str(total_case_studies) + ' case studies found (need 3+). Re-run phases 8.1-8.3 with POSITIONING_OUTPUT.json containing matched_projects[]. ' if not has_minimum_case_studies else ''}"
                f"{'Add metrics tables to case studies. ' if not has_metrics else ''}"
                f"{'Add Relevance to This RFP statements. ' if not has_relevance else ''}"
            ).strip()
        } if not passed else None
    }

findings.append(check_case_study_presence())


# --- SVA7-TECH-LIFECYCLE-VALIDATION (CRITICAL) ---
def check_tech_lifecycle():
    """Verify all proposed technologies have sufficient lifecycle/EOL coverage.

    CRITICAL: Government proposals that recommend technology expiring mid-contract
    are disqualifying failures. Every technology in the tech stack table MUST have
    its EOL date verified AND that date must extend beyond the contract period.

    This rule:
    1. Finds the technology stack table in 03_TECHNICAL.md
    2. Extracts version numbers and technology names
    3. Checks for EOL Date column (REQUIRED — fail if missing)
    4. Verifies no technology expires within 3 years of proposal date
    5. Flags any technology reaching EOL during the project lifecycle
    """
    import re
    from datetime import datetime

    tech_section = ""
    for bf in bid_files:
        if "03_TECHNICAL" in bf:
            tech_section = read_file(bf)
            break

    if not tech_section:
        return {
            "rule_id": "SVA7-TECH-LIFECYCLE-VALIDATION",
            "rule_name": "Technology Stack Lifecycle Validation",
            "category": "Technical Credibility",
            "severity": "CRITICAL",
            "passed": False,
            "score": 0,
            "threshold": 80.0,
            "details": {"note": "03_TECHNICAL.md not found"},
            "corrective_action": {
                "type": "rerun_phase",
                "target_phase": "8.3",
                "auto_correctable": False,
                "instruction": "Technical approach document missing. Re-run Phase 8.3."
            }
        }

    # Check for EOL Date column in technology stack table
    has_eol_column = bool(re.search(r'EOL\s*(?:Date)?', tech_section, re.IGNORECASE))

    # Look for known short-lifecycle technologies
    current_year = datetime.now().year
    min_acceptable_eol = current_year + 3

    # Known problematic versions (web search these to keep current)
    short_lifecycle_flags = []

    # Check for .NET versions with known short EOL
    dotnet_matches = re.findall(r'\.NET\s+(\d+)(?:\.0)?(?:\s+LTS)?', tech_section)
    for ver in dotnet_matches:
        ver_num = int(ver)
        # .NET even numbers are LTS (3 yr), odd are STS (18 mo)
        if ver_num % 2 == 1:  # STS version — 18 month support
            short_lifecycle_flags.append(f".NET {ver_num} is STS (18-month support) — use next LTS version")
        elif ver_num == 8:  # .NET 8 LTS EOL Nov 2026
            short_lifecycle_flags.append(f".NET 8.0 LTS EOL is November 2026 — expires mid-contract for multi-year projects starting 2026+")
        elif ver_num == 6:  # .NET 6 LTS EOL Nov 2024 — already expired
            short_lifecycle_flags.append(f".NET 6.0 LTS is ALREADY END-OF-LIFE (Nov 2024)")

    # Check for Node.js odd versions (non-LTS)
    node_matches = re.findall(r'Node(?:\.js)?\s+(\d+)', tech_section)
    for ver in node_matches:
        if int(ver) % 2 == 1:
            short_lifecycle_flags.append(f"Node.js {ver} is non-LTS (no long-term support)")

    # Check for "ASP.NET Core 8" specifically
    if re.search(r'ASP\.NET\s+Core\s+8', tech_section):
        if "ASP.NET Core 8" not in str(short_lifecycle_flags):
            short_lifecycle_flags.append("ASP.NET Core 8.0 LTS EOL November 2026 — too short for multi-year government contracts")

    # Score
    score_components = {
        "has_eol_column": 40 if has_eol_column else 0,
        "no_short_lifecycle_flags": 60 if len(short_lifecycle_flags) == 0 else max(0, 60 - len(short_lifecycle_flags) * 20)
    }
    score = sum(score_components.values())
    passed = has_eol_column and len(short_lifecycle_flags) == 0

    result = {
        "rule_id": "SVA7-TECH-LIFECYCLE-VALIDATION",
        "rule_name": "Technology Stack Lifecycle Validation",
        "category": "Technical Credibility",
        "severity": "CRITICAL",
        "passed": passed,
        "score": round(score, 1),
        "threshold": 80.0,
        "details": {
            "has_eol_column_in_tech_table": has_eol_column,
            "short_lifecycle_flags": short_lifecycle_flags,
            "flag_count": len(short_lifecycle_flags),
            "score_breakdown": score_components,
            "guidance": "ALL proposed technologies must have active support through contract period + 2 years"
        }
    }

    if not passed:
        result["corrective_action"] = {
            "type": "rerun_phase",
            "target_phase": "8.3",
            "auto_correctable": False,
            "instruction": (
                f"{'Tech stack table MISSING EOL Date column — add EOL dates for every technology. ' if not has_eol_column else ''}"
                f"{'Lifecycle issues: ' + '; '.join(short_lifecycle_flags) + '. ' if short_lifecycle_flags else ''}"
                "Re-run Phase 8.3 with web-search-verified LTS versions that have 3+ years remaining support."
            )
        }

    return result

findings.append(check_tech_lifecycle())


# --- SVA7-FINANCIAL-SANITY (HIGH) ---
def check_financial_sanity():
    """Verify financial proposal has valid rates, costs align with effort estimation, and markup is reasonable."""
    financial_text = ""
    for bf in bid_files:
        if "05_FINANCIAL" in bf:
            financial_text = read_file(bf)
            break

    if not financial_text:
        return {
            "rule_id": "SVA7-FINANCIAL-SANITY",
            "rule_name": "Financial Proposal Integrity",
            "category": "Financial Validation",
            "severity": "HIGH",
            "passed": False,
            "score": 0,
            "threshold": 70.0,
            "details": {"note": "05_FINANCIAL.md not found"},
            "corrective_action": {
                "type": "rerun_phase",
                "target_phase": "5",
                "auto_correctable": False,
                "instruction": "Financial proposal document missing. Re-run Phase 5 (Financial)."
            }
        }

    # Load effort estimation data
    effort_data = read_json_safe(f"{folder}/shared/EFFORT_ESTIMATION.md")
    effort_json = read_json_safe(f"{folder}/shared/EFFORT_ESTIMATION.json")

    issues = []
    block_conditions = []
    advisory_conditions = []

    # 1. Check for zero-rate roles: look for rate table rows with $0
    zero_rate_matches = re.findall(r'\$\s*0(?:\.00)?\s*(?:/hr|per\s*hour|hourly)?', financial_text)
    if zero_rate_matches:
        block_conditions.append(f"Found {len(zero_rate_matches)} zero-rate ($0) entries")

    # 2. Check total cost > $0
    cost_matches = re.findall(r'\$\s*([\d,]+(?:\.\d{2})?)', financial_text)
    costs = []
    for c in cost_matches:
        try:
            costs.append(float(c.replace(",", "")))
        except ValueError:
            pass
    total_cost_found = max(costs) if costs else 0
    if total_cost_found == 0:
        block_conditions.append("Total cost appears to be $0 or no costs found")

    # 3. Check labor rates > $0 (look for rate patterns)
    rate_pattern = r'\$\s*([\d,]+(?:\.\d{2})?)\s*(?:/hr|per\s*hour|hourly)'
    rate_matches = re.findall(rate_pattern, financial_text, re.IGNORECASE)
    rates = []
    for r in rate_matches:
        try:
            rates.append(float(r.replace(",", "")))
        except ValueError:
            pass
    all_rates_positive = all(r > 0 for r in rates) if rates else True

    # 4. Check overhead rate (5-35% range)
    overhead_pattern = r'(?:overhead|indirect|G&A)\s*(?:rate)?[:\s]*(\d+(?:\.\d+)?)\s*%'
    overhead_matches = re.findall(overhead_pattern, financial_text, re.IGNORECASE)
    overhead_in_range = True
    for oh in overhead_matches:
        oh_val = float(oh)
        if oh_val < 5 or oh_val > 35:
            advisory_conditions.append(f"Overhead rate {oh_val}% outside 5-35% range")
            overhead_in_range = False

    # 5. Check profit margin (10-40% range for government IT contracts per GSA guidelines)
    profit_pattern = r'(?:profit|margin|fee)\s*(?:rate|margin)?[:\s]*(\d+(?:\.\d+)?)\s*%'
    profit_matches = re.findall(profit_pattern, financial_text, re.IGNORECASE)
    profit_in_range = True
    for pm in profit_matches:
        pm_val = float(pm)
        if pm_val < 10 or pm_val > 40:
            advisory_conditions.append(f"Profit margin {pm_val}% outside 10-40% range (GSA IT guideline)")
            profit_in_range = False

    # 6. Check rate source attribution
    has_company_rate = bool(re.search(r'\[COMPANY\s*RATE\]', financial_text, re.IGNORECASE))
    has_market_default = bool(re.search(r'\[MARKET\s*DEFAULT\]', financial_text, re.IGNORECASE))
    rate_source_attributed = has_company_rate or has_market_default
    if not rate_source_attributed:
        advisory_conditions.append("Rate source attribution missing ([COMPANY RATE] or [MARKET DEFAULT] tags)")

    # 7. Check payment schedule section
    has_payment_schedule = bool(re.search(
        r'(?:payment\s+schedule|milestone\s+payments?|billing\s+schedule|invoicing\s+schedule)',
        financial_text, re.IGNORECASE
    ))
    if not has_payment_schedule:
        advisory_conditions.append("Payment schedule section with milestone dates not found")

    # 8. Cross-check hours with effort estimation
    hours_in_financial = re.findall(r'([\d,]+)\s*(?:hours?|hrs?)\b', financial_text)
    financial_hours = []
    for h in hours_in_financial:
        try:
            financial_hours.append(float(h.replace(",", "")))
        except ValueError:
            pass
    max_financial_hours = max(financial_hours) if financial_hours else 0

    effort_hours_match = False
    effort_mismatch_pct = 0
    if effort_json:
        effort_total_hours = effort_json.get("total_hours", effort_json.get("totalHours", 0))
        if effort_total_hours > 0 and max_financial_hours > 0:
            effort_mismatch_pct = abs(max_financial_hours - effort_total_hours) / effort_total_hours * 100
            if effort_mismatch_pct > 25:
                block_conditions.append(
                    f"Effort-to-cost mismatch: financial={max_financial_hours:.0f}hrs vs effort={effort_total_hours:.0f}hrs ({effort_mismatch_pct:.0f}% difference, >25% threshold)"
                )
            elif effort_mismatch_pct <= 10:
                effort_hours_match = True

    # Score calculation
    score_components = {
        "no_zero_rates": 20 if not zero_rate_matches else 0,
        "total_cost_positive": 15 if total_cost_found > 0 else 0,
        "markup_in_range": 15 if (overhead_in_range and profit_in_range) else 5,
        "rate_attribution": 10 if rate_source_attributed else 0,
        "payment_schedule": 10 if has_payment_schedule else 0,
        "effort_alignment": 20 if effort_hours_match else (10 if effort_mismatch_pct <= 25 else 0),
        "rates_found": 10 if rates else 0
    }
    score = sum(score_components.values())

    has_block = len(block_conditions) > 0
    passed = not has_block and score >= 70

    result = {
        "rule_id": "SVA7-FINANCIAL-SANITY",
        "rule_name": "Financial Proposal Integrity",
        "category": "Financial Validation",
        "severity": "HIGH",
        "passed": passed,
        "score": round(score, 1),
        "threshold": 70.0,
        "details": {
            "total_cost_found": total_cost_found,
            "labor_rates_found": len(rates),
            "all_rates_positive": all_rates_positive,
            "zero_rate_entries": len(zero_rate_matches),
            "overhead_in_range": overhead_in_range,
            "profit_in_range": profit_in_range,
            "rate_source_attributed": rate_source_attributed,
            "has_payment_schedule": has_payment_schedule,
            "effort_mismatch_pct": round(effort_mismatch_pct, 1),
            "max_financial_hours": max_financial_hours,
            "block_conditions": block_conditions,
            "advisory_conditions": advisory_conditions,
            "score_breakdown": score_components
        }
    }

    if not passed:
        result["corrective_action"] = {
            "type": "rerun_phase" if has_block else "user_review",
            "target_phase": "5",
            "auto_correctable": False,
            "instruction": (
                f"{'BLOCK: ' + '; '.join(block_conditions) + '. ' if block_conditions else ''}"
                f"{'Advisory: ' + '; '.join(advisory_conditions) + '. ' if advisory_conditions else ''}"
                "Review and correct financial proposal. Rate decisions require human input."
            )
        }

    return result

findings.append(check_financial_sanity())


# --- SVA7-PROOF-POINT-DENSITY (MEDIUM) ---
def check_proof_point_density():
    """Verify that bid sections contain substantive evidence, not just claims."""

    proof_patterns = [
        r'\b\d+[%+]',  # Quantified metrics (e.g., "95%", "200+") — word boundary avoids partial matches
        r'\b\d+\s+years?\b',  # Experience duration (e.g., "39 years")
        r'\b(?:ISO|SOC|CMMI|FedRAMP|FISMA)\s*\d*',  # Certifications
        r'\b(?:proven|demonstrated|track record|successfully delivered)\b',  # Evidence language
        r'\b(?:case study|past performance|reference)\b',  # Past performance citations
    ]

    major_sections = ["01_SUBMITTAL", "02_MANAGEMENT", "03_TECHNICAL", "04_SOLUTION", "05_FINANCIAL"]
    sections_with_proofs = 0
    section_details = []

    for section_key in major_sections:
        # Find matching bid text (prefix match against filenames)
        section_text = ""
        for fname, text in bid_texts.items():
            if section_key in fname.upper():
                section_text += text + "\n"

        proof_count = 0
        for pattern in proof_patterns:
            proof_count += len(re.findall(pattern, section_text, re.IGNORECASE))

        has_proof = proof_count >= 1
        if has_proof:
            sections_with_proofs += 1

        section_details.append({
            "section": section_key,
            "proof_points_found": proof_count,
            "has_evidence": has_proof
        })

    proof_density_pct = (sections_with_proofs / len(major_sections)) * 100 if major_sections else 0
    rule_score = min(100, proof_density_pct)

    advisory_notes = []
    if proof_density_pct < 60:
        advisory_notes.append(
            f"Low proof point density: only {sections_with_proofs}/{len(major_sections)} "
            f"sections contain evidence ({proof_density_pct:.0f}%)"
        )

    passed = proof_density_pct >= 60

    return {
        "rule_id": "SVA7-PROOF-POINT-DENSITY",
        "rule_name": "Proof Point Density",
        "category": "Content",
        "severity": "MEDIUM",
        "passed": passed,
        "score": round(rule_score, 1),
        "threshold": 60.0,
        "details": {
            "sections_checked": len(major_sections),
            "sections_with_evidence": sections_with_proofs,
            "proof_density_pct": round(proof_density_pct, 1),
            "section_details": section_details,
            "advisory_notes": advisory_notes
        },
        "corrective_action": {
            "type": "supplement_phase",
            "target_phase": "8",
            "auto_correctable": False,
            "instruction": (
                f"Low proof point density: {sections_with_proofs}/{len(major_sections)} sections "
                f"contain quantified evidence. Add metrics, certifications, or past performance "
                f"citations to sections lacking proof points. Use matched_evidence from "
                f"POSITIONING_OUTPUT.json to populate."
            )
        } if not passed else None
    }

findings.append(check_proof_point_density())
```

### Step 3: Calculate Disposition

```python
def calculate_disposition(findings):
    has_critical_fail = any(f["severity"] == "CRITICAL" and not f["passed"] for f in findings)
    has_high_fail = any(f["severity"] == "HIGH" and not f["passed"] for f in findings)

    if has_critical_fail:
        return "BLOCK"
    elif has_high_fail:
        return "ADVISORY"
    else:
        return "PASS"

disposition = calculate_disposition(findings)
passed_count = sum(1 for f in findings if f["passed"])
failed_count = len(findings) - passed_count
critical_failures = sum(1 for f in findings if f["severity"] == "CRITICAL" and not f["passed"])
high_failures = sum(1 for f in findings if f["severity"] == "HIGH" and not f["passed"])
overall_score = sum(f["score"] for f in findings) / len(findings) if findings else 0

# Enhancement: Lohfeld 7 Quality Measures Summary
def build_lohfeld_summary(findings):
    """Map SVA-7 findings to Lohfeld's 7 Quality Measures for winning proposals.
    See docs/process-gold-standard.md Section 7 for full mapping rationale."""
    lohfeld_mapping = {
        "Compliant": {"rules": ["SVA7-COMPLIANCE-BID-COVERAGE"], "desc": "Addresses all requirements"},
        "Responsive": {"rules": ["SVA7-RISK-BID-INTEGRATION"], "desc": "Answers the question asked"},
        "Understandable": {"rules": ["SVA7-FORMAT-COMPLIANCE"], "desc": "Clear, well-organized"},
        "Credible": {"rules": ["SVA7-CONSISTENCY-CHECK", "SVA7-CASE-STUDY-VALIDATION", "SVA7-PROOF-POINT-DENSITY"], "desc": "Claims supported by evidence"},
        "Has Strengths": {"rules": ["SVA7-THEME-THREADING-DEPTH"], "desc": "Contains discriminating strengths"},
        "Low Risk": {"rules": ["SVA7-RISK-BID-INTEGRATION", "SVA7-TECH-LIFECYCLE-VALIDATION"], "desc": "Risk awareness and mitigation"},
        "Winning": {"rules": ["SVA7-COMPETITIVE-CONTRAST", "SVA7-PERSONA-SATISFACTION"], "desc": "Overall winning impression"}
    }
    findings_by_id = {f["rule_id"]: f for f in findings}
    results = []
    for measure, cfg in lohfeld_mapping.items():
        mapped = [findings_by_id[r] for r in cfg["rules"] if r in findings_by_id]
        avg_score = sum(f["score"] for f in mapped) / len(mapped) if mapped else 0
        all_passed = all(f["passed"] for f in mapped) if mapped else False
        results.append({
            "measure": measure, "description": cfg["desc"],
            "status": "PASS" if all_passed else "NEEDS ATTENTION",
            "score": round(avg_score, 1), "mapped_rules": cfg["rules"]
        })
    return results

lohfeld_summary = build_lohfeld_summary(findings)
```

### Step 4: Build Gold Team Report

```python
# Evaluator simulation using actual evaluation criteria
evaluator_sim = {"factors": [], "weighted_total": 0, "max_possible": 0, "percentage": 0}
total_max = 0
total_estimated = 0

for factor in eval_factors:
    factor_name = factor.get("factor_name", factor.get("name", "Unknown"))
    max_points = factor.get("points", factor.get("max_points", factor.get("weight", 100)))

    # At Gold Team stage, estimate score based on:
    # - Compliance coverage (30%)
    # - Theme threading (20%)
    # - Risk mitigation presence (20%)
    # - Persona satisfaction (15%)
    # - Format compliance (15%)
    compliance_score = findings[2]["score"] / 100 if len(findings) > 2 else 0.8
    theme_score = findings[0]["score"] / 100 if len(findings) > 0 else 0.7
    risk_score = findings[1]["score"] / 100 if len(findings) > 1 else 0.7
    persona_score = findings[4]["score"] / 100 if len(findings) > 4 else 0.7
    format_score = findings[5]["score"] / 100 if len(findings) > 5 else 0.8

    estimated_pct = (
        compliance_score * 0.30 +
        theme_score * 0.20 +
        risk_score * 0.20 +
        persona_score * 0.15 +
        format_score * 0.15
    )
    estimated_score = max_points * estimated_pct

    evaluator_sim["factors"].append({
        "factor_name": factor_name,
        "max_points": max_points,
        "estimated_score": round(estimated_score, 1),
        "score_percentage": round(estimated_pct * 100, 1),
        "notes": f"Compliance: {compliance_score*100:.0f}%, Themes: {theme_score*100:.0f}%, Risk: {risk_score*100:.0f}%"
    })

    total_max += max_points
    total_estimated += estimated_score

evaluator_sim["weighted_total"] = round(total_estimated, 1)
evaluator_sim["max_possible"] = total_max
evaluator_sim["percentage"] = round(total_estimated / total_max * 100, 1) if total_max else 0

# Theme threading analysis for report
theme_data = None
for f in findings:
    if f["rule_id"] == "SVA7-THEME-THREADING-DEPTH":
        themes_detail = f.get("details", {}).get("theme_analysis", [])
        if themes_detail:
            theme_data = {
                "overall_score": f["score"],
                "themes": themes_detail
            }
        break

# Traceability integrity from RTM
rtm_verification = rtm.get("verification", {})
traceability_data = {
    "forward_coverage_pct": rtm_verification.get("forward_coverage", {}).get("requirements_with_specs_pct", 0),
    "backward_coverage_pct": rtm_verification.get("backward_coverage", {}).get("specs_with_requirements_pct", 0),
    "false_positive_estimate_pct": 0,
    "compliance_chain_complete": findings[2]["passed"] if len(findings) > 2 else False
}

gold_team_report = {
    "team": "gold",
    "review_type": "Gold Team (~95% Completion - Final Gate)",
    "recommendation": (
        "PROCEED" if disposition == "PASS" and overall_score >= 85 else
        "PROCEED_WITH_CAUTION" if disposition in ["PASS", "ADVISORY"] else
        "REVISE_AND_RESUBMIT" if disposition == "BLOCK" and critical_failures <= 1 else
        "STOP"
    ),
    "top_concerns": [
        f"{f['rule_id']}: {f['rule_name']} (score: {f['score']})"
        for f in sorted(findings, key=lambda x: x['score'])
        if not f["passed"]
    ][:5],
    "evaluator_simulation": evaluator_sim,
    "traceability_integrity": traceability_data,
    "theme_threading": theme_data,
    "submission_readiness": {
        "bid_section_count": len(bid_texts),
        "total_pages_estimated": round(len(combined_bid_text) / 3000),
        "mandatory_items_addressed_pct": findings[2]["score"] if len(findings) > 2 else 0,
        "risk_mitigations_verified_pct": findings[1]["score"] if len(findings) > 1 else 0,
        "cross_doc_integrity_pct": findings[3]["score"] if len(findings) > 3 else 0,
        "submission_structure_matched": submission_structure is not None
    },
    "final_verdict": {
        "ready_for_submission": disposition == "PASS" and overall_score >= 80,
        "projected_evaluator_score": evaluator_sim["percentage"],
        "critical_blockers": critical_failures,
        "high_advisories": high_failures,
        "recommendation_summary": (
            "Bid is ready for final PDF assembly and submission."
            if disposition == "PASS" and overall_score >= 80 else
            "Bid has minor issues that should be addressed but can proceed."
            if disposition == "ADVISORY" else
            "Bid has critical gaps. Must revise before submission."
        )
    }
}
```

### Step 5: Build Corrective Actions Summary

```python
corrective_actions = []
for f in findings:
    if f.get("corrective_action"):
        corrective_actions.append({
            "priority": f["severity"],
            "action": f["corrective_action"].get("instruction", f["rule_name"]),
            "target_phase": f["corrective_action"].get("target_phase"),
            "auto_correctable": f["corrective_action"].get("auto_correctable", False),
            "rule_id": f["rule_id"]
        })

severity_order = {"CRITICAL": 0, "HIGH": 1, "MEDIUM": 2, "LOW": 3}
corrective_actions.sort(key=lambda x: severity_order.get(x["priority"], 4))
```

### Step 5b: Generate Gold Team Human Review Checklist

```python
# ---- Generate Gold Team Human Review Checklist ----

def generate_gold_team_checklist(rules_results, evaluator_factors, theme_threading, final_disposition, score):
    """Generate GOLD_TEAM_CHECKLIST.md for human reviewer sign-off."""

    checklist = f"""# Gold Team Review Checklist

**Generated:** {datetime.now().strftime('%Y-%m-%d %H:%M')}
**Overall Score:** {score:.0f}/100
**Disposition:** {final_disposition}

---

## Executive Summary

| Metric | Value |
|--------|-------|
| Overall Score | {score:.0f}/100 |
| Disposition | {final_disposition} |
| Critical Issues | {sum(1 for r in rules_results if r.get('severity') == 'CRITICAL' and not r.get('passed'))} |
| High Issues | {sum(1 for r in rules_results if r.get('severity') == 'HIGH' and not r.get('passed'))} |
| Advisory Items | {sum(1 for r in rules_results if r.get('severity') in ['MEDIUM', 'LOW'] and not r.get('passed'))} |
| Rules Passed | {sum(1 for r in rules_results if r.get('passed'))}/{len(rules_results)} |

---

## Section 1: Critical Findings (Must Fix Before Submission)

"""

    # CRITICAL items
    critical_items = [r for r in rules_results if r.get('severity') == 'CRITICAL' and not r.get('passed')]
    if critical_items:
        for i, item in enumerate(critical_items, 1):
            ca = item.get('corrective_action') or {}
            checklist += f"""### {i}. {item.get('rule_name', 'Unknown Rule')}

**Finding:** {ca.get('instruction', item.get('rule_name', 'Issue detected'))}
**Score:** {item.get('score', 'N/A')}/100 (threshold: {item.get('threshold', 'N/A')})
**Location:** See validation report for details

- [ ] **Approved** -- Issue resolved
- [ ] **Needs Revision** -- Requires rework (notes: _______________)

---

"""
    else:
        checklist += "_No critical issues found._\n\n---\n\n"

    # HIGH items
    checklist += "## Section 2: High Priority Findings (Should Fix)\n\n"
    high_items = [r for r in rules_results if r.get('severity') == 'HIGH' and not r.get('passed')]
    if high_items:
        for i, item in enumerate(high_items, 1):
            ca = item.get('corrective_action') or {}
            checklist += f"""### {i}. {item.get('rule_name', 'Unknown Rule')}

**Finding:** {ca.get('instruction', item.get('rule_name', 'Issue detected'))}
**Score:** {item.get('score', 'N/A')}/100 (threshold: {item.get('threshold', 'N/A')})
**Location:** See validation report for details

- [ ] **Approved** -- Acceptable as-is
- [ ] **Needs Revision** -- Requires rework
- [ ] **Deferred** -- Accepted risk (justification: _______________)

---

"""
    else:
        checklist += "_No high priority issues found._\n\n---\n\n"

    # ADVISORY items
    checklist += "## Section 3: Advisory Items (Noted)\n\n"
    advisory_items = [r for r in rules_results if r.get('severity') in ['MEDIUM', 'LOW'] and not r.get('passed')]
    if advisory_items:
        checklist += "| # | Rule | Finding | Action |\n"
        checklist += "|---|------|---------|--------|\n"
        for i, item in enumerate(advisory_items, 1):
            ca = item.get('corrective_action') or {}
            finding_short = (ca.get('instruction', item.get('rule_name', '')))[:80]
            checklist += f"| {i} | {item.get('rule_name', '')} | {finding_short} | [ ] Reviewed |\n"
        checklist += "\n---\n\n"
    else:
        checklist += "_No advisory items._\n\n---\n\n"

    # PASSED items
    checklist += "## Section 4: Verified (Passed)\n\n"
    passed_items = [r for r in rules_results if r.get('passed')]
    if passed_items:
        checklist += "| Rule | Score | Status |\n"
        checklist += "|------|-------|--------|\n"
        for item in passed_items:
            checklist += f"| {item.get('rule_name', '')} | {item.get('score', 'N/A')}/100 | PASS |\n"
        checklist += "\n---\n\n"

    # Evaluator simulation
    checklist += "## Section 5: Evaluator Simulation Scores\n\n"
    if evaluator_factors:
        checklist += "| Evaluation Factor | Projected Score | Max Points | Percentage |\n"
        checklist += "|-------------------|----------------|------------|------------|\n"
        for factor in evaluator_factors:
            checklist += f"| {factor.get('factor_name', '')} | {factor.get('estimated_score', 'N/A')} | {factor.get('max_points', 'N/A')} | {factor.get('score_percentage', 'N/A')}% |\n"
        checklist += "\n---\n\n"

    # Win theme coverage
    checklist += "## Section 6: Win Theme Threading Coverage\n\n"
    if theme_threading and theme_threading.get('themes'):
        checklist += "| Theme | Sections Present | Major Coverage | Depth | Status |\n"
        checklist += "|-------|-----------------|----------------|-------|--------|\n"
        for theme in theme_threading['themes']:
            status = "PASS" if theme.get('major_coverage_pct', 0) >= 50 else "LOW"
            checklist += f"| {theme.get('theme', '')} | {theme.get('sections_present', 0)}/{theme.get('sections_total', 0)} | {theme.get('major_coverage_pct', 0):.0f}% | {theme.get('depth', 'N/A')} | {status} |\n"
        checklist += "\n---\n\n"
    else:
        checklist += "_No theme threading data available._\n\n---\n\n"

    # Lohfeld 7 Quality Measures Assessment
    lohfeld_measures = build_lohfeld_summary(rules_results)
    checklist += "## Section 7: Lohfeld 7 Quality Measures Assessment\n\n"
    checklist += "_Maps SVA-7 findings to Lohfeld Consulting's industry-standard 7 Quality Measures for winning proposals._\n\n"
    checklist += "| Quality Measure | Description | Status | Score | SVA-7 Rules |\n"
    checklist += "|----------------|-------------|--------|-------|-------------|\n"
    for lm in lohfeld_measures:
        rules_str = ", ".join(lm["mapped_rules"])
        status_icon = "PASS" if lm["status"] == "PASS" else "NEEDS ATTENTION"
        checklist += f"| {lm['measure']} | {lm['description']} | {status_icon} | {lm['score']}/100 | {rules_str} |\n"
    lohfeld_pass_count = sum(1 for lm in lohfeld_measures if lm["status"] == "PASS")
    checklist += f"\n**Lohfeld Summary:** {lohfeld_pass_count}/7 quality measures met.\n\n---\n\n"

    # Final sign-off
    checklist += f"""## Final Sign-Off

**Bid Readiness Assessment:**

| Question | Response |
|----------|----------|
| Does the bid address all mandatory requirements? | [ ] Yes  [ ] No |
| Are all financial figures accurate and justified? | [ ] Yes  [ ] No |
| Do case studies effectively demonstrate relevance? | [ ] Yes  [ ] No |
| Is the executive summary compelling? | [ ] Yes  [ ] No |
| Would you recommend submission? | [ ] Yes  [ ] No  [ ] With revisions |

**Overall Decision:**

- [ ] **APPROVED FOR SUBMISSION** -- Bid is ready
- [ ] **APPROVED WITH MINOR REVISIONS** -- Submit after noted fixes
- [ ] **REQUIRES MAJOR REVISION** -- Do not submit until critical issues resolved
- [ ] **NO-BID RECOMMENDED** -- Fundamental issues identified

**Reviewer:** _________________________
**Date:** _________________________
**Notes:** _________________________

---

_This checklist was auto-generated by the SVA-7 Gold Team Review._
_Pipeline disposition: {final_disposition} | Score: {score:.0f}/100_
"""

    return checklist

# Generate and write checklist using variables from prior steps
checklist_content = generate_gold_team_checklist(
    rules_results=findings,
    evaluator_factors=evaluator_sim.get("factors", []),
    theme_threading=theme_data,
    final_disposition=disposition,
    score=overall_score
)

write_file(f"{folder}/outputs/GOLD_TEAM_CHECKLIST.md", checklist_content)
```

### Step 6: Write Report

```python
report = {
    "validator": "SVA-7",
    "stage_validated": 7,
    "validated_at": datetime.now().isoformat(),
    "disposition": disposition,
    "color_team": "gold",
    "summary": {
        "total_rules": len(findings),
        "passed": passed_count,
        "failed": failed_count,
        "critical_failures": critical_failures,
        "high_failures": high_failures,
        "overall_score": round(overall_score, 1)
    },
    "findings": findings,
    "color_team_report": gold_team_report,
    "lohfeld_quality_measures": lohfeld_summary,
    "corrective_actions_summary": corrective_actions,
    "execution_metadata": {
        "files_analyzed": len(bid_texts) + 7,
        "bid_sections_scanned": len(bid_texts),
        "total_bid_chars": len(combined_bid_text),
        "data_sources_read": [
            "UNIFIED_RTM.json",
            "COMPLIANCE_MATRIX.json",
            "EVALUATION_CRITERIA.json",
            "REQUIREMENT_RISKS.json",
            "PERSONA_COVERAGE.json",
            "CLIENT_INTELLIGENCE.json",
            "bid-context-bundle.json",
            "SUBMISSION_STRUCTURE.json"
        ] + list(bid_texts.keys())
    }
}

write_json(f"{folder}/shared/validation/sva7-gold-team.json", report)
```

### Step 7: Report Results

```
🥇 GOLD TEAM REVIEW COMPLETE (SVA-7)
======================================
Disposition: {disposition} {"✅" if disposition == "PASS" else "⚠️" if disposition == "ADVISORY" else "❌"}
Score: {overall_score:.0f}/100
Rules: {passed_count}/{len(findings)} passed | {critical_failures} CRITICAL fails | {high_failures} HIGH fails

Findings:
{for each finding: severity_icon + rule_id + score + pass/fail}

Gold Team Assessment:
  Recommendation: {gold_team_report["recommendation"]}
  Ready for Submission: {"YES ✅" if gold_team_report["final_verdict"]["ready_for_submission"] else "NO ❌"}

Evaluator Simulation:
  Projected Score: {evaluator_sim["percentage"]:.0f}% ({evaluator_sim["weighted_total"]}/{evaluator_sim["max_possible"]} points)
  {for each factor: factor_name + estimated_score/max_points + pct}

Win Theme Threading:
  {for each theme: theme + depth + sections_present/sections_total}

Compliance Verification:
  Mandatory Items: {compliance coverage}%
  Risk Mitigations: {risk coverage}%
  Cross-Doc IDs: {id integrity}%

Submission Readiness:
  Bid Sections: {len(bid_texts)}
  Estimated Pages: {pages}
  Structure Match: {"YES" if submission_structure else "N/A"}

Lohfeld 7 Quality Measures:
  {for each lohfeld_quality_measures: measure + status + score}
  Summary: {pass_count}/7 measures met

Final Verdict: {gold_team_report["final_verdict"]["recommendation_summary"]}

Output: shared/validation/sva7-gold-team.json
Human Review Checklist: outputs/GOLD_TEAM_CHECKLIST.md
```

## Quality Checklist

- [ ] `sva7-gold-team.json` created in `shared/validation/`
- [ ] `GOLD_TEAM_CHECKLIST.md` created in `outputs/` for human review sign-off
- [ ] All 13 rules evaluated (including SVA7-CASE-STUDY-VALIDATION, SVA7-TECH-LIFECYCLE-VALIDATION, SVA7-FINANCIAL-SANITY, and SVA7-PROOF-POINT-DENSITY)
- [ ] SVA7-TECH-LIFECYCLE-VALIDATION passed (no technology expires within 3 years, EOL column present)
- [ ] SVA7-FINANCIAL-SANITY checked (rates > $0, costs align with effort, markup reasonable)
- [ ] SVA7-THEME-THREADING-DEPTH checks eval factor callout boxes and >=50% major section coverage
- [ ] Disposition correctly calculated (BLOCK/ADVISORY/PASS)
- [ ] Evaluator simulation produces factor-by-factor projected scoring
- [ ] Win theme threading analysis includes depth assessment per theme
- [ ] Every mandatory item checked for bid presence
- [ ] Every HIGH/CRITICAL risk mitigation verified in bid text
- [ ] Cross-document ID integrity validated (no phantom references)
- [ ] Evaluator persona satisfaction checked per persona
- [ ] Format/page compliance estimated
- [ ] Statistical consistency verified
- [ ] Corrective actions sorted by priority
- [ ] SVA7-PROOF-POINT-DENSITY checked (>=60% of major sections contain quantified evidence)
- [ ] Lohfeld 7 Quality Measures mapped from SVA-7 findings (Compliant, Responsive, Understandable, Credible, Has Strengths, Low Risk, Winning)
- [ ] Lohfeld summary included in both `sva7-gold-team.json` and `GOLD_TEAM_CHECKLIST.md`
- [ ] Final submission readiness verdict included
