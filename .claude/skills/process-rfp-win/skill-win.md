---
name: process-rfp-win
description: Full RFP-to-bid pipeline (~3-4 hrs, 46 execution units across 7 stages with 8 SVA quality gates) that ingests RFP source documents and produces evaluator-ready bid PDFs (Submittal, Management, Technical, Solution, Financial, Integration, plus Executive Summary). Use AFTER /process-rfp-screen returns a GO recommendation. Sprint mode (~14 units, skips Go/No-Go) is available for pre-approved bids on tight timelines.
when_to_use: 'Triggers on "process rfp win", "rfp win", "win pipeline", "full rfp pipeline", "full bid pipeline", "bid pipeline", "generate bid", "draft full bid", "build the bid", "rfp bid generation", "post go-no-go", "process rfp full", "win edition".'
argument-hint: "[path-to-docs-folder] [--sprint]"
allowed-tools: [Bash, Read, Write, Glob, Grep, WebFetch, WebSearch, Agent, AskUserQuestion]
created: 2026-02-23
updated: 2026-05-18
disable-model-invocation: true
---

<!-- Change log:
  2026-05-18: First skills-excellence-update audit (Tranches A+B).
    Frontmatter: relocated to line 1 (was after H1 — exact bug fixed in screen 2026-05-13); added created, updated, when_to_use, disable-model-invocation; rewrote description from title fragment to full descriptive sentence; cleaned allowed-tools (removed mcp__grok__* per docs convention, renamed Task→Agent, added AskUserQuestion); fixed XML in argument-hint.
    Body: added Configuration block with ${CLAUDE_SKILL_DIR} resolution (replaces 5 hardcoded /home/ddubiel paths); added Required Helper Functions block with UTF-8 helpers (resolves NameError at line 1686 read_file undefined + bare open() encoding regression vector that hit screen 2026-05-18); added Execution Discipline block with Read-Before-Execute Gate, Schema Fidelity rule, Anomaly Protocol (38 phases means 4× the improvisation surface area vs screen); added Memory Integration section; created memory/ folder.
    Background body code keeps Task(...) calls — backwards compatible with Agent dispatch, scheduled for rename in Tranche D.
    Phase file fixes (Linux paths, bare open(), [:N] truncation in schema fields, traceability audit gate), CSS renderer documentation, and orphan-file decisions deferred to Tranches C-G pending user review.
    See memory/last-audit.md for full audit record.

  2026-05-18 (skill-architecture refactor — same day, follow-on tranche):
    DEPRECATED 8 orphan / superseded phase files (renamed *.md.deprecated, archival-only):
      phase8a-title-win.md, phase8b-solution-win.md, phase8c-timeline-win.md (legacy 8a/8b/8c chain — superseded by 8.1-8.6 multi-volume),
      phase8.0a-intel-win.md (relocated to Phase 1.95),
      phase8-bid-author-win.md (legacy monolith — superseded by 8.1-8f),
      phase-addendum-win.md (orphan; never wired into PHASES),
      phase-postrun-metrics-win.md (orphan; metrics now inline via aggregate_metrics()),
      phase2.5-sample-win.md (merged into phase2-extract-win.md Step 7c — same shared/sample-data-analysis.json output),
      phase6b-navigation-win.md (merged into phase6-manifest-win.md — same outputs/NAVIGATION_GUIDE.md output).
    Phase 9 (phase9-postbid-win.md) preserved — confirmed REFERENCED in PHASES list as post-pipeline optional.

    NEW PHASE FILES (4):
      phase3a-tech-stack-win.md (Stage 3, BEFORE phase3a) — split out evidence-backed LTS lookup from phase3a Step 4. Single producer of shared/tech-lifecycle-evidence.json; phase3a now consumes that file deterministically (no WebFetch/WebSearch in architecture authoring).
      phase3h-diagrams-win.md (Stage 3, AFTER phase3g) — authors DIAGRAM_BLUEPRINTS.md and shared/diagram-blueprints.json with 6 mandatory diagrams (logical architecture, sequence/integration, ER, Gantt-with-critical-path, org chart, risk heat map). Phase 8d becomes a pure renderer of these blueprints.
      phase1.85-questions-win.md (Stage 1, BEFORE Phase 1.9) — deadline-aware clarifying-questions list (outputs/CLARIFYING_QUESTIONS.md); honors questions-deadline status from SUBMISSION_STRUCTURE.json so an already-passed deadline becomes a documented input to the Go/No-Go calculation.
      phase1.97-competitive-position-win.md (Stage 1, AFTER Phase 1.95) — pulls ghost strategy + pain-point map + switching-cost analysis + win conditions from Stage 7 to Stage 1. Stage 3 architects now design with competitive frame; Phase 8.0 consumes COMPETITIVE_POSITION.json instead of recomputing.

    SVA-7 RULES ADDED (3, diagram quality):
      SVA7-DIAGRAM-LEGIBILITY (HIGH) — every rendered PNG meets size threshold AND mermaid source declares fontSize >= 14px.
      SVA7-DIAGRAM-CONTRAST (HIGH) — every classDef color pair meets WCAG 2.2 AA contrast 4.5:1.
      SVA7-DIAGRAM-LABELING (MEDIUM) — nodes labeled >2 chars, sequence messages labeled, decision diamonds have branch labels.

    PHASES list updated: added 4 new entries; removed 2.5 + 6b. STAGE_BOUNDARIES updated: Stage 1 now ["0","1","1.5","1.6","1.7","1.8","1.85","1.9","1.95","1.97"], Stage 2 ["2a","2","2b","2c","2d"] (no 2.5), Stage 3 ["3a-tech-stack","3a","3b","3c","3e","3f","3g","3h"], Stage 5 ["6","6c"] (no 6b). Sprint mode unchanged in spirit; sprint phase lists also pruned.
-->


# /process-rfp-win — Full RFP-to-Bid Pipeline (Mayor Orchestrator)

## ⛔ Phase Verifier Dispatch Pattern (READ FIRST — BLOCKING)

Five high-risk phases have dedicated content-aware verifiers that run AFTER the phase agent reports done and BEFORE the stage's SVA gate. These verifiers catch bug classes (truncation, row caps, stale versions, orphaned embeds, wrong orientation) that SVA structural checks miss.

### Verifier Registry

| Phase | Verifier File | SVA Gate After |
|-------|---------------|----------------|
| 3a-tech-stack | `phases-win/verifier-phase3a-tech-stack-win.md` | SVA-3 |
| 4 (Traceability) | `phases-win/verifier-phase4-win.md` | SVA-4 |
| 8.4k (Risk Register) | `phases-win/verifier-phase8.4k-riskreg-win.md` | SVA-7 |
| 8d (Diagrams) | `phases-win/verifier-phase8d-diagrams-win.md` | phase8e |
| 8e (PDF Assembly) | `phases-win/verifier-phase8e-pdf-win.md` | SVA-7 |

### Dispatch Protocol (MANDATORY)

```
phase agent reports done
  → Read verifier-{phase-id}-win.md in FULL
  → Run all verifier checks
  → If PASS: continue to next phase / SVA gate
  → If CONCERN: log advisory, continue (do not block)
  → If FAIL:
      1. Format corrective instructions from verifier's "Corrective Instructions on FAIL" section
      2. Re-dispatch original phase agent with those instructions (max 1 retry)
      3. Re-run verifier after retry
      4. If FAIL again: HALT and escalate to human via AskUserQuestion with full verifier report
         Do NOT attempt a second retry or silent workaround
```

### Why these 5 phases (rationale — do not skip)

| Phase | Today's bug it would have caught |
|-------|----------------------------------|
| 3a-tech-stack | .NET 8/9 contamination from stale training-data versions |
| 4 (RTM) | Requirements silently dropped; risks not cross-linked |
| 8.4k (Risk Register) | Row cap at 15/281, empty mitigations, mid-word `entit`, portrait overflow |
| 8d (Diagrams) | Rendered PNGs with zero embed references → zero-diagram PDFs |
| 8e (PDF Assembly) | Portrait Risk Register, missing column headers, zero embedded images |

---

## ⛔ Execution Discipline (READ FIRST — BLOCKING)

This skill is a 46-unit multi-phase pipeline. Each phase has a prescriptive file with its OWN required-output JSON schema, scoring framework, and quality criteria. **Never improvise a phase from first principles — always Read the phase file in full, in this conversation, immediately before executing that phase.**

**Why this gate exists:** the sibling pipeline `/process-rfp-screen` regressed on 2026-05-14 when an executing agent improvised phases instead of Reading their prescriptive files in full — output dropped from 65 KB / 430 paragraphs (V1 baseline) to 47 KB / 70 paragraphs. The win pipeline has 38 phases vs screen's 10, so the improvisation surface area is roughly 4× larger. Phase files exceed 500 lines specifically because they prescribe full output schemas — they are large by design, not by accident.

**Three non-negotiable rules:**

1. **Read-Before-Execute Gate.** Before dispatching any phase via the Task / Agent tool, the dispatching prompt MUST instruct the subagent to Read the phase file in FULL before generating any output. Skimming, inferring, or selectively reading produces silently-degraded output. Reading the orchestrator file (this file) does NOT satisfy this gate for the individual phase files.

2. **Schema Fidelity.** Each phase file declares its required output keys / sub-objects (e.g., `BID_SCREEN.json.pipeline_metadata.pre_phase6_audit`, `chain_links[]`, `score_breakdown.technology`). The phase JSON written must contain ALL keys the phase file lists. Do not omit, rename, or restructure. If a key cannot be populated due to insufficient evidence, fill it with `null` or the documented conservative score (0 or 1) — do not drop the key. Schema verification happens at SVA gates; missing keys → SVA failure → retry → cap hit → halt.

3. **Anomaly Protocol.** If you detect an anomaly — conflicting evidence between phase outputs, ambiguous instruction in a phase file, missing input from an upstream phase, a phase-file schema element that contradicts a memory rule, a hardcoded path that doesn't resolve, or a baseline output that has materially more depth than your in-progress output — STOP and ASK the user via the AskUserQuestion tool before improvising. Never silently choose a shorter / thinner / faster path.

**Regression sentinels:** if any of the following occur, halt and audit before continuing:
- A phase JSON output is materially smaller than the size declared in the phase file's "Min Size" column without a documented reason
- An SVA report fails schema validation (cross-check against `schemas/sva-report.schema.json`)
- A `read_file` / `read_json` / `write_json` call raises NameError (helpers must be defined per the Required Helper Functions section below)
- A pipeline run finishes Stage 7 without producing PDFs in `outputs/bid/` (phase8e MUST execute; markdown-only output is failure, not success)

---

## Skill Description

Mayor orchestrator for RFP document processing. Coordinates focused subskills for requirements extraction, specification generation, and bid drafting.

**Mayor Responsibilities (ONLY):**
1. Initialize progress tracking in `shared/progress.json`
2. Execute phases in order via Task agent
3. Verify outputs after each phase
4. Retry on failure (up to 3x with RED notification)
5. Final verification of all required outputs

**Mayor Does NOT:**
- Process documents directly
- Extract requirements
- Generate specifications
- Draft bid content

**⚠️ MANDATORY: PDF OUTPUT IS NON-NEGOTIABLE ⚠️**
The pipeline is NOT complete until professional PDF files exist in `outputs/bid/`.
Phase 8e (PDF Assembly) MUST execute after SVA-7 and MUST produce actual PDF files.
Markdown-only output is UNACCEPTABLE. Humans need PDFs, not markdowns.
If `npx md-to-pdf` is unavailable, use Python `markdown_pdf` library as fallback.
**The pipeline FAILS verification if zero PDFs exist in `outputs/bid/`.**

---

## ⚠️ CRITICAL: Stage Execution Discipline ⚠️

**THIS IS A MANDATORY PROTOCOL. NEVER VIOLATE THIS.**

### The Problem (Lesson Learned)
When a user requests "Stage 5", executing BOTH Stage 5 AND Stage 6 (or any other stages) violates user trust and causes confusion. Combining stages without explicit permission makes tracking impossible and skips phases.

### The Rule: STRICT STAGE BOUNDARIES

**When the user requests a stage, execute ONLY the phases belonging to that stage:**

| Stage | Phases | SVA Gate | Outputs |
|-------|--------|----------|---------|
| **Stage 1** | 0, 1, 1.5, 1.6, 1.7, 1.8, **1.85**, 1.9, 1.95, **1.97** | SVA-1 (Intake) | Folder setup, flattening, domain, evaluation, compliance, submission, clarifying questions, go/no-go, client intel, competitive position |
| **Stage 2** | 2a, 2, 2b, 2c, 2d | SVA-2 (Pink Team) | Workflow, requirements (incl. sample data analysis as Step 7c), normalize, catalog, coverage |
| **Stage 3** | **3a-tech-stack**, 3a, 3b, 3c, 3e, 3f, 3g, **3h** | SVA-3 (Spec Validator) | Tech-stack evidence, architecture, interop, security, UI, entities, risks, diagram blueprints |
| **Stage 4** | 4, 5 | SVA-4 (Red Team) | Traceability + UNIFIED_RTM.json, effort estimation |
| **Stage 5** | 6, 6c | SVA-5 (Doc Validator) | Manifest + executive summary + navigation guide (merged into phase 6), context bundle |
| **Stage 6** | 7, 7c, 7d | SVA-6 (Pre-Bid Gate) | Validation + gap analysis, personas, win scorecard |
| **Stage 7** | 8.0, 8.1-8.6, 8.4r, 8.4k, 8f, 8d, **8e** | SVA-0 (Blue Team) + SVA-7 (Gold Team, includes 3 diagram-QA rules) | Positioning, multi-volume bid, RTM verify, diagram rendering (from Stage 3 blueprints), **⚠️ MANDATORY PDFs** |

### Self-Check Before Execution

- [ ] Did the user request a specific stage?
- [ ] Am I executing ONLY the phases for that stage?
- [ ] Am I NOT executing phases from the next stage?
- [ ] Have I confirmed all phases within the requested stage are complete?

**If you generate outputs for Stage N+1 when user requested Stage N, you have FAILED this protocol.**

### Correct Behavior

```
User: "lets do stage 5"

✅ CORRECT: Generate ONLY Phase 6 (MANIFEST.md + EXECUTIVE_SUMMARY.md + NAVIGATION_GUIDE.md — merged 2026-05-18) and Phase 6c (context bundle)
❌ WRONG: Generate Stage 5 outputs AND Stage 6 outputs (validation, gap analysis, personas, scoring)
```

**When in doubt, ask the user: "You requested Stage X. Should I also proceed with Stage Y?"**

---

## ⚠️ CRITICAL: File Organization Rules ⚠️

**ALL Markdown files MUST be in `outputs/` folder - NEVER in `outputs/bid/`**

