---
name: phase1.6-evaluation-win
expert-role: Procurement Specialist
domain-expertise: RFP evaluation criteria, scoring methodologies
---

# Phase 1.6: Evaluation Criteria Analysis

## Expert Role

You are a **Procurement Specialist** with deep expertise in:
- RFP evaluation criteria extraction
- Scoring methodologies (LPTA, Best Value, Quality-Based)
- Weight assignment and factor analysis
- Government and enterprise procurement processes

## Purpose

Extract evaluation criteria, weights, and selection method from RFP documents to inform bid strategy.

## Inputs

- `{folder}/flattened/*.md` - Flattened RFP documents
- `{folder}/shared/domain-context.json` - Domain context

## Required Outputs

- `{folder}/shared/EVALUATION_CRITERIA.json` - Extracted criteria with weights

## Instructions

### Step 1: Load Documents

```python
import glob

flattened_files = glob.glob(f"{folder}/flattened/*.md")
combined_content = ""

for file_path in flattened_files:
    combined_content += read_file(file_path) + "\n\n"
```

### Step 2: Identify Evaluation Sections

Look for sections containing evaluation criteria:

```python
SECTION_PATTERNS = [
    r"evaluation\s+(criteria|factors?|methodology)",
    r"selection\s+(criteria|process|method)",
    r"scoring\s+(criteria|methodology|matrix)",
    r"award\s+(criteria|basis|factors?)",
    r"proposal\s+evaluation",
    r"technical\s+evaluation",
    r"cost\s+evaluation"
]

evaluation_sections = []
for pattern in SECTION_PATTERNS:
    matches = re.finditer(pattern, combined_content, re.IGNORECASE)
    for match in matches:
        # Extract surrounding context (500 chars before and after)
        start = max(0, match.start() - 500)
        end = min(len(combined_content), match.end() + 2000)
        evaluation_sections.append(combined_content[start:end])
```

### Step 3: Detect Selection Method

```python
SELECTION_METHODS = {
    "LPTA": {
        "signals": ["lowest price technically acceptable", "lpta", "lowest cost"],
        "description": "Lowest Price Technically Acceptable - Price is primary factor"
    },
    "Best Value": {
        "signals": ["best value", "trade-off", "tradeoff", "price/technical"],
        "description": "Best Value Trade-Off - Balance of price and technical"
    },
    "Quality-Based": {
        "signals": ["quality based", "qualifications based", "qbs", "technical only"],
        "description": "Quality-Based Selection (FAR 36.6 / Brooks Act — separate from FAR Part 15) - Technical merit primary"
    }
}

def detect_selection_method(content):
    content_lower = content.lower()
    for method, config in SELECTION_METHODS.items():
        for signal in config["signals"]:
            if signal in content_lower:
                return method, config["description"]
    return "Best Value", "Default assumption - Best Value Trade-Off"

selection_method, method_description = detect_selection_method(combined_content)
```

### Step 4: Extract Evaluation Factors

```python
COMMON_FACTORS = [
    {"name": "Technical Approach", "aliases": ["technical solution", "technical merit", "approach"]},
    {"name": "Past Performance", "aliases": ["experience", "prior experience", "references"]},
    {"name": "Management Approach", "aliases": ["project management", "management plan", "staffing"]},
    {"name": "Price/Cost", "aliases": ["cost", "pricing", "price proposal", "cost proposal"]},
    {"name": "Corporate Experience", "aliases": ["company experience", "relevant experience"]},
    {"name": "Key Personnel", "aliases": ["staff qualifications", "personnel", "team"]},
    {"name": "Small Business", "aliases": ["small business participation", "subcontracting"]}
]

def extract_factors(content):
    factors = []
    content_lower = content.lower()

    for factor in COMMON_FACTORS:
        # Check if factor mentioned
        if factor["name"].lower() in content_lower or any(a in content_lower for a in factor["aliases"]):
            # Try to extract weight
            weight = extract_weight(content, factor["name"], factor["aliases"])
            factors.append({
                "name": factor["name"],
                "weight": weight,
                "detected": True
            })

    return factors

def extract_weight(content, name, aliases):
    """Extract percentage weight for a factor."""
    patterns = [
        rf"{name}\s*[:\-]\s*(\d+)\s*%",
        rf"(\d+)\s*%\s*{name}",
        rf"{name}\s*\((\d+)\s*%\)"
    ]

    for alias in aliases:
        patterns.extend([
            rf"{alias}\s*[:\-]\s*(\d+)\s*%",
            rf"(\d+)\s*%\s*{alias}"
        ])

    for pattern in patterns:
        match = re.search(pattern, content, re.IGNORECASE)
        if match:
            return int(match.group(1))

    return None  # Weight not found

factors = extract_factors(combined_content)
```

