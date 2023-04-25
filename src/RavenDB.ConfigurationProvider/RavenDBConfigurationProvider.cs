using System.Configuration.Json;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Raven.Client.Documents;
using Raven.Client.Documents.Changes;
using Raven.Client.Documents.Commands;
using Raven.Client.Documents.Session;

namespace RavenDB.Configuration;

public sealed class RavenDBConfigurationProvider : ConfigurationProvider, IObserver<DocumentChange>
{
    private readonly ILogger<RavenDBConfigurationProvider> _logger;

    /// <summary>
    /// Initializes a new instance with the specified source.
    /// </summary>
    /// <param name="source">The source settings.</param>
    /// <param name="logger"></param>
    public RavenDBConfigurationProvider(RavenDBConfigurationSource source, ILogger<RavenDBConfigurationProvider> logger)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(source.DocumentStore);
 
        Source = source;
        _logger = logger;

        Source.ConfigurationPrefix = Source.ConfigurationPrefix?.Trim();
        Source.PrefixSegments = string.IsNullOrEmpty(Source.ConfigurationPrefix) ? 1 : 2;
            
            var keyDelimiterChar = ConfigurationPath.KeyDelimiter.AsSpan().GetPinnableReference();
            foreach (var c in Source.ConfigurationPrefix ?? "") 
                if (c == keyDelimiterChar) Source.PrefixSegments++;

