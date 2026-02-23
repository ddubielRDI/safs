---
name: phase8c-timeline-win
expert-role: Project Manager
domain-expertise: Schedules, milestones, Gantt charts
---

# Phase 8c: Timeline + Pricing

## Expert Role

You are a **Project Manager** with deep expertise in:
- Project scheduling
- Milestone planning
- Gantt chart creation
- Resource allocation

## Purpose

Generate timeline section with Gantt chart for the bid.

## Inputs

- `{folder}/outputs/EFFORT_ESTIMATION.md`
- `{folder}/shared/requirements-normalized.json`
- `{folder}/shared/domain-context.json`

## Required Outputs

- `{folder}/outputs/timeline.md`
- `{folder}/outputs/bid/timeline.mmd` (Mermaid Gantt source)

## Instructions

### Step 1: Load Estimation Data

```python
estimation_content = read_file(f"{folder}/outputs/EFFORT_ESTIMATION.md")
requirements = read_json(f"{folder}/shared/requirements-normalized.json")
domain_context = read_json(f"{folder}/shared/domain-context.json")

req_count = len(requirements.get("requirements", []))
```

### Step 2: Define Project Phases

```python
PROJECT_PHASES = [
    {
        "name": "Discovery & Planning",
        "duration_weeks": 2,
        "description": "Requirements validation, stakeholder alignment, project kickoff",
        "deliverables": ["Project Plan", "Requirements Baseline", "Communication Plan"],
        "color": "#4a90a4"
    },
    {
        "name": "Design & Architecture",
        "duration_weeks": 3,
        "description": "Technical design, architecture finalization, prototype development",
        "deliverables": ["Technical Design Document", "Architecture Diagrams", "Prototype"],
        "color": "#0097a7"
    },
    {
        "name": "Development Sprint 1-3",
        "duration_weeks": 6,
        "description": "Core functionality implementation, database setup, API development",
        "deliverables": ["Core Modules", "Database Schema", "API Endpoints"],
        "color": "#003366"
    },
    {
        "name": "Development Sprint 4-6",
        "duration_weeks": 6,
        "description": "Advanced features, integrations, reporting capabilities",
        "deliverables": ["Integration Layer", "Reports", "Admin Functions"],
        "color": "#003366"
    },
    {
        "name": "Testing & QA",
        "duration_weeks": 4,
        "description": "System testing, integration testing, UAT support",
        "deliverables": ["Test Results", "Bug Fixes", "UAT Sign-off"],
        "color": "#7b1fa2"
    },
    {
        "name": "Deployment & Go-Live",
        "duration_weeks": 2,
        "description": "Production deployment, data migration, training",
        "deliverables": ["Production System", "Training Materials", "Go-Live Support"],
        "color": "#2e7d32"
    }
]

total_weeks = sum(p["duration_weeks"] for p in PROJECT_PHASES)
```

### Step 3: Generate Timeline Section

```python
def generate_timeline_section(phases, req_count):
    """Generate timeline section content."""

    timeline_md = f"""
## Project Timeline

### Overview

Our proposed timeline delivers a complete, production-ready solution within **{total_weeks} weeks** ({total_weeks // 4} months).

**Key Milestones:**

| Milestone | Target Week | Deliverable |
|-----------|-------------|-------------|
| Project Kickoff | Week 1 | Kickoff meeting, project plan |
| Design Complete | Week 5 | Technical design approved |
| Core Development | Week 11 | Core functionality complete |
| Feature Complete | Week 17 | All features implemented |
| Testing Complete | Week 21 | UAT sign-off |
| Go-Live | Week 23 | Production deployment |

### Gantt Chart

![Project Timeline](timeline.png)

### Phase Details

"""

    start_week = 1
    for phase in phases:
        end_week = start_week + phase["duration_weeks"] - 1
        timeline_md += f"""
#### {phase["name"]}
**Weeks {start_week}-{end_week}** ({phase["duration_weeks"]} weeks)

{phase["description"]}

**Deliverables:**
"""
        for deliverable in phase["deliverables"]:
            timeline_md += f"- {deliverable}\n"

        timeline_md += "\n"
        start_week = end_week + 1

    # Add resource section
    timeline_md += f"""
### Team Structure

| Role | Count | Allocation |
|------|-------|------------|
| Project Manager | 1 | 50% |
| Tech Lead | 1 | 100% |
| Senior Developer | 2 | 100% |
| Developer | 2 | 100% |
| QA Engineer | 1 | 75% |
| Business Analyst | 1 | 50% |

### Investment Summary

**Development Investment:**

| Category | Estimated Effort |
|----------|------------------|
| Total Requirements | {req_count} |
| Total Development | See EFFORT_ESTIMATION.md |
| AI-Assisted Savings | ~35% |

*Detailed pricing provided separately as required.*

### Assumptions

1. Requirements finalized within 2 weeks of kickoff
2. Client resources available for UAT
3. Access to test environments provided
4. Timely feedback on deliverables
5. No major scope changes during development

### Risk Mitigation

| Risk | Impact | Mitigation |
|------|--------|------------|
| Scope Creep | Schedule delay | Change control process |
| Resource Availability | Quality impact | Backup resources identified |
| Integration Issues | Technical delay | Early integration testing |

---

"""

    return timeline_md

timeline_md = generate_timeline_section(PROJECT_PHASES, req_count)
```

