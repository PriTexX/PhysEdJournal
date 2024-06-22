namespace Tests.Setup.Utils.Comparers;

public static class DoubleComparer
{
    public static bool Compare(double first, double second)
    {
        if (first == second)
        {
            return true;
        }

        return Math.Abs(first - second) < 1e-9 * Math.Max(first, second);
    }
}
