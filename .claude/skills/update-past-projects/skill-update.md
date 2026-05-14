---
name: update-past-projects
description: Refresh `Past_Projects.md` from web sources — Company Intelligence (awards, certifications, partnerships, employee count, revenue), Government Contracts Summary verification, new project discovery, and opportunistic recency refresh. Auto-modifies the file with a timestamped backup. Default budget ~3-5 min (20 web searches). Triggers on "update past projects", "refresh past projects", "update case studies", "refresh company intel", "refresh past performance".
argument-hint: "[--scope=...] [--budget=N] [--context=path] [--dry-run] [--skip-stale-check]"
allowed-tools: [Bash, Read, Write, Edit, Glob, Grep, WebSearch]
created: 2026-05-14
updated: 2026-05-14
disable-model-invocation: true
---

# /update-past-projects — Refresh Past_Projects.md from Web Sources

## Skill Description

Refreshes `Past_Projects.md` (the single source-of-truth for RDI case studies + Government Contracts + Additional Known Clients + Company Intelligence) from web research. Runs in ~3-5 minutes under a strict web-search budget. Auto-modifies the file with a timestamped backup; use `--dry-run` to preview without writing.

**Designed for two invocation paths:**
- **Standalone:** `/update-past-projects` — manual refresh on demand.
- **Inline from screen pipeline:** invoked automatically as Phase 0.5 of `/process-rfp-screen` (unless `--skip-refresh` is passed) so downstream phases consume the freshest data.

**Key Design Decisions:**
- **Auto-modify with backup** — every run writes `Past_Projects.backup-{YYYY-MM-DD-HHMMSS}.md` adjacent to the source before any edit. Atomic write order: parse → propose → backup → write → verify round-trip.
- **Strict budget** — 20 web searches default. The refresh is fast on purpose; deep verification of every project is out of scope (it would cost 35+ searches alone).
- **Stale check** — if `Past_Projects.md` was modified within the last 7 days, the refresh is skipped (logged) unless `--skip-stale-check` is passed. No point spending 5 minutes re-verifying data that was just curated.
- **Provenance discipline** — every edit carries a source citation in the diff log. The traceability rules from `process-rfp-screen/memory/gotchas.md` apply here too: regional/dated award qualifiers preserved, single-source claims attributed in-line, internal estimates labeled `[estimate]`.
- **Never deletes content** — the skill only adds, updates, or corrects. Removal of historical entries requires manual editing (project sunsets are a human judgment).

---

## Required Outputs (Completion Checklist)

| Required Output | When | Min Size |
|-----------------|------|----------|
| `Past_Projects.backup-{ts}.md` | Always (before any edit) | matches source size |
| `Past_Projects.md` (modified) | When changes proposed | matches source size + delta |
| `Past_Projects.refresh-log.md` | Always (audit trail) | 1KB |
| `Past_Projects.proposals.md` | Only with `--dry-run` | 1KB |

---

## User Invocation

```
/update-past-projects [--scope=A,B,C,D] [--budget=N] [--context=path] [--dry-run] [--skip-stale-check]
```

**Arguments:**
- `--scope=` (optional, default: all four): comma-separated subset of
  - `company-intel` — Company Intelligence section (awards, partnerships, certs, employees, revenue)
  - `contracts` — Government Contracts Summary verification
  - `projects` — new project discovery (last 12 months)
  - `recency` — opportunistic refresh of the 3 oldest dated projects
- `--budget=N` (optional, default: 20): max web searches across all scopes.
- `--context=PATH` (optional): a JSON file with RFP context (`industry`, `agency`, `state`, `scope_keywords`) — used to prioritize new-project discovery searches toward relevant work. Passed automatically when invoked from `/process-rfp-screen`.
- `--dry-run` (optional): compute proposed edits and write `Past_Projects.proposals.md`; do NOT modify `Past_Projects.md`.
- `--skip-stale-check` (optional): bypass the 7-day freshness gate.

---

## Pre-Approved Permissions

| Operation | Scope | Notes |
|-----------|-------|-------|
| Read | `Past_Projects.md`, `Past_Projects.backup-*.md`, `--context` JSON | Source + backups |
| Write | `Past_Projects.md`, `Past_Projects.backup-*.md`, `Past_Projects.refresh-log.md`, `Past_Projects.proposals.md` | Output + backups |
| Edit | `Past_Projects.md` | In-place section updates |
| WebSearch | Max 20 queries (budget-enforced) | Refresh sources |

---

## Configuration

