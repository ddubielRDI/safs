# process-rfp-screen — Gotchas

Known pitfalls discovered through use or audit. Read before running the pipeline.

---

## `services` in company-profile.json is a DICT, not a list

**Status:** ACTIVE
**First observed:** unknown (captured from project memory MEMORY.md)
**Symptom:** `TypeError` or wrong scoring when Phase 2 iterates `services` expecting a flat list.
**Cause:** `services` is keyed by category (e.g., `cloud`, `data`, `dev`) with arrays of service names as values.
**Resolution:** Flatten before iteration:
```python
services = [svc for cat in company_profile["services"].values() for svc in cat]
```

---

## `locations` are dicts with city/state, not flat strings

**Status:** ACTIVE
**Symptom:** Geographic-proximity scoring (Phase 4) breaks when treating locations as strings.
**Resolution:** Each entry has `{"city": ..., "state": ...}`. Format display with `f"{loc['city']}, {loc['state']}"`.

---

## Go/No-Go output field is `recommendation`, not `decision`

**Status:** ACTIVE
**Symptom:** Phase 5 / Phase 6 `KeyError: 'decision'` when reading `go-nogo-score.json`.
**Resolution:** The field is `recommendation` (GO | CONDITIONAL | NO_GO). Treat any code/doc referencing `decision` as bug.

---

## Phase 0 documents are MOVED, not copied

**Status:** ACTIVE
**Symptom:** Source files disappear from input folder; users expect copies.
**Cause:** Phase 0 moves docs to `{folder}/original/` to enforce a clean intake state.
**Resolution:** This is intentional. Document it in user-facing messages; do not change to copy without explicit user direction.

---

## .NET 8.0 LTS EOL: 2026-11-10

**Status:** ACTIVE
**Why this matters here:** Phase 2 (Go/No-Go) and Phase 4.5 (themes) reference tech stacks. NEVER propose .NET 8.0 for projects starting in 2026+ — score the bid down on Technical Capability and flag in risk assessment.

---

## Skill audit 2026-05-13 (RESOLVED — see audit history)

