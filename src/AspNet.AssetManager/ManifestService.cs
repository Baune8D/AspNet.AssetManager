// <copyright file="ManifestService.cs" company="Baune8D">
// Copyright (c) Baune8D. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
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

        JsonDocument manifest;

        if (_manifest == null)
        {
            var json = assetConfiguration.DevelopmentMode
                ? await FetchDevelopmentManifestAsync(HttpClient, assetConfiguration.ManifestPath).ConfigureAwait(false)
                : await fileSystem.File.ReadAllTextAsync(assetConfiguration.ManifestPath).ConfigureAwait(false);

            manifest = JsonDocument.Parse(json);
            if (!assetConfiguration.DevelopmentMode)
            {
                _manifest = manifest;
            }
        }
        else
        {
            manifest = _manifest;
        }

        if (assetConfiguration.ManifestType == ManifestType.Vite)
        {
            return GetFromViteManifest(manifest, bundle);
        }

        return GetFromKeyValueManifest(manifest, bundle);
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
            var name = entry.GetProperty("name").GetString();

            if (name != nameToFind)
            {
                continue;
            }

            if (!bundle.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
            {
                return assetConfiguration.DevelopmentMode
                    ? entry.GetProperty("src").GetString()
                    : entry.GetProperty("file").GetString();
            }

            if (entry.TryGetProperty("css", out var css))
            {
                return css.EnumerateArray()
                    .Select(cssElement => cssElement.GetString() ?? string.Empty)
                    .FirstOrDefault(value => value.StartsWith(nameToFind, StringComparison.Ordinal));
            }

            return null;
        }

        return null;
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
