namespace AwsKickStarter.Lambda;

/// <summary>
/// The base class for a lambda function that takes no input and returns output.
/// </summary>
/// <typeparam name="TOutput">The type of output.</typeparam>
public abstract class LambdaOut<TOutput> : ILambdaOut<TOutput>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LambdaOut{TOutput}"/> class.
    /// </summary>
    public LambdaOut() => ServiceBuilder = new LambdaServiceBuilder(GetType().Assembly);

    /// <summary>
    /// Initializes a new instance of the <see cref="LambdaOut{TOutput}"/> class.
    /// </summary>
    /// <param name="lambdaServiceBuilder">The service builder to use to resolve the handler.</param>
    internal LambdaOut(ILambdaServiceBuilder lambdaServiceBuilder) => ServiceBuilder = lambdaServiceBuilder;

    /// <summary>
    /// Gets the service builder to use to resolve the handler.
    /// </summary>
    internal ILambdaServiceBuilder ServiceBuilder { get; init; }

    /// <summary>
    /// The handler function for the lambda that should be registered with AWS to be invoked.
    /// This will call the Handle method on the <see cref="ILambdaOutHandler{TOutput}"/> resolved from the service provider.
    /// </summary>
    /// <param name="context">The runtime context supplied by AWS on invocation.</param>
    /// <returns>The output from the lambda.</returns>
    [LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
    public async Task<TOutput> Handler(ILambdaContext context)
    {
        using var scope = ServiceBuilder.ServiceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ILambdaOutHandler<TOutput>>();
        return await handler.Handle();
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => ServiceBuilder.DisposeAsync();
}
