# MealWise User Guide (Initial Draft)

## Start the application

1. Launch MealWise from Visual Studio or the built application executable.
2. On the Dashboard, use **Test database connection** to confirm that MealPlannerDb is available.

## Find and save a recipe

1. Open **Find Recipes (API)**.
2. Enter a meal name (for example, chicken, pasta, or salmon) and choose **Search recipes**.
3. Select a result to load its full recipe details.
4. Review the recipe name, category, cuisine/area, ingredients, measures, and cooking instructions.
5. Choose **Save to recipe library** to import it into the local database.
6. The Recipe Library module (to be completed) will provide local browsing and editing of saved recipes.

## Recipe Library, Weekly Planner, and Grocery List

These modules are currently assigned for implementation. Update this guide with final screenshots and precise steps after the classmate's screens are integrated.

Expected workflow:

1. Review or add recipes in Recipe Library.
2. Create a meal plan and set its week-start date.
3. Add a saved recipe to a date and meal type, and set the number of servings.
4. Generate a grocery list from the plan.
5. Check off items as they are purchased.

## Important data notes

- The API content is supplied by TheMealDB; verify recipe instructions and ingredients yourself.
- The app does not calculate nutrition values or guarantee allergy/dietary suitability.
- Do not delete a recipe that is already used in a meal plan unless the application clearly explains how the planned entry will be handled.
