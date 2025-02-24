namespace AwsKickStarter.Lambda.Tests;

public class SqsLambdaTests : LambdaTestsBase<SqsLambda, ISqsLambdaHandler, TestSqsLambdaHandler>
{
    private readonly Fixture _fixture = new();

    internal override SqsLambda CreateSut() => new Sut();
    internal override SqsLambda CreateSut(ILambdaServiceBuilder lambdaServiceBuilder) => new Sut(lambdaServiceBuilder);

    [Fact]
    public async Task Handler_CallsHandler()
    {
        var input = new SQSEvent { Records = _fixture.Build<SQSEvent.SQSMessage>().With(_ => _.MessageAttributes, []).CreateMany().ToList() };
        await _sut.Handler(input, _mockLambdaContext);
        await _mockHandler.Received(1).Handle(Arg.Any<IEnumerable<SQSEvent.SQSMessage>>());
    }

    [Fact]
    public async Task Handler_DecodesInput()
    {
        var input = new SQSEvent { Records = _fixture.Build<SQSEvent.SQSMessage>().With(_ => _.MessageAttributes, []).CreateMany().ToList() };
        var handled = new ConcurrentBag<string>();
        _mockHandler
            .When(_ => _.Handle(Arg.Any<IEnumerable<SQSEvent.SQSMessage>>()))
            .Do(callInfo =>
            {
                var messages = callInfo.ArgAt<IEnumerable<SQSEvent.SQSMessage>>(0);
                foreach (var message in messages)
                    handled.Add($"DECODED:{message.Body}");
            });
        await _sut.Handler(input, _mockLambdaContext);
        handled.ShouldBe(input.Records.Select(_ => $"DECODED:{_.Body}"), ignoreOrder: true);
    }

    private class Sut : SqsLambda
    {
        public Sut() : base() { }
        public Sut(ILambdaServiceBuilder lambdaServiceBuilder) : base(lambdaServiceBuilder) { }
    }
}
