using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace NatMarchand.OpenApiSourceGenerator;

[Generator]
public class SourceGenerator : ISourceGenerator
{
    private static readonly HashSet<string> DefinitionFileExtensions = new(StringComparer.InvariantCultureIgnoreCase) { ".yaml", ".yml", ".json" };

    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var definitionFiles = context.AdditionalFiles.Where(at => DefinitionFileExtensions.Contains(Path.GetExtension(at.Path)));
        var assemblyNamespace = context.Compilation.SyntaxTrees
            .Select(t => context.AnalyzerConfigOptions.GetOptions(t).TryGetValue("build_property.rootnamespace", out var n) ? n : default)
            .FirstOrDefault();
        
        if (assemblyNamespace is null)
        {
            return;
        }

        var toto = new DocumentsGenerator();
        var files = definitionFiles.Select(f => (f.Path, f.GetText(context.CancellationToken)?.ToString())).ToList();
        var sources = toto.GenerateSourceFiles(assemblyNamespace, files);
        foreach (var (fileName, content) in sources)
        {
            context.AddSource(fileName, SourceText.From(content, Encoding.UTF8));
        }
    }
}