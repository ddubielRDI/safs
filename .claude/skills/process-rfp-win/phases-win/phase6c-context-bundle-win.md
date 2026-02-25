---
name: phase6c-context-bundle-win
expert-role: Data Integration Architect
domain-expertise: Data aggregation, context synthesis, information architecture
---

# Phase 6c: Bid Context Bundle

## Expert Role

You are a **Data Integration Architect** with deep expertise in:
- Data aggregation and synthesis
- Information architecture
- Context preservation across system boundaries
- JSON schema design for AI consumption

## Purpose

Aggregate all critical data from Stages 1-6 into a single comprehensive context bundle for Stage 7 bid generation. This solves the **context starvation problem** where bid authors only receive 4 of 15+ available data sources.

## Why This Phase Exists

**Problem Identified:**
- Stage 7 subskills received only: `POSITIONING_OUTPUT.json`, `CLIENT_INTELLIGENCE.json`, `requirements-normalized.json`, `domain-context.json`
- Missing critical context: risks, evaluation criteria, compliance achievements, personas, win scorecard, workflow coverage
- Result: Generic bids that don't leverage full analysis

**Solution:**
- Single aggregated file: `shared/bid-context-bundle.json`
- Contains summarized and structured data from ALL prior phases
- Enables Stage 7 Opus agent to generate compelling, fully-informed bids

## Inputs

**Required (must exist):**
- `{folder}/shared/domain-context.json` - Domain and industry context
- `{folder}/shared/requirements-normalized.json` - All requirements with metadata
- `{folder}/shared/EVALUATION_CRITERIA.json` - Scoring methodology
- `{folder}/shared/COMPLIANCE_MATRIX.json` - Mandatory items tracking
- `{folder}/shared/REQUIREMENT_RISKS.json` - Risk assessments and mitigations
- `{folder}/shared/bid/CLIENT_INTELLIGENCE.json` - Competitor and client research
- `{folder}/shared/bid/POSITIONING_OUTPUT.json` - Win themes and differentiators

**Optional (include if exists):**
- `{folder}/shared/workflow-extracted-reqs.json` - Workflow requirements
- `{folder}/shared/workflow-coverage.json` - Coverage metrics
- `{folder}/shared/sample-data-analysis.json` - Data entity analysis
- `{folder}/shared/PERSONA_COVERAGE.json` - Evaluator personas
- `{folder}/shared/WIN_SCORECARD.json` - Win probability factors
- `{folder}/shared/validation-results.json` - QA validation results
- `{folder}/shared/UNIFIED_RTM.json` - Unified RTM with composite priority scores

## Required Outputs

- `{folder}/shared/bid-context-bundle.json` - Comprehensive aggregated context

## Instructions

### Step 1: Load All Available Data Sources

```python
import json
from pathlib import Path
from datetime import datetime

def load_json_safe(path):
    """Load JSON file, return empty dict if not found."""
    try:
        with open(path, 'r') as f:
            return json.load(f)
    except FileNotFoundError:
        return {}
    except json.JSONDecodeError:
        log(f"WARNING: Invalid JSON in {path}")
        return {}

# Required sources
domain_context = load_json_safe(f"{folder}/shared/domain-context.json")
requirements = load_json_safe(f"{folder}/shared/requirements-normalized.json")
evaluation = load_json_safe(f"{folder}/shared/EVALUATION_CRITERIA.json")
compliance = load_json_safe(f"{folder}/shared/COMPLIANCE_MATRIX.json")
risks = load_json_safe(f"{folder}/shared/REQUIREMENT_RISKS.json")
client_intel = load_json_safe(f"{folder}/shared/bid/CLIENT_INTELLIGENCE.json")
positioning = load_json_safe(f"{folder}/shared/bid/POSITIONING_OUTPUT.json")

# Optional sources
workflow_reqs = load_json_safe(f"{folder}/shared/workflow-extracted-reqs.json")
workflow_coverage = load_json_safe(f"{folder}/shared/workflow-coverage.json")
sample_data = load_json_safe(f"{folder}/shared/sample-data-analysis.json")
personas = load_json_safe(f"{folder}/shared/PERSONA_COVERAGE.json")
win_scorecard = load_json_safe(f"{folder}/shared/WIN_SCORECARD.json")
validation = load_json_safe(f"{folder}/shared/validation-results.json")
unified_rtm = load_json_safe(f"{folder}/shared/UNIFIED_RTM.json")
```

