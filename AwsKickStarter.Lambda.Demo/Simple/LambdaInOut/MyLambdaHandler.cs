namespace AwsKickStarter.Lambda.Demo.Simple.LambdaInOut;

public class MyLambdaHandler : ILambdaInOutHandler<MyInput, MyOutput>
{
    private readonly IMyService _myService;
    private readonly ILogger _logger;

    public MyLambdaHandler(
        IMyService myService,
        ILogger<MyLambdaHandler> logger)
    {
        _myService = myService;
        _logger = logger;
    }

    public async Task<MyOutput> Handle(MyInput input)
    {
        _logger.LogInformation("Handler received input: {@Input}", input);
        await _myService.Process(input);
        return new MyOutput(DateTimeOffset.UtcNow, 1);
    }
}
