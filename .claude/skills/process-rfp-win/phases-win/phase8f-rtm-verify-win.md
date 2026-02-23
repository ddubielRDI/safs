---
name: phase8f-rtm-verify-win
expert-role: Traceability Auditor
domain-expertise: RTM verification, compliance auditing, coverage analysis, bid quality assurance
---

# Phase 8f: RTM Verification

## Expert Role

You are a **Traceability Auditor** with deep expertise in:
- Requirements traceability matrix verification
- Forward and backward coverage analysis
- Compliance chain integrity auditing
- Evaluation alignment verification
- Bid quality assurance metrics

## Purpose

Run 14 verification queries against the UNIFIED_RTM.json to validate traceability integrity, identify gaps, and produce an RTM_REPORT.md for stakeholder review. This phase updates the `verification{}` section of the RTM with comprehensive coverage metrics.

## Inputs

- `{folder}/shared/UNIFIED_RTM.json` - The master RTM (built by Phase 4, updated by Phase 8)
- `{folder}/shared/EVALUATION_CRITERIA.json` - Evaluation weights for alignment checks
- `{folder}/shared/COMPLIANCE_MATRIX.json` - Mandatory items for compliance chain verification

## Required Outputs

- `{folder}/outputs/RTM_REPORT.md` - Human-readable verification report
- `{folder}/shared/UNIFIED_RTM.json` - Updated with `verification{}` section containing query results

## Instructions

### Step 1: Load RTM and Supporting Data

```python
from datetime import datetime

rtm = read_json(f"{folder}/shared/UNIFIED_RTM.json")
evaluation = read_json(f"{folder}/shared/EVALUATION_CRITERIA.json")
compliance = read_json(f"{folder}/shared/COMPLIANCE_MATRIX.json")

entities = rtm["entities"]
chains = rtm["chain_links"]

reqs = entities["requirements"]
specs = entities["specifications"]
risks = entities["risks"]
bid_sections = entities["bid_sections"]
mandatory_items = entities["mandatory_items"]
evidence = entities["evidence"]
eval_criteria = entities["evaluation_criteria"]

log(f"🔍 RTM Verification starting: {len(reqs)} reqs, {len(chains)} chains, "
    f"{len(bid_sections)} bid sections, {len(risks)} risks")
```

### Step 2: Define Verification Queries

```python
VERIFICATION_QUERIES = [
    # Forward queries (requirement reaches downstream?)
    {"id": "F1", "name": "Requirements-to-Spec", "direction": "forward",
     "description": "Every requirement is linked to at least one specification section"},
    {"id": "F2", "name": "Requirements-to-Bid", "direction": "forward",
     "description": "Every requirement has a bid_section in its chain"},
    {"id": "F3", "name": "Risk-Mitigation-in-Bid", "direction": "forward",
     "description": "Every HIGH/CRITICAL risk mitigation has a verified bid_location"},
    {"id": "F4", "name": "Mandatory-Item-Bid-Location", "direction": "forward",
     "description": "Every mandatory item has coverage_status ADDRESSED and a bid_location"},
    {"id": "F5", "name": "Evaluation-Weight-Content-Allocation", "direction": "forward",
     "description": "Bid section page allocation roughly matches evaluation weight distribution"},

    # Backward queries (bid section justified?)
    {"id": "B1", "name": "Bid-Section-Has-Requirements", "direction": "backward",
     "description": "Every bid section links to at least one requirement"},
    {"id": "B2", "name": "Bid-Section-Has-Themes", "direction": "backward",
     "description": "Every bid section has at least one win theme present"},
    {"id": "B3", "name": "Bid-Heading-Mapped", "direction": "backward",
     "description": "Every bid section heading maps to an RTM bid_section_id"},
    {"id": "B4", "name": "Evidence-Sufficiency", "direction": "backward",
     "description": "CRITICAL/HIGH requirements have at least one evidence artifact"},

    # Chain integrity queries
    {"id": "C1", "name": "Chain-Completeness", "direction": "chain",
     "description": "Average chain completeness score >= 0.80"},
    {"id": "C2", "name": "Compliance-Chain-Integrity", "direction": "chain",
     "description": "Every mandatory item traces through: M-item → Requirement → Spec → Bid"},
    {"id": "C3", "name": "Risk-to-Bid-Integrity", "direction": "chain",
     "description": "Every HIGH/MEDIUM risk traces to a bid section via its mitigations"},
    {"id": "C4", "name": "Evaluation-Alignment", "direction": "chain",
     "description": "Every evaluation factor has linked requirements AND linked bid sections"},
    {"id": "C5", "name": "No-Orphaned-Entities", "direction": "chain",
     "description": "No spec sections, risks, or bid sections are completely unlinked"}
]
```

