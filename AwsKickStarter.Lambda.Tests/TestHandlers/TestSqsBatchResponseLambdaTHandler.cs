namespace AwsKickStarter.Lambda.Tests.TestHandlers;

public class TestSqsBatchResponseLambdaHandler<TMessage> : ISqsBatchResponseLambdaHandler<TMessage>
{
    public Task<bool> Handle(TMessage input) => throw new NotImplementedException();
}
