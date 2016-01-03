using System.Linq;
using System.Collections.Generic;

namespace Xp.Runners
{
    /// <summary>help [$command]</summary>
    public class Help : Command
    {
        /// <summary>Command line arguments.</summary>
        protected override IEnumerable<string> ArgumentsFor(CommandLine cmd)
        {
            return new string[]
            {
                "xp.runtime.ShowResource",
                (cmd.Arguments.FirstOrDefault() ?? "usage") + ".txt",
                "1"
            };
        }
    }
}