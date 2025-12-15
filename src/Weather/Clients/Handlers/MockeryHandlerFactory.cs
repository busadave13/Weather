#nullable enable

using Microsoft.Extensions.Options;

namespace Weather.Clients.Handlers;

/// <summary>
/// Factory interface for creating <see cref="MockeryHandler"/> instances with specific service names.
/// </summary>
public interface IMockeryHandlerFactory
{
    /// <summary>
    /// Creates a new <see cref="MockeryHandler"/> configured for the specified service.
    /// </summary>
    /// <param name="serviceName">The name of the downstream service the handler will be configured for.</param>
    /// <returns>A new <see cref="MockeryHandler"/> instance.</returns>
    MockeryHandler Create(string serviceName);
}

/// <summary>
/// Factory for creating <see cref="MockeryHandler"/> instances with specific service names.
/// </summary>
/// <remarks>
/// This factory resolves dependencies from the service provider and creates handler instances
/// with the specified service name. Each handler instance is configured for a specific
/// downstream service, allowing for service-specific mock routing and logging.
/// </remarks>
public class MockeryHandlerFactory : IMockeryHandlerFactory
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="MockeryHandlerFactory"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider for resolving dependencies.</param>
    /// <exception cref="ArgumentNullException">Thrown when serviceProvider is null.</exception>
    public MockeryHandlerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc />
    public MockeryHandler Create(string serviceName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName, nameof(serviceName));

        return new MockeryHandler(
            serviceName,
            _serviceProvider.GetRequiredService<IHttpClientFactory>(),
            _serviceProvider.GetRequiredService<IHttpContextAccessor>(),
            _serviceProvider.GetRequiredService<ILogger<MockeryHandler>>(),
            _serviceProvider.GetRequiredService<IOptions<MockeryHandlerOptions>>());
    }
}
