# SASQUATCH Modular Architecture Guide

## Overview

SASQUATCH is designed with a modular architecture to support independent development and deployment of each demo section. Per the RFP, each of the three work sections may be awarded to different vendors, requiring clean separation and well-defined interfaces.

## Solution Structure

```
Sasquatch.sln
├── Sasquatch.Core/              # Shared class library (all sections depend on this)
│   ├── Models/
│   │   ├── Shared/              # District, School, Esd, User, AuditLog, DataLock, EditRule
│   │   ├── Collection/          # EnrollmentSubmission, EnrollmentData, EnrollmentEdit, Budget*
│   │   ├── Calculation/         # Scenario, StateConstant, ApportionmentResult
│   │   └── Reporting/           # ReportDefinition, ReportExecution
│   ├── Data/
│   │   └── SasquatchDbContext   # Single shared Entity Framework context
│   └── Interfaces/              # Service contracts for cross-section compatibility
│
├── Sasquatch.Collection/        # Section 1: Data Collection
│   ├── Controllers/
│   │   ├── EnrollmentController [Area("Collection")]
│   │   └── AdminController      [Area("Collection")]
│   └── ViewModels/
│
├── Sasquatch.Calculation/       # Section 2: Data Calculation
│   ├── Controllers/
│   │   ├── ProductionController [Area("Calculation")]
│   │   └── SandboxController    [Area("Calculation")]
│   └── Services/
│
├── Sasquatch.Reporting/         # Section 3: Data Reporting
│   ├── Controllers/
│   │   ├── ReportsController    [Area("Reporting")]
│   │   └── ApiController        [Area("Reporting")]
│   └── Services/
│
└── Sasquatch/                   # Unified Web Host
    ├── Program.cs               # Composes sections via AddApplicationPart()
    ├── Areas/
    │   ├── Collection/Views/    # Section 1 views
    │   ├── Calculation/Views/   # Section 2 views
    │   └── Reporting/Views/     # Section 3 views
    ├── Views/                   # Shared views (Home, Layout, Error)
    └── wwwroot/                 # Static assets
```

## Design Principles

### 1. Single Shared DbContext
All domain models reside in `Sasquatch.Core` with a single `SasquatchDbContext`. This avoids complex multi-context migrations while still allowing independent section development.

### 2. Interface-Driven Architecture
Each section defines and implements interfaces in `Sasquatch.Core/Interfaces/`:
- **Collection:** `IEnrollmentService`, `IBudgetService`, `IValidationEngine`, `IDataLockService`
- **Calculation:** `IApportionmentEngine`, `IScenarioService`
- **Reporting:** `IReportService`, `IExportService`, `IApiDataService`
- **Shared:** `IAuditService`, `IReferenceDataService`, `IUserService`

This ensures vendor compatibility - different vendors implementing the same interfaces will produce interoperable sections.

### 3. Area-Based Routing
Controllers use `[Area("SectionName")]` attributes, and views are organized in `Areas/{SectionName}/Views/`. This provides clean URL separation:
- `/Collection/Enrollment/...`
- `/Calculation/Production/...`
- `/Reporting/Reports/...`

### 4. Composition via AddApplicationPart
The unified web host composes sections at startup:

```csharp
builder.Services.AddControllersWithViews()
    .AddApplicationPart(typeof(Sasquatch.Collection.CollectionMarker).Assembly)
    .AddApplicationPart(typeof(Sasquatch.Calculation.CalculationMarker).Assembly)
    .AddApplicationPart(typeof(Sasquatch.Reporting.ReportingMarker).Assembly);
```

Each section project includes a marker class for assembly discovery.

## Building & Running

### Full Solution
```bash
dotnet build "SAFS.sln"
dotnet run --project Sasquatch
```

### Individual Section (for isolated development)
```bash
dotnet build Sasquatch.Core
dotnet build Sasquatch.Collection
```

## Vendor Handoff Considerations

When handing off a section to a different vendor:

1. **Provide:**
   - `Sasquatch.Core` project (shared models and interfaces)
   - Section-specific project (e.g., `Sasquatch.Collection`)
   - SQL scripts for database setup
   - Section-specific documentation

2. **The vendor can:**
   - Implement interfaces defined in Core
   - Extend the section's controllers and services
   - Add section-specific views
   - Run standalone for development (with Core dependency)

3. **Integration:**
   - Vendor delivers compiled DLL
   - Host application includes via AddApplicationPart
   - No source code sharing required beyond interfaces

## Database Schema Organization

SQL scripts are organized to support incremental deployment:
- `00_create_database.sql` - Database creation
- `01_create_schema.sql` - All table definitions
- `02_seed_reference_data.sql` - Districts, Schools, ESDs, Users
- `03_load_tumwater_enrollment.sql` - Demo enrollment data

Future scripts may be added per-section:
- `04_seed_calculation_data.sql` - State constants, scenarios
- `05_seed_reporting_data.sql` - Report definitions

## Data Organization

```
data/
├── shared/              # Reference data (districts, schools, ESDs)
├── collection/          # Sample enrollment/budget CSVs for upload testing
├── calculation/         # State constants, scenario templates
└── reporting/           # Report templates, sample outputs
```

## Adding a New Section

1. Create class library: `Sasquatch.{SectionName}/`
2. Add reference to `Sasquatch.Core`
3. Create marker class: `{SectionName}Marker.cs`
4. Create controllers with `[Area("{SectionName}")]`
5. Add to solution and Sasquatch.csproj references
6. Register in Program.cs via AddApplicationPart
7. Create views in `Sasquatch/Areas/{SectionName}/Views/`

---

*Architecture Version: 1.0*
*Last Updated: 2026-01-09*
