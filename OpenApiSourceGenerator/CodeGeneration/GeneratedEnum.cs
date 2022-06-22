using System.Text;

namespace NatMarchand.OpenApiSourceGenerator.CodeGeneration;

public class GeneratedEnum : GeneratedType
{
    public override string Kind
    {
        get => "enum";
        set => throw new NotSupportedException();
    }

    public ISet<string> Values { get; } = new HashSet<string>();
    public string? DefaultValue { get; set; }

    public GeneratedEnum(string name, XmlDoc? documentation = default)
        : base(name, documentation)
    {
    }

    protected override void RenderInner(StringBuilder sb)
    {
        base.RenderInner(sb);

        if (DefaultValue is { })
        {
            sb.AppendLine($"{DefaultValue}=0,");
        }

        foreach (var value in Values)
        {
            if (value == DefaultValue) { continue; }

            sb.AppendLine($"{value},");
        }
    }
}