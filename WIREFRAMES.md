# MealWise Wireframes

These are low-fidelity screen plans. The implementation may refine spacing and styling while preserving the required workflow.

## 1. Dashboard

```text
+---------------------------------------------------------------+
| MEALWISE       | Plan your week. Make mealtime easier.        |
|                |                                               |
| Dashboard      | [Database connection: status] [Test]          |
| Find Recipes   |                                               |
| Recipe Library | [Recipes saved] [Meals planned] [Open grocery] |
| Weekly Planner |                                               |
| Grocery List   | Recent plan / quick actions                    |
| Database       |                                               |
+---------------------------------------------------------------+
```

## 2. Find Recipes (API) — implemented in starter

```text
+---------------------------------------------------------------+
| Search [ chicken / pasta / salmon              ] [Search]     |
+----------------------+----------------------------------------+
| Search results       | Recipe title                            |
| - Chicken...         | Category / cuisine                      |
| - Chicken...         | [Recipe image]          [Save to library]|
| - ...                |                                        |
|                      | Ingredients                             |
|                      | - measures and ingredient names         |
|                      |                                        |
|                      | Cooking instructions                    |
+----------------------+----------------------------------------+
| Recipe data provided by TheMealDB                             |
+---------------------------------------------------------------+
```

## 3. Recipe Library — classmate module

- Search box and sort selector (name, category, cuisine, date added).
- Data grid with recipe name, category, cuisine, and favorite status.
- Add, edit, view, and delete controls.
- Detail editor with ingredient rows, amount/unit, instructions, servings, and optional dietary tags.
- API-imported recipes can be edited locally; preserve their source/provider ID when updating.

## 4. Weekly Meal Planner — classmate module

- Select or create a plan with a week-start date.
- Seven-day grid with rows for breakfast, lunch, dinner, and snack.
- Each slot can add, change, or remove a saved recipe and servings.
- Validate that a planned date is within the plan's seven-day window.
- Prevent duplicate use of the same meal-type slot in a plan.

## 5. Grocery List — classmate module

- Select a meal plan and open its associated grocery list.
- Show ingredient, quantity, unit, and purchased checkbox.
- Allow manual add, edit, delete, and mark-purchased actions.
- Provide a generate/refresh-from-plan action; aggregate quantities only when unit values match. Keep API free-text measures visible where a numeric quantity/unit cannot be parsed safely.
- Show remaining and purchased item counts.
