using Microsoft.Extensions.Options;

namespace AwsKickStarter.Lambda.Demo.Services;

internal class MyService : IMyService
{
    private readonly MyConfig _myConfig;
    private readonly ILogger _logger;

    public MyService(
        IOptions<MyConfig> myConfig,
        ILogger<MyService> logger)
    {
        _myConfig = myConfig.Value;
        _logger = logger;
    }

    public Task Process(MyInput input)
    {
        _logger.LogTrace("MyService processing {@Input}", input);
        if (_myConfig.Blacklist.Contains(input.name))
            throw new AccessViolationException($"{input.name} is not allowed");
        return Task.CompletedTask;
    }

    public Task Process(string input)
    {
        _logger.LogTrace("MyService processing {Input}", input);
        if (_myConfig.Blacklist.Any(name => input.Contains(name)))
            throw new AccessViolationException("Input contains blacklisted name");
        return Task.CompletedTask;
    }
}
