// <copyright file="GetBundlePathTests.cs" company="Baune8D">
// Copyright (c) Baune8D. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Threading.Tasks;
using AspNet.AssetManager.Tests.Data;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace AspNet.AssetManager.Tests.AssetServiceTests;

public sealed class GetBundlePathTests
{
    [Fact]
    public async Task GetBundlePath_EmptyString_ShouldReturnNull()
    {
        // Arrange
        var fixture = new GetBundlePathFixture(string.Empty);

        // Act
        var result = await fixture.GetBundlePathAsync();

        // Assert
        fixture.VerifyEmpty(result);
    }

    [Fact]
    public async Task GetBundlePath_ValidBundleWithoutExtension_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var fixture = new GetBundlePathFixture(AssetServiceFixture.ValidBundleWithoutExtension);

        // Act
        Func<Task> act = () => fixture.GetBundlePathAsync();

        // Assert
        await act.Should()
            .ThrowExactlyAsync<InvalidOperationException>()
            .WithMessage("A file extension is needed either in bundle name or as file type parameter.");
    }

    [Fact]
    public async Task GetBundlePath_ValidBundleWithExtensionAndFileTypeParam_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var fixture = new GetBundlePathFixture(GetBundlePathFixture.ValidBundleWithExtension, FileType.CSS);

        // Act
        Func<Task> act = () => fixture.GetBundlePathAsync();

        // Assert
        await act.Should()
            .ThrowExactlyAsync<InvalidOperationException>()
            .WithMessage("If bundle name already has an extension then do not specify it again as file type parameter.");
    }

    [Fact]
    public async Task GetBundlePath_InvalidBundleWithExtension_ShouldReturnNull()
    {
        // Arrange
        var fixture = new GetBundlePathFixture(GetBundlePathFixture.InvalidBundleWithExtension);

        // Act
        var result = await fixture.GetBundlePathAsync();

        // Assert
        fixture.VerifyNonExisting(result);
    }

    [Fact]
    public async Task GetBundlePath_ValidBundleWithExtension_ShouldReturnBundlePath()
    {
        // Arrange
        var fixture = new GetBundlePathFixture(GetBundlePathFixture.ValidBundleWithExtension);

        // Act
        var result = await fixture.GetBundlePathAsync();

        // Assert
        fixture.VerifyExisting(result);
    }

    [Fact]
    public async Task GetBundlePath_InvalidBundleWithExtensionAsParam_ShouldReturnNull()
    {
        // Arrange
        var fixture = new GetBundlePathFixture(AssetServiceFixture.ValidBundleWithoutExtension, FileType.CSS);

        // Act
        var result = await fixture.GetBundlePathAsync();

        // Assert
        fixture.VerifyNonExisting(result);
    }

    [Fact]
    public async Task GetBundlePath_ValidBundleWithExtensionAsParam_ShouldReturnBundlePath()
    {
        // Arrange
        var fixture = new GetBundlePathFixture(AssetServiceFixture.ValidBundleWithoutExtension, FileType.JS);

        // Act
        var result = await fixture.GetBundlePathAsync();

        // Assert
        fixture.VerifyExisting(result);
    }

    [Theory]
    [InlineData("Views_Home_Index.css", "Views_Home_Index-C1.css")]
    [InlineData(null, "Views_Home_Index-C1.css")]
    public async Task GetBundlePath_ViteCssBundleImportingSharedChunk_ShouldReturnEntryStylesheet(string? bundle, string expectedFile)
    {
        // Arrange - the view entry imports the layout chunk, which has a shared stylesheet.
        // The bundle path must point at the view's own stylesheet, not the shared one.
        const string manifest = """
                                {
                                  "Views/Home/Index.cshtml.ts": {
                                    "file": "Views_Home_Index-D1.js",
                                    "name": "Views_Home_Index",
                                    "imports": ["__Layout.cshtml-B7.js"],
                                    "css": ["Views_Home_Index-C1.css"]
                                  },
                                  "__Layout.cshtml-B7.js": {
                                    "file": "_Layout.cshtml-B7.js",
                                    "name": "_Layout.cshtml",
                                    "css": ["_Layout-WW.css"]
                                  }
                                }
                                """;

        var assetConfiguration = DependencyMocker.GetAssetConfiguration(TestValues.Production, ManifestType.Vite).Object;
        using var manifestService = new ManifestService(assetConfiguration, DependencyMocker.GetFileSystem(manifest).Object);
        var assetService = new AssetService(assetConfiguration, manifestService, new Mock<ITagBuilder>().Object);

        // Act
        var result = bundle != null
            ? await assetService.GetBundlePathAsync(bundle)
            : await assetService.GetBundlePathAsync("Views_Home_Index", FileType.CSS);

        // Assert
        result.Should().Be($"{TestValues.AssetsWebPath}{expectedFile}");
    }
}
