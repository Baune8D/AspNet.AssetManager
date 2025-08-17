// <copyright file="SharedSettings.cs" company="Morten Larsen">
// Copyright (c) Morten Larsen. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace AspNet.AssetManager;

/// <summary>
/// A collection of shared settings for other services.
/// </summary>
public class SharedSettings : ISharedSettings
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SharedSettings"/> class.
    /// </summary>
    /// <param name="options">Asset manager options.</param>
    /// <param name="webHostEnvironment">Web host environment.</param>
    public SharedSettings(IOptions<AssetManagerOptions> options, IWebHostEnvironment webHostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(webHostEnvironment);

        DevelopmentMode = webHostEnvironment.IsDevelopment();

        ManifestType = options.Value.ManifestType;

        var publicPath = DevelopmentMode && ManifestType == ManifestType.Vite
            ? string.Empty
            : options.Value.PublicPath;

        AssetsDirectoryPath = DevelopmentMode
            ? (options.Value.InternalDevServer ?? options.Value.PublicDevServer) + publicPath
            : webHostEnvironment.WebRootPath + publicPath;

        AssetsWebPath = DevelopmentMode
            ? options.Value.PublicDevServer + publicPath
            : publicPath;

        ManifestPath = AssetsDirectoryPath + options.Value.ManifestFile;
    }

    /// <summary>
    /// Gets a value indicating whether the development mode is active.
    /// </summary>
    public bool DevelopmentMode { get; }

    /// <summary>
    /// Gets the full directory path for assets.
    /// </summary>
    public string AssetsDirectoryPath { get; }

    /// <summary>
    /// Gets the web path for UI assets.
    /// </summary>
    public string AssetsWebPath { get; }

    /// <summary>
    /// Gets the manifest file path.
    /// </summary>
    public string ManifestPath { get; }

    /// <summary>
    /// Gets the type of manifest.
    /// </summary>
    public ManifestType ManifestType { get; }
}
