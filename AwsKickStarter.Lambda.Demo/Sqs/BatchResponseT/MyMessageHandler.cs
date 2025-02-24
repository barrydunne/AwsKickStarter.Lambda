namespace AwsKickStarter.Lambda.Demo.Sqs.BatchResponseT;

public class MyMessageHandler : ISqsBatchResponseLambdaHandler<MyInput>
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

    public async Task<bool> Handle(MyInput message)
    {
        _logger.LogInformation("Handler received message: {@Message}", message);
        await _myService.Process(message);
        return true;
    }
}