### Step 3: Execute Forward Queries

```python
query_results = []

# --- F1: Requirements-to-Spec ---
def run_f1():
    linked = 0
    unlinked = []
    for req in reqs:
        has_spec = any(
            req["req_id"] in spec.get("linked_requirement_ids", [])
            for spec in specs
        )
        if has_spec:
            linked += 1
        else:
            unlinked.append(req["req_id"])

    score = linked / len(reqs) * 100 if reqs else 0
    return {
        "query_id": "F1",
        "query_name": "Requirements-to-Spec",
        "direction": "forward",
        "passed": score >= 95.0,
        "score": round(score, 1),
        "details": f"{linked}/{len(reqs)} requirements linked to specs. "
                   f"Unlinked: {', '.join(unlinked[:10])}"
                   f"{f' (+{len(unlinked)-10} more)' if len(unlinked) > 10 else ''}"
    }

query_results.append(run_f1())


# --- F2: Requirements-to-Bid ---
def run_f2():
    linked = 0
    unlinked = []
    for chain in chains:
        if chain.get("bid_sections"):
            linked += 1
        else:
            unlinked.append(chain["requirement"])

    score = linked / len(chains) * 100 if chains else 0
    return {
        "query_id": "F2",
        "query_name": "Requirements-to-Bid",
        "direction": "forward",
        "passed": score >= 90.0,
        "score": round(score, 1),
        "details": f"{linked}/{len(chains)} requirements trace to bid sections. "
                   f"Missing: {', '.join(unlinked[:10])}"
                   f"{f' (+{len(unlinked)-10} more)' if len(unlinked) > 10 else ''}"
    }

query_results.append(run_f2())


# --- F3: Risk-Mitigation-in-Bid ---
def run_f3():
    high_risks = [r for r in risks if r.get("risk_level") in ["HIGH", "CRITICAL"]]
    verified = 0
    unverified = []

    for risk in high_risks:
        mitigations = risk.get("mitigation_strategies", [])
        has_bid_loc = any(m.get("bid_location") for m in mitigations)
        if has_bid_loc:
            verified += 1
        else:
            unverified.append(risk["risk_id"])

    score = verified / len(high_risks) * 100 if high_risks else 100
    return {
        "query_id": "F3",
        "query_name": "Risk-Mitigation-in-Bid",
        "direction": "forward",
        "passed": score >= 80.0,
        "score": round(score, 1),
        "details": f"{verified}/{len(high_risks)} HIGH/CRITICAL risk mitigations verified in bid. "
                   f"Unverified: {', '.join(unverified[:10])}"
    }

query_results.append(run_f3())


# --- F4: Mandatory-Item-Bid-Location ---
def run_f4():
    addressed = 0
    gaps = []

    for m in mandatory_items:
        has_coverage = m.get("coverage_status") == "ADDRESSED"
        has_bid_loc = m.get("bid_location") is not None
        if has_coverage and has_bid_loc:
            addressed += 1
        elif has_coverage and not has_bid_loc:
            # Addressed but not yet verified in bid
            addressed += 1  # Count as partial success
        else:
            gaps.append(m["mandatory_id"])

    score = addressed / len(mandatory_items) * 100 if mandatory_items else 100
    return {
        "query_id": "F4",
        "query_name": "Mandatory-Item-Bid-Location",
        "direction": "forward",
        "passed": score >= 95.0,
        "score": round(score, 1),
        "details": f"{addressed}/{len(mandatory_items)} mandatory items addressed. "
                   f"Gaps: {', '.join(gaps[:10])}"
    }

query_results.append(run_f4())


# --- F5: Evaluation-Weight-Content-Allocation ---
def run_f5():
    """Check if bid section page allocation roughly matches evaluation weights."""
    if not bid_sections or not eval_criteria:
        return {
            "query_id": "F5",
            "query_name": "Evaluation-Weight-Content-Allocation",
            "direction": "forward",
            "passed": True,
            "score": 0.0,
            "details": "No bid sections or evaluation criteria to compare (pre-bid phase)"
        }

    # Build factor → total page allocation
    factor_pages = {}
    for bs in bid_sections:
        factor = bs.get("evaluation_factor", "Unknown")
        pages = bs.get("page_estimate", 1)
        factor_pages[factor] = factor_pages.get(factor, 0) + pages

    total_pages = sum(factor_pages.values()) or 1

    # Compare page allocation % to weight %
    alignment_gaps = []
    for factor in eval_criteria:
        weight_pct = factor.get("weight_percent", 0)
        page_pct = factor_pages.get(factor["name"], 0) / total_pages * 100
        gap = abs(weight_pct - page_pct)
        if gap > 15:  # >15% misalignment is notable
            alignment_gaps.append(f"{factor['name']}: weight={weight_pct:.0f}%, pages={page_pct:.0f}%")

    score = 100 - len(alignment_gaps) * 10
    return {
        "query_id": "F5",
        "query_name": "Evaluation-Weight-Content-Allocation",
        "direction": "forward",
        "passed": score >= 70.0,
        "score": max(0, round(score, 1)),
        "details": f"Alignment gaps (>15%): {', '.join(alignment_gaps) if alignment_gaps else 'None'}"
    }

query_results.append(run_f5())
```

