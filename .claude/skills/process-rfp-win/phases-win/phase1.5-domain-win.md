---
name: phase1.5-domain-win
expert-role: Business Analyst
domain-expertise: Industry classification, compliance frameworks
skill: procurement-analyst
---

# Phase 1.5: Domain Detection

## Purpose

Automatically detect the RFP domain from document content to load the appropriate domain profile.

## Inputs

- `{folder}/flattened/*.md` - Flattened document content
- Domain profiles in `${CLAUDE_SKILL_DIR}/../process-rfp/domain-profiles/` (sibling skill — process-rfp generic — confirm presence before relying)

## Required Outputs

- `{folder}/shared/domain-context.json` - Detected domain with confidence scores

## Domain Profiles Available

| Domain | Profile | Key Signals |
|--------|---------|-------------|
| E-Commerce | `ecommerce` | product, cart, checkout, SKU, PCI-DSS |
| K-12 Education | `education` | student, enrollment, FERPA, CEDARS |
| Finance | `finance` | account, transaction, loan, BSA/AML |
| Healthcare | `healthcare` | patient, encounter, HIPAA, HL7/FHIR |
| Federal Government | `government` | federal agency, FISMA, FedRAMP, FAR, SAM.gov |
| State/Local Government | `state_local_government` | secretary of state, treasurer, comptroller, GASB, GFOA, CAFR/ACFR, municipal audit, county clerk, board of supervisors, township, ORS / Cal. Gov / Tex. Gov / etc. state codes |
| Generic | `default` | (fallback when no domain matches) |

---

## ⛔ HARD RULES (READ FIRST — non-negotiable, codified 2026-05-20 after MARS jurisdiction-hallucination incident)

### Rule J1: NEVER fabricate jurisdiction.

Jurisdiction (state, agency, issuing authority, statute citations) MUST be extracted from RFP text only — never inferred, guessed, or completed from training-data knowledge of plausible-sounding state codes.

**Incident driving this rule (2026-05-20, MARS RFP run):** Phase 1.5 agent wrote `"buyer": "State of Alaska, Department of Administration, Division of Finance"` and `"Title 36/37 (AK)"` for an RFP that is unambiguously **State of Oregon, Secretary of State, Audits Division** (issuing authority on RFP cover page line 24 "Salem, OR 97310"; statute cite line 156 "Oregon Revised Statutes 297.405"; portal line 9 "OregonBuys"). The agent picked a different state's department name and statute numbers that don't exist in the RFP. Phase 1.6 (Evaluation) and 1.7 (Compliance) correctly read "Oregon" because they were forced to cite RFP sections — Phase 1.5 had no such constraint, so it free-styled.

**Codification:** the jurisdiction-anchor step below (Step 2.5) is MANDATORY. Every field in `jurisdiction_anchor` MUST be backed by a `flattened/{file}:{line}` citation that can be grep-verified against the actual RFP text. Fields that cannot be grep-verified MUST be `null`, never invented.

### Rule J2: Domain scoring is independent of jurisdiction.

The domain score (ecommerce/healthcare/finance/government/state_local_government) classifies the *kind of system* being procured. The jurisdiction anchor records *who is buying it*. Both must be populated independently — a low-confidence domain score does NOT permit jurisdiction guessing as a "workaround."

### Rule J3: `mars_domain_hints` / `*_domain_hints` are forbidden.

Past runs added ad-hoc `mars_domain_hints` blocks to compensate for low confidence. These mix inferred industry tagging with hallucinated jurisdiction and are a known footgun. Do NOT add per-RFP hint blocks. Improve the DOMAIN_SIGNALS table and the jurisdiction_anchor step instead, so the same RFP family is handled correctly on the next run without bespoke fixes.

---

## Instructions

### Step 1: Load Flattened Documents

```python
import glob

flattened_files = glob.glob(f"{folder}/flattened/*.md")
combined_content = ""

for file_path in flattened_files:
    combined_content += read_file(file_path) + "\n\n"

# Convert to lowercase for matching
content_lower = combined_content.lower()
```

### Step 2: Define Detection Signals

