# RFP Win Pipeline — Design Rationale & Technical Reference

## Overview

`/process-rfp-win` is a full-scale proposal development pipeline that transforms raw RFP documents into a complete, multi-volume bid submission with PDF deliverables. It coordinates **45 execution units** (38 phases + 7 validation gates) across **27 expert roles**, producing a traceable chain from every RFP requirement through specifications to final bid text.

**Primary deliverables:** Professional PDFs in `{rfp-folder}/outputs/bid/` — 6 named volumes + consolidated Draft_Bid.pdf

```
/process-rfp-win <path-to-rfp-folder> [--sprint]
```

**Runtime:** ~3–4 hours (full mode), ~1.5–2 hours (sprint mode)

---

## Why This Pipeline Exists

Traditional proposal development is manual, error-prone, and produces inconsistent results:

- **Traceability gaps** — evaluators score proposals on requirement-by-requirement coverage. Missing a single mandatory item can mean disqualification. Manual processes routinely miss 5–15% of requirements.
- **Theme inconsistency** — win themes stated in the executive summary often don't appear in technical sections. Evaluators notice.
- **Compliance drift** — between first draft and final submission, compliance items fall through the cracks as sections get edited independently.
- **No institutional learning** — most organizations don't systematically capture what worked in winning bids vs. what failed in losses.

This pipeline addresses all four by building a **Unified Requirements Traceability Matrix (RTM)** that links every entity — from RFP source text through requirements, specifications, risks, and bid sections — into a single auditable chain. Seven validation gates enforce integrity at each stage.

---

## Architecture: 7 Stages with Validation Gates

The pipeline follows the **Shipley Business Development Lifecycle** structure, enhanced with automated validation at each stage boundary. The Shipley model defines color-team reviews at specific completion milestones; this pipeline automates those reviews as **Stage Validation Agents (SVAs)**.

| Stage | Name | Phases | SVA Gate | Color Team | Completion % |
|-------|------|--------|----------|------------|-------------|
| 1 | Document Intake | 9 | SVA-1 | — | ~15% |
| 2 | Requirements Engineering | 6 | SVA-2 | Pink | ~25% |
| 3 | Specification Generation | 6 | SVA-3 | — | ~40% |
| 4 | Traceability & Estimation | 2 | SVA-4 | Red | ~55% |
| 5 | Documentation | 3 | SVA-5 | — | ~65% |
| 6 | Quality Assurance | 3 | SVA-6 | Red (Final) | ~80% |
| 7 | Bid Generation | 12 | SVA-7 | Gold | ~95% |
| Post | PDF Assembly + Diagrams | 2 | — | — | 100% |

**Post-pipeline (optional):** Phase 9 (bid outcome logging), Post-Run Metrics aggregation.

---

## Stage-by-Stage Design Rationale

### Stage 1: Document Intake (9 phases + SVA-1)

**Gold standard parallel:** Shipley Phase A — "Opportunity Assessment and Capture Planning." APMP's first principle: understand the opportunity before responding to it.

**What happens:** Raw RFP documents are organized, converted to markdown, classified by industry domain, analyzed for evaluation criteria, checked for compliance requirements, assessed for bid viability, and enriched with competitive intelligence.

| Phase | Expert Role | Purpose | Key Output |
|-------|-------------|---------|------------|
| 0 | DevOps Engineer | Create directory structure (original/, flattened/, shared/, outputs/) | Folder hierarchy |
| 1 | Document Processing Specialist | Convert PDF/DOCX/XLSX to markdown via `markitdown` | `flattened/*.md` |
| 1.5 | Business Analyst | Classify industry domain, identify compliance frameworks (HIPAA, FERPA, FedRAMP) | `domain-context.json` |
| 1.6 | Procurement Specialist | Extract evaluation factors, weights, scoring methodology. Assign stable `factor_id` | `EVALUATION_CRITERIA.json` |
| 1.7 | Compliance Officer | Extract all SHALL/MUST items, create compliance matrix. **BLOCKING GATE** | `COMPLIANCE_MATRIX.json` |
| 1.8 | Procurement Analyst | Detect submission volume structure, naming conventions, page limits | `SUBMISSION_STRUCTURE.json` |
| 1.9 | Bid Decision Analyst | Go/No-Go scoring (5 dimensions, 0–100). **ADVISORY GATE** | `GO_NOGO_DECISION.json` |
| 1.95 | Competitive Intelligence Analyst | Client research: incumbents, tech stack, decision-makers (max 15 web searches). **Conditional: GO only** | `CLIENT_INTELLIGENCE.json` |

**Why Phase 1.7 is a blocking gate:** A compliance matrix with gaps means the proposal could be disqualified on a technicality. Every mandatory item must be identified before requirements engineering begins. Industry data shows that ~30% of proposal disqualifications are compliance-related.

**Why Phase 1.9 is advisory (not blocking):** The Go/No-Go decision is ultimately a human business judgment. The pipeline provides a structured score (same 5-dimension model as the screening pipeline) but the user can override. Skipped entirely in Sprint mode where the bid decision is pre-approved.

**Why Phase 1.95 was relocated from Stage 7 to Stage 1:** Client intelligence informs requirements interpretation (Stage 2), architecture decisions (Stage 3), and positioning strategy (Stage 7). Having it available from the beginning produces better-informed analysis at every stage.

**SVA-1 validates:** Flattening completeness (every source file converted), quality (reasonable character/KB ratio), domain confidence (>=0.8), evaluation criteria extraction (>=3 factors), and compliance extraction depth (mandatory items vs SHALL/MUST occurrences ratio >0.7).

---

### Stage 2: Requirements Engineering (6 phases + SVA-2 Pink Team)

**Gold standard parallel:** Shipley Phase B — "Requirements Analysis." APMP's requirements management discipline. The Pink Team review (~25% completion) checks that requirements capture is complete before specification work begins.