| File Type | Location | Examples |
|-----------|----------|----------|
| **Markdown (.md)** | `outputs/` | Specs, documentation, catalog |
| **Bid Section (.md)** | `outputs/bid-sections/` | Multi-file bid volume markdown (01_SUBMITTAL.md, etc.) |
| **PDF (.pdf)** | `outputs/bid/` | Final deliverable PDFs only |
| **Images (.png)** | `outputs/bid/` | Rendered diagrams |
| **Mermaid (.mmd)** | `outputs/bid/` | Diagram source files |
| **JSON (.json)** | `shared/` or `shared/bid/` | Data files |
| **SVA Reports (.json)** | `shared/validation/` | SVA-1 through SVA-7 validation reports |

**Rationale:** Keeping MD files in `outputs/` ensures they remain editable and accessible, while `outputs/bid/` contains only final rendered artifacts (PDFs, images).

---

## Required Outputs (Completion Checklist)

### Stage 1-6 Artifacts (Specs, Data, Documentation)

| Required Output | Phase | Min Size |
|-----------------|-------|----------|
| `outputs/EXECUTIVE_SUMMARY.md` | 6 | 2KB |
| `outputs/REQUIREMENTS_CATALOG.md` | 2c | 10KB |
| `outputs/ARCHITECTURE.md` | 3a | 15KB |
| `outputs/SECURITY_REQUIREMENTS.md` | 3c | 8KB |
| `outputs/INTEROPERABILITY.md` | 3b | 5KB |
| `outputs/EFFORT_ESTIMATION.md` | 5 | 8KB |
| `outputs/TRACEABILITY.md` | 4 | 10KB |
| `outputs/NAVIGATION_GUIDE.md` | 6 | 3KB |
| `outputs/MANIFEST.md` | 6 | 2KB |
| `outputs/CLARIFYING_QUESTIONS.md` | 1.85 | 2KB |
| `outputs/COMPETITIVE_POSITION.md` | 1.97 | 3KB |
| `outputs/DIAGRAM_BLUEPRINTS.md` | 3h | 5KB |
| `shared/bid/COMPETITIVE_POSITION.json` | 1.97 | 1KB |
| `shared/diagram-blueprints.json` | 3h | 1KB |
| `shared/tech-lifecycle-evidence.json` | 3a-tech-stack | 1KB |
| `shared/UNIFIED_RTM.json` | 4 | 10KB |
| `shared/SUBMISSION_STRUCTURE.json` | 1.8 | 2KB |
| `shared/GO_NOGO_DECISION.json` | 1.9 | 1KB |
| `shared/bid/CLIENT_INTELLIGENCE.json` | 1.95 | 2KB |
| `shared/bid/POSITIONING_OUTPUT.json` | 8.0 | 3KB |

### Stage 7 Bid Volumes (Multi-File)

| Required Output | Phase | Min Size |
|-----------------|-------|----------|
| `outputs/bid-sections/01_SUBMITTAL.md` | 8.1 | 3KB |
| `outputs/bid-sections/02_MANAGEMENT.md` | 8.2 | 8KB |
| `outputs/bid-sections/03_TECHNICAL.md` | 8.3 | 15KB |
| `outputs/bid-sections/04a_SOLUTION_*.md` | 8.4 | 10KB |
| `outputs/bid-sections/04_REQUIREMENTS_REVIEW.md` | 8.4r | 5KB |
| `outputs/bid-sections/04_RISK_REGISTER.md` | 8.4k | 3KB |
| `outputs/bid-sections/05_FINANCIAL.md` | 8.5 | 5KB |
| `outputs/bid-sections/06_INTEGRATION.md` | 8.6 | 5KB |
| `outputs/RTM_REPORT.md` | 8f | 5KB |

### ⚠️ MANDATORY: Rendered Artifacts (PDFs, Diagrams) — PIPELINE FAILS WITHOUT THESE

| Required Output | Phase | Min Size | MANDATORY |
|-----------------|-------|----------|-----------|
| `outputs/bid/Draft_Bid.pdf` | 8e | 100KB | **YES — consolidated review PDF** |
| `outputs/bid/ResourceData_1_SUBMITTAL.pdf` | 8e | 10KB | **YES** |
| `outputs/bid/ResourceData_2_MANAGEMENT.pdf` | 8e | 20KB | **YES** |
| `outputs/bid/ResourceData_3_TECHNICAL.pdf` | 8e | 30KB | **YES** |
| `outputs/bid/ResourceData_4_SOLUTION.pdf` | 8e | 20KB | **YES** |
| `outputs/bid/ResourceData_5_FINANCIAL.pdf` | 8e | 10KB | **YES** |
| `outputs/bid/ResourceData_6_INTEGRATION.pdf` | 8e | 10KB | **YES** |
| `outputs/bid/EXECUTIVE_SUMMARY.pdf` | 8e | 5KB | **YES** |
| `outputs/bid/architecture.png` | 8d | 10KB | Optional |
| `outputs/bid/timeline.png` | 8d | 10KB | Optional |
| `outputs/bid/orgchart.png` | 8d | 5KB | Optional |

**The pipeline is NOT COMPLETE without PDF files. Phase 8e MUST always execute. If it fails, the pipeline fails.**

### SVA Validation Reports

| Required Output | SVA | Generated After |
|-----------------|-----|-----------------|
| `shared/validation/sva1-intake.json` | SVA-1 | Stage 1 |
| `shared/validation/sva2-pink-team.json` | SVA-2 | Stage 2 |
| `shared/validation/sva3-spec.json` | SVA-3 | Stage 3 |
| `shared/validation/sva4-red-team.json` | SVA-4 | Stage 4 |
| `shared/validation/sva5-doc.json` | SVA-5 | Stage 5 |
| `shared/validation/sva6-pre-bid.json` | SVA-6 | Stage 6 |
| `shared/validation/sva0-blue-team.json` | SVA-0 | Stage 7 (pre-authoring gate) |
| `outputs/BLUE_TEAM_READINESS.md` | SVA-0 | Stage 7 (human review) |
| `shared/validation/sva7-gold-team.json` | SVA-7 | Stage 7 (pre-PDF) |
| `outputs/GOLD_TEAM_CHECKLIST.md` | SVA-7 | Stage 7 (human review handoff) |

---

## User Invocation

```
/process-rfp-win <path-to-docs-folder>
```

**Arguments:**
- `path-to-docs-folder` (required): Path to folder containing RFP documents
- `--sprint` (optional): Run in Sprint mode (~14 units instead of 46). Skips Go/No-Go gate. For simpler RFPs with pre-approved bid decisions and tight timelines.

---

## Sprint Mode

Sprint mode consolidates the full 46-unit pipeline into ~14 execution units for faster turnaround on simpler RFPs. Each sprint phase invokes multiple subskills in a single Task agent call.

**When to use Sprint:**
- Bid decision already made (no Go/No-Go needed)
- Simpler RFPs with clear requirements
- Tight timelines (1-2 weeks)
- Routine renewals or follow-on contracts

**Sprint Phase Map:**

```
SPRINT PIPELINE (~14 execution units)
======================================

S0:     Intake (combines 0 + 1 + 1.5 + 1.6 + 1.7 + 1.8 + 1.95)
        → Organize, flatten, detect domain, extract eval criteria,
          compliance check, submission structure, client intel
        → Skips Phase 1.9 (Go/No-Go) — bid decision pre-approved

SVA-S1: Intake + Requirements Validation (combines SVA-1 + SVA-2 rules)

S2:     Requirements (combines 2a + 2 + 2.5 + 2b + 2c + 2d)
        → Workflow extraction, requirements extraction, sample data,
          normalize, catalog, coverage validation

S3:     Specifications (combines 3a + 3b + 3c + 3e + 3f + 3g)
        → Architecture, interop, security, UI/UX, entities, risks

SVA-S2: Spec + Traceability Validation (combines SVA-3 + SVA-4 rules)

S4:     Traceability & Estimation (combines 4 + 5)
        → UNIFIED_RTM.json build + effort estimation

S5:     Documentation (combines 6 + 6b + 6c)
        → Manifest, navigation guide, context bundle

S6:     Quality Assurance (combines 7 + 7c + 7d)
        → Validation + gap analysis, personas, scoring

SVA-S3: Pre-Bid Validation (combines SVA-5 + SVA-6 rules)

S7:     Bid Generation (combines 8.0 + 8.1 + 8.2 + 8.3 + 8.4 + 8.4r + 8.4k + 8.5 + 8.6 + 8f)
        → Positioning, all bid volumes, RTM verification

S8:     Assembly (combines 8d + 8e)
        → Diagram rendering + PDF assembly

SVA-S4: Gold Team Review (SVA-7 — full rules, human gate)
```

**Sprint STAGE_BOUNDARIES:**

```python
SPRINT_STAGES = {
    "S0": {
        "name": "Sprint Intake",
        # Sprint skips Phase 1.85 (clarifying questions) and 1.97 (competitive position)
        # by default — sprint mode assumes pre-approved bid with these analyses already done.
        # Re-add to the list if you want them on a per-run basis.
        "phases": ["0", "1", "1.5", "1.6", "1.7", "1.8", "1.95"],
        "sva": "SVA-S1",
        "sva_rules": ["SVA-1", "SVA-2"],  # Combined rule sets
        "notes": "Skips Phase 1.9 (Go/No-Go), 1.85 (Clarifying Questions), 1.97 (Competitive Position) — sprint assumes pre-approved bid with strategy already set"
    },
    "S2": {
        "name": "Sprint Requirements",
        # 2.5 merged into Phase 2 Step 7c on 2026-05-18; sample-data-analysis.json still produced.
        "phases": ["2a", "2", "2b", "2c", "2d"]
    },
    "S3": {
        "name": "Sprint Specifications",
        # 3a-tech-stack and 3h (diagram blueprints) added 2026-05-18; sprint may include them
        # for full diagram quality + LTS gating. Drop them if sprint should defer to defaults.
        "phases": ["3a-tech-stack", "3a", "3b", "3c", "3e", "3f", "3g", "3h"]
    },
    "S4": {
        "name": "Sprint Traceability",
        "phases": ["4", "5"],
        "sva": "SVA-S2",
        "sva_rules": ["SVA-3", "SVA-4"]
    },
    "S5": {
        "name": "Sprint Documentation",
        # 6b merged into Phase 6 on 2026-05-18; NAVIGATION_GUIDE.md still produced.
        "phases": ["6", "6c"]
    },
    "S6": {
        "name": "Sprint QA",
        "phases": ["7", "7c", "7d"],
        "sva": "SVA-S3",
        "sva_rules": ["SVA-5", "SVA-6"]
    },
    "S7": {
        "name": "Sprint Bid Generation",
        "phases": ["8.0", "8.1", "8.2", "8.3", "8.4", "8.4r", "8.4k", "8.5", "8.6", "8f"],
        "pre_gate": "SVA-0",
        "pre_gate_subskill": "sva0-blue-team-win.md",
        "pre_gate_report": "sva0-blue-team.json"
    },
    "S8": {
        "name": "Sprint Assembly",
        "phases": ["8d", "8e"],
        "sva": "SVA-S4",
        "sva_rules": ["SVA-7"],  # Full Gold Team review
        "mandatory": True
    }
}
```

**Sprint execution:** Each sprint phase sends ONE Task prompt containing ALL subskill references for that sprint phase. The agent reads and executes them sequentially within a single context window.

---

## Pre-Approved Permissions

**All file operations within `{folder}` and its subdirectories are PRE-APPROVED:**

| Operation | Scope | Notes |
|-----------|-------|-------|
| Read | `{folder}/**/*` | All files in input folder |
| Write | `{folder}/**/*` | Create/overwrite any file |
| Create | `{folder}/**/*` | New files and directories |
| Edit | `{folder}/**/*` | Modify existing files |
| Delete | `{folder}/**/*` | Remove files (cleanup, moves) |
| Move | `{folder}/**/*` | Relocate files within folder |

**Rationale:** User explicitly invokes skill on this folder, granting full access for RFP processing.

**Also pre-approved:**
- Read skill files from this skill's own directory (resolved via `${CLAUDE_SKILL_DIR}` — see Configuration block below)
- Run `npx` commands from skill directory (for mermaid, md-to-pdf)
- Execute `markitdown` for document conversion

---

## Configuration

```python
import os

# Paths — resolved at runtime from skill location.
# Claude Code sets CLAUDE_SKILL_DIR to this skill's directory at load time.
# Fall back to __file__-relative resolution if env var is absent; abort if neither works.
# Pattern mirrored from sibling skill /process-rfp-screen (2026-05-13 audit).
SKILL_DIR = os.environ.get("CLAUDE_SKILL_DIR") or os.path.dirname(os.path.abspath(__file__))
if not SKILL_DIR or not os.path.isdir(SKILL_DIR):
    error("ABORT: Cannot resolve SKILL_DIR — set CLAUDE_SKILL_DIR or invoke from the skill directory")
    halt()

PHASES_DIR        = f"{SKILL_DIR}/phases-win"
SKILLS_DIR        = PHASES_DIR  # legacy alias used by execute_phase / SVA dispatch
DOMAIN_SKILLS_DIR = f"{SKILL_DIR}/.."          # sibling domain skills: capture-strategist, procurement-analyst, etc.
CONFIG_DIR        = f"{SKILL_DIR}/config-win"
SCHEMAS_DIR       = f"{SKILL_DIR}/schemas"
HOOKS_DIR         = f"{SKILL_DIR}/hooks-win"

# Shared with /process-rfp-screen (intentional — both pipelines read same company profile)
COMPANY_PROFILE   = f"{CONFIG_DIR}/company-profile.json"

# Shared registries / schemas referenced by SVA gates
SVA_RULES_REGISTRY = f"{CONFIG_DIR}/sva-rules-registry.json"
SVA_REPORT_SCHEMA  = f"{SCHEMAS_DIR}/sva-report.schema.json"
UNIFIED_RTM_SCHEMA = f"{SCHEMAS_DIR}/unified-rtm.schema.json"
```

---

## ⛔ Required Helper Functions (BLOCKING — define BEFORE any phase runs)

**MANDATORY:** This orchestrator and the phase files reference `read_json` / `read_json_safe` / `write_json` / `read_file` / `write_file` throughout. These are NOT auto-defined — Claude must define them in the runtime Python before phase execution. **They MUST be defined exactly as shown below.**

**Why this matters (incident-driven, not theoretical):**
1. The sibling pipeline `/process-rfp-screen` regressed on 2026-05-18 with em-dash mojibake (`â€"` in place of `—`) because helpers were defined without `encoding='utf-8'`. On Windows the default encoding is cp1252, which silently corrupts UTF-8 em-dash bytes (`E2 80 94`) into three cp1252 chars on read-back.
2. This orchestrator (skill-win.md) calls `read_file(bf)` at the USER INPUT MARKERS reporting step. Without the definition below, that call raises NameError and the pipeline crashes during final reporting.
3. Several phase files (`phase-addendum-win.md`, `phase8-bid-author-win.md`, `phase6c-context-bundle-win.md`, `sva1`, `sva5`, and inline scripts in `hooks-win/theme-validation.json`) define their own LOCAL helpers using bare `open()` without `encoding='utf-8'` — they will reproduce the screen regression. Phase-file fixes are scheduled for Tranche D; until then, executing agents should use the helpers defined HERE in preference to any locally-redefined ones in phase files.

