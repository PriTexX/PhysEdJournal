namespace PhysEdJournal.Api.Api.ResponseType;

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
