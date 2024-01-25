using Microsoft.AspNetCore.Mvc;
using PhysEdJournal.Api.Controllers;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using static PhysEdJournal.Core.Constants.PermissionConstants;

namespace PhysEdJournal.Api.Api.System;

public static class SystemController
{
    public static void MapSystemEndpoints(IEndpointRouteBuilder router)
    {
        router.MapPost("/UpdateStudentsInfo", UpdateStudentsInfo);
    }

    public static async Task<IResult> UpdateStudentsInfo(
        [FromServices] ILogger<UpdateStudentsInfoCommand> logger,
        [FromServices] UpdateStudentsInfoCommand updateStudentsInfoCommand,
        [FromServices] PermissionValidator permissionValidator,
        HttpContext ctx
    )
    {
        var callerGuid = ctx.User.Claims.First(c => c.Type == "IndividualGuid").Value;

        await permissionValidator.ValidateTeacherPermissionsAndThrow(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS
        );

        // We run this command in the background because it takes
        // to much time so client closes connection before command ends
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        Task.Run(async () =>
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        {
            try
            {
                await updateStudentsInfoCommand.ExecuteAsync(EmptyPayload.Empty);

                logger.LogInformation(
                    "{commandName} has successfully finished",
                    nameof(UpdateStudentsInfoCommand)
                );
            }
            catch (Exception e)
            {
                logger.LogError(
                    e,
                    "Unhandled exception happened in {commandName}",
                    nameof(UpdateStudentsInfoCommand)
                );
            }
        });

        return Results.Ok();
    }
}
