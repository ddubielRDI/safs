---
name: process-rfp-screen
description: Lightweight RFP screener that runs a 10-phase pipeline (~18-35 min including a ~3-5 min Past_Projects.md refresh at Phase 1.5) producing a GO/NO-GO bid decision in BID_SCREEN.docx. Use BEFORE /process-rfp-win (3+ hrs) to triage opportunities.
when_to_use: 'Triggers on "screen rfp", "rfp screen", "bid screen", "go no-go", "go/no-go", "rfp triage", "should we bid", "quick rfp screen", "screen bid".'
argument-hint: "[path-to-rfp-folder] [--quick] [--skip-refresh]"
allowed-tools: [Bash, Read, Write, Glob, Grep, WebSearch, AskUserQuestion]
created: 2026-02-23
updated: 2026-05-17
disable-model-invocation: true
---

<!-- Change log:
  2026-05-17: Phase 3b migration (description→when_to_use split); added AskUserQuestion to allowed-tools (referenced by Anomaly Protocol); updated date normalized to plain ISO. Body unchanged.
  2026-05-14: Added top-of-file Execution Discipline block (Read-Before-Execute Gate, Schema Fidelity, Anomaly Protocol); strengthened phase-loop verification; recorded regression incident in memory/2026-05-14-regression-incident.md.
  2026-05-13: First skills-excellence-update audit — frontmatter relocation, hardcoded paths replaced, memory infrastructure created. -->


# /process-rfp-screen — Lightweight RFP Screening Pipeline

## ⛔ Execution Discipline (READ FIRST — BLOCKING)

This skill is a multi-phase pipeline. Each phase has a prescriptive file with its OWN required-output JSON schema, scoring framework, and quality criteria. **Never improvise a phase from first principles — always read the phase file in full, in this conversation, immediately before executing that phase.** The "speed > polish" tagline in the Skill Description refers to skipping Grok review and Task-agent dispatch, NOT skipping schema fidelity.

**Three non-negotiable rules:**

1. **Read-Before-Execute Gate.** Before executing any phase (0, 1, 1.5, 2, 3, 4, 4.5, 5, 5.5, 6), you MUST have called the Read tool on that phase's file in the current conversation turn. Reading the skill file (this file) does not satisfy this gate. If you have not Read the phase file, STOP and Read it first.

2. **Schema Fidelity.** Each phase file declares its required output keys / sub-objects. The phase JSON you write to `screen/` must contain ALL keys the phase file lists. Do not omit, rename, or restructure. If a key cannot be populated due to insufficient evidence, fill it with `null` or the documented Rule-5 conservative score (0 or 1) — do not drop the key.

3. **Anomaly Protocol.** If you detect an anomaly — conflicting evidence between phase outputs, ambiguous instruction, missing input, a phase-file schema element that contradicts a memory rule, or a baseline (e.g., prior `V1/screen/`) output that has materially more depth than your in-progress output — STOP and ASK the user before improvising. Never silently choose a shorter / thinner / faster path.

**Regression baseline:** A reference V1 BID_SCREEN.docx is ~65 KB / ~430 paragraphs / ~70 KB markdown / ~137 KB consolidated BID_SCREEN.json. If your in-progress output is materially smaller than these scales without a documented reason (e.g., `--quick` mode, abort condition), you are likely regressing — stop and audit before continuing. (See `memory/2026-05-14-regression-incident.md` for the incident that established this baseline.)

---

## Skill Description

Lightweight RFP screening pipeline that analyzes an RFP across 6 dimensions and produces a single `BID_SCREEN.docx` for human GO/NO-GO judgment. Designed to run in ~15-30 minutes — use BEFORE committing to the full `/process-rfp-win` pipeline (3+ hours).

**Three Intelligence Layers** (v2 -- 2026-03-25):
- **Client Tone Detection** — detects communication register (formal/outcomes/innovation/compliance/mission-driven) from RFP text and adapts all downstream outputs to mirror evaluator language.
- **Full Technology Intelligence** — maps tech stacks, versions, ecosystem relationships, maturity, and competitive positioning (differentiator/table-stakes/gap).
- **Evaluator-Lens Positioning** — maps themes to evaluation criteria point values, estimates scoring impact, aligns questions to high-point criteria.

**Key Design Decisions:**
- **Grok Review: SKIPPED** — this is a screening tool, not a production bid. Speed > polish.
- **Agent Dispatch: NONE** — all phases execute inline in main context (saves ~5 min overhead).
- **Output Isolation:** All outputs in `{folder}/screen/` — never touches `shared/` or `outputs/`.

---

## Required Outputs (Completion Checklist)

