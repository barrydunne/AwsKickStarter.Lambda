using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AwsKickStarter.Lambda.Tests;

public abstract class LambdaTestsBase<TLambda, TLambdaHandler, TTestHandler> : IAsyncLifetime where TLambda : IAsyncDisposable where TLambdaHandler : class
{
    protected readonly ILambdaContext _mockLambdaContext;
    protected readonly TLambdaHandler _mockHandler;
    protected readonly TLambda _sut;

    private readonly ILambdaServiceBuilder _mockLambdaServiceBuilder;
    private readonly IServiceScope _mockServiceScope;
    private readonly IServiceScopeFactory _mockServiceScopeFactory;
    private readonly IServiceProvider _mockServiceProvider;
    private readonly ILambdaMiddleware _mockLambdaMiddleware;
    private readonly TLambda _sutDefault;

    protected LambdaTestsBase()
    {
        _mockLambdaContext = Substitute.For<ILambdaContext>();
        _mockHandler = Substitute.For<TLambdaHandler>();

        _mockLambdaServiceBuilder = Substitute.For<ILambdaServiceBuilder>();
        _mockLambdaServiceBuilder
            .ServiceProvider
            .Returns(_ => _mockServiceProvider);

        _mockServiceProvider = Substitute.For<IServiceProvider>();
        _mockServiceProvider
            .GetService(typeof(IServiceScopeFactory))
            .Returns(_ => _mockServiceScopeFactory);
        _mockServiceProvider
            .GetService(typeof(ILambdaMiddleware))
            .Returns(_ => _mockLambdaMiddleware);
        _mockServiceProvider
            .GetService(typeof(TLambdaHandler))
            .Returns(_ => _mockHandler);

        _mockServiceScopeFactory = Substitute.For<IServiceScopeFactory>();
        _mockServiceScopeFactory
            .CreateScope()
            .Returns(_ => _mockServiceScope);

        _mockServiceScope = Substitute.For<IServiceScope>();
        _mockServiceScope
            .ServiceProvider
            .Returns(_ => _mockServiceProvider);

        _mockLambdaMiddleware = Substitute.For<ILambdaMiddleware>();
        _mockLambdaMiddleware
            .Decode(Arg.Any<S3Event.S3EventNotificationRecord>())
            .Returns(callInfo =>
            {
                var record = callInfo.ArgAt<S3Event.S3EventNotificationRecord>(0);
                record.S3.Object.Key = $"DECODED:{record.S3.Object.Key}";
                return record;
            });
        _mockLambdaMiddleware
            .Decode(Arg.Any<SNSEvent.SNSRecord>())
            .Returns(callInfo =>
            {
                var record = callInfo.ArgAt<SNSEvent.SNSRecord>(0);
                record.Sns.Message = $"DECODED:{record.Sns.Message}";
                return record;
            });
        _mockLambdaMiddleware
            .Decode(Arg.Any<SQSEvent.SQSMessage>())
            .Returns(callInfo =>
            {
                var message = callInfo.ArgAt<SQSEvent.SQSMessage>(0);
                message.Body = $"DECODED:{message.Body}";
                return message;
            });
        _mockLambdaMiddleware
            .Deserialize<TestInput>(Arg.Any<SNSEvent.SNSRecord>())
            .Returns(callInfo =>
            {
                var record = callInfo.ArgAt<SNSEvent.SNSRecord>(0);
                return new TestInput($"DESERIALIZED:{record.Sns.Message}");
            });
        _mockLambdaMiddleware
            .Deserialize<TestInput>(Arg.Any<SQSEvent.SQSMessage>())
            .Returns(callInfo =>
            {
                var message = callInfo.ArgAt<SQSEvent.SQSMessage>(0);
                return new TestInput($"DESERIALIZED:{message.Body}");
            });

        _sut = CreateSut(_mockLambdaServiceBuilder);
        _sutDefault = CreateSut();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:Closing parenthesis should be spaced correctly", Justification = "Null-forgiving operator")]
    private ILambdaServiceBuilder DefaultServiceBuilder
        => (_sutDefault.GetType().GetProperty("ServiceBuilder", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(_sutDefault) as ILambdaServiceBuilder)!;

    internal abstract TLambda CreateSut();
    internal abstract TLambda CreateSut(ILambdaServiceBuilder lambdaServiceBuilder);

    [Fact]
    public void DefaultContructor_CreatesLambdaServiceBuilder()
        => DefaultServiceBuilder.ShouldBeOfType<LambdaServiceBuilder>();

    [Fact]
    public void DefaultContructor_AddsHandlerFromSameAssembly()
    {
        using var scope = DefaultServiceBuilder.ServiceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<TLambdaHandler>();
        handler.ShouldBeOfType<TTestHandler>();
    }

    [Fact]
    public async Task DisposeAsync_CallsDisposeAsyncOnLambdaServiceBuilder()
    {
        await _sut.DisposeAsync();
        await _mockLambdaServiceBuilder.Received(1).DisposeAsync();
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _sutDefault.DisposeAsync();
}
