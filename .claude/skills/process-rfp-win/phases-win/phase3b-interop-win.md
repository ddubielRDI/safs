---
name: phase3b-interop-win
expert-role: Integration Architect
domain-expertise: APIs, data exchange, EDI, HL7/FHIR
---

# Phase 3b: Interoperability Specifications

## Expert Role

You are an **Integration Architect** with deep expertise in:
- API design and implementation (REST, GraphQL, SOAP)
- Data exchange formats and protocols
- EDI, HL7/FHIR, and industry-specific standards
- Integration patterns and middleware

## Purpose

Generate interoperability specifications for external system integrations.

## Inputs

- `{folder}/shared/requirements-normalized.json` - Normalized requirements
- `{folder}/shared/domain-context.json` - Domain context

## Required Outputs

- `{folder}/outputs/INTEROPERABILITY.md` - Integration specifications (>5KB)

## Instructions

### Step 1: Extract Integration Requirements

```python
requirements = read_json(f"{folder}/shared/requirements-normalized.json")
domain_context = read_json(f"{folder}/shared/domain-context.json")

integration_keywords = [
    "integrate", "interface", "import", "export", "api",
    "external", "third-party", "connect", "data exchange",
    "synchronize", "sync", "transfer", "send", "receive"
]

integration_reqs = [
    req for req in requirements.get("requirements", [])
    if any(kw in req.get("text", "").lower() for kw in integration_keywords)
]
```

### Step 2: Identify External Systems

```python
DOMAIN_SYSTEMS = {
    "education": [
        {"name": "CEDARS/CEISDARS", "type": "State Reporting", "protocol": "SFTP/XML"},
        {"name": "SIS (Skyward/PowerSchool)", "type": "Student Information", "protocol": "REST API"},
        {"name": "OSPI Systems", "type": "State Agency", "protocol": "Web Services"},
        {"name": "ESD Systems", "type": "Regional Service", "protocol": "REST API"}
    ],
    "healthcare": [
        # V1-F5 fix 2026-05-18: protocols now specify sub-versions with source citation.
        # Defaults are conservative minimums — RFP may mandate newer; scan step below checks.
        {"name": "Epic/Cerner", "type": "EHR", "protocol": "HL7 FHIR R4", "_source": "HL7 FHIR R4 is the normative published version (R5 published 2023); verify R5 if RFP requires"},
        {"name": "Lab Systems", "type": "Laboratory", "protocol": "HL7 v2.5.1", "_source": "IHE minimum; verify v2.9 if RFP requires newer"},
        {"name": "Pharmacy", "type": "Prescription", "protocol": "NCPDP SCRIPT 2017071", "_source": "Federally required for ePrescribing per CMS; verify newer if RFP specifies"},
        {"name": "Insurance", "type": "Payer", "protocol": "X12 5010", "_source": "Federally required for HIPAA transactions per CMS"}
    ],
    "default": [
        {"name": "ERP System", "type": "Enterprise", "protocol": "REST API"},
        {"name": "CRM", "type": "Customer", "protocol": "REST API"},
        {"name": "Email Service", "type": "Notification", "protocol": "SMTP/API"}
    ]
}

domain = domain_context.get("selected_domain", "default")
external_systems = DOMAIN_SYSTEMS.get(domain, DOMAIN_SYSTEMS["default"])

# V1-F5 fix 2026-05-18: scan normalized requirements for explicit protocol
# versions BEFORE applying the conservative defaults above. If the RFP cites
# a specific version (e.g., "HL7 v2.9", "FHIR R5", "NCPDP SCRIPT 2023011",
# "X12 7030"), override the defaults so we don't propose a stale version.
import re as _re
all_req_text = " ".join(
    req.get("text", "") + " " + req.get("full_context", "")
    for req in requirements.get("requirements", [])
).lower()

PROTOCOL_VERSION_PATTERNS = {
    "HL7 v2": _re.compile(r"hl7\s*v?(\d+\.\d+(?:\.\d+)?)", _re.IGNORECASE),
    "HL7 FHIR": _re.compile(r"fhir\s*r?(\d+)", _re.IGNORECASE),
    "NCPDP SCRIPT": _re.compile(r"ncpdp\s*(?:script\s*)?(\d{6,7})", _re.IGNORECASE),
    "X12": _re.compile(r"x12\s*(\d{4})", _re.IGNORECASE),
}

detected_versions = {}
for label, pattern in PROTOCOL_VERSION_PATTERNS.items():
    matches = pattern.findall(all_req_text)
    if matches:
        detected_versions[label] = sorted(set(matches), reverse=True)[0]
        log(f"  Protocol version detected from RFP: {label} {detected_versions[label]}")

# Apply detected versions to external_systems entries
for system in external_systems:
    proto = system.get("protocol", "")
    for label, detected in detected_versions.items():
        if label.lower() in proto.lower():
            system["protocol_detected_from_rfp"] = f"{label} {detected}"
            system["protocol"] = system["protocol_detected_from_rfp"]  # override default
            system["_source"] = f"Detected from RFP requirements text — overrides default"
```

