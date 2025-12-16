using Weather.Middleware;

namespace Weather.Extensions;

/// <summary>
/// Extension methods for configuring load shedding middleware.
/// </summary>
public static class LoadSheddingExtensions
{
    /// <summary>
    /// Adds load shedding services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration section containing load shedding options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLoadShedding(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<LoadSheddingOptions>(
            configuration.GetSection("LoadShedding"));

        return services;
    }

    /// <summary>
    /// Adds load shedding services to the service collection with custom configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure load shedding options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLoadShedding(
        this IServiceCollection services,
        Action<LoadSheddingOptions> configureOptions)
    {
        services.Configure(configureOptions);
        return services;
    }

    /// <summary>
    /// Adds load shedding middleware to the application pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseLoadShedding(this IApplicationBuilder app)
    {
        return app.UseMiddleware<LoadSheddingMiddleware>();
    }
}
