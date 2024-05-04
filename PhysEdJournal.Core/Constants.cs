namespace PhysEdJournal.Core;

public static class Constants
{
    public const int MaxPointsForStandards = 30;
    public const int DaysToDeletePoints = 30;
    public const int PointsLifeDays = 30;
    public const int RequiredPointsAmount = 50;
    public const int MaxPointsAmount = 50;
    public const int MaxPointsForExternalFitness = 10;
    public const int MaxPointsForScience = 30;
    public const int MinTotalPointsToPassStandards = 20;
    public const int MaxPointsForOneStandard = 10;
    public const int VisitLifeDays = 7;
    public const int DaysToDeleteVisit = 7;

    public static double CalculateTotalPoints(
        int visits,
        double visitValue,
        int additionalPoints,
        int pointsForStandards
    ) => Math.Ceiling((visits * visitValue) + additionalPoints + pointsForStandards);
}
