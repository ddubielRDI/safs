---
name: phase8.6-integration-win
expert-role: Integration Architect
domain-expertise: Multi-vendor coordination, system integration planning, API architecture, data migration
---

# Phase 8.6: Technical Integration Plan

## ⛔ NO-TRUNCATION DISCIPLINE (READ FIRST — BLOCKING)

**Render ALL integration risks. Render FULL descriptions and mitigations.** Per SAFS memory (`feedback_screen_encoding_truncation.md`), this phase regressed 2026-05-19 with `integration_risks[:6]` cap and `desc[:220]` / `mit_text[:200]` truncations in its risk-summary table. The rule:

- **NEVER `[:N]` slice description, mitigation, or any deliverable-content string.**
- **NEVER cap rows** (`integration_risks[:6]`, `top_risks[:N]`, etc.). All applicable risks render.
- **Mitigation cells** populated from BOTH `mitigation_strategies` array AND `mitigation_strategy` singular field.

Pipelines produce FULL DATA. The PDF renderer handles pagination.

## Expert Role

You are an **Integration Architect** with expertise in:
- Multi-vendor and multi-system coordination
- API architecture and service orchestration
- Data migration and conversion planning
- Integration testing strategy

## Purpose

Generate the Technical Integration Plan — demonstrating how the proposed solution integrates with the client's existing systems, third-party vendors, and infrastructure. Addresses evaluator concerns about "Will this actually work in our environment?"

## Inputs

- `{folder}/outputs/ARCHITECTURE.md` - System architecture
- `{folder}/outputs/INTEROPERABILITY.md` - Integration specifications
- `{folder}/shared/bid-context-bundle.json` - Context
- `{folder}/shared/requirements-normalized.json` - Integration requirements
- `{folder}/shared/domain-context.json` - Client environment context
- `{folder}/shared/REQUIREMENT_RISKS.json` - Integration risks

## Required Output

- `{folder}/outputs/bid-sections/06_INTEGRATION.md` (>8KB)

## Instructions

### Step 1: Load Integration Context

```python
architecture = read_file(f"{folder}/outputs/ARCHITECTURE.md")
interop = read_file(f"{folder}/outputs/INTEROPERABILITY.md")
context = read_json(f"{folder}/shared/bid-context-bundle.json")
requirements = read_json(f"{folder}/shared/requirements-normalized.json")
domain = read_json(f"{folder}/shared/domain-context.json")
risks = read_json(f"{folder}/shared/REQUIREMENT_RISKS.json")

# Filter for integration-related requirements
all_reqs = requirements.get("requirements", [])
integration_reqs = [
    r for r in all_reqs
    if any(kw in r.get("text", "").lower()
           for kw in ["integrat", "interface", "api", "import", "export", "migrate",
                       "connect", "interop", "exchange", "sync"])
    or r.get("category", "").lower() in ["integration", "interoperability", "data exchange"]
]
```

### Step 2: Generate Integration Plan

```markdown
# Technical Integration Plan

## 1. Integration Overview

### 1.1 Integration Scope
[Summary of all integration points identified from requirements
and interoperability specifications. Table: System | Direction | Protocol | Data]

### 1.2 Integration Architecture
[High-level integration architecture. Reference ARCHITECTURE.md
and INTEROPERABILITY.md. Show how the solution connects to
the client's existing ecosystem.]

## 2. External System Integrations

### 2.1 [System Name 1]
[For each external system identified in the RFP:]
- **Interface Type**: REST API / SOAP / File Transfer / Database Link
- **Direction**: Inbound / Outbound / Bidirectional
- **Data Format**: JSON / XML / CSV / Custom
- **Frequency**: Real-time / Batch / On-demand
- **Authentication**: OAuth 2.0 / API Key / Certificate
- **Error Handling**: Retry policy, dead letter queue, alerting
- **SLA**: Latency targets, availability requirements

[Repeat for each integration point]

## 3. Data Migration Plan

### 3.1 Migration Strategy
[Approach to migrating data from legacy systems.
Phased migration vs. big-bang. Data validation approach.]

### 3.2 Data Mapping
[Table: Source Field | Target Field | Transformation | Validation
Cover key data entities from ENTITY_DEFINITIONS.md]

### 3.3 Migration Execution
[Timeline, rollback plan, data verification,
parallel run period if applicable.]

## 4. API Management

### 4.1 API Standards
[API design standards: REST, versioning, authentication,
rate limiting, documentation (OpenAPI/Swagger).]

### 4.2 API Catalog
[Table: API Endpoint | Method | Purpose | Consumer
List key APIs the solution will expose/consume.]

## 5. Multi-Vendor Coordination

### 5.1 Vendor Touchpoints
[If multiple vendors involved: coordination approach,
interface agreements, testing responsibilities.]

### 5.2 Responsibility Matrix
[Table: Integration | Our Responsibility | Client Responsibility | Vendor Responsibility]

## 6. Integration Testing

### 6.1 Testing Strategy
[Integration testing approach: stub/mock services,
end-to-end testing, performance testing of interfaces.]

### 6.2 Test Environments
[Environment strategy for integration testing.
How to simulate external systems.]

## 7. Integration Risks and Mitigations

| Risk | Severity | Mitigation |
|------|----------|------------|
{integration risks from REQUIREMENT_RISKS.json}

## 8. Ongoing Integration Support

### 8.1 Monitoring
[API monitoring, health checks, alerting for integration failures.]

### 8.2 Change Management
[How integration changes are managed: API versioning,
backward compatibility, deprecation policy.]
```

