// <copyright file="AssetService.cs" company="Morten Larsen">
// Copyright (c) Morten Larsen. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;

namespace AspNet.AssetManager;

/// <summary>
/// Service for including frontend assets in UI projects.
/// </summary>
public sealed class AssetService : IAssetService
{
    private readonly IManifestService _manifestService;
    private readonly ITagBuilder _tagBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssetService"/> class.
    /// </summary>
    /// <param name="assetConfiguration">Shared settings.</param>
    /// <param name="manifestService">Asset manifest service.</param>
    /// <param name="tagBuilder">Asset builder service.</param>
    public AssetService(IAssetConfiguration assetConfiguration, IManifestService manifestService, ITagBuilder tagBuilder)
    {
        ArgumentNullException.ThrowIfNull(assetConfiguration);

        DirectoryPath = assetConfiguration.AssetsDirectoryPath;
        WebPath = assetConfiguration.AssetsWebPath;

        _manifestService = manifestService;
        _tagBuilder = tagBuilder;
    }

    /// <summary>
    /// Gets the full directory path for assets.
    /// </summary>
    public string DirectoryPath { get; }

    /// <summary>
    /// Gets the web path for UI assets.
    /// </summary>
    public string WebPath { get; }

    /// <summary>
    /// Gets the full file path.
    /// </summary>
    /// <param name="bundle">The bundle filename.</param>
    /// <param name="fileType">The bundle file type will append extension to bundle if specified.</param>
    /// <returns>The full file path.</returns>
    public async Task<string?> GetBundlePathAsync(string bundle, FileType? fileType = null)
    {
        if (string.IsNullOrEmpty(bundle))
        {
            return null;
        }

        if (!Path.HasExtension(bundle) && !fileType.HasValue)
        {
            throw new InvalidOperationException("A file extension is needed either in bundle name or as file type parameter.");
        }

        if (fileType.HasValue)
        {
            if (Path.HasExtension(bundle))
            {
                throw new InvalidOperationException("If bundle name already has an extension then do not specify it again as file type parameter.");
            }

            bundle = fileType switch
            {
                FileType.CSS => $"{bundle}.css",
                FileType.JS => $"{bundle}.js",
                _ => throw new InvalidEnumArgumentException(nameof(fileType), (int)fileType, typeof(FileType)),
            };
        }

        var file = await _manifestService.GetFromManifestAsync(bundle).ConfigureAwait(false);

        return file != null
            ? $"{WebPath}{file}"
            : null;
    }

    /// <summary>
    /// Returns the specified script asset.
    /// </summary>
    /// <param name="bundle">The name of the frontend bundle.</param>
    /// <param name="fallback">The name of the bundle to fall back to if the main bundle does not exist.</param>
    /// <returns>A string containing the script asset.</returns>
    public async Task<string?> GetScriptSrc(string bundle, string? fallback = null)
    {
        var file = await GetJsBundleName(bundle).ConfigureAwait(false);

        if (file == null)
        {
            file = await GetJsBundleName(fallback).ConfigureAwait(false);
        }

        return file;
    }

    /// <summary>
    /// Gets an HTML script tag for the specified asset.
    /// </summary>
    /// <param name="bundle">The name of the frontend bundle.</param>
    /// <param name="load">Enum for modifying script load behavior.</param>
    /// <returns>An HtmlString containing the HTML script tag.</returns>
    public async Task<HtmlString> GetScriptTagAsync(string bundle, ScriptLoad load = ScriptLoad.Normal)
    {
        return await GetScriptTagAsync(bundle, null, load).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets an HTML script tag for the specified asset.
    /// </summary>
    /// <param name="bundle">The name of the frontend bundle.</param>
    /// <param name="fallback">The name of the bundle to fall back to if the main bundle does not exist.</param>
    /// <param name="load">Enum for modifying script load behavior.</param>
    /// <returns>An HtmlString containing the HTML script tag.</returns>
    public async Task<HtmlString> GetScriptTagAsync(string bundle, string? fallback, ScriptLoad load = ScriptLoad.Normal)
    {
        var file = await GetScriptSrc(bundle, fallback).ConfigureAwait(false);

        return file != null
            ? new HtmlString(_tagBuilder.BuildScriptTag(file, load))
            : HtmlString.Empty;
    }

    /// <summary>
    /// Returns the specified link asset.
    /// </summary>
    /// <param name="bundle">The name of the frontend bundle.</param>
    /// <param name="fallback">The name of the bundle to fall back to if the main bundle does not exist.</param>
    /// <returns>A string containing the link asset.</returns>
    public async Task<string?> GetLinkHref(string bundle, string? fallback = null)
    {
        var file = await GetCssBundleName(bundle).ConfigureAwait(false);

        if (file == null)
        {
            file = await GetCssBundleName(fallback).ConfigureAwait(false);
        }

        return file;
    }

    /// <summary>
    /// Gets an HTML link tag for the specified asset.
    /// </summary>
    /// <param name="bundle">The name of the frontend bundle.</param>
    /// <param name="fallback">The name of the bundle to fall back to if the main bundle does not exist.</param>
    /// <returns>An HtmlString containing the HTML link tag.</returns>
    public async Task<HtmlString> GetLinkTagAsync(string bundle, string? fallback = null)
    {
        var file = await GetLinkHref(bundle, fallback).ConfigureAwait(false);

        return file != null
            ? new HtmlString(_tagBuilder.BuildLinkTag(file))
            : HtmlString.Empty;
    }

    /// <summary>
    /// Gets an HTML style tag for the specified asset.
    /// </summary>
    /// <param name="bundle">The name of the frontend bundle.</param>
    /// <param name="fallback">The name of the bundle to fall back to if the main bundle does not exist.</param>
    /// <returns>An HtmlString containing the HTML style tag.</returns>
    public async Task<HtmlString> GetStyleTagAsync(string bundle, string? fallback = null)
    {
        var file = await GetLinkHref(bundle, fallback).ConfigureAwait(false);

        return file != null
            ? new HtmlString(await _tagBuilder.BuildStyleTagAsync(file).ConfigureAwait(false))
            : HtmlString.Empty;
    }

    private async Task<string?> GetJsBundleName(string? bundle)
    {
        if (string.IsNullOrEmpty(bundle))
        {
            return null;
        }

        bundle = TryFixBundleName(bundle, "js");
        return await _manifestService.GetFromManifestAsync(bundle).ConfigureAwait(false);
    }

    private async Task<string?> GetCssBundleName(string? bundle)
    {
        if (string.IsNullOrEmpty(bundle))
        {
            return null;
        }

        bundle = TryFixBundleName(bundle, "css");
        return await _manifestService.GetFromManifestAsync(bundle).ConfigureAwait(false);
    }

    private static string TryFixBundleName(string bundle, string extension)
    {
        return !Path.HasExtension(bundle)
            ? $"{bundle}.{extension}"
            : bundle;
    }
}
