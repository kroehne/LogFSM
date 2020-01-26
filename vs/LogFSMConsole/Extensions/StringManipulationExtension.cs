namespace LogFSM 
{
    #region using
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    #endregion

    public static class StringManipulationExtension
    {
        public static string CreateValidEnumValue(this string str)
        {
            return str.RemoveWhitespace().RemoveSymbols().RemoveEndingCR();
        }

        public static string RemoveSymbols(this string str)
        {
            return Regex.Replace(str, "[@&'(\\s)<>#-]", "");
        }

        public static string RemoveWhitespace(this string str)
        {
            return string.Join("", str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
        }
        public static string RemoveEndingCR(this string str)
        {
            while (str.EndsWith("\r\n"))
                str = str.Substring(0, str.Length - 2);
            return str;
        }
    }

}
