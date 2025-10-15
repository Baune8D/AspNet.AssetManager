// <copyright file="IManifestService.cs" company="Baune8D">
// Copyright (c) Baune8D. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Threading.Tasks;

namespace AspNet.AssetManager;

/// <summary>
/// Provides functionality to manage and access asset manifests.
/// </summary>
public interface IManifestService
{
    /// <summary>
    /// Retrieves the asset filename for a given frontend bundle from the manifest.
    /// </summary>
    /// <param name="bundle">The name of the frontend bundle.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// On completion, contains the asset filename or null if the bundle does not exist.
    /// </returns>
    Task<string?> GetFromManifestAsync(string bundle);

    /// <summary>
    /// Retrieves the content of a file, either from a file system or from a development server.
    /// </summary>
    /// <param name="file">The name of the file, including any query parameters.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// On completion, contains the content of the requested file as a string.
    /// </returns>
    Task<string> GetFileContentAsync(string file);
}