```python
DOMAIN_SIGNALS = {
    "state_local_government": {
        # Added 2026-05-20 (codification of MARS run failure: muni accounting / ERP RFPs
        # were defaulting because "government" only covered federal signals. This new
        # entry catches state, county, municipal, special-district procurements
        # — the dominant class of RFPs Resource Data, Inc. bids on.)
        "high_confidence": {
            "primary_entities": [
                "secretary of state", "state treasurer", "state comptroller",
                "state auditor", "municipal", "municipality", "county clerk",
                "county auditor", "board of supervisors", "board of commissioners",
                "city council", "township", "special district", "school district",
                "fire district", "water district", "transit district",
                "local government", "audits division", "audits bureau"
            ],
            "compliance_terms": [
                "gasb", "gfoa", "cafr", "acfr", "single audit", "uniform guidance",
                "2 cfr 200", "yellow book", "gagas",
                "ors ", "ors§", "ors ", "oar ", "cal. gov", "tex. gov",
                "fla. stat", "n.y. state", "ohio rev", "ill. comp",
                "municipal audit law", "public records law",
                "open meetings act", "sunshine law",
                "ocipa"  # Oregon Consumer Information Protection Act (state privacy)
            ],
            "external_systems": [
                "oregonbuys", "calpia", "txsmart", "munis", "tyler tech",
                "tyler technologies", "opengov", "bs&a", "centralsquare",
                "workday government", "infor public sector"
            ],
            "file_formats": [
                "1099-misc", "1099-nec", "w-2", "form 990",  # IRS forms gov entities issue
                "single audit data collection form"
            ]
        },
        "medium_confidence": {
            "terminology": [
                "appropriation", "fund accounting", "encumbrance", "warrant",
                "budgetary fund balance", "intergovernmental", "grant award",
                "millage", "ad valorem", "property tax", "general fund",
                "enterprise fund", "internal service fund", "fiduciary fund",
                "governmental accounting"
            ],
            "organizational": [
                "city manager", "city clerk", "city attorney", "town manager",
                "county administrator", "village clerk", "borough", "parish"
            ]
        },
        "anti_signals": [
            # Things that DEFINITIVELY are not state/local gov
            "department of defense", "dod", "dfars", "armed forces",
            "patient", "diagnosis", "prescription", "hl7", "fhir",  # healthcare
            "student enrollment", "transcript", "graduation",          # K-12
            "merchant", "shopping cart", "checkout flow"               # ecommerce
        ]
    },
    "education": {
        "high_confidence": {
            "primary_entities": ["student", "enrollment", "district", "school", "grade level", "attendance"],
            "compliance_terms": ["ferpa", "ceisdars", "cedars", "ospi", "eds"],
            "external_systems": ["skyward", "powerschool", "infinite campus", "aspen"],
            "file_formats": ["f-195f", "f-196", "p-223", "s-275"]
        },
        "medium_confidence": {
            "terminology": ["semester", "academic year", "curriculum", "graduation", "transcript"],
            "organizational": ["superintendent", "principal", "teacher", "classroom"]
        },
        "anti_signals": ["patient", "diagnosis", "prescription", "healthcare"]
    },
    "healthcare": {
        "high_confidence": {
            "primary_entities": ["patient", "encounter", "diagnosis", "prescription", "provider"],
            "compliance_terms": ["hipaa", "hitech", "meaningful use", "phi"],
            "external_systems": ["epic", "cerner", "allscripts", "meditech"],
            "file_formats": ["hl7", "fhir", "ccda", "ccd"]
        },
        "medium_confidence": {
            "terminology": ["clinical", "medical record", "treatment", "discharge"],
            "organizational": ["physician", "nurse", "hospital", "clinic"]
        },
        "anti_signals": ["student", "enrollment", "curriculum"]
    },
    "finance": {
        "high_confidence": {
            "primary_entities": ["account", "transaction", "loan", "customer", "balance"],
            "compliance_terms": ["bsa", "aml", "kyc", "pci-dss", "sox"],
            "external_systems": ["swift", "ach", "fedwire", "core banking"]
        },
        "medium_confidence": {
            "terminology": ["interest rate", "credit", "debit", "ledger", "reconciliation"],
            "organizational": ["teller", "branch", "underwriter"]
        }
    },
    "government": {
        "high_confidence": {
            "primary_entities": ["citizen", "case", "permit", "license", "applicant"],
            "compliance_terms": ["fisma", "fedramp", "fips", "nist 800"],
            "external_systems": ["grants.gov", "sam.gov", "usajobs"]
        },
        "medium_confidence": {
            "terminology": ["agency", "jurisdiction", "regulation", "ordinance"],
            "organizational": ["commissioner", "director", "clerk"]
        }
    },
    "ecommerce": {
        "high_confidence": {
            "primary_entities": ["product", "cart", "order", "customer", "sku"],
            "compliance_terms": ["pci-dss", "gdpr", "ccpa"],
            "external_systems": ["shopify", "magento", "woocommerce", "stripe"]
        },
        "medium_confidence": {
            "terminology": ["checkout", "inventory", "shipping", "return", "catalog"],
            "organizational": ["merchant", "vendor", "warehouse"]
        }
    }
}

SIGNAL_WEIGHTS = {
    "primary_entities": 15,
    "compliance_terms": 12,
    "external_systems": 10,
    "file_formats": 10,
    "terminology": 5,
    "organizational": 3
}
```

