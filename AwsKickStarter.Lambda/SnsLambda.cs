using Serilog;

namespace AwsKickStarter.Lambda;

/// <summary>
/// The base class for a lambda function with an SNS source.
/// </summary>
public abstract class SnsLambda : ILambdaIn<SNSEvent>, ILogConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SnsLambda"/> class.
    /// </summary>
    public SnsLambda() => ServiceBuilder = new LambdaServiceBuilder(GetType().Assembly, this);

    /// <summary>
    /// Initializes a new instance of the <see cref="SnsLambda"/> class.
    /// </summary>
    /// <param name="lambdaServiceBuilder">The service builder to use to resolve the handler.</param>
    internal SnsLambda(ILambdaServiceBuilder lambdaServiceBuilder) => ServiceBuilder = lambdaServiceBuilder;

    /// <summary>
    /// Gets the service builder to use to resolve the handler.
    /// </summary>
    internal ILambdaServiceBuilder ServiceBuilder { get; init; }

    /// <summary>
    /// The handler function for the lambda that should be registered with AWS to be invoked.
    /// This will call the Handle method on the <see cref="ISnsLambdaHandler"/> resolved from the service provider.
    /// </summary>
    /// <param name="snsEvent">The SNS event to process.</param>
    /// <param name="context">The runtime context supplied by AWS on invocation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
    public async Task Handler(SNSEvent snsEvent, ILambdaContext context)
    {
        using var scope = ServiceBuilder.ServiceProvider.CreateScope();
        var middleware = scope.ServiceProvider.GetRequiredService<ILambdaMiddleware>();
        var handler = scope.ServiceProvider.GetRequiredService<ISnsLambdaHandler>();
        await handler.Handle(snsEvent.Records.Select(middleware.Decode));
    }

    /// <inheritdoc/>
    public virtual void ConfigureLogging(LoggerConfiguration loggerConfiguration) { }

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => ServiceBuilder.DisposeAsync();
}
