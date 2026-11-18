# MealWise Developer Guide

## Tools used

- C#
- .NET 8 Windows Forms
- SQL Server LocalDB or SQL Server Express
- Microsoft.Data.SqlClient
- TheMealDB API

## Project files

The source files are currently kept in the main project folder.

- Main window and screens: `MainForm.cs`, `DashboardControl.cs`, and the other control files
- Database code: `DatabaseService.cs` and `RecipeImportRepository.cs`
- API code: `MealDbApiService.cs`
- API data classes: `ExternalMealDetails.cs` and `ExternalMealSummary.cs`
- SQL scripts: `00_CreateDatabase.sql` through `06_SmokeTest.sql`
- Documentation: the Markdown files in the project folder

## Setting up a development copy

1. Install Visual Studio 2022 with the .NET desktop development workload.
2. Install .NET 8 and SQL Server LocalDB or Express.
3. Clone the repository and open `MealWise.csproj`.
4. Run the database scripts in order: 00, 01, and 02.
5. Run `05_VerifyDatabase.sql` and `06_SmokeTest.sql`.
6. Set the connection string in `appsettings.json), or use the `CAPSTONE_CONNECTION_STRING` environment variable.
7. Restore packages, build, and run the project.

Only run `01_CreateTables.sql` on a new database. It creates the tables and is not a migration script.

## Database code

- Use `DatabaseService.CreateConnection()` when opening a SQL connection.
- Dispose connections, commands, and readers when finished.
- Use SQL parameters for values instead of adding user input directly into SQL strings.
- Keep the primary-key and foreign-key relationships working.
- Update the ERD and database documentation if the schema changes.

The sample user in the seed data uses `demo@mealwise.local`.

## API code

- API requests are handled in `MealDbApiService`.
- Check that the search field is not empty before sending a request.
- Handle an empty API response.
- TheMealDB ingredient fields are numbered properties in the response.
- Recipe imports are saved through `RecipeImportRepository` in a SQL transaction.

## Ingredient measurements

The API measurements are saved in `RecipeIngredients.MeasureText`. The text is kept as provided because not every measurement can be safely changed into a number and standard unit. Grocery list totals should not combine different units, such as cups and grams.

## SQL scripts

1. `00_CreateDatabase.sql` creates the database.
2. `01_CreateTables.sql` creates the tables.
3. `02_InsertSampleData.sql` adds sample data.
4. `03_Data_Change_Examples.sql` shows insert, update, and delete examples.
5. `04_Complex_Select_Queries.sql` has reporting queries.
6. `05_VerifyDatabase.sql` checks the database setup.
7. `06_SmokeTest.sql` runs database tests inside a transaction and rolls back the test data.

## Working on the project

Use GitHub issues to keep track of work. Make changes on a branch instead of directly on `main). Before merging, build the project and test the changes. Update the user guide if the way a screen works changes.
