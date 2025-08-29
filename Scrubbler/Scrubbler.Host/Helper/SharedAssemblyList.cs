using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrubbler.Host.Helper;

internal static class SharedAssemblyList
{
    private static readonly Lazy<List<string>> _patterns = new(() =>
    {
        var path = Path.Combine(AppContext.BaseDirectory, "shared-assemblies.txt");
        return File.Exists(path)
            ? File.ReadAllLines(path).Where(l => !string.IsNullOrWhiteSpace(l)).ToList()
            : new List<string>();
    });

    public static bool IsShared(string assemblyName)
    {
        foreach (var pattern in _patterns.Value)
        {
            if (MatchesPattern(assemblyName, pattern))
                return true;
        }
        return false;
    }

    private static bool MatchesPattern(string input, string pattern)
    {
        if (pattern == "*") return true;

        var regex = "^" + System.Text.RegularExpressions.Regex.Escape(pattern)
            .Replace("\\*", ".*") + "$";
        return System.Text.RegularExpressions.Regex.IsMatch(input, regex, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }
}
