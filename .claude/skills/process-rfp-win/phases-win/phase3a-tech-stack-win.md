---
name: phase3a-tech-stack-win
expert-role: Technology Strategist
domain-expertise: Technology lifecycle, LTS scheduling, vendor support matrices, evidence-backed version selection
---

# Phase 3a-tech-stack: Evidence-Backed Tech Stack Lookup

## Purpose

Produce `shared/tech-lifecycle-evidence.json` — the authoritative on-disk record of every primary tech component's current LTS version, EOL date, and contract-fit verdict. This phase is the ONLY producer of that file. Phase 3a (Architecture) and SVA-3 (`SVA3-TECH-STACK-LTS-VERIFIED`) both consume it.

This phase was split out from `phase3a-architecture-win.md` Step 4 on 2026-05-18 so that:

1. Architecture authoring (Phase 3a) no longer issues live WebFetch/WebSearch calls — it reads pre-verified evidence and renders ADRs deterministically.
2. The evidence file becomes a single-responsibility artifact with a clear producer/consumer contract.
3. SVA-3 has a stable, isolated phase to point at when retrying lookups (no risk of corrupting partial ARCHITECTURE.md output).

## Expert Role

You are a **Technology Strategist** with deep expertise in:

- Technology lifecycle management (LTS / STS / preview classifications)
- Vendor support matrices for runtimes, databases, frameworks, cloud platforms
- Government / enterprise contract lifecycle planning (multi-year support windows)
- Evidence-backed technology selection — never training-data guesses

## Inputs

- `{folder}/shared/domain-context.json` — for `contract_years` (drives lifecycle window)
- `{folder}/shared/requirements-normalized.json` — for any explicit tech mandates from the RFP
- `${CLAUDE_SKILL_DIR}/config-win/company-profile.json` — for the bidder's standing capabilities and preferred stacks

## Required Outputs

- `{folder}/shared/tech-lifecycle-evidence.json` — see schema below; SVA-3 will BLOCK if missing, stale (>7 days), or contains any component with `passes_contract_lifecycle: false`

## ⛔ Contract Lifecycle Rule (NON-NEGOTIABLE)

> Every proposed component's vendor-supported end-of-life MUST be `>= go_live_date + contract_years + 2`. For a 5-year contract starting 2026, that is 2033 at minimum. Components that fall short are REJECTED — propose the next supported version.

This is documented across `sva3-spec-validator-win.md` rule `SVA3-TECH-STACK-LTS-VERIFIED` and prior regression incidents where agents proposed `.NET 8` (EOL Nov 2026) or `.NET 9` (STS, EOL May 2026) from stale training-data anchors.

## Instructions

### Step 1: Load Context

```python
domain_context = read_json(f"{folder}/shared/domain-context.json")
requirements   = read_json_safe(f"{folder}/shared/requirements-normalized.json") or {}
all_reqs       = requirements.get("requirements", [])
domain         = domain_context.get("selected_domain", "Generic")
contract_years = domain_context.get("contract_years", 5)  # V1-F2 fix: dynamic, not hardcoded 7

# Capability hints from the bidder
import os as _os
skill_dir = _os.environ.get("CLAUDE_SKILL_DIR") or _os.path.dirname(_os.path.abspath(__file__))
company_profile_path = f"{skill_dir}/config-win/company-profile.json"
company_profile = read_json_safe(company_profile_path) or {}
```

### Step 2: Determine Required Components

The component LIST per layer may be domain-flavored (healthcare adds HL7 FHIR; government adds StateRAMP-aligned services), but VERSIONS come from `query_lts_for_component()` at runtime — never from training data.

```python
layers_needed = {
    "frontend":       ["react", "typescript"],       # add Next.js, Tailwind per domain
    "backend":        [".net", "asp.net"],            # OR java, OR node — pick based on bidder capability + RFP
    "database":       ["sql server"],                 # OR postgresql — pick based on RFP / domain
    "infrastructure": ["azure"],                       # OR AWS — pick based on company-profile.json
    "integration":    [],                              # populated by Phase 3b
}

# Domain overlays (component LIST only, never versions)
if domain == "healthcare":
    layers_needed["integration"].append("HL7 FHIR")
if domain in ("government", "gov", "state-government", "local-government"):
    # CIS Controls IG2 + StateRAMP context, no version constraint
    pass

# Bidder-capability overlay — if company-profile lists explicit preferences, honor them
preferred = company_profile.get("technology_preferences", {})
for layer, prefs in preferred.items():
    if isinstance(prefs, list) and layer in layers_needed:
        for p in prefs:
            if p not in layers_needed[layer]:
                layers_needed[layer].append(p)
```

### Step 3: Evidence Lookup Helper

