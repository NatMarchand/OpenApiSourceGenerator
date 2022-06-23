using System.Text;

namespace NatMarchand.OpenApiSourceGenerator.CodeGeneration;

public abstract class GeneratedType : GeneratedObject, GeneratedObject.IHasDoc
{
    public XmlDoc Documentation { get; }
    public string Name { get; }
    public abstract string Kind { get; set; }
    public string Visibility { get; set; } = "public";
    public string? Modifier { get; set; }
    public string? Inherits { get; set; }
    public IList<string> Attributes { get; } = new List<string>
    {
        "CompilerGenerated"
    };
    public IList<GeneratedType> InnerTypes { get; } = new List<GeneratedType>();
    protected GeneratedType(string name, XmlDoc? documentation = default)
    {
        Name = name;
        Documentation = documentation ?? new XmlDoc(string.Empty);
    }

    protected override void RenderBefore(StringBuilder sb)
    {
        base.RenderBefore(sb);

        foreach (var attribute in Attributes)
        {
            sb.AppendLine($"[{attribute}]");
        }

        sb.AppendLine(string.Join(" ", new[] { Visibility, Modifier, Kind, Name }.Where(x => !string.IsNullOrWhiteSpace(x))));
        if (Inherits is { })
        {
            sb.AppendLine($": {Inherits}");
        }
        sb.AppendLine("{");
    }

    protected override void RenderInner(StringBuilder sb)
    {
        base.RenderInner(sb);
        foreach (var innerType in InnerTypes)
        {
            innerType.Render(sb);
        }
    }

    protected override void RenderAfter(StringBuilder sb)
    {
        sb.AppendLine("}");
        base.RenderAfter(sb);
    }
}