using System.Collections.Generic;

namespace db
{
    public interface ICacheReader<TK, TV>
    {
        TV this[TK k] { get; }

        TV FindByKey(TK k);

        IEnumerable<TV> Find<TI>(in IndexBuilderR<TI, TK, TV> builder);
        IEnumerable<TV> Find<TI, TI2>(in IndexBuilderR<TI,TI2, TK, TV> builder);
        IEnumerable<TV> Find<TI, TI2, TI3>(in IndexBuilderR<TI, TI2, TI3, TK, TV> builder);
    }

    public interface IIndexReader<out TV, in TI>
    {
        IEnumerable<TV> this[TI k] { get; }
    }

    public interface IKey<out T> { T Key { get; } }
}
