// <copyright file="GetStyleTagFixture.cs" company="Baune8D">
// Copyright (c) Baune8D. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.AspNetCore.Html;
using Moq;

namespace AspNet.AssetManager.Tests.Data;

internal sealed class GetStyleTagFixture : AssetServiceFixture
{
    public const string ValidBundleWithExtension = $"{ValidBundleWithoutExtension}.css";

    public const string ValidFallbackBundleWithExtension = $"{ValidFallbackBundleWithoutExtension}.css";

    private const string StyleTag = "<style>Some Content</script>";
    private const string FallbackStyleTag = "<style>Some Fallback Content</style>";

    public GetStyleTagFixture(string bundle, string? fallbackBundle = null)
        : base(ValidBundleWithExtension, ValidFallbackBundleWithExtension)
    {
        Bundle = bundle;
        FallbackBundle = fallbackBundle;
        SetupGetFromManifest();
        SetupBuildStyleTag(ValidBundleResult, StyleTag);
        SetupBuildStyleTag(ValidFallbackBundleResult, FallbackStyleTag);
    }

    private string Bundle { get; }

    private string? FallbackBundle { get; }

    public async Task<HtmlString> GetStyleTagAsync()
    {
        return await AssetService
            .GetStyleTagAsync(Bundle, FallbackBundle)
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
        result.Should().BeEquivalentTo(new HtmlString(StyleTag));
        VerifyDependencies();
        VerifyGetFromManifest(Bundle, FallbackBundle, ".css");
        VerifyBuildStyleTag(ValidBundleResult);
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
        result.Should().BeEquivalentTo(new HtmlString(FallbackStyleTag));
        VerifyDependencies();
        VerifyGetFromManifest(Bundle, FallbackBundle, ".css");
        VerifyBuildStyleTag(ValidFallbackBundleResult);
        VerifyNoOtherCalls();
    }

    private void SetupBuildStyleTag(string resultBundle, string returnValue)
    {
        TagBuilderMock
            .Setup(x => x.BuildStyleTag(resultBundle))
            .Returns(returnValue);
    }

    private void VerifyBuildStyleTag(string resultBundle)
    {
        TagBuilderMock.Verify(x => x.BuildStyleTag(resultBundle), Times.Once());
    }
}
