namespace AwsKickStarter.Lambda.Tests.TestHandlers;

public class TestSnsLambdaHandler<TMessage> : ISnsLambdaHandler<TMessage>
{
    public Task Handle(IEnumerable<TMessage> input) => throw new NotImplementedException();
}