| Required Output | Phase | Min Size |
|-----------------|-------|----------|
| `screen/source-manifest.json` | 0 | 1KB |
| `screen/rfp-summary.json` | 1 | 1KB |
| `screen/go-nogo-score.json` | 2 | 1KB |
| `screen/client-intel-snapshot.json` | 3 | 1KB (skipped with --quick) |
| `screen/compliance-check.json` | 4 | 1KB |
| `screen/past-projects-match.json` | 4 | 1KB |
| `screen/preliminary-themes.json` | 4.5 | 1KB |
| `screen/risk-assessment.json` | 5 | 1KB |
| `screen/BID_SCREEN.json` | 5 | 3KB |
| `screen/clarifying-questions.json` | 5.5 | 1KB |
| `screen/BID_SCREEN.md` | 6 | 5KB |
| `screen/BID_SCREEN.docx` | 6 | 30KB |

---

## User Invocation

```
/process-rfp-screen <path-to-rfp-folder> [--quick] [--skip-refresh]
```

**Arguments:**
- `path-to-rfp-folder` (required): Folder containing RFP source documents
- `--quick` (optional): Skip Phase 3 (client intelligence), saves 4-6 min
- `--skip-refresh` (optional): Skip Phase 1.5 (Past_Projects.md refresh), saves 3-5 min. Use when Past_Projects.md was recently curated by hand and a fresh refresh would burn web searches unnecessarily. The refresh skill also has its own 7-day stale check, so back-to-back screen runs auto-skip the refresh even without this flag.

---

## Pre-Approved Permissions

**All file operations within `{folder}` and its subdirectories are PRE-APPROVED:**

| Operation | Scope | Notes |
|-----------|-------|-------|
| Read | `{folder}/**/*` | All files in input folder |
| Write | `{folder}/screen/**/*` | Create/overwrite screen outputs |
| Create | `{folder}/screen/**/*` | New files and directories |
| Bash | `markitdown`, `python3`, `pip` | Document conversion, DOCX generation |
| WebSearch | Max 8 (Phase 3) + Max 20 (Phase 1.5 refresh) | Client intel + Past_Projects.md refresh |
| Read/Write | `Past_Projects.md`, `Past_Projects.backup-*.md`, `Past_Projects.refresh-log.md` | Phase 1.5 refresh outputs (in repo root, not `{folder}/screen/`) |

---

## Configuration

```python
import os

# Paths — resolved at runtime from skill location.
# Claude Code sets CLAUDE_SKILL_DIR to this skill's directory at load time.
# Fall back to __file__-relative resolution if env var is absent; abort if neither works.
SKILL_DIR = os.environ.get("CLAUDE_SKILL_DIR") or os.path.dirname(os.path.abspath(__file__))
if not SKILL_DIR or not os.path.isdir(SKILL_DIR):
    error("ABORT: Cannot resolve SKILL_DIR — set CLAUDE_SKILL_DIR or invoke from the skill directory")
    halt()
PHASES_DIR = f"{SKILL_DIR}/phases-screen"
CONFIG_DIR = f"{SKILL_DIR}/../process-rfp-win/config-win"

# Company profile — shared with full pipeline
COMPANY_PROFILE = f"{CONFIG_DIR}/company-profile.json"

# Past projects — shared with full pipeline.
# Search upward from SKILL_DIR for Past_Projects.md (more robust than fixed-depth
# math, which would break if the skill is moved or symlinked). Cap the upward walk
# at 6 levels so we never escape a repo unexpectedly.
def _find_past_projects(start_dir, max_up=6):
    cur = os.path.abspath(start_dir)
    for _ in range(max_up):
        candidate = os.path.join(cur, "Past_Projects.md")
        if os.path.isfile(candidate):
            return candidate
        parent = os.path.dirname(cur)
        if parent == cur:  # filesystem root reached
            break
        cur = parent
    # Fallback: the historically-correct location three levels up. If this also
    # doesn't exist, downstream phases will warn and continue with empty matches.
    return os.path.abspath(os.path.join(start_dir, "..", "..", "..", "Past_Projects.md"))

PAST_PROJECTS = _find_past_projects(SKILL_DIR)

# Scan limits
SCAN_LIMIT = 80000  # Max chars of RFP text to analyze
MAX_WEB_SEARCHES = 8  # Client intel search budget

# Domain skills — sibling directories of this skill
DOMAIN_SKILLS_DIR = f"{SKILL_DIR}/.."

# Scoring thresholds (identical to Phase 1.9)
THRESHOLD_GO = 50
THRESHOLD_CONDITIONAL = 40
# Below THRESHOLD_CONDITIONAL = NO-GO
```

---

## ⛔ Required Helper Functions (BLOCKING — define BEFORE any phase runs)

**MANDATORY:** The phase files reference `read_json` / `write_json` / `read_file` / `write_file` and similar helpers. These are NOT auto-defined — Claude must define them in the runtime Python before phase execution. **They MUST be defined exactly as shown below.** Every recurrence of em-dash mojibake (`â€"` in place of `—`) traces to one of these helpers being defined without `encoding='utf-8'`, because Windows defaults to cp1252 and silently mangles UTF-8 bytes on read-back.

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
    """Same as read_json but returns None if file missing/invalid (Phase 3 client_intel may be optional)."""
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

