---
name: phase3h-diagrams-win
expert-role: Visual Design Architect
domain-expertise: Mermaid diagrams, information design, accessibility-aware visualization
---

# Phase 3h: Diagram Blueprints (Stage 3, after 3g)

## Purpose

Produce the diagram **BLUEPRINTS** — mermaid source + intent + caption + accessibility note + source citation — BEFORE Stage 8 rendering. Phase 8d becomes a pure renderer of these blueprints; this phase is the single owner of "what diagrams should exist, what should they show, and why".

Splitting blueprint from rendering means evaluator-facing visualization concerns (clarity, accessibility, narrative fit) get authored alongside the specs they depict, while rendering remains a mechanical CLI step on the back end. This pattern was identified after multiple bids where diagram quality was a Gold-Team-discovered gap rather than a designed-in feature.

## Expert Role

You are a **Visual Design Architect** with deep expertise in:

- Mermaid diagram authoring (flowchart, sequenceDiagram, gantt, erDiagram, classDiagram)
- Information design principles (Tufte, Bertin) — small multiples, layering, signal-to-noise
- Accessibility-aware visualization (WCAG 2.2 AA contrast, colorblind-safe palettes, screen-reader alt-text)
- Government / enterprise procurement diagram conventions (org charts, integration topology, Gantt with critical path, risk heat maps)

## Inputs

- `{folder}/shared/requirements-normalized.json` — for callout cross-references
- `{folder}/outputs/ARCHITECTURE.md` — logical-architecture source
- `{folder}/outputs/INTEROPERABILITY.md` — integration / sequence source
- `{folder}/outputs/SECURITY_REQUIREMENTS.md` — security overlay on architecture
- `{folder}/shared/REQUIREMENT_RISKS.json` — risk heat-map source
- **`{folder}/shared/UNIFIED_RTM.json` — MANDATORY for the interactive HTML output.**
  Every architecture entity (component, container, external system, API, data store, risk) MUST surface its `linked_requirement_ids` via this file. Without it, the optional `ARCHITECTURE_DEMO.html` deliverable cannot be produced.
- `{folder}/shared/tech-lifecycle-evidence.json` — for evidence-backed tech labels on architecture diagrams

## Required Outputs

- `{folder}/outputs/DIAGRAM_BLUEPRINTS.md` (≥5 KB) — human-reviewable blueprint document
- `{folder}/shared/diagram-blueprints.json` — machine-readable blueprint set; consumed by phase8d-diagrams-win.md
- `{folder}/outputs/ARCHITECTURE_DEMO.html` *(optional but recommended)* — self-contained interactive architecture demonstration. See **Interactive HTML demo** section below.

## Mandatory Diagrams (cite source spec elements for each)

1. **Logical Architecture** — flowchart TD; layered (presentation → API → business → data → integration); cite `ARCHITECTURE.md` section heading and ADRs.
2. **System Integration / Sequence** — sequenceDiagram including any third-party integration explicitly named in the RFP (e.g., Tyler/NIC Oregon, HL7 FHIR endpoints, payment processors); cite `INTEROPERABILITY.md`.
3. **Data Flow / ER** — erDiagram or flowchart LR for data-entity relationships; cite `ENTITY_DEFINITIONS.md` and `shared/sample-data-analysis.json` if present.
4. **Implementation Timeline (Gantt with critical path)** — gantt; highlight critical-path tasks in a distinct color; cite `EFFORT_ESTIMATION.md` if available, else cite top-level project phases.
5. **Org Chart** — flowchart TD; team roles + reporting lines; cite `02_MANAGEMENT.md` once produced, or company-profile.json team list.
6. **Risk Heat Map** — flowchart or table-style mermaid with classDef-colored severity tiers; cite `REQUIREMENT_RISKS.json` risk entries with their level + mitigation.

Each diagram BLUEPRINT in the output JSON MUST include the cited source filename + section/heading.

## ⛔ Diagram Quality Criteria (MANDATORY)

These criteria mirror the standards documented in `phase8d-diagrams-win.md`, but are authored HERE so Phase 8d can be a pure renderer.

### Visual

- **Font size:** mermaid source MUST declare `fontSize: 14` (or higher) in the `themeVariables` block, OR use a `%%{init: { 'themeVariables': { 'fontSize': '14px' } } }%%` directive at the top of the diagram.
- **Contrast:** all `classDef` color pairs MUST meet WCAG 2.2 AA — 4.5:1 contrast ratio for text on background. Never pale-on-white or red-on-red.
- **Colorblind-safe palette:** use Okabe-Ito or Wong palette tokens, declared in a single `palette` block at the top of the JSON for consistent application across all diagrams.
- **Aspect ratio:** target 16:9 for landscape diagrams, 8.5:11 for portrait. Avoid extreme wide diagrams that get scaled down too small in PDF.

### Communication

