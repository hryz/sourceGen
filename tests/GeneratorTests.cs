using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using gen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace tests
{
    public class GeneratorTests
    {
        [Fact]
        public async Task ItShallBuildSuccessfully()
        {
            // Given
            string sourceCode = @"
using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace net5 
{

    [DataContract]
    public class SourceModel
    {
        [DataMember(Order = 0)] public int A { get; init; }
        [DataMember(Order = 1)] public string? B { get; init; }
        [DataMember(Order = 2)] public bool C { get; init; }
        [DataMember(Order = 3)] public DateTime D { get; init; }
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
}
";
            var outputAssembly = Path.Combine(Path.GetTempPath(), $"Test_{nameof(ItShallBuildSuccessfully)}.dll");

            // When
            var result = await WhenTheSourceCodeIsCompiled(sourceCode, outputAssembly);

            // Then
            result.Success.Should().BeTrue();
            var ctx = new AssemblyLoadContext(nameof(ItShallBuildSuccessfully), true);
            var loadedAssembly = ctx.LoadFromAssemblyPath(outputAssembly);
            loadedAssembly.GetType("net5.SourceModelExtensions")
                .Should().NotBeNull()
                .And.Subject.GetMember("FindByA").Length.Should().BeGreaterOrEqualTo(1);
        }

        private async Task<EmitResult> WhenTheSourceCodeIsCompiled(string sourceCode, string outputAssembly, [CallerMemberName] string compilationAssemblyName = "TestCompilation")
        {
            var host = MefHostServices.Create(MefHostServices.DefaultAssemblies);
            var projectInfo = ProjectInfo.Create(
                ProjectId.CreateNewId(),
                VersionStamp.Create(),
                compilationAssemblyName,
                compilationAssemblyName,
                LanguageNames.CSharp
                )
                .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).WithNullableContextOptions(NullableContextOptions.Enable))
                .WithParseOptions(new CSharpParseOptions(LanguageVersion.Preview))
                .WithMetadataReferences(new[]
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location)!, "netstandard.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location)!, "System.Runtime.dll")),
                    MetadataReference.CreateFromFile(typeof(DataContractAttribute).Assembly.Location)
                    //MetadataReference.CreateFromFile(typeof(GenerateDataBuilderAttribute).Assembly.Location)
                })
                .WithAnalyzerReferences(new[]{
                    new AnalyzerFileReference(typeof(MySourceGenerator).Assembly.Location, new DataBuilderGeneratorAnalyzerLoader())
                    });

            var compilation = await new AdhocWorkspace(host)
                .CurrentSolution
                .AddProject(projectInfo)
                .AddDocument(DocumentId.CreateNewId(projectInfo.Id, compilationAssemblyName + ".cs"), compilationAssemblyName + ".cs", SourceText.From(sourceCode, Encoding.UTF8))
                .GetProject(projectInfo.Id)!
                .GetCompilationAsync();

            var result = compilation!.Emit(outputAssembly);
            return result;
        }

        private class DataBuilderGeneratorAnalyzerLoader : IAnalyzerAssemblyLoader
        {
            public void AddDependencyLocation(string fullPath)
            {
            }

            public Assembly LoadFromPath(string fullPath)
            {
                return typeof(MySourceGenerator).Assembly;
            }
        }
    }
}