```python
import json, os, sys

# ── ENCODING DISCIPLINE (MANDATORY) ──
# Every file open in this pipeline MUST specify encoding='utf-8'.
# Windows default (cp1252) corrupts em dashes (—) into "â€"" mojibake.
# Stdout must also be reconfigured before any print() that may include unicode.
sys.stdout.reconfigure(encoding='utf-8', errors='replace')

def read_json(path):
    """Read JSON with explicit UTF-8. Never rely on platform default."""
    with open(path, "r", encoding="utf-8") as f:
        return json.load(f)

def read_json_safe(path):
    """Same as read_json but returns None if file missing/invalid (some phases produce optional artifacts)."""
    try:
        return read_json(path)
    except (FileNotFoundError, json.JSONDecodeError):
        return None

def write_json(path, data):
    """Write JSON with UTF-8 + ensure_ascii=False so em dashes survive round-trip.
    Forces LF line endings so byte-level diffs across machines stay clean."""
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        json.dump(data, f, indent=2, ensure_ascii=False)

def read_file(path):
    """Read plain text with explicit UTF-8."""
    with open(path, "r", encoding="utf-8") as f:
        return f.read()

def write_file(path, text):
    """Write plain text with explicit UTF-8 + LF line endings."""
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write(text)
```

**Why `ensure_ascii=False`:** the older default (`True`) escapes em dashes as `—` but does the same to mojibake `â€"`, hiding the corruption. With `ensure_ascii=False` and `encoding='utf-8'`, mojibake becomes immediately visible as `â€"` on inspection, catching bugs at the file boundary.

**This applies to:** inline Python invoked via Bash, generated `.py` scripts dropped into `{folder}/shared/`, the Phase 4 RTM builder, the Phase 6c context bundle, the Phase 8 bid authoring chain, ALL SVA validators, AND the Phase 8e PDF renderer. Do not write a `with open(...)` line in this pipeline without `encoding='utf-8'`.

---

## Phase Execution Order

```
STAGE 1: Document Intake (11 phases + SVA-1)
  Phase 0:    Folder Organization       → phase0-organize-win.md
  Phase 1:    Document Flattening       → phase1-flatten-win.md (PARALLEL)
  Phase 1.5:  Domain Detection          → phase1.5-domain-win.md
  Phase 1.6:  Evaluation Criteria       → phase1.6-evaluation-win.md
  Phase 1.7:  Compliance Gatekeeper     → phase1.7-compliance-win.md [BLOCKING GATE]
  Phase 1.8:  Submission Structure      → phase1.8-submission-win.md
  Phase 1.85: Clarifying Questions      → phase1.85-questions-win.md [NEW 2026-05-18]
  Phase 1.9:  Go/No-Go Decision Gate    → phase1.9-gonogo-win.md [ADVISORY GATE]
  Phase 1.95: Client Intelligence       → phase1.95-intel-win.md [CONDITIONAL: GO only]
  Phase 1.97: Competitive Position      → phase1.97-competitive-position-win.md [NEW 2026-05-18]
  [SVA-1]     Intake Validator          → sva1-intake-validator-win.md

STAGE 2: Requirements Engineering (5 phases + SVA-2)
  Phase 2a:   Workflow Extraction       → phase2a-workflow-win.md
  Phase 2:    Requirements Extraction   → phase2-extract-win.md (now incl. Step 7c: Sample Data Analysis — merged 2026-05-18 from former Phase 2.5)
  Phase 2b:   Normalize Requirements    → phase2b-normalize-win.md
  Phase 2c:   Requirements Catalog      → phase2c-catalog-win.md
  Phase 2d:   Coverage Validation       → phase2d-coverage-win.md [BLOCKING GATE]
  [SVA-2]     PINK TEAM REVIEW          → sva2-pink-team-win.md

STAGE 3: Specifications (8 phases + SVA-3)
  Phase 3a-tech-stack: Tech Stack LTS Lookup → phase3a-tech-stack-win.md [NEW 2026-05-18: produces shared/tech-lifecycle-evidence.json]
  Phase 3a:   Architecture Specs        → phase3a-architecture-win.md  (parallel — now CONSUMES tech-lifecycle-evidence.json instead of producing it)
  Phase 3b:   Interoperability Specs    → phase3b-interop-win.md       (parallel)
  Phase 3c:   Security Specs            → phase3c-security-win.md      (parallel)
  Phase 3e:   UI/UX Specs               → phase3e-ui-win.md            (parallel)
  Phase 3f:   Entity Definitions        → phase3f-entities-win.md      (parallel)
  Phase 3g:   Risk Assessment           → phase3g-risks-win.md         (sequential after 3a-3f)
  Phase 3h:   Diagram Blueprints        → phase3h-diagrams-win.md      [NEW 2026-05-18: blueprints consumed by Phase 8d]
  [SVA-3]     Specification Validator   → sva3-spec-validator-win.md

STAGE 4: Traceability & Estimation (2 phases + SVA-4)
  Phase 4:    Traceability + RTM Build  → phase4-traceability-win.md [MAJOR: builds UNIFIED_RTM.json]
  Phase 5:    Effort Estimation         → phase5-estimation-win.md
  [SVA-4]     RED TEAM REVIEW           → sva4-red-team-win.md

STAGE 5: Documentation (2 phases + SVA-5)
  Phase 6:    Manifest + Exec Summary + Navigation Guide → phase6-manifest-win.md (NAVIGATION_GUIDE.md merged 2026-05-18 from former Phase 6b)
  Phase 6c:   Context Bundle            → phase6c-context-bundle-win.md (enhanced: RTM scores + full eval mapping + theme mandates)
  [SVA-5]     Documentation Validator   → sva5-doc-validator-win.md

STAGE 6: Quality Assurance (4 phases + SVA-6)
  Phase 7:    Validation + Gap Analysis  → phase7-validation-win.md
  Phase 7c:   Evaluator Personas        → phase7c-personas-win.md
  Phase 7d:   Bid Scoring Model         → phase7d-scoring-win.md
  [SVA-6]     PRE-BID GATE (Red Final)  → sva6-pre-bid-validator-win.md

STAGE 7: Bid Generation (12 phases + SVA-0 + SVA-7)
  [SVA-0]     BLUE TEAM GATE            → sva0-blue-team-win.md [BLOCKING PRE-GATE]
  Phase 8.0:  Strategic Positioning     → phase8.0-positioning-win.md (+ theme_eval_mapping, section_theme_mandates, ghost_strategy)
  Phase 8.1:  Letter of Submittal       → phase8.1-submittal-win.md [NEW]
  Phase 8.2:  Management Proposal       → phase8.2-management-win.md [NEW]
  Phase 8.3:  Technical Approach        → phase8.3-technical-win.md [NEW]
  Phase 8.4:  Business Solution (x3)    → phase8.4-solution-win.md [NEW]
  Phase 8.4r: Requirements Review       → phase8.4r-reqreview-win.md [NEW]
  Phase 8.4k: Risk Register             → phase8.4k-riskreg-win.md [NEW]
  Phase 8.5:  Financial Proposal        → phase8.5-financial-win.md [ENHANCED: market rate fallback]
  Phase 8.6:  Technical Integration     → phase8.6-integration-win.md [NEW]
  Phase 8f:   RTM Verification          → phase8f-rtm-verify-win.md [NEW]
  [SVA-7]     GOLD TEAM REVIEW          → sva7-gold-team-win.md (now incl. 3 diagram-QA rules: LEGIBILITY, CONTRAST, LABELING — added 2026-05-18)
  Phase 8d:   Diagram Rendering         → phase8d-diagrams-win.md (renders blueprints from Phase 3h)
  Phase 8e:   Multi-File PDF Assembly   → phase8e-pdf-win.md [RESTRUCTURED]
```

**Total: 41 phases + 8 SVAs = 49 execution units (full mode, post-2026-05-18 refactor — net +3 over the prior 38 + 8 = 46)**
**Sprint: ~14 execution units (see Sprint Mode below)**

---

## Execution Protocol

### Step 1: Initialize Session

```python
folder = args[0]  # User-provided folder path
sprint_mode = "--sprint" in args  # Check for sprint flag

# Validate folder exists
if not exists(folder):
    error(f"Folder does not exist: {folder}")
    halt()

# Initialize progress tracking
progress = {
    "pipeline_start": datetime.now().isoformat(),
    "folder": folder,
    "mode": "sprint" if sprint_mode else "full",
    "current_phase": None,
    "phases": {},
    "retry_counts": {}
}
write_json(f"{folder}/shared/progress.json", progress)

if sprint_mode:
    log(f"📂 Processing RFP folder: {folder}")
    log(f"⚡ SPRINT MODE: ~14 execution units (consolidated pipeline)")
    log(f"   Skipping Go/No-Go gate — bid decision pre-approved")
    log("=" * 50)
else:
    log(f"📂 Processing RFP folder: {folder}")
    log("=" * 50)
```

### Step 2: Define Stage Boundaries and Phase Configuration

