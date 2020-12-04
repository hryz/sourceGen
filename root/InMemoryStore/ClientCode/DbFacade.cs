using db;

namespace net5.InMemoryStore.ClientCode
{
    public class InMemoryDatabase : IInMemoryDatabaseReader, IInMemoryDatabaseWriter
    {
        public InMemoryDatabase()
        {
            _modelAs = new Cache<int, ModelA>()
                .IndexByA().Register()
                .IndexByB().AndByC().Register();

            _modelBs = new Cache<int, ModelB>()
                .IndexByB().AndByC().AndByD().Register();
        }

        //private
        private readonly Cache<int, ModelA> _modelAs;
        private readonly Cache<int, ModelB> _modelBs;

        //readers
        ICacheReader<int, ModelA> IInMemoryDatabaseReader.ModelA => _modelAs;
        ICacheReader<int, ModelB> IInMemoryDatabaseReader.ModelB => _modelBs;
        
        //writers
        ICacheWriter<int, ModelA> IInMemoryDatabaseWriter.ModelA => _modelAs;
        ICacheWriter<int, ModelB> IInMemoryDatabaseWriter.ModelB => _modelBs;
    }
    
    
}
