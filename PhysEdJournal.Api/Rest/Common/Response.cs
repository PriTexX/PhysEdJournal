using PhysEdJournal.Api.Api;

namespace PhysEdJournal.Api.Rest.Common;

public static class Response
{
    public static IResult Ok<T>(T? data)
    {
        return Results.Ok(new { success = "success", data });
    }

    public static IResult Error(Exception ex)
    {
        return ErrorHandler.HandleErrorResult(ex);
    }
}
