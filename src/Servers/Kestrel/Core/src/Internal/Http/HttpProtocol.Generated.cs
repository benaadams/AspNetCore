// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;

using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.AspNetCore.Server.Kestrel.Core.Features;

namespace Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http
{
    internal partial class HttpProtocol : IFeatureCollection
    {
        private static readonly Type IHttpRequestFeatureType = typeof(IHttpRequestFeature);
        private static readonly Type IHttpResponseFeatureType = typeof(IHttpResponseFeature);
        private static readonly Type IHttpResponseBodyFeatureType = typeof(IHttpResponseBodyFeature);
        private static readonly Type IRequestBodyPipeFeatureType = typeof(IRequestBodyPipeFeature);
        private static readonly Type IHttpRequestIdentifierFeatureType = typeof(IHttpRequestIdentifierFeature);
        private static readonly Type IServiceProvidersFeatureType = typeof(IServiceProvidersFeature);
        private static readonly Type IHttpRequestLifetimeFeatureType = typeof(IHttpRequestLifetimeFeature);
        private static readonly Type IHttpConnectionFeatureType = typeof(IHttpConnectionFeature);
        private static readonly Type IRouteValuesFeatureType = typeof(IRouteValuesFeature);
        private static readonly Type IEndpointFeatureType = typeof(IEndpointFeature);
        private static readonly Type IHttpAuthenticationFeatureType = typeof(IHttpAuthenticationFeature);
        private static readonly Type IHttpRequestTrailersFeatureType = typeof(IHttpRequestTrailersFeature);
        private static readonly Type IQueryFeatureType = typeof(IQueryFeature);
        private static readonly Type IFormFeatureType = typeof(IFormFeature);
        private static readonly Type IHttpUpgradeFeatureType = typeof(IHttpUpgradeFeature);
        private static readonly Type IHttp2StreamIdFeatureType = typeof(IHttp2StreamIdFeature);
        private static readonly Type IHttpResponseTrailersFeatureType = typeof(IHttpResponseTrailersFeature);
        private static readonly Type IResponseCookiesFeatureType = typeof(IResponseCookiesFeature);
        private static readonly Type IItemsFeatureType = typeof(IItemsFeature);
        private static readonly Type ITlsConnectionFeatureType = typeof(ITlsConnectionFeature);
        private static readonly Type IHttpWebSocketFeatureType = typeof(IHttpWebSocketFeature);
        private static readonly Type ISessionFeatureType = typeof(ISessionFeature);
        private static readonly Type IHttpMaxRequestBodySizeFeatureType = typeof(IHttpMaxRequestBodySizeFeature);
        private static readonly Type IHttpMinRequestBodyDataRateFeatureType = typeof(IHttpMinRequestBodyDataRateFeature);
        private static readonly Type IHttpMinResponseDataRateFeatureType = typeof(IHttpMinResponseDataRateFeature);
        private static readonly Type IHttpBodyControlFeatureType = typeof(IHttpBodyControlFeature);
        private static readonly Type IHttpResetFeatureType = typeof(IHttpResetFeature);

        private IHttpRequestFeature _currentIHttpRequestFeature;
        private IHttpResponseFeature _currentIHttpResponseFeature;
        private IHttpResponseBodyFeature _currentIHttpResponseBodyFeature;
        private IRequestBodyPipeFeature _currentIRequestBodyPipeFeature;
        private IHttpRequestIdentifierFeature _currentIHttpRequestIdentifierFeature;
        private IServiceProvidersFeature _currentIServiceProvidersFeature;
        private IHttpRequestLifetimeFeature _currentIHttpRequestLifetimeFeature;
        private IHttpConnectionFeature _currentIHttpConnectionFeature;
        private IRouteValuesFeature _currentIRouteValuesFeature;
        private IEndpointFeature _currentIEndpointFeature;
        private IHttpAuthenticationFeature _currentIHttpAuthenticationFeature;
        private IHttpRequestTrailersFeature _currentIHttpRequestTrailersFeature;
        private IQueryFeature _currentIQueryFeature;
        private IFormFeature _currentIFormFeature;
        private IHttpUpgradeFeature _currentIHttpUpgradeFeature;
        private IHttp2StreamIdFeature _currentIHttp2StreamIdFeature;
        private IHttpResponseTrailersFeature _currentIHttpResponseTrailersFeature;
        private IResponseCookiesFeature _currentIResponseCookiesFeature;
        private IItemsFeature _currentIItemsFeature;
        private ITlsConnectionFeature _currentITlsConnectionFeature;
        private IHttpWebSocketFeature _currentIHttpWebSocketFeature;
        private ISessionFeature _currentISessionFeature;
        private IHttpMaxRequestBodySizeFeature _currentIHttpMaxRequestBodySizeFeature;
        private IHttpMinRequestBodyDataRateFeature _currentIHttpMinRequestBodyDataRateFeature;
        private IHttpMinResponseDataRateFeature _currentIHttpMinResponseDataRateFeature;
        private IHttpBodyControlFeature _currentIHttpBodyControlFeature;
        private IHttpResetFeature _currentIHttpResetFeature;