### Step 3: Generate Integration Specifications

```python
def generate_interop_md(integration_reqs, external_systems, domain):
    doc = f"""# Interoperability Specification

**Domain:** {domain}
**Generated:** {datetime.now().strftime('%Y-%m-%d %H:%M')}

---

## Executive Summary

This document defines the integration architecture and external system interfaces for the solution. The system will integrate with {len(external_systems)} external systems using modern API patterns and industry-standard protocols.

---

## External System Inventory

| System | Type | Protocol | Direction | Frequency |
|--------|------|----------|-----------|-----------|
"""

    for system in external_systems:
        doc += f"| {system['name']} | {system['type']} | {system['protocol']} | Bidirectional | Real-time/Batch |\n"

    doc += """

---

## Integration Patterns

### Synchronous Integration (REST APIs)
- Real-time data exchange
- Request/Response pattern
- JSON payloads
- OAuth 2.0 authentication

### Asynchronous Integration (Message Queue)
- Event-driven architecture
- Publish/Subscribe pattern
- At-least-once delivery
- Dead letter queue for failures

### Batch Integration (SFTP/Files)
- Scheduled file transfers
- CSV/XML file formats
- Checksum validation
- Archive and audit trail

---

## API Specifications

### Inbound APIs (Data Received)

"""

    # Generate API specs from requirements
    for i, req in enumerate(integration_reqs[:10]):
        if "import" in req.get("text", "").lower() or "receive" in req.get("text", "").lower():
            doc += f"""#### API-IN-{i+1:03d}: {req.get('canonical_id', 'N/A')}

**Purpose:** {req.get('text', '')[:200]}

**Endpoint:** `POST /api/v1/import/{{resource}}`

**Authentication:** OAuth 2.0 Bearer Token

**Request Format:**
```json
{{
  "source": "string",
  "timestamp": "ISO8601",
  "data": [...]
}}
```

**Response:** 202 Accepted (async processing)

---

"""

    doc += """### Outbound APIs (Data Sent)

"""

    for i, req in enumerate(integration_reqs[:10]):
        if "export" in req.get("text", "").lower() or "send" in req.get("text", "").lower():
            doc += f"""#### API-OUT-{i+1:03d}: {req.get('canonical_id', 'N/A')}

**Purpose:** {req.get('text', '')[:200]}

**Endpoint:** `GET /api/v1/export/{{resource}}`

**Authentication:** OAuth 2.0 Bearer Token

**Response Format:**
```json
{{
  "generated_at": "ISO8601",
  "count": 0,
  "data": [...]
}}
```

---

"""

    doc += generate_data_mapping_section(domain)
    doc += generate_error_handling_section()
    doc += generate_security_section()

    return doc

def generate_data_mapping_section(domain):
    return """
## Data Mapping

### Field Mapping Strategy
- Source-to-target field mapping documentation
- Data type transformations
- Default values for missing fields
- Validation rules at integration boundary

### Transformation Rules
| Source Format | Target Format | Transformation |
|---------------|---------------|----------------|
| Date (MM/DD/YYYY) | ISO8601 | Parse and reformat |
| Boolean (Y/N) | true/false | Map values |
| Currency | Decimal | Remove formatting |

"""

def generate_error_handling_section():
    return """
## Error Handling

### Retry Strategy
- Exponential backoff: 1s, 2s, 4s, 8s, 16s
- Maximum 5 retry attempts
- Circuit breaker after 50% failure rate

### Error Response Format
```json
{
  "error_code": "INT-001",
  "message": "External system unavailable",
  "timestamp": "2024-01-15T10:30:00Z",
  "retry_after": 60
}
```

### Monitoring
- Integration health dashboard
- Alert on failure thresholds
- Daily success/failure reports

"""

def generate_security_section():
    return """
## Security Considerations

### Authentication
- OAuth 2.0 for API access
- API keys for batch processes
- Certificate-based auth for SFTP

### Data Protection
- TLS 1.3 for all connections
- Encrypt sensitive fields
- Mask PII in logs

### Access Control
- IP whitelisting for external systems
- Rate limiting per client
- Audit logging for all integrations

---

## Appendices

### A. Sample Payloads
[Detailed request/response examples]

### B. Error Code Reference
[Complete error code documentation]

### C. Testing Guidelines
[Integration testing procedures]
"""

interop_md = generate_interop_md(integration_reqs, external_systems, domain)
```

