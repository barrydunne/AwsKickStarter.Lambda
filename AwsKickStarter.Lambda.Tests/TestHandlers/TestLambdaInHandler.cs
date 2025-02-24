namespace AwsKickStarter.Lambda.Tests.TestHandlers;

public class TestLambdaInHandler : ILambdaInHandler<TestInput>
{
    public Task Handle(TestInput input) => throw new NotImplementedException();
}
