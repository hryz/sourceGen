using Microsoft.CodeAnalysis;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace gen
{
    internal static class WriterBuildersGenerator
    {
        // 0 = model type
        // 1 = prop type
        // 2 = prop index
        // 3 = prop name
        // 4 = key type
        private const string Prop1Template = @"
    public static IndexBuilderW<{1}, {4}, {0}> IndexBy{3}(
            this Cache<{4}, {0}> it) => new()
        {{
            Source = it,
            Field1Index = {2},
            Field1Value = x => x.{3}
        }};";

        private const string Prop2Template = @"
   public static IndexBuilderW<T, {1}, {4}, {0}> AndBy{3}<T>(
            this in IndexBuilderW<T, {4}, {0}> it) 
            where T : notnull
            => new()
        {{
            Source = it.Source,
            Field1Index = it.Field1Index,
            Field1Value = it.Field1Value,
            Field2Index = {2},
            Field2Value = x => x.{3}
        }};";

        private const string Prop3Template = @"
    public static IndexBuilderW<T1, T2, {1}, {4}, {0}> AndBy{3}<T1,T2>(
            this in IndexBuilderW<T1, T2, {4}, {0}> it) 
            where T1 : notnull
            where T2 : notnull
            => new()
            {{
                Source = it.Source,
                Field1Index = it.Field1Index,
                Field1Value = it.Field1Value,
                Field2Index = it.Field2Index,
                Field2Value = it.Field2Value,
                Field3Index = {2},
                Field3Value = x => x.{3}
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
                ClassDeclaration(modelName + "WriteExtensions")
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
}