        private int _featureRevision;

        private List<KeyValuePair<Type, object>> MaybeExtra;

        private void FastReset()
        {
            _currentIHttpRequestFeature = this;
            _currentIHttpResponseFeature = this;
            _currentIHttpResponseBodyFeature = this;
            _currentIRequestBodyPipeFeature = this;
            _currentIHttpUpgradeFeature = this;
            _currentIHttpRequestIdentifierFeature = this;
            _currentIHttpRequestLifetimeFeature = this;
            _currentIHttpRequestTrailersFeature = this;
            _currentIHttpConnectionFeature = this;
            _currentIHttpMaxRequestBodySizeFeature = this;
            _currentIHttpBodyControlFeature = this;
            _currentIRouteValuesFeature = this;
            _currentIEndpointFeature = this;

            _currentIServiceProvidersFeature = null;
            _currentIHttpAuthenticationFeature = null;
            _currentIQueryFeature = null;
            _currentIFormFeature = null;
            _currentIHttp2StreamIdFeature = null;
            _currentIHttpResponseTrailersFeature = null;
            _currentIResponseCookiesFeature = null;
            _currentIItemsFeature = null;
            _currentITlsConnectionFeature = null;
            _currentIHttpWebSocketFeature = null;
            _currentISessionFeature = null;
            _currentIHttpMinRequestBodyDataRateFeature = null;
            _currentIHttpMinResponseDataRateFeature = null;
            _currentIHttpResetFeature = null;
        }

        // Internal for testing
        internal void ResetFeatureCollection()
        {
            FastReset();
            MaybeExtra?.Clear();
            _featureRevision++;
        }

        private object ExtraFeatureGet(Type key)
        {
            if (MaybeExtra == null)
            {
                return null;
            }
            for (var i = 0; i < MaybeExtra.Count; i++)
            {
                var kv = MaybeExtra[i];
                if (kv.Key == key)
                {
                    return kv.Value;
                }
            }
            return null;
        }

        private void ExtraFeatureSet(Type key, object value)
        {
            if (MaybeExtra == null)
            {
                MaybeExtra = new List<KeyValuePair<Type, object>>(2);
            }

            for (var i = 0; i < MaybeExtra.Count; i++)
            {
                if (MaybeExtra[i].Key == key)
                {
                    MaybeExtra[i] = new KeyValuePair<Type, object>(key, value);
                    return;
                }
            }
            MaybeExtra.Add(new KeyValuePair<Type, object>(key, value));
        }

        bool IFeatureCollection.IsReadOnly => false;

        int IFeatureCollection.Revision => _featureRevision;

