# MealWise — Project Proposal

## Project overview

MealWise is a desktop meal-planning application built with C# Windows Forms and SQL Server. It helps a user discover recipes, maintain a personal recipe library, organize meals across a week, and create a grocery list from planned meals.

## Problem statement

Planning meals across a week can involve several disconnected tasks: finding recipes, remembering ingredients, deciding which meal belongs on which day, and keeping track of grocery items. MealWise brings these tasks into one application and stores related information in a normalized relational database.

## Project goals

1. Build an accessible, easy-to-navigate Windows desktop interface.
2. Create a relational database with at least eight tables, PKs, FKs, referential integrity, and a documented 3NF design.
3. Implement complete create, read, update, and delete operations for recipes, meal plans, planned meals, and grocery items.
4. Provide search, sorting, and field validation.
5. Integrate an external recipe API in a way that supports a real workflow: find a recipe, review the full details, and save it to the local recipe library for later planning.
6. Provide SQL scripts, ERD, technical documentation, a user guide, and a developer guide.
7. Use GitHub issues, milestones, branches, pull requests, and code reviews for team collaboration.

## Intended users

A person or household member who wants to save recipes, plan breakfast/lunch/dinner for a week, and organize the ingredients needed for shopping.

## Core features

- Recipe discovery through TheMealDB API
- Local recipe library with CRUD, search, and sorting
- Weekly meal plan creation and editing
- Assignment of a saved recipe to a date and meal type
- Grocery list associated with a meal plan
- Grocery item check-off and list maintenance
- Dashboard and database reporting queries

## External API

TheMealDB V1 REST API will be used for recipe search and full recipe lookup. Search results are not merely displayed: the user can retrieve a recipe's full details and import the recipe and its ingredients into SQL Server. The saved recipe can then be used by the local meal planner and grocery-list modules.

## Database overview

The proposed database contains 15 tables: AppUsers, DietaryRestrictions, UserDietaryRestrictions, MealTypes, RecipeCategories, Cuisines, Ingredients, MeasurementUnits, Recipes, RecipeIngredients, RecipeDietaryRestrictions, MealPlans, MealPlanEntries, GroceryLists, and GroceryListItems. The ERD and normalization rationale are in `ERD.md` and `3NF_AND_SCHEMA.md`.

## Proposed technology

- C# and .NET 8 Windows Forms
- SQL Server LocalDB/Express
- Microsoft.Data.SqlClient
- TheMealDB V1 API
- GitHub for source control and collaboration

## Success criteria

The project is considered complete when all required features work, the database has at least eight related tables and is documented in 3NF, SQL scripts demonstrate required operations and queries, recipe API data is imported into the local database, the UI validates user input, the required documents are present, and both teammates demonstrate their contributions through GitHub history and the final presentation.

## Scope limitations

The initial capstone version is a desktop application for a single local SQL Server database. It does not include cloud synchronization, paid recipe services, user account authentication, automatic nutrition analysis, or a mobile application. API results should be reviewed by the user; recipe import does not certify dietary suitability or allergen safety.
