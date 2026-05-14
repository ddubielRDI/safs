---
name: process-rfp-screen
description: Lightweight RFP screener that runs a 9-phase pipeline (~15-30 min) producing a GO/NO-GO bid decision in BID_SCREEN.docx. Use BEFORE /process-rfp-win (3+ hrs) to triage opportunities. Triggers on "screen rfp", "rfp screen", "bid screen", "go no-go", "go/no-go", "rfp triage", "should we bid", "quick rfp screen", "screen bid".
argument-hint: "[path-to-rfp-folder] [--quick]"
allowed-tools: [Bash, Read, Write, Glob, Grep, WebSearch]
created: 2026-02-23
updated: 2026-05-13 (added six-rule traceability audit gate after post-run audit caught honest-but-imprecise claims; see memory/gotchas.md)
disable-model-invocation: true
---

# /process-rfp-screen — Lightweight RFP Screening Pipeline

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
/process-rfp-screen <path-to-rfp-folder> [--quick]
```

**Arguments:**
- `path-to-rfp-folder` (required): Folder containing RFP source documents
- `--quick` (optional): Skip Phase 3 (client intelligence), saves 4-6 min

---

## Pre-Approved Permissions

**All file operations within `{folder}` and its subdirectories are PRE-APPROVED:**

| Operation | Scope | Notes |
|-----------|-------|-------|
| Read | `{folder}/**/*` | All files in input folder |
| Write | `{folder}/screen/**/*` | Create/overwrite screen outputs |
| Create | `{folder}/screen/**/*` | New files and directories |
| Bash | `markitdown`, `python3`, `pip` | Document conversion, DOCX generation |
| WebSearch | Max 8 queries | Client intelligence (Phase 3 only) |

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

## Phase Execution Order

```
Phase 0: Folder Setup & Document Intake     → phases-screen/phase0-intake.md     [BLOCKING]
Phase 1: RFP Summary Extraction             → phases-screen/phase1-summary.md
Phase 2: Go/No-Go Scoring                   → phases-screen/phase2-gonogo.md
Phase 3: Client Intelligence Snapshot        → phases-screen/phase3-intel.md       [SKIP if --quick]
Phase 4: Compliance Check & Project Match    → phases-screen/phase4-compliance.md
Phase 4.5: Preliminary Win Themes            → phases-screen/phase4.5-themes.md
Phase 5: Risk Assessment & Recommendation    → phases-screen/phase5-recommendation.md
Phase 5.5: Clarifying Questions Generation   → phases-screen/phase5.5-questions.md
Phase 6: Report Generation (DOCX)             → phases-screen/phase6-pdf.md         [MANDATORY]
```

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

    log(f"\n{'='*50}")
    log(f"Phase {phase_id}: {phase_name}")
    log(f"{'='*50}")

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
    # >>> Use Read tool on phase_file, then follow its instructions <<<

    # After execution, verify outputs
    # (Phase-specific verification defined in each phase file)

    results[phase_id] = {"status": "complete"}
```

### Step 3.5: Pre-Phase-6 Traceability Audit Gate (MANDATORY)

After all data-producing phases (0–5.5) complete and BEFORE Phase 6 renders the DOCX, perform the six-rule traceability audit on the assembled JSON outputs. This is the gate that prevents shipping honest-but-imprecise claims.

**Execution model:** This gate runs as a fresh, adversarially-framed audit prompt — the same Claude that produced the outputs cannot self-audit without bias. The prompt below MUST be invoked as a discrete reasoning step with no carry-over from the data-production phases. Treat the six rules as the sole evaluation criteria; do not relax them for any output that "looks fine."

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
Mode: {'QUICK' if quick_mode else 'FULL'}

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

- [ ] All phase files read from `phases-screen/` directory
- [ ] Phases executed sequentially in main context (no Task agents)
- [ ] Phase 3 skipped when `--quick` flag present
- [ ] All JSON outputs written to `{folder}/screen/`
- [ ] `BID_SCREEN.docx` generated and > 30KB
- [ ] `BID_SCREEN.json` contains data from all phases
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
