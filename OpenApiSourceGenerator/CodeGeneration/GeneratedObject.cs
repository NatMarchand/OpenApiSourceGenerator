using System.Text;

namespace NatMarchand.OpenApiSourceGenerator.CodeGeneration;

public abstract class GeneratedObject
{
    public XmlDoc Documentation { get; }
    protected GeneratedObject(XmlDoc? documentation)
    {
        Documentation = documentation ?? new XmlDoc(string.Empty);
    }

    public void Render(StringBuilder sb)
    {
        RenderBefore(sb);
        RenderInner(sb);
        RenderAfter(sb);
    }

    protected virtual void RenderBefore(StringBuilder sb)
    {
        if (Documentation is { HasValues: true })
        {
            sb.AppendLine(Documentation.ToString());
        }
    }

    protected virtual void RenderInner(StringBuilder sb)
    {
    }

    protected virtual void RenderAfter(StringBuilder sb)
    {
    }
}