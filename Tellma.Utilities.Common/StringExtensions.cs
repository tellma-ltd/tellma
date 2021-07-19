using System;
using System.Text;

namespace Tellma.Utilities.Common
{
    public static class StringExtensions
    {
        /// <summary>
        /// Removes any trailing slashes from the specified string.
        /// </summary>
        public static string WithoutTrailingSlash(this string str)
        {
            if (str is null)
            {
                return null;
            }

            while (str.EndsWith('/'))
            {
                str = str[0..^1];
            }

            return str;
        }

        /// <summary>
        /// Adds one trailing slash to the specified string if one is not already there.
        /// </summary>
        public static string WithTrailingSlash(this string str)
        {
            if (str is null)
            {
                return null;
            }

            if (!str.EndsWith("/"))
            {
                str += "/";
            }

            return str;
        }

        /// <summary>
        /// If the string is enclosed in one or more bracket pairs, this function strips away those brackets.
        /// Note: The string "(A) (B)" is NOT enclosed in a bracket pair.
        /// </summary>
        public static string DeBracket(this string str)
        {
            if (str.Length < 2)
            {
                return str;
            }

            int level = 0;
            for (int i = 0; i < str.Length - 1; i++)
            {
                if (str[i] == '(')
                {
                    level++;
                }

                if (str[i] == ')')
                {
                    level--;
                }

                if (level == 0)
                {
                    // There are no enclosing brackets
                    return str;
                }
            }

            if (str.EndsWith(')'))
            {
                // Remove the brackets and call recursively
                str = str[1..^1].DeBracket();
            }

            return str;
        }

        /// <summary>
        /// Removes all characters after a certain length.
        /// </summary>
        public static string Truncate(this string value, int maxLength, bool appendEllipses = false)
        {
            const string ellipses = "...";

            if (maxLength < 0)
            {
                return value;
            }
            else if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            else if (value.Length <= maxLength)
            {
                return value;
            }
            else
            {
                var truncated = value.Substring(0, maxLength);
                if (appendEllipses)
                {
                    truncated += ellipses;
                }

                return truncated;
            }
        }

        /// <summary>
        /// Indents all the lines of the string by a specified number of spaces, useful when formatting nested SQL queries.
        /// </summary>
        public static string IndentLines(this string s, int spaces = 4)
        {
            var lines = s.Split(Environment.NewLine);
            var bldr = new StringBuilder();
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                string indentedLine = new string(' ', spaces) + line;
                if (i == lines.Length - 1)
                {
                    bldr.Append(indentedLine);

                }
                else
                {
                    bldr.AppendLine(indentedLine);
                }
            }

            return bldr.ToString();
        }
    }
}
