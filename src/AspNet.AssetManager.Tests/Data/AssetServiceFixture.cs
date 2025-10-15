// <copyright file="AssetServiceFixture.cs" company="Baune8D">
// Copyright (c) Baune8D. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using Moq;

namespace AspNet.AssetManager.Tests.Data;

internal abstract class AssetServiceFixture
{
    public const string ValidBundleWithoutExtension = "Bundle";

    public const string ValidFallbackBundleWithoutExtension = "FallbackBundle";

    public const string InvalidBundle = "InvalidBundle";

    public const string BundleContent = "Bundle Content";

    public const string FallbackBundleContent = "Fallback Bundle Content";

    protected AssetServiceFixture(string validTestBundle, string? validFallbackTestBundle = null)
    {
        ValidTestBundle = validTestBundle;
        ValidFallbackTestBundle = validFallbackTestBundle;

        AssetConfigurationMock = DependencyMocker.GetAssetConfiguration(TestValues.Development);
        ManifestServiceMock = new Mock<IManifestService>();
        TagBuilderMock = new Mock<ITagBuilder>();

        AssetService = new AssetService(AssetConfigurationMock.Object, ManifestServiceMock.Object, TagBuilderMock.Object);
    }

    protected IAssetService AssetService { get; }

    protected Mock<ITagBuilder> TagBuilderMock { get; }

    protected string ValidBundleResult => ValidTestBundle;

    protected string ValidBundleResultPath => $"{AssetService.WebPath}{ValidBundleResult}";

    protected string ValidFallbackBundleResult => $"{ValidFallbackTestBundle}";

    private Mock<IAssetConfiguration> AssetConfigurationMock { get; }

    private Mock<IManifestService> ManifestServiceMock { get; }

    private string ValidTestBundle { get; }

    private string? ValidFallbackTestBundle { get; }

    protected void VerifyDependencies()
    {
        AssetConfigurationMock.VerifyGet(x => x.AssetsDirectoryPath, Times.Once);
    }

    protected void VerifyGetFromManifest(string bundle, Times? times = null)
    {
        ManifestServiceMock.Verify(x => x.GetFromManifestAsync(bundle), times ?? Times.Once());
    }

    protected void VerifyGetFromManifest(string bundle, string? fallbackBundle, string extension)
    {
        ArgumentNullException.ThrowIfNull(bundle);

        var bundleIsValid =
            bundle == ValidBundleWithoutExtension ||
            bundle == $"{ValidBundleWithoutExtension}{extension}";

        if (bundle == fallbackBundle)
        {
            VerifyGetFromManifest(
                bundle.EndsWith(extension, StringComparison.Ordinal) ? bundle : $"{bundle}{extension}",
                bundleIsValid ? Times.Once() : Times.Exactly(2));
        }
        else
        {
            VerifyGetFromManifest(bundle.EndsWith(extension, StringComparison.Ordinal)
                ? bundle
                : $"{bundle}{extension}");

            if (!string.IsNullOrEmpty(fallbackBundle) && !bundleIsValid)
            {
                VerifyGetFromManifest(fallbackBundle.EndsWith(extension, StringComparison.Ordinal)
                    ? fallbackBundle
                    : $"{fallbackBundle}{extension}");
            }
        }
    }

    protected void VerifyGetFileContent(string file, Times? times = null)
    {
        ManifestServiceMock.Verify(x => x.GetFileContentAsync(file), times ?? Times.Once());
    }

    protected void VerifyNoOtherCalls()
    {
        ManifestServiceMock.VerifyNoOtherCalls();
    }

    protected void SetupGetFromManifest()
    {
        ManifestServiceMock
            .Setup(x => x.GetFromManifestAsync(It.IsAny<string>()))
            .ReturnsAsync((string?)null);

        ManifestServiceMock
            .Setup(x => x.GetFromManifestAsync(ValidTestBundle))
            .ReturnsAsync(ValidBundleResult);

        if (ValidFallbackTestBundle != null)
        {
            ManifestServiceMock
                .Setup(x => x.GetFromManifestAsync(ValidFallbackTestBundle))
                .ReturnsAsync(ValidFallbackBundleResult);
        }
    }

    protected void SetupGetFileContent()
    {
        ManifestServiceMock
            .Setup(x => x.GetFileContentAsync(It.IsAny<string>()))
            .ReturnsAsync(string.Empty);

        ManifestServiceMock
            .Setup(x => x.GetFileContentAsync(ValidBundleResult))
            .ReturnsAsync(BundleContent);

        if (ValidFallbackTestBundle != null)
        {
            ManifestServiceMock
                .Setup(x => x.GetFileContentAsync(ValidFallbackTestBundle))
                .ReturnsAsync(FallbackBundleContent);
        }
    }
}
