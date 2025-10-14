// <copyright file="ScriptLoad.cs" company="Baune8D">
// Copyright (c) Baune8D. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AspNet.AssetManager;

/// <summary>
/// Specifies the loading behavior for script tags.
/// </summary>
public enum ScriptLoad
{
    /// <summary>
    /// Specifies default script loading behavior with no additional attributes.
    /// </summary>
    Normal,

    /// <summary>
    /// Specifies that the script should be loaded asynchronously, allowing
    /// it to execute as soon as it is available, without blocking other
    /// elements on the page.
    /// </summary>
    Async,

    /// <summary>
    /// Specifies that the script should be loaded in a deferred manner, meaning it will
    /// be executed only after the HTML document has been parsed.
    /// </summary>
    Defer,

    /// <summary>
    /// Specifies that the script should be loaded with both "async" and "defer" attributes.
    /// This loading mode enables asynchronous downloading while ensuring the execution order
    /// of scripts matches the document order.
    /// </summary>
    AsyncDefer,
}
