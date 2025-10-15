// <copyright file="TagBuilderTests.cs" company="Baune8D">
// Copyright (c) Baune8D. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.ComponentModel;
using AspNet.AssetManager.Tests.Data;
using AwesomeAssertions;
using Xunit;

namespace AspNet.AssetManager.Tests;

public sealed class TagBuilderTests
{
    private const string Bundle = "Bundle.js";
    private const string StyleContent = "CSS content";

    private TagBuilder? _tagBuilder;

    private static string ValidBundleResult => $"{TestValues.AssetsWebPath}{Bundle}";

    [Fact]
    public void BuildScriptTag_InvalidScriptLoad_ShouldThrowInvalidEnumArgumentException()
    {
        // Arrange
        var assetConfigurationMock = DependencyMocker.GetAssetConfiguration(TestValues.Development);
        _tagBuilder = new TagBuilder(assetConfigurationMock.Object);

        // Act
        Action act = () => _tagBuilder.BuildScriptTag(Bundle, (ScriptLoad)6);

        // Assert
        act.Should().ThrowExactly<InvalidEnumArgumentException>();
    }

    [Fact]
    public void BuildScriptTag_Development_ShouldReturnScriptTag()
    {
        // Arrange
        var assetConfigurationMock = DependencyMocker.GetAssetConfiguration(TestValues.Development);
        _tagBuilder = new TagBuilder(assetConfigurationMock.Object);

        // Act
        var result = _tagBuilder.BuildScriptTag(Bundle, ScriptLoad.Normal);

        // Assert
        VerifyScriptTag(result);
        result.Should().Contain("crossorigin=\"anonymous\"")
            .And.NotContain("type=\"module\"")
            .And.NotContain("async")
            .And.NotContain("defer");
    }

    [Fact]
    public void BuildScriptTag_DevelopmentVite_ShouldReturnScriptTag()
    {
        // Arrange
        var assetConfigurationMock = DependencyMocker.GetAssetConfiguration(TestValues.Development, ManifestType.Vite);
        _tagBuilder = new TagBuilder(assetConfigurationMock.Object);

        // Act
        var result = _tagBuilder.BuildScriptTag(Bundle, ScriptLoad.Normal);

        // Assert
        VerifyScriptTag(result);
        result.Should().Contain("crossorigin=\"anonymous\"")
            .And.Contain("type=\"module\"")
            .And.NotContain("async")
            .And.NotContain("defer");
    }

    [Fact]
    public void BuildScriptTag_DevelopmentAsyncScriptLoad_ShouldReturnScriptTag()
    {
        // Arrange
        var assetConfigurationMock = DependencyMocker.GetAssetConfiguration(TestValues.Development);
        _tagBuilder = new TagBuilder(assetConfigurationMock.Object);

        // Act
        var result = _tagBuilder.BuildScriptTag(Bundle, ScriptLoad.Async);

        // Assert
        VerifyScriptTag(result);
        result.Should().Contain("crossorigin=\"anonymous\"")
            .And.NotContain("type=\"module\"")
            .And.Contain("async")
            .And.NotContain("defer");
    }

    [Fact]
    public void BuildScriptTag_DevelopmentDeferScriptLoad_ShouldReturnScriptTag()
    {
        // Arrange
        var assetConfigurationMock = DependencyMocker.GetAssetConfiguration(TestValues.Development);
        _tagBuilder = new TagBuilder(assetConfigurationMock.Object);

        // Act
        var result = _tagBuilder.BuildScriptTag(Bundle, ScriptLoad.Defer);

        // Assert
        VerifyScriptTag(result);
        result.Should().Contain("crossorigin=\"anonymous\"")
            .And.NotContain("type=\"module\"")
            .And.Contain("defer")
            .And.NotContain("async");
    }

    [Fact]
    public void BuildScriptTag_DevelopmentAsyncDeferScriptLoad_ShouldReturnScriptTag()
    {
        // Arrange
        var assetConfigurationMock = DependencyMocker.GetAssetConfiguration(TestValues.Development);
        _tagBuilder = new TagBuilder(assetConfigurationMock.Object);

        // Act
        var result = _tagBuilder.BuildScriptTag(Bundle, ScriptLoad.AsyncDefer);

        // Assert
        VerifyScriptTag(result);
        result.Should().Contain("crossorigin=\"anonymous\"")
            .And.NotContain("type=\"module\"")
            .And.Contain("async")
            .And.Contain("defer");
    }

