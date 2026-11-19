# MealWise User Guide

## Starting the app

1. Open the project in Visual Studio or run the built application.
2. Go to the Dashboard.
3. Select **Test database connection** to make sure the app can connect to SQL Server.

## Setting up the database

1. Open SQL Server Management Studio.
2. Run `00_CreateDatabase.sql`.
3. Select the new database and run `01_CreateTables.sql`.
4. Run `02_InsertSampleData.sql` to add the sample data.
5. Run `05_VerifyDatabase.sql` to check the tables and sample data.
6. Run `06_SmokeTest.sql` to test some database operations. The test uses a transaction and rolls back the test records.
7. Set the connection string in `appsettings.json` or use the `CAPSTONE_CONNECTION_STRING` environment variable.

Do not run `01_CreateTables.sql` again after the tables have already been created.

## Searching for recipes

1. Open **Find Recipes (API)**.
2. Type a recipe name, such as chicken, pasta, or salmon.
3. Select **Search recipes**.
4. Select a result to see the full recipe.
5. Review the ingredients, measurements, and instructions.
6. Select **Save to recipe library** to save it in the database.

## Using the Recipe Library

The Recipe Library shows recipes saved in the database. You can search for a recipe, sort the list, add a recipe, edit a recipe, or delete one.

## Using the Weekly Meal Planner

1. Open **Weekly Meal Planner**.
2. Create a meal plan and choose the week.
3. Add a saved recipe to a date and meal type.
4. Set the number of servings.
5. Edit or remove planned meals when needed.

The app checks that meals are added to the selected week and does not allow two entries in the same meal slot.

## Using the Grocery List

1. Open **Grocery List**.
2. Select a meal plan and generate its grocery list.
3. Add or edit grocery items if needed.
4. Remove items that are not needed.
5. Check off items after they are purchased.

## Things to keep in mind

- Recipe details come from TheMealDB, so check the information before using it.
- The app does not calculate nutrition or confirm that recipes meet allergy or dietary needs.
- Some measurements stay in the original text format when a safe conversion is not available.
