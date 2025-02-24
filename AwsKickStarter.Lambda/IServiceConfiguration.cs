using Microsoft.Extensions.Configuration;

namespace AwsKickStarter.Lambda;

/// <summary>
/// Configures services for dependency injection that are available to the lambda function handler.
/// </summary>
public interface IServiceConfiguration
{
    /// <summary>
    /// Configures services for dependency injection that are available to the lambda function handler.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <param name="configuration">The current lambda configuration as read from appsettings.json, appsettings.environment.json and environment variables.</param>
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);
}