**Why `ensure_ascii=False`:** the older default (`True`) escapes em dashes as `—` but does the same to mojibake `â€”`, hiding the corruption. With `ensure_ascii=False` and `encoding='utf-8'`, mojibake becomes immediately visible as `â€"` on inspection, catching bugs at the file boundary.

**This applies to inline Python invoked via Bash, generated `.py` scripts dropped into `screen/`, AND the Phase 6 DOCX renderer.** Do not write a `with open(...)` line in this pipeline without `encoding='utf-8'`.

---

## Phase Execution Order

```
Phase 0:   Folder Setup & Document Intake     → phases-screen/phase0-intake.md     [BLOCKING]
Phase 1:   RFP Summary Extraction             → phases-screen/phase1-summary.md
Phase 1.5: Past_Projects.md Refresh           → ../update-past-projects/skill-update.md  [SKIP if --skip-refresh or stale-check passes]
Phase 2:   Go/No-Go Scoring                   → phases-screen/phase2-gonogo.md
Phase 3:   Client Intelligence Snapshot        → phases-screen/phase3-intel.md       [SKIP if --quick]
Phase 4:   Compliance Check & Project Match    → phases-screen/phase4-compliance.md
Phase 4.5: Preliminary Win Themes              → phases-screen/phase4.5-themes.md
Phase 5:   Risk Assessment & Recommendation    → phases-screen/phase5-recommendation.md
Phase 5.5: Clarifying Questions Generation     → phases-screen/phase5.5-questions.md
Phase 6:   Report Generation (DOCX)             → phases-screen/phase6-pdf.md         [MANDATORY]
```

**Why Phase 1.5 sits where it does:** the refresh skill accepts an optional `--context` JSON to scope new-project discovery toward RFP-relevant work. That context comes from Phase 1's `rfp-summary.json` (industry, agency, state, scope_keywords). Running the refresh *after* Phase 1 gives it context; running it *before* Phase 2 (competitive position uses Company Intelligence) and Phase 4 (project matching reads Past_Projects.md) ensures downstream phases consume the refreshed data.

---

## Execution Protocol

### Step 1: Parse Arguments and Initialize

```python
import os, json, sys
from datetime import datetime

# Parse arguments
args = user_input.strip().split()
folder = args[0]
quick_mode = "--quick" in args
skip_refresh = "--skip-refresh" in args

# Validate folder exists
if not os.path.exists(folder):
    error(f"Folder does not exist: {folder}")
    halt()

# Resolve absolute path
folder = os.path.abspath(folder)

# Create screen output directory
screen_dir = f"{folder}/screen"
os.makedirs(screen_dir, exist_ok=True)

log("=" * 60)
log("RFP SCREENING PIPELINE")
log(f"Folder: {folder}")
log(f"Mode: {'QUICK (skip intel)' if quick_mode else 'FULL'}")
log(f"Started: {datetime.now().strftime('%Y-%m-%d %H:%M')}")
log("=" * 60)
```

### Step 2: Validate Prerequisites

```python
# company-profile.json MUST exist (scoring is meaningless without it)
if not os.path.exists(COMPANY_PROFILE):
    error(f"ABORT: company-profile.json not found at {COMPANY_PROFILE}")
    error("Scoring requires company data. Cannot proceed.")
    halt()

# RFP documents must exist somewhere
source_dirs = [folder, f"{folder}/original"]
found_docs = []
for src_dir in source_dirs:
    if os.path.exists(src_dir):
        for f in os.listdir(src_dir):
            ext = os.path.splitext(f)[1].lower()
            if ext in ['.pdf', '.docx', '.xlsx', '.doc', '.xls']:
                found_docs.append(os.path.join(src_dir, f))

if not found_docs:
    error("ABORT: No RFP documents found (PDF, DOCX, XLSX)")
    error(f"Searched: {', '.join(source_dirs)}")
    halt()

log(f"Found {len(found_docs)} source document(s)")
```

### Step 3: Execute Phases Sequentially

**CRITICAL: Execute each phase by reading its phase file and following its instructions inline. Do NOT use Task agents — all phases run in main context for speed.**

