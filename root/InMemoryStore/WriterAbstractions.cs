using System;
using System.Linq.Expressions;

namespace SBTech.Trading.Data.Odds.NextGen
{
    public interface IInMemoryDatabaseWriter
    {
        ICacheWriter<int, ModelA> ModelA { get; }
        ICacheWriter<int, ModelB> ModelB { get; }
    }
    
    public interface ICacheWriter<in TK, in TV>
    {
        void Update(TK key, TV value);
        void Delete(TK key);
    }
    
    public interface IIndexWriter<TK, in TV> where TV : IKey<TK>
    {
        void Update(TV? oldValue, TV newValue);
        void Delete(TV value);
    }
    
    public interface ICacheIndexRegistration<TK, TV> 
        where TV : IKey<TK>
        where TK : notnull
    {
        Cache<TK, TV> WithIndex<TI>(
            Expression<Func<TV, TI>> getIndexKey)
            where TI : notnull;

        Cache<TK, TV> WithIndex<TI1, TI2>(
            Expression<Func<TV, TI1>> getIndexKey1,
            Expression<Func<TV, TI2>> getIndexKey2)
            where TI1 : notnull
            where TI2 : notnull;
        

        Cache<TK, TV> WithIndex<TI1, TI2, TI3>(
            Expression<Func<TV, TI1>> getIndexKey1,
            Expression<Func<TV, TI2>> getIndexKey2,
            Expression<Func<TV, TI3>> getIndexKey3)
            where TI1 : notnull
            where TI2 : notnull
            where TI3 : notnull;
    }
}
