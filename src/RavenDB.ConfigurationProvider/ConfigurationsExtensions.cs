using Microsoft.Extensions.Configuration;
using Raven.Client.Documents;
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace RavenDB.Configuration;

/// <summary>
/// Extension methods for adding <see cref="RavenDBConfigurationProvider"/>.
/// </summary>
public static class ConfigurationsExtensions
{
    internal const string ErrorInvalidDocumentId = "The provided identifier is not valid.";
    internal const string ServiceProviderKey = "ServiceProvider";

    /// <summary>
    /// Add the <see cref="IServiceProvider"/> to the <see cref="IConfigurationBuilder.Properties"/> so it can be used to retrieve the needed services in the configuration Provider.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="serviceProvider">The ServiceProvider to be added</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddServiceProviderProperty(this IConfigurationBuilder builder,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Properties.TryAdd(ServiceProviderKey, serviceProvider);
        return builder;
    }

    #region AddRavenDBDocument
    /// <summary>
    /// Adds the RavenDB configuration provider with connection to <paramref name="documentStore"/> in the default database
    /// as single document identified by <paramref name="documentId"/> to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="documentStore">The Raven Document store to use. if <b>null</b> then one is retrieved from <see cref="IServiceProvider"/> </param>
    /// <param name="documentId">Id of the document to be used</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddRavenDBDocument(this IConfigurationBuilder builder,
        IDocumentStore documentStore,
        string documentId) => AddRavenDBDocument(builder, documentStore, documentId, "", "", false, false);

    /// <summary>
    /// Adds the RavenDB configuration provider with connection to <paramref name="documentStore"/> in the <paramref name="databaseName"/>
    /// as single document identified by <paramref name="documentId"/> to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="documentStore">The Raven Document store to use. if <b>null</b> then one is retrieved from <see cref="IServiceProvider"/> </param>
    /// <param name="documentId">Id of the document to be used</param>
    /// <param name="configurationPrefix">An optional prefix for the configuration settings</param>
    /// <param name="databaseName">Name on the database used, The Default is used if non is provided</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddRavenDBDocument(this IConfigurationBuilder builder,
        IDocumentStore documentStore,
        string documentId,
        string configurationPrefix,
        string databaseName) => AddRavenDBDocument(builder, documentStore, documentId, configurationPrefix, databaseName, false, false);

    /// <summary>
    /// Adds the RavenDB configuration provider with connection to <paramref name="documentStore"/> in the default database
    /// as single document identified by <paramref name="documentId"/> to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="documentStore">The Raven Document store to use. if <b>null</b> then one is retrieved from <see cref="IServiceProvider"/> </param>
    /// <param name="documentId">Id of the document to be used</param>
    /// <param name="optional">Whether this source is optional.</param>
    /// <param name="reloadOnChange">Whether the configuration should be reloaded if the source changes.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddRavenDBDocument(this IConfigurationBuilder builder,
        IDocumentStore documentStore,
        string documentId,
        bool optional,
        bool reloadOnChange) => AddRavenDBDocument(builder, documentStore, documentId, "", "", optional, reloadOnChange);

    /// <summary>
    /// Adds the RavenDB configuration provider with connection to <paramref name="documentStore"/> in the <paramref name="databaseName"/>
    /// as single document identified by <paramref name="documentId"/> to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="documentStore">The Raven Document store to use. if <b>null</b> then one is retrieved from <see cref="IServiceProvider"/> </param>
    /// <param name="documentId">Id of the document to be used</param>
    /// <param name="configurationPrefix">An optional prefix for the configuration settings</param>
    /// <param name="databaseName">Name on the database used, The Default is used if non is provided</param>
    /// <param name="optional">Whether this source is optional.</param>
    /// <param name="reloadOnChange">Whether the configuration should be reloaded if the source changes.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddRavenDBDocument(this IConfigurationBuilder builder,
        IDocumentStore documentStore,
        string documentId,
        string configurationPrefix,
        string databaseName,
        bool optional,
        bool reloadOnChange)
    {
        if (string.IsNullOrWhiteSpace(documentId))
            throw new ArgumentException(ErrorInvalidDocumentId, nameof(documentId));

        return builder.AddRavenDB(s =>
        {
            s.DocumentStore = documentStore;
            s.DatabaseName = databaseName;
            s.Scope = SearchScope.Document;
            s.Identifier = documentId;
            s.ConfigurationPrefix = configurationPrefix;
            s.Optional = optional;
            s.ReloadOnChange = reloadOnChange;
        });
    }
    #endregion

    #region AddRavenDBCollection

    /// <summary>
    /// Adds the RavenDB configuration provider with connection to <paramref name="documentStore"/> in the default database
    /// as a collection identified by <paramref name="collectionName"/> to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="documentStore">The Raven Document store to use. if <b>null</b> then one is retrieved from <see cref="IServiceProvider"/> </param>
    /// <param name="collectionName">Name of the RavenDB collection to be used</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddRavenDBCollection(this IConfigurationBuilder builder,
        IDocumentStore documentStore,
        string collectionName) => AddRavenDBCollection(builder, documentStore, collectionName, "", "", false, false);

    /// <summary>
    /// Adds the RavenDB configuration provider with connection to <paramref name="documentStore"/> in the <paramref name="databaseName"/>
    /// as a collection identified by <paramref name="collectionName"/> to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="documentStore">The Raven Document store to use. if <b>null</b> then one is retrieved from <see cref="IServiceProvider"/> </param>
    /// <param name="collectionName">Name of the RavenDB collection to be used</param>
    /// <param name="configurationPrefix">An optional prefix for the configuration settings. The document id is appended to this</param>
    /// <param name="databaseName">Name on the database used, The Default is used if non is provided</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddRavenDBCollection(this IConfigurationBuilder builder,
        IDocumentStore documentStore,
        string collectionName,
        string configurationPrefix,
        string databaseName) => AddRavenDBCollection(builder, documentStore, collectionName, configurationPrefix, databaseName, false, false);

    /// <summary>
    /// Adds the RavenDB configuration provider with connection to <paramref name="documentStore"/> in the default database
    /// as a collection identified by <paramref name="collectionName"/> to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="documentStore">The Raven Document store to use. if <b>null</b> then one is retrieved from <see cref="IServiceProvider"/> </param>
    /// <param name="collectionName">Name of the RavenDB collection to be used</param>
    /// <param name="optional">Whether this source is optional.</param>
    /// <param name="reloadOnChange">Whether the configuration should be reloaded if the source changes.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddRavenDBCollection(this IConfigurationBuilder builder,
        IDocumentStore documentStore,
        string collectionName,
        bool optional,
        bool reloadOnChange) => AddRavenDBCollection(builder, documentStore, collectionName, "", "", optional, reloadOnChange);

    /// <summary>
    /// Adds the RavenDB configuration provider with connection to <paramref name="documentStore"/> in the <paramref name="databaseName"/>
    /// as a collection identified by <paramref name="collectionName"/> to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="documentStore">The Raven Document store to use. if <b>null</b> then one is retrieved from <see cref="IServiceProvider"/> </param>
    /// <param name="collectionName">Name of the RavenDB collection to be used</param>
    /// <param name="configurationPrefix">An optional prefix for the configuration settings. The document id is appended to this</param>
    /// <param name="databaseName">Name on the database used, The Default is used if non is provided</param>
    /// <param name="optional">Whether this source is optional.</param>
    /// <param name="reloadOnChange">Whether the configuration should be reloaded if the source changes.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddRavenDBCollection(this IConfigurationBuilder builder,
        IDocumentStore documentStore,
        string collectionName,
        string configurationPrefix,
        string databaseName,
        bool optional,
        bool reloadOnChange)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
            throw new ArgumentException(ErrorInvalidDocumentId, nameof(collectionName));

        return builder.AddRavenDB(s =>
        {
            s.DocumentStore = documentStore;
            s.DatabaseName = databaseName;
            s.Scope = SearchScope.Collection;
            s.Identifier = collectionName;
            s.ConfigurationPrefix = configurationPrefix;
            s.Optional = optional;
            s.ReloadOnChange = reloadOnChange;
        });
    }
    #endregion

    #region AddRavenDBDocumentsWithPrefix

    /// <summary>
    /// Adds the RavenDB configuration provider with connection to <paramref name="documentStore"/> in the default database
    /// as list of documents all starting with <paramref name="documentPrefix"/> to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="documentStore">The Raven Document store to use. if <b>null</b> then one is retrieved from <see cref="IServiceProvider"/> </param>
    /// <param name="documentPrefix">The prefix to use when looking for documents.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddRavenDBDocumentsWithPrefix(this IConfigurationBuilder builder,
        IDocumentStore documentStore,
        string documentPrefix) =>
        AddRavenDBDocumentsWithPrefix(builder, documentStore, documentPrefix, "", "", false, false);

    /// <summary>
    /// Adds the RavenDB configuration provider with connection to <paramref name="documentStore"/> in the <paramref name="databaseName"/>
    /// as list of documents all starting with <paramref name="documentPrefix"/> to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="documentStore">The Raven Document store to use. if <b>null</b> then one is retrieved from <see cref="IServiceProvider"/> </param>
    /// <param name="documentPrefix">The prefix to use when looking for documents.</param>
    /// <param name="configurationPrefix">An optional prefix for the configuration settings. The document id is appended to this</param>
    /// <param name="databaseName">Name on the database used, The Default is used if non is provided</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddRavenDBDocumentsWithPrefix(this IConfigurationBuilder builder,
        IDocumentStore documentStore,
        string documentPrefix,
        string configurationPrefix,
        string databaseName) =>
        AddRavenDBDocumentsWithPrefix(builder, documentStore, documentPrefix, configurationPrefix, databaseName, false, false);

    /// <summary>
    /// Adds the RavenDB configuration provider with connection to <paramref name="documentStore"/> in the default database
    /// as list of documents all starting with <paramref name="documentPrefix"/> to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="documentStore">The Raven Document store to use. if <b>null</b> then one is retrieved from <see cref="IServiceProvider"/> </param>
    /// <param name="documentPrefix">The prefix to use when looking for documents.</param>
    /// <param name="optional">Whether this source is optional.</param>
    /// <param name="reloadOnChange">Whether the configuration should be reloaded if the source changes.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddRavenDBDocumentsWithPrefix(this IConfigurationBuilder builder,
        IDocumentStore documentStore,
        string documentPrefix,
        bool optional,
        bool reloadOnChange) =>
        AddRavenDBDocumentsWithPrefix(builder, documentStore, documentPrefix, "", "", optional, reloadOnChange);

    /// <summary>
    /// Adds the RavenDB configuration provider with connection to <paramref name="documentStore"/> in the <paramref name="databaseName"/>
    /// as list of documents all starting with <paramref name="documentPrefix"/> to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="documentStore">The Raven Document store to use. if <b>null</b> then one is retrieved from <see cref="IServiceProvider"/> </param>
    /// <param name="documentPrefix">The prefix to use when looking for documents.</param>
    /// <param name="configurationPrefix">An optional prefix for the configuration settings. The document id is appended to this</param>
    /// <param name="databaseName">Name on the database used, The Default is used if non is provided</param>
    /// <param name="optional">Whether this source is optional.</param>
    /// <param name="reloadOnChange">Whether the configuration should be reloaded if the source changes.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddRavenDBDocumentsWithPrefix(this IConfigurationBuilder builder,
        IDocumentStore documentStore,
        string documentPrefix,
        string configurationPrefix,
        string databaseName,
        bool optional,
        bool reloadOnChange)
    {
        if (string.IsNullOrWhiteSpace(documentPrefix))
            throw new ArgumentException(ErrorInvalidDocumentId, nameof(documentPrefix));

        return builder.AddRavenDB(s =>
        {
            s.DocumentStore = documentStore;
            s.DatabaseName = databaseName;
            s.Scope = SearchScope.Prefix;
            s.Identifier = documentPrefix;
            s.ConfigurationPrefix = configurationPrefix;
            s.Optional = optional;
            s.ReloadOnChange = reloadOnChange;
        });
    }
    #endregion

    #region AddRavenDBAllDocuments

    /// <summary>
    /// Adds the RavenDB configuration provider with connection to <paramref name="documentStore"/> in the default database
    /// as list of all documents to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="documentStore">The Raven Document store to use. if <b>null</b> then one is retrieved from <see cref="IServiceProvider"/> </param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddRavenDBAllDocuments(this IConfigurationBuilder builder,
        IDocumentStore documentStore) => AddRavenDBAllDocuments(builder, documentStore, "", "", false, false);

    /// <summary>
    /// Adds the RavenDB configuration provider with connection to <paramref name="documentStore"/> in the <paramref name="databaseName"/>
    /// as list of all documents to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="documentStore">The Raven Document store to use. if <b>null</b> then one is retrieved from <see cref="IServiceProvider"/> </param>
    /// <param name="configurationPrefix">An optional prefix for the configuration settings. The document id is appended to this</param>
    /// <param name="databaseName">Name on the database used, The Default is used if non is provided</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddRavenDBAllDocuments(this IConfigurationBuilder builder,
        IDocumentStore documentStore,
        string configurationPrefix,
        string databaseName) => AddRavenDBAllDocuments(builder, documentStore, configurationPrefix, databaseName, false, false);

    /// <summary>
    /// Adds the RavenDB configuration provider with connection to <paramref name="documentStore"/> in the default database
    /// as list of all documents to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="documentStore">The Raven Document store to use. if <b>null</b> then one is retrieved from <see cref="IServiceProvider"/> </param>
    /// <param name="optional">Whether this source is optional.</param>
    /// <param name="reloadOnChange">Whether the configuration should be reloaded if the source changes.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddRavenDBAllDocuments(this IConfigurationBuilder builder,
        IDocumentStore documentStore,
        bool optional,
        bool reloadOnChange) => AddRavenDBAllDocuments(builder, documentStore, "", "", optional, reloadOnChange);

    /// <summary>
    /// Adds the RavenDB configuration provider with connection to <paramref name="documentStore"/> in the <paramref name="databaseName"/>
    /// as list of all documents to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="documentStore">The Raven Document store to use. if <b>null</b> then one is retrieved from <see cref="IServiceProvider"/> </param>
    /// <param name="configurationPrefix">An optional prefix for the configuration settings. The document id is appended to this</param>
    /// <param name="databaseName">Name on the database used, The Default is used if non is provided</param>
    /// <param name="optional">Whether this source is optional.</param>
    /// <param name="reloadOnChange">Whether the configuration should be reloaded if the source changes.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddRavenDBAllDocuments(this IConfigurationBuilder builder,
        IDocumentStore documentStore,
        string configurationPrefix,
        string databaseName,
        bool optional,
        bool reloadOnChange)
    {
        return builder.AddRavenDB(s =>
        {
            s.DocumentStore = documentStore;
            s.DatabaseName = databaseName;
            s.Scope = SearchScope.All;
            s.Identifier = null;
            s.ConfigurationPrefix = configurationPrefix;
            s.Optional = optional;
            s.ReloadOnChange = reloadOnChange;
        });
    }
    #endregion

    /// <summary>
    /// Adds the RavenDB configuration provider with the <paramref name="configureSource"/> to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="configureSource">Configures the source.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddRavenDB(this IConfigurationBuilder builder,
        Action<RavenDBConfigurationSource> configureSource)
    {
        ArgumentNullException.ThrowIfNull(builder);
        
        return builder.Add(configureSource);
    }
}