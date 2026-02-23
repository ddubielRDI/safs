# /process-rfp-win - Visual Pipeline Guide (v2)

## Pipeline Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                    /process-rfp-win (Mayor Orchestrator v2)                     │
│                                                                                 │
│  Responsibilities: Progress tracking, Phase execution, SVA gate enforcement,    │
│                    Output verification, Auto-correction retry, Blocking gates,  │
│                    Metrics collection, Optional external integrations           │
│                                                                                 │
│  Total: 38 phases + 7 SVA gates = 45 execution units (full) / ~14 (sprint)    │
│  Optional: 2 post-pipeline phases (Phase 9, Post-Run Metrics)                  │
│  Color Team Reviews: Pink (~25%), Red (~60%), Gold (~95%)                       │
└─────────────────────────────────────────────────────────────────────────────────┘
                                        │
                                        ▼
┌─────────────────────────────────────────────────────────────────────────────────┐
│                         STAGE 1: DOCUMENT INTAKE                                │
│                         (9 phases + SVA-1)                                      │
├─────────────────────────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │ Phase 0: Folder Organization                                            │   │
│  │ Role: DevOps Engineer                                                   │   │
│  │ Task: Create directory structure (original/, flattened/, shared/,       │   │
│  │       shared/validation/, outputs/, outputs/bid-sections/)              │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
│                                        │                                        │
│                                        ▼                                        │
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │ Phase 1: Document Flattening [PARALLEL]                                 │   │
│  │ Role: Document Processing Specialist                                    │   │
│  │ Task: Convert PDF/DOCX/XLSX to markdown using markitdown               │   │
│  │ Output: flattened/*.md                                                  │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
│                                        │                                        │
│                                        ▼                                        │
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │ Phase 1.5: Domain Detection                                             │   │
│  │ Role: Business Analyst                                                  │   │
│  │ Task: Classify industry, identify compliance frameworks                 │   │
│  │ Output: shared/domain-context.json                                      │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
│                                        │                                        │
│                                        ▼                                        │
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │ Phase 1.6: Evaluation Criteria                                          │   │
│  │ Role: Procurement Specialist                                            │   │
│  │ Task: Extract scoring methodology, assign stable factor_id/subfactor_id │   │
│  │ Output: shared/EVALUATION_CRITERIA.json                                 │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
│                                        │                                        │
│                                        ▼                                        │
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │ Phase 1.7: Compliance Gatekeeper ⛔ [BLOCKING GATE]                     │   │
│  │ Role: Compliance Officer                                                │   │
│  │ Task: Verify all mandatory items identified, assign rfp_source_id       │   │
│  │ Output: shared/COMPLIANCE_MATRIX.json + partial RTM entities            │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
│                                        │                                        │
│                                        ▼                                        │
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │ Phase 1.8: Submission Structure Detection [NEW]                         │   │
│  │ Role: Procurement Analyst                                               │   │
│  │ Task: Parse RFP for submission packaging, volumes, naming conventions   │   │
│  │ Output: shared/SUBMISSION_STRUCTURE.json                                │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
│                                        │                                        │
│                                        ▼                                        │
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │ Phase 1.9: Go/No-Go Decision Gate [ADVISORY GATE]                      │   │
│  │ Role: Bid Decision Analyst                                             │   │
│  │ Task: Evaluate bid viability, competitive position, resource fit       │   │
│  │ Output: shared/GO_NOGO_DECISION.json                                   │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
│                                        │                                        │
│                                        ▼                                        │
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │ Phase 1.95: Client Intelligence [CONDITIONAL: GO only]                 │   │
│  │ Role: Competitive Intelligence Analyst                                 │   │
│  │ Task: Research client history, incumbent analysis, competitive landscape│  │
│  │ Output: shared/bid/CLIENT_INTELLIGENCE.json                            │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
├─────────────────────────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │ 🔍 SVA-1: Intake Validator                                              │   │
│  │ Rules: Flattening completeness, quality, domain confidence,             │   │
│  │        evaluation extraction depth, compliance coverage                 │   │
│  │ Report: shared/validation/sva1-intake.json                              │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────────┘
                                        │
                                        ▼
┌─────────────────────────────────────────────────────────────────────────────────┐
│                    STAGE 2: REQUIREMENTS ENGINEERING                             │
│                    (6 phases + SVA-2 PINK TEAM)                                 │
├─────────────────────────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │ Phase 2a: Workflow Extraction                                           │   │
│  │ Role: Business Process Analyst                                          │   │
│  │ Task: Map process flows, identify BPMN workflows                        │   │
│  │ Output: shared/workflow-extracted-reqs.json                             │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
│                                        │                                        │
│                                        ▼                                        │
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │ Phase 2: Requirements Extraction                                        │   │
│  │ Role: Requirements Engineer                                             │   │
│  │ Task: Extract all functional/non-functional requirements + source_ids[] │   │
│  │ Output: shared/requirements-raw.json                                    │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
│                                        │                                        │
│                                        ▼                                        │
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │ Phase 2.5: Sample Data Analysis                                         │   │
│  │ Role: Data Analyst                                                      │   │
│  │ Task: Profile sample data files, map data structures                    │   │
│  │ Output: shared/sample-data-analysis.json                                │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
│                                        │                                        │
│                                        ▼                                        │
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │ Phase 2b: Normalize Requirements                                        │   │
│  │ Role: Requirements Engineer                                             │   │
│  │ Task: Deduplicate, validate, assign IDs, categorize                     │   │
│  │ Output: shared/requirements-normalized.json                             │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
│                                        │                                        │
│                                        ▼                                        │
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │ Phase 2c: Requirements Catalog                                          │   │
│  │ Role: Technical Writer                                                  │   │
│  │ Task: Generate formatted catalog document (10KB+)                       │   │
│  │ Output: outputs/REQUIREMENTS_CATALOG.md, shared/REQUIREMENTS_CATALOG.json│  │
│  └─────────────────────────────────────────────────────────────────────────┘   │
│                                        │                                        │
│                                        ▼                                        │
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │ Phase 2d: Coverage Validation ⛔ [BLOCKING GATE]                        │   │
│  │ Role: QA Engineer                                                       │   │
│  │ Task: Verify 100% workflow coverage by requirements                     │   │
│  │ Output: shared/workflow-coverage.json                                   │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
├─────────────────────────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │ 🔍 SVA-2: PINK TEAM REVIEW (~25% completion)                            │   │
│  │ Rules: Requirement source coverage, dedup quality, priority distribution│   │
│  │        category coverage, compliance-to-requirement mapping,            │   │
│  │        strategy readiness                                               │   │
│  │ Report: shared/validation/sva2-pink-team.json                           │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────────┘
                                        │
                                        ▼
┌─────────────────────────────────────────────────────────────────────────────────┐
│                       STAGE 3: SPECIFICATION GENERATION                         │
│                       (6 phases + SVA-3)                                        │
├─────────────────────────────────────────────────────────────────────────────────┤
│  ┌──────────────────────┐  ┌──────────────────────┐  ┌──────────────────────┐  │
│  │ Phase 3a             │  │ Phase 3b             │  │ Phase 3c             │  │
│  │ Architecture Specs   │  │ Interoperability     │  │ Security Specs       │  │
│  │ Role: Solutions      │  │ Role: Integration    │  │ Role: Security       │  │
│  │ Architect            │  │ Architect            │  │ Architect            │  │
│  │ Output: (15KB+)      │  │ Output: (5KB+)       │  │ Output: (8KB+)       │  │
│  │ ARCHITECTURE.md      │  │ INTEROPERABILITY.md  │  │ SECURITY_REQS.md     │  │
│  └──────────────────────┘  └──────────────────────┘  └──────────────────────┘  │
│                              [PARALLEL GROUP]                                   │
│  ┌──────────────────────┐  ┌──────────────────────┐                            │
│  │ Phase 3e             │  │ Phase 3f             │                            │
│  │ UI/UX Specs          │  │ Entity Definitions   │                            │
│  │ Role: UX Designer    │  │ Role: Data Architect │                            │
│  │ Output:              │  │ Output:              │                            │
│  │ UI_SPECS.md          │  │ ENTITY_DEFINITIONS.md│                            │
│  └──────────────────────┘  └──────────────────────┘                            │
├─────────────────────────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │ Phase 3g: Risk Assessment (sequential after 3a-3f)                      │   │
│  │ Role: Risk Analyst                                                      │   │
│  │ Task: Identify risks, assign stable risk_id/mitigation_id              │   │
│  │ Output: REQUIREMENT_RISKS.md, shared/REQUIREMENT_RISKS.json             │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
├─────────────────────────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │ 🔍 SVA-3: Specification Validator                                       │   │
│  │ Rules: Spec-to-req coverage, internal consistency, domain alignment,    │   │
│  │        risk-spec coverage, entity completeness, spec depth ratio        │   │
│  │ Report: shared/validation/sva3-spec.json                                │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────────┘
                                        │
                                        ▼
┌─────────────────────────────────────────────────────────────────────────────────┐
│                    STAGE 4: TRACEABILITY & ESTIMATION                            │
│                    (2 phases + SVA-4 RED TEAM)                                  │
├─────────────────────────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │ Phase 4: Traceability Matrix + UNIFIED_RTM.json [MAJOR]                 │   │
│  │ Role: Requirements Traceability Engineer                                │   │
│  │ Task: Build UNIFIED_RTM.json linking all entities across full chain:    │   │
│  │       RFP Source → Mandatory Item → Requirement → Specification →       │   │
│  │       Risk → Mitigation → Bid Section → Evidence                        │   │
│  │ Features: Section-level spec linking, composite priority scoring,       │   │
│  │           evaluation weight inheritance, chain_links[] materialization  │   │
│  │ Output: outputs/TRACEABILITY.md (10KB+), shared/UNIFIED_RTM.json       │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
│                                        │                                        │
│                                        ▼                                        │
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │ Phase 5: Effort Estimation                                              │   │
│  │ Role: Project Estimator                                                 │   │
│  │ Task: Calculate effort, resource planning, AI ratios (8KB+)             │   │
│  │ Output: outputs/EFFORT_ESTIMATION.md, shared/effort-estimation.json     │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
├─────────────────────────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │ 🔍 SVA-4: RED TEAM REVIEW (~60% completion)                             │   │
│  │ Rules: Bidirectional traceability, traceability quality audit (20%      │   │
│  │        sample), evaluator simulation, compliance forward trace,         │   │
│  │        estimation consistency, estimation-risk alignment                │   │
│  │ Report: shared/validation/sva4-red-team.json                            │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────────┘
                                        │
                                        ▼
┌─────────────────────────────────────────────────────────────────────────────────┐
│                          STAGE 5: DOCUMENTATION                                 │
│                          (3 phases + SVA-5)                                     │
├─────────────────────────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │ Phase 6: Manifest Generation                                            │   │
│  │ Role: Technical Writer                                                  │   │
│  │ Task: Create audit trail, executive summary                             │   │
│  │ Output: outputs/MANIFEST.md, outputs/EXECUTIVE_SUMMARY.md               │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
│                                        │                                        │
│                                        ▼                                        │
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │ Phase 6b: Navigation Guide                                              │   │
│  │ Role: Technical Writer                                                  │   │
│  │ Task: Create user navigation aids (3KB+)                                │   │
│  │ Output: outputs/NAVIGATION_GUIDE.md                                     │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
│                                        │                                        │
│                                        ▼                                        │
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │ Phase 6c: Context Bundle [ENHANCED]                                     │   │
│  │ Role: Data Integration Architect                                        │   │
│  │ Task: Aggregate all sources, build content_priority_guide from RTM      │   │
│  │       composite scores, content ordering by evaluation weight.          │   │
│  │       Passes full bid_section_mapping, eval_factors_by_weight,          │   │
│  │       theme_eval_mapping, section_theme_mandates, section_content_guide,│   │
│  │       matched_evidence (from evidence-library.json via Phase 8.0)       │   │
│  │ Output: shared/bid-context-bundle.json                                  │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
├─────────────────────────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │ 🔍 SVA-5: Documentation Validator                                       │   │
│  │ Rules: Manifest accuracy, exec summary stats, nav guide link validity,  │   │
│  │        context bundle completeness (10+ sources)                        │   │
│  │ Report: shared/validation/sva5-doc.json                                 │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────────┘
                                        │
                                        ▼
┌─────────────────────────────────────────────────────────────────────────────────┐
│                        STAGE 6: QUALITY ASSURANCE                               │
│                        (3 phases + SVA-6 RED TEAM FINAL)                       │
├─────────────────────────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │ Phase 7: Quality Validation & Gap Analysis [COMBINED]                  │   │
│  │ Role: QA Engineer                                                      │   │
│  │ Task: Structural validation + benchmark comparison + gap analysis      │   │
│  │ Output: shared/validation-results.json, outputs/GAP_ANALYSIS.md        │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
├─────────────────────────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │ Phase 7c: Evaluator Personas                                            │   │
│  │ Role: UX Researcher                                                     │   │
│  │ Task: Define evaluator personas, stakeholder analysis                   │   │
│  │ Output: shared/PERSONA_COVERAGE.json                                    │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
│                                        │                                        │
│                                        ▼                                        │
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │ Phase 7d: Bid Scoring Model                                             │   │
│  │ Role: Bid Strategist                                                    │   │
│  │ Task: Calculate win probability, scoring model                          │   │
│  │ Output: shared/WIN_SCORECARD.json                                       │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
├─────────────────────────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │ 🔍 SVA-6: PRE-BID GATE (Red Team Final, ~80% completion)               │   │
│  │ Rules: Persona coverage >= 80%, win probability >= 60%, gap analysis    │   │
│  │        clear of HIGH gaps, risk mitigation completeness, context bundle │   │
│  │        integrity, evaluation alignment check                            │   │
│  │ Report: shared/validation/sva6-pre-bid.json                             │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────────┘
                                        │
                                        ▼
┌─────────────────────────────────────────────────────────────────────────────────┐
│                       STAGE 7: BID GENERATION                                   │
│                       (12 phases + SVA-7 GOLD TEAM)                            │
├─────────────────────────────────────────────────────────────────────────────────┤
│                                                                                 │
│  LAYER 1: Strategic Foundation                                                  │
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │ Phase 8.0: Strategic Positioning                                        │   │
│  │ Role: Bid Strategist                                                    │   │
│  │ Consumes: shared/bid/CLIENT_INTELLIGENCE.json (from Phase 1.95)        │   │
│  │ Matches evidence from evidence-library.json to RFP requirements        │   │
│  │ Generates: theme_eval_mapping, section_theme_mandates                  │   │
│  │ Output: shared/bid/POSITIONING_OUTPUT.json                              │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
│                                        │                                        │
│                                        ▼                                        │
│  LAYER 2: Core Bid Volumes (8 NEW phases)                                      │
│  ┌──────────────────────────────────────┐  ┌──────────────────────────────────┐│
│  │ Phase 8.1: Letter of Submittal      │  │ Phase 8.2: Management Proposal   ││
│  │ Role: Executive Comms Specialist    │  │ Role: Management Proposal Writer ││
│  │ NOSE formula, certifications        │  │ Experience, team, OCM, references││
│  │ Eval factor callout box, >= 3 win   │  │ Eval factor callout boxes/section││
│  │ themes, EXECUTIVE evaluator persona │  │ >= 2 themes/section (CVD format) ││
│  │ Output: 01_SUBMITTAL.md             │  │ Output: 02_MANAGEMENT.md         ││
│  └──────────────────────────────────────┘  └──────────────────────────────────┘│
│                                                                                 │
│  ┌──────────────────────────────────────┐  ┌──────────────────────────────────┐│
│  │ Phase 8.3: Technical Approach       │  │ Phase 8.4: Business Solution x3  ││
│  │ Role: Sr. Technical Proposal Writer │  │ Role: Business Solution Architect││
│  │ [OPUS] All Stage 3 specs synthesis  │  │ [OPUS] Per work section, ordered ││
│  │ Eval callout boxes, content priority│  │ by composite_priority_score      ││
│  │ guide, tech_mandated_themes (CVD)   │  │                                  ││
│  │ Output: 03_TECHNICAL.md (15KB+)     │  │                                  ││
│  │                                      │  │ Output: 04a_SOLUTION_*.md        ││
│  └──────────────────────────────────────┘  └──────────────────────────────────┘│
│                                                                                 │
│  ┌──────────────────────────────────────┐  ┌──────────────────────────────────┐│
│  │ Phase 8.4r: Requirements Review     │  │ Phase 8.4k: Risk Register        ││
│  │ Role: Requirements Analyst          │  │ Role: Risk Analyst               ││
│  │ Tabular: every req with status      │  │ Tabular: by category + severity  ││
│  │ Output: 04_REQUIREMENTS_REVIEW.md   │  │ Output: 04_RISK_REGISTER.md      ││
│  └──────────────────────────────────────┘  └──────────────────────────────────┘│
│                                                                                 │
│  ┌──────────────────────────────────────┐  ┌──────────────────────────────────┐│
│  │ Phase 8.5: Financial Proposal       │  │ Phase 8.6: Technical Integration ││
│  │ Role: Financial Analyst             │  │ Role: Integration Architect      ││
│  │ Rate tables + USER INPUT markers    │  │ Multi-vendor, APIs, migration    ││
│  │ Market rate fallback (GSA defaults) │  │                                  ││
│  │ Rate source attribution, TCO, cost  │  │                                  ││
│  │ eval alignment, payment schedule    │  │                                  ││
│  │ Output: 05_FINANCIAL.md             │  │ Output: 06_INTEGRATION.md        ││
│  └──────────────────────────────────────┘  └──────────────────────────────────┘│
│                                        │                                        │
│                                        ▼                                        │
│  LAYER 3: Verification                                                          │
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │ Phase 8f: RTM Verification [NEW]                                        │   │
│  │ Role: Traceability Verification Engineer                                │   │
│  │ Task: Run 14 verification queries against UNIFIED_RTM.json              │   │
│  │       5 forward + 4 backward + 5 chain integrity queries               │   │
│  │ Output: outputs/RTM_REPORT.md                                           │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
├─────────────────────────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │ 🔍 SVA-7: GOLD TEAM REVIEW (~95% completion) [13 rules]                 │   │
│  │ Rules: Win theme threading depth (grep, >= 50% coverage, eval linkage),│   │
│  │        risk-bid integration, compliance-bid coverage, cross-doc ID     │   │
│  │        integrity, persona satisfaction, format compliance, statistic   │   │
│  │        consistency, competitive contrast, case study validation,       │   │
│  │        tech lifecycle, financial sanity (Rule 11),                     │   │
│  │        Rule 13: SVA7-PROOF-POINT-DENSITY (evidence coverage/section)  │   │
│  │ Generates: GOLD_TEAM_CHECKLIST.md for human review sign-off           │   │
│  │ Report: shared/validation/sva7-gold-team.json                           │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
├─────────────────────────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │ AWAITING_HUMAN_REVIEW Gate                                              │   │
│  │ Pipeline pauses for human sign-off on GOLD_TEAM_CHECKLIST.md           │   │
│  │ Status: AWAITING_HUMAN_REVIEW → APPROVED (resume) or REJECTED (fix)   │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
├─────────────────────────────────────────────────────────────────────────────────┤
│  LAYER 4: Assembly (post-SVA-7, post-human-review)                             │
│  ┌──────────────────────────────────────┐  ┌──────────────────────────────────┐│
│  │ Phase 8d: Diagram Rendering         │  │ Phase 8e: Multi-File PDF Assembly││
│  │ Role: Visual Design Engineer        │  │ Role: Publication Specialist     ││
│  │ Mermaid → PNG rendering             │  │ Named PDFs per RFP convention    ││
│  │ Generates figure-registry.json with │  │ Uses SUBMISSION_STRUCTURE.json   ││
│  │ persuasive action captions for all  │  │ Output: {Bidder}_N_TITLE.pdf     ││
│  │ diagrams                            │  │                                  ││
│  │ Output: architecture.png,           │  │                                  ││
│  │ timeline.png, orgchart.png          │  │                                  ││
│  └──────────────────────────────────────┘  └──────────────────────────────────┘│
│                                                                                 │
├─────────────────────────────────────────────────────────────────────────────────┤
│  POST-PIPELINE (Optional)                                                       │
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │ Phase 9: Post-Bid Learning (Optional)                                  │   │
│  │ Role: Bid Performance Analyst                                          │   │
│  │ Task: Log bid outcome (win/loss), analyze historical patterns          │   │
│  │ Output: BID_OUTCOME_REPORT.md, updated bid-outcomes.json               │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
│                                                                                 │
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │ Metrics Aggregation (Automatic)                                        │   │
│  │ Task: Collect timing, failure rates, SVA dispositions per run           │   │
│  │ Output: config-win/pipeline-metrics.json (persistent across runs)       │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────────┘
                                        │
                                        ▼
                            ┌───────────────────────┐
                            │   FINAL VERIFICATION  │
                            │   All outputs exist   │
                            │   with minimum sizes  │
                            │   USER INPUT markers  │
                            │   reported            │
                            └───────────────────────┘
```

---

## Phase Reference Table

| Phase | Name | Expert Role | Key Output(s) | Min Size |
|-------|------|-------------|---------------|----------|
| **STAGE 1: DOCUMENT INTAKE** (9 phases + SVA-1) |
| 0 | Folder Organization (includes security validation) | DevOps Engineer | `original/`, `flattened/`, `shared/`, `shared/validation/`, `outputs/`, `outputs/bid-sections/` | - |
| 1 | Document Flattening | Document Processing Specialist | `flattened/*.md` | - |
| 1.5 | Domain Detection | Business Analyst | `domain-context.json` | - |
| 1.6 | Evaluation Criteria | Procurement Specialist | `EVALUATION_CRITERIA.json` (with stable factor_id) | - |
| 1.7 | Compliance Gatekeeper ⛔ | Compliance Officer | `COMPLIANCE_MATRIX.json` (with rfp_source_id) | - |
| 1.8 | Submission Structure | Procurement Analyst | `SUBMISSION_STRUCTURE.json` | 2KB |
| 1.9 | Go/No-Go Decision Gate | Bid Decision Analyst | `GO_NOGO_DECISION.json` | - |
| 1.95 | Client Intelligence [CONDITIONAL] | Competitive Intel Analyst | `CLIENT_INTELLIGENCE.json` | 2KB |
| SVA-1 | Intake Validator | Validation Agent | `shared/validation/sva1-intake.json` | - |
| **STAGE 2: REQUIREMENTS ENGINEERING** (6 phases + SVA-2 PINK TEAM) |
| 2a | Workflow Extraction | Business Process Analyst | `workflow-extracted-reqs.json` | - |
| 2 | Requirements Extraction | Requirements Engineer | `requirements-raw.json` (with source_ids[]) | - |
| 2.5 | Sample Data Analysis | Data Analyst | `sample-data-analysis.json` | - |
| 2b | Normalize Requirements | Requirements Engineer | `requirements-normalized.json` | - |
| 2c | Requirements Catalog | Technical Writer | `REQUIREMENTS_CATALOG.md` | 10KB |
| 2d | Coverage Validation ⛔ | QA Engineer | `workflow-coverage.json` | - |
| SVA-2 | PINK TEAM REVIEW | Validation Agent | `shared/validation/sva2-pink-team.json` | - |
| **STAGE 3: SPECIFICATION GENERATION** (6 phases + SVA-3) |
| 3a | Architecture Specs | Solutions Architect | `ARCHITECTURE.md` | 15KB |
| 3b | Interoperability Specs | Integration Architect | `INTEROPERABILITY.md` | 5KB |
| 3c | Security Specs | Security Architect | `SECURITY_REQUIREMENTS.md` | 8KB |
| 3e | UI/UX Specs | UX Designer | `UI_SPECS.md` | - |
| 3f | Entity Definitions | Data Architect | `ENTITY_DEFINITIONS.md` | - |
| 3g | Risk Assessment | Risk Analyst | `REQUIREMENT_RISKS.json` (with stable risk_id/mitigation_id) | - |
| SVA-3 | Specification Validator | Validation Agent | `shared/validation/sva3-spec.json` | - |
| **STAGE 4: TRACEABILITY & ESTIMATION** (2 phases + SVA-4 RED TEAM) |
| 4 | Traceability + RTM Build | Requirements Traceability Engineer | `TRACEABILITY.md`, `UNIFIED_RTM.json` | 10KB |
| 5 | Effort Estimation | Project Estimator | `EFFORT_ESTIMATION.md` | 8KB |
| SVA-4 | RED TEAM REVIEW | Validation Agent | `shared/validation/sva4-red-team.json` | - |
| **STAGE 5: DOCUMENTATION** (3 phases + SVA-5) |
| 6 | Manifest Generation | Technical Writer | `MANIFEST.md`, `EXECUTIVE_SUMMARY.md` | 2KB |
| 6b | Navigation Guide | Technical Writer | `NAVIGATION_GUIDE.md` | 3KB |
| 6c | Context Bundle | Data Integration Architect | `bid-context-bundle.json` (with RTM composite scores + matched_evidence) | 5KB |
| SVA-5 | Documentation Validator | Validation Agent | `shared/validation/sva5-doc.json` | - |
| **STAGE 6: QUALITY ASSURANCE** (3 phases + SVA-6 RED TEAM FINAL) |
| 7 | Quality Validation & Gap Analysis [COMBINED] | QA Engineer | `validation-results.json`, `GAP_ANALYSIS.md` | - |
| 7c | Evaluator Personas | UX Researcher | `PERSONA_COVERAGE.json` | - |
| 7d | Bid Scoring Model | Bid Strategist | `WIN_SCORECARD.json` | - |
| SVA-6 | PRE-BID GATE | Validation Agent | `shared/validation/sva6-pre-bid.json` | - |
| **STAGE 7: BID GENERATION** (12 phases + SVA-7 GOLD TEAM) |
| 8.0 | Strategic Positioning | Bid Strategist | `POSITIONING_OUTPUT.json` (+ evidence-library matching) | 3KB |
| 8.1 | Letter of Submittal | Executive Communications Specialist | `01_SUBMITTAL.md` | 3KB |
| 8.2 | Management Proposal | Management Proposal Writer | `02_MANAGEMENT.md` | 8KB |
| 8.3 | Technical Approach | Senior Technical Proposal Writer | `03_TECHNICAL.md` | 15KB |
| 8.4 | Business Solution (x3) | Business Solution Architect | `04a_SOLUTION_*.md` | 10KB |
| 8.4r | Requirements Review | Requirements Analyst | `04_REQUIREMENTS_REVIEW.md` | 5KB |
| 8.4k | Risk Register | Risk Analyst | `04_RISK_REGISTER.md` | 3KB |
| 8.5 | Financial Proposal | Financial Analyst | `05_FINANCIAL.md` | 5KB |
| 8.6 | Technical Integration | Integration Architect | `06_INTEGRATION.md` | 5KB |
| 8f | RTM Verification | Traceability Verification Engineer | `RTM_REPORT.md` | 5KB |
| SVA-7 | GOLD TEAM REVIEW [13 rules] | Validation Agent | `shared/validation/sva7-gold-team.json`, `GOLD_TEAM_CHECKLIST.md` | - |
| 8d | Diagram Rendering | Visual Design Engineer | `*.png` (3 diagrams), `figure-registry.json` | 10KB |
| 8e | Multi-File PDF Assembly | Publication Specialist | Named PDFs per SUBMISSION_STRUCTURE | - |
| **POST-PIPELINE (Optional)** |
| 9 | Post-Bid Learning | Bid Performance Analyst | `BID_OUTCOME_REPORT.md`, `bid-outcomes.json` | 2KB |
| - | Metrics Aggregation | (Automatic) | `pipeline-metrics.json` | - |
| - | Post-Run Metrics Report | Pipeline Analytics Engineer | `PIPELINE_METRICS.md` | - |

---

## Expert Roles Summary

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                           25 UNIQUE EXPERT ROLES                                │
├─────────────────────────────────────────────────────────────────────────────────┤
│                                                                                 │
│  ┌─────────────────────┐    ┌─────────────────────┐    ┌─────────────────────┐ │
│  │ TECHNICAL           │    │ BUSINESS            │    │ QUALITY             │ │
│  ├─────────────────────┤    ├─────────────────────┤    ├─────────────────────┤ │
│  │ • Solutions         │    │ • Business Analyst  │    │ • QA Engineer (x3)  │ │
│  │   Architect (x2)    │    │ • Business Process  │    │ • Compliance Officer│ │
│  │ • Integration       │    │   Analyst           │    │ • Risk Analyst (x2) │ │
│  │   Architect (x2)    │    │ • Procurement       │    │ • Traceability      │ │
│  │ • Security Architect│    │   Specialist        │    │   Verification Eng. │ │
│  │ • Data Architect    │    │ • Procurement       │    │ • Validation Agent  │ │
│  │ • DevOps Engineer   │    │   Analyst           │    │   (x7 SVAs)         │ │
│  │ • Business Solution │    │ • Data Analyst      │    │                     │ │
│  │   Architect         │    │ • Financial Analyst │    │                     │ │
│  └─────────────────────┘    └─────────────────────┘    └─────────────────────┘ │
│                                                                                 │
│  ┌─────────────────────┐    ┌─────────────────────┐    ┌─────────────────────┐ │
│  │ WRITING             │    │ DESIGN              │    │ STRATEGY            │ │
│  ├─────────────────────┤    ├─────────────────────┤    ├─────────────────────┤ │
│  │ • Technical Writer  │    │ • UX Designer       │    │ • Bid Strategist    │ │
│  │   (x3)              │    │ • UX Researcher     │    │   (x2)              │ │
│  │ • Proposal Writer   │    │ • Visual Design     │    │ • Bid Decision      │ │
│  │ • Sr. Technical     │    │   Engineer          │    │   Analyst           │ │
│  │   Proposal Writer   │    │                     │    │ • Competitive Intel │ │
│  │ • Management        │    │                     │    │   Analyst           │ │
│  │   Proposal Writer   │    │                     │    │ • Project Manager   │ │
│  │ • Executive Comms   │    │                     │    │ • Project Estimator │ │
│  │   Specialist        │    │                     │    │                     │ │
│  │ • Requirements      │    │                     │    │                     │ │
│  │   Engineer (x3)     │    │                     │    │                     │ │
│  │ • Requirements      │    │                     │    │                     │ │
│  │   Analyst           │    │                     │    │                     │ │
│  │ • Document          │    │                     │    │                     │ │
│  │   Processing Spec.  │    │                     │    │                     │ │
│  │ • Publication       │    │                     │    │                     │ │
│  │   Specialist        │    │                     │    │                     │ │
│  │ • Data Integration  │    │                     │    │                     │ │
│  │   Architect         │    │                     │    │                     │ │
│  └─────────────────────┘    └─────────────────────┘    └─────────────────────┘ │
│                                                                                 │
└─────────────────────────────────────────────────────────────────────────────────┘
```

---

## Gates and Checkpoints

### Blocking Gates (Pipeline Halts)

| Gate | Phase | Condition | Action on Failure |
|------|-------|-----------|-------------------|
| ⛔ Compliance Gate | 1.7 | All mandatory items must be addressed | Pipeline halts; must address mandatory items |
| ⚠️ Go/No-Go Gate | 1.9 | Bid viability assessment (advisory) | NO-GO logged; pipeline continues with user override |
| ⛔ Coverage Gate | 2d | 100% workflow coverage required | Pipeline halts; must achieve full coverage |

### SVA Validation Gates (Color Team Reviews)

| SVA | Color Team | Completion | Disposition Options | Auto-Correct? |
|-----|------------|------------|---------------------|---------------|
| SVA-1 | - | Post-Stage 1 | PASS / ADVISORY / BLOCK | Yes (retry phase) |
| SVA-2 | PINK | ~25% | PASS / ADVISORY / BLOCK | Yes (retry phase) |
| SVA-3 | - | Post-Stage 3 | PASS / ADVISORY / BLOCK | Yes (retry phase) |
| SVA-4 | RED | ~60% | PASS / ADVISORY / BLOCK | Yes (retry phase) |
| SVA-5 | - | Post-Stage 5 | PASS / ADVISORY / BLOCK | Yes (retry phase) |
| SVA-6 | RED (final) | ~80% | PASS / ADVISORY / BLOCK | Yes (retry phase) |
| SVA-7 | GOLD | ~95% | PASS / ADVISORY / BLOCK | Yes (retry phase) |

**Disposition Logic:**
- **PASS**: All rules pass, or only MEDIUM/LOW severity failures
- **ADVISORY**: Any HIGH severity failure (pipeline continues with warnings)
- **BLOCK**: Any CRITICAL severity failure (pipeline halts, auto-correction attempted)

---

## Parallel Execution Groups

| Group | Phases | Purpose |
|-------|--------|---------|
| `phase3` | 3a, 3b, 3c, 3e, 3f | Specification generation (can run simultaneously) |

---

## Sprint Mode (~14 units)

When speed matters more than depth (tight deadlines, simple RFPs, repeat clients), invoke with `--sprint` to run a consolidated ~14-unit pipeline instead of the full 45.

### Sprint Phase Map

| Sprint Phase | Consolidates Full Phases | Name | SVA |
|-------------|-------------------------|------|-----|
| S0 | 0 | Folder Organization | - |
| S1 | 1 | Document Flattening | - |
| S2 | 1.5 + 1.6 + 1.7 + 1.8 | Combined Intake Analysis | SVA-S1 |
| S3 | 1.9 | Go/No-Go Decision Gate | - |
| S4 | 1.95 | Client Intelligence [if GO] | - |
| S5 | 2a + 2 + 2.5 + 2b + 2c + 2d | Combined Requirements | SVA-S2 |
| S6 | 3a-3g | Combined Specifications | - |
| S7 | 4 + 5 | Traceability + Estimation | SVA-S3 |
| S8 | 8.0 + 8.1-8.6 + 8f + 8d + 8e | Combined Bid Generation | SVA-S4 |

**When to use Sprint:**
- Simple or well-understood RFPs
- Repeat client with established relationship
- Tight turnaround (< 5 business days)
- Internal/informal proposals

**What Sprint skips:**
- Stages 5-6 (Documentation, QA) are folded into bid generation
- Individual SVA gates consolidated into 4 sprint checkpoints
- No separate Pink/Red/Gold team reviews (single final review)

---

## Unified RTM Data Flow

```
PHASE 1.6 ─── evaluation_criteria[] ──┐
PHASE 1.7 ─── mandatory_items[]       │
              rfp_sources[]            │
PHASE 2   ─── requirements[] ─────────┤
              (with source_ids[])      │
PHASE 3g  ─── risks[] ────────────────┤──→ PHASE 4: UNIFIED_RTM.json
              (with risk_id,           │         ├── entities (8 arrays)
               mitigation_id)          │         ├── chain_links[]
PHASE 3*  ─── specifications[] ────────┤         └── verification{}
              (section-level linking)  │
                                       │
PHASE 1.95 ── CLIENT_INTELLIGENCE.json ──→ consumed by PHASE 8.0
PHASE 6c  ←── Read RTM for composite_priority_scores
PHASE 8*  ──→ Update RTM: bid_sections[], evidence[], risk.mitigation.bid_location
PHASE 8f  ──→ Run 14 verification queries, update verification{}
```

---

## Output Directory Structure

```
{folder}/
├── original/                    # Source documents (moved here)
│   ├── rfp_document.pdf
│   ├── attachments.docx
│   └── sample_data.xlsx
├── flattened/                   # Markdown conversions
│   ├── rfp_document.md
│   ├── attachments.md
│   └── sample_data.md
├── shared/                      # Intermediate JSON files
│   ├── progress.json
│   ├── domain-context.json
│   ├── EVALUATION_CRITERIA.json
│   ├── COMPLIANCE_MATRIX.json
│   ├── SUBMISSION_STRUCTURE.json      [NEW]
│   ├── UNIFIED_RTM.json               [NEW - backbone]
│   ├── workflow-extracted-reqs.json
│   ├── requirements-raw.json
│   ├── requirements-normalized.json
│   ├── REQUIREMENTS_CATALOG.json
│   ├── REQUIREMENT_RISKS.json
│   ├── sample-data-analysis.json
│   ├── effort-estimation.json
│   ├── workflow-coverage.json
│   ├── validation-results.json
│   ├── PERSONA_COVERAGE.json
│   ├── WIN_SCORECARD.json
│   ├── bid-context-bundle.json
│   ├── source-manifest.json
│   ├── GO_NOGO_DECISION.json            [NEW - bid viability]
│   ├── validation/                     [NEW - SVA reports]
│   │   ├── sva1-intake.json
│   │   ├── sva2-pink-team.json
│   │   ├── sva3-spec.json
│   │   ├── sva4-red-team.json
│   │   ├── sva5-doc.json
│   │   ├── sva6-pre-bid.json
│   │   └── sva7-gold-team.json
│   ├── evidence-library.json                [NEW - proof points for bid sections]
│   └── bid/
│       ├── CLIENT_INTELLIGENCE.json
│       └── POSITIONING_OUTPUT.json
└── outputs/                     # Final deliverables
    ├── EXECUTIVE_SUMMARY.md
    ├── REQUIREMENTS_CATALOG.md
    ├── ARCHITECTURE.md
    ├── INTEROPERABILITY.md
    ├── SECURITY_REQUIREMENTS.md
    ├── UI_SPECS.md
    ├── ENTITY_DEFINITIONS.md
    ├── REQUIREMENT_RISKS.md
    ├── TRACEABILITY.md
    ├── EFFORT_ESTIMATION.md
    ├── MANIFEST.md
    ├── NAVIGATION_GUIDE.md
    ├── GAP_ANALYSIS.md
    ├── RTM_REPORT.md                   [NEW]
    ├── GOLD_TEAM_CHECKLIST.md         [NEW - human review sign-off]
    ├── bid-sections/                    [NEW - multi-file bid volumes]
    │   ├── 01_SUBMITTAL.md
    │   ├── 02_MANAGEMENT.md
    │   ├── 03_TECHNICAL.md
    │   ├── 04a_SOLUTION_COLLECTION.md
    │   ├── 04b_SOLUTION_CALCULATION.md
    │   ├── 04c_SOLUTION_REPORTING.md
    │   ├── 04_REQUIREMENTS_REVIEW.md
    │   ├── 04_RISK_REGISTER.md
    │   ├── 05_FINANCIAL.md
    │   └── 06_INTEGRATION.md
    └── bid/                             # PDFs, diagrams, assembly
        ├── {Bidder}_1_SUBMITTAL.pdf
        ├── {Bidder}_2_MANAGEMENT.pdf
        ├── {Bidder}_3_TECHNICAL.pdf
        ├── {Bidder}_4_SOLUTION.pdf
        ├── {Bidder}_5_FINANCIAL.pdf
        ├── {Bidder}_6_INTEGRATION.pdf
        ├── Draft_Bid.pdf               # Consolidated review copy
        ├── EXECUTIVE_SUMMARY.pdf
        ├── REQUIREMENTS_CATALOG.pdf
        ├── TRACEABILITY_MATRIX.pdf
        ├── assembly-report.json
        ├── figure-registry.json       [NEW - action captions for diagrams]
        ├── architecture.png
        ├── timeline.png
        ├── timeline.mmd
        └── orgchart.png

# Persistent config (shared across RFP runs, lives in skill's config-win/ directory):
config-win/
├── bid-outcomes.json               [NEW - historical bid outcomes across runs]
├── pipeline-metrics.json           [NEW - execution metrics across runs]
└── integrations.json               [NEW - optional external tool hooks]
```

---

## Key Enhancements (v2 over v1)

| Feature | v1 (Original) | v2 (Enhanced) |
|---------|---------------|---------------|
| **Pipeline Size** | 31 phases | 38 phases + 7 SVAs = 45 units (full) / ~14 (sprint) |
| **Validation** | File existence/size only | 7 SVAs with 44 content quality rules |
| **Color Teams** | None | Pink, Red, Gold (Shipley/APMP) |
| **Traceability** | TRACEABILITY.md only | UNIFIED_RTM.json (8 entity types, chain_links) |
| **RTM Verification** | None | 14 queries (5 forward, 4 backward, 5 chain) |
| **Bid Output** | 1 Draft_Bid.pdf | 6+ named PDFs per RFP convention |
| **Bid Volumes** | 3 sections (title, solution, timeline) | 10 volumes (submittal through integration) |
| **Company Data** | None | Pre-populated company-profile.json |
| **Submission Structure** | Hard-coded | Auto-detected from RFP (Phase 1.8) |
| **Priority Scoring** | Requirements priority only | Composite score (eval weight + priority + mandatory + risk) |
| **Risk Tracking** | Bulk risks | Stable risk_id + mitigation_id per risk |
| **User Input Markers** | None | `[USER INPUT REQUIRED]` in bid sections |
| **Financial Rates** | None | Market rate fallback (GSA IT Schedule 70 defaults), rate source attribution |
| **Eval Alignment** | None | theme_eval_mapping, section_theme_mandates, eval callout boxes in bid volumes |
| **Win Theme Threading** | Optional | Mandatory CVD format, >= 50% section coverage, grep-verified (SVA-7) |
| **Expert Roles** | 16 | 25 |
| **Evidence Library** | None | evidence-library.json matched to RFP requirements (Phase 8.0) |
| **Figure Registry** | None | figure-registry.json with persuasive action captions (Phase 8d) |
| **Human Review Gate** | None | AWAITING_HUMAN_REVIEW status + GOLD_TEAM_CHECKLIST.md sign-off |
| **Proof Point Density** | None | SVA7-PROOF-POINT-DENSITY rule (evidence coverage per section) |
| **Post-Bid Learning** | None | bid-outcomes.json + Phase 9 (historical pattern analysis after 3+ bids) |
| **Pipeline Metrics** | None | Automatic timing/failure collection per run (pipeline-metrics.json) |
| **External Integrations** | None | Optional hooks for CRM, proposal DB, doc repo, notifications (integrations.json) |
| **Security Hardening** | None | Phase 0 path sanitization, .gitignore generation, SECURITY_AUDIT.md |

### Priority 3 Enhancements (G/H/I/J)

| Enhancement | Description | Phases Affected |
|-------------|-------------|-----------------|
| **G: Phase 7+7b Consolidation** | Merged structural validation and gap analysis into single Phase 7 | Phase 7 (was 7+7b parallel) |
| **H: Action Captions & Figure Registry** | Phase 8d generates figure-registry.json with persuasive action captions for all diagrams | Phase 8d |
| **I: Evidence Library Integration** | Phase 8.0 matches evidence from evidence-library.json to RFP requirements; Phase 6c passes matched_evidence to bid-context-bundle | Phases 8.0, 6c |
| **J: Gold Team Human Review Handoff** | SVA-7 generates GOLD_TEAM_CHECKLIST.md; pipeline enters AWAITING_HUMAN_REVIEW status before PDF assembly; adds Rule 13 (SVA7-PROOF-POINT-DENSITY) | SVA-7, Layer 4 gate |

### Priority 4 Enhancements (K/L/M/N)

| Enhancement | Description | Phases Affected |
|-------------|-------------|-----------------|
| **K: Post-Bid Learning Loop** | Phase 9 logs bid outcomes; Phase 8.0 consults historical patterns for positioning | Phase 9 (new), Phase 8.0 (Step 0 added) |
| **L: Pipeline Metrics** | update_progress() tracks timing; aggregate_metrics() persists stats across runs | skill-win.md (enhanced), post-run-metrics phase (new) |
| **M: External Tool Integration** | Optional hooks for CRM, proposal DB, document repo, notifications — log-only, adapter templates provided | skill-win.md (hooks), integrations.json (new) |
| **N: Security Audit** | Phase 0 path sanitization, .gitignore generation, comprehensive SECURITY_AUDIT.md with 7 risk assessments | Phase 0 (hardened), SECURITY_AUDIT.md (new) |

---

## Related Skills

| Skill | Description | When to Use |
|-------|-------------|-------------|
| `/process-rfp-win` | Main RFP processing pipeline | Primary invocation |
| `/process-rfp-win-demo` | Demo scenario generation | After Phase 2 completes |

---

## Version

- **Pipeline Version:** WIN Edition v2 (Priority 4)
- **Total Phases:** 38
- **Optional Phases:** 2 (Phase 9, Post-Run Metrics)
- **SVA Gates:** 7
- **Total Execution Units:** 45 (full) / ~14 (sprint)
- **Sprint Mode:** Yes
- **Blocking Gates:** 2
- **Advisory Gates:** 1 (Go/No-Go)
- **Human Review Gates:** 1 (AWAITING_HUMAN_REVIEW after SVA-7)
- **Color Team Reviews:** 3 (Pink, Red, Gold)
- **Expert Roles:** 25 unique
- **SVA Rules:** 47 across 7 validators (SVA-7 has 13 rules)
- **RTM Verification Queries:** 14
- **Final PDF Outputs:** 6+ named volumes + 4 supporting PDFs
- **Security:** SECURITY_AUDIT.md (7 risks assessed)
