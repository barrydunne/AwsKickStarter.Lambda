namespace AwsKickStarter.Lambda.Demo.Sqs.BasicT;

public class MyMessageHandler : ISqsLambdaHandler<MyInput>
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

    public async Task Handle(IEnumerable<MyInput> messages)
    {
        _logger.LogInformation("Handler invoked");
        await Task.WhenAll(messages.Select(_myService.Process));
    }
}
