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
        var (predicates, args) = TransformFilters(filters);

        var where = string.Join(" AND ", predicates);

        return query.Where(where, args.ToArray());
    }

    private static (List<string>, List<object>) TransformFilters(List<Filter> filters)
    {
        var filterPredicates = new List<string>();
        var filterArgs = new List<object>();

        // We split operator `between` into 2 conditions - `<=` and `>=`,
        // so if we see `between` than all indexes after after that will be increased by 1
        // that's why we have `idxOffset` that will tell when idx should be increased to next operators
        var idxOffset = 0;

        for (var idx = 0; idx < filters.Count; idx++)
        {
            var filter = filters[idx];

            var value = UnpackJsonElement.Unpack(filter.Value);
            var comparison = Operators[filter.Operator];

            if (filter.Operator == "contains" && value is string strValue)
            {
                // https://www.ryadel.com/en/system-linq-dynamic-core-case-insensitive-contains/
                // The second option doesnt work as it cannot be translated. So we use double lower case.
                filterPredicates.Add($"{filter.Field}.ToLower().{comparison}(@{idx + idxOffset})");
                filterArgs.Add(strValue.ToLower());

                continue;
            }

            if (filter.Operator == "between" && value is object?[] arrValue && arrValue.Length == 2)
            {
                filterPredicates.Add($"{filter.Field} >= @{idx + idxOffset}");
                filterPredicates.Add($"{filter.Field} <= @{idx + idxOffset}");

                filterArgs.Add(arrValue[0]);
                filterArgs.Add(arrValue[1]);

                idxOffset++;
                continue;
            }

            filterPredicates.Add($"{filter.Field} {comparison} @{idx + idxOffset}");
            filterArgs.Add(value);
        }

        return (filterPredicates, filterArgs);
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
            { "between", "Handled in code" },
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
