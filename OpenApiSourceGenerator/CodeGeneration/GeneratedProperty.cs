using System.Text;

namespace NatMarchand.OpenApiSourceGenerator.CodeGeneration;

public class GeneratedProperty : GeneratedMember, GeneratedMember.IHasBody, GeneratedMember.IHasInitializer
{
    public bool IsReadOnly { get; set; } = false;
    public string? InitialValue { get; set; }

    public GeneratedProperty(string name, XmlDoc? documentation = default)
        : base(name, documentation)
    {
    }

    public void RenderBody(StringBuilder sb)
    {
        if (IsReadOnly)
        {
            sb.AppendLine("{ get; }");
        }
        else
        {
            sb.AppendLine("{ get; set; }");
        }
    }

    public void RenderInitializer(StringBuilder sb)
    {
        if (InitialValue is { })
        {
            sb.AppendLine($" = {InitialValue};");
        }
    }
}