```python
import os, json, shutil
from datetime import datetime, timedelta

# Resolve paths relative to skill location.
SKILL_DIR = os.environ.get("CLAUDE_SKILL_DIR") or os.path.dirname(os.path.abspath(__file__))
if not SKILL_DIR or not os.path.isdir(SKILL_DIR):
    error("ABORT: Cannot resolve SKILL_DIR — set CLAUDE_SKILL_DIR or invoke from the skill directory")
    halt()

# Upward search for Past_Projects.md (same logic as process-rfp-screen).
def _find_past_projects(start_dir, max_up=6):
    cur = os.path.abspath(start_dir)
    for _ in range(max_up):
        candidate = os.path.join(cur, "Past_Projects.md")
        if os.path.isfile(candidate):
            return candidate
        parent = os.path.dirname(cur)
        if parent == cur:
            break
        cur = parent
    return None

PAST_PROJECTS = _find_past_projects(SKILL_DIR)
if not PAST_PROJECTS:
    error("ABORT: Past_Projects.md not found in any ancestor of SKILL_DIR")
    halt()

REPO_ROOT = os.path.dirname(PAST_PROJECTS)

# Defaults — overridable via args.
DEFAULT_BUDGET = 20  # max web searches across all scopes
STALE_THRESHOLD_DAYS = 7  # skip refresh if file modified within this window
DEFAULT_SCOPE = ["company-intel", "contracts", "projects", "recency"]

# Per-scope sub-budgets (ratios of total budget). The "recency" scope is opportunistic
# and consumes only leftover budget. New-project discovery scales with RFP context.
SCOPE_BUDGETS = {
    "company-intel": 5,   # awards, Top Workplaces, partnerships, employee count, recent news
    "contracts":     4,   # verify GSA + DIR + ITPS + MNSITE/MPSA/TOPS contracts
    "projects":      8,   # new project discovery — opportunistically scoped to RFP context
    "recency":       3,   # the 3 oldest dated projects only (NOT all 35)
}

# Output paths.
REFRESH_LOG = os.path.join(REPO_ROOT, "Past_Projects.refresh-log.md")
PROPOSALS_OUT = os.path.join(REPO_ROOT, "Past_Projects.proposals.md")
```

---

## Execution Protocol

### Step 0: Parse Arguments, Stale Check, Backup

```python
# Parse arguments.
args = user_input.strip().split()
scope = DEFAULT_SCOPE
budget = DEFAULT_BUDGET
context_path = None
dry_run = False
skip_stale_check = False

for arg in args:
    if arg.startswith("--scope="):
        scope = [s.strip() for s in arg.split("=", 1)[1].split(",") if s.strip()]
    elif arg.startswith("--budget="):
        try:
            budget = max(1, int(arg.split("=", 1)[1]))
        except ValueError:
            error(f"ABORT: --budget must be a positive integer, got {arg}")
            halt()
    elif arg.startswith("--context="):
        context_path = arg.split("=", 1)[1]
    elif arg == "--dry-run":
        dry_run = True
    elif arg == "--skip-stale-check":
        skip_stale_check = True

# Validate scope choices.
valid_scopes = set(DEFAULT_SCOPE)
unknown = [s for s in scope if s not in valid_scopes]
if unknown:
    error(f"ABORT: unknown --scope values: {unknown}. Valid: {sorted(valid_scopes)}")
    halt()

# Load RFP context if provided.
rfp_context = {}
if context_path and os.path.exists(context_path):
    rfp_context = read_json(context_path)
    log(f"  Context loaded: industry={rfp_context.get('industry_domain', '?')}, "
        f"agency={rfp_context.get('client_name', '?')}, "
        f"state={rfp_context.get('client_state', '?')}")

# Stale check — skip refresh if Past_Projects.md is fresh.
mtime = datetime.fromtimestamp(os.path.getmtime(PAST_PROJECTS))
age_days = (datetime.now() - mtime).days
if age_days < STALE_THRESHOLD_DAYS and not skip_stale_check:
    log(f"  Past_Projects.md modified {age_days} day(s) ago (< {STALE_THRESHOLD_DAYS}-day threshold)")
    log(f"  Skipping refresh. Use --skip-stale-check to force run.")
    write_file(REFRESH_LOG, f"# Past_Projects.md refresh — {datetime.now().isoformat()}\n\n"
                            f"SKIPPED: source modified {age_days} day(s) ago (fresh).\n")
    return

# Write backup BEFORE any edits. Timestamp format avoids collisions on rapid re-runs.
timestamp = datetime.now().strftime("%Y-%m-%d-%H%M%S")
backup_path = os.path.join(REPO_ROOT, f"Past_Projects.backup-{timestamp}.md")
shutil.copy2(PAST_PROJECTS, backup_path)
log(f"  Backup written: {os.path.basename(backup_path)}")

log("=" * 60)
log("PAST_PROJECTS REFRESH")
log(f"Source: {PAST_PROJECTS}")
log(f"Scope: {', '.join(scope)}")
log(f"Budget: {budget} web searches")
log(f"Mode: {'DRY-RUN (no writes)' if dry_run else 'AUTO-MODIFY'}")
log("=" * 60)
```

