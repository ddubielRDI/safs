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
        {"name": "Epic/Cerner", "type": "EHR", "protocol": "HL7 FHIR"},
        {"name": "Lab Systems", "type": "Laboratory", "protocol": "HL7 v2"},
        {"name": "Pharmacy", "type": "Prescription", "protocol": "NCPDP"},
        {"name": "Insurance", "type": "Payer", "protocol": "X12 EDI"}
    ],
    "default": [
        {"name": "ERP System", "type": "Enterprise", "protocol": "REST API"},
        {"name": "CRM", "type": "Customer", "protocol": "REST API"},
        {"name": "Email Service", "type": "Notification", "protocol": "SMTP/API"}
    ]
}

domain = domain_context.get("selected_domain", "default")
external_systems = DOMAIN_SYSTEMS.get(domain, DOMAIN_SYSTEMS["default"])
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

## Quality Checklist

- [ ] `INTEROPERABILITY.md` created in `outputs/`
- [ ] File size > 5KB
- [ ] External systems documented
- [ ] API specifications included
- [ ] Error handling defined
- [ ] Security considerations addressed
