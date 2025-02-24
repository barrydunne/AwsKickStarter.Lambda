namespace AwsKickStarter.Lambda.Tests;

public class LambdaOutTests : LambdaTestsBase<LambdaOut<TestOutput>, ILambdaOutHandler<TestOutput>, TestLambdaOutHandler>
{
    private readonly Fixture _fixture = new();

    internal override LambdaOut<TestOutput> CreateSut() => new Sut();
    internal override LambdaOut<TestOutput> CreateSut(ILambdaServiceBuilder lambdaServiceBuilder) => new Sut(lambdaServiceBuilder);

    [Fact]
    public async Task Handler_CallsHandler()
    {
        await _sut.Handler(_mockLambdaContext);
        await _mockHandler.Received(1).Handle();
    }

    [Fact]
    public async Task Handler_ReturnsResponse()
    {
        var output = _fixture.Create<TestOutput>();
        _mockHandler.Handle().Returns(_ => output);
        var result = await _sut.Handler(_mockLambdaContext);
        result.ShouldBe(output);
    }

    private class Sut : LambdaOut<TestOutput>
    {
        public Sut() : base() { }
        public Sut(ILambdaServiceBuilder lambdaServiceBuilder) : base(lambdaServiceBuilder) { }
    }
}
