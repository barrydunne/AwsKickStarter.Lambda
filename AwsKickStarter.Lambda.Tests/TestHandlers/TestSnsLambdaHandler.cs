namespace AwsKickStarter.Lambda.Tests.TestHandlers;

public class TestSnsLambdaHandler : ISnsLambdaHandler
{
    public Task Handle(IEnumerable<SNSEvent.SNSRecord> input) => throw new NotImplementedException();
}
