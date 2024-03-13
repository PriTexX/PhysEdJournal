using System.Net;
using FluentValidation;

namespace PhysEdJournal.Api.Rest.Common;

public class ValidationFilter<TReq, TValidator> : IEndpointFilter
    where TValidator : IValidator<TReq>
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        var validator = context.HttpContext.RequestServices.GetService<TValidator>();

        if (validator is null)
        {
            throw new Exception("Validator is missing");
        }

        var argToValidate = context.GetArgument<TReq>(0);

        var validationResult = await validator.ValidateAsync(argToValidate!);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(
                validationResult.ToDictionary(),
                statusCode: (int)HttpStatusCode.BadRequest
            );
        }

        return await next.Invoke(context);
    }
}
