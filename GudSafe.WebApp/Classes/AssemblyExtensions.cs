using System.Reflection;

namespace GudSafe.WebApp.Classes;

public static class AssemblyExtensions
{
    public static string ToVersionString(this Assembly? assembly)
    {
        var version = assembly?.GetName().Version;

        if (version == null) return string.Empty;

        return $"{version.Major}.{version.Minor}.{version.Build}";
    }
}