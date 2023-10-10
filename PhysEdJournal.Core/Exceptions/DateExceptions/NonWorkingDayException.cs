namespace PhysEdJournal.Core.Exceptions.DateExceptions;

public class NonWorkingDayException : Exception
{
    public NonWorkingDayException(DayOfWeek date)
        : base($"{date} is a non-working day") { }
}
