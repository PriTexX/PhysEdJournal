using HotChocolate.Execution;
using HotChocolate.Execution.Instrumentation;
using HotChocolate.Execution.Processing;
using HotChocolate.Resolvers;
using Serilog;

namespace Api;

public class ErrorLoggingDiagnosticsEventListener : ExecutionDiagnosticEventListener
{
    private readonly ILogger<ErrorLoggingDiagnosticsEventListener> _logger;
    private readonly IDiagnosticContext _diagnosticContext;

    public ErrorLoggingDiagnosticsEventListener(
        ILogger<ErrorLoggingDiagnosticsEventListener> logger,
        IDiagnosticContext diagnosticContext
    )
    {
        _logger = logger;
        _diagnosticContext = diagnosticContext;
    }

    public override IDisposable ResolveFieldValue(IMiddlewareContext context)
    {
        _diagnosticContext.Set("Resolver", context.Path);
        _diagnosticContext.Set("OperationType", context.Operation.Operation.ToString());
        return EmptyScope;
    }

    public override void ResolverError(IMiddlewareContext context, IError error)
    {
        _logger.LogError(error.Exception, "ResolverError with msg: {errorMsg}", error.Message);
    }

    public override void TaskError(IExecutionTask task, IError error)
    {
        _logger.LogError(error.Exception, "TaskError with msg: {errorMsg}", error.Message);
    }

    public override void RequestError(IRequestContext context, Exception exception)
    {
        _logger.LogError(exception, "RequestError");
    }

    public override void SubscriptionEventError(
        SubscriptionEventContext context,
        Exception exception
    )
    {
        _logger.LogError(exception, "SubscriptionEventError");
    }

    public override void SubscriptionTransportError(ISubscription subscription, Exception exception)
    {
        _logger.LogError(exception, "SubscriptionTransportError");
    }
}
