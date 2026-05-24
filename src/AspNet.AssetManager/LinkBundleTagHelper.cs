// <copyright file="LinkBundleTagHelper.cs" company="Baune8D">
// Copyright (c) Baune8D. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AspNet.AssetManager;

/// <summary>
/// A TagHelper responsible for rendering link tags for CSS bundles. May emit multiple
/// <c>&lt;link&gt;</c> elements when the bundle's import graph contributes more than one
/// stylesheet (typical for Vite manifests with shared chunks or vendor splits).
/// </summary>
[OutputElementHint("link")]
[UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
public class LinkBundleTagHelper(IHtmlHelper htmlHelper, IAssetService assetService) : TagHelper
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
    /// Processes the tag helper and writes one or more link tags for the resolved bundle.
    /// </summary>
    /// <param name="context">The context associated with the tag helper execution.</param>
    /// <param name="output">The output that will be modified for rendering the link tags.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);

        if (htmlHelper is IViewContextAware aware)
        {
            aware.Contextualize(ViewContext);
        }

        var bundle = Name ?? ViewContext.ViewData.GetBundleName() ?? htmlHelper.GetBundleName();
        var html = await assetService.GetLinkTagAsync(bundle, Fallback).ConfigureAwait(false);

        if (html == HtmlString.Empty)
        {
            output.SuppressOutput();
            return;
        }

        output.TagName = null;
        output.Content.SetHtmlContent(html);
    }
}