### Step 4: Write Output

```python
write_file(f"{folder}/outputs/INTEROPERABILITY.md", interop_md)
```

## Quality Checklist (MANDATORY — report each by name with evidence)

The phase agent MUST verify each of the following BEFORE reporting completion. The agent's completion report MUST include a checklist-results block with:
- Item name (verbatim from below)
- PASS / FAIL / SKIPPED-WITH-REASON
- Evidence (file:line citation, grep result, file size, assertion that ran, etc.)

"All checks passed" without per-item evidence is NOT acceptable.

### Required output files
1. **INTEROPERABILITY.md** exists at `{folder}/outputs/INTEROPERABILITY.md` — evidence: `ls -la` showing size > 5,120 bytes (5 KB)

### Schema fidelity
2. **External Systems Inventory table present** — grep "External System Inventory" returned >= 1 hit — evidence: grep result
3. **API Specifications section present** — grep "API Specifications" or "Inbound APIs" returned >= 1 hit — evidence: grep result
4. **Error Handling section present** — grep "Error Handling" or "Retry Strategy" returned >= 1 hit — evidence: grep result
5. No `[:N]` slicing applied to deliverable content strings — evidence: grep for `\[:[0-9]+\]` in production code paths returned 0 hits

### Cross-stage consistency
6. **Protocol versions match RFP-detected versions** — any protocol version detected from RFP requirements (e.g., HL7 FHIR R4/R5, NCPDP SCRIPT version) appears in INTEROPERABILITY.md and matches `protocol_detected_from_rfp` override — evidence: confirm override was applied when detected_versions dict was non-empty
7. **At least 1 external system per domain type** — the domain-appropriate external systems list (education: CEDARS; healthcare: EHR; default: ERP) appears in the document — evidence: grep for domain-specific system names returned >= 1 hit

### Anti-regression rules (universal)
8. **UTF-8 encoding** on every `open()` call — evidence: search this phase's emitted scripts/code for `encoding='utf-8'` in every file-open
9. **ensure_ascii=False** on every `json.dump` call — evidence: same grep
10. **No `_Showing N of M_` row-cap notices** in any deliverable markdown — evidence: grep returned 0 matches
11. **No empty `|  |` mitigation/cell patterns** in any deliverable table — evidence: grep returned 0 matches
12. **No mid-word table-cell truncations** — evidence: line-by-line cell-end check returned 0 hits

### Memory discipline
13. **Relevant SAFS memory entries reviewed and applied** — evidence: list which memory files were read and which rules were applicable