### Step 1: Parse Current Past_Projects.md State

```python
content = read_file(PAST_PROJECTS)

# Locate the four major sections we operate on. Each parser returns (start_line, end_line, body).
# Reuse the regex patterns proven by process-rfp-screen Phase 4 to avoid drift.
import re

def _section_bounds(text, heading_pattern, end_pattern=r'\n##\s+|\Z'):
    """Find a section by heading; return (start, end, body) or None."""
    match = re.search(heading_pattern + r'(.*?)(?=' + end_pattern + r')', text, re.DOTALL)
    if not match:
        return None
    return (match.start(), match.end(), match.group(1).strip())

current = {
    "company_intel":     _section_bounds(content, r'##\s+Company Intelligence'),
    "gov_contracts":     _section_bounds(content, r'##\s+Government Contracts Summary'),
    "known_clients":     _section_bounds(content, r'##\s+Additional Known Clients'),
}

# Parse the 35 project headings + dates (for recency refresh).
project_headings = re.findall(r'^####\s+(\d+)\.\s+(.+?)$', content, re.MULTILINE)
project_dates = []
for m in re.finditer(r'####\s+(\d+)\.\s+(.+?)\n.*?\|\s*\*\*(?:Timeline|Duration|Period)\*\*\s*\|\s*(.+?)\s*\|', content, re.DOTALL):
    project_dates.append({"num": int(m.group(1)), "title": m.group(2), "timeline": m.group(3)})

log(f"  Parsed {len(project_headings)} project headings")
log(f"  Sections present: " + ", ".join(k for k, v in current.items() if v))
```

### Step 2: Refresh Company Intelligence (if in scope)

The Company Intelligence section is the highest-value refresh target — its claims feed Phase 2's competitive-position scoring and Phase 4's compliance check in `/process-rfp-screen`. Each search is targeted; results are LLM-synthesized into a structured update block.

```python
edits = []  # list of {section, before, after, source, rule_warnings}
budget_remaining = budget

if "company-intel" in scope:
    log("\n[Company Intelligence]")
    sub_budget = min(SCOPE_BUDGETS["company-intel"], budget_remaining)

    # Five targeted queries, ordered by decision impact. The LLM synthesizes each
    # response into a structured update proposal that preserves source attribution
    # for every claim (Rule 4 from process-rfp-screen traceability discipline).
    company_queries = [
        '"Resource Data Inc" Anchorage Alaska awards 2026',
        '"Resource Data Inc" "Top Workplaces" ranking',
        '"Resource Data Inc" Esri OR Snowflake OR Databricks partnership',
        '"Resource Data Inc" employees revenue OR "annual revenue"',
        '"Resource Data Inc" news 2026',
    ][:sub_budget]

    web_results = []
    for q in company_queries:
        result = web_search(q)
        web_results.append({"query": q, "result": result})
        budget_remaining -= 1

    # LLM synthesis: parse the web results into structured proposals.
    synthesis_prompt = f"""You are refreshing the "Company Intelligence" section of Past_Projects.md
for Resource Data, Inc. (RDI).

CURRENT SECTION CONTENT:
{current["company_intel"][2] if current["company_intel"] else "(missing)"}

WEB RESEARCH RESULTS:
{json.dumps(web_results, indent=2)[:30000]}

For each claim that should be UPDATED or ADDED, return one proposal with this schema:
{{
  "field":      "awards" | "partnerships" | "certifications" | "employees" | "revenue" | "recent_news",
  "before":     <current text or null if new>,
  "after":      <proposed replacement text, preserving regional/year qualifiers verbatim>,
  "source":     <URL of the web result that supports this claim>,
  "confidence": "Verified" | "Documented" | "Inferred" | "Unknown",
  "warnings":   [<list of Rule violations if any: "single-source", "regional-qualifier-dropped", "monetary-estimate-unlabeled">]
}}

Rules:
- Preserve regional / partial-award scope qualifiers verbatim (e.g., "Top Workplaces #N Midsize Employers — Southwest WA/OR, The Oregonian")
- Label monetary estimates with [estimate] + methodology
- Mark single-source claims with `"warnings": ["single-source"]` so they can be flagged in downstream consumption
- Do NOT remove existing content unless the web evidence contradicts it; in that case mark warnings=["contradicts-current"]

Return JSON only: {{"proposals": [...]}}. Empty array means no updates."""

    synthesis = invoke_llm(synthesis_prompt)
    proposals = synthesis.get("proposals", [])
    log(f"  Used {len(company_queries)} searches, {len(proposals)} proposals")
    edits.extend({"section": "company_intel", **p} for p in proposals)
```

