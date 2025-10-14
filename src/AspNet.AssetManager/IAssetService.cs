// <copyright file="IAssetService.cs" company="Morten Larsen">
// Copyright (c) Morten Larsen. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;

namespace AspNet.AssetManager;

/// <summary>
/// Service for including frontend assets in UI projects.
/// </summary>
public interface IAssetService
{
    /// <summary>
    /// Gets the full directory path for assets.
    /// </summary>
    string DirectoryPath { get; }

    /// <summary>
    /// Gets the web path for UI assets.
    /// </summary>
    string WebPath { get; }

    /// <summary>
    /// Gets the full file path.
    /// </summary>
    /// <param name="bundle">The bundle filename.</param>
    /// <param name="fileType">The bundle file type will append extension to bundle if specified.</param>
    /// <returns>The full file path.</returns>
    Task<string?> GetBundlePathAsync(string bundle, FileType? fileType = null);

    /// <summary>
    /// Returns the specified script asset.
    /// </summary>
    /// <param name="bundle">The name of the frontend bundle.</param>
    /// <param name="fallback">The name of the bundle to fall back to if the main bundle does not exist.</param>
    /// <returns>A string containing the script asset.</returns>
    Task<string?> GetScriptSrc(string bundle, string? fallback = null);

    /// <summary>
    /// Gets an HTML script tag for the specified asset.
    /// </summary>
    /// <param name="bundle">The name of the frontend bundle.</param>
    /// <param name="load">Enum for modifying script load behavior.</param>
    /// <returns>An HtmlString containing the HTML script tag.</returns>
    Task<HtmlString> GetScriptTagAsync(string bundle, ScriptLoad load = ScriptLoad.Normal);

    /// <summary>
    /// Gets an HTML script tag for the specified asset.
    /// </summary>
    /// <param name="bundle">The name of the frontend bundle.</param>
    /// <param name="fallback">The name of the bundle to fall back to if the main bundle does not exist.</param>
    /// <param name="load">Enum for modifying script load behavior.</param>
    /// <returns>An HtmlString containing the HTML script tag.</returns>
    Task<HtmlString> GetScriptTagAsync(string bundle, string? fallback, ScriptLoad load = ScriptLoad.Normal);

    /// <summary>
    /// Returns the specified link asset.
    /// </summary>
    /// <param name="bundle">The name of the frontend bundle.</param>
    /// <param name="fallback">The name of the bundle to fall back to if the main bundle does not exist.</param>
    /// <returns>A string containing the link asset.</returns>
    Task<string?> GetLinkHref(string bundle, string? fallback = null);

    /// <summary>
    /// Gets an HTML link tag for the specified asset.
    /// </summary>
    /// <param name="bundle">The name of the frontend bundle.</param>
    /// <param name="fallback">The name of the bundle to fall back to if the main bundle does not exist.</param>
    /// <returns>An HtmlString containing the HTML link tag.</returns>
    Task<HtmlString> GetLinkTagAsync(string bundle, string? fallback = null);

    /// <summary>
    /// Gets an HTML style tag for the specified asset.
    /// </summary>
    /// <param name="bundle">The name of the frontend bundle.</param>
    /// <param name="fallback">The name of the bundle to fall back to if the main bundle does not exist.</param>
    /// <returns>An HtmlString containing the HTML style tag.</returns>
    Task<HtmlString> GetStyleTagAsync(string bundle, string? fallback = null);
}