### Step 5: Infer Missing Weights

```python
def infer_weights(factors, selection_method):
    """Assign default weights based on selection method."""
    DEFAULT_WEIGHTS = {
        "LPTA": {
            "Technical Approach": 40,
            "Past Performance": 20,
            "Price/Cost": 40,
            "Management Approach": 0,
            "Corporate Experience": 0
        },
        "Best Value": {
            "Technical Approach": 40,
            "Past Performance": 20,
            "Price/Cost": 25,
            "Management Approach": 10,
            "Corporate Experience": 5
        },
        "Quality-Based": {
            "Technical Approach": 50,
            "Past Performance": 25,
            "Price/Cost": 10,
            "Management Approach": 10,
            "Corporate Experience": 5
        }
    }

    defaults = DEFAULT_WEIGHTS.get(selection_method, DEFAULT_WEIGHTS["Best Value"])

    for factor in factors:
        if factor["weight"] is None:
            factor["weight"] = defaults.get(factor["name"], 10)
            factor["weight_inferred"] = True
        else:
            factor["weight_inferred"] = False

    return factors

factors = infer_weights(factors, selection_method)
```

### Step 6: Normalize Weights

```python
def normalize_weights(factors):
    """Ensure weights sum to 100%."""
    total = sum(f["weight"] for f in factors if f["weight"])

    if total != 100 and total > 0:
        for factor in factors:
            if factor["weight"]:
                factor["weight_normalized"] = round(factor["weight"] * 100 / total)
            else:
                factor["weight_normalized"] = factor["weight"]
    else:
        for factor in factors:
            factor["weight_normalized"] = factor["weight"]

    return factors

factors = normalize_weights(factors)
```

### Step 7: Write Output

```python
## RTM CONTRIBUTION: Assign stable factor_id and subfactor_id for UNIFIED_RTM.json

# Assign stable IDs to each evaluation factor (EVAL-01, EVAL-02, etc.)
for idx, f in enumerate(factors, 1):
    f["factor_id"] = f"EVAL-{idx:02d}"
    # If subfactors are detected, assign subfactor IDs (EVAL-01-01, EVAL-01-02, etc.)
    subfactors = f.get("subfactors", [])
    for sub_idx, sf in enumerate(subfactors, 1):
        sf["subfactor_id"] = f"EVAL-{idx:02d}-{sub_idx:02d}"

# Calculate points based on normalized weight (out of 1000 total for scoring)
total_points = 1000
for f in factors:
    f["points"] = round(f["weight_normalized"] * total_points / 100)

evaluation_criteria = {
    "extracted_at": datetime.now().isoformat(),
    "selection_method": {
        "type": selection_method,
        "description": method_description,
        "confidence": "high" if selection_method != "Best Value" else "inferred"
    },
    "evaluation_factors": [
        {
            "factor_id": f["factor_id"],  # RTM: Stable ID for UNIFIED_RTM.json linking
            "name": f["name"],
            "points": f["points"],  # RTM: Points for composite scoring
            "weight_explicit": f["weight"] if not f.get("weight_inferred") else None,
            "weight_inferred": f["weight"] if f.get("weight_inferred") else None,
            "weight_normalized": f["weight_normalized"],
            "detected_in_rfp": f["detected"],
            "bid_section_mapping": EVALUATION_TO_BID_MAPPING.get(f["name"], {}),
            "subfactors": [
                {
                    "subfactor_id": sf.get("subfactor_id"),
                    "name": sf.get("name"),
                    "points": sf.get("points", 0)
                }
                for sf in f.get("subfactors", [])
            ]
        }
        for f in factors
    ],
    "total_weight": sum(f["weight_normalized"] for f in factors),
    "factors_with_explicit_weights": sum(1 for f in factors if not f.get("weight_inferred")),
    "recommendations": generate_recommendations(selection_method, factors),
    "bid_structure_guidance": bid_structure_guidance,
    "section_order_recommendations": section_order_recommendations
}

write_json(f"{folder}/shared/EVALUATION_CRITERIA.json", evaluation_criteria)
```