### Step 2.5: Extract Jurisdiction Anchor (MANDATORY — codified 2026-05-20)

This step is INDEPENDENT of domain scoring. Even when domain confidence is 0.0, the jurisdiction anchor MUST be populated from RFP text alone — never from training-data inference.

Every field below MUST be backed by a `flattened/{file}:{line}` citation that can be grep-verified. If a field cannot be evidenced from the RFP, set it to `null` and record the reason. **NEVER write plausible-sounding state agency names or statute numbers without a citation. NEVER substitute a different state's equivalent department.**

```python
import re

# Step 2.5a — locate the primary RFP file (largest non-attachment .md in flattened/)
primary_rfp_path = None
primary_rfp_size = 0
for f_path in flattened_files:
    fname = os.path.basename(f_path).lower()
    # Skip attachments / forms / supporting files — find the main RFP
    if any(token in fname for token in ["attachment", "rfpatt", "exhibit", "addendum",
                                          "pricing", "_combined"]):
        continue
    size = os.path.getsize(f_path)
    if size > primary_rfp_size:
        primary_rfp_path = f_path
        primary_rfp_size = size

# Step 2.5b — grep-anchored extraction (no LLM inference)
# Each pattern is matched against the primary RFP text + the combined view if present.
# We capture the FIRST citation only, with file:line.

def grep_cite(pattern, content_by_file, flags=re.IGNORECASE | re.MULTILINE):
    """Return (matched_text, file, line) for the first match across the supplied files.
    content_by_file is a dict: {filename: full_text_string}."""
    rx = re.compile(pattern, flags)
    for fname, text in content_by_file.items():
        m = rx.search(text)
        if m:
            line_no = text[:m.start()].count("\n") + 1
            return (m.group(0), fname, line_no)
    return (None, None, None)

# Build the file → content map (re-use already-loaded combined_content where possible)
content_by_file = {}
if primary_rfp_path:
    content_by_file[os.path.basename(primary_rfp_path)] = read_file(primary_rfp_path)
# Also include the _combined.md if present (it concatenates everything with separators)
combined_path = f"{folder}/flattened/_combined.md"
if os.path.exists(combined_path) and os.path.basename(combined_path) not in content_by_file:
    content_by_file[os.path.basename(combined_path)] = read_file(combined_path)

# State of X — first occurrence
state_match, state_file, state_line = grep_cite(
    r"\bState of (Oregon|Washington|California|Texas|New York|Florida|Alaska|Arizona|"
    r"Colorado|Idaho|Montana|Nevada|Utah|Wyoming|Hawaii|Massachusetts|Connecticut|"
    r"Maine|Rhode Island|Vermont|New Hampshire|New Jersey|Pennsylvania|Ohio|Michigan|"
    r"Indiana|Illinois|Wisconsin|Minnesota|Iowa|Missouri|Kansas|Nebraska|"
    r"North Dakota|South Dakota|Oklahoma|Arkansas|Louisiana|Mississippi|Alabama|"
    r"Georgia|South Carolina|North Carolina|Tennessee|Kentucky|West Virginia|"
    r"Virginia|Maryland|Delaware|New Mexico)\b",
    content_by_file
)

# ZIP/city — capture mailing address line if present (e.g. "Salem, OR 97310")
addr_match, addr_file, addr_line = grep_cite(
    r"[A-Z][a-zA-Z .'-]+,\s*([A-Z]{2})\s+\d{5}(?:-\d{4})?",
    content_by_file
)

# Procurement portal — strong jurisdiction signal
portal_patterns = {
    "OregonBuys":  r"OregonBuys",
    "Cal eProcure": r"Cal\s*eProcure",
    "ESM Solutions": r"ESM\s+Solutions",
    "BidNet": r"BidNet",
    "PlanetBids": r"PlanetBids",
    "Periscope S2G": r"Periscope\s*S2G",
    "Public Purchase": r"Public\s+Purchase",
    "SAM.gov": r"\bSAM\.gov\b",
    "Grants.gov": r"\bGrants\.gov\b",
    "BidExpress": r"BidExpress",
    "Bonfire": r"\bBonfire(?:\s+Procurement)?\b",
}
portal_match, portal_file, portal_line = (None, None, None)
portal_name = None
for name, pat in portal_patterns.items():
    m, fn, ln = grep_cite(pat, content_by_file)
    if m:
        portal_match, portal_file, portal_line, portal_name = m, fn, ln, name
        break

# Statute citation — capture the FIRST statute reference we find
statute_match, statute_file, statute_line = grep_cite(
    r"\b(?:ORS|OAR|RCW|WAC|Cal\.\s*Gov|Tex\.\s*Gov|Fla\.\s*Stat|"
    r"N\.Y\.\s*[A-Z]+|Ohio\s*Rev|Ill\.\s*Comp|AS\s+\d+|"
    r"[A-Z]{2,4}\s*Title\s+\d+|\d+\s+CFR\s+\d+|\d+\s+U\.S\.C\.\s+\d+)\s*[§\.\d\-]+",
    content_by_file
)

# Issuing agency line — look for "Issued by", "Procuring Agency", "Agency:" etc.
# Expanded 2026-05-20 after MARS run captured weak "on behalf of an entity or firm"
# phrase (Attachment A boilerplate at _combined:289) instead of the strong cover-page
# phrase "Is issuing this Request for Proposals" at RFP:7. The new patterns prefer
# active-voice "{Agency} is issuing" constructions on the cover page.
agency_patterns = [
    # Active-voice cover-page form: "The State of X is issuing this Request for Proposals"
    r"(?:The\s+)?(?:State\s+of\s+[A-Z][a-zA-Z]+(?:,\s*(?:acting\s+by\s+and\s+through\s+)?[^\n,]{5,150})?)"
    r"\s+(?:is\s+)?issuing\s+(?:this|the)\s+(?:Request\s+for\s+Proposal[s]?|RFP|RFQ|Solicitation)",
    # "Issued by:" header
    r"Issued\s+by\s*[:\-]\s*([^\n]{5,200})",
    # "Issuing Agency" / "Procuring Agency" header
    r"(?:Issuing|Procuring)\s+Agency\s*[:\-]\s*([^\n]{5,200})",
    # "Agency:" header
    r"^\s*Agency\s*[:\-]\s*([^\n]{5,200})",
    # "X is the {Agency}" form (e.g., "The Secretary of State (Agency)")
    r"(?:The\s+)?[A-Z][^\n]{5,150}\s+\((?:Agency|Department)\)",
    # Last-resort: "on behalf of"
    r"on\s+behalf\s+of\s+(?:the\s+)?([^\n]{5,200})",
]
agency_match, agency_file, agency_line = (None, None, None)
for pat in agency_patterns:
    m, fn, ln = grep_cite(pat, content_by_file)
    if m:
        agency_match, agency_file, agency_line = m, fn, ln
        break

# Solicitation number — usually on cover page
sol_match, sol_file, sol_line = grep_cite(
    r"(?:RFP|Solicitation|Bid|RFQ|RFI)\s*(?:Number|No\.?|#)?\s*[:\-]?\s*"
    r"([A-Z0-9][-A-Z0-9\.]{4,30})",
    content_by_file
)

jurisdiction_anchor = {
    "extraction_method": "grep_cite (no inference)",
    "primary_rfp_file": os.path.basename(primary_rfp_path) if primary_rfp_path else None,
    "state": {
        # Case-insensitive normalize — the grep regex is IGNORECASE, so the match
        # can be "STATE OF OREGON" / "State of Oregon" / "state of oregon" depending
        # on the source. Strip the prefix case-insensitively, then title-case the
        # remainder so downstream consumers get a canonical "Oregon" / "Washington"
        # / etc. (codified 2026-05-20 after MARS retry surfaced "STATE OF OREGON"
        # uppercase leaking through; case-sensitive .replace() was a phase-file bug.)
        "value": (
            re.sub(r"^state of\s+", "", state_match, flags=re.IGNORECASE).strip().title()
            if state_match else None
        ),
        "citation": f"{state_file}:{state_line}" if state_file else None,
        "evidence_text": state_match
    },
    "address_token": {
        "value": addr_match,
        "citation": f"{addr_file}:{addr_line}" if addr_file else None
    },
    "procurement_portal": {
        "value": portal_name,
        "citation": f"{portal_file}:{portal_line}" if portal_file else None,
        "evidence_text": portal_match
    },
    "statute_citation": {
        "value": statute_match,
        "citation": f"{statute_file}:{statute_line}" if statute_file else None
    },
    "issuing_agency_line": {
        "value": agency_match,
        "citation": f"{agency_file}:{agency_line}" if agency_file else None
    },
    "solicitation_number": {
        "value": sol_match,
        "citation": f"{sol_file}:{sol_line}" if sol_file else None
    }
}

# Final consistency self-check — if state is set, refuse to keep contradictory tokens.
# (Example: state="Oregon" but statute_citation contains "AS Title 36" → drop the
#  contradictory statute rather than write it.)
if jurisdiction_anchor["state"]["value"]:
    state_val = jurisdiction_anchor["state"]["value"]
    state_to_code_prefix = {
        "Oregon": ("ORS", "OAR"),
        "Washington": ("RCW", "WAC"),
        "California": ("Cal.", "Cal "),
        "Texas": ("Tex.",),
        "Alaska": ("AS",),
        "New York": ("N.Y.",),
        "Florida": ("Fla.",),
        # Extend as encountered — keep deterministic, not exhaustive.
    }
    allowed = state_to_code_prefix.get(state_val, ())
    if jurisdiction_anchor["statute_citation"]["value"] and allowed:
        if not any(jurisdiction_anchor["statute_citation"]["value"].startswith(p)
                   for p in allowed):
            # Statute prefix doesn't match the state — drop it as contradictory.
            jurisdiction_anchor["statute_citation"] = {
                "value": None,
                "citation": None,
                "dropped_reason": f"Found statute did not match state {state_val} "
                                  f"(expected prefix in {allowed})"
            }
```

