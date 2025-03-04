using Serilog;

namespace AwsKickStarter.Lambda;

/// <summary>
/// The base class for a lambda function with an SQS source configured with ReportBatchItemFailures where messages contain JSON that will be deserialized to a type.
/// </summary>
/// <typeparam name="TMessage">The type of message in the SQS event.</typeparam>
public abstract class SqsBatchResponseLambda<TMessage> : ILambdaInOut<SQSEvent, SQSBatchResponse>, ILogConfiguration
{
    private readonly SqsBatchHandler _sqsBatchHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqsBatchResponseLambda{TMessage}"/> class.
    /// </summary>
    public SqsBatchResponseLambda()
    {
        ServiceBuilder = new LambdaServiceBuilder(GetType().Assembly, this);
        _sqsBatchHandler = new();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqsBatchResponseLambda{TMessage}"/> class.
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
    /// This will call the Handle method on the <see cref="ISqsBatchResponseLambdaHandler{TMessage}"/> resolved from the service provider.
    /// </summary>
    /// <param name="sqsEvent">The SQS event to process.</param>
    /// <param name="context">The runtime context supplied by AWS on invocation.</param>
    /// <returns>An instance of <see cref="SQSBatchResponse"/> containing a list of failed message ids.</returns>
    [LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
    public async virtual Task<SQSBatchResponse> Handler(SQSEvent sqsEvent, ILambdaContext context)
    {
        using var scope = ServiceBuilder.ServiceProvider.CreateScope();
        var middleware = scope.ServiceProvider.GetRequiredService<ILambdaMiddleware>();
        var handler = scope.ServiceProvider.GetRequiredService<ISqsBatchResponseLambdaHandler<TMessage>>();
        return await _sqsBatchHandler.Handle(sqsEvent, context, async (message)
            => await handler.Handle(middleware.Deserialize<TMessage>(middleware.Decode(message))));
    }

    /// <inheritdoc/>
    public virtual void ConfigureLogging(LoggerConfiguration loggerConfiguration) { }

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => ServiceBuilder.DisposeAsync();
}
