namespace Core.Commands;

public class ActionFromFutureError() : Exception("Provided date is later than current date") { }