```python
# ============================================================
# STAGE BOUNDARIES: Maps stages to their SVA gate
# ============================================================
STAGE_BOUNDARIES = {
    1: {
        "name": "Document Intake",
        "phases": ["0", "1", "1.5", "1.6", "1.7", "1.8", "1.85", "1.9", "1.95", "1.97"],
        "sva": "SVA-1",
        "sva_subskill": "sva1-intake-validator-win.md",
        "sva_report": "sva1-intake.json",
        "color_team": None
    },
    2: {
        "name": "Requirements Engineering",
        "phases": ["2a", "2", "2b", "2c", "2d"],   # 2.5 merged into 2 (Step 7c) on 2026-05-18
        "sva": "SVA-2",
        "sva_subskill": "sva2-pink-team-win.md",
        "sva_report": "sva2-pink-team.json",
        "color_team": "pink"
    },
    3: {
        "name": "Specifications",
        "phases": ["3a-tech-stack", "3a", "3b", "3c", "3e", "3f", "3g", "3h"],
        "sva": "SVA-3",
        "sva_subskill": "sva3-spec-validator-win.md",
        "sva_report": "sva3-spec.json",
        "color_team": None
    },
    4: {
        "name": "Traceability & Estimation",
        "phases": ["4", "5"],
        "sva": "SVA-4",
        "sva_subskill": "sva4-red-team-win.md",
        "sva_report": "sva4-red-team.json",
        "color_team": "red"
    },
    5: {
        "name": "Documentation",
        "phases": ["6", "6c"],   # 6b merged into 6 on 2026-05-18 (NAVIGATION_GUIDE.md now produced by Phase 6)
        "sva": "SVA-5",
        "sva_subskill": "sva5-doc-validator-win.md",
        "sva_report": "sva5-doc.json",
        "color_team": None
    },
    6: {
        "name": "Quality Assurance",
        "phases": ["7", "7c", "7d"],
        "sva": "SVA-6",
        "sva_subskill": "sva6-pre-bid-validator-win.md",
        "sva_report": "sva6-pre-bid.json",
        "color_team": "red"
    },
    7: {
        "name": "Bid Generation",
        "phases": ["8.0", "8.1", "8.2", "8.3", "8.4", "8.4r", "8.4k", "8.5", "8.6", "8f"],
        "pre_gate": "SVA-0",
        "pre_gate_subskill": "sva0-blue-team-win.md",
        "pre_gate_report": "sva0-blue-team.json",
        "sva": "SVA-7",
        "sva_subskill": "sva7-gold-team-win.md",
        "sva_report": "sva7-gold-team.json",
        "color_team": "gold",
        "post_sva_phases": ["8d", "8e"],  # These run AFTER SVA-7 passes
        "notes": "SVA-0 Blue Team validates strategic readiness before authoring. SVA-7 Gold Team validates bid quality after. Generates GOLD_TEAM_CHECKLIST.md for human review."
    }
}

# ============================================================
# PHASE DEFINITIONS (37 phases)
# ============================================================
PHASES = [
    # ---- STAGE 1: Document Intake ----
    {
        "id": "0",
        "name": "Folder Organization",
        "stage": 1,
        "subskill": "phase0-organize-win.md",
        "expert_role": "DevOps Engineer",
        "domain_expertise": "File systems, directory structures, automation",
        "required_outputs": [
            "{folder}/original/",
            "{folder}/flattened/",
            "{folder}/shared/",
            "{folder}/shared/validation/",
            "{folder}/outputs/",
            "{folder}/outputs/bid-sections/"
        ],
        "is_directory": True
    },
    {
        "id": "1",
        "name": "Document Flattening",
        "stage": 1,
        "subskill": "phase1-flatten-win.md",
        "expert_role": "Document Processing Specialist",
        "domain_expertise": "PDF/DOCX/XLSX parsing, text extraction, OCR",
        "skill": "document-processor",
        "notes": "CRITICAL: PDFs must use markitdown, NEVER pdfplumber or Claude's Read tool (~1MB limit)",
        "required_outputs": ["{folder}/flattened/*.md"],
        "parallel": True
    },
    {
        "id": "1.5",
        "name": "Domain Detection",
        "stage": 1,
        "subskill": "phase1.5-domain-win.md",
        "expert_role": "Business Analyst",
        "domain_expertise": "Industry classification, compliance frameworks",
        "skill": "procurement-analyst",
        "required_outputs": ["{folder}/shared/domain-context.json"]
    },
    {
        "id": "1.6",
        "name": "Evaluation Criteria",
        "stage": 1,
        "subskill": "phase1.6-evaluation-win.md",
        "expert_role": "Procurement Specialist",
        "domain_expertise": "RFP evaluation criteria, scoring methodologies",
        "skill": "procurement-analyst",
        "required_outputs": ["{folder}/shared/EVALUATION_CRITERIA.json"]
    },
    {
        "id": "1.7",
        "name": "Compliance Gatekeeper",
        "stage": 1,
        "subskill": "phase1.7-compliance-win.md",
        "expert_role": "Compliance Officer",
        "domain_expertise": "Regulatory requirements, mandatory items, legal",
        "skill": "procurement-analyst",
        "sub_skill": "compliance-audit",
        "required_outputs": ["{folder}/shared/COMPLIANCE_MATRIX.json"],
        "blocking_gate": True,
        "gate_condition": "All mandatory items must be addressed"
    },
    {
        "id": "1.8",
        "name": "Submission Structure Detection",
        "stage": 1,
        "subskill": "phase1.8-submission-win.md",
        "expert_role": "Procurement Analyst",
        "domain_expertise": "RFP submission requirements, volume structure, naming conventions",
        "skill": "procurement-analyst",
        "required_outputs": ["{folder}/shared/SUBMISSION_STRUCTURE.json"]
    },
    {
        "id": "1.85",
        "name": "Clarifying Questions",
        "stage": 1,
        "subskill": "phase1.85-questions-win.md",
        "expert_role": "Procurement Strategist",
        "domain_expertise": "RFP ambiguity resolution, government procurement Q&A protocols",
        "skill": "procurement-analyst",
        "required_outputs": [
            "{folder}/outputs/CLARIFYING_QUESTIONS.md",
            "{folder}/shared/clarifying-questions-summary.json"
        ],
        "min_size_kb": 2,
        "notes": "Runs BEFORE Phase 1.9 so an already-passed RFP questions deadline becomes a documented input to the Go/No-Go calc. Honors SUBMISSION_STRUCTURE.json deadline status."
    },
    {
        "id": "1.9",
        "name": "Go/No-Go Decision Gate",
        "stage": 1,
        "subskill": "phase1.9-gonogo-win.md",
        "expert_role": "Bid Decision Analyst",
        "domain_expertise": "Bid/no-bid analysis, opportunity qualification, capture management",
        "skill": "capture-strategist",
        "sub_skill": "bid-decision",
        "required_outputs": ["{folder}/shared/GO_NOGO_DECISION.json"],
        "advisory_gate": True,
        "gate_thresholds": {"go": 50, "conditional": 40, "no_go": 0},
        "notes": "Advisory decision point. GO: proceed. CONDITIONAL: ask user. NO-GO: recommend halt (user can override). Skipped in Sprint mode."
    },
    {
        "id": "1.95",
        "name": "Client Intelligence",
        "stage": 1,
        "subskill": "phase1.95-intel-win.md",
        "expert_role": "Competitive Intelligence Analyst",
        "domain_expertise": "Market research, incumbent analysis, FPDS, competitive positioning",
        "skill": "competitive-intel",
        "required_outputs": ["{folder}/shared/bid/CLIENT_INTELLIGENCE.json"],
        "conditional": True,
        "condition": "Only runs if GO_NOGO_DECISION.json recommends GO or user overrides",
        "notes": "Relocated from Phase 8.0a (Stage 7) to Stage 1 for earlier competitive intelligence. Consumed by phases 2a, 3a, 3f (optional), 1.97, and 8.0 (required)."
    },
    {
        "id": "1.97",
        "name": "Competitive Position",
        "stage": 1,
        "subskill": "phase1.97-competitive-position-win.md",
        "expert_role": "Competitive Strategist",
        "domain_expertise": "Win/loss analysis, ghost strategy, incumbent vs challenger positioning",
        "skill": "capture-strategist",
        "sub_skill": "competitive-positioning",
        "required_outputs": [
            "{folder}/outputs/COMPETITIVE_POSITION.md",
            "{folder}/shared/bid/COMPETITIVE_POSITION.json"
        ],
        "min_size_kb": 3,
        "conditional": True,
        "condition": "Only runs if GO_NOGO_DECISION.json recommends GO or user overrides",
        "notes": "NEW 2026-05-18. Pulls ghost strategy + pain-point map + switching-cost analysis + win conditions from Stage 7 (phase8.0) into Stage 1, giving Stage 3 architects a competitive frame BEFORE design. Consumed by phases 3a-3g and 8.0."
    },

    # ---- STAGE 2: Requirements Engineering ----
    {
        "id": "2a",
        "name": "Workflow Extraction",
        "stage": 2,
        "subskill": "phase2a-workflow-win.md",
        "expert_role": "Business Process Analyst",
        "domain_expertise": "Process flows, BPMN, workflow mapping",
        "required_outputs": ["{folder}/shared/workflow-extracted-reqs.json"]
    },
    {
        "id": "2",
        "name": "Requirements Extraction",
        "stage": 2,
        "subskill": "phase2-extract-win.md",
        "expert_role": "Requirements Engineer",
        "domain_expertise": "Requirements elicitation, traceability",
        "required_outputs": ["{folder}/shared/requirements-raw.json"]
    },
    # Phase 2.5 was merged into Phase 2 Step 7c on 2026-05-18; sample-data-analysis.json
    # is now produced inline by phase2-extract-win.md at the same path. No separate phase.
    {
        "id": "2b",
        "name": "Normalize Requirements",
        "stage": 2,
        "subskill": "phase2b-normalize-win.md",
        "expert_role": "Requirements Engineer",
        "domain_expertise": "Deduplication, normalization, validation",
        "required_outputs": ["{folder}/shared/requirements-normalized.json"]
    },
    {
        "id": "2c",
        "name": "Requirements Catalog",
        "stage": 2,
        "subskill": "phase2c-catalog-win.md",
        "expert_role": "Technical Writer",
        "domain_expertise": "Documentation structure, cataloging",
        "required_outputs": [
            "{folder}/outputs/REQUIREMENTS_CATALOG.md",
            "{folder}/shared/REQUIREMENTS_CATALOG.json"
        ]
    },
    {
        "id": "2d",
        "name": "Coverage Validation",
        "stage": 2,
        "subskill": "phase2d-coverage-win.md",
        "expert_role": "QA Engineer",
        "domain_expertise": "Coverage analysis, gap detection",
        "required_outputs": ["{folder}/shared/workflow-coverage.json"],
        "blocking_gate": True,
        "gate_condition": "100% workflow coverage required"
    },

    # ---- STAGE 3: Specifications ----
    {
        "id": "3a-tech-stack",
        "name": "Tech Stack Lifecycle Lookup",
        "stage": 3,
        "subskill": "phase3a-tech-stack-win.md",
        "expert_role": "Technology Strategist",
        "domain_expertise": "Technology lifecycle, LTS scheduling, vendor support matrices, evidence-backed version selection",
        "required_outputs": ["{folder}/shared/tech-lifecycle-evidence.json"],
        "min_size_kb": 1,
        "notes": "NEW 2026-05-18. Sole producer of shared/tech-lifecycle-evidence.json (consumed by phase3a + SVA-3 rule SVA3-TECH-STACK-LTS-VERIFIED). Must run BEFORE phase3a so architecture authoring is deterministic (no WebFetch/WebSearch in phase3a)."
    },
    {
        "id": "3a",
        "name": "Architecture Specs",
        "stage": 3,
        "subskill": "phase3a-architecture-win.md",
        "expert_role": "Solutions Architect",
        "domain_expertise": "System design, cloud architecture, scalability",
        "required_outputs": ["{folder}/outputs/ARCHITECTURE.md"],
        "min_size_kb": 15,
        "parallel_group": "phase3",
        "depends_on": ["3a-tech-stack"],
        "notes": "Now CONSUMES shared/tech-lifecycle-evidence.json (produced by phase3a-tech-stack-win) instead of producing it. HALTS if the evidence file is missing — does not issue WebFetch/WebSearch itself."
    },
    {
        "id": "3b",
        "name": "Interoperability Specs",
        "stage": 3,
        "subskill": "phase3b-interop-win.md",
        "expert_role": "Integration Architect",
        "domain_expertise": "APIs, data exchange, EDI, HL7/FHIR",
        "required_outputs": ["{folder}/outputs/INTEROPERABILITY.md"],
        "min_size_kb": 5,
        "parallel_group": "phase3"
    },
    {
        "id": "3c",
        "name": "Security Specs",
        "stage": 3,
        "subskill": "phase3c-security-win.md",
        "expert_role": "Security Architect",
        "domain_expertise": "OWASP, encryption, auth, compliance (HIPAA/FERPA)",
        "required_outputs": ["{folder}/outputs/SECURITY_REQUIREMENTS.md"],
        "min_size_kb": 8,
        "parallel_group": "phase3"
    },
    {
        "id": "3e",
        "name": "UI/UX Specs",
        "stage": 3,
        "subskill": "phase3e-ui-win.md",
        "expert_role": "UX Designer",
        "domain_expertise": "User interfaces, accessibility, wireframes",
        "required_outputs": ["{folder}/outputs/UI_SPECS.md"],
        "parallel_group": "phase3"
    },
    {
        "id": "3f",
        "name": "Entity Definitions",
        "stage": 3,
        "subskill": "phase3f-entities-win.md",
        "expert_role": "Data Architect",
        "domain_expertise": "Entity modeling, ERD, database design",
        "required_outputs": ["{folder}/outputs/ENTITY_DEFINITIONS.md"],
        "parallel_group": "phase3"
    },
    {
        "id": "3g",
        "name": "Risk Assessment",
        "stage": 3,
        "subskill": "phase3g-risks-win.md",
        "expert_role": "Risk Analyst",
        "domain_expertise": "Risk assessment, mitigation strategies",
        "skill": "risk-analyst",
        "required_outputs": [
            "{folder}/outputs/REQUIREMENT_RISKS.md",
            "{folder}/shared/REQUIREMENT_RISKS.json"
        ]
    },
    {
        "id": "3h",
        "name": "Diagram Blueprints",
        "stage": 3,
        "subskill": "phase3h-diagrams-win.md",
        "expert_role": "Visual Design Architect",
        "domain_expertise": "Mermaid diagrams, information design, accessibility-aware visualization",
        "required_outputs": [
            "{folder}/outputs/DIAGRAM_BLUEPRINTS.md",
            "{folder}/shared/diagram-blueprints.json"
        ],
        "min_size_kb": 5,
        "depends_on": ["3a", "3b", "3c", "3f", "3g"],
        "notes": "NEW 2026-05-18. Authors 6 mandatory diagrams (logical architecture, sequence/integration, ER, Gantt-with-critical-path, org chart, risk heat map) as BLUEPRINTS BEFORE Stage 8 rendering. Phase 8d consumes shared/diagram-blueprints.json as a pure renderer. Quality criteria (font 14pt, WCAG-AA contrast, Okabe-Ito palette, descriptive titles, source citation) gated by SVA-7 diagram-QA rules."
    },

    # ---- STAGE 4: Traceability & Estimation ----
    {
        "id": "4",
        "name": "Traceability Matrix + RTM Build",
        "stage": 4,
        "subskill": "phase4-traceability-win.md",
        "expert_role": "Requirements Traceability Engineer",
        "domain_expertise": "Traceability matrices, cross-referencing, RTM construction, entity linking",
        "required_outputs": [
            "{folder}/outputs/TRACEABILITY.md",
            "{folder}/shared/UNIFIED_RTM.json"
        ],
        "min_size_kb": 10,
        "notes": "MAJOR: Builds UNIFIED_RTM.json linking all entities across full chain"
    },
    {
        "id": "5",
        "name": "Effort Estimation",
        "stage": 4,
        "subskill": "phase5-estimation-win.md",
        "expert_role": "Project Estimator",
        "domain_expertise": "Effort estimation, resource planning, AI ratios",
        "required_outputs": ["{folder}/outputs/EFFORT_ESTIMATION.md"],
        "min_size_kb": 8
    },

    # ---- STAGE 5: Documentation ----
    {
        "id": "6",
        "name": "Manifest + Executive Summary + Navigation Guide",
        "stage": 5,
        "subskill": "phase6-manifest-win.md",
        "expert_role": "Technical Writer",
        "domain_expertise": "Documentation, audit trails",
        "required_outputs": [
            "{folder}/outputs/MANIFEST.md",
            "{folder}/outputs/EXECUTIVE_SUMMARY.md",
            "{folder}/outputs/NAVIGATION_GUIDE.md"
        ],
        "notes": "Phase 6b (Navigation Guide) merged into this phase 2026-05-18. NAVIGATION_GUIDE.md now produced inline at the same path. SVA-5 rule SVA5-NAV-GUIDE-LINKS still validates the file."
    },
    # Phase 6b was merged into Phase 6 on 2026-05-18; NAVIGATION_GUIDE.md is now produced
    # by phase6-manifest-win.md alongside MANIFEST.md and EXECUTIVE_SUMMARY.md.
    {
        "id": "6c",
        "name": "Context Bundle",
        "stage": 5,
        "subskill": "phase6c-context-bundle-win.md",
        "expert_role": "Data Integration Architect",
        "domain_expertise": "Data aggregation, context synthesis, information architecture",
        "required_outputs": ["{folder}/shared/bid-context-bundle.json"],
        "notes": "Enhanced: reads UNIFIED_RTM.json for composite scores. Includes full bid_section_mapping, eval_factors_by_weight, theme_eval_mapping, section_theme_mandates, section_content_guide from EVALUATION_CRITERIA.json and POSITIONING_OUTPUT.json."
    },

    # ---- STAGE 6: Quality Assurance ----
    {
        "id": "7",
        "name": "Quality Validation & Gap Analysis",
        "stage": 6,
        "subskill": "phase7-validation-win.md",
        "expert_role": "QA Engineer",
        "domain_expertise": "Quality assurance, validation rules, gap analysis, benchmark comparison",
        "required_outputs": [
            "{folder}/shared/validation-results.json",
            "{folder}/outputs/GAP_ANALYSIS.md"
        ],
        "notes": "Combined structural validation + benchmark gap analysis. Outputs validation-results.json (with gap data) and GAP_ANALYSIS.md"
    },
    {
        "id": "7c",
        "name": "Evaluator Personas",
        "stage": 6,
        "subskill": "phase7c-personas-win.md",
        "expert_role": "UX Researcher",
        "domain_expertise": "Evaluator personas, stakeholder analysis",
        "required_outputs": ["{folder}/shared/PERSONA_COVERAGE.json"]
    },
    {
        "id": "7d",
        "name": "Bid Scoring Model",
        "stage": 6,
        "subskill": "phase7d-scoring-win.md",
        "expert_role": "Bid Strategist",
        "domain_expertise": "Win probability, scoring models",
        "skill": "capture-strategist",
        "sub_skill": "bid-decision",
        "required_outputs": ["{folder}/shared/WIN_SCORECARD.json"]
    },

    # ---- STAGE 7: Bid Generation ----
    # NOTE: Phase 8.0a (Client Intelligence) relocated to Phase 1.95 in Stage 1
    {
        "id": "8.0",
        "name": "Strategic Positioning",
        "stage": 7,
        "subskill": "phase8.0-positioning-win.md",
        "expert_role": "Bid Strategist",
        "domain_expertise": "Strategic positioning, differentiators, past performance matching",
        "skill": "capture-strategist",
        "sub_skill": "competitive-positioning",
        "required_outputs": ["{folder}/shared/bid/POSITIONING_OUTPUT.json"],
        "additional_inputs": ["Past_Projects.md", "config-win/company-profile.json"],
        "notes": "Reads Past_Projects.md and scores/ranks projects. Outputs matched_projects[], theme_eval_mapping (themes→eval factors), section_theme_mandates (bid sections→required themes), evaluator_messages (persona-tailored messaging)."
    },
    {
        "id": "8.1",
        "name": "Letter of Submittal",
        "stage": 7,
        "subskill": "phase8.1-submittal-win.md",
        "expert_role": "Executive Communications Specialist",
        "domain_expertise": "Cover letters, certifications, NOSE formula, executive persuasion",
        "required_outputs": ["{folder}/outputs/bid-sections/01_SUBMITTAL.md"],
        "additional_inputs": ["Past_Projects.md"],
        "depends_on": ["6c", "8.0"],
        "notes": "Consumes matched_projects[] for NOSE Evidence. Eval factor callout at top. >= 3 win themes mandated. Evaluator message integration (EXECUTIVE persona)."
    },
    {
        "id": "8.2",
        "name": "Management Proposal",
        "stage": 7,
        "subskill": "phase8.2-management-win.md",
        "expert_role": "Management Proposal Writer",
        "domain_expertise": "Experience narratives, team qualifications, OCM, references, past performance",
        "required_outputs": ["{folder}/outputs/bid-sections/02_MANAGEMENT.md"],
        "additional_inputs": ["Past_Projects.md"],
        "depends_on": ["6c", "8.0"],
        "notes": "Auto-populates 3-5 case studies from Past_Projects.md. Eval factor callout boxes at every ## section. >= 2 themes/section in CVD format. Evaluator personas: EXECUTIVE, RISK, TECHNICAL, OPERATIONAL."
    },
    {
        "id": "8.3",
        "name": "Technical Approach",
        "stage": 7,
        "subskill": "phase8.3-technical-win.md",
        "expert_role": "Senior Technical Proposal Writer",
        "domain_expertise": "Methodology, QA processes, risk management, KPIs, technical writing, proven capability evidence",
        "model": "opus",
        "required_outputs": ["{folder}/outputs/bid-sections/03_TECHNICAL.md"],
        "additional_inputs": ["Past_Projects.md"],
        "min_size_kb": 15,
        "depends_on": ["6c", "8.0"],
        "notes": "Proven Capability callouts from past projects. Eval factor callout boxes at every ## section. >= 2 themes/section in CVD format. Content ordered by eval weight. Evaluator persona integration. Tech lifecycle validation mandatory."
    },
    {
        "id": "8.4",
        "name": "Business Solution",
        "stage": 7,
        "subskill": "phase8.4-solution-win.md",
        "expert_role": "Business Solution Architect",
        "domain_expertise": "Solution design per work section, requirements mapping, implementation planning",
        "model": "opus",
        "required_outputs": ["{folder}/outputs/bid-sections/04a_SOLUTION_*.md"],
        "depends_on": ["6c", "8.0"],
        "notes": "Generates one solution file per work section defined in SUBMISSION_STRUCTURE.json"
    },
    {
        "id": "8.4r",
        "name": "Requirements Review Response",
        "stage": 7,
        "subskill": "phase8.4r-reqreview-win.md",
        "expert_role": "Requirements Analyst",
        "domain_expertise": "Requirements response tables, compliance matrices, Attachment A format",
        "required_outputs": ["{folder}/outputs/bid-sections/04_REQUIREMENTS_REVIEW.md"],
        "depends_on": ["8.4"]
    },
    {
        "id": "8.4k",
        "name": "Risk Register",
        "stage": 7,
        "subskill": "phase8.4k-riskreg-win.md",
        "expert_role": "Risk Analyst",
        "domain_expertise": "Risk registers, tabular risk presentation, mitigation tracking",
        "skill": "risk-analyst",
        "required_outputs": ["{folder}/outputs/bid-sections/04_RISK_REGISTER.md"],
        "depends_on": ["8.4"]
    },
    {
        "id": "8.5",
        "name": "Financial Proposal",
        "stage": 7,
        "subskill": "phase8.5-financial-win.md",
        "expert_role": "Financial Analyst",
        "domain_expertise": "Cost narratives, rate tables, pricing strategy",
        "required_outputs": ["{folder}/outputs/bid-sections/05_FINANCIAL.md"],
        "depends_on": ["6c"],
        "notes": "Market rate fallback: company rates > $0 override market defaults from company-profile.json market_rate_defaults. Rate source attribution (COMPANY/MARKET DEFAULT/UNPOPULATED). Cost eval alignment, TCO, auto-generated payment schedule."
    },
    {
        "id": "8.6",
        "name": "Technical Integration Plan",
        "stage": 7,
        "subskill": "phase8.6-integration-win.md",
        "expert_role": "Integration Architect",
        "domain_expertise": "Multi-vendor coordination, integration architecture, interoperability",
        "required_outputs": ["{folder}/outputs/bid-sections/06_INTEGRATION.md"],
        "depends_on": ["6c"]
    },
    {
        "id": "8f",
        "name": "RTM Verification",
        "stage": 7,
        "subskill": "phase8f-rtm-verify-win.md",
        "expert_role": "Traceability Verification Engineer",
        "domain_expertise": "RTM auditing, 14-query verification protocol, coverage metrics",
        "required_outputs": [
            "{folder}/outputs/RTM_REPORT.md"
        ],
        "notes": "Runs 14 verification queries against UNIFIED_RTM.json, updates verification{} section"
    },

    # ---- POST-SVA-7: Rendering & Assembly (run after Gold Team passes) ----
    {
        "id": "8d",
        "name": "Diagram Rendering",
        "stage": 7,
        "subskill": "phase8d-diagrams-win.md",
        "expert_role": "Visual Design Engineer",
        "domain_expertise": "Mermaid, diagrams, visual communication",
        "required_outputs": [
            "{folder}/outputs/bid/architecture.png",
            "{folder}/outputs/bid/timeline.png",
            "{folder}/outputs/bid/orgchart.png"
        ],
        "post_sva": True
    },
    {
        "id": "8e",
        "name": "Multi-File PDF Assembly",
        "stage": 7,
        "subskill": "phase8e-pdf-win.md",
        "expert_role": "Publication Specialist",
        "domain_expertise": "PDF generation, multi-file assembly, typography, layout",
        "skill": "publication-specialist",
        "required_outputs": ["{folder}/outputs/bid/*.pdf"],
        "notes": "⚠️ MANDATORY — Pipeline is INCOMPLETE without PDFs. Uses Python markdown_pdf (primary) or npx md-to-pdf (fallback). Generates named PDFs per SUBMISSION_STRUCTURE.json PLUS consolidated Draft_Bid.pdf. MUST ALWAYS EXECUTE.",
        "post_sva": True,
        "mandatory": True
    },

    # ---- POST-PIPELINE (optional) ----
    {"id": "9", "name": "Post-Bid Learning (Optional)", "subskill": "phase9-postbid-win.md", "stage": "post",
     "skill": "capture-strategist", "sub_skill": "bid-decision",
     "outputs": [("outputs/BID_OUTCOME_REPORT.md", 2)],
     "notes": "Run after bid decision received. Logs outcome to bid-outcomes.json for future pattern analysis."},
]
```

