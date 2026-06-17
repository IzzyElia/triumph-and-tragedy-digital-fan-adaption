using System.Text.RegularExpressions;

namespace TT2026.libraries.Izzy.Utils
{
    public static class StringUtils
    {
        public static char GetLastCharacter(string str)
        {
            string trimmedString = str.Trim();
            return trimmedString[trimmedString.Length - 1];
        }
        public static string RemoveWhitespace(string str)
        {
            return Regex.Replace(str, @"\s+", "");
        }
    }

}