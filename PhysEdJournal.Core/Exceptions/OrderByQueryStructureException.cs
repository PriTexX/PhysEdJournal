namespace PhysEdJournal.Core.Exceptions;

public class OrderByQueryStructureException : Exception
{
    public OrderByQueryStructureException()
        : base(
            $"The sorting query consists of a list of parameters with the ability to add a sorting type to each parameter"
        ) { }
}
