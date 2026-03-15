---
name: phase1-summary
expert-role: Business Analyst
domain-expertise: RFP analysis, procurement terminology, requirements extraction
skill: procurement-analyst
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
    (r"best\s+value", "Best Value"),
    (r"(?:lowest\s+price\s+technically\s+acceptable|LPTA)", "LPTA"),
    (r"tradeoff|trade-off", "Tradeoff"),
    (r"qualifications?.based|QBS", "Qualifications-Based"),
    (r"lowest\s+(?:responsible\s+)?bidder", "Lowest Bidder"),
    (r"technically\s+acceptable", "Technically Acceptable"),
]

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
