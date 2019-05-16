// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Server.HttpSys
{
    using Microsoft.AspNetCore.HttpSys.Internal;

    internal class FeatureContext<TContext> :
        IHttpRequestFeature,
        IHttpConnectionFeature,
        IHttpResponseFeature,
        IHttpSendFileFeature,
        ITlsConnectionFeature,
        ITlsHandshakeFeature,
        // ITlsTokenBindingFeature, TODO: https://github.com/aspnet/HttpSysServer/issues/231
        IHttpBufferingFeature,
        IHttpRequestLifetimeFeature,
        IHttpAuthenticationFeature,
        IHttpUpgradeFeature,
        IHttpRequestIdentifierFeature,
        IHttpMaxRequestBodySizeFeature,
        IHttpBodyControlFeature
    {
        private readonly RequestContext _requestContext;
        private readonly FeatureCollection<TContext> _features;
        private readonly Func<Task> _onResponseStart;

        private bool _enableResponseCaching;
        private Stream _requestBody;
        private IHeaderDictionary _requestHeaders;
        private string _scheme;
        private string _httpMethod;
        private string _httpProtocolVersion;
        private string _query;
        private string _pathBase;
        private string _path;
        private string _rawTarget;
        private IPAddress _remoteIpAddress;
        private IPAddress _localIpAddress;
        private int _remotePort;
        private int _localPort;
        private string _connectionId;
        private string _traceIdentitfier;
        private X509Certificate2 _clientCert;
        private ClaimsPrincipal _user;
        private CancellationToken _disconnectToken;
        private Stream _responseStream;
        private IHeaderDictionary _responseHeaders;

        private Fields _initializedFields;

        private readonly List<(Func<object, Task> callback, object state)> _onStartingActions = new List<(Func<object, Task> callback, object state)>();
        private readonly List<(Func<object, Task> callback, object state)> _onCompletedActions = new List<(Func<object, Task> callback, object state)>();
        private bool _responseStarted;
        private bool _completed;

        internal FeatureContext(RequestContext requestContext)
        {
            _requestContext = requestContext;
            _features = new FeatureCollection<TContext>(new StandardFeatureCollection<TContext>(this));
            _onResponseStart = OnResponseStart;

            Initialize();
        }

        internal void Initialize()
        {
            // Pre-initialize any fields that are not lazy at the lower level.
            _requestHeaders = Request.Headers;
            _scheme = Request.Scheme;
            _httpMethod = Request.Method;

            _query = Request.QueryString;
            _pathBase = Request.PathBase;
            _path = Request.Path;
            _rawTarget = Request.RawUrl;

            _user = _requestContext.User;

            _responseStream = new ResponseStream(_requestContext.Response.Body, _onResponseStart);
            _responseHeaders = Response.Headers;

            _initializedFields = Fields.None;

            _enableResponseCaching = _requestContext.Server.Options.EnableResponseCaching;
        }

        internal void Reset()
        {
            // Reset fields in sequential order according to layout for better memory/cache access behaviour
            /*
            Type layout for 'FeatureContext`1'
            Size: 200 bytes. Paddings: 1 bytes (%0 of empty space)
            |=======================================================|
            | Object Header (8 bytes)                               |
            |-------------------------------------------------------|
            | Method Table Ptr (8 bytes)                            |
            |=======================================================|
            |   0-7: RequestContext _requestContext (8 bytes)       |
            |-------------------------------------------------------|
            |  8-15: FeatureCollection`1 _features (8 bytes)        |
            |-------------------------------------------------------|
            | 16-23: Func`1 _onResponseStart (8 bytes)              |
            |-------------------------------------------------------|
            | 24-31: Stream _requestBody (8 bytes)                  |
            |-------------------------------------------------------|
            | 32-39: IHeaderDictionary _requestHeaders (8 bytes)    |
            |-------------------------------------------------------|
            | 40-47: String _scheme (8 bytes)                       |
            |-------------------------------------------------------|
            | 48-55: String _httpMethod (8 bytes)                   |
            |-------------------------------------------------------|
            | 56-63: String _httpProtocolVersion (8 bytes)          |
            |-------------------------------------------------------|
            | 64-71: String _query (8 bytes)                        |
            |-------------------------------------------------------|
            | 72-79: String _pathBase (8 bytes)                     |
            |-------------------------------------------------------|
            | 80-87: String _path (8 bytes)                         |
            |-------------------------------------------------------|
            | 88-95: String _rawTarget (8 bytes)                    |
            |-------------------------------------------------------|
            | 96-103: IPAddress _remoteIpAddress (8 bytes)          |
            |-------------------------------------------------------|
            | 104-111: IPAddress _localIpAddress (8 bytes)          |
            |-------------------------------------------------------|
            | 112-119: String _connectionId (8 bytes)               |
            |-------------------------------------------------------|
            | 120-127: String _traceIdentitfier (8 bytes)           |
            |-------------------------------------------------------|
            | 128-135: X509Certificate2 _clientCert (8 bytes)       |
            |-------------------------------------------------------|
            | 136-143: ClaimsPrincipal _user (8 bytes)              |
            |-------------------------------------------------------|
            | 144-151: Stream _responseStream (8 bytes)             |
            |-------------------------------------------------------|
            | 152-159: IHeaderDictionary _responseHeaders (8 bytes) |
            |-------------------------------------------------------|
            | 160-167: List`1 _onStartingActions (8 bytes)          |
            |-------------------------------------------------------|
            | 168-175: List`1 _onCompletedActions (8 bytes)         |
            |-------------------------------------------------------|
            | 176-179: Int32 _remotePort (4 bytes)                  |
            |-------------------------------------------------------|
            | 180-183: Int32 _localPort (4 bytes)                   |
            |-------------------------------------------------------|
            | 184-187: Fields _initializedFields (4 bytes)          |
            |-------------------------------------------------------|
            |   188: Boolean _enableResponseCaching (1 byte)        |
            |-------------------------------------------------------|
            |   189: Boolean _responseStarted (1 byte)              |
            |-------------------------------------------------------|
            |   190: Boolean _completed (1 byte)                    |
            |-------------------------------------------------------|
            |   191: padding (1 byte)                               |
            |-------------------------------------------------------|
            | 192-199: CancellationToken _disconnectToken (8 bytes) |
            |=======================================================|
            */

            // RequestContext - kept
            _features.Reset();
            // _onResponseStart - kept
            _requestBody = null;
            _requestHeaders = null;
            _scheme = null;
            _httpMethod = null;
            _httpProtocolVersion = null;
            _query = null;
            _pathBase = null;
            _path = null;
            _rawTarget = null;
            _remoteIpAddress = null;
            _localIpAddress = null;
            _connectionId = null;
            _traceIdentitfier = null;
            _clientCert = null;
            _user = null;
            _responseStream = null;
            _responseHeaders = null;
            _onStartingActions.Clear();
            _onCompletedActions.Clear();
            _remotePort = 0;
            _localPort = 0;
            _initializedFields = Fields.None;
            _enableResponseCaching = false;
            _responseStarted = false;
            _completed = false;
            _disconnectToken = default;
        }

        internal IFeatureCollection Features => _features;

        internal object RequestContext => _requestContext;

        private Request Request => _requestContext.Request;

        private Response Response => _requestContext.Response;

        [Flags]
        // Fields that may be lazy-initialized
        private enum Fields
        {
            None = 0x0,
            Protocol = 0x1,
            RequestBody = 0x2,
            RequestAborted = 0x4,
            LocalIpAddress = 0x8,
            RemoteIpAddress = 0x10,
            LocalPort = 0x20,
            RemotePort = 0x40,
            ConnectionId = 0x80,
            ClientCertificate = 0x100,
            TraceIdentifier = 0x200,
        }

        private bool IsNotInitialized(Fields field)
        {
            return (_initializedFields & field) != field;
        }

        private void SetInitialized(Fields field)
        {
            _initializedFields |= field;
        }

        Stream IHttpRequestFeature.Body
        {
            get
            {
                if (IsNotInitialized(Fields.RequestBody))
                {
                    _requestBody = Request.Body;
                    SetInitialized(Fields.RequestBody);
                }
                return _requestBody;
            }
            set
            {
                _requestBody = value;
                SetInitialized(Fields.RequestBody);
            }
        }

        IHeaderDictionary IHttpRequestFeature.Headers
        {
            get { return _requestHeaders; }
            set { _requestHeaders = value; }
        }

        string IHttpRequestFeature.Method
        {
            get { return _httpMethod; }
            set { _httpMethod = value; }
        }

        string IHttpRequestFeature.Path
        {
            get { return _path; }
            set { _path = value; }
        }

        string IHttpRequestFeature.PathBase
        {
            get { return _pathBase; }
            set { _pathBase = value; }
        }

        string IHttpRequestFeature.Protocol
        {
            get
            {
                if (IsNotInitialized(Fields.Protocol))
                {
                    var protocol = Request.ProtocolVersion;
                    if (protocol == Constants.V2)
                    {
                        _httpProtocolVersion = "HTTP/2";
                    }
                    else if (protocol == Constants.V1_1)
                    {
                        _httpProtocolVersion = "HTTP/1.1";
                    }
                    else if (protocol == Constants.V1_0)
                    {
                        _httpProtocolVersion = "HTTP/1.0";
                    }
                    else
                    {
                        _httpProtocolVersion = "HTTP/" + protocol.ToString(2);
                    }
                    SetInitialized(Fields.Protocol);
                }
                return _httpProtocolVersion;
            }
            set
            {
                _httpProtocolVersion = value;
                SetInitialized(Fields.Protocol);
            }
        }

        string IHttpRequestFeature.QueryString
        {
            get { return _query; }
            set { _query = value; }
        }

        string IHttpRequestFeature.RawTarget
        {
            get { return _rawTarget; }
            set { _rawTarget = value; }
        }

        string IHttpRequestFeature.Scheme
        {
            get { return _scheme; }
            set { _scheme = value; }
        }

        IPAddress IHttpConnectionFeature.LocalIpAddress
        {
            get
            {
                if (IsNotInitialized(Fields.LocalIpAddress))
                {
                    _localIpAddress = Request.LocalIpAddress;
                    SetInitialized(Fields.LocalIpAddress);
                }
                return _localIpAddress;
            }
            set
            {
                _localIpAddress = value;
                SetInitialized(Fields.LocalIpAddress);
            }
        }

        IPAddress IHttpConnectionFeature.RemoteIpAddress
        {
            get
            {
                if (IsNotInitialized(Fields.RemoteIpAddress))
                {
                    _remoteIpAddress = Request.RemoteIpAddress;
                    SetInitialized(Fields.RemoteIpAddress);
                }
                return _remoteIpAddress;
            }
            set
            {
                _remoteIpAddress = value;
                SetInitialized(Fields.RemoteIpAddress);
            }
        }

        int IHttpConnectionFeature.LocalPort
        {
            get
            {
                if (IsNotInitialized(Fields.LocalPort))
                {
                    _localPort = Request.LocalPort;
                    SetInitialized(Fields.LocalPort);
                }
                return _localPort;
            }
            set
            {
                _localPort = value;
                SetInitialized(Fields.LocalPort);
            }
        }

        int IHttpConnectionFeature.RemotePort
        {
            get
            {
                if (IsNotInitialized(Fields.RemotePort))
                {
                    _remotePort = Request.RemotePort;
                    SetInitialized(Fields.RemotePort);
                }
                return _remotePort;
            }
            set
            {
                _remotePort = value;
                SetInitialized(Fields.RemotePort);
            }
        }

        string IHttpConnectionFeature.ConnectionId
        {
            get
            {
                if (IsNotInitialized(Fields.ConnectionId))
                {
                    _connectionId = Request.ConnectionId.ToString(CultureInfo.InvariantCulture);
                    SetInitialized(Fields.ConnectionId);
                }
                return _connectionId;
            }
            set
            {
                _connectionId = value;
                SetInitialized(Fields.ConnectionId);
            }
        }

        X509Certificate2 ITlsConnectionFeature.ClientCertificate
        {
            get
            {
                if (IsNotInitialized(Fields.ClientCertificate))
                {
                    _clientCert = Request.GetClientCertificateAsync().Result; // TODO: Sync;
                    SetInitialized(Fields.ClientCertificate);
                }
                return _clientCert;
            }
            set
            {
                _clientCert = value;
                SetInitialized(Fields.ClientCertificate);
            }
        }

        async Task<X509Certificate2> ITlsConnectionFeature.GetClientCertificateAsync(CancellationToken cancellationToken)
        {
            if (IsNotInitialized(Fields.ClientCertificate))
            {
                _clientCert = await Request.GetClientCertificateAsync(cancellationToken);
                SetInitialized(Fields.ClientCertificate);
            }
            return _clientCert;
        }

        internal ITlsConnectionFeature GetTlsConnectionFeature()
        {
            return Request.IsHttps ? this : null;
        }

        internal ITlsHandshakeFeature GetTlsHandshakeFeature()
        {
            return Request.IsHttps ? this : null;
        }

        /* TODO: https://github.com/aspnet/HttpSysServer/issues/231
        byte[] ITlsTokenBindingFeature.GetProvidedTokenBindingId() => Request.GetProvidedTokenBindingId();

        byte[] ITlsTokenBindingFeature.GetReferredTokenBindingId() => Request.GetReferredTokenBindingId();

        internal ITlsTokenBindingFeature GetTlsTokenBindingFeature()
        {
            return Request.IsHttps ? this : null;
        }
        */
        void IHttpBufferingFeature.DisableRequestBuffering()
        {
            // There is no request buffering.
        }

        void IHttpBufferingFeature.DisableResponseBuffering()
        {
            // TODO: What about native buffering?
        }

        Stream IHttpResponseFeature.Body
        {
            get { return _responseStream; }
            set { _responseStream = value; }
        }

        IHeaderDictionary IHttpResponseFeature.Headers
        {
            get { return _responseHeaders; }
            set { _responseHeaders = value; }
        }

        bool IHttpResponseFeature.HasStarted => Response.HasStarted;

        void IHttpResponseFeature.OnStarting(Func<object, Task> callback, object state)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }
            if (_responseStarted)
            {
                throw new InvalidOperationException("Cannot register new callbacks, the response has already started.");
            }

            _onStartingActions.Add((callback, state));
        }

        void IHttpResponseFeature.OnCompleted(Func<object, Task> callback, object state)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }
            if (_completed)
            {
                throw new InvalidOperationException("Cannot register new callbacks, the response has already completed.");
            }

            _onCompletedActions.Add((callback, state));
        }

        string IHttpResponseFeature.ReasonPhrase
        {
            get { return Response.ReasonPhrase; }
            set { Response.ReasonPhrase = value; }
        }

        int IHttpResponseFeature.StatusCode
        {
            get { return Response.StatusCode; }
            set { Response.StatusCode = value; }
        }

        async Task IHttpSendFileFeature.SendFileAsync(string path, long offset, long? length, CancellationToken cancellation)
        {
            await OnResponseStart();
            await Response.SendFileAsync(path, offset, length, cancellation);
        }

        CancellationToken IHttpRequestLifetimeFeature.RequestAborted
        {
            get
            {
                if (IsNotInitialized(Fields.RequestAborted))
                {
                    _disconnectToken = _requestContext.DisconnectToken;
                    SetInitialized(Fields.RequestAborted);
                }
                return _disconnectToken;
            }
            set
            {
                _disconnectToken = value;
                SetInitialized(Fields.RequestAborted);
            }
        }

        void IHttpRequestLifetimeFeature.Abort() => _requestContext.Abort();

        bool IHttpUpgradeFeature.IsUpgradableRequest => _requestContext.IsUpgradableRequest;

        async Task<Stream> IHttpUpgradeFeature.UpgradeAsync()
        {
            await OnResponseStart();
            return await _requestContext.UpgradeAsync();
        }

        ClaimsPrincipal IHttpAuthenticationFeature.User
        {
            get { return _user; }
            set { _user = value; }
        }

        string IHttpRequestIdentifierFeature.TraceIdentifier
        {
            get
            {
                if (IsNotInitialized(Fields.TraceIdentifier))
                {
                    _traceIdentitfier = _requestContext.TraceIdentifier.ToString();
                    SetInitialized(Fields.TraceIdentifier);
                }
                return _traceIdentitfier;
            }
            set
            {
                _traceIdentitfier = value;
                SetInitialized(Fields.TraceIdentifier);
            }
        }

        bool IHttpBodyControlFeature.AllowSynchronousIO
        {
            get => _requestContext.AllowSynchronousIO;
            set => _requestContext.AllowSynchronousIO = value;
        }

        bool IHttpMaxRequestBodySizeFeature.IsReadOnly => Request.HasRequestBodyStarted;

        long? IHttpMaxRequestBodySizeFeature.MaxRequestBodySize
        {
            get => Request.MaxRequestBodySize;
            set => Request.MaxRequestBodySize = value;
        }

        SslProtocols ITlsHandshakeFeature.Protocol => Request.Protocol;

        CipherAlgorithmType ITlsHandshakeFeature.CipherAlgorithm => Request.CipherAlgorithm;

        int ITlsHandshakeFeature.CipherStrength => Request.CipherStrength;

        HashAlgorithmType ITlsHandshakeFeature.HashAlgorithm => Request.HashAlgorithm;

        int ITlsHandshakeFeature.HashStrength => Request.HashStrength;

        ExchangeAlgorithmType ITlsHandshakeFeature.KeyExchangeAlgorithm => Request.KeyExchangeAlgorithm;

        int ITlsHandshakeFeature.KeyExchangeStrength => Request.KeyExchangeStrength;

        internal async Task OnResponseStart()
        {
            if (_responseStarted)
            {
                return;
            }
            _responseStarted = true;
            await NotifiyOnStartingAsync();
            ConsiderEnablingResponseCache();
        }

        private Task NotifiyOnStartingAsync()
        {
            var actions = _onStartingActions;

            if (actions.Count == 0)
            {
                return Task.CompletedTask;
            }

            return Awaited(actions);

            static async Task Awaited(List<(Func<object, Task> callback, object state)> actions)
            {
                actions.Reverse();
                // Execute last to first. This mimics a stack unwind.
                foreach (var actionPair in actions)
                {
                    await actionPair.callback(actionPair.state);
                }
            }
        }

        private void ConsiderEnablingResponseCache()
        {
            if (_enableResponseCaching)
            {
                // We don't have to worry too much about what Http.Sys supports, caching is a best-effort feature.
                // If there's something about the request or response that prevents it from caching then the response
                // will complete normally without caching.
                _requestContext.Response.CacheTtl = GetCacheTtl(_requestContext);
            }
        }

        private static TimeSpan? GetCacheTtl(RequestContext requestContext)
        {
            var response = requestContext.Response;
            // Only consider kernel-mode caching if the Cache-Control response header is present.
            var cacheControlHeader = response.Headers[HeaderNames.CacheControl];
            if (string.IsNullOrEmpty(cacheControlHeader))
            {
                return null;
            }

            // Before we check the header value, check for the existence of other headers which would
            // make us *not* want to cache the response.
            if (response.Headers.ContainsKey(HeaderNames.SetCookie)
                || response.Headers.ContainsKey(HeaderNames.Vary)
                || response.Headers.ContainsKey(HeaderNames.Pragma))
            {
                return null;
            }

            // We require 'public' and 's-max-age' or 'max-age' or the Expires header.
            CacheControlHeaderValue cacheControl;
            if (CacheControlHeaderValue.TryParse(cacheControlHeader.ToString(), out cacheControl) && cacheControl.Public)
            {
                if (cacheControl.SharedMaxAge.HasValue)
                {
                    return cacheControl.SharedMaxAge;
                }
                else if (cacheControl.MaxAge.HasValue)
                {
                    return cacheControl.MaxAge;
                }

                DateTimeOffset expirationDate;
                if (HeaderUtilities.TryParseDate(response.Headers[HeaderNames.Expires].ToString(), out expirationDate))
                {
                    var expiresOffset = expirationDate - DateTimeOffset.UtcNow;
                    if (expiresOffset > TimeSpan.Zero)
                    {
                        return expiresOffset;
                    }
                }
            }

            return null;
        }

        internal Task OnCompleted()
        {
            if (_completed)
            {
                return Task.CompletedTask;
            }
            _completed = true;
            return NotifyOnCompletedAsync();
        }

        private Task NotifyOnCompletedAsync()
        {
            var actions = _onCompletedActions;
            if (actions.Count == 0)
            {
                return Task.CompletedTask;
            }

            return Awaited(actions);

            static async Task Awaited(List<(Func<object, Task> callback, object state)> actions)
            {
                actions.Reverse();
                // Execute last to first. This mimics a stack unwind.
                foreach (var actionPair in actions)
                {
                    await actionPair.callback(actionPair.state);
                }
            }
        }
    }
}
