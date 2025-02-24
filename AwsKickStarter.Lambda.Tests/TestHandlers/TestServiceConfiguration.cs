using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AwsKickStarter.Lambda.Tests.TestHandlers;

public class TestServiceConfiguration : IServiceConfiguration
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration) { }
}
