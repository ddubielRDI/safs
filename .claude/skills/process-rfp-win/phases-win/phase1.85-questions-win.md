---
name: phase1.85-questions-win
expert-role: Procurement Strategist
domain-expertise: RFP ambiguity resolution, government procurement Q&A protocols
---

# Phase 1.85: Clarifying Questions (Stage 1, before 1.9 Go/No-Go)

## Purpose

Surface ambiguities in the RFP into a deadline-aware, prioritized question list for user review and (where applicable) submission to the procuring agency BEFORE the RFP's questions-deadline expires. Running this BEFORE Phase 1.9 Go/No-Go means an already-passed questions deadline becomes a documented input to the no-go calculation — not a post-hoc discovery.

Government and enterprise RFPs almost always permit a written-questions window (typically 2-4 weeks after issuance, closing 1-2 weeks before bid due). Missing that window is a strategic loss: ambiguity costs go from "free clarification" to "absorbed risk in bid price". This phase makes the cost visible early.

## Expert Role

You are a **Procurement Strategist** with deep expertise in:

- Reading SHALL / MUST / SHOULD requirement language with a lawyer's eye for ambiguity
- Government procurement Q&A protocols (FAR Part 15, state procurement codes, EU procurement directives)
- Question-framing techniques that elicit useful answers without telegraphing competitive position
- Triage discipline — Critical (must answer to bid), Important (changes scope/price), Nice-to-have (clarifies but not blocking)

## Inputs

- `{folder}/shared/COMPLIANCE_MATRIX.json` — mandatory items list (each may have ambiguity)
- `{folder}/shared/SUBMISSION_STRUCTURE.json` — for the questions-deadline status
- `{folder}/shared/EVALUATION_CRITERIA.json` — to see what factors get scored (gaps here = bid risk)
- `{folder}/shared/requirements-raw.json` *(if exists)* — raw requirements text for ambiguity scans
- `{folder}/shared/domain-context.json` — for domain-specific ambiguity patterns
- `{folder}/flattened/*.md` — full RFP text for sentence-level scanning

## Required Outputs

- `{folder}/outputs/CLARIFYING_QUESTIONS.md` (≥2 KB) — human-reviewable, deadline-aware question list

## Honor the Submission Schedule

`SUBMISSION_STRUCTURE.json` should contain the questions-deadline status. Possible states:

- `questions_deadline: "open"` with `deadline_date: <future>` — produce the full question list with a clear deadline reminder.
- `questions_deadline: "closing_soon"` (e.g., <= 5 business days) — highlight Critical questions; deprioritize Nice-to-have.
- `questions_deadline: "closed"` — produce the list anyway, but flag every Critical/Important question as a Phase 1.9 risk input ("This ambiguity must now be absorbed in the bid price/scope").
- `questions_deadline: "none"` (no Q&A allowed) — produce the list as an internal risk register; mark all entries `internal_only: true`.

## Instructions

### Step 1: Load Inputs

```python
import os, glob, re
from datetime import datetime

compliance      = read_json_safe(f"{folder}/shared/COMPLIANCE_MATRIX.json") or {}
submission      = read_json_safe(f"{folder}/shared/SUBMISSION_STRUCTURE.json") or {}
evaluation      = read_json_safe(f"{folder}/shared/EVALUATION_CRITERIA.json") or {}
requirements    = read_json_safe(f"{folder}/shared/requirements-raw.json") or {}
domain_context  = read_json_safe(f"{folder}/shared/domain-context.json") or {}

flattened_files = glob.glob(f"{folder}/flattened/*.md")
flattened_text  = ""
for fp in flattened_files:
    flattened_text += read_file(fp) + "\n\n"
```

### Step 2: Resolve Deadline Posture

**⛔ CANONICAL TOKEN SET (codified 2026-05-20 — MARS Phase 1.85 incident):**
`deadline_status` written to `clarifying-questions-summary.json` MUST be one of EXACTLY these eight lowercase tokens:
`{"open", "closing_soon", "imminent", "closed", "expired", "none", "not_permitted", "unknown"}`.