If `state.value` is `None` AND `procurement_portal.value` is `None` AND `address_token.value` is `None`, log a HIGH-severity warning — this is suspicious for a real RFP and should be flagged for SVA-1 review.

### Step 3: Score Each Domain

```python
def count_signals(content, signals):
    """Count occurrences of signal terms in content."""
    count = 0
    matched = []
    for term in signals:
        if term.lower() in content:
            count += 1
            matched.append(term)
    return count, matched

def calculate_domain_score(content, domain_config):
    """Calculate confidence score for a domain."""
    score = 0
    matches = {}

    for category, signals in domain_config.get("high_confidence", {}).items():
        count, matched = count_signals(content, signals)
        weight = SIGNAL_WEIGHTS.get(category, 5)
        score += count * weight
        if matched:
            matches[category] = matched

    for category, signals in domain_config.get("medium_confidence", {}).items():
        count, matched = count_signals(content, signals)
        weight = SIGNAL_WEIGHTS.get(category, 3)
        score += count * weight
        if matched:
            matches[category] = matched

    # Apply anti-signal penalty
    for anti_signal in domain_config.get("anti_signals", []):
        if anti_signal.lower() in content:
            score -= 20

    return score, matches

# Score all domains
domain_scores = {}
for domain, config in DOMAIN_SIGNALS.items():
    score, matches = calculate_domain_score(content_lower, config)
    domain_scores[domain] = {"score": score, "matches": matches}
```

