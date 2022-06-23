using System.Text;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using NatMarchand.OpenApiSourceGenerator.CodeGeneration;

namespace NatMarchand.OpenApiSourceGenerator;

public class DocumentGenerator
{
    private readonly string _key;
    private readonly string _assemblyNamespace;
    private readonly OpenApiDocument _document;

    public DocumentGenerator(string key, string assemblyNamespace, OpenApiDocument document)
    {
        _key = key;
        _assemblyNamespace = assemblyNamespace;
        _document = document;
    }

    public string GenerateDocument()
    {
        var stringBuilder = new StringBuilder();
        var generatedNamespace = new GeneratedNamespace(string.Join(".", _assemblyNamespace, Utils.KeyToNamespace(_key)))
        {
            Usings =
            {
                Constants.Namespace.SystemComponentmodelDataannotations,
                Constants.Namespace.SystemThreading,
                Constants.Namespace.SystemRuntimeCompilerservices,
                Constants.Namespace.MicrosoftAspnetcoreMvc
            }
        };

        foreach (var t in GenerateControllers().Concat(GenerateModels()))
        {
            generatedNamespace.Types.Add(t);
        }
        generatedNamespace.Render(stringBuilder);
        return stringBuilder.ToString();
    }

    private IEnumerable<GeneratedType> GenerateControllers()
    {
        foreach (var tag in _document.Tags)
        {
            var c = new GeneratedClass($"{tag.Name}ControllerContract", tag.Description)
            {
                Modifier = "abstract",
                Inherits = Constants.Type.ControllerBase,
                Attributes =
                {
                    Constants.Attribute.ApiController,
                    $"{Constants.Attribute.ApiExplorerSettings}(GroupName = OpenApiInfos.{Utils.KeyToSymbol(_key)}.Key)"
                }
            };

            foreach (var (path, operation) in _document.Paths.SelectMany(p => p.Value.Operations.Where(o => o.Value.Tags.Contains(tag)), (p, o) => (Path: p, Operation: o)))
            {
                var methodName = operation.Value.OperationId ?? operation.Value.Summary.Replace(" ", "");
                var doc = new XmlDoc(operation.Value.Summary ?? methodName);
                var m = new GeneratedMethod(methodName, doc)
                {
                    Modifier = "abstract",
                    ReturnType = $"{Constants.Type.Task}<{Constants.Type.IActionResult}>",
                    HasBody = false,
                    Attributes =
                    {
                        $@"Http{operation.Key}(""{path.Key}"", Name = ""{methodName}"")"
                    }
                };
                var isDefaultReturnType = true;

                if (!string.IsNullOrWhiteSpace(operation.Value.Description))
                {
                    doc.AddRemarks(operation.Value.Description);
                }

                if (operation.Value.Deprecated)
                {
                    m.Attributes.Add(Constants.Attribute.Obsolete);
                }

                foreach (var response in operation.Value.Responses)
                {
                    if (response.Key == "default")
                    {
                        m.Attributes.Add(Constants.Attribute.ProducesDefaultResponseType);
                        continue;
                    }
                    else if (response.Value.Content.FirstOrDefault() is { Value.Schema: { } } firstContent)
                    {
                        var (returnType, _) = GetTypeOf(firstContent.Value.Schema);
                        m.Attributes.Add($"{Constants.Attribute.ProducesResponseType}(typeof({returnType}), {response.Key})");
                        if (isDefaultReturnType && response.Key[0] is '2')
                        {
                            m.ReturnType = $"{Constants.Type.Task}<{Constants.Type.ActionResult}<{returnType}>>";
                            isDefaultReturnType = false;
                        }
                    }
                    else
                    {
                        m.Attributes.Add($"{Constants.Attribute.ProducesResponseType}({response.Key})");
                    }

                    doc.AddResponse(response.Key, response.Value.Description);
                }

                foreach (var parameter in operation.Value.Parameters)
                {
                    string type;
                    bool isStruct;
                    if (parameter.Schema.Enum.Count > 0 && parameter.Schema.Reference is null)
                    {
                        type = $"{methodName}_{parameter.Name}";
                        isStruct = true;
                        c.InnerTypes.Add(CreateEnum(type, parameter.Schema));
                    }
                    else
                    {
                        (type, isStruct) = GetTypeOf(parameter.Schema);
                    }

                    switch (parameter)
                    {
                        case { Explode: true }:
                            type += "[]";
                            break;
                        case { Required: false } when isStruct:
                            type += "?";
                            break;
                    }

                    var p = new GeneratedMethod.Parameter(parameter.Name, type);
                    if (parameter.Required)
                    {
                        p.Attributes.Add(Constants.Attribute.Required);
                    }

                    switch (parameter.In)
                    {
                        case ParameterLocation.Query:
                            p.Attributes.Add(Constants.Attribute.FromQuery);
                            break;
                        case ParameterLocation.Header:
                            p.Attributes.Add($"{Constants.Attribute.FromHeader}(Name=\"{parameter.Name}\")");
                            break;
                        case ParameterLocation.Path:
                            p.Attributes.Add(Constants.Attribute.FromRoute);
                            break;
                    }
                    m.AddParameter(p, parameter.Description);
                }

                if (operation.Value.RequestBody is { } requestBody)
                {
                    var (type, isStruct) = GetTypeOf(requestBody.Content.First().Value.Schema);
                    if (requestBody.Required && isStruct)
                    {
                        type += "?";
                    }

                    var parameter = new GeneratedMethod.Parameter("body", type)
                    {
                        Attributes = { Constants.Attribute.FromBody }
                    };
                    m.AddParameter(parameter, requestBody.Description);
                }

                m.AddParameter(new GeneratedMethod.Parameter("cancellationToken", Constants.Type.CancellationToken), "Cancellation token");
                c.Members.Add(m);
            }

            yield return c;
        }
    }

