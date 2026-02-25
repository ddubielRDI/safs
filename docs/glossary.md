# SAFS Pipeline Glossary

A centralized reference for terminology used across the `/process-rfp-screen` and `/process-rfp-win` pipelines. Organized by category for quick lookup.

---

## 1. Core Decision Terms

| Term | Definition |
|------|-----------|
| **GO** | Recommendation to proceed with full proposal development. Screen pipeline: score >= 50/100. Win pipeline: Phase 1.9 score >= 50/100. |
| **CONDITIONAL** | Recommendation to proceed only after resolving specific risks. Score 40–49/100. Requires human review of flagged conditions before committing resources. |
| **NO_GO** | Recommendation against bidding. Score < 40/100. Override available if strategic reasons justify investment despite low score. |
| **Dealbreaker** | A high-severity risk in compliance or scoring categories that could disqualify the proposal. Surfaced prominently in reports but does not automatically change the recommendation — human judgment decides. |
| **Override** | Manual bypass of an algorithmic recommendation. Company profile can specify `override_projects[]` to force-include past projects; users can override NO_GO decisions for strategic reasons. |
| **Advisory Gate** | A decision point that provides a recommendation but does not halt the pipeline. Phase 1.9 (Go/No-Go) is advisory — the user can proceed despite a NO_GO score. |
| **Blocking Gate** | A decision point that halts the pipeline on failure. Phase 1.7 (Compliance Matrix) is blocking — incomplete compliance extraction must be resolved before continuing. |

---

## 2. Scoring Methodology

The Go/No-Go framework scores opportunities across 7 weighted assessment areas using LLM narrative analysis with cited evidence. Each area is scored 0-100; the final score is a weighted sum. Used identically in both pipelines.

| Area | Weight | What It Measures |
|------|--------|-----------------|
| **Strategic Fit** | 15% | Geographic proximity (with state adjacency), industry alignment, existing state contracts, revenue significance, repeat client potential. Parallels APMP's "Strategic Fit" + Shipley's "Customer Relationship." |
| **Technical Capability** | 25% | Service alignment, domain expertise, past project evidence, technology stack overlap, mandatory requirement coverage. Parallels Shipley's "Solution Alignment" + "Technical Capability." |
| **Competitive Position** | 20% | Evaluation criteria alignment, advantage/disadvantage signals, competition level, preference points, COTS positioning. Parallels Shipley's "Competitive Assessment" + APMP's "Win Probability." Includes competition headwind detection (prior proposal counts, preference point ineligibility, COTS/platform bias). |
| **Resource Availability** | 15% | Staffing capacity, personnel requirements, certification gaps, proposal volume complexity, timeline feasibility. Parallels Shipley's "Resource Availability" + "Key Personnel" + "Proposal Preparation Time." |
| **Financial Viability** | 10% | Contract value vs company size, cost formula impact, budget constraints, payment terms, indirect cost limits. |
| **Risk Assessment** | 10% | Technical complexity, compliance obligations, political risk, integration burden, data sensitivity. Higher score = lower risk. |
| **Win Probability** | 5% | Estimated win chance, structural advantages/disadvantages, teaming opportunities. Synthesizes all other areas. |
| **Match Quality** | — | Aggregate assessment of past project alignment: "strong" (top project > 15 pts), "moderate" (> 8 pts), or "weak" (≤ 8 pts). Assessed in Phase 4 (screen) / Phase 8.0 (win), not in Go/No-Go. |

**Evidence mandate:** Every score must cite specific evidence from the inputs (RFP text, company profile, past projects). Unsupported claims are prohibited. Gaps in evidence must be explicitly noted and scored conservatively.

**Scoring formula:** `overall_score = round(sum(area_score × area_weight))`

**Thresholds:** GO ≥ 50 | CONDITIONAL 40–49 | NO_GO < 40

---

## 3. Industry Frameworks