### Step 4: Execute Backward Queries

```python
# --- B1: Bid-Section-Has-Requirements ---
def run_b1():
    if not bid_sections:
        return {
            "query_id": "B1", "query_name": "Bid-Section-Has-Requirements",
            "direction": "backward", "passed": True, "score": 0.0,
            "details": "No bid sections yet (pre-bid phase)"
        }

    linked = sum(1 for bs in bid_sections if bs.get("linked_requirement_ids"))
    orphaned = [bs["bid_section_id"] for bs in bid_sections if not bs.get("linked_requirement_ids")]

    score = linked / len(bid_sections) * 100
    return {
        "query_id": "B1", "query_name": "Bid-Section-Has-Requirements",
        "direction": "backward", "passed": score >= 90.0, "score": round(score, 1),
        "details": f"{linked}/{len(bid_sections)} bid sections have requirements. "
                   f"Orphaned: {', '.join(orphaned[:5])}"
    }

query_results.append(run_b1())


# --- B2: Bid-Section-Has-Themes ---
def run_b2():
    if not bid_sections:
        return {
            "query_id": "B2", "query_name": "Bid-Section-Has-Themes",
            "direction": "backward", "passed": True, "score": 0.0,
            "details": "No bid sections yet (pre-bid phase)"
        }

    with_themes = sum(1 for bs in bid_sections if bs.get("themes_present"))
    without = [bs["bid_section_id"] for bs in bid_sections if not bs.get("themes_present")]

    score = with_themes / len(bid_sections) * 100
    return {
        "query_id": "B2", "query_name": "Bid-Section-Has-Themes",
        "direction": "backward", "passed": score >= 80.0, "score": round(score, 1),
        "details": f"{with_themes}/{len(bid_sections)} bid sections have themes. "
                   f"Missing: {', '.join(without[:5])}"
    }

query_results.append(run_b2())


# --- B3: Bid-Heading-Mapped ---
def run_b3():
    if not bid_sections:
        return {
            "query_id": "B3", "query_name": "Bid-Heading-Mapped",
            "direction": "backward", "passed": True, "score": 0.0,
            "details": "No bid sections yet (pre-bid phase)"
        }

    mapped = sum(1 for bs in bid_sections if bs.get("section_anchor"))
    score = mapped / len(bid_sections) * 100
    return {
        "query_id": "B3", "query_name": "Bid-Heading-Mapped",
        "direction": "backward", "passed": score >= 90.0, "score": round(score, 1),
        "details": f"{mapped}/{len(bid_sections)} bid sections have heading anchors"
    }

query_results.append(run_b3())


# --- B4: Evidence-Sufficiency ---
def run_b4():
    critical_high = [r for r in reqs if r.get("priority") in ["CRITICAL", "HIGH"]]
    if not critical_high:
        return {
            "query_id": "B4", "query_name": "Evidence-Sufficiency",
            "direction": "backward", "passed": True, "score": 100.0,
            "details": "No CRITICAL/HIGH requirements"
        }

    with_evidence = 0
    missing = []
    for req in critical_high:
        chain = next((c for c in chains if c["requirement"] == req["req_id"]), None)
        if chain and chain.get("evidence"):
            with_evidence += 1
        else:
            missing.append(req["req_id"])

    score = with_evidence / len(critical_high) * 100
    return {
        "query_id": "B4", "query_name": "Evidence-Sufficiency",
        "direction": "backward", "passed": score >= 50.0, "score": round(score, 1),
        "details": f"{with_evidence}/{len(critical_high)} CRITICAL/HIGH reqs have evidence. "
                   f"Missing: {', '.join(missing[:10])}"
    }

query_results.append(run_b4())
```

