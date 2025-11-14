using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using db;
using FluentAssertions;
using gen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace tests;

public class GeneratorTests
{
    [Fact]
    public async Task Generated_Code_Should_Pass_Complication()
    {
        // Given
        string sourceCode = @"
            using System.Runtime.Serialization;
            using System.Collections.Generic;
            using db;

            namespace client
            {
                [DataContract]
                public class ModelA : IKey<int>
                {
                    int IKey<int>.Key => A;
                    [DataMember(Order = 0)] public int A { get; set; }
                    [DataMember(Order = 1)] public string B { get; set; } = """";
                    [DataMember(Order = 2)] public bool C { get; set; }
                    [DataMember(Order = 3)] public List<int> Bla { get; set; } = new();
                }

                [DataContract]
                public class ModelB : IKey<int>
                {
                    public int Key => A;
                    [DataMember(Order = 0)] public int A { get; set; }
                }
            }";
        var outputAssembly = Path.Combine(Path.GetTempPath(), 
            $"Test_{nameof(Generated_Code_Should_Pass_Complication)}.dll");

        // When
        var result = await WhenTheSourceCodeIsCompiled(sourceCode, outputAssembly);

        // Then
        result.Success.Should().BeTrue(
            String.Join(Environment.NewLine,result.Diagnostics.Select(x => x.ToString())));
        var ctx = new AssemblyLoadContext(nameof(Generated_Code_Should_Pass_Complication), true);
        var loadedAssembly = ctx.LoadFromAssemblyPath(outputAssembly);
            
        loadedAssembly.GetType("client.ModelAReadExtensions")
            .Should().NotBeNull()
            .And.Subject.GetMember("FindByA").Length.Should().BeGreaterThanOrEqualTo(1);
            
        loadedAssembly.GetType("client.ModelAWriteExtensions")
            .Should().NotBeNull()
            .And.Subject.GetMember("IndexByA").Length.Should().BeGreaterThanOrEqualTo(1);
    }

    private async Task<EmitResult> WhenTheSourceCodeIsCompiled(
        string sourceCode, 
        string outputAssembly, 
        [CallerMemberName] string compilationAssemblyName = "TestCompilation")
    {
        var host = MefHostServices.Create(MefHostServices.DefaultAssemblies);
        var frameworkLocation = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var projectInfo = ProjectInfo.Create(
                ProjectId.CreateNewId(),
                VersionStamp.Create(),
                compilationAssemblyName,
                compilationAssemblyName,
                LanguageNames.CSharp
            )
            .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithNullableContextOptions(NullableContextOptions.Enable))
            .WithParseOptions(new CSharpParseOptions(LanguageVersion.Latest))
            .WithMetadataReferences(new[]
            {
                //framework assemblies
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(frameworkLocation, "netstandard.dll")),
                MetadataReference.CreateFromFile(Path.Combine(frameworkLocation, "System.Runtime.dll")),
                //project dependencies assemblies
                MetadataReference.CreateFromFile(typeof(DataContractAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Cache<,>).Assembly.Location)
            })
            .WithAnalyzerReferences(new[]{
                //our generator
                new AnalyzerFileReference(
                    typeof(DbQueryGenerator).Assembly.Location, 
                    new DbQueryGeneratorAnalyzerLoader())
            });

        var compilation = await new AdhocWorkspace(host)
            .CurrentSolution
            .AddProject(projectInfo)
            .AddDocument(
                documentId: DocumentId.CreateNewId(projectInfo.Id, compilationAssemblyName + ".cs"), 
                name: compilationAssemblyName + ".cs", 
                text: SourceText.From(sourceCode, Encoding.UTF8))
            .GetProject(projectInfo.Id)!
            .GetCompilationAsync();

        return compilation!.Emit(outputAssembly);
    }

    private class DbQueryGeneratorAnalyzerLoader : IAnalyzerAssemblyLoader
    {
        public void AddDependencyLocation(string fullPath) { }
        public Assembly LoadFromPath(string fullPath) => typeof(DbQueryGenerator).Assembly;
    }
}