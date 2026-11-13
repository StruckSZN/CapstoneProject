USE MealPlannerDb;
GO

/* Lookup data */
IF NOT EXISTS (SELECT 1 FROM dbo.AppUsers WHERE Email = N'demo@mealwise.local')
    INSERT dbo.AppUsers (DisplayName, Email) VALUES (N'Demo User', N'demo@mealwise.local');

INSERT dbo.MealTypes (TypeName, SortOrder)
SELECT v.TypeName, v.SortOrder
FROM (VALUES (N'Breakfast', 1), (N'Lunch', 2), (N'Dinner', 3), (N'Snack', 4)) v(TypeName, SortOrder)
WHERE NOT EXISTS (SELECT 1 FROM dbo.MealTypes mt WHERE mt.TypeName = v.TypeName);

INSERT dbo.DietaryRestrictions (RestrictionName, Description)
SELECT v.RestrictionName, v.Description
FROM (VALUES
    (N'Vegetarian', N'Excludes meat and fish.'),
    (N'Vegan', N'Excludes animal-derived foods.'),
    (N'Gluten-Free', N'Excludes gluten-containing ingredients.'),
    (N'Dairy-Free', N'Excludes milk and dairy ingredients.'),
    (N'Nut-Free', N'Excludes tree nuts and peanuts.')
) v(RestrictionName, Description)
WHERE NOT EXISTS (SELECT 1 FROM dbo.DietaryRestrictions d WHERE d.RestrictionName = v.RestrictionName);

INSERT dbo.RecipeCategories (CategoryName)
SELECT v.CategoryName
FROM (VALUES (N'Chicken'), (N'Pasta'), (N'Vegetarian'), (N'Seafood'), (N'Beef')) v(CategoryName)
WHERE NOT EXISTS (SELECT 1 FROM dbo.RecipeCategories c WHERE c.CategoryName = v.CategoryName);

INSERT dbo.Cuisines (CuisineName)
SELECT v.CuisineName
FROM (VALUES (N'American'), (N'Italian'), (N'Mexican'), (N'Mediterranean')) v(CuisineName)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Cuisines c WHERE c.CuisineName = v.CuisineName);

INSERT dbo.MeasurementUnits (UnitName, Abbreviation)
SELECT v.UnitName, v.Abbreviation
FROM (VALUES
    (N'Cup', N'cup'), (N'Tablespoon', N'tbsp'), (N'Teaspoon', N'tsp'),
    (N'Gram', N'g'), (N'Ounce', N'oz'), (N'Piece', N'pc')
) v(UnitName, Abbreviation)
WHERE NOT EXISTS (SELECT 1 FROM dbo.MeasurementUnits u WHERE u.Abbreviation = v.Abbreviation);

INSERT dbo.Ingredients (IngredientName)
SELECT v.IngredientName
FROM (VALUES
    (N'Chicken breast'), (N'White rice'), (N'Broccoli'), (N'Olive oil'),
    (N'Pasta'), (N'Tomato'), (N'Basil'), (N'Garlic'), (N'Black beans'),
    (N'Corn tortillas'), (N'Avocado'), (N'Salmon'), (N'Lemon'), (N'Potato')
) v(IngredientName)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Ingredients i WHERE i.IngredientName = v.IngredientName);
GO

/* Example recipes. The API importer can add more recipes and ingredients later. */
DECLARE @ChickenCategory int = (SELECT CategoryId FROM dbo.RecipeCategories WHERE CategoryName = N'Chicken');
DECLARE @PastaCategory int = (SELECT CategoryId FROM dbo.RecipeCategories WHERE CategoryName = N'Pasta');
DECLARE @VegetarianCategory int = (SELECT CategoryId FROM dbo.RecipeCategories WHERE CategoryName = N'Vegetarian');
DECLARE @SeafoodCategory int = (SELECT CategoryId FROM dbo.RecipeCategories WHERE CategoryName = N'Seafood');
DECLARE @AmericanCuisine int = (SELECT CuisineId FROM dbo.Cuisines WHERE CuisineName = N'American');
DECLARE @ItalianCuisine int = (SELECT CuisineId FROM dbo.Cuisines WHERE CuisineName = N'Italian');
DECLARE @MexicanCuisine int = (SELECT CuisineId FROM dbo.Cuisines WHERE CuisineName = N'Mexican');

