using System.Linq.Dynamic.Core;
using FluentValidation;

namespace Admin.Api.GetMany;

public sealed class Sort
{
    public required string Field { get; init; }
    public string Order { get; init; } = "ASC";

    public static IQueryable<T> Apply<T>(IQueryable<T> query, IEnumerable<Sort> sort)
    {
        if (sort is null || !sort.Any())
        {
            return query;
        }

        var ordering = string.Join(",", sort.Select(s => $"{s.Field} {s.Order}"));
        return query.OrderBy(ordering);
    }
}

public sealed class SortValidator : AbstractValidator<Sort>
{
    public SortValidator(string[]? sortFields)
    {
        RuleFor(x => x.Field).NotEmpty();

        if (sortFields is not null)
        {
            RuleFor(x => x.Field).In(caseInsensetive: true, sortFields);
        }

        RuleFor(x => x.Order).In("ASC", "DESC");
    }
}
