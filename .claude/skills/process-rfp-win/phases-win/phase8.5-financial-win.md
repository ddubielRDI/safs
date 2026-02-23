---
name: phase8.5-financial-win
expert-role: Financial Analyst
domain-expertise: Cost proposal development, labor rate structuring, effort-based pricing, government cost narratives
---

# Phase 8.5: Financial Proposal

## Expert Role

You are a **Financial Analyst** with expertise in:
- Government cost proposal development
- Labor rate structuring and justification
- Effort-based pricing models
- Cost narrative writing that demonstrates value

## Purpose

Generate the Financial Proposal / Cost Proposal volume. This combines effort estimates from Phase 5 with labor rates and cost structure from the company profile. User-provided rates are used where available; placeholders are inserted where rates need to be filled in.

## Inputs

- `{folder}/shared/effort-estimation.json` - Effort estimates by phase/role
- `{folder}/shared/bid-context-bundle.json` - Context
- `{folder}/shared/EVALUATION_CRITERIA.json` - How cost is weighted
- `{folder}/shared/domain-context.json` - Domain context
- `config-win/company-profile.json` - Labor rates, overhead, profit margin

## Required Output

- `{folder}/outputs/bid-sections/05_FINANCIAL.md` (>5KB)

## Instructions

### Step 1: Load Financial Data with Market Rate Fallback

```python
effort = read_json_safe(f"{folder}/shared/effort-estimation.json")
context = read_json(f"{folder}/shared/bid-context-bundle.json")
evaluation = read_json(f"{folder}/shared/EVALUATION_CRITERIA.json")
domain = read_json(f"{folder}/shared/domain-context.json")
company = read_json("config-win/company-profile.json")

# Primary: Company-specific rates (user-populated)
company_rates = company.get("financial_defaults", {}).get("labor_rates", {})
company_overhead = company.get("financial_defaults", {}).get("overhead_rate_percent", 0)
company_profit = company.get("financial_defaults", {}).get("profit_margin_percent", 0)

# Fallback: Market rate defaults (baseline benchmarks)
market_defaults = company.get("market_rate_defaults", {})
market_rates = market_defaults.get("labor_rates", {})
market_overhead = market_defaults.get("overhead_rate_percent", 15)
market_profit = market_defaults.get("profit_margin_percent", 10)

# Resolve rates: company rates > $0 override market defaults
# Normalize role names to prevent mismatches (e.g., "Project Manager" vs "project_manager")
def normalize_role(name):
    return name.strip().lower().replace("_", " ")

# Build normalized lookup dicts
company_norm = {normalize_role(k): (k, v) for k, v in company_rates.items()}
market_norm = {normalize_role(k): (k, v) for k, v in market_rates.items()}
all_roles_norm = set(list(company_norm.keys()) + list(market_norm.keys()))

labor_rates = {}  # role -> {"hourly_rate": X, "source": "COMPANY RATE" | "MARKET DEFAULT"}
for norm_role in all_roles_norm:
    # Use the display name from whichever dict has it (prefer company name)
    display_name = company_norm[norm_role][0] if norm_role in company_norm else market_norm[norm_role][0]
    company_rate = company_norm[norm_role][1] if norm_role in company_norm else {}
    c_rate = company_rate.get("hourly_rate", 0) if isinstance(company_rate, dict) else company_rate
    market_rate = market_norm[norm_role][1] if norm_role in market_norm else {}
    m_rate = market_rate.get("hourly_rate", 0) if isinstance(market_rate, dict) else market_rate
    gsa_range = market_rate.get("gsa_range", "") if isinstance(market_rate, dict) else ""
    role = display_name  # Use original casing for display

    if c_rate > 0:
        labor_rates[role] = {"hourly_rate": c_rate, "source": "COMPANY RATE", "gsa_range": gsa_range}
    elif m_rate > 0:
        labor_rates[role] = {"hourly_rate": m_rate, "source": "MARKET DEFAULT", "gsa_range": gsa_range}
    else:
        labor_rates[role] = {"hourly_rate": 0, "source": "UNPOPULATED", "gsa_range": ""}

# Resolve overhead and profit (company overrides market)
overhead_pct = company_overhead if company_overhead > 0 else market_overhead
profit_pct = company_profit if company_profit > 0 else market_profit
overhead_source = "COMPANY RATE" if company_overhead > 0 else "MARKET DEFAULT"
profit_source = "COMPANY RATE" if company_profit > 0 else "MARKET DEFAULT"

# Track rate sources for attribution
rate_sources = {
    "company_rates_used": sum(1 for r in labor_rates.values() if r["source"] == "COMPANY RATE"),
    "market_defaults_used": sum(1 for r in labor_rates.values() if r["source"] == "MARKET DEFAULT"),
    "unpopulated": sum(1 for r in labor_rates.values() if r["source"] == "UNPOPULATED"),
    "market_rate_date": market_defaults.get("last_updated", "Unknown"),
    "market_rate_source": market_defaults.get("rate_source", "Industry benchmarks")
}
```

### Step 2: Build Cost Model from Effort Estimates

