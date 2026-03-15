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
- Domain profiles in `/home/ddubiel/repos/safs/.claude/skills/process-rfp/domain-profiles/`

## Required Outputs

- `{folder}/shared/domain-context.json` - Detected domain with confidence scores

## Domain Profiles Available

| Domain | Profile | Key Signals |
|--------|---------|-------------|
| E-Commerce | `ecommerce` | product, cart, checkout, SKU, PCI-DSS |
| K-12 Education | `education` | student, enrollment, FERPA, CEDARS |
| Finance | `finance` | account, transaction, loan, BSA/AML |
| Healthcare | `healthcare` | patient, encounter, HIPAA, HL7/FHIR |
| Government | `government` | citizen, case, permit, FISMA, FedRAMP |
| Generic | `default` | (fallback when no domain matches) |

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
PROFILES_DIR = "/home/ddubiel/repos/safs/.claude/skills/process-rfp/domain-profiles"

if clear_winner and confidence >= 0.5:
    selected_domain = top_domain
else:
    selected_domain = "default"

profile_path = f"{PROFILES_DIR}/{selected_domain}.yaml"
profile = read_yaml(profile_path)
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
    "custom_terminology": extract_custom_terms(combined_content, profile)
}

# Enhancement: Auto-include Section 508 for government domain (FAR 39.2 mandate)
if selected_domain == "government":
    compliance_frameworks = domain_context.get("profile", {}).get("compliance_frameworks", [])
    if "Section 508" not in compliance_frameworks:
        compliance_frameworks.append("Section 508")
    domain_context["section_508_note"] = "Auto-included per FAR Subpart 39.2 — all federal ICT procurements require Section 508 accessibility compliance"

# Enhancement: Auto-flag CMMC for DoD/defense domain (Phase 1 live Nov 2025)
defense_signals = ["dod", "defense", "military", "army", "navy", "air force", "marines", "space force"]
if any(signal in content_lower for signal in defense_signals):
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

## Quality Checklist

- [ ] `domain-context.json` created in `shared/`
- [ ] Confidence score calculated and reasonable
- [ ] Domain profile loaded from profiles directory
- [ ] Custom terminology extracted
- [ ] All domain scores logged for transparency