IF NOT EXISTS (SELECT 1 FROM dbo.Recipes WHERE RecipeName = N'Chicken Rice Bowl' AND SourceName IS NULL)
    INSERT dbo.Recipes (RecipeName, CategoryId, CuisineId, Instructions, PrepMinutes, CookMinutes, DefaultServings)
    VALUES (N'Chicken Rice Bowl', @ChickenCategory, @AmericanCuisine, N'Cook the rice. Season and cook the chicken thoroughly. Steam broccoli and serve together.', 15, 25, 2);

IF NOT EXISTS (SELECT 1 FROM dbo.Recipes WHERE RecipeName = N'Tomato Basil Pasta' AND SourceName IS NULL)
    INSERT dbo.Recipes (RecipeName, CategoryId, CuisineId, Instructions, PrepMinutes, CookMinutes, DefaultServings)
    VALUES (N'Tomato Basil Pasta', @PastaCategory, @ItalianCuisine, N'Cook pasta according to package directions. Warm tomatoes and garlic in olive oil. Toss together with basil.', 10, 20, 2);

IF NOT EXISTS (SELECT 1 FROM dbo.Recipes WHERE RecipeName = N'Black Bean Tacos' AND SourceName IS NULL)
    INSERT dbo.Recipes (RecipeName, CategoryId, CuisineId, Instructions, PrepMinutes, CookMinutes, DefaultServings)
    VALUES (N'Black Bean Tacos', @VegetarianCategory, @MexicanCuisine, N'Warm black beans with seasoning. Heat tortillas and fill with beans and avocado.', 10, 10, 2);

IF NOT EXISTS (SELECT 1 FROM dbo.Recipes WHERE RecipeName = N'Lemon Salmon Tray Bake' AND SourceName IS NULL)
    INSERT dbo.Recipes (RecipeName, CategoryId, CuisineId, Instructions, PrepMinutes, CookMinutes, DefaultServings)
    VALUES (N'Lemon Salmon Tray Bake', @SeafoodCategory, @AmericanCuisine, N'Place salmon and chopped potatoes on a tray. Season with olive oil and lemon. Bake until salmon reaches a safe internal temperature.', 10, 25, 2);
GO

/* Recipe ingredient bridge records */
DECLARE @ChickenRecipe int = (SELECT RecipeId FROM dbo.Recipes WHERE RecipeName = N'Chicken Rice Bowl' AND SourceName IS NULL);
DECLARE @PastaRecipe int = (SELECT RecipeId FROM dbo.Recipes WHERE RecipeName = N'Tomato Basil Pasta' AND SourceName IS NULL);
DECLARE @TacoRecipe int = (SELECT RecipeId FROM dbo.Recipes WHERE RecipeName = N'Black Bean Tacos' AND SourceName IS NULL);
DECLARE @SalmonRecipe int = (SELECT RecipeId FROM dbo.Recipes WHERE RecipeName = N'Lemon Salmon Tray Bake' AND SourceName IS NULL);
DECLARE @Cup int = (SELECT UnitId FROM dbo.MeasurementUnits WHERE Abbreviation = N'cup');
DECLARE @Tbsp int = (SELECT UnitId FROM dbo.MeasurementUnits WHERE Abbreviation = N'tbsp');
DECLARE @Pc int = (SELECT UnitId FROM dbo.MeasurementUnits WHERE Abbreviation = N'pc');

IF NOT EXISTS (SELECT 1 FROM dbo.RecipeIngredients WHERE RecipeId = @ChickenRecipe)
BEGIN
    INSERT dbo.RecipeIngredients (RecipeId, IngredientId, Quantity, UnitId)
    SELECT @ChickenRecipe, IngredientId, v.Quantity, v.UnitId
    FROM (VALUES (N'Chicken breast', CAST(1.00 AS decimal(10,2)), @Pc),
                 (N'White rice', CAST(1.00 AS decimal(10,2)), @Cup),
                 (N'Broccoli', CAST(1.00 AS decimal(10,2)), @Cup),
                 (N'Olive oil', CAST(1.00 AS decimal(10,2)), @Tbsp)) v(IngredientName, Quantity, UnitId)
    JOIN dbo.Ingredients i ON i.IngredientName = v.IngredientName;
