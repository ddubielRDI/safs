---
name: phase4-compliance
expert-role: Compliance Officer & Bid Strategist
domain-expertise: Procurement compliance, proposal past performance, project matching
---

# Phase 4: Compliance Quick-Check & Past Project Match

**Expert Role:** Compliance Officer & Bid Strategist

**Purpose:** Two outputs: (1) Compliance gap analysis against company profile, (2) Top 5 past project matches scored by relevance. These feed into the final recommendation.

**Inputs:**
- Combined RFP text (in memory)
- `{folder}/screen/rfp-summary.json` — Extracted metadata
- `{folder}/screen/go-nogo-score.json` — Scoring context
- Company profile: `config-win/company-profile.json`
- `Past_Projects.md` — 28 case studies in repo root

**Required Outputs:**
- `{folder}/screen/compliance-check.json` (>1KB)
- `{folder}/screen/past-projects-match.json` (>1KB)

---

## Part A: Compliance Quick-Check

### Step 1: Extract Compliance Items from RFP Text

```python
import re

compliance_items = []

# Mandatory qualifications
qual_patterns = [
    r"(?:must\s+have|required|mandatory|minimum)\s+(?:qualification|requirement|experience)[:\s]+([^\n]{10,200})",
    r"(?:shall|must)\s+(?:demonstrate|possess|provide|have)\s+([^\n]{10,150})",
]

# Certification requirements
cert_patterns = [
    r"(?:certif(?:ied|ication)|accredit(?:ed|ation))\s+(?:in|for|as)?\s*([^\n]{5,100})",
    r"(PMP|CISSP|AWS\s+Certified|Azure\s+Certified|ITIL|ISO\s+\d+|SOC\s+\d|FedRAMP|CMMC)",
]

# Insurance/bonding
insurance_patterns = [
    r"(?:insurance|bonding|liability)\s+(?:requirement|minimum|of)\s*[:\s]+([^\n]{10,100})",
    r"(?:professional\s+liability|errors?\s+(?:and|&)\s+omissions?|workers?\s+comp)[:\s]+([^\n]{10,100})",
]

# Set-aside/preference
setaside_patterns = [
    r"(?:set.aside|restricted|limited)\s+(?:to|for)\s+([^\n]{10,100})",
    r"(small\s+business|8\(a\)|HUBZone|SDVOSB|WOSB|veteran.owned|minority.owned|MBE|WBE|DBE)",
]

for patterns, category in [
    (qual_patterns, "qualification"),
    (cert_patterns, "certification"),
    (insurance_patterns, "insurance"),
    (setaside_patterns, "set_aside")
]:
    for pattern in patterns:
        matches = re.findall(pattern, combined_text, re.IGNORECASE)
        for match in matches:
            compliance_items.append({
                "requirement": match.strip()[:200],
                "category": category,
                "source": "rfp_text"
            })
```

### Step 2: Cross-Reference Against Company Profile

```python
company = read_json(COMPANY_PROFILE)

# CRITICAL: services is a DICT not list — flatten it
rdi_services = [svc.lower() for cat in company.get("services", {}).values() for svc in cat]
rdi_capabilities = [cap.lower() for cap in company.get("capabilities_summary", [])]
rdi_certs = company.get("bid_defaults", {}).get("certifications", [])
rdi_industries = [i.lower() for i in company.get("industries", [])]

# Score each compliance item
for item in compliance_items:
    req_lower = item["requirement"].lower()

    # Check against services, capabilities, certs, industries
    matched = False
    for source_list, source_name in [
        (rdi_services, "services"),
        (rdi_capabilities, "capabilities"),
        (rdi_certs, "certifications"),
        (rdi_industries, "industries")
    ]:
        for source_item in source_list:
            if isinstance(source_item, str) and "[USER INPUT" not in source_item:
                if source_item.lower() in req_lower or req_lower in source_item.lower():
                    item["status"] = "PASS"
                    item["match_source"] = source_name
                    item["match_detail"] = source_item
                    matched = True
                    break
        if matched:
            break

    if not matched:
        # Check if it's a hard gap, partial match, or risk
        if item["category"] in ["set_aside", "insurance"]:
            item["status"] = "RISK"
            item["match_source"] = None
            item["match_detail"] = "Cannot verify — requires manual confirmation"
        else:
            # Check for partial match — any service/capability words overlap
            req_words = set(req_lower.split())
            partial_match = False
            for source_list in [rdi_services, rdi_capabilities]:
                for source_item in source_list:
                    if isinstance(source_item, str) and "[USER INPUT" not in source_item:
                        source_words = set(source_item.lower().split())
                        if len(req_words & source_words) >= 2:
                            item["status"] = "PARTIAL"
                            item["match_source"] = "partial_capability"
                            item["match_detail"] = f"Partial overlap with: {source_item}"
                            partial_match = True
                            break
                if partial_match:
                    break
            if not partial_match:
                item["status"] = "GAP"
                item["match_source"] = None
                item["match_detail"] = "No matching capability found in company profile"
```

