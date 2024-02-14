namespace PhysEdJournal.Api.Api.ResponseType;

public static class Response
{
    public static IResult Ok(object? data = null)
    {
        var okResult = new OkResponse { Data = data };

        return Results.Ok(okResult);
    }

    public static IResult NotOk(Exception ex)
    {
        return ErrorHandler.HandleErrorResult(ex);
    }
}
