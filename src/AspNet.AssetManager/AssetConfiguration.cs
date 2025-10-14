// <copyright file="AssetConfiguration.cs" company="Baune8D">
// Copyright (c) Baune8D. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace AspNet.AssetManager;

internal class AssetConfiguration : IAssetConfiguration
{
    public AssetConfiguration(IOptions<AssetManagerOptions> options, IWebHostEnvironment webHostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(webHostEnvironment);

        DevelopmentMode = webHostEnvironment.IsDevelopment();

        ManifestType = options.Value.ManifestType;

        var publicPath = DevelopmentMode && ManifestType == ManifestType.Vite
            ? "/"
            : options.Value.PublicPath;

        AssetsDirectoryPath = DevelopmentMode
            ? (options.Value.InternalDevServer ?? options.Value.PublicDevServer) + publicPath
            : webHostEnvironment.WebRootPath + publicPath;

        AssetsWebPath = DevelopmentMode
            ? options.Value.PublicDevServer + publicPath
            : publicPath;

        ManifestPath = AssetsDirectoryPath + options.Value.ManifestFile;
    }

    public bool DevelopmentMode { get; }

    public string AssetsDirectoryPath { get; }

    public string AssetsWebPath { get; }

    public string ManifestPath { get; }

    public ManifestType ManifestType { get; }
}