### Step 2: Build Requirements Summary

```python
def build_requirements_summary(requirements_data):
    """Summarize requirements for bid context."""
    reqs = requirements_data.get("requirements", [])

    # Count by priority
    priority_counts = {"CRITICAL": 0, "HIGH": 0, "MEDIUM": 0, "LOW": 0}
    for req in reqs:
        priority = req.get("priority", "MEDIUM")
        priority_counts[priority] = priority_counts.get(priority, 0) + 1

    # Count by category
    category_counts = {}
    for req in reqs:
        cat = req.get("category", "OTHER")
        category_counts[cat] = category_counts.get(cat, 0) + 1

    # Extract critical requirements for emphasis
    critical_reqs = [
        {
            "id": req.get("canonical_id"),
            "text": req.get("text", "")[:200],  # Truncate for summary
            "category": req.get("category")
        }
        for req in reqs if req.get("priority") == "CRITICAL"
    ][:20]  # Top 20 critical

    return {
        "total": len(reqs),
        "by_priority": priority_counts,
        "by_category": category_counts,
        "critical_requirements": critical_reqs,
        "coverage_claim": "100% of requirements addressed"
    }

requirements_summary = build_requirements_summary(requirements)
```

### Step 3: Build Risk Highlights

```python
def build_risk_highlights(risks_data):
    """Extract top risks with mitigations for bid narrative."""
    all_risks = risks_data.get("risks", [])

    # Sort by severity (HIGH first, then MEDIUM)
    severity_order = {"HIGH": 0, "MEDIUM": 1, "LOW": 2}
    sorted_risks = sorted(
        all_risks,
        key=lambda r: severity_order.get(r.get("severity", "LOW"), 3)
    )

    # Extract top 10 for bid integration
    top_risks = []
    for risk in sorted_risks[:10]:
        top_risks.append({
            "id": risk.get("risk_id", risk.get("id")),
            "category": risk.get("category"),
            "description": risk.get("description", "")[:300],
            "severity": risk.get("severity"),
            "mitigation": {
                "strategy": risk.get("mitigation", {}).get("strategy", ""),
                "owner": risk.get("mitigation", {}).get("owner_role", ""),
                "timeline": risk.get("mitigation", {}).get("timeline", ""),
                "evidence": risk.get("mitigation", {}).get("evidence", [])[:3]
            },
            "residual_risk": risk.get("residual_risk", "low")
        })

    # Summary counts
    severity_counts = {"HIGH": 0, "MEDIUM": 0, "LOW": 0}
    for risk in all_risks:
        sev = risk.get("severity", "LOW")
        severity_counts[sev] = severity_counts.get(sev, 0) + 1

    return {
        "total_risks_assessed": len(all_risks),
        "by_severity": severity_counts,
        "top_10_risks": top_risks,
        "mitigation_coverage": f"{len([r for r in all_risks if r.get('mitigation')])} / {len(all_risks)} risks have mitigations"
    }

risk_highlights = build_risk_highlights(risks)
```

### Step 4: Build Evaluation Alignment

```python
def build_evaluation_alignment(evaluation_data, requirements_summary):
    """Map evaluation criteria to our strengths."""
    method = evaluation_data.get("evaluation_method", "Best Value")
    factors = evaluation_data.get("factors", evaluation_data.get("evaluation_factors", []))

    criteria = []
    for factor in factors:
        criteria.append({
            "factor": factor.get("name", factor.get("factor")),
            "weight": factor.get("weight", factor.get("points")),
            "weight_normalized": factor.get("weight_normalized", 0),
            "description": factor.get("description", ""),
            "our_approach": f"Addressed through {factor.get('name', 'comprehensive solution')}",
            "evidence_refs": []  # To be populated by bid author
        })

    # Sort by weight descending
    criteria.sort(key=lambda c: c.get("weight", 0), reverse=True)

    return {
        "method": method,
        "criteria": criteria,
        "total_points": sum(c.get("weight", 0) for c in criteria),
        "recommendation": "Structure bid sections by evaluation weight for maximum impact"
    }

evaluation_alignment = build_evaluation_alignment(evaluation, requirements_summary)
```

