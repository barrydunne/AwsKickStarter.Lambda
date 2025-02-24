namespace AwsKickStarter.Lambda;

/// <summary>
/// The base interface for Lambda handlers that have no input or output.
/// </summary>
public interface ILambdaHandler
{
    /// <summary>
    /// Handle an invocation of the lambda.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Handle();
}

/// <summary>
/// The base interface for Lambda handlers that have input and no output.
/// </summary>
/// <typeparam name="TInput">The type of input to the lambda.</typeparam>
public interface ILambdaInHandler<in TInput>
{
    /// <summary>
    /// Handle an invocation of the lambda.
    /// </summary>
    /// <param name="input">The input to the lambda.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Handle(TInput input);
}

/// <summary>
/// The base interface for Lambda handlers that have output and no input.
/// </summary>
/// <typeparam name="TOutput">The type of output from the lambda.</typeparam>
public interface ILambdaOutHandler<TOutput>
{
    /// <summary>
    /// Handle an invocation of the lambda.
    /// </summary>
    /// <returns>The output of the lambda invocation.</returns>
    Task<TOutput> Handle();
}

/// <summary>
/// The base interface for Lambda handlers that have input and output.
/// </summary>
/// <typeparam name="TInput">The type of input to the lambda.</typeparam>
/// <typeparam name="TOutput">The type of output from the lambda.</typeparam>
public interface ILambdaInOutHandler<in TInput, TOutput>
{
    /// <summary>
    /// Handle an invocation of the lambda.
    /// </summary>
    /// <param name="input">The input to the lambda.</param>
    /// <returns>The output of the lambda invocation.</returns>
    Task<TOutput> Handle(TInput input);
}

/// <summary>
/// The base interface for Lambda handlers that have an S3 source.
/// </summary>
public interface IS3LambdaHandler : ILambdaInHandler<IEnumerable<S3Event.S3EventNotificationRecord>> { }

/// <summary>
/// The base interface for Lambda handlers that have an SNS source and handle <see cref="SNSEvent.SNSRecord"/> input.
/// </summary>
public interface ISnsLambdaHandler : ILambdaInHandler<IEnumerable<SNSEvent.SNSRecord>> { }

/// <summary>
/// The base interface for Lambda handlers that have an SNS source and handle deserialized input.
/// </summary>
/// <typeparam name="TMessage">The type that the JSON input will be deserialized to.</typeparam>
public interface ISnsLambdaHandler<TMessage> : ILambdaInHandler<IEnumerable<TMessage>> { }

/// <summary>
/// The base interface for Lambda handlers that have an SQS source and handle <see cref="SQSEvent.SQSMessage"/> input.
/// </summary>
public interface ISqsLambdaHandler : ILambdaInHandler<IEnumerable<SQSEvent.SQSMessage>> { }

/// <summary>
/// The base interface for Lambda handlers that have an SQS source and handle deserialized input.
/// </summary>
/// <typeparam name="TMessage">The type that the JSON input will be deserialized to.</typeparam>
public interface ISqsLambdaHandler<TMessage> : ILambdaInHandler<IEnumerable<TMessage>> { }

/// <summary>
/// The base interface for Lambda handlers that have an SQS source configured with ReportBatchItemFailures.
/// </summary>
public interface ISqsBatchResponseLambdaHandler : ILambdaInOutHandler<SQSEvent.SQSMessage, bool> { }

/// <summary>
/// The base interface for Lambda handlers that have an SQS source configured with ReportBatchItemFailures and handle deserialized input.
/// </summary>
/// <typeparam name="TMessage">The type that the JSON input will be deserialized to.</typeparam>
public interface ISqsBatchResponseLambdaHandler<TMessage> : ILambdaInOutHandler<TMessage, bool> { }
