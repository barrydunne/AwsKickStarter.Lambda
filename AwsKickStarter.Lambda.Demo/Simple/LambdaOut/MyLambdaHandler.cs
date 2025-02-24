namespace AwsKickStarter.Lambda.Demo.Simple.LambdaOut;

public class MyLambdaHandler : ILambdaOutHandler<MyOutput>
{
    private readonly ILogger _logger;

    public MyLambdaHandler(ILogger<MyLambdaHandler> logger) => _logger = logger;

    public Task<MyOutput> Handle()
    {
        _logger.LogInformation("Handler invoked");
        return Task.FromResult(new MyOutput(DateTimeOffset.UtcNow, 1));
    }
}
