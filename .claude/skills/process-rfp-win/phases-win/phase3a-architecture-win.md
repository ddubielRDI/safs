---
name: phase3a-architecture-win
expert-role: Solutions Architect
domain-expertise: System design, cloud architecture, scalability, microservices
---

# Phase 3a: Architecture Specifications

## Expert Role

You are a **Solutions Architect** with deep expertise in:
- System design and cloud architecture
- Scalability patterns and microservices
- Technology selection and trade-offs
- Architecture Decision Records (ADRs)

## Purpose

Generate comprehensive architecture specifications from requirements.

## Inputs

- `{folder}/shared/requirements-normalized.json` - Normalized requirements
- `{folder}/shared/domain-context.json` - Domain context

## Required Outputs

- `{folder}/outputs/ARCHITECTURE.md` - Architecture specification (>15KB)

## Instructions

### Step 1: Load Requirements

```python
requirements = read_json(f"{folder}/shared/requirements-normalized.json")
domain_context = read_json(f"{folder}/shared/domain-context.json")

all_reqs = requirements.get("requirements", [])
domain = domain_context.get("selected_domain", "Generic")
```

### Step 2: Extract Architecture-Relevant Requirements

```python
def extract_arch_requirements(requirements):
    """Extract requirements relevant to architecture."""
    arch_keywords = [
        "system", "architecture", "component", "module", "layer",
        "api", "interface", "integration", "database", "storage",
        "scale", "performance", "availability", "reliability",
        "security", "authentication", "authorization",
        "cloud", "deploy", "infrastructure"
    ]

    arch_reqs = []
    for req in requirements:
        text_lower = req.get("text", "").lower()
        if any(kw in text_lower for kw in arch_keywords):
            arch_reqs.append(req)

    return arch_reqs

arch_requirements = extract_arch_requirements(all_reqs)
```

### Step 3: Determine Architecture Layers

```python
ARCHITECTURE_LAYERS = {
    "presentation": {
        "name": "Presentation Layer",
        "keywords": ["ui", "user interface", "screen", "dashboard", "portal", "web", "mobile"],
        "components": []
    },
    "api": {
        "name": "API Gateway Layer",
        "keywords": ["api", "rest", "graphql", "endpoint", "service"],
        "components": []
    },
    "business": {
        "name": "Business Logic Layer",
        "keywords": ["process", "workflow", "rule", "calculate", "validate"],
        "components": []
    },
    "data": {
        "name": "Data Access Layer",
        "keywords": ["database", "storage", "query", "repository", "entity"],
        "components": []
    },
    "integration": {
        "name": "Integration Layer",
        "keywords": ["integrate", "external", "third-party", "api", "import", "export"],
        "components": []
    },
    "infrastructure": {
        "name": "Infrastructure Layer",
        "keywords": ["server", "cloud", "container", "deploy", "monitor"],
        "components": []
    }
}

def classify_by_layer(requirements):
    """Classify requirements by architecture layer."""
    for req in requirements:
        text_lower = req.get("text", "").lower()
        assigned = False

        for layer_id, layer in ARCHITECTURE_LAYERS.items():
            if any(kw in text_lower for kw in layer["keywords"]):
                layer["components"].append({
                    "req_id": req.get("canonical_id"),
                    "description": req.get("text", "")[:200]
                })
                assigned = True
                break

        if not assigned:
            ARCHITECTURE_LAYERS["business"]["components"].append({
                "req_id": req.get("canonical_id"),
                "description": req.get("text", "")[:200]
            })

    return ARCHITECTURE_LAYERS

layers = classify_by_layer(arch_requirements)
```

### Step 4: Generate Technology Stack

