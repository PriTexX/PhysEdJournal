namespace PhysEdJournal.Infrastructure.Services.StaticFunctions;

public static class DoubleComparer
{
    public static bool Compare(double? first, double? second)
    {
        if (first is null && second is null)
        {
            return true;
        }

        if (first is null)
        {
            return false;
        }

        if (second is null)
        {
            return false;
        }

        return Compare((double)first, (double)second);
    }

    private static bool Compare(double first, double second)
    {
        if (first == second)
        {
            return true;
        }

        return Math.Abs(first - second) < 1e-9 * Math.Max(first, second);
    }
}