```python
# If effort estimation exists, build cost breakdown
if effort:
    phases = effort.get("phases", effort.get("work_packages", []))
    total_hours = effort.get("total_hours", effort.get("summary", {}).get("total_hours", 0))
else:
    phases = []
    total_hours = 0
    # Insert placeholder noting that effort estimation was not available

# Build role-based cost table
roles = list(labor_rates.keys())
role_hours = {}  # role -> total hours

# Extract per-role hours from effort estimation
if effort:
    for phase in phases:
        role_breakdown = phase.get("roles", phase.get("labor", {}))
        if isinstance(role_breakdown, dict):
            for role, hours in role_breakdown.items():
                h = hours if isinstance(hours, (int, float)) else hours.get("hours", 0)
                role_hours[role] = role_hours.get(role, 0) + h

# Calculate extended costs per role
role_costs = {}
labor_total = 0
for role in roles:
    hours = role_hours.get(role, 0)
    rate_info = labor_rates[role]
    rate = rate_info["hourly_rate"]
    extended = hours * rate
    role_costs[role] = {
        "hours": hours,
        "rate": rate,
        "source": rate_info["source"],
        "gsa_range": rate_info["gsa_range"],
        "extended": extended
    }
    labor_total += extended

# Calculate totals
overhead = labor_total * (overhead_pct / 100)
odc = 0  # Other Direct Costs — [USER INPUT REQUIRED]
subtotal = labor_total + overhead + odc
profit = subtotal * (profit_pct / 100)
total = subtotal + profit
```

### Step 3: Generate Financial Proposal

```markdown
# Financial Proposal

## 1. Cost Summary

### 1.1 Total Cost Overview

| Category | Amount |
|----------|--------|
| Direct Labor | ${labor_total} |
| Overhead ({overhead_pct}%) | ${overhead} |
| Other Direct Costs | ${odc} |
| Subtotal | ${subtotal} |
| Profit ({profit_pct}%) | ${profit} |
| **Total Proposed Cost** | **${total}** |

> [USER INPUT REQUIRED: Verify all rates and totals before submission.
> Rates in company-profile.json must be populated with actual values.]

### 1.2 Cost Evaluation Alignment
[How the proposed cost represents best value.
Reference evaluation criteria: cost is weighted at XX points/XX%.
Emphasize value over lowest price if applicable.]

## 2. Labor Rate Schedule

| Role | Hourly Rate | Rate Source | GSA Range | Estimated Hours | Extended Cost |
|------|-------------|-------------|-----------|-----------------|---------------|
{for each role in role_costs: role_name | ${rate} | {source} | {gsa_range} | {hours} | ${extended}}
| **Total Direct Labor** | | | | **{total_hours}** | **${labor_total}** |

**Rate Source Legend:**
- **COMPANY RATE**: Rate set by Resource Data in company-profile.json (authoritative)
- **MARKET DEFAULT**: Baseline rate from GSA IT Schedule 70 / industry benchmarks (last updated: {market_rate_date}). [USER INPUT REQUIRED: Review and adjust market default rates before submission]
- **UNPOPULATED**: No rate available — must be populated before submission

> **Rate Sources:** {rate_sources["company_rates_used"]} company rates, {rate_sources["market_defaults_used"]} market defaults, {rate_sources["unpopulated"]} unpopulated
> **Market Rate Benchmark:** {rate_sources["market_rate_source"]} (as of {rate_sources["market_rate_date"]})

## 3. Cost Breakdown by Phase

| Phase | Hours | Labor Cost | % of Total |
|-------|-------|------------|------------|
{for each phase: hours + cost + percentage}

## 4. Staffing Plan Cost Impact

### 4.1 Role Allocation
[Table showing how roles are allocated across project phases.
Connect to staffing plan in Management Proposal.]

### 4.2 Key Assumptions
- [Rate assumptions (e.g., blended vs. specific rates)]
- [Travel assumptions]
- [License and tool costs]
- [Subcontractor costs if applicable]
- [Escalation assumptions for multi-year contracts]

## 5. Value Proposition

### 5.1 Cost Evaluation Alignment

```python
# Reference evaluation criteria to show cost alignment
eval_factors = evaluation.get("evaluation_factors", evaluation.get("factors", []))
cost_factor = next((f for f in eval_factors if any(kw in f.get("name", "").lower() for kw in ["cost", "price", "financial", "budget"])), None)
if cost_factor:
    cost_weight = cost_factor.get("weight_normalized", cost_factor.get("points", ""))
    # Write narrative positioning cost against value