### Step 3: Verify Government Contracts (if in scope)

```python
if "contracts" in scope and budget_remaining > 0:
    log("\n[Government Contracts]")
    sub_budget = min(SCOPE_BUDGETS["contracts"], budget_remaining)

    # The contracts to verify (sourced from Past_Projects.md Government Contracts Summary).
    # Batched into 4 searches: federal GSA, Alaska/Oregon/Washington, Texas DIR, Minnesota MNSITE.
    contract_queries = [
        '"Resource Data" "GS-35F-0229S" GSA Schedule',
        '"Resource Data" Alaska TOPS OR Oregon MPSA OR Washington ITPS contract',
        '"Resource Data" Texas DIR-CPO-6036 OR DIR-CPO-6069',
        '"Resource Data" Minnesota MNSITE professional technical',
    ][:sub_budget]

    web_results = []
    for q in contract_queries:
        result = web_search(q)
        web_results.append({"query": q, "result": result})
        budget_remaining -= 1

    synthesis_prompt = f"""You are verifying Government Contracts Summary entries in Past_Projects.md.

CURRENT SECTION CONTENT:
{current["gov_contracts"][2] if current["gov_contracts"] else "(missing)"}

WEB RESEARCH RESULTS:
{json.dumps(web_results, indent=2)[:30000]}

For each contract listed in the current section, determine status from web evidence:
- "active": contract still in force, web confirms current period of performance
- "expired": web evidence shows contract ended without renewal
- "renewed": new contract number or extended period found
- "unchanged": no new web evidence; keep as-is
- "unverifiable": no web evidence either way (do NOT mark expired in this case)

Return JSON: {{"contract_updates": [{{"contract_id": ..., "status": ..., "evidence": ..., "proposed_text": ..., "source": <url>}}]}}.
Only return entries that need a change. Preserve all existing contract IDs unless web evidence contradicts."""

    synthesis = invoke_llm(synthesis_prompt)
    proposals = synthesis.get("contract_updates", [])
    log(f"  Used {len(contract_queries)} searches, {len(proposals)} contract updates")
    edits.extend({"section": "gov_contracts", **p} for p in proposals)
```

### Step 4: New Project Discovery (if in scope)

This step scales with RFP context. If `--context` was provided, queries are scoped to the RFP industry/agency/state to find directly-relevant new projects. Without context, the search is generic ("last 12 months RDI case studies").

