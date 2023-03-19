namespace PhysEdJournal.Core.Exceptions.VisitsExceptions;

public class DayOfVisitBiggerThanNowException : Exception
{
    public DayOfVisitBiggerThanNowException(DateOnly date): base($"{date} is later than current date"){}
}