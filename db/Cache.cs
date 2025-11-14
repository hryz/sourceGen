namespace db;

public class IndexNotRegisteredException : Exception { }

public class IndexAlreadyRegisteredException: Exception { }
    
public class Cache<TK, TV> : 
    ICacheReader<TK, TV>, 
    ICacheWriter<TK, TV>, 
    ICacheIndexRegistration<TK, TV> 
    where TV : IKey<TK>
    where TK : notnull
{
    private readonly ConcurrentDictionary<TK, TV> _store = new();
    private readonly ConcurrentDictionary<(int, int?, int?), IIndex> _indexes = new();

    public TV this[TK k] => _store[k];

    public TV FindByKey(TK k) => _store[k];
        
    public IEnumerable<TV> Find<TI>(in IndexBuilderR<TI, TK, TV> builder)
    {
        var indexBy = (builder.Field1Index, (int?)null, (int?)null);
        if (_indexes.ContainsKey(indexBy) && _indexes[indexBy] is IIndexReader<TV, TI> idx)
            return idx[builder.Field1Value];

        throw new IndexNotRegisteredException();
    }

    public IEnumerable<TV> Find<TI, TI2>(in IndexBuilderR<TI, TI2, TK, TV> builder)
    {
        var indexBy = (builder.Field1Index, builder.Field2Index, (int?) null);
        if (_indexes.ContainsKey(indexBy) && _indexes[indexBy] is IIndexReader<TV, (TI, TI2)> idx)
            return idx[(builder.Field1Value, builder.Field2Value)];

        throw new IndexNotRegisteredException();
    }

    public IEnumerable<TV> Find<TI, TI2, TI3>(in IndexBuilderR<TI, TI2, TI3, TK, TV> builder)
    {
        var indexBy = (builder.Field1Index, builder.Field2Index, builder.Field3Index);
        if (_indexes.ContainsKey(indexBy) && _indexes[indexBy] is IIndexReader<TV, (TI, TI2, TI3)> idx)
            return idx[(builder.Field1Value, builder.Field2Value, builder.Field3Value)];

        throw new IndexNotRegisteredException();
    }

    public void Update(TK key, TV value)
    {
        var oldValue = _store.TryGetValue(key, out var old) ? old : default; //single writer, multi-reader
        foreach (var (_, idx) in _indexes)
        {
            if (idx is IIndexWriter<TK, TV> writer)
                writer.Update(oldValue, value);
        }
            
        _store[key] = value;
    }

    public void Delete(TK key)
    {
        if (_store.TryRemove(key, out var value))
        {
            foreach (var (_, idx) in _indexes)
            {
                if(idx is IIndexWriter<TK, TV> writer)
                    writer.Delete(value);
            }
        }
    }

    public Cache<TK,TV> RegisterIndex<TI>(
        in IndexBuilderW<TI, TK, TV> builder) 
        where TI : IEquatable<TI>
    {
        var i = new Index<TK, TV, TI>(builder.Field1Value);
        if (!_indexes.TryAdd((builder.Field1Index, null, null), i))
            throw new IndexAlreadyRegisteredException();
                    
        return this;
    }

    public Cache<TK,TV> RegisterIndex<TI, TI2>(
        in IndexBuilderW<TI, TI2, TK, TV> builder) 
        where TI : IEquatable<TI>
        where TI2 : IEquatable<TI2>
    {
        var (f1, f2) = (builder.Field1Value, builder.Field2Value);
        var i = new Index<TK, TV, (TI, TI2)>(x => (f1(x), f2(x)));
        if(!_indexes.TryAdd((builder.Field1Index, builder.Field2Index, null), i))
            throw new IndexAlreadyRegisteredException();
            
        return this;
    }

    public Cache<TK,TV> RegisterIndex<TI, TI2, TI3>(
        in IndexBuilderW<TI, TI2, TI3, TK, TV> builder) 
        where TI : IEquatable<TI>
        where TI2 : IEquatable<TI2>
        where TI3 : IEquatable<TI3>
    {
        var (f1, f2,f3) = (builder.Field1Value, builder.Field2Value, builder.Field3Value);
        var i = new Index<TK, TV, (TI, TI2, TI3)>(x => (f1(x), f2(x), f3(x)));
        if(!_indexes.TryAdd((builder.Field1Index, builder.Field2Index, builder.Field3Index), i))
            throw new IndexAlreadyRegisteredException();
            
        return this;
    }
}