/*
    MealWise database verification
    Run after 00_CreateDatabase.sql, 01_CreateTables.sql, and 02_InsertSampleData.sql.
    Read-only: this script validates required objects and sample records, then reports counts.
*/
USE MealPlannerDb;
GO

SET NOCOUNT ON;

DECLARE @RequiredTables TABLE (TableName sysname NOT NULL PRIMARY KEY);
INSERT INTO @RequiredTables (TableName)
VALUES
    (N'AppUsers'), (N'DietaryRestrictions'), (N'UserDietaryRestrictions'),
    (N'MealTypes'), (N'RecipeCategories'), (N'Cuisines'), (N'Ingredients'),
    (N'MeasurementUnits'), (N'Recipes'), (N'RecipeIngredients'),
    (N'RecipeDietaryRestrictions'), (N'MealPlans'), (N'MealPlanEntries'),
    (N'GroceryLists'), (N'GroceryListItems');

IF EXISTS
(
    SELECT 1
    FROM @RequiredTables required
    WHERE OBJECT_ID(N'dbo.' + QUOTENAME(required.TableName), N'U') IS NULL
)
BEGIN
    SELECT required.TableName AS MissingTable
    FROM @RequiredTables required
    WHERE OBJECT_ID(N'dbo.' + QUOTENAME(required.TableName), N'U') IS NULL;
    THROW 51000, 'Database verification failed: one or more required tables are missing. Run scripts 00 and 01.', 1;
END;

IF (SELECT COUNT(*) FROM sys.foreign_keys WHERE schema_id = SCHEMA_ID(N'dbo')) < 17
    THROW 51001, 'Database verification failed: expected foreign-key relationships are missing.', 1;

IF NOT EXISTS (SELECT 1 FROM dbo.AppUsers WHERE Email = N'demo@mealwise.local')
    THROW 51002, 'Database verification failed: demo user is missing. Run 02_InsertSampleData.sql.', 1;

IF (SELECT COUNT(*) FROM dbo.MealTypes) < 4
    THROW 51003, 'Database verification failed: expected meal-type lookup rows are missing.', 1;

IF (SELECT COUNT(*) FROM dbo.Recipes WHERE SourceName IS NULL) < 4
    THROW 51004, 'Database verification failed: expected sample recipes are missing.', 1;

SELECT
    N'PASS' AS VerificationStatus,
    (SELECT COUNT(*) FROM @RequiredTables) AS RequiredTables,
    (SELECT COUNT(*) FROM sys.foreign_keys WHERE schema_id = SCHEMA_ID(N'dbo')) AS ForeignKeyRelationships,
    (SELECT COUNT(*) FROM dbo.AppUsers) AS AppUsers,
    (SELECT COUNT(*) FROM dbo.Recipes) AS Recipes,
    (SELECT COUNT(*) FROM dbo.RecipeIngredients) AS RecipeIngredients,
    (SELECT COUNT(*) FROM dbo.MealPlans) AS MealPlans,
    (SELECT COUNT(*) FROM dbo.MealPlanEntries) AS MealPlanEntries,
    (SELECT COUNT(*) FROM dbo.GroceryLists) AS GroceryLists,
    (SELECT COUNT(*) FROM dbo.GroceryListItems) AS GroceryListItems;
GO