```python
phases = [
    {
        "id": 0,
        "name": "Folder Setup & Document Intake",
        "file": "phase0-intake.md",
        "blocking": True,
        "skip_condition": None,
        "skill": "document-processor"
    },
    {
        "id": 1,
        "name": "RFP Summary Extraction",
        "file": "phase1-summary.md",
        "blocking": False,
        "skip_condition": None,
        "skill": "procurement-analyst",
        "additional_skills": [
            "capture-strategist/tone-calibration",
            "competitive-intel/tech-intelligence"
        ]  # v2: Phase 1 also loads tone + tech intelligence sub-skills
    },
    {
        "id": 1.5,
        "name": "Past_Projects.md Refresh",
        "file": "../../update-past-projects/skill-update.md",  # resolved relative to PHASES_DIR
        "blocking": False,
        "skip_condition": "skip_refresh",  # also self-skips on 7-day stale check
        "skill": None,                     # this entry is a sibling skill, not a phase-script
        "is_external_skill": True,         # signals special invocation handling below
        "external_args": [
            "--context=" + f"{folder}/screen/rfp-summary.json",
        ],
    },
    {
        "id": 2,
        "name": "Go/No-Go Scoring",
        "file": "phase2-gonogo.md",
        "blocking": False,
        "skip_condition": None,
        "skill": "capture-strategist",
        "sub_skill": "bid-decision"
    },
    {
        "id": 3,
        "name": "Client Intelligence Snapshot",
        "file": "phase3-intel.md",
        "blocking": False,
        "skip_condition": "quick_mode",  # Skip if --quick
        "skill": "competitive-intel"
    },
    {
        "id": 4,
        "name": "Compliance Quick-Check & Past Project Match",
        "file": "phase4-compliance.md",
        "blocking": False,
        "skip_condition": None,
        "skill": "procurement-analyst",
        "sub_skill": "compliance-audit"
    },
    {
        "id": 4.5,
        "name": "Preliminary Win Theme Derivation",
        "file": "phase4.5-themes.md",
        "blocking": False,
        "skip_condition": None,
        "skill": "capture-strategist",
        "sub_skill": "competitive-positioning"
    },
    {
        "id": 5,
        "name": "Risk Assessment & Recommendation",
        "file": "phase5-recommendation.md",
        "blocking": False,
        "skip_condition": None,
        "skill": "risk-analyst"
    },
    {
        "id": 5.5,
        "name": "Clarifying Questions Generation",
        "file": "phase5.5-questions.md",
        "blocking": False,
        "skip_condition": None,
        "skill": "capture-strategist",
        "sub_skill": "competitive-positioning"
    },
    {
        "id": 6,
        "name": "Report Generation (DOCX)",
        "file": "phase6-pdf.md",
        "blocking": True,  # Pipeline fails without DOCX
        "skip_condition": None,
        "skill": "publication-specialist"
    }
]

results = {}
for phase in phases:
    phase_id = phase["id"]
    phase_name = phase["name"]

    # Check skip condition
    if phase["skip_condition"] == "quick_mode" and quick_mode:
        log(f"\nPhase {phase_id}: {phase_name} -- SKIPPED (--quick mode)")
        results[phase_id] = {"status": "skipped", "reason": "--quick mode"}
        continue
    if phase["skip_condition"] == "skip_refresh" and skip_refresh:
        log(f"\nPhase {phase_id}: {phase_name} -- SKIPPED (--skip-refresh)")
        results[phase_id] = {"status": "skipped", "reason": "--skip-refresh"}
        continue

    # External-skill invocation (Phase 1.5 only): read the sibling skill file and
    # follow its instructions inline with the supplied context arguments. The skill
    # has its own stale check (7-day window) which will further self-skip if
    # Past_Projects.md was modified recently. No phase Read/Write happens here —
    # the refresh skill writes directly to repo root, not {folder}/screen/.
    if phase.get("is_external_skill"):
        external_path = os.path.normpath(f"{PHASES_DIR}/{phase['file']}")
        log(f"\nPhase {phase_id}: {phase_name}")
        log(f"  Loading external skill: {external_path}")
        if not os.path.exists(external_path):
            log(f"  WARNING: external skill not found at {external_path} -- skipping")
            results[phase_id] = {"status": "skipped", "reason": "skill not found"}
            continue
        # >>> Use Read tool on external_path; then execute its Step 0–8 with these args: phase["external_args"] <<<
        # The refresh skill is self-contained — it parses its own args, runs its own
        # stale check, writes its own log. This pipeline only needs to invoke it and
        # log completion. No JSON output flows back into screen/ (the side effect IS
        # the refreshed Past_Projects.md that downstream phases will read).
        results[phase_id] = {"status": "complete (external skill)"}
        continue

    log(f"\n{'='*50}")
    log(f"Phase {phase_id}: {phase_name}")
    log(f"{'='*50}")

    # ====================================================================
    # ⛔ READ-BEFORE-EXECUTE GATE (BLOCKING — see top-of-file discipline)
    # ====================================================================
    # Before doing ANYTHING for this phase, you MUST:
    #   1. Call the Read tool on {DOMAIN_SKILLS_DIR}/{skill}.md (and sub_skill if present)
    #   2. Call the Read tool on {PHASES_DIR}/{phase["file"]} — read it IN FULL
    #   3. Note the phase's "Required Output" JSON schema verbatim
    #   4. Note any "Quality Checklist" or "Schema Fidelity" sections
    #
    # If you have not Read these files in the current conversation turn, STOP HERE.
    # Do not improvise. Do not infer the schema from context. Read first.
    #
    # SCHEMA FIDELITY: The output JSON you write must contain every key listed in the
    # phase file's required-output schema. Missing keys = incomplete phase = abort gate.
    # If a key cannot be populated with evidence, set it to null or the documented
    # conservative score (Rule 5: 0 or 1), but DO NOT DROP THE KEY.
    #
    # ANOMALY PROTOCOL: If the phase file's schema conflicts with another file you've
    # read (memory rule, V1 baseline, prior phase output), ASK the user. Do not pick
    # one silently. Do not skip a section because it "seems redundant."
    # ====================================================================

    # --- SKILL LOADING (MANDATORY) ---
    # Before executing the phase, load domain expertise into context.
    # The skill file contains standards, frameworks, and quality criteria
    # that improve output quality for this phase.
    #
    # HOW THIS WORKS (inline execution model):
    # Since screen phases run in main context (no Task agents), skill loading
    # means using the Read tool to read the skill file(s) BEFORE reading the
    # phase file. The skill content then exists in context and informs execution.
    #
    # STEPS:
    # 1. Check phase["skill"] for a skill name
    # 2. READ {DOMAIN_SKILLS_DIR}/{skill_name}.md using the Read tool
    # 3. If phase["sub_skill"] exists, ALSO READ {DOMAIN_SKILLS_DIR}/{skill_name}/{sub_skill_name}.md
    # 4. Log what was loaded
    # 5. THEN read and execute the phase file
    #
    # Skill loading is defensive: if a file is missing, warn and proceed without it.

    skill_name = phase.get("skill")
    sub_skill_name = phase.get("sub_skill")

    if skill_name:
        # READ the core skill file: {DOMAIN_SKILLS_DIR}/{skill_name}.md
        skill_path = f"{DOMAIN_SKILLS_DIR}/{skill_name}.md"
        log(f"  Loading skill: {skill_name}")
        # >>> Use Read tool on skill_path <<<

        # v2: Load additional cross-domain sub-skills if specified
        for extra in phase.get("additional_skills", []):
            extra_path = f"{DOMAIN_SKILLS_DIR}/{extra}.md"
            log(f"  Loading additional skill: {extra}")
            # >>> Use Read tool on extra_path <<<
        # After reading the skill, note the key frameworks, quality criteria, and
        # anti-patterns it defines. When executing the phase, actively apply these
        # frameworks to structure outputs and verify quality. The skill content is
        # not background reading -- it defines the expert standard the phase must meet.
        #
        # Each phase file now contains a "Skill Integration" section that explicitly
        # bridges loaded skill frameworks into execution. Look for:
        # - Framework application directives (specific frameworks to apply)
        # - Quality criteria checkpoints (checks before writing output)
        # - Anti-pattern guards (output patterns to avoid)

        if sub_skill_name:
            # ALSO READ the sub-skill file: {DOMAIN_SKILLS_DIR}/{skill_name}/{sub_skill_name}.md
            sub_skill_path = f"{DOMAIN_SKILLS_DIR}/{skill_name}/{sub_skill_name}.md"
            log(f"  Loading sub-skill: {sub_skill_name}")
            # >>> Use Read tool on sub_skill_path <<<

        log(f"  Skill: {skill_name}{' + ' + sub_skill_name if sub_skill_name else ''} (loaded)")
    else:
        log(f"  Skill: none (procedural phase)")

    # NOW read the phase file and execute its instructions.
    # The skill content already in context will inform the phase execution.
    phase_file = f"{PHASES_DIR}/{phase['file']}"
    # ⛔ MANDATORY: Use Read tool on phase_file BEFORE executing this phase.
    # Read the file IN FULL (no offset/limit unless file > 2000 lines).
    # Implement every step the phase file declares. Produce every key the
    # phase file's "Required Output" schema declares. Match field names verbatim.
    # If a phase-file step seems redundant with another phase, DO NOT skip it —
    # downstream renderers (Phase 6) depend on the full schema.

    # After execution, verify outputs
    # (Phase-specific verification defined in each phase file)
    # SCHEMA CHECK: read the file you just wrote and verify it has every key the
    # phase file's required-output schema lists. If any are missing, the phase is
    # NOT complete — go back and produce them. Do not advance to the next phase
    # with a partial output.

    results[phase_id] = {"status": "complete"}
```

