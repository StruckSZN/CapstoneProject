# MealWise Database Schema — Data Dictionary

The SQL Server database is named `MealPlannerDb`. The build script in `Sql/01_CreateTables.sql` is the source of truth; this document describes the columns and relationships for review and grading.

## AppUsers
| Column | Type | Key / rules | Meaning |
|---|---|---|---|
| UserId | int IDENTITY | PK | Local user identifier |
| DisplayName | nvarchar(100) | NOT NULL | User's display name |
| Email | nvarchar(254) | NULL, unique when present | Optional email |
| CreatedAt | datetime2(0) | NOT NULL, default UTC | Creation timestamp |

## DietaryRestrictions
| Column | Type | Key / rules | Meaning |
|---|---|---|---|
| RestrictionId | int IDENTITY | PK | Restriction identifier |
| RestrictionName | nvarchar(80) | NOT NULL, UNIQUE | Restriction label |
| Description | nvarchar(300) | NULL | Description |

## UserDietaryRestrictions
| Column | Type | Key / rules | Meaning |
|---|---|---|---|
| UserId | int | PK, FK → AppUsers | User in the relationship |
| RestrictionId | int | PK, FK → DietaryRestrictions | Restriction in the relationship |
| Notes | nvarchar(250) | NULL | Optional user-specific note |

## MealTypes
| Column | Type | Key / rules | Meaning |
|---|---|---|---|
| MealTypeId | int IDENTITY | PK | Meal type identifier |
| TypeName | nvarchar(40) | NOT NULL, UNIQUE | Breakfast, Lunch, Dinner, Snack |
| SortOrder | tinyint | NOT NULL | Display order |

## RecipeCategories
| Column | Type | Key / rules | Meaning |
|---|---|---|---|
| CategoryId | int IDENTITY | PK | Category identifier |
| CategoryName | nvarchar(80) | NOT NULL, UNIQUE | Category name |

## Cuisines
| Column | Type | Key / rules | Meaning |
|---|---|---|---|
| CuisineId | int IDENTITY | PK | Cuisine identifier |
| CuisineName | nvarchar(80) | NOT NULL, UNIQUE | Cuisine/region name |

## Recipes
| Column | Type | Key / rules | Meaning |
|---|---|---|---|
| RecipeId | int IDENTITY | PK | Local recipe identifier |
| RecipeName | nvarchar(160) | NOT NULL | Recipe title |
| CategoryId | int | FK → RecipeCategories, NULL | Optional category |
| CuisineId | int | FK → Cuisines, NULL | Optional cuisine/area |
| Instructions | nvarchar(max) | NULL | Cooking instructions |
| PrepMinutes | smallint | NULL, CHECK >= 0 | Preparation time |
| CookMinutes | smallint | NULL, CHECK >= 0 | Cooking time |
| DefaultServings | tinyint | NOT NULL, default 4, CHECK > 0 | Base number of servings |
| ImageUrl | nvarchar(1000) | NULL | Recipe image URL |
| SourceName | nvarchar(80) | NULL | External source, e.g. TheMealDB |
| SourceRecipeId | nvarchar(50) | NULL | Provider's recipe ID |
| SourceUrl | nvarchar(1000) | NULL | Provider/source page |
| IsFavorite | bit | NOT NULL, default 0 | Local favorite flag |
| DateAdded | datetime2(0) | NOT NULL, default UTC | Date imported/created |
| UpdatedAt | datetime2(0) | NULL | Last update timestamp |

## Ingredients
| Column | Type | Key / rules | Meaning |
|---|---|---|---|
| IngredientId | int IDENTITY | PK | Ingredient identifier |
| IngredientName | nvarchar(120) | NOT NULL, UNIQUE | Canonical ingredient name |

## MeasurementUnits
| Column | Type | Key / rules | Meaning |
|---|---|---|---|
| UnitId | int IDENTITY | PK | Unit identifier |
| UnitName | nvarchar(50) | NOT NULL, UNIQUE | Full unit name |
| Abbreviation | nvarchar(15) | NOT NULL, UNIQUE | Short display form |

