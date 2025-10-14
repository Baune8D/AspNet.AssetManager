// <copyright file="ViewDataExtensions.cs" company="Baune8D">
// Copyright (c) Baune8D. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace AspNet.AssetManager;

/// <summary>
/// Provides extension methods for working with ViewData in ASP.NET Core.
/// </summary>
public static class ViewDataExtensions
{
    /// <summary>
    /// Retrieves the name of the frontend bundle from the specified ViewData.
    /// </summary>
    /// <param name="viewData">The ViewDataDictionary instance containing view data entries.</param>
    /// <returns> The name of the frontend bundle if found and valid; otherwise, null.</returns>
    public static string? GetBundleName(this ViewDataDictionary viewData)
    {
        ArgumentNullException.ThrowIfNull(viewData);

        if (!viewData.ContainsKey("Bundle") || viewData["Bundle"] is not string)
        {
            return null;
        }

        var bundle = (string)viewData["Bundle"]!;
        if (!bundle.StartsWith('/'))
        {
            return bundle;
        }

        // Use Razor Page logic to resolve bundle. E.g. /Some/Bundle = Some_Bundle
        var viewPaths = bundle
            .Split('/')
            .ToList();
        viewPaths.Remove(string.Empty);
        return string.Join("_", viewPaths);
    }
}
