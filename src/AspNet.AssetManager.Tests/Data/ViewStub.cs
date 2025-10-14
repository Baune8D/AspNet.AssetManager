// <copyright file="ViewStub.cs" company="Baune8D">
// Copyright (c) Baune8D. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace AspNet.AssetManager.Tests.Data;

internal sealed class ViewStub(string view) : IView
{
    public string Path { get; } = view;

    public Task RenderAsync(ViewContext context)
    {
        throw new System.NotImplementedException();
    }
}