**What happens:** Business workflows are mapped, requirements are extracted from RFP text, sample data is profiled, requirements are deduplicated and normalized, a formal catalog is produced, and coverage is validated.

| Phase | Expert Role | Purpose | Key Output |
|-------|-------------|---------|------------|
| 2a | Business Process Analyst | Map process flows, BPMN workflows, identify workflow-embedded requirements | `workflow-extracted-reqs.json` |
| 2 | Requirements Engineer | Extract all functional/non-functional requirements with `source_ids[]` tracing back to RFP sections | `requirements-raw.json` |
| 2.5 | Data Analyst | Profile sample data files (if included in RFP), map data structures and entities | `sample-data-analysis.json` |
| 2b | Requirements Engineer | Deduplicate, validate, assign stable IDs, categorize. Target: 10–40% reduction from raw | `requirements-normalized.json` |
| 2c | Technical Writer | Generate formatted requirements catalog document (10KB+) | `REQUIREMENTS_CATALOG.md` |
| 2d | QA Engineer | Verify 100% workflow coverage by requirements. **BLOCKING GATE** | `workflow-coverage.json` |

**Why workflow extraction (2a) precedes requirements extraction (2):** RFPs often embed requirements within process descriptions rather than stating them explicitly. Workflow analysis surfaces these implicit requirements. Phase 2 then extracts explicit requirements. The combination catches requirements that either approach alone would miss.

**Why Phase 2d is a blocking gate:** If workflows aren't fully covered by requirements, the specification stage will produce incomplete designs. The 100% coverage requirement ensures no workflow gaps persist.

