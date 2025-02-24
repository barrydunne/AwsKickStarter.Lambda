namespace AwsKickStarter.Lambda.Internal;

/// <summary>
/// Provides the common functionality for handling SQS batch events.
/// </summary>
internal class SqsBatchHandler
{
    /// <summary>
    /// Handles the SQS batch event.
    /// </summary>
    /// <param name="sqsEvent">The incoming event.</param>
    /// <param name="context">The context provided by AWS on invocation of the lambda.</param>
    /// <param name="handle">The handling implementation.</param>
    /// <returns>An instanace of a <see cref="SQSBatchResponse"/> that contains the message ids of any failures.</returns>
    internal async Task<SQSBatchResponse> Handle(
        SQSEvent sqsEvent,
        ILambdaContext context,
        Func<SQSEvent.SQSMessage, Task<bool>> handle)
    {
        var batchItemFailures = new ConcurrentBag<SQSBatchResponse.BatchItemFailure>();
        await Parallel.ForEachAsync(sqsEvent.Records, async (record, _) =>
        {
            var success = false;
            try
            {
                success = await handle(record);
            }
            catch (Exception ex)
            {
                context.Logger.LogError(ex, "Error processing message {Body}", record.Body);
            }
            if (!success)
            {
                batchItemFailures.Add(new() { ItemIdentifier = record.MessageId });
            }
        });

        return new(batchItemFailures.ToList());
    }
}
