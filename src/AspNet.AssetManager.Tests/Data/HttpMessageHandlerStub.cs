// <copyright file="HttpMessageHandlerStub.cs" company="Baune8D">
// Copyright (c) Baune8D. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AspNet.AssetManager.Tests.Data;

internal sealed class HttpMessageHandlerStub(HttpStatusCode httpStatusCode, string content, bool json) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new HttpResponseMessage
        {
            StatusCode = httpStatusCode,
            Content = json
                ? new StringContent(content, Encoding.UTF8, "application/json")
                : new StringContent(content),
        });
    }
}
