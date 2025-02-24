namespace AwsKickStarter.Lambda.Demo.Sns.Lambda;

public class MyMessageHandler : ISnsLambdaHandler
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

    public async Task Handle(IEnumerable<SNSEvent.SNSRecord> records)
    {
        _logger.LogInformation("Handler invoked");
        await Task.WhenAll(records.Select(_ => _myService.Process(_.Sns.Message)));
    }
}
