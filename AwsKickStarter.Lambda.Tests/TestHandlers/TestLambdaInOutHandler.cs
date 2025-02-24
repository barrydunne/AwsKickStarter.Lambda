namespace AwsKickStarter.Lambda.Tests.TestHandlers;

public class TestLambdaInOutHandler : ILambdaInOutHandler<TestInput, TestOutput>
{
    public Task<TestOutput> Handle(TestInput input) => throw new NotImplementedException();
}
