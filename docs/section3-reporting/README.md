# Section 3: Data Reporting

## Overview
Report generation capabilities including monthly enrollment (P-223), annual budget (F-195), financial statements (F-196), Excel exports, and API integration.

## Implementation Status
- **Report Dashboard:** Placeholder
- **Enrollment Reports:** Placeholder
- **Budget Reports:** Placeholder
- **Excel Export:** Placeholder
- **REST API:** Placeholder endpoints

## Key Components

### Controllers
- `ReportsController` - Report generation and viewing
- `ApiController` - REST API for external integrations

### Models (in Sasquatch.Core)
- `ReportDefinition` - Report configuration
- `ReportExecution` - Execution history

### Views
- `/Reporting/Reports/Index` - Report dashboard
- `/Reporting/Reports/Enrollment` - P-223 reports
- `/Reporting/Reports/Budget` - F-195 reports
- `/Reporting/Reports/FinancialStatement` - F-196 reports

### API Endpoints
- `GET /api/enrollment/{districtCode}` - Get enrollment data
- `GET /api/apportionment/{districtCode}` - Get calculation results
- `GET /api/districts` - List districts

## Demo Script

### Duration: 20-25 minutes

1. **Monthly Enrollment Report (5 min)**
   - Navigate to `/Reporting/Reports`
   - Select P-223 report
   - Filter for Tumwater, October 2024
   - Generate and view report
   - Export to PDF

2. **Annual Budget Report (5 min)**
   - Select F-195 report
   - Filter for Tumwater, 2024-25
   - Generate budget projections
   - Export to Excel

3. **Financial Statement (3 min)**
   - Select F-196 report
   - Generate actuals report
   - Show variance analysis

4. **Excel Export (3 min)**
   - Show raw data export
   - Select fields for export
   - Download and open in Excel

5. **API Integration (5 min)**
   - Show API documentation
   - Execute sample API call
   - Display JSON response
   - Explain integration patterns

## Report Types

| Report | Form | Description |
|--------|------|-------------|
| Monthly Enrollment | P-223 | Monthly headcount and FTE by school |
| Budget Projection | F-195 | Annual revenue/expenditure projections |
| Financial Statement | F-196 | Year-end actuals vs budget |
| Apportionment Summary | - | State funding allocation by district |

## Export Formats
- PDF (official reports)
- Excel (.xlsx)
- CSV (bulk data)
- JSON (API)
