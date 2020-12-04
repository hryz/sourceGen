using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace gen
{
    [Generator]
    public class DbQueryGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new MySyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not MySyntaxReceiver receiver)
                return;

            var compilation = context.Compilation;
            foreach (var candidateTypeNode in receiver.Candidates)
            {
                var semanticModel = compilation.GetSemanticModel(candidateTypeNode.SyntaxTree);
                if (semanticModel.GetDeclaredSymbol(candidateTypeNode) is ITypeSymbol typeSymbol)
                {
                    var keyType = ExtractKeyType(typeSymbol);
                    if(keyType == null) 
                        continue; // no implementation of IKey
                    
                    var props = ExtractFieldIndexes(typeSymbol).ToArray();
                    var readerExt = ReaderBuildersGenerator.CreateClass(typeSymbol, keyType, props);
                    var writerExt = WriterBuildersGenerator.CreateClass(typeSymbol, keyType, props);
                    context.AddSource(typeSymbol.Name + "ReadExtensions", SourceText.From(readerExt, Encoding.UTF8));
                    context.AddSource(typeSymbol.Name + "WriteExtensions", SourceText.From(writerExt, Encoding.UTF8));
                }
            }
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
                
                if(attr == null) continue;

                var order = attr.NamedArguments
                    .SingleOrDefault(x => x.Key.Equals("Order", StringComparison.Ordinal));

                if (order.Key == null) continue;

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

    internal class MySyntaxReceiver : ISyntaxReceiver
    {
        public List<TypeDeclarationSyntax> Candidates { get; } = new();
        
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is not TypeDeclarationSyntax type) 
                return;

            var candidates =
                from attributeList in type.AttributeLists
                from attribute in attributeList.Attributes
                let name = attribute.Name.ToFullString()
                where name.Equals("DataContract", StringComparison.Ordinal)
                select type;
            
            Candidates.AddRange(candidates);
        }
    }
}
