namespace AwsKickStarter.Lambda.Tests.TestHandlers;

public class TestS3LambdaHandler : IS3LambdaHandler
{
    public Task Handle(IEnumerable<S3Event.S3EventNotificationRecord> input) => throw new NotImplementedException();
}
