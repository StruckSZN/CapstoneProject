# MealWise User Guide

## Start the application

1. Launch MealWise from Visual Studio or the built application executable.
2. On Dashboard, select **Test database connection** to confirm that the configured SQL Server database is reachable.

## First-time database setup

1. Run `00_CreateDatabase.sql` in SSMS.
2. Run `01_CreateTables.sql` against the new database.
3. Run `02_InsertSampleData.sql` to add lookup data, example recipes, a demo user, and a sample plan.
4. Configure the connection string in `appsettings.json` or set `CAPSTONE_CONNECTION_STRING`.

Do not run `01_CreateTables.sql` repeatedly against an existing database.

## Find and save a recipe

1. Open **Find Recipes (API)**.
2. Enter a meal name (for example, chicken, pasta, or salmon) and choose **Search recipes**.
3. Select a result to load its full recipe details.
4. Review the recipe name, category, cuisine/area, ingredients, measures, and cooking instructions.
5. Choose **Save to recipe library** to import it into the local database.

## Planned local workflows

The Recipe Library, Weekly Meal Planner, and Grocery List navigation destinations are currently placeholders in the checked-in application. The expected final workflow is:

1. Browse, search, sort, and maintain locally saved recipes.
2. Create a weekly meal plan and select its week-start date.
3. Assign a recipe to a date and meal type, then set servings.
4. Generate a grocery list from the selected plan.
5. Add or remove items and mark purchased items.

Do not assume these workflows are available until the corresponding screens have replaced the placeholders and have been tested.

## Important data notes

- Recipe content and images are supplied by TheMealDB. Verify recipe instructions and ingredients yourself.
- The app does not calculate nutrition values or guarantee allergy/dietary suitability.
- Imported measurements may remain as source text if they cannot be safely converted into numeric quantities and standardized units.
- Avoid deleting a recipe referenced by a meal plan unless the application clearly explains how that planned entry will be handled.
