// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http2
{
    internal sealed class Http2Stream<THostContext> : Http2Stream, IContextContainer<THostContext>
    {
        private readonly IHttpApplication<THostContext> _application;

        public Http2Stream(IHttpApplication<THostContext> application, Http2StreamContext context) : base(context)
        {
            _application = application;
        }

        public override void Execute()
        {
            // REVIEW: Should we store this in a field for easy debugging?
            _ = ProcessRequestsAsync(_application);
        }

        // Pooled Host context
        THostContext IContextContainer<THostContext>.HostContext { get; set; }
        DefaultHttpContext IDefaultHttpContextContainer.HttpContext { get; set; }
    }
}
