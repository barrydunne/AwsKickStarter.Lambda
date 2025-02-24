namespace AwsKickStarter.Lambda.Tests.TestHandlers;

public class TestSqsLambdaHandler : ISqsLambdaHandler
{
    public Task Handle(IEnumerable<SQSEvent.SQSMessage> input) => throw new NotImplementedException();
}
