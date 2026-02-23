# RFP Screening Pipeline — Design Rationale & Technical Reference

## Overview

`/process-rfp-screen` is a lightweight bid/no-bid screening pipeline that evaluates an RFP across 6 phases and produces a single `BID_SCREEN.pdf` for human decision-making. It runs in 15–30 minutes — a fraction of the 3–4 hours required by the full `/process-rfp-win` pipeline.

**Primary deliverable:** `{rfp-folder}/screen/BID_SCREEN.pdf` (6–8 pages)

```
/process-rfp-screen <path-to-rfp-folder> [--quick]
```

---

## Why a Screening Pipeline Exists

Industry data consistently shows that **50–70% of proposals submitted are non-competitive** — they consume resources but have negligible win probability. The Shipley Business Development Lifecycle, APMP Body of Knowledge, and Lohfeld Consulting all recommend formal gate reviews before committing to full proposal development.

The screening pipeline addresses the most expensive mistake in business development: investing 80–200 labor-hours in a proposal that should never have been pursued. By front-loading analysis into a 15–30 minute automated assessment, the team gets structured data to make that call before committing.

---

## Phase Architecture at a Glance

| Phase | Name | Gold Standard Parallel | Time |
|-------|------|----------------------|------|
| 0 | Document Intake | — (operational prerequisite) | 1–3 min |
| 1 | RFP Summary Extraction | Shipley "Opportunity Assessment" | 3–5 min |
| 2 | Go/No-Go Scoring | Shipley "Bid/No-Bid Decision Gate" | 2–4 min |
| 3 | Client Intelligence | APMP "Customer Intimacy" | 4–6 min |
| 4 | Compliance & Project Match | Shipley "Compliance Matrix" + "Past Performance" | 3–5 min |
| 5 | Risk Assessment & Recommendation | APMP "Risk-Adjusted Opportunity Evaluation" | 1–2 min |
| 6 | PDF Report | — (deliverable generation) | 1–2 min |

---

## Phase-by-Phase Design Rationale

### Phase 0: Document Intake

**What it does:** Validates the RFP folder, discovers source documents (PDF, DOCX, XLSX), converts them to markdown, and assembles combined text for downstream analysis.

**Why it exists:** Every subsequent phase depends on having clean, parseable text. RFPs arrive in inconsistent formats — sometimes a single 200-page PDF, sometimes a bundle of attachments across formats. This phase normalizes everything into a single text corpus.

**Key technical decisions:**
- **80,000 character scan limit** — prevents context window exhaustion on massive RFPs while retaining enough text for accurate analysis. Most RFPs have their critical content (scope, evaluation criteria, requirements) in the first 60–80K characters.
- **Priority sorting** — largest PDF first (likely the main RFP body), then remaining PDFs, then other formats. This ensures the most important content survives truncation.
- **Conversion chain** — `markitdown` primary, with `python-docx` and `openpyxl` fallbacks. Triple redundancy because document conversion is the most failure-prone step.
- **500 character minimum** — hard abort if combined text is too thin to analyze meaningfully.

**Output:** `screen/source-manifest.json` — per-document conversion status, character counts, methods used.

---

### Phase 1: RFP Summary Extraction

**What it does:** Single-pass metadata extraction — client name, solicitation number, title, deadline, contract value/type, scope keywords, industry domain, evaluation criteria, set-asides, mandatory requirements, and key deliverables.

**Gold standard parallel:** Shipley's **Opportunity Assessment** phase focuses on answering "What is this opportunity?" before analyzing whether to pursue it. APMP's Practitioner certification requires demonstrating the ability to extract and classify opportunity metadata from solicitation documents.

**Why these specific fields were selected:**

| Field | Why It Matters |
|-------|---------------|
| Client name | Required for Phase 3 intelligence; drives repeat-client detection |
| RFP number/title | Identification and tracking across the organization |
| Submission deadline | Single most important feasibility constraint |
| Estimated value | Revenue potential scoring; resource allocation decisions |
| Contract type | FFP vs T&M vs IDIQ fundamentally changes bid strategy and risk profile |
| Scope keywords | Technology/service matching against company capabilities |
| Industry domain | Past performance relevance; regulatory awareness |
| Evaluation criteria | Determines proposal emphasis and win strategy |
| Set-aside | Immediate eligibility filter — if restricted to 8(a) and you're not 8(a), it's a hard stop |
| Mandatory requirements | Compliance gap detection |
| Key deliverables | Scope sizing and staffing estimation |

