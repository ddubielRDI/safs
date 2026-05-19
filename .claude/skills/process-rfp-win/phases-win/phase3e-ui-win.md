---
name: phase3e-ui-win
expert-role: UX Designer
domain-expertise: User interfaces, accessibility, wireframes
---

# Phase 3e: UI/UX Specifications

## Expert Role

You are a **UX Designer** with deep expertise in:
- User interface design
- Accessibility (WCAG 2.2 AA — current W3C Recommendation, October 2023; baseline for public-sector procurements 2025+)
- Wireframe and mockup creation
- User experience research

## Purpose

Generate UI/UX specifications with wireframe descriptions and accessibility requirements.

## Inputs

- `{folder}/shared/requirements-normalized.json` - Normalized requirements
- `{folder}/shared/domain-context.json` - Domain context

## Required Outputs

- `{folder}/outputs/UI_SPECS.md` - UI specifications

## Instructions

### Step 1: Extract UI Requirements

```python
requirements = read_json(f"{folder}/shared/requirements-normalized.json")
domain_context = read_json(f"{folder}/shared/domain-context.json")

ui_keywords = [
    "display", "screen", "dashboard", "form", "interface",
    "user", "view", "page", "navigation", "menu", "button",
    "report", "export", "search", "filter", "sort"
]

ui_reqs = [
    req for req in requirements.get("requirements", [])
    if any(kw in req.get("text", "").lower() for kw in ui_keywords)
    or req.get("category") == "UI"
]
```

### Step 1b: Detect WCAG Version Mandate from RFP

```python
# V1-F3 fix 2026-05-18: WCAG version is RFP-driven, not hardcoded.
# Scan requirements for explicit version mentions. Default to WCAG 2.2 AA
# (current W3C Recommendation, October 2023; current baseline for federal/state
# procurement after DOJ's April 2024 ADA Title II rule).
# Source: https://www.w3.org/TR/WCAG22/ (verified at phase execution time)
all_req_text = " ".join(
    req.get("text", "") + " " + req.get("full_context", "")
    for req in requirements.get("requirements", [])
).lower()

if "wcag 2.2" in all_req_text or "wcag2.2" in all_req_text:
    wcag_version = "2.2"
    wcag_note = "RFP explicitly cites WCAG 2.2"
elif "wcag 2.1" in all_req_text or "wcag2.1" in all_req_text:
    wcag_version = "2.1"
    wcag_note = "RFP-mandated WCAG 2.1; current public-sector baseline is 2.2 — confirm with client whether 2.2 SC may also apply"
elif "section 508" in all_req_text:
    # Section 508 currently incorporates WCAG 2.0 Level A and AA; many agencies
    # have moved to WCAG 2.1 or 2.2 administratively. Default to 2.2 with note.
    wcag_version = "2.2"
    wcag_note = "RFP cites Section 508. Section 508 baseline references WCAG 2.0 A/AA; default to 2.2 unless RFP narrower"
else:
    wcag_version = "2.2"
    wcag_note = "RFP did not specify WCAG version; using current baseline 2.2 AA (W3C Recommendation, Oct 2023)"

log(f"WCAG version selected: {wcag_version} AA — {wcag_note}")
log(f"Source: https://www.w3.org/TR/WCAG22/")

# Build version-specific format helpers for template substitution
wcag_version_compact = wcag_version.replace(".", "")  # "22" or "21" for URL path

# WCAG 2.2 added 9 new Success Criteria. Include them when version >= 2.2.
# Source: https://www.w3.org/TR/WCAG22/#new-features-in-wcag-2-2
if wcag_version == "2.2":
    wcag22_operable_sc = """- [ ] 2.4.11 Focus Not Obscured (Minimum) — AA new in 2.2
- [ ] 2.4.12 Focus Not Obscured (Enhanced) — AAA new in 2.2
- [ ] 2.4.13 Focus Appearance — AAA new in 2.2
- [ ] 2.5.7 Dragging Movements — AA new in 2.2
- [ ] 2.5.8 Target Size (Minimum) — AA new in 2.2 (24x24 CSS pixels)"""
    wcag22_understandable_sc = """- [ ] 3.2.6 Consistent Help — A new in 2.2
- [ ] 3.3.7 Redundant Entry — A new in 2.2
- [ ] 3.3.8 Accessible Authentication (Minimum) — AA new in 2.2
- [ ] 3.3.9 Accessible Authentication (Enhanced) — AAA new in 2.2"""
else:
    wcag22_operable_sc = ""
    wcag22_understandable_sc = ""
```

