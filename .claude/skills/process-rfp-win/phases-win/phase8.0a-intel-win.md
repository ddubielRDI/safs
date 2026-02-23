---
name: phase8.0a-intel-win
expert-role: Competitive Intelligence Analyst
domain-expertise: Market research, incumbent analysis, FPDS
---

# Phase 8.0a: Client Intelligence

## Expert Role

You are a **Competitive Intelligence Analyst** with deep expertise in:
- Market research and analysis
- Incumbent analysis
- FPDS (Federal Procurement Data System)
- Competitive positioning

## Purpose

Gather client intelligence through web research to inform bid strategy.

## Inputs

- `{folder}/shared/domain-context.json`
- `{folder}/flattened/*.md` (for client name extraction)

## Required Outputs

- `{folder}/shared/bid/CLIENT_INTELLIGENCE.json`

## Instructions

### Step 1: Extract Client Information

```python
import glob

flattened_files = glob.glob(f"{folder}/flattened/*.md")
combined_content = ""

for file_path in flattened_files:
    combined_content += read_file(file_path) + "\n"

domain_context = read_json(f"{folder}/shared/domain-context.json")
```

### Step 2: Identify Client Organization

```python
def extract_client_info(content):
    """Extract client organization details."""
    client_info = {
        "organization_name": None,
        "location": None,
        "industry": None,
        "size_indicators": []
    }

    # Look for organization patterns
    org_patterns = [
        r"(?:issued\s+by|from|submitted\s+to)\s+([A-Z][A-Za-z\s]+(?:District|County|City|Agency|Department|Corporation|Inc|LLC))",
        r"(?:RFP|Request\s+for\s+Proposal)\s+(?:from|by)\s+([A-Z][A-Za-z\s]+)",
        r"([A-Z][A-Za-z]+(?:\s+[A-Z][A-Za-z]+){1,3})\s+(?:School\s+District|County|Agency)"
    ]

    for pattern in org_patterns:
        match = re.search(pattern, content)
        if match:
            client_info["organization_name"] = match.group(1).strip()
            break

    # Look for location
    location_patterns = [
        r"(?:located\s+in|based\s+in|serving)\s+([A-Z][a-z]+(?:,\s+[A-Z]{2})?)",
        r"([A-Z][a-z]+),\s+([A-Z]{2})\s+\d{5}"  # City, ST ZIP
    ]

    for pattern in location_patterns:
        match = re.search(pattern, content)
        if match:
            client_info["location"] = match.group(0).strip()
            break

    return client_info

client_info = extract_client_info(combined_content)
```

### Step 3: Perform Web Research (Limited)

**Note:** This phase uses WebSearch with a maximum of 15 queries.

```python
def gather_intelligence(client_name, domain, max_searches=15):
    """Gather client intelligence via web search."""
    intelligence = {
        "news": [],
        "leadership": [],
        "contracts": [],
        "strategic_initiatives": [],
        "technology_stack": [],
        "challenges": []
    }

    if not client_name:
        return {"status": "skipped", "reason": "Client name not identified"}

    search_count = 0

    # Search 1-3: Recent news
    if search_count < max_searches:
        # WebSearch(query=f"{client_name} news 2024")
        # Parse results into intelligence["news"]
        search_count += 1

    # Search 4-5: Leadership
    if search_count < max_searches:
        # WebSearch(query=f"{client_name} leadership executives")
        search_count += 1

    # Search 6-8: Technology/IT
    if search_count < max_searches:
        # WebSearch(query=f"{client_name} IT technology projects")
        search_count += 1

    # Search 9-10: Strategic initiatives
    if search_count < max_searches:
        # WebSearch(query=f"{client_name} strategic plan initiatives")
        search_count += 1

    # Search 11-12: Past contracts (if government)
    if search_count < max_searches and domain in ["education", "government"]:
        # WebSearch(query=f"site:usaspending.gov {client_name}")
        search_count += 1

    # Search 13-15: Pain points / challenges
    if search_count < max_searches:
        # WebSearch(query=f"{client_name} challenges issues")
        search_count += 1

    intelligence["searches_performed"] = search_count
    return intelligence

# NOTE: In actual execution, WebSearch tool would be called
# For now, generate placeholder structure
intelligence = {
    "status": "research_required",
    "client_name": client_info.get("organization_name"),
    "searches_to_perform": [
        f"{client_info.get('organization_name')} recent news",
        f"{client_info.get('organization_name')} leadership team",
        f"{client_info.get('organization_name')} technology initiatives",
        f"{client_info.get('organization_name')} strategic plan"
    ][:15],
    "categories": {
        "news": "To be populated from web search",
        "leadership": "To be populated from web search",
        "contracts": "To be populated from FPDS/USASpending",
        "strategic_initiatives": "To be populated from web search",
        "technology_stack": "To be populated from job postings/news",
        "challenges": "To be populated from news/reports"
    }
}
```

