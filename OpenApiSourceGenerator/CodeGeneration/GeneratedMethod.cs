using System.Text;

namespace NatMarchand.OpenApiSourceGenerator.CodeGeneration;

public class GeneratedMethod : GeneratedMember, GeneratedMember.IHasBody, GeneratedMember.IHasParameters
{
    private readonly List<Parameter> _parameters = new();

    public bool HasBody { get; set; }

    public GeneratedMethod(string name, XmlDoc? documentation = default)
        : base(name, documentation)
    {
    }

    public void AddParameter(Parameter parameter, string? description = default)
    {
        _parameters.Add(parameter);
        Documentation.AddParam(parameter.Name, description);
    }

    public void RenderBody(StringBuilder sb)
    {
        if (HasBody)
        {
            sb.AppendLine("{}");
        }
        else
        {
            sb.AppendLine(";");
        }
    }

    public void RenderParameters(StringBuilder sb)
    {
        sb.AppendLine("(");
        sb.AppendLine(string.Join(",\r\n", _parameters.Select(p =>
        {
            var s = string.Empty;
            if (p.Attributes.Any())
            {
                s += $"[{string.Join(", ", p.Attributes)}]";
            }
            return s + $"{p.Type} {p.Name}";
        })));
        sb.AppendLine(")");
    }

    public class Parameter
    {
        public string Name { get; }
        public string Type { get; }
        public IList<string> Attributes { get; } = new List<string>();

        public Parameter(string name, string type)
        {
            Name = name;
            Type = type;
        }
    }
}