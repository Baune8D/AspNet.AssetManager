// <copyright file="ManifestType.cs" company="Baune8D">
// Copyright (c) Baune8D. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AspNet.AssetManager;

/// <summary>
/// Represents the type of asset manifests used to manage application resources.
/// </summary>
public enum ManifestType
{
    /// <summary>
    /// Key-value pair-based manifest used to map asset names to their corresponding resource paths.
    /// </summary>
    KeyValue,

    /// <summary>
    /// Represents a Vite-based manifest for managing assets.
    /// Vite manifests typically provide detailed metadata for each asset,
    /// including file paths, names, dependencies, and associated CSS files.
    /// </summary>
    Vite,
}
