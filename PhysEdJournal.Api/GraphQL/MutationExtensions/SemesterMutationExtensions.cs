﻿using System.Security.Claims;
using PhysEdJournal.Api.GraphQL.ScalarTypes;
using PhysEdJournal.Core.Exceptions.SemesterExceptions;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using static PhysEdJournal.Core.Constants.PermissionConstants;

namespace PhysEdJournal.Api.GraphQL.MutationExtensions;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class SemesterMutationExtensions
{
    [Error(typeof(SemesterNameValidationException))]
    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(TeacherNotFoundException))]
    public async Task<Success> StartNewSemester(
        string semesterName,
        [Service] StartNewSemesterCommand startNewSemesterCommand,
        [Service] PermissionValidator permissionValidator,
        ClaimsPrincipal claimsPrincipal
    )
    {
        var callerGuid = claimsPrincipal.FindFirstValue("IndividualGuid");
        ThrowIfCallerGuidIsNull(callerGuid);

        await permissionValidator.ValidateTeacherPermissionsAndThrow(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS
        );

        var res = await startNewSemesterCommand.ExecuteAsync(semesterName);

        return res.Match(_ => true, exception => throw exception);
    }

    private static void ThrowIfCallerGuidIsNull(string? callerGuid)
    {
        if (callerGuid is null)
        {
            throw new Exception("IndividualGuid cannot be empty. Wrong token was passed");
        }
    }
}