### Step 3: Execute Phase with Retry Logic

```python
MAX_RETRIES = 3
# SKILLS_DIR and DOMAIN_SKILLS_DIR are defined in the Configuration block at the
# top of this file (resolved from ${CLAUDE_SKILL_DIR}). Do NOT redefine here.

def execute_phase(phase, folder, enhanced_instruction=None):
    """
    Execute a single phase via Task agent with retry logic.
    Returns True if phase completed successfully, False otherwise.

    Args:
        phase: phase dict from PHASES table
        folder: working folder for the run
        enhanced_instruction: optional corrective guidance string appended to the
            dispatch prompt. Used by SVA auto-correction to pass targeted re-run
            instructions back into the next attempt.
    """
    phase_id = phase["id"]
    subskill = phase["subskill"]
    expert_role = phase["expert_role"]
    domain_expertise = phase["domain_expertise"]
    required_outputs = phase["required_outputs"]

    retry_count = 0

    while retry_count < MAX_RETRIES:
        # Update progress: in_progress
        update_progress(folder, phase_id, "in_progress", f"Attempt {retry_count + 1}/{MAX_RETRIES}")

        # Load domain skill if phase declares one
        skill_name = phase.get("skill")
        sub_skill_name = phase.get("sub_skill")
        skill_instruction = ""

        if skill_name:
            skill_path = f"{DOMAIN_SKILLS_DIR}/{skill_name}.md"
            skill_instruction = f"\n**Domain Skill:** Read `{skill_path}` FIRST for domain expertise context."
            skill_log = f"Skill: {skill_name}"

            if sub_skill_name:
                sub_skill_path = f"{DOMAIN_SKILLS_DIR}/{skill_name}/{sub_skill_name}.md"
                skill_instruction += f"\n**Sub-skill:** Also read `{sub_skill_path}` for specialized depth."
                skill_log += f" + {sub_skill_name}"

            log(f"  {skill_log} (loading)")
        else:
            log(f"  Skill: none (procedural phase)")

        # Build Task prompt with expert role and skill injection
        prompt = f"""
You are a **{expert_role}** with deep expertise in:
- {domain_expertise}
{skill_instruction}

Execute the subskill: {SKILLS_DIR}/{subskill}

**Working folder:** {folder}

**Required outputs (MUST create):**
{chr(10).join(f"- {o.replace('{folder}', folder)}" for o in required_outputs)}

**Instructions:**
1. {"Read the domain skill file(s) listed above to load expertise context" if skill_name else "Read the subskill file in FULL before generating any output. Do not skim, infer, or selectively read. The file is prescriptive — output schemas and quality criteria live in it."}
2. **Read the subskill file in FULL before generating any output. Do not skim, infer, or selectively read. The file is prescriptive — output schemas and quality criteria live in it.**
3. Execute as the expert role specified
4. Create ALL required outputs
5. Verify outputs exist before completing

**CRITICAL:** Do not report completion until all required outputs exist.
"""

        # Append enhanced instruction (e.g., SVA auto-correction guidance) if provided.
        # This lets a re-dispatch carry corrective context from an SVA finding into the
        # subagent prompt without recompiling the whole dispatch logic.
        if enhanced_instruction:
            prompt += f"\n\n**Corrective guidance (from prior SVA finding):**\n{enhanced_instruction}\n"

        # Invoke Task agent
        Task(
            prompt=prompt,
            subagent_type="general-purpose",
            description=f"Phase {phase_id}: {phase['name']}"
        )

        # Verify outputs exist
        missing = verify_outputs(folder, required_outputs, phase.get("min_size_kb"), phase.get("is_directory", False))

        if not missing:
            # SUCCESS - phase completed
            update_progress(folder, phase_id, "completed", f"{phase['name']} completed successfully")
            log(f"✅ Phase {phase_id}: {phase['name']} COMPLETED")
            return True
        else:
            # FAILURE - retry
            retry_count += 1
            log(f"\033[91m❌ Phase {phase_id}: {phase['name']} FAILED (attempt {retry_count}/{MAX_RETRIES})\033[0m")
            log(f"\033[91m   Missing outputs: {missing}\033[0m")

            if retry_count < MAX_RETRIES:
                log(f"\033[93m   Retrying...\033[0m")

    # All retries exhausted
    update_progress(folder, phase_id, "failed", f"Failed after {MAX_RETRIES} attempts")
    log(f"\033[91m🛑 Phase {phase_id}: {phase['name']} FAILED after {MAX_RETRIES} attempts\033[0m")

    # Check if blocking gate
    if phase.get("blocking_gate"):
        log(f"\033[91m⛔ BLOCKING GATE FAILED: {phase.get('gate_condition')}\033[0m")
        return False

    return False


def execute_sva(stage_num, folder):
    """
    Execute a Stage Validation Agent after all phases in a stage complete.
    Returns disposition: "PASS", "ADVISORY", or "BLOCK".

    SVA reads all artifacts produced by the stage, validates against rules
    from sva-rules-registry.json, and produces a structured JSON report.
    """
    stage = STAGE_BOUNDARIES[stage_num]
    sva_id = stage["sva"]
    sva_subskill = stage["sva_subskill"]
    sva_report_file = f"{folder}/shared/validation/{stage['sva_report']}"
    color_team = stage.get("color_team")

    color_label = f" ({color_team.upper()} TEAM)" if color_team else ""
    log(f"\n{'='*60}")
    log(f"🔍 {sva_id}: {stage['name']} Validation{color_label}")
    log(f"{'='*60}")

    # Build SVA prompt
    sva_prompt = f"""
You are a **Stage Validation Agent ({sva_id})** performing {'a ' + color_team.upper() + ' TEAM review' if color_team else 'validation'} after Stage {stage_num}: {stage['name']}.

Execute the subskill: {SKILLS_DIR.replace('phases-win', 'phases-win')}/{sva_subskill}

**Working folder:** {folder}
**SVA Rules Registry:** {SVA_RULES_REGISTRY}
**SVA Report Schema:** {SVA_REPORT_SCHEMA}
**Report Output:** {sva_report_file}

**Instructions:**
1. **Read the SVA subskill file in FULL before generating any output. Do not skim, infer, or selectively read. The file is prescriptive — output schemas and quality criteria live in it.**
2. Read the SVA rules registry for {sva_id} rule definitions
3. Validate all artifacts produced by Stage {stage_num}
4. Produce a structured JSON report conforming to sva-report.schema.json
5. Write report to {sva_report_file}

**CRITICAL:** Report must include disposition (PASS/ADVISORY/BLOCK) and all findings.
"""

    Task(
        prompt=sva_prompt,
        subagent_type="general-purpose",
        description=f"{sva_id}: {stage['name']} Validation{color_label}"
    )

    # Read SVA report and extract disposition
    if exists(sva_report_file):
        report = read_json(sva_report_file)
        disposition = report.get("disposition", "BLOCK")
        summary = report.get("summary", {})
        score = summary.get("overall_score", 0)

        # Display results
        if disposition == "PASS":
            log(f"✅ {sva_id} PASSED (score: {score}/100)")
        elif disposition == "ADVISORY":
            log(f"⚠️ {sva_id} ADVISORY (score: {score}/100) - Continuing with warnings")
            # Log top concerns
            if report.get("color_team_report", {}).get("top_concerns"):
                for concern in report["color_team_report"]["top_concerns"][:3]:
                    log(f"   ⚠️ {concern}")
        else:  # BLOCK
            log(f"\033[91m⛔ {sva_id} BLOCKED (score: {score}/100)\033[0m")
            critical_findings = [f for f in report.get("findings", [])
                               if f.get("severity") == "CRITICAL" and not f.get("passed")]
            for finding in critical_findings[:5]:
                log(f"\033[91m   ❌ {finding['rule_id']}: {finding['rule_name']}\033[0m")
                if finding.get("corrective_action", {}).get("auto_correctable"):
                    log(f"\033[93m      → Auto-correctable via Phase {finding['corrective_action'].get('target_phase')}\033[0m")

            # Attempt auto-correction for BLOCK disposition (max 1 retry)
            auto_correctable = [f for f in critical_findings
                               if f.get("corrective_action", {}).get("auto_correctable")]
            if auto_correctable:
                log(f"\033[93m   Attempting auto-correction for {len(auto_correctable)} finding(s)...\033[0m")
                for finding in auto_correctable:
                    target_phase = finding["corrective_action"]["target_phase"]
                    instruction = finding["corrective_action"].get("instruction", "")
                    # Find and re-run the target phase with enhanced instructions
                    target = next((p for p in PHASES if p["id"] == target_phase), None)
                    if target:
                        log(f"   🔄 Re-running Phase {target_phase}: {target['name']}")
                        execute_phase(target, folder, enhanced_instruction=instruction)

                # Re-run SVA after corrections
                log(f"   🔄 Re-running {sva_id} after corrections...")
                return execute_sva(stage_num, folder)  # Recursive retry (once)

            # Non-auto-correctable BLOCK: halt and present options
            log(f"\033[91m⛔ {sva_id} BLOCK requires manual intervention\033[0m")
            log(f"   Options:")
            log(f"   1. Fix issues manually and re-run Stage {stage_num}")
            log(f"   2. Override with user approval")
            log(f"   3. Abort pipeline")

        return disposition
    else:
        log(f"\033[91m❌ {sva_id} report not generated\033[0m")
        return "BLOCK"


def verify_outputs(folder, required_outputs, min_size_kb=None, is_directory=False):
    """
    Verify all required outputs exist with minimum size.
    Returns list of missing outputs.
    """
    missing = []

    for output_pattern in required_outputs:
        path = output_pattern.replace("{folder}", folder)

        if "*" in path:
            # Glob pattern - check at least one file matches
            import glob
            matches = glob.glob(path)
            if not matches:
                missing.append(path)
        elif is_directory:
            # Directory check
            if not exists(path) or not isdir(path):
                missing.append(path)
        else:
            # File check
            if not exists(path):
                missing.append(path)
            elif min_size_kb:
                # Check minimum size
                size_kb = os.path.getsize(path) / 1024
                required_size = min_size_kb if isinstance(min_size_kb, int) else min_size_kb.get(os.path.basename(path), min_size_kb.get("default", 1))
                if size_kb < required_size:
                    missing.append(f"{path} (size: {size_kb:.1f}KB < {required_size}KB required)")

    return missing


def update_progress(folder, phase_id, status, message):
    """Update progress.json with phase status and timing metrics."""
    progress_file = f"{folder}/shared/progress.json"

    if exists(progress_file):
        progress = read_json(progress_file)
    else:
        progress = {"phases": {}, "metrics": {}}

    now = datetime.now().isoformat()

    # Get or create phase entry
    phase_entry = progress["phases"].get(phase_id, {})
    phase_entry["status"] = status
    phase_entry["timestamp"] = now
    phase_entry["message"] = message

    # Track timing
    if status == "in_progress" and "start_timestamp" not in phase_entry:
        phase_entry["start_timestamp"] = now
    elif status in ("completed", "failed", "skipped"):
        phase_entry["end_timestamp"] = now
        # Calculate duration if we have a start time
        if "start_timestamp" in phase_entry:
            from datetime import datetime as dt
            try:
                start = dt.fromisoformat(phase_entry["start_timestamp"])
                end = dt.fromisoformat(now)
                phase_entry["duration_seconds"] = (end - start).total_seconds()
            except (ValueError, TypeError):
                pass

    progress["current_phase"] = phase_id
    progress["phases"][phase_id] = phase_entry

    write_json(progress_file, progress)


def aggregate_metrics(folder, pipeline_success):
    """Aggregate current run metrics into persistent pipeline-metrics.json."""
    import os

    progress_file = f"{folder}/shared/progress.json"
    # Use skill directory for persistent metrics (works across machines/platforms)
    skill_dir = os.path.dirname(os.path.abspath(__file__))  # Resolves to process-rfp-win/ at runtime
    metrics_file = f"{skill_dir}/config-win/pipeline-metrics.json"

    if not exists(progress_file):
        return

    progress = read_json(progress_file)

    # Build run summary
    run_summary = {
        "folder": progress.get("folder", folder),
        "mode": progress.get("mode", "full"),
        "pipeline_start": progress.get("pipeline_start", ""),
        "pipeline_end": datetime.now().isoformat(),
        "success": pipeline_success,
        "phase_count": len(progress.get("phases", {})),
        "phase_durations": {},
        "phase_statuses": {},
        "sva_results": {},
        "total_retries": sum(progress.get("retry_counts", {}).values()) if progress.get("retry_counts") else 0
    }

    # Extract per-phase data
    for phase_id, phase_data in progress.get("phases", {}).items():
        if "duration_seconds" in phase_data:
            run_summary["phase_durations"][phase_id] = phase_data["duration_seconds"]
        run_summary["phase_statuses"][phase_id] = phase_data.get("status", "unknown")
        # Capture SVA dispositions
        if phase_id.startswith("SVA") or phase_id.startswith("sva"):
            run_summary["sva_results"][phase_id] = phase_data.get("status", "unknown")

    # Calculate total duration
    if progress.get("pipeline_start"):
        from datetime import datetime as dt
        try:
            start = dt.fromisoformat(progress["pipeline_start"])
            end = dt.now()
            run_summary["total_duration_minutes"] = round((end - start).total_seconds() / 60, 1)
        except (ValueError, TypeError):
            pass

    # Load or initialize persistent metrics
    if exists(metrics_file):
        metrics = read_json(metrics_file)
    else:
        metrics = {
            "version": "1.0",
            "runs": [],
            "aggregates": {
                "total_runs": 0,
                "total_full_runs": 0,
                "total_sprint_runs": 0,
                "avg_total_duration_minutes": 0,
                "phase_avg_durations": {},
                "phase_failure_rates": {},
                "sva_dispositions": {},
                "outcomes_logged": 0
            }
        }

    # Append run
    metrics["runs"].append(run_summary)
    metrics["last_updated"] = datetime.now().isoformat()

    # Update aggregates
    agg = metrics["aggregates"]
    agg["total_runs"] = len(metrics["runs"])
    agg["total_full_runs"] = sum(1 for r in metrics["runs"] if r.get("mode") == "full")
    agg["total_sprint_runs"] = sum(1 for r in metrics["runs"] if r.get("mode") == "sprint")

    # Average total duration
    durations = [r["total_duration_minutes"] for r in metrics["runs"] if "total_duration_minutes" in r]
    agg["avg_total_duration_minutes"] = round(sum(durations) / len(durations), 1) if durations else 0

    # Per-phase average durations
    phase_durations_all = {}
    phase_failure_counts = {}
    phase_total_counts = {}
    for run in metrics["runs"]:
        for pid, dur in run.get("phase_durations", {}).items():
            phase_durations_all.setdefault(pid, []).append(dur)
        for pid, status in run.get("phase_statuses", {}).items():
            phase_total_counts[pid] = phase_total_counts.get(pid, 0) + 1
            if status == "failed":
                phase_failure_counts[pid] = phase_failure_counts.get(pid, 0) + 1

    agg["phase_avg_durations"] = {pid: round(sum(durs)/len(durs), 1) for pid, durs in phase_durations_all.items()}
    agg["phase_failure_rates"] = {pid: round(phase_failure_counts.get(pid, 0) / phase_total_counts[pid], 2) for pid in phase_total_counts}

    write_json(metrics_file, metrics)
    log(f"Pipeline metrics aggregated: run #{agg['total_runs']} logged to pipeline-metrics.json")


def load_integrations():
    """Load optional integration configuration. Returns empty dict if not configured."""
    import os
    skill_dir = os.path.dirname(os.path.abspath(__file__))  # The AI will resolve this
    integrations_file = f"{skill_dir}/config-win/integrations.json"

    if not exists(integrations_file):
        return {}

    try:
        data = read_json(integrations_file)
        enabled = {k: v for k, v in data.get("integrations", {}).items() if v.get("enabled")}
        if enabled:
            log(f"Integrations loaded: {', '.join(enabled.keys())} enabled")
        return data.get("integrations", {})
    except Exception:
        return {}


def check_integration_hook(integrations, hook_phase, hook_type="pre_phase"):
    """Check if any integration has a hook for the given phase. Log-only — no execution."""
    for name, config in integrations.items():
        if not config.get("enabled"):
            continue
        if config.get("hook_phase") == hook_phase and config.get("hook_type") == hook_type:
            provider = config.get("config", {}).get("provider", "unconfigured")
            log(f"  [INTEGRATION] {name} ({provider}) hook available for Phase {hook_phase}")
            log(f"  [INTEGRATION] To activate: implement adapter per INTEGRATION_GUIDE in skill-win.md")
```