END;

IF NOT EXISTS (SELECT 1 FROM dbo.RecipeIngredients WHERE RecipeId = @PastaRecipe)
BEGIN
    INSERT dbo.RecipeIngredients (RecipeId, IngredientId, Quantity, UnitId)
    SELECT @PastaRecipe, IngredientId, v.Quantity, v.UnitId
    FROM (VALUES (N'Pasta', CAST(2.00 AS decimal(10,2)), @Cup),
                 (N'Tomato', CAST(2.00 AS decimal(10,2)), @Pc),
                 (N'Basil', CAST(1.00 AS decimal(10,2)), @Pc),
                 (N'Garlic', CAST(1.00 AS decimal(10,2)), @Pc),
                 (N'Olive oil', CAST(1.00 AS decimal(10,2)), @Tbsp)) v(IngredientName, Quantity, UnitId)
    JOIN dbo.Ingredients i ON i.IngredientName = v.IngredientName;
END;

IF NOT EXISTS (SELECT 1 FROM dbo.RecipeIngredients WHERE RecipeId = @TacoRecipe)
BEGIN
    INSERT dbo.RecipeIngredients (RecipeId, IngredientId, Quantity, UnitId)
    SELECT @TacoRecipe, IngredientId, v.Quantity, v.UnitId
    FROM (VALUES (N'Black beans', CAST(1.00 AS decimal(10,2)), @Cup),
                 (N'Corn tortillas', CAST(4.00 AS decimal(10,2)), @Pc),
                 (N'Avocado', CAST(1.00 AS decimal(10,2)), @Pc)) v(IngredientName, Quantity, UnitId)
    JOIN dbo.Ingredients i ON i.IngredientName = v.IngredientName;
END;

IF NOT EXISTS (SELECT 1 FROM dbo.RecipeIngredients WHERE RecipeId = @SalmonRecipe)
BEGIN
    INSERT dbo.RecipeIngredients (RecipeId, IngredientId, Quantity, UnitId)
    SELECT @SalmonRecipe, IngredientId, v.Quantity, v.UnitId
    FROM (VALUES (N'Salmon', CAST(2.00 AS decimal(10,2)), @Pc),
                 (N'Lemon', CAST(1.00 AS decimal(10,2)), @Pc),
                 (N'Potato', CAST(2.00 AS decimal(10,2)), @Pc),
                 (N'Olive oil', CAST(1.00 AS decimal(10,2)), @Tbsp)) v(IngredientName, Quantity, UnitId)
    JOIN dbo.Ingredients i ON i.IngredientName = v.IngredientName;
END;
GO

/* Sample meal plan and grocery list so reports can be demonstrated immediately. */
DECLARE @UserId int = (SELECT TOP (1) UserId FROM dbo.AppUsers WHERE Email = N'demo@mealwise.local');
DECLARE @MealPlanId int;

IF NOT EXISTS (SELECT 1 FROM dbo.MealPlans WHERE UserId = @UserId AND PlanName = N'Sample Week')
BEGIN
    INSERT dbo.MealPlans (UserId, PlanName, WeekStartDate)
    VALUES (@UserId, N'Sample Week', CAST(GETDATE() AS date));
END;
SET @MealPlanId = (SELECT TOP (1) MealPlanId FROM dbo.MealPlans WHERE UserId = @UserId AND PlanName = N'Sample Week' ORDER BY MealPlanId DESC);

IF NOT EXISTS (SELECT 1 FROM dbo.MealPlanEntries WHERE MealPlanId = @MealPlanId)
BEGIN
    INSERT dbo.MealPlanEntries (MealPlanId, RecipeId, MealTypeId, PlannedDate, Servings)
    SELECT @MealPlanId, r.RecipeId, mt.MealTypeId, CAST(GETDATE() AS date), 2
    FROM dbo.Recipes r
    CROSS JOIN dbo.MealTypes mt
    WHERE r.RecipeName = N'Chicken Rice Bowl' AND mt.TypeName = N'Dinner';
END;

IF NOT EXISTS (SELECT 1 FROM dbo.GroceryLists WHERE MealPlanId = @MealPlanId)
    INSERT dbo.GroceryLists (MealPlanId, ListName) VALUES (@MealPlanId, N'Sample Week Groceries');
GO
