// <copyright file="TagBuilder.cs" company="Baune8D">
// Copyright (c) Baune8D. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Abstractions;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AspNet.AssetManager;

/// <summary>
/// Service for including frontend assets in UI projects.
/// </summary>
public sealed class TagBuilder : ITagBuilder, IDisposable
{
    private readonly Dictionary<string, string> _inlineStyles = new();
    private readonly IAssetConfiguration _assetConfiguration;
    private readonly IFileSystem _fileSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagBuilder"/> class.
    /// </summary>
    /// <param name="assetConfiguration">Shared settings.</param>
    /// <param name="fileSystem">File system.</param>
    public TagBuilder(IAssetConfiguration assetConfiguration, IFileSystem fileSystem)
    {
        _assetConfiguration = assetConfiguration;
        _fileSystem = fileSystem;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TagBuilder"/> class.
    /// </summary>
    /// <param name="assetConfiguration">Shared settings.</param>
    /// <param name="fileSystem">File system.</param>
    /// <param name="httpClientFactory">HttpClient factory.</param>
    public TagBuilder(IAssetConfiguration assetConfiguration, IFileSystem fileSystem, IHttpClientFactory httpClientFactory)
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
        HttpClient?.Dispose();
    }

    /// <summary>
    /// Builds the script tag.
    /// </summary>
    /// <param name="file">The JS file to use in the tag.</param>
    /// <param name="load">Enum for modifying script load behavior.</param>
    /// <returns>A string containing the script tag.</returns>
    public string BuildScriptTag(string file, ScriptLoad load)
    {
        var attributes = new List<string>();

        if (_assetConfiguration.DevelopmentMode)
        {
            if (_assetConfiguration.ManifestType == ManifestType.Vite)
            {
                attributes.Add("type=\"module\"");
            }

            attributes.Add("crossorigin=\"anonymous\"");
        }

        switch (load)
        {
            case ScriptLoad.Normal:
                break;
            case ScriptLoad.Async:
                attributes.Add("async");
                break;
            case ScriptLoad.Defer:
                attributes.Add("defer");
                break;
            case ScriptLoad.AsyncDefer:
                attributes.Add("async defer");
                break;
            default:
                throw new InvalidEnumArgumentException(nameof(load), (int)load, typeof(ScriptLoad));
        }

        var space = attributes.Count != 0 ? " " : string.Empty;

        return $"<script src=\"{_assetConfiguration.AssetsWebPath}{file}\"{space}{string.Join(' ', attributes)}></script>";
    }

    /// <summary>
    /// Builds the link/style tag.
    /// </summary>
    /// <param name="file">The CSS file to use in the tag.</param>
    /// <returns>A string containing the link/style tag.</returns>
    public string BuildLinkTag(string file)
    {
        var crossOrigin = string.Empty;
        if (_assetConfiguration.DevelopmentMode)
        {
            crossOrigin = "crossorigin=\"anonymous\" ";
        }

        return $"<link href=\"{_assetConfiguration.AssetsWebPath}{file}\" rel=\"stylesheet\" {crossOrigin}/>";
    }

    /// <summary>
    /// Builds the link/style tag.
    /// </summary>
    /// <param name="file">The CSS file to use in the tag.</param>
    /// <returns>A string containing the link/style tag.</returns>
    public async Task<string> BuildStyleTagAsync(string file)
    {
        ArgumentNullException.ThrowIfNull(file);

        if (!_assetConfiguration.DevelopmentMode && _inlineStyles.TryGetValue(file, out var cached))
        {
            return cached;
        }

        var filename = file;
        var queryIndex = filename.IndexOf('?', StringComparison.Ordinal);
        if (queryIndex != -1)
        {
            filename = filename[..queryIndex];
        }

        var fullPath = $"{_assetConfiguration.AssetsDirectoryPath}{filename}";

        var style = _assetConfiguration.DevelopmentMode
            ? await FetchDevelopmentStyleAsync(HttpClient, fullPath).ConfigureAwait(false)
            : await _fileSystem.File.ReadAllTextAsync(fullPath).ConfigureAwait(false);

        var result = $"<style>{style}</style>";

        if (!_assetConfiguration.DevelopmentMode)
        {
            _inlineStyles.Add(file, result);
        }

        return result;
    }

    private static async Task<string> FetchDevelopmentStyleAsync(HttpClient? httpClient, string fullPath)
    {
        if (httpClient == null)
        {
            throw new ArgumentNullException(nameof(httpClient), "HttpClient only available in development mode.");
        }

        return await httpClient.GetStringAsync(new Uri(fullPath)).ConfigureAwait(false);
    }
}
