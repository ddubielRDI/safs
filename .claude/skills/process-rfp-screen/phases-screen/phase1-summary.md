---
name: phase1-summary
expert-role: Business Analyst
domain-expertise: RFP analysis, procurement terminology, requirements extraction
skill: procurement-analyst
sub-skill: tone-calibration
---

# Phase 1: RFP Summary Extraction

## Purpose

Single-pass extraction of key RFP metadata from the combined text assembled in Phase 0. Identifies the client, solicitation details, timeline, financials, scope, evaluation criteria, and mandatory requirements.

## Inputs

- Combined RFP text from Phase 0 (in memory)
- `{folder}/screen/source-manifest.json` -- Document inventory from Phase 0

## Required Output

- `{folder}/screen/rfp-summary.json` (>1KB)

## Instructions

### Step 1: Load Combined Text

```python
# Combined text is available in memory from Phase 0 execution
# If not in memory (e.g., resumed session), re-read from source documents using manifest

if not combined_text or len(combined_text.strip()) < 500:
    import json

    manifest_path = f"{folder}/screen/source-manifest.json"
    if not os.path.exists(manifest_path):
        error("ABORT: source-manifest.json not found -- Phase 0 must run first")
        halt()

    manifest = json.load(open(manifest_path))
    log("  Re-reading source documents from manifest...")

    # Re-convert documents to rebuild combined text
    converted_texts = []
    for doc_entry in manifest["documents"]:
        if doc_entry["conversion_status"] == "success":
            result = subprocess.run(
                ["markitdown", doc_entry["path"]],
                capture_output=True, text=True, timeout=120
            )
            if result.returncode == 0 and result.stdout.strip():
                converted_texts.append(result.stdout.strip())

    combined_text = "\n\n---\n\n".join(converted_texts)
    if manifest.get("truncated", False):
        combined_text = combined_text[:manifest.get("scan_limit", 80000)]

    log(f"  Rebuilt combined text: {len(combined_text)} chars")
```

### Step 2: Extract RFP Metadata

Using regex patterns and text analysis, extract all key fields into a structured summary.

```python
import re
import json
from datetime import datetime

rfp_summary = {
    "phase": "1",
    "timestamp": datetime.now().isoformat(),

    # Identification
    "client_name": None,        # Organization/agency name
    "rfp_number": None,         # RFP/RFQ/IFB number
    "rfp_title": None,          # Full title of the solicitation

    # Timeline
    "submission_deadline": None, # Due date/time
    "period_of_performance": None, # Contract duration
    "questions_deadline": None,  # Q&A deadline if found

    # Financials
    "estimated_value": None,    # Dollar amount if disclosed
    "contract_type": None,      # FFP, T&M, IDIQ, etc.
    "pricing_structure": None,  # How pricing is requested

    # Scope
    "scope_keywords": [],       # Key technology/service terms
    "industry_domain": None,    # Education, healthcare, government, etc.
    "set_aside": None,          # Small business, 8(a), HUBZone, etc.
    "preference_restrictions": [], # Geographic, veteran, MWBE, etc.

    # Evaluation
    "evaluation_criteria": [],  # [{name, weight_or_priority}]
    "evaluation_method": None,  # Best value, LPTA, tradeoff

    # Requirements (high-level)
    "mandatory_requirements": [], # Key mandatory items
    "key_deliverables": [],     # Major deliverables mentioned

    # Extraction confidence
    "extraction_confidence": "low",
    "fields_not_found": []
}
```

### Step 3: Pattern Matching for Each Field

#### Client Name

```python
org_patterns = [
    r"(?:issued\s+by|from|prepared\s+for|on\s+behalf\s+of)\s+(?:the\s+)?([A-Z][A-Za-z\s,]+(?:District|County|City|State|Agency|Department|Corporation|Inc|LLC|Authority|Commission|Board|University|College))",
    r"([A-Z][A-Za-z]+(?:\s+[A-Z][A-Za-z]+){1,5})\s+(?:requests?\s+proposals?|seeks?\s+(?:proposals?|vendors?))",
    r"(?:Owner|Client|Agency|Contracting\s+(?:Officer|Agency))[:\s]+([A-Z][A-Za-z\s,]+?)(?:\n|\.)",
]

for pattern in org_patterns:
    match = re.search(pattern, combined_text)
    if match:
        rfp_summary["client_name"] = match.group(1).strip().rstrip(",. ")
        log(f"  Client: {rfp_summary['client_name']}")
        break

if not rfp_summary["client_name"]:
    log("  Client name: NOT FOUND -- will attempt extraction from document headers")
    # Fallback: check first 2000 chars for prominent organization names
    header_text = combined_text[:2000]
    header_match = re.search(r"^#\s+(.+?)$", header_text, re.MULTILINE)
    if header_match:
        potential_org = header_match.group(1).strip()
        if len(potential_org) > 5 and len(potential_org) < 100:
            rfp_summary["client_name"] = potential_org
            log(f"  Client (from header): {rfp_summary['client_name']}")
```

#### RFP Number

```python
rfp_patterns = [
    r"(?:RFP|RFQ|IFB|ITB|Solicitation)\s*(?:#|No\.?|Number)?\s*[:.]?\s*([A-Z0-9][\w-]{3,30})",
    r"(?:Contract|Project)\s*(?:#|No\.?|Number)\s*[:.]?\s*([A-Z0-9][\w-]{3,30})",
    r"(?:Bid|Proposal)\s*(?:#|No\.?|Number)\s*[:.]?\s*([A-Z0-9][\w-]{3,30})",
]

for pattern in rfp_patterns:
    match = re.search(pattern, combined_text, re.IGNORECASE)
    if match:
        rfp_summary["rfp_number"] = match.group(1).strip()
        log(f"  RFP Number: {rfp_summary['rfp_number']}")
        break
```

#### RFP Title

```python
title_patterns = [
    r"(?:Request\s+for\s+(?:Proposal|Quotation|Information))\s*(?:for|:)\s*(.+?)(?:\n|$)",
    r"(?:Title|Subject|Project\s+Name)\s*[:]\s*(.+?)(?:\n|$)",
    r"(?:Solicitation\s+(?:Title|Name))\s*[:]\s*(.+?)(?:\n|$)",
]

for pattern in title_patterns:
    match = re.search(pattern, combined_text, re.IGNORECASE)
    if match:
        title = match.group(1).strip().rstrip(",. ")
        if len(title) > 5 and len(title) < 300:
            rfp_summary["rfp_title"] = title
            log(f"  Title: {rfp_summary['rfp_title']}")
            break
```

#### Submission Deadline

```python
deadline_patterns = [
    r"(?:due\s+(?:date|by)|deadline|submit(?:ted)?\s+(?:by|before|no\s+later\s+than))[:\s]+([^\n]{10,60})",
    r"(?:proposals?\s+(?:must\s+be\s+)?(?:received|submitted)\s+(?:by|before|no\s+later))[:\s]+([^\n]{10,60})",
    r"(?:closing\s+date|response\s+deadline|submission\s+date)[:\s]+([^\n]{10,60})",
    r"(?:no\s+later\s+than)\s+([^\n]{10,60})",
]

for pattern in deadline_patterns:
    match = re.search(pattern, combined_text, re.IGNORECASE)
    if match:
        rfp_summary["submission_deadline"] = match.group(1).strip().rstrip(",. ")
        log(f"  Deadline: {rfp_summary['submission_deadline']}")
        break
```

#### Period of Performance

