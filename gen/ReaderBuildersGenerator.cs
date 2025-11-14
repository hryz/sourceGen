using Microsoft.CodeAnalysis;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace gen;

internal static class ReaderBuildersGenerator
{
    // 0 = model type
    // 1 = prop type
    // 2 = prop index
    // 3 = prop name
    // 4 = key type
    private const string Prop1Template = @"
    public static IndexBuilderR<{1},{4},{0}> FindBy{3}(this ICacheReader<{4}, {0}>s, {1} x) => new()
    {{
        Source = s,
        Field1Index = {2}, Field1Value = x
    }};";

    private const string Prop2Template = @"
    public static IndexBuilderR<T, {1}, {4}, {0}> AndBy{3}<T>(in this IndexBuilderR<T,{4},{0}> it, {1} x) => new()
    {{
        Source = it.Source,
        Field1Index = it.Field1Index, Field1Value = it.Field1Value,
        Field2Index = {2}, Field2Value = x
    }};";

    private const string Prop3Template = @"
    public static IndexBuilderR<T1, T2, {1}, {4}, {0}> AndBy{3}<T1, T2>(in this IndexBuilderR<T1, T2, {4}, {0}> it, {1} x) => new()
    {{
        Source = it.Source,
        Field1Index = it.Field1Index, Field1Value = it.Field1Value,
        Field2Index = it.Field2Index, Field2Value = it.Field2Value,
        Field3Index = {2}, Field3Value = x
    }};";
    
    public static string CreateClass(
        ITypeSymbol typeSymbol, 
        ITypeSymbol keyType,
        (int idx, ITypeSymbol type, string name)[] props)
    {
        var modelName = typeSymbol.Name;
        var modelType = typeSymbol.OriginalDefinition.Name;
        var modelNameSpace = typeSymbol.ContainingNamespace.ToDisplayString();
        
        var classDeclaration = 
            ClassDeclaration(modelName + "ReadExtensions")
                .AddModifiers(Token(PublicKeyword), Token(StaticKeyword));

        foreach (var (idx, type, name) in props)
        {
            var method1String = string.Format(Prop1Template, modelType, type.ToDisplayString(), idx, name, keyType.ToDisplayString());
            var method2String = string.Format(Prop2Template, modelType, type.ToDisplayString(), idx, name, keyType.ToDisplayString());
            var method3String = string.Format(Prop3Template, modelType, type.ToDisplayString(), idx, name, keyType.ToDisplayString());
            classDeclaration = classDeclaration.AddMembers(
                ParseMemberDeclaration(method1String)!,
                ParseMemberDeclaration(method2String)!,
                ParseMemberDeclaration(method3String)!);
        }

        var syntaxFactory = CompilationUnit()
            .AddUsings(
                //UsingDirective(ParseName("System")),
                UsingDirective(ParseName("db")))
            .AddMembers(
                NamespaceDeclaration(ParseName(modelNameSpace))
                    .AddMembers(classDeclaration));
        
        return syntaxFactory
            .NormalizeWhitespace()
            .ToFullString();
    }
}