// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.PooledObjects
{
    internal sealed partial class FrozenDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        private readonly Entry[] _entries;

        private struct Entry
        {
            public TKey Key;
            public TValue Value;
            public uint Hash;
        }

        public int Count { get; }

        private FrozenDictionary(Dictionary<TKey, TValue> dict)
        {
            Count = dict.Count;
            uint capacity = (uint)(Count / 0.9);
            _entries = new Entry[capacity];

            foreach (var kvp in dict)
            {
                insert(kvp.Key, kvp.Value);
            }

            void insert(TKey key, TValue value)
            {
                uint hash = getHash(key);
                uint slot = hash % capacity;
                uint distance = 0;
                while (true)
                {
                    if (_entries[slot].Hash == 0)
                    {
                        _entries[slot].Key = key;
                        _entries[slot].Value = value;
                        _entries[slot].Hash = hash;
                        return;
                    }

                    uint existingDistance = probeDistance(_entries[slot].Hash, slot);
                    if (existingDistance < distance)
                    {
                        swap(ref hash, ref _entries[slot].Hash);
                        swap(ref key, ref _entries[slot].Key);
                        swap(ref value, ref _entries[slot].Value);
                        distance = existingDistance;
                    }

                    slot = unchecked(slot + 1) % (uint)_entries.Length;
                    ++distance;
                }
            }

            static uint getHash(in TKey key)
            {
                int hash = key.GetHashCode();
                if (hash == 0)
                {
                    hash = 1;
                }
                return (uint)hash;
            }

            uint probeDistance(uint hash, uint slot)
                => unchecked(slot + capacity - (hash % capacity)) % capacity;

            static void swap<T>(ref T p1, ref T p2)
            {
                T tmp = p1;
                p1 = p2;
                p2 = tmp;
            }
        }

        public TValue this[TKey key] => throw new System.NotImplementedException();

        public IEnumerable<TKey> Keys => throw new System.NotImplementedException();

        public IEnumerable<TValue> Values => throw new System.NotImplementedException();

        public bool ContainsKey(TKey key)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            throw new System.NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new System.NotImplementedException();
        }
    }
}
