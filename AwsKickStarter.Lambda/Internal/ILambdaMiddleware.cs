namespace AwsKickStarter.Lambda.Internal;

/// <summary>
/// Middleware processing for lambda input handling.
/// </summary>
internal interface ILambdaMiddleware
{
    /// <summary>
    /// Decode an S3 event notification record.
    /// This will restore the keys that may have been URL encoded to change characters such as spaces.
    /// </summary>
    /// <param name="record">The original input.</param>
    /// <returns>The updated input with the key decoded.</returns>
    S3Event.S3EventNotificationRecord Decode(S3Event.S3EventNotificationRecord record);

    /// <summary>
    /// Decode an SNS record to inflate messages with gzip content encoding if required.
    /// </summary>
    /// <param name="record">The original input.</param>
    /// <returns>The updated input with the message decoded.</returns>
    SNSEvent.SNSRecord Decode(SNSEvent.SNSRecord record);

    /// <summary>
    /// Decode an SQS message to inflate messages with gzip content encoding if required.
    /// </summary>
    /// <param name="message">The original input.</param>
    /// <returns>The updated input with the message decoded.</returns>
    SQSEvent.SQSMessage Decode(SQSEvent.SQSMessage message);

    /// <summary>
    /// Deserialize the message from the SNS record.
    /// </summary>
    /// <typeparam name="TMessage">The type to deserialize to.</typeparam>
    /// <param name="record">The input with the JSON message to be deserialized.</param>
    /// <returns>A deserialized instance.</returns>
    /// <exception cref="DeserializeException{TMessage}">Thrown when the input cannot be deserialized.</exception>
    TMessage Deserialize<TMessage>(SNSEvent.SNSRecord record);

    /// <summary>
    /// Deserialize the message from the SQS message.
    /// </summary>
    /// <typeparam name="TMessage">The type to deserialize to.</typeparam>
    /// <param name="message">The input with the JSON message to be deserialized.</param>
    /// <returns>A deserialized instance.</returns>
    /// <exception cref="DeserializeException{TMessage}">Thrown when the input cannot be deserialized.</exception>
    TMessage Deserialize<TMessage>(SQSEvent.SQSMessage message);
}
