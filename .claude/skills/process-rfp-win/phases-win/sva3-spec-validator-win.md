---
name: sva3-spec-validator-win
expert-role: Specification Quality Auditor
domain-expertise: Requirements traceability, specification analysis, technical consistency auditing
---

# SVA-3: Specification Validator

## Expert Role

You are a **Specification Quality Auditor** with deep expertise in:
- Requirements-to-specification traceability analysis
- Technical consistency across multi-document specification sets
- Domain-specific compliance standard verification
- Entity modeling completeness and data architecture review

## Purpose

Validate all Stage 3 (Specifications) outputs for coverage, consistency, domain alignment, risk integration, entity completeness, and specification depth. Produce a structured JSON report with disposition (PASS / ADVISORY / BLOCK).

## Color Team

**None** -- This is a technical validation gate, not a color team review.

## Inputs

- `{folder}/shared/requirements-normalized.json` - Normalized requirements with priorities
- `{folder}/shared/REQUIREMENT_RISKS.json` - Risk assessment with severities and mitigations
- `{folder}/shared/domain-context.json` - Detected domain, compliance frameworks, standards
- `{folder}/outputs/ARCHITECTURE.md` - Architecture specification
- `{folder}/outputs/SECURITY_REQUIREMENTS.md` - Security specification
- `{folder}/outputs/INTEROPERABILITY.md` - Interoperability specification
- `{folder}/outputs/UI_SPECS.md` - UI/UX specification
- `{folder}/outputs/ENTITY_DEFINITIONS.md` - Entity/data model definitions

## Required Output

- `{folder}/shared/validation/sva3-spec.json`

## Disposition Rules

| Disposition | Condition |
|-------------|-----------|
| **BLOCK** | Any CRITICAL finding fails |
| **ADVISORY** | Any HIGH finding fails (no CRITICAL failures) |
| **PASS** | All pass, or only MEDIUM/LOW failures |

---

## Instructions

### Step 1: Load All Inputs

```python
import os, json, re
from datetime import datetime

start_time = datetime.now()

# Load requirements
requirements = read_json(f"{folder}/shared/requirements-normalized.json")
all_reqs = requirements.get("requirements", [])

# Load risks
risks = read_json(f"{folder}/shared/REQUIREMENT_RISKS.json")
all_risks = risks.get("risks", [])

# Load domain context
domain_context = read_json(f"{folder}/shared/domain-context.json")
domain = domain_context.get("selected_domain", "Generic")
compliance_frameworks = domain_context.get("compliance_frameworks", [])
domain_standards = domain_context.get("standards", [])

# Load all spec documents
spec_files = {
    "ARCHITECTURE": read_file(f"{folder}/outputs/ARCHITECTURE.md"),
    "SECURITY_REQUIREMENTS": read_file(f"{folder}/outputs/SECURITY_REQUIREMENTS.md"),
    "INTEROPERABILITY": read_file(f"{folder}/outputs/INTEROPERABILITY.md"),
    "UI_SPECS": read_file(f"{folder}/outputs/UI_SPECS.md"),
    "ENTITY_DEFINITIONS": read_file(f"{folder}/outputs/ENTITY_DEFINITIONS.md"),
}

# Codified 2026-05-21 — MARS SVA-3 incident: SVA3-SPEC-REQ-COVERAGE additionally
# scans REQUIREMENTS_CATALOG.md and REQUIREMENT_RISKS.md because canonical_ids
# are guaranteed to appear there by construction (catalog has every req; risks
# cite linked_requirement_ids). Without this expansion, 61 short-procedural
# requirements (~5% of CRITICAL/HIGH set) were marked uncovered despite their
# canonical_ids being present in the bid corpus. Other rules (INTERNAL-CONSISTENCY,
# ENTITY-COMPLETENESS, DOMAIN-ALIGNMENT) iterate over spec_files only — adding
# the catalog/risks to those rules would dilute their cross-spec conflict signal.
coverage_scan_files = dict(spec_files)
try:
    coverage_scan_files["REQUIREMENTS_CATALOG"] = read_file(f"{folder}/outputs/REQUIREMENTS_CATALOG.md")
except (OSError, FileNotFoundError):
    pass  # catalog optional at this verifier — skip if absent
try:
    coverage_scan_files["REQUIREMENT_RISKS"] = read_file(f"{folder}/outputs/REQUIREMENT_RISKS.md")
except (OSError, FileNotFoundError):
    pass

# Combine all spec text for cross-spec searches
all_spec_text = "\n".join(spec_files.values()).lower()

findings = []
```

### Step 1b: Rule SVA3-SPEC-MIN-FILE-SIZE (CRITICAL — added 2026-05-18 HUNT-C-0008 fix)

