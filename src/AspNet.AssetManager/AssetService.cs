// <copyright file="AssetService.cs" company="Baune8D">
// Copyright (c) Baune8D. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;

namespace AspNet.AssetManager;

internal sealed class AssetService : IAssetService
{
    private readonly IManifestService _manifestService;
    private readonly ITagBuilder _tagBuilder;

    public AssetService(IAssetConfiguration assetConfiguration, IManifestService manifestService, ITagBuilder tagBuilder)
    {
        ArgumentNullException.ThrowIfNull(assetConfiguration);

        DirectoryPath = assetConfiguration.AssetsDirectoryPath;
        WebPath = assetConfiguration.AssetsWebPath;

        _manifestService = manifestService;
        _tagBuilder = tagBuilder;
    }

    public string DirectoryPath { get; }

    public string WebPath { get; }

    public async Task<string?> GetBundlePathAsync(string bundle, FileType? fileType = null)
    {
        if (string.IsNullOrEmpty(bundle))
        {
            return null;
        }

        if (!Path.HasExtension(bundle) && !fileType.HasValue)
        {
            throw new InvalidOperationException("A file extension is needed either in bundle name or as file type parameter.");
        }

        if (fileType.HasValue)
        {
            if (Path.HasExtension(bundle))
            {
                throw new InvalidOperationException("If bundle name already has an extension then do not specify it again as file type parameter.");
            }

            bundle = fileType switch
            {
                FileType.CSS => $"{bundle}.css",
                FileType.JS => $"{bundle}.js",
                _ => throw new InvalidEnumArgumentException(nameof(fileType), (int)fileType, typeof(FileType)),
            };
        }

        string? file;
        if (bundle.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
        {
            var cssFiles = await _manifestService.GetCssFromManifestAsync(bundle).ConfigureAwait(false);
            file = cssFiles.Count > 0 ? cssFiles[0] : null;
        }
        else
        {
            file = await _manifestService.GetFromManifestAsync(bundle).ConfigureAwait(false);
        }

        return file != null
            ? $"{WebPath}{file}"
            : null;
    }

    public async Task<string?> GetScriptSrc(string bundle, string? fallback = null)
    {
        return await GetJsBundleName(bundle).ConfigureAwait(false)
               ?? await GetJsBundleName(fallback).ConfigureAwait(false);
    }

    public async Task<HtmlString> GetScriptTagAsync(string bundle, ScriptLoad load = ScriptLoad.Normal, bool? module = null)
    {
        return await GetScriptTagAsync(bundle, null, load, module).ConfigureAwait(false);
    }

    public async Task<HtmlString> GetScriptTagAsync(string bundle, string? fallback, ScriptLoad load = ScriptLoad.Normal, bool? module = null)
    {
        var file = await GetScriptSrc(bundle, fallback).ConfigureAwait(false);

        return file != null
            ? new HtmlString(_tagBuilder.BuildScriptTag(file, load, module))
            : HtmlString.Empty;
    }

    public async Task<IReadOnlyList<string>> GetLinkHrefs(string bundle, string? fallback = null)
    {
        var files = await GetCssBundleFiles(bundle).ConfigureAwait(false);
        return files.Count > 0
            ? files
            : await GetCssBundleFiles(fallback).ConfigureAwait(false);
    }

    public async Task<HtmlString> GetLinkTagAsync(string bundle, string? fallback = null)
    {
        var files = await GetLinkHrefs(bundle, fallback).ConfigureAwait(false);

        if (files.Count == 0)
        {
            return HtmlString.Empty;
        }

        var sb = new StringBuilder();
        for (var i = 0; i < files.Count; i++)
        {
            if (i > 0)
            {
                sb.Append('\n');
            }

            sb.Append(_tagBuilder.BuildLinkTag(files[i]));
        }

        return new HtmlString(sb.ToString());
    }

    public async Task<string?> GetStyleContent(string bundle, string? fallback = null)
    {
        var files = await GetLinkHrefs(bundle, fallback).ConfigureAwait(false);

        if (files.Count == 0)
        {
            return null;
        }

        var sb = new StringBuilder();
        foreach (var file in files)
        {
            var content = await _manifestService.GetFileContentAsync(file).ConfigureAwait(false);
            sb.Append(content);
        }

        return sb.ToString();
    }

    public async Task<HtmlString> GetStyleTagAsync(string bundle, string? fallback = null)
    {
        var content = await GetStyleContent(bundle, fallback).ConfigureAwait(false);

        return content != null
            ? new HtmlString(_tagBuilder.BuildStyleTag(content))
            : HtmlString.Empty;
    }

    private async Task<string?> GetJsBundleName(string? bundle)
    {
        if (string.IsNullOrEmpty(bundle))
        {
            return null;
        }

        bundle = TryFixBundleName(bundle, "js");
        return await _manifestService.GetFromManifestAsync(bundle).ConfigureAwait(false);
    }

    private async Task<IReadOnlyList<string>> GetCssBundleFiles(string? bundle)
    {
        if (string.IsNullOrEmpty(bundle))
        {
            return [];
        }

        bundle = TryFixBundleName(bundle, "css");
        return await _manifestService.GetCssFromManifestAsync(bundle).ConfigureAwait(false);
    }

    private static string TryFixBundleName(string bundle, string extension)
    {
        return !Path.HasExtension(bundle)
            ? $"{bundle}.{extension}"
            : bundle;
    }
}