### Step 4: Determine Winner

```python
# Sort by score
ranked = sorted(domain_scores.items(), key=lambda x: x[1]["score"], reverse=True)
top_domain, top_data = ranked[0]
second_domain, second_data = ranked[1] if len(ranked) > 1 else (None, {"score": 0})

# Calculate confidence
total_score = sum(d["score"] for d in domain_scores.values())
confidence = top_data["score"] / total_score if total_score > 0 else 0

# Determine if clear winner
margin = top_data["score"] - second_data["score"]
clear_winner = margin >= 20 or confidence >= 0.85
```

### Step 5: Load Domain Profile

```python
import os
try:
    _file_fallback = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
except NameError:
    # HUNT-B-012 fix 2026-05-18: `__file__` is undefined when Python is executed
    # inline (e.g., `python -c "..."` or LLM-evaluated without saving to a file).
    # CLAUDE_SKILL_DIR env var is the reliable source.
    _file_fallback = None
SKILL_DIR = os.environ.get("CLAUDE_SKILL_DIR") or _file_fallback
if not SKILL_DIR:
    raise RuntimeError("CLAUDE_SKILL_DIR env var not set and __file__ unavailable — cannot resolve skill directory")
# Try the sibling skill location first; fall back to per-skill config-win/ where the
# profiles should live (recommended relocation — Grok consensus 2026-05-18). Either
# path becomes None if neither exists; downstream code must handle profile = {}.
_candidates = [
    os.path.abspath(f"{SKILL_DIR}/../process-rfp/domain-profiles"),
    os.path.abspath(f"{SKILL_DIR}/config-win/domain-profiles"),
]
PROFILES_DIR = next((p for p in _candidates if os.path.isdir(p)), None)
if PROFILES_DIR is None:
    log(f"  ⚠️  Domain profiles not found at any candidate location; falling back to empty profile")

# Selection gate (revised 2026-05-20 after MARS run).
# Old gate `confidence >= 0.5` was too strict: with 6 domains splitting the score
# pool, the share-of-total formula rarely reaches 50% even when one domain is the
# overwhelming winner (MARS: state_local_government=230 vs next=117, margin=113,
# but confidence=230/602=0.38 → fell back to "default" despite a 113-pt margin).
#
# New gate accepts EITHER a strong clear_winner (margin ≥ 30 AND top score ≥ 60)
# OR a high-confidence share (confidence ≥ 0.5). The margin path is the dominant
# signal — a domain that beats the runner-up by ≥30 points with non-trivial absolute
# score IS the right pick regardless of share-of-total math.
strong_margin = (margin >= 30 and top_data["score"] >= 60)
if (clear_winner and confidence >= 0.5) or strong_margin:
    selected_domain = top_domain
else:
    selected_domain = "default"

# Guard against missing profile directory (bug-hunt 2026-05-18 HUNT-B-006 fix).
# Domain detection is optional — every downstream phase falls back to generic
# behavior when profile == {}. Crashing here would block the entire pipeline.
if PROFILES_DIR is not None:
    profile_path = f"{PROFILES_DIR}/{selected_domain}.yaml"
    try:
        profile = read_yaml(profile_path)
    except FileNotFoundError:
        log(f"  ⚠️  Profile file not found: {profile_path}; using empty profile")
        profile = {}
else:
    profile = {}
```