### Step 4: Main Execution Loop (Stage-Aware with SVA Gates)

```python
def run_pipeline(folder, start_stage=1, end_stage=7, sprint_mode=False):
    """
    Execute the RFP processing pipeline with SVA validation gates.

    The pipeline executes stages sequentially. Within each stage, phases
    run in order (with parallel groups where configured). After all phases
    in a stage complete, the corresponding SVA validates the stage output.

    Args:
        folder: Path to RFP working folder
        start_stage: Stage to start from (1-7), for resuming after fixes
        end_stage: Stage to end at (1-7), for partial execution
        sprint_mode: If True, use consolidated Sprint pipeline (~14 units)
    """

    # Load optional integrations
    integrations = load_integrations()

    # ---- SPRINT MODE: Consolidated execution ----
    if sprint_mode:
        log("=" * 60)
        log("⚡ STARTING RFP PROCESSING PIPELINE (SPRINT MODE)")
        log(f"📂 Folder: {folder}")
        log(f"📋 ~14 execution units (consolidated from 46)")
        log(f"   Skipping Go/No-Go gate — bid decision pre-approved")
        log("=" * 60)

        # Sprint executes each sprint phase as a SINGLE Task with all subskills
        for sprint_id, sprint_stage in SPRINT_STAGES.items():
            log(f"\n{'='*60}")
            log(f"⚡ {sprint_id}: {sprint_stage['name']}")
            log(f"{'='*60}")

            # Execute all phases in this sprint phase sequentially
            for phase_id in sprint_stage["phases"]:
                phase = next((p for p in PHASES if p["id"] == phase_id), None)
                if phase:
                    # Skip advisory gate in sprint mode
                    if phase.get("advisory_gate"):
                        log(f"⏭️ Skipping Phase {phase_id} (Go/No-Go) in Sprint mode")
                        continue
                    success = execute_phase(phase, folder)
                    if not success and phase.get("blocking_gate"):
                        log(f"\033[91m⛔ Pipeline halted at blocking gate: Phase {phase_id}\033[0m")
                        return False

            # Execute pre-gate if this sprint stage has one (e.g., SVA-0 Blue Team)
            if sprint_stage.get("pre_gate"):
                pre_gate_id = sprint_stage["pre_gate"]
                pre_gate_report_file = f"{folder}/shared/validation/{sprint_stage['pre_gate_report']}"
                log(f"\n🔵 {pre_gate_id}: Sprint pre-authoring gate")
                pre_gate_prompt = f"""
You are a **Pre-Gate Validator ({pre_gate_id})** in Sprint mode.

Execute the subskill: {SKILLS_DIR}/{sprint_stage['pre_gate_subskill']}

**Working folder:** {folder}
**Report Output:** {pre_gate_report_file}

Read the subskill file and execute all validation rules. Write the report JSON.
"""
                Task(
                    prompt=pre_gate_prompt,
                    subagent_type="general-purpose",
                    description=f"{pre_gate_id}: Sprint Pre-Gate"
                )
                if exists(pre_gate_report_file):
                    pre_report = read_json(pre_gate_report_file)
                    pre_disposition = pre_report.get("disposition", "BLOCK")
                    if pre_disposition == "BLOCK":
                        log(f"\033[91m⛔ Sprint halted at {pre_gate_id} pre-gate\033[0m")
                        return False
                    elif pre_disposition == "ADVISORY":
                        log(f"⚠️ {pre_gate_id} advisory. Proceeding with caution.")
                else:
                    log(f"\033[91m❌ {pre_gate_id} report not generated — halting sprint\033[0m")
                    return False

            # Execute combined SVA if this sprint stage has one
            if sprint_stage.get("sva"):
                sva_rules = sprint_stage.get("sva_rules", [])
                log(f"\n🔍 {sprint_stage['sva']}: Combined validation ({', '.join(sva_rules)})")
                # Run the LAST SVA in the combined set (most comprehensive)
                last_sva_num = max(int(r.split("-")[1]) for r in sva_rules)
                disposition = execute_sva(last_sva_num, folder)
                if disposition == "BLOCK":
                    log(f"\033[91m⛔ Sprint halted at {sprint_stage['sva']}\033[0m")
                    return False

        result = final_verification(folder)
        aggregate_metrics(folder, result)
        return result

    # ---- FULL MODE: Standard 46-unit execution ----
    log("=" * 60)
    log("🚀 STARTING RFP PROCESSING PIPELINE (WIN Edition v2)")
    log(f"📂 Folder: {folder}")
    log(f"📊 Stages: {start_stage} through {end_stage}")
    log(f"📋 Total: 38 phases + 8 SVA gates = 46 execution units")
    log("=" * 60)

    sva_results = {}  # Track SVA dispositions

    for stage_num in range(start_stage, end_stage + 1):
        stage = STAGE_BOUNDARIES[stage_num]
        color_label = f" [{stage['color_team'].upper()} TEAM]" if stage.get('color_team') else ""

        log(f"\n{'='*60}")
        log(f"📋 STAGE {stage_num}: {stage['name']}{color_label}")
        log(f"{'='*60}")

        # ---- PRE-GATE: Execute Blue Team or similar pre-gate before phases ----
        if stage.get("pre_gate"):
            pre_gate_id = stage["pre_gate"]
            pre_gate_subskill = stage["pre_gate_subskill"]
            pre_gate_report_file = f"{folder}/shared/validation/{stage['pre_gate_report']}"

            log(f"\n🔵 {pre_gate_id}: Pre-authoring strategic readiness gate")
            pre_gate_prompt = f"""
You are a **Pre-Gate Validator ({pre_gate_id})** running before Stage {stage_num}: {stage['name']}.

Execute the subskill: {SKILLS_DIR}/{pre_gate_subskill}

**Working folder:** {folder}
**Report Output:** {pre_gate_report_file}

Read the subskill file and execute all validation rules. Write the report JSON to the output path.
"""
            Task(
                prompt=pre_gate_prompt,
                subagent_type="general-purpose",
                description=f"{pre_gate_id}: Pre-Gate Validation"
            )

            # Check pre-gate disposition
            if exists(pre_gate_report_file):
                pre_report = read_json(pre_gate_report_file)
                pre_disposition = pre_report.get("disposition", "BLOCK")
                pre_score = pre_report.get("summary", {}).get("overall_score", 0)

                if pre_disposition == "PASS":
                    log(f"✅ {pre_gate_id} PASSED (score: {pre_score}/100) — Proceeding to phases")
                elif pre_disposition == "ADVISORY":
                    log(f"⚠️ {pre_gate_id} ADVISORY (score: {pre_score}/100) — Proceeding with caution")
                else:  # BLOCK
                    log(f"\033[91m⛔ {pre_gate_id} BLOCKED (score: {pre_score}/100)\033[0m")
                    log(f"   Fix strategic readiness issues before Phase 8 authoring.")
                    log(f"   See: outputs/BLUE_TEAM_READINESS.md for details.")
                    return False
            else:
                log(f"\033[91m❌ {pre_gate_id} report not generated — halting\033[0m")
                return False

        # Get phases for this stage (excluding post_sva phases)
        stage_phases = [p for p in PHASES
                       if p.get("stage") == stage_num and not p.get("post_sva")]

        # Group phases for parallel execution within stage
        parallel_groups = {}
        execution_order = []
        seen_groups = set()

        for phase in stage_phases:
            group = phase.get("parallel_group")
            if group:
                if group not in parallel_groups:
                    parallel_groups[group] = []
                parallel_groups[group].append(phase)
                if group not in seen_groups:
                    execution_order.append(("parallel", group))
                    seen_groups.add(group)
            else:
                execution_order.append(("sequential", phase))

        # Execute phases within stage
        for exec_type, item in execution_order:
            if exec_type == "parallel":
                group_phases = parallel_groups[item]
                log(f"\n📊 Executing parallel group: {item}")
                log(f"   Phases: {', '.join(p['id'] for p in group_phases)}")

                # Launch all phases in parallel using multiple Task calls
                for phase in group_phases:
                    success = execute_phase(phase, folder)
                    if not success and phase.get("blocking_gate"):
                        log(f"\033[91m⛔ Pipeline halted at blocking gate: Phase {phase['id']}\033[0m")
                        return False
            else:
                phase = item

                # --- GO/NO-GO ADVISORY GATE (Phase 1.9) ---
                if phase.get("advisory_gate"):
                    success = execute_phase(phase, folder)
                    if success:
                        decision_file = f"{folder}/shared/GO_NOGO_DECISION.json"
                        if exists(decision_file):
                            decision = read_json(decision_file)
                            score = decision.get("total_score", 0)
                            recommendation = decision.get("recommendation", "NO_GO")

                            if recommendation == "GO":
                                log(f"✅ Go/No-Go: GO (score: {score}/100) — Proceeding with bid")
                            elif recommendation == "CONDITIONAL":
                                log(f"⚠️ Go/No-Go: CONDITIONAL (score: {score}/100)")
                                log(f"   Risk factors: {', '.join(decision.get('risk_factors', []))}")
                                log(f"   Ask user: Proceed with bid? (y/n)")
                                # Present to user for decision — pipeline pauses
                                # If user says no, halt pipeline
                            else:  # NO_GO
                                log(f"\033[91m⛔ Go/No-Go: NO-GO (score: {score}/100)\033[0m")
                                log(f"   Risk factors: {', '.join(decision.get('risk_factors', []))}")
                                log(f"   Recommendation: Do not bid on this RFP")
                                log(f"   User can override to continue")
                                # Present to user — default is halt
                    continue  # Move to next phase regardless

                # --- CONDITIONAL PHASE (Phase 1.95 - Intel, only if GO) ---
                if phase.get("conditional"):
                    decision_file = f"{folder}/shared/GO_NOGO_DECISION.json"
                    if exists(decision_file):
                        decision = read_json(decision_file)
                        if decision.get("recommendation") == "NO_GO" and not decision.get("user_override"):
                            log(f"⏭️ Skipping Phase {phase['id']}: {phase['name']} (Go/No-Go = NO-GO)")
                            update_progress(folder, phase['id'], "skipped", "Skipped due to NO-GO decision")
                            continue

                success = execute_phase(phase, folder)
                if not success and phase.get("blocking_gate"):
                    log(f"\033[91m⛔ Pipeline halted at blocking gate: Phase {phase['id']}\033[0m")
                    return False

        # ---- SVA GATE: Validate stage before proceeding ----
        disposition = execute_sva(stage_num, folder)
        sva_results[stage_num] = disposition

        if disposition == "BLOCK":
            log(f"\033[91m⛔ Pipeline halted at {stage['sva']} (Stage {stage_num})\033[0m")
            log(f"   Fix issues and re-run: start_stage={stage_num}")
            return False
        elif disposition == "ADVISORY":
            log(f"⚠️ {stage['sva']} advisory noted. Continuing to Stage {stage_num + 1}...")

        # ---- HUMAN REVIEW CHECKPOINT (Gold Team) ----
        if stage_num == 7:  # Stage 7 = Bid Generation with SVA-7
            # Read SVA-7 report for score details
            sva7_report_path = f"{folder}/shared/validation/sva7-gold-team.json"
            sva7_score = "N/A"
            if exists(sva7_report_path):
                sva7_data = read_json(sva7_report_path)
                sva7_score = sva7_data.get("summary", {}).get("overall_score", "N/A")

            # Update progress for human review
            update_progress(folder, "GOLD_TEAM_REVIEW", "AWAITING_HUMAN_REVIEW",
                f"Gold Team review complete. Disposition: {disposition}. See outputs/GOLD_TEAM_CHECKLIST.md")

            if disposition == "PASS":
                log(f"\n{'='*60}")
                log("GOLD TEAM REVIEW: PASS")
                log(f"{'='*60}")
                log(f"Score: {sva7_score}/100")
                log(f"Report: shared/validation/sva7-gold-team.json")
                log(f"Checklist: outputs/GOLD_TEAM_CHECKLIST.md")
                log(f"Recommendation: Review checklist before final submission.")
                log(f"Pipeline continuing to PDF assembly...")
                log(f"{'='*60}\n")
            elif disposition == "ADVISORY":
                log(f"\n{'='*60}")
                log("GOLD TEAM REVIEW: ADVISORY -- REVIEW RECOMMENDED")
                log(f"{'='*60}")
                log(f"Score: {sva7_score}/100")
                log(f"HIGH priority findings detected. Review outputs/GOLD_TEAM_CHECKLIST.md")
                log(f"Pipeline continuing to PDF assembly -- address findings before submission.")
                log(f"{'='*60}\n")
            # BLOCK case is already handled above (pipeline returns False)

        # Execute post-SVA phases (e.g., 8d, 8e run after SVA-7 passes)
        # ⚠️ CRITICAL: Phase 8e (PDF Assembly) is MANDATORY — pipeline is NOT complete without PDFs
        post_sva_phases = [p for p in PHASES
                          if p.get("stage") == stage_num and p.get("post_sva")]
        for phase in post_sva_phases:
            success = execute_phase(phase, folder)
            if not success:
                if phase.get("mandatory"):
                    log(f"\033[91m⛔ MANDATORY post-SVA phase {phase['id']} ({phase['name']}) FAILED — pipeline INCOMPLETE\033[0m")
                    log(f"\033[91m   PDFs are REQUIRED. Humans need PDFs, not markdowns.\033[0m")
                    return False
                else:
                    log(f"\033[91m❌ Post-SVA phase {phase['id']} failed\033[0m")

    # Display SVA summary
    log(f"\n{'='*60}")
    log("📊 SVA VALIDATION SUMMARY")
    log(f"{'='*60}")
    for stage_num, disposition in sva_results.items():
        stage = STAGE_BOUNDARIES[stage_num]
        symbol = "✅" if disposition == "PASS" else ("⚠️" if disposition == "ADVISORY" else "❌")
        color = stage.get("color_team", "").upper()
        color_label = f" [{color} TEAM]" if color else ""
        log(f"  {symbol} {stage['sva']}: {stage['name']}{color_label} → {disposition}")

    # Report [USER INPUT REQUIRED] markers
    log(f"\n{'='*60}")
    log("📝 USER INPUT REQUIRED MARKERS")
    log(f"{'='*60}")
    # Scan bid-sections for markers
    import glob
    bid_files = glob.glob(f"{folder}/outputs/bid-sections/*.md")
    markers_found = []
    for bf in bid_files:
        content = read_file(bf)
        import re
        markers = re.findall(r'\[USER INPUT REQUIRED:.*?\]', content)
        if markers:
            markers_found.append((os.path.basename(bf), markers))

    if markers_found:
        for filename, markers in markers_found:
            log(f"  📄 {filename}:")
            for marker in markers:
                log(f"     → {marker}")
        log(f"\n  Fill in {sum(len(m) for _, m in markers_found)} markers, then re-run Phase 8e for final PDFs.")
    else:
        log("  ✅ No user input markers found - bid is complete.")

    # Final verification and metrics aggregation
    result = final_verification(folder)
    aggregate_metrics(folder, result)
    return result


def final_verification(folder):
    """Verify all required outputs exist across all categories."""

    log("\n" + "=" * 60)
    log("📋 FINAL VERIFICATION")
    log("=" * 60)

    all_present = True
    results = []

    # Category 1: Specification & Documentation artifacts
    log("\n📄 Specifications & Documentation:")
    spec_outputs = [
        (f"{folder}/outputs/EXECUTIVE_SUMMARY.md", "Executive Summary", 2),
        (f"{folder}/outputs/REQUIREMENTS_CATALOG.md", "Requirements Catalog", 10),
        (f"{folder}/outputs/ARCHITECTURE.md", "Architecture Specs", 15),
        (f"{folder}/outputs/SECURITY_REQUIREMENTS.md", "Security Specs", 8),
        (f"{folder}/outputs/INTEROPERABILITY.md", "Interoperability Specs", 5),
        (f"{folder}/outputs/EFFORT_ESTIMATION.md", "Effort Estimation", 8),
        (f"{folder}/outputs/TRACEABILITY.md", "Traceability Matrix", 10),
        (f"{folder}/outputs/RTM_REPORT.md", "RTM Verification Report", 5),
        (f"{folder}/outputs/NAVIGATION_GUIDE.md", "Navigation Guide", 3),
        (f"{folder}/outputs/MANIFEST.md", "Manifest", 2),
    ]
    for path, name, min_kb in spec_outputs:
        result, ok = _check_output(path, name, min_kb)
        results.append(result)
        if not ok: all_present = False
        log(f"  {result}")

    # Category 2: Traceability & Data
    log("\n📊 Traceability & Data:")
    data_outputs = [
        (f"{folder}/shared/GO_NOGO_DECISION.json", "Go/No-Go Decision", 1),
        (f"{folder}/shared/UNIFIED_RTM.json", "Unified RTM", 10),
        (f"{folder}/shared/SUBMISSION_STRUCTURE.json", "Submission Structure", 2),
        (f"{folder}/shared/bid-context-bundle.json", "Context Bundle", 5),
        (f"{folder}/shared/bid/CLIENT_INTELLIGENCE.json", "Client Intelligence", 2),
        (f"{folder}/shared/bid/POSITIONING_OUTPUT.json", "Strategic Positioning", 3),
    ]
    for path, name, min_kb in data_outputs:
        result, ok = _check_output(path, name, min_kb)
        results.append(result)
        if not ok: all_present = False
        log(f"  {result}")

    # Category 3: Bid Section Volumes
    log("\n📝 Bid Section Volumes:")
    bid_sections = [
        (f"{folder}/outputs/bid-sections/01_SUBMITTAL.md", "Letter of Submittal", 3),
        (f"{folder}/outputs/bid-sections/02_MANAGEMENT.md", "Management Proposal", 8),
        (f"{folder}/outputs/bid-sections/03_TECHNICAL.md", "Technical Approach", 15),
        (f"{folder}/outputs/bid-sections/04_REQUIREMENTS_REVIEW.md", "Requirements Review", 5),
        (f"{folder}/outputs/bid-sections/04_RISK_REGISTER.md", "Risk Register", 3),
        (f"{folder}/outputs/bid-sections/05_FINANCIAL.md", "Financial Proposal", 5),
        (f"{folder}/outputs/bid-sections/06_INTEGRATION.md", "Technical Integration", 5),
    ]
    for path, name, min_kb in bid_sections:
        result, ok = _check_output(path, name, min_kb)
        results.append(result)
        if not ok: all_present = False
        log(f"  {result}")

    # Also check for solution files (04a_SOLUTION_*.md)
    import glob
    solution_files = glob.glob(f"{folder}/outputs/bid-sections/04a_SOLUTION_*.md")
    if solution_files:
        log(f"  ✅ Business Solutions: {len(solution_files)} section(s)")
    else:
        log(f"  ❌ Business Solutions: MISSING (04a_SOLUTION_*.md)")
        all_present = False

    # Category 4: SVA Validation Reports
    log("\n🔍 SVA Validation Reports:")
    for stage_num in range(1, 8):
        stage = STAGE_BOUNDARIES[stage_num]
        # Check pre-gate report if this stage has one
        if stage.get("pre_gate"):
            pre_path = f"{folder}/shared/validation/{stage['pre_gate_report']}"
            result, ok = _check_output(pre_path, f"{stage['pre_gate']} Report", 1)
            log(f"  {result}")
        report_path = f"{folder}/shared/validation/{stage['sva_report']}"
        result, ok = _check_output(report_path, f"{stage['sva']} Report", 1)
        log(f"  {result}")

    # Category 5: Rendered artifacts
    log("\n🖼️ Rendered Artifacts:")
    render_outputs = [
        (f"{folder}/outputs/bid/architecture.png", "Architecture Diagram", 10),
        (f"{folder}/outputs/bid/timeline.png", "Timeline Diagram", 10),
        (f"{folder}/outputs/bid/orgchart.png", "Org Chart", 5),
    ]
    for path, name, min_kb in render_outputs:
        result, ok = _check_output(path, name, min_kb)
        results.append(result)
        if not ok: all_present = False
        log(f"  {result}")

    # ⚠️ MANDATORY CHECK: PDFs MUST exist — pipeline is NOT complete without them
    pdf_files = glob.glob(f"{folder}/outputs/bid/*.pdf")
    if pdf_files and len(pdf_files) >= 7:  # At minimum: 6 volumes + Draft_Bid.pdf
        log(f"  ✅ PDF Files: {len(pdf_files)} generated (MANDATORY CHECK PASSED)")
        for pf in sorted(pdf_files):
            size_kb = os.path.getsize(pf) / 1024
            log(f"     📄 {os.path.basename(pf)}: {size_kb:.1f}KB")
    elif pdf_files:
        log(f"  ⚠️ PDF Files: Only {len(pdf_files)} generated (expected 7+)")
        for pf in sorted(pdf_files):
            size_kb = os.path.getsize(pf) / 1024
            log(f"     📄 {os.path.basename(pf)}: {size_kb:.1f}KB")
    else:
        log(f"  ⛔ PDF Files: NONE GENERATED — PIPELINE INCOMPLETE")
        log(f"     Humans need PDFs, not markdowns. Phase 8e MUST produce PDF files.")
        log(f"     Use Python markdown_pdf library if npx md-to-pdf is unavailable.")
        all_present = False

    log("\n" + "=" * 60)

    if all_present:
        log("✅ PIPELINE COMPLETE - All outputs verified")
        log(f"\n📁 Bid sections:  {folder}/outputs/bid-sections/")
        log(f"📁 Final PDFs:    {folder}/outputs/bid/")
        log(f"📁 RTM:           {folder}/shared/UNIFIED_RTM.json")
        log(f"📁 SVA reports:   {folder}/shared/validation/")
        log(f"\n💡 TIP: After bid decision is received, run Phase 9 to log the outcome.")
        log(f"   This builds historical patterns for future bid positioning.")
        return True
    else:
        log("\033[91m❌ PIPELINE INCOMPLETE - Missing or undersized outputs\033[0m")
        return False


def _check_output(path, name, min_kb):
    """Check if output exists and meets minimum size. Returns (display_string, passed)."""
    if exists(path):
        size_kb = os.path.getsize(path) / 1024
        if size_kb >= min_kb:
            return (f"✅ {name}: {size_kb:.1f}KB", True)
        else:
            return (f"⚠️ {name}: {size_kb:.1f}KB (< {min_kb}KB min)", False)
    else:
        return (f"❌ {name}: MISSING", False)


# Execute pipeline
run_pipeline(folder, sprint_mode=sprint_mode)
```

