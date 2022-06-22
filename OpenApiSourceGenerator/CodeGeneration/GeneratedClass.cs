using System.Text;

namespace NatMarchand.OpenApiSourceGenerator.CodeGeneration;

public class GeneratedClass : GeneratedType
{
    public override string Kind { get; set; } = "partial class";
    public IList<GeneratedMember> Members { get; } = new List<GeneratedMember>();
    public GeneratedClass(string name, XmlDoc? documentation = default) : base(name, documentation)
    {
    }

    protected override void RenderInner(StringBuilder sb)
    {
        foreach (var m in Members.OrderBy(m => m.GetType().Name))
        {
            m.Render(sb);
        }
        base.RenderInner(sb);
    }
}