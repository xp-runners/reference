using System.Linq;
using System.Collections.Generic;

namespace Xp.Runners
{
    /// <summary>dump $code [$arg0 [$arg1 [...]]]</summary>
    public class Dump : Command
    {
        /// <summary>Command line arguments.</summary>
        protected override IEnumerable<string> ArgumentsFor(CommandLine cmd)
        {
            return (new string[] { "xp.runtime.Dump", "-d" }).Concat(cmd.Arguments);
        }
    }
}