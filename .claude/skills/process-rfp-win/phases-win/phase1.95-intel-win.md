---
name: phase1.95-intel-win
expert-role: Competitive Intelligence Analyst
domain-expertise: Market research, incumbent analysis, FPDS/USASpending, competitive positioning, OSINT
skill: competitive-intel
---

# Phase 1.95: Client Intelligence

> **Conditional Execution:** This phase runs ONLY if `{folder}/shared/GO_NOGO_DECISION.json` exists and its `recommendation` field is `"GO"`, OR the user explicitly overrides with a NO-GO decision. If the recommendation is `"NO_GO"` and the user has NOT overridden, SKIP this phase and log: "Phase 1.95 skipped — Go/No-Go decision was NO_GO."

## Purpose

Gather actionable client intelligence through REAL web searches to inform bid strategy. This intelligence feeds downstream phases that craft the proposal narrative, align technology recommendations, and position Resource Data competitively.

**This phase MUST execute actual WebSearch tool calls.** Do not generate placeholder comments or pseudocode for searches. Every search category below must produce real results or document why no results were found.

## Inputs

- `{folder}/shared/domain-context.json` — Domain, industry, client context
- `{folder}/shared/GO_NOGO_DECISION.json` — Go/No-Go decision (for conditional gate)
- `{folder}/flattened/*.md` — Flattened RFP documents (for client name extraction)
- `{folder}/shared/COMPLIANCE_MATRIX.json` — Optional, for contract type context

## Required Output

- `{folder}/shared/bid/CLIENT_INTELLIGENCE.json` (>2KB)

## Downstream Consumption

The `CLIENT_INTELLIGENCE.json` produced here is consumed by multiple downstream phases:

| Consumer Phase | Usage | Required? |
|----------------|-------|-----------|
| **Phase 2a** (Workflow Extraction) | Client context for understanding business processes | Optional |
| **Phase 3a** (Architecture) | Technology stack alignment, integration context | Optional |
| **Phase 3f** (Entity Definitions) | Data model context from client's existing systems | Optional |
| **Phase 8.0** (Strategic Positioning) | Primary consumer — drives win themes, differentiators, competitive strategy | **Required** |

## Instructions

### Step 1: Verify Conditional Gate

```python
import os, json

go_nogo_path = f"{folder}/shared/GO_NOGO_DECISION.json"
if os.path.exists(go_nogo_path):
    decision = read_json(go_nogo_path)
    if decision.get("recommendation") == "NO_GO":
        log("Phase 1.95 skipped — Go/No-Go decision was NO_GO.")
        # EXIT PHASE — do not proceed
        return
else:
    log("WARNING: GO_NOGO_DECISION.json not found. Proceeding with intelligence gathering.")
```

### Step 2: Extract Client Information

```python
import glob, re

flattened_files = sorted(glob.glob(f"{folder}/flattened/*.md"))
combined_content = ""
for file_path in flattened_files:
    combined_content += read_file(file_path) + "\n"

domain_context = read_json(f"{folder}/shared/domain-context.json")

# Extract client organization name
client_info = {
    "organization_name": None,
    "location": None,
    "industry": domain_context.get("selected_domain"),
    "state": None,
    "is_government": False,
    "size_indicators": []
}

# Pattern matching for organization name
org_patterns = [
    r"(?:issued\s+by|from|submitted\s+to|prepared\s+for)\s+(?:the\s+)?([A-Z][A-Za-z\s]+(?:District|County|City|State|Agency|Department|Corporation|Inc|LLC|Authority|Commission|Board|University|College))",
    r"(?:RFP|RFQ|Request\s+for\s+Proposal)\s+(?:from|by|for)\s+(?:the\s+)?([A-Z][A-Za-z\s]+)",
    r"([A-Z][A-Za-z]+(?:\s+[A-Z][A-Za-z]+){1,4})\s+(?:School\s+District|County|Agency|Department)"
]

for pattern in org_patterns:
    match = re.search(pattern, combined_content)
    if match:
        client_info["organization_name"] = match.group(1).strip()
        break

# Extract location (city, state)
location_patterns = [
    r"(?:located\s+in|based\s+in|serving)\s+([A-Z][a-z]+(?:\s+[A-Z][a-z]+)?,\s*[A-Z]{2})",
    r"([A-Z][a-z]+(?:\s+[A-Z][a-z]+)?),\s+([A-Z]{2})\s+\d{5}"
]

for pattern in location_patterns:
    match = re.search(pattern, combined_content)
    if match:
        client_info["location"] = match.group(0).strip()
        break

# Determine if government entity
gov_indicators = ["county", "city of", "state of", "department", "agency", "district", "commission", "authority", "board"]
if client_info["organization_name"]:
    client_info["is_government"] = any(ind in client_info["organization_name"].lower() for ind in gov_indicators)

client_name = client_info["organization_name"]
if not client_name:
    log("WARNING: Could not extract client organization name from RFP documents.")
    log("Attempting to use domain_context for fallback...")
    client_name = domain_context.get("client_name") or domain_context.get("organization")
    if client_name:
        client_info["organization_name"] = client_name
```

