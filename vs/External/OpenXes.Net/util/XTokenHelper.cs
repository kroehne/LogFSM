using System;
using System.Text;
using System.Collections.Generic;

namespace OpenXesNet.util
{
    public static class XTokenHelper
    {
        public static string FormatTokenString(List<string> tokens)
        {
            if (tokens.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(FormatToken(tokens[0]));
                for (int i = 1; i < tokens.Count; ++i)
                {
                    sb.Append(' ');
                    sb.Append(FormatToken(tokens[i]));
                }
                return sb.ToString();
            }
            return "";
        }

        static string FormatToken(string token)
        {
            token = token.Trim();
            if ((token.IndexOf((char)32) >= 0) || (token.IndexOf((char)9) >= 0))
            {
                StringBuilder sb = new StringBuilder();
                token = token.Replace("'", "\\'");
                sb.Append('\'');
                sb.Append(token);
                sb.Append('\'');
                return sb.ToString();
            }
            return token;
        }

        public static List<string> ExtractTokens(String tokenString)
        {
            List<string> tokens = new List<string>();
            bool isEscaped = false;
            bool isQuoted = false;
            StringBuilder sb = new StringBuilder();
            foreach (char c in tokenString)
            {
                if ((c == ' ') && (!(isQuoted)) && (!(isEscaped)))
                {
                    string tk = sb.ToString().Trim();
                    if (tk.Length > 0)
                    {
                        tokens.Add(tk);
                    }

                    sb = new StringBuilder();
                }
                else if (c == '\\')
                {
                    isEscaped = true;
                }
                else if (c == '\'')
                {
                    if (!(isEscaped))
                        isQuoted = !(isQuoted);
                    else
                    {
                        sb.Append(c);
                    }
                    isEscaped = false;
                }
                else
                {
                    sb.Append(c);
                    isEscaped = false;
                }
            }

            string token = sb.ToString().Trim();
            if (token.Length > 0)
            {
                tokens.Add(token);
            }
            return tokens;
        }
    }
}