---

## Progress Display

After each phase, display progress bar grouped by stage with SVA results:

```
📊 Pipeline Progress (WIN Edition v2) - 38 phases + 8 SVA gates = 46 units
==================================================================

STAGE 1: Document Intake
  ✅ Phase 0:    Folder Organization     - 5 documents organized
  ✅ Phase 1:    Document Flattening     - 5/5 converted to markdown
  ✅ Phase 1.5:  Domain Detection        - Education (92% confidence)
  ✅ Phase 1.6:  Evaluation Criteria     - Best Value (5 factors)
  ✅ Phase 1.7:  Compliance Gate ⛔      - PASSED (47/47 mandatory)
  ✅ Phase 1.8:  Submission Structure    - 6 volumes detected
  ✅ Phase 1.9:  Go/No-Go Gate          - GO (score: 72/100)
  ✅ Phase 1.95: Client Intelligence     - 12 searches, 3 decision-makers found
  🔍 SVA-1:     Intake Validator        - PASS (92/100)

STAGE 2: Requirements Engineering [PINK TEAM]
  ✅ Phase 2a:   Workflow Extraction     - 8 workflows, 152 candidates
  ✅ Phase 2:    Requirements Extraction - 247 requirements
  ✅ Phase 2.5:  Sample Data Analysis    - 12 files mapped
  ✅ Phase 2b:   Normalization           - 235 validated
  ✅ Phase 2c:   Requirements Catalog    - REQUIREMENTS_CATALOG.md
  ✅ Phase 2d:   Coverage Gate ⛔        - 100% coverage (PASSED)
  🔍 SVA-2:     PINK TEAM REVIEW        - PASS (88/100)

STAGE 3: Specifications
  ✅ Phase 3:    Specifications          - 5/5 parallel complete
     ├── ✅ Architecture      ├── ✅ Interoperability
     ├── ✅ Security          ├── ✅ UI/UX
     └── ✅ Entity Definitions
  ✅ Phase 3g:   Risk Assessment         - 24 risks identified
  🔍 SVA-3:     Spec Validator          - ADVISORY (82/100)

STAGE 4: Traceability & Estimation [RED TEAM]
  ✅ Phase 4:    Traceability + RTM      - UNIFIED_RTM.json (235 chains)
  ✅ Phase 5:    Effort Estimation       - 12,500 hours estimated
  🔍 SVA-4:     RED TEAM REVIEW         - PASS (90/100)

STAGE 5: Documentation
  ✅ Phase 6:    Manifest + Exec Summary
  ✅ Phase 6b:   Navigation Guide
  ✅ Phase 6c:   Context Bundle          - 18 sources aggregated
  🔍 SVA-5:     Documentation Validator  - PASS (95/100)

STAGE 6: Quality Assurance [RED TEAM FINAL]
  ✅ Phase 7:    Validation + Gap Analysis - 0 HIGH gaps remaining
  ✅ Phase 7c:   Evaluator Personas      - 4 personas, 82% coverage
  ✅ Phase 7d:   Bid Scoring Model       - 72% win probability
  🔍 SVA-6:     PRE-BID GATE            - PASS (87/100)

STAGE 7: Bid Generation [GOLD TEAM]
  ✅ Phase 8.0:  Strategic Positioning
  ✅ Phase 8.1:  Letter of Submittal     - 01_SUBMITTAL.md
  ✅ Phase 8.2:  Management Proposal     - 02_MANAGEMENT.md
  ✅ Phase 8.3:  Technical Approach      - 03_TECHNICAL.md (Opus)
  ✅ Phase 8.4:  Business Solution (x3)  - 04a_SOLUTION_*.md
  ✅ Phase 8.4r: Requirements Review     - 04_REQUIREMENTS_REVIEW.md
  ✅ Phase 8.4k: Risk Register           - 04_RISK_REGISTER.md
  ✅ Phase 8.5:  Financial Proposal      - 05_FINANCIAL.md
  ✅ Phase 8.6:  Technical Integration   - 06_INTEGRATION.md
  ✅ Phase 8f:   RTM Verification        - 14/14 queries passed
  🔍 SVA-7:     GOLD TEAM REVIEW        - PASS (91/100)
  ✅ Phase 8d:   Diagram Rendering       - 3 diagrams
  ✅ Phase 8e:   Multi-File PDF Assembly - 6 PDFs generated

📝 USER INPUT MARKERS: 8 remaining (financial rates, references)
```

