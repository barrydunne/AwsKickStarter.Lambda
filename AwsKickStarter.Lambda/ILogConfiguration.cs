using Serilog;

namespace AwsKickStarter.Lambda;

/// <summary>
/// Provides a way to apply additional configuration of the logging for the lambda function.
/// </summary>
public interface ILogConfiguration
{
    /// <summary>
    /// Applies any custom logging configuration for the lambda function.
    /// </summary>
    /// <param name="loggerConfiguration">The current configiration that will be used to create the logger.</param>
    void ConfigureLogging(LoggerConfiguration loggerConfiguration);
}
