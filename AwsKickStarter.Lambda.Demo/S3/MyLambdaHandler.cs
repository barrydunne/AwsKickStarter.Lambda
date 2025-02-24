namespace AwsKickStarter.Lambda.Demo.S3;

public class MyLambdaHandler : IS3LambdaHandler
{
    private readonly ILogger _logger;

    public MyLambdaHandler(ILogger<MyLambdaHandler> logger) => _logger = logger;

    public Task Handle(IEnumerable<S3Event.S3EventNotificationRecord> input)
    {
        _logger.LogInformation("Handler invoked");
        foreach (var record in input)
            _logger.LogInformation("S3Event: {@Record}", record);
        return Task.CompletedTask;
    }
}