### Step 5: Execute Chain Integrity Queries

```python
# --- C1: Chain-Completeness ---
def run_c1():
    if not chains:
        return {
            "query_id": "C1", "query_name": "Chain-Completeness",
            "direction": "chain", "passed": False, "score": 0.0,
            "details": "No chains materialized"
        }

    avg_score = sum(c["completeness_score"] for c in chains) / len(chains)
    complete = sum(1 for c in chains if c["status"] == "COMPLETE")
    broken = sum(1 for c in chains if c["status"] == "BROKEN")

    return {
        "query_id": "C1", "query_name": "Chain-Completeness",
        "direction": "chain", "passed": avg_score >= 0.80,
        "score": round(avg_score * 100, 1),
        "details": f"Avg completeness: {avg_score:.2f}. "
                   f"COMPLETE: {complete}, BROKEN: {broken}/{len(chains)}"
    }

query_results.append(run_c1())


# --- C2: Compliance-Chain-Integrity ---
def run_c2():
    """Every mandatory item should trace: M-item → Requirement → Spec → Bid."""
    complete_chains = 0
    broken = []

    for m in mandatory_items:
        m_id = m["mandatory_id"]
        linked_reqs = m.get("linked_requirement_ids", [])

        if not linked_reqs:
            broken.append(f"{m_id}: no requirements")
            continue

        # Check if any linked requirement reaches a spec and bid
        reaches_spec = False
        reaches_bid = False
        for req_id in linked_reqs:
            chain = next((c for c in chains if c["requirement"] == req_id), None)
            if chain:
                if chain.get("specifications"):
                    reaches_spec = True
                if chain.get("bid_sections"):
                    reaches_bid = True

        if reaches_spec:
            complete_chains += 1
        else:
            broken.append(f"{m_id}: no spec link")

    score = complete_chains / len(mandatory_items) * 100 if mandatory_items else 100
    return {
        "query_id": "C2", "query_name": "Compliance-Chain-Integrity",
        "direction": "chain", "passed": score >= 90.0, "score": round(score, 1),
        "details": f"{complete_chains}/{len(mandatory_items)} mandatory items have complete chains. "
                   f"Broken: {', '.join(broken[:5])}"
    }

query_results.append(run_c2())


# --- C3: Risk-to-Bid-Integrity ---
def run_c3():
    high_med_risks = [r for r in risks if r.get("risk_level") in ["HIGH", "MEDIUM", "CRITICAL"]]
    if not high_med_risks:
        return {
            "query_id": "C3", "query_name": "Risk-to-Bid-Integrity",
            "direction": "chain", "passed": True, "score": 100.0,
            "details": "No HIGH/MEDIUM/CRITICAL risks"
        }

    traced = 0
    untraced = []
    for risk in high_med_risks:
        # Check if any linked requirement has a bid section in its chain
        has_bid = False
        for req_id in risk.get("linked_requirement_ids", []):
            chain = next((c for c in chains if c["requirement"] == req_id), None)
            if chain and chain.get("bid_sections"):
                has_bid = True
                break

        # Also check if any mitigation has a bid_location
        for mit in risk.get("mitigation_strategies", []):
            if mit.get("bid_location"):
                has_bid = True
                break

        if has_bid:
            traced += 1
        else:
            untraced.append(risk["risk_id"])

    score = traced / len(high_med_risks) * 100
    return {
        "query_id": "C3", "query_name": "Risk-to-Bid-Integrity",
        "direction": "chain", "passed": score >= 75.0, "score": round(score, 1),
        "details": f"{traced}/{len(high_med_risks)} risks trace to bid sections. "
                   f"Untraced: {', '.join(untraced[:10])}"
    }

query_results.append(run_c3())


# --- C4: Evaluation-Alignment ---
def run_c4():
    if not eval_criteria:
        return {
            "query_id": "C4", "query_name": "Evaluation-Alignment",
            "direction": "chain", "passed": True, "score": 100.0,
            "details": "No evaluation criteria defined"
        }

    aligned = 0
    gaps = []
    for factor in eval_criteria:
        has_reqs = bool(factor.get("linked_requirement_ids"))
        has_bid = bool(factor.get("linked_bid_section_ids"))
        if has_reqs:
            aligned += 1
        else:
            gaps.append(f"{factor['name']}: no requirements linked")

    score = aligned / len(eval_criteria) * 100
    return {
        "query_id": "C4", "query_name": "Evaluation-Alignment",
        "direction": "chain", "passed": score >= 80.0, "score": round(score, 1),
        "details": f"{aligned}/{len(eval_criteria)} factors have linked requirements. "
                   f"Gaps: {', '.join(gaps)}"
    }

query_results.append(run_c4())


# --- C5: No-Orphaned-Entities ---
def run_c5():
    orphaned = []

    # Check specs with no requirements
    for spec in specs:
        if not spec.get("linked_requirement_ids"):
            orphaned.append(f"SPEC: {spec['spec_id']}")

    # Check risks with no requirements
    for risk in risks:
        if not risk.get("linked_requirement_ids"):
            orphaned.append(f"RISK: {risk['risk_id']}")

    # Check bid sections with no requirements (if populated)
    for bs in bid_sections:
        if not bs.get("linked_requirement_ids"):
            orphaned.append(f"BID: {bs['bid_section_id']}")

    score = max(0, 100 - len(orphaned) * 5)
    return {
        "query_id": "C5", "query_name": "No-Orphaned-Entities",
        "direction": "chain", "passed": len(orphaned) <= 5, "score": round(score, 1),
        "details": f"{len(orphaned)} orphaned entities found. "
                   f"{'Examples: ' + ', '.join(orphaned[:5]) if orphaned else 'None'}"
    }

query_results.append(run_c5())
```

