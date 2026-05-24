// <copyright file="ManifestService.cs" company="Baune8D">
// Copyright (c) Baune8D. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AspNet.AssetManager;

internal sealed class ManifestService(IAssetConfiguration assetConfiguration, IFileSystem fileSystem)
    : IManifestService, IDisposable
{
    private readonly Dictionary<string, string> _fileContents = new();

    private JsonDocument? _manifest;

    public ManifestService(IAssetConfiguration assetConfiguration, IFileSystem fileSystem, IHttpClientFactory httpClientFactory)
        : this(assetConfiguration, fileSystem)
    {
        if (assetConfiguration.DevelopmentMode)
        {
            HttpClient = httpClientFactory.CreateClient();
        }
    }

    private HttpClient? HttpClient { get; }

    public void Dispose()
    {
        _manifest?.Dispose();
        HttpClient?.Dispose();
    }

    public async Task<string?> GetFromManifestAsync(string bundle)
    {
        ArgumentNullException.ThrowIfNull(bundle);

        var manifest = await LoadManifestAsync().ConfigureAwait(false);

        return assetConfiguration.ManifestType == ManifestType.Vite
            ? GetFromViteManifest(manifest, bundle)
            : GetFromKeyValueManifest(manifest, bundle);
    }

    public async Task<IReadOnlyList<string>> GetCssFromManifestAsync(string bundle)
    {
        ArgumentNullException.ThrowIfNull(bundle);

        var manifest = await LoadManifestAsync().ConfigureAwait(false);

        if (assetConfiguration.ManifestType == ManifestType.Vite)
        {
            return GetCssFromViteManifest(manifest, bundle);
        }

        var single = GetFromKeyValueManifest(manifest, bundle);
        return single != null ? [single] : [];
    }

    public async Task<string> GetFileContentAsync(string file)
    {
        ArgumentNullException.ThrowIfNull(file);

        if (!assetConfiguration.DevelopmentMode && _fileContents.TryGetValue(file, out var cached))
        {
            return cached;
        }

        var filename = file;
        var queryIndex = filename.IndexOf('?', StringComparison.Ordinal);
        if (queryIndex != -1)
        {
            filename = filename[..queryIndex];
        }

        var fullPath = $"{assetConfiguration.AssetsDirectoryPath}{filename}";

        var content = assetConfiguration.DevelopmentMode
            ? await FetchDevelopmentFileContentAsync(HttpClient, fullPath).ConfigureAwait(false)
            : await fileSystem.File.ReadAllTextAsync(fullPath).ConfigureAwait(false);

        if (!assetConfiguration.DevelopmentMode)
        {
            _fileContents.Add(file, content);
        }

        return content;
    }

    private async Task<JsonDocument> LoadManifestAsync()
    {
        if (_manifest != null)
        {
            return _manifest;
        }

        var json = assetConfiguration.DevelopmentMode
            ? await FetchDevelopmentManifestAsync(HttpClient, assetConfiguration.ManifestPath).ConfigureAwait(false)
            : await fileSystem.File.ReadAllTextAsync(assetConfiguration.ManifestPath).ConfigureAwait(false);

        var manifest = JsonDocument.Parse(json);

        if (!assetConfiguration.DevelopmentMode)
        {
            _manifest = manifest;
        }

        return manifest;
    }

    private static string? GetFromKeyValueManifest(JsonDocument manifest, string bundle)
    {
        try
        {
            return manifest.RootElement.GetProperty(bundle).GetString();
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }

    private string? GetFromViteManifest(JsonDocument manifest, string bundle)
    {
        var nameToFind = Path.GetFileNameWithoutExtension(bundle);

        foreach (var property in manifest.RootElement.EnumerateObject())
        {
            var entry = property.Value;

            if (!entry.TryGetProperty("name", out var nameProperty))
            {
                continue;
            }

            if (nameProperty.GetString() != nameToFind)
            {
                continue;
            }

            return assetConfiguration.DevelopmentMode
                ? entry.GetProperty("src").GetString()
                : entry.GetProperty("file").GetString();
        }

        return null;
    }

    private static List<string> GetCssFromViteManifest(JsonDocument manifest, string bundle)
    {
        var nameToFind = Path.GetFileNameWithoutExtension(bundle);

        foreach (var property in manifest.RootElement.EnumerateObject())
        {
            var entry = property.Value;

            if (!entry.TryGetProperty("name", out var nameProperty))
            {
                continue;
            }

            if (nameProperty.GetString() != nameToFind)
            {
                continue;
            }

            var result = new List<string>();
            var visited = new HashSet<string>(StringComparer.Ordinal) { property.Name };
            CollectCssFromViteEntry(manifest.RootElement, entry, result, visited);
            return result;
        }

        return [];
    }

    // Vite's manifest stores CSS on the chunk that imports it, not on the entry that depends
    // on the chunk. To get the full stylesheet set for an entry we walk its `imports` graph
    // depth-first (deepest dependency first) and append each chunk's own `css` entries last.
    // This matches the order Vite's dev runtime injects styles, so cascade behavior is the
    // same in dev and prod.
    private static void CollectCssFromViteEntry(
        JsonElement root,
        JsonElement entry,
        List<string> result,
        HashSet<string> visited)
    {
        if (entry.TryGetProperty("imports", out var imports) && imports.ValueKind == JsonValueKind.Array)
        {
            foreach (var importElement in imports.EnumerateArray())
            {
                var importKey = importElement.GetString();
                if (string.IsNullOrEmpty(importKey) || !visited.Add(importKey))
                {
                    continue;
                }

                if (root.TryGetProperty(importKey, out var importedEntry))
                {
                    CollectCssFromViteEntry(root, importedEntry, result, visited);
                }
            }
        }

        if (entry.TryGetProperty("css", out var css) && css.ValueKind == JsonValueKind.Array)
        {
            foreach (var cssElement in css.EnumerateArray())
            {
                var value = cssElement.GetString();
                if (!string.IsNullOrEmpty(value) && !result.Contains(value))
                {
                    result.Add(value);
                }
            }
        }
    }

    private static async Task<string> FetchDevelopmentManifestAsync(HttpClient? httpClient, string manifestPath)
    {
        if (httpClient == null)
        {
            throw new ArgumentNullException(nameof(httpClient), "HttpClient only available in development mode.");
        }

        try
        {
            return await httpClient.GetStringAsync(new Uri(manifestPath)).ConfigureAwait(false);
        }
        catch (HttpRequestException)
        {
            throw new DevServerException();
        }
    }

    private static async Task<string> FetchDevelopmentFileContentAsync(HttpClient? httpClient, string fullPath)
    {
        if (httpClient == null)
        {
            throw new ArgumentNullException(nameof(httpClient), "HttpClient only available in development mode.");
        }

        return await httpClient.GetStringAsync(new Uri(fullPath)).ConfigureAwait(false);
    }
}