```python
if "projects" in scope and budget_remaining > 0:
    log("\n[New Project Discovery]")
    sub_budget = min(SCOPE_BUDGETS["projects"], budget_remaining)

    # Build context-aware queries. Existing project titles are passed as an EXCLUSION list
    # so the LLM only proposes genuinely new entries.
    existing_titles = [t for _, t in project_headings]

    if rfp_context:
        industry = rfp_context.get("industry_domain", "")
        agency = rfp_context.get("client_name", "")
        state = rfp_context.get("client_state", "")
        scope_kws = rfp_context.get("scope_keywords", [])[:3]
        project_queries = [
            f'"Resource Data" {industry} case study 2025 OR 2026',
            f'"Resource Data" {agency}' if agency else f'"Resource Data" government contract 2026',
            f'"Resource Data" {state}' if state else f'"Resource Data" Pacific Northwest project 2026',
            f'"Resource Data" {" OR ".join(scope_kws)} project',
            'site:resourcedata.com case study 2025 OR 2026',
            '"Resource Data" press release 2026',
            '"Resource Data" "we delivered" OR "we built" 2025 OR 2026',
            '"Resource Data Inc" customer story',
        ][:sub_budget]
    else:
        project_queries = [
            '"Resource Data" case study 2026',
            '"Resource Data" press release 2025 OR 2026',
            'site:resourcedata.com case study',
            '"Resource Data" "we delivered" 2025 OR 2026',
            '"Resource Data Inc" project announcement',
            '"Resource Data" customer story 2025',
        ][:sub_budget]

    web_results = []
    for q in project_queries:
        result = web_search(q)
        web_results.append({"query": q, "result": result})
        budget_remaining -= 1

    synthesis_prompt = f"""You are discovering NEW projects for Resource Data, Inc. (RDI) that are not
already documented in Past_Projects.md.

EXISTING PROJECT TITLES (do NOT re-propose these):
{json.dumps(existing_titles, indent=2)}

WEB RESEARCH RESULTS:
{json.dumps(web_results, indent=2)[:40000]}

For each NEW project you can identify with at least 2 corroborating signals (multiple URLs,
press release + client testimonial, etc.), propose a Past_Projects.md entry using this schema:
{{
  "project_number":   <next available int after {len(existing_titles)}>,
  "title":            <project name>,
  "client":           <client organization>,
  "industry":         <one of: Education, Fisheries, Government, Manufacturing, Mining, Natural Resources, Oil & Gas, Transportation, Utilities>,
  "timeline":         <year range>,
  "technologies":     [<tech list from sources>],
  "summary":          <2-3 sentence project description, prose>,
  "key_outcomes":     [<measurable outcomes, if cited>],
  "challenges":       [<challenges addressed, if cited>],
  "quote_text":       <client quote if available, verbatim>,
  "quote_attribution": <speaker name + role + organization, if available>,
  "sources":          [<url1>, <url2>, ...],
  "confidence":       "Verified" | "Documented" | "Inferred",
  "warnings":         [<warnings: "single-source", "no-metrics", "no-quote">]
}}

Rules:
- Require AT LEAST 2 independent web sources per proposal (Rule 4 — single-source claims).
  If only 1 source exists, return the proposal with confidence="Documented" and warnings=["single-source"]
  so a human can review before merge.
- Do NOT invent metrics, dates, or quotes. If the web doesn't cite them, leave the field null.
- Preserve client/organization names verbatim (Rule 1 — distinguish contracting client from end-user).

Return JSON only: {{"new_projects": [...]}}. Empty array means no new projects discovered."""

    synthesis = invoke_llm(synthesis_prompt)
    proposals = synthesis.get("new_projects", [])
    log(f"  Used {len(project_queries)} searches, {len(proposals)} new project proposals")
    edits.extend({"section": "new_project", **p} for p in proposals)
```

### Step 5: Opportunistic Recency Refresh (if in scope, budget permitting)

```python
if "recency" in scope and budget_remaining > 0:
    log("\n[Recency Refresh — oldest 3 projects]")

    # Sort projects by timeline (oldest first). Refresh only the 3 oldest dated projects
    # — refreshing all 35 would cost the entire budget. The oldest projects are also the
    # most likely to be obsolete / replaced / sunset.
    def _earliest_year(timeline):
        years = re.findall(r'\b(19\d{2}|20\d{2})\b', timeline or "")
        return min((int(y) for y in years), default=9999)

    dated = sorted(project_dates, key=lambda p: _earliest_year(p["timeline"]))
    targets = dated[:min(3, budget_remaining)]

    if not targets:
        log("  No timelined projects parsed; skipping recency refresh")
    else:
        web_results = []
        for proj in targets:
            q = f'"{proj["title"]}" "Resource Data" status OR update OR 2025 OR 2026'
            result = web_search(q)
            web_results.append({"project_number": proj["num"], "title": proj["title"], "result": result})
            budget_remaining -= 1
            if budget_remaining <= 0:
                break

        synthesis_prompt = f"""You are checking the current status of these older RDI projects.

PROJECTS UNDER REFRESH:
{json.dumps([{"num": p["num"], "title": p["title"], "timeline": p["timeline"]} for p in targets], indent=2)}

WEB RESEARCH RESULTS:
{json.dumps(web_results, indent=2)[:25000]}

For each project, return EITHER nothing (no new info found) OR a status update:
{{
  "project_number":  <int>,
  "update_type":     "outcome" | "metric" | "ongoing" | "completed" | "sunset",
  "addendum":        <text to APPEND to the project's existing entry — do NOT rewrite the original>,
  "source":          <url>,
  "confidence":      "Verified" | "Documented" | "Inferred",
  "warnings":        [<warnings>]
}}

Rules:
- NEVER rewrite the original project body. Only APPEND a status addendum after the existing content.
- Require at least 1 reliable source (org website, press release, government records).
- Skip the project if no actionable web evidence exists.

Return JSON only: {{"recency_updates": [...]}}."""

        synthesis = invoke_llm(synthesis_prompt)
        proposals = synthesis.get("recency_updates", [])
        log(f"  Used {len(web_results)} searches, {len(proposals)} recency updates")
        edits.extend({"section": "recency", **p} for p in proposals)
```

