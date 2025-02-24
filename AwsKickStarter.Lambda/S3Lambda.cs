namespace AwsKickStarter.Lambda;

/// <summary>
/// The base class for a lambda function with an S3 source.
/// </summary>
public abstract class S3Lambda : ILambdaIn<S3Event>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="S3Lambda"/> class.
    /// </summary>
    public S3Lambda() => ServiceBuilder = new LambdaServiceBuilder(GetType().Assembly);

    /// <summary>
    /// Initializes a new instance of the <see cref="S3Lambda"/> class.
    /// </summary>
    /// <param name="lambdaServiceBuilder">The service builder to use to resolve the handler.</param>
    internal S3Lambda(ILambdaServiceBuilder lambdaServiceBuilder) => ServiceBuilder = lambdaServiceBuilder;

    /// <summary>
    /// Gets the service builder to use to resolve the handler.
    /// </summary>
    internal ILambdaServiceBuilder ServiceBuilder { get; init; }

    /// <summary>
    /// The handler function for the lambda that should be registered with AWS to be invoked.
    /// This will call the Handle method on the <see cref="IS3LambdaHandler"/> resolved from the service provider.
    /// </summary>
    /// <param name="s3Event">The S3 event that triggered the lambda.</param>
    /// <param name="context">The runtime context supplied by AWS on invocation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
    public async Task Handler(S3Event s3Event, ILambdaContext context)
    {
        using var scope = ServiceBuilder.ServiceProvider.CreateScope();
        var middleware = scope.ServiceProvider.GetRequiredService<ILambdaMiddleware>();
        var handler = scope.ServiceProvider.GetRequiredService<IS3LambdaHandler>();
        await handler.Handle(s3Event.Records.Select(middleware.Decode));
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => ServiceBuilder.DisposeAsync();
}
