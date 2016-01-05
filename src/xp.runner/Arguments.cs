using System;
using System.Text;

namespace Xp.Runners
{
    static class Arguments
    {
        /// <summary>Encode string suitable as a binary-safe command line argument</summary>
        public static string Encode(this string self)
        {
            var bytes = Encoding.UTF7.GetBytes(self);
            var ret = new StringBuilder();

            ret.Append('"');
            for (var i = 0; i < bytes.Length; i++)
            {
                ret.Append((char)bytes[i]);
            }
            ret.Append('"');

            return ret.ToString();
        }

        /// <summary>Escape string as command line argument</summary>
        public static string Escape(this string self)
        {
            var ret = new StringBuilder();

            ret.Append('"');
            for (var i = 0; i < self.Length; i++)
            {
                if ('"' == self[i])
                {
                    ret.Append("\"\"");     // Double-quote -> double double-quote
                }
                else if ('\\' == self[i])
                {
                    ret.Append("\\\\");     // Backslash -> double backslash
                }
                else
                {
                    ret.Append(self[i]);
                }
            }
            ret.Append('"');

            return ret.ToString();
        }
    }
}