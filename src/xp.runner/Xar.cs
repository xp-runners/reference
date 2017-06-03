using System.Linq;
using System.Collections.Generic;

namespace Xp.Runners
{
    public class Xar
    {
        private static string[] command = new string[] {"ar"};

        /// <summary>Entry point</summary>
        public static int Main(string[] args)
        {
            return Xp.Main(command.Concat(args).ToArray());
        }
    }
}