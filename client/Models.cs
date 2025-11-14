namespace client;

[DataContract]
public class ModelA : IKey<int>
{
    int IKey<int>.Key => A;
    [DataMember(Order = 0)] public int A { get; set; }
    [DataMember(Order = 1)] public string B { get; set; } = "";
    [DataMember(Order = 2)] public bool C { get; set; }
    [DataMember(Order = 3)] public List<int> Bla2 { get; set; } = new();
}
    
[DataContract]
public class ModelB : IKey<int>
{
    public int Key => A;
    [DataMember(Order = 0)] public int A { get; set; }
    [DataMember(Order = 1)] public bool B { get; set; }
    [DataMember(Order = 2)] public bool C { get; set; }
    [DataMember(Order = 3)] public bool D { get; set; }
    [DataMember(Order = 4)] public StrKey StructField { get; set; }
    [DataMember(Order = 5)] public (int,int) TupleField { get; set; }
    [DataMember(Order = 6)] public int[] NonEquatableField { get; set; } = Array.Empty<int>();
}

[DataContract]
public readonly struct StrKey : IEquatable<StrKey>
{
    public StrKey(int x, int y) => (X, Y) = (x, y);
    [DataMember(Order = 0)] public int X { get; }
    [DataMember(Order = 1)] public int Y { get; }

    public bool Equals(StrKey other) => X == other.X && Y == other.Y;
    public override bool Equals(object? obj) => obj is StrKey other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(X, Y);
}