**What industry standards include that we deliberately omit:**
- **Detailed work breakdown structure** — belongs in full pipeline (Phase 2a workflow analysis)
- **Pricing strategy** — premature at screening; requires full technical understanding
- **Teaming partner analysis** — only relevant if GO decision is made
- **Oral presentation requirements** — tactical detail for proposal development, not screening

**Extraction approach:** Regex pattern matching with layered fallbacks (3–5 patterns per field, header analysis as last resort). Confidence scoring based on critical field coverage.

**Output:** `screen/rfp-summary.json`

---

### Phase 2: Go/No-Go Scoring

**What it does:** Scores the opportunity across 5 dimensions (0–20 each, total 0–100) and recommends GO (>=50), CONDITIONAL (40–49), or NO-GO (<40).

**Gold standard parallel:** This is the core of Shipley's **Bid/No-Bid Decision Gate** — the formal checkpoint where organizations decide whether to invest in proposal development. Every major BD methodology (Shipley, APMP, Lohfeld, KSI) includes a structured scoring gate at this point.

**Why 5 dimensions, and why these 5:**

Industry bid/no-bid frameworks typically evaluate 8–15 factors. We consolidated to 5 because screening needs speed, not granularity. Each dimension maps to a cluster of industry-standard factors:

#### Dimension 1: Capability Match (0–20)

**Industry parallel:** Shipley's "Solution Alignment" + "Technical Capability" factors.

Assesses how well company services align with RFP scope. Flattens the company service catalog, matches against scope keywords and mandatory requirements, and checks industry alignment.

**Scoring rubric:**
- 20: >80% keyword match AND industry alignment
- 15: >60% match AND industry alignment
- 12: >60% match OR industry alignment
- 10: >40% match
- 5: >20% match
- 0: <20% match

#### Dimension 2: Competitive Position (0–20)

**Industry parallel:** Shipley's "Competitive Assessment" + APMP's "Win Probability" analysis.

Goes beyond simple capability matching to assess *relative* positioning. Detects:
- **Advantage signals** in RFP language (innovation emphasis, best value evaluation, digital transformation)
- **Disadvantage signals** (incumbent references, sole source language, restrictive set-asides)
- **Competition headwinds** — prior proposal counts, preference point ineligibility, COTS/platform bias

This dimension is where most simplistic screening tools fail. They check "can we do this?" but not "can we win this?" The competition headwind detection (preference points, COTS positioning, prior competition volume) was added after observing real bid outcomes where capability-aligned proposals still lost due to structural disadvantages.

#### Dimension 3: Resource Availability (0–20)

**Industry parallel:** Shipley's "Resource Availability" + "Key Personnel" assessment.

Starts from a company-size baseline (larger bench = higher starting score), then adjusts for:
- **Certification gaps** — RFP requires certs the company doesn't document
- **Personnel complexity** — many distinct role references = complex staffing demand

Placeholder certification data (common in early-stage company profiles) is treated as moderate risk rather than zero capability — a deliberate calibration to avoid false NO-GO signals from incomplete profile data.

#### Dimension 4: Timeline Feasibility (0–20)

**Industry parallel:** Shipley's "Proposal Preparation Time" assessment.

Evaluates whether the submission deadline allows adequate proposal preparation. A detected deadline scores 15 (with manual verification advisory); no detected deadline scores 10 (unknown = risk).

Deliberately simpler than the full pipeline's timeline analysis (Phase 8c) because screening doesn't need to build a detailed schedule — it just needs to flag obvious time constraints.

#### Dimension 5: Strategic Alignment (0–20)

**Industry parallel:** APMP's "Strategic Fit" + Shipley's "Customer Relationship" factors.

The most organization-specific dimension. Evaluates:
- **Geographic proximity** — including state adjacency mapping (e.g., Oregon office counts for Washington RFPs)
- **State contract overlap** — existing contracts in the RFP's jurisdiction
- **Growth industry alignment** — does this RFP match the company's target markets?
- **Revenue potential** — is the value disclosed and significant?
- **Repeat client** — past performance with this specific client

