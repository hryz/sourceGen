using System;
using System.Collections.Generic;
using System.Linq;

namespace db
{
    public class Index<TK, TV, TI> : IIndexReader<TV, TI> , IIndexWriter<TK, TV>
        where TV : IKey<TK>
        where TI : IEquatable<TI>
        where TK : notnull
    {
        private readonly IMultimap<TI, TK, TV> _store = new Multimap<TI, TK, TV>();
        private readonly Func<TV, TI> _getIndexKey;

        public Index(Func<TV, TI> getKey) => _getIndexKey = getKey;

        public IEnumerable<TV> this[TI k] => _store.TryGetAllValues(k, out var values)
            ? values.Values
            : Enumerable.Empty<TV>();

        public void Update(TV? oldValue, TV newValue)
        {
            var newIdxKey = _getIndexKey(newValue);

            if (oldValue == null) //insert
            {
                _store.AddOrUpdate(newIdxKey, newValue.Key, newValue);
            }
            else //update
            {
                var oldIdxKey = _getIndexKey(oldValue);
                if (oldIdxKey.Equals(newIdxKey))
                {
                    //indexed field stayed the same - update only value
                    _store.AddOrUpdate(oldIdxKey, oldValue.Key, newValue);
                }
                else
                {
                    //indexed field has changed - move to a new bucket
                    _store.TryRemove(oldIdxKey, oldValue.Key);
                    _store.AddOrUpdate(newIdxKey, newValue.Key, newValue);
                }
            }
        }

        public void Delete(TV value)
        {
            var oldIdxKey = _getIndexKey(value);
            _store.TryRemove(oldIdxKey, value.Key);
        }
    }
}