### Step 3.5: Pre-Phase-6 Traceability Audit Gate (MANDATORY)

After all data-producing phases (0–5.5) complete and BEFORE Phase 6 renders the DOCX, perform the six-rule traceability audit on the assembled JSON outputs. This is the gate that prevents shipping honest-but-imprecise claims.

**Execution model:** This gate runs as a fresh, adversarially-framed audit prompt — the same Claude that produced the outputs cannot self-audit without bias. The prompt below MUST be invoked as a discrete reasoning step with no carry-over from the data-production phases. Treat the six rules as the sole evaluation criteria; do not relax them for any output that "looks fine."

**Placeholder convention (read first):** Two calls in the prescribed Python below — `invoke_llm(audit_prompt)` and `apply_finding_correction(audit_inputs[f["file"]], f["json_path"], f["fix"])` — are **agent-reasoning placeholders**, not Python helpers like `read_json`. They cannot be defined as functions in the conventional sense because the LLM (Claude, executing this skill) IS the reasoning engine. Treat them as:
- **`invoke_llm(prompt)`** — "I, Claude, will perform this LLM reasoning step inline in a fresh sub-context, returning the JSON I would have produced." Equivalent to the `llm(prompt, json_mode=True)` convention used in phase1-summary.md, except scoped to a discrete adversarial reasoning pass with no carry-over from prior phase data.
- **`apply_finding_correction(json_obj, json_path, fix)`** — "I, Claude, will apply this finding's `fix` value at the given `json_path` inside `json_obj`, mutating in place." For a `json_path` like `"matched_projects[2].score_breakdown.contract_type"`, walk the path and assign the fix value.

