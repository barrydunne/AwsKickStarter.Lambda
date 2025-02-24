namespace AwsKickStarter.Lambda.Demo.Simple.LambdaIn;

public class MyLambdaHandler : ILambdaInHandler<MyInput>
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

    public async Task Handle(MyInput input)
    {
        _logger.LogInformation("Handler received input: {@Input}", input);
        await _myService.Process(input);
    }
}
