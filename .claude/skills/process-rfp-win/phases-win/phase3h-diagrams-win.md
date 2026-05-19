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
- `{folder}/shared/UNIFIED_RTM.json` *(if exists; produced by Phase 4)* — for in-document IDs to cite
- `{folder}/shared/tech-lifecycle-evidence.json` — for evidence-backed tech labels on architecture diagrams

## Required Outputs

- `{folder}/outputs/DIAGRAM_BLUEPRINTS.md` (≥5 KB) — human-reviewable blueprint document
- `{folder}/shared/diagram-blueprints.json` — machine-readable blueprint set; consumed by phase8d-diagrams-win.md

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
PALETTE = {
    "primary":   "#0072B2",  # blue
    "accent":    "#E69F00",  # orange
    "success":   "#009E73",  # green
    "warning":   "#F0E442",  # yellow (use for text on dark fill only)
    "danger":    "#D55E00",  # vermillion
    "info":      "#56B4E9",  # sky blue
    "neutral":   "#999999",  # gray
    "critical":  "#CC79A7",  # reddish purple — risk heat-map "Critical"
    "text_dark": "#1A1A1A",
    "bg_light":  "#FFFFFF",
}

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

## Quality Checklist

- [ ] `outputs/DIAGRAM_BLUEPRINTS.md` ≥ 5 KB
- [ ] `shared/diagram-blueprints.json` written and contains all six mandatory diagrams
- [ ] Every blueprint cites at least one spec element
- [ ] Every mermaid block parses (mmdc --validate)
- [ ] Every classDef uses an Okabe-Ito palette token
- [ ] Every diagram declares `fontSize: 14px` (or higher) via theme init
- [ ] Every diagram has a persuasive `caption` AND a screen-reader `alt_text`
- [ ] Sequence-diagram messages are labeled
- [ ] Gantt critical-path tasks are flagged with the `crit` keyword
