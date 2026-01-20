# SASQUATCH RFP Processing - Navigation Guide

**Document Set Navigation for Proposal Development Team**

---

## Quick Reference

| If you need to... | Go to... |
|-------------------|----------|
| Get an overview of the project | [EXECUTIVE_SUMMARY.md](EXECUTIVE_SUMMARY.md) |
| Find a specific requirement | [REQUIREMENTS_CATALOG.md](REQUIREMENTS_CATALOG.md) |
| Understand system architecture | [MODULAR_ARCHITECTURE.md](MODULAR_ARCHITECTURE.md) |
| Plan integrations | [INTEROPERABILITY.md](INTEROPERABILITY.md) |
| Review security requirements | [SECURITY_REQUIREMENTS.md](SECURITY_REQUIREMENTS.md) |
| Prepare demo scripts | [DEMO_SCENARIOS.md](DEMO_SCENARIOS.md) |
| Map requirements to components | [TRACEABILITY.md](TRACEABILITY.md) |
| Estimate effort/cost | [EFFORT_ESTIMATION.md](EFFORT_ESTIMATION.md) |
| See all generated files | [MANIFEST.md](MANIFEST.md) |

---

## By Audience

### For Executives / Business Development
1. **Start here:** [EXECUTIVE_SUMMARY.md](EXECUTIVE_SUMMARY.md)
2. **Budget/timeline:** [EFFORT_ESTIMATION.md](EFFORT_ESTIMATION.md) - Section 8 (Cost Breakdown)
3. **Risk factors:** [EFFORT_ESTIMATION.md](EFFORT_ESTIMATION.md) - Section 6

### For Solution Architects
1. **Start here:** [MODULAR_ARCHITECTURE.md](MODULAR_ARCHITECTURE.md)
2. **Integrations:** [INTEROPERABILITY.md](INTEROPERABILITY.md)
3. **Security:** [SECURITY_REQUIREMENTS.md](SECURITY_REQUIREMENTS.md)
4. **Requirements mapping:** [TRACEABILITY.md](TRACEABILITY.md)

### For Technical Leads
1. **Full requirements:** [REQUIREMENTS_CATALOG.md](REQUIREMENTS_CATALOG.md)
2. **Component mapping:** [TRACEABILITY.md](TRACEABILITY.md) - Section 4
3. **API specs:** [INTEROPERABILITY.md](INTEROPERABILITY.md) - Section 3
4. **Demo preparation:** [DEMO_SCENARIOS.md](DEMO_SCENARIOS.md)

### For Project Managers
1. **Scope:** [EXECUTIVE_SUMMARY.md](EXECUTIVE_SUMMARY.md) - Scope Summary
2. **Work breakdown:** [EFFORT_ESTIMATION.md](EFFORT_ESTIMATION.md) - Section 3
3. **Timeline:** [EFFORT_ESTIMATION.md](EFFORT_ESTIMATION.md) - Section 5
4. **Resources:** [EFFORT_ESTIMATION.md](EFFORT_ESTIMATION.md) - Section 4

### For Demo Team
1. **Start here:** [DEMO_SCENARIOS.md](DEMO_SCENARIOS.md)
2. **Test data:** Section 3 (Test Data Specifications) - Tumwater SD (34033)
3. **Scoring criteria:** Section 8 (Evaluation Rubric)

---

## Document Relationships

```
                     EXECUTIVE_SUMMARY.md
                            │
                            ▼
    ┌───────────────────────┼───────────────────────┐
    │                       │                       │
    ▼                       ▼                       ▼
REQUIREMENTS           MODULAR                EFFORT
CATALOG.md          ARCHITECTURE.md        ESTIMATION.md
    │                       │                       │
    │                       ▼                       │
    │              ┌────────┴────────┐              │
    │              ▼                 ▼              │
    │      INTEROPERABILITY   SECURITY             │
    │           .md           REQUIREMENTS.md      │
    │              │                 │              │
    └──────────────┼─────────────────┼──────────────┘
                   ▼                 ▼
              DEMO_SCENARIOS.md  TRACEABILITY.md
```

---

## Key Reference Points

### RFP Critical Dates
- **Proposal Due:** February 13, 2026 at 3:00 PM PT
- **Contract Start:** July 1, 2026
- **Implementation End:** June 30, 2028

### Budget
- **Total:** $9,000,000 (including 3 years M&O)
- **Development:** ~$5.4M
- **Post-Implementation:** ~$1.8M

### Demo Requirements
- **Duration:** 2 hours per work section
- **Q&A:** 30-45 minutes
- **Points:** Up to 300 per section (900 total)
- **Test Data:** Tumwater School District (34033), 2024-25

### Work Sections
| Section | Requirements | % of Work |
|---------|--------------|-----------|
| Data Collection | 90 | 43% |
| Data Calculations | 50 | 24% |
| Data Reporting | 50 | 19% |
| Technical/All | 50 | 14% |

---

## Shared Resources

The `/shared/` folder contains machine-readable data:

| File | Purpose | Format |
|------|---------|--------|
| `domain-detection.json` | Domain classification | JSON |
| `requirements.json` | Raw extracted requirements (568) | JSON |
| `requirements-normalized.json` | Normalized requirements (243) | JSON |
| `sample-data-mappings.json` | Demo data references | JSON |
| `progress.json` | Processing pipeline status | JSON |

---

## Document Conventions

### Priority Indicators
- 🔴 High Priority
- 🟡 Medium Priority
- 🟢 Low Priority

### Complexity Indicators
- 📊 High Complexity
- 📈 Medium Complexity
- 📉 Low Complexity

### Requirement ID Format
- `###APP` - Apportionment requirements
- `###BUD` - Budget requirements
- `###ENR` - Enrollment requirements
- `###TEC` - Technical requirements
- `###SAFS` - Core system requirements
- `###COL` - Data Collection
- `###CAL` - Data Calculations
- `###RPT` - Data Reporting

---

## Changelog

| Date | Version | Changes |
|------|---------|---------|
| 2026-01-19 | 1.0 | Initial document set generated |

---

*This navigation guide was generated as part of the RFP processing pipeline.*
