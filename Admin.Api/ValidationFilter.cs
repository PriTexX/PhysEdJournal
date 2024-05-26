using System.Net;
using FluentValidation;

namespace Admin.Api;

public static class ValidationFilter
{
    public static RouteHandlerBuilder AddValidation<TReq>(
        this RouteHandlerBuilder builder,
        IValidator<TReq> validator
    )
    {
        return builder.AddEndpointFilter(
            async (invocationContext, next) =>
                await ValidateAsync(invocationContext, next, validator)
        );
    }

    private static async ValueTask<object?> ValidateAsync<TReq>(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next,
        IValidator<TReq> validator
    )
    {
        var argToValidate = context.GetArgument<TReq>(0);

        var validationResult = await validator.ValidateAsync(argToValidate);
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
