namespace AwsKickStarter.Lambda.Demo.Sqs.Basic;

public class MyMessageHandler : ISqsLambdaHandler
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

    public async Task Handle(IEnumerable<SQSEvent.SQSMessage> messages)
    {
        _logger.LogInformation("Handler invoked");
        await Task.WhenAll(messages.Select(_ => _myService.Process(_.Body)));
    }
}
