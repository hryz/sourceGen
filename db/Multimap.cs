using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace db
{
    /// <summary>
    /// Single Writer Multiple Readers nested concurrent hash map
    /// </summary>
    /// <typeparam name="TKey1">Key 1</typeparam>
    /// <typeparam name="TKey2">Key 2</typeparam>
    /// <typeparam name="TValue">Value</typeparam>
    public interface IMultimap<in TKey1, TKey2, TValue>
    {
        void AddOrUpdate(TKey1 key1, TKey2 key2, TValue value);
        bool TryRemove(TKey1 key1, TKey2 key2);
        bool TryRemoveAll(TKey1 key1);
        bool TryGetValue(TKey1 key1, TKey2 key2, [MaybeNullWhen(false)]out TValue result);
        bool TryGetAllValues(TKey1 key1, [MaybeNullWhen(false)]out IReadOnlyDictionary<TKey2, TValue> result);
    }

    internal class Multimap<TKey1, TKey2, TValue> : IMultimap<TKey1, TKey2, TValue>
        where TKey1 : notnull
        where TKey2 : notnull
    {
        private readonly ConcurrentDictionary<TKey1, ConcurrentDictionary<TKey2, TValue>> _map = new();

        public void AddOrUpdate(TKey1 key1, TKey2 key2, TValue value)
        {
            if (_map.TryGetValue(key1, out var inner))
            {
                inner[key2] = value;
            }
            else
            {
                _map.TryAdd(key1, new ConcurrentDictionary<TKey2, TValue> {[key2] = value});
            }
        }

        public bool TryRemove(TKey1 key1, TKey2 key2)
        {
            if (_map.TryGetValue(key1, out var inner) && inner.TryRemove(key2, out _))
            {
                if (inner.IsEmpty)
                    _map.TryRemove(key1, out _);

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TryRemoveAll(TKey1 key1)
        {
            return _map.TryGetValue(key1, out _);
        }

        public bool TryGetValue(TKey1 key1, TKey2 key2, [MaybeNullWhen(false)]out TValue result)
        {
            if (_map.TryGetValue(key1, out var inner) && inner.TryGetValue(key2, out var r))
            {
                result = r;
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        public bool TryGetAllValues(TKey1 key1, [MaybeNullWhen(false)]out IReadOnlyDictionary<TKey2, TValue> result)
        {
            if (_map.TryGetValue(key1, out var r))
            {
                result = r;
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }
    }
}