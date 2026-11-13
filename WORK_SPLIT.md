# Work Split — MealWise Capstone

The division below gives each student ownership of complete deliverables. Both students still review and test the integrated application. Each feature is developed on a separate GitHub branch and merged through a reviewed pull request.

## You — database, API, and application foundation

1. Own `Sql/00_CreateDatabase.sql` through `Sql/04_Complex_Select_Queries.sql`.
2. Own `Docs/ERD.md` and `Docs/3NF_AND_SCHEMA.md`.
3. Own `Data/DatabaseService.cs`, `Data/RecipeImportRepository.cs`, `Models/`, and `Services/MealDbApiService.cs`.
4. Own the `Find Recipes (API)` screen and its recipe-detail/import workflow.
5. Own dashboard and main navigation/application shell.
6. Own `Docs/API_DOCUMENTATION.md`, initial project proposal, and developer-guide draft.
7. Create the repository structure and GitHub issue/milestone framework with the classmate.

**Definition of done:** database scripts run in order; the schema has at least eight tables and valid PK/FK constraints; sample data is present; the ERD matches the schema; SQL examples execute; API search returns results; full recipe details display; selecting Save imports one recipe and its ingredients transactionally; duplicate import updates rather than duplicates.

## Classmate — local application features and user experience

1. Own the Recipe Library screen and its CRUD operations for locally saved recipes.
2. Own the Weekly Meal Planner screen, meal plan CRUD, and meal-entry CRUD.
3. Own the Grocery List screen and grocery-item CRUD.
4. Own search, sorting, form validation, and user-facing error messages across assigned screens.
5. Own wireframe refinement, `Docs/USER_GUIDE.md`, and README screenshots/final usage instructions.
6. Own feature testing notes for their assigned modules.

**Definition of done:** users can create/read/update/delete local recipes; search and sort the library; create/edit/delete a weekly plan; assign one recipe per date/meal type with a serving count; generate/update a grocery list from a plan; add/edit/delete grocery items; mark items purchased; validate input and show useful errors.

## Shared responsibilities

- Both agree on the final feature list and database changes before implementation.
- Both create/resolve GitHub issues and follow sprint milestones.
- Both use feature branches and pull requests; neither emails source code.
- Each reviews the other's pull requests and records review feedback.
- Both run the full application and test cross-module workflows.
- Both contribute to meeting notes, sprint planning, final reflection, and presentation.

## Integration contract

- Database names, table names, columns, and relationships in `Sql/01_CreateTables.sql` are the shared contract. Propose schema changes through an issue and coordinate before changing a table.
- The classmate should use `DatabaseService.CreateConnection()` and parameterized `SqlCommand` statements or agree a shared repository abstraction with You. Never concatenate user-entered text into SQL.
- The API import writes a recipe into `Recipes` and its ingredients into `Ingredients` / `RecipeIngredients`. The Recipe Library module should read those same tables; do not create a separate recipe table.
- The planner stores plan records in `MealPlans` and scheduled slots in `MealPlanEntries`.
- The grocery module stores lists in `GroceryLists` and items in `GroceryListItems`.
- A plan has one grocery list (`GroceryLists.MealPlanId` is unique). Ingredient aggregation must not combine quantities expressed in different units. Leave imported `MeasureText` visible when its amount/unit cannot be safely converted.
- Keep `main` buildable. Open a PR for each completed issue and request review from the other teammate.

## Suggested GitHub issues

| Issue | Owner | Suggested branch |
|---|---|---|
| Create SQL schema and seed scripts | You | `feature/database-schema` |
| Add ERD and 3NF documentation | You | `docs/erd-3nf` |
| Implement TheMealDB search and details | You | `feature/themealdb-search` |
| Import API recipes into SQL Server | You | `feature/recipe-import` |
| Build Recipe Library CRUD/search/sort | Classmate | `feature/recipe-library` |
| Build Weekly Meal Planner CRUD | Classmate | `feature/weekly-planner` |
| Build Grocery List generation and CRUD | Classmate | `feature/grocery-list` |
| Integration and regression testing | Both | `test/integration` |
| Complete docs and presentation | Both | `docs/final-submission` |
