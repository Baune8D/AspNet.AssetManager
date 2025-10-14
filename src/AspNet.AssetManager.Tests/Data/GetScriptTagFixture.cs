// <copyright file="GetScriptTagFixture.cs" company="Baune8D">
// Copyright (c) Baune8D. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.AspNetCore.Html;
using Moq;

namespace AspNet.AssetManager.Tests.Data;

internal sealed class GetScriptTagFixture : AssetServiceBaseFixture
{
    public const string ValidFallbackBundleWithExtension = $"{ValidFallbackBundleWithoutExtension}.js";

    private const string ValidBundleWithExtension = $"{ValidBundleWithoutExtension}.js";

    private const string ScriptTag = "<script src=\"Bundle.js\"></script>";
    private const string FallbackScriptTag = "<script src=\"FallbackBundle.js\"></script>";

    public GetScriptTagFixture(string bundle, ScriptLoad scriptLoad = ScriptLoad.Normal)
        : base(ValidBundleWithExtension, ValidFallbackBundleWithExtension)
    {
        Bundle = bundle;
        ScriptLoad = scriptLoad;
        SetupGetFromManifest();
        SetupBuildScriptTag(ValidBundleResult, ScriptTag);
        SetupBuildScriptTag(ValidFallbackBundleResult, FallbackScriptTag);
    }

    public GetScriptTagFixture(string bundle, string fallbackBundle, ScriptLoad scriptLoad = ScriptLoad.Normal)
        : this(bundle, scriptLoad)
    {
        FallbackBundle = fallbackBundle;
    }

    private ScriptLoad ScriptLoad { get; }

    private string Bundle { get; }

    private string? FallbackBundle { get; }

    public async Task<HtmlString> GetScriptTagAsync()
    {
        if (FallbackBundle == null)
        {
            return await AssetService
                .GetScriptTagAsync(Bundle, ScriptLoad)
                .ConfigureAwait(false);
        }

        return await AssetService
            .GetScriptTagAsync(Bundle, FallbackBundle, ScriptLoad)
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
        VerifyGetFromManifest(Bundle, FallbackBundle, ".js");
        VerifyNoOtherCalls();
    }

    public void VerifyExisting(HtmlString result)
    {
        result.Should().BeEquivalentTo(new HtmlString(ScriptTag));
        VerifyDependencies();
        VerifyGetFromManifest(Bundle, FallbackBundle, ".js");
        VerifyBuildScriptTag(ValidBundleResult);
        VerifyNoOtherCalls();
    }

    public void VerifyFallbackEmpty(HtmlString result)
    {
        result.Should().Be(HtmlString.Empty);
        VerifyDependencies();
        VerifyGetFromManifest(Bundle, FallbackBundle, ".js");
        VerifyNoOtherCalls();
    }

    public void VerifyFallbackNonExisting(HtmlString result)
    {
        result.Should().Be(HtmlString.Empty);
        VerifyDependencies();
        VerifyGetFromManifest(Bundle, FallbackBundle, ".js");
        VerifyNoOtherCalls();
    }

    public void VerifyFallbackExisting(HtmlString result)
    {
        result.Should().BeEquivalentTo(new HtmlString(FallbackScriptTag));
        VerifyDependencies();
        VerifyGetFromManifest(Bundle, FallbackBundle, ".js");
        VerifyBuildScriptTag(ValidFallbackBundleResult);
        VerifyNoOtherCalls();
    }

    private void SetupBuildScriptTag(string resultBundle, string returnValue)
    {
        TagBuilderMock
            .Setup(x => x.BuildScriptTag(resultBundle, ScriptLoad))
            .Returns(returnValue);
    }

    private void VerifyBuildScriptTag(string resultBundle)
    {
        TagBuilderMock.Verify(x => x.BuildScriptTag(resultBundle, ScriptLoad), Times.Once());
    }
}
