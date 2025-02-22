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
    }
}