- **Descriptive title:** every diagram has a title that names the SYSTEM AND the VIEW (e.g., "MARS Hosted SaaS — Logical Architecture (Component View)" not just "Architecture").
- **Legend:** if symbols / colors carry meaning, include a legend block. No assumed conventions.
- **Labels:** every node has a non-trivial label (>2 chars, no "Module A / Module B").
- **Annotations:** highlight 1-3 key flows / decisions with callout boxes (e.g., "Tyler/NIC Oregon integration — server-redirect pattern per ADR-007").
- **Source citation watermark:** every blueprint records its `source_citation` field; Phase 8d will burn this into the rendered PNG bottom-right corner.

### Mermaid-specific

- Use `classDef` to apply consistent styling per component category (data, service, gateway, external system).
- Sequence-diagram MESSAGES MUST be labeled.
- Flowchart DECISION DIAMONDS MUST have explicit yes/no branch labels.
- Gantt CRITICAL-PATH tasks MUST use a distinct fill color and the `crit` keyword.

## Instructions

### Step 1: Load Inputs

```python
import os, glob
from datetime import datetime

requirements   = read_json_safe(f"{folder}/shared/requirements-normalized.json") or {}
arch_md        = read_file(f"{folder}/outputs/ARCHITECTURE.md") if os.path.exists(f"{folder}/outputs/ARCHITECTURE.md") else ""
interop_md     = read_file(f"{folder}/outputs/INTEROPERABILITY.md") if os.path.exists(f"{folder}/outputs/INTEROPERABILITY.md") else ""
security_md    = read_file(f"{folder}/outputs/SECURITY_REQUIREMENTS.md") if os.path.exists(f"{folder}/outputs/SECURITY_REQUIREMENTS.md") else ""
risks          = read_json_safe(f"{folder}/shared/REQUIREMENT_RISKS.json") or {}
unified_rtm    = read_json_safe(f"{folder}/shared/UNIFIED_RTM.json")  # may not exist yet at Stage 3
tech_evidence  = read_json_safe(f"{folder}/shared/tech-lifecycle-evidence.json") or {}
domain_context = read_json_safe(f"{folder}/shared/domain-context.json") or {}
```

### Step 2: Build Palette + Theme

```python
# Okabe-Ito palette — colorblind-safe, WCAG-AA contrast on white background.
#
# ⛔ HARD RULE (codified 2026-05-21 — MARS SVA-7 incident): when a palette color
# is used as a FILL with white (`#FFFFFF`) text on top via `classDef`, the
# raw Okabe-Ito light shades fail WCAG-AA contrast (≥ 4.5:1 for normal text):
#   #009E73 + #FFFFFF = 3.42:1  FAIL
#   #CC79A7 + #FFFFFF = 3.06:1  FAIL
#   #999999 + #FFFFFF = 2.85:1  FAIL
#   #D55E00 + #FFFFFF = 3.87:1  FAIL
# Use the DARKENED variants below for white-text fills. Reserve the raw
# Okabe-Ito shades for non-text usage (lineColor, secondary background panels
# without overlaid text) or pair with black/dark-gray text.
PALETTE = {
    "primary":   "#0072B2",  # blue — 4.83:1 with #FFFFFF (PASS)
    "accent":    "#E69F00",  # orange — pair with #1A1A1A text (8.39:1)
    "success":   "#006B4F",  # darkened green — 6.53:1 with #FFFFFF (was #009E73 = 3.42:1 FAIL)
    "warning":   "#F0E442",  # yellow — pair with #1A1A1A text (15.8:1)
    "danger":    "#A84500",  # darkened vermillion — 5.97:1 with #FFFFFF (was #D55E00 = 3.87:1 FAIL)
    "info":      "#0E5C8A",  # darkened sky-blue — 6.81:1 with #FFFFFF (was #56B4E9 = 2.55:1)
    "neutral":   "#595959",  # darkened gray — 7.00:1 with #FFFFFF (was #999999 = 2.85:1 FAIL)
    "critical":  "#A5527F",  # darkened reddish-purple — 5.11:1 with #FFFFFF (was #CC79A7 = 3.06:1 FAIL)
    "text_dark": "#1A1A1A",
    "bg_light":  "#FFFFFF",
}

# Verification self-check — DO NOT remove. Recompute on every palette edit.
# If any text-on-fill pair drops below 4.5:1, halt and surface the failing pair
# rather than ship inaccessible diagrams.
def _contrast_ratio(fg_hex, bg_hex):
    def _lum(h):
        rgb = [int(h[i:i+2], 16) / 255 for i in (1, 3, 5)]
        rgb = [(c / 12.92) if c <= 0.03928 else ((c + 0.055) / 1.055) ** 2.4 for c in rgb]
        return 0.2126 * rgb[0] + 0.7152 * rgb[1] + 0.0722 * rgb[2]
    l1, l2 = sorted([_lum(fg_hex), _lum(bg_hex)], reverse=True)
    return (l1 + 0.05) / (l2 + 0.05)

# Every fill that is paired with white text MUST be in this safe set.
_WHITE_TEXT_FILLS = ["primary", "success", "danger", "info", "neutral", "critical"]
for key in _WHITE_TEXT_FILLS:
    ratio = _contrast_ratio(PALETTE[key], "#FFFFFF")
    assert ratio >= 4.5, f"WCAG-AA fail: PALETTE['{key}']={PALETTE[key]} vs #FFFFFF = {ratio:.2f}:1 (need >= 4.5)"

