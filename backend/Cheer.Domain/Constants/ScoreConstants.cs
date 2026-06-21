namespace Cheer.Domain.Constants;

public static class ScoreConstants
{
    public static readonly Dictionary<int, double> PlacementPoints = new()
    {
        { 1, 100.0 },
        { 2, 70.0 },
        { 3, 50.0 },
        { 4, 30.0 },
        { 5, 20.0 },
    };

    public const double DefaultPlacementPoints = 10.0;

    public static readonly Dictionary<string, double> ImportanceWeights = new()
    {
        { "Internacional", 3.0 },
        { "Nacional", 2.5 },
        { "Estadual", 2.0 },
        { "Regional", 1.7 },
        { "Municipal", 1.5 },
    };

    public const double DefaultImportanceWeight = 1.0;

    public static readonly Dictionary<int, double> LevelWeights = new()
    {
        { 1, 1.1 },
        { 2, 1.2 },
        { 3, 1.3 },
        { 4, 1.4 },
        { 5, 1.5 },
    };

    public const double DefaultLevelWeight = 1.0;

    public static readonly Dictionary<string, double> CategoryWeights = new()
    {
        { "Team Cheer", 1.5 },
        { "Categoria de Grupo", 1.2 },
        { "Coed", 1.2 },
        { "All Girl", 1.2 },
        { "All Boy", 1.2 },
        { "Elite", 1.2 },
        { "Partner / Duplas", 1.1 },
        { "Skills Individuais", 0.9 },
        { "Best Jump", 0.9 },
        { "Best Tumbling", 0.9 },
        { "Best Basket", 0.9 },
        { "Best Cheer", 0.9 },
    };

    public const double DefaultCategoryWeight = 1.0;

    public const double YearDecayRate = 0.1;
    public const double MinDecay = 0.0;
}
