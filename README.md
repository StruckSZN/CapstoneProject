# MealWise — Meal Planner Capstone

MealWise is a Windows desktop meal-planning application built with C#/.NET 8 Windows Forms and SQL Server. It combines a local recipe library with TheMealDB recipe search and a normalized relational database.

## Current project status

- **Implemented:** WinForms application shell, dashboard/database connectivity check, TheMealDB search and recipe details, transactional recipe import, SQL Server schema and seed/reporting scripts.
- **Implemented on the `fix/capstone-readiness` branch:** Recipe Library CRUD/search, Weekly Meal Planner plan/entry management, and Grocery List generation/item management. The latest Windows CI build passed; live SQL Server integration and end-to-end UI testing remain outstanding.
- **Database:** 15 normalized tables are defined in `01_CreateTables.sql`.

## Requirements

- Windows 10/11
- Visual Studio 2022 with the **.NET desktop development** workload
- .NET 8 SDK
- SQL Server LocalDB or SQL Server Express
- Internet access for TheMealDB recipe search

## Quick start

1. Clone this repository and open `MealWise.csproj` in Visual Studio.
2. Ensure SQL Server LocalDB is installed and running.
3. Open the SQL scripts in SQL Server Management Studio (SSMS) and execute them in order:
   - `00_CreateDatabase.sql`
   - `01_CreateTables.sql`
   - `02_InsertSampleData.sql`
   - `05_VerifyDatabase.sql` (read-only verification; run after the sample data script)\n   - `06_SmokeTest.sql` (transactional CRUD/relationship smoke test; test rows are rolled back)
4. Configure the connection string:
   - Copy `appsettings.example.json` to `appsettings.json` if needed.
   - Set `ConnectionStrings:DefaultConnection` to your SQL Server instance.
   - Alternatively set the `CAPSTONE_CONNECTION_STRING` environment variable; it takes precedence.
5. In Visual Studio, restore NuGet packages and build the project.
6. Run the application. On Dashboard, select **Test database connection**.

> Run `01_CreateTables.sql` only against a newly created/empty `MealPlannerDb`. It is not an idempotent migration and will fail if its tables already exist. Scripts `03_Data_Change_Examples.sql` and `04_Complex_Select_Queries.sql` are examples, not required setup steps.

## Repository layout

The project currently keeps source and documentation files at the repository root (rather than in the future `Forms/`, `Data/`, `Models/`, `Services/`, `Sql/`, or `Docs/` folders). Key files:

| File | Purpose |
|---|---|
| `MealWise.csproj` | .NET 8 Windows Forms project |
| `MainForm.cs`, `DashboardControl.cs` | Main window and dashboard |
| `MealDbSearchControl.cs`, `MealDbApiService.cs` | External recipe search |
| `RecipeImportRepository.cs`, `DatabaseService.cs` | SQL Server persistence/connectivity |
| `ExternalMealDetails.cs`, `ExternalMealSummary.cs` | API models |
| `00_*.sql`–`06_*.sql` | Database creation, schema, seed, examples, reports, verification, transactional smoke test |
| `ERD.md`, `3NF_AND_SCHEMA.md` | Data model and normalization |
| `DEVELOPER_GUIDE.md`, `USER_GUIDE.md` | Setup and usage documentation |

## TheMealDB

The app uses TheMealDB's public educational V1 API key. Recipe data and images are provided by TheMealDB; retain source attribution and review the provider's current terms before any public/commercial release. Verify recipe content independently.

## Security and configuration

- Never commit real credentials, API secrets, local database files, or machine-specific settings.
- Prefer `CAPSTONE_CONNECTION_STRING` for local or CI configuration.
- `appsettings.example.json` contains a LocalDB example only; it is not a production connection string.
- SQL values must be parameterized. Never concatenate user-provided text into SQL.

## Team workflow

1. Create a GitHub issue describing one feature or bug.
2. Create a feature branch from `main`.
3. Keep commits focused and update documentation when behavior changes.
4. Build and test locally; include exact steps and results in the pull request.
5. Request review from the other teammate. Merge only after review and verification.

See `WORK_SPLIT.md`, `GITHUB_WORKFLOW.md`, and `CONTRIBUTING.md`.

## Known limitations

- Nutrition, allergy, and dietary suitability are not calculated or guaranteed.
- Imported ingredient measures are preserved as source text when they cannot be safely normalized to numeric quantities and units.
- The current project is a student capstone prototype, not a production food-safety or medical tool.