---

## Status Symbols

| Symbol | Meaning |
|--------|---------|
| ✅ | Phase completed successfully |
| ⏳ | Phase in progress |
| ⏸️ | Phase waiting (not started) |
| ⚠️ | Phase completed with warnings / SVA ADVISORY |
| ❌ | Phase failed (see errors) |
| ⛔ | Blocking gate / SVA BLOCK |
| 🔍 | SVA validation gate |
| 📝 | User input required |

---

## Error Handling

### Retry Notification (RED)

When a phase fails, display in RED:

```
❌ Phase 3a: Architecture Specs FAILED (attempt 1/3)
   Missing outputs: /path/to/outputs/ARCHITECTURE.md
   Retrying...
```

### Blocking Gate Failure

```
⛔ BLOCKING GATE FAILED: Phase 1.7 Compliance Gatekeeper
   Condition: All mandatory items must be addressed
   Status: 3 mandatory items unaddressed

   Options:
   1. Address the mandatory items and retry
   2. Approve gaps with user confirmation
   3. Abort pipeline
```

---

## Memory Integration

**On skill start — read:** `${CLAUDE_SKILL_DIR}/memory/`
- `gotchas.md` — known pitfalls (UTF-8 encoding, table-cell truncation, hardcoded paths, schema-conformance, fitz.Story CSS constraints, orphaned phase files, false coverage claims, etc.). Read in full before dispatching Stage 1.
- `last-audit.md` — previous audit's findings + scores. Use as delta baseline if running an audit; use as context for what was recently changed/why if running the pipeline.
- Recent dated entries (`YYYY-MM-DD-*.md`) for run-specific incidents.

**On skill end — write checklist:**
- [ ] Did a phase fail unexpectedly (NameError, FileNotFoundError, schema validation reject)? → **MUST** record file:line + cause + resolution.
- [ ] Did an SVA gate cap-hit or halt? → **MUST** record which SVA, what cap was hit, what findings remained OPEN.
- [ ] Did the executing agent improvise output (smaller than baseline, missing schema keys, dropped JSON fields)? → **MUST** record — this is the Phase-File-Improvisation regression pattern.
- [ ] Did a hardcoded path cause a runtime failure? → **MUST** record (until Tranche C hardens all phase files).
- [ ] Did UTF-8 mojibake (`â€"` in any output) appear? → **MUST** record which phase produced it; confirm the phase is using helpers from skill-win.md.
- [ ] Did `[:N]` truncation of requirement text corrupt the RTM chain or bid content? → **MUST** record file:line.
- [ ] Did fitz.Story render badly (ghost-fill, missing hr, em-dash mojibake in PDF)? → **MUST** record + cross-check `phase8e-pdf-win.md` inline CSS.
- [ ] Did the user reject a bid section or correct positioning? → **SHOULD** record what they changed and why.
- [ ] Was a new evaluator-feedback pattern discovered worth reusing? → **SHOULD** record in `patterns.md`.
- [ ] Routine run with no surprises? → Skip (do NOT write "ran successfully").

**Always:** Update `${CLAUDE_SKILL_DIR}/memory/last-audit.md` with this run's outcome IF an audit was performed (not for normal pipeline runs).

---

## Subskill Invocation Pattern

Each phase invokes its subskill via Task agent:

```python
Task(
    prompt=f"""
You are a **{expert_role}** with deep expertise in:
- {domain_expertise}

Execute the subskill: {SKILLS_DIR}/{subskill}

**Working folder:** {folder}
**Required outputs:** {required_outputs}

Read the subskill file and execute its instructions.
Create ALL required outputs before completing.
""",
    subagent_type="general-purpose",
    description=f"Phase {phase_id}: {phase_name}"
)
```

---

## Configuration Files

- **PDF Theme:** `config-win/pdf-theme.css`
- **md-to-pdf Config:** `config-win/md-to-pdf.config.js`
- **Mermaid Themes:** `config-win/mermaid-themes/*.json`
- **Hooks:** `hooks-win/phase-verification.json`
- **Company Profile:** `config-win/company-profile.json` (Resource Data, Inc. - pre-populated)
- **Past Projects:** `Past_Projects.md` (28 case studies for auto-selection by phases 8.0-8.3)
- **Case Study Template:** `config-win/case-study-template.md` (formatting guide for case study output)
- **SVA Rules Registry:** `config-win/sva-rules-registry.json` (all 7 SVA rule definitions)
- **SVA Report Schema:** `schemas/sva-report.schema.json`
- **Unified RTM Schema:** `schemas/unified-rtm.schema.json`
- **Integrations:** `config-win/integrations.json` (optional external tool hooks - CRM, proposal DB, notifications)

---

## Integration Guide (Optional)

The pipeline supports optional external tool integrations configured in `config-win/integrations.json`.
No integrations are required — the pipeline works fully standalone.

### How Integration Hooks Work

Integration hooks are **log-only checkpoints** in the pipeline. When an integration is enabled in
`integrations.json`, the pipeline logs that a hook point was reached. To actually execute integration
logic, you must implement an adapter script.

### Implementing a Custom Adapter

Each integration type expects a Python function with this signature:

```python
# CRM Adapter Example (Phase 1.95 - Client Intelligence)
def crm_adapter(client_name, config):
    """
    Fetch client data from CRM to enrich Phase 1.95 intelligence.

    Args:
        client_name: Client/organization name from RFP
        config: Integration config from integrations.json

    Returns:
        dict with keys: contacts[], past_interactions[], contract_history[], notes
    """
    import os
    api_key = os.environ.get(config["api_key_env_var"])
    # Your CRM API call here
    return {"contacts": [], "past_interactions": [], "contract_history": [], "notes": ""}


# Proposal Database Adapter Example (Phase 8.0 - Positioning)
def proposal_db_adapter(keywords, domain, config):
    """
    Search proposal database for reusable content blocks.

    Args:
        keywords: Search terms from RFP requirements
        domain: RFP domain classification
        config: Integration config from integrations.json

    Returns:
        list of {"title": str, "content_preview": str, "source_bid": str, "relevance_score": float}
    """
    import os
    api_key = os.environ.get(config["api_key_env_var"])
    # Your proposal DB API call here
    return []


# Notification Adapter Example (Global events)
def notification_adapter(event_type, message, config):
    """
    Send notification on pipeline event.

    Args:
        event_type: One of pipeline_complete, sva_block, human_review_needed, pipeline_failed
        message: Human-readable status message
        config: Integration config from integrations.json

    Returns:
        bool indicating success
    """
    import os
    webhook_url = os.environ.get(config["webhook_url_env_var"])
    # Your webhook/API call here
    return True
```

### Security Notes for Integrations

- **NEVER** store API keys or secrets in `integrations.json` — use environment variable names only
- All integration configs store `api_key_env_var` (the name of the env var), not the actual key
- Adapter scripts should validate API responses before injecting data into the pipeline
- Test adapters in dry-run mode before enabling in production bids

---

## Final Output Summary

**⚠️ THE FINAL OUTPUT IS PDF FILES — NOT MARKDOWN. If the summary below shows zero PDFs, the pipeline FAILED.**

When pipeline completes successfully:

```
=========================================================
✅ RFP PROCESSING COMPLETE (WIN Edition v2.1)
=========================================================

📄 BID PDFs (⚠️ MANDATORY — these MUST exist):
   | Volume                     | PDF File                    | Size   |
   |----------------------------|-----------------------------|--------|
   | CONSOLIDATED DRAFT         | Draft_Bid.pdf               | ~1.5MB |
   | 1. Letter of Submittal     | {Bidder}_1_SUBMITTAL.pdf    | ~200KB |
   | 2. Management Proposal     | {Bidder}_2_MANAGEMENT.pdf   | ~400KB |
   | 3. Technical Approach       | {Bidder}_3_TECHNICAL.pdf    | ~500KB |
   | 4. Business Solution        | {Bidder}_4_SOLUTION.pdf     | ~300KB |
   | 5. Financial Proposal       | {Bidder}_5_FINANCIAL.pdf    | ~350KB |
   | 6. Technical Integration    | {Bidder}_6_INTEGRATION.pdf  | ~350KB |
   | Executive Summary          | EXECUTIVE_SUMMARY.pdf       | ~250KB |

🔍 SVA Validation Summary:
   | Gate    | Team   | Score  | Disposition |
   |---------|--------|--------|-------------|
   | SVA-1   | -      | 92/100 | PASS        |
   | SVA-2   | PINK   | 88/100 | PASS        |
   | SVA-3   | -      | 82/100 | ADVISORY    |
   | SVA-4   | RED    | 90/100 | PASS        |
   | SVA-5   | -      | 95/100 | PASS        |
   | SVA-6   | RED    | 87/100 | PASS        |
   | SVA-7   | GOLD   | 91/100 | PASS        |

📊 Traceability (UNIFIED_RTM.json):
   - Requirements: 235 (100% traced to specs)
   - Mandatory items: 47 (100% traced to bid)
   - Complete chains: 228/235 (97%)
   - RTM Verification: 14/14 queries passed

📊 Statistics:
   - Requirements extracted: 247 → 235 normalized
   - Specifications generated: 6
   - Bid volumes: 6 + appendices
   - Diagrams rendered: 3
   - SVA gates passed: 7/7
   - Total pipeline time: ~25m

📝 User Input Markers: 8 remaining
   → Fill in markers and re-run Phase 8e for updated PDFs

📁 FINAL PDFs:    {folder}/outputs/bid/ (⚠️ THIS IS THE PRIMARY DELIVERABLE)
📁 Bid sections:  {folder}/outputs/bid-sections/ (source markdown for reference)
📁 RTM:           {folder}/shared/UNIFIED_RTM.json
📁 SVA reports:   {folder}/shared/validation/

⚠️ If outputs/bid/ contains ZERO PDFs, the pipeline FAILED.
   Phase 8e MUST execute and produce PDF files. Re-run if necessary.
```
