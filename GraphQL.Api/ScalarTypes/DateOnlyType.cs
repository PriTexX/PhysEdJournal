using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using HotChocolate.Language;

namespace GraphQL.Api.ScalarTypes;

public class DateOnlyType : ScalarType<DateOnly, StringValueNode>
{
    public DateOnlyType()
        : base("DateOnly") { }

    public override IValueNode ParseResult(object? resultValue)
    {
        return ParseValue(resultValue);
    }

    protected override DateOnly ParseLiteral(StringValueNode valueSyntax)
    {
        return DateOnly.FromDateTime(
            DateTime.ParseExact(valueSyntax.Value, "dd.MM.yyyy", CultureInfo.InvariantCulture)
        );
    }

    protected override StringValueNode ParseValue(DateOnly runtimeValue)
    {
        return new StringValueNode(runtimeValue.ToString("dd.MM.yyyy"));
    }

    public override bool TryDeserialize(object? resultValue, [UnscopedRef] out object? runtimeValue)
    {
        runtimeValue = null;

        if (
            resultValue is string strValue
            && DateTime.TryParseExact(
                strValue,
                "dd.MM.yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date
            )
        )
        {
            runtimeValue = DateOnly.FromDateTime(date);
            return true;
        }

        return false;
    }

    public override bool TrySerialize(object? runtimeValue, [UnscopedRef] out object? resultValue)
    {
        resultValue = null;

        if (runtimeValue is DateOnly date)
        {
            resultValue = date.ToString("MM/dd/yyyy");
            return true;
        }

        return false;
    }
}
