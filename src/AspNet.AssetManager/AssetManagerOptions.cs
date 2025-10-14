// <copyright file="AssetManagerOptions.cs" company="Baune8D">
// Copyright (c) Baune8D. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AspNet.AssetManager;

/// <summary>
/// Represents configurable options for the Asset Manager.
/// </summary>
public class AssetManagerOptions
{
    /// <summary>
    /// Gets the publicly accessible URL of the development server.
    /// Typically used to serve frontend assets during development.
    /// </summary>
    public string? PublicDevServer { get; init; }

    /// <summary>
    /// Gets the internal URL of the development server.
    /// This is used for server-side operations during development and may not be publicly accessible.
    /// </summary>
    public string? InternalDevServer { get; init; }

    /// <summary>
    /// Gets the public base path where assets are served from.
    /// This path is used to resolve asset URLs both during development and production.
    /// </summary>
    public string PublicPath { get; init; } = "/dist/";

    /// <summary>
    /// Gets the name of the file containing the frontend asset manifest.
    /// This is used to map asset identifiers to their respective file paths.
    /// </summary>
    public string ManifestFile { get; init; } = "assets-manifest.json";

    /// <summary>
    /// Gets the type of the frontend manifest.
    /// Specifies the structure or format expected in the manifest file (e.g., KeyValue or Vite).
    /// </summary>
    public ManifestType ManifestType { get; init; } = ManifestType.KeyValue;
}