SUBMISSION_STRUCTURE.json uses upstream event-stage markers (e.g., `status: "PASSED"`, `status: "UPCOMING"`) that are NOT in this set. The canonicalization map below MUST be applied — never write event-stage values like `"passed"` directly. The Phase 1.85 verifier's Check 4 enforces this set explicitly; a mismatch is a CRITICAL FAIL that halts the pipeline.

```python
deadline_block  = submission.get("questions_deadline") or {}
# SUBMISSION_STRUCTURE schema: questions_deadline may be either a plain string
# (legacy) or a nested {status, date_iso, time, days_relative_to_today} dict.
# Handle both shapes.
if isinstance(deadline_block, str):
    raw_status = deadline_block
    deadline_date = submission.get("questions_deadline_date")
    days_relative = None
elif isinstance(deadline_block, dict):
    raw_status = deadline_block.get("status", "UNKNOWN")
    deadline_date = deadline_block.get("date_iso") or deadline_block.get("date")
    days_relative = deadline_block.get("days_relative_to_today")
else:
    raw_status = "UNKNOWN"
    deadline_date = None
    days_relative = None

# Canonicalize to the closed token set (MANDATORY — see header note above).
# event-stage markers (PASSED, UPCOMING, NA) → canonical states based on the
# date_relative signal when available, otherwise on the literal token.
CANONICAL_STATUS_MAP = {
    # Event-stage markers from SUBMISSION_STRUCTURE
    "PASSED": "closed",       # deadline date has passed
    "EXPIRED": "expired",
    "UPCOMING": "open",       # future date but unspecified urgency
    "IMMINENT": "imminent",
    "CLOSING_SOON": "closing_soon",
    "OPEN": "open",
    "NA": "not_permitted",
    "NOT_PERMITTED": "not_permitted",
    "NONE": "none",
    "UNKNOWN": "unknown",
    # Already-canonical tokens
    "closed": "closed", "expired": "expired", "open": "open",
    "imminent": "imminent", "closing_soon": "closing_soon",
    "none": "none", "not_permitted": "not_permitted", "unknown": "unknown",
}
deadline_status = CANONICAL_STATUS_MAP.get(str(raw_status).strip(), "unknown")

# If the upstream marker was "UPCOMING" but days_relative is small/negative,
# upgrade urgency. Avoids stale "open" labels when the deadline is actually close.
if deadline_status == "open" and isinstance(days_relative, int):
    if days_relative < 0:
        deadline_status = "closed"
    elif days_relative <= 2:
        deadline_status = "imminent"
    elif days_relative <= 5:
        deadline_status = "closing_soon"

# Defensive assertion — refuse to proceed if the token isn't canonical.
_ALLOWED_TOKENS = {
    "open", "closing_soon", "imminent", "closed", "expired",
    "none", "not_permitted", "unknown",
}
assert deadline_status in _ALLOWED_TOKENS, (
    f"Phase 1.85 contract violation: deadline_status={deadline_status!r} "
    f"is not in the canonical set {_ALLOWED_TOKENS}. raw_status={raw_status!r}. "
    f"Update CANONICAL_STATUS_MAP above to handle this upstream token."
)

# Mode follows from canonical status.
mode = "open"
if deadline_status in ("closed", "expired"):
    mode = "post_deadline"
elif deadline_status in ("closing_soon", "imminent"):
    mode = "triage_critical"
elif deadline_status in ("none", "not_permitted"):
    mode = "internal_only"

log(f"Question deadline posture: {mode} (status={deadline_status}, "
    f"raw_upstream={raw_status!r}, date={deadline_date}, days={days_relative})")
```

**⛔ EVIDENCE-SNIPPET MOJIBAKE SCRUB (codified 2026-05-20 — MARS Phase 1.85 Q-I11 incident):**
Every evidence snippet captured from `flattened_text` for inclusion in `outputs/CLARIFYING_QUESTIONS.md` MUST be passed through `scrub_mojibake(snippet, source_hint=f"Q-{qid}.evidence")` before being written. The MARS run shipped two U+FFFD `�` characters in Q-I11's `ADEQUATE � Response meets...` evidence quote, traceable to upstream font-mapping loss in the RFP §5.2 rubric. The scrub helper (defined in `skill-win.md`) repairs the common patterns; residuals MUST be replaced with ` -- ` (ASCII-safe em-dash) or annotated `[?]` before the snippet leaves Phase 1.85.

