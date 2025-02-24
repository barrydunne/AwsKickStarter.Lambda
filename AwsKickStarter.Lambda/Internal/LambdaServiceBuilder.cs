using Microsoft.Extensions.Configuration;
using Serilog;
using System.Reflection;

namespace AwsKickStarter.Lambda.Internal;

/// <inheritdoc/>
internal class LambdaServiceBuilder : ILambdaServiceBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LambdaServiceBuilder"/> class.
    /// </summary>
    /// <param name="serviceAssembly">The assembly to scan for interface implementations.</param>
    internal LambdaServiceBuilder(Assembly serviceAssembly)
    {
        Configuration = BuildConfiguration();

        var services = new ServiceCollection();
        services.AddSingleton(Configuration);
        ConfigureLogging(services);
        ApplyCustomServiceConfiguration(services, serviceAssembly);
        ServiceProvider = services.BuildServiceProvider();
    }

    /// <inheritdoc/>
    public IServiceProvider ServiceProvider { get; }

    /// <inheritdoc/>
    public IConfiguration Configuration { get; }

    private static IConfiguration BuildConfiguration()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        return new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
    }

    private void ConfigureLogging(ServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(Configuration)
            .CreateLogger();

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog();
        });
    }

    private void ApplyCustomServiceConfiguration(ServiceCollection services, Assembly serviceAssembly)
    {
        services.AddSingleton<ILambdaMiddleware, LambdaMiddleware>();

        var typeofIServiceConfiguration = typeof(IServiceConfiguration);
        var serviceConfigurations = serviceAssembly
            .GetTypes()
            .Where(type => typeofIServiceConfiguration.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
            .Select(Activator.CreateInstance)
            .Cast<IServiceConfiguration>();

        foreach (var serviceConfiguration in serviceConfigurations)
            serviceConfiguration.ConfigureServices(services, Configuration);

        services
            .Scan(scan => scan
            .FromAssemblies(serviceAssembly)
            .AddClasses(classes => classes.AssignableTo<ILambdaHandler>()).AsImplementedInterfaces().WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(ILambdaInHandler<>))).AsImplementedInterfaces().WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(ILambdaOutHandler<>))).AsImplementedInterfaces().WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(ILambdaInOutHandler<,>))).AsImplementedInterfaces().WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo<IS3LambdaHandler>()).AsImplementedInterfaces().WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo<ISqsLambdaHandler>()).AsImplementedInterfaces().WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(ISqsLambdaHandler<>))).AsImplementedInterfaces().WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo<ISqsBatchResponseLambdaHandler>()).AsImplementedInterfaces().WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(ISqsBatchResponseLambdaHandler<>))).AsImplementedInterfaces().WithScopedLifetime());
    }

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:Closing parenthesis should be spaced correctly", Justification = "Null-forgiving operator")]
    public ValueTask DisposeAsync() => (ServiceProvider as IAsyncDisposable)!.DisposeAsync();
}
