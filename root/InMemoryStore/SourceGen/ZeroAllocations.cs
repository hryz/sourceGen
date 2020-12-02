using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace net5
{
    [DataContract]
    public class ModelA
    {
        [DataMember(Order = 0)] public int FieldA { get; init; }
        [DataMember(Order = 1)] public string? FieldB { get; init; }
        [DataMember(Order = 2)] public bool FieldC { get; init; }
        [DataMember(Order = 3)] public DateTime FieldE { get; init; }
    }
    
    public class ZeroAllocations
    {
        public void Run()
        {
            var x = Collection.FindByFieldA(42).AndByFieldB("foo").Now();
        }

        private List<ModelA> Collection { get; set; } = new();
    }


    public struct IndexBuilder<T1, TModel>
    {
        public List<TModel> Source { get; init; }
        public int Field1Index { get; init; }
        public T1 Field1Value { get; init; }
        public List<TModel> Now() => throw new NotImplementedException(); //call internal logic of index with accumulated params
    }
    public struct IndexBuilder<T1,T2,TModel>
    {
        public List<TModel> Source { get; init; }
        public int Field1Index { get; init; } 
        public T1 Field1Value { get; init; }
        public int Field2Index { get; init; } 
        public T2 Field2Value { get; init; }
        public List<TModel> Now() => throw new NotImplementedException(); //call internal logic of index with accumulated params
    }
    public struct IndexBuilder<T1,T2,T3, TModel>
    {
        public List<TModel> Source { get; init; }
        public int Field1Index { get; init; } 
        public T1 Field1Value { get; init; }
        public int Field2Index { get; init; } 
        public T2 Field2Value { get; init; }
        public int Field3Index { get; init; } 
        public T3 Field3Value { get; init; }
        public List<TModel> Now() => throw new NotImplementedException(); //call internal logic of index with accumulated params
    }
    
    /*
     🚀 This code was moved into Source Generators 🚀
     
    static class ModelAExt //generated class per contract
    {
        //3 generated ext methods per field (A) to support a field in all positions
        //also generate similar builders for the index registration
        //sort fields during indexing and search (avoid [a,b] vs [b,a])
        public static IndexBuilder<int, ModelA> FindByA(this List<ModelA>s, int x) => new()
        {
            Source = s,
            Field1Index = 0, Field1Value = x  // 0 = field index
        };

        public static IndexBuilder<T, int, ModelA> AndByA<T>(in this IndexBuilder<T, ModelA> it, int x) => new()
        {
            Source = it.Source,
            Field1Index = it.Field1Index, Field1Value = it.Field1Value,
            Field2Index = 0, Field2Value = x
        };

        public static IndexBuilder<T1, T2, int, ModelA> AndByA<T1, T2>(in this IndexBuilder<T1, T2, ModelA> it, int x) => new()
        {
            Source = it.Source,
            Field1Index = it.Field1Index, Field1Value = it.Field1Value,
            Field2Index = it.Field2Index, Field2Value = it.Field2Value,
            Field3Index = 0, Field3Value = x
        };
        
        //3 generated ext methods per field (B) to support a field in all positions
        public static IndexBuilder<string> FindByB(this List<ModelA>s, string x) => new()
        {
            Source = s,
            Field1Index = 1, Field1Value = x // 0 = field index
        };

        public static IndexBuilder<T, string> AndByB<T>(in this IndexBuilder<T> it, string x) => new()
        {
            Source = it.Source,
            Field1Index = it.Field1Index, Field1Value = it.Field1Value,
            Field2Index = 1, Field2Value = x
        };

        public static IndexBuilder<T1, T2, string> AndByB<T1, T2>(in this IndexBuilder<T1, T2> it, string x) => new()
        {
            Source = it.Source,
            Field1Index = it.Field1Index, Field1Value = it.Field1Value,
            Field2Index = it.Field2Index, Field2Value = it.Field2Value,
            Field3Index = 1, Field3Value = x
        };

        public static (int a, int b, int c) Sort(int a, int b, int c)
        {
            if (a == b || a == c || b == c) throw new Exception("indices are collide");
            if (a < b && a < c) return b < c ? (a, b, c) : (a, c, b);
            if (b < a && b < c) return a < c ? (b, a, c) : (b, c, a);
            if (c < a && c < b) return a < b ? (c, a, b) : (c, b, a);
            throw new Exception("something went terribly wrong");
        }
    }
    */
    
    
}