| Term | Definition |
|------|-----------|
| **Shipley Lifecycle** | The Shipley Business Development Lifecycle — the industry-standard framework for structured proposal development. Defines phases from opportunity identification through post-submission review. The win pipeline maps its 7 stages to Shipley phases. |
| **APMP** | Association of Proposal Management Professionals. Publishes the Body of Knowledge (BoK) for proposal development best practices. Their Practitioner/Professional certifications define competency standards. |
| **Win Theme** | A concise, evaluator-facing message that connects a company strength to a specific client need. Full pipeline uses CVD format (Capability → Value → Differentiator). Screen pipeline generates preliminary themes as directional hints. |
| **Customer Intimacy** | APMP's principle that knowing the client's priorities, pain points, and decision-makers is the #1 predictor of win probability. Drives Phases 3 (screen) and 1.95 (win) intelligence gathering. |
| **Black Hat Review** | Analyzing the opportunity from the competition's perspective — what would they bid, what are their strengths? Partially captured in competitive position scoring and evaluator persona generation. |
| **Pink Team Review** | Color team review at ~25% completion. Checks that requirements capture is complete before specification work begins. Automated as SVA-2 in the win pipeline. |
| **Red Team Review** | Color team review at ~55% and ~80% completion. Evaluates proposal quality, compliance, and win strategy effectiveness. Automated as SVA-4 and SVA-6 in the win pipeline. |
| **Gold Team Review** | Final review at ~95% completion before submission. Validates the complete proposal against 13 rules. Automated as SVA-7, generates `GOLD_TEAM_CHECKLIST.md` for human review. Uses a soft pause (PASS/ADVISORY/BLOCK). |
| **Compliance Matrix** | Industry-standard artifact mapping every RFP requirement to a proposal response location. Created in Phase 1.7 (win) and simplified as a quick-check in Phase 4 (screen). |
| **Capture Management** | The overall process of identifying, qualifying, and pursuing a specific business opportunity from initial identification through proposal submission. |
| **Best Value** | Evaluation methodology where technical merit, past performance, and other factors are weighted alongside price. Favors proposals demonstrating superior capability. Contrast with LPTA. |
| **LPTA** | Lowest Price Technically Acceptable. Evaluation methodology where any proposal meeting minimum technical requirements competes solely on price. Changes bid strategy fundamentally — capability differentiation matters less. |

---

## 4. RFP & Procurement Terms

| Term | Definition |
|------|-----------|
| **FFP** | Firm Fixed Price. Contract type where the price is agreed upon upfront and doesn't change regardless of actual costs. Shifts cost risk to the contractor. |
| **T&M** | Time and Materials. Contract type where the client pays for labor hours at agreed rates plus materials. Cost risk shared between client and contractor. |
| **IDIQ** | Indefinite Delivery/Indefinite Quantity. Framework contract establishing terms for future task orders. Actual work is ordered via individual task orders against the IDIQ ceiling. |
| **CPFF** | Cost Plus Fixed Fee. Contract type where the client reimburses all allowable costs plus a predetermined fee. Cost risk lies primarily with the client. |
| **BPA** | Blanket Purchase Agreement. Simplified method for recurring purchases. Establishes pricing and terms; individual orders placed against the BPA as needs arise. |
| **Set-Aside** | A procurement restriction limiting competition to specific business categories. An immediate eligibility filter — if restricted to a category the company doesn't hold, it's a hard stop. |
| **8(a)** | SBA program for small disadvantaged businesses. Participation requires SBA certification. 8(a) set-asides restrict competition to certified firms. |
| **HUBZone** | Historically Underutilized Business Zone. SBA program for businesses in designated economically distressed areas. |
| **SDVOSB** | Service-Disabled Veteran-Owned Small Business. Federal set-aside category for veteran-owned firms with service-connected disabilities. |
| **Incumbent** | The current contract holder. Incumbents have relationship advantages, institutional knowledge, and transition cost leverage. Detected during Phase 3/1.95 intelligence gathering. |
| **Past Performance** | Documented track record of successfully completing similar work. A formal evaluation factor in most government procurements. The pipeline maintains 28 case studies in `Past_Projects.md`. |
| **Evaluation Criteria** | The factors and weights an evaluation panel uses to score proposals. Extracted in Phase 1 (screen) and Phase 1.6 (win). Determines proposal emphasis and win strategy. |
| **Period of Performance** | The contract's start-to-end timeframe. Affects staffing plans, pricing, and technology lifecycle planning. |
| **Preference Points** | Additional scoring points awarded to firms meeting specific criteria (e.g., in-state business, minority-owned). Can be decisive in close competitions. |

---

## 5. Pipeline Architecture Terms

