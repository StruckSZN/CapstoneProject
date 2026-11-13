# Database Schema and Third Normal Form

## Table inventory

| Table | Purpose | Primary key |
|---|---|---|
| AppUsers | Local user profile/owner of meal plans | UserId |
| DietaryRestrictions | Controlled list of dietary restriction labels | RestrictionId |
| UserDietaryRestrictions | Restrictions selected by a user | UserId + RestrictionId |
| MealTypes | Breakfast, lunch, dinner, snack | MealTypeId |
| RecipeCategories | Recipe category lookup values | CategoryId |
| Cuisines | Cuisine/region lookup values | CuisineId |
| Recipes | Saved recipe details and external source identifiers | RecipeId |
| Ingredients | Canonical ingredient names | IngredientId |
| MeasurementUnits | Units used to quantify recipe/grocery ingredients | UnitId |
| RecipeIngredients | Ingredient and amount details for each recipe | RecipeIngredientId |
| RecipeDietaryRestrictions | Dietary labels associated with recipes | RecipeId + RestrictionId |
| MealPlans | A user's named weekly plan | MealPlanId |
| MealPlanEntries | A recipe scheduled for a date and meal type | MealPlanEntryId |
| GroceryLists | Grocery list associated with a meal plan | GroceryListId |
| GroceryListItems | Ingredient items and purchase status on a list | GroceryListItemId |

## Normalization rationale

### First Normal Form (1NF)

- Each table has a primary key.
- Each column holds a single value rather than a repeating group or comma-separated list.
- Recipe ingredients are stored as separate rows in `RecipeIngredients`, not as a text list inside `Recipes`.
- A planned breakfast, lunch, dinner, or snack is represented by a separate `MealPlanEntries` row.

### Second Normal Form (2NF)

- Tables with a single-column primary key have all non-key attributes dependent on that key.
- The two many-to-many bridge tables, `UserDietaryRestrictions` and `RecipeDietaryRestrictions`, use composite primary keys. They contain no non-key attribute that depends on only part of the composite key.
- `RecipeIngredients` uses its own row identifier; each row describes one ingredient's use in one recipe.

### Third Normal Form (3NF)

- Lookup descriptions are stored once in lookup tables. For example, category names are stored in `RecipeCategories`, cuisine names in `Cuisines`, and meal type names in `MealTypes`; recipes and meal entries store only their foreign keys.
- Ingredient names are stored in `Ingredients`, and unit names/abbreviations are stored in `MeasurementUnits`. Recipe and grocery rows reference those entities instead of repeating their descriptive values.
- User restriction names and recipe restriction names are stored once in `DietaryRestrictions`; bridge tables store only relationship keys and relationship-specific notes where applicable.
- `Recipes` stores recipe-specific attributes and foreign keys to its category/cuisine. It does not store category/cuisine descriptions.
- No non-key attribute is intentionally dependent on another non-key attribute. The week end date is calculated by the UI from `WeekStartDate` rather than stored redundantly.

## Referential integrity

Foreign keys are created in `Sql/01_CreateTables.sql`. Cascading deletes are limited to dependent rows that should not exist without their parent (for example, deleting a meal plan removes its entries and grocery list). A recipe referenced by a meal-plan entry cannot be deleted until its planned use is resolved, preventing accidental loss of meal-plan history.

## Constraints and indexes

- Unique constraints prevent duplicate lookup names.
- A filtered unique index prevents duplicate imports of the same TheMealDB recipe ID.
- Check constraints prevent negative time/quantity values and zero servings.
- A unique key prevents two entries occupying the same meal type on the same date within one plan.
- Indexes support recipe name, ingredient lookup, plan-by-week, and grocery-list status queries.

The SQL scripts are the authoritative schema. Update this document and `ERD.md` whenever a schema change is accepted.
