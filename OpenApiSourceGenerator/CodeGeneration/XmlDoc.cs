namespace NatMarchand.OpenApiSourceGenerator.CodeGeneration;

public class XmlDoc
{
    private readonly List<string> _lines = new();
    public bool HasValues => _lines.Count > 0;

    public XmlDoc(string summary)
    {
        Add(Constants.XmlDoc.SummarySection, summary);
    }

    public override string ToString()
    {
        return string.Join("\r\n", _lines);
    }

    public XmlDoc Add(string type, string? content, params KeyValuePair<string, string>[] attributes)
    {
        content ??= string.Empty;
        var split = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
        var serializedAttributes = string.Join(" ", attributes.Select(p => $"{p.Key}=\"{p.Value}\""));
        switch (split)
        {
            case { Length: 0 } when attributes.Length > 0:
                _lines.Add($"/// <{type} {serializedAttributes}> </{type}>");
                break;
            case { Length: 1 } when attributes.Length == 0:
                _lines.Add($"/// <{type}> {split[0]} </{type}>");
                break;
            case { Length: 1 } when attributes.Length > 0:
                _lines.Add($"/// <{type} {serializedAttributes}> {split[0]} </{type}>");
                break;
            case { Length: > 1 } when attributes.Length == 0:
                _lines.AddRange(split.Select(x => $"/// {x}").Prepend($"/// <{type}>").Append($"/// </{type}>"));
                break;
            case { Length: > 1 } when attributes.Length > 0:
                _lines.AddRange(split.Select(x => $"/// {x}").Prepend($"/// <{type} {serializedAttributes}>").Append($"/// </{type}>"));
                break;
        }
        return this;
    }

    public static implicit operator XmlDoc?(string? summary)
    {
        return summary == null ? default : new XmlDoc(summary);
    }
}

public static class XmlDocExtensions
{
    public static XmlDoc AddParam(this XmlDoc doc, string name, string? content = default)
    {
        doc.Add(Constants.XmlDoc.ParamSection, content, new KeyValuePair<string, string>(Constants.XmlDoc.ParamNameAttribute, name));
        return doc;
    }

    public static XmlDoc AddResponse(this XmlDoc doc, string code, string? content = default)
    {
        doc.Add(Constants.XmlDoc.ResponseSection, content, new KeyValuePair<string, string>(Constants.XmlDoc.ResponseCodeAttribute, code));
        return doc;
    }
}