### Step 6: Apply Edits with Provenance + Atomic Save

```python
log(f"\n[Apply Edits] {len(edits)} proposal(s), budget used: {budget - budget_remaining}/{budget}")

if not edits:
    log("  No edits to apply. Past_Projects.md unchanged.")
    write_file(REFRESH_LOG, f"# Past_Projects.md refresh — {datetime.now().isoformat()}\n\n"
                            f"No proposals from {budget - budget_remaining} web searches.\n"
                            f"Backup: {os.path.basename(backup_path)} (no diff vs source)\n")
    # Remove the unchanged backup (atomic — no point keeping zero-delta backups).
    if not dry_run:
        os.remove(backup_path)
    return

# In --dry-run mode, write proposals to a separate file and stop.
if dry_run:
    proposals_md = _format_proposals_markdown(edits)
    write_file(PROPOSALS_OUT, proposals_md)
    log(f"  DRY-RUN: {len(edits)} proposals written to {os.path.basename(PROPOSALS_OUT)}")
    log("  Past_Projects.md NOT modified.")
    return

# Apply edits in place. Use the Edit tool semantics: each proposal is a discrete
# old_string → new_string replacement. Apply in reverse-line order so earlier edits
# don't shift the offsets of later ones.
updated_content = content

for edit in edits:
    section = edit["section"]

    if section == "company_intel":
        # Field-level update inside the Company Intelligence section.
        before = edit.get("before") or ""
        after = edit.get("after") or ""
        if before and before in updated_content:
            updated_content = updated_content.replace(before, after, 1)
        elif not before:
            # New field — insert at end of Company Intelligence section.
            section_match = re.search(r'(##\s+Company Intelligence.*?)(\n##\s+|\Z)', updated_content, re.DOTALL)
            if section_match:
                insert_point = section_match.end(1)
                updated_content = updated_content[:insert_point] + f"\n- {after} ([source]({edit['source']}))\n" + updated_content[insert_point:]

    elif section == "gov_contracts":
        # Contract status update — append a status note adjacent to the contract row.
        contract_id = edit.get("contract_id", "")
        proposed = edit.get("proposed_text", "")
        if contract_id and contract_id in updated_content:
            # Insert status annotation after the contract row.
            status_note = f" [{edit['status']} as of {datetime.now().strftime('%Y-%m-%d')} — [source]({edit['source']})]"
            updated_content = updated_content.replace(contract_id, contract_id + status_note, 1)

    elif section == "new_project":
        # Append a new ####N. project block at the end of the projects list.
        new_block = _format_new_project_block(edit)
        # Insert before the first "## " heading that follows the project block (usually Government Contracts Summary).
        insertion_match = re.search(r'\n(##\s+Government Contracts Summary)', updated_content)
        if insertion_match:
            insert_point = insertion_match.start()
            updated_content = updated_content[:insert_point] + "\n" + new_block + "\n" + updated_content[insert_point:]
        else:
            # Fallback: append to end.
            updated_content += "\n" + new_block + "\n"

    elif section == "recency":
        # Append a status addendum to the existing project's body.
        proj_num = edit["project_number"]
        proj_pattern = rf'(####\s+{proj_num}\.\s+.+?)(?=\n####\s+\d+\.|\n##\s+|\Z)'
        proj_match = re.search(proj_pattern, updated_content, re.DOTALL)
        if proj_match:
            addendum = f"\n\n**Status update ({datetime.now().strftime('%Y-%m-%d')}):** {edit['addendum']} ([source]({edit['source']}))\n"
            insert_point = proj_match.end()
            updated_content = updated_content[:insert_point] + addendum + updated_content[insert_point:]

# Atomic write: write to a .tmp file, parse-back verify, then rename.
tmp_path = PAST_PROJECTS + ".tmp"
write_file(tmp_path, updated_content)

# Verify the written file still parses with the same section structure we started with.
# If a regex section bound is lost, the edits corrupted the structure — abort and restore from backup.
verify_content = read_file(tmp_path)
verify_sections = {
    "company_intel": _section_bounds(verify_content, r'##\s+Company Intelligence'),
    "gov_contracts": _section_bounds(verify_content, r'##\s+Government Contracts Summary'),
}
if not verify_sections["company_intel"] or not verify_sections["gov_contracts"]:
    error("ABORT: post-edit Past_Projects.md is missing expected sections; restoring from backup")
    shutil.copy2(backup_path, PAST_PROJECTS)
    os.remove(tmp_path)
    halt()

# Atomic rename: tmp → final.
os.replace(tmp_path, PAST_PROJECTS)
log(f"  Past_Projects.md updated ({len(edits)} edits applied)")
log(f"  Backup retained: {os.path.basename(backup_path)}")
```

