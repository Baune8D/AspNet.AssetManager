// <copyright file="ScriptBundleTagHelper.cs" company="Baune8D">
// Copyright (c) Baune8D. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AspNet.AssetManager;

/// <summary>
/// A TagHelper implementation that generates a "script" tag for a JavaScript bundle,
/// with support for fallback, async, and defer attributes.
/// </summary>
[OutputElementHint("script")]
public class ScriptBundleTagHelper(
    IHtmlHelper htmlHelper,
    IAssetService assetService,
    IAssetConfiguration assetConfiguration)
    : TagHelper
{
    /// <summary>
    /// Gets or sets the current view context, which encapsulates information about the current view execution,
    /// including view data, temporary data, and other context-specific settings.
    /// </summary>
    [ViewContext]
    public required ViewContext ViewContext { get; set; }

    /// <summary>
    /// Gets or sets the fallback value used when a JavaScript bundle source cannot be resolved.
    /// </summary>
    public string? Fallback { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the script tag should include the "async" attribute.
    /// </summary>
    public bool Async { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the script should be deferred until the HTML document has been parsed.
    /// When set to true, the "defer" attribute will be added to the generated script tag.
    /// </summary>
    public bool Defer { get; set; }

    /// <summary>
    /// Processes the tag helper to generate a script tag for a JavaScript bundle.
    /// </summary>
    /// <param name="context">The context for the tag helper execution.</param>
    /// <param name="output">The output object used to generate the HTML content.</param>
    /// <returns>A task that represents the asynchronous operation of processing the tag helper.</returns>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);

        if (output.IsContentModified)
        {
            return;
        }

        if (htmlHelper is IViewContextAware aware)
        {
            aware.Contextualize(ViewContext);
        }

        output.TagName = "script";
        output.TagMode = TagMode.StartTagAndEndTag;

        var bundle = ViewContext.ViewData.GetBundleName() ?? htmlHelper.GetBundleName();
        var file = await assetService.GetScriptSrc(bundle, Fallback).ConfigureAwait(false);

        output.Attributes.SetAttribute("src", $"{assetConfiguration.AssetsWebPath}{file}");

        if (assetConfiguration.DevelopmentMode)
        {
            if (assetConfiguration.ManifestType == ManifestType.Vite)
            {
                output.Attributes.SetAttribute("type", "module");
            }

            output.Attributes.SetAttribute("crossorigin", "anonymous");
        }

        if (Async)
        {
            output.Attributes.SetAttribute(new TagHelperAttribute("async"));
        }

        if (Defer)
        {
            output.Attributes.SetAttribute(new TagHelperAttribute("defer"));
        }
    }
}
