// <copyright file="IManifestService.cs" company="Baune8D">
// Copyright (c) Baune8D. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Threading.Tasks;

namespace AspNet.AssetManager;

/// <summary>
/// Service for including frontend assets in UI projects.
/// </summary>
public interface IManifestService
{
    /// <summary>
    /// Gets the asset filename from the frontend manifest.
    /// </summary>
    /// <param name="bundle">The name of the frontend bundle.</param>
    /// <returns>The asset filename.</returns>
    Task<string?> GetFromManifestAsync(string bundle);
}
