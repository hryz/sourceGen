using System.Collections.Generic;

namespace SBTech.Trading.Data.Odds.NextGen
{
    public class InMemoryDatabase : IInMemoryDatabaseReader, IInMemoryDatabaseWriter
    {
        public InMemoryDatabase()
        {
            _modelAs = new Cache<int, ModelA>()
                .WithIndex(x => x.A)
                .WithIndex(x => x.B, x => x.C);

            _modelBs = new Cache<int, ModelB>()
                .WithIndex(x => x.B, x => x.C, x => x.D);
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

    
    public class ModelA : IKey<int>
    {
        public int Key => A;
        public int A { get; set; }
        public string B { get; set; } = "";
        public bool C { get; set; }
    }
    
    public class ModelB : IKey<int>
    {
        public int Key => A;
        public int A { get; set; }
        public bool B { get; set; }
        public bool C { get; set; }
        public bool D { get; set; }
    }
}
