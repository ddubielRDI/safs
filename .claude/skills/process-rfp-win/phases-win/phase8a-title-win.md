---
name: phase8a-title-win
expert-role: Proposal Writer
domain-expertise: Executive summaries, persuasive writing
---

# Phase 8a: Title + Executive Summary

## Expert Role

You are a **Proposal Writer** with deep expertise in:
- Executive summary writing
- Persuasive technical writing
- Proposal structure
- Value proposition communication

## Purpose

Generate title page and executive summary section for the bid.

## Inputs

- `{folder}/shared/bid/POSITIONING_OUTPUT.json`
- `{folder}/shared/bid/CLIENT_INTELLIGENCE.json`
- `{folder}/shared/requirements-normalized.json`
- `{folder}/shared/domain-context.json`

## Required Outputs

- `{folder}/outputs/title-page.md`

## Instructions

### Step 1: Create Output Directory

```bash
# No directory creation needed - outputs/ already exists
```

### Step 2: Load Data

```python
positioning = read_json(f"{folder}/shared/bid/POSITIONING_OUTPUT.json")
client_intel = read_json(f"{folder}/shared/bid/CLIENT_INTELLIGENCE.json")
requirements = read_json(f"{folder}/shared/requirements-normalized.json")
domain_context = read_json(f"{folder}/shared/domain-context.json")
```

### Step 3: Generate Title Page

```python
def generate_title_page(client_intel, positioning, domain):
    """Generate proposal title page."""
    client_name = client_intel.get("client_info", {}).get("organization_name", "[Client Name]")
    project_name = f"{domain.title()} Management Solution"

    title_page = f"""# Technical Proposal

## {project_name}

**Submitted to:**
{client_name}

**Submitted by:**
[Company Name]
[Address Line 1]
[Address Line 2]
[City, State ZIP]

**Date:** {datetime.now().strftime('%B %d, %Y')}

**RFP Reference:** [RFP Number]

---

**Primary Contact:**
[Contact Name]
[Title]
[Email]
[Phone]

---

*This proposal contains confidential and proprietary information.*

---

"""

    return title_page

title_page = generate_title_page(client_intel, positioning, domain_context.get("selected_domain", "Technology"))
```

### Step 4: Generate Executive Summary

```python
def generate_executive_summary(positioning, client_intel, requirements, domain):
    """Generate executive summary section."""
    client_name = client_intel.get("client_info", {}).get("organization_name", "your organization")
    req_count = len(requirements.get("requirements", []))
    value_prop = positioning.get("core_positioning", {}).get("value_proposition", "")
    themes = positioning.get("core_positioning", {}).get("themes", [])
    differentiators = positioning.get("core_positioning", {}).get("key_differentiators", [])

    exec_summary = f"""
## Executive Summary

### Understanding Your Needs

We are pleased to submit this proposal in response to {client_name}'s Request for Proposal for a comprehensive {domain} solution. We have thoroughly analyzed your requirements—identifying **{req_count} distinct requirements**—and have designed a solution that addresses each one while delivering exceptional value.

### Our Value Proposition

{value_prop}

### Key Themes

"""

    for theme in themes:
        exec_summary += f"**{theme}:** We prioritize {theme.lower()} in every aspect of our solution design and delivery.\n\n"

    exec_summary += f"""
### Why Choose Us

Our proposal stands out for several key reasons:

"""

    for i, diff in enumerate(differentiators[:4], 1):
        exec_summary += f"""**{i}. {diff.get('differentiator', 'Key Advantage')}**
{diff.get('evidence', 'Demonstrated capability in this area.')}

"""

    # Add key metrics
    exec_summary += f"""
### Solution Highlights

| Aspect | Our Approach |
|--------|--------------|
| Requirements Coverage | 100% of {req_count} requirements addressed |
| Technology | Modern cloud-native architecture |
| Compliance | Full {domain}-specific compliance |
| Timeline | Aggressive yet achievable delivery |
| Support | Dedicated team with domain expertise |

### Commitment to Success

We are committed to not just meeting, but exceeding your expectations. Our team brings deep {domain} expertise, proven methodologies, and a genuine passion for delivering solutions that make a difference.

We welcome the opportunity to discuss our proposal and demonstrate how we can help {client_name} achieve its goals.

"""

    return exec_summary

exec_summary = generate_executive_summary(
    positioning,
    client_intel,
    requirements,
    domain_context.get("selected_domain", "technology")
)
```

### Step 5: Combine and Write Output

```python
# Combine sections
full_content = title_page + exec_summary

# Write to file
write_file(f"{folder}/outputs/title-page.md", full_content)
```

### Step 6: Report Results

```python
log(f"""
📝 Title Page + Executive Summary Generated
==========================================
Client: {client_intel.get("client_info", {}).get("organization_name", "N/A")}
Domain: {domain_context.get("selected_domain")}
Requirements Covered: {len(requirements.get("requirements", []))}

Themes:
{chr(10).join(f"  • {t}" for t in positioning.get("core_positioning", {}).get("themes", []))}

Output: {folder}/outputs/title-page.md
""")
```

## Quality Checklist

- [ ] `title-page.md` created in `outputs/` (NOT in bid/)
- [ ] Title page with client name
- [ ] Executive summary with value proposition
- [ ] Key themes highlighted
- [ ] Differentiators listed
- [ ] Requirements count included

**⚠️ REMINDER: ALL MD files go in `outputs/`, NOT `outputs/bid/`**
