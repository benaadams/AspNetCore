// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.AspNetCore.HttpSys.Internal
{
    // From src/Http/Http.Features/src/FeatureCollection.cs
    // With the addition of Initialize, Reset, IDefaultHttpContextContainer and IHostContextContainer<TContext>
    internal sealed class FeatureCollection<TContext> : IFeatureCollection, IDefaultHttpContextContainer, IHostContextContainer<TContext>
    {
        private static KeyComparer FeatureKeyComparer = new KeyComparer();

        private IFeatureCollection _defaults;
        private IDictionary<Type, object> _features;
        private volatile int _containerRevision;

        private TContext _context;

        public FeatureCollection(IFeatureCollection defaults)
        {
            Initialize(defaults);
        }

        public void Initialize(IFeatureCollection defaults)
        {
            _defaults = defaults;
        }

        public void Reset()
        {
            _containerRevision = 0;
            _features?.Clear();
        }

        public int Revision
        {
            get { return _containerRevision + (_defaults?.Revision ?? 0); }
        }

        public bool IsReadOnly { get { return false; } }

        public object this[Type key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                object result;
                return _features != null && _features.TryGetValue(key, out result) ? result : _defaults?[key];
            }
            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                if (value == null)
                {
                    if (_features != null && _features.Remove(key))
                    {
                        _containerRevision++;
                    }
                    return;
                }

                if (_features == null)
                {
                    _features = new Dictionary<Type, object>();
                }
                _features[key] = value;
                _containerRevision++;
            }
        }

        DefaultHttpContext IDefaultHttpContextContainer.HttpContext { get; set; }

        ref TContext IHostContextContainer<TContext>.HostContext => ref _context;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<Type, object>> GetEnumerator()
        {
            if (_features != null)
            {
                foreach (var pair in _features)
                {
                    yield return pair;
                }
            }

            if (_defaults != null)
            {
                // Don't return features masked by the wrapper.
                foreach (var pair in _features == null ? _defaults : _defaults.Except(_features, FeatureKeyComparer))
                {
                    yield return pair;
                }
            }
        }

        public TFeature Get<TFeature>()
        {
            return (TFeature)this[typeof(TFeature)];
        }

        public void Set<TFeature>(TFeature instance)
        {
            this[typeof(TFeature)] = instance;
        }

        private class KeyComparer : IEqualityComparer<KeyValuePair<Type, object>>
        {
            public bool Equals(KeyValuePair<Type, object> x, KeyValuePair<Type, object> y)
            {
                return x.Key.Equals(y.Key);
            }

            public int GetHashCode(KeyValuePair<Type, object> obj)
            {
                return obj.Key.GetHashCode();
            }
        }
    }
}
