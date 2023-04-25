using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;

namespace RavenDB.Configuration;

public class RavenDBConfigurationSource : IConfigurationSource
{
    /// <summary>
    /// The Raven document store to use then looking fot the configuration.
    /// if non is provided, the one in the registered with the <see cref="IServiceProvider"/> is used.
    /// </summary>
    public IDocumentStore? DocumentStore { get; set; }

    /// <summary>
    /// The Database to look for the configuration in. If non is provided the store default is used.
    /// </summary>
    public string? DatabaseName { get; set; }

    /// <summary>
    /// Defines how do find the relevant document(s)
    /// </summary>
    public SearchScope Scope { get; set; }

    /// <summary>
    /// Identifies what to find.
    /// </summary>
    public string? Identifier { get; set; }

    /// <summary>
    /// the prefix for the configurations found
    /// </summary>
    public string? ConfigurationPrefix { get; set; }

    /// <summary>
    /// how many configurations the prefix contains
    /// </summary>
    public int PrefixSegments { get; set; }

    /// <summary>
    /// Determines if an empty result is acceptable.
    /// </summary>
    public bool Optional { get; set; }

    /// <summary>
    /// Determines whether the source will be loaded if the underlying document changes.
    /// </summary>
    public bool ReloadOnChange { get; set; }

    /// <summary>
    /// Builds the <see cref="IConfigurationProvider"/> for this source.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
    /// <returns>A <see cref="IConfigurationProvider"/></returns>
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        builder.Properties.TryGetValue(ConfigurationsExtensions.ServiceProviderKey, out var value);
        if (value is not IServiceProvider serviceProvider)
            throw new InvalidOperationException("No IServiceProvider has been registered for the Configuration");

        DocumentStore ??= serviceProvider.GetService<IDocumentStore>();
        var logger = serviceProvider.GetRequiredService<ILogger<RavenDBConfigurationProvider>>();

        return new RavenDBConfigurationProvider(this, logger);
    }
}