```python
pop_patterns = [
    r"(?:period\s+of\s+performance|contract\s+(?:term|duration|period))[:\s]+([^\n]{5,100})",
    r"(\d+)\s*(?:year|month|week)s?\s+(?:base|option|period|contract)",
    r"(?:base\s+(?:year|period))\s*(?:of\s+)?(\d+)\s*(?:year|month)s?",
]

for pattern in pop_patterns:
    match = re.search(pattern, combined_text, re.IGNORECASE)
    if match:
        rfp_summary["period_of_performance"] = match.group(0).strip() if match.lastindex == 0 else match.group(1).strip()
        log(f"  Period of Performance: {rfp_summary['period_of_performance']}")
        break
```

#### Questions Deadline

```python
questions_patterns = [
    r"(?:questions?\s+(?:due|deadline|must\s+be\s+(?:submitted|received)))[:\s]+([^\n]{10,60})",
    r"(?:Q&A|inquiry|inquiries)\s+(?:deadline|due\s+(?:date|by))[:\s]+([^\n]{10,60})",
]

for pattern in questions_patterns:
    match = re.search(pattern, combined_text, re.IGNORECASE)
    if match:
        rfp_summary["questions_deadline"] = match.group(1).strip().rstrip(",. ")
        log(f"  Questions Deadline: {rfp_summary['questions_deadline']}")
        break
```

#### Estimated Value

```python
value_patterns = [
    r"(?:budget|estimated\s+(?:cost|value)|not\s+to\s+exceed|ceiling|maximum)\s*(?:of\s*)?\$?([\d,]+(?:\.\d{2})?)\s*(?:million|M)?",
    r"\$([\d,]+(?:\.\d{2})?)\s*(?:million|M|thousand|K)?",
    r"(?:budget|ceiling|NTE)\s*[:]\s*\$?([\d,]+(?:\.\d{2})?)",
]

for pattern in value_patterns:
    match = re.search(pattern, combined_text, re.IGNORECASE)
    if match:
        raw_value = match.group(1).strip()
        full_match = match.group(0).strip()
        # Check for million/M/thousand/K qualifier
        if re.search(r"million|M\b", full_match, re.IGNORECASE):
            rfp_summary["estimated_value"] = f"${raw_value} million"
        elif re.search(r"thousand|K\b", full_match, re.IGNORECASE):
            rfp_summary["estimated_value"] = f"${raw_value} thousand"
        else:
            rfp_summary["estimated_value"] = f"${raw_value}"
        log(f"  Estimated Value: {rfp_summary['estimated_value']}")
        break
```

#### Contract Type

```python
type_patterns = [
    r"(firm.fixed.price|FFP)",
    r"(time.and.materials?|T&M)",
    r"(IDIQ|indefinite.delivery.indefinite.quantity)",
    r"(cost.plus.fixed.fee|CPFF)",
    r"(cost.plus.incentive.fee|CPIF)",
    r"(cost.reimbursement|cost.reimbursable)",
    r"(labor.hour|LH)",
    r"(fixed.price.incentive|FPI)",
    r"(blanket.purchase.agreement|BPA)",
]

contract_type_map = {
    "firm": "FFP", "ffp": "FFP",
    "time": "T&M", "t&m": "T&M", "t and m": "T&M",
    "idiq": "IDIQ", "indefinite": "IDIQ",
    "cost plus fixed": "CPFF", "cpff": "CPFF",
    "cost plus incentive": "CPIF", "cpif": "CPIF",
    "cost reimburs": "Cost Reimbursement",
    "labor hour": "Labor Hour", "lh": "Labor Hour",
    "fixed price incentive": "FPI", "fpi": "FPI",
    "blanket": "BPA", "bpa": "BPA",
}

for pattern in type_patterns:
    match = re.search(pattern, combined_text, re.IGNORECASE)
    if match:
        found = match.group(1).lower()
        for key, label in contract_type_map.items():
            if key in found:
                rfp_summary["contract_type"] = label
                break
        if not rfp_summary["contract_type"]:
            rfp_summary["contract_type"] = match.group(1).strip()
        log(f"  Contract Type: {rfp_summary['contract_type']}")
        break
```

#### Pricing Structure

```python
pricing_patterns = [
    r"(?:pricing\s+(?:structure|format|approach|model))[:\s]+([^\n]{10,100})",
    r"(?:offerors?\s+(?:shall|must|should)\s+(?:provide|submit)\s+(?:pricing|cost))\s+([^\n]{10,100})",
]

for pattern in pricing_patterns:
    match = re.search(pattern, combined_text, re.IGNORECASE)
    if match:
        rfp_summary["pricing_structure"] = match.group(1).strip().rstrip(",. ")
        log(f"  Pricing Structure: {rfp_summary['pricing_structure']}")
        break
```

#### Set-Aside and Preference Restrictions

```python
setaside_patterns = [
    r"(?:set.aside|restricted\s+to|limited\s+to)\s+([^\n.]{10,80})",
    r"(small\s+business\s+set.aside)",
    r"(8\(a\)\s+(?:set.aside|sole\s+source|competitive))",
    r"(HUBZone\s+(?:set.aside|small\s+business))",
    r"((?:SDVOSB|service.disabled\s+veteran.owned)\s+(?:set.aside)?)",
    r"((?:WOSB|woman.owned\s+small\s+business)\s+(?:set.aside)?)",
    r"((?:EDWOSB|economically\s+disadvantaged\s+women?.owned))",
]

for pattern in setaside_patterns:
    match = re.search(pattern, combined_text, re.IGNORECASE)
    if match:
        rfp_summary["set_aside"] = match.group(1).strip()
        log(f"  Set-Aside: {rfp_summary['set_aside']}")
        break

# Check for preference/restriction keywords
preference_keywords = {
    "geographic": r"(?:local|geographic|state|regional)\s+preference",
    "veteran-owned": r"veteran.owned|VOSB|SDVOSB",
    "minority-owned": r"minority.owned|MBE|MWBE|DBE",
    "woman-owned": r"woman.owned|WBE|WOSB",
    "small-business": r"small\s+business|SBA|SBC",
    "HUBZone": r"HUBZone",
    "8(a)": r"8\(a\)",
}

for label, pattern in preference_keywords.items():
    if re.search(pattern, combined_text, re.IGNORECASE):
        rfp_summary["preference_restrictions"].append(label)

if rfp_summary["preference_restrictions"]:
    log(f"  Preferences: {', '.join(rfp_summary['preference_restrictions'])}")
```

#### Scope Keywords

```python
# Technology and service-related terms to detect
tech_keywords = [
    "cloud", "API", "database", "web", "mobile", "GIS", "analytics",
    "migration", "modernization", "cybersecurity", "integration",
    "data", "AI", "machine learning", "DevOps", "agile",
    "SaaS", "PaaS", "IaaS", "microservices", "containers",
    "helpdesk", "service desk", "support", "managed services",
    "ERP", "CRM", "CMS", "LMS", "HRIS", "BPM",
    "network", "infrastructure", "hosting", "maintenance",
    "training", "staffing", "consulting", "assessment",
    "security", "compliance", "governance", "audit",
    "testing", "QA", "quality assurance", "508", "accessibility",
    "data center", "disaster recovery", "backup", "monitoring"
]

combined_lower = combined_text.lower()
scope_keywords = [kw for kw in tech_keywords if kw.lower() in combined_lower]
rfp_summary["scope_keywords"] = scope_keywords

if scope_keywords:
    log(f"  Scope Keywords: {', '.join(scope_keywords[:15])}")
    if len(scope_keywords) > 15:
        log(f"    ... and {len(scope_keywords) - 15} more")
```

