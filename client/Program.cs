namespace client;

public static class Program
{
    public static void Main(string[] args)
    {
        var store = new InMemoryDatabase();
            
        //from the kafka loader side
        var writer = (IInMemoryDatabaseWriter) store;
        writer.ModelA.Update(1, new ModelA{ A = 1, B = "b", C = true}); //create
        writer.ModelA.Update(1, new ModelA{ A = 1, B = "b2", C = false}); //update
        writer.ModelA.Update(2, new ModelA{ A = 2, B = "z", C = true}); 
        writer.ModelA.Delete(2); //delete
        writer.ModelB.Update(1, new ModelB{ A = 1, StructField = new StrKey(1,2)});
            
        //from the client side
        var reader = (IInMemoryDatabaseReader) store;
        var m0 = reader.ModelA.FindByKey(1);
        var m1 = reader.ModelA.FindByA(1).Run();
        var m2 = reader.ModelA.FindByB("b2").AndByC(false).Run();
        var complex = reader.ModelB.FindByStructField(new StrKey(1, 2)).Run();
    }
}