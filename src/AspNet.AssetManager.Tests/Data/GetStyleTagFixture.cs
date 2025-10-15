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

    private const string StyleTag = $"<style>{BundleContent}</script>";
    private const string FallbackStyleTag = $"<style>{FallbackBundleContent}</script>";

    public GetStyleTagFixture(string bundle, string? fallbackBundle = null)
        : base(ValidBundleWithExtension, ValidFallbackBundleWithExtension)
    {
        Bundle = bundle;
        FallbackBundle = fallbackBundle;
        SetupGetFromManifest();
        SetupGetFileContent();
        SetupBuildStyleTag();
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
        VerifyGetFileContent(ValidBundleWithExtension);
        VerifyBuildStyleTag(BundleContent);
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
        VerifyGetFileContent(ValidFallbackBundleWithExtension);
        VerifyBuildStyleTag(FallbackBundleContent);
        VerifyNoOtherCalls();
    }

    private void SetupBuildStyleTag()
    {
        TagBuilderMock
            .Setup(x => x.BuildStyleTag(BundleContent))
            .Returns(StyleTag);

        TagBuilderMock
            .Setup(x => x.BuildStyleTag(FallbackBundleContent))
            .Returns(FallbackStyleTag);
    }

    private void VerifyBuildStyleTag(string resultBundle)
    {
        TagBuilderMock.Verify(x => x.BuildStyleTag(resultBundle), Times.Once());
    }
}