#### Industry Domain

```python
domain_indicators = {
    "Education": r"(?:school|education|student|academic|university|college|K-12|curriculum|learning)",
    "Healthcare": r"(?:health|medical|clinical|patient|hospital|EHR|HIPAA|pharmacy|nursing)",
    "Government - Federal": r"(?:federal|FAR\s+\d|DFAR|GSA|DoD|Department\s+of\s+(?:Defense|Energy|State|Interior|Commerce))",
    "Government - State": r"(?:state\s+(?:of|agency|government)|governor|legislature|state\s+procurement)",
    "Government - Local": r"(?:county|city\s+of|municipality|town\s+of|borough|parish|local\s+government)",
    "Finance": r"(?:banking|financial|insurance|fintech|investment|securities|lending)",
    "Transportation": r"(?:transit|transportation|DOT|highway|airport|railway|fleet)",
    "Energy": r"(?:energy|utility|electric|gas|power|renewable|solar|wind|grid)",
    "Defense": r"(?:defense|military|DoD|armed\s+forces|combat|tactical|clearance)",
    "IT Services": r"(?:managed\s+services|IT\s+support|help\s*desk|service\s+desk|NOC|SOC)",
}

domain_scores = {}
for domain, pattern in domain_indicators.items():
    matches = re.findall(pattern, combined_text, re.IGNORECASE)
    if matches:
        domain_scores[domain] = len(matches)

if domain_scores:
    rfp_summary["industry_domain"] = max(domain_scores, key=domain_scores.get)
    log(f"  Industry Domain: {rfp_summary['industry_domain']} ({domain_scores[rfp_summary['industry_domain']]} indicators)")
```

#### Evaluation Criteria

```python
eval_criteria = []

# Look for structured evaluation criteria sections
eval_section_patterns = [
    r"(?:evaluation\s+(?:criteria|factors?))[:\s]*\n((?:.+\n){1,20})",
    r"(?:scoring\s+criteria|selection\s+criteria)[:\s]*\n((?:.+\n){1,20})",
]

for pattern in eval_section_patterns:
    match = re.search(pattern, combined_text, re.IGNORECASE)
    if match:
        section_text = match.group(1)
        # Extract individual criteria with weights
        criteria_lines = re.findall(
            r"(?:[-*]\s*)?([A-Za-z][A-Za-z\s/]+?)(?:\s*[-:–]\s*(\d+)\s*(?:%|points?|pts?))?(?:\n|$)",
            section_text
        )
        for name, weight in criteria_lines:
            name = name.strip().rstrip(",.- ")
            if len(name) > 3 and len(name) < 80:
                eval_criteria.append({
                    "name": name,
                    "weight_or_priority": f"{weight}%" if weight else "not specified"
                })
        if eval_criteria:
            break

# Fallback: look for individual factor mentions with points
if not eval_criteria:
    factor_patterns = [
        r"(technical\s+(?:approach|proposal|capability))\s*[:\-\u2013]\s*(\d+)\s*(?:%|points?|pts?)",
        r"(management\s+(?:approach|plan|proposal))\s*[:\-\u2013]\s*(\d+)\s*(?:%|points?|pts?)",
        r"(past\s+performance)\s*[:\-\u2013]\s*(\d+)\s*(?:%|points?|pts?)",
        r"(price|cost)\s*[:\-\u2013]\s*(\d+)\s*(?:%|points?|pts?)",
        r"(staffing|personnel|key\s+personnel)\s*[:\-\u2013]\s*(\d+)\s*(?:%|points?|pts?)",
        r"(experience|qualifications?)\s*[:\-\u2013]\s*(\d+)\s*(?:%|points?|pts?)",
    ]
    for pattern in factor_patterns:
        match = re.search(pattern, combined_text, re.IGNORECASE)
        if match:
            eval_criteria.append({
                "name": match.group(1).strip(),
                "weight_or_priority": f"{match.group(2)}%"
            })

rfp_summary["evaluation_criteria"] = eval_criteria
if eval_criteria:
    log(f"  Evaluation Criteria: {len(eval_criteria)} found")
    for ec in eval_criteria:
        log(f"    - {ec['name']}: {ec['weight_or_priority']}")
```

#### Evaluation Method

```python
eval_method_patterns = [
    # QBS / RFQ patterns first (municipal procurement often uses these)
    (r"request\s+for\s+qualifications|RFQ(?!\s*\d)", "Qualifications-Based Selection (QBS)"),
    (r"qualifications?.based|QBS", "Qualifications-Based Selection (QBS)"),
    (r"statement\s+of\s+qualifications|SOQ", "Qualifications-Based Selection (QBS)"),
    # Federal/standard patterns
    (r"best\s+value", "Best Value"),
    (r"(?:lowest\s+price\s+technically\s+acceptable|LPTA)", "LPTA"),
    (r"tradeoff|trade-off", "Tradeoff"),
    (r"lowest\s+(?:responsible\s+)?bidder", "Lowest Bidder"),
    (r"technically\s+acceptable", "Technically Acceptable"),
]
# QBS note: Qualifications-Based Selection means no price competition at
# evaluation stage. Fees are negotiated ONLY with the top-ranked firm.
# This is common in municipal professional services procurement (Brooks Act
# for A/E, and many cities apply similar QBS to IT/GIS consulting).
# When QBS detected, set technical_to_price_ratio to "100:0" in evaluation_model.

for pattern, method in eval_method_patterns:
    if re.search(pattern, combined_text, re.IGNORECASE):
        rfp_summary["evaluation_method"] = method
        log(f"  Evaluation Method: {method}")
        break
```

#### Mandatory Requirements

```python
mandatory_patterns = [
    r"(?:shall|must|required\s+to|mandatory)[:\s]+([^\n]{15,200})",
    r"(?:minimum\s+(?:requirements?|qualifications?))[:\s]+([^\n]{15,200})",
]

mandatory_items = set()
for pattern in mandatory_patterns:
    matches = re.findall(pattern, combined_text, re.IGNORECASE)
    for m in matches[:10]:  # Cap at 10 to avoid noise
        item = m.strip().rstrip(",. ")
        if len(item) > 15 and len(item) < 200:
            mandatory_items.add(item)

rfp_summary["mandatory_requirements"] = list(mandatory_items)[:8]  # Top 8

if rfp_summary["mandatory_requirements"]:
    log(f"  Mandatory Requirements: {len(rfp_summary['mandatory_requirements'])} identified")
```

#### Key Deliverables

```python
deliverable_patterns = [
    r"(?:deliverables?)[:\s]*\n((?:.+\n){1,15})",
    r"(?:[-*]\s*)?((?:final|draft|monthly|weekly|quarterly)\s+[A-Za-z\s]+(?:report|plan|document|analysis|assessment))",
    r"(?:contractor\s+(?:shall|must)\s+(?:deliver|provide|submit))\s+([^\n]{10,100})",
]

deliverable_items = set()
for pattern in deliverable_patterns:
    matches = re.findall(pattern, combined_text, re.IGNORECASE)
    for m in matches[:10]:
        item = m.strip().rstrip(",. ")
        if len(item) > 5 and len(item) < 150:
            deliverable_items.add(item)

rfp_summary["key_deliverables"] = list(deliverable_items)[:8]

if rfp_summary["key_deliverables"]:
    log(f"  Key Deliverables: {len(rfp_summary['key_deliverables'])} identified")
```

