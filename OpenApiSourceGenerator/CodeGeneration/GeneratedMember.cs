using System.Text;

namespace NatMarchand.OpenApiSourceGenerator.CodeGeneration;

public abstract class GeneratedMember : GeneratedObject
{
    public string Name { get; }
    public string Visibility { get; set; } = "public";
    public string? Modifier { get; set; }
    public string ReturnType { get; set; } = "object";
    public IList<string> Attributes { get; } = new List<string>();
    protected GeneratedMember(string name, XmlDoc? documentation)
        : base(documentation)
    {
        Name = name;
    }

    protected sealed override void RenderBefore(StringBuilder sb)
    {
        base.RenderBefore(sb);

        foreach (var attribute in Attributes)
        {
            sb.AppendLine($"[{attribute}]");
        }

        sb.AppendLine(string.Join(" ", new[] { Visibility, Modifier, ReturnType, Name }.Where(x => !string.IsNullOrWhiteSpace(x))));
        if (this is IHasParameters hasParameters)
        {
            hasParameters.RenderParameters(sb);
        }
    }

    protected sealed override void RenderInner(StringBuilder sb)
    {
        if (this is IHasBody hasBody)
        {
            hasBody.RenderBody(sb);
        }
    }

    protected sealed override void RenderAfter(StringBuilder sb)
    {
        if (this is IHasInitializer hasInitializer)
        {
            hasInitializer.RenderInitializer(sb);
        }
        base.RenderAfter(sb);
    }

    public interface IHasBody
    {
        void RenderBody(StringBuilder sb);
    }

    public interface IHasParameters
    {
        void RenderParameters(StringBuilder sb);
    }

    public interface IHasInitializer
    {
        void RenderInitializer(StringBuilder sb);
    }
}