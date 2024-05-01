using Serilog.Context;
using Serilog.Core;
using Serilog.Events;

namespace PhysEdJournal.Api.Middlewares;

public static class RequestIdExtensions
{
    public static IApplicationBuilder UseRequestId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestIdMiddleware>();
    }
}

file struct SerilogRequestIdEnricher : ILogEventEnricher
{
    private readonly string _requestId;

    public SerilogRequestIdEnricher(string requestId)
    {
        _requestId = requestId;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddOrUpdateProperty(
            new LogEventProperty("RequestId", new ScalarValue(_requestId))
        );
    }
}

public sealed class RequestIdMiddleware
{
    private readonly RequestDelegate _next;

    public RequestIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        context.TraceIdentifier = Guid.NewGuid().ToString();

        context.Response.Headers["x-sx-request-id"] = context.TraceIdentifier;

        using (LogContext.Push(new SerilogRequestIdEnricher(context.TraceIdentifier)))
        {
            await _next(context);
        }
    }
}