### Step 3: Scan for Ambiguity Patterns

Run sentence-level passes over the flattened RFP text. Each pattern emits a candidate question with a category and a draft.

```python
candidates = []  # each: {category, severity, draft_question, evidence_snippet, source_section}

# --- Pattern A: Undefined acronyms / system names ---
# Look for ALL-CAPS tokens that appear without an expansion within 80 chars on first occurrence.
ALLCAPS = re.compile(r"\b([A-Z]{3,8})\b")
seen_acronyms = set()
for m in ALLCAPS.finditer(flattened_text):
    acro = m.group(1)
    if acro in seen_acronyms:
        continue
    seen_acronyms.add(acro)
    window = flattened_text[max(0, m.start() - 80):m.end() + 80]
    if "(" not in window and " is " not in window.lower() and "stands for" not in window.lower():
        # No expansion or definition in immediate context — candidate question
        candidates.append({
            "category": "Definition",
            "severity": "Important",
            "draft_question": f"Please provide the definition or expansion of the acronym \"{acro}\" as used in the RFP, and identify which sections it applies to.",
            "evidence_snippet": window.strip()[:300],
            "source_section": None,
        })

# --- Pattern B: Vague quantifiers ---
VAGUE = ["as appropriate", "as needed", "as required", "where applicable",
         "reasonable", "industry standard", "adequate", "sufficient",
         "timely", "promptly", "minor", "significant"]
for vague_phrase in VAGUE:
    # Use word boundary + case-insensitive
    pat = re.compile(r"\b" + re.escape(vague_phrase) + r"\b", re.IGNORECASE)
    for m in pat.finditer(flattened_text):
        snippet = flattened_text[max(0, m.start() - 120):m.end() + 120].strip()
        candidates.append({
            "category": "Quantification",
            "severity": "Important",
            "draft_question": (
                f"The phrase \"{vague_phrase}\" appears in the requirement context shown below. "
                f"Please specify the measurable threshold or acceptance criterion you will use to "
                f"evaluate compliance, e.g., a specific number, frequency, or service-level value."
            ),
            "evidence_snippet": snippet[:300],
            "source_section": None,
        })

# --- Pattern C: Conflicting SHALL/MUST language ---
# Look for sentences containing both "shall" and "may" or "should" — these often indicate
# unclear obligation level.
SENT_SPLIT = re.compile(r"(?<=[.!?])\s+")
sentences = SENT_SPLIT.split(flattened_text)
for s in sentences:
    sl = s.lower()
    if "shall" in sl and ("may " in sl or "should " in sl):
        candidates.append({
            "category": "Obligation level",
            "severity": "Critical",
            "draft_question": (
                "The following sentence mixes mandatory (SHALL) and discretionary (MAY/SHOULD) language. "
                "Please clarify which clauses are mandatory versus optional, and confirm whether bidders "
                "must satisfy ALL clauses to be considered compliant."
            ),
            "evidence_snippet": s.strip()[:300],
            "source_section": None,
        })

# --- Pattern D: Mandatory items without measurable acceptance criteria ---
for item in compliance.get("mandatory_items", []) or []:
    text = item.get("text") or item.get("description") or ""
    if text and not re.search(r"\b(within|by|>=?|<=?|less than|more than|at least|no more than|"
                              r"\d+%|\d+ days?|\d+ hours?|\d+ years?)\b", text.lower()):
        candidates.append({
            "category": "Acceptance criterion",
            "severity": "Critical",
            "draft_question": (
                f"Mandatory item {item.get('id') or item.get('compliance_id', '')}: "
                f"Please provide the measurable acceptance criterion that the agency will use to "
                f"verify compliance with this requirement (e.g., test method, performance threshold, "
                f"audit evidence)."
            ),
            "evidence_snippet": text[:300],
            "source_section": item.get("source") or item.get("section"),
        })

# --- Pattern E: Evaluation criteria without weights ---
factors = evaluation.get("evaluation_factors") or evaluation.get("factors") or []
unweighted = [f for f in factors if not (f.get("weight") or f.get("points") or f.get("max_points"))]
if unweighted:
    names = ", ".join((f.get("factor_name") or f.get("name") or "(unnamed)")
                      for f in unweighted[:5])
    candidates.append({
        "category": "Evaluation",
        "severity": "Critical",
        "draft_question": (
            f"Evaluation factor(s) {names} appear without explicit weights or maximum point values. "
            f"Please publish the full scoring rubric, including weight or maximum points per factor "
            f"and any sub-factor breakdown, so bidders can size their response appropriately."
        ),
        "evidence_snippet": None,
        "source_section": "EVALUATION_CRITERIA.json",
    })
```

