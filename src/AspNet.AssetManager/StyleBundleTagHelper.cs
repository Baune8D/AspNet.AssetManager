// <copyright file="StyleBundleTagHelper.cs" company="Baune8D">
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
/// A TagHelper implementation that generates and renders a style tag for a CSS bundle.
/// </summary>
[OutputElementHint("style")]
[UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
public class StyleBundleTagHelper(IHtmlHelper htmlHelper, IAssetService assetService) : TagHelper
{
    /// <summary>
    /// Gets or sets the context information about the current view rendering process.
    /// This property is automatically populated by the framework when the tag helper is used in a view.
    /// It is used to access data, services, and other information related to the current rendering context.
    /// </summary>
    [ViewContext]
    public required ViewContext ViewContext { get; set; }

    /// <summary>
    /// Gets or sets the name of the CSS bundle to be rendered.
    /// This property is used to identify the specific bundle from which styles should be loaded.
    /// If not explicitly set, the name may be derived from the current view context or HTML helper extensions.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the fallback bundle name to be used if the primary bundle is unavailable.
    /// </summary>
    public string? Fallback { get; set; }

    /// <summary>
    /// Processes the tag helper's logic asynchronously to generate a style tag for a CSS bundle.
    /// </summary>
    /// <param name="context">The context for the tag helper execution.</param>
    /// <param name="output">The output object used to generate the HTML content.</param>
    /// <returns>A task that represents the asynchronous operation of processing the tag helper.</returns>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);

        if (htmlHelper is IViewContextAware aware)
        {
            aware.Contextualize(ViewContext);
        }

        output.TagName = "style";
        output.TagMode = TagMode.StartTagAndEndTag;

        var bundle = Name ?? ViewContext.ViewData.GetBundleName() ?? htmlHelper.GetBundleName();
        var content = await assetService.GetStyleContent(bundle, Fallback).ConfigureAwait(false);

        if (string.IsNullOrEmpty(content))
        {
            output.SuppressOutput();
            return;
        }

        output.Content.SetHtmlContent(content);
    }
}
