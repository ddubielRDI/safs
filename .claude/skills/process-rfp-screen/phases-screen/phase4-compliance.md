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
        # Check if it's a hard gap or potential risk
        if item["category"] in ["set_aside", "insurance"]:
            item["status"] = "RISK"
            item["match_source"] = None
            item["match_detail"] = "Cannot verify — requires manual confirmation"
        else:
            item["status"] = "GAP"
            item["match_source"] = None
            item["match_detail"] = "No matching capability found in company profile"
```

### Step 3: Write Compliance Output

```python
pass_count = sum(1 for i in compliance_items if i["status"] == "PASS")
gap_count = sum(1 for i in compliance_items if i["status"] == "GAP")
risk_count = sum(1 for i in compliance_items if i["status"] == "RISK")

compliance_check = {
    "phase": "4a",
    "timestamp": datetime.now().isoformat(),
    "total_items": len(compliance_items),
    "summary": {
        "pass": pass_count,
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

Scoring algorithm: industry(10/5) + tech(3/match, max 15) + metrics(5) + quote(2) + recency(1-5) + scale(0-3) = 0-40 max

```python
rfp_summary = read_json(f"{folder}/screen/rfp-summary.json")
rfp_domain = (rfp_summary.get("industry_domain") or "").lower()
rfp_keywords = [kw.lower() for kw in rfp_summary.get("scope_keywords", [])]

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
    p["relevance_statement"] = (
        f"#{i+1} match (score: {p['relevance_score']}). "
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
- [ ] Override projects from company-profile.json applied
- [ ] Related industry mapping used for partial matches
- [ ] Each project has score_breakdown and relevance_statement
