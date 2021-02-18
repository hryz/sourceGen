using System;

namespace db
{
    public interface ICacheWriter<in TK, in TV>
    {
        void Update(TK key, TV value);
        void Delete(TK key);
    }
    
    public interface IIndexWriter : IIndex { } 
    
    public interface IIndexWriter<TK, in TV> : IIndexWriter where TV : IKey<TK>
    {
        void Update(TV? oldValue, TV newValue);
        void Delete(TV value);
    }

    internal interface ICacheIndexRegistration<TK, TV>
        where TV : IKey<TK>
        where TK : notnull
    {
        Cache<TK, TV> RegisterIndex<TI>(in IndexBuilderW<TI, TK, TV> builder)
            where TI : IEquatable<TI>;

        Cache<TK, TV> RegisterIndex<TI, TI2>(in IndexBuilderW<TI, TI2, TK, TV> builder)
            where TI : IEquatable<TI>
            where TI2 : IEquatable<TI2>;

        Cache<TK, TV> RegisterIndex<TI, TI2, TI3>(in IndexBuilderW<TI, TI2, TI3, TK, TV> builder)
            where TI : IEquatable<TI>
            where TI2 : IEquatable<TI2>
            where TI3 : IEquatable<TI3>;
    }
}
