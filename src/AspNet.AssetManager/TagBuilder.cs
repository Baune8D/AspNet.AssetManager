// <copyright file="TagBuilder.cs" company="Baune8D">
// Copyright (c) Baune8D. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.ComponentModel;

namespace AspNet.AssetManager;

internal sealed class TagBuilder(IAssetConfiguration assetConfiguration) : ITagBuilder
{
    public string BuildScriptTag(string file, ScriptLoad load)
    {
        var attributes = new List<string>();

        if (assetConfiguration.DevelopmentMode)
        {
            if (assetConfiguration.ManifestType == ManifestType.Vite)
            {
                attributes.Add("type=\"module\"");
            }

            attributes.Add("crossorigin=\"anonymous\"");
        }

        switch (load)
        {
            case ScriptLoad.Normal:
                break;
            case ScriptLoad.Async:
                attributes.Add("async");
                break;
            case ScriptLoad.Defer:
                attributes.Add("defer");
                break;
            case ScriptLoad.AsyncDefer:
                attributes.Add("async defer");
                break;
            default:
                throw new InvalidEnumArgumentException(nameof(load), (int)load, typeof(ScriptLoad));
        }

        var space = attributes.Count != 0 ? " " : string.Empty;

        return $"<script src=\"{assetConfiguration.AssetsWebPath}{file}\"{space}{string.Join(' ', attributes)}></script>";
    }

    public string BuildLinkTag(string file)
    {
        var crossOrigin = string.Empty;
        if (assetConfiguration.DevelopmentMode)
        {
            crossOrigin = "crossorigin=\"anonymous\" ";
        }

        return $"<link href=\"{assetConfiguration.AssetsWebPath}{file}\" rel=\"stylesheet\" {crossOrigin}/>";
    }

    public string BuildStyleTag(string content)
    {
        return $"<style>{content}</style>";
    }
}
