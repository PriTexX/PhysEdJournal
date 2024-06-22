using HotChocolate.Language;

namespace Api.GraphQL.ScalarTypes;

public readonly struct Success
{
    public static readonly Success Yes = new Success(true);
    public static readonly Success No = new Success(false);

    private readonly bool _value;

    private Success(bool value)
    {
        _value = value;
    }

    public static implicit operator bool(Success success)
    {
        return success._value;
    }

    public static implicit operator Success(bool success)
    {
        return success switch
        {
            true => Yes,
            false => No
        };
    }

    public override string ToString()
    {
        return _value ? "YES" : "NO";
    }
}

[GraphQLName("Success")]
public class SuccessType : ScalarType<Success, BooleanValueNode>
{
    public SuccessType()
        : base("Success") { }

    public override IValueNode ParseResult(object? resultValue)
    {
        return ParseValue(resultValue);
    }

    protected override Success ParseLiteral(BooleanValueNode valueSyntax)
    {
        return valueSyntax.Value;
    }

    protected override BooleanValueNode ParseValue(Success runtimeValue)
    {
        return new BooleanValueNode(runtimeValue);
    }
}
