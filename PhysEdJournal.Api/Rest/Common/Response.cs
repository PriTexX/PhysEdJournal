using PResult;

namespace PhysEdJournal.Api.Rest.Common;

public static class Response
{
    public static IResult Ok<T>(T data)
    {
        return Results.Ok(new { success = true, data });
    }

    public static IResult Ok(Unit data)
    {
        return Results.Ok(new { success = true });
    }

    public static IResult Error(Exception ex)
    {
        var err = ErrorHandler.HandleErrorResult(ex);

        return Results.Json(
            new
            {
                success = false,
                type = err.Type,
                detail = err.Detail,
            },
            statusCode: err.StatusCode
        );
    }

    public static IResult Error(ErrorResponse err)
    {
        return Results.Json(
            new
            {
                success = false,
                type = err.Type,
                detail = err.Detail,
            },
            statusCode: err.StatusCode
        );
    }
}
