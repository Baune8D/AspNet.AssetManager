// <copyright file="GetBundlePathFixture.cs" company="Baune8D">
// Copyright (c) Baune8D. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.ComponentModel;
using System.Threading.Tasks;
using AwesomeAssertions;

namespace AspNet.AssetManager.Tests.Data;

internal sealed class GetBundlePathFixture : AssetServiceFixture
{
    public const string ValidBundleWithExtension = $"{ValidBundleWithoutExtension}.js";

    public const string InvalidBundleWithExtension = $"{InvalidBundle}.js";

    public GetBundlePathFixture(string bundle, FileType? fileType = null)
        : base(ValidBundleWithExtension)
    {
        Bundle = bundle;
        FileType = fileType;
        SetupGetFromManifest();
    }

    private string Bundle { get; }

    private FileType? FileType { get; }

    private string BundleWithCssExtension => $"{Bundle}.css";

    private string BundleWithJsExtension => $"{Bundle}.js";

    public async Task<string?> GetBundlePathAsync()
    {
        return await AssetService
            .GetBundlePathAsync(Bundle, FileType)
            .ConfigureAwait(false);
    }

    public void VerifyEmpty(string? result)
    {
        result.Should().BeNull();
        VerifyDependencies();
        VerifyNoOtherCalls();
    }

    public void VerifyNonExisting(string? result)
    {
        result.Should().BeNull();
        VerifyDependencies();
        VerifyGetFromManifest();
        VerifyNoOtherCalls();
    }

    public void VerifyExisting(string? result)
    {
        result.Should().NotBeNull()
            .And.Be(ValidBundleResultPath);
        VerifyDependencies();
        VerifyGetFromManifest();
        VerifyNoOtherCalls();
    }

    private void VerifyGetFromManifest()
    {
        switch (FileType)
        {
            case AssetManager.FileType.CSS:
                VerifyGetFromManifest(BundleWithCssExtension);
                break;
            case AssetManager.FileType.JS:
                VerifyGetFromManifest(BundleWithJsExtension);
                break;
            case null:
                VerifyGetFromManifest(Bundle);
                break;
            default:
                throw new InvalidEnumArgumentException(nameof(FileType), (int)FileType, typeof(FileType));
        }
    }
}