**CRITICAL:** If `client_name` is still None after all extraction attempts, write a minimal `CLIENT_INTELLIGENCE.json` with `"status": "client_not_identified"` and exit. Do NOT run web searches without a client name.

### Step 3: Execute Web Research (Max 15 Queries)

**You MUST actually call WebSearch for each category below.** Track your search count. Stop at 15 total queries. Prioritize categories in order listed.

Initialize the results structure:

```python
intelligence = {
    "news": [],
    "decision_makers": [],
    "technology_stack": [],
    "strategic_initiatives": [],
    "past_contracts": [],
    "pain_points": [],
    "competitive_landscape": {
        "incumbent": None,
        "known_competitors": []
    },
    "searches_performed": 0,
    "search_log": []  # Track every query for transparency
}

search_count = 0
MAX_SEARCHES = 15
```

#### Category A: Recent News (2-3 searches)

Execute these searches to understand the client's current situation:

```
WebSearch: "{client_name} news 2025 2026"
WebSearch: "{client_name} technology project announcement"
WebSearch: "{client_name} contract award" (if government)
```

For each result, extract:
- `headline` — Article title
- `date` — Publication date
- `source` — Publisher name
- `url` — Article URL
- `relevance` — Brief note on why this matters for the bid
- `sentiment` — "positive", "neutral", or "negative"

```python
# ACTUALLY CALL WebSearch — example:
results = WebSearch(query=f"{client_name} news 2025 2026")
search_count += 1
intelligence["search_log"].append({"query": f"{client_name} news 2025 2026", "result_count": len(results)})

for r in results:
    intelligence["news"].append({
        "headline": r.title,
        "date": r.date,
        "source": r.source,
        "url": r.url,
        "relevance": "...",  # Analyst assessment
        "sentiment": "..."   # positive/neutral/negative
    })
```

#### Category B: Leadership & Decision-Makers (1-2 searches)

```
WebSearch: "{client_name} CIO CTO IT director leadership"
WebSearch: "{client_name} director technology linkedin" (if first search yields little)
```

For each person identified, extract:
- `name` — Full name
- `title` — Job title
- `linkedin_url` — LinkedIn profile URL if found
- `source` — Where this info was found
- `relevance` — Role in procurement/decision-making

```python
results = WebSearch(query=f"{client_name} CIO CTO IT director leadership")
search_count += 1
intelligence["search_log"].append({"query": f"{client_name} CIO CTO IT director leadership", "result_count": len(results)})

for r in results:
    # Parse names and titles from results
    intelligence["decision_makers"].append({
        "name": "...",
        "title": "...",
        "linkedin_url": "...",  # null if not found
        "source": r.url,
        "relevance": "..."
    })
```

#### Category C: Technology Stack & IT Initiatives (2-3 searches)

```
WebSearch: "{client_name} technology stack software systems"
WebSearch: "{client_name} IT modernization digital transformation"
WebSearch: "{client_name} ERP CRM database platform" (if relevant to RFP domain)
```