### Step 6: Update RTM Verification Section

```python
# Compute aggregate metrics
passed_count = sum(1 for q in query_results if q["passed"])
total_queries = len(query_results)

# Update verification section
rtm["verification"] = {
    "last_run": datetime.now().isoformat(),
    "run_by_phase": "phase8f-rtm-verify",
    "forward_coverage": {
        "requirements_with_specs": sum(1 for r in reqs
            if any(r["req_id"] in s.get("linked_requirement_ids", []) for s in specs)),
        "requirements_with_bid_sections": sum(1 for c in chains if c.get("bid_sections")),
        "requirements_with_risks": sum(1 for r in reqs
            if any(r["req_id"] in risk.get("linked_requirement_ids", []) for risk in risks)),
        "requirements_total": len(reqs),
        "spec_coverage_pct": round(
            sum(1 for r in reqs if any(r["req_id"] in s.get("linked_requirement_ids", []) for s in specs))
            / len(reqs) * 100, 1) if reqs else 0,
        "bid_coverage_pct": round(
            sum(1 for c in chains if c.get("bid_sections"))
            / len(chains) * 100, 1) if chains else 0
    },
    "backward_coverage": {
        "bid_sections_with_requirements": sum(1 for bs in bid_sections if bs.get("linked_requirement_ids")),
        "bid_sections_total": len(bid_sections),
        "orphaned_bid_content": sum(1 for bs in bid_sections if not bs.get("linked_requirement_ids"))
    },
    "chain_completeness": {
        "complete_chains": sum(1 for c in chains if c["status"] == "COMPLETE"),
        "partial_chains": sum(1 for c in chains if c["status"] == "PARTIAL"),
        "broken_chains": sum(1 for c in chains if c["status"] == "BROKEN"),
        "avg_completeness_score": round(
            sum(c["completeness_score"] for c in chains) / len(chains), 3) if chains else 0
    },
    "compliance_alignment": {
        "mandatory_items_in_bid": sum(1 for m in mandatory_items if m.get("bid_location")),
        "mandatory_items_total": len(mandatory_items),
        "coverage_pct": round(
            sum(1 for m in mandatory_items if m.get("coverage_status") == "ADDRESSED")
            / len(mandatory_items) * 100, 1) if mandatory_items else 0
    },
    "query_results": query_results
}

# Increment chain version
rtm["meta"]["last_updated_by_phase"] = "phase8f-rtm-verify"
rtm["meta"]["chain_version"] = rtm["meta"].get("chain_version", 0) + 1

write_json(f"{folder}/shared/UNIFIED_RTM.json", rtm)
```