### Step 3b: Skill-Informed Extraction Validation (procurement-analyst)

After regex extraction, apply procurement-analyst skill frameworks to validate and enrich the extraction:

```python
# 1. Solicitation Type Classification (from skill taxonomy)
#    Classify the RFP type and note implications for bid strategy
solicitation_type_indicators = {
    "RFP": r"request\s+for\s+proposal",
    "RFQ": r"request\s+for\s+quotation",
    "IFB": r"invitation\s+for\s+bid",
    "BAA": r"broad\s+agency\s+announcement",
    "IDIQ": r"indefinite\s+delivery|IDIQ",
    "RFI": r"request\s+for\s+information",
    "RFSQ": r"request\s+for\s+(?:statement\s+of\s+)?qualification",
}
detected_type = None
for sol_type, pattern in solicitation_type_indicators.items():
    if re.search(pattern, combined_text, re.IGNORECASE):
        detected_type = sol_type
        break
rfp_summary["solicitation_type"] = detected_type or "RFP"  # Default to RFP
rfp_summary["solicitation_type_implications"] = {
    "RFP": "Best value evaluation likely -- technical approach matters as much as price",
    "RFQ": "Price-competitive -- lowest price technically acceptable may apply",
    "IFB": "Sealed bid, lowest price wins -- focus on cost, not technical differentiators",
    "BAA": "Research-oriented -- innovation and technical approach are primary",
    "IDIQ": "Task order vehicle -- past performance on similar vehicles is key",
    "RFI": "Information only -- no bid, but positions for future solicitation",
    "RFSQ": "Qualification-based -- demonstrate capability, pricing secondary",
}.get(rfp_summary["solicitation_type"], "Standard competitive procurement")
log(f"  Solicitation Type: {rfp_summary['solicitation_type']} -- {rfp_summary['solicitation_type_implications']}")

# 2. Evaluation Methodology Identification (from skill language markers)
#    Distinguish LPTA vs Best Value vs QBS using specific language
eval_method = rfp_summary.get("evaluation_method")
if not eval_method:
    lpta_markers = ["lowest price", "technically acceptable", "pass/fail", "acceptable/unacceptable"]
    best_value_markers = ["best value", "tradeoff", "trade-off", "most advantageous"]
    qbs_markers = ["qualifications-based", "qualification based", "QBS", "Brooks Act"]
    combined_lower = combined_text.lower()
    if any(m in combined_lower for m in lpta_markers):
        rfp_summary["evaluation_method"] = "LPTA"
    elif any(m in combined_lower for m in best_value_markers):
        rfp_summary["evaluation_method"] = "Best Value"
    elif any(m in combined_lower for m in qbs_markers):
        rfp_summary["evaluation_method"] = "Qualifications-Based"

# 3. Contract Type Pricing Strategy Implications (from skill's FFP/T&M/CPFF table)
contract_type = rfp_summary.get("contract_type")
if contract_type:
    pricing_strategy_map = {
        "FFP": "Fixed price -- all cost risk on contractor. Price must include contingency. Tight scope definition critical.",
        "T&M": "Time and materials -- rate competition. Focus on competitive hourly rates and efficient staffing.",
        "IDIQ": "Task order vehicle -- demonstrate flexibility and rapid response capability.",
        "CPFF": "Cost plus fixed fee -- government assumes cost risk. Focus on fee reasonableness and cost controls.",
        "BPA": "Blanket purchase -- volume pricing and availability matter. Quick turnaround expected.",
        "Labor Hour": "Hourly rate competition -- similar to T&M but without materials.",
    }
    rfp_summary["pricing_strategy_note"] = pricing_strategy_map.get(contract_type, "Review contract type for pricing approach")
```

### Step 4: Assess Extraction Confidence

```python
critical_fields = ["client_name", "rfp_title", "submission_deadline", "scope_keywords"]
found_critical = sum(1 for f in critical_fields if rfp_summary.get(f))

if found_critical >= 3:
    rfp_summary["extraction_confidence"] = "high"
elif found_critical >= 2:
    rfp_summary["extraction_confidence"] = "medium"
else:
    rfp_summary["extraction_confidence"] = "low"

# Build list of fields that could not be extracted
skip_fields = {"phase", "timestamp", "extraction_confidence", "fields_not_found",
               "scope_keywords", "preference_restrictions", "evaluation_criteria",
               "mandatory_requirements", "key_deliverables"}

rfp_summary["fields_not_found"] = [
    k for k, v in rfp_summary.items()
    if k not in skip_fields and (v is None or v == [])
]

log(f"  Extraction Confidence: {rfp_summary['extraction_confidence']}")
log(f"  Fields not found: {len(rfp_summary['fields_not_found'])}")
if rfp_summary["fields_not_found"]:
    for field in rfp_summary["fields_not_found"]:
        log(f"    - {field}")
```

### Step 4b: Extract Required Technologies & Evaluation Subfactors (LLM)

After regex-based extraction, use LLM analysis to extract precise technology requirements and evaluation subfactors from the combined RFP text. These overlay the generic `scope_keywords` with specific named technologies and decompose evaluation criteria into what evaluators actually assess.