For each technology identified, extract:
- `technology` — Technology/product name
- `category` — e.g., "Database", "CRM", "Cloud Platform", "Language/Framework"
- `source` — "job_posting", "news_article", "vendor_announcement", "government_filing"
- `confidence` — "confirmed" or "inferred"
- `url` — Source URL

```python
results = WebSearch(query=f"{client_name} technology stack software systems")
search_count += 1
intelligence["search_log"].append({"query": ..., "result_count": len(results)})

for r in results:
    intelligence["technology_stack"].append({
        "technology": "...",
        "category": "...",
        "source": "news_article",
        "confidence": "inferred",
        "url": r.url
    })
```

#### Category D: Strategic Plan & Goals (1-2 searches)

```
WebSearch: "{client_name} strategic plan 2025 2026 goals"
WebSearch: "{client_name} annual report budget priorities" (if government)
```

Extract strategic initiatives relevant to the RFP's scope:
- `initiative` — Name/description
- `timeframe` — When planned/active
- `relevance_to_rfp` — How it connects to what they're procuring
- `source` — URL

```python
results = WebSearch(query=f"{client_name} strategic plan 2025 2026 goals")
search_count += 1
intelligence["search_log"].append({"query": ..., "result_count": len(results)})

for r in results:
    intelligence["strategic_initiatives"].append({
        "initiative": "...",
        "timeframe": "...",
        "relevance_to_rfp": "...",
        "source": r.url
    })
```

#### Category E: Past Contracts via USASpending (1-2 searches, government clients only)

**Only execute if `client_info["is_government"]` is True.**

```
WebSearch: "site:usaspending.gov {client_name}"
WebSearch: "{client_name} contract award FPDS information technology"
```

For each contract found, extract:
- `contract_title` — Description of the contract
- `value` — Dollar value (formatted)
- `vendor` — Awarded vendor name
- `date` — Award date
- `naics_code` — If available
- `relevance` — Connection to current RFP
- `source` — URL

```python
if client_info["is_government"]:
    results = WebSearch(query=f"site:usaspending.gov {client_name}")
    search_count += 1
    intelligence["search_log"].append({"query": ..., "result_count": len(results)})

    for r in results:
        intelligence["past_contracts"].append({
            "contract_title": "...",
            "value": "...",
            "vendor": "...",
            "date": "...",
            "naics_code": None,
            "relevance": "...",
            "source": r.url
        })
```

#### Category F: Pain Points & Challenges (1-2 searches)

```
WebSearch: "{client_name} challenges issues problems technology"
WebSearch: "{client_name} audit findings improvement recommendations" (if government)
```

For each pain point identified, extract:
- `pain_point` — Description
- `source` — "news", "audit_report", "public_comment", "job_posting"
- `severity` — "high", "medium", "low"
- `relevance_to_rfp` — How this connects to what we can solve
- `url` — Source URL

```python
results = WebSearch(query=f"{client_name} challenges issues problems technology")
search_count += 1
intelligence["search_log"].append({"query": ..., "result_count": len(results)})

for r in results:
    intelligence["pain_points"].append({
        "pain_point": "...",
        "source": "news",
        "severity": "...",
        "relevance_to_rfp": "...",
        "url": r.url
    })
```

#### Category G: Job Postings for Tech Stack Clues (1 search)

```
WebSearch: "{client_name} job posting software developer IT analyst"
```

Job postings reveal the client's ACTUAL technology stack. Parse required skills/technologies from job listings and add to `technology_stack[]` with `source: "job_posting"`.

```python
results = WebSearch(query=f"{client_name} job posting software developer IT analyst")
search_count += 1
intelligence["search_log"].append({"query": ..., "result_count": len(results)})

# Parse technologies mentioned in job postings
# Add to intelligence["technology_stack"] with source="job_posting"
```

#### Category H: Incumbent Vendor Analysis (1-2 searches)

```
WebSearch: "{client_name} current vendor contractor IT services"
WebSearch: "{client_name} {incumbent_name} contract" (only if incumbent identified from RFP text)
```