```python
def recommend_tech_stack(domain, requirements):
    """Recommend technology stack based on domain and requirements.

    ⚠️ CRITICAL: ALL proposed technologies MUST have active vendor support
    (LTS or equivalent) extending AT MINIMUM through the full contract period
    PLUS 2 years of post-deployment maintenance.

    NEVER propose a technology version that reaches End-of-Life within 3 years.
    For .NET: Always propose the latest LTS version with 3+ years of remaining support.
    For Node.js/React: Always propose the current LTS version.
    For databases: Always propose the current GA version with active support.

    USE WEB SEARCH to verify current LTS versions and their EOL dates before recommending.
    """
    stack = {
        "frontend": [],
        "backend": [],
        "database": [],
        "infrastructure": [],
        "integration": []
    }

    # ⚠️ MANDATORY: Web search for current LTS versions before proposing anything.
    # Example: Search ".NET LTS versions end of life schedule {current_year}"
    # Example: Search "React LTS release schedule {current_year}"
    # NEVER hardcode a specific version number without verifying its EOL date.

    # Domain-specific recommendations (versions are EXAMPLES — verify via web search)
    if domain == "education":
        stack["frontend"] = ["React/Next.js (latest LTS)", "Tailwind CSS (latest stable)", "TypeScript (latest stable)"]
        stack["backend"] = ["ASP.NET Core (latest LTS with 3+ years support)", "C# (latest stable)", "Entity Framework Core (latest stable)"]
        stack["database"] = ["SQL Server / Azure SQL (current GA)", "Redis Cache (latest stable)"]
        stack["infrastructure"] = ["Azure App Service", "Azure SQL", "Azure AD B2C"]
        stack["integration"] = ["Azure Service Bus", "REST APIs", "SFTP"]
    elif domain == "healthcare":
        stack["frontend"] = ["React (latest LTS)", "Material-UI (latest stable)", "TypeScript (latest stable)"]
        stack["backend"] = ["ASP.NET Core (latest LTS with 3+ years support)", "C# (latest stable)", "EF Core (latest stable)"]
        stack["database"] = ["SQL Server / Azure SQL (current GA)", "Azure Cosmos DB"]
        stack["infrastructure"] = ["Azure (HIPAA compliant)", "Azure Key Vault"]
        stack["integration"] = ["HL7 FHIR", "Azure API Management"]
    else:
        # Generic stack
        stack["frontend"] = ["React (latest LTS)", "TypeScript (latest stable)", "Tailwind CSS (latest stable)"]
        stack["backend"] = ["ASP.NET Core (latest LTS with 3+ years support)", "C# (latest stable)"]
        stack["database"] = ["PostgreSQL (latest GA)", "Redis (latest stable)"]
        stack["infrastructure"] = ["Azure/AWS", "Docker", "Kubernetes"]
        stack["integration"] = ["REST APIs", "Message Queue"]

    return stack

tech_stack = recommend_tech_stack(domain, all_reqs)

# ⚠️ MANDATORY: Validate technology lifecycle BEFORE writing architecture document
def validate_tech_lifecycle(tech_stack, contract_years=5):
    """
    MANDATORY VALIDATION: Every proposed technology must have active
    LTS/vendor support extending through contract_period + 2 years.

    USE WEB SEARCH to verify:
    1. Current LTS version for each technology
    2. End-of-life date for each proposed version
    3. That EOL date is AFTER (today + contract_years + 2 years)

    If a technology fails validation:
    - Propose the next LTS version instead
    - If no suitable LTS exists, flag as RISK in the architecture document

    NEVER propose a technology that expires mid-contract. This is a
    disqualifying error in government proposals.
    """
    min_support_date = datetime.now().year + contract_years + 2

    for category, technologies in tech_stack.items():
        for tech in technologies:
            # Web search: "{tech} end of life schedule"
            # Verify: EOL date > min_support_date
            # If not: Replace with next LTS version
            pass  # AI must perform actual web search verification

    return tech_stack

tech_stack = validate_tech_lifecycle(tech_stack)
```

### Step 5: Generate Architecture Document

```python
def generate_architecture_md(layers, tech_stack, domain, requirements):
    """Generate architecture specification document."""
    doc = f"""# Architecture Specification

**Domain:** {domain}
**Generated:** {datetime.now().strftime('%Y-%m-%d %H:%M')}

---

## Executive Summary

This document defines the technical architecture for the {domain} solution based on {len(requirements)} extracted requirements. The architecture follows modern cloud-native principles with emphasis on scalability, security, and maintainability.

---

## Architecture Overview

### High-Level Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                        │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐          │
│  │   Web App   │  │ Mobile App  │  │   Portal    │          │
│  └─────────────┘  └─────────────┘  └─────────────┘          │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                    API GATEWAY LAYER                         │
│  ┌─────────────────────────────────────────────────────┐    │
│  │              API Gateway / Load Balancer             │    │
│  │         (Authentication, Rate Limiting, Routing)     │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                  BUSINESS LOGIC LAYER                        │
│  ┌───────────┐  ┌───────────┐  ┌───────────┐  ┌──────────┐  │
│  │  Service  │  │  Service  │  │  Service  │  │ Workflow │  │
│  │     A     │  │     B     │  │     C     │  │  Engine  │  │
│  └───────────┘  └───────────┘  └───────────┘  └──────────┘  │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                   DATA ACCESS LAYER                          │
│  ┌───────────────────┐  ┌───────────────────┐               │
│  │    Repository     │  │      Cache        │               │
│  │     Pattern       │  │     Layer         │               │
│  └───────────────────┘  └───────────────────┘               │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                   DATA STORAGE LAYER                         │
│  ┌───────────┐  ┌───────────┐  ┌───────────┐  ┌──────────┐  │
│  │  Primary  │  │   Cache   │  │   File    │  │  Search  │  │
│  │    DB     │  │  (Redis)  │  │  Storage  │  │  Index   │  │
│  └───────────┘  └───────────┘  └───────────┘  └──────────┘  │
└─────────────────────────────────────────────────────────────┘
```

---

## Technology Stack

### Frontend
{chr(10).join(f"- {tech}" for tech in tech_stack["frontend"])}

### Backend
{chr(10).join(f"- {tech}" for tech in tech_stack["backend"])}

### Database
{chr(10).join(f"- {tech}" for tech in tech_stack["database"])}