### Step 4: Generate Mermaid Gantt Chart

```python
def generate_gantt_mermaid(phases):
    """Generate Mermaid Gantt chart source."""

    gantt = """gantt
    title Project Implementation Timeline
    dateFormat  YYYY-MM-DD
    excludes    weekends

    section Discovery
    Project Kickoff          :a1, 2024-07-01, 1w
    Requirements Validation  :a2, after a1, 1w

    section Design
    Technical Design         :b1, after a2, 2w
    Architecture Review      :b2, after b1, 1w

    section Development
    Sprint 1-2 (Core)        :c1, after b2, 4w
    Sprint 3-4 (Features)    :c2, after c1, 4w
    Sprint 5-6 (Integration) :c3, after c2, 4w

    section Testing
    System Testing           :d1, after c3, 2w
    UAT                      :d2, after d1, 2w

    section Deployment
    Production Prep          :e1, after d2, 1w
    Go-Live                  :milestone, e2, after e1, 1w

"""

    return gantt

gantt_mermaid = generate_gantt_mermaid(PROJECT_PHASES)
```

### Step 5: Generate Org Chart Mermaid

```python
def generate_orgchart_mermaid():
    """Generate Mermaid org chart source."""

    orgchart = """flowchart TB
    subgraph Client["Client Organization"]
        Sponsor["Executive Sponsor"]
        PM_Client["Project Manager"]
        SME["Subject Matter Experts"]
    end

    subgraph Vendor["[Company Name]"]
        direction TB
        Exec["Account Executive"]
        PD["Project Director"]

        subgraph Delivery["Delivery Team"]
            TL["Tech Lead"]
            Dev1["Senior Developer"]
            Dev2["Senior Developer"]
            Dev3["Developer"]
            QA["QA Lead"]
            BA["Business Analyst"]
        end
    end

    Sponsor --> Exec
    PM_Client --> PD
    SME --> BA
    PD --> TL
    TL --> Dev1
    TL --> Dev2
    TL --> Dev3
    TL --> QA

    style Sponsor fill:#003366,color:#fff
    style PM_Client fill:#003366,color:#fff
    style Exec fill:#2e7d32,color:#fff
    style PD fill:#2e7d32,color:#fff
    style TL fill:#4a90a4,color:#fff
"""

    return orgchart

orgchart_mermaid = generate_orgchart_mermaid()
```

### Step 6: Write Outputs

```python
# Write timeline section
write_file(f"{folder}/outputs/timeline.md", timeline_md)

# Write Mermaid sources
write_file(f"{folder}/outputs/bid/timeline.mmd", gantt_mermaid)
write_file(f"{folder}/outputs/bid/orgchart.mmd", orgchart_mermaid)
```

### Step 7: Report Results

```python
log(f"""
📅 Timeline & Pricing Generated
===============================
Total Duration: {total_weeks} weeks
Phases: {len(PROJECT_PHASES)}
Requirements: {req_count}

Outputs:
  ✅ {folder}/outputs/timeline.md
  ✅ {folder}/outputs/bid/timeline.mmd
  ✅ {folder}/outputs/bid/orgchart.mmd
""")
```

## Quality Checklist

- [ ] `timeline.md` created in `outputs/` (NOT in bid/)
- [ ] `timeline.mmd` created in `outputs/bid/` for Gantt rendering
- [ ] `orgchart.mmd` created in `outputs/bid/` for org chart rendering
- [ ] All phases documented
- [ ] Milestones defined
- [ ] Team structure included

**⚠️ REMINDER: ALL MD files go in `outputs/`, NOT `outputs/bid/`**
