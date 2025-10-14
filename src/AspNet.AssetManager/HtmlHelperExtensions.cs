// <copyright file="HtmlHelperExtensions.cs" company="Baune8D">
// Copyright (c) Baune8D. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AspNet.AssetManager;

/// <summary>
/// Contains extension methods for the <see cref="IHtmlHelper"/> interface to facilitate
/// working with view-related HTML utilities in ASP.NET Core applications.
/// </summary>
public static class HtmlHelperExtensions
{
    /// <summary>
    /// Retrieves the bundle name for the current view based on its path.
    /// </summary>
    /// <param name="html">The HTML helper used to access the view context information.</param>
    /// <returns>A string representing the bundle name derived from the view path.</returns>
    public static string GetBundleName(this IHtmlHelper html)
    {
        ArgumentNullException.ThrowIfNull(html);

        var path = html.ViewContext.View.Path;
        var viewPaths = path
            .Replace(".cshtml", string.Empty, StringComparison.Ordinal)
            .Split('/')
            .ToList();
        viewPaths.Remove(string.Empty);
        return string.Join("_", viewPaths);
    }
}
