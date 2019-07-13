// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Hosting.Server.Abstractions
{
    /// <summary>
    /// When implemented by a Server allows an <see cref="IHttpApplication{THostContext}"/> to pool and reuse
    /// its <typeparamref name="THostContext"/> between requests.
    /// </summary>
    /// <typeparam name="THostContext">The <see cref="IHttpApplication{THostContext}"/> Host context</typeparam>
    public interface IContextContainer<THostContext> : IDefaultHttpContextContainer
    {
        THostContext HostContext { get; set; }
    }
}