### Step 3: Write Compliance Output

```python
pass_count = sum(1 for i in compliance_items if i["status"] == "PASS")
partial_count = sum(1 for i in compliance_items if i["status"] == "PARTIAL")
gap_count = sum(1 for i in compliance_items if i["status"] == "GAP")
risk_count = sum(1 for i in compliance_items if i["status"] == "RISK")

# Enhancement: Auto-include Section 508 for government domain (FAR 39.2 mandate)
rfp_domain = (rfp_summary.get("industry_domain") or "").lower()
if rfp_domain == "government":
    has_508 = any("508" in item["requirement"] or "accessibility" in item["requirement"].lower() for item in compliance_items)
    if not has_508:
        compliance_items.append({
            "requirement": "Section 508 accessibility compliance (FAR 39.2 — auto-included for federal ICT)",
            "category": "certification",
            "source": "auto_detected",
            "status": "RISK",
            "match_source": None,
            "match_detail": "Auto-included per FAR Subpart 39.2 — verify WCAG 2.0 AA compliance capability"
        })
        risk_count += 1

# Enhancement: Auto-flag CMMC for DoD/defense procurement
combined_lower = combined_text.lower()
defense_signals = ["dod", "defense", "military", "department of defense"]
if any(signal in combined_lower for signal in defense_signals):
    has_cmmc = any("cmmc" in item["requirement"].lower() for item in compliance_items)
    if not has_cmmc:
        compliance_items.append({
            "requirement": "CMMC Level 1/2 certification (auto-flagged — DoD Phase 1 live Nov 2025)",
            "category": "certification",
            "source": "auto_detected",
            "status": "RISK",
            "match_source": None,
            "match_detail": "DoD domain detected — all new solicitations include CMMC requirements. Verify required level."
        })
        risk_count += 1

# Enhancement: FedRAMP 20x awareness
if "fedramp" in combined_lower or "fed-ramp" in combined_lower:
    compliance_items.append({
        "requirement": "FedRAMP authorization — NOTE: 20x modernization (March 2025+) reduced timelines to ~3 months; KSIs replace static checklists",
        "category": "certification",
        "source": "auto_detected",
        "status": "RISK",
        "match_source": None,
        "match_detail": "FedRAMP detected — verify current authorization status and 20x program alignment"
    })
    risk_count += 1

compliance_check = {
    "phase": "4a",
    "timestamp": datetime.now().isoformat(),
    "total_items": len(compliance_items),
    "summary": {
        "pass": pass_count,
        "partial": partial_count,
        "gap": gap_count,
        "risk": risk_count
    },
    "compliance_items": compliance_items,
    "overall_status": "PASS" if gap_count == 0 else ("RISK" if gap_count <= 2 else "CONCERN")
}
write_json(f"{folder}/screen/compliance-check.json", compliance_check)
```

---

## Part B: Past Project Matching

### Step 4: Load and Parse Past Projects

```python
# Read Past_Projects.md from repo root
past_projects_path = os.path.join(os.path.dirname(folder), "Past_Projects.md")
# Or try the known location
if not os.path.exists(past_projects_path):
    past_projects_path = "Past_Projects.md"

if not os.path.exists(past_projects_path):
    write_json(f"{folder}/screen/past-projects-match.json", {
        "phase": "4b",
        "timestamp": datetime.now().isoformat(),
        "status": "no_data",
        "note": "Past_Projects.md not found",
        "matched_projects": []
    })
    log("WARNING: Past_Projects.md not found. Skipping project matching.")
    return

past_projects_md = read_file(past_projects_path)
```

### Step 5: Parse Projects from Markdown

```python
# Projects start with "#### N. Project Title" headings
# Extract: number, title, client, industry, technologies, metrics, timeline
import re

projects = []
# Split by project headings — format: "#### N. Title"
project_sections = re.split(r'\n####\s+(\d+)\.\s+', past_projects_md)

# project_sections[0] is the preamble, then alternating: number, content
for i in range(1, len(project_sections)-1, 2):
    proj_num = int(project_sections[i])
    content = project_sections[i+1]

    # Extract title (first line)
    title = content.split('\n')[0].strip()

    # Extract fields from the content
    project = {
        "project_number": proj_num,
        "title": title,
        "content": content,
        "client": extract_field(content, "Client"),
        "industry": extract_field(content, "Industry"),
        "technologies": extract_technologies(content),
        "timeline": extract_field(content, "Timeline|Duration|Period"),
        "key_metrics": extract_metrics(content),
        "has_metrics_table": bool(re.search(r'\|.*\|.*\|', content)),
        "has_quote": bool(re.search(r'["""]', content) or re.search(r'testimonial', content, re.I)),
        "description": content[:500]
    }
    projects.append(project)
```

