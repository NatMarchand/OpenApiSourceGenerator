using System.Text.RegularExpressions;

namespace NatMarchand.OpenApiSourceGenerator;

public static class Utils
{
    public static string KeyToSymbol(string key)
    {
        return Regex.Replace(key, "\\W+", "_");
    }

    public static string KeyToNamespace(string key)
    {
        return Regex.Replace(key, "\\W+", ".");
    }
}