```python
# First check RFP text for incumbent mentions
incumbent_patterns = [
    r"current\s+(?:vendor|contractor|provider)[:\s]+([A-Z][A-Za-z]+(?:\s+[A-Z][A-Za-z]+)*)",
    r"replace\s+(?:the\s+)?(?:existing|current)\s+([A-Z][A-Za-z]+)",
    r"transition\s+from\s+([A-Z][A-Za-z]+(?:\s+[A-Za-z]+)*)"
]

incumbent_name = None
for pattern in incumbent_patterns:
    match = re.search(pattern, combined_content, re.IGNORECASE)
    if match:
        incumbent_name = match.group(1).strip()
        break

# Web search for incumbent info
results = WebSearch(query=f"{client_name} current vendor contractor IT services")
search_count += 1
intelligence["search_log"].append({"query": ..., "result_count": len(results)})

intelligence["competitive_landscape"]["incumbent"] = {
    "vendor_name": incumbent_name,
    "identified_from": "rfp_text" if incumbent_name else "web_search",
    "relationship_duration": None,
    "known_issues": [],
    "contract_value": None,
    "source": "..."
}

# Search for known competitors
# Parse from contract awards, news about competing bids
intelligence["competitive_landscape"]["known_competitors"] = [
    # {"name": "...", "source": "...", "relationship_to_client": "..."}
]
```

### Step 4: Generate Leverage Points

Based on all gathered intelligence, identify strategic leverage points:

```python
leverage_points = []

# Domain-specific leverage
domain = domain_context.get("selected_domain", "").lower()

if domain == "education":
    leverage_points.append({
        "type": "compliance",
        "point": "FERPA compliance expertise",
        "evidence": "Demonstrated experience with student data protection"
    })
elif domain == "healthcare":
    leverage_points.append({
        "type": "compliance",
        "point": "HIPAA compliance certification",
        "evidence": "SOC 2 Type II audited"
    })

# Technology alignment leverage (from web research)
for tech in intelligence["technology_stack"]:
    leverage_points.append({
        "type": "technology_alignment",
        "point": f"Experience with {tech['technology']}",
        "evidence": f"Client uses {tech['technology']} — we can demonstrate integration expertise"
    })

# Pain point leverage (from web research)
for pp in intelligence["pain_points"]:
    if pp["severity"] in ["high", "medium"]:
        leverage_points.append({
            "type": "solution_fit",
            "point": f"Address: {pp['pain_point']}",
            "evidence": f"Identified from {pp['source']} — direct alignment with our capabilities"
        })

# General leverage
leverage_points.append({
    "type": "innovation",
    "point": "Modern cloud-native architecture",
    "evidence": "Reduced TCO, improved scalability, faster time-to-value"
})
```

### Step 5: Write CLIENT_INTELLIGENCE.json

```python
import os
from datetime import datetime

os.makedirs(f"{folder}/shared/bid", exist_ok=True)

client_intelligence = {
    "gathered_at": datetime.now().isoformat(),
    "phase": "1.95",
    "status": "complete",

    "client_info": client_info,

    "intelligence": {
        "news": intelligence["news"],
        "decision_makers": intelligence["decision_makers"],
        "technology_stack": intelligence["technology_stack"],
        "strategic_initiatives": intelligence["strategic_initiatives"],
        "past_contracts": intelligence["past_contracts"],
        "pain_points": intelligence["pain_points"]
    },

    "competitive_landscape": intelligence["competitive_landscape"],

    "leverage_points": leverage_points,

    "recommendations": [
        # Generate 3-5 specific recommendations based on research
        # NOT generic boilerplate — these must reference actual findings
    ],

    "research_metadata": {
        "total_searches": search_count,
        "max_searches": MAX_SEARCHES,
        "search_log": intelligence["search_log"],
        "data_freshness": "Searches performed on " + datetime.now().strftime("%Y-%m-%d")
    }
}

write_json(f"{folder}/shared/bid/CLIENT_INTELLIGENCE.json", client_intelligence)
```

### Step 6: Report Results