### Step 4b: Extract Full Evaluation-to-Bid Mapping

```python
def build_full_eval_mapping(evaluation_data, positioning_data):
    """Extract the FULL bid_section_mapping and evaluation factor details.

    This preserves the detailed mapping from Phase 1.6 that tells each bid section
    which evaluation factors it addresses, emphasis notes, and associated win themes.
    The summary in Step 4 loses this granularity -- this step preserves it.
    """

    # Full evaluation-to-bid mapping (from Phase 1.6)
    eval_to_bid_mapping = evaluation_data.get("bid_section_mapping",
        evaluation_data.get("EVALUATION_TO_BID_MAPPING", {}))

    # Evaluation factors sorted by weight (descending) for prioritization
    raw_factors = evaluation_data.get("evaluation_factors",
        evaluation_data.get("factors", []))
    factors_by_weight = sorted(
        raw_factors,
        key=lambda f: f.get("weight_normalized", f.get("weight", 0)),
        reverse=True
    )

    # Theme-to-eval mapping and section mandates from positioning
    theme_eval_mapping = {}
    section_theme_mandates = {}
    evaluator_messages = {}

    if positioning_data:
        theme_eval_mapping = positioning_data.get("theme_eval_mapping", {})
        section_theme_mandates = positioning_data.get("section_theme_mandates", {})
        evaluator_messages = positioning_data.get("evaluator_messages", {})

    return {
        "evaluation_to_bid_mapping": eval_to_bid_mapping,
        "evaluation_factors_by_weight": factors_by_weight,
        "theme_eval_mapping": theme_eval_mapping,
        "section_theme_mandates": section_theme_mandates,
        "evaluator_messages": evaluator_messages
    }

full_eval_mapping = build_full_eval_mapping(evaluation, positioning)
```

### Step 4c: Build Section Content Guide

```python
def build_section_content_guide(eval_to_bid_mapping, section_theme_mandates, content_priority_guide):
    """Build a combined guide for each bid section that merges:
    - Evaluation factors it addresses (from bid_section_mapping)
    - Win themes mandated for it (from section_theme_mandates)
    - Top requirements by composite score (from content_priority_guide)

    This gives bid authors a single lookup per section with everything they
    need to know about what to emphasize and why.
    """

    section_guide = {}

    # Start with evaluation-to-bid mapping as the skeleton
    for section_key, mapping in eval_to_bid_mapping.items():
        section_guide[section_key] = {
            "evaluation_factors": mapping.get("evaluation_factors",
                [mapping.get("primary_factor", section_key)]),
            "emphasis_notes": mapping.get("emphasis_notes",
                mapping.get("emphasis", "")),
            "subsections": mapping.get("subsections", []),
            "weight_total": mapping.get("weight_total",
                mapping.get("combined_weight", 0)),
            "mandated_themes": [],
            "top_requirements": []
        }

    # Merge in mandated themes from positioning
    for section_key, mandates in section_theme_mandates.items():
        if section_key in section_guide:
            section_guide[section_key]["mandated_themes"] = mandates if isinstance(mandates, list) else [mandates]
        else:
            # Section exists in theme mandates but not in eval mapping
            section_guide[section_key] = {
                "evaluation_factors": [],
                "emphasis_notes": "",
                "subsections": [],
                "weight_total": 0,
                "mandated_themes": mandates if isinstance(mandates, list) else [mandates],
                "top_requirements": []
            }

    # Merge in top requirements from content_priority_guide (if available)
    if content_priority_guide.get("available"):
        top_reqs = content_priority_guide.get("top_30_requirements", [])
        for req in top_reqs:
            eval_factor = req.get("evaluation_factor", "")
            category = req.get("category", "GENERAL")
            # Try to match requirement to a section by evaluation_factor or category
            matched = False
            for section_key, guide in section_guide.items():
                factors = guide.get("evaluation_factors", [])
                # Match if eval_factor is in the section's factors list
                if eval_factor and any(eval_factor.lower() in f.lower() for f in factors):
                    guide["top_requirements"].append({
                        "req_id": req["req_id"],
                        "composite_score": req["composite_score"],
                        "is_mandatory": req["is_mandatory"],
                        "text_preview": req["text_preview"]
                    })
                    matched = True
                    break
            # If no match by eval factor, put in an "unmatched" bucket
            if not matched:
                if "_unmatched" not in section_guide:
                    section_guide["_unmatched"] = {
                        "evaluation_factors": [],
                        "emphasis_notes": "Requirements not yet mapped to a specific bid section",
                        "subsections": [],
                        "weight_total": 0,
                        "mandated_themes": [],
                        "top_requirements": []
                    }
                section_guide["_unmatched"]["top_requirements"].append({
                    "req_id": req["req_id"],
                    "composite_score": req["composite_score"],
                    "is_mandatory": req["is_mandatory"],
                    "text_preview": req["text_preview"]
                })

    # Sort top_requirements within each section by composite_score descending
    for section_key in section_guide:
        section_guide[section_key]["top_requirements"].sort(
            key=lambda r: r.get("composite_score", 0), reverse=True
        )

    return section_guide

section_content_guide = build_section_content_guide(
    full_eval_mapping["evaluation_to_bid_mapping"],
    full_eval_mapping["section_theme_mandates"],
    content_priority_guide
)
```

