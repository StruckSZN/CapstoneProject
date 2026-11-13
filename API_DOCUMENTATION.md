# External API Documentation — TheMealDB

## Purpose and meaningful use

MealWise uses TheMealDB to help users discover recipes. The API is part of a real application workflow:

1. A user searches by meal name in the **Find Recipes (API)** screen.
2. The app displays matching recipe names and thumbnails.
3. The user selects a result; the app requests the full recipe record.
4. The app displays category, cuisine/area, ingredients, measures, instructions, and image.
5. The user clicks **Save to recipe library**.
6. MealWise saves the recipe and ingredient relationships to SQL Server in a single transaction. The saved recipe can then be scheduled in a weekly meal plan and used by the grocery-list feature.

This goes beyond displaying a static API response: API data becomes usable application data and participates in the app's core meal-planning workflow.

## Provider and official documentation

- Provider: [TheMealDB](https://www.themealdb.com/)
- API guide: [https://www.themealdb.com/api.php](https://www.themealdb.com/api.php)
- Terms: [https://www.themealdb.com/terms_of_use.php](https://www.themealdb.com/terms_of_use.php)
- Base URL used by this application: `https://www.themealdb.com/api/json/v1/1/`
- Format: HTTPS / JSON
- Authentication for the educational sample: shared test key `1` in the URL path

The provider's documentation identifies key `1` as a development/learning test key. The provider's terms restrict app-store publication under free access unless the developer has a paid supporter subscription. The team's use is for an educational capstone demonstration; re-check the terms and obtain the required access before any app-store publication.

## Endpoints used

| Endpoint | Example | Use |
|---|---|---|
| Search by name | `search.php?s=chicken` | Returns a list of matching meal summaries |
| Full meal lookup | `lookup.php?i=52772` | Returns full instructions, ingredient/measure pairs, category, area, and image URL |

## Request/response behavior

- Requests use `HttpClient` over HTTPS.
- The search term and recipe ID are URL-encoded.
- Non-success HTTP status codes are handled as API failures.
- A response with `meals: null` is treated as no search results.
- The full meal response includes ingredient and measure values as numbered properties (`strIngredient1`/`strMeasure1` through index 20). `MealDbApiService` iterates those pairs and removes blank ingredient slots.
- The app does not assume that recipe data contains nutrition facts or guaranteed allergen information.

## Local database mapping

| TheMealDB data | MealWise table/column |
|---|---|
| `idMeal` | `Recipes.SourceRecipeId` |
| Constant provider name | `Recipes.SourceName` = `TheMealDB` |
| `strMeal` | `Recipes.RecipeName` |
| `strCategory` | `RecipeCategories.CategoryName`, referenced by `Recipes.CategoryId` |
| `strArea` | `Cuisines.CuisineName`, referenced by `Recipes.CuisineId` |
| `strInstructions` | `Recipes.Instructions` |
| `strMealThumb` | `Recipes.ImageUrl` |
| `strSource` | `Recipes.SourceUrl` |
| Ingredient/measure pairs | `Ingredients` and `RecipeIngredients.MeasureText` |

The import repository creates/reuses lookup rows, inserts or updates the recipe by provider ID, replaces its ingredient bridge rows, and commits the transaction. Duplicate imports update the existing local recipe rather than creating a second copy.

## Error handling

- Empty search terms are rejected before calling the API.
- No search results produce an informational message.
- API/network errors produce an error message and leave the local database unchanged.
- Database import uses a SQL transaction; any failure rolls back the recipe and ingredient changes.

## Attribution and limitations

The recipe search page displays attribution to TheMealDB. Recipe data may be community-contributed. The app is not a nutrition calculator and must not present API recipes as medically or allergen safe. Users should check recipe details and ingredient labels themselves. For the capstone, the shared test key is intended for education/development; public release requires reviewing and complying with the provider's current terms.
