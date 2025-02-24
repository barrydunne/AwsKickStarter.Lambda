namespace AwsKickStarter.Lambda.Tests;

public class LambdaInOutTests : LambdaTestsBase<LambdaInOut<TestInput, TestOutput>, ILambdaInOutHandler<TestInput, TestOutput>, TestLambdaInOutHandler>
{
    private readonly Fixture _fixture = new();

    internal override LambdaInOut<TestInput, TestOutput> CreateSut() => new Sut();
    internal override LambdaInOut<TestInput, TestOutput> CreateSut(ILambdaServiceBuilder lambdaServiceBuilder) => new Sut(lambdaServiceBuilder);

    [Fact]
    public async Task Handler_CallsHandler()
    {
        var input = _fixture.Create<TestInput>();
        await _sut.Handler(input, _mockLambdaContext);
        await _mockHandler.Received(1).Handle(input);
    }

    [Fact]
    public async Task Handler_ReturnsResponse()
    {
        var input = _fixture.Create<TestInput>();
        var output = _fixture.Create<TestOutput>();
        _mockHandler.Handle(input).Returns(_ => output);
        var result = await _sut.Handler(input, _mockLambdaContext);
        result.ShouldBe(output);
    }

    private class Sut : LambdaInOut<TestInput, TestOutput>
    {
        public Sut() : base() { }
        public Sut(ILambdaServiceBuilder lambdaServiceBuilder) : base(lambdaServiceBuilder) { }
    }
}