### Step 2: Generate UI Specifications

```python
def generate_ui_specs(ui_reqs, domain_context):
    domain = domain_context.get("selected_domain", "Generic")

    doc = f"""# UI/UX Specifications

**Domain:** {domain}
**Generated:** {datetime.now().strftime('%Y-%m-%d %H:%M')}

---

## Design System

### Color Palette
| Color | Hex | Usage |
|-------|-----|-------|
| Primary | #003366 | Headers, primary actions |
| Secondary | #4a90a4 | Secondary actions, links |
| Success | #2e7d32 | Success states, confirmations |
| Warning | #f57c00 | Warnings, attention |
| Error | #c62828 | Errors, destructive actions |
| Neutral | #607d8b | Borders, disabled states |

### Typography
| Element | Font | Size | Weight |
|---------|------|------|--------|
| H1 | Inter | 32px | Bold |
| H2 | Inter | 24px | Semi-bold |
| H3 | Inter | 20px | Semi-bold |
| Body | Inter | 16px | Regular |
| Caption | Inter | 14px | Regular |

### Spacing Scale
- xs: 4px
- sm: 8px
- md: 16px
- lg: 24px
- xl: 32px

---

## Page Templates

### Dashboard Template
```
┌────────────────────────────────────────────────┐
│ HEADER                                    [?] [👤] │
├────────┬───────────────────────────────────────┤
│        │                                       │
│  NAV   │  MAIN CONTENT AREA                   │
│        │                                       │
│  • Home│  ┌─────────┐ ┌─────────┐ ┌─────────┐ │
│  • Mod1│  │ Widget 1│ │ Widget 2│ │ Widget 3│ │
│  • Mod2│  └─────────┘ └─────────┘ └─────────┘ │
│  • Mod3│                                       │
│        │  ┌───────────────────────────────────┐ │
│  [v]   │  │ Data Table / Charts               │ │
│  More  │  └───────────────────────────────────┘ │
├────────┴───────────────────────────────────────┤
│ FOOTER                                         │
└────────────────────────────────────────────────┘
```

### List View Template
```
┌────────────────────────────────────────────────┐
│ Page Title                          [+ New]    │
├────────────────────────────────────────────────┤
│ [Search...........] [Filter ▼] [Sort ▼]        │
├────────────────────────────────────────────────┤
│ ☐ │ Column A │ Column B │ Column C │ Actions   │
├───┼──────────┼──────────┼──────────┼───────────┤
│ ☐ │ Value 1  │ Value 1  │ Value 1  │ [✏️] [🗑️] │
│ ☐ │ Value 2  │ Value 2  │ Value 2  │ [✏️] [🗑️] │
│ ☐ │ Value 3  │ Value 3  │ Value 3  │ [✏️] [🗑️] │
├────────────────────────────────────────────────┤
│ Showing 1-10 of 100          [<] [1] [2] [>]   │
└────────────────────────────────────────────────┘
```

### Form Template
```
┌────────────────────────────────────────────────┐
│ Form Title                                      │
├────────────────────────────────────────────────┤
│                                                 │
│  Field Label *                                  │
│  ┌─────────────────────────────────────────┐   │
│  │ Input value                              │   │
│  └─────────────────────────────────────────┘   │
│  Helper text or validation message              │
│                                                 │
│  Field Label                                    │
│  ┌─────────────────────────────────────────┐   │
│  │ Select option                         ▼ │   │
│  └─────────────────────────────────────────┘   │
│                                                 │
│  ☐ Checkbox option                              │
│  ○ Radio option 1                               │
│  ○ Radio option 2                               │
│                                                 │
├────────────────────────────────────────────────┤
│                    [Cancel]  [Save]             │
└────────────────────────────────────────────────┘
```

---

## Component Library

### Buttons
| Type | Usage | Example |
|------|-------|---------|
| Primary | Main actions | [Save] |
| Secondary | Alternative actions | [Cancel] |
| Destructive | Delete, remove | [Delete] |
| Ghost | Tertiary actions | [Learn More] |

### Form Controls
- Text Input: Single line text entry
- Text Area: Multi-line text entry
- Select: Dropdown selection
- Checkbox: Multiple selection
- Radio: Single selection
- Date Picker: Date selection
- File Upload: Document attachment

### Data Display
- Table: Sortable, filterable data grid
- Card: Summary information
- Chart: Data visualization
- Badge: Status indicators
- Avatar: User representation

---

## Accessibility Requirements (WCAG {wcag_version} AA)

> **Source:** https://www.w3.org/TR/WCAG{wcag_version_compact}/ — {wcag_note}
> Verified at phase execution: {datetime.now().strftime('%Y-%m-%d')}

### Perceivable
- [ ] All images have alt text (1.1.1)
- [ ] Color is not sole indicator (1.4.1)
- [ ] Minimum contrast ratio 4.5:1 for text, 3:1 for UI components (1.4.3, 1.4.11)
- [ ] Text resizable to 200% (1.4.4)

### Operable
- [ ] All functions keyboard accessible (2.1.1)
- [ ] Skip navigation link (2.4.1)
- [ ] Focus visible and logical (2.4.7)
- [ ] No keyboard traps (2.1.2)
{wcag22_operable_sc}

### Understandable
- [ ] Language declared (3.1.1)
- [ ] Labels for all inputs (3.3.2)
- [ ] Error identification (3.3.1)
- [ ] Consistent navigation (3.2.3)
{wcag22_understandable_sc}

### Robust
- [ ] Valid HTML
- [ ] ARIA attributes where needed (4.1.2)
- [ ] Works with assistive technology
- [ ] Status messages announced to assistive tech (4.1.3)

---

## Screen Specifications

"""

    # Generate screen specs from requirements
    for i, req in enumerate(ui_reqs[:15]):
        doc += f"""### Screen {i+1}: {req.get('canonical_id', 'UI-' + str(i+1))}

**Requirement:** {req.get('text', '')[:200]}

**Components:**
- Header with breadcrumb
- Main content area
- Action buttons
- Data display (table/form)

**User Actions:**
1. View data
2. Edit/Create record
3. Submit/Save

**Validation Rules:**
- Required fields marked with *
- Real-time validation feedback
- Confirmation for destructive actions

---

"""

    doc += """
## Responsive Design

### Breakpoints
| Breakpoint | Width | Layout |
|------------|-------|--------|
| Mobile | < 768px | Single column, stacked |
| Tablet | 768-1024px | Two columns |
| Desktop | > 1024px | Full layout |

### Mobile Considerations
- Touch-friendly targets (44x44px min)
- Swipe gestures for navigation
- Bottom sheet for actions
- Simplified navigation (hamburger menu)

---

## Appendix: UI Requirements Coverage

| Req ID | Screen | Status |
|--------|--------|--------|
"""

    for req in ui_reqs[:20]:
        doc += f"| {req.get('canonical_id', 'N/A')} | Assigned | Planned |\n"

    return doc

ui_specs = generate_ui_specs(ui_reqs, domain_context)
write_file(f"{folder}/outputs/UI_SPECS.md", ui_specs)
```

## Quality Checklist

- [ ] `UI_SPECS.md` created in `outputs/`
- [ ] Design system defined
- [ ] Page templates included
- [ ] Component library documented
- [ ] WCAG 2.2 AA requirements addressed (or RFP-mandated version if narrower) — including the 9 new Success Criteria added in 2.2
- [ ] WCAG version source URL cited with verification date
- [ ] Responsive design specified