The state adjacency mapping is a non-obvious feature worth noting. Federal BD frameworks often ignore geography, but state/local procurements heavily favor regional presence. The adjacency map (e.g., OR neighbors WA/ID/CA/NV) captures proximity advantage without requiring exact city-level matching.

**Output:** `screen/go-nogo-score.json` — full dimensional breakdown with rationale, evidence, risk/opportunity factors.

---

### Phase 3: Client Intelligence

**What it does:** Web research (max 8 queries) across 5 categories: recent news, leadership, technology stack, strategic initiatives, and incumbent/competitor identification.

**Gold standard parallel:** APMP's **Customer Intimacy** principle — the #1 predictor of win probability according to Lohfeld Consulting's win rate studies. Shipley's "Customer Focus" phases emphasize knowing the customer's priorities, decision-makers, and competitive landscape *before* proposal development.

**Why this matters for screening (not just proposal writing):**

Client intelligence transforms a screening decision from "Can we do this work?" to "Can we win this specific client?" Key signals:
- **Incumbent identification** — if the incumbent is a major integrator with 10 years of relationship, that's a competitive position factor
- **Technology stack** — if the client runs technologies the company specializes in, that's a capability signal
- **Strategic initiatives** — if the client is mid-modernization, proposals emphasizing innovation score higher
- **Leadership** — knowing the CIO's priorities helps assess whether the company's strengths align with decision-maker values

