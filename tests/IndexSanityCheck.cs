using client;
using FluentAssertions;
using Xunit;

namespace tests
{
    public class IndexSanityCheck
    {
        [Fact]
        public void Can_Find_By_Index()
        {
            var store = new InMemoryDatabase();
            
            //from the kafka loader side
            var writer = (IInMemoryDatabaseWriter) store;
            writer.ModelA.Update(1, new ModelA{ A = 1, B = "b", C = true}); // create 1st
            writer.ModelA.Update(1, new ModelA{ A = 1, B = "b2", C = false}); //update 1st
            writer.ModelA.Update(2, new ModelA{ A = 2, B = "z", C = true}); // create 2nd
            writer.ModelA.Delete(2);                                    //delete 2nd
            writer.ModelB.Update(1, new ModelB{ A = 1, StructField = new StrKey(1,2)}); //create 3rd
            
            //from the client side
            var reader = (IInMemoryDatabaseReader) store;
            reader.ModelA.FindByKey(1).Should().NotBeNull(); //PK
            reader.ModelA.FindByA(1).Run().Should().HaveCount(1); //IX
            reader.ModelA.FindByB("b2").AndByC(false).Run().Should().HaveCount(1); //Composite IX
            reader.ModelB.FindByStructField(new StrKey(1, 2)).Run().Should().HaveCount(1); //Complex type IX
        }
    }
}