### Step 5: Build Competitive Position

```python
def build_competitive_position(client_intel, positioning_data):
    """Synthesize competitive positioning for bid narrative."""

    # Extract incumbent information
    incumbent = client_intel.get("incumbent_analysis", {})
    competitors = client_intel.get("competitors", [])

    # Extract our differentiators
    differentiators = positioning_data.get("core_positioning", {}).get("key_differentiators", [])

    # Build contrast matrix
    contrasts = []
    incumbent_weaknesses = incumbent.get("weaknesses", [])

    for i, weakness in enumerate(incumbent_weaknesses[:5]):
        matching_diff = differentiators[i] if i < len(differentiators) else {}
        contrasts.append({
            "incumbent_weakness": weakness,
            "our_advantage": matching_diff.get("differentiator", "Modern solution"),
            "evidence": matching_diff.get("evidence", "Proven capability")
        })

    return {
        "incumbent": {
            "name": incumbent.get("name", "Current Solution"),
            "weaknesses": incumbent_weaknesses[:5],
            "contract_value": incumbent.get("contract_value"),
            "contract_end": incumbent.get("contract_end_date")
        },
        "our_differentiators": [
            {
                "differentiator": d.get("differentiator"),
                "evidence": d.get("evidence"),
                "quantified_benefit": d.get("quantified_benefit", "")
            }
            for d in differentiators[:6]
        ],
        "competitive_contrasts": contrasts,
        "win_themes": positioning_data.get("core_positioning", {}).get("themes", [])
    }

competitive_position = build_competitive_position(client_intel, positioning)
```

### Step 6: Build Compliance Achievements

```python
def build_compliance_achievements(compliance_data, domain_context):
    """Summarize compliance achievements for trust building."""

    mandatory_items = compliance_data.get("mandatory_items", [])
    addressed = [item for item in mandatory_items if item.get("addressed", False)]

    frameworks = domain_context.get("compliance_frameworks", [])

    return {
        "mandatory_items": {
            "total": len(mandatory_items),
            "addressed": len(addressed),
            "percentage": f"{(len(addressed) / len(mandatory_items) * 100):.0f}%" if mandatory_items else "100%",
            "unaddressed": [
                item.get("description", item.get("item"))
                for item in mandatory_items if not item.get("addressed", False)
            ][:5]
        },
        "compliance_frameworks": frameworks,
        "certifications_claimed": [
            "SOC 2 Type II",
            "ISO 27001"
        ],  # Template - user should customize
        "compliance_narrative": f"We address all {len(addressed)} mandatory requirements and comply with {', '.join(frameworks[:3]) if frameworks else 'industry standards'}."
    }

compliance_achievements = build_compliance_achievements(compliance, domain_context)
```

### Step 7: Build Win Themes Structure