### Step 8: Generate Recommendations

```python
def generate_recommendations(selection_method, factors):
    recommendations = []

    if selection_method == "LPTA":
        recommendations.append({
            "priority": "high",
            "message": "Focus on meeting minimum technical requirements efficiently",
            "action": "Minimize cost while meeting all mandatory requirements"
        })
    elif selection_method == "Best Value":
        tech_weight = next((f["weight"] for f in factors if f["name"] == "Technical Approach"), 0)
        price_weight = next((f["weight"] for f in factors if f["name"] == "Price/Cost"), 0)

        if tech_weight > price_weight:
            recommendations.append({
                "priority": "high",
                "message": "Technical approach weighted higher than price",
                "action": "Invest in strong technical narrative and innovation"
            })

    # Past performance recommendation
    pp_weight = next((f["weight"] for f in factors if f["name"] == "Past Performance"), 0)
    if pp_weight >= 20:
        recommendations.append({
            "priority": "medium",
            "message": f"Past Performance weighted at {pp_weight}%",
            "action": "Prepare strong references and case studies"
        })

    return recommendations
```

### Step 8b: Map Evaluation Criteria to Bid Sections

```python
# Evaluation Factor → Bid Section mapping
# This drives bid structure to align with how evaluators will score

EVALUATION_TO_BID_MAPPING = {
    "Technical Approach": {
        "primary_section": "solution.md",
        "subsections": ["Technical Solution", "Architecture Overview", "Implementation Methodology"],
        "emphasis": "Demonstrate technical depth, innovation, and feasibility",
        "win_themes": ["Modern Architecture", "Domain Expertise"]
    },
    "Past Performance": {
        "primary_section": "title-page.md",
        "subsections": ["Executive Summary", "Case Studies"],
        "emphasis": "Highlight relevant experience with quantified results",
        "win_themes": ["Domain Expertise", "Risk Mitigation"]
    },
    "Management Approach": {
        "primary_section": "timeline.md",
        "subsections": ["Team Structure", "Project Governance", "Communication Plan"],
        "emphasis": "Show clear accountability and proven management methodology",
        "win_themes": ["Partnership Approach"]
    },
    "Price/Cost": {
        "primary_section": "timeline.md",
        "subsections": ["Investment Summary", "Pricing Structure"],
        "emphasis": "Transparent pricing with clear value justification",
        "win_themes": ["Risk Mitigation"]
    },
    "Corporate Experience": {
        "primary_section": "title-page.md",
        "subsections": ["Company Overview", "Relevant Experience"],
        "emphasis": "Demonstrate organizational capability and stability",
        "win_themes": ["Domain Expertise", "Compliance Excellence"]
    },
    "Key Personnel": {
        "primary_section": "timeline.md",
        "subsections": ["Team Structure", "Key Personnel Bios"],
        "emphasis": "Highlight qualifications and availability of key staff",
        "win_themes": ["Domain Expertise"]
    },
    "Small Business": {
        "primary_section": "timeline.md",
        "subsections": ["Subcontracting Plan"],
        "emphasis": "Show commitment to small business participation goals",
        "win_themes": ["Partnership Approach"]
    }
}

def generate_bid_structure_guidance(factors):
    """
    Generate bid section structure guidance based on evaluation weights.
    Higher weighted factors get more emphasis (page allocation, placement).
    """

    # Sort factors by weight (highest first)
    sorted_factors = sorted(factors, key=lambda f: f.get("weight_normalized", 0), reverse=True)

    bid_structure = {
        "title-page.md": {
            "sections": [],
            "total_weight": 0,
            "page_allocation": "4-6 pages",
            "guidance": []
        },
        "solution.md": {
            "sections": [],
            "total_weight": 0,
            "page_allocation": "15-25 pages",
            "guidance": []
        },
        "timeline.md": {
            "sections": [],
            "total_weight": 0,
            "page_allocation": "8-12 pages",
            "guidance": []
        }
    }

    for factor in sorted_factors:
        factor_name = factor.get("name")
        weight = factor.get("weight_normalized", 0)
        mapping = EVALUATION_TO_BID_MAPPING.get(factor_name, {})

        primary = mapping.get("primary_section", "solution.md")

        if primary in bid_structure:
            bid_structure[primary]["sections"].append({
                "factor": factor_name,
                "weight": weight,
                "subsections": mapping.get("subsections", []),
                "emphasis": mapping.get("emphasis", ""),
                "win_themes_to_include": mapping.get("win_themes", [])
            })
            bid_structure[primary]["total_weight"] += weight

    # Generate guidance based on weights
    for section_name, section in bid_structure.items():
        total_w = section["total_weight"]
        if total_w >= 50:
            section["guidance"].append("CRITICAL: This section addresses majority of scoring weight")
            section["guidance"].append("Allocate extra pages and strongest content here")
        elif total_w >= 25:
            section["guidance"].append("IMPORTANT: Significant scoring weight in this section")
        else:
            section["guidance"].append("STANDARD: Support content, ensure completeness")

        # Order subsections by weight
        section["sections"] = sorted(section["sections"],
                                      key=lambda s: s["weight"], reverse=True)

    return bid_structure


def generate_section_order_recommendation(bid_structure):
    """
    Generate recommended section order within each bid document based on evaluator expectations.
    Rule: Lead with highest-weighted content.
    """

    recommendations = {}

    for section_name, section in bid_structure.items():
        ordered_subsections = []

        for factor_section in section.get("sections", []):
            for subsection in factor_section.get("subsections", []):
                ordered_subsections.append({
                    "subsection": subsection,
                    "factor": factor_section["factor"],
                    "weight": factor_section["weight"],
                    "emphasis": factor_section["emphasis"]
                })

        # Sort by weight
        ordered_subsections.sort(key=lambda x: x["weight"], reverse=True)

        recommendations[section_name] = {
            "recommended_order": [s["subsection"] for s in ordered_subsections],
            "weight_distribution": ordered_subsections,
            "lead_with": ordered_subsections[0] if ordered_subsections else None
        }

    return recommendations


# Generate bid structure guidance
bid_structure_guidance = generate_bid_structure_guidance(factors)
section_order_recommendations = generate_section_order_recommendation(bid_structure_guidance)
```