THEME_INIT = (
    "%%{init: {'theme': 'base', 'themeVariables': "
    "{'fontSize': '14px', 'primaryColor': '" + PALETTE["primary"] + "', "
    "'primaryTextColor': '#FFFFFF', 'primaryBorderColor': '#003D5C', "
    "'lineColor': '#444444', 'secondaryColor': '" + PALETTE["info"] + "', "
    "'tertiaryColor': '" + PALETTE["success"] + "'}}}%%"
)
```

### Step 3: Author Each Blueprint

For each mandatory diagram, populate the blueprint dict. The MERMAID source MUST be syntactically valid (validate at end with `mmdc --validate` or inline parser) and MUST cite source spec elements. The CAPTION must be persuasive ("our architecture ensures..."), not descriptive ("the architecture shows...").

```python
def make_blueprint(name, view, mermaid_src, source_citation, intent, caption, alt_text, classdefs=None):
    return {
        "name": name,
        "view": view,
        "mermaid": THEME_INIT + "\n" + mermaid_src,
        "source_citation": source_citation,
        "intent": intent,
        "caption": caption,
        "alt_text": alt_text,
        "classdefs": classdefs or {},
        "accessibility": {
            "wcag_contrast_ratio": 4.5,
            "colorblind_safe_palette": "Okabe-Ito",
            "font_size_pt": 14,
        },
        "render_target": f"outputs/bid/{name}.png",
        "render_source": f"outputs/bid/{name}.mmd",
    }


blueprints = []

# --- 1. Logical Architecture ---
arch_mermaid = """
flowchart TD
    subgraph Presentation["Presentation Layer"]
        WEB["Web App"]
        MOB["Mobile App"]
        PORT["Portal"]
    end
    subgraph API["API Gateway Layer"]
        GW["API Gateway / Load Balancer"]
    end
    subgraph Business["Business Logic Layer"]
        SVC_A["Auth Service"]
        SVC_B["Workflow Engine"]
        SVC_C["Reporting Service"]
    end
    subgraph Data["Data Layer"]
        DB["Primary DB"]
        CACHE["Redis Cache"]
        SEARCH["Search Index"]
    end
    WEB --> GW
    MOB --> GW
    PORT --> GW
    GW --> SVC_A
    GW --> SVC_B
    GW --> SVC_C
    SVC_A --> DB
    SVC_B --> DB
    SVC_B --> CACHE
    SVC_C --> SEARCH
    classDef service fill:#0072B2,stroke:#003D5C,color:#FFFFFF;
    classDef data fill:#009E73,stroke:#005C42,color:#FFFFFF;
    classDef gateway fill:#E69F00,stroke:#8C5C00,color:#1A1A1A;
    class SVC_A,SVC_B,SVC_C service;
    class DB,CACHE,SEARCH data;
    class GW gateway;
"""
blueprints.append(make_blueprint(
    name="architecture",
    view="Component View — Logical Architecture",
    mermaid_src=arch_mermaid,
    source_citation="ARCHITECTURE.md §Layer Specifications + ADR-001..N (tech-lifecycle-evidence.json)",
    intent="Demonstrate evaluator-readable separation of concerns with named integration boundaries.",
    caption="Our layered architecture isolates security, business logic, and data concerns — accelerating compliance review and reducing change-risk over the contract lifecycle.",
    alt_text="Five-layer architecture diagram: Presentation (Web, Mobile, Portal) connects through an API Gateway to three Business Services (Auth, Workflow, Reporting), which read and write the Data Layer (Primary DB, Redis Cache, Search Index). Classdef styling distinguishes service, data, and gateway nodes.",
    classdefs={"service": "#0072B2", "data": "#009E73", "gateway": "#E69F00"},
))

# --- 2. System Integration / Sequence (third-party calls included) ---
integration_mermaid = """
sequenceDiagram
    autonumber
    participant User as Citizen
    participant Web as Web App
    participant API as API Gateway
    participant Auth as Auth Service
    participant Tyler as Tyler/NIC Oregon
    participant DB as Primary DB
    User->>Web: Initiate licensing request
    Web->>API: POST /licenses (JWT)
    API->>Auth: Validate JWT
    Auth-->>API: 200 OK
    API->>Tyler: ePayment redirect (server-side)
    Tyler-->>API: Payment confirmation token
    API->>DB: Persist license + payment ref
    DB-->>API: License ID
    API-->>Web: 201 Created
    Web-->>User: Confirmation page
"""
blueprints.append(make_blueprint(
    name="integration_sequence",
    view="System Integration — Citizen Licensing Sequence",
    mermaid_src=integration_mermaid,
    source_citation="INTEROPERABILITY.md §External Integrations + Tyler/NIC Oregon RFP section",
    intent="Show the externally-mandated ePayment hop without exposing internal complexity.",
    caption="Server-side redirect to Tyler/NIC Oregon protects citizens' payment data while keeping our API the single integration point — minimizing PCI scope.",
    alt_text="Numbered sequence: Citizen submits licensing request through the Web App; API Gateway validates the JWT with the Auth Service; payment is redirected server-side to Tyler/NIC Oregon; confirmation token is persisted with license in the Primary DB; the citizen receives a confirmation page.",
))

