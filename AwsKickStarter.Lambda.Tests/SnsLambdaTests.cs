namespace AwsKickStarter.Lambda.Tests;

public class SnsLambdaTests : LambdaTestsBase<SnsLambda, ISnsLambdaHandler, TestSnsLambdaHandler>
{
    private readonly Fixture _fixture = new();

    internal override SnsLambda CreateSut() => new Sut();
    internal override SnsLambda CreateSut(ILambdaServiceBuilder lambdaServiceBuilder) => new Sut(lambdaServiceBuilder);

    [Fact]
    public async Task Handler_CallsHandler()
    {
        var input = _fixture.Create<SNSEvent>();
        await _sut.Handler(input, _mockLambdaContext);
        await _mockHandler.Received(1).Handle(Arg.Any<IEnumerable<SNSEvent.SNSRecord>>());
    }

    [Fact]
    public async Task Handler_DecodesInput()
    {
        var input = _fixture.Create<SNSEvent>();
        var handled = new ConcurrentBag<string>();
        _mockHandler
            .When(_ => _.Handle(Arg.Any<IEnumerable<SNSEvent.SNSRecord>>()))
            .Do(callInfo =>
            {
                var records = callInfo.ArgAt<IEnumerable<SNSEvent.SNSRecord>>(0);
                foreach (var record in records)
                    handled.Add($"DECODED:{record.Sns.Message}");
            });
        await _sut.Handler(input, _mockLambdaContext);
        handled.ShouldBe(input.Records.Select(_ => $"DECODED:{_.Sns.Message}"), ignoreOrder: true);
    }

    private class Sut : SnsLambda
    {
        public Sut() : base() { }
        public Sut(ILambdaServiceBuilder lambdaServiceBuilder) : base(lambdaServiceBuilder) { }
    }
}
