using System.Collections.Generic;

namespace db
{
    public class Cache<TK, TV> : 
        ICacheReader<TK, TV>, 
        ICacheWriter<TK, TV>, 
        ICacheIndexRegistration<TK, TV> 
        where TV : IKey<TK>
        where TK : notnull
    {
        private readonly Dictionary<TK, TV> _store = new();
        private readonly Dictionary<(int, int?, int?), object> _indexes = new();

        public TV this[TK k] => _store[k];

        public TV FindByKey(TK k) => _store[k];
        
        public IEnumerable<TV> Find<TI>(in IndexBuilderR<TI, TK, TV> builder)
        {
            var idx = _indexes[(builder.Field1Index, null, null)] as IIndexReader<TV, TI>;
            return idx![builder.Field1Value];
        }

        public IEnumerable<TV> Find<TI, TI2>(in IndexBuilderR<TI, TI2, TK, TV> builder)
        {
            var idx = _indexes[(builder.Field1Index, builder.Field2Index, null)] as IIndexReader<TV, (TI, TI2)>;
            return idx![(builder.Field1Value, builder.Field2Value)];
        }

        public IEnumerable<TV> Find<TI, TI2, TI3>(in IndexBuilderR<TI, TI2, TI3, TK, TV> builder)
        {
            var idx = _indexes[(builder.Field1Index, builder.Field2Index, builder.Field3Index)] as IIndexReader<TV, (TI, TI2, TI3)>;
            return idx![(builder.Field1Value, builder.Field2Value, builder.Field3Value)];
        }

        public void Update(TK key, TV value)
        {
            var oldValue = _store.ContainsKey(key) ? _store[key] : default;
            foreach (var (_, idx) in _indexes)
            {
                var writer = idx as IIndexWriter<TK, TV>;
                writer?.Update(oldValue, value);
            }
            _store[key] = value;
        }

        public void Delete(TK key)
        {
            var value = _store[key];
            foreach (var (_, idx) in _indexes)
            {
                var writer = idx as IIndexWriter<TK, TV>;
                writer?.Delete(value);
            }
            _store.Remove(key);
        }

        public Cache<TK,TV> RegisterIndex<TI>(
            in IndexBuilderW<TI, TK, TV> builder) 
            where TI : notnull
        {
            var i = new Index<TK, TV, TI>(builder.Field1Value);
            _indexes.Add((builder.Field1Index, null, null), i);
            return this;
        }

        public Cache<TK,TV> RegisterIndex<TI, TI2>(
            in IndexBuilderW<TI, TI2, TK, TV> builder) 
            where TI : notnull 
            where TI2 : notnull
        {
            var (f1, f2) = (builder.Field1Value, builder.Field2Value);
            var i = new Index<TK, TV, (TI, TI2)>(x => (f1(x), f2(x)));
            _indexes.Add((builder.Field1Index, builder.Field2Index, null), i);
            return this;
        }

        public Cache<TK,TV> RegisterIndex<TI, TI2, TI3>(
            in IndexBuilderW<TI, TI2, TI3, TK, TV> builder) 
            where TI : notnull 
            where TI2 : notnull 
            where TI3 : notnull
        {
            var (f1, f2,f3) = (builder.Field1Value, builder.Field2Value, builder.Field3Value);
            var i = new Index<TK, TV, (TI, TI2, TI3)>(x => (f1(x), f2(x), f3(x)));
            _indexes.Add((builder.Field1Index, builder.Field2Index, builder.Field3Index), i);
            return this;
        }
    }
}
