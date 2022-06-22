using System.Text;

namespace NatMarchand.OpenApiSourceGenerator.CodeGeneration;

public class GeneratedNamespace : GeneratedObject
{
    public string Name { get; }
    public IList<string> Usings { get; } = new List<string>();
    public IList<GeneratedType> Types { get; } = new List<GeneratedType>();

    public GeneratedNamespace(string name)
        : base(default)
    {
        Name = name;
    }

    protected override void RenderBefore(StringBuilder sb)
    {
        base.RenderBefore(sb);
        sb.AppendLine($"namespace {Name} {{");
        foreach (var @using in Usings)
        {
            sb.AppendLine($"using {@using};");
        }
    }

    protected override void RenderInner(StringBuilder sb)
    {
        base.RenderInner(sb);
        foreach (var type in Types)
        {
            type.Render(sb);
        }
    }

    protected override void RenderAfter(StringBuilder sb)
    {
        sb.AppendLine("}");
        base.RenderAfter(sb);
    }
}