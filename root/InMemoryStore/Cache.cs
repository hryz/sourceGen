using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace SBTech.Trading.Data.Odds.NextGen
{
    public class Cache<TK, TV> : 
        ICacheReader<TK, TV>, 
        ICacheWriter<TK, TV>, 
        ICacheIndexRegistration<TK, TV> 
        where TV : IKey<TK>
        where TK : notnull
    {
        private readonly Dictionary<TK, TV> _store = new();
        private readonly Dictionary<(string?, string?, string?), object> _indexes = new();

        public TV this[TK k] => _store[k];

        public TV FindByKey(TK k) => _store[k];

        public IEnumerable<TV> FindBy<TI>(
            Expression<Func<TV, TI>> f, TI v)
        {
            return FindBy(GetPropName(f), v);
        }

        public IEnumerable<TV> FindBy<TI1, TI2>(
            Expression<Func<TV, TI1>> f1, TI1 v1, 
            Expression<Func<TV, TI2>> f2, TI2 v2)
        {
            return FindBy(GetPropName(f1), v1, GetPropName(f2), v2);
        }

        public IEnumerable<TV> FindBy<TI1, TI2, TI3>(
            Expression<Func<TV, TI1>> f1, TI1 v1, 
            Expression<Func<TV, TI2>> f2, TI2 v2,
            Expression<Func<TV, TI3>> f3, TI3 v3)
        {
            return FindBy(GetPropName(f1), v1, GetPropName(f2), v2, GetPropName(f3), v3);
        }

        public IEnumerable<TV> FindBy<TI>(string f, TI v)
        {
            var idx = _indexes[(f, null, null)] as IIndexReader<TV, TI>;
            return idx![v];
        }

        public IEnumerable<TV> FindBy<TI1, TI2>(string f1, TI1 v1, string f2, TI2 v2)
        {
            var idx = _indexes[(f1, f2, null)] as IIndexReader<TV, (TI1, TI2)>;
            return idx![(v1,v2)];
        }

        public IEnumerable<TV> FindBy<TI1, TI2, TI3>(string f1, TI1 v1, string f2, TI2 v2, string f3, TI3 v3)
        {
            var idx = _indexes[(f1, f2, f3)] as IIndexReader<TV, (TI1, TI2, TI3)>;
            return idx![(v1,v2,v3)];
        }

        public Cache<TK, TV> WithIndex<TI>(
            Expression<Func<TV, TI>> getIndexKey)
            where TI : notnull
        {
            var prop = GetPropName(getIndexKey);
            var i = new Index<TK, TV, TI>(getIndexKey.Compile());
            _indexes.Add((prop, null, null), i);
            return this;
        }

        public Cache<TK, TV> WithIndex<TI1, TI2>(
            Expression<Func<TV, TI1>> getIndexKey1,
            Expression<Func<TV, TI2>> getIndexKey2)
            where TI1 : notnull
            where TI2 : notnull
        {
            var prop1 = GetPropName(getIndexKey1);
            var prop2 = GetPropName(getIndexKey2);

            var get1 = getIndexKey1.Compile();
            var get2 = getIndexKey2.Compile();

            var i = new Index<TK, TV, (TI1, TI2)>(x => (get1(x), get2(x)));
            _indexes.Add((prop1, prop2, null), i);
            return this;
        }
        
        public Cache<TK, TV> WithIndex<TI1, TI2, TI3>(
            Expression<Func<TV, TI1>> getIndexKey1,
            Expression<Func<TV, TI2>> getIndexKey2,
            Expression<Func<TV, TI3>> getIndexKey3)
            where TI1 : notnull
            where TI2 : notnull
            where TI3 : notnull
        {
            var prop1 = GetPropName(getIndexKey1);
            var prop2 = GetPropName(getIndexKey2);
            var prop3 = GetPropName(getIndexKey3);

            var get1 = getIndexKey1.Compile();
            var get2 = getIndexKey2.Compile();
            var get3 = getIndexKey3.Compile();

            var i = new Index<TK, TV, (TI1, TI2, TI3)>(x => (get1(x), get2(x), get3(x)));
            _indexes.Add((prop1, prop2, prop3), i);
            return this;
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
        
        private static string GetPropName<TV, TI>(Expression<Func<TV, TI>> getIndexKey) =>
            ((PropertyInfo)((MemberExpression)getIndexKey.Body).Member).Name;
    }
}
