using HotChocolate.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhysEdJournal.Api.Endpoints.MeEndpoint;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;

[ApiController]
[Route("[controller]")]
public sealed class MeController : ControllerBase
{
    private readonly MeInfoService _meInfoService;
    private readonly ILogger<MeInfoService> _logger;

    public MeController(MeInfoService meInfoService, ILogger<MeInfoService> logger)
    {
        _meInfoService = meInfoService;
        _logger = logger;
    }
    
    [HttpGet]
    [ResponseCache(Duration = 120, Location = ResponseCacheLocation.Client)]
    [Authorize]
    public async Task<IActionResult> Get([FromQuery]MeRequest request)
    {
        var guidClaim = User.FindFirst(c => c.Type == "IndividualGuid");

        if (guidClaim is null)
        {
            return StatusCode(500);
        }

        switch (request.Type)
        {
            case UserType.Student:
                var studentInfoResult = await _meInfoService.GetStudentInfo(guidClaim.Value);

                return studentInfoResult.Match<IActionResult>(
                    info => new OkObjectResult(info),
                    exception =>
                    {
                        if (exception is StudentNotFoundException)
                            return NotFound();
                        
                        _logger.LogError(exception, "unhandled error in me endpoint");
                        return StatusCode(500);
                    });
            
            case UserType.Professor:
                var professorInfoResult = await _meInfoService.GetProfessorInfo(guidClaim.Value);

                return professorInfoResult.Match<IActionResult>(
                    info => new OkObjectResult(info),
                    exception =>
                    {
                        if (exception is TeacherNotFoundException)
                            return NotFound();
                        
                        _logger.LogError(exception, "unhandled error in me endpoint");
                        return StatusCode(500);
                    });
            
            default:
                return StatusCode(500);
        }
    }
}