### Step 6: Write Domain Context

```python
domain_context = {
    "detected_at": datetime.now().isoformat(),
    "selected_domain": selected_domain,
    "confidence": round(confidence, 2),
    "clear_winner": clear_winner,
    "margin": margin,
    "scores": {
        domain: {
            "score": data["score"],
            "top_matches": data["matches"]
        }
        for domain, data in ranked[:3]
    },
    "profile": profile,
    "custom_terminology": extract_custom_terms(combined_content, profile),
    # MANDATORY — jurisdiction_anchor is the canonical "who is buying this" answer
    # that all downstream Stage 1+ phases inherit. SVA1-JURISDICTION-CONSISTENCY
    # checks that 1.6/1.7/1.8 outputs agree with this anchor. Every field traces
    # back to a flattened/{file}:{line} citation — see Step 2.5.
    "jurisdiction_anchor": jurisdiction_anchor
}

# Enhancement: Auto-include Section 508 for government domain (FAR 39.2 mandate)
if selected_domain == "government":
    compliance_frameworks = domain_context.get("profile", {}).get("compliance_frameworks", [])
    if "Section 508" not in compliance_frameworks:
        compliance_frameworks.append("Section 508")
    domain_context["section_508_note"] = "Auto-included per FAR Subpart 39.2 — all federal ICT procurements require Section 508 accessibility compliance"

# Enhancement: Auto-flag CMMC for DoD/defense domain (Phase 1 live Nov 2025)
# WORD-BOUNDARY MATCHING REQUIRED — substring matching caused a false positive on
# 2026-05-20 MARS run where "defense of claims" indemnity language in Attachment A
# triggered cmmc_flag for a State of Oregon municipal-ERP RFP.
# Also require a corroborating DoD-context term (DFARS / Department of Defense /
# armed forces / CMMC) so legal-defense phrasing alone is never sufficient.
import re as _re_cmmc
defense_word_patterns = [
    r"\bDoD\b", r"\bdefense\b", r"\bmilitary\b", r"\barmy\b", r"\bnavy\b",
    r"\bair force\b", r"\bmarines\b", r"\bspace force\b"
]
defense_corroborators = [
    r"\bdepartment of defense\b", r"\bDFARS\b", r"\barmed forces\b",
    r"\bCMMC\b", r"\bDoD contract\b", r"\bdefense contractor\b"
]
has_defense_word = any(_re_cmmc.search(p, combined_content, _re_cmmc.IGNORECASE)
                       for p in defense_word_patterns)
has_corroborator = any(_re_cmmc.search(p, combined_content, _re_cmmc.IGNORECASE)
                       for p in defense_corroborators)
if has_defense_word and has_corroborator:
    domain_context["cmmc_flag"] = {
        "detected": True,
        "note": "DoD/defense domain detected — CMMC Level 1/2 likely required (Phase 1 live Nov 2025)",
        "level_guidance": "Level 1 (self-assessment) for FCI, Level 2 (third-party assessment) for CUI"
    }

# Enhancement: FedRAMP 20x awareness
fedramp_signals = ["fedramp", "fed-ramp", "federal risk and authorization"]
if any(signal in content_lower for signal in fedramp_signals):
    domain_context["fedramp_20x_note"] = (
        "FedRAMP 20x modernization (March 2025+): authorization timelines changed from 18+ months to ~3 months; "
        "Key Security Indicators (KSIs) replace static checklists; continuous monitoring emphasis increased"
    )

write_json(f"{folder}/shared/domain-context.json", domain_context)
```