    [Fact]
    public void BuildScriptTag_Production_ShouldReturnScriptTag()
    {
        // Arrange
        var assetConfigurationMock = DependencyMocker.GetAssetConfiguration(TestValues.Production);
        _tagBuilder = new TagBuilder(assetConfigurationMock.Object);

        // Act
        var result = _tagBuilder.BuildScriptTag(Bundle, ScriptLoad.Normal);

        // Assert
        VerifyScriptTag(result);
        result.Should().NotContain("crossorigin=\"anonymous\"")
            .And.NotContain("async")
            .And.NotContain("defer");
    }

    [Fact]
    public void BuildScriptTag_ProductionVite_ShouldReturnScriptTag()
    {
        // Arrange
        var assetConfigurationMock = DependencyMocker.GetAssetConfiguration(TestValues.Production, ManifestType.Vite);
        _tagBuilder = new TagBuilder(assetConfigurationMock.Object);

        // Act
        var result = _tagBuilder.BuildScriptTag(Bundle, ScriptLoad.Normal);

        // Assert
        VerifyScriptTag(result);
        result.Should().NotContain("crossorigin=\"anonymous\"")
            .And.NotContain("type=\"module\"")
            .And.NotContain("async")
            .And.NotContain("defer");
    }

    [Fact]
    public void BuildLinkTag_Development_ShouldReturnLinkTag()
    {
        // Arrange
        var assetConfigurationMock = DependencyMocker.GetAssetConfiguration(TestValues.Development);
        _tagBuilder = new TagBuilder(assetConfigurationMock.Object);

        // Act
        var result = _tagBuilder.BuildLinkTag(Bundle);

        // Assert
        VerifyLinkTag(result);
        result.Should().Contain("crossorigin=\"anonymous\"");
    }

    [Fact]
    public void BuildLinkTag_Production_ShouldReturnLinkTag()
    {
        // Arrange
        var assetConfigurationMock = DependencyMocker.GetAssetConfiguration(TestValues.Production);
        _tagBuilder = new TagBuilder(assetConfigurationMock.Object);

        // Act
        var result = _tagBuilder.BuildLinkTag(Bundle);

        // Assert
        VerifyLinkTag(result);
        result.Should().NotContain("crossorigin=\"anonymous\"");
    }

    [Fact]
    public void BuildStyleTag_Development_ShouldReturnStyleTag()
    {
        // Arrange
        var assetConfigurationMock = DependencyMocker.GetAssetConfiguration(TestValues.Development);
        _tagBuilder = new TagBuilder(assetConfigurationMock.Object);

        // Act
        var result = _tagBuilder.BuildStyleTag(StyleContent);

        // Assert
        VerifyStyleTag(result);
    }

    [Fact]
    public void BuildStyleTag_Production_ShouldReturnStyleTag()
    {
        // Arrange
        var assetConfigurationMock = DependencyMocker.GetAssetConfiguration(TestValues.Production);
        _tagBuilder = new TagBuilder(assetConfigurationMock.Object);

        // Act
        var result = _tagBuilder.BuildStyleTag(StyleContent);

        // Assert
        VerifyStyleTag(result);
    }

    private static void VerifyStyleTag(string result)
    {
        result.Should().StartWith("<style>")
            .And.EndWith("</style>")
            .And.Contain(StyleContent);
    }

    private static void VerifyScriptTag(string result)
    {
        result.Should().StartWith("<script ")
            .And.EndWith("</script>")
            .And.Contain($"src=\"{ValidBundleResult}\"");
    }

    private static void VerifyLinkTag(string result)
    {
        result.Should().StartWith("<link ")
            .And.EndWith(" />")
            .And.Contain($"href=\"{ValidBundleResult}\"")
            .And.Contain("rel=\"stylesheet\"");
    }
}
