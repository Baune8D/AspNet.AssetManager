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

/// <summary>
/// Service for including frontend assets in UI projects.
/// </summary>
public sealed class ManifestService : IManifestService, IDisposable
{
    private readonly IAssetConfiguration _assetConfiguration;
    private readonly IFileSystem _fileSystem;

    private JsonDocument? _manifest;

    /// <summary>
    /// Initializes a new instance of the <see cref="ManifestService"/> class.
    /// </summary>
    /// <param name="assetConfiguration">Shared settings.</param>
    /// <param name="fileSystem">File system.</param>
    public ManifestService(IAssetConfiguration assetConfiguration, IFileSystem fileSystem)
    {
        _assetConfiguration = assetConfiguration;
        _fileSystem = fileSystem;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ManifestService"/> class.
    /// </summary>
    /// <param name="assetConfiguration">Shared settings.</param>
    /// <param name="fileSystem">File system.</param>
    /// <param name="httpClientFactory">HttpClient factory.</param>
    public ManifestService(IAssetConfiguration assetConfiguration, IFileSystem fileSystem, IHttpClientFactory httpClientFactory)
        : this(assetConfiguration, fileSystem)
    {
        if (_assetConfiguration.DevelopmentMode)
        {
            HttpClient = httpClientFactory.CreateClient();
        }
    }

    private HttpClient? HttpClient { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        _manifest?.Dispose();
        HttpClient?.Dispose();
    }

    /// <summary>
    /// Gets the asset filename from the frontend manifest.
    /// </summary>
    /// <param name="bundle">The name of the frontend bundle.</param>
    /// <returns>The asset filename.</returns>
    public async Task<string?> GetFromManifestAsync(string bundle)
    {
        ArgumentNullException.ThrowIfNull(bundle);

        JsonDocument manifest;

        if (_manifest == null)
        {
            var json = _assetConfiguration.DevelopmentMode
                ? await FetchDevelopmentManifestAsync(HttpClient, _assetConfiguration.ManifestPath).ConfigureAwait(false)
                : await _fileSystem.File.ReadAllTextAsync(_assetConfiguration.ManifestPath).ConfigureAwait(false);

            manifest = JsonDocument.Parse(json);
            if (!_assetConfiguration.DevelopmentMode)
            {
                _manifest = manifest;
            }
        }
        else
        {
            manifest = _manifest;
        }

        if (_assetConfiguration.ManifestType == ManifestType.Vite)
        {
            return GetFromViteManifest(manifest, bundle);
        }

        return GetFromKeyValueManifest(manifest, bundle);
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
                return _assetConfiguration.DevelopmentMode
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
            throw new InvalidOperationException("Development server not started!");
        }
    }
}
