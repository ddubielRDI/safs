# update-past-projects — Gotchas

Known pitfalls discovered through use or audit. Read before running the refresh.

---

## Backups must precede any edit — atomic write order is non-negotiable

**Status:** ACTIVE
**Cause:** Without backup-before-write, a malformed LLM synthesis can corrupt a 1200+ line source-of-truth document with no rollback path.
**Resolution:** The Step 0 sequence (validate args → stale check → backup → THEN edit) is the canonical order. Never reorder. If the backup write fails, ABORT — do not proceed with edits.

---

## LLM synthesis can hallucinate project clients / metrics / quotes

**Status:** ACTIVE
**Symptom:** A web result mentioning "Resource Data" tangentially (e.g., a third-party blog citing them) produces a synthesis proposal with a fabricated client name or invented outcome metrics.
**Resolution:**
- Require AT LEAST 2 independent web sources per new project proposal (Rule 4 from process-rfp-screen traceability discipline).
- If only 1 source exists, the proposal MUST carry `warnings: ["single-source"]` so a human can review.
- Never invent metrics, dates, or quotes. If the web result doesn't cite them verbatim, leave the field `null`.

---

## Regional award scope qualifiers are silently dropped without explicit instruction

**Status:** ACTIVE
**Symptom:** Web sources state "Top Workplaces #48 Midsize Employers — Southwest WA/OR, The Oregonian" but synthesis returns "Top Workplaces #48 (2025)" — losing the regional scope, the size category, and the publisher.
**Resolution:** The Company Intelligence synthesis prompt explicitly requires `"preserve regional / partial-award scope qualifiers verbatim"`. When the LLM still drops them, mark `warnings: ["regional-qualifier-dropped"]` and require human review.

---

## Contract status "unverifiable" is NOT the same as "expired"

**Status:** ACTIVE
**Symptom:** A contract ID returns no recent web results; the synthesis is tempted to mark it "expired" by default.
**Resolution:** "No web evidence" means UNVERIFIABLE, not expired. Government contracts often have non-public modification histories. Default to `status: "unverifiable"` and leave the entry unchanged. Only mark `"expired"` when web evidence affirmatively states the contract ended.

---

## Stale-check window (7 days) is a heuristic — short for active development, longer for steady-state

**Status:** MONITORING
**Reason for current value:** During active bid pipelines, 7 days is a reasonable freshness window — multiple screens per week shouldn't each spend 5 minutes re-verifying the same data.
**Promotion criterion:** If users frequently invoke `--skip-stale-check` because they want fresher data, shorten the window. If users complain about excess refreshes during quiet periods, lengthen it. Track override frequency.

---

## Section regex parsing is brittle to header reformatting in Past_Projects.md

**Status:** ACTIVE
**Symptom:** If a maintainer renames a section heading (e.g., "Company Intelligence" → "Company Profile"), every Step 1 parser silently returns None, every scope quietly does nothing, and the refresh log says "0 proposals."
**Resolution:**
- The Step 1 parser logs which sections were located. If "Sections present" is missing any expected section, the operator should be alerted.
- For long-term robustness, replace the heading regex with a more flexible parser (allow synonyms, fuzzy match) — promoted to skill body if section drift becomes recurrent.

---

## `--dry-run` proposals file is not git-tracked by default

**Status:** ACTIVE
**Symptom:** A user runs `--dry-run`, reviews the proposals, then runs without `--dry-run` to apply — but if they edit `Past_Projects.proposals.md` directly hoping it lands in the source, nothing happens. Proposals are advisory only.
**Resolution:** Proposals are computed fresh on each run; manual edits to `Past_Projects.proposals.md` are never read back. The canonical way to customize edits is to (a) review the dry-run output, (b) decide which scopes / proposals to accept, (c) re-invoke with adjusted `--scope=` or accept all via no-flag run.
