namespace Tests.Setup.Utils;

public static class DateOnlyGenerator
{
    public static DateOnly GetWorkingDate(DateOnly date = default)
    {
        if (date == default)
        {
            date = DateOnly.FromDateTime(DateTime.Now);
        }

        return date.DayOfWeek switch
        {
            DayOfWeek.Sunday => date.AddDays(-1),
            DayOfWeek.Monday => date.AddDays(-2),
            _ => date
        };
    }

    public static DateOnly GetNonWorkingDate()
    {
        var date = DateOnly.FromDateTime(DateTime.Now);
        var targetDate = date.AddDays(-7);

        while (targetDate.DayOfWeek != DayOfWeek.Sunday && targetDate.DayOfWeek != DayOfWeek.Monday)
        {
            if (targetDate > date)
            {
                throw new Exception("There is no day that is non-working");
            }

            targetDate = targetDate.AddDays(1);
        }

        return targetDate;
    }
}
