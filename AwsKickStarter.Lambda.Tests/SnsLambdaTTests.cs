namespace AwsKickStarter.Lambda.Tests;

public class SnsLambdaTTests : LambdaTestsBase<SnsLambda<TestInput>, ISnsLambdaHandler<TestInput>, TestSnsLambdaHandler<TestInput>>
{
    private readonly Fixture _fixture = new();

    internal override SnsLambda<TestInput> CreateSut() => new Sut();
    internal override SnsLambda<TestInput> CreateSut(ILambdaServiceBuilder lambdaServiceBuilder) => new Sut(lambdaServiceBuilder);

    [Fact]
    public async Task Handler_CallsHandler()
    {
        var input = _fixture.Create<SNSEvent>();
        await _sut.Handler(input, _mockLambdaContext);
        await _mockHandler.Received(1).Handle(Arg.Any<IEnumerable<TestInput>>());
    }

    [Fact]
    public async Task Handler_DecodesAndDeserializesInput()
    {
        var input = _fixture.Create<SNSEvent>();
        var expectedMessages = input.Records.Select(_ => new TestInput($"DESERIALIZED:DECODED:{_.Sns.Message}")).ToArray();
        var handled = new ConcurrentBag<TestInput>();
        _mockHandler
            .When(_ => _.Handle(Arg.Any<IEnumerable<TestInput>>()))
            .Do(callInfo =>
            {
                var records = callInfo.ArgAt<IEnumerable<TestInput>>(0);
                foreach (var record in records)
                    handled.Add(record);
            });
        await _sut.Handler(input, _mockLambdaContext);
        handled.ShouldBe(expectedMessages, ignoreOrder: true);
    }

    private class Sut : SnsLambda<TestInput>
    {
        public Sut() : base() { }
        public Sut(ILambdaServiceBuilder lambdaServiceBuilder) : base(lambdaServiceBuilder) { }
    }
}