```
CLIENT INTELLIGENCE GATHERED (Phase 1.95)
==========================================
Client: {client_info["organization_name"]}
Location: {client_info["location"] or "Not identified"}
Domain: {domain_context.get("selected_domain")}
Government Entity: {"Yes" if client_info["is_government"] else "No"}

Research Summary:
  Searches Performed: {search_count}/{MAX_SEARCHES}
  News Articles Found: {len(intelligence["news"])}
  Decision-Makers Identified: {len(intelligence["decision_makers"])}
  Technologies Discovered: {len(intelligence["technology_stack"])}
  Strategic Initiatives: {len(intelligence["strategic_initiatives"])}
  Past Contracts: {len(intelligence["past_contracts"])}
  Pain Points: {len(intelligence["pain_points"])}

Incumbent: {"Yes - " + intelligence["competitive_landscape"]["incumbent"]["vendor_name"] if intelligence["competitive_landscape"]["incumbent"] and intelligence["competitive_landscape"]["incumbent"].get("vendor_name") else "Not identified"}
Known Competitors: {len(intelligence["competitive_landscape"]["known_competitors"])}

Leverage Points: {len(leverage_points)}
{chr(10).join(f"  - [{lp['type']}] {lp['point']}" for lp in leverage_points)}

Output: {folder}/shared/bid/CLIENT_INTELLIGENCE.json

Downstream consumers:
  - Phase 2a (Workflow) - optional context
  - Phase 3a (Architecture) - tech stack alignment
  - Phase 3f (Entity Definitions) - data model context
  - Phase 8.0 (Strategic Positioning) - REQUIRED primary consumer
```

## Quality Checklist (MANDATORY — report each by name with evidence)

The phase agent MUST verify each of the following BEFORE reporting completion. The agent's completion report MUST include a checklist-results block with:
- Item name (verbatim from below)
- PASS / FAIL / SKIPPED-WITH-REASON
- Evidence (file:line citation, grep result, file size, assertion that ran, etc.)

"All checks passed" without per-item evidence is NOT acceptable.

### Required output files
1. **CLIENT_INTELLIGENCE.json** exists at `{folder}/shared/bid/CLIENT_INTELLIGENCE.json` — evidence: `ls -la` showing size > 2,048 bytes and parses as valid JSON

### Schema fidelity
2. **CLIENT_INTELLIGENCE.json top-level keys** include `gathered_at`, `status`, `client_info`, `intelligence`, `competitive_landscape`, `leverage_points`, `recommendations`, `research_metadata` — evidence: list actual top-level keys found
3. **search_log** array length >= 5 — evidence: print `len(research_metadata["search_log"])`
4. **searches_performed** matches actual search_log length — evidence: print both values
5. No `[:N]` slicing applied to deliverable content strings — evidence: grep for `\[:[0-9]+\]` in production code paths returned 0 hits

### Cross-stage consistency
6. **Conditional gate verified** — GO_NOGO_DECISION.json was read and `recommendation` field was checked before proceeding — evidence: confirm gate check ran (log line or assertion)
7. **WebSearch tool ACTUALLY called** (not placeholder code) — evidence: print first 3 entries of search_log showing real query strings and result_count > 0
8. **technology_stack[] populated** with at least 1 real technology entry with source_url — evidence: print technology_stack[0] if exists
9. **Leverage points reference actual findings** — no leverage point contains generic template text like "Modern cloud-native architecture" as its only evidence — evidence: spot-check leverage_points[0].evidence

### Anti-regression rules (universal)
10. **UTF-8 encoding** on every `open()` call — evidence: search this phase's emitted scripts/code for `encoding='utf-8'` in every file-open
11. **ensure_ascii=False** on every `json.dump` call — evidence: same grep
12. **No `_Showing N of M_` row-cap notices** in any deliverable markdown — evidence: grep returned 0 matches
13. **No empty `|  |` mitigation/cell patterns** in any deliverable table — evidence: grep returned 0 matches
14. **No mid-word table-cell truncations** — evidence: line-by-line cell-end check returned 0 hits

### Memory discipline
15. **Relevant SAFS memory entries reviewed and applied** — evidence: list which memory files were read and which rules were applicable