**Status:** RESOLVED
**What was fixed:**
- Malformed frontmatter (was after H1; moved to line 1)
- Missing `created`, `updated` dates
- Missing `Triggers on:` phrases (added 9 trigger variants)
- Hardcoded `/path/to/safs/...` placeholders (replaced with `${CLAUDE_SKILL_DIR}` resolution)
- Missing `## Memory Integration` section (added)
- Missing `memory/` folder (this file is the seed)
- Over-broad `allowed-tools` (removed unused `Edit`, `WebFetch`)
- Added `disable-model-invocation: true` (multi-phase pipeline with destructive writes shouldn't auto-trigger)

---

## Phase file line counts exceed 500-line cap (MONITORING)

**Status:** MONITORING
**Observation:** phase1-summary (1310), phase4-compliance (1020), phase4.5-themes (838), phase5.5-questions (655), phase2-gonogo (592), phase5-recommendation (581), phase6-pdf (1701) all exceed the 500-line heuristic.
**Decision:** Not fragmenting now — these are procedural runbooks with embedded prompts and Python; extraction would harm flow.
**Promotion criterion:** If a phase file becomes unreadable in practice (Claude misses sections, edits are error-prone), revisit and extract longest blocks to `reference/`.

---

## Traceability discipline — six anti-patterns caught during 2026-05-13 audit

**Status:** ACTIVE
**Origin:** Post-run traceability audit of the NOAA NMFS Alaska RFQ (1305M326Q0143) BID_SCREEN.docx caught six honest-but-imprecise patterns. The overall recommendation (GO/84) was unchanged after correction, but each pattern would have weakened evaluator trust if shipped. Apply these checks in every future run.

### Rule 1: Distinguish contracting client from end-user agency
**Symptom:** Treating "work delivered via prime contractor X for end-user agency Y" as "direct Y past performance."
**Real example:** ORCA tablet app was delivered by RDI through its **PSMFC** partnership for use by **NOAA WCROP** observers. PSMFC was the contracting client; NOAA was the program beneficiary. Calling this "direct NOAA past performance" overstates the contracting relationship.
**How to apply (Phase 3 / Phase 4 / Phase 4.5):**
- When a past project's end-user differs from its contracting client, label evidence with both: `"end-user: <agency>; contracting client: <prime>"`
- Use "mission-adjacent via <prime>" framing, not "direct <agency> past performance," unless RDI held the underlying contract directly
- In `client-intel-snapshot.json:resource_data_noaa_history[]` and `preliminary-themes.json:evidence[]`, require the contracting-client field

### Rule 2: Regional / partial awards must retain their scope qualifier
**Symptom:** Stating "Top Workplaces #48 (2025)" when the source says "Top Workplaces #48 Midsize Employers — Southwest WA/OR, The Oregonian." Dropping the regional qualifier implies national recognition.
**How to apply (Phase 4 / Phase 4.5):**
- Verbatim transcription of award scope (national vs regional vs industry-specific) is mandatory
- In `compliance-check.json:awards[]` and theme evidence/proof_metric fields, always include the scope qualifier
- If `Past_Projects.md` award text contains region, year, sponsor, or category modifiers, preserve all of them

### Rule 3: Internal estimates must disclose methodology and not exceed source math
**Symptom:** Writing "Estimated annual value $1.2M–$1.6M" without (a) labeling it as an estimate, (b) disclosing methodology, or (c) ensuring the upper bound is supported by the underlying calculation. Audit found the upper bound was ~15% above what the cited rates+LoE actually produce.
**How to apply (Phase 1 / Phase 2 / Phase 5):**
- Any monetary or quantitative figure NOT stated in the RFP must be labeled `[estimate — methodology: <source rates × LoE / what>]`
- Compute the actual bounds from the cited inputs and use those — never round up the upper bound for narrative effect
- In `rfp-summary.json:estimated_value`, if RFP doesn't disclose, the field must start with `"Not disclosed in RFP. Internal estimate using ..."`

### Rule 4: Single-source claims need attribution every use
**Symptom:** Citing a striking figure (e.g., "45% IT vacancy") from a single news article once with attribution, then reusing it elsewhere as bare fact. Each subsequent mention loses the source provenance.
**How to apply (Phase 3 / Phase 5 / Phase 4.5):**
- When a fact has only ONE source (news article citing internal memo, single industry analyst report, single LinkedIn post), tag it as `single_source: true` in the JSON
- Every downstream consumer of that fact MUST include attribution inline (e.g., "per Alaska Beacon, April 2025") — not just the first appearance
- In the DOCX render, the attribution should appear in the same sentence as the figure, every time

### Rule 5: Past-project scoring must rely on documented evidence
**Symptom:** Scoring PSMFC `contract_type` = 3 (max — exact match) when Past_Projects.md does not document PSMFC's contract mechanism. PSMFC operates on cooperative agreements with NOAA; whether RDI's underlying contracts are T&M, FFP, or grant-funded is not stated in the source. Score 3 implies evidence that doesn't exist.
**How to apply (Phase 4b):**
- For each scoring criterion, if the source document doesn't contain the evidence the criterion measures, score 0 or 1 (general/inferred), not the max
- Add a `_score_evidence_note` field to `score_breakdown` when a criterion is awarded based on inference rather than verbatim source text
- The total relevance score must equal the sum of breakdown values — verify after every edit (already in phase4 quality checklist; enforce more strictly)

### Rule 6: Facts vs. inferred motivations
**Symptom:** Stating "NOAA wants broader competition" or "incumbent graduated from 8(a)" as if the buyer's reasoning were disclosed, when only the procurement-mechanism change is verified.
**How to apply (Phase 3 / Phase 5):**
- Any claim about the buyer's intent, motivation, or strategy that is not explicitly stated in the RFP or a citable buyer-side source must be labeled `[inference]`
- Use phrasing: "The shift from X to Y is verified; the buyer's motivation is inference (possibilities include A, B, C)" — never assert one motivation as fact
- In `client-intel-snapshot.json:strategic_implications[]`, factual changes and inferred motivations must be sentence-separated and labeled

### Audit gate (mandatory before Phase 6 docx render)

Before generating `BID_SCREEN.docx`, run a verification pass on the assembled `BID_SCREEN.json`:
1. Every monetary figure not from RFP → has `[estimate]` or methodology label
2. Every award/credential → preserves source-document scope qualifier
3. Every past-performance claim where contracting client ≠ end-user → carries both labels
4. Every single-source fact → attributed in every appearance
5. Every score_breakdown → criterion-by-criterion evidence is documented or score = 0/1
6. Every claim about buyer intent not in RFP → labeled `[inference]`

If any of the six checks fail, halt Phase 6, fix the underlying JSON, then proceed.
