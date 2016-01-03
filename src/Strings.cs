using System;
using System.Text;

namespace Xp.Runners
{
    static class Strings
    {
        /// <summary>PHP's ucfirst() in C#</summary>
        public static string UpperCaseFirst(this string self)
        {
            if (string.IsNullOrEmpty(self))
            {
                return string.Empty;
            }
            else
            {
                var a = self.ToCharArray();
                a[0] = char.ToUpper(a[0]);
                return new string(a);
            }
        }

        /// <summary>Encode string suitable as a binary-safe command line argument</summary>
        public static string AsArgument(this string self)
        {
            var bytes = Encoding.UTF7.GetBytes(self);
            var ret = new StringBuilder();

            ret.Append('"');
            for (var i = 0; i < bytes.Length; i++)
            {
                if (34 == bytes[i])
                {
                    ret.Append("\"\"");     // Double-quote -> double double-quote
                }
                else if (92 == bytes[i])
                {
                    ret.Append("\\\\");     // Backslash -> double backslash
                }
                else
                {
                    ret.Append(Convert.ToString((char)bytes[i]));
                }
            }

            ret.Append('"');
            return ret.ToString();
        }
    }
}