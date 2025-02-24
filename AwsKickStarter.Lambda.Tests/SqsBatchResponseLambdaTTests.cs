using NSubstitute.ReceivedExtensions;

namespace AwsKickStarter.Lambda.Tests;

public class SqsBatchResponseLambdaTTests : LambdaTestsBase<SqsBatchResponseLambda<TestInput>, ISqsBatchResponseLambdaHandler<TestInput>, TestSqsBatchResponseLambdaHandler<TestInput>>
{
    private readonly Fixture _fixture = new();

    internal override SqsBatchResponseLambda<TestInput> CreateSut() => new Sut();
    internal override SqsBatchResponseLambda<TestInput> CreateSut(ILambdaServiceBuilder lambdaServiceBuilder) => new Sut(lambdaServiceBuilder);

    [Fact]
    public async Task Handler_CallsHandler()
    {
        var input = new SQSEvent { Records = _fixture.Build<SQSEvent.SQSMessage>().With(_ => _.MessageAttributes, []).CreateMany().ToList() };
        await _sut.Handler(input, _mockLambdaContext);
        await _mockHandler.Received(input.Records.Count).Handle(Arg.Any<TestInput>());
    }

    [Fact]
    public async Task Handler_DecodesAndDeserializesInput()
    {
        var input = new SQSEvent { Records = _fixture.Build<SQSEvent.SQSMessage>().With(_ => _.MessageAttributes, []).CreateMany().ToList() };
        var expectedMessages = input.Records.Select(_ => new TestInput($"DESERIALIZED:DECODED:{_.Body}")).ToArray();
        var handled = new ConcurrentBag<TestInput>();
        _mockHandler
            .When(_ => _.Handle(Arg.Any<TestInput>()))
            .Do(callInfo =>
            {
                var record = callInfo.ArgAt<TestInput>(0);
                handled.Add(record);
            });
        await _sut.Handler(input, _mockLambdaContext);
        handled.ShouldBe(expectedMessages, ignoreOrder: true);
    }

    [Fact]
    public async Task Handler_Successful_ReturnsEmptyResponse()
    {
        var input = new SQSEvent { Records = _fixture.Build<SQSEvent.SQSMessage>().With(_ => _.MessageAttributes, []).CreateMany().ToList() };
        _mockHandler
            .Handle(Arg.Any<TestInput>())
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
            .Handle(Arg.Any<TestInput>())
            .Returns(callInfo => !failingMessages.Any(_ => callInfo.ArgAt<TestInput>(0).message.EndsWith(_.Body)));
        var result = await _sut.Handler(input, _mockLambdaContext);
        result.BatchItemFailures.Select(_ => _.ItemIdentifier).ShouldBe(failingMessages.Select(_ => _.MessageId), ignoreOrder: true);
    }

    [Fact]
    public async Task Handler_Exception_ReturnsFailedIds()
    {
        var input = new SQSEvent { Records = _fixture.Build<SQSEvent.SQSMessage>().With(_ => _.MessageAttributes, []).CreateMany(10).ToList() };
        var failingMessages = input.Records.OrderBy(_ => Random.Shared.Next()).Take(4).ToArray();
        _mockHandler
            .Handle(Arg.Any<TestInput>())
            .Returns(callInfo => failingMessages.Any(_ => callInfo.ArgAt<TestInput>(0).message.EndsWith(_.Body)) ? throw new ApplicationException() : true);
        var result = await _sut.Handler(input, _mockLambdaContext);
        result.BatchItemFailures.Select(_ => _.ItemIdentifier).ShouldBe(failingMessages.Select(_ => _.MessageId), ignoreOrder: true);
    }

    private class Sut : SqsBatchResponseLambda<TestInput>
    {
        public Sut() : base() { }
        public Sut(ILambdaServiceBuilder lambdaServiceBuilder) : base(lambdaServiceBuilder) { }
    }
}