### Step 9: Report Results

```
📊 Evaluation Criteria Analysis Complete
=========================================
Selection Method: {selection_method}
  {method_description}

Evaluation Factors:
| Factor | Weight | Source |
|--------|--------|--------|
{table rows}

Total Weight: {total}%
Explicit Weights Found: {count}
Inferred Weights: {count}

Recommendations:
{recommendations}

📝 Bid Structure Guidance (by Evaluation Weight):
=================================================

{For each bid section, show weight allocation}

title-page.md ({total_weight}% of score):
  Lead with: {highest_weighted_factor}
  Sections: {ordered_list}
  Guidance: {guidance}

solution.md ({total_weight}% of score):
  Lead with: {highest_weighted_factor}
  Sections: {ordered_list}
  Guidance: {guidance}

timeline.md ({total_weight}% of score):
  Lead with: {highest_weighted_factor}
  Sections: {ordered_list}
  Guidance: {guidance}

⚠️ CRITICAL: Structure bid sections to lead with highest-weighted evaluation factors.
   Evaluators scan for what they're scoring - put it where they'll find it.
```

## Quality Checklist

- [ ] `EVALUATION_CRITERIA.json` created in `shared/`
- [ ] Selection method detected or reasonably inferred
- [ ] All detected factors have weights (explicit or inferred)
- [ ] Weights normalized to 100%
- [ ] Recommendations generated based on criteria
- [ ] **Bid structure guidance** generated mapping factors → bid sections
- [ ] **Section order recommendations** prioritize highest-weighted factors
- [ ] **Win themes** linked to evaluation factors for consistent threading
