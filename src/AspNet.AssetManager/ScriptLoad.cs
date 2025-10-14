// <copyright file="ScriptLoad.cs" company="Baune8D">
// Copyright (c) Baune8D. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AspNet.AssetManager;

/// <summary>
/// Defines how to load the script.
/// </summary>
public enum ScriptLoad
{
    /// <summary>
    /// The normal way.
    /// </summary>
    Normal,

    /// <summary>
    /// With async on the script tag.
    /// </summary>
    Async,

    /// <summary>
    /// With defer on the script tag.
    /// </summary>
    Defer,

    /// <summary>
    /// With both async and defer on the script tag.
    /// </summary>
    AsyncDefer,
}
