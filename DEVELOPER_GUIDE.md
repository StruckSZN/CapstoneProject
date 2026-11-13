# MealWise Developer Guide

## Architecture

- `Forms/` contains the WinForms UI controls and main window.
- `Data/DatabaseService.cs` creates SQL Server connections and tests connectivity.
- `Data/RecipeImportRepository.cs` persists TheMealDB recipe details using parameterized SQL and a transaction.
- `Services/MealDbApiService.cs` handles HTTPS requests and converts TheMealDB JSON into typed models.
- `Models/` contains data-transfer models used by the API integration.
- `Sql/` contains the authoritative database build, seed, data-change, and reporting scripts.
- `Docs/` contains proposal, ERD, normalization, API, sprint, and user/developer documentation.

## Local setup

1. Install Visual Studio 2022 with .NET desktop development and SQL Server LocalDB/Express.
2. Clone the GitHub repository.
3. Open `MealWise.csproj` and restore NuGet packages.
4. Run SQL scripts 00, 01, and 02 in order.
5. Configure the SQL Server connection in `appsettings.json`.
6. Build and run the WinForms project.

## Database access conventions

- Use `DatabaseService.CreateConnection()` to create a SQL connection.
- Use `using` / `await using` to dispose connections, commands, readers, and transactions.
- Parameterize values. Never concatenate user input into a SQL statement.
- Preserve PK/FK constraints; do not use a UI-only workaround for referential integrity.
- Update `ERD.md` and `3NF_AND_SCHEMA.md` in the same PR as any schema changes.

## API conventions

- Keep HTTP calls in `MealDbApiService`; do not call the API directly from every form.
- Use cancellation tokens for future long-running or cancellable API operations.
- Validate empty search input before making a request.
- Treat null `meals` as no results.
- TheMealDB V1 ingredients are numbered properties, not a JSON array; use the existing parsing loop.
- Persist imports through `RecipeImportRepository`, which wraps parent recipe and ingredient changes in one SQL transaction.
- Preserve provider attribution and review the provider terms before any public release.

## SQL build order

1. `00_CreateDatabase.sql`
2. `01_CreateTables.sql`
3. `02_InsertSampleData.sql`
4. `03_Data_Change_Examples.sql` (demonstrates insert/update/delete)
5. `04_Complex_Select_Queries.sql` (reporting/examples)

Scripts 03 and 04 are demonstration scripts and may be run after each fresh seed. Do not run script 01 against an existing populated database.

## Pull request expectations

- One issue and one branch per feature/fix where practical.
- Include a short summary, testing steps, and screenshots for UI changes.
- Request review from the other student.
- Do not commit credentials, connection passwords, build output, or local database files.