### Step 7: Report Results

```
🎯 Domain Detection Complete
=============================
Selected Domain: {selected_domain}
Confidence: {confidence:.0%}
Clear Winner: {clear_winner}

Score Breakdown:
  1. {top_domain}: {top_data["score"]} points
     Key matches: {top_data["matches"]}
  2. {second_domain}: {second_data["score"]} points

Profile loaded: {profile_path}
```

## Custom Terminology Extraction

```python
def extract_custom_terms(content, profile):
    """Extract domain-specific terminology not in standard profile."""
    known_terms = set()
    for category in profile.get("terminology", {}).values():
        known_terms.update(t.lower() for t in category)

    # Find capitalized terms (potential entities/acronyms)
    potential_terms = re.findall(r'\b[A-Z][A-Za-z]{2,}\b', content)
    custom = [t for t in set(potential_terms) if t.lower() not in known_terms]

    # Find acronyms
    acronyms = re.findall(r'\b[A-Z]{2,6}\b', content)
    custom_acronyms = [a for a in set(acronyms) if a not in profile.get("terminology", {}).get("acronyms", [])]

    return {
        "entities": custom[:20],
        "acronyms": custom_acronyms[:15]
    }
```

## Quality Checklist (MANDATORY — report each by name with evidence)

The phase agent MUST verify each of the following BEFORE reporting completion. The agent's completion report MUST include a checklist-results block with:
- Item name (verbatim from below)
- PASS / FAIL / SKIPPED-WITH-REASON
- Evidence (file:line citation, grep result, file size, assertion that ran, etc.)