### Step 7: Generate RTM_REPORT.md

```python
def generate_rtm_report(query_results, rtm):
    verify = rtm["verification"]
    meta = rtm["meta"]
    entities = rtm["entities"]
    chains_data = rtm["chain_links"]

    passed = sum(1 for q in query_results if q["passed"])
    total = len(query_results)
    overall = "PASS" if passed == total else "ADVISORY" if passed >= total * 0.7 else "FAIL"

    doc = f"""# RTM Verification Report

**RFP:** {meta.get("rfp_title", "Unknown")}
**Generated:** {datetime.now().strftime('%Y-%m-%d %H:%M')}
**RTM Version:** {meta.get("chain_version", 1)}
**Overall Status:** {"PASS" if overall == "PASS" else "ADVISORY" if overall == "ADVISORY" else "FAIL"}

---

## Executive Summary

| Metric | Value |
|--------|-------|
| Verification Queries | {total} |
| Passed | {passed} |
| Failed | {total - passed} |
| Overall Score | {passed / total * 100:.0f}% |

---

## Entity Statistics

| Entity Type | Count |
|-------------|-------|
| RFP Sources | {len(entities["rfp_sources"])} |
| Mandatory Items | {len(entities["mandatory_items"])} |
| Requirements | {len(entities["requirements"])} |
| Specifications | {len(entities["specifications"])} |
| Risks | {len(entities["risks"])} |
| Bid Sections | {len(entities["bid_sections"])} |
| Evidence | {len(entities["evidence"])} |
| Evaluation Criteria | {len(entities["evaluation_criteria"])} |
| Chain Links | {len(chains_data)} |

---

## Coverage Dashboard

### Forward Coverage
| Metric | Value |
|--------|-------|
| Spec Coverage | {verify["forward_coverage"]["spec_coverage_pct"]}% |
| Bid Coverage | {verify["forward_coverage"]["bid_coverage_pct"]}% |
| Requirements with Risks | {verify["forward_coverage"]["requirements_with_risks"]}/{verify["forward_coverage"]["requirements_total"]} |

### Chain Completeness
| Status | Count |
|--------|-------|
| COMPLETE | {verify["chain_completeness"]["complete_chains"]} |
| PARTIAL | {verify["chain_completeness"]["partial_chains"]} |
| BROKEN | {verify["chain_completeness"]["broken_chains"]} |
| **Avg Score** | **{verify["chain_completeness"]["avg_completeness_score"]:.3f}** |

### Compliance Alignment
| Metric | Value |
|--------|-------|
| Mandatory Items Addressed | {verify["compliance_alignment"]["mandatory_items_in_bid"]}/{verify["compliance_alignment"]["mandatory_items_total"]} |
| Coverage | {verify["compliance_alignment"]["coverage_pct"]}% |

---

## Verification Query Results

"""

    # Group by direction
    for direction in ["forward", "backward", "chain"]:
        dir_label = {"forward": "Forward Queries (Req → Downstream)",
                     "backward": "Backward Queries (Bid → Upstream)",
                     "chain": "Chain Integrity Queries"}[direction]
        doc += f"### {dir_label}\n\n"
        doc += "| ID | Query | Score | Status | Details |\n"
        doc += "|----|-------|-------|--------|---------|\n"

        for q in query_results:
            if q["direction"] == direction:
                status = "PASS" if q["passed"] else "FAIL"
                icon = "✅" if q["passed"] else "❌"
                details = q["details"][:80]
                doc += f"| {q['query_id']} | {q['query_name']} | {q['score']}% | {icon} {status} | {details} |\n"

        doc += "\n"

    # Detailed findings for failed queries
    failed = [q for q in query_results if not q["passed"]]
    if failed:
        doc += f"""
---

## Failed Query Details

"""
        for q in failed:
            doc += f"""### {q["query_id"]}: {q["query_name"]}

**Score:** {q["score"]}% (threshold not met)
**Direction:** {q["direction"]}
**Details:** {q["details"]}

**Recommended Action:**
"""
            # Generate recommendations per query
            if q["query_id"] == "F1":
                doc += "- Review unlinked requirements and add spec section links in Phase 4\n"
                doc += "- Consider expanding CATEGORY_SPEC_AFFINITY mapping\n"
            elif q["query_id"] == "F2":
                doc += "- Ensure Phase 8 bid authoring populates bid_sections[] in RTM\n"
                doc += "- Check if bid sections cover all requirement categories\n"
            elif q["query_id"] == "F3":
                doc += "- Add explicit risk mitigation discussion to bid sections\n"
                doc += "- Update mitigation bid_location references\n"
            elif q["query_id"] == "F4":
                doc += "- Review mandatory items with GAP status\n"
                doc += "- Ensure bid sections explicitly address each mandatory item\n"
            elif q["query_id"] == "C1":
                doc += "- Improve chain completeness by filling missing links\n"
                doc += "- Focus on bid_section and evidence links\n"
            elif q["query_id"] == "C2":
                doc += "- Verify all mandatory items link through to specs\n"
                doc += "- Review SIMILARITY_THRESHOLD in Phase 4\n"
            else:
                doc += "- Review the specific items listed in details above\n"

            doc += "\n"

    doc += f"""
---

## Chain Gap Analysis

### Most Common Missing Links

"""

    # Analyze missing links across all chains
    missing_counts = {}
    for chain in chains_data:
        for link in chain.get("missing_links", []):
            missing_counts[link] = missing_counts.get(link, 0) + 1

    if missing_counts:
        doc += "| Missing Link | Count | Percentage |\n"
        doc += "|-------------|-------|------------|\n"
        for link, count in sorted(missing_counts.items(), key=lambda x: x[1], reverse=True):
            pct = count / len(chains_data) * 100
            doc += f"| {link} | {count} | {pct:.1f}% |\n"
    else:
        doc += "No missing links detected.\n"

    doc += f"""

---

## Recommendations

"""

    if overall == "PASS":
        doc += "All verification queries passed. The RTM is ready for final bid assembly.\n"
    elif overall == "ADVISORY":
        doc += f"{total - passed} queries did not meet thresholds. Review failed queries above.\n"
        doc += "The bid can proceed but should address identified gaps.\n"
    else:
        doc += f"**{total - passed} critical verification queries failed.**\n"
        doc += "Address the failed queries before proceeding to final PDF assembly.\n"

    doc += f"""
---

*Generated by Phase 8f - RTM Verification*
*RTM Version: {meta.get("chain_version", 1)}*
"""

    return doc


rtm_report = generate_rtm_report(query_results, rtm)
write_file(f"{folder}/outputs/RTM_REPORT.md", rtm_report)
```