```python
# Build context from already-extracted fields
extraction_context = {
    "scope_keywords": rfp_summary.get("scope_keywords", []),
    "evaluation_criteria": rfp_summary.get("evaluation_criteria", []),
    "mandatory_requirements": rfp_summary.get("mandatory_requirements", []),
    "key_deliverables": rfp_summary.get("key_deliverables", [])
}

tech_subfactor_prompt = f"""Analyze this RFP text and extract two things:

1. REQUIRED TECHNOLOGIES: Specific named technologies, platforms, products, and standards
   mentioned as required or preferred. Extract proper nouns and specific versions — not
   generic terms like "database" or "web" (those are already in scope_keywords).
   Examples of what TO extract: "ArcGIS Server", "SQL Server 2019", "React", ".NET 8",
   "AWS GovCloud", "ServiceNow", "Salesforce", "JIRA", "Power BI"
   Examples of what NOT to extract: "database", "cloud", "web application", "analytics"

2. EVALUATION SUBFACTORS: For each evaluation criterion listed below, identify what the
   RFP text says the evaluator will specifically assess. Look for:
   - Section headings or numbered items under each criterion
   - Stated sub-factors, sub-elements, or scoring breakdowns
   - Emphasis language ("must demonstrate", "will be evaluated on", "should include")
   - If no subfactors are discernible for a criterion, return an empty list

Already-extracted evaluation criteria:
{json.dumps(extraction_context["evaluation_criteria"], indent=2)}

Already-extracted scope keywords (for reference — do NOT duplicate these):
{json.dumps(extraction_context["scope_keywords"])}

RFP TEXT:
{combined_text[:60000]}

Return JSON only:
{{
  "required_technologies": ["TechName1", "TechName2", ...],
  "evaluation_subfactors": [
    {{
      "criterion": "Criterion Name (must match an evaluation_criteria name)",
      "weight": "weight if known, e.g. 60%",
      "subfactors": ["What evaluator assesses #1", "What evaluator assesses #2", ...]
    }}
  ]
}}
"""

tech_result = llm(tech_subfactor_prompt, json_mode=True)

# Validate and store
if isinstance(tech_result.get("required_technologies"), list):
    rfp_summary["required_technologies"] = tech_result["required_technologies"]
    log(f"  Required Technologies: {len(rfp_summary['required_technologies'])} extracted")
    for tech in rfp_summary["required_technologies"][:10]:
        log(f"    - {tech}")

    # CRITICAL: Merge required_technologies INTO scope_keywords so downstream phases
    # (matching, scoring, themes) use precise terms, not generic ones.
    # Dedup by lowercase comparison; required_technologies take precedence.
    existing_lower = {kw.lower() for kw in rfp_summary.get("scope_keywords", [])}
    for tech in rfp_summary["required_technologies"]:
        if tech.lower() not in existing_lower:
            rfp_summary["scope_keywords"].append(tech)
            existing_lower.add(tech.lower())
    log(f"  Scope keywords enriched: {len(rfp_summary['scope_keywords'])} total (merged required_technologies)")
else:
    rfp_summary["required_technologies"] = []
    log("  Required Technologies: extraction failed — falling back to scope_keywords")

if isinstance(tech_result.get("evaluation_subfactors"), list):
    rfp_summary["evaluation_subfactors"] = tech_result["evaluation_subfactors"]
    log(f"  Evaluation Subfactors: {len(rfp_summary['evaluation_subfactors'])} criteria decomposed")
    for sf in rfp_summary["evaluation_subfactors"]:
        log(f"    - {sf.get('criterion', '?')}: {len(sf.get('subfactors', []))} subfactors")
else:
    rfp_summary["evaluation_subfactors"] = []
    log("  Evaluation Subfactors: extraction failed")

# --- Derive evaluation_model from evaluation_criteria + evaluation_subfactors ---
# This synthesizes already-extracted data into an evaluator-perspective model.

eval_criteria = rfp_summary.get("evaluation_criteria", [])
eval_subfactors = rfp_summary.get("evaluation_subfactors", [])
eval_method = rfp_summary.get("evaluation_method", "Best Value")

# Step A: Determine total points and per-criterion allocation
# If explicit weights/points are stated, use them directly.
# If relative importance language found, estimate using standard splits.
has_explicit_weights = any(
    ec.get("weight_or_priority", "").replace("%", "").strip().isdigit()
    for ec in eval_criteria
)

point_allocation = []
total_estimated_points = 1000  # Default assumption

if has_explicit_weights:
    # Use explicit weights -- normalize to 1000-point scale
    for ec in eval_criteria:
        raw_weight = ec.get("weight_or_priority", "0").replace("%", "").replace("points", "").replace("pts", "").strip()
        try:
            weight_val = float(raw_weight)
        except ValueError:
            weight_val = 0
        point_allocation.append({
            "criterion": ec.get("name", "Unknown"),
            "points": round(weight_val * 10),  # Convert percentage to 1000-scale
            "pct": f"{int(weight_val)}%",
            "discriminator_potential": "high" if weight_val >= 30 else ("medium" if weight_val >= 15 else "low")
        })
else:
    # Estimate from relative importance language in RFP text
    combined_lower = combined_text.lower()
    sig_more_important = any(phrase in combined_lower for phrase in [
        "significantly more important", "considerably more important",
        "technical factors are more important"
    ])
    approx_equal = any(phrase in combined_lower for phrase in [
        "approximately equal", "equal importance", "equally weighted",
        "given equal weight"
    ])
    somewhat_more = any(phrase in combined_lower for phrase in [
        "somewhat more important", "slightly more important",
        "more important than"
    ])

    n_criteria = max(len(eval_criteria), 1)
    if sig_more_important and n_criteria >= 2:
        # 60/25/15 split (or 60/40 for 2 criteria)
        splits = [60, 25, 15] if n_criteria >= 3 else [60, 40]
    elif somewhat_more and n_criteria >= 2:
        # 50/30/20 split
        splits = [50, 30, 20] if n_criteria >= 3 else [55, 45]
    elif approx_equal:
        # Equal split
        equal_share = round(100 / n_criteria)
        splits = [equal_share] * n_criteria
    else:
        # Default: slight technical advantage (common in Best Value)
        if n_criteria >= 3:
            splits = [45, 30, 25]
        elif n_criteria == 2:
            splits = [55, 45]
        else:
            splits = [100]

    for i, ec in enumerate(eval_criteria):
        pct = splits[i] if i < len(splits) else splits[-1]
        point_allocation.append({
            "criterion": ec.get("name", "Unknown"),
            "points": round(pct * 10),
            "pct": f"{pct}%",
            "discriminator_potential": "high" if pct >= 30 else ("medium" if pct >= 15 else "low")
        })

# Step B: Determine technical-to-price ratio
tech_points = sum(
    pa["points"] for pa in point_allocation
    if not any(kw in pa["criterion"].lower() for kw in ["price", "cost"])
)
price_points = sum(
    pa["points"] for pa in point_allocation
    if any(kw in pa["criterion"].lower() for kw in ["price", "cost"])
)
if price_points == 0 and tech_points > 0:
    # Price not listed as scored criterion -- assume tradeoff
    tech_to_price_ratio = "100:0 (price evaluated separately)"
elif tech_points + price_points > 0:
    tech_pct = round(tech_points / (tech_points + price_points) * 100)
    price_pct = 100 - tech_pct
    tech_to_price_ratio = f"{tech_pct}:{price_pct}"
else:
    tech_to_price_ratio = "unknown"

# Step C: Derive evaluator persona from method + subfactors
evaluator_persona_map = {
    "LPTA": "compliance_panel",
    "Lowest Bidder": "price_panel",
    "Qualifications-Based": "qualifications_panel",
    "Best Value": "technical_panel",
    "Tradeoff": "technical_panel",
}
evaluator_persona = evaluator_persona_map.get(eval_method, "technical_panel")

# Step D: Build evaluation method implications string
implications_map = {
    "LPTA": "LPTA -- meet minimum technical threshold, then lowest price wins",
    "Best Value": "Best Value -- technical differentiators matter more than price alone",
    "Tradeoff": "Tradeoff -- technical superiority can justify higher price",
    "Qualifications-Based": "QBS -- demonstrated qualifications drive selection; price negotiated after",
    "Lowest Bidder": "Sealed bid -- lowest responsive, responsible bidder wins",
}
eval_method_implications = implications_map.get(eval_method, f"{eval_method} -- review evaluation section for specific implications")

# Step E: Assemble evaluation_model
rfp_summary["evaluation_model"] = {
    "total_estimated_points": total_estimated_points,
    "technical_to_price_ratio": tech_to_price_ratio,
    "evaluation_method_implications": eval_method_implications,
    "evaluator_persona": evaluator_persona,
    "point_allocation": point_allocation
}

log(f"  Evaluation Model: {len(point_allocation)} criteria allocated, {tech_to_price_ratio} tech:price")
for pa in point_allocation:
    log(f"    - {pa['criterion']}: {pa['points']}pts ({pa['pct']}) [{pa['discriminator_potential']}]")
```

### Step 4c: Identify Buyer Priorities (LLM)

Identify 3–6 buyer priorities (decision drivers) by analyzing repetition, emphasis, evaluation weight, and structural patterns in the RFP. These represent what the evaluator cares about most — Shipley "hot buttons" / APMP "Customer Intimacy" / Lohfeld "decision drivers."

