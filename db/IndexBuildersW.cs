using System;

namespace db
{
    public struct IndexBuilderW<T1, TK, TV>
        where TV : IKey<TK>
        where TK : notnull
        where T1 : notnull
    {
        public Cache<TK, TV> Source { get; init; }
        public int Field1Index { get; init; }
        public Func<TV,T1> Field1Value { get; init; }
        public Cache<TK,TV> Register() => Source.RegisterIndex<T1>(in this);
    }
    public struct IndexBuilderW<T1,T2,TK, TV>
        where TV : IKey<TK>
        where TK : notnull
        where T1 : notnull
        where T2 : notnull
    {
        public Cache<TK, TV> Source { get; init; }
        public int Field1Index { get; init; } 
        public Func<TV,T1> Field1Value { get; init; }
        public int Field2Index { get; init; } 
        public Func<TV,T2> Field2Value { get; init; }
        public Cache<TK,TV> Register() => Source.RegisterIndex<T1, T2>(in this);
    }
    
    public struct IndexBuilderW<T1,T2,T3, TK, TV>
        where TV : IKey<TK>
        where TK : notnull
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
    {
        public Cache<TK, TV> Source { get; init; }
        public int Field1Index { get; init; } 
        public Func<TV,T1> Field1Value { get; init; }
        public int Field2Index { get; init; } 
        public Func<TV,T2> Field2Value { get; init; }
        public int Field3Index { get; init; } 
        public Func<TV,T3> Field3Value { get; init; }
        public Cache<TK, TV> Register() => Source.RegisterIndex<T1, T2, T3>(in this);
    }
}