using Api.Rest.Common;
using Api.Rest.Contracts;
using DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Rest;

public static class GroupController
{
    public static void MapEndpoints(IEndpointRouteBuilder router)
    {
        var groupRouter = router.MapGroup("/group");

        groupRouter.MapGet("/", GetGroups);
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
