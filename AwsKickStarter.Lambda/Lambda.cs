namespace AwsKickStarter.Lambda;

/// <summary>
/// The base class for a lambda function that takes no input and returns no output.
/// </summary>
public abstract class Lambda : ILambda
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Lambda"/> class.
    /// </summary>
    public Lambda() => ServiceBuilder = new LambdaServiceBuilder(GetType().Assembly);

    /// <summary>
    /// Initializes a new instance of the <see cref="Lambda"/> class.
    /// </summary>
    /// <param name="lambdaServiceBuilder">The service builder to use to resolve the handler.</param>
    internal Lambda(ILambdaServiceBuilder lambdaServiceBuilder) => ServiceBuilder = lambdaServiceBuilder;

    /// <summary>
    /// Gets the service builder to use to resolve the handler.
    /// </summary>
    internal ILambdaServiceBuilder ServiceBuilder { get; init; }

    /// <summary>
    /// The handler function for the lambda that should be registered with AWS to be invoked.
    /// This will call the Handle method on the <see cref="ILambdaHandler"/> resolved from the service provider.
    /// </summary>
    /// <param name="context">The runtime context supplied by AWS on invocation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
    public async Task Handler(ILambdaContext context)
    {
        using var scope = ServiceBuilder.ServiceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ILambdaHandler>();
        await handler.Handle();
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => ServiceBuilder.DisposeAsync();
}
