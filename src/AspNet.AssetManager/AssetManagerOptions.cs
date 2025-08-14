// <copyright file="AssetManagerOptions.cs" company="Morten Larsen">
// Copyright (c) Morten Larsen. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace AspNet.AssetManager;

/// <summary>
/// Asset manager options configured in appsettings.
/// </summary>
public class AssetManagerOptions
{
    /// <summary>
    /// Gets or sets the public url for the dev server. This needs to be accessible from the client.
    /// </summary>
    public string? PublicDevServer { get; set; }

    /// <summary>
    /// Gets or sets the internal url for the dev server. This needs to be accessible from the server.
    /// </summary>
    public string? InternalDevServer { get; set; }

    /// <summary>
    /// Gets or sets the public path.
    /// </summary>
    public string PublicPath { get; set; } = "/dist/";

    /// <summary>
    /// Gets or sets the name of the manifest file.
    /// </summary>
    public string ManifestFile { get; set; } = "assets-manifest.json";

    /// <summary>
    /// Gets or sets the type of manifest.
    /// </summary>
    public ManifestType ManifestType { get; set; } = ManifestType.KeyValue;
}
