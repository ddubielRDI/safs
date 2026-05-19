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
                    # HUNT-B-010 fix 2026-05-18: removed [:200] truncation. Storage-side
                    # truncation was redundant and corrupted text for any other consumer.
                    "description": req.get("text", "")
                })
                assigned = True
                break

        if not assigned:
            ARCHITECTURE_LAYERS["business"]["components"].append({
                "req_id": req.get("canonical_id"),
                "description": req.get("text", "")
            })

    return ARCHITECTURE_LAYERS

layers = classify_by_layer(arch_requirements)
```

### Step 4: Load Tech-Stack Evidence (CONSUMER ONLY — split out 2026-05-18)

<!--
  REFACTOR 2026-05-18: The evidence-backed LTS lookup that previously lived
  here has moved to `phase3a-tech-stack-win.md` (a separate Stage 3 phase
  that runs BEFORE phase3a-architecture-win). Rationale:

  - Single-responsibility: one phase produces tech-lifecycle-evidence.json,
    one phase consumes it. No risk of partial ARCHITECTURE.md output
    coexisting with a half-written evidence file.
  - SVA-3 already gates on the evidence file via SVA3-TECH-STACK-LTS-VERIFIED;
    a dedicated producer phase makes SVA retry targeting unambiguous.
  - Phase 3a no longer issues WebFetch/WebSearch calls — it reads the
    pre-verified evidence and renders ADRs deterministically.

  This phase HALTS if `shared/tech-lifecycle-evidence.json` is missing.
  Producer: phases-win/phase3a-tech-stack-win.md
  Validator: phases-win/sva3-spec-validator-win.md rule SVA3-TECH-STACK-LTS-VERIFIED
-->

**⛔ CRITICAL — NO HARDCODED VERSIONS. NO TRAINING-DATA GUESSES.** This file must NOT issue WebFetch/WebSearch lookups for versions. All version evidence is loaded from `shared/tech-lifecycle-evidence.json` written by `phase3a-tech-stack-win.md`. If the file is missing, HALT — do not guess.

**The contract lifecycle rule (still non-negotiable, enforced by the producer phase):**

> Every proposed component's vendor-supported end-of-life MUST be ≥ `(go_live_date + contract_years + 2)`. For a 5-year contract starting 2026, that is 2033 at minimum. Components that fall short are REJECTED upstream — they never appear in the evidence file.

**The procedure (consumer-only):**

```python
import os as _os
from datetime import datetime

evidence_path = f"{folder}/shared/tech-lifecycle-evidence.json"
if not _os.path.exists(evidence_path):
    raise PhaseHalt(
        "PHASE 3A HALT — shared/tech-lifecycle-evidence.json is missing. "
        "Run phase3a-tech-stack-win.md FIRST to produce the evidence file. "
        "This phase no longer issues version lookups itself."
    )

tech_lifecycle = read_json(evidence_path)
tech_stack_evidence = tech_lifecycle.get("components", [])
contract_years = tech_lifecycle.get("contract_years", domain_context.get("contract_years", 5))

# Defensive: any component flagged as failing the contract+2yr lifecycle should
# already have been blocked upstream by phase3a-tech-stack. If we find one here,
# HALT loudly — the producer phase was bypassed or its gate was disabled.
failed = [c for c in tech_stack_evidence if not c.get("passes_contract_lifecycle")]
if failed:
    for c in failed:
        log(f"  ⛔ FAIL — {c['component']} {c.get('recommended_version')} EOL {c.get('eol_date')} < required")
    raise PhaseHalt(
        f"PHASE 3A HALT — evidence file contains {len(failed)} component(s) failing the "
        f"contract+2yr lifecycle gate. Rerun phase3a-tech-stack-win.md to refresh."
    )

# Build the legacy `tech_stack` dict that downstream rendering uses. Every
# label is `"{component} {recommended_version} (LTS, EOL {eol_date})"` — the
# version is BAKED FROM EVIDENCE, never from training data.
tech_stack = {"frontend": [], "backend": [], "database": [], "infrastructure": [], "integration": []}
for c in tech_stack_evidence:
    label = f"{c['component']} {c['recommended_version']} ({c['classification']}, EOL {c['eol_date']})"
    tech_stack.setdefault(c["category"], []).append(label)
```

**Why this split is structural, not cosmetic:**

- `phase3a-tech-stack-win.md` is the SINGLE producer of `tech-lifecycle-evidence.json`. SVA-3 retry targeting is unambiguous — `instruction: "Re-run phase3a-tech-stack-win.md"`.
- Phase 3a (this file) is now deterministic: same evidence → same ARCHITECTURE.md, no network variability.
- The per-domain examples that used to be here (`stack["backend"] = ["ASP.NET Core (latest LTS with 3+ years support)"]`) have been removed because they read as version-agnostic to a human but were rendered as `.NET 8` by agents whose training data was older than the file claimed. Stale example code is anchoring bias — it has no place in a procedure that must reflect current reality.

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
- Azure AD B2C / Microsoft Entra External ID / Okta — identity provider selected per RFP requirements and verified-current vendor LTS
- JWT tokens with 15-minute expiry
- Refresh token rotation

<!-- V1-F7 fix 2026-05-18: removed "Identity Server" — IS4 reached EOL July 2022;
     IS5/6/7 (Duende) requires commercial license. Replaced with managed IdPs
     that have active vendor support and clear licensing. -->


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

⛔ **Do NOT hardcode version numbers in this template.** Render each ADR by reading the verified evidence from `shared/tech-lifecycle-evidence.json` (written earlier in this phase). The pattern for every framework / runtime / database ADR is:

```
### ADR-{N}: Use {component} {recommended_version}
**Status:** Accepted
**Context:** {one-sentence problem framing — RFP requirement / constraint that drives this choice}
**Decision:** {component} {recommended_version} ({classification}, EOL {eol_date})
**Alternatives considered:**
  - {prior_version} — rejected because EOL {prior_eol_date} < contract+2yr ({min_required_eol})
  - {alternative_component} — rejected because {reason from domain or RFP}
**Evidence:** Sourced from {source_url}, fetched {fetched_at}
**Consequences:** {forward-looking impact — migration plan if classification != "LTS" for a long contract}
```

The agent rendering this section MUST loop over the entries in `tech-lifecycle-evidence.json` and emit one ADR per primary component (runtime, primary database, frontend framework, identity provider, message bus). Do not invent ADRs for components that are not in the evidence file — and do not skip components that are.

**Worked example (the kind of ADR this template produces — values illustrative only, real values come from the live evidence lookup):**

```
### ADR-005: Use .NET {LTS_VERSION_FROM_EVIDENCE}
**Status:** Accepted
**Context:** Multi-year hosted SaaS contract requires a runtime whose vendor support outlasts contract + 2 years of post-deployment maintenance.
**Decision:** .NET {LTS_VERSION_FROM_EVIDENCE} ({classification}, EOL {eol_date})
**Alternatives considered:**
  - .NET 8 LTS — rejected: EOL Nov 2026 falls inside contract term
  - .NET 9 — rejected: STS (Standard Term Support), ~18-month support window, EOL May 2026
**Evidence:** Sourced from https://learn.microsoft.com/en-us/dotnet/core/releases-and-support/, fetched {timestamp}
**Consequences:** First in-contract upgrade to the next LTS planned for year 3 of the contract to preserve LTS coverage through end of term.
```

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