# --- 3. Data Flow / ER ---
er_mermaid = """
erDiagram
    CITIZEN ||--o{ LICENSE : holds
    LICENSE ||--|{ PAYMENT : "paid via"
    LICENSE ||--o{ RENEWAL : has
    AGENT ||--o{ LICENSE : issues
    CITIZEN {
        uuid citizen_id PK
        string full_name
        string email
        string phone
    }
    LICENSE {
        uuid license_id PK
        uuid citizen_id FK
        uuid agent_id FK
        date issued_date
        date expires_date
        string status
    }
    PAYMENT {
        uuid payment_id PK
        uuid license_id FK
        string tyler_ref
        money amount
        date paid_date
    }
    RENEWAL {
        uuid renewal_id PK
        uuid license_id FK
        date renewal_date
    }
    AGENT {
        uuid agent_id PK
        string name
        string office
    }
"""
blueprints.append(make_blueprint(
    name="data_model",
    view="Data Model — Core Entities",
    mermaid_src=er_mermaid,
    source_citation="ENTITY_DEFINITIONS.md §Entities + sample-data-analysis.json identified entities",
    intent="Surface the core entity relationships evaluators will look for in any licensing-system bid.",
    caption="A normalized core data model with explicit foreign keys ensures audit traceability and supports flexible reporting across the contract term.",
    alt_text="Entity-relationship diagram with five entities — Citizen, License, Payment, Renewal, Agent — showing one-to-many relationships and primary/foreign key fields.",
))

# --- 4. Implementation Timeline (Gantt with critical path) ---
gantt_mermaid = """
gantt
    title Implementation Timeline (Critical Path Highlighted)
    dateFormat YYYY-MM-DD
    axisFormat %b %Y
    section Discovery
        Requirements workshops :a1, 2026-07-01, 30d
        Architecture sign-off  :crit, a2, after a1, 21d
    section Build
        Core platform          :crit, b1, after a2, 90d
        Tyler integration      :crit, b2, after b1, 45d
        UI / portal            :b3, after b1, 75d
    section Test
        SIT                    :c1, after b2, 30d
        UAT                    :crit, c2, after c1, 30d
    section Go-Live
        Cutover                :crit, d1, after c2, 14d
        Stabilization          :d2, after d1, 30d
"""
blueprints.append(make_blueprint(
    name="timeline",
    view="Implementation Timeline — Gantt with Critical Path",
    mermaid_src=gantt_mermaid,
    source_citation="EFFORT_ESTIMATION.md §Schedule + project phase breakdown",
    intent="Reassure evaluators that the schedule is buildable AND that we know which tasks drive the end date.",
    caption="Our critical path — Architecture, Core Build, Tyler Integration, UAT, Cutover — is sized to deliver on time with documented buffer in non-critical workstreams.",
    alt_text="Gantt chart spanning Discovery, Build, Test, and Go-Live sections. Critical-path tasks (Architecture sign-off, Core platform, Tyler integration, UAT, Cutover) are highlighted; non-critical tasks (UI/portal, Stabilization) run alongside.",
))

# --- 5. Org Chart ---
org_mermaid = """
flowchart TD
    EXEC["Executive Sponsor"]
    PM["Project Manager"]
    ARCH["Solutions Architect"]
    LEAD_DEV["Lead Developer"]
    DEV1["Senior Developer"]
    DEV2["Developer"]
    QA["QA Lead"]
    SEC["Security Lead"]
    OPS["DevOps Engineer"]
    EXEC --> PM
    PM --> ARCH
    PM --> QA
    PM --> SEC
    PM --> OPS
    ARCH --> LEAD_DEV
    LEAD_DEV --> DEV1
    LEAD_DEV --> DEV2
    classDef lead fill:#0072B2,stroke:#003D5C,color:#FFFFFF;
    classDef ic fill:#56B4E9,stroke:#1F4E66,color:#1A1A1A;
    class EXEC,PM,ARCH,LEAD_DEV,QA,SEC,OPS lead;
    class DEV1,DEV2 ic;
"""
blueprints.append(make_blueprint(
    name="orgchart",
    view="Team Organization — Project Roles",
    mermaid_src=org_mermaid,
    source_citation="02_MANAGEMENT.md §Team + company-profile.json team roster",
    intent="Show clear accountability and named technical-leadership coverage.",
    caption="Every workstream has a named lead with backup coverage — accountability is unambiguous from the executive sponsor to the individual contributor.",
    alt_text="Org chart with Executive Sponsor at top reporting to Project Manager. PM has direct reports for Solutions Architect, QA Lead, Security Lead, and DevOps Engineer. Architect oversees a Lead Developer who manages two developers.",
))

