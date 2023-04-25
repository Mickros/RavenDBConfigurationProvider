// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// https://source.dot.net/#Microsoft.Extensions.Configuration.Json/JsonConfigurationFileParser.cs

using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

// ReSharper disable once CheckNamespace
namespace System.Configuration.Json;

public sealed class JsonConfigurationParser
{
    /// <summary>Top-level JSON element must be an object. Instead, '{0}' was found.</summary>
    private const string ErrorInvalidTopLevelJsonElement = @"Top-level JSON element must be an object. Instead, '{0}' was found.";

    /// <summary>A duplicate key '{0}' was found.</summary>
    private const string ErrorKeyIsDuplicated = @"A duplicate key '{0}' was found.";

    /// <summary>Unsupported JSON token '{0}' was found.</summary>
    private const string ErrorUnsupportedJsonToken = @"Unsupported JSON token '{0}' was found.";

    private JsonConfigurationParser() { }
 
    private readonly Dictionary<string, string?> _data = new(StringComparer.OrdinalIgnoreCase);
    private readonly Stack<string> _paths = new();
 
    /// <summary>
    /// Parses the given <paramref name="input"/> to get a <see cref="Dictionary{TKey,TValue}"/> with the settings.
    /// </summary>
    /// <param name="input">The json to be parsed</param>
    /// <param name="prefix">A prefix for all the entries</param>
    /// <returns></returns>
    public static IDictionary<string, string?> Parse(Stream input, string prefix = "")
        => new JsonConfigurationParser().ParsePrivate(input, prefix);
 
    /// <summary>
    /// Parses the given <paramref name="input"/> to get a <see cref="Dictionary{TKey,TValue}"/> with the settings.
    /// </summary>
    /// <param name="input">The json to be parsed</param>
    /// <param name="prefix">A prefix for all the entries</param>
    /// <returns></returns>
    public static IDictionary<string, string?> Parse(string input, string prefix = "")
        => new JsonConfigurationParser().ParsePrivate(input, prefix);
 
    private Dictionary<string, string?> ParsePrivate(Stream input, string prefix = "")
    {
        using var reader = new StreamReader(input);
        return ParsePrivate(reader.ReadToEnd(), prefix);
    }
 
    private Dictionary<string, string?> ParsePrivate(string input, string prefix = "")
    {
        var jsonDocumentOptions = new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };
 
        using (var doc = JsonDocument.Parse(input, jsonDocumentOptions))
        {
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new FormatException(string.Format(ErrorInvalidTopLevelJsonElement, doc.RootElement.ValueKind));
            }
            EnterContext(prefix);
            VisitObjectElement(doc.RootElement);
            ExitContext();
        }
 
        return _data;
    }
 
    private void VisitObjectElement(JsonElement element)
    {
        var isEmpty = true;
 
        foreach (var property in element.EnumerateObject())
        {
            isEmpty = false;
            EnterContext(property.Name);
            VisitValue(property.Value);
            ExitContext();
        }
 
        SetNullIfElementIsEmpty(isEmpty);
    }
 
    private void VisitArrayElement(JsonElement element)
    {
        var index = 0;
 
        foreach (var arrayElement in element.EnumerateArray())
        {
            EnterContext(index.ToString());
            VisitValue(arrayElement);
            ExitContext();
            index++;
        }
 
        SetNullIfElementIsEmpty(isEmpty: index == 0);
    }
 
    private void SetNullIfElementIsEmpty(bool isEmpty)
    {
        if (isEmpty && _paths.Count > 0)
        {
            _data[_paths.Peek()] = null;
        }
    }
 
    private void VisitValue(JsonElement value)
    {
        Debug.Assert(_paths.Count > 0);
 
        switch (value.ValueKind)
        {
            case JsonValueKind.Object:
                VisitObjectElement(value);
                break;
 
            case JsonValueKind.Array:
                VisitArrayElement(value);
                break;
 
            case JsonValueKind.Number:
            case JsonValueKind.String:
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
                var key = _paths.Peek();
                if (_data.ContainsKey(key))
                {
                    throw new FormatException(string.Format(ErrorKeyIsDuplicated, key));
                }
                _data[key] = value.ToString();
                break;
 
            default:
                throw new FormatException(string.Format(ErrorUnsupportedJsonToken, value.ValueKind));
        }
    }
 
    private void EnterContext(string context) =>
        _paths.Push(_paths.Count > 0 ?
            _paths.Peek() + ConfigurationPath.KeyDelimiter + context :
            context);
 
    private void ExitContext() => _paths.Pop();
}