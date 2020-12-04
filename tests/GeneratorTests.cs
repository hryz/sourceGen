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

namespace tests
{
    public class GeneratorTests
    {
        [Fact]
        public async Task ItShallBuildSuccessfully()
        {
            // Given
            string sourceCode = @"
using System.Runtime.Serialization;
using db;

namespace net5.InMemoryStore.ClientCode 
{
    public interface IInMemoryDatabaseReader
    {
        ICacheReader<int, ModelA> ModelA { get; }
        ICacheReader<int, ModelB> ModelB { get; }
    }    

    [DataContract]
    public class ModelA : IKey<int>
    {
        int IKey<int>.Key => A;
        [DataMember(Order = 0)] public int A { get; set; }
        [DataMember(Order = 1)] public string B { get; set; } = """";
        [DataMember(Order = 2)] public bool C { get; set; }
    }

    [DataContract]
    public class ModelB : IKey<int>
    {
        public int Key => A;
        [DataMember(Order = 0)] public int A { get; set; }
        [DataMember(Order = 1)] public bool B { get; set; }
        [DataMember(Order = 2)] public bool C { get; set; }
        [DataMember(Order = 3)] public bool D { get; set; }
    }

    public class Root
    {
        public void Run()
        {
            var store = new object();
            
            //from the client side
            var reader = (IInMemoryDatabaseReader) store;
            var m0 = reader.ModelA.FindByKey(1);
            var m1 = reader.ModelA.FindByA(1).Now();
            var m2 = reader.ModelA.FindByB(""b2"").AndByC(false).Now();
        }
    }
}
";
            var outputAssembly = Path.Combine(Path.GetTempPath(), $"Test_{nameof(ItShallBuildSuccessfully)}.dll");

            // When
            var result = await WhenTheSourceCodeIsCompiled(sourceCode, outputAssembly);

            // Then
            result.Success.Should().BeTrue(result.ToString());
            var ctx = new AssemblyLoadContext(nameof(ItShallBuildSuccessfully), true);
            var loadedAssembly = ctx.LoadFromAssemblyPath(outputAssembly);
            
            loadedAssembly.GetType("net5.InMemoryStore.ClientCode.ModelAReadExtensions")
                .Should().NotBeNull()
                .And.Subject.GetMember("FindByA").Length.Should().BeGreaterOrEqualTo(1);
            
            loadedAssembly.GetType("net5.InMemoryStore.ClientCode.ModelAWriteExtensions")
                .Should().NotBeNull()
                .And.Subject.GetMember("IndexByA").Length.Should().BeGreaterOrEqualTo(1);
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
                    MetadataReference.CreateFromFile(typeof(DataContractAttribute).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Cache<,>).Assembly.Location)
                })
                .WithAnalyzerReferences(new[]{
                    new AnalyzerFileReference(typeof(DbQueryGenerator).Assembly.Location, new DataBuilderGeneratorAnalyzerLoader())
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
                return typeof(DbQueryGenerator).Assembly;
            }
        }
    }
}