### Step 6: Score Each Project

Scoring algorithm (FAR 15.305 relevance factors): industry(10/5) + tech(3/match, max 15) + metrics(5) + quote(2) + recency(1-5) + scale(0-3) + contract_type(0-3) + dollar_proximity(0-3) = 0-46 max

```python
rfp_summary = read_json(f"{folder}/screen/rfp-summary.json")
rfp_domain = (rfp_summary.get("industry_domain") or "").lower()

# Prefer required_technologies (specific named products) over scope_keywords (generic terms)
# Fallback ensures backward compatibility with older rfp-summary.json files
tech_list = rfp_summary.get("required_technologies") or rfp_summary.get("scope_keywords", [])
rfp_keywords = [kw.lower() for kw in tech_list]

# Related industry mapping for partial matches (5pts instead of 10)
related_industries = {
    "government": ["education", "transportation", "utilities", "natural resources"],
    "education": ["government"],
    "natural resources": ["government", "oil & gas", "fisheries", "mining"],
    "oil & gas": ["natural resources", "mining", "manufacturing"],
    "fisheries": ["natural resources", "government"],
    "transportation": ["government", "manufacturing"],
    "utilities": ["government", "manufacturing"],
}

scored_projects = []
for project in projects:
    score = 0
    breakdown = {}

    # Industry match (max 10)
    proj_industry = (project.get("industry") or "").lower()
    if proj_industry == rfp_domain:
        breakdown["industry"] = 10
    elif proj_industry in related_industries.get(rfp_domain, []):
        breakdown["industry"] = 5
    else:
        breakdown["industry"] = 0
    score += breakdown["industry"]

    # Technology overlap (3pts per match, max 15)
    proj_techs = [t.lower() for t in project.get("technologies", [])]
    tech_matches = []
    for kw in rfp_keywords:
        for tech in proj_techs:
            if kw in tech or tech in kw:
                tech_matches.append(kw)
                break
    tech_score = min(len(tech_matches) * 3, 15)
    breakdown["technology"] = {"score": tech_score, "matches": tech_matches[:5]}
    score += tech_score

    # Metrics (5pts if metrics table present)
    breakdown["metrics"] = 5 if project.get("has_metrics_table") else 0
    score += breakdown["metrics"]

    # Quote (2pts if client quote/testimonial present)
    breakdown["quote"] = 2 if project.get("has_quote") else 0
    score += breakdown["quote"]

    # Recency (max 5)
    timeline = project.get("timeline") or ""
    if any(y in timeline for y in ["2024", "2025", "2026"]):
        breakdown["recency"] = 5
    elif any(y in timeline for y in ["2022", "2023"]):
        breakdown["recency"] = 4
    elif any(y in timeline for y in ["2020", "2021"]):
        breakdown["recency"] = 3
    else:
        breakdown["recency"] = 1
    score += breakdown["recency"]

    # Scale (max 3)
    desc = (project.get("description") or "").lower()
    if any(kw in desc for kw in ["enterprise", "statewide", "million", "$1m"]):
        breakdown["scale"] = 3
    elif any(kw in desc for kw in ["department", "agency"]):
        breakdown["scale"] = 1
    else:
        breakdown["scale"] = 0
    score += breakdown["scale"]

    # Contract type similarity (max 3) [FAR 15.305 relevance factor]
    contract_type_score = 0
    rfp_contract_type = (rfp_summary.get("contract_type") or "").upper()
    project_content_lower = (project.get("description") or "").lower()

    contract_type_families = {
        "FFP": ["firm fixed", "fixed price", "ffp", "fixed-price"],
        "T&M": ["time and material", "t&m", "time & material", "hourly"],
        "IDIQ": ["idiq", "indefinite delivery", "task order", "blanket purchase"],
        "CPFF": ["cost plus", "cpff", "cost-reimbursement", "cpaf"],
        "BPA": ["bpa", "blanket purchase", "standing order"]
    }
    related_types = {
        "FFP": ["CPFF", "BPA"], "T&M": ["IDIQ", "BPA"],
        "IDIQ": ["T&M", "BPA"], "CPFF": ["FFP", "T&M"], "BPA": ["IDIQ", "T&M"]
    }

    if rfp_contract_type:
        exact_kws = contract_type_families.get(rfp_contract_type, [rfp_contract_type.lower()])
        if any(kw in project_content_lower for kw in exact_kws):
            contract_type_score = 3
        else:
            for related in related_types.get(rfp_contract_type, []):
                rel_kws = contract_type_families.get(related, [related.lower()])
                if any(kw in project_content_lower for kw in rel_kws):
                    contract_type_score = 1
                    break
    breakdown["contract_type"] = contract_type_score
    score += contract_type_score

    # Dollar value proximity (max 3) [FAR 15.305 relevance factor]
    dollar_score = 0
    rfp_value = rfp_summary.get("estimated_value", 0)
    import re as _re
    project_value = 0
    val_match = _re.search(r'\$\s*([\d,.]+)\s*(million|m|k|thousand|billion|b)?', project_content_lower)
    if val_match:
        val_num = val_match.group(1).replace(",", "")
        mult_map = {"million": 1e6, "m": 1e6, "billion": 1e9, "b": 1e9, "thousand": 1e3, "k": 1e3}
        try:
            project_value = float(val_num) * mult_map.get(val_match.group(2), 1)
        except ValueError:
            project_value = 0

    if rfp_value and project_value:
        ratio = max(rfp_value, project_value) / max(min(rfp_value, project_value), 1)
        if ratio <= 2:
            dollar_score = 3
        elif ratio <= 5:
            dollar_score = 1
    breakdown["dollar_proximity"] = dollar_score
    score += dollar_score

    scored_projects.append({
        "project_number": project["project_number"],
        "title": project["title"],
        "client": project.get("client", ""),
        "industry": project.get("industry", ""),
        "relevance_score": score,
        "score_breakdown": breakdown,
        "technologies": project.get("technologies", []),
        "key_metrics": project.get("key_metrics", []),
        "timeline": project.get("timeline", ""),
        "description_summary": (project.get("description") or "")[:300]
    })

# Sort by score descending, take top 5
scored_projects.sort(key=lambda p: p["relevance_score"], reverse=True)

# Check for override projects from company profile
overrides = company.get("past_performance", {}).get("override_projects", [])
if overrides:
    override_entries = [p for p in scored_projects if p["project_number"] in overrides]
    non_override = [p for p in scored_projects if p["project_number"] not in overrides]
    scored_projects = override_entries + non_override

top_5 = scored_projects[:5]

# Add rank and relevance statement
for i, p in enumerate(top_5):
    p["rank"] = i + 1
    tech_matches = p["score_breakdown"].get("technology", {}).get("matches", [])
    # FAR 15.305 relevance rating based on score
    if p['relevance_score'] >= 25:
        relevance_rating = "Very Relevant"
    elif p['relevance_score'] >= 15:
        relevance_rating = "Relevant"
    elif p['relevance_score'] >= 8:
        relevance_rating = "Somewhat Relevant"
    else:
        relevance_rating = "Not Relevant"
    p["relevance_rating"] = relevance_rating

    p["relevance_statement"] = (
        f"#{i+1} match (score: {p['relevance_score']}, {relevance_rating}). "
        f"Industry: {p['industry']}. "
        + (f"Tech overlap: {', '.join(tech_matches[:3])}. " if tech_matches else "")
        + (f"Key result: {p['key_metrics'][0]}. " if p.get('key_metrics') else "")
    )
```

