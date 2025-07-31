// <copyright file="HtmlHelperExtensionsTests.cs" company="Morten Larsen">
// Copyright (c) Morten Larsen. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using AspNet.AssetManager.Tests.Data;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Xunit;

namespace AspNet.AssetManager.Tests;

public class HtmlHelperExtensionsTests
{
    [Fact]
    public void GetBundleName_Null_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => ((IHtmlHelper)null!).GetBundleName();

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void GetBundleName_MvcView_ShouldReturnBundleName()
    {
        // Arrange
        IHtmlHelper htmlHelper = new HtmlHelperStub("/Views/Some/Page.cshtml");

        // Act
        var result = htmlHelper.GetBundleName();

        // Assert
        result.Should().Be("Views_Some_Page");
    }

    [Fact]
    public void GetBundleName_AreaMvcView_ShouldReturnBundleName()
    {
        // Arrange
        IHtmlHelper htmlHelper = new HtmlHelperStub("/Areas/Test/Views/Some/Page.cshtml");

        // Act
        var result = htmlHelper.GetBundleName();

        // Assert
        result.Should().Be("Areas_Test_Views_Some_Page");
    }

    [Fact]
    public void GetBundleName_RazorPage_ShouldReturnBundleName()
    {
        // Arrange
        IHtmlHelper htmlHelper = new HtmlHelperStub("/Pages/Some/Page.cshtml");

        // Act
        var result = htmlHelper.GetBundleName();

        // Assert
        result.Should().Be("Pages_Some_Page");
    }

    [Fact]
    public void GetBundleName_AreaRazorPage_ShouldReturnBundleName()
    {
        // Arrange
        IHtmlHelper htmlHelper = new HtmlHelperStub("/Areas/Test/Pages/Some/Page.cshtml");

        // Act
        var result = htmlHelper.GetBundleName();

        // Assert
        result.Should().Be("Areas_Test_Pages_Some_Page");
    }
}
