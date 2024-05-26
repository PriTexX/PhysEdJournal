using FluentValidation;

namespace Admin.Api;

public static class ValidatorExtensions
{
    public static IRuleBuilderOptions<T, TProperty> In<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder,
        params TProperty[] validOptions
    )
    {
        var formatted = $"{string.Join(", ", validOptions)}";

        return ruleBuilder
            .Must(validOptions.Contains)
            .WithMessage($"{{PropertyName}} must be one of these values: {formatted}");
    }

    public static IRuleBuilderOptions<T, string> In<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        bool caseInsensetive,
        params string[] validOptions
    )
    {
        var formatted = $"{string.Join(", ", validOptions)}";

        return ruleBuilder
            .Must(v =>
                validOptions.Contains(
                    v,
                    caseInsensetive
                        ? StringComparer.InvariantCultureIgnoreCase
                        : StringComparer.CurrentCulture
                )
            )
            .WithMessage($"{{PropertyName}} must be one of these values: {formatted}");
    }
}
