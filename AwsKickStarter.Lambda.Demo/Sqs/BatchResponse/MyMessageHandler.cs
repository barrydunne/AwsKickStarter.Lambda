namespace AwsKickStarter.Lambda.Demo.Sqs.BatchResponse;

public class MyMessageHandler : ISqsBatchResponseLambdaHandler
{
    private readonly IMyService _myService;
    private readonly ILogger _logger;

    public MyMessageHandler(
        IMyService myService,
        ILogger<MyMessageHandler> logger)
    {
        _myService = myService;
        _logger = logger;
    }

    public async Task<bool> Handle(SQSEvent.SQSMessage message)
    {
        _logger.LogInformation("Handler received message: {Message}", message.Body);
        await _myService.Process(message.Body);
        return true;
    }
}
