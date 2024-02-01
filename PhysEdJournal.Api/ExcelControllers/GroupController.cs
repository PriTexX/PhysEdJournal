using Microsoft.AspNetCore.Mvc;
using PhysEdJournal.Infrastructure.Commands.ServiceCommands;

namespace PhysEdJournal.Api.ExcelControllers;

[Route("api/[action]")]
public sealed class GroupController : Controller
{
    private readonly UpdateGroupsVisitValueCommand _command;

    public GroupController(UpdateGroupsVisitValueCommand command)
    {
        _command = command;
    }

    [HttpGet]
    public async Task<IResult> UpdateGroupsVisitValue(string fileName)
    {
        try
        {
            var payload = new UpdateGroupsVisitValuePayload { FileName = fileName };

            await _command.ExecuteAsync(payload);

            return Results.Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);

            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
