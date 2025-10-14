// <copyright file="ManifestType.cs" company="Baune8D">
// Copyright (c) Baune8D. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AspNet.AssetManager;

/// <summary>
/// Type of the frontend manifest.
/// </summary>
public enum ManifestType
{
    /// <summary>
    /// Manifests with a simple key value structure.
    /// </summary>
    KeyValue,

    /// <summary>
    /// Vite's built-in manifest.
    /// </summary>
    Vite,
}