### Step 3: Write Output

```python
write_file(f"{folder}/outputs/bid-sections/06_INTEGRATION.md", integration_content)

log(f"""
🔗 TECHNICAL INTEGRATION PLAN COMPLETE (Phase 8.6)
====================================================
Integration Requirements: {len(integration_reqs)}
External Systems Identified: {external_system_count}
APIs Defined: {api_count}
Integration Risks: {integration_risk_count}

Output: outputs/bid-sections/06_INTEGRATION.md
""")
```

## Quality Checklist (MANDATORY — report each by name with evidence)

The phase agent MUST verify each of the following BEFORE reporting completion. The agent's completion report MUST include a checklist-results block with:
- Item name (verbatim from below)
- PASS / FAIL / SKIPPED-WITH-REASON
- Evidence (file:line citation, grep result, file size, assertion that ran, etc.)

"All checks passed" without per-item evidence is NOT acceptable.

### Required output files
1. **06_INTEGRATION.md** exists at `{folder}/outputs/bid-sections/06_INTEGRATION.md` — evidence: `ls -la` showing size > 8,192 bytes

### Schema fidelity
2. **Integration Scope / External System Integrations section present** — grep "Integration Scope" or "External System" returned >= 1 hit — evidence: grep result
3. **Data Migration Plan section present** — grep "Data Migration" or "Migration Strategy" returned >= 1 hit — evidence: grep result
4. **API Management section present** — grep "API Management" or "API Standards" returned >= 1 hit — evidence: grep result
5. **Integration Risks and Mitigations table present** — grep "Integration Risks" or "Risk.*Severity.*Mitigation" returned >= 1 hit — evidence: grep result
6. No `[:N]` slicing applied to deliverable content strings — evidence: grep for `\[:[0-9]+\]` in production code paths returned 0 hits; confirm NO `integration_risks[:6]`, `desc[:220]`, `mit_text[:200]` truncations per 2026-05-19 fix

### Cross-stage consistency
7. **All integration requirements addressed** — print `len(integration_reqs)` and confirm the document covers all identified integration requirements (spot-check 3 requirement IDs appear in document)
8. **Mitigation cells in risk table are populated** from both `mitigation_strategies` array AND `mitigation_strategy` singular — evidence: count empty Mitigation cells in risk table (must be 0)

### Anti-regression rules (universal)
9. **UTF-8 encoding** on every `open()` call — evidence: search this phase's emitted scripts/code for `encoding='utf-8'` in every file-open
10. **ensure_ascii=False** on every `json.dump` call — evidence: same grep
11. **No `_Showing N of M_` row-cap notices** in any deliverable markdown — evidence: grep returned 0 matches
12. **No empty `|  |` mitigation/cell patterns** in any deliverable table — evidence: grep returned 0 matches
13. **No mid-word table-cell truncations** — evidence: line-by-line cell-end check returned 0 hits

### Memory discipline
14. **Relevant SAFS memory entries reviewed and applied** — evidence: list which memory files were read and which rules were applicable (e.g., "NEVER cap integration_risks[:6] or truncate desc[:220] — rendered ALL risks FULL per 2026-05-19 fix")