## RecipeIngredients
| Column | Type | Key / rules | Meaning |
|---|---|---|---|
| RecipeIngredientId | int IDENTITY | PK | Bridge-row identifier |
| RecipeId | int | FK → Recipes, ON DELETE CASCADE | Recipe |
| IngredientId | int | FK → Ingredients | Ingredient |
| Quantity | decimal(10,2) | NULL, CHECK > 0 when present | Numeric amount when parsed/entered |
| UnitId | int | FK → MeasurementUnits, NULL | Normalized unit when available |
| MeasureText | nvarchar(100) | NULL | Original provider measure or free-text amount |
| PreparationNote | nvarchar(150) | NULL | e.g. chopped, divided |
| IsOptional | bit | NOT NULL, default 0 | Whether optional |

## RecipeDietaryRestrictions
| Column | Type | Key / rules | Meaning |
|---|---|---|---|
| RecipeId | int | PK, FK → Recipes, ON DELETE CASCADE | Recipe being tagged |
| RestrictionId | int | PK, FK → DietaryRestrictions | Restriction tag |

## MealPlans
| Column | Type | Key / rules | Meaning |
|---|---|---|---|
| MealPlanId | int IDENTITY | PK | Plan identifier |
| UserId | int | FK → AppUsers, ON DELETE CASCADE | Plan owner |
| PlanName | nvarchar(100) | NOT NULL | User-defined plan name |
| WeekStartDate | date | NOT NULL | First day; UI calculates the 7-day range |
| CreatedAt | datetime2(0) | NOT NULL, default UTC | Creation timestamp |

## MealPlanEntries
| Column | Type | Key / rules | Meaning |
|---|---|---|---|
| MealPlanEntryId | int IDENTITY | PK | Planned entry identifier |
| MealPlanId | int | FK → MealPlans, ON DELETE CASCADE | Parent plan |
| RecipeId | int | FK → Recipes | Selected recipe |
| MealTypeId | int | FK → MealTypes | Breakfast/lunch/dinner/snack |
| PlannedDate | date | NOT NULL | Scheduled day |
| Servings | tinyint | NOT NULL, default 2, CHECK > 0 | Planned servings |
| Notes | nvarchar(250) | NULL | Optional preparation note |

Unique rule: `(MealPlanId, PlannedDate, MealTypeId)` prevents two recipes from occupying the same meal slot in one plan.

## GroceryLists
| Column | Type | Key / rules | Meaning |
|---|---|---|---|
| GroceryListId | int IDENTITY | PK | List identifier |
| MealPlanId | int | NOT NULL, UNIQUE, FK → MealPlans, ON DELETE CASCADE | One list per plan |
| ListName | nvarchar(100) | NOT NULL | List display name |
| CreatedAt | datetime2(0) | NOT NULL, default UTC | Creation timestamp |

## GroceryListItems
| Column | Type | Key / rules | Meaning |
|---|---|---|---|
| GroceryListItemId | int IDENTITY | PK | Grocery row identifier |
| GroceryListId | int | FK → GroceryLists, ON DELETE CASCADE | Parent grocery list |
| IngredientId | int | FK → Ingredients | Ingredient to purchase |
| Quantity | decimal(10,2) | NULL, CHECK > 0 when present | Optional numeric amount |
| UnitId | int | FK → MeasurementUnits, NULL | Optional normalized unit |
| IsPurchased | bit | NOT NULL, default 0 | Purchased checkbox state |
| Notes | nvarchar(200) | NULL | Optional note |

## Referential integrity and delete behavior

- Child bridge rows for a deleted recipe are removed through `RecipeIngredients` and `RecipeDietaryRestrictions` cascade rules.
- Deleting a meal plan removes its planned entries and its grocery list; deleting a grocery list removes its items.
- A recipe referenced by `MealPlanEntries` is protected from deletion by the foreign key. The UI should show a meaningful message and require the user to remove or reassign planned entries first.
- A lookup value used by another table cannot be deleted until its references are resolved.