### Step 7: Write Refresh Log

```python
log_lines = [
    f"# Past_Projects.md refresh — {datetime.now().isoformat()}",
    "",
    f"**Source:** `{PAST_PROJECTS}`",
    f"**Backup:** `{os.path.basename(backup_path)}`" if not dry_run else f"**Dry-run output:** `{os.path.basename(PROPOSALS_OUT)}`",
    f"**Scope:** {', '.join(scope)}",
    f"**Budget used:** {budget - budget_remaining}/{budget} web searches",
    f"**Edits applied:** {len(edits)}" if not dry_run else f"**Edits proposed:** {len(edits)}",
    "",
    "## Per-section summary",
    "",
]

by_section = {}
for e in edits:
    by_section.setdefault(e["section"], []).append(e)

for section, items in by_section.items():
    log_lines.append(f"### {section}")
    log_lines.append("")
    for item in items:
        rule_warnings = item.get("warnings", [])
        warn_tag = f" ⚠️ {','.join(rule_warnings)}" if rule_warnings else ""
        confidence = item.get("confidence", "Unknown")
        source = item.get("source", "(no source)")
        if section == "new_project":
            log_lines.append(f"- **NEW** #{item.get('project_number', '?')}: {item.get('title', '?')} "
                             f"[{confidence}]{warn_tag} — source: {source}")
        elif section == "recency":
            log_lines.append(f"- **#{item['project_number']}** {item.get('update_type', '?')}: "
                             f"{item.get('addendum', '')[:120]}... [{confidence}]{warn_tag}")
        elif section == "gov_contracts":
            log_lines.append(f"- **{item.get('contract_id', '?')}** → {item.get('status', '?')} "
                             f"[{confidence}]{warn_tag} — source: {source}")
        elif section == "company_intel":
            log_lines.append(f"- **{item.get('field', '?')}**: {item.get('after', '')[:120]}... "
                             f"[{confidence}]{warn_tag} — source: {source}")
    log_lines.append("")

# Surface rule-violation warnings prominently.
all_warnings = [w for e in edits for w in e.get("warnings", [])]
if all_warnings:
    log_lines.append("## ⚠️  Warnings (rule violations to review)")
    log_lines.append("")
    for e in edits:
        if e.get("warnings"):
            log_lines.append(f"- `{e['section']}` / {e.get('title') or e.get('field') or e.get('contract_id') or '?'}: {', '.join(e['warnings'])}")
    log_lines.append("")

write_file(REFRESH_LOG, "\n".join(log_lines))
log(f"  Refresh log: {os.path.basename(REFRESH_LOG)}")
```

### Step 8: Report

```
PAST_PROJECTS REFRESH COMPLETE
================================
Source:        Past_Projects.md
Backup:        Past_Projects.backup-{timestamp}.md
Scope:         {scope}
Budget used:   {used}/{budget} web searches
Edits applied: {count}
Warnings:      {warning_count} (see Past_Projects.refresh-log.md)

Per-section:
  Company Intel:      {n} updates
  Government Contracts: {n} status changes
  New Projects:        {n} added
  Recency Refresh:     {n} addenda

Log: Past_Projects.refresh-log.md
{("Proposals: Past_Projects.proposals.md (DRY-RUN — source not modified)" if dry_run else "")}
```

---

## Helper Functions

