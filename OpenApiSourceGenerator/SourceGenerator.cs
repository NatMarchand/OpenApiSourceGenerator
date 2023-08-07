using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NSwag.CodeGeneration.CSharp;
using NSwag;
using NSwag.CodeGeneration.OperationNameGenerators;

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
        var cancellationToken = context.CancellationToken;

        var definitionFiles = context.AdditionalFiles.Where(at => DefinitionFileExtensions.Contains(Path.GetExtension(at.Path)));
        var assemblyNamespace = context.Compilation.SyntaxTrees
            .Select(t => context.AnalyzerConfigOptions.GetOptions(t).TryGetValue("build_property.rootnamespace", out var n) ? n : default)
            .FirstOrDefault();

        if (assemblyNamespace is null)
        {
            return;
        }

        foreach (var definitionFile in definitionFiles)
        {
            var text = definitionFile.GetText(cancellationToken);
            if (text is null) continue;

            OpenApiDocument document;

            switch (Path.GetExtension(definitionFile.Path))
            {
                case ".yml":
                case ".yaml":
                    document = OpenApiYamlDocument.FromYamlAsync(text.ToString(), cancellationToken).GetAwaiter().GetResult();
                    break;
                case ".json":
                    document = OpenApiDocument.FromJsonAsync(text.ToString(), cancellationToken).GetAwaiter().GetResult();
                    break;
                default: continue;
            }

            const string optionsPrefix = "build_metadata.additionalfiles.NSwag_";

            var generatorSettings = new CSharpControllerGeneratorSettings
            {
                CSharpGeneratorSettings =
                {
                    Namespace = assemblyNamespace
                }
            };

            var generatorSettingsProperties = generatorSettings.GetType()
                .GetProperties()
                .ToDictionary(p => p.Name, StringComparer.InvariantCultureIgnoreCase);
            var codeGeneratorSettingsProperties = generatorSettings.CodeGeneratorSettings.GetType()
                .GetProperties()
                .ToDictionary(p => p.Name, StringComparer.InvariantCultureIgnoreCase);

            var definitionFileOptions = context.AnalyzerConfigOptions.GetOptions(definitionFile);
            foreach (var key in definitionFileOptions.Keys
                         .Where(k => k.StartsWith(optionsPrefix, StringComparison.InvariantCultureIgnoreCase))
                         .Where(k => definitionFileOptions.TryGetValue(k, out var v) && !string.IsNullOrWhiteSpace(v)))
            {
                var nswagOption = key.Substring(optionsPrefix.Length);
                if (definitionFileOptions.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
                {
                    object? target;
                    if (generatorSettingsProperties.TryGetValue(nswagOption, out var property))
                    {
                        target = generatorSettings;
                    }
                    else if (codeGeneratorSettingsProperties.TryGetValue(nswagOption, out property))
                    {
                        target = generatorSettings.CodeGeneratorSettings;
                    }
                    else if (nswagOption.Equals("OperationGenerationMode", StringComparison.InvariantCultureIgnoreCase))
                    {
                        generatorSettings.OperationNameGenerator = value switch
                        {
                            "MultipleClientsFromPathSegments" => new MultipleClientsFromPathSegmentsOperationNameGenerator(),
                            "MultipleClientsFromFirstTagAndPathSegments" => new MultipleClientsFromFirstTagAndPathSegmentsOperationNameGenerator(),
                            "MultipleClientsFromFirstTagAndOperationId" => new MultipleClientsFromFirstTagAndOperationIdGenerator(),
                            "MultipleClientsFromFirstTagAndOperationName" => new MultipleClientsFromFirstTagAndOperationNameGenerator(),
                            "SingleClientFromOperationId" => new SingleClientFromOperationIdOperationNameGenerator(),
                            "SingleClientFromPathSegments" => new SingleClientFromPathSegmentsOperationNameGenerator(),
                            _ => new MultipleClientsFromOperationIdOperationNameGenerator()
                        };
                        continue;
                    }
                    else
                    {
                        continue;
                    }

                    if (property.PropertyType == typeof(string))
                    {
                        property.SetValue(target, value);
                    }
                    else if (property.PropertyType == typeof(bool))
                    {
                        property.SetValue(target, bool.Parse(value));
                    }
                    else if (property.PropertyType == typeof(string[]))
                    {
                        property.SetValue(target, value.Split(';'));
                    }
                    else if (property.PropertyType.IsEnum)
                    {
                        property.SetValue(target, Enum.Parse(property.PropertyType, value));
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(generatorSettings.CodeGeneratorSettings.TemplateDirectory))
            {
                generatorSettings.CodeGeneratorSettings.TemplateDirectory = Path.GetFullPath(generatorSettings.CodeGeneratorSettings.TemplateDirectory);
            }

            var generator = new CSharpControllerGenerator(document, generatorSettings);
            var generatedCode = generator.GenerateFile();
            context.AddSource($"{Path.GetFileNameWithoutExtension(definitionFile.Path)}.generated.cs", SourceText.From(generatedCode, Encoding.UTF8));
        }
    }
}