Do NOT attempt to import or define these as Python functions; instead, perform the reasoning/mutation inline when the prescribed code reaches each call. If either step cannot be performed (e.g., the audit prompt produces malformed JSON), abort the gate per the existing 3-iteration cap rather than falling back to a no-op.

```python
# Load every JSON the audit needs to inspect.
audit_inputs = {
    "rfp_summary":  read_json(f"{folder}/screen/rfp-summary.json"),
    "go_nogo":      read_json(f"{folder}/screen/go-nogo-score.json"),
    "client_intel": read_json_safe(f"{folder}/screen/client-intel-snapshot.json") or {},
    "compliance":   read_json(f"{folder}/screen/compliance-check.json"),
    "past_proj":    read_json(f"{folder}/screen/past-projects-match.json"),
    "themes":       read_json(f"{folder}/screen/preliminary-themes.json"),
    "risk":         read_json(f"{folder}/screen/risk-assessment.json"),
    "questions":    read_json(f"{folder}/screen/clarifying-questions.json"),
}

# Read the source-of-truth rule definitions so the audit can quote them.
gotchas_path = f"{SKILL_DIR}/memory/gotchas.md"
gotchas_text = read_file(gotchas_path) if os.path.exists(gotchas_path) else ""

# Adversarial audit prompt — the LLM step that actually scans the data.
audit_prompt = f"""You are an ADVERSARIAL traceability auditor with no prior context on
this RFP. Your job is to find every place where the six traceability rules below have
been violated. You are rewarded for finding violations; do not soften, defer, or
"trust the writer." If a phrasing is ambiguous, flag it.

THE SIX RULES (from memory/gotchas.md "Traceability discipline"):
{gotchas_text}

THE OUTPUTS UNDER AUDIT (JSON files produced by Phases 0–5.5):
{json.dumps(audit_inputs, indent=2)[:60000]}

For EACH violation you find, return one finding with this exact schema:
{{
  "rule":       <int 1-6>,
  "file":       <"rfp_summary" | "go_nogo" | "client_intel" | "compliance"
                 | "past_proj" | "themes" | "risk" | "questions">,
  "json_path":  <dotted path into that JSON, e.g. "matched_projects[2].score_breakdown.contract_type">,
  "quote":      <verbatim text from the JSON that violates the rule>,
  "why":        <one-sentence explanation tied to the rule>,
  "fix":        <concrete replacement text or the score adjustment required>
}}

Return JSON only: {{"findings": [...]}}. An empty findings array means CLEAN. Be
exhaustive — re-reading a passage twice is acceptable; missing a violation is not."""

audit_response = invoke_llm(audit_prompt)  # subagent/fresh-context call
audit_findings = audit_response.get("findings", [])
iterations_run = 0  # initialized so the post-loop metadata stamp never NameErrors

def _persist_corrected(file_key, payload, screen_dir):
    """Write a corrected JSON back to disk and verify it round-trips through json.loads.
    A malformed correction would otherwise surface only in Phase 6, far from its cause."""
    out_path = f"{screen_dir}/{file_key.replace('_','-')}.json"
    write_json(out_path, payload)
    try:
        _ = read_json(out_path)  # parse-back validation
    except Exception as e:
        error(f"ABORT: traceability correction produced unparseable JSON at {out_path}: {e}")
        halt()

if audit_findings:
    log(f"  TRACEABILITY AUDIT: {len(audit_findings)} finding(s) -- correcting JSON before Phase 6")
    for f in audit_findings:
        log(f"    - [Rule {f['rule']}] {f['file']}:{f['json_path']} -- {f['why']}")
        # Apply the fix into the source JSON (in memory), then write it back to disk
        # with parse-back validation so malformed corrections fail loudly here, not in Phase 6.
        apply_finding_correction(audit_inputs[f["file"]], f["json_path"], f["fix"])
        _persist_corrected(f["file"], audit_inputs[f["file"]], f"{folder}/screen")

    # Re-run the audit against the corrected JSONs. Loop until clean (max 3 iterations
    # to bound runaway). If the audit is non-empty after 3 passes, abort Phase 6 and
    # surface the residual findings to the user.
    iterations_run = 1  # the initial pass that produced the corrections counts as iteration 1
    for iteration in range(3):
        audit_response = invoke_llm(audit_prompt)
        audit_findings = audit_response.get("findings", [])
        iterations_run += 1
        if not audit_findings:
            break
        log(f"  TRACEABILITY AUDIT (re-run {iteration+1}): {len(audit_findings)} residual finding(s)")
        for f in audit_findings:
            apply_finding_correction(audit_inputs[f["file"]], f["json_path"], f["fix"])
            _persist_corrected(f["file"], audit_inputs[f["file"]], f"{folder}/screen")

    if audit_findings:
        error(f"ABORT Phase 6: traceability audit still has {len(audit_findings)} findings after 3 correction passes")
        for f in audit_findings:
            error(f"    - [Rule {f['rule']}] {f['file']}:{f['json_path']}")
        halt()

# Stamp the consolidated JSON so the audit ordering is provable from the output.
# This must happen BEFORE Phase 6 reads BID_SCREEN.json — otherwise the DOCX
# is built against unstamped data and the ordering proof is lost.
log("  TRACEABILITY AUDIT: clean -- proceeding to Phase 6")
bid_screen_path = f"{folder}/screen/BID_SCREEN.json"
bid_screen = read_json(bid_screen_path)
bid_screen["pipeline_metadata"] = {
    "pre_phase6_audit":         "clean",
    "audit_timestamp":          datetime.now().isoformat(),
    "audit_iterations":         iterations_run,    # 0 = clean on first pass; 1+ = corrections applied
    "gotchas_loaded":           bool(gotchas_text),
    "gotchas_path":             gotchas_path,
    "quick_mode":               quick_mode,
}
write_json(bid_screen_path, bid_screen)
```

