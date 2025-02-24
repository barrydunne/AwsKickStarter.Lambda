using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AwsKickStarter.Lambda.Demo;

public class MyLambdaServiceConfiguration : IServiceConfiguration
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IMyService, MyService>();
        services.Configure<MyConfig>(configuration.GetSection("MyConfig"));
    }
}