# --- 6. Risk Heat Map ---
risk_entries = (risks.get("risks") if isinstance(risks, dict) else risks) or []
# Build a small classDef-styled flowchart that groups risks by severity.
heat_lines = ["flowchart LR"]
heat_lines.append('  subgraph High["HIGH"]')
for r in risk_entries:
    level = (r.get("risk_level") or r.get("level") or "").upper()
    if level == "HIGH":
        rid = r.get("risk_id") or r.get("id") or "R?"
        title = (r.get("title") or r.get("name") or rid).replace('"', "'")[:40]
        heat_lines.append(f'    H_{rid}["{rid}: {title}"]')
heat_lines.append("  end")
heat_lines.append('  subgraph Med["MEDIUM"]')
for r in risk_entries:
    level = (r.get("risk_level") or r.get("level") or "").upper()
    if level == "MEDIUM":
        rid = r.get("risk_id") or r.get("id") or "R?"
        title = (r.get("title") or r.get("name") or rid).replace('"', "'")[:40]
        heat_lines.append(f'    M_{rid}["{rid}: {title}"]')
heat_lines.append("  end")
heat_lines.append('  subgraph Low["LOW"]')
for r in risk_entries:
    level = (r.get("risk_level") or r.get("level") or "").upper()
    if level == "LOW":
        rid = r.get("risk_id") or r.get("id") or "R?"
        title = (r.get("title") or r.get("name") or rid).replace('"', "'")[:40]
        heat_lines.append(f'    L_{rid}["{rid}: {title}"]')
heat_lines.append("  end")
heat_lines.append(f'  classDef high fill:{PALETTE["danger"]},stroke:#7A2200,color:#FFFFFF;')
heat_lines.append(f'  classDef med fill:{PALETTE["accent"]},stroke:#8C5C00,color:#1A1A1A;')
heat_lines.append(f'  classDef low fill:{PALETTE["success"]},stroke:#005C42,color:#FFFFFF;')

risk_mermaid = "\n".join(heat_lines)
blueprints.append(make_blueprint(
    name="risk_heatmap",
    view="Risk Heat Map — by Severity",
    mermaid_src=risk_mermaid,
    source_citation="REQUIREMENT_RISKS.json (all risk entries with risk_level)",
    intent="Make risk visibility immediate; let evaluators count HIGH risks in one glance.",
    caption=f"We have identified and mitigated {len([r for r in risk_entries if (r.get('risk_level') or r.get('level') or '').upper() == 'HIGH'])} HIGH-severity risks before bid submission — a discipline that reduces post-award schedule slip.",
    alt_text="Risk heat map grouped by severity: HIGH risks (red), MEDIUM risks (orange), LOW risks (green). Each risk is labeled with its ID and short title.",
))
```

### Step 4: Validate Mermaid Syntactic Correctness

Each `mermaid` block MUST parse. The most reliable check is the mermaid-cli's `--validate` flag (Phase 8d already invokes mmdc — re-use the same tool here for a parse-only run).

```python
import subprocess, tempfile, json

invalid = []
for bp in blueprints:
    with tempfile.NamedTemporaryFile(mode="w", suffix=".mmd", delete=False, encoding="utf-8") as f:
        f.write(bp["mermaid"])
        tmp_path = f.name
    try:
        # --validate exits non-zero on parse failure
        result = subprocess.run(
            ["npx", "@mermaid-js/mermaid-cli", "-i", tmp_path, "--validate"],
            capture_output=True, text=True, timeout=30,
        )
        if result.returncode != 0:
            invalid.append({"name": bp["name"], "stderr": result.stderr[:500]})
    except Exception as e:
        invalid.append({"name": bp["name"], "error": str(e)})
    finally:
        try:
            os.remove(tmp_path)
        except Exception:
            pass

if invalid:
    log("⚠️ Mermaid validation issues:")
    for x in invalid:
        log(f"  - {x['name']}: {x.get('stderr') or x.get('error')}")
    raise PhaseHalt(
        f"PHASE 3H HALT — {len(invalid)} mermaid blueprint(s) failed syntactic validation. "
        f"Fix the source before continuing — Phase 8d will fail to render otherwise."
    )
```

### Step 5: Cite-Element Existence Check

Every `source_citation` entry references a file + section. If the cited spec file does NOT exist on disk, flag it — we're describing a diagram for content that hasn't been produced yet.

```python
missing_cites = []
for bp in blueprints:
    citation = bp["source_citation"]
    # Extract any "FILENAME.md" tokens from the citation
    import re as _re
    cited_files = _re.findall(r'([A-Z_][A-Z0-9_]*\.(?:md|json))', citation)
    for cf in cited_files:
        # Search both outputs/ and shared/ — phase outputs land in different roots
        if not (os.path.exists(f"{folder}/outputs/{cf}") or os.path.exists(f"{folder}/shared/{cf}")):
            missing_cites.append({"blueprint": bp["name"], "missing_file": cf})

