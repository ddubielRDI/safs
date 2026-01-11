-- ============================================
-- SASQUATCH Demo Schema Creation Script
-- Run after 00_create_database.sql
-- ============================================

USE SASQUATCH_Demo;
GO

-- ============================================
-- CORE REFERENCE TABLES
-- ============================================

-- Educational Service Districts (ESDs)
CREATE TABLE dbo.ESDs (
    ESDCode CHAR(3) PRIMARY KEY,
    ESDName NVARCHAR(100) NOT NULL,
    RegionName NVARCHAR(50)
);
GO

-- School Districts
CREATE TABLE dbo.Districts (
    DistrictCode CHAR(5) PRIMARY KEY,         -- CCDDD format (34033 for Tumwater)
    DistrictName NVARCHAR(100) NOT NULL,
    CountyCode CHAR(2) NOT NULL,
    ESDCode CHAR(3) NOT NULL REFERENCES dbo.ESDs(ESDCode),
    Class TINYINT NOT NULL DEFAULT 1,         -- 1 or 2
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETDATE()
);
GO

-- Schools/Buildings
CREATE TABLE dbo.Schools (
    SchoolCode CHAR(4) PRIMARY KEY,
    DistrictCode CHAR(5) NOT NULL REFERENCES dbo.Districts(DistrictCode),
    SchoolName NVARCHAR(100) NOT NULL,
    SchoolType NVARCHAR(20),                  -- Elementary, Middle, High, Other
    GradeLow CHAR(2),
    GradeHigh CHAR(2),
    IsActive BIT DEFAULT 1
);
GO

-- ============================================
-- ENROLLMENT TABLES (P-223)
-- ============================================

-- Enrollment Submissions (header record per district/month)
CREATE TABLE dbo.EnrollmentSubmissions (
    SubmissionId INT IDENTITY PRIMARY KEY,
    DistrictCode CHAR(5) NOT NULL REFERENCES dbo.Districts(DistrictCode),
    SchoolYear CHAR(7) NOT NULL,              -- '2024-25'
    Month TINYINT NOT NULL,                   -- 1=Sept, 12=Aug
    SubmissionStatus NVARCHAR(20) NOT NULL DEFAULT 'Draft',
    SubmittedBy NVARCHAR(100),
    SubmittedDate DATETIME2,
    ApprovedBy NVARCHAR(100),
    ApprovedDate DATETIME2,
    IsLocked BIT DEFAULT 0,
    LockedBy NVARCHAR(100),
    LockedDate DATETIME2,
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    ModifiedDate DATETIME2,
    CONSTRAINT UQ_EnrollmentSubmission UNIQUE (DistrictCode, SchoolYear, Month)
);
GO

-- Enrollment Detail Data
CREATE TABLE dbo.EnrollmentData (
    EnrollmentId INT IDENTITY PRIMARY KEY,
    SubmissionId INT NOT NULL REFERENCES dbo.EnrollmentSubmissions(SubmissionId),
    SchoolCode CHAR(4) NOT NULL REFERENCES dbo.Schools(SchoolCode),
    GradeLevel CHAR(2) NOT NULL,              -- K, 01-12, PK
    ProgramType NVARCHAR(30) NOT NULL DEFAULT 'BasicEd',
    ResidentDistrictCode CHAR(5),
    Headcount INT NOT NULL,                   -- Integer only
    FTE DECIMAL(10,2) NOT NULL,               -- 2 decimal places
    PriorMonthHeadcount INT,
    PriorMonthFTE DECIMAL(10,2),
    HeadcountVariance AS (Headcount - ISNULL(PriorMonthHeadcount, Headcount)) PERSISTED,
    FTEVariance AS (FTE - ISNULL(PriorMonthFTE, FTE)) PERSISTED
);
GO

-- Edit/Validation results for enrollment
CREATE TABLE dbo.EnrollmentEdits (
    EditId INT IDENTITY PRIMARY KEY,
    SubmissionId INT NOT NULL REFERENCES dbo.EnrollmentSubmissions(SubmissionId),
    EditRuleId NVARCHAR(20) NOT NULL,
    Severity NVARCHAR(10) NOT NULL,           -- Error, Warning, Info
    Message NVARCHAR(500) NOT NULL,
    FieldName NVARCHAR(50),
    FieldValue NVARCHAR(100),
    DistrictComment NVARCHAR(1000),
    IsResolved BIT DEFAULT 0,
    ResolvedBy NVARCHAR(100),
    ResolvedDate DATETIME2,
    CreatedDate DATETIME2 DEFAULT GETDATE()
);
GO

-- ============================================
-- BUDGET TABLES (F-195/F-200)
-- ============================================

-- Budget Submissions (header record per district/year/form)
CREATE TABLE dbo.BudgetSubmissions (
    SubmissionId INT IDENTITY PRIMARY KEY,
    DistrictCode CHAR(5) NOT NULL REFERENCES dbo.Districts(DistrictCode),
    FiscalYear CHAR(7) NOT NULL,              -- '2024-25'
    FormType NVARCHAR(10) NOT NULL,           -- F-195, F-200
    SubmissionStatus NVARCHAR(20) NOT NULL DEFAULT 'Draft',
    SubmittedBy NVARCHAR(100),
    SubmittedDate DATETIME2,
    ApprovedBy NVARCHAR(100),
    ApprovedDate DATETIME2,
    IsLocked BIT DEFAULT 0,
    LockedBy NVARCHAR(100),
    LockedDate DATETIME2,
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    ModifiedDate DATETIME2,
    CONSTRAINT UQ_BudgetSubmission UNIQUE (DistrictCode, FiscalYear, FormType)
);
GO

