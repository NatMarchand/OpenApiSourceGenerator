using System.Text;
using Microsoft.OpenApi.Models;
using NatMarchand.OpenApiSourceGenerator.CodeGeneration;

namespace NatMarchand.OpenApiSourceGenerator;

public class DocumentsGenerator
{
    public IEnumerable<(string fileName, string content)> GenerateSourceFiles(string assemblyNamespace, IEnumerable<(string Path, string? Content)> definitionFiles)
    {
        var infos = new Dictionary<string, OpenApiDocument>();
        foreach (var file in definitionFiles)
        {
            if (file.Content is null)
            {
                continue;
            }
            var key = Path.GetFileNameWithoutExtension(file.Path);
            var reader = new Microsoft.OpenApi.Readers.OpenApiStringReader();
            var doc = reader.Read(file.Content, out var diagnostic);
            if (diagnostic.Errors.Any())
            {
                continue;
            }
            infos.Add(key, doc);

            var documentGenerator = new DocumentGenerator(key, assemblyNamespace, doc);
            yield return ($"{key}.cs", documentGenerator.GenerateDocument());
        }
        yield return ($"OpenApiInfos.cs", GenerateOpenApiInfos(assemblyNamespace, infos));
    }


    private string GenerateOpenApiInfos(string assemblyNamespace, IReadOnlyDictionary<string, OpenApiDocument> documents)
    {
        var stringBuilder = new StringBuilder();
        var generatedNamespace = new GeneratedNamespace(assemblyNamespace)
        {
            Usings =
            {
                Constants.Namespace.SystemRuntimeCompilerservices,
                Constants.Namespace.MicrosoftOpenapiModels
            }
        };

        var o = new GeneratedClass("OpenApiInfos", "Object containing metadata from all generated apis")
        {
            Modifier = "static"
        };

        var allApisInitialValue = $"new Dictionary<{Constants.Type.String}, {Constants.Type.OpenApiInfo}> {{\r\n";
        foreach (var pair in documents)
        {
            allApisInitialValue += $@"[{Utils.KeyToSymbol(pair.Key)}.Key] = {Utils.KeyToSymbol(pair.Key)}.Infos,{Environment.NewLine}";
            o.InnerTypes.Add(new GeneratedClass(Utils.KeyToSymbol(pair.Key), $"Metadata for API {pair.Value.Info.Title}")
            {
                Modifier = "static",
                Members =
                {
                    new GeneratedField("Key","The API document key")
                    {
                        Modifier = "const",
                        ReturnType = Constants.Type.String,
                        InitialValue =  @$"""{pair.Key}"""
                    },
                    new GeneratedProperty("Infos", "The API infos")
                    {
                        IsReadOnly = true,
                        Modifier = "static",
                        ReturnType = Constants.Type.OpenApiInfo,
                        InitialValue = @$"new () {{ Title = ""{pair.Value.Info.Title}"", Version = ""{pair.Value.Info.Version}"" }}"
                    }
                }
            });
        }
        allApisInitialValue += "}";
        o.Members.Insert(0, new GeneratedField("AllApis", "All generated APIs")
        {
            Modifier = "static readonly",
            ReturnType = $"IReadOnlyDictionary<{Constants.Type.String}, {Constants.Type.OpenApiInfo}>",
            InitialValue = allApisInitialValue
        });
        generatedNamespace.Types.Add(o);
        generatedNamespace.Render(stringBuilder);
        return stringBuilder.ToString();
    }
}