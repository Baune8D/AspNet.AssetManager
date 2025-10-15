// <copyright file="DevServerException.cs" company="Baune8D">
// Copyright (c) Baune8D. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Diagnostics.CodeAnalysis;

namespace AspNet.AssetManager;

/// <summary>
/// Represents an exception that occurs when the development server fails to start or is not running as expected.
/// </summary>
/// <remarks>
/// This exception is a specialized InvalidOperationException for development server scenarios.
/// </remarks>
[SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "Not needed")]
public class DevServerException() : InvalidOperationException("Development server not started!");
