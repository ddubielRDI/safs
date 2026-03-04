# /process-rfp-screen - Lightweight RFP Screening Pipeline

---
name: process-rfp-screen
description: Lightweight RFP Screener — fast GO/NO-GO bid decision in ~15-30 minutes
argument-hint: <path-to-rfp-folder> [--quick]
allowed-tools: [Bash, Write, Edit, Glob, Grep, Read, WebFetch, WebSearch]
---

## Skill Description

Lightweight RFP screening pipeline that analyzes an RFP across 6 dimensions and produces a single `BID_SCREEN.pdf` for human GO/NO-GO judgment. Designed to run in ~15-30 minutes — use BEFORE committing to the full `/process-rfp-win` pipeline (3+ hours).

**Key Design Decisions:**
- **Grok Review: SKIPPED** — this is a screening tool, not a production bid. Speed > polish.
- **Agent Dispatch: NONE** — all phases execute inline in main context (saves ~5 min overhead).
- **Output Isolation:** All outputs in `{folder}/screen/` — never touches `shared/` or `outputs/`.

---

## Required Outputs (Completion Checklist)

| Required Output | Phase | Min Size |
|-----------------|-------|----------|
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
| `screen/BID_SCREEN.pdf` | 6 | 10KB |

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
| Bash | `markitdown`, `python3`, `pip` | Document conversion, PDF generation |
| WebSearch | Max 8 queries | Client intelligence (Phase 3 only) |

---

## Configuration

```python
# Paths — resolved relative to this skill file
SKILL_DIR = "/path/to/safs/.claude/skills/process-rfp-screen"
PHASES_DIR = f"{SKILL_DIR}/phases-screen"
CONFIG_DIR = "/path/to/safs/.claude/skills/process-rfp-win/config-win"

# Company profile — shared with full pipeline
COMPANY_PROFILE = f"{CONFIG_DIR}/company-profile.json"

# Past projects — shared with full pipeline
PAST_PROJECTS = "Past_Projects.md"  # In repo root

# Scan limits
SCAN_LIMIT = 80000  # Max chars of RFP text to analyze
MAX_WEB_SEARCHES = 8  # Client intel search budget

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
Phase 6: PDF Generation                      → phases-screen/phase6-pdf.md         [MANDATORY]
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
        "skip_condition": None
    },
    {
        "id": 1,
        "name": "RFP Summary Extraction",
        "file": "phase1-summary.md",
        "blocking": False,
        "skip_condition": None
    },
    {
        "id": 2,
        "name": "Go/No-Go Scoring",
        "file": "phase2-gonogo.md",
        "blocking": False,
        "skip_condition": None
    },
    {
        "id": 3,
        "name": "Client Intelligence Snapshot",
        "file": "phase3-intel.md",
        "blocking": False,
        "skip_condition": "quick_mode"  # Skip if --quick
    },
    {
        "id": 4,
        "name": "Compliance Quick-Check & Past Project Match",
        "file": "phase4-compliance.md",
        "blocking": False,
        "skip_condition": None
    },
    {
        "id": 4.5,
        "name": "Preliminary Win Theme Derivation",
        "file": "phase4.5-themes.md",
        "blocking": False,
        "skip_condition": None
    },
    {
        "id": 5,
        "name": "Risk Assessment & Recommendation",
        "file": "phase5-recommendation.md",
        "blocking": False,
        "skip_condition": None
    },
    {
        "id": 5.5,
        "name": "Clarifying Questions Generation",
        "file": "phase5.5-questions.md",
        "blocking": False,
        "skip_condition": None
    },
    {
        "id": 6,
        "name": "PDF Generation",
        "file": "phase6-pdf.md",
        "blocking": True,  # Pipeline fails without PDF
        "skip_condition": None
    }
]

results = {}
for phase in phases:
    phase_id = phase["id"]
    phase_name = phase["name"]

    # Check skip condition
    if phase["skip_condition"] == "quick_mode" and quick_mode:
        log(f"\nPhase {phase_id}: {phase_name} — SKIPPED (--quick mode)")
        results[phase_id] = {"status": "skipped", "reason": "--quick mode"}
        continue

    log(f"\n{'='*50}")
    log(f"Phase {phase_id}: {phase_name}")
    log(f"{'='*50}")

    # Read phase file and execute its instructions inline
    phase_file = f"{PHASES_DIR}/{phase['file']}"
    # Execute phase instructions...

    # After execution, verify outputs
    # (Phase-specific verification defined in each phase file)

    results[phase_id] = {"status": "complete"}
```

### Step 4: Final Verification

```python
log("\n" + "=" * 60)
log("FINAL VERIFICATION")
log("=" * 60)

required_outputs = [
    ("screen/rfp-summary.json", "RFP Summary", 1),
    ("screen/go-nogo-score.json", "Go/No-Go Score", 1),
    ("screen/compliance-check.json", "Compliance Check", 1),
    ("screen/past-projects-match.json", "Past Project Matches", 1),
    ("screen/preliminary-themes.json", "Preliminary Themes", 1),
    ("screen/risk-assessment.json", "Risk Assessment", 1),
    ("screen/clarifying-questions.json", "Clarifying Questions", 1),
    ("screen/BID_SCREEN.json", "Consolidated Data", 3),
    ("screen/BID_SCREEN.md", "Markdown Report", 5),
    ("screen/BID_SCREEN.pdf", "PDF Report", 10),
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
Primary: BID_SCREEN.pdf

{"Next step: /process-rfp-win " + folder if recommendation == "GO" else ""}
{"Review risks in BID_SCREEN.pdf before deciding." if recommendation == "CONDITIONAL" else ""}
{"Recommendation is NO-GO. Override available if strategic reasons exist." if recommendation == "NO_GO" else ""}
""")

log("=" * 60)
```

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
| PDF generation fails | Retry once; if still fails, output BID_SCREEN.json only |

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
- [ ] `BID_SCREEN.pdf` generated and > 10KB
- [ ] `BID_SCREEN.json` contains data from all phases
- [ ] Recommendation follows threshold rules (GO >= 50, CONDITIONAL 40-49, NO-GO < 40)
- [ ] No ghost fills in PDF (CSS constraints respected)
- [ ] `screen/` directory does not interfere with `shared/` or `outputs/`
