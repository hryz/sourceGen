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
    public class MySourceGenerator : ISourceGenerator
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
                if (ModelExtensions.GetDeclaredSymbol(semanticModel, candidateTypeNode) is ITypeSymbol typeSymbol)
                {
                    var props = ExtractFieldIndexes(typeSymbol).ToArray();
                    var @class = MemberGenerator.CreateClass(typeSymbol, props);
                    context.AddSource(typeSymbol.Name + "Extensions", SourceText.From(@class, Encoding.UTF8));
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
