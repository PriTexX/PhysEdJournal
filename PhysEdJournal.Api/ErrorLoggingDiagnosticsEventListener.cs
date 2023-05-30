using HotChocolate.Execution;
using HotChocolate.Execution.Instrumentation;
using HotChocolate.Execution.Processing;
using HotChocolate.Resolvers;

namespace PhysEdJournal.Api;

public class ErrorLoggingDiagnosticsEventListener : ExecutionDiagnosticEventListener
{
    private readonly ILogger<ErrorLoggingDiagnosticsEventListener> _logger;

    public ErrorLoggingDiagnosticsEventListener(
        ILogger<ErrorLoggingDiagnosticsEventListener> logger)
    {
        _logger = logger;
    }

    public override void ResolverError(
        IMiddlewareContext context,
        IError error)
    {
        _logger.LogError(error.Exception, "ResolverError");
    }

    public override void TaskError(
        IExecutionTask task,
        IError error)
    {
        _logger.LogError(error.Exception, "TaskError");
    }

    public override void RequestError(
        IRequestContext context,
        Exception exception)
    {
        _logger.LogError(exception, "RequestError");
    }

    public override void SubscriptionEventError(
        SubscriptionEventContext context,
        Exception exception)
    {
        _logger.LogError(exception, "SubscriptionEventError");
    }

    public override void SubscriptionTransportError(
        ISubscription subscription,
        Exception exception)
    {
        _logger.LogError(exception, "SubscriptionTransportError");
    }
}