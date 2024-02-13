namespace PhysEdJournal.Api.Api;

public static class Result
{
    public static IResult Ok(object? data = null)
    {
        var okResult = new OkResult { Data = data };

        return Results.Ok(okResult);
    }

    public static IResult NotOk(Exception ex)
    {
        return ErrorHandler.HandleErrorResult(ex);
    }
}
