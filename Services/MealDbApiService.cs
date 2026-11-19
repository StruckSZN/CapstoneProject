using System.Net;
using System.Text.Json;
using MealWise.Models;

namespace MealWise.Services;

/// <summary>
/// Client for TheMealDB's free V1 educational API. The public test key is "1".
/// API documentation: https://www.themealdb.com/api.php
/// </summary>
public sealed class MealDbApiService
{
    private static readonly HttpClient Http = new()
    {
        BaseAddress = new Uri("https://www.themealdb.com/api/json/v1/1/"),
        Timeout = TimeSpan.FromSeconds(20)
    };

    public async Task<IReadOnlyList<ExternalMealSummary>> SearchMealsAsync(
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            throw new ArgumentException("Enter a meal name to search.", nameof(searchTerm));
        }

        using var document = await GetJsonAsync(
            $"search.php?s={Uri.EscapeDataString(searchTerm.Trim())}", cancellationToken);

        if (!document.RootElement.TryGetProperty("meals", out var meals)
            || meals.ValueKind == JsonValueKind.Null)
        {
            return [];
        }

        var results = new List<ExternalMealSummary>();
        foreach (var meal in meals.EnumerateArray())
        {
            var id = ReadString(meal, "idMeal");
            var name = ReadString(meal, "strMeal");
            if (!string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(name))
            {
                results.Add(new ExternalMealSummary(
                    id,
                    name,
                    ReadString(meal, "strMealThumb") ?? string.Empty));
            }
        }

        return results;
    }

    public async Task<ExternalMealDetails> GetMealDetailsAsync(
        string externalMealId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(externalMealId))
        {
            throw new ArgumentException("A recipe ID is required.", nameof(externalMealId));
        }

        using var document = await GetJsonAsync(
            $"lookup.php?i={Uri.EscapeDataString(externalMealId.Trim())}", cancellationToken);

        if (!document.RootElement.TryGetProperty("meals", out var meals)
            || meals.ValueKind != JsonValueKind.Array
            || meals.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("The recipe could not be found in TheMealDB.");
        }

        var meal = meals[0];
        var ingredients = new List<ExternalMealIngredient>();

        // TheMealDB V1 returns ingredients and measures as numbered fields (1–20).
        for (var index = 1; index <= 20; index++)
        {
            var ingredientName = ReadString(meal, $"strIngredient{index}")?.Trim();
            var measure = ReadString(meal, $"strMeasure{index}")?.Trim();

            if (!string.IsNullOrWhiteSpace(ingredientName))
            {
                ingredients.Add(new ExternalMealIngredient(
                    ingredientName,
                    string.IsNullOrWhiteSpace(measure) ? null : measure));
            }
        }

        return new ExternalMealDetails
        {
            Id = ReadString(meal, "idMeal") ?? externalMealId,
            Name = ReadString(meal, "strMeal") ?? "Unnamed recipe",
            Category = ReadString(meal, "strCategory"),
            Area = ReadString(meal, "strArea"),
            Instructions = ReadString(meal, "strInstructions"),
            ImageUrl = ReadString(meal, "strMealThumb"),
            SourceUrl = ReadString(meal, "strSource"),
            YoutubeUrl = ReadString(meal, "strYoutube"),
            Ingredients = ingredients
        };
    }

    private static async Task<JsonDocument> GetJsonAsync(
        string relativeUrl,
        CancellationToken cancellationToken)
    {
        using var response = await Http.GetAsync(relativeUrl, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new InvalidOperationException("The recipe service did not find that request.");
        }

        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value)
            || value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : value.ToString();
    }
}