    private IEnumerable<GeneratedType> GenerateModels()
    {
        foreach (var schema in _document.Components.Schemas)
        {
            if (schema.Key is Constants.Type.ProblemDetails)
            {
                continue;
            }

            if (schema.Value.Enum.Any())
            {
                yield return CreateEnum(schema.Key, schema.Value);
            }
            else
            {
                yield return CreateClass(schema.Key, schema.Value);
            }
        }
    }

    private static GeneratedClass CreateClass(string name, OpenApiSchema schema)
    {
        var t = new GeneratedClass(name, schema.Description);
        foreach (var p in schema.Properties)
        {
            var isRequired = schema.Required.Contains(p.Key);

            string type;
            bool isStruct;
            if (p.Value.Enum.Count > 0 && p.Value.Reference is null)
            {
                type = $"{name}_{p.Key}";
                isStruct = true;
                t.InnerTypes.Add(CreateEnum(type, p.Value));
            }
            else
            {
                (type, isStruct) = GetTypeOf(p.Value);
            }

            if (!isRequired && isStruct)
            {
                type += "?";
            }


            var generatedProperty = new GeneratedProperty(p.Key, p.Value.Description)
            {
                ReturnType = type
            };

            if (isRequired)
            {
                generatedProperty.Attributes.Add(Constants.Attribute.Required);
            }

            if (p.Value.Deprecated)
            {
                generatedProperty.Attributes.Add(Constants.Attribute.Obsolete);
            }

            t.Members.Add(generatedProperty);
        }

        return t;
    }

    private static GeneratedEnum CreateEnum(string name, OpenApiSchema schema)
    {
        var e = new GeneratedEnum(name, schema.Description)
        {
            DefaultValue = schema.Default is OpenApiString s ? s.Value : null
        };

        foreach (var v in schema.Enum.OfType<OpenApiString>())
        {
            e.Values.Add(v.Value);
        }

        return e;
    }

    private static (string type, bool isStruct) GetTypeOf(OpenApiSchema s)
    {
        return s.Type switch
        {
            "string" when s.Format == "uuid" => (Constants.Type.Guid, true),
            "string" when s.Enum.Count > 0 && s.Reference is { } enumReference => (enumReference.Id, true),
            "string" when s.Format == "date-time" => (Constants.Type.DateTime, true),
            "string" when s.Format == "date" => (Constants.Type.DateOnly, true),
            "string" when s.Format is "byte" or "binary" => (Constants.Type.ByteArray, false),
            "string" => (Constants.Type.String, false),
            "integer" when s.Format == "int64" => (Constants.Type.Long, true),
            "integer" => (Constants.Type.Int, true),
            "number" when s.Format == "double" => (Constants.Type.Double, true),
            "number" => (Constants.Type.Float, true),
            "boolean" => (Constants.Type.Boolean, true),
            "array" => ($"{GetTypeOf(s.Items).type}[]", false),
            _ when s.Reference is { } => (s.Reference.Id, false),
            _ => ("object", false)
        };
    }
}