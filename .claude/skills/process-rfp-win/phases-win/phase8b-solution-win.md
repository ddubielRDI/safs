---
name: phase8b-solution-win
expert-role: Solutions Architect
domain-expertise: Technical narrative, solution description
---

# Phase 8b: Solution Description

## Expert Role

You are a **Solutions Architect** with deep expertise in:
- Technical narrative writing
- Solution architecture communication
- Technology stack justification
- Integration approach description

## Purpose

Generate the solution description section for the bid.

## Inputs

- `{folder}/outputs/ARCHITECTURE.md`
- `{folder}/outputs/SECURITY_REQUIREMENTS.md`
- `{folder}/outputs/INTEROPERABILITY.md`
- `{folder}/shared/bid/POSITIONING_OUTPUT.json`

## Required Outputs

- `{folder}/outputs/solution.md`

## Instructions

### Step 1: Load Architecture Data

```python
architecture_content = read_file(f"{folder}/outputs/ARCHITECTURE.md")
security_content = read_file(f"{folder}/outputs/SECURITY_REQUIREMENTS.md")
interop_content = read_file(f"{folder}/outputs/INTEROPERABILITY.md")
positioning = read_json(f"{folder}/shared/bid/POSITIONING_OUTPUT.json")
```

### Step 2: Generate Solution Overview

```python
def generate_solution_section(architecture, security, interop, positioning):
    """Generate solution description section."""

    solution_md = f"""
## Technical Solution

### Solution Overview

Our proposed solution delivers a modern, scalable, and secure platform designed to meet all identified requirements while providing a foundation for future growth.

{positioning.get("core_positioning", {}).get("value_proposition", "")}

### Architecture Approach

#### High-Level Architecture

Our solution employs a modern N-tier architecture with clear separation of concerns:

![Architecture Diagram](architecture.png)

**Key Architecture Principles:**

1. **Modularity** - Independent, loosely-coupled components for flexibility
2. **Scalability** - Horizontal scaling to handle growing demands
3. **Security** - Defense-in-depth with zero-trust principles
4. **Maintainability** - Clean code practices and comprehensive documentation

#### Technology Stack

| Layer | Technology | Rationale |
|-------|------------|-----------|
| Frontend | React, TypeScript | Modern, component-based UI |
| API | ASP.NET Core 8 | High-performance, cross-platform |
| Database | SQL Server | Enterprise-grade reliability |
| Cache | Redis | High-performance caching |
| Infrastructure | Azure | Scalable cloud platform |

### Security Architecture

Security is fundamental to our solution design:

**Authentication & Authorization:**
- OAuth 2.0 / OpenID Connect for authentication
- Role-Based Access Control (RBAC)
- Multi-Factor Authentication (MFA) for sensitive operations

**Data Protection:**
- TLS 1.3 for all data in transit
- AES-256 encryption for data at rest
- Field-level encryption for sensitive data

**Compliance:**
- Full compliance with applicable regulations
- Comprehensive audit logging
- Regular security assessments

### Integration Approach

Our solution integrates seamlessly with existing systems:

**Integration Methods:**
- RESTful APIs for real-time integration
- Batch processing for bulk data exchange
- Event-driven architecture for notifications

**Key Integrations:**
| System | Method | Frequency |
|--------|--------|-----------|
| Core Systems | REST API | Real-time |
| Reporting | Batch | Scheduled |
| External Services | API Gateway | On-demand |

### Scalability & Performance

**Performance Targets:**
| Metric | Target |
|--------|--------|
| API Response Time | < 200ms (p95) |
| Page Load Time | < 2 seconds |
| Concurrent Users | 10,000+ |
| System Availability | 99.9% |

**Scaling Strategy:**
- Auto-scaling based on demand
- Read replicas for reporting
- CDN for static content
- Connection pooling

### User Experience

Our solution prioritizes user experience:

- **Intuitive Interface** - Modern, clean design
- **Responsive Design** - Works on all devices
- **Accessibility** - WCAG 2.1 AA compliant
- **Customization** - Configurable workflows and preferences

### Deployment Architecture

**Environment Strategy:**
- Development → Staging → Production
- Blue-green deployments for zero downtime
- Infrastructure as Code (Terraform)
- Automated CI/CD pipeline

### Support & Maintenance

**Ongoing Support Includes:**
- 24/7 system monitoring
- Regular security updates
- Performance optimization
- Feature enhancements

---

"""

    return solution_md

solution_md = generate_solution_section(
    architecture_content,
    security_content,
    interop_content,
    positioning
)
```

### Step 3: Add Mermaid Architecture Diagram Source

```python
def generate_architecture_mermaid():
    """Generate Mermaid source for architecture diagram."""
    mermaid = """```mermaid
flowchart TB
    subgraph Presentation["Presentation Layer"]
        WebApp["Web Application"]
        MobileApp["Mobile App"]
        AdminPortal["Admin Portal"]
    end

    subgraph API["API Gateway"]
        Gateway["API Gateway\\nAuth, Rate Limiting"]
    end

    subgraph Services["Business Services"]
        CoreService["Core Service"]
        ReportService["Reporting Service"]
        NotifyService["Notification Service"]
    end

    subgraph Data["Data Layer"]
        Database[(SQL Server)]
        Cache[(Redis Cache)]
        FileStore[(Blob Storage)]
    end

    subgraph External["External Systems"]
        ExtAPI["External APIs"]
        StateSystem["State Systems"]
    end

    Presentation --> API
    API --> Services
    Services --> Data
    Services --> External

    style Presentation fill:#003366,color:#fff
    style API fill:#4a90a4,color:#fff
    style Services fill:#2e7d32,color:#fff
    style Data fill:#e65100,color:#fff
    style External fill:#607d8b,color:#fff
```
"""
    return mermaid

# Write Mermaid source file
mermaid_source = generate_architecture_mermaid()
write_file(f"{folder}/outputs/bid/architecture.mmd", mermaid_source.replace("```mermaid", "").replace("```", ""))
```

### Step 4: Write Output

```python
write_file(f"{folder}/outputs/solution.md", solution_md)
```

### Step 5: Report Results

```python
log(f"""
🏗️ Solution Description Generated
=================================
Sections included:
  • Solution Overview
  • Architecture Approach
  • Technology Stack
  • Security Architecture
  • Integration Approach
  • Scalability & Performance
  • User Experience
  • Deployment Architecture
  • Support & Maintenance

Outputs:
  ✅ {folder}/outputs/solution.md
  ✅ {folder}/outputs/bid/architecture.mmd
""")
```

## Quality Checklist

- [ ] `solution.md` created in `outputs/` (NOT in bid/)
- [ ] `architecture.mmd` created in `outputs/bid/` for diagram rendering
- [ ] Architecture overview included
- [ ] Technology stack justified
- [ ] Security approach documented
- [ ] Integration strategy explained

**⚠️ REMINDER: ALL MD files go in `outputs/`, NOT `outputs/bid/`**
