namespace AwsKickStarter.Lambda.Tests;

public class LambdaTests : LambdaTestsBase<Lambda, ILambdaHandler, TestLambdaHandler>
{
    internal override Lambda CreateSut() => new Sut();
    internal override Lambda CreateSut(ILambdaServiceBuilder lambdaServiceBuilder) => new Sut(lambdaServiceBuilder);

    [Fact]
    public async Task Handler_CallsHandler()
    {
        await _sut.Handler(_mockLambdaContext);
        await _mockHandler.Received(1).Handle();
    }

    private class Sut : Lambda
    {
        public Sut() : base() { }
        public Sut(ILambdaServiceBuilder lambdaServiceBuilder) : base(lambdaServiceBuilder) { }
    }
}
