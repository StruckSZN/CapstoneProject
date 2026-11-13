/*
    Run this script in SQL Server Management Studio (SSMS) or Azure Data Studio.
    It creates the empty project database. Add the actual tables only after the
    capstone requirements and entity relationships have been confirmed.
*/
IF DB_ID(N'CapstoneDb') IS NULL
BEGIN
    CREATE DATABASE [CapstoneDb];
END;
GO

USE [CapstoneDb];
GO

-- TODO: Add project-specific tables, primary keys, foreign keys,
-- constraints, indexes, and seed data based on the approved requirements.
