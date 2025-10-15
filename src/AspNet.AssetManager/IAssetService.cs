// <copyright file="IAssetService.cs" company="Baune8D">
// Copyright (c) Baune8D. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;

namespace AspNet.AssetManager;

/// <summary>
/// Provides services for managing assets, including generating file paths and retrieving scripts or styles.
/// </summary>
public interface IAssetService
{
    /// <summary>
    /// Gets the directory path where asset files are stored.
    /// </summary>
    // ReSharper disable once UnusedMemberInSuper.Global
    string DirectoryPath { get; }

    /// <summary>
    /// Gets the web-accessible base path where asset files are located.
    /// </summary>
    string WebPath { get; }

    /// <summary>
    /// Retrieves the full file path for the specified bundle, optionally appending the file extension
    /// based on the provided file type.
    /// </summary>
    /// <param name="bundle">The filename of the bundle. Must not be null or empty.</param>
    /// <param name="fileType">
    /// The type of the file (e.g., CSS or JS).
    /// If specified, it appends the corresponding extension to the bundle if not already present.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the full file path if found; otherwise, null.
    /// </returns>
    Task<string?> GetBundlePathAsync(string bundle, FileType? fileType = null);

    /// <summary>
    /// Retrieves the source URL of the specified script bundle,
    /// using a fallback bundle if the primary bundle is not available.
    /// </summary>
    /// <param name="bundle">The name of the primary frontend bundle to retrieve.</param>
    /// <param name="fallback">
    /// The name of the fallback bundle to use if the primary bundle does not exist.
    /// This parameter is optional and can be null.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the script source URL as a string if the bundle is found; otherwise, null.
    /// </returns>
    Task<string?> GetScriptSrc(string bundle, string? fallback = null);

    /// <summary>
    /// Generates an HTML script tag for the specified bundle with the specified script loading behavior.
    /// </summary>
    /// <param name="bundle">The name of the bundle for which the script tag is generated.</param>
    /// <param name="load">
    /// The loading behavior of the script tag (e.g., normal, async, defer). Defaults to normal.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains an HtmlString with the generated script tag.
    /// </returns>
    Task<HtmlString> GetScriptTagAsync(string bundle, ScriptLoad load = ScriptLoad.Normal);

    /// <summary>
    /// Generates an HTML script tag for the specified bundle, allowing customization of script loading behavior
    /// and providing an optional fallback if the primary bundle is unavailable.
    /// </summary>
    /// <param name="bundle">The name of the primary script bundle. Must not be null or empty.</param>
    /// <param name="fallback">
    /// The optional name of the fallback bundle to use when the primary bundle is unavailable.
    /// If null, no fallback script will be included.
    /// </param>
    /// <param name="load">
    /// Specifies the script loading behavior, such as normal, async, defer, or both async and defer.
    /// Defaults to the normal script loading behavior.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains an HTML string representing the generated script tag.
    /// </returns>
    Task<HtmlString> GetScriptTagAsync(string bundle, string? fallback, ScriptLoad load = ScriptLoad.Normal);

    /// <summary>
    /// Retrieves the hyperlink reference (href) for a specified CSS bundle,
    /// optionally providing a fallback if the primary bundle does not exist.
    /// </summary>
    /// <param name="bundle">The name of the primary CSS bundle to retrieve.</param>
    /// <param name="fallback">
    /// The name of the fallback bundle to use if the primary bundle is not found. This parameter is optional.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the href string
    /// for the specified bundle if found; otherwise, null.
    /// </returns>
    Task<string?> GetLinkHref(string bundle, string? fallback = null);

    /// <summary>
    /// Generates an HTML link tag referencing the specified frontend bundle.
    /// </summary>
    /// <param name="bundle">
    /// The name of the frontend bundle. This is the main asset file to reference.
    /// </param>
    /// <param name="fallback">
    /// The name of the bundle to fall back to if the main bundle does not exist. This parameter is optional.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains an HtmlString with the generated HTML link tag.
    /// If neither the main bundle nor the fallback is available, an empty HtmlString is returned.
    /// </returns>
    Task<HtmlString> GetLinkTagAsync(string bundle, string? fallback = null);

    /// <summary>
    /// Retrieves the CSS style content for the specified bundle,
    /// optionally using a fallback if the bundle is not found.
    /// </summary>
    /// <param name="bundle">The name of the bundle representing the asset. Must not be null or empty.</param>
    /// <param name="fallback">The optional name of a fallback bundle to use if the main bundle is unavailable.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the CSS style content if found; otherwise, null.
    /// </returns>
    Task<string?> GetStyleContent(string bundle, string? fallback = null);

    /// <summary>
    /// Asynchronously generates an HTML style tag for the specified bundle. If the main bundle does not exist,
    /// a fallback bundle can be used to provide the style tag instead.
    /// </summary>
    /// <param name="bundle">The name of the bundle representing the asset. Must not be null or empty.</param>
    /// <param name="fallback">The optional name of a fallback bundle to use if the main bundle is unavailable.</param>
    /// <returns>
    /// A task representing the asynchronous operation,
    /// resulting in an HtmlString containing the generated HTML style tag.
    /// If the bundle and fallback are unavailable, the result will be an empty HtmlString.
    /// </returns>
    Task<HtmlString> GetStyleTagAsync(string bundle, string? fallback = null);
}