```python
# Build enriched context from all prior extractions
priority_context = {
    "required_technologies": rfp_summary.get("required_technologies", []),
    "evaluation_subfactors": rfp_summary.get("evaluation_subfactors", []),
    "evaluation_criteria": rfp_summary.get("evaluation_criteria", []),
    "scope_keywords": rfp_summary.get("scope_keywords", []),
    "mandatory_requirements": rfp_summary.get("mandatory_requirements", [])
}

buyer_priority_prompt = f"""Analyze this RFP to identify 3-6 BUYER PRIORITIES — the things the
evaluator cares about most deeply. These are NOT just scope items; they are decision drivers
that will separate winning proposals from adequate ones.

Detect priorities by looking for CONVERGENCE of these signals:
- REPETITION: Same concept appears in multiple RFP sections (scope, requirements, evaluation, staffing)
- EMPHASIS: Words like "critical", "essential", "key", "must demonstrate", "extensive experience"
- EVALUATION WEIGHT: Higher-weighted criteria = proxy for buyer priorities
- STRUCTURAL EMPHASIS: Dedicated RFP sections, phased approach, detailed sub-requirements
- QUALIFICATION SPECIFICITY: Named platforms/certifications (not generic "technical experience")

Already-extracted data for context:
{json.dumps(priority_context, indent=2)}

RFP TEXT:
{combined_text[:60000]}

For each priority, rate importance:
- HIGH: Evaluator will actively score on this; appears in 3+ RFP sections or is explicitly weighted
- MEDIUM: Evaluator notices but doesn't weight separately; appears in 1-2 sections

Return JSON only:
{{
  "buyer_priorities": [
    {{
      "name": "Short descriptive name (e.g., 'ESRI GIS Platform Expertise')",
      "importance": "HIGH or MEDIUM",
      "signal": "Evidence string: where/how this priority was detected in the RFP (cite sections, repetition count, emphasis language)",
      "evaluation_criterion": "Which evaluation criterion this maps to, or 'Multiple' / 'None'",
      "linked_scope_keywords": ["keyword1", "keyword2"]
    }}
  ]
}}

Rules:
- Return 3-6 priorities, ordered by importance (HIGH first)
- linked_scope_keywords: max 5 per priority, drawn from scope_keywords or required_technologies
- Every priority MUST cite specific RFP evidence in the signal field
- Do NOT invent priorities not supported by the text
"""

priority_result = llm(buyer_priority_prompt, json_mode=True)

# Validate and store
if isinstance(priority_result.get("buyer_priorities"), list):
    # Cap linked_scope_keywords at 5 per priority
    for p in priority_result["buyer_priorities"]:
        if isinstance(p.get("linked_scope_keywords"), list):
            p["linked_scope_keywords"] = p["linked_scope_keywords"][:5]
        else:
            p["linked_scope_keywords"] = []

    rfp_summary["buyer_priorities"] = priority_result["buyer_priorities"]
    log(f"  Buyer Priorities: {len(rfp_summary['buyer_priorities'])} identified")
    for bp in rfp_summary["buyer_priorities"]:
        log(f"    - [{bp.get('importance', '?')}] {bp.get('name', '?')}")
else:
    rfp_summary["buyer_priorities"] = []
    log("  Buyer Priorities: extraction failed")
```

### Step 4d: Client Tone Detection (LLM)

Perform a SINGLE LLM call analyzing the combined RFP text for communication style patterns. The detected tone profile informs downstream proposal writing phases — matching the client's register increases evaluator receptivity (Shipley "mirror the customer's language" principle).

```python
# Build context from prior extractions
tone_context = {
    "industry_domain": rfp_summary.get("industry_domain"),
    "solicitation_type": rfp_summary.get("solicitation_type", "RFP"),
    "evaluation_method": rfp_summary.get("evaluation_method"),
    "client_name": rfp_summary.get("client_name")
}

tone_detection_prompt = f"""Analyze this RFP text for communication style and tone patterns. Your goal is
to characterize HOW this client writes so we can mirror their register in the proposal response.

QUANTITATIVE SIGNALS to measure:
- Passive voice ratio: estimate what percentage of directive sentences use passive voice
  (e.g., "shall be provided" vs "the contractor shall provide")
- "Shall" density: count instances of "shall" per 1000 words
- Acronym density: count unique acronyms per 1000 words (exclude common: US, RFP, etc.)
- Metric mention count: how many specific numeric targets/thresholds appear
  (e.g., "99.9% uptime", "within 2 business days", "no fewer than 5 years")
- Transformation vocabulary count: instances of change-oriented language
  (e.g., "modernize", "transform", "innovate", "re-engineer", "next-generation")
- Regulatory reference density: count of specific regulatory citations per 1000 words
  (e.g., "FAR 52.212-1", "HIPAA", "FISMA", "Section 508", "FedRAMP")

QUALITATIVE SIGNALS to assess:
- SOW vs PWS vs SOO: which statement type is used? (SOW = prescriptive, PWS = outcomes,
  SOO = high-level objectives)
- Section M language style: does it read as checklist-like ("pass/fail") or narrative
  ("evaluators will assess the degree to which...")?
- Past performance description style: does the RFP ask for narratives, structured forms,
  CPARS references, or client contact lists?

MIRRORING VOCABULARY: Identify 6-10 terms or phrases the client uses REPEATEDLY that a
proposal response should mirror. These are specific to THIS client — not generic procurement
terms. Look for:
- Preferred labels for concepts (e.g., "enterprise solution" vs "platform", "stakeholder" vs "user")
- Recurring qualified phrases (e.g., "mission-critical", "cost-effective", "proven methodology")
- Domain jargon the client favors

TONE REGISTER: Classify the PRIMARY and SECONDARY style from:
- formal_bureaucratic: Heavy "shall" usage, passive voice, regulatory citations, structured sections
- outcomes_focused: PWS/SOO style, metric targets, performance standards, results language
- innovation_driven: Transformation vocabulary, modernization, future-state language, agile references
- compliance_heavy: Dense regulatory references, certification requirements, audit language
- mission_driven: Agency mission references, public benefit language, stakeholder impact focus

Context:
{json.dumps(tone_context, indent=2)}

RFP TEXT:
{combined_text[:60000]}

Return JSON only:
{{
  "primary_style": "one of the 5 registers",
  "secondary_style": "one of the 5 registers (different from primary)",
  "formality_level": 1-5,
  "vocabulary_register": {{
    "bureaucratic_density": 0.0-1.0,
    "outcomes_density": 0.0-1.0,
    "innovation_density": 0.0-1.0,
    "compliance_density": 0.0-1.0
  }},
  "quantitative_signals": {{
    "passive_voice_ratio": 0.0-1.0,
    "shall_density_per_1k": 0.0,
    "acronym_density_per_1k": 0.0,
    "metric_mention_count": 0,
    "transformation_vocabulary_count": 0,
    "regulatory_reference_density_per_1k": 0.0
  }},
  "qualitative_signals": {{
    "statement_type": "SOW|PWS|SOO|mixed",
    "section_m_style": "checklist|narrative|hybrid|not_found",
    "past_performance_style": "narrative|structured_form|cpars|contact_list|not_specified"
  }},
  "directive_voice": "prescriptive|outcomes_based|collaborative|mixed",
  "mirroring_vocabulary": ["term1", "term2", ...],
  "adaptation_rules": {{
    "use_passive_voice": true/false,
    "prefer_terms": ["term1", "term2"],
    "avoid_terms": ["term1", "term2"]
  }},
  "confidence": "high|medium|low",
  "evidence": ["supporting observation 1", "supporting observation 2"]
}}
"""

tone_result = llm(tone_detection_prompt, json_mode=True)

# Validate required fields and store
valid_styles = {"formal_bureaucratic", "outcomes_focused", "innovation_driven", "compliance_heavy", "mission_driven"}
if (isinstance(tone_result.get("primary_style"), str)
        and tone_result["primary_style"] in valid_styles
        and isinstance(tone_result.get("mirroring_vocabulary"), list)):

    # Enforce mirroring_vocabulary bounds (6-10 terms)
    mirror_vocab = tone_result["mirroring_vocabulary"]
    if len(mirror_vocab) < 6:
        log(f"  WARNING: Only {len(mirror_vocab)} mirroring terms detected (target: 6-10)")
    tone_result["mirroring_vocabulary"] = mirror_vocab[:10]

    # Ensure formality_level is in 1-5 range
    tone_result["formality_level"] = max(1, min(5, int(tone_result.get("formality_level", 3))))

    # Ensure confidence is valid
    if tone_result.get("confidence") not in {"high", "medium", "low"}:
        tone_result["confidence"] = "medium"

    # Default adaptation_rules structure
    adapt = tone_result.get("adaptation_rules", {})
    tone_result["adaptation_rules"] = {
        "use_passive_voice": adapt.get("use_passive_voice", False),
        "prefer_terms": adapt.get("prefer_terms", [])[:10],
        "avoid_terms": adapt.get("avoid_terms", [])[:10]
    }

    rfp_summary["client_tone"] = tone_result
    log(f"  Client Tone: {tone_result['primary_style']} (secondary: {tone_result.get('secondary_style', 'none')})")
    log(f"    Formality: {tone_result['formality_level']}/5, Confidence: {tone_result['confidence']}")
    log(f"    Mirroring vocabulary: {len(tone_result['mirroring_vocabulary'])} terms")
    for term in tone_result["mirroring_vocabulary"]:
        log(f"      - \"{term}\"")
    log(f"    Adaptation: passive_voice={tone_result['adaptation_rules']['use_passive_voice']}, "
        f"prefer {len(tone_result['adaptation_rules']['prefer_terms'])} terms, "
        f"avoid {len(tone_result['adaptation_rules']['avoid_terms'])} terms")
else:
    rfp_summary["client_tone"] = {
        "primary_style": "formal_bureaucratic",
        "secondary_style": "compliance_heavy",
        "formality_level": 3,
        "vocabulary_register": {"bureaucratic_density": 0.0, "outcomes_density": 0.0, "innovation_density": 0.0, "compliance_density": 0.0},
        "quantitative_signals": {},
        "qualitative_signals": {},
        "directive_voice": "mixed",
        "mirroring_vocabulary": [],
        "adaptation_rules": {"use_passive_voice": False, "prefer_terms": [], "avoid_terms": []},
        "confidence": "low",
        "evidence": ["Tone detection failed -- using neutral defaults"]
    }
    log("  Client Tone: detection failed -- using neutral defaults")
```

