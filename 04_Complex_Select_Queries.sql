USE MealPlannerDb;
GO

/* 1. Multi-table JOIN: display a meal plan with recipe, meal type, date, and cuisine. */
SELECT
    mp.PlanName,
    mpe.PlannedDate,
    mt.TypeName AS MealType,
    r.RecipeName,
    c.CuisineName,
    mpe.Servings
FROM dbo.MealPlanEntries AS mpe
INNER JOIN dbo.MealPlans AS mp ON mp.MealPlanId = mpe.MealPlanId
INNER JOIN dbo.Recipes AS r ON r.RecipeId = mpe.RecipeId
INNER JOIN dbo.MealTypes AS mt ON mt.MealTypeId = mpe.MealTypeId
LEFT JOIN dbo.Cuisines AS c ON c.CuisineId = r.CuisineId
ORDER BY mpe.PlannedDate, mt.SortOrder;
GO

/* 2. Aggregate functions: count planned meals and total servings by plan. */
SELECT
    mp.MealPlanId,
    mp.PlanName,
    COUNT(mpe.MealPlanEntryId) AS PlannedMealCount,
    COALESCE(SUM(mpe.Servings), 0) AS TotalPlannedServings,
    MIN(mpe.PlannedDate) AS FirstPlannedDate,
    MAX(mpe.PlannedDate) AS LastPlannedDate
FROM dbo.MealPlans AS mp
LEFT JOIN dbo.MealPlanEntries AS mpe ON mpe.MealPlanId = mp.MealPlanId
GROUP BY mp.MealPlanId, mp.PlanName
ORDER BY PlannedMealCount DESC;
GO

/* 3. CTE and aggregation: show ingredients required across a selected plan.
      This sums quantities when the same ingredient and unit occur in several recipes. */
DECLARE @SelectedMealPlanId int = (SELECT TOP (1) MealPlanId FROM dbo.MealPlans ORDER BY MealPlanId);

WITH PlannedIngredientTotals AS
(
    SELECT
        ri.IngredientId,
        ri.UnitId,
        SUM(ri.Quantity * CAST(mpe.Servings AS decimal(10,2)) / NULLIF(r.DefaultServings, 0)) AS RequiredQuantity,
        COUNT(DISTINCT mpe.MealPlanEntryId) AS UsedInMealCount
    FROM dbo.MealPlanEntries AS mpe
    INNER JOIN dbo.Recipes AS r ON r.RecipeId = mpe.RecipeId
    INNER JOIN dbo.RecipeIngredients AS ri ON ri.RecipeId = r.RecipeId
    WHERE mpe.MealPlanId = @SelectedMealPlanId
      AND ri.Quantity IS NOT NULL
    GROUP BY ri.IngredientId, ri.UnitId
)
SELECT
    i.IngredientName,
    u.Abbreviation AS Unit,
    CAST(pit.RequiredQuantity AS decimal(10,2)) AS RequiredQuantity,
    pit.UsedInMealCount
FROM PlannedIngredientTotals AS pit
INNER JOIN dbo.Ingredients AS i ON i.IngredientId = pit.IngredientId
LEFT JOIN dbo.MeasurementUnits AS u ON u.UnitId = pit.UnitId
ORDER BY i.IngredientName;
GO

/* 4. Search/filter: recipes that include an ingredient and show their cuisine. */
DECLARE @IngredientSearch nvarchar(120) = N'Chicken breast';
SELECT DISTINCT
    r.RecipeName,
    COALESCE(c.CuisineName, N'Not specified') AS Cuisine,
    r.PrepMinutes,
    r.CookMinutes,
    r.IsFavorite
FROM dbo.Recipes AS r
INNER JOIN dbo.RecipeIngredients AS ri ON ri.RecipeId = r.RecipeId
INNER JOIN dbo.Ingredients AS i ON i.IngredientId = ri.IngredientId
LEFT JOIN dbo.Cuisines AS c ON c.CuisineId = r.CuisineId
WHERE i.IngredientName LIKE N'%' + @IngredientSearch + N'%'
ORDER BY r.RecipeName;
GO

/* 5. Grocery list status using conditional aggregation. */
SELECT
    gl.ListName,
    COUNT(gli.GroceryListItemId) AS TotalItems,
    SUM(CASE WHEN gli.IsPurchased = 1 THEN 1 ELSE 0 END) AS PurchasedItems,
    SUM(CASE WHEN gli.IsPurchased = 0 THEN 1 ELSE 0 END) AS RemainingItems
FROM dbo.GroceryLists AS gl
LEFT JOIN dbo.GroceryListItems AS gli ON gli.GroceryListId = gl.GroceryListId
GROUP BY gl.GroceryListId, gl.ListName
ORDER BY gl.ListName;
GO

/* 6. Dietary restriction tags for recipes (many-to-many relationship). */
SELECT
    r.RecipeName,
    dr.RestrictionName
FROM dbo.RecipeDietaryRestrictions AS rdr
INNER JOIN dbo.Recipes AS r ON r.RecipeId = rdr.RecipeId
INNER JOIN dbo.DietaryRestrictions AS dr ON dr.RestrictionId = rdr.RestrictionId
ORDER BY r.RecipeName, dr.RestrictionName;
GO
