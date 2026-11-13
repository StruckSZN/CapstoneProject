namespace MealWise.Models;

public sealed record ExternalMealSummary(string Id, string Name, string ImageUrl)
{
    public override string ToString() => Name;
}
