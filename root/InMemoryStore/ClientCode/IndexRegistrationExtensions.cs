using db;

namespace net5.InMemoryStore.ClientCode
{
    //todo: move implement autogen (and implement key determination to close generics)
    public static class IndexRegistrationExtensions
    {
        //3 methods per property, depending on the position
        public static IndexBuilderW<int, int, ModelA> IndexByA( //1st field 1st position
            this Cache<int, ModelA> it) => new()
        {
            Source = it,
            Field1Index = 0,
            Field1Value = x => x.A
        };
        
        public static IndexBuilderW<string, int, ModelA> IndexByB( //2nd field 1st position
            this Cache<int, ModelA> it) => new()
        {
            Source = it,
            Field1Index = 1,
            Field1Value = x => x.B
        };
        
        public static IndexBuilderW<string, bool, int, ModelA> AndByC( //3rd field 2nd position
            this in IndexBuilderW<string, int, ModelA> it) => new()
        {
            Source = it.Source,
            Field1Index = it.Field1Index,
            Field1Value = it.Field1Value,
            Field2Index = 1,
            Field2Value = x => x.C
        };
    }
}