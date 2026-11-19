# TheMealDB API

## How the app uses the API

MealWise uses TheMealDB to search for recipes and get their full details.

1. Search for a meal name in the recipe search screen.
2. Select a result to load its full details.
3. Review the category, cuisine, ingredients, measurements, instructions, and image.
4. Save the recipe to the local recipe library.
5. The app stores the recipe and its ingredients in SQL Server. The saved recipe can then be used in a meal plan and grocery list.

## Provider information

- Provider: [TheMealDB](https://www.themealdb.com/)
- API guide: [https://www.themealdb.com/api.php](https://www.themealdb.com/api.php)
- Terms: [https://www.themealdb.com/terms_of_use.php](https://www.themealdb.com/terms_of_use.php)
- Base URL: `https://www.themealdb.com/api/json/v1/1/`
- Format: HTTPS and JSON
- The educational version uses the shared test key `1`.

Check TheMealDB's current terms before distributing the application publicly. The shared key is intended for learning and development.

## Endpoints

| Endpoint | Example | What it returns |
|---|---|---|
| Search by name | `search.php?s=chicken` | Matching recipe summaries |
| Full meal lookup | `lookup.php?i=52772` | Full recipe information and ingredients |

## Data saved by MealWise

| API value | Database location |
|---|---|
| `idMeal` | `Recipes.SourceRecipeId` |
| Provider name | `Recipes.SourceName` |
| `strMeal` | `Recipes.RecipeName` |
| `strCategory` | `RecipeCategories` |
| `strArea` | `Cuisines` |
| `strInstructions` | `Recipes.Instructions` |
| `strMealThumb` | `Recipes.ImageUrl` |
| `strSource` | `Recipes.SourceUrl` |
| Ingredient and measure pairs | `Ingredients` and `RecipeIngredients` |

The API returns ingredient and measurement values in numbered fields, from 1 to 20. The service reads those pairs and skips blank ingredient fields.

The import code checks whether the recipe was already saved using its TheMealDB ID. If it exists, the recipe is updated instead of adding another copy. The recipe and ingredient changes are saved in a database transaction.

## Error handling

- An empty search term is rejected.
- If there are no results, the app shows a message.
- Network and API errors are shown to the user.
- If saving a recipe fails, the database transaction is rolled back.

## Limitations and attribution

TheMealDB recipe information may be community-contributed. The app does not provide nutrition facts or guarantee allergen information. Users should check recipe details and ingredient labels themselves. Recipe search screens include attribution to TheMealDB.
