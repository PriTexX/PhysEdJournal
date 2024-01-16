namespace PhysEdJournal.Core.Constants;

public static class PointsConstants
{
    public const int MAX_POINTS_FOR_STANDARDS = 30;
    public const int DAYS_TO_DELETE_VISITS = 7;
    public const int DAYS_TO_DELETE_POINTS = 30;
    public const int POINTS_LIFE_DAYS = 30;
    public const int REQUIRED_POINT_AMOUNT = 50;
    public const int MAX_POINTS_FOR_ONE_STANDARD = 10;

    public static double CalculateTotalPoints(
        int visits,
        double visitValue,
        int additionalPoints,
        int pointsForStandards
    ) => Math.Ceiling((visits * visitValue) + additionalPoints + pointsForStandards);
}
