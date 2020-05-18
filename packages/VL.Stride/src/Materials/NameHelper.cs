using System.Text.RegularExpressions;

namespace VL.Stride.Materials
{
    static class NameHelper
    {
        static readonly Regex FSpaceAndCharRegex = new Regex(" [a-zA-Z]", RegexOptions.Compiled);
        static readonly Regex FLowerAndUpperRegex = new Regex("[a-z0-9][A-Z0-9]", RegexOptions.Compiled);

        public static string UpperCaseAfterSpace(this string name)
        {
            return FSpaceAndCharRegex.Replace(name, m => $" {char.ToUpper(m.Value[1])}");
        }

        public static string InsertSpaces(this string name)
        {
            return FLowerAndUpperRegex.Replace(name, m => $"{m.Value[0]} {m.Value[1]}");
        }
    }
}
