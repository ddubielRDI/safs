# Memory: process-rfp-win

This folder tracks execution patterns, gotchas, and audit history for the `/process-rfp-win` executable pipeline skill.

## Convention

- **Path:** `${CLAUDE_SKILL_DIR}/memory/`
- **Isolation:** Scoped to `process-rfp-win` only. Do not read/write the sibling `/process-rfp-screen` memory folder — both pipelines maintain independent histories even though they share `config-win/company-profile.json` and `Past_Projects.md`.
- **File naming:**
  - `gotchas.md` — accumulated known pitfalls (read before every run)
  - `last-audit.md` — most recent `/skills-excellence-update` audit findings + scores (delta baseline)
  - `patterns.md` — reusable evaluator-feedback patterns discovered across runs (read before authoring Stage 7 bid sections)
  - `YYYY-MM-DD-topic.md` — incident-specific entries (e.g., `2026-05-18-mojibake-incident.md`)

## What to Track (Executable Pipeline Skills)

- **Pipeline failures:** Which stage / phase / SVA cap-hit, the OPEN finding text, the resolution applied. Include file:line if a phase file change resolved it.
- **Schema-fidelity regressions:** When a phase JSON output dropped a required key, was renamed, or restructured. Note which downstream SVA caught it (or didn't — that's worse).
- **UTF-8 mojibake recurrence:** Any appearance of `â€"` (em-dash mojibake) in any output. Cross-check that the phase that produced it used the helpers from `skill-win.md` "Required Helper Functions" block (not a locally-defined bare-open helper).
- **`[:N]` truncation in material fields:** When a `[:N]` slice corrupted a JSON-stored canonical value (e.g., RTM `requirement_text`, bundle `text` field). The screen pipeline regressed on this 2026-05-18; win was identified as carrying analogous instances in `phase4-traceability-win.md`, `phase6c-context-bundle-win.md`, `phase8.4r-reqreview-win.md`.
- **fitz.Story PDF render bugs:** Ghost-fill from `background-color` on block elements, broken `hr` rendering, em-dash mojibake in PDF body, missing borders that shouldn't render. Cross-check `phase8e-pdf-win.md` inline CSS (`PROFESSIONAL_CSS`, `COVER_CSS`) — that is the authoritative source for the fitz.Story render path; `config-win/pdf-theme.css` is for the `npx md-to-pdf` (Chromium) fallback.
- **Hardcoded path failures:** Until Tranche C fully replaces `/home/ddubiel/repos/safs/...` references across phase files, any FileNotFoundError tracing to those literal paths goes here. Capture phase + line.
- **Orphan / ghost file confusion:** When an executing agent reads a phase file that's not in the PHASES array (`phase8a/8b/8c-win.md`, `phase8.0a-intel-win.md`, `phase8-bid-author-win.md`) and produces output to deprecated paths. Cross-reference Tranche F decisions.
- **SVA report schema non-conformance:** SVA-0 (Blue Team) is known to fail `schemas/sva-report.schema.json` validation (validator pattern `^SVA-[1-7]$` excludes SVA-0; color_team enum excludes "blue"). Tranche G fix pending.
- **False-attestation patterns:** `phase6c-context-bundle-win.md:130` hardcodes `coverage_claim: "100% of requirements addressed"`. Until Tranche D fix lands, watch for downstream bid sections inheriting this claim.
- **Evaluator feedback** (post-debrief): What positioning landed; what got marked down; which past projects scored well/poorly. These graduate to `patterns.md` after 2-3 confirmations.
- **User corrections** mid-pipeline: When the user halts at an SVA gate and rewrites bid content, note what they changed.

## What NOT to Track

- Reference material (use `config-win/` or external docs)
- Skill instructions (update `skill-win.md` or the relevant `phases-win/*.md` directly)
- Shared constraints (use `.claude/constraints.md` in the project root)
- Cross-skill learnings (cross-reference `process-rfp-screen/memory/gotchas.md` when patterns apply to both pipelines, but capture them in BOTH skills' memory folders since they are independent)
- Routine successes (do NOT write "ran successfully" — empty memory turn is fine)

## Lifecycle

- **On skill start:** Read `gotchas.md` and `last-audit.md` for pre-run context. Cross-read `process-rfp-screen/memory/gotchas.md` if the screen pipeline ran in this same session (some learnings cross over).
- **On skill end:** If anything noteworthy happened (per the Memory Integration write-checklist in `skill-win.md`), append to the appropriate file.
- **On audit:** `/skills-excellence-update --skill process-rfp-win` reviews this folder for health, staleness, and graduation candidates.
- **On graduation:** When a MONITORING entry has recurred 3+ times, Phase 5 of the audit promotes it into `skill-win.md` (or the relevant phase file) as a permanent rule, and marks the source entry `GRADUATED — promoted to <file> on YYYY-MM-DD`.

## Related skill memory

- `../../process-rfp-screen/memory/gotchas.md` — sibling pipeline's accumulated knowledge. Many gotchas (UTF-8 encoding, table truncation, traceability rules) apply identically to both pipelines.
- `../../process-rfp-screen/memory/2026-05-14-regression-incident.md` — the phase-file-improvisation incident that drove the Read-Before-Execute Gate (now also enforced in win's Execution Discipline block).