Detect stub / partially-written / empty spec files BEFORE any content-based rule runs.
A spec that crashed mid-write or wrote 0 bytes will silently pass keyword-coverage checks
(because there's nothing to fail against), masking the underlying generation failure.

```python
# Minimum byte sizes per spec file — derived from quality checklists in each phase file.
# Numbers chosen below the phase file's stated minimums (15KB / 8KB / 5KB) so a slightly
# under-target spec still passes this gate but a stub / empty file fails loudly.
MIN_SPEC_BYTES = {
    "ARCHITECTURE": 5000,         # phase3a target 15KB; gate at 5KB catches stubs
    "INTEROPERABILITY": 2000,     # phase3b target 5KB
    "SECURITY_REQUIREMENTS": 3000,  # phase3c target 8KB
    "UI_SPECS": 2000,             # phase3e target 5KB
    "ENTITY_DEFINITIONS": 2000,   # phase3f target 5KB
}

def check_spec_min_file_size(spec_files):
    undersize = []
    for spec_name, spec_content in spec_files.items():
        size_bytes = len(spec_content.encode("utf-8")) if spec_content else 0
        threshold = MIN_SPEC_BYTES.get(spec_name, 1000)
        if size_bytes < threshold:
            undersize.append({
                "spec": spec_name,
                "size_bytes": size_bytes,
                "min_threshold": threshold,
                "likely_cause": ("file not generated" if size_bytes == 0
                                else "stub / partial write / mid-write crash")
            })
    passed = len(undersize) == 0
    # Map spec_name → phase that produces it (for accurate corrective_action — HUNT-C-0013 partial fix)
    spec_to_phase = {
        "ARCHITECTURE": "3a", "INTEROPERABILITY": "3b",
        "SECURITY_REQUIREMENTS": "3c", "UI_SPECS": "3e",
        "ENTITY_DEFINITIONS": "3f"
    }
    failing_phases = sorted({spec_to_phase.get(u["spec"], "3a") for u in undersize})
    return {
        "rule_id": "SVA3-SPEC-MIN-FILE-SIZE",
        "severity": "CRITICAL",
        "passed": passed,
        "score": 100 if passed else max(0, 100 - len(undersize) * 25),
        "threshold": None,
        "details": {
            "undersize_specs": undersize,
            "all_specs_checked": list(spec_files.keys())
        },
        "corrective_action": {
            "type": "retry_phases", "target_phases": failing_phases,
            "auto_correctable": True,
            "instruction": f"Re-run phase(s) {failing_phases} — each produces a spec that is under minimum size, suggesting the phase crashed mid-write or didn't produce content."
        } if not passed else None
    }

findings.append(check_spec_min_file_size(spec_files))
```

### Step 2: Rule SVA3-SPEC-REQ-COVERAGE (CRITICAL)

Every CRITICAL and HIGH priority requirement must be addressed by at least one specification document.

```python
def check_spec_req_coverage(all_reqs, spec_files):
    """
    For each CRITICAL/HIGH requirement, search all spec documents for
    references to its canonical_id or semantically similar text (keywords).
    """
    critical_high_reqs = [
        r for r in all_reqs
        if r.get("priority") in ("CRITICAL", "HIGH")
    ]
    total = len(critical_high_reqs)
    covered = 0
    uncovered_reqs = []

    for req in critical_high_reqs:
        req_id = req.get("canonical_id", "")
        req_text = req.get("text", "").lower()
        # Codified 2026-05-21 — MARS SVA-3 incident: widen keyword extraction.
        # At 1,116-requirement scale the previous 2-word-bigram cap-5 missed
        # 136 of 507 reqs even though spec coverage was rich. Two-tier strategy:
        # (1) bigrams (existing) — covers most reqs
        # (2) high-signal proper-noun unigrams — covers reqs whose distinctive
        #     identity is a single domain token (Tyler, Splunk, Entra, etc.)
        bigram_keywords = extract_keywords(req_text, min_words=2, max_phrases=8)
        # Domain proper-noun tokens that act as strong single-token signals
        DOMAIN_TOKENS = {
            "tyler", "splunk", "entra", "ccp", "azure", "sql server",
            "wcag", "soc 2", "soc2", "nist", "cis", "oauth", "saml", "oidc",
            "mfa", "fips", "hipaa", "ocipa", "ors 279", "fedramp",
            "sfma", "muni", "audits division", "secretary of state",
        }
        unigram_keywords = {t for t in DOMAIN_TOKENS if t in req_text}
        keywords = list(bigram_keywords) + list(unigram_keywords)

        found_in_any_spec = False
        # Codified 2026-05-21 — iterate over coverage_scan_files (spec_files +
        # REQUIREMENTS_CATALOG.md + REQUIREMENT_RISKS.md) so short-procedural
        # requirements whose canonical_id lives in the catalog are properly
        # counted as covered.
        for spec_name, spec_content in coverage_scan_files.items():
            spec_lower = spec_content.lower()
            # Check for explicit ID reference (also try category_code prefix)
            if req_id.lower() in spec_lower:
                found_in_any_spec = True
                break
            # Check for keyword match (at least 2 keywords must appear, OR
            # at least 1 high-signal domain token from unigram_keywords).
            keyword_hits = sum(1 for kw in keywords if kw in spec_lower)
            domain_hits = sum(1 for kw in unigram_keywords if kw in spec_lower)
            if keyword_hits >= min(2, len(keywords)) or domain_hits >= 1:
                found_in_any_spec = True
                break

        if found_in_any_spec:
            covered += 1
        else:
            uncovered_reqs.append({
                "req_id": req_id,
                "priority": req.get("priority"),
                "text_snippet": req.get("text", "")[:120]
            })

    coverage_pct = (covered / total * 100) if total > 0 else 100.0
    passed = coverage_pct >= 95.0

    return {
        "rule_id": "SVA3-SPEC-REQ-COVERAGE",
        "rule_name": "Specification Requirement Coverage",
        "category": "Traceability",
        "severity": "CRITICAL",
        "passed": passed,
        "score": round(coverage_pct, 1),
        "threshold": 95.0,
        "details": {
            "total_critical_high_reqs": total,
            "covered": covered,
            "uncovered_count": len(uncovered_reqs),
            "uncovered_reqs": uncovered_reqs[:20]  # Cap detail list
        },
        "corrective_action": None if passed else {
            "type": "retry_phase",
            # HUNT-C-0013 fix 2026-05-18: route corrective action to the most likely
            # spec-producing phase by category of uncovered requirement. Default to
            # 3a (Architecture) when no clear single phase dominates.
            "target_phase": _infer_target_phase_from_uncovered(uncovered_reqs),
            "instruction": f"Re-examine uncovered requirements and update specifications. {len(uncovered_reqs)} CRITICAL/HIGH reqs have no spec coverage.",
            "auto_correctable": True
        }
    }


def _infer_target_phase_from_uncovered(uncovered_reqs):
    """Map uncovered-requirement categories to the most likely spec phase that should fix them.
    HUNT-C-0013 fix 2026-05-18."""
    if not uncovered_reqs:
        return "3a"
    # Category → phase mapping (matches phase3a/b/c/e/f domains).
    cat_to_phase = {
        "SEC": "3c", "INT": "3b", "UI": "3e", "APP": "3f",
        "TEC": "3a", "ADM": "3a", "ENR": "3f", "BUD": "3a", "RPT": "3a", "STF": "3a"
    }
    counts = {}
    for r in uncovered_reqs:
        cat = r.get("category") or r.get("priority", "TEC")
        phase = cat_to_phase.get(cat, "3a")
        counts[phase] = counts.get(phase, 0) + 1
    # Most-affected phase wins; ties broken in favor of earliest phase
    return max(counts.items(), key=lambda kv: (kv[1], -ord(kv[0][-1])))[0]

findings.append(check_spec_req_coverage(all_reqs, spec_files))
```

### Step 3: Rule SVA3-SPEC-INTERNAL-CONSISTENCY (HIGH)

Technology choices must align across specification documents. Flag contradictions such as one spec recommending PostgreSQL while another references SQL Server, or conflicting authentication mechanisms.

**ADR scope refinement (2026-05-18):** When scanning `ARCHITECTURE.md`, only consider text within ADR `**Decision:**` blocks (and surrounding non-ADR prose). ADR `**Context:**` and `**Alternatives considered:**` sections are intentionally allowed to cite REJECTED technologies (Java 21, AWS, Node.js, etc.) to document trade-offs — those citations are not conflicts and must be stripped before cross-spec comparison. Other spec files have no ADR structure and are scanned in full.

```python
def extract_adr_decisions(arch_text):
    """Return arch_text with ADR Context + Alternatives Considered sections stripped.
    ADR pattern:
        ### ADR-NNN: Title
        **Status:** ...
        **Context:** ...        <-- STRIPPED (may cite rejected tech)
        **Decision:** ...        <-- KEPT (the actual chosen tech)
        **Alternatives considered:** ...  <-- STRIPPED (cites rejected tech intentionally)
        **Evidence:** ...
        **Consequences:** ...

    Keeps Decision/Status/Evidence/Consequences and everything outside ADR blocks.
    Strips Context and Alternatives Considered.

    Refinement 2, 2026-05-18: prevents false-positive INTERNAL-CONSISTENCY conflicts
    caused by deliberately-documented rejected alternatives in ADR prose.
    """
    import re
    lines = arch_text.split("\n")
    result = []
    in_adr = False
    skip_until_next_section = False
    # Also skip "Why X Over Y" tradeoff discussion sections — these live outside
    # ADR structure but serve the same purpose (documenting REJECTED alternatives).
    # Heuristic: H3 whose title contains "Why ... Over ..." or "Why ... vs ..." or
    # ends with "Considered Alternatives" / "Rejected Alternatives".
    skip_tradeoff_section = False
    TRADEOFF_HEADER = re.compile(
        r"^###\s+.*\b(?:why\b.+\b(?:over|vs\.?)\b|considered alternatives?|rejected alternatives?)",
        re.IGNORECASE
    )

    for line in lines:
        # Detect ADR header
        if re.match(r"^###\s+ADR-\d+", line):
            in_adr = True
            skip_until_next_section = False
            skip_tradeoff_section = False
            result.append(line)
            continue

        # Detect tradeoff-discussion H3 (outside ADR structure)
        if re.match(r"^###\s+", line) and TRADEOFF_HEADER.match(line):
            in_adr = False
            skip_until_next_section = False
            skip_tradeoff_section = True
            continue  # drop the header itself too

        # Any new H1/H2/H3 ends a tradeoff section
        if skip_tradeoff_section and re.match(r"^#{1,3}\s+", line):
            skip_tradeoff_section = False
            # fall through and process this line normally

        if skip_tradeoff_section:
            continue

        if in_adr:
            # Section detection within ADR. Accepts both:
            #   **Context:** ...                  (bare)
            #   - **Context:** ...                (bulleted, dash-prefixed)
            # because ARCHITECTURE.md may use either format (the phase file's
            # ADR template is bare, but human-edited ADRs frequently bullet them).
            m = re.match(r"^\s*-?\s*\*\*(Context|Decision|Alternatives considered|Consequences|Status|Evidence):\*\*", line, re.IGNORECASE)
            if m:
                section_name = m.group(1).lower()
                if section_name in ("context", "alternatives considered"):
                    skip_until_next_section = True
                    continue  # don't include this line
                else:
                    skip_until_next_section = False
                    result.append(line)
                    continue
            # End of ADR block — next H3 header that's not ADR, or H2/H1
            if re.match(r"^###?\s+", line) and not re.match(r"^###\s+ADR-\d+", line):
                in_adr = False
                skip_until_next_section = False
                result.append(line)
                continue
            if not skip_until_next_section:
                result.append(line)
        else:
            result.append(line)

    return "\n".join(result)


def check_internal_consistency(spec_files):
    """
    Extract technology mentions from each spec and cross-compare for conflicts.

    Refinement 2, 2026-05-18: ARCHITECTURE.md is preprocessed via
    extract_adr_decisions() to drop ADR Context + Alternatives Considered prose,
    which intentionally cites rejected technologies and would otherwise produce
    false-positive conflicts (e.g., Java 21 / AWS / Node.js in rejected-alternative
    lists getting flagged as cross-spec divergence).
    """
    # Technology categories to check for conflicts
    tech_categories = {
        "database": ["sql server", "postgresql", "mysql", "mongodb", "cosmos db", "oracle", "dynamodb", "sqlite"],
        "auth": ["oauth", "saml", "jwt", "openid connect", "azure ad", "okta", "auth0", "ldap", "kerberos"],
        "frontend": ["react", "angular", "vue", "blazor", "svelte", "jquery"],
        "backend": ["asp.net", "node.js", "django", "spring boot", "flask", "express", "fastapi"],
        "cloud": ["azure", "aws", "gcp", "on-premises", "on-prem"],
        "messaging": ["rabbitmq", "kafka", "azure service bus", "sqs", "redis pub/sub"]
    }

    conflicts = []
    tech_by_spec = {}

    # Refinement 2 follow-up, 2026-05-18: use word-boundary matching to avoid
    # substring false-positives (e.g., "JAWS" screen reader matching "aws",
    # "expressed" matching "express"). Previous naive `in` checks produced
    # spurious cross-spec conflicts every time these substrings appeared.
    import re as _re_consistency
    def _tech_matches(tech, text):
        # Escape special regex chars in tech name; require word boundaries
        return _re_consistency.search(r"\b" + _re_consistency.escape(tech) + r"\b", text) is not None

    # Codified 2026-05-21 — MARS SVA-3 incident: defensive-prose exclusion.
    # SECURITY_REQUIREMENTS.md mentions injection-defense topics like "LDAP/NoSQL/OS
    # command injection mitigated" — these are NOT declarations that LDAP is an auth
    # mechanism choice; they're defensive-prose mentions in OWASP Top 10 / NIST
    # control prose. Strip lines that match defensive-prose patterns before
    # extracting category tokens. Same idea for "PostgreSQL alternative
    # (REJECTED)" rows in ARCHITECTURE.md non-ADR tables.
    DEFENSIVE_PROSE_PATTERNS = [
        # OWASP / injection defense
        _re_consistency.compile(r"\binjection\s+mitigated\b|\bsanitiz\w*\s+input|"
                                r"\bsql\s+injection\b|\bldap\s+injection\b|"
                                r"\bnosql\s+injection\b|\bos\s+command\s+injection\b",
                                _re_consistency.IGNORECASE),
        # Rejected-alternative rows in non-ADR tables
        _re_consistency.compile(r"\bREJECTED\b|\bALTERNATIVE\b\s*\(REJECTED\)|"
                                r"\bnot\s+selected\b|\bunder\s+evaluation\b|"
                                r"\bconsidered\s+but\s+excluded\b",
                                _re_consistency.IGNORECASE),
    ]
    def _strip_defensive_lines(text):
        out = []
        for line in text.split("\n"):
            if any(p.search(line) for p in DEFENSIVE_PROSE_PATTERNS):
                continue
            out.append(line)
        return "\n".join(out)

    for spec_name, spec_content in spec_files.items():
        # Refinement 2, 2026-05-18: strip ADR Context + Alternatives Considered
        # from ARCHITECTURE.md so rejected-alternative citations don't fire as conflicts.
        if spec_name == "ARCHITECTURE":
            spec_content = extract_adr_decisions(spec_content)
        # Refinement 3, 2026-05-21: defensive-prose exclusion across ALL specs
        spec_content = _strip_defensive_lines(spec_content)
        spec_lower = spec_content.lower()
        tech_by_spec[spec_name] = {}
        for category, technologies in tech_categories.items():
            found = [t for t in technologies if _tech_matches(t, spec_lower)]
            if found:
                tech_by_spec[spec_name][category] = found

    # Codified 2026-05-21 — MARS SVA-3 incident: complementary-technology sets.
    # Some tech categories include items that legitimately COEXIST in a single
    # solution rather than compete. The federated-auth stack is the canonical
    # example: OIDC is built on OAuth; JWT is the token format OIDC issues; SAML
    # is an alternative federation protocol that often coexists with OIDC in the
    # same enterprise. Flagging "JWT + OAuth + OIDC + SAML across specs" as a
    # conflict was a false-positive 100% of the time. Define complementary sets
    # per category — items within a complementary set never conflict with each
    # other, even if multiple appear across multiple specs.
    COMPLEMENTARY_SETS = {
        "auth": [
            # Federated auth stack — these coexist
            {"oauth", "openid connect", "jwt", "saml", "azure ad"},
        ],
        "database": [
            # Polyglot persistence — primary OLTP + cache + search legitimately coexist
            {"sql server", "redis"},
            {"postgresql", "redis"},
        ],
        "cloud": [
            # No complementary sets — these are mutually exclusive cloud choices
        ],
        "messaging": [
            # Modern .NET stack legitimately mixes service bus + Redis pub/sub
            {"azure service bus", "redis pub/sub"},
        ],
    }

    def _is_complementary(category, mentioned_set):
        """Return True if every tech in mentioned_set belongs to a single
        complementary set for this category."""
        for comp_set in COMPLEMENTARY_SETS.get(category, []):
            if mentioned_set.issubset(comp_set):
                return True
        return False

    # Cross-compare: within each category, if different specs mention
    # competing technologies, flag as conflict
    for category in tech_categories:
        all_mentioned = set()
        per_spec = {}
        for spec_name, techs in tech_by_spec.items():
            spec_techs = set(techs.get(category, []))
            if spec_techs:
                per_spec[spec_name] = spec_techs
                all_mentioned.update(spec_techs)

        # If multiple competing techs in same category across different specs,
        # AND they aren't all members of a single complementary set, flag as conflict.
        if len(all_mentioned) > 1 and len(per_spec) > 1:
            if _is_complementary(category, all_mentioned):
                continue  # complementary stack — not a conflict
            conflicts.append({
                "category": category,
                "technologies_mentioned": list(all_mentioned),
                "by_spec": {k: list(v) for k, v in per_spec.items()}
            })

    score = max(0, 100 - (len(conflicts) * 20))
    passed = len(conflicts) == 0

    return {
        "rule_id": "SVA3-SPEC-INTERNAL-CONSISTENCY",
        "rule_name": "Specification Internal Consistency",
        "category": "Consistency",
        "severity": "HIGH",
        "passed": passed,
        "score": round(score, 1),
        "threshold": 80.0,
        "details": {
            "conflicts_found": len(conflicts),
            "conflicts": conflicts,
            "tech_by_spec_summary": {
                spec: list(cats.keys()) for spec, cats in tech_by_spec.items()
            }
        },
        "corrective_action": None if passed else {
            "type": "retry_phase",
            "target_phase": "3a",
            "instruction": f"Resolve {len(conflicts)} technology conflict(s) across specs: {', '.join(c['category'] for c in conflicts)}",
            "auto_correctable": True
        }
    }

findings.append(check_internal_consistency(spec_files))
```

### Step 3b: Rule SVA3-TECH-STACK-LTS-VERIFIED (CRITICAL — BLOCKING)

Every primary technology component cited in Stage 3 specifications MUST trace back to a vendor lifecycle lookup performed within the last 7 days. The lookup evidence lives in `shared/tech-lifecycle-evidence.json` (produced by Phase 3a Step 4). This rule enforces the structural gate: no specs may pass SVA-3 if the evidence file is missing, stale, or contains any component whose EOL date falls inside `contract_years + 2`.

**Why this rule exists (incident-driven, not theoretical):** Stage 3 has twice produced ARCHITECTURE.md with the wrong .NET version — once with `.NET 8 LTS` (EOL Nov 2026, falling inside a 5-yr contract) and once with `.NET 9 LTS` (a category error — .NET 9 is STS, not LTS, EOL May 2026). Both errors traced to agents relying on training-data knowledge instead of running the lookup the phase file mandated. This rule moves enforcement from "the phase file said to" to "evidence on disk or BLOCK".

```python
def check_tech_stack_lts_verified(spec_files, folder, contract_years=5):
    """
    BLOCKING: Verify shared/tech-lifecycle-evidence.json exists, is fresh,
    and every component passes contract+2yr lifecycle.
    """
    evidence_path = f"{folder}/shared/tech-lifecycle-evidence.json"
    findings_detail = {
        "evidence_file_exists": False,
        "evidence_age_days": None,
        "components_checked": 0,
        "components_failed_lifecycle": [],
        # Refinement 1, 2026-05-18: components that fall short of contract+2yr EOL
        # BUT have an explicit, ADR-documented migration plan are recorded here
        # for audit trail (visible exception), not silently failed.
        "exception_with_migration_count": 0,
        "exception_with_migration": [],
        "components_missing_source_url": [],
        "components_missing_fetched_at": [],
        "specs_mentioning_unverified_versions": [],
    }

    # Gate 1: file must exist
    if not os.path.exists(evidence_path):
        return {
            "rule_id": "SVA3-TECH-STACK-LTS-VERIFIED",
            "rule_name": "Tech Stack LTS Lifecycle Verified",
            "category": "Tech Stack Governance",
            "severity": "CRITICAL",
            "passed": False,
            "score": 0.0,
            "threshold": 100.0,
            "details": findings_detail,
            "corrective_action": {
                "type": "retry_phase",
                "target_phase": "3a",
                "instruction": "Re-run Phase 3a Step 4 with real WebFetch/WebSearch lookups. shared/tech-lifecycle-evidence.json is missing — no version may be cited without it.",
                "auto_correctable": True
            }
        }

    evidence = read_json(evidence_path)
    generated_at = datetime.fromisoformat(evidence.get("generated_at", "1970-01-01T00:00:00"))
    age_days = (datetime.now() - generated_at).days
    findings_detail["evidence_file_exists"] = True
    findings_detail["evidence_age_days"] = age_days
    findings_detail["components_checked"] = len(evidence.get("components", []))

    # Gate 2: freshness — evidence older than 7 days is considered stale
    # because LTS schedules update with new releases / out-of-band patches.
    if age_days > 7:
        return {
            "rule_id": "SVA3-TECH-STACK-LTS-VERIFIED",
            "rule_name": "Tech Stack LTS Lifecycle Verified",
            "category": "Tech Stack Governance",
            "severity": "CRITICAL",
            "passed": False,
            "score": 30.0,
            "threshold": 100.0,
            "details": findings_detail,
            "corrective_action": {
                "type": "retry_phase",
                "target_phase": "3a",
                "instruction": f"tech-lifecycle-evidence.json is {age_days} days old. Re-run Phase 3a Step 4 to refresh.",
                "auto_correctable": True
            }
        }

    # Gate 3: per-component validity
    # Refinement 1, 2026-05-18: split short-EOL components into two buckets —
    # those with an ADR-documented migration plan (exception, audit-only) vs
    # those without (true failure). Exceptions remain visible but do not BLOCK.
    for c in evidence.get("components", []):
        if not c.get("source_url"):
            findings_detail["components_missing_source_url"].append(c.get("component"))
        if not c.get("fetched_at"):
            findings_detail["components_missing_fetched_at"].append(c.get("component"))
        if not c.get("passes_contract_lifecycle", False):
            if c.get("migration_plan_present") and c.get("migration_plan_adr"):
                # Exception path — record but do not fail
                findings_detail["exception_with_migration"].append({
                    "component": c.get("component"),
                    "version": c.get("recommended_version"),
                    "eol_date": c.get("eol_date"),
                    "migration_plan_adr": c.get("migration_plan_adr"),
                    "migration_plan_summary": c.get("migration_plan_summary"),
                })
            else:
                findings_detail["components_failed_lifecycle"].append({
                    "component": c.get("component"),
                    "version": c.get("recommended_version"),
                    "classification": c.get("classification"),
                    "eol_date": c.get("eol_date"),
                    "min_required_eol": c.get("min_required_eol"),
                })
    findings_detail["exception_with_migration_count"] = len(findings_detail["exception_with_migration"])

    # Gate 4: cross-check — every version string the ARCHITECTURE.md prose
    # actually cites must appear in the evidence file. Catches the case
    # where the agent ran the lookup but then ignored the result and wrote
    # a different version into the prose.
    #
    # Refinement 2, 2026-05-18: scan ONLY ADR Decision blocks (plus non-ADR prose),
    # NOT ADR Context / Alternatives Considered. The latter intentionally cite
    # rejected versions (e.g., ".NET 8 rejected because EOL Nov 2026") and would
    # otherwise produce false-positive unverified-version flags.
    import re
    version_pattern = re.compile(r"\.NET\s+(\d+)|Node(?:\.js)?\s+(\d+)|React\s+(\d+)|PostgreSQL\s+(\d+)|SQL Server\s+(\d{4})|Java\s+(\d+)|Python\s+(\d+\.\d+)", re.IGNORECASE)
    arch_text = spec_files.get("ARCHITECTURE", "")
    arch_text_for_versions = extract_adr_decisions(arch_text)  # Strip Alternatives + Context prose

    # Refinement 2 follow-up, 2026-05-18: even after ADR/tradeoff filtering,
    # ADR titles and lifecycle-rationale callouts may contain NEGATIVE citations
    # like "ADR-005 — .NET 10 LTS (not .NET 8, not .NET 9 STS)" or
    # "Lifecycle gate: .NET 8 LTS reaches EOL Nov 2026". These are
    # JUSTIFICATIONS for the chosen version, not version recommendations, and
    # must not fire unverified-version flags. Skip any line containing a
    # negative-citation marker BEFORE scanning for versions.
    NEGATIVE_CITATION = re.compile(
        r"\b(not\s+\.NET|neither\s+\.NET|rejected|reaches\s+EOL|reaches\s+end\s+of\s+life|"
        r"\bis\s+STS\b|is\s+not\s+LTS|forbids|forbidden|disqualifying|preview|post-GA|"
        r"end\s+of\s+life\s+within|unsupported|force\s+(?:an?\s+)?unsupported|"
        r"selecting\s+\.NET\s+\d+\s+would|proposing\s+\.NET\s+\d+\s+would|"
        # Codified 2026-05-21 — MARS SVA-3 incident: additional NEGATIVE phrasings
        r"selected\s+to\s+AVOID|to\s+AVOID\s+\.NET|"
        r"frequently\s+propose|bidders\s+(?:frequently\s+)?propose|"
        r"verified\s+not\s+\.NET|verified\s+not\s+\d+|"
        r"explicitly\s+(?:EXCLUDED|REJECTED|DISQUALIFIED)|"
        r"\bAVOID\b\s+\.NET\s+\d+|\bAVOID\b\s+version|"
        r"version\s+selected:\s+[^()]+\s+\(verified\s+not|"
        # Rolling-LTS migration plan future-version citations are explanatory, not
        # recommendation: e.g., ".NET 12 LTS (~Nov 2027)", "Node 26 LTS (~2028)"
        r"~Nov\s+\d{4}|~\d{4}\b|migration[- ]plan|rolling\s+LTS|"
        r"target\s+version\s+for\s+\d{4}|"
        r"EOL\s+(?:May|November|October|June|July|August|"
        r"September|January|February|March|April|December)\s+\d{4})",
        re.IGNORECASE
    )
    cited_versions = set()
    for line in arch_text_for_versions.split("\n"):
        # Skip ADR title lines — they often contain "(not .NET 8, not .NET 9)" rationale
        if re.match(r"^###\s+ADR-\d+", line):
            continue
        if NEGATIVE_CITATION.search(line):
            continue
        for match in version_pattern.finditer(line):
            for group in match.groups():
                if group:
                    cited_versions.add(group)
    evidence_versions = set()
    for c in evidence.get("components", []):
        v = str(c.get("recommended_version", ""))
        # Codified 2026-05-21 — MARS SVA-3 incident:
        # `recommended_version` field can carry the component name as a prefix
        # ("Kubernetes 1.32 (AKS LTS)") or a parenthesized lifecycle suffix
        # ("10 (LTS)", "v8.1 (IG2)"). Use re.findall to extract ALL numeric
        # version tokens regardless of position, then add each to the
        # endorsed set. This catches:
        #   - "10" from ".NET 10" / ".NET 10 (LTS)"
        #   - "1.32" from "Kubernetes 1.32 (AKS LTS)"
        #   - "19.2" from "React 19.2.0" / "19.2"
        #   - "8.1" from "CIS Controls v8.1"
        for token in re.findall(r"(\d+(?:\.\d+)*)", v):
            evidence_versions.add(token)
            # Major-only form for prose that cites major version
            evidence_versions.add(token.split(".")[0])
        # Refinement 1 follow-up, 2026-05-18: when a component has a documented
        # migration plan, any versions cited in migration_plan_summary are also
        # "known good" — the bid intentionally references future LTS versions.
        # Without this, Gate 4 would falsely flag the migration plan's own prose.
        summary = c.get("migration_plan_summary") or ""
        for vm in re.finditer(r"\b(\d+(?:\.\d+)?)\b", summary):
            evidence_versions.add(vm.group(1))
    unverified = cited_versions - evidence_versions
    if unverified:
        findings_detail["specs_mentioning_unverified_versions"] = sorted(unverified)

    # Disposition
    failed_lifecycle = len(findings_detail["components_failed_lifecycle"]) > 0
    missing_evidence = (
        len(findings_detail["components_missing_source_url"]) > 0
        or len(findings_detail["components_missing_fetched_at"]) > 0
    )
    cited_but_unverified = len(findings_detail["specs_mentioning_unverified_versions"]) > 0

    passed = not (failed_lifecycle or missing_evidence or cited_but_unverified)
    score = 100.0 if passed else (
        0.0 if failed_lifecycle else
        50.0 if missing_evidence else
        60.0  # cited_but_unverified only
    )

    return {
        "rule_id": "SVA3-TECH-STACK-LTS-VERIFIED",
        "rule_name": "Tech Stack LTS Lifecycle Verified",
        "category": "Tech Stack Governance",
        "severity": "CRITICAL",
        "passed": passed,
        "score": round(score, 1),
        "threshold": 100.0,
        "details": findings_detail,
        "corrective_action": None if passed else {
            "type": "retry_phase",
            "target_phase": "3a",
            "instruction": (
                "Failed lifecycle: " + ", ".join(c["component"] for c in findings_detail["components_failed_lifecycle"])
                if failed_lifecycle else
                "Architecture cites version(s) not in evidence file: " + ", ".join(findings_detail["specs_mentioning_unverified_versions"])
                if cited_but_unverified else
                "Evidence entries missing source_url or fetched_at — re-run lookup."
            ),
            "auto_correctable": True
        }
    }

findings.append(check_tech_stack_lts_verified(spec_files, folder, contract_years=5))
```

### Step 3c: Rule SVA3-TECH-STACK-VERSION-CONSISTENCY (CRITICAL)

Cross-check every technology version cited in bid markdown files against `shared/tech-lifecycle-evidence.json`. The existing `SVA3-TECH-STACK-LTS-VERIFIED` gate (Step 3b) validates structural lifecycle evidence and ARCHITECTURE.md. This rule extends coverage to the final deliverable markdown — `outputs/*.md` and `outputs/bid-sections/*.md` — where stale version strings (.NET 8, .NET 9) contaminate evaluator-visible PDFs even after the evidence file and ARCHITECTURE.md are corrected.

**Why separate from SVA3-TECH-STACK-LTS-VERIFIED:** Phase 8 (bid authoring) re-reads specs and past-project data, providing a second path for stale versions to enter the deliverable. SVA3-TECH-STACK-LTS-VERIFIED only checks spec files (ARCHITECTURE.md, SECURITY_REQUIREMENTS.md, etc.); it does NOT scan bid-sections/*.md. This rule closes that gap.

**Negative-citation exemption:** Lines that reference a version in a *competitive warning* or *rejected-alternative* context are skipped. The same `NEGATIVE_CITATION` pattern from Step 3b applies here, extended with common bid-prose rejection markers ("bidders proposing", "avoid", "risk of proposing", "competitor").

```python
def check_tech_stack_version_consistency(folder, evidence_path=None):
    """
    CRITICAL: Scan outputs/*.md and outputs/bid-sections/*.md for technology version
    mentions. Each cited version must appear in shared/tech-lifecycle-evidence.json
    recommended_version fields. Versions found only in negative-citation / competitive-
    warning context are exempted (they document why we rejected them).

    Counterfactual: would have caught .NET 8 LTS / .NET 9 LTS contamination in
    03_TECHNICAL.md and 04_RISK_REGISTER.md during the 2026-05-18 rfp-mars run,
    where SVAs reported PASS 94.2/100 while these strings were live in PDFs.
    """
    import re as _re
    import glob as _glob

    if evidence_path is None:
        evidence_path = f"{folder}/shared/tech-lifecycle-evidence.json"

    # Gate: evidence file must exist (Step 3b already blocks if missing, but be safe)
    if not os.path.exists(evidence_path):
        return {
            "rule_id": "SVA3-TECH-STACK-VERSION-CONSISTENCY",
            "rule_name": "Tech Stack Version Consistency (Bid Markdown)",
            "category": "Tech Stack Governance",
            "severity": "CRITICAL",
            "passed": False,
            "score": 0.0,
            "threshold": 100.0,
            "details": {"error": "tech-lifecycle-evidence.json missing; cannot verify bid markdown versions"},
            "corrective_action": {
                "type": "retry_phase", "target_phase": "3a", "auto_correctable": True,
                "instruction": "Re-run Phase 3a Step 4 to generate tech-lifecycle-evidence.json before scanning bid markdown."
            }
        }

    evidence = read_json(evidence_path)
    # Build set of all evidence-endorsed version strings (major + full)
    evidence_versions = set()
    for c in evidence.get("components", []):
        v = str(c.get("recommended_version", ""))
        _m = _re.match(r"(\d+(?:\.\d+)?)", v)
        if _m:
            evidence_versions.add(_m.group(1))
            evidence_versions.add(_m.group(1).split(".")[0])
        # Also accept versions cited in migration plan summaries (planned future upgrades)
        for _vm in _re.finditer(r"\b(\d+(?:\.\d+)?)\b", c.get("migration_plan_summary") or ""):
            evidence_versions.add(_vm.group(1))
            evidence_versions.add(_vm.group(1).split(".")[0])

    # Regex: extract version numbers from common technology patterns
    VERSION_PATTERN = _re.compile(
        r"\.NET\s+(\d+)|"
        r"Node(?:\.js)?\s+(\d+)|"
        r"React\s+(\d+(?:\.\d+)?)|"
        r"PostgreSQL\s+(\d+)|"
        r"SQL\s+Server\s+(\d{4})|"
        r"Java\s+(\d+)|"
        r"Python\s+(\d+\.\d+)|"
        r"Kubernetes\s+(\d+\.\d+)",
        _re.IGNORECASE
    )

    # Negative-citation filter: lines containing these patterns are NOT recommendations
    # They document competitor risk, rejected alternatives, or compliance warnings.
    # Rule: if ANY of these tokens appears on the line, skip ALL version matches on that line.
    # Patterns cover:
    #   - "NOT an LTS release", "is STS", "are STS", "Standard Term Support"
    #   - "rejected", "forbids", "forbidden", "disqualifying", "unsupported"
    #   - "EOL is approximately", "EOL <Month> <Year>"
    #   - "bidders proposing", "Selecting .NET N would"
    #   - Lifecycle education prose: "odd-numbered ... are STS", "are LTS"
    NEGATIVE_CITATION = _re.compile(
        r"\b(not\s+\.NET|neither\s+\.NET|rejected|reaches\s+EOL|reaches\s+end\s+of\s+life|"
        r"\bis\s+STS\b|\bare\s+STS\b|is\s+not\s+LTS|NOT\s+(?:an?\s+)?LTS|"
        r"Standard\s+Term\s+Support|STS\s+\(|\bSTS\b|forbids|forbidden|disqualifying|preview|post-GA|"
        r"end\s+of\s+life\s+within|unsupported|bidders\s+propos|propos(?:ing|ed)\s+\.NET\s+\d+\s+would|"
        r"Selecting\s+\.NET\s+\d+\s+would|avoid\s+\.NET|risk\s+of\s+propos|competitor|"
        r"EOL\s+is\s+approximately|"
        # Codified 2026-05-21 — MARS SVA-3 incident: additional NEGATIVE phrasings
        r"selected\s+to\s+AVOID|to\s+AVOID\s+\.NET|frequently\s+propose|"
        r"verified\s+not\s+\.NET|verified\s+not\s+\d+|"
        r"explicitly\s+(?:EXCLUDED|REJECTED|DISQUALIFIED)|"
        r"\bAVOID\b\s+\.NET\s+\d+|"
        r"version\s+selected:\s+[^()]+\s+\(verified\s+not|"
        # Rolling-LTS migration plan future-version mentions are NOT current
        # recommendations — they're roadmap/ladder citations.
        r"~Nov\s+\d{4}|~\d{4}\b|migration[- ]plan|rolling\s+LTS|"
        r"target\s+version\s+for\s+\d{4}|"
        r"EOL\s+(?:May|November|October|June|July|August|"
        r"September|January|February|March|April|December)\s+\d{4})",
        _re.IGNORECASE
    )

    # Scan bid markdown files — outputs/*.md and outputs/bid-sections/*.md
    scan_patterns = [
        f"{folder}/outputs/bid-sections/*.md",
        f"{folder}/outputs/*.md",
    ]
    cited_unverified = []  # {file, line_no, version, match_text}
    files_scanned = 0

    for pattern in scan_patterns:
        for fpath in _glob.glob(pattern):
            files_scanned += 1
            try:
                with open(fpath, "r", encoding="utf-8") as _fh:
                    lines = _fh.readlines()
            except (OSError, UnicodeDecodeError):
                continue
            fname = os.path.basename(fpath)
            for line_no, line in enumerate(lines, 1):
                # Skip negative-citation lines (competitor warnings, rejected-alt rationale)
                if NEGATIVE_CITATION.search(line):
                    continue
                for _m in VERSION_PATTERN.finditer(line):
                    # Extract whichever capture group matched
                    version = next((g for g in _m.groups() if g), None)
                    if not version:
                        continue
                    # Check against evidence-endorsed versions
                    major = version.split(".")[0]
                    if version not in evidence_versions and major not in evidence_versions:
                        cited_unverified.append({
                            "file": fname,
                            "line_no": line_no,
                            "version": version,
                            "match_text": _m.group(0).strip(),
                            "line_preview": line.strip()[:120]
                        })

    passed = len(cited_unverified) == 0
    score = 100.0 if passed else max(0.0, 100.0 - len(cited_unverified) * 5)

    return {
        "rule_id": "SVA3-TECH-STACK-VERSION-CONSISTENCY",
        "rule_name": "Tech Stack Version Consistency (Bid Markdown)",
        "category": "Tech Stack Governance",
        "severity": "CRITICAL",
        "passed": passed,
        "score": round(score, 1),
        "threshold": 100.0,
        "details": {
            "files_scanned": files_scanned,
            "evidence_endorsed_versions": sorted(evidence_versions),
            "cited_unverified_count": len(cited_unverified),
            "cited_unverified": cited_unverified[:20]
        },
        "corrective_action": None if passed else {
            "type": "retry_phase",
            "target_phase": "8",
            "auto_correctable": True,
            "instruction": (
                f"{len(cited_unverified)} bid markdown lines cite technology versions not in "
                f"tech-lifecycle-evidence.json. Endorsed versions: {sorted(evidence_versions)}. "
                "Re-run Phase 8 bid authoring with updated tech context, or manually correct stale version strings."
            )
        }
    }

findings.append(check_tech_stack_version_consistency(folder))
```

### Step 4: Rule SVA3-SPEC-DOMAIN-ALIGNMENT (HIGH)

Specifications should reference domain-appropriate standards and compliance frameworks identified during intake.

```python
def check_domain_alignment(spec_files, domain_context):
    """
    Verify specs mention the compliance frameworks and domain standards
    detected in domain-context.json.
    """
    frameworks = domain_context.get("compliance_frameworks", [])
    standards = domain_context.get("standards", [])
    domain = domain_context.get("selected_domain", "")

    # Build list of expected domain terms
    expected_terms = []
    for fw in frameworks:
        name = fw if isinstance(fw, str) else fw.get("name", "")
        if name:
            expected_terms.append(name.lower())
    for std in standards:
        name = std if isinstance(std, str) else std.get("name", "")
        if name:
            expected_terms.append(name.lower())

    if not expected_terms:
        # No domain terms to check -- pass by default
        return {
            "rule_id": "SVA3-SPEC-DOMAIN-ALIGNMENT",
            "rule_name": "Domain Alignment",
            "category": "Content",
            "severity": "HIGH",
            "passed": True,
            "score": 100.0,
            "threshold": 70.0,
            "details": {"note": "No domain standards defined in domain-context.json; rule trivially passes."},
            "corrective_action": None
        }

    found_terms = []
    missing_terms = []
    all_spec_lower = "\n".join(spec_files.values()).lower()

    for term in expected_terms:
        if term in all_spec_lower:
            found_terms.append(term)
        else:
            missing_terms.append(term)

    coverage_pct = (len(found_terms) / len(expected_terms) * 100) if expected_terms else 100
    passed = coverage_pct >= 70.0

    return {
        "rule_id": "SVA3-SPEC-DOMAIN-ALIGNMENT",
        "rule_name": "Domain Alignment",
        "category": "Content",
        "severity": "HIGH",
        "passed": passed,
        "score": round(coverage_pct, 1),
        "threshold": 70.0,
        "details": {
            "domain": domain,
            "expected_terms": expected_terms,
            "found": found_terms,
            "missing": missing_terms
        },
        "corrective_action": None if passed else {
            "type": "supplement_phase",
            "target_phase": "3c",
            "instruction": f"Add references to missing domain standards: {', '.join(missing_terms)}",
            "auto_correctable": True
        }
    }

findings.append(check_domain_alignment(spec_files, domain_context))
```

### Step 5: Rule SVA3-RISK-SPEC-COVERAGE (HIGH)

Every HIGH-severity risk mitigation must be reflected in at least one spec document.

```python
def check_risk_spec_coverage(all_risks, spec_files):
    """
    For each HIGH/CRITICAL risk, verify its mitigation strategy keywords
    appear in at least one specification document.
    """
    high_risks = [
        r for r in all_risks
        if r.get("severity") in ("HIGH", "CRITICAL")
    ]
    total = len(high_risks)
    covered = 0
    uncovered_risks = []

    all_spec_lower = "\n".join(spec_files.values()).lower()

    # Codified 2026-05-21 — MARS SVA-3 incident: high-signal domain tokens
    # for title-first weighting. The prior mitigation-only bigram extractor
    # missed Tyler/Splunk/Entra/etc. because the first 6 bigrams from
    # mitigation text were boilerplate ("proof concept", "concept poc",
    # "kickoff any"); domain-relevant tokens appeared later and were dropped
    # by max_phrases=4. Reality: Tyler was referenced 101x in specs,
    # Splunk 126x — coverage was rich, rule mis-measured.
    DOMAIN_TOKENS = {
        "tyler", "splunk", "entra", "ccp", "azure", "sql server",
        "wcag", "soc 2", "soc2", "nist", "cis", "oauth", "saml", "oidc",
        "mfa", "fips", "hipaa", "ocipa", "ors 279", "fedramp",
        "sfma", "muni", "gasb", "rto", "rpo", "backup", "encryption",
    }

    for risk in high_risks:
        risk_id = risk.get("risk_id", risk.get("id", ""))
        title = (risk.get("title") or "").lower()
        mitigation = risk.get("mitigation_strategy", risk.get("mitigation", ""))
        mitigation_lower = mitigation.lower() if mitigation else ""

        # Title-first keyword extraction (codified 2026-05-21):
        # 1. Bigrams from TITLE (higher signal than mitigation boilerplate)
        # 2. Bigrams from mitigation as supplemental
        # 3. Domain proper-noun unigrams across title + mitigation
        title_keywords = extract_keywords(title, min_words=2, max_phrases=6)
        mitigation_keywords = extract_keywords(mitigation_lower, min_words=2, max_phrases=4)
        combined_text = title + " " + mitigation_lower
        domain_keywords = [t for t in DOMAIN_TOKENS if t in combined_text]
        keywords = list(title_keywords) + list(mitigation_keywords) + domain_keywords

        # Check if risk ID or mitigation keywords appear in specs
        found = False
        if risk_id.lower() in all_spec_lower:
            found = True
        elif keywords:
            keyword_hits = sum(1 for kw in keywords if kw in all_spec_lower)
            # A single domain-proper-noun hit is sufficient (high signal);
            # otherwise require ≥2 keyword hits as before.
            domain_hits = sum(1 for kw in domain_keywords if kw in all_spec_lower)
            if domain_hits >= 1 or keyword_hits >= min(2, len(keywords)):
                found = True

        if found:
            covered += 1
        else:
            uncovered_risks.append({
                "risk_id": risk_id,
                "severity": risk.get("severity"),
                "description": risk.get("description", "")[:100],
                "mitigation_snippet": mitigation[:100] if mitigation else "N/A"
            })

    coverage_pct = (covered / total * 100) if total > 0 else 100.0
    passed = coverage_pct >= 80.0

    return {
        "rule_id": "SVA3-RISK-SPEC-COVERAGE",
        "rule_name": "Risk-to-Specification Coverage",
        "category": "Traceability",
        "severity": "HIGH",
        "passed": passed,
        "score": round(coverage_pct, 1),
        "threshold": 80.0,
        "details": {
            "total_high_critical_risks": total,
            "covered": covered,
            "uncovered_count": len(uncovered_risks),
            "uncovered_risks": uncovered_risks[:15]
        },
        "corrective_action": None if passed else {
            "type": "retry_phase",
            "target_phase": "3g",
            "instruction": f"{len(uncovered_risks)} HIGH/CRITICAL risk mitigations not reflected in specs. Review and update.",
            "auto_correctable": True
        }
    }

findings.append(check_risk_spec_coverage(all_risks, spec_files))
```

### Step 6: Rule SVA3-ENTITY-COMPLETENESS (MEDIUM)

Data entities referenced in workflow-derived requirements must have definitions in ENTITY_DEFINITIONS.md.

```python
def check_entity_completeness(all_reqs, entity_spec):
    """
    Extract entity-like nouns from workflow requirements and verify
    they appear in ENTITY_DEFINITIONS.md.
    """
    # Gather entity candidates from requirements mentioning data/entities
    entity_keywords = [
        "entity", "record", "table", "object", "model",
        "form", "document", "profile", "account", "report"
    ]
    # Codified 2026-05-21 — MARS SVA-3 incident: noun-extraction discipline.
    # The previous regex caught any capitalized phrase, producing 435 candidates
    # of which 230 were false positives like "Account Administrative", "Adobe",
    # "Agreed Upon Procedures" — fragments, vendor names, procedural phrases.
    # Tightened logic:
    #   1. Reject single-word entries (need ≥2 words to be entity-like)
    #   2. Reject RFP-meta / procurement noise (procurement terms, common form labels)
    #   3. Reject vendor product names that aren't system entities
    #   4. Reject fragments that look like UI labels rather than persistent entities
    NON_ENTITY_BLACKLIST = {
        "agreed upon procedures", "request for proposal", "request for proposals",
        "scope of work", "statement of work", "service level agreement",
        "proposal information", "reference check", "responsibility inquiry",
        "disclosure exemption", "cost proposal", "cover letter",
        "secretary of state", "department of administration",  # gov bodies, not entities
        "general fund", "enterprise fund", "fiduciary fund",  # accounting buckets, not system entities
        "adobe", "microsoft", "tyler", "splunk", "azure", "oracle",  # vendors
    }
    entity_candidates = set()
    for req in all_reqs:
        text = req.get("text", "")
        text_lower = text.lower()
        if any(kw in text_lower for kw in entity_keywords):
            # Extract capitalized multi-word phrases as entity candidates
            phrases = re.findall(r'[A-Z][a-z]+(?:\s+[A-Z][a-z]+)*', text)
            for phrase in phrases:
                phrase_lower = phrase.lower()
                # ≥2 words required (single capitalized words like "Adobe" out)
                if len(phrase.split()) < 2:
                    continue
                # Blacklist filter
                if phrase_lower in NON_ENTITY_BLACKLIST:
                    continue
                # Skip any candidate that starts with a blacklisted prefix
                if any(phrase_lower.startswith(b + " ") for b in NON_ENTITY_BLACKLIST):
                    continue
                if len(phrase) > 3 and phrase_lower not in ("the", "this", "that", "with"):
                    entity_candidates.add(phrase)

    if not entity_candidates:
        return {
            "rule_id": "SVA3-ENTITY-COMPLETENESS",
            "rule_name": "Entity Definition Completeness",
            "category": "Completeness",
            "severity": "MEDIUM",
            "passed": True,
            "score": 100.0,
            "threshold": 75.0,
            "details": {"note": "No entity candidates extracted from requirements."},
            "corrective_action": None
        }

    entity_lower = entity_spec.lower()
    found = []
    missing = []
    for entity in entity_candidates:
        if entity.lower() in entity_lower:
            found.append(entity)
        else:
            missing.append(entity)

    coverage_pct = (len(found) / len(entity_candidates) * 100)
    passed = coverage_pct >= 75.0

    return {
        "rule_id": "SVA3-ENTITY-COMPLETENESS",
        "rule_name": "Entity Definition Completeness",
        "category": "Completeness",
        "severity": "MEDIUM",
        "passed": passed,
        "score": round(coverage_pct, 1),
        "threshold": 75.0,
        "details": {
            "entity_candidates_count": len(entity_candidates),
            "found_in_spec": len(found),
            "missing_from_spec": missing[:20],
            "sample_found": found[:10]
        },
        "corrective_action": None if passed else {
            "type": "supplement_phase",
            "target_phase": "3f",
            "instruction": f"Add definitions for {len(missing)} missing entities: {', '.join(missing[:10])}",
            "auto_correctable": True
        }
    }

findings.append(check_entity_completeness(all_reqs, spec_files["ENTITY_DEFINITIONS"]))
```

### Step 7: Rule SVA3-SPEC-DEPTH-RATIO (MEDIUM)

Each spec must have proportional depth relative to the number of requirements it addresses. Minimum 200 bytes per addressed requirement.

```python
def check_spec_depth_ratio(all_reqs, spec_files):
    """
    For each spec, estimate how many requirements it addresses,
    then compute bytes-per-requirement ratio.
    """
    results_per_spec = []
    total_ratio_sum = 0
    specs_below_threshold = []

    for spec_name, spec_content in spec_files.items():
        spec_lower = spec_content.lower()
        spec_size_bytes = len(spec_content.encode("utf-8"))

        # Count requirements addressed by this spec
        addressed_count = 0
        for req in all_reqs:
            req_id = req.get("canonical_id", "")
            if req_id.lower() in spec_lower:
                addressed_count += 1
                continue
            # Keyword fallback: check 2+ keywords
            keywords = extract_keywords(req.get("text", "").lower(), min_words=2, max_phrases=3)
            hits = sum(1 for kw in keywords if kw in spec_lower)
            if hits >= min(2, len(keywords)):
                addressed_count += 1

        if addressed_count == 0:
            addressed_count = 1  # Avoid division by zero

        ratio = spec_size_bytes / addressed_count
        results_per_spec.append({
            "spec": spec_name,
            "size_bytes": spec_size_bytes,
            "reqs_addressed": addressed_count,
            "bytes_per_req": round(ratio, 1)
        })
        total_ratio_sum += ratio

        if ratio < 200:
            specs_below_threshold.append(spec_name)

    avg_ratio = total_ratio_sum / len(spec_files) if spec_files else 0
    passed = len(specs_below_threshold) == 0
    score = max(0, 100 - (len(specs_below_threshold) * 25))

    return {
        "rule_id": "SVA3-SPEC-DEPTH-RATIO",
        "rule_name": "Specification Depth Ratio",
        "category": "Content",
        "severity": "MEDIUM",
        "passed": passed,
        "score": round(score, 1),
        "threshold": 200,
        "details": {
            "min_bytes_per_req": 200,
            "average_bytes_per_req": round(avg_ratio, 1),
            "per_spec": results_per_spec,
            "specs_below_threshold": specs_below_threshold
        },
        "corrective_action": None if passed else {
            "type": "retry_phase",
            "target_phase": "3a",
            "instruction": f"Specs below depth threshold: {', '.join(specs_below_threshold)}. Expand with more detail.",
            "auto_correctable": True
        }
    }

findings.append(check_spec_depth_ratio(all_reqs, spec_files))
```

### Step 8: Compute Overall Disposition and Score

```python
def compute_disposition(findings):
    """
    BLOCK  = any CRITICAL failure
    ADVISORY = any HIGH failure (no CRITICAL failures)
    PASS   = all pass, or only MEDIUM/LOW failures
    """
    has_critical_failure = any(
        f["severity"] == "CRITICAL" and not f["passed"] for f in findings
    )
    has_high_failure = any(
        f["severity"] == "HIGH" and not f["passed"] for f in findings
    )

    if has_critical_failure:
        return "BLOCK"
    elif has_high_failure:
        return "ADVISORY"
    else:
        return "PASS"

def compute_overall_score(findings):
    """
    Weighted average: CRITICAL=30, HIGH=25, MEDIUM=15, LOW=10
    """
    severity_weights = {"CRITICAL": 30, "HIGH": 25, "MEDIUM": 15, "LOW": 10}
    total_weight = 0
    weighted_score = 0

    for f in findings:
        w = severity_weights.get(f["severity"], 10)
        total_weight += w
        weighted_score += w * (f.get("score", 100.0 if f["passed"] else 0.0))

    return round(weighted_score / total_weight, 1) if total_weight > 0 else 0

disposition = compute_disposition(findings)
overall_score = compute_overall_score(findings)
passed_count = sum(1 for f in findings if f["passed"])
failed_count = sum(1 for f in findings if not f["passed"])
critical_failures = sum(1 for f in findings if f["severity"] == "CRITICAL" and not f["passed"])
high_failures = sum(1 for f in findings if f["severity"] == "HIGH" and not f["passed"])
```

### Step 9: Build Corrective Actions Summary

```python
corrective_actions = []
for f in findings:
    if not f["passed"] and f.get("corrective_action"):
        corrective_actions.append({
            "priority": f["severity"],
            "action": f["corrective_action"].get("instruction", "Review and fix"),
            "target_phase": f["corrective_action"].get("target_phase"),
            "auto_correctable": f["corrective_action"].get("auto_correctable", False),
            "rule_id": f["rule_id"]
        })

# Sort: CRITICAL first, then HIGH, MEDIUM, LOW
priority_order = {"CRITICAL": 0, "HIGH": 1, "MEDIUM": 2, "LOW": 3}
corrective_actions.sort(key=lambda x: priority_order.get(x["priority"], 4))
```

### Step 10: Write SVA Report

```python
duration_ms = int((datetime.now() - start_time).total_seconds() * 1000)

sva_report = {
    "validator": "SVA-3",
    "stage_validated": 3,
    "validated_at": datetime.now().isoformat(),
    "disposition": disposition,
    "color_team": None,
    "summary": {
        "total_rules": len(findings),
        "passed": passed_count,
        "failed": failed_count,
        "critical_failures": critical_failures,
        "high_failures": high_failures,
        "overall_score": overall_score
    },
    "findings": findings,
    "color_team_report": None,
    "corrective_actions_summary": corrective_actions,
    "execution_metadata": {
        "duration_ms": duration_ms,
        "files_analyzed": len(spec_files) + 3,  # specs + reqs + risks + domain
        "data_sources_read": [
            "requirements-normalized.json",
            "REQUIREMENT_RISKS.json",
            "domain-context.json",
            "ARCHITECTURE.md",
            "SECURITY_REQUIREMENTS.md",
            "INTEROPERABILITY.md",
            "UI_SPECS.md",
            "ENTITY_DEFINITIONS.md"
        ]
    }
}

# Ensure validation directory exists
os.makedirs(f"{folder}/shared/validation", exist_ok=True)

write_json(f"{folder}/shared/validation/sva3-spec.json", sva_report)
```

### Step 11: Report Results

```python
log(f"""
{'='*60}
SVA-3: Specification Validator -- {disposition}
{'='*60}
Overall Score: {overall_score}/100
Rules: {len(findings)} total | {passed_count} passed | {failed_count} failed
Critical Failures: {critical_failures} | High Failures: {high_failures}

Findings:
""")

for f in findings:
    status = "PASS" if f["passed"] else "FAIL"
    icon = "  " if f["passed"] else "  "
    log(f"{icon} [{f['severity']}] {f['rule_id']}: {f['rule_name']} -- {status} (score: {f.get('score', 'N/A')})")

if corrective_actions:
    log(f"\nCorrective Actions ({len(corrective_actions)}):")
    for ca in corrective_actions:
        log(f"  [{ca['priority']}] {ca['rule_id']} -> Phase {ca['target_phase']}: {ca['action'][:100]}")

log(f"\nDisposition: {disposition}")
log(f"Report: {folder}/shared/validation/sva3-spec.json")
```

## Utility: Keyword Extraction

Used by multiple rules to extract meaningful keyword phrases from requirement text.

```python
def extract_keywords(text, min_words=2, max_phrases=5):
    """
    Extract short keyword phrases from text for fuzzy matching.
    Removes stopwords and returns the most distinctive phrases.
    """
    stopwords = {
        "the", "a", "an", "is", "are", "was", "were", "be", "been",
        "being", "have", "has", "had", "do", "does", "did", "will",
        "shall", "should", "may", "might", "must", "can", "could",
        "would", "of", "in", "to", "for", "with", "on", "at", "by",
        "from", "as", "into", "through", "during", "before", "after",
        "and", "but", "or", "not", "no", "all", "each", "every",
        "both", "few", "more", "most", "other", "some", "such", "than",
        "that", "this", "these", "those", "it", "its"
    }

    words = re.findall(r'[a-z]+', text.lower())
    meaningful = [w for w in words if w not in stopwords and len(w) > 2]

    # Build bigrams from meaningful words
    phrases = []
    for i in range(len(meaningful) - 1):
        phrases.append(f"{meaningful[i]} {meaningful[i+1]}")

    # Deduplicate and take top N
    seen = set()
    unique = []
    for p in phrases:
        if p not in seen:
            seen.add(p)
            unique.append(p)

    return unique[:max_phrases]
```

## Quality Checklist

- [ ] `sva3-spec.json` created in `shared/validation/`
- [ ] All 7 rules executed with scores (includes SVA3-TECH-STACK-VERSION-CONSISTENCY added 2026-05-19)
- [ ] Disposition correctly computed (PASS / ADVISORY / BLOCK)
- [ ] Corrective actions populated for failed rules
- [ ] Report conforms to sva-report.schema.json
- [ ] No CRITICAL failures left unaddressed in corrective_actions
- [ ] SVA3-TECH-STACK-VERSION-CONSISTENCY checked bid-sections/*.md AND outputs/*.md for stale version strings
