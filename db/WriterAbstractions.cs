namespace db
{
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

    internal interface ICacheIndexRegistration<TK, TV>
        where TV : IKey<TK>
        where TK : notnull
    {
        Cache<TK, TV> RegisterIndex<TI>(in IndexBuilderW<TI, TK, TV> builder) 
            where TI : notnull;
        
        Cache<TK, TV> RegisterIndex<TI, TI2>(in IndexBuilderW<TI, TI2, TK, TV> builder)
            where TI : notnull
            where TI2 : notnull;
        
        Cache<TK, TV> RegisterIndex<TI, TI2, TI3>(in IndexBuilderW<TI, TI2, TI3, TK, TV> builder)
            where TI : notnull
            where TI2 : notnull
            where TI3 : notnull;
    }
}