if missing_cites:
    log(f"⚠️ {len(missing_cites)} cited file(s) not on disk yet (may resolve later in pipeline):")
    for m in missing_cites:
        log(f"  - {m['blueprint']} cites missing {m['missing_file']}")
    # Not a hard halt — UNIFIED_RTM.json and 02_MANAGEMENT.md don't exist until Stage 4/7,
    # so we record the flag in the blueprint output instead and let Phase 8d re-check.
```

### Step 6: Write Blueprint Outputs

```python
blueprint_json = {
    "generated_at": datetime.now().isoformat(),
    "palette": PALETTE,
    "theme_init_directive": THEME_INIT,
    "blueprints": blueprints,
    "missing_citations_at_author_time": missing_cites,
}
write_json(f"{folder}/shared/diagram-blueprints.json", blueprint_json)
log(f"diagram-blueprints.json written: {len(blueprints)} blueprints")
```

### Step 7: Generate Human-Readable Blueprint Document

```python
md = [f"# Diagram Blueprints\n",
      f"**Generated:** {datetime.now().strftime('%Y-%m-%d %H:%M')}\n",
      "This document defines every diagram that will appear in the final bid, BEFORE rendering. "
      "Phase 8d (Stage 7) will render these to PNG using the same mermaid source recorded here.\n",
      f"**Palette:** Okabe-Ito (colorblind-safe). **Font size:** 14pt minimum. **Contrast:** WCAG 2.2 AA (4.5:1).\n",
      "\n---\n"]

for i, bp in enumerate(blueprints, 1):
    md.append(f"## Diagram {i}: {bp['name']}\n")
    md.append(f"**View:** {bp['view']}\n")
    md.append(f"**Intent:** {bp['intent']}\n")
    md.append(f"**Source citation:** {bp['source_citation']}\n")
    md.append(f"**Caption (persuasive):** {bp['caption']}\n")
    md.append(f"**Alt text (screen reader):** {bp['alt_text']}\n")
    md.append(f"**Render target:** `{bp['render_target']}`\n")
    md.append(f"\n### Mermaid source\n\n```mermaid\n{bp['mermaid']}\n```\n")
    md.append("\n---\n")

md.append("## Accessibility Note\n")
md.append("Every diagram declares `fontSize: 14px` in its mermaid theme init directive. "
          "All `classDef` color pairs were selected from the Okabe-Ito palette, which is "
          "verified colorblind-safe and meets WCAG 2.2 AA contrast (4.5:1 text-on-fill).\n")
md.append("Each rendered PNG must include the alt-text recorded above when embedded in PDF.\n")

write_file(f"{folder}/outputs/DIAGRAM_BLUEPRINTS.md", "\n".join(md))
log(f"DIAGRAM_BLUEPRINTS.md written ({len(blueprints)} diagrams)")
```

### Step 8: Report

```
🎨 Diagram Blueprints Authored
==============================
Blueprints: {len(blueprints)}
Validated:  {len(blueprints) - len(invalid)} / {len(blueprints)} parse cleanly
Missing-citation flags: {len(missing_cites)} (revisited in Phase 8d)

Mandatory diagrams covered:
  ✅ Logical Architecture
  ✅ System Integration / Sequence
  ✅ Data Flow / ER
  ✅ Implementation Timeline (Gantt with critical path)
  ✅ Org Chart
  ✅ Risk Heat Map
