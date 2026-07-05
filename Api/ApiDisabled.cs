using Api.Rest.Common;

namespace Api;

public static class ApiDisabled
{
    public static IResult DisableForTime()
    {
        return Response.Error(
            new ErrorResponse
            {
                Type = "ApiDisabled",
                StatusCode = 422,
                Detail = "Выставление баллов недоступно до 31.08",
            }
        );
    }
}
