using System.Globalization;
using HotChocolate.Language;

namespace PhysEdJournal.Api.GraphQL.ScalarTypes;

public class DateOnlyType : ScalarType<DateOnly, StringValueNode>
{
    public DateOnlyType() : base("DateOnly")
    {
    }

    public override IValueNode ParseResult(object? resultValue)
    {
        return ParseValue(resultValue);
    }

    protected override DateOnly ParseLiteral(StringValueNode valueSyntax)
    {
        return DateOnly.FromDateTime(DateTime.ParseExact(valueSyntax.Value, "dd.MM.yyyy", CultureInfo.InvariantCulture));
    }

    protected override StringValueNode ParseValue(DateOnly runtimeValue)
    {
        return new StringValueNode(runtimeValue.ToString("dd.MM.yyyy"));
    }
}