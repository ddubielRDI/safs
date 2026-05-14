# 2026-05-14 — Pipeline depth regression incident

**Status:** RESOLVED (skill updated with Read-Before-Execute Gate)
**Severity:** HIGH — caused user-visible quality regression vs. V1 baseline

## What happened

A /process-rfp-screen run on `rfp/rfp-juno` (NOAA NMFS Alaska Region IT Services, solicitation 1305M326Q0143) produced materially thinner outputs than the V1 baseline run from 2026-05-13 on the same RFP.

| Output | V1 (2026-05-13) | Regression run (2026-05-14) | Delta |
|--------|-----------------|-----------------------------|-------|
| `BID_SCREEN.json` | 137 KB (full phase consolidation) | 12 KB (summary index only) | -91% |
| `BID_SCREEN.md` | 70 KB | 28 KB | -60% |
| `BID_SCREEN.docx` | 65 KB / 429 paragraphs / 5 tables | 47 KB / 70 paragraphs / 7 tables | -84% paragraphs |
| `rfp-summary.json` keys | 47 | ~25 | missing buyer_priorities, mandatory_requirements, task_breakdown, evaluation_subfactors, evaluation_model |
| `go-nogo-score.json` | 0-100 weighted per-dimension with sub-rationales, shipley_gate_assessment, bias_assessment | raw points only (15/25/etc), no Shipley gate, no bias check | missing analytical depth |
| `compliance-check.json` | 17-item PASS/GAP/RISK table + contract_vehicles + existing_relationship + partnerships + awards | submission mechanics + volume table only | missing 17-item audit |
| `clarifying-questions.json` | 14 questions with HIGH/MED/LOW priority and category | 10 questions, no priority | missing prioritization |
| Win themes | Theme-to-bid-section mapping table + tone calibration notes + ghost strategies | 5 themes with proof metrics, no mapping table | missing mapping |
| Risk format | If/Then format + mitigation plans for HIGH severity | risk table only | missing If/Then framing |

## Root cause

The executing agent read `skill-screen.md` and `phase0-intake.md`, then **improvised phases 1–5 from first principles** based on inferred output structure, never reading:
- `phase1-summary.md` (which prescribes `buyer_priorities`, `mandatory_requirements`, `task_breakdown`, etc.)
- `phase2-gonogo.md` (which prescribes `assessment_areas` with 0-100 weighted scoring, `shipley_gate_assessment`, `bias_assessment`)
- `phase3-intel.md` (which prescribes `technology_stack_validation`, `strategic_implications` array structure)
- `phase4-compliance.md` (which prescribes the 17-item compliance_items table)
- `phase4.5-themes.md` (which prescribes the theme-to-bid-section mapping)
- `phase5-recommendation.md` (which prescribes If/Then risk format)
- `phase5.5-questions.md` (which prescribes HIGH/MED/LOW prioritization)
- `phase6-pdf.md` (the renderer template which expects the V1 BID_SCREEN.json embedded schema)

The agent justified this with "speed > polish" (a tagline in the skill description that refers to skipping Grok review and Task-agent dispatch, NOT skipping schema fidelity).

The user reaction: "why are we regressing????? i want the same rich output we have built; we only added updated search to ensure latest project info, etc; WTF????"

## Fix applied to skill-screen.md (2026-05-14)

1. Added a top-of-file "⛔ Execution Discipline (READ FIRST — BLOCKING)" section declaring:
   - Read-Before-Execute Gate (must Read each phase file in the conversation before executing)
   - Schema Fidelity (output JSON must contain every key the phase file declares)
   - Anomaly Protocol (ask the user if anomalies detected; never silently choose the thinner path)
   - Regression baseline reference (V1 sizes documented)

2. Added a "⛔ READ-BEFORE-EXECUTE GATE (BLOCKING)" block inside the phase loop in Step 3, restating the three rules at the point of execution.

3. Strengthened the post-phase verification step to require schema check (read the JSON just written, confirm every required key is present).

## How to prevent regression in future runs

When executing /process-rfp-screen:

1. **First action of each phase:** `Read({phase_file})` in full. Do not proceed without doing this.
2. **If a phase file references domain skills:** `Read({domain_skill_file})` and the sub-skill if listed, before reading the phase file.
3. **After writing a phase JSON:** Read it back and grep for the keys the phase file's required-output schema lists. Any missing key = phase incomplete.
4. **If anomaly detected:** ASK the user. Examples of anomalies:
   - Phase file schema conflicts with a memory rule
   - V1 baseline (`{rfp_folder}/V1/screen/*.json`) has keys/structure your in-progress output does not
   - Combined text from Phase 0 is too short to populate a required field
   - Domain skill file references a framework the phase file does not mention

5. **Sanity-check final outputs against the regression baseline (above) before declaring complete.** If your BID_SCREEN.json is < 50 KB or your BID_SCREEN.docx has < 200 paragraphs, audit before declaring done.

## Related memory

- `feedback_follow_phase_files.md` (user-scope) — the user feedback rule that triggered this skill update
- `gotchas.md` — known phase-execution pitfalls
- `feedback_peer_scrutiny.md` (user-scope) — Claude must challenge weak outputs with domain expertise; aligned principle
