// <copyright file="GetLinkTagFixture.cs" company="Baune8D">
// Copyright (c) Baune8D. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.AspNetCore.Html;
using Moq;

namespace AspNet.AssetManager.Tests.Data;

internal sealed class GetLinkTagFixture : AssetServiceBaseFixture
{
    public const string ValidBundleWithExtension = $"{ValidBundleWithoutExtension}.css";

    public const string ValidFallbackBundleWithExtension = $"{ValidFallbackBundleWithoutExtension}.css";

    private const string LinkTag = "<link href=\"Bundle.css\" />";
    private const string FallbackLinkTag = "<link href=\"FallbackBundle.css\" />";

    public GetLinkTagFixture(string bundle, string? fallbackBundle = null)
        : base(ValidBundleWithExtension, ValidFallbackBundleWithExtension)
    {
        Bundle = bundle;
        FallbackBundle = fallbackBundle;
        SetupGetFromManifest();
        SetupBuildLinkTag(ValidBundleResult, LinkTag);
        SetupBuildLinkTag(ValidFallbackBundleResult, FallbackLinkTag);
    }

    private string Bundle { get; }

    private string? FallbackBundle { get; }

    public async Task<HtmlString> GetLinkTagAsync()
    {
        return await AssetService
            .GetLinkTagAsync(Bundle, FallbackBundle)
            .ConfigureAwait(false);
    }

    public void VerifyEmpty(HtmlString result)
    {
        result.Should().Be(HtmlString.Empty);
        VerifyDependencies();
        VerifyNoOtherCalls();
    }

    public void VerifyNonExisting(HtmlString result)
    {
        result.Should().Be(HtmlString.Empty);
        VerifyDependencies();
        VerifyGetFromManifest(Bundle, FallbackBundle, ".css");
        VerifyNoOtherCalls();
    }

    public void VerifyExisting(HtmlString result)
    {
        result.Should().BeEquivalentTo(new HtmlString(LinkTag));
        VerifyDependencies();
        VerifyGetFromManifest(Bundle, FallbackBundle, ".css");
        VerifyBuildLinkTag(ValidBundleResult);
        VerifyNoOtherCalls();
    }

    public void VerifyFallbackEmpty(HtmlString result)
    {
        result.Should().Be(HtmlString.Empty);
        VerifyDependencies();
        VerifyGetFromManifest(Bundle, FallbackBundle, ".css");
        VerifyNoOtherCalls();
    }

    public void VerifyFallbackNonExisting(HtmlString result)
    {
        result.Should().Be(HtmlString.Empty);
        VerifyDependencies();
        VerifyGetFromManifest(Bundle, FallbackBundle, ".css");
        VerifyNoOtherCalls();
    }

    public void VerifyFallbackExisting(HtmlString result)
    {
        result.Should().BeEquivalentTo(new HtmlString(FallbackLinkTag));
        VerifyDependencies();
        VerifyGetFromManifest(Bundle, FallbackBundle, ".css");
        VerifyBuildLinkTag(ValidFallbackBundleResult);
        VerifyNoOtherCalls();
    }

    private void SetupBuildLinkTag(string resultBundle, string returnValue)
    {
        TagBuilderMock
            .Setup(x => x.BuildLinkTag(resultBundle))
            .Returns(returnValue);
    }

    private void VerifyBuildLinkTag(string resultBundle)
    {
        TagBuilderMock.Verify(x => x.BuildLinkTag(resultBundle), Times.Once());
    }
}
