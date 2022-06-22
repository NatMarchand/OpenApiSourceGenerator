using System.Text;

namespace NatMarchand.OpenApiSourceGenerator.CodeGeneration;

public class GeneratedField : GeneratedMember, GeneratedMember.IHasInitializer
{
    public string? InitialValue { get; set; }

    public GeneratedField(string name, XmlDoc? documentation = default)
        : base(name, documentation)
    {
    }

    public void RenderInitializer(StringBuilder sb)
    {
        if (InitialValue is { })
        {
            sb.AppendLine($" = {InitialValue};");
        }
    }
}