// <copyright file="IAssetConfiguration.cs" company="Baune8D">
// Copyright (c) Baune8D. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AspNet.AssetManager;

/// <summary>
/// Defines configuration settings for the asset management system.
/// </summary>
public interface IAssetConfiguration
{
    /// <summary>
    /// Gets a value indicating whether the application is running in development mode.
    /// </summary>
    bool DevelopmentMode { get; }

    /// <summary>
    /// Gets the file system path to the directory where assets are stored.
    /// </summary>
    string AssetsDirectoryPath { get; }

    /// <summary>
    /// Gets the base web path where asset files are served.
    /// </summary>
    string AssetsWebPath { get; }

    /// <summary>
    /// Gets the path to the manifest file used for asset management.
    /// </summary>
    string ManifestPath { get; }

    /// <summary>
    /// Gets the type of manifest used for managing asset references.
    /// </summary>
    public ManifestType ManifestType { get; }
}
