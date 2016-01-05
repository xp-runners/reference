using System;
using System.Linq;
using System.Collections.Generic;
using Xp.Runners;
using Xp.Runners.Config;

namespace Xp.Runners.Commands
{
    /// <summary>help [$command]</summary>
    public class Help : Command
    {
        private IEnumerable<string> arguments;
        private IEnumerable<string> modules;

        /// <summary>Initialize this command</summary>
        protected override void Initialize(CommandLine cmd, ConfigSource configuration)
        {
            var arg = cmd.Arguments.FirstOrDefault();
            if (null == arg) 
            {
                arguments = new string[] { "xp.runtime.ShowResource", "usage.txt", "1" };
                modules = new string[] { };
            }
            else
            {
                var plugin = new Plugin(arg);
                arguments = new string[] { "xp.runtime.Help", plugin.EntryPoint.Type };
                modules = plugin.Modules;
            }
        }

        /// <summary>Command line arguments.</summary>
        protected override IEnumerable<string> ArgumentsFor(CommandLine cmd)
        {
            return arguments;
        }

        /// <summary>Command line arguments.</summary>
        protected override IEnumerable<string> ModulesFor(CommandLine cmd)
        {
            return modules;
        }
    }
}