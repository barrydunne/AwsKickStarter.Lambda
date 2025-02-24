namespace AwsKickStarter.Lambda.Tests.TestHandlers;

public class TestSqsBatchResponseLambdaHandler : ISqsBatchResponseLambdaHandler
{
    public Task<bool> Handle(SQSEvent.SQSMessage input) => throw new NotImplementedException();
}
