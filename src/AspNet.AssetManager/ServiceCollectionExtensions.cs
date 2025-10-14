// <copyright file="ServiceCollectionExtensions.cs" company="Baune8D">
// Copyright (c) Baune8D. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.IO.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace AspNet.AssetManager;

/// <summary>
/// Provides extension methods for IServiceCollection to register services required for asset management.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Asset Manager services to the specified service collection.
    /// </summary>
    /// <param name="serviceCollection">
    /// The service collection to which the Asset Manager services will be added.
    /// </param>
    /// <param name="configuration">The application configuration containing Asset Manager settings.</param>
    /// <param name="webHostEnvironment">The hosting environment of the current application.</param>
    /// <returns>The modified service collection.</returns>
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IServiceCollection AddAssetManager(
        this IServiceCollection serviceCollection,
        IConfiguration configuration,
        IWebHostEnvironment webHostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        if (webHostEnvironment.IsDevelopment())
        {
            serviceCollection.AddHttpClient();
        }

        serviceCollection.Configure<AssetManagerOptions>(configuration.GetSection("AssetManager"));
        serviceCollection.TryAddTransient<IFileSystem, FileSystem>();
        serviceCollection.AddSingleton<IAssetConfiguration, AssetConfiguration>();
        serviceCollection.AddSingleton<ITagBuilder, TagBuilder>();
        serviceCollection.AddSingleton<IManifestService, ManifestService>();
        serviceCollection.AddSingleton<IAssetService, AssetService>();

        return serviceCollection;
    }
}