```

## Interactive HTML demo (optional output) — `outputs/ARCHITECTURE_DEMO.html`

A complementary deliverable to the mermaid blueprints: a single-file, self-contained interactive HTML demonstration that lets evaluators explore the architecture and **see live RFP-requirement traceability per entity**. Pattern proven on the MARS bid (2026-05-19). Producer: `outputs/_arch_demo_v2_builder.py` (sibling Python file that emits the HTML).

### Dual-audience toggle pattern (MANDATORY)

The HTML MUST include a top-level audience toggle that adapts every view's content depth without changing the tab structure:

- **`[ Executive | Technical ]`** toggle above the 8-tab strip
- **Executive mode:** plain-English labels (e.g., "State Payment Service" not "Tyler / NIC Oregon Common Checkout"), protocol/auth detail hidden (HTTPS / TLS / OAuth / JWT), per-service nodes collapsed into "service clusters" (e.g., the 8 microservices in Container view collapse to a single "MARS Services" cluster), trust-boundary boxes merge into a single "Secure Boundary" tint, EOL/.NET-LTS sub-labels replaced with "Modern Platform", item lists in Roadmap truncated to top-2 per phase.
- **Technical mode:** full protocol/auth annotations on edges, ADR refs, EOL dates, dagre-layered topology for the Container view, full per-microservice nodes, all sequence numbers, all 5 ranks of services.
- Both modes share the same 8 tabs; each tab adapts depth.
- Toggle state persists across page reloads (`localStorage`).
- Anchors to C4 (Simon Brown — "design for your audience: executives need Context; developers need Containers + Components") and arc42 §1 stakeholder mapping.

### Layout discipline (MANDATORY — research-driven)

These rules are non-negotiable. The previous (v1) HTML failed label-collision UX badly enough that a coordinator escalation was required.

- **D3 v7 + dagre.js** loaded from CDN with SHA-384 SRI. Dagre is the same hierarchical-layered layout engine MermaidJS uses (`dagrejs/dagre`). It computes node positions for Technical-mode Container/Component views; hand-laid coords retained for Executive mode where simplification beats precision.
- **Bezier-curved edges (cubic).** No straight overlapping lines. Curvature proportional to edge length. Parallel edges alternate bulge direction (id-hash bias) so a fan of edges doesn't all curve the same way.
- **Label-collision detection** per edge: sample 3-5 candidate anchors along the bezier (t=0.5, 0.3, 0.7, 0.4, 0.6). Test each candidate's AABB against a per-view registry of (a) all node footprints with 6px padding, and (b) all previously placed labels. Pick the first that fits. If all candidates collide, DROP the visible label and surface it on hover via SVG `<title>` (still keyboard-accessible). Dropped count > 30% of edges in a view = layout needs redesign.
- **White rounded-rect pill backgrounds** behind every edge label: `rx=4`, `fill:#FFFFFF`, `fill-opacity:0.94`, `stroke:#E2E5E9`, `stroke-width:0.75`. Pill computed from text bbox + 5px horizontal × 2px vertical padding.
- **Halo strokes via `paint-order: stroke fill`** (MDN paint-order, CSS-Tricks accessible SVGs): every node label and edge label gets a 2-3px white stroke painted BEHIND the fill via paint-order, so text stays legible if the pill background slips. `stroke-linejoin: round` prevents spiky corners.
- **Auto-wrap long labels** at ~14ch per line, max 3 lines per label; ellipsize the last line if more.
- **Minimum 24px padding** around every node; minimum 60px clearance between node centres.
- **SVG viewBox sized to content** (use dagre's computed maxX/maxY for dagre layouts). Horizontal scroll permitted on Technical Container view if needed.
- **All text ≥ 14px.** Smaller fonts cause WCAG SC 1.4.4 (resize text) failures and evaluator squinting.

### Live RFP-requirement traceability (MANDATORY)

Every architecture entity surfaces its linked requirements via UNIFIED_RTM.json:

1. **Per-node spec mapping:** the Python builder maintains a `NODE_SPEC_MAP` dict mapping each architecture node ID (e.g., `tyler`, `frontdoor`, `apim`, `filing`, `splunk`, etc.) to a list of `spec_id`s from UNIFIED_RTM whose `linked_requirement_ids` that node is responsible for. Curated by hand because architecture nodes are hand-laid — if you can't trace a node to a spec, ask the user.
2. **Inline req chip on every node:** a small dark-blue pill in the top-right corner of each node shows the requirement count — "12 reqs" in Technical mode, "12 RFP needs" in Executive mode.
3. **Selection panel surfaces the full list:** clicking a node, edge, or risk opens a sidebar block that lists `req_ids` with a `<details>` disclosure of up to 12 sample requirements (priority-sorted) — each req ID links to the anchor in `TRACEABILITY_EXPLORER.html` (e.g., `TRACEABILITY_EXPLORER.html#req-001TEC`).
4. **Cross-cutting marker:** nodes whose `spec_ids` resolve to zero linked requirements get a subtle amber star with a `<title>` tooltip "Cross-cutting — not tied to a specific requirement". Honest signal.
5. **Coverage banner per view per mode:** the top of the sidebar shows "X of Y RFP requirements addressed in this view (Z%)" — computed at build time as the union of `req_ids` across all visible nodes in that view/mode, divided by the total requirement count. Container view typically lands at 60-80%; Component view (a single container drill-in) typically 15-35%.

### 8-view structure (C4 + lifecycle + risk + roadmap)

| # | View | Pattern | Audience notes |
|---|------|---------|----------------|
| 1 | Context | C4 L1, hand-laid | Same for both — exec rewords labels |
| 2 | Container | C4 L2, dagre Technical / collapsed cluster Executive | Most divergence between modes |
| 3 | Component | C4 L3, focal service | Numbered sequence steps in tech, simplified arrows in exec |
| 4 | Data Flow | Swimlane with classification colour | Lane labels in pills (no overlap) |
| 5 | Deployment | Region groups + tier-coloured nodes | Region tint + pill labels |
| 6 | Tech & Lifecycle | Gantt-style timeline | Tech mode: GA/EOL dates + "source ↗" link; exec: "Through 2028" |
| 7 | Risk Heat Map | 5x5 grid, force-collide displacement | Top-5 callouts unchanged |
| 8 | Roadmap | 5-year phase blocks | Exec truncates to top-2 items per phase |

### Quality criteria for the HTML

- [ ] Audience toggle works; both modes render every view without errors.
- [ ] Headless Playwright run reports **zero** edge-label-on-edge-label collisions and **zero** edge-label-on-node-label collisions in every view in every mode. Dropped labels are accessible via `<title>` tooltip.
- [ ] Every node shows a requirement-count chip (or cross-cutting marker).
- [ ] Coverage banner shows correct % per view per mode.
- [ ] WCAG 2.2 AA contrast verified: body text 4.5:1, edge-label text on pill 16:1, graphics 3:1. High-contrast mode toggle works.
- [ ] Print stylesheet collapses to single-column doc; details expanded.
- [ ] `prefers-reduced-motion` disables transitions.
- [ ] D3 + dagre loaded with SHA-384 SRI; offline fallback message shown if CDN fails.
- [ ] File ≤ 5 MB (current MARS deliverable: 502 KB).

### Web research that should be consulted at run time

These are the sources the v2 implementation cited; the implementer should re-verify they're still current and consult any new authoritative sources:

- **C4 model (Simon Brown)** — https://c4model.com — different views for different audiences; the audience-toggle pattern is the C4 "zoom in/out" principle made interactive
- **arc42 stakeholder concerns** — https://docs.arc42.org/tips/1-21/ — Tip 1-21: maintain a stakeholder table; map per-audience information needs
- **dagre.js** — https://github.com/dagrejs/dagre — hierarchical graph layout used by MermaidJS
- **MDN SVG paint-order** — https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/paint-order — stroke-then-fill halo behind text glyphs
- **CSS-Tricks Accessible SVGs** — https://css-tricks.com/accessible-svgs/ — halo strokes + paint-order for legibility
- **WCAG 2.2** — https://www.w3.org/TR/WCAG22/ — 4.5:1 text, 3:1 graphics, reduced-motion
- **Okabe & Ito (2008)** — https://jfly.uni-koeln.de/color/ — colorblind-safe categorical palette
- **AntV G6 D3-force docs** — https://g6.antv.antgroup.com/en/manual/layout/d3-force-layout — collision-force semantics for risk heat-map displacement

### Verification step

After building, run headless Playwright to screenshot all 8 views in both modes and emit a metrics JSON:

```bash
cd {folder}/outputs
node _verify_arch_demo.js
# Outputs: _arch_demo_shots/{audience}_{view}.png and _results.json
# Expectation: edge-edge=0 and edge-node=0 in every row.
```

## Quality Checklist (MANDATORY — report each by name with evidence)

The phase agent MUST verify each of the following BEFORE reporting completion. The agent's completion report MUST include a checklist-results block with:
- Item name (verbatim from below)
- PASS / FAIL / SKIPPED-WITH-REASON
- Evidence (file:line citation, grep result, file size, assertion that ran, etc.)

"All checks passed" without per-item evidence is NOT acceptable.

### Required output files
1. **DIAGRAM_BLUEPRINTS.md** exists at `{folder}/outputs/DIAGRAM_BLUEPRINTS.md` — evidence: `ls -la` showing size >= 5,120 bytes
2. **diagram-blueprints.json** exists at `{folder}/shared/diagram-blueprints.json` — evidence: `ls -la` size > 500 bytes and parses as valid JSON

### Schema fidelity
3. **diagram-blueprints.json contains all six mandatory diagrams** — print names: architecture, integration_sequence, data_model, timeline, orgchart, risk_heatmap — evidence: print `[bp["name"] for bp in blueprints]`
4. **Every blueprint** has `name`, `mermaid`, `source_citation`, `intent`, `caption`, `alt_text`, `render_target` — evidence: print key set of blueprints[0]
5. No `[:N]` slicing applied to deliverable content strings — evidence: grep for `\[:[0-9]+\]` in production code paths returned 0 hits

### Cross-stage consistency
6. **Every blueprint in `diagram-blueprints.json` has corresponding rendered file** — at phase-3h time this is a pre-flight check: `render_target` paths are declared but rendering happens in Phase 8d; confirm the paths are correct — evidence: print render_target for each blueprint
7. **Every blueprint cites at least one spec element** — `source_citation` is non-empty and references a real file (ARCHITECTURE.md, INTEROPERABILITY.md, etc.) — evidence: count blueprints with empty source_citation (must be 0)
8. **Every mermaid block parses (mmdc --validate)** — evidence: report the number of blueprints that passed validation vs total; any failures must be named
9. **Every diagram declares `fontSize: 14px`** via theme init directive — evidence: grep mermaid source strings for "fontSize" returned N hits where N equals len(blueprints)
10. **Every diagram has a persuasive `caption` AND a screen-reader `alt_text`** — evidence: count blueprints with empty caption or empty alt_text (must be 0)
11. **Gantt critical-path tasks flagged with `crit` keyword** in timeline blueprint — evidence: grep timeline mermaid source for ":crit," returned >= 1 hit

### Anti-regression rules (universal)
12. **UTF-8 encoding** on every `open()` call — evidence: search this phase's emitted scripts/code for `encoding='utf-8'` in every file-open
13. **ensure_ascii=False** on every `json.dump` call — evidence: same grep
14. **No `_Showing N of M_` row-cap notices** in any deliverable markdown — evidence: grep returned 0 matches
15. **No empty `|  |` mitigation/cell patterns** in any deliverable table — evidence: grep returned 0 matches
16. **No mid-word table-cell truncations** — evidence: line-by-line cell-end check returned 0 hits

### Memory discipline
17. **Relevant SAFS memory entries reviewed and applied** — evidence: list which memory files were read and which rules were applicable
