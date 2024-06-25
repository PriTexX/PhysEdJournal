using DB.Tables;

namespace Api.Rest.Common.Filters;

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
            .HttpContext.User.Claims.FirstOrDefault(c => c.Type == "IndividualGuid")
            ?.Value;

        var validator = context.HttpContext.RequestServices.GetService<PermissionValidator>();

        ArgumentNullException.ThrowIfNull(validator);
        ArgumentNullException.ThrowIfNull(callerGuid);

        var validationResult = await validator.ValidateTeacherPermissions(callerGuid, permissions);

        if (validationResult.IsErr)
        {
            return Response.Error(
                new ErrorResponse
                {
                    Type = "not-enough-permissions",
                    Detail = "У вас недостаточно прав",
                    StatusCode = StatusCodes.Status403Forbidden,
                }
            );
        }

        return await next.Invoke(context);
    }
}
