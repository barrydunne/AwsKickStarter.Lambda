namespace AwsKickStarter.Lambda.Demo.Simple.Lambda;

public class MyLambdaHandler : ILambdaHandler
{
    private readonly ILogger _logger;

    public MyLambdaHandler(ILogger<MyLambdaHandler> logger) => _logger = logger;

    public Task Handle()
    {
        _logger.LogInformation("Handler invoked");
        return Task.CompletedTask;
    }
}
