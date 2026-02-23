---
name: phase3-intel
expert-role: Competitive Intelligence Analyst
domain-expertise: OSINT, market research, competitive analysis, procurement data
---

# Phase 3: Client Intelligence Snapshot

> **Conditional Execution:** This phase is SKIPPED when `--quick` flag is used. Check `quick_mode` variable.

**Expert Role:** Competitive Intelligence Analyst

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

### Step 3: Execute Web Research (Max 8 Queries)

Track search count. Stop at 8. Prioritize categories in order.

```python
intel = {
    "news": [],
    "leadership": [],
    "technology_stack": [],
    "strategic_initiatives": [],
    "incumbent_competitors": [],
    "search_count": 0,
    "search_log": []
}
```

**Category A: Recent News (2 searches)**

```
WebSearch: "{client_name} news 2025 2026"
WebSearch: "{client_name} technology project announcement"
```

Extract per result: headline, date, source, url, relevance, sentiment

**Category B: Leadership (1 search)**

```
WebSearch: "{client_name} CIO CTO IT director leadership"
```

Extract per result: name, title, source

**Category C: Technology Stack (2 searches)**

```
WebSearch: "{client_name} technology stack software systems"
WebSearch: "{client_name} IT modernization digital transformation"
```

Extract per result: technology, category, confidence

**Category D: Strategic Initiatives (1 search)**

```
WebSearch: "{client_name} strategic plan 2025 2026 goals"
```

Extract per result: initiative, timeframe, relevance_to_rfp

**Category E: Incumbent/Competitors (2 searches)**

```
WebSearch: "{client_name} current vendor contractor IT services"
WebSearch: "{client_name} contract award information technology"
```

Extract per result: incumbent name, competitors, contract value

> **Enforcement:** After each WebSearch call, increment `intel["search_count"]` and append the query string to `intel["search_log"]`. If `search_count >= 8`, stop immediately regardless of remaining categories.

### Step 4: Write Output

```python
client_intel = {
    "phase": "3",
    "timestamp": datetime.now().isoformat(),
    "status": "complete",
    "client_name": client_name,
    "intelligence": {
        "news": intel["news"],
        "leadership": intel["leadership"],
        "technology_stack": intel["technology_stack"],
        "strategic_initiatives": intel["strategic_initiatives"],
        "incumbent_competitors": intel["incumbent_competitors"]
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
News: {len(news)} articles
Leadership: {len(leadership)} contacts
Tech Stack: {len(technology_stack)} technologies
Initiatives: {len(strategic_initiatives)} found
Incumbent: {identified or "Not identified"}
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
