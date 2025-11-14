namespace db;

public struct IndexBuilderR<T1, TK, TV> : IEnumerable<TV>
{
    public ICacheReader<TK, TV> Source { get; init; }
    public int Field1Index { get; init; }
    public T1 Field1Value { get; init; }
        
    public IEnumerable<TV> Run() => Source.Find<T1>(in this);

    IEnumerator<TV> IEnumerable<TV>.GetEnumerator() => Run().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Run().GetEnumerator();
}
    
public struct IndexBuilderR<T1,T2,TK, TV> : IEnumerable<TV>
{
    public ICacheReader<TK, TV> Source { get; init; }
    public int Field1Index { get; init; } 
    public T1 Field1Value { get; init; }
    public int Field2Index { get; init; } 
    public T2 Field2Value { get; init; }
        
    public IEnumerable<TV> Run() => Source.Find<T1, T2>(in this);
        
    IEnumerator<TV> IEnumerable<TV>.GetEnumerator() => Run().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Run().GetEnumerator();
}
    
public struct IndexBuilderR<T1,T2,T3, TK, TV> : IEnumerable<TV>
{
    public ICacheReader<TK, TV> Source { get; init; }
    public int Field1Index { get; init; } 
    public T1 Field1Value { get; init; }
    public int Field2Index { get; init; } 
    public T2 Field2Value { get; init; }
    public int Field3Index { get; init; } 
    public T3 Field3Value { get; init; }
        
    public IEnumerable<TV> Run() => Source.Find<T1, T2, T3>(in this);
        
    IEnumerator<TV> IEnumerable<TV>.GetEnumerator() => Run().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Run().GetEnumerator();
}