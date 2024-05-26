using System.Linq.Dynamic.Core;
using System.Text.Json;
using FluentValidation;

namespace Admin.Api.GetMany;

public sealed class Filter
{
    public required string Field { get; init; }
    public required string Operator { get; init; }
    public required JsonElement Value { get; init; }

    public static IQueryable<T> Apply<T>(IQueryable<T> query, List<Filter> filters)
    {
        var transformedFilters = filters
            .Select(
                (filter, idx) =>
                {
                    var comparison = Operators[filter.Operator];

                    if (filter.Operator == "contains")
                    {
                        // https://www.ryadel.com/en/system-linq-dynamic-core-case-insensitive-contains/
                        // The second option doesnt work as it cannot be translated. So we use double lower case.
                        return $"{filter.Field}.ToLower().{comparison}(@{idx})";
                    }

                    return $"{filter.Field} {comparison} @{idx}";
                }
            )
            .ToArray();

        var where = string.Join(" AND ", transformedFilters);
        var values = filters
            .Select(f =>
            {
                var value = UnpackJsonElement.Unpack(f.Value);

                if (f.Operator == "contains" && value is string strValue)
                {
                    return strValue.ToLower();
                }

                return value;
            })
            .ToArray();

        return query.Where(where, values);
    }

    private static readonly Dictionary<string, string> Operators =
        new()
        {
            { "eq", "==" },
            { "neq", "!=" },
            { "lt", "<" },
            { "lte", "<=" },
            { "gt", ">" },
            { "gte", ">=" },
            { "contains", "Contains" },
        };

    public static readonly string[] AllowedOperators = Operators.Keys.ToArray();
}

public sealed class FilterValidator : AbstractValidator<Filter>
{
    public FilterValidator(string[]? filterFields)
    {
        RuleFor(x => x.Field).NotEmpty();

        if (filterFields is not null)
        {
            RuleFor(x => x.Field).In(caseInsensetive: true, filterFields);
        }

        RuleFor(x => x.Operator).In(Filter.AllowedOperators);
    }
}
