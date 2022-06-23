using System.Text;

namespace NatMarchand.OpenApiSourceGenerator.CodeGeneration;

public abstract class GeneratedObject
{
    public void Render(StringBuilder sb)
    {
        RenderBefore(sb);
        RenderInner(sb);
        RenderAfter(sb);
    }

    protected virtual void RenderBefore(StringBuilder sb)
    {
        if (this is IHasDoc { Documentation.HasValues: true } hasDoc)
        {
            sb.AppendLine(hasDoc.Documentation.ToString());
        }
    }

    protected virtual void RenderInner(StringBuilder sb)
    {
    }

    protected virtual void RenderAfter(StringBuilder sb)
    {
    }

    public interface IHasDoc
    {
        XmlDoc Documentation { get; }
    }
}