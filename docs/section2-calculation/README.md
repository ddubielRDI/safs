# Section 2: Data Calculation

## Overview
Apportionment calculation engine with Production and Sandbox environments, supporting what-if scenarios for OSPI, districts, and legislature.

## Implementation Status
- **Production Dashboard:** Placeholder
- **Calculation Engine:** Placeholder
- **OSPI Adjustments:** Placeholder
- **Sandbox Scenarios:** Placeholder
- **Scenario Comparison:** Placeholder

## Key Components

### Controllers
- `ProductionController` - Live apportionment calculations
- `SandboxController` - What-if scenario management

### Models (in Sasquatch.Core)
- `StateConstant` - State-level calculation constants
- `Scenario` - Sandbox scenario metadata
- `ApportionmentResult` - Calculation results

### Views
- `/Calculation/Production/Index` - Production dashboard
- `/Calculation/Production/Results` - Calculation results
- `/Calculation/Sandbox/Index` - Scenario list
- `/Calculation/Sandbox/Create` - New scenario
- `/Calculation/Sandbox/Compare` - Scenario comparison

## Demo Script

### Duration: 25-30 minutes

1. **Production Overview (3 min)**
   - Navigate to `/Calculation/Production`
   - Show state constants
   - Display current calculation status

2. **Run Calculation (5 min)**
   - Select Tumwater district
   - Execute apportionment calculation
   - Display results breakdown
   - Highlight sub-second execution

3. **OSPI Adjustment (5 min)**
   - Modify Tumwater data
   - Enter adjustment reason
   - Re-run calculation
   - View audit trail

4. **Create Sandbox Scenario (5 min)**
   - Navigate to `/Calculation/Sandbox`
   - Create new scenario
   - Copy production data
   - Name: "FY25 Budget Proposal"

5. **Modify & Compare (5 min)**
   - Increase base allocation 5%
   - Run sandbox calculation
   - Compare to production
   - Export comparison

## State Constants (Demo Values)

| Constant | Value | Description |
|----------|-------|-------------|
| Base Allocation | $5,500 | Per-pupil funding base |
| K-3 Factor | 1.2 | Class size enhancement |
| Special Ed Weight | 0.9309 | Special education weight |
| Running Start Rate | 0.85 | Running Start funding rate |
