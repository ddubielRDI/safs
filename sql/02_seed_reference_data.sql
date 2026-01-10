-- ============================================
-- SASQUATCH Demo Seed Data Script
-- Populates reference tables and edit rules
-- ============================================

USE SASQUATCH_Demo;
GO

-- ============================================
-- ESDs (Educational Service Districts)
-- ============================================

INSERT INTO dbo.ESDs (ESDCode, ESDName, RegionName) VALUES
('101', 'ESD 101 - Northeast Washington', 'Northeast'),
('105', 'ESD 105 - Yakima Region', 'Central'),
('112', 'ESD 112 - Southwest Washington', 'Southwest'),
('113', 'ESD 113 - Capital Region', 'Capital'),       -- Tumwater's ESD
('114', 'ESD 114 - Olympic Region', 'Olympic'),
('121', 'ESD 121 - Puget Sound', 'Puget Sound'),
('123', 'ESD 123 - Southeast Washington', 'Southeast'),
('171', 'ESD 171 - North Central', 'North Central'),
('189', 'ESD 189 - Northwest Washington', 'Northwest');
GO

-- ============================================
-- DISTRICTS (Tumwater + Sample Districts)
-- ============================================

-- Tumwater School District (main demo district)
INSERT INTO dbo.Districts (DistrictCode, DistrictName, CountyCode, ESDCode, Class) VALUES
('34033', 'Tumwater School District', '34', '113', 1);

-- Additional sample districts in ESD 113 for demo
INSERT INTO dbo.Districts (DistrictCode, DistrictName, CountyCode, ESDCode, Class) VALUES
('34003', 'North Thurston School District', '34', '113', 1),
('34311', 'Olympia School District', '34', '113', 1),
('34401', 'Rochester School District', '34', '113', 2),
('34501', 'Tenino School District', '34', '113', 2),
('34601', 'Yelm Community Schools', '34', '113', 1);
GO

-- ============================================
-- SCHOOLS (Tumwater Schools)
-- ============================================

-- Tumwater School District schools
INSERT INTO dbo.Schools (SchoolCode, DistrictCode, SchoolName, SchoolType, GradeLow, GradeHigh) VALUES
-- High Schools
('0001', '34033', 'Tumwater High School', 'High', '09', '12'),
('0002', '34033', 'Black Hills High School', 'High', '09', '12'),
-- Middle Schools
('0003', '34033', 'Tumwater Middle School', 'Middle', '06', '08'),
('0004', '34033', 'Bush Middle School', 'Middle', '06', '08'),
-- Elementary Schools
('0005', '34033', 'East Olympia Elementary', 'Elementary', 'K', '05'),
('0006', '34033', 'Littlerock Elementary', 'Elementary', 'K', '05'),
('0007', '34033', 'Michael T. Simmons Elementary', 'Elementary', 'K', '05'),
('0008', '34033', 'Peter G. Schmidt Elementary', 'Elementary', 'K', '05'),
('0009', '34033', 'Tumwater Hill Elementary', 'Elementary', 'K', '05'),
('0010', '34033', 'Black Lake Elementary', 'Elementary', 'K', '05');
GO

-- ============================================
-- EDIT RULES
-- ============================================

