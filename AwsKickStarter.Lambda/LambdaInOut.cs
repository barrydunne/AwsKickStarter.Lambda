namespace AwsKickStarter.Lambda;

/// <summary>
/// The base class for a lambda function that takes an input and returns output.
/// </summary>
/// <typeparam name="TInput">The type of input.</typeparam>
/// <typeparam name="TOutput">The type of output.</typeparam>
public abstract class LambdaInOut<TInput, TOutput> : ILambdaInOut<TInput, TOutput>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LambdaInOut{TInput, TOutput}"/> class.
    /// </summary>
    public LambdaInOut() => ServiceBuilder = new LambdaServiceBuilder(GetType().Assembly);

    /// <summary>
    /// Initializes a new instance of the <see cref="LambdaInOut{TInput, TOutput}"/> class.
    /// </summary>
    /// <param name="lambdaServiceBuilder">The service builder to use to resolve the handler.</param>
    internal LambdaInOut(ILambdaServiceBuilder lambdaServiceBuilder) => ServiceBuilder = lambdaServiceBuilder;

    /// <summary>
    /// Gets the service builder to use to resolve the handler.
    /// </summary>
    internal ILambdaServiceBuilder ServiceBuilder { get; init; }

    /// <summary>
    /// The handler function for the lambda that should be registered with AWS to be invoked.
    /// This will call the Handle method on the <see cref="ILambdaInOutHandler{TInput, TOutput}"/> resolved from the service provider.
    /// </summary>
    /// <param name="input">The input to the lambda.</param>
    /// <param name="context">The runtime context supplied by AWS on invocation.</param>
    /// <returns>The output from the lambda.</returns>
    [LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
    public async Task<TOutput> Handler(TInput input, ILambdaContext context)
    {
        using var scope = ServiceBuilder.ServiceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ILambdaInOutHandler<TInput, TOutput>>();
        return await handler.Handle(input);
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => ServiceBuilder.DisposeAsync();
}
