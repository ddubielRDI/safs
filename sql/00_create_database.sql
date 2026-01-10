-- ============================================
-- SASQUATCH Demo Database Creation Script
-- Run this script from SSMS connected to PC995
-- ============================================

USE master;
GO

-- Drop database if exists (for clean rebuild)
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'SASQUATCH_Demo')
BEGIN
    ALTER DATABASE SASQUATCH_Demo SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE SASQUATCH_Demo;
END
GO

-- Create database (uses SQL Server default data directory)
CREATE DATABASE SASQUATCH_Demo;
GO

-- Set recovery model to Simple for demo purposes
ALTER DATABASE SASQUATCH_Demo SET RECOVERY SIMPLE;
GO

USE SASQUATCH_Demo;
GO

PRINT 'Database SASQUATCH_Demo created successfully.';
GO
