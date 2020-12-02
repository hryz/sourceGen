using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace net5
{
    public static class Generators
    {
        public static void Run()
        {
            var dbSet = new List<SourceModel>();
            var result = dbSet.FindByA(1).AndByC(true);
        }
    }
    
    [DataContract]
    public class SourceModel
    {
        [DataMember(Order = 0)] public int A { get; init; }
        [DataMember(Order = 1)] public string? B { get; init; }
        [DataMember(Order = 2)] public bool C { get; init; }
        [DataMember(Order = 3)] public DateTime D { get; init; }
    }
}