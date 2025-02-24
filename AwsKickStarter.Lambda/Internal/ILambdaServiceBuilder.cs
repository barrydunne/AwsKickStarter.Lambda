using Microsoft.Extensions.Configuration;

namespace AwsKickStarter.Lambda.Internal;

/// <summary>
/// Builds the service provider and configuration for the lambda function.
/// </summary>
internal interface ILambdaServiceBuilder : IAsyncDisposable
{
    /// <summary>
    /// Gets the configuration for the lambda function.
    /// </summary>
    IConfiguration Configuration { get; }

    /// <summary>
    /// Gets the service provider for the lambda function.
    /// </summary>
    IServiceProvider ServiceProvider { get; }
}
