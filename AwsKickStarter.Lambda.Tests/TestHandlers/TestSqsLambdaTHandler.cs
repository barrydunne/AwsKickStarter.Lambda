namespace AwsKickStarter.Lambda.Tests.TestHandlers;

public class TestSqsLambdaHandler<TMessage> : ISqsLambdaHandler<TMessage>
{
    public Task Handle(IEnumerable<TMessage> input) => throw new NotImplementedException();
}
