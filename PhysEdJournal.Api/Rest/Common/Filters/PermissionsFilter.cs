using PhysEdJournal.Core.Entities.Types;

namespace PhysEdJournal.Api.Rest.Common.Filters;

public static class PermissionsFilter
{
    public static RouteHandlerBuilder AddPermissionsValidation(
        this RouteHandlerBuilder builder,
        TeacherPermissions permissions
    )
    {
        return builder.AddEndpointFilter(
            async (invocationContext, next) =>
                await ValidatePermissionsAsync(invocationContext, next, permissions)
        );
    }

    private static async ValueTask<object?> ValidatePermissionsAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next,
        TeacherPermissions permissions
    )
    {
        var callerGuid = context
            .HttpContext.User.Claims.First(c => c.Type == "IndividualGuid")
            .Value;

        var validator = context.HttpContext.RequestServices.GetService<PermissionValidator>();

        ArgumentNullException.ThrowIfNull(validator);

        var validationResult = await validator.ValidateTeacherPermissions(callerGuid, permissions);

        if (validationResult.IsErr)
        {
            return Results.Json(
                new
                {
                    success = false,
                    type = "not-enough-permissions",
                    detail = "У вас недостаточно прав",
                },
                statusCode: StatusCodes.Status403Forbidden
            );
        }

        return await next.Invoke(context);
    }
}
