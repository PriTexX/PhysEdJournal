namespace PhysEdJournal.Api.Rest.Common;

public static class Response
{
    public static IResult Ok<T>(T data)
    {
        return Results.Ok(new { success = "success", data });
    }

    public static IResult Error(Exception ex)
    {
        return ToResult(ErrorHandler.HandleErrorResult(ex));
    }

    private static IResult ToResult(ProblemDetailsResponse response)
    {
        return Results.Json(response, statusCode: response.Status);
    }
}