### Step 4e: Full Technology Intelligence

Enrich the already-extracted `required_technologies` list with ecosystem mapping, maturity classification, version detection, stack coherence analysis, and RDI alignment against `company-profile.json` services.

```python
# Load company profile services for RDI alignment
import json as _json

company_profile_path = os.path.join(os.path.dirname(os.path.dirname(folder)), "company-profile.json")
if not os.path.exists(company_profile_path):
    # Try alternate location at project root
    company_profile_path = os.path.join(folder, "company-profile.json")

rdi_services = []
if os.path.exists(company_profile_path):
    try:
        cp = _json.load(open(company_profile_path))
        # services is DICT not list -- flatten all categories
        services_data = cp.get("services", {})
        if isinstance(services_data, dict):
            rdi_services = [svc for cat in services_data.values() for svc in (cat if isinstance(cat, list) else [cat])]
        elif isinstance(services_data, list):
            rdi_services = services_data
        log(f"  Loaded {len(rdi_services)} RDI services from company-profile.json")
    except Exception as e:
        log(f"  WARNING: Could not load company-profile.json: {e}")
else:
    log("  WARNING: company-profile.json not found -- RDI alignment will be empty")

required_techs = rfp_summary.get("required_technologies", [])

tech_intel_prompt = f"""Analyze the required technologies from this RFP and produce a full technology
intelligence assessment. You have two inputs:
1. The list of specific technologies already extracted from the RFP
2. The full RFP text (to find version numbers and additional context)

REQUIRED TECHNOLOGIES (already extracted):
{json.dumps(required_techs, indent=2)}

RDI SERVICE CATALOG (our company's capabilities):
{json.dumps(rdi_services, indent=2)}

RFP TEXT (for version numbers and context):
{combined_text[:50000]}

For each technology, determine:
- What ecosystem/stack it belongs to (e.g., "Cloud (AWS)", "Data Platform", "Frontend", "DevOps")
- Its role in the architecture (hosting, compute, storage, orchestration, UI, etc.)
- Maturity classification:
  * emerging: technology < 3 years since GA release
  * established: 3-10 years since GA, active development
  * mature: 10+ years, stable, widely adopted
  * declining: EOL announced or approaching, successor recommended
- Version number if explicitly mentioned in the RFP text (null if not stated)

GROUP technologies into coherent stacks. Assess each stack's internal coherence:
- high: components are designed to work together (e.g., AWS services)
- medium: components commonly used together but from different vendors
- low: unusual combination, potential integration challenges

CROSS-REFERENCE each technology against the RDI service catalog:
- strong_match: technology or direct equivalent appears in our services
- partial_match: related capability exists (e.g., we do "cloud migration" but RFP needs specific AWS service)
- no_match: technology not represented in our service catalog

FLAG any technology risks:
- Declining/EOL technologies
- Technologies requiring specialized certifications we may not hold
- Unusual combinations that signal integration complexity
- Technologies not in RDI catalog that would require partnerships

Return JSON only:
{{
  "technology_stacks": [
    {{
      "stack_name": "Cloud (AWS)",
      "components": [
        {{
          "name": "AWS GovCloud",
          "version": null,
          "role": "hosting",
          "maturity": "established"
        }}
      ],
      "coherence": "high"
    }}
  ],
  "unversioned_technologies": ["tech names where RFP did not specify a version"],
  "stack_coherence_score": 0.0-1.0,
  "rdi_alignment": {{
    "strong_match": ["tech1", "tech2"],
    "partial_match": ["tech3"],
    "no_match": ["tech4"],
    "coverage_ratio": 0.0-1.0
  }},
  "maturity_profile": {{
    "emerging": 0,
    "established": 0,
    "mature": 0,
    "declining": 0
  }},
  "technology_risk_flags": ["risk description 1", "risk description 2"]
}}
"""

tech_intel_result = llm(tech_intel_prompt, json_mode=True)

# Validate and store
if isinstance(tech_intel_result.get("technology_stacks"), list):
    # Validate stack_coherence_score bounds
    raw_coherence = tech_intel_result.get("stack_coherence_score", 0.5)
    try:
        tech_intel_result["stack_coherence_score"] = max(0.0, min(1.0, float(raw_coherence)))
    except (ValueError, TypeError):
        tech_intel_result["stack_coherence_score"] = 0.5

    # Validate rdi_alignment structure
    rdi = tech_intel_result.get("rdi_alignment", {})
    tech_intel_result["rdi_alignment"] = {
        "strong_match": rdi.get("strong_match", []),
        "partial_match": rdi.get("partial_match", []),
        "no_match": rdi.get("no_match", []),
        "coverage_ratio": max(0.0, min(1.0, float(rdi.get("coverage_ratio", 0.0))))
    }

    # Validate maturity_profile
    mat = tech_intel_result.get("maturity_profile", {})
    tech_intel_result["maturity_profile"] = {
        "emerging": int(mat.get("emerging", 0)),
        "established": int(mat.get("established", 0)),
        "mature": int(mat.get("mature", 0)),
        "declining": int(mat.get("declining", 0))
    }

    # Default risk flags to empty list
    if not isinstance(tech_intel_result.get("technology_risk_flags"), list):
        tech_intel_result["technology_risk_flags"] = []

    # Default unversioned_technologies to empty list
    if not isinstance(tech_intel_result.get("unversioned_technologies"), list):
        tech_intel_result["unversioned_technologies"] = []

    rfp_summary["tech_intelligence"] = tech_intel_result

    stacks = tech_intel_result["technology_stacks"]
    rdi_align = tech_intel_result["rdi_alignment"]
    mat_prof = tech_intel_result["maturity_profile"]

    log(f"  Tech Intelligence: {len(stacks)} stacks, coherence {tech_intel_result['stack_coherence_score']:.2f}")
    for stack in stacks:
        log(f"    Stack: {stack.get('stack_name', '?')} ({len(stack.get('components', []))} components, coherence: {stack.get('coherence', '?')})")
        for comp in stack.get("components", [])[:5]:
            ver_str = f" v{comp['version']}" if comp.get("version") else ""
            log(f"      - {comp.get('name', '?')}{ver_str} [{comp.get('maturity', '?')}] ({comp.get('role', '?')})")
    log(f"    RDI Alignment: {len(rdi_align['strong_match'])} strong, {len(rdi_align['partial_match'])} partial, {len(rdi_align['no_match'])} no match (coverage: {rdi_align['coverage_ratio']:.0%})")
    log(f"    Maturity: {mat_prof['established']} established, {mat_prof['emerging']} emerging, {mat_prof['mature']} mature, {mat_prof['declining']} declining")
    if tech_intel_result["technology_risk_flags"]:
        log(f"    Risk Flags: {len(tech_intel_result['technology_risk_flags'])}")
        for flag in tech_intel_result["technology_risk_flags"]:
            log(f"      ! {flag}")
else:
    rfp_summary["tech_intelligence"] = {
        "technology_stacks": [],
        "unversioned_technologies": [],
        "stack_coherence_score": 0.0,
        "rdi_alignment": {"strong_match": [], "partial_match": [], "no_match": [], "coverage_ratio": 0.0},
        "maturity_profile": {"emerging": 0, "established": 0, "mature": 0, "declining": 0},
        "technology_risk_flags": ["Tech intelligence extraction failed"]
    }
    log("  Tech Intelligence: extraction failed -- using empty defaults")
```

