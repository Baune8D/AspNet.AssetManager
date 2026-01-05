// <copyright file="LinkBundleTagHelper.cs" company="Baune8D">
// Copyright (c) Baune8D. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AspNet.AssetManager;

/// <summary>
/// A TagHelper responsible for rendering a link tag for CSS bundles.
/// </summary>
[OutputElementHint("link")]
[UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
public class LinkBundleTagHelper(
    IHtmlHelper htmlHelper,
    IAssetService assetService,
    IAssetConfiguration assetConfiguration)
    : TagHelper
{
    /// <summary>
    /// Gets or sets the context information about the current view rendering process.
    /// This property is automatically populated by the framework when the tag helper is used in a view.
    /// It is used to access data, services, and other information related to the current rendering context.
    /// </summary>
    [ViewContext]
    public required ViewContext ViewContext { get; set; }

    /// <summary>
    /// Gets or sets the name of the CSS bundle to be rendered by the tag helper.
    /// This property is used to identify the bundle for which the link tag will be generated.
    /// If not provided, the bundle name may be resolved from the current view context or HTML helper extensions.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the fallback bundle name to be used if the primary bundle is unavailable.
    /// </summary>
    public string? Fallback { get; set; }

    /// <summary>
    /// Processes the tag helper and modifies the output to render a link tag for a CSS bundle.
    /// </summary>
    /// <param name="context">The context associated with the tag helper execution.</param>
    /// <param name="output">The output that will be modified for rendering the link tag.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);

        if (htmlHelper is IViewContextAware aware)
        {
            aware.Contextualize(ViewContext);
        }

        output.TagName = "link";
        output.TagMode = TagMode.SelfClosing;

        var bundle = Name ?? ViewContext.ViewData.GetBundleName() ?? htmlHelper.GetBundleName();
        var file = await assetService.GetLinkHref(bundle, Fallback).ConfigureAwait(false);

        if (file is null)
        {
            output.SuppressOutput();
            return;
        }

        output.Attributes.SetAttribute("href", $"{assetConfiguration.AssetsWebPath}{file}");

        output.Attributes.SetAttribute("rel", "stylesheet");

        if (assetConfiguration.DevelopmentMode)
        {
            output.Attributes.SetAttribute("crossorigin", "anonymous");
        }
    }
}
