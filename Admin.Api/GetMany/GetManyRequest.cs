using FluentValidation;

namespace Admin.Api.GetMany;

public sealed class GetManyRequest
{
    public int Limit { get; init; } = 50;
    public int Offset { get; init; } = 0;
    public List<Sort>? Sorters { get; init; }
    public List<Filter>? Filters { get; init; }
}

public sealed class GetManyRequestValidator : AbstractValidator<GetManyRequest>
{
    public GetManyRequestValidator(string[]? sortFields, string[]? filterFields)
    {
        RuleFor(x => x.Limit).GreaterThan(0).LessThan(200);
        RuleFor(x => x.Offset).GreaterThanOrEqualTo(0);

        RuleForEach(x => x.Filters).SetValidator(new FilterValidator(filterFields));
        RuleForEach(x => x.Sorters).SetValidator(new SortValidator(sortFields));
    }
}