        object IFeatureCollection.this[Type key]
        {
            get
            {
                object feature = null;
                if (key == IHttpRequestFeatureType)
                {
                    feature = _currentIHttpRequestFeature;
                }
                else if (key == IHttpResponseFeatureType)
                {
                    feature = _currentIHttpResponseFeature;
                }
                else if (key == IHttpResponseBodyFeatureType)
                {
                    feature = _currentIHttpResponseBodyFeature;
                }
                else if (key == IRequestBodyPipeFeatureType)
                {
                    feature = _currentIRequestBodyPipeFeature;
                }
                else if (key == IHttpRequestIdentifierFeatureType)
                {
                    feature = _currentIHttpRequestIdentifierFeature;
                }
                else if (key == IServiceProvidersFeatureType)
                {
                    feature = _currentIServiceProvidersFeature;
                }
                else if (key == IHttpRequestLifetimeFeatureType)
                {
                    feature = _currentIHttpRequestLifetimeFeature;
                }
                else if (key == IHttpConnectionFeatureType)
                {
                    feature = _currentIHttpConnectionFeature;
                }
                else if (key == IRouteValuesFeatureType)
                {
                    feature = _currentIRouteValuesFeature;
                }
                else if (key == IEndpointFeatureType)
                {
                    feature = _currentIEndpointFeature;
                }
                else if (key == IHttpAuthenticationFeatureType)
                {
                    feature = _currentIHttpAuthenticationFeature;
                }
                else if (key == IHttpRequestTrailersFeatureType)
                {
                    feature = _currentIHttpRequestTrailersFeature;
                }
                else if (key == IQueryFeatureType)
                {
                    feature = _currentIQueryFeature;
                }
                else if (key == IFormFeatureType)
                {
                    feature = _currentIFormFeature;
                }
                else if (key == IHttpUpgradeFeatureType)
                {
                    feature = _currentIHttpUpgradeFeature;
                }
                else if (key == IHttp2StreamIdFeatureType)
                {
                    feature = _currentIHttp2StreamIdFeature;
                }
                else if (key == IHttpResponseTrailersFeatureType)
                {
                    feature = _currentIHttpResponseTrailersFeature;
                }
                else if (key == IResponseCookiesFeatureType)
                {
                    feature = _currentIResponseCookiesFeature;
                }
                else if (key == IItemsFeatureType)
                {
                    feature = _currentIItemsFeature;
                }
                else if (key == ITlsConnectionFeatureType)
                {
                    feature = _currentITlsConnectionFeature;
                }
                else if (key == IHttpWebSocketFeatureType)
                {
                    feature = _currentIHttpWebSocketFeature;
                }
                else if (key == ISessionFeatureType)
                {
                    feature = _currentISessionFeature;
                }
                else if (key == IHttpMaxRequestBodySizeFeatureType)
                {
                    feature = _currentIHttpMaxRequestBodySizeFeature;
                }
                else if (key == IHttpMinRequestBodyDataRateFeatureType)
                {
                    feature = _currentIHttpMinRequestBodyDataRateFeature;
                }
                else if (key == IHttpMinResponseDataRateFeatureType)
                {
                    feature = _currentIHttpMinResponseDataRateFeature;
                }
                else if (key == IHttpBodyControlFeatureType)
                {
                    feature = _currentIHttpBodyControlFeature;
                }
                else if (key == IHttpResetFeatureType)
                {
                    feature = _currentIHttpResetFeature;
                }
                else if (MaybeExtra != null)
                {
                    feature = ExtraFeatureGet(key);
                }

                return feature ?? ConnectionFeatures[key];
            }