```python
def build_win_themes(positioning_data, domain_context):
    """Structure win themes for consistent threading."""

    raw_themes = positioning_data.get("core_positioning", {}).get("themes", [])
    value_prop = positioning_data.get("core_positioning", {}).get("value_proposition", "")

    # Ensure 3-5 themes (3-4 optimal per industry consensus, 5 max)
    themes = []
    default_themes = [
        "Modern Architecture",
        "Domain Expertise",
        "Risk Mitigation",
        "Compliance Excellence",
        "Partnership Approach"
    ]

    target_count = max(3, min(len(raw_themes), 5))  # 3-5 themes, prefer actual count
    for i in range(target_count):
        if i < len(raw_themes):
            theme_name = raw_themes[i]
        else:
            theme_name = default_themes[i]

        themes.append({
            "theme": theme_name,
            "tagline": f"Excellence in {theme_name.lower()}",
            "evidence": [],  # To be populated with specific proof points
            "sections_to_emphasize": ["title-page.md", "solution.md", "timeline.md"],
            "keywords": [theme_name.lower().replace(" ", "-")]
        })

    return {
        "value_proposition": value_prop,
        "themes": themes,
        "threading_instruction": "Each theme MUST appear in executive summary, solution narrative, and timeline. Use exact theme names for consistency.",
        "verification_grep": [t["theme"] for t in themes]
    }

win_themes = build_win_themes(positioning, domain_context)
```

### Step 8: Build Personas Summary

```python
def build_personas_summary(personas_data):
    """Summarize evaluator personas for targeted writing."""

    if not personas_data:
        return {
            "identified": False,
            "recommendation": "Write for general technical and executive audience"
        }

    evaluators = personas_data.get("evaluators", [])

    return {
        "identified": True,
        "primary_evaluators": [
            {
                "role": e.get("role"),
                "concerns": e.get("primary_concerns", [])[:3],
                "messaging_focus": e.get("recommended_messaging", "")
            }
            for e in evaluators[:4]
        ],
        "coverage_score": personas_data.get("coverage_score", 0),
        "writing_guidance": "Tailor technical depth to IT Director, ROI emphasis to CFO, compliance focus to Legal."
    }

personas_summary = build_personas_summary(personas)
```

### Step 9: Build Win Probability Summary

```python
def build_win_probability(win_scorecard_data):
    """Summarize win probability factors."""

    if not win_scorecard_data:
        return {
            "calculated": False,
            "recommendation": "Focus on evaluation criteria alignment"
        }

    return {
        "calculated": True,
        "overall_probability": win_scorecard_data.get("win_probability", 0),
        "confidence_level": win_scorecard_data.get("confidence", "medium"),
        "strengths": win_scorecard_data.get("strength_factors", [])[:5],
        "risks_to_address": win_scorecard_data.get("risk_factors", [])[:5],
        "recommendation": win_scorecard_data.get("strategic_recommendation", "")
    }

win_probability = build_win_probability(win_scorecard)
```

### Step 9b: Build Content Priority Guide from RTM (NEW)

