# MealWise Team Work

This project was completed by a two-person team. Work was split between the database/API parts and the application screens, with both team members helping with testing and putting the project together.

## Database and API work

- Database creation and table scripts
- Sample data and SQL query examples
- ERD and database normalization documents
- Database connection code
- TheMealDB API search and recipe import
- Dashboard and main application setup

## Application screen work

- Recipe Library
- Weekly Meal Planner
- Grocery List
- Form checks and error messages
- Testing the screens and fixing issues
- User guide and instructions

## Work done together

- Planning the features
- Agreeing on how the screens use the database
- Testing the full app
- Fixing problems found during testing
- Updating project documents
- Preparing the final presentation and reflection

## How the parts connect

- Recipes are stored in `Recipes` and their ingredients are stored in `RecipeIngredients`.
- The Recipe Library uses the same recipe tables as the API import.
- Meal plans use `MealPlans` and `MealPlanEntries`.
- Grocery lists use `GroceryLists` and `GroceryListItems`.
- Each meal plan has one grocery list.
- Ingredient measurements are kept as text when they cannot be safely converted.

## Main feature requirements

### Recipe Library
- Show saved recipes.
- Add, edit, and delete recipes.
- Search and sort the list.
- Keep imported recipe information and ingredients.

### Weekly Meal Planner
- Create, rename, and delete meal plans.
- Add, edit, and remove planned meals.
- Keep meals within the selected week.
- Use positive serving amounts.

### Grocery List
- Create a list from a meal plan.
- Add, edit, and delete grocery items.
- Mark items as purchased.
- Do not add together measurements that use different units.

### Final checks
- Set up the database using the SQL scripts.
- Test recipe search and import.
- Test the Recipe Library, Meal Planner, and Grocery List.
- Make sure the guides match the current application.