-- Enrollment Edit Rules (P-223)
INSERT INTO dbo.EditRules (RuleId, RuleName, Description, FormType, Severity, Formula, Threshold, BlocksSubmission, IsActive) VALUES
('ENR-001', 'Month-over-Month Headcount', 'Flags >10% headcount change from prior month', 'P-223', 'Warning', 'ABS(HeadcountVariance / NULLIF(PriorMonthHeadcount, 0) * 100) > @Threshold', 10.0, 0, 1),
('ENR-002', 'Invalid Grade for School', 'Validates grade level is appropriate for school type', 'P-223', 'Error', NULL, NULL, 1, 1),
('ENR-003', 'FTE Exceeds Headcount', 'FTE cannot exceed headcount', 'P-223', 'Error', 'FTE > Headcount', NULL, 1, 1),
('ENR-004', 'Negative Values', 'Headcount and FTE must be non-negative', 'P-223', 'Error', 'Headcount < 0 OR FTE < 0', NULL, 1, 1),
('ENR-005', 'Decimal Headcount', 'Headcount must be a whole number', 'P-223', 'Error', 'Headcount <> FLOOR(Headcount)', NULL, 1, 1),
('ENR-006', 'Large Enrollment Drop', 'Flags >25% enrollment decrease from prior month', 'P-223', 'Warning', 'HeadcountVariance / NULLIF(PriorMonthHeadcount, 0) * 100 < -25', NULL, 0, 1),
('ENR-007', 'Missing School Code', 'School code is required', 'P-223', 'Error', 'SchoolCode IS NULL OR SchoolCode = ''''', NULL, 1, 1);

-- Budget Edit Rules (F-195)
INSERT INTO dbo.EditRules (RuleId, RuleName, Description, FormType, Severity, Formula, Threshold, BlocksSubmission, IsActive) VALUES
('BUD-001', 'Month-over-Month Budget', 'Flags >15% budget change from prior month', 'F-195', 'Warning', 'ABS(Variance / NULLIF(PriorMonthAmount, 0) * 100) > @Threshold', 15.0, 0, 1),
('BUD-002', 'Invalid Fund Code', 'Fund code must be valid (10, 20, 30, 40, 50, etc.)', 'F-195', 'Error', NULL, NULL, 1, 1),
('BUD-003', 'Invalid Program/Activity/Object', 'Validates code combination is valid', 'F-195', 'Error', NULL, NULL, 1, 1),
('BUD-004', 'Unreasonable Amount', 'Flags amounts exceeding $100M (likely data entry error)', 'F-195', 'Warning', 'ABS(Amount) > 100000000', NULL, 0, 1),
('BUD-005', 'Negative Revenue', 'Revenue amounts should typically be positive', 'F-195', 'Warning', 'Amount < 0 AND ItemType = ''Revenue''', NULL, 0, 1),
('BUD-006', 'Missing Required Fields', 'Fund code and amount are required', 'F-195', 'Error', 'FundCode IS NULL OR Amount IS NULL', NULL, 1, 1),
('BUD-007', 'Budget Balance Check', 'Revenues should approximately equal expenditures', 'F-195', 'Info', NULL, NULL, 0, 1);

-- Budget Extension Rules (F-200)
INSERT INTO dbo.EditRules (RuleId, RuleName, Description, FormType, Severity, Formula, Threshold, BlocksSubmission, IsActive) VALUES
('EXT-001', 'Extension Exceeds Original', 'Budget extension significantly exceeds original budget', 'F-200', 'Warning', 'ABS(Variance / NULLIF(PriorMonthAmount, 0) * 100) > @Threshold', 20.0, 0, 1),
('EXT-002', 'Missing Justification', 'Budget extensions require explanatory comments', 'F-200', 'Warning', NULL, NULL, 0, 1);
GO

-- ============================================
-- DEMO USERS
-- ============================================

INSERT INTO dbo.Users (Username, DisplayName, Email, UserRole, DistrictCode, ESDCode) VALUES
-- OSPI Users
('ospi.admin', 'OSPI Administrator', 'admin@k12.wa.us', 'OSPI', NULL, NULL),
('ospi.reviewer', 'OSPI Data Reviewer', 'reviewer@k12.wa.us', 'OSPI', NULL, NULL),
-- District Users (Tumwater)
('tumwater.admin', 'Tumwater District Admin', 'admin@tumwater.k12.wa.us', 'District', '34033', '113'),
('tumwater.finance', 'Tumwater Finance Director', 'finance@tumwater.k12.wa.us', 'District', '34033', '113'),
-- ESD Users
('esd113.coordinator', 'ESD 113 Coordinator', 'coordinator@esd113.org', 'ESD', NULL, '113'),
-- Legislature User (for sandbox scenarios)
('leg.analyst', 'Legislative Budget Analyst', 'analyst@leg.wa.gov', 'Legislature', NULL, NULL);
GO

PRINT 'Reference data seeded successfully.';
GO
