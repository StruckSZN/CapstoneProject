# MealWise - Meal Planner Capstone

MealWise is a Windows desktop app made with C#, .NET 8 Windows Forms, and SQL Server. The app lets users search for recipes, save them, plan meals for the week, and make a grocery list.

This project was completed as a team by Austin Carter and a classmate.

## Project status

The main parts of the application are in place:

- Dashboard and database connection check
- Recipe search using TheMealDB
- Saving recipes and ingredients to SQL Server
- Recipe Library with add, edit, delete, search, and sorting
- Weekly Meal Planner for creating plans and adding meals
- Grocery List creation and item management
- SQL scripts for creating, filling, and checking the database

The database has 15 tables. The Windows .NET 8 build passed, and the database check and SQL smoke test passed in the local test setup. The main screens were also tested in the Windows application.

## What is needed

- Windows 10 or 11
- Visual Studio 2022 with the .NET desktop development workload
- .NET 8 SDK
- SQL Server LocalDB or SQL Server Express
- Internet connection for recipe search

## Getting started

1. Open `MealWise.csproj` in Visual Studio.
2. Make sure SQL Server LocalDB or SQL Server Express is installed.
3. Open the SQL files in SQL Server Management Studio and run them in this order:
   - `00_CreateDatabase.sql`
   - `01_CreateTables.sql`
   - `02_InsertSampleData.sql`
   - `05_VerifyDatabase.sql`
   - `06_SmokeTest.sql`
4. Copy `appsettings.example.json` to `appsettings.json` if needed.
5. Set the database connection string for your SQL Server instance. You can also use the `CAPSTONE_CONNECTION_STRING` environment variable.
6. Restore NuGet packages, build the project, and run the app.
7. Use the Dashboard screen to test the database connection.

Run the table creation script only on a new, empty `MealPlannerDb` database. It is not meant to be run again on a database that already has the tables.

The files `03_Data_Change_Examples.sql` and `04_Complex_Select_Queries.sql` are examples and are not needed for the first setup.

## Main files

| File | What it does |
|---|---|
| `MealWise.csproj` | Project settings |
| `MainForm.cs` | Main application window |
| `DashboardControl.cs` | Dashboard and database check |
| `MealDbSearchControl.cs` | Recipe search screen |
| `MealDbApiService.cs` | Connects to TheMealDB |
| `RecipeImportRepository.cs` | Saves imported recipes |
| `DatabaseService.cs` | Database connection |
| `RecipeLibraryControl.cs` | Local recipe library |
| `WeeklyMealPlannerControl.cs` | Weekly meal planning |
| `GroceryListControl.cs` | Grocery list screen |
| `00_*.sql` through `06_*.sql` | Database setup and test scripts |

## TheMealDB

The app uses TheMealDB's public educational API key. Recipe information and pictures come from TheMealDB. The source should be credited if the project is shared publicly.

## Notes

- The app does not calculate nutrition information.
- It does not guarantee that a recipe is safe for a certain allergy or diet.
- Some ingredient measurements are saved as the original text because they cannot always be converted safely into a number and unit.
- This is a school project and is not meant to be used as a medical or food-safety tool.
