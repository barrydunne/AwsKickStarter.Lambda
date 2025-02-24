namespace AwsKickStarter.Lambda.Tests.TestHandlers;

public class TestLambdaOutHandler : ILambdaOutHandler<TestOutput>
{
    public Task<TestOutput> Handle() => throw new NotImplementedException();
}
