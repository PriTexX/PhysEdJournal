using PResult;

namespace PhysEdJournal.Api.Rest.Common;

public static class Response
{
    public static IResult Ok<T>(T data)
    {
        return Results.Ok(new { success = true, data });
    }

    // In case of no data we need to return json with
    // success: true, data: null. To do this we cast null to
    // any nullable type
    public static IResult Ok(Unit data)
    {
        return Results.Ok(new { success = true, data = (Unit?)null });
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
}
