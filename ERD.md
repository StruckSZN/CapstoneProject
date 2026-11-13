# MealWise Entity Relationship Diagram

The schema contains **15 tables**, exceeding the minimum requirement of eight.

```mermaid
erDiagram
    AppUsers ||--o{ MealPlans : creates
    AppUsers ||--o{ UserDietaryRestrictions : has
    DietaryRestrictions ||--o{ UserDietaryRestrictions : assigned_to
    DietaryRestrictions ||--o{ RecipeDietaryRestrictions : labels
    Recipes ||--o{ RecipeDietaryRestrictions : has
    RecipeCategories ||--o{ Recipes : classifies
    Cuisines ||--o{ Recipes : describes
    Recipes ||--o{ RecipeIngredients : contains
    Ingredients ||--o{ RecipeIngredients : used_in
    MeasurementUnits ||--o{ RecipeIngredients : measures
    MealPlans ||--o{ MealPlanEntries : contains
    Recipes ||--o{ MealPlanEntries : scheduled_as
    MealTypes ||--o{ MealPlanEntries : categorizes
    MealPlans ||--o| GroceryLists : generates
    GroceryLists ||--o{ GroceryListItems : contains
    Ingredients ||--o{ GroceryListItems : item
    MeasurementUnits ||--o{ GroceryListItems : measures

    AppUsers {
        int UserId PK
        nvarchar DisplayName
        nvarchar Email UK
        datetime2 CreatedAt
    }
    DietaryRestrictions {
        int RestrictionId PK
        nvarchar RestrictionName UK
        nvarchar Description
    }
    UserDietaryRestrictions {
        int UserId PK
        int RestrictionId PK
        nvarchar Notes
    }
    MealTypes {
        int MealTypeId PK
        nvarchar TypeName UK
        tinyint SortOrder
    }
    RecipeCategories {
        int CategoryId PK
        nvarchar CategoryName UK
    }
    Cuisines {
        int CuisineId PK
        nvarchar CuisineName UK
    }
    Recipes {
        int RecipeId PK
        nvarchar RecipeName
        int CategoryId FK
        int CuisineId FK
        nvarchar Instructions
        nvarchar SourceName
        nvarchar SourceRecipeId
    }
    Ingredients {
        int IngredientId PK
        nvarchar IngredientName UK
    }
    MeasurementUnits {
        int UnitId PK
        nvarchar UnitName UK
        nvarchar Abbreviation UK
    }
    RecipeIngredients {
        int RecipeIngredientId PK
        int RecipeId FK
        int IngredientId FK
        decimal Quantity
        int UnitId FK
        nvarchar MeasureText
    }
    RecipeDietaryRestrictions {
        int RecipeId PK
        int RestrictionId PK
    }
    MealPlans {
        int MealPlanId PK
        int UserId FK
        nvarchar PlanName
        date WeekStartDate
    }
    MealPlanEntries {
        int MealPlanEntryId PK
        int MealPlanId FK
        int RecipeId FK
        int MealTypeId FK
        date PlannedDate
        tinyint Servings
    }
    GroceryLists {
        int GroceryListId PK
        int MealPlanId FK
        nvarchar ListName
    }
    GroceryListItems {
        int GroceryListItemId PK
        int GroceryListId FK
        int IngredientId FK
        decimal Quantity
        int UnitId FK
        bit IsPurchased
    }
```

## Relationship summary

- One user can create many meal plans.
- Users and dietary restrictions have a many-to-many relationship through `UserDietaryRestrictions`.
- Recipes belong to an optional category and cuisine.
- Recipes and ingredients have a many-to-many relationship through `RecipeIngredients`. The bridge stores the amount, unit, and original measure text for the ingredient in that recipe.
- Recipes and dietary restrictions have a many-to-many relationship through `RecipeDietaryRestrictions`.
- One meal plan contains many meal-plan entries. Each entry references one recipe, one meal type, and one planned date.
- A meal plan can have one grocery list; a grocery list contains many grocery list items.
- Grocery list items reference the ingredient and optional measurement unit.

## ERD maintenance

When the schema changes, update the SQL table scripts and this diagram in the same pull request. The diagram must match the actual primary and foreign keys in `Sql/01_CreateTables.sql`.
