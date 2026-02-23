---
name: phase8.6-integration-win
expert-role: Integration Architect
domain-expertise: Multi-vendor coordination, system integration planning, API architecture, data migration
---

# Phase 8.6: Technical Integration Plan

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

## Quality Checklist

- [ ] `06_INTEGRATION.md` created (>8KB)
- [ ] All integration requirements from requirements-normalized.json addressed
- [ ] Architecture and interoperability specs synthesized
- [ ] External system integration details per system
- [ ] Data migration plan with mapping
- [ ] API management standards defined
- [ ] Multi-vendor coordination (if applicable)
- [ ] Integration testing strategy
- [ ] Integration-specific risks with mitigations
