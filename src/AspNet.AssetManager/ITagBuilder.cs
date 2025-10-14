// <copyright file="ITagBuilder.cs" company="Baune8D">
// Copyright (c) Baune8D. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Threading.Tasks;

namespace AspNet.AssetManager;

/// <summary>
/// Provides functionality to build HTML tags for including assets
/// such as scripts and styles in a web application.
/// </summary>
public interface ITagBuilder
{
    /// <summary>
    /// Builds an HTML script tag for including a JavaScript file in a web application.
    /// </summary>
    /// <param name="file">The path to the JavaScript file.</param>
    /// <param name="load">
    /// The script loading behavior, specifying how the script should be loaded (e.g., normal, async, defer).
    /// </param>
    /// <returns>A string containing the HTML script tag with the specified configuration.</returns>
    string BuildScriptTag(string file, ScriptLoad load);

    /// <summary>
    /// Builds an HTML link tag for including a CSS stylesheet in a web application.
    /// </summary>
    /// <param name="file">The path to the CSS stylesheet file.</param>
    /// <returns>A string containing the HTML link tag with the specified configuration.</returns>
    string BuildLinkTag(string file);

    /// <summary>
    /// Builds an HTML style tag for including a CSS file in a web application.
    /// </summary>
    /// <param name="file">The path to the CSS file.</param>
    /// <returns>
    /// A task representing the asynchronous operation,
    /// containing a string with the HTML style tag wrapping the content of the specified CSS file.
    /// </returns>
    Task<string> BuildStyleTagAsync(string file);
}