### Infrastructure
{chr(10).join(f"- {tech}" for tech in tech_stack["infrastructure"])}

### Integration
{chr(10).join(f"- {tech}" for tech in tech_stack["integration"])}

---

## Layer Specifications

"""

    # Add layer details
    for layer_id, layer in layers.items():
        if layer["components"]:
            doc += f"""### {layer["name"]}

**Component Count:** {len(layer["components"])}

| Requirement | Description |
|-------------|-------------|
"""
            for comp in layer["components"][:15]:
                doc += f"| {comp['req_id']} | {comp['description'][:80]}... |\n"

            doc += "\n"

    # Add more sections
    doc += generate_security_section(domain)
    doc += generate_scalability_section()
    doc += generate_deployment_section(tech_stack)
    doc += generate_adr_section()

    return doc

def generate_security_section(domain):
    """Generate security architecture section."""
    return f"""
---

## Security Architecture

### Authentication
- Azure AD B2C / Identity Server for user authentication
- JWT tokens with 15-minute expiry
- Refresh token rotation

### Authorization
- Role-Based Access Control (RBAC)
- Policy-based authorization for fine-grained access
- Resource-level permissions

### Data Protection
- TLS 1.3 for data in transit
- AES-256 encryption for data at rest
- Field-level encryption for PII

### Compliance
{"- FERPA compliance for student data" if domain == "education" else ""}
{"- HIPAA compliance for patient data" if domain == "healthcare" else ""}
- SOC 2 Type II audit trail
- Data retention policies

"""

def generate_scalability_section():
    """Generate scalability architecture section."""
    return """
---

## Scalability Architecture

### Horizontal Scaling
- Stateless application servers
- Auto-scaling based on CPU/memory metrics
- Load balancer with health checks

### Caching Strategy
- Redis distributed cache
- Response caching at API gateway
- Browser caching for static assets

### Database Scaling
- Read replicas for reporting queries
- Connection pooling
- Query optimization and indexing

### Performance Targets
| Metric | Target |
|--------|--------|
| API Response Time | < 200ms (p95) |
| Page Load Time | < 2s |
| Concurrent Users | 10,000+ |
| Availability | 99.9% |

"""

def generate_deployment_section(tech_stack):
    """Generate deployment architecture section."""
    return """
---

## Deployment Architecture

### Environment Strategy
- **Development**: Feature branches, ephemeral environments
- **Staging**: Production-like, performance testing
- **Production**: Blue-green deployment, zero-downtime

### CI/CD Pipeline
1. Code commit triggers build
2. Unit tests, integration tests
3. Security scanning (SAST/DAST)
4. Container image build
5. Deploy to staging
6. Automated smoke tests
7. Manual approval gate
8. Production deployment

### Infrastructure as Code
- Terraform for cloud resources
- Helm charts for Kubernetes
- GitOps with ArgoCD

"""

def generate_adr_section():
    """Generate Architecture Decision Records section."""
    return """
---

## Architecture Decision Records

### ADR-001: Use ASP.NET Core 8
**Status:** Accepted
**Context:** Need modern, performant backend framework
**Decision:** ASP.NET Core 8 with C#
**Consequences:** Strong typing, excellent performance, enterprise support

### ADR-002: SQL Server for Primary Database
**Status:** Accepted
**Context:** Need reliable RDBMS with strong ACID compliance
**Decision:** Azure SQL / SQL Server
**Consequences:** Familiar tooling, good Entity Framework support

### ADR-003: React for Frontend
**Status:** Accepted
**Context:** Need modern, component-based UI framework
**Decision:** React with TypeScript
**Consequences:** Large ecosystem, strong community, type safety

---

## Appendices

### A. Component Inventory
[See detailed component specifications]

### B. API Contracts
[See API documentation]

### C. Data Models
[See entity definitions]
"""

architecture_md = generate_architecture_md(layers, tech_stack, domain, arch_requirements)
```

### Step 6: Write Output

```python
write_file(f"{folder}/outputs/ARCHITECTURE.md", architecture_md)

# Verify size
import os
file_size = os.path.getsize(f"{folder}/outputs/ARCHITECTURE.md")
size_kb = file_size / 1024

if size_kb < 15:
    log(f"⚠️ Warning: ARCHITECTURE.md is only {size_kb:.1f}KB (target: >15KB)")
```

### Step 7: Report Results

```
🏗️ Architecture Specification Complete
=======================================
Output: ARCHITECTURE.md ({size_kb:.1f} KB)
Domain: {domain}
Architecture Requirements: {len(arch_requirements)}

Layer Distribution:
| Layer | Components |
|-------|------------|
{table rows}

Technology Stack Summary:
  Frontend: {tech_stack["frontend"]}
  Backend: {tech_stack["backend"]}
  Database: {tech_stack["database"]}
```

## Quality Checklist

- [ ] `ARCHITECTURE.md` created in `outputs/`
- [ ] File size > 15KB
- [ ] All architecture layers documented
- [ ] Technology stack justified
- [ ] Security section included
- [ ] Scalability considerations documented
- [ ] ADRs included
