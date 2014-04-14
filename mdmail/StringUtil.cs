using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailTemplate
{
    public  static class StringUtil
    {

        #region Nulls and Whitespaces

        public static bool IsNullOrEmpty(this string text)
        {
            return text == null || text.Length == 0;
        }
        public static bool IsNullOrWhiteSpace(this string text)
        {
            if (text == null || text.Length == 0)
                return true;

            for (int i = 0; i < text.Length; i++)
            {
                if (!char.IsWhiteSpace(text[i]))
                    return false;
            }

            return true;
        }

        public static bool IsNotNullOrEmpty(this string text)
        {
            return !IsNullOrEmpty(text);
        }
        public static bool IsNotNullOrWhiteSpace(this string text)
        {
            return !IsNullOrWhiteSpace(text);
        }

        public static string ToNullIfEmpty(this string text)
        {
            return IsNullOrEmpty(text) ? null : text;
        }
        public static string ToNullIfWhiteSpace(this string text)
        {
            return IsNullOrWhiteSpace(text) ? null : text;
        }

        public static string DefaultIfNullOrEmpty(this string text, string defaultValue)
        {
            return IsNullOrEmpty(text) ? defaultValue : text;
        }

        public static string DefaultIfNullOrWhiteSpace(this string text, string defaultValue)
        {
            return IsNullOrWhiteSpace(text) ? defaultValue : text;
        }

        #endregion

        #region Format

        public static string Fmt(this string format, params object[] args)
        {
            return string.Format(format, args);
        }

        public static string FmtInvariant(this string format, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, format, args);
        }


        #endregion


        public static string Repeat(this string text, int number)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < number; i++)
            {
                sb.Append(text);
            }

            return text;
        }


        #region Null Safe Operations

        public static string NullSafeTrim(this string text)
        {
            return text == null ? null : text.Trim();
        }
        public static string NullSafeToLower(this string text)
        {
            if (text == null)
                return null;
            else
                return text.ToLowerInvariant();
        }
        public static string NullSafeToUpper(this string text)
        {
            if (text == null)
                return null;
            else
                return text.ToUpperInvariant();
        }
        public static string SafeSubString(this string input, int startIndex, int length)
        {
            if (input == null)
                return null;

            // Todo: Check that startIndex + length does not cause an arithmetic overflow
            if (input.Length >= (startIndex + length))
            {
                return input.Substring(startIndex, length);
            }
            else
            {
                if (input.Length > startIndex)
                {
                    return input.Substring(startIndex);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        #endregion

        #region Casing

        public static string ToTitleCase(this string text)
        {
            if (text == null)
                return null;
            if (text == string.Empty)
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < text.Length; i++)
            {
                bool isFirstChar = i == 0;

                if (char.IsUpper(text[i]) && !isFirstChar)
                {
                    sb.Append(" ");
                }


                char charToAppend = text[i];

                if (charToAppend == '_') //replace underscores with spaces
                    charToAppend = ' ';

                sb.Append(charToAppend);
            }

            return sb.ToString();
        }


        public static string ToTitleCase(this Enum value)
        {
            var s = value.ToString();
            return ToTitleCase(s);
        }



        #endregion

        #region Enum Helpers

        public static T ToEnum<T>(this string value) where T : struct
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }
        public static T ToEnumOrDefault<T>(this string value, T defaultValue) where T : struct
        {
            if (String.IsNullOrEmpty(value)) return defaultValue;
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static bool TryToEnum<T>(this string value, out T result) where T : struct
        {
            T tryResult = default(T);
            bool success = Enum.TryParse<T>(value, out tryResult);
            result = tryResult;
            return success;
        }

        #endregion


        #region string arrays

        public static string JoinStrings(this IEnumerable<string> items, string separator)
        {
            return string.Join(separator, items);
        }

        public static string[] Split(this string s, params string[] separator)
        {
         
            var sep = separator;
            return s.Split(separator, StringSplitOptions.None);
        }

        public static string[] Split(this string s, params char[] separator)
        {
            return s.Split(separator, StringSplitOptions.None);
        }


        #endregion


        #region Non Extension Methods

        public static string Pluralize(int count, string singular)
        {
            string ending = "s";

            if (singular.EndsWith("x"))
                ending = "es";

            if (singular.EndsWith("y"))
                ending = "ies";

            string pluralVersion = singular + ending;

            return Pluralize(count, singular, pluralVersion);
        }
        public static string Pluralize(int count, string singular, string plural)
        {
            return count + " " + (count == 1 ? singular : plural);
        }

        #endregion
    }
}