### Step 7: Write Past Projects Output

```python
past_match = {
    "phase": "4b",
    "timestamp": datetime.now().isoformat(),
    "total_projects_evaluated": len(projects),
    "projects_selected": len(top_5),
    "rfp_domain": rfp_domain,
    "rfp_keywords": rfp_keywords,
    "matched_projects": top_5,
    "match_quality": "strong" if top_5 and top_5[0]["relevance_score"] > 15 else
                     "moderate" if top_5 and top_5[0]["relevance_score"] > 8 else "weak"
}
write_json(f"{folder}/screen/past-projects-match.json", past_match)
```

### Step 8: Report

```
COMPLIANCE & PROJECT MATCHING (Phase 4)
========================================
Compliance:
  Items checked: {total_items}
  PASS: {pass_count} | GAP: {gap_count} | RISK: {risk_count}
  Status: {overall_status}

Past Projects:
  Evaluated: {total_projects_evaluated}
  Top 5 Matches:
  {for each in top_5: f"  #{rank}. {title} ({industry}) — Score: {relevance_score}"}
  Match Quality: {match_quality}

Outputs:
  screen/compliance-check.json
  screen/past-projects-match.json
```

---

## Quality Checklist

- [ ] `compliance-check.json` written (>1KB) with PASS/GAP/RISK per item
- [ ] `past-projects-match.json` written (>1KB) with top 5 projects
- [ ] Company profile loaded — services flattened (DICT not list)
- [ ] Past_Projects.md parsed — projects scored by algorithm
- [ ] Technology matching uses required_technologies (with fallback to scope_keywords)
- [ ] Override projects from company-profile.json applied
- [ ] Related industry mapping used for partial matches
- [ ] Each project has score_breakdown and relevance_statement
