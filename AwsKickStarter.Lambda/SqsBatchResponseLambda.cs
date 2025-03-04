using Serilog;

namespace AwsKickStarter.Lambda;

/// <summary>
/// The base class for a lambda function with an SQS source configured with ReportBatchItemFailures.
/// </summary>
public abstract class SqsBatchResponseLambda : ILambdaInOut<SQSEvent, SQSBatchResponse>, ILogConfiguration
{
    private readonly SqsBatchHandler _sqsBatchHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqsBatchResponseLambda"/> class.
    /// </summary>
    public SqsBatchResponseLambda()
    {
        ServiceBuilder = new LambdaServiceBuilder(GetType().Assembly, this);
        _sqsBatchHandler = new();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqsBatchResponseLambda"/> class.
    /// </summary>
    /// <param name="lambdaServiceBuilder">The service builder to use to resolve the handler.</param>
    internal SqsBatchResponseLambda(ILambdaServiceBuilder lambdaServiceBuilder)
    {
        ServiceBuilder = lambdaServiceBuilder;
        _sqsBatchHandler = new();
    }

    /// <summary>
    /// Gets the service builder to use to resolve the handler.
    /// </summary>
    internal ILambdaServiceBuilder ServiceBuilder { get; init; }

    /// <summary>
    /// The handler function for the lambda that should be registered with AWS to be invoked.
    /// This will call the Handle method on the <see cref="ISqsBatchResponseLambdaHandler"/> resolved from the service provider.
    /// </summary>
    /// <param name="sqsEvent">The SQS event to process.</param>
    /// <param name="context">The runtime context supplied by AWS on invocation.</param>
    /// <returns>An instance of <see cref="SQSBatchResponse"/> containing a list of failed message ids.</returns>
    [LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
    public async virtual Task<SQSBatchResponse> Handler(SQSEvent sqsEvent, ILambdaContext context)
    {
        using var scope = ServiceBuilder.ServiceProvider.CreateScope();
        var middleware = scope.ServiceProvider.GetRequiredService<ILambdaMiddleware>();
        var handler = scope.ServiceProvider.GetRequiredService<ISqsBatchResponseLambdaHandler>();
        return await _sqsBatchHandler.Handle(sqsEvent, context, async (message)
            => await handler.Handle(middleware.Decode(message)));
    }

    /// <inheritdoc/>
    public virtual void ConfigureLogging(LoggerConfiguration loggerConfiguration) { }

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => ServiceBuilder.DisposeAsync();
}
