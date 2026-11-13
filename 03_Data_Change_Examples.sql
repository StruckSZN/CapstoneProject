USE MealPlannerDb;
GO

/* INSERT example: add an item to the demo grocery list. */
DECLARE @ListId int = (SELECT TOP (1) GroceryListId FROM dbo.GroceryLists ORDER BY GroceryListId);
DECLARE @IngredientId int = (SELECT TOP (1) IngredientId FROM dbo.Ingredients WHERE IngredientName = N'Olive oil');
DECLARE @UnitId int = (SELECT TOP (1) UnitId FROM dbo.MeasurementUnits WHERE Abbreviation = N'tbsp');

IF @ListId IS NOT NULL AND @IngredientId IS NOT NULL
BEGIN
    INSERT dbo.GroceryListItems (GroceryListId, IngredientId, Quantity, UnitId, Notes)
    VALUES (@ListId, @IngredientId, 1.00, @UnitId, N'SQL change example row');
END;
GO

/* UPDATE example: mark the demo item as purchased and update its quantity. */
UPDATE dbo.GroceryListItems
SET Quantity = 2.00,
    IsPurchased = 1
WHERE Notes = N'SQL change example row';
GO

/* DELETE example: remove only the dedicated demonstration row. */
DELETE FROM dbo.GroceryListItems
WHERE Notes = N'SQL change example row';
GO
