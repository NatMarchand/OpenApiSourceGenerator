using System.Text;
using FluentAssertions;
using NatMarchand.OpenApiSourceGenerator.CodeGeneration;

namespace NatMarchand.OpenApiSourceGenerator.Tests;

public class GeneratedEnumTests
{
    [Fact]
    public void GeneratedEnumWithoutDefaultShouldRenderCorrectly()
    {
        var e = new GeneratedEnum("Color", "the color")
        {
            Values = { "Red", "Green", "Blue" }
        };
        var sb = new StringBuilder();
        e.Render(sb);
        sb.ToString()
            .Should()
            .HaveNoDifferencesWith(@"
///<summary>the color</summary>
[CompilerGenerated]
public enum Color
{
    Red,
    Green,
    Blue,
}");
    }

    [Fact]
    public void GeneratedEnumWithDefaultShouldRenderCorrectly()
    {
        var e = new GeneratedEnum("Color", "the color")
        {
            Values = { "Red", "Green", "Blue" },
            DefaultValue = "Green"
        };
        var sb = new StringBuilder();
        e.Render(sb);
        sb.ToString()
            .Should()
            .HaveNoDifferencesWith(@"
///<summary>the color</summary>
[CompilerGenerated]
public enum Color
{
    Green=0,
    Red,
    Blue,
}");
    }

    [Fact]
    public void GeneratedEnumWithoutSummaryShouldRenderCorrectly()
    {
        var e = new GeneratedEnum("Color")
        {
            Values = { "Red", "Green", "Blue" }
        };
        var sb = new StringBuilder();
        e.Render(sb);
        sb.ToString()
            .Should()
            .HaveNoDifferencesWith(@"
[CompilerGenerated]
public enum Color
{
    Red,
    Green,
    Blue,
}");
    }
}