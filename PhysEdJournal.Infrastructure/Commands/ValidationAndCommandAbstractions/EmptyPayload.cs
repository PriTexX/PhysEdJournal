namespace PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;

public sealed class EmptyPayload
{
    private EmptyPayload(){}

    public static EmptyPayload Empty { get; } = new ();
}