```python
def build_content_priority_guide(rtm_data):
    """Extract composite priority scores from UNIFIED_RTM.json to guide bid content ordering."""

    if not rtm_data:
        return {
            "available": False,
            "note": "UNIFIED_RTM.json not available - using default content ordering"
        }

    rtm_reqs = rtm_data.get("entities", {}).get("requirements", [])
    if not rtm_reqs:
        return {"available": False, "note": "No requirements in RTM"}

    # Sort by composite_priority_score descending
    scored_reqs = sorted(rtm_reqs,
        key=lambda r: r.get("composite_priority_score", 0), reverse=True)

    # Top 30 requirements that should receive the most emphasis in bid
    top_requirements = [
        {
            "req_id": r["req_id"],
            "category": r["category"],
            "priority": r["priority"],
            "composite_score": r.get("composite_priority_score", 0),
            "evaluation_factor": r.get("evaluation_factor"),
            "is_mandatory": bool(r.get("mandatory_item_ids")),
            "text_preview": r["text"][:150]
        }
        for r in scored_reqs[:30]
    ]

    # Category ordering by aggregate composite score
    category_scores = {}
    for r in rtm_reqs:
        cat = r.get("category", "GENERAL")
        score = r.get("composite_priority_score", 0)
        if cat not in category_scores:
            category_scores[cat] = {"total_score": 0, "count": 0}
        category_scores[cat]["total_score"] += score
        category_scores[cat]["count"] += 1

    category_order = sorted(category_scores.items(),
        key=lambda x: x[1]["total_score"], reverse=True)

    # Chain completeness summary
    chains = rtm_data.get("chain_links", [])
    chain_summary = {
        "total": len(chains),
        "complete": sum(1 for c in chains if c.get("status") == "COMPLETE"),
        "partial": sum(1 for c in chains if c.get("status") == "PARTIAL"),
        "broken": sum(1 for c in chains if c.get("status") == "BROKEN"),
        "avg_score": round(sum(c.get("completeness_score", 0) for c in chains) / len(chains), 3) if chains else 0
    }

    return {
        "available": True,
        "top_30_requirements": top_requirements,
        "category_content_order": [
            {"category": cat, "total_score": info["total_score"], "req_count": info["count"]}
            for cat, info in category_order
        ],
        "chain_completeness": chain_summary,
        "bid_content_instruction": (
            "Order bid content by category_content_order. Within each category, "
            "address top_30_requirements first with the most detail and evidence. "
            "Requirements with is_mandatory=true MUST be explicitly addressed with "
            "a clear compliance statement."
        )
    }

content_priority_guide = build_content_priority_guide(unified_rtm)
```

### Step 10: Assemble Final Bundle

```python
def assemble_context_bundle():
    """Assemble the complete bid context bundle."""

    bundle = {
        "meta": {
            "generated_at": datetime.now().isoformat(),
            "rfp_folder": folder,
            "domain": domain_context.get("selected_domain", "general"),
            "sources_included": [],
            "bundle_version": "1.0"
        },

        "requirements_summary": requirements_summary,
        "risk_highlights": risk_highlights,
        "evaluation_alignment": evaluation_alignment,
        "evaluation_to_bid_mapping": full_eval_mapping["evaluation_to_bid_mapping"],
        "evaluation_factors_by_weight": full_eval_mapping["evaluation_factors_by_weight"],
        "theme_eval_mapping": full_eval_mapping["theme_eval_mapping"],
        "section_theme_mandates": full_eval_mapping["section_theme_mandates"],
        "evaluator_messages": full_eval_mapping["evaluator_messages"],
        "section_content_guide": section_content_guide,
        "competitive_position": competitive_position,
        "compliance_achievements": compliance_achievements,
        "win_themes": win_themes,
        "personas": personas_summary,
        "win_probability": win_probability,

        "domain_context": {
            "industry": domain_context.get("industry"),
            "selected_domain": domain_context.get("selected_domain"),
            "external_systems": domain_context.get("external_systems", [])[:10],
            "compliance_frameworks": domain_context.get("compliance_frameworks", [])
        },

        "matched_evidence": positioning.get("matched_evidence", []),
        "evidence_summary": positioning.get("evidence_summary", {}),

        "case_study_placeholders": {
            "instruction": "Replace [CASE STUDY PLACEHOLDER] markers with real examples",
            "template": {
                "client_name": "[Client Name]",
                "project_scope": "[Brief description of project]",
                "metrics_achieved": "[Quantified outcomes]",
                "relevance": "[Why this is relevant to current RFP]"
            },
            "suggested_count": 3
        },

        "content_priority_guide": content_priority_guide,

        "bid_author_instructions": {
            "theme_threading": "Each win theme (3-5) MUST appear in every major section",
            "risk_integration": "Top 10 risks with mitigations should be woven into solution narrative",
            "evaluation_alignment": "Structure sections by evaluation criteria weight",
            "competitive_contrast": "Include 'Why Us' comparisons against incumbent weaknesses",
            "compliance_emphasis": "Prominently feature mandatory item coverage in executive summary",
            "content_ordering": "Use content_priority_guide to order content by composite_priority_score"
        }
    }

    # Track which sources were included
    sources = []
    if requirements: sources.append("requirements-normalized.json")
    if risks: sources.append("REQUIREMENT_RISKS.json")
    if evaluation: sources.append("EVALUATION_CRITERIA.json")
    if compliance: sources.append("COMPLIANCE_MATRIX.json")
    if client_intel: sources.append("CLIENT_INTELLIGENCE.json")
    if positioning: sources.append("POSITIONING_OUTPUT.json")
    if personas: sources.append("PERSONA_COVERAGE.json")
    if win_scorecard: sources.append("WIN_SCORECARD.json")
    if workflow_coverage: sources.append("workflow-coverage.json")
    if sample_data: sources.append("sample-data-analysis.json")
    if domain_context: sources.append("domain-context.json")
    if unified_rtm: sources.append("UNIFIED_RTM.json")

    bundle["meta"]["sources_included"] = sources
    bundle["meta"]["source_count"] = len(sources)

    return bundle

context_bundle = assemble_context_bundle()
```

