namespace MealWise.Models;

public sealed class ExternalMealDetails
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Category { get; init; }
    public string? Area { get; init; }
    public string? Instructions { get; init; }
    public string? ImageUrl { get; init; }
    public string? SourceUrl { get; init; }
    public string? YoutubeUrl { get; init; }
    public List<ExternalMealIngredient> Ingredients { get; init; } = [];
}

public sealed record ExternalMealIngredient(string Name, string? MeasureText);