**Budget constraint:** 8 web searches maximum (vs. 15 in the full pipeline's Phase 1.95). Categories are prioritized: news (2), leadership (1), technology (2), strategy (1), competitors (2). This order reflects diminishing screening value — news and competitors matter most for a GO/NO-GO call; detailed tech stack matters more for proposal strategy.

**Skip condition:** `--quick` flag bypasses this phase entirely (~4–6 min saved). Quick mode is appropriate when the team already has strong client familiarity.

**Output:** `screen/client-intel-snapshot.json`

---

### Phase 4: Compliance & Past Project Match

**What it does:** Two-part analysis: (A) compliance gap detection against company profile, and (B) past project scoring and ranking.

**Gold standard parallel:** Two distinct Shipley processes combined for screening efficiency:
1. **Compliance Matrix** — the industry-standard artifact that maps every RFP requirement to a proposal response
2. **Past Performance Volume** — Shipley's and APMP's guidance on selecting and presenting relevant experience

#### Part A: Compliance Quick-Check

Extracts compliance items from RFP text (mandatory qualifications, certifications, insurance/bonding, set-asides) and cross-references against the company profile. Each item gets a status:

| Status | Meaning |
|--------|---------|
| PASS | Company profile demonstrates this capability |
| GAP | No matching capability found — potential disqualifier |
| RISK | Cannot verify automatically (insurance, bonding) — needs manual confirmation |

**Why this matters at screening:** A single hard compliance gap (e.g., "must be 8(a) certified" when the company isn't) makes the entire opportunity non-viable. Catching this in 3 minutes rather than 3 hours is the screening pipeline's primary value proposition.

#### Part B: Past Project Matching

Parses the `Past_Projects.md` case study library (28 projects) and scores each against the current RFP using a weighted algorithm:

| Factor | Points | Rationale |
|--------|--------|-----------|
| Industry match | 10 (exact) / 5 (related) | Evaluators weight domain experience heavily |
| Technology overlap | 3 per match, max 15 | Technical relevance is the strongest differentiator |
| Metrics present | 5 | Quantified results dramatically improve past performance scores |
| Client quote | 2 | Testimonials add credibility to past performance volumes |
| Recency | 1–5 | Recent projects (2024–2026) score highest; older projects depreciate |
| Scale | 0–3 | Enterprise/statewide projects demonstrate capacity |

**Related industry mapping** is a deliberate feature — "Government" and "Education" are treated as related domains because evaluators in one frequently value experience in the other. This prevents false-weak signals when the company has strong experience in adjacent sectors.

**Override mechanism:** Company profile can specify `override_projects[]` to force-include specific projects regardless of algorithm score — useful when the team knows a project is strategically important for a particular client.

**Outputs:** `screen/compliance-check.json`, `screen/past-projects-match.json`

---

### Phase 5: Risk Assessment & Recommendation

**What it does:** Consolidates risks from all prior phases, checks historical bid patterns, identifies opportunities, and produces the final recommendation with rationale and next steps.

**Gold standard parallel:** APMP's **Risk-Adjusted Opportunity Evaluation** — the synthesis step that integrates all assessment dimensions into a single decision framework. Shipley's "Black Hat Review" concept (understanding the competition's perspective) is partially captured in the competitive position and incumbent analysis.

**Risk consolidation sources:**
- Go/No-Go scoring gaps (Phase 2)
- Compliance gaps and risks (Phase 4a)
- Weak past project alignment (Phase 4b)
- Timeline concerns (Phase 2, dimension 4)

**Dealbreaker detection:** High-severity risks in compliance or scoring categories are flagged as potential dealbreakers. A dealbreaker doesn't automatically change the recommendation — it surfaces prominently in the report for human review.

**Historical pattern analysis:** If `bid-outcomes.json` has 3+ logged outcomes, the pipeline calculates overall and domain-specific win rates. This is the beginning of organizational learning — over time, the screening pipeline gets calibrated by actual results.

**Recommendation tiers:**

| Tier | Score | Action |
|------|-------|--------|
| GO | >= 50 | Proceed to `/process-rfp-win` |
| CONDITIONAL | 40–49 | Review risks; resolve conditions before committing |
| NO-GO | < 40 | Do not bid (override available for strategic reasons) |

**Output:** `screen/risk-assessment.json`, `screen/BID_SCREEN.json` (consolidated data from all phases)

---

### Phase 6: PDF Generation

**What it does:** Renders the consolidated `BID_SCREEN.json` into a professional 6–8 page PDF report with 7 sections.

**Report sections:**
1. Recommendation banner (GO/CONDITIONAL/NO-GO with score)
2. Quick Facts table (client, deadline, value, contract type, domain)
3. Go/No-Go Scorecard (5 dimensions with scores and rationale)
4. Client Intelligence (conditional — omitted in `--quick` mode)
5. Compliance Quick-Check (requirement-by-requirement status)
6. Past Project Matches (top 5 ranked with score breakdowns)
7. Risks, Opportunities, and Final Recommendation

**Technical constraint:** Uses `markdown_pdf` (PyMuPDF `fitz.Story` renderer) which only supports an HTML4/CSS2 subset. CSS `border` properties and `background-color` on block elements are prohibited — they cause rendering artifacts (ghost fills that leak across pages). Elements are visually distinguished via font weight, color, and spacing instead.

**Output:** `screen/BID_SCREEN.md`, `screen/BID_SCREEN.pdf`

---

## What Industry Standards Include That We Deliberately Defer

These elements are part of comprehensive BD frameworks but are omitted from screening because they require full proposal-level analysis:

| Element | Industry Standard | Where It Lives in Full Pipeline |
|---------|------------------|-------------------------------|
| Detailed WBS/SOW analysis | Shipley Phase B | Phase 2a (Workflow Analysis) |
| Pricing/cost strategy | APMP Pricing Volume | Phase 8.5 (Financial) |
| Teaming/subcontracting plan | Shipley Capture Plan | Phase 8.6 (Integration) |
| Technical approach | Shipley Phase C | Phases 8.1–8.4 (Proposal Authoring) |
| Oral presentation prep | APMP Orals | Not in current pipeline |
| Formal compliance matrix | Shipley Compliance Matrix | Phase 1.7 (Compliance) |
| Evaluation criteria alignment | APMP Win Theme Development | Phase 1.6 (Evaluation) |
| Solution architecture | Shipley Technical Volume | Phase 3a (Architecture) |

---

## Data Flow

```
RFP Documents (PDF/DOCX/XLSX)
    |
    v
Phase 0: Intake ──> source-manifest.json + combined_text (in memory)
    |
    v
Phase 1: Summary ──> rfp-summary.json
    |
    v
Phase 2: Go/No-Go ──> go-nogo-score.json
    |
    v
Phase 3: Intel ──> client-intel-snapshot.json  [skipped with --quick]
    |
    v
Phase 4: Compliance ──> compliance-check.json + past-projects-match.json
    |
    v
Phase 5: Risks ──> risk-assessment.json + BID_SCREEN.json (consolidated)
    |
    v
Phase 6: PDF ──> BID_SCREEN.md + BID_SCREEN.pdf
```

**Shared inputs (not phase outputs):**
- `config-win/company-profile.json` — company capabilities, services, locations, certifications
- `Past_Projects.md` — 28 case studies at repo root
- `config-win/bid-outcomes.json` — historical bid win/loss data (optional)

---

## Output Files

All outputs are written to `{rfp-folder}/screen/`. This isolation is deliberate — the full `/process-rfp-win` pipeline writes to `shared/` and `outputs/`, so screening never conflicts with full proposal development.

| File | Phase | Size | Description |
|------|-------|------|-------------|
| `source-manifest.json` | 0 | ~1 KB | Document inventory with conversion status |
| `rfp-summary.json` | 1 | ~2 KB | Extracted RFP metadata (20+ fields) |
| `go-nogo-score.json` | 2 | ~3 KB | 5-dimension scoring with rationale and evidence |
| `client-intel-snapshot.json` | 3 | ~2 KB | Web research results (skipped with --quick) |
| `compliance-check.json` | 4 | ~2 KB | Requirement-by-requirement PASS/GAP/RISK |
| `past-projects-match.json` | 4 | ~3 KB | Top 5 projects with relevance scores |
| `risk-assessment.json` | 5 | ~2 KB | Consolidated risks, opportunities, dealbreakers |
| `BID_SCREEN.json` | 5 | ~10 KB | All phase data consolidated (machine-readable) |
| `BID_SCREEN.md` | 6 | ~8 KB | Full report in markdown |
| `BID_SCREEN.pdf` | 6 | ~50 KB | Professional PDF report (6–8 pages) |

---

## Scoring Thresholds

The 50/40 thresholds were calibrated against the Shipley model's typical bid/no-bid gate criteria:

- **Shipley recommends GO when 60–70% of evaluation factors are favorable.** Our 50/100 threshold (50%) is slightly more permissive because screening has less information fidelity than a full capture assessment.
- **CONDITIONAL (40–49)** corresponds to Shipley's "Bid with conditions" — the opportunity has potential but specific risks must be addressed before resource commitment.
- **NO-GO (<40)** corresponds to a clear "No Bid" — fundamental alignment problems that additional effort won't overcome.

The 5-dimension, 20-point-per-dimension structure ensures no single factor can push an opportunity to GO alone. A perfect score on capability (20) with zeros elsewhere still lands at NO-GO. This forces holistic assessment rather than single-dimension enthusiasm.

---

## Configuration Dependencies

```
safs/
├── Past_Projects.md                              # 28 case studies (required)
├── .claude/skills/process-rfp-screen/
│   ├── skill-screen.md                           # Pipeline orchestrator
│   └── phases-screen/                            # Phase instruction files
│       ├── phase0-intake.md
│       ├── phase1-summary.md
│       ├── phase2-gonogo.md
│       ├── phase3-intel.md
│       ├── phase4-compliance.md
│       ├── phase5-recommendation.md
│       └── phase6-pdf.md
└── .claude/skills/process-rfp-win/config-win/    # Shared config (from full pipeline)
    ├── company-profile.json                      # Company capabilities, services, locations
    ├── bid-outcomes.json                          # Historical bid data (optional)
    └── evidence-library.json                      # Evidence catalog (used by full pipeline)
```

**Note:** The screen pipeline shares `config-win/` with the full win pipeline. This is intentional — company profile data should be maintained in one place and both pipelines read from it.

---

## Relationship to the Full Pipeline

The screening pipeline is designed as a **precursor gate**, not a replacement for `/process-rfp-win`:

| Aspect | Screen Pipeline | Win Pipeline |
|--------|----------------|--------------|
| Time | 15–30 minutes | 3–4 hours |
| Phases | 7 (including intake and PDF) | 45+ core units |
| Agent dispatch | None (inline execution) | Multi-agent team |
| Grok review | Skipped | Mandatory |
| Output | Single PDF report | Full proposal package |
| Purpose | Should we bid? | How do we win? |

A GO recommendation from screening feeds directly into the full pipeline: the next step is literally `/process-rfp-win {same-folder}`. Screening outputs are preserved in `screen/` and don't interfere with the full pipeline's working directories.
