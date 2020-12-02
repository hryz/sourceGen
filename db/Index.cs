using System;
using System.Collections.Generic;
using System.Linq;

namespace db
{
    public class Index<TK, TV, TI> : IIndexReader<TV, TI> , IIndexWriter<TK, TV>
        where TV : IKey<TK>
        where TI : notnull
        where TK : notnull
    {
        private readonly Dictionary<TI, Dictionary<TK,TV>> _store = new();
        private readonly Func<TV, TI> _getIndexKey;

        public Index(Func<TV, TI> getKey) => _getIndexKey = getKey;

        public IEnumerable<TV> this[TI k] =>
            _store.TryGetValue(k, out var values) ? values.Values : Enumerable.Empty<TV>();

        public void Update(TV? oldValue, TV newValue)
        {
            //remove old value from its bucket
            if (oldValue != null)
            {
                var oldIdxKey = _getIndexKey(oldValue);
                 _store[oldIdxKey].Remove(oldValue.Key);
                 if (_store[oldIdxKey].Count == 0)
                     _store.Remove(oldIdxKey);
            }

            //add new value to its bucket
            var newIdxKey = _getIndexKey(newValue);
            if (!_store.ContainsKey(newIdxKey))
                _store[newIdxKey] = new Dictionary<TK, TV>();
            
            var values = _store[newIdxKey];
            values[newValue.Key] = newValue;
        }

        public void Delete(TV value)
        {
            if(!_store.ContainsKey(_getIndexKey(value)))
                return;

            var values = _store[_getIndexKey(value)];
            values.Remove(value.Key);
            if (values.Count == 0)
                _store.Remove(_getIndexKey(value));
        }
    }
}
