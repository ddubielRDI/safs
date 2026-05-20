---
name: phase8.4k-riskreg-win
expert-role: Risk Analyst
domain-expertise: Risk register formatting, tabular risk presentation, mitigation tracking, risk categorization
skill: risk-analyst
---

# Phase 8.4k: Risk Register

## ⛔ NO-TRUNCATION DISCIPLINE (READ FIRST — BLOCKING)

**Render ALL rows. Render FULL text. Render REAL mitigations.** Per SAFS memory (`feedback_screen_encoding_truncation.md`), the win pipeline regressed on 2026-05-19 producing mid-word truncation like "with a repor" / "for entit" / "halt payments and cre", plus `_Showing 15 of 281 risks_` row caps, plus empty `|  |` Mitigation cells across every risk row. The fix discipline is non-negotiable:

- **NEVER `[:N]` slice description, text, mitigation, risk_factors, or any deliverable-content string.** If the source data has 1,200 characters, render 1,200 characters. The evaluator's PDF reader handles long cells; you do not.
- **NEVER cap rows.** No `risks[:15]`, `risks[:N per category]`, `[:5 fallback]`, or any other row-limit pattern. If there are 281 risks, render 281 rows. Tables can be long. That's fine. The bid IS long.
- **NEVER emit `_Showing N of M_` notices.** These hide content from the evaluator. If a table is too big for one page, the PDF renderer handles pagination — but the data stays whole.
- **Mitigation column MUST be populated** from BOTH `mitigation_strategies` (array on structural risks RISK-S-###) AND `mitigation_strategy` (singular string on req-level risks RISK-R-####). Join multi-strategy arrays with `<br/>` so they survive markdown→PDF. If a risk genuinely has no mitigation field populated in source data, emit `[MITIGATION TBD]` — never leave the cell empty `|  |`.

The pipeline produces FULL DATA. Humans decide what to trim. Not the agent.

## Purpose

Generate a formal Risk Register document in tabular format, organized by work section. This provides evaluators with a clear, structured view of identified risks and mitigation strategies — demonstrating proactive risk management.

## ⛔ LANDSCAPE-ORIENTATION DISCIPLINE (added 2026-05-19, BLOCKING)

The Risk Register table has **10 columns**: Risk ID, RTM Risk ID, Description,
Severity, Likelihood, Impact, Mitigation, Owner, Verification, Status. This
**does not fit on Letter portrait**. The 2026-05-19 regression shipped a
portrait PDF whose Owner column was clipped to "Owne" / "Prog" / "Manag",
Verification and Status completely off the page edge.

**Mandatory rule:** phase8e (PDF assembly) MUST render the
`04_RISK_REGISTER.md` volume in **landscape orientation** with a tighter
table CSS so all 10 columns fit edge-to-edge.

The phase8e implementation MUST pass `paper_size="Letter-L"` (or equivalent
landscape paper-size constant) to `Section()` and apply a CSS override with
`table { font-size: 7.5pt; }` for that section. Both `Draft_Bid.pdf` (which
includes the Risk Register appendix) and `ResourceData_A_RISK_REGISTER.pdf`
(the standalone appendix PDF) MUST use landscape.

**Verification:** after phase8e completes, open the standalone risk register
PDF via PyMuPDF, assert `page.rect.width > page.rect.height` for every page,
and assert the strings "Owner", "Verification", and "Status" all appear in
the rendered text. If any column word is missing or truncated (e.g. "Owne"
instead of "Owner"), fail the phase and report which column was clipped.

**Why this exists:** the 10-column risk register is a deliberate design
choice — every column is load-bearing for evaluator traceability (Risk ID
+ RTM Risk ID together close the SVA-7 traceability gap; Mitigation +
Owner + Verification together close the FAR-15 "managed risk" evaluation
criterion). Trimming columns to fit portrait would defeat the purpose.
Landscape is the right answer.

## ⛔ COLUMN HEADER REPEAT DISCIPLINE (added 2026-05-19, BLOCKING)

**Problem:** fitz.Story (PyMuPDF 1.26.x) does NOT repeat `<thead>` on page
breaks even with `display: table-header-group` CSS. The 281-row risk register
rendered as a single markdown table had headers on page 1 only. An evaluator
on page 30 had no column labels — a critical readability defect.

**Rule:** phase8e MUST apply `convert_wide_tables_to_chunked_html()` to the
risk register markdown BEFORE fitz.Story renders it. This function splits
wide tables into multiple HTML tables each with its own `<thead>`, ensuring
column headers appear at the start of every ~3-row chunk.

**Calibrated setting:** `rows_per_chunk=3` for 10-column risk register at
7.5pt landscape — achieves 94% data-page header coverage (45/48 pages).

**Cross-reference:** phase8e-pdf-win.md contains the full implementation
of `convert_wide_tables_to_chunked_html()` and the Wide-table HEAD-repeat
discipline section. This note is a pointer only — implement in phase8e.

**Verification:** PyMuPDF check after render — normalize text (ﬁ→fi, ﬂ→fl),
assert ALL of {"Risk ID", "RTM Risk ID", "Description", "Severity",
"Likelihood", "Impact", "Mitigation", "Owner", "Verification", "Status"}
appear on >=90% of data pages (pages with >200 chars of extracted text).

## Inputs

- `{folder}/shared/REQUIREMENT_RISKS.json` - All risk assessments
- `{folder}/shared/UNIFIED_RTM.json` - Risk-to-requirement-to-bid traceability
- `{folder}/shared/requirements-normalized.json` - Requirements for context
- `{folder}/shared/domain-context.json` - Domain context

## Required Output

- `{folder}/outputs/bid-sections/04_RISK_REGISTER.md` (>5KB)

## Instructions

### Step 1: Load Risk Data

```python
risks_data = read_json(f"{folder}/shared/REQUIREMENT_RISKS.json")
rtm = read_json_safe(f"{folder}/shared/UNIFIED_RTM.json")
requirements = read_json(f"{folder}/shared/requirements-normalized.json")
domain = read_json(f"{folder}/shared/domain-context.json")

all_risks = risks_data.get("risks", [])
rtm_risks = rtm.get("entities", {}).get("risks", []) if rtm else []

# V4-F4 fix 2026-05-18: build a lookup so the rendered register can cite the
# RTM `RISK-###` id alongside each REQUIREMENT_RISKS row. SVA-7 audits the
# register with a `RISK-\d{3}` regex against UNIFIED_RTM.json — without this
# cross-reference every row fails traceability.
def build_rtm_risk_index(rtm_risks_list, all_risks_list):
    """Map REQUIREMENT_RISKS risks to UNIFIED_RTM.json RISK-### ids.

    Heuristic match: same risk_id, same title, OR same linked_requirement_ids overlap.
    Returns dict: source_risk_id -> "RISK-###" (or "RISK-UNMAPPED" if no match).
    """
    index = {}
    for src_risk in all_risks_list:
        src_id = src_risk.get("risk_id", src_risk.get("id", ""))
        src_title = src_risk.get("title", "").strip().lower()
        src_reqs = set(src_risk.get("linked_requirement_ids", []))
        matched = None
        for rtm_risk in rtm_risks_list:
            rtm_id = rtm_risk.get("risk_id", "")
            if src_id and src_id == rtm_risk.get("source_risk_id", ""):
                matched = rtm_id
                break
            if src_title and src_title == rtm_risk.get("title", "").strip().lower():
                matched = rtm_id
                break
            rtm_reqs = set(rtm_risk.get("linked_requirement_ids", []))
            if src_reqs and rtm_reqs and src_reqs & rtm_reqs:
                matched = rtm_id
                break
        index[src_id] = matched or "RISK-UNMAPPED"
    return index

rtm_risk_index = build_rtm_risk_index(rtm_risks, all_risks)
unmapped_count = sum(1 for v in rtm_risk_index.values() if v == "RISK-UNMAPPED")
if unmapped_count:
    log(f"⚠️  {unmapped_count} risks could not be mapped to a UNIFIED_RTM RISK-### id — SVA-7 will flag these. Backfill UNIFIED_RTM.json or update REQUIREMENT_RISKS source_risk_id linkages.")
```

### Step 2: Organize Risks by Category and Severity

```python
risk_categories = {}
for risk in all_risks:
    cat = risk.get("category", risk.get("risk_category", "General"))
    if cat not in risk_categories:
        risk_categories[cat] = []
    risk_categories[cat].append(risk)

# Sort within categories by severity
severity_order = {"CRITICAL": 0, "HIGH": 1, "MEDIUM": 2, "LOW": 3}
for cat in risk_categories:
    risk_categories[cat].sort(key=lambda r: severity_order.get(r.get("severity", "MEDIUM"), 2))

# Count by severity
severity_counts = {"CRITICAL": 0, "HIGH": 0, "MEDIUM": 0, "LOW": 0}
for risk in all_risks:
    sev = risk.get("severity", "MEDIUM")
    severity_counts[sev] = severity_counts.get(sev, 0) + 1
```

### Step 3: Generate Risk Register Document

```markdown
# Risk Register

## Executive Summary

| Severity | Count | Status |
|----------|-------|--------|
| CRITICAL | {critical} | All mitigated |
| HIGH | {high} | All mitigated |
| MEDIUM | {medium} | Monitored |
| LOW | {low} | Accepted |
| **Total** | **{total}** | |

## Risk Assessment Methodology

Resource Data employs a structured risk management approach:
- **Identification**: Systematic analysis of requirements, architecture, and integration points
- **Assessment**: Severity (impact x likelihood) using a 4-level scale
- **Mitigation**: Proactive strategies with assigned owners and verification criteria
- **Monitoring**: Ongoing risk tracking through project governance

---

## Risk Register by Category

> **Risk ID convention:** Each row cites both the source REQUIREMENT_RISKS.json id
> AND the RTM `RISK-###` id from UNIFIED_RTM.json. SVA-7 (risk traceability audit)
> uses a `RISK-\d{3}` pattern to verify every register entry traces back to the RTM —
> rows without an `RTM Risk ID` column will FAIL audit even if the underlying risk
> exists. V4-F4 fix 2026-05-18.

### [Category Name]

| Risk ID | RTM Risk ID | Description | Severity | Likelihood | Impact | Mitigation | Owner | Verification | Status |
|---------|-------------|-------------|----------|------------|--------|------------|-------|--------------|--------|
{for each risk in category: formatted row including RTM_id from UNIFIED_RTM.json entities.risks lookup. If a risk has no RTM_id (e.g., it was added post-Phase 4), use "RISK-UNMAPPED" and flag for backfill.}

[Repeat for each category]

---

## Risk Mitigation Timeline

| Phase | Risks Addressed | Mitigation Activities |
|-------|----------------|----------------------|
| Discovery | {risks} | Initial risk validation, stakeholder review |
| Design | {risks} | Architecture risk mitigations, security review |
| Build | {risks} | Technical risk mitigations, integration testing |
| Test | {risks} | Performance risk validation, UAT |
| Deploy | {risks} | Deployment risk mitigations, rollback plans |

## Risk Monitoring and Reporting

- Weekly risk review in project status meetings
- Monthly risk register updates to steering committee
- Escalation triggers for severity changes
- Lessons learned captured in project knowledge base
```

### Step 4: Write Output

```python
write_file(f"{folder}/outputs/bid-sections/04_RISK_REGISTER.md", register_content)

log(f"""
⚠️ RISK REGISTER COMPLETE (Phase 8.4k)
========================================
Total Risks: {len(all_risks)}
Categories: {len(risk_categories)}
CRITICAL: {severity_counts["CRITICAL"]}
HIGH: {severity_counts["HIGH"]}
MEDIUM: {severity_counts["MEDIUM"]}
LOW: {severity_counts["LOW"]}

Output: outputs/bid-sections/04_RISK_REGISTER.md
""")
```

## Quality Checklist (MANDATORY — report each by name with evidence)

The phase agent MUST verify each of the following BEFORE reporting completion. The agent's completion report MUST include a checklist-results block with:
- Item name (verbatim from below)
- PASS / FAIL / SKIPPED-WITH-REASON
- Evidence (file:line citation, grep result, file size, assertion that ran, etc.)

"All checks passed" without per-item evidence is NOT acceptable.

### Required output files
1. **04_RISK_REGISTER.md** exists at `{folder}/outputs/bid-sections/04_RISK_REGISTER.md` — evidence: `ls -la` showing size > 5,120 bytes

### Schema fidelity
2. **Risk register row count matches `RISKS.json` count ±5%** — evidence: print row count in 04_RISK_REGISTER.md (count `|` separator rows minus header and separator) vs `len(rtm_risks)` in REQUIREMENT_RISKS.json; confirm within ±5%
3. **Every risk row's Mitigation cell traces to source `mitigation_strategy` (singular) OR `mitigation_strategies` (array)** — evidence: grep for empty `|  |` Mitigation cells in HIGH/CRITICAL rows returned 0 matches; confirm `[MITIGATION TBD]` is used only when source data is genuinely empty
4. **All 10 columns present in every data row** — Risk ID, RTM Risk ID, Description, Severity, Likelihood, Impact, Mitigation, Owner, Verification, Status — evidence: count `|` delimiters in a data row (must be 11 for 10 columns); spot-check 3 rows
5. No `_Showing N of M_` notices in document — evidence: grep "_Showing" returned 0 matches
6. No `[:N]` slicing applied to deliverable content strings — evidence: grep for `\[:[0-9]+\]` in production code paths returned 0 hits; confirm NO `risks[:15]` or similar row caps

### Cross-stage consistency
7. **PDF orientation = landscape** — this phase mandates phase8e render 04_RISK_REGISTER.md in landscape (Letter-L); confirm the landscape mandate was passed to phase8e or logged as a requirement — evidence: confirm landscape instruction is present in the output or hand-off notes
8. **Owner column populated** for every HIGH/CRITICAL row — evidence: count HIGH/CRITICAL rows with empty Owner cell (must be 0)
9. **Verification criteria specified** for every HIGH/CRITICAL row — evidence: count HIGH/CRITICAL rows with empty Verification cell (must be 0)

### Anti-regression rules (universal)
10. **UTF-8 encoding** on every `open()` call — evidence: search this phase's emitted scripts/code for `encoding='utf-8'` in every file-open
11. **ensure_ascii=False** on every `json.dump` call — evidence: same grep
12. **No mid-word table-cell truncations** — evidence: line-by-line cell-end check returned 0 hits

### Memory discipline
13. **Relevant SAFS memory entries reviewed and applied** — evidence: list which memory files were read and which rules were applicable (e.g., "NEVER cap rows — rendered all 281 risks per 2026-05-19 discipline; NEVER leave Mitigation cell empty — used [MITIGATION TBD] where needed")
