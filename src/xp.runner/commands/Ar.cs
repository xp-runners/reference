using System.Linq;
using System.Collections.Generic;
using Xp.Runners;

namespace Xp.Runners.Commands
{
    /// <summary>ar $flags [$options]</summary>
    public class Ar : Command
    {
        /// <summary>Command line arguments.</summary>
        protected override IEnumerable<string> ArgumentsFor(CommandLine cmd)
        {
            return (new string[] { "xp.xar.Runner" }).Concat(cmd.Arguments);
        }
    }
}