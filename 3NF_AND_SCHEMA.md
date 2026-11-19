# Database Schema and Third Normal Form

## Tables

| Table | Purpose | Primary key |
|---|---|---|
| AppUsers | Local user profile and meal-plan owner | UserId |
| DietaryRestrictions | List of dietary restriction labels | RestrictionId |
| UserDietaryRestrictions | Restrictions selected by a user | UserId + RestrictionId |
| MealTypes | Breakfast, lunch, dinner, and snack | MealTypeId |
| RecipeCategories | Recipe category names | CategoryId |
| Cuisines | Cuisine or region names | CuisineId |
| Recipes | Saved recipe details and source IDs | RecipeId |
| Ingredients | Ingredient names | IngredientId |
| MeasurementUnits | Measurement units | UnitId |
| RecipeIngredients | Ingredients and amounts for recipes | RecipeIngredientId |
| RecipeDietaryRestrictions | Dietary labels for recipes | RecipeId + RestrictionId |
| MealPlans | A user's weekly meal plan | MealPlanId |
| MealPlanEntries | A recipe scheduled for a date and meal type | MealPlanEntryId |
| GroceryLists | Grocery list for a meal plan | GroceryListId |
| GroceryListItems | Items and purchase status | GroceryListItemId |

## First Normal Form (1NF)

- Each table has a primary key.
- Each column stores one value.
- Recipe ingredients are stored in separate rows in `RecipeIngredients`, not as a list in the recipe table.
- Each scheduled meal has its own row in `MealPlanEntries`.

## Second Normal Form (2NF)

- Non-key columns depend on the key for their row.
- The bridge tables `UserDietaryRestrictions` and `RecipeDietaryRestrictions` use composite keys.
- The ingredient rows describe the ingredients used by each recipe.

## Third Normal Form (3NF)

- Category, cuisine, meal type, ingredient, unit, and restriction names are kept in their own tables.
- Recipes and meal entries use foreign keys to refer to those values.
- The bridge tables store the relationships between users, recipes, and dietary restrictions.
- The week end date is calculated from `WeekStartDate` instead of being stored as another date.

## Relationships and constraints

The table creation script is `01_CreateTables.sql`. It defines the foreign keys and other constraints. Some related rows are deleted when their parent meal plan is deleted. A recipe that is still used by a meal-plan entry cannot be deleted until that entry is removed.

The database also uses unique constraints, check constraints, and indexes for common lookups. The SQL scripts are the source for the actual table definitions, so this document should be updated if the schema changes.