-- Budget Detail Data
CREATE TABLE dbo.BudgetData (
    BudgetId INT IDENTITY PRIMARY KEY,
    SubmissionId INT NOT NULL REFERENCES dbo.BudgetSubmissions(SubmissionId),
    FundCode CHAR(2) NOT NULL,                -- 10=General, 20=Capital, etc.
    ProgramCode CHAR(2),
    ActivityCode CHAR(2),
    ObjectCode CHAR(3),
    ItemCode NVARCHAR(10),
    ItemDescription NVARCHAR(200),
    FiscalYear NVARCHAR(10) NOT NULL DEFAULT '2024-25',  -- e.g., "2024-25", "2025-26"
    Amount DECIMAL(18,2) NOT NULL,
    PriorMonthAmount DECIMAL(18,2),
    Variance AS (Amount - ISNULL(PriorMonthAmount, Amount)) PERSISTED,
    CreatedDate DATETIME2 DEFAULT GETDATE()
);
GO

-- Edit/Validation results for budget
CREATE TABLE dbo.BudgetEdits (
    EditId INT IDENTITY PRIMARY KEY,
    SubmissionId INT NOT NULL REFERENCES dbo.BudgetSubmissions(SubmissionId),
    EditRuleId NVARCHAR(20) NOT NULL,
    Severity NVARCHAR(10) NOT NULL,
    Message NVARCHAR(500) NOT NULL,
    FieldName NVARCHAR(50),
    ExpectedValue NVARCHAR(100),
    ActualValue NVARCHAR(100),
    DistrictComment NVARCHAR(1000),
    IsResolved BIT DEFAULT 0,
    ResolvedBy NVARCHAR(100),
    ResolvedDate DATETIME2,
    CreatedDate DATETIME2 DEFAULT GETDATE()
);
GO

-- ============================================
-- CONFIGURATION TABLES
-- ============================================

-- Edit/Validation Rules Configuration
CREATE TABLE dbo.EditRules (
    RuleId NVARCHAR(20) PRIMARY KEY,
    RuleName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    FormType NVARCHAR(10) NOT NULL,           -- P-223, F-195, F-200
    Severity NVARCHAR(10) NOT NULL,           -- Error, Warning, Info
    Formula NVARCHAR(1000),                   -- Calculation/comparison logic
    Threshold DECIMAL(10,4),                  -- Variance threshold (e.g., 10.0 = 10%)
    BlocksSubmission BIT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETDATE()
);
GO

-- Data Lock Control
CREATE TABLE dbo.DataLocks (
    LockId INT IDENTITY PRIMARY KEY,
    LockScope NVARCHAR(20) NOT NULL,          -- All, ESD, District
    ScopeValue NVARCHAR(10),                  -- NULL for All, ESD code, or District code
    FormType NVARCHAR(10),                    -- NULL = all forms, or specific form
    LockType NVARCHAR(20) NOT NULL,           -- Monthly, Annual, Audit
    SchoolYear CHAR(7),
    Month TINYINT,
    LockedBy NVARCHAR(100) NOT NULL,
    LockedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    UnlockedBy NVARCHAR(100),
    UnlockedDate DATETIME2,
    Reason NVARCHAR(500),
    IsActive BIT DEFAULT 1
);
GO

-- ============================================
-- AUDIT TRAIL
-- ============================================

CREATE TABLE dbo.AuditLog (
    AuditId INT IDENTITY PRIMARY KEY,
    TableName NVARCHAR(50) NOT NULL,
    RecordId INT NOT NULL,
    Action NVARCHAR(10) NOT NULL,             -- INSERT, UPDATE, DELETE
    FieldName NVARCHAR(50),
    OldValue NVARCHAR(MAX),
    NewValue NVARCHAR(MAX),
    ChangedBy NVARCHAR(100) NOT NULL,
    ChangedDate DATETIME2 DEFAULT GETDATE(),
    Reason NVARCHAR(500)
);
GO

-- Index for audit queries
CREATE INDEX IX_AuditLog_Table ON dbo.AuditLog(TableName, RecordId);
CREATE INDEX IX_AuditLog_Date ON dbo.AuditLog(ChangedDate);
GO

-- ============================================
-- USER/ROLE TABLES (Demo purposes)
-- ============================================

CREATE TABLE dbo.Users (
    UserId INT IDENTITY PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL UNIQUE,
    DisplayName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(200),
    UserRole NVARCHAR(20) NOT NULL,           -- District, ESD, OSPI, Legislature
    DistrictCode CHAR(5) REFERENCES dbo.Districts(DistrictCode),
    ESDCode CHAR(3) REFERENCES dbo.ESDs(ESDCode),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETDATE()
);
GO

PRINT 'Schema created successfully.';
GO
