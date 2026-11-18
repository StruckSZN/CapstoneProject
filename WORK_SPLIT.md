# Work Split — MealWise Capstone

This document records the original responsibility split and the shared integration contract. It is a planning agreement, not evidence that a feature has been completed. Update ownership with the teammate before changing assignments.

## Original ownership — project foundation

1. SQL schema, seed scripts, and reporting examples: `00_CreateDatabase.sql` through `04_Complex_Select_Queries.sql`.
2. Data model documentation: `ERD.md`, `3NF_AND_SCHEMA.md`.
3. Database/API foundation: `DatabaseService.cs`, `RecipeImportRepository.cs`, `MealDbApiService.cs`, and API model files.
4. The **Find Recipes (API)** screen and transactional import workflow.
5. Dashboard and main navigation/application shell.
6. Initial proposal, API documentation, and developer guide.

## Original ownership — local application modules

1. Recipe Library browsing, CRUD, search, and sorting.
2. Weekly Meal Planner CRUD and meal-entry assignment.
3. Grocery List generation, CRUD, and purchased-item check-off.
4. Form validation, useful error messages, and module testing.
5. User-guide updates, screenshots, and final usage instructions.

## Shared responsibilities

- Agree on feature scope and schema changes before implementation.
- Create GitHub issues and use feature branches and reviewed pull requests.
- Each teammate reviews the other's work and records feedback.
- Both run the complete application and test cross-module workflows.
- Both contribute to meeting notes, sprint planning, final reflection, and presentation.

## Integration contract

- `01_CreateTables.sql` is the database contract. It defines 15 tables and the PK/FK relationships.
- Use `DatabaseService.CreateConnection()` and parameterized SQL.
- API imports write to `Recipes` and `RecipeIngredients`; the local library must use those same tables, not a second recipe store.
- The planner uses `MealPlans` and `MealPlanEntries`.
- The grocery module uses `GroceryLists` and `GroceryListItems`.
- One grocery list is allowed per meal plan.
- Imported provider measures are stored as `MeasureText`. Never sum quantities across different or unknown units. Preserve original text if a safe numeric conversion is unavailable.
- Keep `main` buildable and use pull requests for integration.

## Feature acceptance criteria

### Recipe Library
- Read all saved recipes and display useful details.
- Create, edit, and delete recipes with validation.
- Search by name and sort results.
- Preserve source metadata and ingredient relationships.

### Weekly Meal Planner
- Create, rename, and delete a plan.
- Add, edit, and remove planned entries.
- Enforce one recipe per date/meal type, consistent with the database unique constraint.
- Store positive serving counts and display the selected week clearly.

### Grocery List
- Create or retrieve the list for a meal plan.
- Generate items from planned recipes without combining incompatible units.
- Add, edit, delete, and mark items purchased.
- Preserve provider measure text where numeric normalization is not safe.

### Integration and submission
- Fresh database setup works from the SQL scripts.
- API search/import and all local modules operate against the same database.
- Build and regression test results are recorded in the pull request.
- User guide and final reflection match the delivered application.
