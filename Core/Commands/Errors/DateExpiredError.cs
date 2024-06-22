namespace Core.Commands;

public class DateExpiredError()
    : Exception($"The ability to set points for provided date has expired") { }
