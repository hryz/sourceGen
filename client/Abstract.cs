using db;

namespace client
{
    public interface IInMemoryDatabaseReader
    {
        ICacheReader<int, ModelA> ModelA { get; }
        ICacheReader<int, ModelB> ModelB { get; }
    }
    
    public interface IInMemoryDatabaseWriter
    {
        ICacheWriter<int, ModelA> ModelA { get; }
        ICacheWriter<int, ModelB> ModelB { get; }
    }
}