            set
            {
                _featureRevision++;

                if (key == IHttpRequestFeatureType)
                {
                    _currentIHttpRequestFeature = (IHttpRequestFeature)value;
                }
                else if (key == IHttpResponseFeatureType)
                {
                    _currentIHttpResponseFeature = (IHttpResponseFeature)value;
                }
                else if (key == IHttpResponseBodyFeatureType)
                {
                    _currentIHttpResponseBodyFeature = (IHttpResponseBodyFeature)value;
                }
                else if (key == IRequestBodyPipeFeatureType)
                {
                    _currentIRequestBodyPipeFeature = (IRequestBodyPipeFeature)value;
                }
                else if (key == IHttpRequestIdentifierFeatureType)
                {
                    _currentIHttpRequestIdentifierFeature = (IHttpRequestIdentifierFeature)value;
                }
                else if (key == IServiceProvidersFeatureType)
                {
                    _currentIServiceProvidersFeature = (IServiceProvidersFeature)value;
                }
                else if (key == IHttpRequestLifetimeFeatureType)
                {
                    _currentIHttpRequestLifetimeFeature = (IHttpRequestLifetimeFeature)value;
                }
                else if (key == IHttpConnectionFeatureType)
                {
                    _currentIHttpConnectionFeature = (IHttpConnectionFeature)value;
                }
                else if (key == IRouteValuesFeatureType)
                {
                    _currentIRouteValuesFeature = (IRouteValuesFeature)value;
                }
                else if (key == IEndpointFeatureType)
                {
                    _currentIEndpointFeature = (IEndpointFeature)value;
                }
                else if (key == IHttpAuthenticationFeatureType)
                {
                    _currentIHttpAuthenticationFeature = (IHttpAuthenticationFeature)value;
                }
                else if (key == IHttpRequestTrailersFeatureType)
                {
                    _currentIHttpRequestTrailersFeature = (IHttpRequestTrailersFeature)value;
                }
                else if (key == IQueryFeatureType)
                {
                    _currentIQueryFeature = (IQueryFeature)value;
                }
                else if (key == IFormFeatureType)
                {
                    _currentIFormFeature = (IFormFeature)value;
                }
                else if (key == IHttpUpgradeFeatureType)
                {
                    _currentIHttpUpgradeFeature = (IHttpUpgradeFeature)value;
                }
                else if (key == IHttp2StreamIdFeatureType)
                {
                    _currentIHttp2StreamIdFeature = (IHttp2StreamIdFeature)value;
                }
                else if (key == IHttpResponseTrailersFeatureType)
                {
                    _currentIHttpResponseTrailersFeature = (IHttpResponseTrailersFeature)value;
                }
                else if (key == IResponseCookiesFeatureType)
                {
                    _currentIResponseCookiesFeature = (IResponseCookiesFeature)value;
                }
                else if (key == IItemsFeatureType)
                {
                    _currentIItemsFeature = (IItemsFeature)value;
                }
                else if (key == ITlsConnectionFeatureType)
                {
                    _currentITlsConnectionFeature = (ITlsConnectionFeature)value;
                }
                else if (key == IHttpWebSocketFeatureType)
                {
                    _currentIHttpWebSocketFeature = (IHttpWebSocketFeature)value;
                }
                else if (key == ISessionFeatureType)
                {
                    _currentISessionFeature = (ISessionFeature)value;
                }
                else if (key == IHttpMaxRequestBodySizeFeatureType)
                {
                    _currentIHttpMaxRequestBodySizeFeature = (IHttpMaxRequestBodySizeFeature)value;
                }
                else if (key == IHttpMinRequestBodyDataRateFeatureType)
                {
                    _currentIHttpMinRequestBodyDataRateFeature = (IHttpMinRequestBodyDataRateFeature)value;
                }
                else if (key == IHttpMinResponseDataRateFeatureType)
                {
                    _currentIHttpMinResponseDataRateFeature = (IHttpMinResponseDataRateFeature)value;
                }
                else if (key == IHttpBodyControlFeatureType)
                {
                    _currentIHttpBodyControlFeature = (IHttpBodyControlFeature)value;
                }
                else if (key == IHttpResetFeatureType)
                {
                    _currentIHttpResetFeature = (IHttpResetFeature)value;
                }
                else
                {
                    ExtraFeatureSet(key, value);
                }
            }
        }