"All checks passed" without per-item evidence is NOT acceptable.

### Required output files
1. **domain-context.json** exists at `{folder}/shared/domain-context.json` — evidence: `ls -la` size > 100 bytes and parses as valid JSON via `python -c "import json; json.load(open(...))"` succeeds

### Schema fidelity
2. **domain-context.json top-level keys** include `detected_at`, `selected_domain`, `confidence`, `clear_winner`, `scores`, `profile`, `custom_terminology`, `jurisdiction_anchor` — evidence: list actual top-level keys found
3. **confidence** is a float between 0.0 and 1.0 — evidence: print the actual value
4. **selected_domain** is one of the known domains or "default" — evidence: print the actual value
5. No `[:N]` slicing applied to deliverable content strings — evidence: grep for `\[:[0-9]+\]` in production code paths returned 0 hits

### Jurisdiction grounding (codified 2026-05-20 — MARS incident)
J1. **`jurisdiction_anchor` is present** with sub-keys: `state`, `address_token`, `procurement_portal`, `statute_citation`, `issuing_agency_line`, `solicitation_number`, `primary_rfp_file`, `extraction_method` — evidence: print top-level anchor keys
J2. **Every non-null anchor field has a `citation` of form `{file}:{line}`** — evidence: list each populated field and its citation
J3. **Every citation is grep-verifiable** — pick at least 3 non-null fields, grep the cited file for the `evidence_text` (or `value`), confirm the line number matches (±2 lines) — evidence: paste the grep results
J4. **No fabricated jurisdiction** — if `state.value` is set, verify `statute_citation.value` (when present) uses an allowed prefix for that state (e.g., Oregon → ORS/OAR; Washington → RCW/WAC; Alaska → AS); contradictory citations MUST be dropped to null with `dropped_reason` set — evidence: confirm or explicitly state none were dropped
J5. **No `*_domain_hints` blocks** — output JSON must NOT contain ad-hoc `mars_domain_hints`, `{rfp}_domain_hints`, or similar per-RFP hint dictionaries — evidence: grep output for `_domain_hints` returned 0 hits

### Cross-stage consistency
6. **All domain scores present** — every domain in DOMAIN_SIGNALS appears in the `scores` dict — evidence: print the domain keys in scores vs expected domains
7. **Profile loaded** — `profile` key is a dict (may be empty `{}` if no profile found, but must not be absent) — evidence: `type(domain_context["profile"])` returns dict

### Anti-regression rules (universal)
8. **UTF-8 encoding** on every `open()` call — evidence: search this phase's emitted scripts/code for `encoding='utf-8'` in every file-open
9. **ensure_ascii=False** on every `json.dump` call — evidence: same grep
10. **No `_Showing N of M_` row-cap notices** in any deliverable markdown — evidence: grep returned 0 matches
11. **No empty `|  |` mitigation/cell patterns** in any deliverable table — evidence: grep returned 0 matches in cells with HIGH/MEDIUM/CRITICAL severity rows
12. **No mid-word table-cell truncations** — evidence: line-by-line cell-end check returned 0 hits

### Memory discipline
13. **Relevant SAFS memory entries reviewed and applied** — evidence: list which memory files were read and which rules were applicable (e.g., "services is a DICT not list — applied flatten pattern")
