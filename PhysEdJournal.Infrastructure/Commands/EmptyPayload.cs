namespace PhysEdJournal.Infrastructure.Commands;

public sealed class EmptyPayload
{
    private EmptyPayload(){}
    private static readonly EmptyPayload Instance = new ();

    public EmptyPayload Empty => Instance;
}