# MealWise Developer Guide

## Technology

- C# / .NET 8 Windows Forms
- SQL Server LocalDB or SQL Server Express
- Microsoft.Data.SqlClient
- TheMealDB public educational V1 API

## Repository layout

Source files and documentation currently live at the repository root. The folder names below describe logical responsibilities, not existing directories:

- UI: `MainForm.cs`, `DashboardControl.cs`, `MealDbSearchControl.cs`, and related controls.
- Data access: `DatabaseService.cs`, `RecipeImportRepository.cs`.
- API integration: `MealDbApiService.cs`.
- API models: `ExternalMealDetails.cs`, `ExternalMealSummary.cs`.
- SQL: `00_CreateDatabase.sql` through `05_VerifyDatabase.sql`.
- Documentation: Markdown files at the repository root.

Do not add project-relative paths such as `Forms/`, `Data/`, `Models/`, `Services/`, `Sql/`, or `Docs/` to instructions unless those directories have actually been created and the project has been migrated.

## Local setup

1. Install Visual Studio 2022 with .NET desktop development, .NET 8, and SQL Server LocalDB/Express.
2. Clone the repository and open `MealWise.csproj`.
3. Create the database by running `00_CreateDatabase.sql`, then run `01_CreateTables.sql` and `02_InsertSampleData.sql` in SSMS.
4. Run `05_VerifyDatabase.sql` to verify the required tables, relationships, and seed records.
5. Configure `appsettings.json` using `appsettings.example.json`, or set `CAPSTONE_CONNECTION_STRING`.
6. Restore NuGet packages, build, and run the WinForms application.

Run `01_CreateTables.sql` only against an empty database. It creates tables and is not a migration script. Scripts 03 and 04 are optional examples/reports; script 05 is a read-only setup verification.

## Database access conventions

- Use `DatabaseService.CreateConnection()` to create SQL connections.
- Dispose connections, commands, readers, and transactions with `using` / `await using`.
- Parameterize all values. Never concatenate user input into SQL.
- Preserve primary-key and foreign-key constraints.
- Update `ERD.md` and `3NF_AND_SCHEMA.md` in the same PR as schema changes.
- The seeded demo account is identified by `demo@mealwise.local`; do not assume arbitrary hard-coded user IDs.

## API conventions

- Keep HTTP calls in `MealDbApiService`.
- Validate empty search input before making a request.
- Treat a null `meals` response as no results.
- TheMealDB V1 ingredients are numbered properties, not a JSON array.
- Persist imports through `RecipeImportRepository`, which wraps recipe and ingredient writes in one SQL transaction.
- Preserve TheMealDB attribution and review current provider terms before public release.

## Ingredient quantity integrity

The API import stores provider measurements in `RecipeIngredients.MeasureText` because source strings are not reliably parseable into numeric quantities and standardized units. Grocery aggregation must not add unlike units (for example, cups and grams), or invent a conversion. Preserve the original measure text when conversion is not known.

## SQL build order

1. `00_CreateDatabase.sql`
2. `01_CreateTables.sql`
3. `02_InsertSampleData.sql`
4. `03_Data_Change_Examples.sql` (demonstration insert/update/delete)
5. `04_Complex_Select_Queries.sql` (reporting examples)
6. `05_VerifyDatabase.sql` (read-only setup verification)

Do not run schema creation against an existing populated database.

## Pull request expectations

- One issue and one branch per feature/fix where practical.
- Include a concise summary, testing steps, and screenshots for UI changes.
- Request review from the other student.
- Do not commit credentials, connection passwords, build output, or local database files.
