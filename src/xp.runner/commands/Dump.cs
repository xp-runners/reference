using System.Linq;
using System.Collections.Generic;
using Xp.Runners;

namespace Xp.Runners.Commands
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