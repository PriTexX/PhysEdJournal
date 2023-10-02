using Serilog;
using Serilog.Context;
using RequestDelegate = Microsoft.AspNetCore.Http.RequestDelegate;

namespace PhysEdJournal.Api.Monitoring.Logging;

public static class LogUserGuidExtensions
{
    public static IApplicationBuilder UseUserGuidLogger(this IApplicationBuilder app)
    {
        return app.UseMiddleware<UserGuidLoggerMiddleware>();
    }
}

public sealed class UserGuidLoggerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDiagnosticContext _diagnosticContext;

    public UserGuidLoggerMiddleware(RequestDelegate next, IDiagnosticContext diagnosticContext)
    {
        _next = next;
        _diagnosticContext = diagnosticContext;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var userClaim = context.User.FindFirst(c => c.Type == "IndividualGuid");

        if (userClaim is not null)
        {
            _diagnosticContext.Set("UserGuid", userClaim.Value);
            using (LogContext.PushProperty("UserGuid", userClaim.Value))
            {
                await _next(context);
            }
        }
        else
        {
            await _next(context);   
        }
    }
}