        TFeature IFeatureCollection.Get<TFeature>()
        {
            TFeature feature = default;
            if (typeof(TFeature) == typeof(IHttpRequestFeature))
            {
                feature = (TFeature)_currentIHttpRequestFeature;
            }
            else if (typeof(TFeature) == typeof(IHttpResponseFeature))
            {
                feature = (TFeature)_currentIHttpResponseFeature;
            }
            else if (typeof(TFeature) == typeof(IHttpResponseBodyFeature))
            {
                feature = (TFeature)_currentIHttpResponseBodyFeature;
            }
            else if (typeof(TFeature) == typeof(IRequestBodyPipeFeature))
            {
                feature = (TFeature)_currentIRequestBodyPipeFeature;
            }
            else if (typeof(TFeature) == typeof(IHttpRequestIdentifierFeature))
            {
                feature = (TFeature)_currentIHttpRequestIdentifierFeature;
            }
            else if (typeof(TFeature) == typeof(IServiceProvidersFeature))
            {
                feature = (TFeature)_currentIServiceProvidersFeature;
            }
            else if (typeof(TFeature) == typeof(IHttpRequestLifetimeFeature))
            {
                feature = (TFeature)_currentIHttpRequestLifetimeFeature;
            }
            else if (typeof(TFeature) == typeof(IHttpConnectionFeature))
            {
                feature = (TFeature)_currentIHttpConnectionFeature;
            }
            else if (typeof(TFeature) == typeof(IRouteValuesFeature))
            {
                feature = (TFeature)_currentIRouteValuesFeature;
            }
            else if (typeof(TFeature) == typeof(IEndpointFeature))
            {
                feature = (TFeature)_currentIEndpointFeature;
            }
            else if (typeof(TFeature) == typeof(IHttpAuthenticationFeature))
            {
                feature = (TFeature)_currentIHttpAuthenticationFeature;
            }
            else if (typeof(TFeature) == typeof(IHttpRequestTrailersFeature))
            {
                feature = (TFeature)_currentIHttpRequestTrailersFeature;
            }
            else if (typeof(TFeature) == typeof(IQueryFeature))
            {
                feature = (TFeature)_currentIQueryFeature;
            }
            else if (typeof(TFeature) == typeof(IFormFeature))
            {
                feature = (TFeature)_currentIFormFeature;
            }
            else if (typeof(TFeature) == typeof(IHttpUpgradeFeature))
            {
                feature = (TFeature)_currentIHttpUpgradeFeature;
            }
            else if (typeof(TFeature) == typeof(IHttp2StreamIdFeature))
            {
                feature = (TFeature)_currentIHttp2StreamIdFeature;
            }
            else if (typeof(TFeature) == typeof(IHttpResponseTrailersFeature))
            {
                feature = (TFeature)_currentIHttpResponseTrailersFeature;
            }
            else if (typeof(TFeature) == typeof(IResponseCookiesFeature))
            {
                feature = (TFeature)_currentIResponseCookiesFeature;
            }
            else if (typeof(TFeature) == typeof(IItemsFeature))
            {
                feature = (TFeature)_currentIItemsFeature;
            }
            else if (typeof(TFeature) == typeof(ITlsConnectionFeature))
            {
                feature = (TFeature)_currentITlsConnectionFeature;
            }
            else if (typeof(TFeature) == typeof(IHttpWebSocketFeature))
            {
                feature = (TFeature)_currentIHttpWebSocketFeature;
            }
            else if (typeof(TFeature) == typeof(ISessionFeature))
            {
                feature = (TFeature)_currentISessionFeature;
            }
            else if (typeof(TFeature) == typeof(IHttpMaxRequestBodySizeFeature))
            {
                feature = (TFeature)_currentIHttpMaxRequestBodySizeFeature;
            }
            else if (typeof(TFeature) == typeof(IHttpMinRequestBodyDataRateFeature))
            {
                feature = (TFeature)_currentIHttpMinRequestBodyDataRateFeature;
            }
            else if (typeof(TFeature) == typeof(IHttpMinResponseDataRateFeature))
            {
                feature = (TFeature)_currentIHttpMinResponseDataRateFeature;
            }
            else if (typeof(TFeature) == typeof(IHttpBodyControlFeature))
            {
                feature = (TFeature)_currentIHttpBodyControlFeature;
            }
            else if (typeof(TFeature) == typeof(IHttpResetFeature))
            {
                feature = (TFeature)_currentIHttpResetFeature;
            }
            else if (MaybeExtra != null)
            {
                feature = (TFeature)(ExtraFeatureGet(typeof(TFeature)));
            }

            if (feature == null)
            {
                feature = ConnectionFeatures.Get<TFeature>();
            }

            return feature;
        }

