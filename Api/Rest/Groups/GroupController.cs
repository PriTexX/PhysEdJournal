using Api.Rest.Common;
using Api.Rest.Common.Filters;
using Api.Rest.Groups.Contracts;
using Core.Commands;
using DB;
using DB.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Rest;

public static class GroupController
{
    public static void MapEndpoints(IEndpointRouteBuilder router)
    {
        var groupRouter = router.MapGroup("/group");

        groupRouter
            .MapPost("/archive", ArchiveGroup)
            .AddValidation(ArchiveGroupRequest.GetValidator())
            .AddPermissionsValidation(TeacherPermissions.AdminAccess)
            .RequireAuthorization();

        groupRouter.MapGet("/", GetGroups);
    }

    private static async Task<IResult> ArchiveGroup(
        [FromBody] ArchiveGroupRequest request,
        [FromServices] ArchiveGroupCommand command,
        [FromServices] PermissionValidator permissionValidator,
        HttpContext ctx
    )
    {
        var userGuid = ctx.User.Claims.First(c => c.Type == "IndividualGuid").Value;

        var isAdmin = await permissionValidator.ValidateTeacherPermissions(
            userGuid,
            TeacherPermissions.AdminAccess
        );

        var payload = new ArchiveGroupPayload
        {
            GroupName = request.GroupName,
            IsAdmin = isAdmin.IsOk,
        };

        var res = await command.ExecuteAsync(payload);

        return res.Match(
            rowData =>
            {
                var response = new List<ArchiveGroupResponse>();
                foreach (var stud in rowData)
                {
                    var item = new ArchiveGroupResponse
                    {
                        IsArchived = stud.IsArchived,
                        Guid = stud.Guid,
                        FullName = stud.FullName,
                    };

                    if (stud.Error is null)
                    {
                        continue;
                    }

                    item.Error = ErrorHandler.HandleErrorResult(stud.Error);

                    response.Add(item);
                }

                return Response.Ok(response);
            },
            Response.Error
        );
    }

    private static async Task<IResult> GetGroups(
        [FromQuery] string curatorGuid,
        [FromServices] ApplicationContext appCtx
    )
    {
        var groups = await appCtx
            .Groups.Where(g => g.CuratorGuid == curatorGuid)
            .Select(g => new GroupsResponse
            {
                GroupName = g.GroupName,
                CuratorFullName = g.Curator!.FullName,
            })
            .ToListAsync();

        return Response.Ok(groups);
    }
}
