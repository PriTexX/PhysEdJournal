namespace Core.Commands;

public sealed class EmptyPayload
{
    private EmptyPayload() { }

    public static EmptyPayload Empty { get; } = new();
}