Full rule definitions and worked examples are in `memory/gotchas.md` under "Traceability discipline — six anti-patterns caught during 2026-05-13 audit". When in doubt, re-read that section before judging an edge case.

**Why an adversarial fresh-context call:** The same Claude that wrote the JSONs has a confirmation-bias incentive to declare its own work clean. A discrete audit prompt with no carry-over forces the model to treat the JSONs as untrusted input and the six rules as the binding standard. Self-audit from prior context is not acceptable for this gate.

### Step 4: Final Verification

```python
log("\n" + "=" * 60)
log("FINAL VERIFICATION")
log("=" * 60)

required_outputs = [
    ("screen/source-manifest.json", "Source Manifest", 1),
    ("screen/rfp-summary.json", "RFP Summary", 1),
    ("screen/go-nogo-score.json", "Go/No-Go Score", 1),
    ("screen/compliance-check.json", "Compliance Check", 1),
    ("screen/past-projects-match.json", "Past Project Matches", 1),
    ("screen/preliminary-themes.json", "Preliminary Themes", 1),
    ("screen/risk-assessment.json", "Risk Assessment", 1),
    ("screen/clarifying-questions.json", "Clarifying Questions", 1),
    ("screen/BID_SCREEN.json", "Consolidated Data", 3),
    ("screen/BID_SCREEN.md", "Markdown Report", 5),
    ("screen/BID_SCREEN.docx", "DOCX Report", 30),
]

if not quick_mode:
    required_outputs.insert(2, ("screen/client-intel-snapshot.json", "Client Intel", 1))

all_present = True
for rel_path, name, min_kb in required_outputs:
    full_path = f"{folder}/{rel_path}"
    if os.path.exists(full_path):
        size_kb = os.path.getsize(full_path) / 1024
        status = "OK" if size_kb >= min_kb else f"SMALL ({size_kb:.1f}KB < {min_kb}KB)"
        log(f"  {name}: {size_kb:.1f}KB — {status}")
        if size_kb < min_kb:
            all_present = False
    else:
        log(f"  {name}: MISSING")
        all_present = False
```

### Step 5: Report Final Results

```python
# Load the scoring result for the summary
score_path = f"{folder}/screen/go-nogo-score.json"
if os.path.exists(score_path):
    score_data = read_json(score_path)
    total_score = score_data.get("total_score", 0)
    recommendation = score_data.get("recommendation", "UNKNOWN")
else:
    total_score = 0
    recommendation = "ERROR"

log("\n" + "=" * 60)
if all_present:
    log("RFP SCREENING COMPLETE")
else:
    log("RFP SCREENING INCOMPLETE — see missing outputs above")
log("=" * 60)

log(f"""
Recommendation: {recommendation}
Total Score: {total_score}/100
Mode: {'QUICK' if quick_mode else 'FULL'}{' + refresh skipped' if skip_refresh else ''}
Past_Projects refresh: {results.get(1.5, {}).get('status', 'unknown')}

Outputs: {folder}/screen/
Primary: BID_SCREEN.docx

{"Next step: /process-rfp-win " + folder if recommendation == "GO" else ""}
{"Review risks in BID_SCREEN.docx before deciding." if recommendation == "CONDITIONAL" else ""}
{"Recommendation is NO-GO. Override available if strategic reasons exist." if recommendation == "NO_GO" else ""}
""")

log("=" * 60)
```

---

## Memory Integration

