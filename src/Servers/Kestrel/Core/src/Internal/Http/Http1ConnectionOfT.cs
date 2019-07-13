// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Hosting.Server.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http
{
    internal sealed class Http1Connection<THostContext> : Http1Connection, IContextContainer<THostContext>
    {
        public Http1Connection(HttpConnectionContext context) : base(context) { }

        THostContext IContextContainer<THostContext>.HostContext { get; set; }
        DefaultHttpContext IDefaultHttpContextContainer.HttpContext { get; set; }
    }
}
