// <copyright file="GetLinkTagTests.cs" company="Baune8D">
// Copyright (c) Baune8D. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;
using AspNet.AssetManager.Tests.Data;
using AwesomeAssertions;
using Microsoft.AspNetCore.Html;
using Moq;
using Xunit;

namespace AspNet.AssetManager.Tests.AssetServiceTests;

public sealed class GetLinkTagTests
{
    [Fact]
    public async Task GetLinkTag_BundleWithMultipleCss_ShouldEmitOneLinkPerFile()
    {
        // Arrange - manifest service reports two CSS files (typical for an entry that
        // imports a vendor chunk plus its own styles); the tag helper must render both.
        var manifestServiceMock = new Mock<IManifestService>();
        manifestServiceMock
            .Setup(x => x.GetCssFromManifestAsync("Layout.css"))
            .ReturnsAsync((IReadOnlyList<string>)["vendor.css", "layout.css"]);

        var tagBuilderMock = new Mock<ITagBuilder>();
        tagBuilderMock.Setup(x => x.BuildLinkTag("vendor.css")).Returns("<link href=\"/Path/To/Assets/vendor.css\" rel=\"stylesheet\" />");
        tagBuilderMock.Setup(x => x.BuildLinkTag("layout.css")).Returns("<link href=\"/Path/To/Assets/layout.css\" rel=\"stylesheet\" />");

        var assetConfigurationMock = DependencyMocker.GetAssetConfiguration(TestValues.Production);
        var assetService = new AssetService(assetConfigurationMock.Object, manifestServiceMock.Object, tagBuilderMock.Object);

        // Act
        var result = await assetService.GetLinkTagAsync("Layout");

        // Assert
        result.Should().BeEquivalentTo(new HtmlString(
            "<link href=\"/Path/To/Assets/vendor.css\" rel=\"stylesheet\" />\n" +
            "<link href=\"/Path/To/Assets/layout.css\" rel=\"stylesheet\" />"));
    }

    [Fact]
    public async Task GetStyleContent_BundleWithMultipleCss_ShouldConcatenateFileContent()
    {
        // Arrange
        var manifestServiceMock = new Mock<IManifestService>();
        manifestServiceMock
            .Setup(x => x.GetCssFromManifestAsync("Layout.css"))
            .ReturnsAsync((IReadOnlyList<string>)["a.css", "b.css"]);
        manifestServiceMock.Setup(x => x.GetFileContentAsync("a.css")).ReturnsAsync("a {}");
        manifestServiceMock.Setup(x => x.GetFileContentAsync("b.css")).ReturnsAsync("b {}");

        var tagBuilderMock = new Mock<ITagBuilder>();
        var assetConfigurationMock = DependencyMocker.GetAssetConfiguration(TestValues.Production);
        var assetService = new AssetService(assetConfigurationMock.Object, manifestServiceMock.Object, tagBuilderMock.Object);

        // Act
        var result = await assetService.GetStyleContent("Layout");

        // Assert
        result.Should().Be("a {}b {}");
    }


    [Fact]
    public async Task GetLinkTag_EmptyString_ShouldReturnEmptyHtmlString()
    {
        // Arrange
        var fixture = new GetLinkTagFixture(string.Empty);

        // Act
        var result = await fixture.GetLinkTagAsync();

        // Assert
        fixture.VerifyEmpty(result);
    }

    [Fact]
    public async Task GetLinkTag_InvalidBundle_ShouldReturnEmptyHtmlString()
    {
        // Arrange
        var fixture = new GetLinkTagFixture(AssetServiceFixture.InvalidBundle);

        // Act
        var result = await fixture.GetLinkTagAsync();

        // Assert
        fixture.VerifyNonExisting(result);
    }

    [Fact]
    public async Task GetLinkTag_ValidBundle_ShouldReturnLinkTag()
    {
        // Arrange
        var fixture = new GetLinkTagFixture(AssetServiceFixture.ValidBundleWithoutExtension);

        // Act
        var result = await fixture.GetLinkTagAsync();

        // Assert
        fixture.VerifyExisting(result);
    }

    [Fact]
    public async Task GetLinkTag_ValidBundleWithExtension_ShouldReturnStyleTag()
    {
        // Arrange
        var fixture = new GetLinkTagFixture(GetLinkTagFixture.ValidBundleWithExtension);

        // Act
        var result = await fixture.GetLinkTagAsync();

        // Assert
        fixture.VerifyExisting(result);
    }

    [Fact]
    public async Task GetLinkTag_FallbackEmptyString_ShouldReturnEmptyHtmlString()
    {
        // Arrange
        var fixture = new GetLinkTagFixture(AssetServiceFixture.InvalidBundle, string.Empty);

        // Act
        var result = await fixture.GetLinkTagAsync();

        // Assert
        fixture.VerifyFallbackEmpty(result);
    }

    [Fact]
    public async Task GetLinkTag_InvalidFallbackBundle_ShouldReturnEmptyHtmlString()
    {
        // Arrange
        var fixture = new GetLinkTagFixture(AssetServiceFixture.InvalidBundle, AssetServiceFixture.InvalidBundle);

        // Act
        var result = await fixture.GetLinkTagAsync();

        // Assert
        fixture.VerifyFallbackNonExisting(result);
    }

    [Fact]
    public async Task GetLinkTag_ValidFallbackBundle_ShouldReturnStyleTag()
    {
        // Arrange
        var fixture = new GetLinkTagFixture(AssetServiceFixture.InvalidBundle, AssetServiceFixture.ValidFallbackBundleWithoutExtension);

        // Act
        var result = await fixture.GetLinkTagAsync();

        // Assert
        fixture.VerifyFallbackExisting(result);
    }

    [Fact]
    public async Task GetLinkTag_ValidFallbackBundleWithExtension_ShouldReturnStyleTag()
    {
        // Arrange
        var fixture = new GetLinkTagFixture(AssetServiceFixture.InvalidBundle, GetLinkTagFixture.ValidFallbackBundleWithExtension);

        // Act
        var result = await fixture.GetLinkTagAsync();

        // Assert
        fixture.VerifyFallbackExisting(result);
    }
}
