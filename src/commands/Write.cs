using System.Linq;
using System.Collections.Generic;

namespace Xp.Runners
{
    /// <summary>write $code [$arg0 [$arg1 [...]]]</summary>
    public class Write : Command
    {
        /// <summary>Command line arguments.</summary>
        protected override IEnumerable<string> ArgumentsFor(CommandLine cmd)
        {
            return (new string[] { "xp.runtime.Dump", "-w" }).Concat(cmd.Arguments);
        }
    }
}