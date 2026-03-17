---
name: phase3-intel
expert-role: Competitive Intelligence Analyst
domain-expertise: OSINT, market research, competitive analysis, procurement data
skill: competitive-intel
---

# Phase 3: Client Intelligence Snapshot

> **Conditional Execution:** This phase is SKIPPED when `--quick` flag is used. Check `quick_mode` variable.

**Purpose:** Gather actionable client intelligence through web searches to inform the screening recommendation. Lighter-weight version of full pipeline's Phase 1.95 — max 8 searches instead of 15, fewer categories.

**Inputs:**
- `{folder}/screen/rfp-summary.json` — Client name, domain, scope
- Combined RFP text (in memory)

**Required Output:**
- `{folder}/screen/client-intel-snapshot.json` (>1KB)

**CRITICAL:** If client name is None/unknown, write minimal output with `"status": "client_not_identified"` and exit. Do NOT run web searches without a client name.

---

## Instructions

### Step 1: Check Skip Condition

```python
if quick_mode:
    log("Phase 3 SKIPPED — --quick mode")
    return  # Skip entirely
```

### Step 2: Load Client Name

```python
rfp_summary = read_json(f"{folder}/screen/rfp-summary.json")
client_name = rfp_summary.get("client_name")

if not client_name:
    # Write minimal output
    write_json(f"{folder}/screen/client-intel-snapshot.json", {
        "phase": "3",
        "status": "client_not_identified",
        "timestamp": datetime.now().isoformat(),
        "note": "Client name not extracted from RFP. Skipping intelligence gathering."
    })
    log("Phase 3 SKIPPED — client name not identified")
    return
```

### Skill Integration: Competitive-Intel Framework Application (MANDATORY)

The **competitive-intel** skill is loaded in context. Apply these frameworks:

**CRAAP Source Evaluation Framework:** For each web search result, assess:
- Currency — How recent is the information?
- Relevance — Does it relate to the RFP context?
- Authority — Is the source credible? (.gov, .edu, official sites > blogs, forums)
- Accuracy — Can the claims be verified from multiple sources?
- Purpose — What is the source's intent? (news vs. marketing vs. opinion)

Only include findings that score at least 3/5 on the CRAAP scale.

**Collection Planning Allocation:** Distribute the 8 searches by priority:
- 30% Incumbent ID (2-3 searches) — Who has the current contract?
- 25% Competitor Landscape (2 searches) — Who else will bid?
- 25% Client Environment (2 searches) — Technology stack, org structure
- 20% Strategic Context (1-2 searches) — Client initiatives, pain points

**Intelligence Cycle Terminology:** Structure output using:
- **Collection** — raw search results gathered
- **Processing** — validated and cross-referenced findings
- **Analysis** — synthesized intelligence with confidence levels

**Triangulation Rule:** For any claim to be reported as "high confidence," it must be corroborated by at least 2 independent sources. Single-source claims should be marked as "low-medium confidence."

---

### Step 3: Execute Web Research (Max 8 Queries)

Track search count. Stop at 8. Prioritize categories in order.

```python
intel = {
    "organization_profile": {},
    "news": [],
    "leadership": [],
    "technology_stack": [],
    "strategic_initiatives": [],
    "competitive_landscape": {},
    "search_count": 0,
    "search_log": []
}
```

**Category A: Organization Profile + Recent News (2 searches)**

```
WebSearch: "{client_name} population size demographics government budget"
WebSearch: "{client_name} news 2025 2026 technology project"
```

From search results, build the `organization_profile` object:
```python
intel["organization_profile"] = {
    "name": client_name,
    "industry": "Municipal Government" or appropriate classification,
    "size": "population, employee count, or other scale indicator",
    "headquarters": "city, state, zip (metro area if applicable)",
    "governance": "form of government, city manager/mayor name if found",
    "demographics": "median income, notable demographics, major employers if found",
    "budget": "annual budget if found, bond packages, CIP info"
}
```

Extract news per result: headline, date, source, url, relevance, sentiment

**Category B: Leadership (1 search)**

```
WebSearch: "{client_name} CIO CTO IT director GIS manager technology leadership"
```

Extract per result: name, title, source, note (certifications, responsibilities)

**Category C: Technology Stack (2 searches)**

```
WebSearch: "{client_name} technology stack software systems GIS Esri"
WebSearch: "{client_name} IT modernization digital transformation"
```

Extract per result: technology, category, confidence, note

**Category D: Strategic Initiatives (1 search)**

```
WebSearch: "{client_name} strategic plan 2025 2026 goals"
```

Extract per result: initiative, timeframe, relevance_to_rfp

**Category E: Competitors & Incumbent (2 searches)**

```
WebSearch: "{client_name} GIS vendor contractor IT services contract award"
WebSearch: "site:dir.texas.gov OR site:esri.com {client_name} OR {state} GIS partner vendor"
```

Build the `competitive_landscape` object:
```python
intel["competitive_landscape"] = {
    "incumbent": "Name and details of current/prior vendor, or 'Unknown' with explanation",
    "known_competitors": [
        "Competitor Name (detail: location, Esri partner status, relevant contracts)"
    ],
    "notes": "How the competitive field is shaped -- in-house capability, vendor preferences, etc."
}
```

> **Enforcement:** After each WebSearch call, increment `intel["search_count"]` and append the query string to `intel["search_log"]`. If `search_count >= 8`, stop immediately regardless of remaining categories.

### Step 4: Write Output

```python
client_intel = {
    "phase": "3",
    "timestamp": datetime.now().isoformat(),
    "status": "complete",
    "client_name": client_name,
    "intelligence": {
        "organization_profile": intel["organization_profile"],
        "news": intel["news"],
        "leadership": intel["leadership"],
        "technology_stack": intel["technology_stack"],
        "strategic_initiatives": intel["strategic_initiatives"],
        "competitive_landscape": intel["competitive_landscape"]
    },
    "research_metadata": {
        "total_searches": intel["search_count"],
        "max_searches": 8,
        "search_log": intel["search_log"],
        "data_freshness": datetime.now().strftime("%Y-%m-%d")
    }
}
write_json(f"{folder}/screen/client-intel-snapshot.json", client_intel)
```

### Step 5: Report

```
CLIENT INTELLIGENCE SNAPSHOT (Phase 3)
=======================================
Client: {client_name}
Searches: {search_count}/8
Org Profile: {organization_profile.get("size", "Not found")}
News: {len(news)} articles
Leadership: {len(leadership)} contacts
Tech Stack: {len(technology_stack)} technologies
Initiatives: {len(strategic_initiatives)} found
Competitors: {len(competitive_landscape.get("known_competitors", []))} identified
Incumbent: {competitive_landscape.get("incumbent", "Not identified")}
Output: screen/client-intel-snapshot.json
```

---

## Quality Checklist

- [ ] Phase skipped when --quick mode
- [ ] Client name verified before searching
- [ ] WebSearch ACTUALLY called (not placeholder)
- [ ] Max 8 searches enforced
- [ ] `client-intel-snapshot.json` written (>1KB)
- [ ] Search log documents every query

### Skill Integration Quality Checks (competitive-intel)
- [ ] CRAAP framework applied to source evaluation (only 3/5+ included)
- [ ] Collection planning allocation roughly followed (30% incumbent, 25% competitor, 25% client, 20% strategic)
- [ ] Triangulation rule applied (high-confidence claims require 2+ independent sources)
- [ ] Intelligence structured using collection/processing/analysis terminology
