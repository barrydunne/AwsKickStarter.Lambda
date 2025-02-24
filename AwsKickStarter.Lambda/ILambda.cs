namespace AwsKickStarter.Lambda;

/// <summary>
/// A lambda function with no input or output.
/// </summary>
internal interface ILambda : IAsyncDisposable
{
    /// <summary>
    /// The handler to be registered with AWS.
    /// </summary>
    /// <param name="context">The context that will be provided by AWS on invocation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Handler(ILambdaContext context);
}

/// <summary>
/// A lambda function with input and no output.
/// </summary>
/// <typeparam name="TInput">The type of input.</typeparam>
internal interface ILambdaIn<in TInput> : IAsyncDisposable
{
    /// <summary>
    /// The handler to be registered with AWS.
    /// </summary>
    /// <param name="input">The input to the lambda.</param>
    /// <param name="context">The context that will be provided by AWS on invocation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Handler(TInput input, ILambdaContext context);
}

/// <summary>
/// A lambda function with output and no input.
/// </summary>
/// <typeparam name="TOutput">The type of output.</typeparam>
internal interface ILambdaOut<TOutput> : IAsyncDisposable
{
    /// <summary>
    /// The handler to be registered with AWS.
    /// </summary>
    /// <param name="context">The context that will be provided by AWS on invocation.</param>
    /// <returns>The output from the lambda.</returns>
    Task<TOutput> Handler(ILambdaContext context);
}

/// <summary>
/// A lambda function with input and output.
/// </summary>
/// <typeparam name="TInput">The type of input.</typeparam>
/// <typeparam name="TOutput">The type of output.</typeparam>
internal interface ILambdaInOut<in TInput, TOutput> : IAsyncDisposable
{
    /// <summary>
    /// The handler to be registered with AWS.
    /// </summary>
    /// <param name="input">The input to the lambda.</param>
    /// <param name="context">The context that will be provided by AWS on invocation.</param>
    /// <returns>The output from the lambda.</returns>
    Task<TOutput> Handler(TInput input, ILambdaContext context);
}
