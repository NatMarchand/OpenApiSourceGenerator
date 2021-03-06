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
/// <summary>the color</summary>
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
/// <summary>the color</summary>
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


public class XmlDocTests
{
    [Fact]
    public void MultilineCommentShouldBePreserved()
    {
        XmlDoc d = @"
Hello there

General Kenobi"!;

        d.ToString()
            .Should()
            .Be(@"/// <summary>
/// Hello there
/// 
/// General Kenobi
/// </summary>");
    }


    [Fact]
    public void EmptyShouldBeEmpty()
    {
        XmlDoc d = new XmlDoc(string.Empty);

        d.ToString()
            .Should()
            .Be(string.Empty);
    }

    [Fact]
    public void ReservedCharsShouldBeEscaped()
    {
        XmlDoc d = new XmlDoc("& <3 '");
        d.ToString()
            .Should()
            .Be(@"/// <summary>&amp; &lt;3 &apos;</summary>");
    }
}