### Step 5: Write Output

```python
import json

output_path = f"{folder}/screen/rfp-summary.json"
with open(output_path, "w") as f:
    json.dump(rfp_summary, f, indent=2)

output_size = os.path.getsize(output_path)
log(f"  Written: screen/rfp-summary.json ({output_size / 1024:.1f} KB)")

if output_size < 1024:
    log("  WARNING: rfp-summary.json is under 1KB -- extraction may be incomplete")
```

### Step 6: Report

```
RFP SUMMARY EXTRACTED (Phase 1)
================================
Client: {client_name or "Not identified"}
RFP: {rfp_number or "Not found"} -- {rfp_title or "Not found"}
Deadline: {submission_deadline or "Not found"}
Value: {estimated_value or "Not disclosed"}
Contract Type: {contract_type or "Not specified"}
Domain: {industry_domain or "Not classified"}
Set-Aside: {set_aside or "None detected"}
Scope Keywords: {', '.join(scope_keywords) or "None extracted"}
Required Technologies: {len(required_technologies)} extracted
Evaluation: {len(evaluation_criteria)} criteria, {len(evaluation_subfactors)} decomposed
Evaluation Model: {evaluation_model.get('technical_to_price_ratio', 'unknown')} tech:price, {len(evaluation_model.get('point_allocation', []))} criteria allocated
Client Tone: {client_tone.get('primary_style', 'unknown')} (confidence: {client_tone.get('confidence', 'low')}), {len(client_tone.get('mirroring_vocabulary', []))} mirroring terms
Tech Intelligence: {len(tech_intelligence.get('technology_stacks', []))} stacks, coherence {tech_intelligence.get('stack_coherence_score', 0):.2f}, RDI coverage {tech_intelligence.get('rdi_alignment', {}).get('coverage_ratio', 0):.0%}
  Risk Flags: {len(tech_intelligence.get('technology_risk_flags', []))}
Buyer Priorities: {len(buyer_priorities)} identified ({high_count} HIGH, {medium_count} MEDIUM)
Confidence: {extraction_confidence}
Missing Fields: {len(fields_not_found)}
Output: screen/rfp-summary.json
```

## Quality Checklist

- [ ] `rfp-summary.json` written (>1KB)
- [ ] Client name extraction attempted with multiple patterns
- [ ] Deadline extraction attempted
- [ ] Contract value/type extraction attempted
- [ ] Scope keywords identified from technology/service term list
- [ ] Evaluation criteria extracted if present
- [ ] Set-aside and preference restrictions checked
- [ ] Industry domain classified from indicator patterns
- [ ] Mandatory requirements and key deliverables captured
- [ ] Extraction confidence assessed based on critical fields
- [ ] Missing fields documented in `fields_not_found` array
- [ ] Required technologies extracted via LLM (specific named products/platforms, not generic terms)
- [ ] Evaluation subfactors decomposed per criterion (what evaluator actually assesses)
- [ ] Buyer priorities identified (3-6, each with importance HIGH/MEDIUM, signal evidence, linked keywords)
- [ ] linked_scope_keywords capped at 5 per buyer priority
- [ ] LLM prompts grounded in already-extracted regex data (scope_keywords, evaluation_criteria)

- [ ] `client_tone` present with primary_style, confidence, and 6+ mirroring vocabulary terms
- [ ] `client_tone` adaptation_rules include prefer_terms and avoid_terms arrays
- [ ] `client_tone` quantitative_signals populated (passive voice ratio, shall density, acronym density)
- [ ] `tech_intelligence` present with at least 1 technology stack and rdi_alignment
- [ ] `tech_intelligence` maturity_profile accounts for all extracted technologies
- [ ] `tech_intelligence` rdi_alignment cross-referenced against company-profile.json services
- [ ] `tech_intelligence` risk flags populated (or empty list if no risks)
- [ ] `evaluation_model` present with point_allocation array
- [ ] `evaluation_model` technical_to_price_ratio derived from criteria weights
- [ ] `evaluation_model` evaluator_persona matches evaluation method

### Skill Integration Quality Checks (procurement-analyst)
- [ ] Solicitation type classified (RFP/RFQ/IFB/BAA/IDIQ) with strategy implications
- [ ] Evaluation methodology identified via language markers (LPTA vs Best Value vs QBS)
- [ ] Contract type pricing strategy note populated (FFP/T&M/CPFF implications)
- [ ] **Anti-pattern check:** Not treating all factors equally, not conflating RFQ with RFP, not ignoring Section H special clauses

### Tone & Tech Intelligence Quality Checks
- [ ] Client tone detection used SINGLE LLM call (not multiple)
- [ ] Mirroring vocabulary contains client-specific terms (not generic procurement language)
- [ ] Technology stacks group related technologies (not one-tech-per-stack)
- [ ] Stack coherence assessed per-stack AND overall (stack_coherence_score)
- [ ] RDI alignment coverage_ratio is proportion of required techs matched (not binary)
