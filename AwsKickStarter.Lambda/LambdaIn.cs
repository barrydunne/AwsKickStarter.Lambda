using Serilog;

namespace AwsKickStarter.Lambda;

/// <summary>
/// The base class for a lambda function that takes an input and returns no output.
/// </summary>
/// <typeparam name="TInput">The type of input.</typeparam>
public abstract class LambdaIn<TInput> : ILambdaIn<TInput>, ILogConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LambdaIn{TInput}"/> class.
    /// </summary>
    public LambdaIn() => ServiceBuilder = new LambdaServiceBuilder(GetType().Assembly, this);

    /// <summary>
    /// Initializes a new instance of the <see cref="LambdaIn{TInput}"/> class.
    /// </summary>
    /// <param name="lambdaServiceBuilder">The service builder to use to resolve the handler.</param>
    internal LambdaIn(ILambdaServiceBuilder lambdaServiceBuilder) => ServiceBuilder = lambdaServiceBuilder;

    /// <summary>
    /// Gets the service builder to use to resolve the handler.
    /// </summary>
    internal ILambdaServiceBuilder ServiceBuilder { get; init; }

    /// <summary>
    /// The handler function for the lambda that should be registered with AWS to be invoked.
    /// This will call the Handle method on the <see cref="ILambdaInHandler{TInput}"/> resolved from the service provider.
    /// </summary>
    /// <param name="input">The input to the lambda.</param>
    /// <param name="context">The runtime context supplied by AWS on invocation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
    public async Task Handler(TInput input, ILambdaContext context)
    {
        using var scope = ServiceBuilder.ServiceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ILambdaInHandler<TInput>>();
        await handler.Handle(input);
    }

    /// <inheritdoc/>
    public virtual void ConfigureLogging(LoggerConfiguration loggerConfiguration) { }

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => ServiceBuilder.DisposeAsync();
}