### Step 4: Deduplicate and Prioritize

```python
# Dedup on (category, normalized draft snippet)
seen = set()
deduped = []
for c in candidates:
    key = (c["category"], (c["evidence_snippet"] or "")[:120].lower())
    if key in seen:
        continue
    seen.add(key)
    deduped.append(c)

# Triage by mode
SEVERITY_ORDER = {"Critical": 0, "Important": 1, "Nice-to-have": 2}
if mode == "triage_critical":
    deduped = [c for c in deduped if c["severity"] in ("Critical", "Important")]

deduped.sort(key=lambda c: SEVERITY_ORDER.get(c["severity"], 99))
```

### Step 5: Generate the CLARIFYING_QUESTIONS.md

```python
lines = ["# Clarifying Questions\n"]
lines.append(f"**Generated:** {datetime.now().strftime('%Y-%m-%d %H:%M')}\n")

if mode == "open":
    lines.append(f"**Question deadline:** OPEN — submit by **{deadline_date or '(see RFP)'}**.\n")
elif mode == "triage_critical":
    lines.append(f"**Question deadline:** CLOSING SOON ({deadline_date}). Only Critical and Important questions listed.\n")
elif mode == "post_deadline":
    lines.append(f"**Question deadline:** CLOSED ({deadline_date}). These questions are recorded as INTERNAL risk inputs; their unresolved ambiguity will be absorbed in our bid price or risk register.\n")
elif mode == "internal_only":
    lines.append("**Question deadline:** NOT PERMITTED. The list below is an INTERNAL risk register only — no submission to the agency.\n")

lines.append(f"\n**Total questions:** {len(deduped)}\n")
by_sev = {sev: [c for c in deduped if c['severity'] == sev]
          for sev in ("Critical", "Important", "Nice-to-have")}
lines.append(f"- Critical: {len(by_sev['Critical'])}\n")
lines.append(f"- Important: {len(by_sev['Important'])}\n")
lines.append(f"- Nice-to-have: {len(by_sev['Nice-to-have'])}\n\n---\n")

for sev in ("Critical", "Important", "Nice-to-have"):
    bucket = by_sev[sev]
    if not bucket:
        continue
    lines.append(f"\n## {sev} Questions ({len(bucket)})\n")
    for i, c in enumerate(bucket, 1):
        lines.append(f"\n### Q-{sev[0]}{i:02d}: {c['category']}\n")
        lines.append(f"**Question:** {c['draft_question']}\n")
        if c.get("source_section"):
            lines.append(f"**RFP source:** {c['source_section']}\n")
        if c.get("evidence_snippet"):
            lines.append(f"**Evidence snippet:**\n\n```\n{c['evidence_snippet']}\n```\n")

lines.append("\n---\n## Submission Guidance\n\n")
if mode in ("open", "triage_critical"):
    lines.append("1. **User review (you):** edit / strike / consolidate as needed.\n"
                 "2. **Submission format:** match the RFP's prescribed format (typically email, vendor portal, or written letter).\n"
                 "3. **Internal review:** confirm no question telegraphs our competitive position.\n"
                 "4. **Submit by deadline.**\n")
else:
    lines.append("This list is an INTERNAL risk register. Each unresolved ambiguity should be considered "
                 "during Phase 1.9 Go/No-Go scoring (Risk Assessment area) and during Phase 8.5 pricing "
                 "(add risk buffer where appropriate).\n")

write_file(f"{folder}/outputs/CLARIFYING_QUESTIONS.md", "".join(lines))
log(f"CLARIFYING_QUESTIONS.md written: {len(deduped)} questions ({len(by_sev['Critical'])} critical)")
```

