using Core.Commands;
using DB;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Api;

public static class ClearStudentHandler
{
    public static async Task<IResult> Handle(
        string id,
        [FromServices] ClearStudentCommand command,
        [FromServices] ApplicationContext ctx
    )
    {
        var res = await command.ExecuteAsync(new ClearStudentPayload { StudentGuid = id });

        return res.Match(_ => Results.Ok(), _ => Results.NotFound());
    }
}
