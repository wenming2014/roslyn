// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.PooledObjects
{
    internal partial class FrozenDictionary<TKey, TValue>
    {
        public static Builder GetBuilder(IEqualityComparer<TKey> comparer) => new Builder(comparer);

        public sealed class Builder : IDictionary<TKey, TValue>
        {
            public FrozenDictionary<TKey, TValue> ToImmutable() => new FrozenDictionary<TKey, TValue>(_dict);

            private readonly Dictionary<TKey, TValue> _dict;

            public Builder(IEqualityComparer<TKey> comparer)
            {
                _dict = new Dictionary<TKey, TValue>(comparer);
            }

            public TValue this[TKey key]
            {
                get => _dict[key];
                set => _dict[key] = value;
            }

            public ICollection<TKey> Keys => _dict.Keys;

            public ICollection<TValue> Values => _dict.Values;

            public int Count => _dict.Count;

            public bool IsReadOnly => false;

            public void Add(TKey key, TValue value)
            {
                _dict.Add(key, value);
            }

            public void Add(KeyValuePair<TKey, TValue> item)
            {
                ((IDictionary<TKey, TValue>)_dict).Add(item);
            }

            public void Clear()
            {
                _dict.Clear();
            }

            public bool Contains(KeyValuePair<TKey, TValue> item) => ((IDictionary<TKey, TValue>)_dict).Contains(item);

            public bool ContainsKey(TKey key) => _dict.ContainsKey(key);

            public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            {
                ((IDictionary<TKey, TValue>)_dict).CopyTo(array, arrayIndex);
            }

            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dict.GetEnumerator();

            public bool Remove(TKey key) => _dict.Remove(key);

            public bool Remove(KeyValuePair<TKey, TValue> item)
            {
                return ((IDictionary<TKey, TValue>)_dict).Remove(item);
            }

            public bool TryGetValue(TKey key, out TValue value) => _dict.TryGetValue(key, out value);

            IEnumerator IEnumerable.GetEnumerator() => _dict.GetEnumerator();
        }
    }
}
