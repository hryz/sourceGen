using System.Collections.Immutable;
using System.Runtime.Serialization;
using db;

namespace net5.InMemoryStore.ClientCode
{
    [DataContract]
    public class ModelA : IKey<int>
    {
        int IKey<int>.Key => A;
        [DataMember(Order = 0)] public int A { get; set; }
        [DataMember(Order = 1)] public string B { get; set; } = "";
        [DataMember(Order = 2)] public bool C { get; set; }
        [DataMember(Order = 3)] public ImmutableSortedSet<int> Bla { get; set; } = ImmutableSortedSet<int>.Empty;
    }
    
    [DataContract]
    public class ModelB : IKey<int>
    {
        public int Key => A;
        [DataMember(Order = 0)] public int A { get; set; }
        [DataMember(Order = 1)] public bool B { get; set; }
        [DataMember(Order = 2)] public bool C { get; set; }
        [DataMember(Order = 3)] public bool D { get; set; }
    }
}