```python
from datetime import datetime, timedelta


def query_lts_for_component(category, component_name, contract_years=5):
    """Resolve the latest vendor-supported version + EOL date for one component.

    MANDATORY: This function MUST issue a real WebFetch / WebSearch call.
    A return value that lacks `source_url` and `fetched_at` is INVALID and the
    phase must HALT, not proceed.
    """
    AUTHORITATIVE_SOURCES = {
        ".net":        "https://learn.microsoft.com/en-us/dotnet/core/releases-and-support/",
        "asp.net":     "https://learn.microsoft.com/en-us/dotnet/core/releases-and-support/",
        "node":        "https://nodejs.org/en/about/previous-releases",
        "node.js":     "https://nodejs.org/en/about/previous-releases",
        "react":       "https://react.dev/blog",
        "next.js":     "https://nextjs.org/blog",
        "postgresql":  "https://www.postgresql.org/support/versioning/",
        "sql server":  "https://learn.microsoft.com/en-us/lifecycle/products/?products=sql-server",
        "azure sql":   "https://learn.microsoft.com/en-us/azure/azure-sql/",
        "java":        "https://www.oracle.com/java/technologies/java-se-support-roadmap.html",
        "kubernetes":  "https://kubernetes.io/releases/",
        "python":      "https://devguide.python.org/versions/",
    }

    family_url = AUTHORITATIVE_SOURCES.get(component_name.lower())
    if not family_url:
        evidence = web_search(
            f"{component_name} LTS schedule end of life {datetime.now().year}"
        )
    else:
        evidence = web_fetch(family_url, prompt=(
            f"List ALL currently supported {component_name} versions with their "
            f"support classification (LTS / STS / preview) and end-of-life dates. "
            f"Identify the most recent LTS release. Return version, classification, "
            f"GA date, EOL date, support_url."
        ))

    if not evidence or "eol_date" not in evidence:
        raise PhaseHalt(
            f"PHASE 3A-TECH-STACK HALT — could not verify lifecycle for {component_name}. "
            f"DO NOT proceed with a guessed version. Resolve the lookup first."
        )

    min_required_eol = (datetime.now() + timedelta(days=365 * (contract_years + 2))).date().isoformat()
    return {
        "component": component_name,
        "category": category,
        "recommended_version": evidence["latest_lts_version"],
        "classification": evidence["classification"],  # MUST be "LTS" for primary runtimes
        "ga_date": evidence["ga_date"],
        "eol_date": evidence["eol_date"],
        "min_required_eol": min_required_eol,
        "passes_contract_lifecycle": evidence["eol_date"] >= min_required_eol,
        "source_url": evidence["source_url"],
        "fetched_at": datetime.now().isoformat(),
        # Migration-plan exception (set by Phase 3a ADR author when applicable;
        # SVA-3 reads these to record an audit-trail exception instead of failing).
        "migration_plan_present": False,
        "migration_plan_adr": None,
        "migration_plan_summary": None,
    }
```

### Step 4: Build the Stack with Full Evidence

```python
stack_evidence = []
for layer, components in layers_needed.items():
    for component in components:
        stack_evidence.append(query_lts_for_component(layer, component, contract_years))

# HARD GATE — any component that fails the lifecycle check halts the phase
failed = [c for c in stack_evidence if not c["passes_contract_lifecycle"]]
if failed:
    for c in failed:
        log(f"  ⛔ FAIL — {c['component']} {c['recommended_version']} EOL {c['eol_date']} < required {c['min_required_eol']}")
    raise PhaseHalt(
        f"PHASE 3A-TECH-STACK HALT — {len(failed)} component(s) cannot meet contract+2yr lifecycle. "
        f"Pick the next supported version (often the upcoming LTS) and re-run."
    )
```

### Step 5: Write Evidence File (the only producer)

```python
evidence_path = f"{folder}/shared/tech-lifecycle-evidence.json"
write_json(evidence_path, {
    "generated_at": datetime.now().isoformat(),
    "contract_years": contract_years,
    "min_required_eol": (datetime.now() + timedelta(days=365 * (contract_years + 2))).date().isoformat(),
    "components": stack_evidence,
})
log(f"tech-lifecycle-evidence.json written: {len(stack_evidence)} components verified")
```

### Step 6: Report Results

```
🔧 Tech-Stack Lifecycle Lookup Complete
=======================================
Contract years: {contract_years}
Minimum required EOL: {min_required_eol}
Components verified: {len(stack_evidence)}
All pass contract+2yr: {'✅' if not failed else f'❌ {len(failed)} failures'}

Components (by category):
| Category | Component | Version | Class. | EOL |
|----------|-----------|---------|--------|-----|
{table rows}
```

## Why This Is Structural, Not Advisory

- `query_lts_for_component()` CANNOT return without `source_url` + `fetched_at` — no quiet fall-through to a guessed version.
- The phase HALTS if any component fails the contract+2yr lifecycle check. Downstream phases never see a stale stack.
- `tech-lifecycle-evidence.json` is the on-disk audit trail. SVA-3 cross-checks it; without the file, SVA-3 BLOCKS the architecture phase regardless of how good ARCHITECTURE.md prose looks.

## Downstream Consumers (read-only)

- `phase3a-architecture-win.md` — reads the evidence file to build the `tech_stack` dict labels and to render the ADR section. Phase 3a no longer issues version lookups itself.
- `sva3-spec-validator-win.md` — rule `SVA3-TECH-STACK-LTS-VERIFIED` reads the file to verify freshness (<= 7 days) and that no component fails the contract+2yr lifecycle check.

## Quality Checklist

- [ ] `tech-lifecycle-evidence.json` written to `{folder}/shared/`
- [ ] Every component has `source_url` AND `fetched_at`
- [ ] No component has `passes_contract_lifecycle: false`
- [ ] No hardcoded version numbers anywhere in this phase's output
- [ ] `contract_years` resolved from `domain-context.json`, not hardcoded