**The deduplication quality band (10–40%):** Below 10% reduction means duplicates remain (different phrasings of the same requirement weren't merged). Above 40% means over-merging (distinct requirements were incorrectly combined). This band was calibrated from observed patterns across multiple RFP types.

**SVA-2 (Pink Team) validates:** Source coverage (every SHALL/MUST section has requirements traced to it), dedup quality (10–40% reduction), priority distribution (CRITICAL 5–15%, HIGH 20–35%, MEDIUM 35–55%, LOW 10–25%), category coverage (>=5 categories), compliance-to-requirement mapping, workflow alignment quality, sample data integration, and strategy readiness (>=50 requirements, >=3 categories).

---

### Stage 3: Specification Generation (6 phases + SVA-3)

**Gold standard parallel:** Shipley Phase C — "Solution Development." This is where the technical response takes shape. Five of six phases run in parallel for efficiency.

**What happens:** Six specification documents are produced from normalized requirements — architecture, interoperability, security, UI/UX, entity definitions, and risk assessment.

| Phase | Expert Role | Purpose | Key Output | Parallel |
|-------|-------------|---------|------------|----------|
| 3a | Solutions Architect | System design, cloud architecture, scalability, technology selection | `ARCHITECTURE.md` (15KB+) | Yes |
| 3b | Integration Architect | APIs, data exchange, EDI, HL7/FHIR, third-party integrations | `INTEROPERABILITY.md` (5KB+) | Yes |
| 3c | Security Architect | OWASP, encryption, authentication, domain-specific compliance (HIPAA/FERPA) | `SECURITY_REQUIREMENTS.md` (8KB+) | Yes |
| 3e | UX Designer | User interfaces, accessibility (508), wireframes, user workflows | `UI_SPECS.md` | Yes |
| 3f | Data Architect | Entity modeling, ERD, database design, data relationships | `ENTITY_DEFINITIONS.md` | Yes |
| 3g | Risk Analyst | Risk identification, severity scoring, mitigation strategies. Runs AFTER 3a–3f | `REQUIREMENT_RISKS.md` + JSON |

**Why Phase 3g runs sequentially after the parallel group:** Risk assessment requires understanding the proposed architecture, security posture, and integration approach. It synthesizes findings from all five specification documents to identify risks that span multiple domains (e.g., a security risk that arises from an architectural decision).

**Tech lifecycle validation (Phase 3a + 8.3):** Architecture and technical approach phases must validate that proposed technologies have current LTS support through the contract period. Web search is mandatory for verifying current LTS versions. An EOL Date column is required in technology recommendation tables. SVA-7 (Gold Team) includes a dedicated rule (SVA7-TECH-LIFECYCLE-VALIDATION) that catches expiring technology choices.

**SVA-3 validates:** Spec-to-requirement coverage (every CRITICAL/HIGH requirement addressed), internal consistency (technology choices align across specs), domain alignment (domain-appropriate standards referenced), risk-to-spec coverage (HIGH risk mitigations reflected in specs), entity completeness, and spec depth ratio (minimum 200 bytes per requirement addressed).

---

### Stage 4: Traceability & Estimation (2 phases + SVA-4 Red Team)

**Gold standard parallel:** Shipley's Red Team review (~55% completion). The Red Team simulates the actual evaluation process, scoring the proposal as evaluators would. This is the first review that assesses the response from the evaluator's perspective rather than the author's.

**What happens:** The central data artifact (UNIFIED_RTM.json) is built, linking all entities into a single traceable chain. Effort estimation produces staffing and cost projections.

| Phase | Expert Role | Purpose | Key Output |
|-------|-------------|---------|------------|
| 4 | Requirements Traceability Engineer | Build UNIFIED_RTM.json linking all entities across the full chain | `UNIFIED_RTM.json` (10KB+) + `TRACEABILITY.md` |
| 5 | Project Estimator | Effort estimation, resource planning, AI-adjusted ratios | `EFFORT_ESTIMATION.md` (8KB+) |

#### The Unified RTM — Central Data Artifact

The UNIFIED_RTM.json is the pipeline's most important data structure. It links:

```
RFP Source Sections → Mandatory Items → Requirements → Specifications → Risks → Bid Sections → Evidence
```

Each entity type has stable IDs assigned in earlier phases:
- `rfp_source_id` (Phase 1.7)
- `mandatory_item_id` (Phase 1.7)
- `requirement_id` (Phase 2b)
- `spec_id` (Phase 3a–3f)
- `risk_id` / `mitigation_id` (Phase 3g)
- `factor_id` / `subfactor_id` (Phase 1.6)
- `bid_section_id` (Phase 8.0+)
- `evidence_id` (Phase 8.0+)

**Why this matters for proposals:** Government and enterprise evaluators score proposals by tracing requirements to responses. If a requirement appears in Section C of the RFP but isn't addressed in the technical volume, the evaluator marks it as unaddressed. The RTM ensures 100% forward traceability (every requirement has a bid response) and 100% backward traceability (every bid claim traces to a requirement).

The RTM schema is defined in `schemas/unified-rtm.schema.json` and enforces structural consistency.

**SVA-4 (Red Team) validates:** Bidirectional traceability completeness, traceability quality (20% sample audit for semantic alignment), Red Team evaluator simulation (mock scoring using evaluation criteria weights), compliance forward traceability (every mandatory item traces through the full chain), estimation consistency (500–50,000 hours typical range), and estimation-risk alignment (high-risk requirements should have higher effort).

---

### Stage 5: Documentation (3 phases + SVA-5)

**Gold standard parallel:** This stage produces the internal documentation that enables quality assurance. In Shipley terms, this is the preparation for the final review cycle — assembling all information into a reviewable format.

| Phase | Expert Role | Purpose | Key Output |
|-------|-------------|---------|------------|
| 6 | Technical Writer | Manifest of all pipeline outputs + executive summary | `MANIFEST.md` + `EXECUTIVE_SUMMARY.md` |
| 6b | Technical Writer | Navigation guide for reviewers | `NAVIGATION_GUIDE.md` |
| 6c | Data Integration Architect | Context bundle aggregating 10+ source files for bid authoring | `bid-context-bundle.json` |

**Why the context bundle (6c) is critical:** Bid authors in Stage 7 cannot read all 20+ JSON files individually. The context bundle pre-aggregates: evaluation factors by weight, theme-to-evaluation-factor mapping, section-level theme mandates, RTM composite scores, compliance status, client intelligence highlights, and content priority ordering. This single file becomes the "briefing document" that informs every bid section.

The context bundle includes:
- `bid_section_mapping` — which themes, eval factors, and requirements map to each bid section
- `eval_factors_by_weight` — evaluation criteria ranked by importance
- `theme_eval_mapping` — how each win theme supports specific evaluation factors
- `section_theme_mandates` — which themes and eval factors MUST appear in each section
- `section_content_guide` — content priorities per section based on eval weights

**SVA-5 validates:** Manifest accuracy (every listed file exists), executive summary statistics (match actual data), navigation guide link validity, and context bundle completeness (10+ source files aggregated).

---

### Stage 6: Quality Assurance (3 phases + SVA-6 Red Team Final)

**Gold standard parallel:** The final Red Team review before bid writing begins. Shipley positions this as the last opportunity to change direction before investing in full proposal text.

| Phase | Expert Role | Purpose | Key Output |
|-------|-------------|---------|------------|
| 7 | QA Engineer | Structural validation + benchmark gap analysis | `validation-results.json` + `GAP_ANALYSIS.md` |
| 7c | UX Researcher | Build evaluator personas modeling how different evaluator types will read the proposal | `PERSONA_COVERAGE.json` |
| 7d | Bid Strategist | Win probability model based on all available data | `WIN_SCORECARD.json` |

**Why evaluator personas (7c) matter:** Different evaluators read proposals differently. A technical evaluator focuses on methodology and architecture. A financial evaluator scrutinizes cost reasonableness. A risk evaluator looks for red flags. The persona model ensures the bid addresses each evaluator type's specific concerns, and SVA-7 (Gold Team) verifies that each persona's top 3 concerns are addressed.

The pipeline models these personas:
- **EXECUTIVE** — Strategic value, organizational capability, past performance
- **TECHNICAL** — Methodology rigor, architecture soundness, technology currency
- **FINANCIAL** — Cost reasonableness, rate competitiveness, TCO accuracy
- **RISK** — Risk identification completeness, mitigation feasibility
- **OPERATIONAL** — Transition plan, staffing, knowledge transfer

**SVA-6 (Pre-Bid Gate) validates:** Persona coverage minimum (overall >=80, each persona >=70), win probability threshold (>=60%), gap analysis (no HIGH severity gaps remaining), risk mitigation completeness (every HIGH/CRITICAL risk has mitigation strategy + owner + verification criteria), bid context integrity (counts match source files), and evaluation weight coverage (each factor >=20% weight has specs and requirements addressing it).

---

### Stage 7: Bid Generation (12 phases + SVA-7 Gold Team)

**Gold standard parallel:** Shipley Phase D — "Proposal Development." The Gold Team review (~95% completion) is the final quality gate. This is the most rigorous review, simulating the actual evaluation scoring process.

**What happens:** Strategic positioning is established, then six bid volumes are authored, followed by RTM verification, Gold Team review, diagram rendering, and PDF assembly.

| Phase | Expert Role | Purpose | Key Output |
|-------|-------------|---------|------------|
| 8.0 | Bid Strategist | Strategic positioning, win themes, theme-to-eval mapping, past project scoring | `POSITIONING_OUTPUT.json` |
| 8.1 | Executive Communications Specialist | Letter of Submittal using NOSE formula, eval factor callouts | `01_SUBMITTAL.md` |
| 8.2 | Management Proposal Writer | Experience narratives, team qualifications, OCM, 3–5 case studies auto-populated | `02_MANAGEMENT.md` |
| 8.3 | Senior Technical Proposal Writer | Methodology, QA processes, risk management, KPIs, tech lifecycle validation | `03_TECHNICAL.md` (15KB+) |
| 8.4 | Business Solution Architect | Per-work-section solution design (one file per section) | `04a_SOLUTION_*.md` |
| 8.4r | Requirements Analyst | Requirements review response table in Attachment A format | `04_REQUIREMENTS_REVIEW.md` |
| 8.4k | Risk Analyst | Tabular risk register with mitigation tracking | `04_RISK_REGISTER.md` |
| 8.5 | Financial Analyst | Cost narratives, rate tables, pricing strategy, TCO | `05_FINANCIAL.md` |
| 8.6 | Integration Architect | Multi-vendor coordination, integration architecture | `06_INTEGRATION.md` |
| 8f | Traceability Verification Engineer | 14-query verification protocol against UNIFIED_RTM.json | `RTM_REPORT.md` |

**Post-SVA-7 (after Gold Team passes):**

| Phase | Expert Role | Purpose | Key Output |
|-------|-------------|---------|------------|
| 8d | Visual Design Engineer | Render Mermaid diagrams (architecture, timeline, org chart) to PNG | `outputs/bid/*.png` |
| 8e | Publication Specialist | Multi-file PDF assembly — 6 named volumes + Draft_Bid.pdf. **MANDATORY** | `outputs/bid/*.pdf` |

#### Phase 8.0: Strategic Positioning (The Pivot Point)

This is where analysis transforms into persuasion. Phase 8.0:

1. **Scores and ranks past projects** from `Past_Projects.md` against the current RFP using the same algorithm as the screening pipeline (industry + technology + metrics + quote + recency + scale)
2. **Consults historical bid outcomes** from `bid-outcomes.json` — if 3+ completed bids exist, it identifies effective themes (themes in wins vs losses) and recurring weaknesses
3. **Generates `theme_eval_mapping`** — maps each win theme to the specific evaluation factors it supports, with alignment scores
4. **Generates `section_theme_mandates`** — prescribes which themes and evaluation factors must appear in each bid section
5. **Produces `evaluator_messages`** — tailored messages for each evaluator persona (EXECUTIVE, TECHNICAL, FINANCIAL, RISK, OPERATIONAL)

This output drives all subsequent bid sections. Every section author reads the positioning output to ensure consistent messaging.

#### CVD Writing Format

Bid sections use the **Capability → Value → Differentiator** (CVD) format for win theme integration:

- **Capability:** What we can do (factual claim)
- **Value:** Why it matters to the client (benefit statement)
- **Differentiator:** Why our approach is better than alternatives (competitive contrast)

Each bid section must include >=2 win themes in CVD format per major section (## heading level).

#### Evaluation Alignment System

Every bid section includes **eval factor callout boxes** at the top of each major section, explicitly labeling which evaluation factors that section addresses. This makes it easy for evaluators to find the content relevant to their scoring criteria.

Content within sections is ordered by evaluation factor weight — the most heavily weighted factors appear first.

#### Past Projects Integration

Phases 8.0–8.3 automatically select and integrate case studies from `Past_Projects.md`:

- Phase 8.0 scores all 28 projects and ranks them by relevance
- Phase 8.1 uses top matches for NOSE (Need-Outcome-Scope-Evidence) evidence
- Phase 8.2 auto-populates 3–5 case studies with formatted narratives
- Phase 8.3 includes "Proven Capability" callouts citing specific project outcomes

The company profile can override project selection via `override_projects[]` in `company-profile.json`.

#### Financial Proposal (Phase 8.5) — Market Rate Fallback

The financial proposal uses a three-tier rate sourcing strategy:

1. **Company rates** from `company-profile.json` (preferred — most accurate)
2. **Market rate defaults** from `company-profile.json market_rate_defaults` (fallback for roles without company rates)
3. **Unpopulated** markers for roles with no data available

Every rate includes source attribution: `COMPANY`, `MARKET DEFAULT`, or `UNPOPULATED [USER INPUT REQUIRED]`. This transparency prevents accidentally submitting with placeholder rates.

#### RTM Verification (Phase 8f) — 14-Query Protocol

After all bid sections are authored, Phase 8f runs 14 verification queries against UNIFIED_RTM.json:

- Forward trace: requirement → spec → bid section
- Backward trace: bid section → spec → requirement
- Compliance chain: mandatory item → requirement → spec → bid section
- Orphan detection: bid claims without requirement backing
- Coverage gaps: requirements without bid responses
- ID integrity: all referenced IDs exist in source files

#### SVA-7 (Gold Team) — 13 Validation Rules

The Gold Team is the most rigorous validation gate. It enforces:

| Rule ID | Name | Severity | What It Checks |
|---------|------|----------|----------------|
| SVA7-THEME-THREADING-DEPTH | Win Theme Semantic Threading | HIGH | Themes appear with evidence (2+ substantive paragraphs), not just as headings |
| SVA7-RISK-BID-INTEGRATION | Risk-to-Bid Traceability | CRITICAL | Every HIGH risk mitigation verified present in bid text |
| SVA7-COMPLIANCE-BID-COVERAGE | Compliance-to-Bid Verification | CRITICAL | Every mandatory item from compliance matrix addressed in bid |
| SVA7-CROSS-DOC-ID-INTEGRITY | Cross-Document ID Integrity | HIGH | All requirement/risk IDs referenced in bid sections exist in source JSON |
| SVA7-PERSONA-SATISFACTION | Evaluator Persona Satisfaction | HIGH | Each evaluator persona's top 3 concerns addressed |
| SVA7-FORMAT-COMPLIANCE | Format Compliance | HIGH | Page count within limits, font/margin requirements met |
| SVA7-CONSISTENCY-CHECK | Statistic Consistency | MEDIUM | Numbers cited in bid match source data |
| SVA7-COMPETITIVE-CONTRAST | Competitive Positioning | MEDIUM | Incumbent weaknesses from client intelligence referenced |

After SVA-7 passes, it generates `GOLD_TEAM_CHECKLIST.md` — a human-readable checklist for final sign-off before submission. The pipeline displays `AWAITING_HUMAN_REVIEW` status at this point.

#### PDF Assembly (Phase 8e) — Mandatory

The pipeline is explicitly designed to produce PDFs, not markdown. Phase 8e:

1. Reads `SUBMISSION_STRUCTURE.json` for volume naming and ordering
2. Generates named PDFs per volume: `ResourceData_1_SUBMITTAL.pdf`, `ResourceData_2_MANAGEMENT.pdf`, etc.
3. Generates a consolidated `Draft_Bid.pdf` combining all volumes
4. Generates `EXECUTIVE_SUMMARY.pdf`
5. Uses Python `markdown_pdf` (PyMuPDF `fitz.Story`) as primary renderer, `npx md-to-pdf` as fallback

**The pipeline returns `False` (failure) if zero PDFs are generated.** This is enforced in the final verification step.

**CSS constraints (fitz.Story renderer):**
- No CSS `border` properties (render as thick filled rectangles)
- No `background-color` on block elements (ghost fills leak across pages)
- Safe: `color`, `font-*`, `padding`, `margin`, `text-align`
- `hr` must use `height: 0; color: #ffffff; background-color: #ffffff;`

---

## The SVA Validation Framework

### Design Philosophy

Traditional proposal reviews are subjective — different reviewers catch different issues. The SVA framework makes review criteria explicit, measurable, and repeatable.

Each SVA:
1. Reads all artifacts produced by its stage
2. Evaluates against rules defined in `config-win/sva-rules-registry.json`
3. Produces a structured JSON report conforming to `schemas/sva-report.schema.json`
4. Returns a disposition: **PASS**, **ADVISORY**, or **BLOCK**

### Disposition Rules

| Disposition | Trigger | Pipeline Action |
|-------------|---------|-----------------|
| **PASS** | All findings pass, or only MEDIUM/LOW failures | Continue to next stage |
| **ADVISORY** | Any HIGH finding fails (no CRITICAL failures) | Continue with warnings logged |
| **BLOCK** | Any CRITICAL finding fails | Attempt auto-correction, then halt if unresolvable |

### Auto-Correction

When a BLOCK disposition includes auto-correctable findings, the pipeline:
1. Identifies the corrective phase (defined per rule in the registry)
2. Re-runs that phase with enhanced instructions
3. Re-runs the SVA
4. If still BLOCK, halts and presents options to the user

### Color Team Reviews (Industry Standard)

| Color | When | Focus | Industry Equivalent |
|-------|------|-------|-------------------|
| **Pink** | After Stage 2 (~25%) | Requirements completeness, strategy readiness | Shipley Pink Team — "Do we have all the pieces?" |
| **Red** | After Stage 4 (~55%) | Evaluator simulation, scoring dry-run | Shipley Red Team — "Would we win with this?" |
| **Red Final** | After Stage 6 (~80%) | Gap closure, risk mitigation, pre-bid readiness | Shipley Red Team Scrub |
| **Gold** | After Stage 7 (~95%) | Final quality, compliance verification, theme threading | Shipley Gold Team — "Is this ready to submit?" |

### Rule Registry (Extensible)

The `sva-rules-registry.json` file defines all validation rules for all 7 SVAs. Users can add custom rules to the `custom_rules[]` array. Each rule specifies:
- `id` — unique identifier
- `severity` — CRITICAL, HIGH, MEDIUM, or LOW
- `category` — Completeness, Content, Consistency, or Traceability
- `enabled` — toggle rules on/off
- `auto_correctable` — whether the pipeline can fix it automatically
- `corrective_phase` — which phase to re-run for auto-correction

**Total rules across all SVAs:** 44 (6 in SVA-1, 8 in SVA-2, 6 in SVA-3, 6 in SVA-4, 4 in SVA-5, 6 in SVA-6, 8 in SVA-7)

---

## Sprint Mode

Sprint mode consolidates the 45-unit pipeline into ~14 execution units for faster turnaround. Each sprint phase invokes multiple subskills in a single agent context.

**When to use Sprint:**
- Bid decision already made (Go/No-Go skipped)
- Simpler RFPs with clear requirements
- Tight timelines (1–2 weeks)
- Routine renewals or follow-on contracts

**Sprint phase map:**

| Sprint Phase | Combines | SVA |
|-------------|----------|-----|
| S0: Intake | Phases 0, 1, 1.5, 1.6, 1.7, 1.8, 1.95 (skips 1.9 Go/No-Go) | SVA-S1 (rules from SVA-1 + SVA-2) |
| S2: Requirements | Phases 2a, 2, 2.5, 2b, 2c, 2d | — |
| S3: Specifications | Phases 3a, 3b, 3c, 3e, 3f, 3g | SVA-S2 (SVA-3 + SVA-4) |
| S4: Traceability | Phases 4, 5 | — |
| S5: Documentation | Phases 6, 6b, 6c | — |
| S6: QA | Phases 7, 7c, 7d | SVA-S3 (SVA-5 + SVA-6) |
| S7: Bid Generation | Phases 8.0, 8.1–8.6, 8.4r, 8.4k, 8f | — |
| S8: Assembly | Phases 8d, 8e | SVA-S4 (SVA-7 full rules) |

**Sprint SVA strategy:** Combined SVAs run the highest-numbered SVA in the combined set (e.g., SVA-S1 runs SVA-2 rules, which are more comprehensive than SVA-1 alone).

---

## 27 Expert Roles

Each phase is executed by a specialized agent with an assigned expert role. This isn't cosmetic — the role determines how the agent interprets ambiguous RFP language and what domain knowledge it applies.

| Role | Phases | Domain Expertise |
|------|--------|-----------------|
| DevOps Engineer | 0 | File systems, directory structures |
| Document Processing Specialist | 1 | PDF/DOCX/XLSX parsing, text extraction |
| Business Analyst | 1.5 | Industry classification, compliance frameworks |
| Procurement Specialist | 1.6 | RFP evaluation criteria, scoring methodologies |
| Compliance Officer | 1.7 | Regulatory requirements, mandatory items |
| Procurement Analyst | 1.8 | Submission requirements, volume structure |
| Bid Decision Analyst | 1.9 | Bid/no-bid analysis, opportunity qualification |
| Competitive Intelligence Analyst | 1.95 | Market research, incumbent analysis, FPDS |
| Business Process Analyst | 2a | Process flows, BPMN, workflow mapping |
| Requirements Engineer | 2, 2b | Requirements elicitation, deduplication |
| Data Analyst | 2.5 | Data profiling, sample data analysis |
| Technical Writer | 2c, 6, 6b | Documentation, cataloging, navigation |
| QA Engineer | 2d, 7 | Coverage analysis, gap detection |
| Solutions Architect | 3a | System design, cloud architecture |
| Integration Architect | 3b, 8.6 | APIs, data exchange, multi-vendor coordination |
| Security Architect | 3c | OWASP, encryption, HIPAA/FERPA compliance |
| UX Designer | 3e | User interfaces, accessibility, wireframes |
| Data Architect | 3f | Entity modeling, ERD, database design |
| Risk Analyst | 3g, 8.4k | Risk assessment, mitigation, registers |
| Requirements Traceability Engineer | 4 | RTM construction, entity linking |
| Project Estimator | 5 | Effort estimation, AI-adjusted ratios |
| Data Integration Architect | 6c | Data aggregation, context synthesis |
| UX Researcher | 7c | Evaluator personas, stakeholder analysis |
| Bid Strategist | 7d, 8.0 | Win probability, strategic positioning |
| Executive Communications Specialist | 8.1 | Cover letters, NOSE formula |
| Management Proposal Writer | 8.2 | Experience narratives, team qualifications |
| Senior Technical Proposal Writer | 8.3 | Methodology, QA processes, KPIs |
| Business Solution Architect | 8.4 | Solution design per work section |
| Requirements Analyst | 8.4r | Requirements response tables |
| Financial Analyst | 8.5 | Cost narratives, rate tables, TCO |
| Traceability Verification Engineer | 8f | RTM auditing, 14-query protocol |
| Visual Design Engineer | 8d | Mermaid diagrams, visual communication |
| Publication Specialist | 8e | PDF generation, typography, layout |
| Gold Team Reviewer | SVA-7 | Shipley Gold Team, win theme threading |

---

## Data Flow

```
RFP Documents (PDF/DOCX/XLSX)
    │
    ▼
STAGE 1: Document Intake
    Phase 0:    Folder structure
    Phase 1:    flattened/*.md (markdown conversions)
    Phase 1.5:  domain-context.json
    Phase 1.6:  EVALUATION_CRITERIA.json (with factor_id/subfactor_id)
    Phase 1.7:  COMPLIANCE_MATRIX.json (with rfp_source_id, mandatory_item_id)
    Phase 1.8:  SUBMISSION_STRUCTURE.json
    Phase 1.9:  GO_NOGO_DECISION.json
    Phase 1.95: CLIENT_INTELLIGENCE.json
    ├── SVA-1 validates ──→ sva1-intake.json
    │
    ▼
STAGE 2: Requirements Engineering
    Phase 2a:   workflow-extracted-reqs.json
    Phase 2:    requirements-raw.json (with source_ids[])
    Phase 2.5:  sample-data-analysis.json
    Phase 2b:   requirements-normalized.json (with requirement_id)
    Phase 2c:   REQUIREMENTS_CATALOG.md + .json
    Phase 2d:   workflow-coverage.json
    ├── SVA-2 (PINK TEAM) ──→ sva2-pink-team.json
    │
    ▼
STAGE 3: Specification Generation
    Phase 3a-3f (parallel): ARCHITECTURE.md, INTEROPERABILITY.md,
                            SECURITY_REQUIREMENTS.md, UI_SPECS.md,
                            ENTITY_DEFINITIONS.md (each with spec_id)
    Phase 3g:   REQUIREMENT_RISKS.md + .json (with risk_id, mitigation_id)
    ├── SVA-3 validates ──→ sva3-spec.json
    │
    ▼
STAGE 4: Traceability & Estimation
    Phase 4:    UNIFIED_RTM.json ◄── CENTRAL ARTIFACT (links all entities)
                TRACEABILITY.md
    Phase 5:    EFFORT_ESTIMATION.md
    ├── SVA-4 (RED TEAM) ──→ sva4-red-team.json
    │
    ▼
STAGE 5: Documentation
    Phase 6:    MANIFEST.md + EXECUTIVE_SUMMARY.md
    Phase 6b:   NAVIGATION_GUIDE.md
    Phase 6c:   bid-context-bundle.json ◄── AGGREGATED BRIEFING DOCUMENT
    ├── SVA-5 validates ──→ sva5-doc.json
    │
    ▼
STAGE 6: Quality Assurance
    Phase 7:    validation-results.json + GAP_ANALYSIS.md
    Phase 7c:   PERSONA_COVERAGE.json
    Phase 7d:   WIN_SCORECARD.json
    ├── SVA-6 (RED FINAL) ──→ sva6-pre-bid.json
    │
    ▼
STAGE 7: Bid Generation
    Phase 8.0:  POSITIONING_OUTPUT.json (themes, eval mapping, past projects)
    Phase 8.1:  01_SUBMITTAL.md
    Phase 8.2:  02_MANAGEMENT.md (with 3-5 case studies from Past_Projects.md)
    Phase 8.3:  03_TECHNICAL.md (with tech lifecycle validation)
    Phase 8.4:  04a_SOLUTION_*.md (one per work section)
    Phase 8.4r: 04_REQUIREMENTS_REVIEW.md
    Phase 8.4k: 04_RISK_REGISTER.md
    Phase 8.5:  05_FINANCIAL.md (with rate source attribution)
    Phase 8.6:  06_INTEGRATION.md
    Phase 8f:   RTM_REPORT.md (14-query verification)
    ├── SVA-7 (GOLD TEAM) ──→ sva7-gold-team.json + GOLD_TEAM_CHECKLIST.md
    │
    ▼
POST-SVA-7: Final Assembly
    Phase 8d:   architecture.png, timeline.png, orgchart.png
    Phase 8e:   ResourceData_*_VOLUME.pdf (×6) + Draft_Bid.pdf + EXECUTIVE_SUMMARY.pdf
```

---

## Output Structure

```
{rfp-folder}/
├── original/                          # Source RFP documents (moved here by Phase 0)
├── flattened/                         # Markdown conversions of source documents
├── shared/                            # Machine-readable data files
│   ├── progress.json                  # Pipeline progress tracking
│   ├── domain-context.json            # Industry classification
│   ├── EVALUATION_CRITERIA.json       # Scoring methodology
│   ├── COMPLIANCE_MATRIX.json         # Mandatory items
│   ├── SUBMISSION_STRUCTURE.json      # Volume structure
│   ├── GO_NOGO_DECISION.json          # Bid viability assessment
│   ├── workflow-extracted-reqs.json   # Workflow requirements
│   ├── requirements-raw.json          # Raw extracted requirements
│   ├── requirements-normalized.json   # Deduplicated requirements
│   ├── REQUIREMENTS_CATALOG.json      # Catalog data
│   ├── sample-data-analysis.json      # Sample data profile
│   ├── workflow-coverage.json         # Coverage validation
│   ├── REQUIREMENT_RISKS.json         # Risk assessments
│   ├── UNIFIED_RTM.json              # ◄ CENTRAL: Unified Requirements Traceability Matrix
│   ├── validation-results.json        # QA results
│   ├── PERSONA_COVERAGE.json          # Evaluator personas
│   ├── WIN_SCORECARD.json             # Win probability
│   ├── bid-context-bundle.json        # ◄ Aggregated context for bid authors
│   ├── bid/
│   │   ├── CLIENT_INTELLIGENCE.json   # Client research
│   │   └── POSITIONING_OUTPUT.json    # Strategic positioning
│   └── validation/                    # SVA reports
│       ├── sva1-intake.json
│       ├── sva2-pink-team.json
│       ├── sva3-spec.json
│       ├── sva4-red-team.json
│       ├── sva5-doc.json
│       ├── sva6-pre-bid.json
│       └── sva7-gold-team.json
├── outputs/                           # Human-readable documents
│   ├── EXECUTIVE_SUMMARY.md
│   ├── REQUIREMENTS_CATALOG.md
│   ├── ARCHITECTURE.md
│   ├── SECURITY_REQUIREMENTS.md
│   ├── INTEROPERABILITY.md
│   ├── UI_SPECS.md
│   ├── ENTITY_DEFINITIONS.md
│   ├── REQUIREMENT_RISKS.md
│   ├── TRACEABILITY.md
│   ├── EFFORT_ESTIMATION.md
│   ├── MANIFEST.md
│   ├── NAVIGATION_GUIDE.md
│   ├── GAP_ANALYSIS.md
│   ├── RTM_REPORT.md
│   ├── GOLD_TEAM_CHECKLIST.md
│   ├── bid-sections/                  # Bid volume markdown sources
│   │   ├── 01_SUBMITTAL.md
│   │   ├── 02_MANAGEMENT.md
│   │   ├── 03_TECHNICAL.md
│   │   ├── 04a_SOLUTION_*.md
│   │   ├── 04_REQUIREMENTS_REVIEW.md
│   │   ├── 04_RISK_REGISTER.md
│   │   ├── 05_FINANCIAL.md
│   │   └── 06_INTEGRATION.md
│   └── bid/                           # ◄ FINAL DELIVERABLES (PDFs + diagrams)
│       ├── Draft_Bid.pdf
│       ├── ResourceData_1_SUBMITTAL.pdf
│       ├── ResourceData_2_MANAGEMENT.pdf
│       ├── ResourceData_3_TECHNICAL.pdf
│       ├── ResourceData_4_SOLUTION.pdf
│       ├── ResourceData_5_FINANCIAL.pdf
│       ├── ResourceData_6_INTEGRATION.pdf
│       ├── EXECUTIVE_SUMMARY.pdf
│       ├── architecture.png
│       ├── timeline.png
│       └── orgchart.png
```

---

## Configuration Files

All configuration lives under `.claude/skills/process-rfp-win/`:

| File | Purpose |
|------|---------|
| `config-win/company-profile.json` | Company capabilities, services (DICT format), locations (city/state dicts), certifications, bid defaults, market rate defaults, override projects |
| `config-win/bid-outcomes.json` | Historical bid win/loss data — used by Phase 8.0 for pattern analysis when 3+ outcomes logged |
| `config-win/evidence-library.json` | 6 categories of reusable evidence for bid sections, tag-overlap matching, >=60% section coverage target |
| `config-win/sva-rules-registry.json` | All 44 validation rules across 7 SVAs, extensible via `custom_rules[]` |
| `config-win/integrations.json` | Optional external tool hooks (CRM, proposal DB, notifications) |
| `config-win/pdf-theme.css` | Professional PDF styling (fitz.Story-safe CSS) |
| `config-win/md-to-pdf.config.js` | Configuration for `npx md-to-pdf` fallback renderer |
| `config-win/mermaid-themes/*.json` | Themed Mermaid diagram configs (architecture, gantt, orgchart) |
| `config-win/case-study-template.md` | Formatting guide for case study output in bid sections |
| `config-win/pipeline-metrics.json` | Persistent metrics across pipeline runs (auto-updated) |
| `hooks-win/phase-verification.json` | Post-phase verification hooks |
| `hooks-win/theme-validation.json` | Win theme threading validation hooks |
| `schemas/sva-report.schema.json` | JSON Schema for SVA validation reports |
| `schemas/unified-rtm.schema.json` | JSON Schema for UNIFIED_RTM.json |

### Key Configuration Gotchas

- **`services` in `company-profile.json` is a DICT, not a list.** Flatten with: `[svc for cat in services.values() for svc in cat]`
- **`locations` are dicts with `city`/`state` keys**, not flat strings
- **Go/No-Go output field is `"recommendation"`**, not `"decision"`
- **Sprint mode SVA:** runs the highest SVA in the combined set: `max(int(r.split("-")[1]))`
- **Phase 0 moves documents** with `shutil.move`, not copy — originals are relocated to `original/`

---

## Orchestrator Architecture

The pipeline is coordinated by a **Mayor Orchestrator** (`skill-win.md`) that:

1. **Does NOT process documents directly** — delegates all work to specialized agents via the Task tool
2. **Tracks progress** in `shared/progress.json` with per-phase timing
3. **Enforces stage boundaries** — executing Stage N+1 when user requested Stage N is a protocol violation
4. **Retries on failure** — up to 3 attempts per phase with RED notification
5. **Enforces gates** — blocking gates halt the pipeline; advisory gates present options
6. **Runs SVAs** after each stage completes
7. **Attempts auto-correction** when SVA returns BLOCK with correctable findings
8. **Aggregates metrics** into `pipeline-metrics.json` at pipeline end
9. **Scans for `[USER INPUT REQUIRED]` markers** in bid sections and reports them

### Stage Execution Discipline

Users can request specific stages: "do Stage 5." The orchestrator executes ONLY the phases belonging to that stage. This prevents scope creep and allows incremental progress with human review between stages.

### Error Handling

| Scenario | Response |
|----------|----------|
| Phase fails | Retry up to 3x with RED notification |
| Blocking gate fails | Halt pipeline, present options (fix, approve gaps, abort) |
| SVA BLOCK | Attempt auto-correction for correctable findings; halt if unresolvable |
| SVA ADVISORY | Log warnings, continue |
| PDF generation fails | Pipeline returns `False` (mandatory) |
| All retries exhausted | Mark phase as failed, halt at blocking gates |

---

## Metrics & Organizational Learning

### Pipeline Metrics

`pipeline-metrics.json` accumulates data across runs:
- Per-run: phase durations, phase statuses, SVA dispositions, total duration, retry counts
- Aggregates: average duration, phase failure rates, SVA disposition distribution

### Post-Bid Learning (Phase 9 — Optional)

After a bid decision is received (win or loss), Phase 9 logs the outcome to `bid-outcomes.json`:
- Outcome (win/loss/no-decision)
- Domain, themes used, strengths cited, weaknesses cited
- Evaluator feedback if available

When 3+ outcomes accumulate, Phase 8.0 (Strategic Positioning) uses them to:
- Calculate domain-specific win rates
- Identify effective themes (themes in wins vs losses)
- Flag recurring weaknesses to avoid
- Advise on positioning strategy

This creates a **feedback loop** — each bid makes the next one more informed.

---

## Integration Hooks (Optional)

The pipeline supports optional external tool integrations via `config-win/integrations.json`. No integrations are required — the pipeline works fully standalone.

| Integration Type | Hook Phase | Purpose |
|-----------------|------------|---------|
| CRM | 1.95 | Enrich client intelligence with CRM data |
| Proposal Database | 8.0 | Search for reusable content blocks |
| Notifications | Global | Alert on pipeline events (complete, block, review needed) |

All integrations use environment variable references for API keys — never stored in config files. Integration hooks are **log-only checkpoints** by default; actual execution requires implementing an adapter script.

---

## Relationship to the Screening Pipeline

| Aspect | Screen Pipeline | Win Pipeline |
|--------|----------------|--------------|
| Purpose | Should we bid? | How do we win? |
| Time | 15–30 minutes | 3–4 hours |
| Phases | 7 | 45+ |
| Agents | None (inline) | Multi-agent dispatch |
| Grok review | Skipped | Mandatory |
| Validation gates | None | 7 SVAs with color teams |
| Past project scoring | Same algorithm | Same algorithm + deeper integration |
| Go/No-Go scoring | Same 5 dimensions | Same 5 dimensions |
| Output | Single PDF report | Full multi-volume bid package |
| Shared config | Reads `config-win/` | Owns `config-win/` |

A GO recommendation from `/process-rfp-screen` feeds directly into `/process-rfp-win` on the same folder. Screening outputs in `screen/` don't conflict with the win pipeline's `shared/` and `outputs/` directories.

---

## Security Considerations

Detailed in `SECURITY_AUDIT.md` (7 risk assessments, R1–R7):

- **Path sanitization** — prevents directory traversal in file operations
- **`.gitignore` generation** — automatically created in output folders to prevent accidental commit of sensitive bid data
- **No secrets in config** — API keys referenced by environment variable name only
- **Input validation** — combined text minimum thresholds prevent analysis on corrupted data
- **SVA integrity** — validation reports are machine-readable and auditable

---

## Quick Reference: File Size Minimums

The pipeline enforces minimum file sizes to catch empty or stub outputs:

| Output | Phase | Minimum |
|--------|-------|---------|
| `ARCHITECTURE.md` | 3a | 15 KB |
| `REQUIREMENTS_CATALOG.md` | 2c | 10 KB |
| `TRACEABILITY.md` | 4 | 10 KB |
| `UNIFIED_RTM.json` | 4 | 10 KB |
| `SECURITY_REQUIREMENTS.md` | 3c | 8 KB |
| `EFFORT_ESTIMATION.md` | 5 | 8 KB |
| `02_MANAGEMENT.md` | 8.2 | 8 KB |
| `03_TECHNICAL.md` | 8.3 | 15 KB |
| `INTEROPERABILITY.md` | 3b | 5 KB |
| `Draft_Bid.pdf` | 8e | 100 KB |
| Individual volume PDFs | 8e | 10–30 KB |