        void IFeatureCollection.Set<TFeature>(TFeature feature)
        {
            _featureRevision++;
            if (typeof(TFeature) == typeof(IHttpRequestFeature))
            {
                _currentIHttpRequestFeature = (IHttpRequestFeature)feature;
            }
            else if (typeof(TFeature) == typeof(IHttpResponseFeature))
            {
                _currentIHttpResponseFeature = (IHttpResponseFeature)feature;
            }
            else if (typeof(TFeature) == typeof(IHttpResponseBodyFeature))
            {
                _currentIHttpResponseBodyFeature = (IHttpResponseBodyFeature)feature;
            }
            else if (typeof(TFeature) == typeof(IRequestBodyPipeFeature))
            {
                _currentIRequestBodyPipeFeature = (IRequestBodyPipeFeature)feature;
            }
            else if (typeof(TFeature) == typeof(IHttpRequestIdentifierFeature))
            {
                _currentIHttpRequestIdentifierFeature = (IHttpRequestIdentifierFeature)feature;
            }
            else if (typeof(TFeature) == typeof(IServiceProvidersFeature))
            {
                _currentIServiceProvidersFeature = (IServiceProvidersFeature)feature;
            }
            else if (typeof(TFeature) == typeof(IHttpRequestLifetimeFeature))
            {
                _currentIHttpRequestLifetimeFeature = (IHttpRequestLifetimeFeature)feature;
            }
            else if (typeof(TFeature) == typeof(IHttpConnectionFeature))
            {
                _currentIHttpConnectionFeature = (IHttpConnectionFeature)feature;
            }
            else if (typeof(TFeature) == typeof(IRouteValuesFeature))
            {
                _currentIRouteValuesFeature = (IRouteValuesFeature)feature;
            }
            else if (typeof(TFeature) == typeof(IEndpointFeature))
            {
                _currentIEndpointFeature = (IEndpointFeature)feature;
            }
            else if (typeof(TFeature) == typeof(IHttpAuthenticationFeature))
            {
                _currentIHttpAuthenticationFeature = (IHttpAuthenticationFeature)feature;
            }
            else if (typeof(TFeature) == typeof(IHttpRequestTrailersFeature))
            {
                _currentIHttpRequestTrailersFeature = (IHttpRequestTrailersFeature)feature;
            }
            else if (typeof(TFeature) == typeof(IQueryFeature))
            {
                _currentIQueryFeature = (IQueryFeature)feature;
            }
            else if (typeof(TFeature) == typeof(IFormFeature))
            {
                _currentIFormFeature = (IFormFeature)feature;
            }
            else if (typeof(TFeature) == typeof(IHttpUpgradeFeature))
            {
                _currentIHttpUpgradeFeature = (IHttpUpgradeFeature)feature;
            }
            else if (typeof(TFeature) == typeof(IHttp2StreamIdFeature))
            {
                _currentIHttp2StreamIdFeature = (IHttp2StreamIdFeature)feature;
            }
            else if (typeof(TFeature) == typeof(IHttpResponseTrailersFeature))
            {
                _currentIHttpResponseTrailersFeature = (IHttpResponseTrailersFeature)feature;
            }
            else if (typeof(TFeature) == typeof(IResponseCookiesFeature))
            {
                _currentIResponseCookiesFeature = (IResponseCookiesFeature)feature;
            }
            else if (typeof(TFeature) == typeof(IItemsFeature))
            {
                _currentIItemsFeature = (IItemsFeature)feature;
            }
            else if (typeof(TFeature) == typeof(ITlsConnectionFeature))
            {
                _currentITlsConnectionFeature = (ITlsConnectionFeature)feature;
            }
            else if (typeof(TFeature) == typeof(IHttpWebSocketFeature))
            {
                _currentIHttpWebSocketFeature = (IHttpWebSocketFeature)feature;
            }
            else if (typeof(TFeature) == typeof(ISessionFeature))
            {
                _currentISessionFeature = (ISessionFeature)feature;
            }
            else if (typeof(TFeature) == typeof(IHttpMaxRequestBodySizeFeature))
            {
                _currentIHttpMaxRequestBodySizeFeature = (IHttpMaxRequestBodySizeFeature)feature;
            }
            else if (typeof(TFeature) == typeof(IHttpMinRequestBodyDataRateFeature))
            {
                _currentIHttpMinRequestBodyDataRateFeature = (IHttpMinRequestBodyDataRateFeature)feature;
            }
            else if (typeof(TFeature) == typeof(IHttpMinResponseDataRateFeature))
            {
                _currentIHttpMinResponseDataRateFeature = (IHttpMinResponseDataRateFeature)feature;
            }
            else if (typeof(TFeature) == typeof(IHttpBodyControlFeature))
            {
                _currentIHttpBodyControlFeature = (IHttpBodyControlFeature)feature;
            }
            else if (typeof(TFeature) == typeof(IHttpResetFeature))
            {
                _currentIHttpResetFeature = (IHttpResetFeature)feature;
            }
            else
            {
                ExtraFeatureSet(typeof(TFeature), feature);
            }
        }

