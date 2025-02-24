using NSubstitute.ReceivedExtensions;

namespace AwsKickStarter.Lambda.Tests;

public class SqsBatchResponseLambdaTests : LambdaTestsBase<SqsBatchResponseLambda, ISqsBatchResponseLambdaHandler, TestSqsBatchResponseLambdaHandler>
{
    private readonly Fixture _fixture = new();

    internal override SqsBatchResponseLambda CreateSut() => new Sut();
    internal override SqsBatchResponseLambda CreateSut(ILambdaServiceBuilder lambdaServiceBuilder) => new Sut(lambdaServiceBuilder);

    [Fact]
    public async Task Handler_CallsHandler()
    {
        var input = new SQSEvent { Records = _fixture.Build<SQSEvent.SQSMessage>().With(_ => _.MessageAttributes, []).CreateMany().ToList() };
        await _sut.Handler(input, _mockLambdaContext);
        await _mockHandler.Received(input.Records.Count).Handle(Arg.Any<SQSEvent.SQSMessage>());
    }

    [Fact]
    public async Task Handler_DecodesInput()
    {
        var input = new SQSEvent { Records = _fixture.Build<SQSEvent.SQSMessage>().With(_ => _.MessageAttributes, []).CreateMany().ToList() };
        var handled = new ConcurrentBag<string>();
        _mockHandler
            .When(_ => _.Handle(Arg.Any<SQSEvent.SQSMessage>()))
            .Do(callInfo =>
            {
                var message = callInfo.ArgAt<SQSEvent.SQSMessage>(0);
                handled.Add($"DECODED:{message.Body}");
            });
        await _sut.Handler(input, _mockLambdaContext);
        handled.ShouldBe(input.Records.Select(_ => $"DECODED:{_.Body}"), ignoreOrder: true);
    }

    [Fact]
    public async Task Handler_Successful_ReturnsEmptyResponse()
    {
        var input = new SQSEvent { Records = _fixture.Build<SQSEvent.SQSMessage>().With(_ => _.MessageAttributes, []).CreateMany().ToList() };
        _mockHandler
            .Handle(Arg.Any<SQSEvent.SQSMessage>())
            .Returns(_ => true);
        var result = await _sut.Handler(input, _mockLambdaContext);
        result.BatchItemFailures.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handler_Unsuccessful_ReturnsFailedIds()
    {
        var input = new SQSEvent { Records = _fixture.Build<SQSEvent.SQSMessage>().With(_ => _.MessageAttributes, []).CreateMany(10).ToList() };
        var failingMessages = input.Records.OrderBy(_ => Random.Shared.Next()).Take(4).ToArray();
        _mockHandler
            .Handle(Arg.Any<SQSEvent.SQSMessage>())
            .Returns(callInfo => !failingMessages.Contains(callInfo.ArgAt<SQSEvent.SQSMessage>(0)));
        var result = await _sut.Handler(input, _mockLambdaContext);
        result.BatchItemFailures.Select(_ => _.ItemIdentifier).ShouldBe(failingMessages.Select(_ => _.MessageId), ignoreOrder: true);
    }

    [Fact]
    public async Task Handler_Exception_ReturnsFailedIds()
    {
        var input = new SQSEvent { Records = _fixture.Build<SQSEvent.SQSMessage>().With(_ => _.MessageAttributes, []).CreateMany(10).ToList() };
        var failingMessages = input.Records.OrderBy(_ => Random.Shared.Next()).Take(4).ToArray();
        _mockHandler
            .Handle(Arg.Any<SQSEvent.SQSMessage>())
            .Returns(callInfo => failingMessages.Contains(callInfo.ArgAt<SQSEvent.SQSMessage>(0)) ? throw new ApplicationException() : true);
        var result = await _sut.Handler(input, _mockLambdaContext);
        result.BatchItemFailures.Select(_ => _.ItemIdentifier).ShouldBe(failingMessages.Select(_ => _.MessageId), ignoreOrder: true);
    }

    private class Sut : SqsBatchResponseLambda
    {
        public Sut() : base() { }
        public Sut(ILambdaServiceBuilder lambdaServiceBuilder) : base(lambdaServiceBuilder) { }
    }
}
