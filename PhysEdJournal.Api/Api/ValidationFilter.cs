using System.Net;
using System.Reflection;
using FluentValidation;

namespace PhysEdJournal.Api.Api;

[AttributeUsage(AttributeTargets.Parameter)]
public class ValidateAttribute : Attribute { }

public static class ValidationFilter
{
    public static EndpointFilterDelegate ValidationFilterFactory(
        EndpointFilterFactoryContext context,
        EndpointFilterDelegate next
    )
    {
        IEnumerable<ValidationDescriptor> validationDescriptors = GetValidators(
            context.MethodInfo,
            context.ApplicationServices
        );

        var descriptors = validationDescriptors.ToList();
        if (descriptors.Any())
        {
            return invocationContext => Validate(descriptors, invocationContext, next);
        }

        // pass-thru
        return next;
    }

    private static async ValueTask<object?> Validate(
        IEnumerable<ValidationDescriptor> validationDescriptors,
        EndpointFilterInvocationContext invocationContext,
        EndpointFilterDelegate next
    )
    {
        foreach (ValidationDescriptor descriptor in validationDescriptors)
        {
            var argument = invocationContext.Arguments[descriptor.ArgumentIndex];

            if (argument is not null)
            {
                var validationResult = await descriptor.Validator.ValidateAsync(
                    new ValidationContext<object>(argument)
                );

                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(
                        validationResult.ToDictionary(),
                        statusCode: (int)HttpStatusCode.UnprocessableEntity
                    );
                }
            }
        }

        return await next.Invoke(invocationContext);
    }

    private static IEnumerable<ValidationDescriptor> GetValidators(
        MethodBase methodInfo,
        IServiceProvider serviceProvider
    )
    {
        ParameterInfo[] parameters = methodInfo.GetParameters();

        for (var i = 0; i < parameters.Length; i++)
        {
            ParameterInfo parameter = parameters[i];

            if (parameter.GetCustomAttribute<ValidateAttribute>() is not null)
            {
                Type validatorType = typeof(IValidator<>).MakeGenericType(parameter.ParameterType);

                // Note that FluentValidation validators needs to be registered as singleton
                if (serviceProvider.GetService(validatorType) is IValidator validator)
                {
                    yield return new ValidationDescriptor
                    {
                        ArgumentIndex = i,
                        ArgumentType = parameter.ParameterType,
                        Validator = validator,
                    };
                }
            }
        }
    }

    private class ValidationDescriptor
    {
        public required int ArgumentIndex { get; init; }
        public required Type ArgumentType { get; init; }
        public required IValidator Validator { get; init; }
    }
}