        private IEnumerable<KeyValuePair<Type, object>> FastEnumerable()
        {
            if (_currentIHttpRequestFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IHttpRequestFeatureType, _currentIHttpRequestFeature);
            }
            if (_currentIHttpResponseFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IHttpResponseFeatureType, _currentIHttpResponseFeature);
            }
            if (_currentIHttpResponseBodyFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IHttpResponseBodyFeatureType, _currentIHttpResponseBodyFeature);
            }
            if (_currentIRequestBodyPipeFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IRequestBodyPipeFeatureType, _currentIRequestBodyPipeFeature);
            }
            if (_currentIHttpRequestIdentifierFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IHttpRequestIdentifierFeatureType, _currentIHttpRequestIdentifierFeature);
            }
            if (_currentIServiceProvidersFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IServiceProvidersFeatureType, _currentIServiceProvidersFeature);
            }
            if (_currentIHttpRequestLifetimeFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IHttpRequestLifetimeFeatureType, _currentIHttpRequestLifetimeFeature);
            }
            if (_currentIHttpConnectionFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IHttpConnectionFeatureType, _currentIHttpConnectionFeature);
            }
            if (_currentIRouteValuesFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IRouteValuesFeatureType, _currentIRouteValuesFeature);
            }
            if (_currentIEndpointFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IEndpointFeatureType, _currentIEndpointFeature);
            }
            if (_currentIHttpAuthenticationFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IHttpAuthenticationFeatureType, _currentIHttpAuthenticationFeature);
            }
            if (_currentIHttpRequestTrailersFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IHttpRequestTrailersFeatureType, _currentIHttpRequestTrailersFeature);
            }
            if (_currentIQueryFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IQueryFeatureType, _currentIQueryFeature);
            }
            if (_currentIFormFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IFormFeatureType, _currentIFormFeature);
            }
            if (_currentIHttpUpgradeFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IHttpUpgradeFeatureType, _currentIHttpUpgradeFeature);
            }
            if (_currentIHttp2StreamIdFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IHttp2StreamIdFeatureType, _currentIHttp2StreamIdFeature);
            }
            if (_currentIHttpResponseTrailersFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IHttpResponseTrailersFeatureType, _currentIHttpResponseTrailersFeature);
            }
            if (_currentIResponseCookiesFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IResponseCookiesFeatureType, _currentIResponseCookiesFeature);
            }
            if (_currentIItemsFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IItemsFeatureType, _currentIItemsFeature);
            }
            if (_currentITlsConnectionFeature != null)
            {
                yield return new KeyValuePair<Type, object>(ITlsConnectionFeatureType, _currentITlsConnectionFeature);
            }
            if (_currentIHttpWebSocketFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IHttpWebSocketFeatureType, _currentIHttpWebSocketFeature);
            }
            if (_currentISessionFeature != null)
            {
                yield return new KeyValuePair<Type, object>(ISessionFeatureType, _currentISessionFeature);
            }
            if (_currentIHttpMaxRequestBodySizeFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IHttpMaxRequestBodySizeFeatureType, _currentIHttpMaxRequestBodySizeFeature);
            }
            if (_currentIHttpMinRequestBodyDataRateFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IHttpMinRequestBodyDataRateFeatureType, _currentIHttpMinRequestBodyDataRateFeature);
            }
            if (_currentIHttpMinResponseDataRateFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IHttpMinResponseDataRateFeatureType, _currentIHttpMinResponseDataRateFeature);
            }
            if (_currentIHttpBodyControlFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IHttpBodyControlFeatureType, _currentIHttpBodyControlFeature);
            }
            if (_currentIHttpResetFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IHttpResetFeatureType, _currentIHttpResetFeature);
            }

            if (MaybeExtra != null)
            {
                foreach (var item in MaybeExtra)
                {
                    yield return item;
                }
            }
        }

        IEnumerator<KeyValuePair<Type, object>> IEnumerable<KeyValuePair<Type, object>>.GetEnumerator() => FastEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => FastEnumerable().GetEnumerator();
    }
}