### Step 4: Analyze Incumbent (if identifiable)

```python
def analyze_incumbent(content, client_name):
    """Identify potential incumbent vendor."""
    incumbent_info = {
        "identified": False,
        "vendor_name": None,
        "relationship_duration": None,
        "known_issues": []
    }

    # Look for incumbent mentions
    incumbent_patterns = [
        r"current\s+(?:vendor|contractor|provider)[:\s]+([A-Z][A-Za-z]+(?:\s+[A-Z][A-Za-z]+)?)",
        r"replace\s+(?:the\s+)?(?:existing|current)\s+([A-Z][A-Za-z]+)",
        r"transition\s+from\s+([A-Z][A-Za-z]+)"
    ]

    for pattern in incumbent_patterns:
        match = re.search(pattern, content, re.IGNORECASE)
        if match:
            incumbent_info["identified"] = True
            incumbent_info["vendor_name"] = match.group(1).strip()
            break

    return incumbent_info

incumbent = analyze_incumbent(combined_content, client_info.get("organization_name"))
```

### Step 5: Generate Leverage Points

```python
def identify_leverage_points(client_info, intelligence, domain):
    """Identify leverage points for bid positioning."""
    leverage_points = []

    # Domain-specific leverage
    if domain == "education":
        leverage_points.append({
            "type": "compliance",
            "point": "FERPA compliance expertise",
            "evidence": "Demonstrated experience with student data protection"
        })
        leverage_points.append({
            "type": "integration",
            "point": "State reporting integration",
            "evidence": "CEDARS/CEISDARS submission experience"
        })

    elif domain == "healthcare":
        leverage_points.append({
            "type": "compliance",
            "point": "HIPAA compliance certification",
            "evidence": "SOC 2 Type II audited"
        })

    # General leverage points
    leverage_points.append({
        "type": "technology",
        "point": "Modern cloud-native architecture",
        "evidence": "Reduced TCO, improved scalability"
    })

    leverage_points.append({
        "type": "innovation",
        "point": "AI-assisted development",
        "evidence": "35% efficiency gains, faster delivery"
    })

    return leverage_points

leverage_points = identify_leverage_points(client_info, intelligence, domain_context.get("selected_domain"))
```

### Step 6: Write Output

```python
import os

os.makedirs(f"{folder}/shared/bid", exist_ok=True)

client_intelligence = {
    "gathered_at": datetime.now().isoformat(),
    "client_info": client_info,
    "intelligence": intelligence,
    "incumbent_analysis": incumbent,
    "leverage_points": leverage_points,
    "competitive_context": {
        "market_position": "To be determined from research",
        "key_competitors": "To be identified",
        "differentiators": [lp["point"] for lp in leverage_points]
    },
    "recommendations": [
        "Emphasize compliance expertise in proposal",
        "Highlight modern technology approach",
        "Reference AI efficiency gains",
        "Address incumbent transition concerns if applicable"
    ]
}

write_json(f"{folder}/shared/bid/CLIENT_INTELLIGENCE.json", client_intelligence)
```

### Step 7: Report Results

```python
log(f"""
🔍 Client Intelligence Gathering Complete
=========================================
Client: {client_info.get("organization_name", "Not identified")}
Location: {client_info.get("location", "Not identified")}
Domain: {domain_context.get("selected_domain")}

Incumbent Identified: {"Yes - " + incumbent.get("vendor_name") if incumbent.get("identified") else "No"}

Leverage Points: {len(leverage_points)}
{chr(10).join(f"  • {lp['point']}" for lp in leverage_points)}

Output: {folder}/shared/bid/CLIENT_INTELLIGENCE.json
""")
```

## Quality Checklist

- [ ] `CLIENT_INTELLIGENCE.json` created in `shared/bid/`
- [ ] Client organization identified
- [ ] Web research queries defined (max 15)
- [ ] Incumbent analysis attempted
- [ ] Leverage points generated
- [ ] Recommendations provided