| Term | Definition |
|------|-----------|
| **SVA** | Stage Validation Agent. Automated quality gates in the win pipeline that enforce integrity at stage boundaries. 7 SVAs map to Shipley's color team reviews. SVA-7 (Gold Team) has 13 validation rules. |
| **Sprint Mode** | Win pipeline flag (`--sprint`) that pre-approves the bid decision, skips Go/No-Go, and combines SVAs into 4 sprint gates (S1=1+2, S2=3+4, S3=5+6, S4=7). Reduces runtime to ~1.5–2 hours. Runs highest SVA in combined set: `max(int(r.split("-")[1]))`. |
| **CVD Format** | Capability → Value → Differentiator. An industry-standard proposal writing framework consistent with Shipley/APMP principles (not attributable to a single source; Shipley's native framework is FBPD — Features-Benefits-Proofs-Discriminators). The standard theme format in the win pipeline: what we can do → why it matters to the client → why we're uniquely positioned. |
| **NOSE Formula** | Need → Outcome → Solution → Evidence. Structuring framework for proposal narrative sections. |
| **Evidence Library** | `evidence-library.json` — catalog of reusable proof points organized in 6 categories. Tag-overlap matching ensures >=60% section coverage when populating bid volumes. |
| **RTM** | Requirements Traceability Matrix. Links every entity from RFP source text through requirements, specifications, risks, and bid sections into a single auditable chain. The pipeline's central integrity artifact. |
| **Context Bundle** | Phase 6c output — a compiled package of all upstream data (requirements, specifications, themes, evidence) that bid authoring phases consume. Prevents each authoring phase from re-reading dozens of files. |
| **Theme-Eval Mapping** | Links win themes to specific evaluation factors. Ensures every scored factor has at least one supporting theme. Prevents orphaned themes that don't influence scoring. |
| **Section-Theme Mandates** | Rules requiring specific bid sections to include specific themes. Ensures theme consistency across the proposal — the executive summary's themes must appear in technical sections. |
| **Evaluator Personas** | Phase 7c constructs fictional evaluator profiles (technical reviewer, contracting officer, end user) to simulate how proposals will be read and scored. Informs tone and emphasis in bid authoring. |
| **Pipeline Metrics** | `pipeline-metrics.json` — persistent performance data across runs. Tracks phase timing, output sizes, error rates. `aggregate_metrics()` runs at pipeline end. |
| **Auto-Correction** | SVA behavior where detected issues are automatically fixed rather than just flagged. Applied when corrections are deterministic (e.g., missing cross-references, format normalization). |
| **Quick Mode** | Screen pipeline flag (`--quick`) that skips Phase 3 (client intelligence), saving 4–6 minutes. Appropriate when the team already has strong client familiarity. |
| **Scan Limit** | Maximum characters of RFP text analyzed. Screen pipeline: 80,000 chars. Prevents context window exhaustion while retaining critical content (scope, evaluation criteria, requirements). |
| **Preliminary Win Themes** | Directional positioning hints generated during screening (Phase 4.5). Rule-based, not evaluator-aligned. Intended to give the team messaging direction if the screening result is GO, before the full pipeline runs. |

---

## 6. Bid Authoring Terms

| Term | Definition |
|------|-----------|
| **Positioning** | Strategic framing of the company's strengths relative to the opportunity and competition. Phase 8.0 (win) generates production positioning; Phase 4.5 (screen) generates preliminary hints. |
| **Content Priority Order** | The sequence in which bid sections are authored, prioritized by evaluation weight. Higher-weighted sections are written first to receive the most context window attention. |
| **Evaluator Messages** | Key points embedded in bid text that directly address evaluation criteria. Each message targets a specific scorer's concerns. |
| **Eval Factor Callout Box** | A formatted block within bid sections that explicitly maps content to the evaluation factor it addresses. Makes it easy for evaluators to find responsive content. |
| **Case Study** | A structured past performance narrative following a template (`config-win/case-study-template.md`). Includes client, challenge, approach, results with metrics, and relevance statement. The pipeline selects and tailors case studies per bid section. |

---

## 7. Data Artifacts

All pipeline outputs organized by source phase.

### Screen Pipeline (`{rfp-folder}/screen/`)

| Artifact | Phase | Format | Description |
|----------|-------|--------|-------------|
| `source-manifest.json` | 0 | JSON | Document inventory with conversion status and character counts |
| `rfp-summary.json` | 1 | JSON | Extracted RFP metadata (20+ fields) |
| `go-nogo-score.json` | 2 | JSON | 7-area weighted scoring with narrative rationale, evidence, and risks |
| `client-intel-snapshot.json` | 3 | JSON | Web research results (skipped with --quick) |
| `compliance-check.json` | 4 | JSON | Requirement-by-requirement PASS/GAP/RISK |
| `past-projects-match.json` | 4 | JSON | Top 5 projects with relevance scores and breakdowns |
| `preliminary-themes.json` | 4.5 | JSON | 3–4 preliminary win themes with scores and evidence |
| `risk-assessment.json` | 5 | JSON | Consolidated risks, opportunities, dealbreakers |
| `BID_SCREEN.json` | 5 | JSON | All phase data consolidated (machine-readable) |
| `BID_SCREEN.md` | 6 | MD | Full report in markdown |
| `BID_SCREEN.pdf` | 6 | PDF | Professional PDF report (6–8 pages) |

### Win Pipeline (`{rfp-folder}/shared/` and `{rfp-folder}/outputs/`)

| Artifact | Phase | Format | Description |
|----------|-------|--------|-------------|
| `domain-context.json` | 1.5 | JSON | Industry classification and compliance frameworks |
| `EVALUATION_CRITERIA.json` | 1.6 | JSON | Extracted evaluation factors with weights and stable IDs |
| `COMPLIANCE_MATRIX.json` | 1.7 | JSON | Mandatory requirements mapped to response locations |
| `SUBMISSION_STRUCTURE.json` | 1.8 | JSON | Volume structure, naming conventions, page limits |
| `GO_NOGO_DECISION.json` | 1.9 | JSON | Go/No-Go scoring (same 7-area weighted model as screen) |
| `CLIENT_INTELLIGENCE.json` | 1.95 | JSON | Client research with up to 15 web searches |
| `POSITIONING_OUTPUT.json` | 8.0 | JSON | Production win themes with eval factor mapping |
| `outputs/bid/*.pdf` | 8e | PDF | Final bid volumes (6 named volumes + consolidated) |
| `GOLD_TEAM_CHECKLIST.md` | SVA-7 | MD | Final review checklist for human sign-off |
| `bid-outcomes.json` | 9 | JSON | Post-bid outcome logging for pattern analysis |
| `pipeline-metrics.json` | Post | JSON | Aggregate performance metrics |

---

## 8. Configuration Terms

| Config File | Purpose | Key Gotchas |
|-------------|---------|------------|
| **`company-profile.json`** | Company capabilities, services, locations, certifications, bid defaults. Shared by both pipelines. | `services` is a **DICT** (categories → service lists), not a flat list. Flatten with: `[svc for cat in services.values() for svc in cat]`. `locations` are dicts with `city`/`state` keys, not flat strings. |
| **`bid-outcomes.json`** | Historical bid win/loss data. Phase 9 logs outcomes; Phase 8.0 and Phase 5 (screen) use 3+ outcomes for pattern analysis. | Optional — pipelines function without it but lose historical calibration. |
| **`evidence-library.json`** | Reusable proof points in 6 categories. Tag-overlap matching during bid authoring. | Requires >=60% section coverage when populating bid volumes. |
| **`sva-rules-registry.json`** | Validation rule definitions for all 7 SVAs. | SVA-7 has 13 rules including `SVA7-TECH-LIFECYCLE-VALIDATION` for expiring technology detection. |
| **`pipeline-metrics.json`** | Persistent performance data across pipeline runs. | Accumulates across runs — `aggregate_metrics()` runs at pipeline end. |
| **`integrations.json`** | External service connections and API configurations. | Used for MCP server coordination and external tool integration. |

---

## Cross-References

- **Screen Pipeline Design:** [docs/process-rfp-screen.md](process-rfp-screen.md)
- **Win Pipeline Design:** [docs/process-rfp-win.md](process-rfp-win.md)
- **Screen Pipeline Skill:** `.claude/skills/process-rfp-screen/skill-screen.md`
- **Win Pipeline Skill:** `.claude/skills/process-rfp-win/skill-win.md`
- **Security Audit:** `.claude/skills/process-rfp-win/SECURITY_AUDIT.md`
- **Visual Guide:** `.claude/skills/process-rfp-win/PIPELINE_VISUAL_GUIDE.md`
- **Gold Standard Reference:** [docs/process-gold-standard.md](process-gold-standard.md) — authoritative source validation for all framework claims
