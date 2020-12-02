using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SBTech.Trading.Data.Odds.NextGen
{
    public interface IInMemoryDatabaseReader
    {
        ICacheReader<int, ModelA> ModelA { get; }
        ICacheReader<int, ModelB> ModelB { get; }
    }

    public interface ICacheReader<in TK, TV>
    {
        TV this[TK k] { get; }

        TV FindByKey(TK k);
        
        IEnumerable<TV> FindBy<TI>(
            Expression<Func<TV,TI>> f, TI v);

        IEnumerable<TV> FindBy<TI1, TI2>(
            Expression<Func<TV, TI1>> f1, TI1 v1,
            Expression<Func<TV, TI2>> f2, TI2 v2);
        
        IEnumerable<TV> FindBy<TI1, TI2, TI3>(
            Expression<Func<TV, TI1>> f1, TI1 v1,
            Expression<Func<TV, TI2>> f2, TI2 v2,
            Expression<Func<TV, TI3>> f3, TI3 v3);
        
        IEnumerable<TV> FindBy<TI>(
            string f, TI v);

        IEnumerable<TV> FindBy<TI1, TI2>(
            string f1, TI1 v1,
            string f2, TI2 v2);
        
        IEnumerable<TV> FindBy<TI1, TI2, TI3>(
            string f1, TI1 v1,
            string f2, TI2 v2,
            string f3, TI3 v3);
    }

    public interface IIndexReader<out TV, in TI>
    {
        IEnumerable<TV> this[TI k] { get; }
    }

    public interface IKey<out T> { T Key { get; } }
}
