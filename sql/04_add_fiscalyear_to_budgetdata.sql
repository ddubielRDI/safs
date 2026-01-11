-- Migration: Add FiscalYear column to BudgetData for multi-year budget projections
-- This allows importing all 4 fiscal years from F-195 budget files

USE Sasquatch_Demo;
GO

-- Add FiscalYear column if it doesn't exist
IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.BudgetData')
    AND name = 'FiscalYear'
)
BEGIN
    ALTER TABLE dbo.BudgetData
    ADD FiscalYear NVARCHAR(10) NOT NULL DEFAULT '2024-25';

    PRINT 'FiscalYear column added to BudgetData table';
END
ELSE
BEGIN
    PRINT 'FiscalYear column already exists';
END
GO
