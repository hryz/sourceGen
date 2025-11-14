using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace gen;

[Generator]
public class DbQueryGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: (node, _) => IsDataContract(node),
            transform: (syntax, _) => syntax.SemanticModel.GetDeclaredSymbol(syntax.Node) as ITypeSymbol);

        context.RegisterSourceOutput(provider, Execute);
    }

    private static bool IsDataContract(SyntaxNode node) =>
        node is TypeDeclarationSyntax t && t.AttributeLists.SelectMany(al => al.Attributes).Any(IsDataContract);

    private static bool IsDataContract(AttributeSyntax a) =>
        a.Name.ToFullString().Equals("DataContract", StringComparison.Ordinal);

    private static void Execute(SourceProductionContext context, ITypeSymbol typeSymbol)
    {
        var keyType = ExtractKeyType(typeSymbol);
        if (keyType == null)
            return; // no implementation of IKey

        var props = ExtractFieldIndexes(typeSymbol).ToArray();
        var readerExt = ReaderBuildersGenerator.CreateClass(typeSymbol, keyType, props);
        var writerExt = WriterBuildersGenerator.CreateClass(typeSymbol, keyType, props);
        context.AddSource(typeSymbol.Name + "ReadExtensions", SourceText.From(readerExt, Encoding.UTF8));
        context.AddSource(typeSymbol.Name + "WriteExtensions", SourceText.From(writerExt, Encoding.UTF8));
    }

    private static IEnumerable<(int, ITypeSymbol, string)> ExtractFieldIndexes(ITypeSymbol candidate)
    {
        var props = candidate.GetMembers().OfType<IPropertySymbol>();
        foreach (var prop in props)
        {
            var attr = prop
                .GetAttributes()
                .SingleOrDefault(x =>
                    x.AttributeClass?.Name.Equals("DataMemberAttribute", StringComparison.Ordinal) ?? false);

            if (attr == null) continue;

            var order = attr.NamedArguments
                .SingleOrDefault(x => x.Key.Equals("Order", StringComparison.Ordinal));

            if (order.Key == null) continue;

            if (!prop.Type.Interfaces.Any(x =>
                    x.Name.Equals("IEquatable", StringComparison.Ordinal)
                    && x.TypeArguments.Any(a => SymbolEqualityComparer.Default.Equals(a, prop.Type))))
                continue;

            yield return ((int)order.Value.Value!, prop.Type, prop.Name);
        }
    }

    private static ITypeSymbol ExtractKeyType(ITypeSymbol candidate)
    {
        var props = candidate.GetMembers().OfType<IPropertySymbol>();
        foreach (var prop in props)
        {
            //explicit impl
            if (prop.ExplicitInterfaceImplementations.Length > 0)
            {
                var impl = prop.ExplicitInterfaceImplementations[0];
                if (impl.ContainingType.Name.Equals("IKey", StringComparison.Ordinal)
                    && impl.Name.Equals("Key", StringComparison.Ordinal))
                {
                    return impl.Type;
                }
            }

            //implicit impl
            if (prop.Name.Equals("Key", StringComparison.Ordinal))
            {
                return prop.Type;
            }
        }

        return null;
    }
}