### Step 11: Write Output

```python
# Ensure bid subdirectory exists
Path(f"{folder}/shared/bid").mkdir(parents=True, exist_ok=True)

# Write the context bundle
output_path = f"{folder}/shared/bid-context-bundle.json"
with open(output_path, 'w') as f:
    json.dump(context_bundle, f, indent=2)

log(f"Context bundle written to: {output_path}")
```

### Step 12: Report Results

```python
log(f"""
📦 Bid Context Bundle Generated
================================
Sources Aggregated: {context_bundle['meta']['source_count']}
Domain: {context_bundle['domain_context']['selected_domain']}

📊 Requirements Summary:
   Total: {context_bundle['requirements_summary']['total']}
   Critical: {context_bundle['requirements_summary']['by_priority']['CRITICAL']}

⚠️ Risk Highlights:
   Total Assessed: {context_bundle['risk_highlights']['total_risks_assessed']}
   High Severity: {context_bundle['risk_highlights']['by_severity']['HIGH']}
   Top 10 Extracted: Yes

📋 Evaluation Alignment:
   Method: {context_bundle['evaluation_alignment']['method']}
   Criteria Count: {len(context_bundle['evaluation_alignment']['criteria'])}
   Eval-to-Bid Mappings: {len(context_bundle['evaluation_to_bid_mapping'])} sections mapped
   Factors by Weight: {len(context_bundle['evaluation_factors_by_weight'])} factors ranked
   Theme-Eval Mappings: {len(context_bundle['theme_eval_mapping'])} themes mapped
   Section Content Guide: {len(context_bundle['section_content_guide'])} sections with merged guidance

🏆 Win Themes: {len(context_bundle['win_themes']['themes'])}
   {chr(10).join(f"   • {t['theme']}" for t in context_bundle['win_themes']['themes'])}

✅ Compliance:
   Mandatory Items: {context_bundle['compliance_achievements']['mandatory_items']['addressed']}/{context_bundle['compliance_achievements']['mandatory_items']['total']}

📁 Output: {output_path}

Stage 7 bid author now has FULL CONTEXT for compelling bid generation.
""")
```

## Quality Checklist

- [ ] `bid-context-bundle.json` created in `shared/`
- [ ] At least 10 sources aggregated
- [ ] Requirements summary includes priority breakdown
- [ ] Top 10 risks extracted with mitigations
- [ ] Evaluation criteria sorted by weight
- [ ] 3-5 win themes defined
- [ ] Competitive contrasts populated
- [ ] Compliance achievements summarized
- [ ] Case study placeholder template included
- [ ] Bid author instructions included
- [ ] `evaluation_to_bid_mapping` included (full mapping, not just summary)
- [ ] `evaluation_factors_by_weight` sorted descending by weight
- [ ] `theme_eval_mapping` included if POSITIONING_OUTPUT.json exists
- [ ] `section_theme_mandates` included if available
- [ ] `evaluator_messages` included if available
- [ ] `section_content_guide` merges eval factors + themes + top requirements per section
- [ ] `matched_evidence` passed through from POSITIONING_OUTPUT.json
- [ ] `evidence_summary` passed through from POSITIONING_OUTPUT.json

## Verification

After creation, verify the bundle:

```bash
# Check file exists and has content
ls -la {folder}/shared/bid-context-bundle.json

# Verify JSON is valid
python -c "import json; json.load(open('{folder}/shared/bid-context-bundle.json'))"

# Check source count
grep '"source_count"' {folder}/shared/bid-context-bundle.json
```