### Step 6: Surface Deadline Posture for Phase 1.9

```python
# Write a small artifact Phase 1.9 can read.
write_json(f"{folder}/shared/clarifying-questions-summary.json", {
    "generated_at": datetime.now().isoformat(),
    "mode": mode,
    "deadline_status": deadline_status,
    "deadline_date": deadline_date,
    "totals": {
        "critical": len(by_sev["Critical"]),
        "important": len(by_sev["Important"]),
        "nice_to_have": len(by_sev["Nice-to-have"]),
        "all": len(deduped),
    },
})
```

### Step 7: Report

```
❓ Clarifying Questions Authored
================================
Mode:               {mode}
Deadline:           {deadline_date}
Total questions:    {len(deduped)}
  - Critical:       {len(by_sev['Critical'])}
  - Important:      {len(by_sev['Important'])}
  - Nice-to-have:   {len(by_sev['Nice-to-have'])}

Output: outputs/CLARIFYING_QUESTIONS.md
```

## Quality Checklist (MANDATORY — report each by name with evidence)

The phase agent MUST verify each of the following BEFORE reporting completion. The agent's completion report MUST include a checklist-results block with:
- Item name (verbatim from below)
- PASS / FAIL / SKIPPED-WITH-REASON
- Evidence (file:line citation, grep result, file size, assertion that ran, etc.)

"All checks passed" without per-item evidence is NOT acceptable.

### Required output files
1. **CLARIFYING_QUESTIONS.md** exists at `{folder}/outputs/CLARIFYING_QUESTIONS.md` — evidence: `ls -la` showing size >= 2,048 bytes
2. **clarifying-questions-summary.json** exists at `{folder}/shared/clarifying-questions-summary.json` — evidence: `ls -la` size > 100 bytes and parses as valid JSON

### Schema fidelity
3. **clarifying-questions-summary.json** contains `generated_at`, `mode`, `deadline_status`, `totals` — evidence: list actual top-level keys found
4. **totals** contains `critical`, `important`, `nice_to_have`, `all` — evidence: print totals block
5. **Three severity tiers present** in CLARIFYING_QUESTIONS.md — evidence: grep for "## Critical Questions", "## Important Questions", "## Nice-to-have Questions" (or equivalent headers) each returns >= 1 hit
6. No `[:N]` slicing applied to deliverable content strings — evidence: grep for `\[:[0-9]+\]` in production code paths returned 0 hits

### Cross-stage consistency
7. **Deadline posture surfaced** — CLARIFYING_QUESTIONS.md opens with a deadline-status line (open / closing_soon / closed / not_permitted) — evidence: print first 5 lines of the file
8. **Every question includes evidence snippet OR source section** — evidence: count questions lacking both `evidence_snippet` and `source_section` (must be 0 for Critical tier)

### Anti-regression rules (universal)
9. **No competitive position leakage** — grep CLARIFYING_QUESTIONS.md for bidder-specific proper nouns or strategy language returned 0 hits outside the "Submission Guidance" section — evidence: grep result
10. **UTF-8 encoding** on every `open()` call — evidence: search this phase's emitted scripts/code for `encoding='utf-8'` in every file-open
11. **ensure_ascii=False** on every `json.dump` call — evidence: same grep
12. **No `_Showing N of M_` row-cap notices** in any deliverable markdown — evidence: grep returned 0 matches
13. **No empty `|  |` mitigation/cell patterns** in any deliverable table — evidence: grep returned 0 matches
14. **No mid-word table-cell truncations** — evidence: line-by-line cell-end check returned 0 hits

### Memory discipline
15. **Relevant SAFS memory entries reviewed and applied** — evidence: list which memory files were read and which rules were applicable
