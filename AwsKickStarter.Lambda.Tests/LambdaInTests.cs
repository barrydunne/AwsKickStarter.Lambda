namespace AwsKickStarter.Lambda.Tests;

public class LambdaInTests : LambdaTestsBase<LambdaIn<TestInput>, ILambdaInHandler<TestInput>, TestLambdaInHandler>
{
    private readonly Fixture _fixture = new();

    internal override LambdaIn<TestInput> CreateSut() => new Sut();
    internal override LambdaIn<TestInput> CreateSut(ILambdaServiceBuilder lambdaServiceBuilder) => new Sut(lambdaServiceBuilder);

    [Fact]
    public async Task Handler_CallsHandler()
    {
        var input = _fixture.Create<TestInput>();
        await _sut.Handler(input, _mockLambdaContext);
        await _mockHandler.Received(1).Handle(input);
    }

    private class Sut : LambdaIn<TestInput>
    {
        public Sut() : base() { }
        public Sut(ILambdaServiceBuilder lambdaServiceBuilder) : base(lambdaServiceBuilder) { }
    }
}
