using System.Text.Json;
using Serilog;

namespace AwsKickStarter.Lambda;

/// <summary>
/// The base class for a lambda function with an SQS source where messages contain JSON that will be deserialized to a type.
/// </summary>
/// <typeparam name="TMessage">The type of message in the SQS event.</typeparam>
public abstract class SqsLambda<TMessage> : ILambdaIn<SQSEvent>, ILogConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqsLambda{TMessage}"/> class.
    /// </summary>
    public SqsLambda() => ServiceBuilder = new LambdaServiceBuilder(GetType().Assembly, this);

    /// <summary>
    /// Initializes a new instance of the <see cref="SqsLambda{TMessage}"/> class.
    /// </summary>
    /// <param name="lambdaServiceBuilder">The service builder to use to resolve the handler.</param>
    internal SqsLambda(ILambdaServiceBuilder lambdaServiceBuilder) => ServiceBuilder = lambdaServiceBuilder;

    /// <summary>
    /// Gets the service builder to use to resolve the handler.
    /// </summary>
    internal ILambdaServiceBuilder ServiceBuilder { get; init; }

    /// <summary>
    /// The handler function for the lambda that should be registered with AWS to be invoked.
    /// This will call the Handle method on the <see cref="ISqsLambdaHandler{TMessage}"/> resolved from the service provider.
    /// </summary>
    /// <param name="sqsEvent">The SQS event to process.</param>
    /// <param name="context">The runtime context supplied by AWS on invocation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
    public async Task Handler(SQSEvent sqsEvent, ILambdaContext context)
    {
        using var scope = ServiceBuilder.ServiceProvider.CreateScope();
        var middleware = scope.ServiceProvider.GetRequiredService<ILambdaMiddleware>();
        var handler = scope.ServiceProvider.GetRequiredService<ISqsLambdaHandler<TMessage>>();
        await handler.Handle(sqsEvent.Records.Select(middleware.Decode).Select(middleware.Deserialize<TMessage>));
    }

    /// <inheritdoc/>
    public virtual void ConfigureLogging(LoggerConfiguration loggerConfiguration) { }

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => ServiceBuilder.DisposeAsync();
}
