using System.Linq;

namespace SBTech.Trading.Data.Odds.NextGen
{
    public class Root
    {
        public void Run()
        {
            var store = new InMemoryDatabase();
            
            //from the kafka loader side
            var writer = (IInMemoryDatabaseWriter) store;
            writer.ModelA.Update(1, new ModelA{ A = 1, B = "b", C = true}); //create
            writer.ModelA.Update(1, new ModelA{ A = 1, B = "b2", C = false}); //update
            writer.ModelA.Update(2, new ModelA{ A = 2, B = "z", C = true}); 
            writer.ModelA.Delete(2); //delete
            
            //from the client side
            var reader = (IInMemoryDatabaseReader) store;
            var m0 = reader.ModelA.FindByKey(1);
            var m1 = reader.ModelA.FindBy(x => x.A, 1).ToArray();
            var m2 = reader.ModelA.FindBy(x => x.B, "b2", x => x.C, false).ToArray();
            
        }
    }
}
