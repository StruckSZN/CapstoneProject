/* Run this file in SQL Server Management Studio before running the other scripts. */
IF DB_ID(N'MealPlannerDb') IS NULL
BEGIN
    CREATE DATABASE MealPlannerDb;
END;
GO