```python
def _format_new_project_block(edit):
    """Render a new project proposal as a Past_Projects.md ####N. block."""
    num = edit["project_number"]
    title = edit["title"]
    client = edit.get("client", "Unknown")
    industry = edit.get("industry", "Unknown")
    timeline = edit.get("timeline", "Unknown")
    techs = edit.get("technologies", [])
    summary = edit.get("summary", "")
    outcomes = edit.get("key_outcomes", [])
    challenges = edit.get("challenges", [])
    quote_text = edit.get("quote_text")
    quote_attribution = edit.get("quote_attribution")
    sources = edit.get("sources", [])

    lines = [
        f"#### {num}. {title}",
        "",
        f"| **Client** | {client} |",
        f"| **Industry** | {industry} |",
        f"| **Timeline** | {timeline} |",
        f"| **Technologies** | {', '.join(techs)} |" if techs else "",
        "",
        summary,
        "",
    ]
    if outcomes:
        lines += ["**Key Outcomes:**", *[f"- {o}" for o in outcomes], ""]
    if challenges:
        lines += ["**Challenges Addressed:**", *[f"- {c}" for c in challenges], ""]
    if quote_text:
        lines += [f'*"{quote_text}"* — {quote_attribution or "client (unattributed)"}', ""]
    if sources:
        lines += ["**Sources:**", *[f"- {s}" for s in sources], ""]
    return "\n".join(filter(None, lines))


def _format_proposals_markdown(edits):
    """Render proposals as a human-reviewable markdown file (used in --dry-run mode)."""
    lines = [
        f"# Past_Projects.md — Proposed Updates ({datetime.now().isoformat()})",
        "",
        f"**Total proposals:** {len(edits)}",
        "",
        "Review each proposal below. To apply, re-run `/update-past-projects` without `--dry-run`.",
        "",
    ]
    for i, e in enumerate(edits, 1):
        lines.append(f"## {i}. [{e['section']}] {e.get('title') or e.get('field') or e.get('contract_id') or 'update'}")
        lines.append("")
        lines.append(f"- **Confidence:** {e.get('confidence', 'Unknown')}")
        lines.append(f"- **Source:** {e.get('source', '(no source)')}")
        if e.get("warnings"):
            lines.append(f"- **⚠️  Warnings:** {', '.join(e['warnings'])}")
        lines.append("")
        if e.get("before"):
            lines.append("**Before:**")
            lines.append(f"> {e['before']}")
            lines.append("")
        if e.get("after"):
            lines.append("**After:**")
            lines.append(f"> {e['after']}")
            lines.append("")
        if e.get("addendum"):
            lines.append(f"**Append to project #{e.get('project_number', '?')}:**")
            lines.append(f"> {e['addendum']}")
            lines.append("")
        lines.append("---")
        lines.append("")
    return "\n".join(lines)
```

---

## Memory Integration

**On skill start:** Read `${CLAUDE_SKILL_DIR}/memory/`:
- `gotchas.md` — known pitfalls (e.g., parsing quirks in Past_Projects.md sections, web-source patterns that produce hallucinated project names)

**On skill end — write checklist:**
- [ ] Did web search return zero or all-irrelevant results for any scope? → record query patterns that need refining
- [ ] Did the LLM synthesis propose obviously-wrong claims (wrong client, fabricated metric)? → record the source patterns that triggered hallucination
- [ ] Did a section regex fail to parse after edits? → record the structural pattern that broke
- [ ] Did the backup mechanism fail (disk full, permission denied)? → record + escalate
- [ ] Routine refresh with no surprises? → skip

---

## Error Handling

| Scenario | Action |
|----------|--------|
| `Past_Projects.md` not found | ABORT — cannot refresh what doesn't exist |
| Backup write fails | ABORT before any edits — no edits without provable rollback |
| Web search budget exhausted mid-scope | Continue with remaining scopes at reduced budget; log the truncation |
| LLM synthesis returns malformed JSON | Skip that scope; log + continue with remaining scopes |
| Post-edit section parse fails | RESTORE from backup, abort |
| Stale-check skip | Write refresh log noting SKIPPED; exit cleanly |

---

## Quality Checklist

- [ ] Backup written BEFORE any edit to `Past_Projects.md`
- [ ] Atomic write order respected: parse → propose → backup → write tmp → verify → rename
- [ ] Every edit carries a source citation (URL) in the refresh log
- [ ] No edit removes existing content (only adds, updates, or appends)
- [ ] Single-source claims are flagged with `warnings: ["single-source"]`
- [ ] Regional / partial-award scope qualifiers preserved verbatim from web sources
- [ ] Monetary estimates labeled `[estimate]` with methodology
- [ ] New project proposals require at least 2 corroborating sources (or single-source warning)
- [ ] Web search budget never exceeded
- [ ] Stale check honored unless `--skip-stale-check` passed
- [ ] Refresh log written even on no-edit / skipped runs (provides audit trail)
- [ ] Post-edit verification confirms all expected sections still parse
- [ ] `--dry-run` mode writes proposals file and never touches `Past_Projects.md`