        if (!Source.ReloadOnChange) return;
        switch (source.Scope)
        {
            case SearchScope.Document:
                source.DocumentStore.Changes(source.DatabaseName).ForDocument(source.Identifier).Subscribe(this);
                break;
            case SearchScope.Collection:
                source.DocumentStore.Changes(source.DatabaseName).ForDocumentsInCollection(source.Identifier).Subscribe(this);
                break;
            case SearchScope.Prefix:
                source.DocumentStore.Changes(source.DatabaseName).ForDocumentsStartingWith(source.Identifier).Subscribe(this);
                break;
            case SearchScope.All:
                source.DocumentStore.Changes(source.DatabaseName).ForAllDocuments().Subscribe(this);
                break;
            default:
                throw new Exception($"This is an unexpected value {Source.Scope}");
        }
    }

    /// <summary>
    /// The source settings for this provider.
    /// </summary>
    public RavenDBConfigurationSource Source { get; }

    /// <summary>
    /// Generates a string representing this provider name and relevant details.
    /// </summary>
    /// <returns> The configuration name. </returns>
    public override string ToString()
        => $"{GetType().Name} for '{Source.DocumentStore}'/{(string.IsNullOrWhiteSpace(Source.DatabaseName) ? Source.DocumentStore?.Database : Source.DatabaseName)} ({(Source.Optional ? "Optional" : "Required")})";

    public class DocId
    {
        public string Id { get; set; } = "";
    }

    /// <summary>
    /// Loads the contents of the file at <see cref="Path"/>.
    /// </summary>
    /// <exception cref="DirectoryNotFoundException">Optional is <c>false</c> on the source and a
    /// directory cannot be found at the specified Path.</exception>
    /// <exception cref="FileNotFoundException">Optional is <c>false</c> on the source and a
    /// file does not exist at specified Path.</exception>
    /// <exception cref="InvalidDataException">An exception was thrown by the concrete implementation of the
    /// <see cref="Load()"/> method. Use the source <see cref="FileConfigurationSource.OnLoadException"/> callback
    /// if you need more control over the exception.</exception>
    public override async void Load()
    {
        using var session = Source.DocumentStore!.OpenAsyncSession(new SessionOptions
        {
            Database = Source.DatabaseName,
            NoTracking = true,
            NoCaching = false,
        });

        switch (Source.Scope)
        {
            case SearchScope.Document:
                if (!await session.Advanced.ExistsAsync(Source.Identifier) && !Source.Optional)
                    await LoadErrorHandler(session);

                Data = await LoadConfigurationAsync(session, Source.Identifier!, Source.ConfigurationPrefix ?? "");

                _logger.LogInformation("Configuration loaded from {Scope}. {DocumentID}, {Prefix}", Source.Scope, Source.Identifier, Source.ConfigurationPrefix ?? "");
                break;
            case SearchScope.Collection:
            case SearchScope.Prefix:
            case SearchScope.All:
        Data = await LoadMultiple(session);
                break;
            default:
                throw new Exception($"This is an unexpected value {Source.Scope}");
        }
        
        OnReload();
    }

    private async Task<IGroupedDictionary<string, string, string?>> LoadMultiple(IAsyncDocumentSession session)
    {
        IGroupedDictionary<string, string, string?> data = new GroupedDictionary<string, string, string?>(GetGroup,
            StringComparer.InvariantCultureIgnoreCase,
            StringComparer.InvariantCultureIgnoreCase);
        var docList = new List<DocId>();

        switch (Source.Scope)
        {
            case SearchScope.Collection:
        _ = await session.Query<object>(collectionName: Source.Identifier).ToListAsync();
        docList = await session.Query<DocId>(collectionName: Source.Identifier).ToListAsync();
                break;
            case SearchScope.Prefix:
        _ = await session.Advanced.LoadStartingWithAsync<object>(Source.Identifier);
        docList.AddRange(await session.Advanced.LoadStartingWithAsync<DocId>(Source.Identifier));
                break;
            case SearchScope.All:
        _ = await session.Query<object>().ToListAsync();
        docList = await session.Query<DocId>().ToListAsync();
                break;
            case SearchScope.Document:
            default:
                throw new Exception($"This is an unexpected value {Source.Scope}");
        }

        if (docList.Count == 0 && !Source.Optional) await LoadErrorHandler(session);

        foreach (var docId in docList)
        {
            var prefix = Source.ConfigurationPrefix + ConfigurationPath.KeyDelimiter + docId.Id;

            var grpKey = data.CategorizeOnKey(prefix);
            if (grpKey == null)
            {
                _logger.LogError("Configuration Categorize Error when trying to load. {Prefix}", prefix);
                continue;
            }

            data.Groups[grpKey] = await LoadConfigurationAsync(session, docId.Id, prefix);
            _logger.LogInformation("Configuration loaded from {Scope}. {Identifier}, {DocumentId}, {Prefix}", Source.Scope, Source.Identifier, docId.Id, Source.ConfigurationPrefix ?? "");
        }

        data.RemoveEmptyGroups();
        return data;
    }

    [DoesNotReturn]
    private async Task LoadErrorHandler(IAsyncDocumentSession session)
    {
        _logger.LogCritical("Mandatory Configuration Not Found. {Source}, {Current DocumentStore}, {Current Connection}",
            Source,
            session.Advanced.DocumentStore,
            await session.Advanced.GetCurrentSessionNode());
        throw new FileNotFoundException("Mandatory Configuration Not Found in RavenDB");
    }

    private string? GetGroup(string key)
    {
        var pos = 0;
        for (var i = 0; i < Source.PrefixSegments; i++)
        {
            var r = key.IndexOf(ConfigurationPath.KeyDelimiter, pos, StringComparison.InvariantCultureIgnoreCase);
            if (r == -1) return null;
            if (r > 0) pos = r + 1;
        }

        return key[..pos];
    }

    private async Task<IDictionary<string, string?>> LoadConfigurationAsync(IAsyncDocumentSession session,
        string documentIdentifier,
        string configurationPrefix)
    {
        var command = new GetDocumentsCommand(documentIdentifier, null, false);
        await session.Advanced.RequestExecutor.ExecuteAsync(command, session.Advanced.Context); // TODO use ChangeVector
        var jObject = JObject.Parse(command.Result.Results[0].ToString() ?? "{}");
        jObject.Remove("@metadata");
        return JsonConfigurationParser.Parse(jObject.ToString(), configurationPrefix);
    }

    #region IObserver<DocumentChange>
    void IObserver<DocumentChange>.OnCompleted() { }

    void IObserver<DocumentChange>.OnError(Exception error)
    {
        _logger.LogError(error, "An error occurred while trying to reload.");
        // TODO Change Application State ???
    }

    async void IObserver<DocumentChange>.OnNext(DocumentChange value)
    {
        _logger.LogTrace("Configuration Update Event {event}", value);

        if (!value.Type.HasFlag(DocumentChangeTypes.Delete) && !value.Type.HasFlag(DocumentChangeTypes.Put)) return;

        using var session = Source.DocumentStore!.OpenAsyncSession(new SessionOptions
        {
            Database = Source.DatabaseName,
            NoTracking = true,
            NoCaching = true
        });
        switch (Source.Scope)
        {
            case SearchScope.Document:
                Data = await LoadConfigurationAsync(session, value.Id, Source.ConfigurationPrefix ?? "");
                _logger.LogInformation("Configuration reloaded. {Prefix}, {Update Event}", Source.ConfigurationPrefix ?? "", value);
                break;
            case SearchScope.Collection:
            case SearchScope.Prefix:
            case SearchScope.All:
                if (Data is not IGroupedDictionary<string, string, string?> data)
                {
                    _logger.LogError("Configuration Storage Error when trying to reload. {DataType}, {Update Event}", Data.GetType(), value);
                    // TODO Change Application State
                    return;
                }

                var prefix = Source.ConfigurationPrefix + ConfigurationPath.KeyDelimiter + value.Id;
                var grpKey = data.CategorizeOnKey(prefix);
                if (grpKey == null)
                {
                    _logger.LogError("Configuration Categorize Error when trying to reload. {Prefix}, {Update Event}", prefix, value);
                    // TODO Change Application State
                    return;
                }

                data.Groups[grpKey] = await LoadConfigurationAsync(session, value.Id, prefix);
                data.RemoveEmptyGroups();
                _logger.LogInformation("Configuration reloaded. {Prefix}, {Update Event}", prefix, value);
                break;
            default:
                throw new Exception($"This is an unexpected value {Source.Scope}");
        }

        OnReload();
    }
    #endregion
}