**On skill start (MANDATORY — observable):** Read `${CLAUDE_SKILL_DIR}/memory/`:
- `gotchas.md` — known pitfalls and resolutions from prior runs (e.g., markitdown failures, `services` dict-vs-list trap in `company-profile.json`, malformed Past_Projects entries)
- Recent dated entries (`YYYY-MM-DD-*.md`) for situational context

Apply discovered corrections during execution — do not repeat past mistakes.

**Observability:** Step 3.5 (Traceability Audit Gate) stamps `pipeline_metadata.gotchas_loaded` and `pipeline_metadata.gotchas_path` into `BID_SCREEN.json` so downstream audits can verify the memory protocol ran. If `gotchas.md` is missing at skill start, log a WARNING but do not abort — the gotchas file is optional context, not a hard prerequisite.

**On skill end — write checklist:**
- [ ] Did a phase fail unexpectedly (markitdown crash, web search exhausted, DOCX generation failure)? → **MUST** record in `gotchas.md`
- [ ] Did an RFP structure surprise the pipeline (unusual document layout, missing client name, non-English text)? → **MUST** record
- [ ] Did Phase 2 score diverge wildly from human judgment? → **MUST** record the divergence pattern
- [ ] Did `company-profile.json` need to be re-flattened or reshaped to work? → **MUST** record
- [ ] Did a phase file's procedural logic break (Python `KeyError`, JSON schema drift, missing input)? → **MUST** record
- [ ] Routine screening with no surprises? → Skip — do NOT write "ran successfully"

---

## Error Handling

| Scenario | Action |
|----------|--------|
| All document conversions fail | ABORT with clear message |
| Some conversions fail | WARN, continue with available text |
| Client name not extracted | Skip Phase 3, flag as RISK, score conservatively |
| Combined text < 500 chars | ABORT — insufficient text for analysis |
| company-profile.json missing | ABORT — scoring meaningless without company data |
| Past_Projects.md missing | Continue, empty project matches, flag as RISK |
| WebSearch failures | Continue with remaining searches; min 1 for intel |
| DOCX generation fails | Retry once; if still fails, output BID_SCREEN.json only |

---

## Output Isolation

**Screen outputs MUST go to `{folder}/screen/` — never to `shared/` or `outputs/`.**

This ensures:
- Full pipeline can run after screening without conflicts
- Screening data is preserved alongside bid outputs
- Multiple screenings can coexist (rename `screen/` to `screen-v1/` etc.)

---

## Quality Checklist

### Execution Discipline (from top-of-file BLOCKING rules)
- [ ] **Every phase file Read in full** before that phase began execution (Read-Before-Execute Gate)
- [ ] **Output JSONs contain every key** the corresponding phase file's required-output schema declares (Schema Fidelity)
- [ ] **No anomaly was silently resolved** — every conflicting evidence / ambiguous schema / missing input was either resolved with the user or documented as a known limitation
- [ ] **Final BID_SCREEN.json size sanity check** — if < 50 KB without a documented reason (--quick mode, abort), re-audit phase completeness
- [ ] **Final BID_SCREEN.docx size sanity check** — if < 50 KB or < 200 paragraphs without a documented reason, re-audit phase completeness against `phase6-pdf.md` section list

### Standard Pipeline
- [ ] All phase files read from `phases-screen/` directory
- [ ] Phases executed sequentially in main context (no Task agents)
- [ ] Phase 3 skipped when `--quick` flag present
- [ ] Phase 1.5 honored (refresh or skip per stale check)
- [ ] All JSON outputs written to `{folder}/screen/`
- [ ] `BID_SCREEN.docx` generated and > 30KB
- [ ] `BID_SCREEN.json` is a CONSOLIDATION of all phase outputs (V1 baseline: ~137 KB), not a summary index
- [ ] Recommendation follows threshold rules (GO >= 50, CONDITIONAL 40-49, NO-GO < 40)
- [ ] DOCX uses python-docx with Calibri font, navy headings, Light Grid Accent 1 tables
- [ ] `screen/` directory does not interfere with `shared/` or `outputs/`

### Traceability Audit Quality Checks (added 2026-05-13)
- [ ] Six-rule pre-Phase-6 audit performed against all assembled JSON outputs
- [ ] Rule 1: contracting client vs end-user agency distinguished for every past-performance claim
- [ ] Rule 2: regional/partial award scope qualifiers preserved verbatim from `Past_Projects.md`
- [ ] Rule 3: internal monetary estimates labeled `[estimate — methodology: ...]`; upper bound matches actual calculation
- [ ] Rule 4: single-source facts (e.g., news-article-only figures) carry source attribution in every appearance, not just the first
- [ ] Rule 5: every `score_breakdown` value >= 2 traces to documented evidence in `Past_Projects.md`
- [ ] Rule 6: assertions about buyer intent/motivation are labeled `[inference]` unless stated in the RFP
- [ ] If any of these six fail, halt Phase 6, fix the JSON, re-verify, then proceed
