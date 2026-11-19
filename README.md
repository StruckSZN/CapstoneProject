# MealWise - Meal Planner Capstone

MealWise is a Windows desktop meal-planning app built with C# Windows Forms, .NET 8, and SQL Server. It lets users search for recipes, save recipes, plan meals for the week, and make a grocery list.

The project was worked on by a two-person team.

## Main features

- Dashboard with a database connection check
- Recipe search using TheMealDB
- Save recipes and ingredients to SQL Server
- Recipe Library with add, edit, delete, search, and sorting
- Weekly Meal Planner for creating plans and scheduling meals
- Grocery List linked to a meal plan
- SQL scripts for creating and checking the database

## Requirements

- Windows 10 or 11
- Visual Studio 2022 with the .NET desktop development workload
- .NET 8 SDK
- SQL Server LocalDB or SQL Server Express
- Internet connection for recipe search

## Setup

1. Open `MealWise.csproj` in Visual Studio.
2. Make sure SQL Server LocalDB or SQL Server Express is installed.
3. Run the SQL files in SQL Server Management Studio in this order:
   - `00_CreateDatabase.sql`
   - `01_CreateTables.sql`
   - `02_InsertSampleData.sql`
   - `05_VerifyDatabase.sql`
   - `06_SmokeTest.sql`
4. Copy `appsettings.example.json` to `appsettings.json` if you do not already have a local settings file.
5. Set the connection string for your SQL Server instance. The app can also read `CAPSTONE_CONNECTION_STRING` from the environment.
6. Restore NuGet packages, build the project, and run it.
7. Use the Dashboard screen to check the database connection.

Run the table creation script on a new, empty `MealPlannerDb` database. Do not run it again on a database that already has the tables.

The files `03_Data_Change_Examples.sql` and `04_Complex_Select_Queries.sql` contain additional SQL examples.

## Source code folders

| Folder/file | Purpose |
|---|---|
| `Data/` | Database connection and recipe import |
| `Models/` | Recipe data models |
| `Services/` | TheMealDB API service |
| `Forms/` | Main window and application screens |
| `Program.cs` | Starts the application |
| `AppSettings.cs` | Reads application settings |
| `MealWise.csproj` | Project settings |

The SQL scripts and project documents are kept in the repository root.

## TheMealDB

The app uses TheMealDB's public educational API key. Recipe information and pictures come from TheMealDB. The source should be credited if the project is shared publicly.

## Limitations

- The app does not calculate nutrition information.
- It does not confirm that a recipe is safe for a certain allergy or diet.
- Some ingredient measurements are kept as text because they cannot always be converted safely into a number and unit.
- This is a school project, not a medical or food-safety tool.