```

[If cost is evaluated (LPTA or weighted), explain pricing strategy:
- **LPTA:** Emphasize competitive rates within GSA range, minimal overhead
- **Best Value:** Emphasize ROI, reduced risk, efficiency gains that justify investment
- Reference evaluation weight: "Cost represents {cost_weight}% of the evaluation score.
  Our pricing strategy optimizes for {LPTA: lowest compliant cost / Best Value: best return on investment}."]

### 5.2 Cost Efficiency
[Explain how Resource Data delivers value:
- Experienced team = less ramp-up time
- Reusable frameworks and accelerators
- Multi-office resource flexibility
- AI-assisted development (reference Phase 5 AI savings if available):
  {effort.get("ai_savings_hours", 0)} hours saved through AI-assisted development]

### 5.3 Total Cost of Ownership
[Look beyond implementation: maintenance, support, training costs.
Show long-term value of the proposed solution.
Include 3-year and 5-year TCO comparison if multi-year contract.]

## 6. Payment Schedule

```python
# Tie payment milestones to Phase 5 estimation phases
if effort:
    phases_list = effort.get("phases", effort.get("work_packages", []))
    # Generate milestone table from estimation phases
    milestones = []
    cumulative_pct = 0
    for i, phase in enumerate(phases_list):
        phase_hours = sum(h if isinstance(h, (int, float)) else h.get("hours", 0)
                         for h in phase.get("roles", phase.get("labor", {})).values())
        phase_pct = round((phase_hours / max(total_hours, 1)) * 100)
        cumulative_pct += phase_pct
        milestones.append({
            "milestone": f"Milestone {i+1}: {phase.get('name', phase.get('phase', ''))}",
            "deliverable": phase.get("deliverables", ["Phase completion"])[0] if phase.get("deliverables") else "Phase completion",
            "percentage": f"{phase_pct}%",
            "cumulative": f"{min(cumulative_pct, 100)}%"
        })
```

| Milestone | Key Deliverable | Payment % | Cumulative % |
|-----------|----------------|-----------|--------------|
{for each milestone: milestone | deliverable | percentage | cumulative}
| **Project Completion** | Final acceptance | **Remaining** | **100%** |

> [USER INPUT REQUIRED: Payment terms, milestone dates, and percentages need confirmation.
> The schedule above is auto-generated from effort estimation phases as a starting point.]
```

### Step 4: Flag Rate Sources and Required Actions

```python
# Categorize rates by source
unpopulated = [role for role, info in labor_rates.items() if info["source"] == "UNPOPULATED"]
market_defaulted = [role for role, info in labor_rates.items() if info["source"] == "MARKET DEFAULT"]
company_set = [role for role, info in labor_rates.items() if info["source"] == "COMPANY RATE"]

# Build status header for top of document
if unpopulated:
    warning = f"""
> **ACTION REQUIRED**: The following labor rates have NO rate (company or market):
> {', '.join(unpopulated)}
> These roles show $0 in all cost calculations. Populate rates in
> `config-win/company-profile.json` financial_defaults.labor_rates before submission.
"""

if market_defaulted:
    notice = f"""
> **REVIEW REQUIRED**: The following roles use MARKET DEFAULT rates (not company-specific):
> {', '.join(market_defaulted)}
> Market defaults are based on {rate_sources['market_rate_source']} (updated {rate_sources['market_rate_date']}).
> Review and adjust in `config-win/company-profile.json` financial_defaults.labor_rates
> to override with company-specific rates before final submission.
"""

if company_set:
    confirmation = f"""
> **CONFIRMED**: {len(company_set)} role(s) using company-specific rates: {', '.join(company_set)}
"""
```

### Step 5: Write Output

```python
write_file(f"{folder}/outputs/bid-sections/05_FINANCIAL.md", financial_content)

log(f"""
💰 FINANCIAL PROPOSAL COMPLETE (Phase 8.5)
============================================
Total Hours: {total_hours}
Total Proposed Cost: ${total:,.2f}
  Direct Labor: ${labor_total:,.2f}
  Overhead ({overhead_pct}%): ${overhead:,.2f}
  Profit ({profit_pct}%): ${profit:,.2f}
Roles: {len(roles)}
Rate Sources: {rate_sources['company_rates_used']} company, {rate_sources['market_defaults_used']} market default, {rate_sources['unpopulated']} unpopulated
Payment Milestones: {len(milestones) if effort else 0}
User Input Markers: {financial_content.count("[USER INPUT REQUIRED")}

Output: outputs/bid-sections/05_FINANCIAL.md
""")
```

## Quality Checklist

- [ ] `05_FINANCIAL.md` created (>5KB)
- [ ] Cost summary table with totals (labor + overhead + profit)
- [ ] Labor rate schedule with Rate Source column (COMPANY RATE / MARKET DEFAULT / UNPOPULATED)
- [ ] GSA Range column for market context
- [ ] All rates > $0 (no UNPOPULATED roles — if any remain, flag prominently)
- [ ] Rate source attribution: company rates override market defaults
- [ ] Cost breakdown by phase from effort estimation
- [ ] Key assumptions documented
- [ ] Cost evaluation alignment section (LPTA vs Best Value strategy)
- [ ] Value proposition connecting cost to AI-assisted efficiency
- [ ] Total cost of ownership (3-year / 5-year if multi-year contract)
- [ ] Payment schedule auto-generated from Phase 5 milestones
- [ ] Market default rates flagged with REVIEW REQUIRED markers
- [ ] All unpopulated rates flagged with ACTION REQUIRED markers
