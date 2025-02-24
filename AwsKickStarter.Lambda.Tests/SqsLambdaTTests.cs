namespace AwsKickStarter.Lambda.Tests;

public class SqsLambdaTTests : LambdaTestsBase<SqsLambda<TestInput>, ISqsLambdaHandler<TestInput>, TestSqsLambdaHandler<TestInput>>
{
    private readonly Fixture _fixture = new();

    internal override SqsLambda<TestInput> CreateSut() => new Sut();
    internal override SqsLambda<TestInput> CreateSut(ILambdaServiceBuilder lambdaServiceBuilder) => new Sut(lambdaServiceBuilder);

    [Fact]
    public async Task Handler_CallsHandler()
    {
        var input = new SQSEvent { Records = _fixture.Build<SQSEvent.SQSMessage>().With(_ => _.MessageAttributes, []).CreateMany().ToList() };
        await _sut.Handler(input, _mockLambdaContext);
        await _mockHandler.Received(1).Handle(Arg.Any<IEnumerable<TestInput>>());
    }

    [Fact]
    public async Task Handler_DecodesAndDeserializesInput()
    {
        var input = new SQSEvent { Records = _fixture.Build<SQSEvent.SQSMessage>().With(_ => _.MessageAttributes, []).CreateMany().ToList() };
        var expectedMessages = input.Records.Select(_ => new TestInput($"DESERIALIZED:DECODED:{_.Body}")).ToArray();
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

    private class Sut : SqsLambda<TestInput>
    {
        public Sut() : base() { }
        public Sut(ILambdaServiceBuilder lambdaServiceBuilder) : base(lambdaServiceBuilder) { }
    }
}