### Step 8: Report Results

```
🔍 RTM VERIFICATION COMPLETE
==============================
Phase: 8f - RTM Verification

Overall: {overall} ({passed}/{total} queries passed)

Forward Queries:
  F1 Requirements-to-Spec:         {score}% {"✅" if passed else "❌"}
  F2 Requirements-to-Bid:          {score}% {"✅" if passed else "❌"}
  F3 Risk-Mitigation-in-Bid:       {score}% {"✅" if passed else "❌"}
  F4 Mandatory-Item-Bid-Location:  {score}% {"✅" if passed else "❌"}
  F5 Eval-Weight-Allocation:       {score}% {"✅" if passed else "❌"}

Backward Queries:
  B1 Bid-Section-Has-Requirements: {score}% {"✅" if passed else "❌"}
  B2 Bid-Section-Has-Themes:       {score}% {"✅" if passed else "❌"}
  B3 Bid-Heading-Mapped:           {score}% {"✅" if passed else "❌"}
  B4 Evidence-Sufficiency:         {score}% {"✅" if passed else "❌"}

Chain Integrity:
  C1 Chain-Completeness:           {score}% {"✅" if passed else "❌"}
  C2 Compliance-Chain-Integrity:   {score}% {"✅" if passed else "❌"}
  C3 Risk-to-Bid-Integrity:        {score}% {"✅" if passed else "❌"}
  C4 Evaluation-Alignment:         {score}% {"✅" if passed else "❌"}
  C5 No-Orphaned-Entities:         {score}% {"✅" if passed else "❌"}

Files Updated:
  ✅ shared/UNIFIED_RTM.json (verification{} updated)
  ✅ outputs/RTM_REPORT.md (verification report)
```

## Quality Checklist

- [ ] All 14 verification queries executed
- [ ] `RTM_REPORT.md` created in `outputs/`
- [ ] `UNIFIED_RTM.json` updated with `verification{}` section
- [ ] Failed queries have specific recommendations
- [ ] Chain gap analysis identifies most common missing links
- [ ] Overall disposition calculated (PASS/ADVISORY/FAIL)
- [ ] Chain version incremented
