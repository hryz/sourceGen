using System;
using Microsoft.CodeAnalysis;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

internal static class MemberGenerator
{
    // 0 = model type
    // 1 = prop type
    // 2 = prop index
    // 3 = prop name
    private const string Prop1Template = @"
    public static IndexBuilderR<{1},TK,{0}> FindBy{3}<TK>(this ICacheReader<TK, {0}>s, {1} x) => new()
    {{
        Source = s,
        Field1Index = {2}, Field1Value = x
    }};";

    private const string Prop2Template = @"
    public static IndexBuilderR<T, {1}, TK, {0}> AndBy{3}<T,TK>(in this IndexBuilderR<T,TK,{0}> it, {1} x) => new()
    {{
        Source = it.Source,
        Field1Index = it.Field1Index, Field1Value = it.Field1Value,
        Field2Index = {2}, Field2Value = x
    }};";

    private const string Prop3Template = @"
    public static IndexBuilderR<T1, T2, {1}, TK, {0}> AndBy{3}<T1, T2, TK>(in this IndexBuilderR<T1, T2, TK, {0}> it, {1} x) => new()
    {{
        Source = it.Source,
        Field1Index = it.Field1Index, Field1Value = it.Field1Value,
        Field2Index = it.Field2Index, Field2Value = it.Field2Value,
        Field3Index = {2}, Field3Value = x
    }};";
    
    public static string CreateClass(ITypeSymbol typeSymbol,(int idx, ITypeSymbol type, string name)[] props)
    {
        var modelName = typeSymbol.Name;
        var modelType = typeSymbol.OriginalDefinition.Name;
        var modelNameSpace = typeSymbol.ContainingNamespace.ToDisplayString();
        
        var classDeclaration = 
            ClassDeclaration(modelName + "Extensions")
                .AddModifiers(Token(PublicKeyword), Token(StaticKeyword));

        foreach (var (idx, type, name) in props)
        {
            var method1String = string.Format(Prop1Template, modelType, type.Name, idx, name);
            var method2String = string.Format(Prop2Template, modelType, type.Name, idx, name);
            var method3String = string.Format(Prop3Template, modelType, type.Name, idx, name);
            classDeclaration = classDeclaration.AddMembers(
                ParseMemberDeclaration(method1String)!,
                ParseMemberDeclaration(method2String)!,
                ParseMemberDeclaration(method3String)!);
        }

        var syntaxFactory = CompilationUnit()
            .AddUsings(
                UsingDirective(ParseName("System")),
                UsingDirective(ParseName("System.Collections.Generic")),
                UsingDirective(ParseName("db")))
            .AddMembers(
                NamespaceDeclaration(ParseName(modelNameSpace)).NormalizeWhitespace()
                    .AddMembers(classDeclaration));
        
        return syntaxFactory
            .NormalizeWhitespace()
            .ToFullString();
    }
}