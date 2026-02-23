---
name: phase3e-ui-win
expert-role: UX Designer
domain-expertise: User interfaces, accessibility, wireframes
---

# Phase 3e: UI/UX Specifications

## Expert Role

You are a **UX Designer** with deep expertise in:
- User interface design
- Accessibility (WCAG 2.1)
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

## Accessibility Requirements (WCAG 2.1 AA)

### Perceivable
- [ ] All images have alt text
- [ ] Color is not sole indicator
- [ ] Minimum contrast ratio 4.5:1
- [ ] Text resizable to 200%

### Operable
- [ ] All functions keyboard accessible
- [ ] Skip navigation link
- [ ] Focus visible and logical
- [ ] No keyboard traps

### Understandable
- [ ] Language declared
- [ ] Labels for all inputs
- [ ] Error identification
- [ ] Consistent navigation

### Robust
- [ ] Valid HTML
- [ ] ARIA attributes where needed
- [ ] Works with assistive technology

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
- [ ] WCAG 2.1 AA requirements addressed
- [ ] Responsive design specified
