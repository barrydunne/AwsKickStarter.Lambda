namespace AwsKickStarter.Lambda.Tests;

public class S3LambdaTests : LambdaTestsBase<S3Lambda, IS3LambdaHandler, TestS3LambdaHandler>
{
    private readonly Fixture _fixture = new();

    internal override S3Lambda CreateSut() => new Sut();
    internal override S3Lambda CreateSut(ILambdaServiceBuilder lambdaServiceBuilder) => new Sut(lambdaServiceBuilder);

    [Fact]
    public async Task Handler_CallsHandler()
    {
        var input = _fixture.Create<S3Event>();
        await _sut.Handler(input, _mockLambdaContext);
        await _mockHandler.Received(1).Handle(Arg.Any<IEnumerable<S3Event.S3EventNotificationRecord>>());
    }

    [Fact]
    public async Task Handler_DecodesInput()
    {
        var input = _fixture.Create<S3Event>();
        var handled = new ConcurrentBag<string>();
        _mockHandler
            .When(_ => _.Handle(Arg.Any<IEnumerable<S3Event.S3EventNotificationRecord>>()))
            .Do(callInfo =>
            {
                var records = callInfo.ArgAt<IEnumerable<S3Event.S3EventNotificationRecord>>(0);
                foreach (var record in records)
                    handled.Add($"DECODED:{record.S3.Object.Key}");
            });
        await _sut.Handler(input, _mockLambdaContext);
        handled.ShouldBe(input.Records.Select(_ => $"DECODED:{_.S3.Object.Key}"), ignoreOrder: true);
    }

    private class Sut : S3Lambda
    {
        public Sut() : base() { }
        public Sut(ILambdaServiceBuilder lambdaServiceBuilder) : base(lambdaServiceBuilder) { }
    }
}
