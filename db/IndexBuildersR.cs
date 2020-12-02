using System.Collections.Generic;

namespace db
{
    public struct IndexBuilderR<T1, TK, TV>
    {
        public ICacheReader<TK, TV> Source { get; init; }
        public int Field1Index { get; init; }
        public T1 Field1Value { get; init; }
        public IEnumerable<TV> Now() => Source.Find<T1>(in this);
    }
    public struct IndexBuilderR<T1,T2,TK, TV>
    {
        public ICacheReader<TK, TV> Source { get; init; }
        public int Field1Index { get; init; } 
        public T1 Field1Value { get; init; }
        public int Field2Index { get; init; } 
        public T2 Field2Value { get; init; }
        public IEnumerable<TV> Now() => Source.Find<T1, T2>(in this);
    }
    public struct IndexBuilderR<T1,T2,T3, TK, TV>
    {
        public ICacheReader<TK, TV> Source { get; init; }
        public int Field1Index { get; init; } 
        public T1 Field1Value { get; init; }
        public int Field2Index { get; init; } 
        public T2 Field2Value { get; init; }
        public int Field3Index { get; init; } 
        public T3 Field3Value { get; init; }
        public IEnumerable<TV> Now() => Source.Find<T1, T2, T3>(in this);
    }
}