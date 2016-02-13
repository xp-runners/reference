using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Xp.Runners;
using Xp.Runners.IO;
using Xp.Runners.Config;

namespace Xp.Runners.Commands
{
    /// <summary>version</summary>
    public class Version : Command
    {
        /// <summary>Command line arguments.</summary>
        protected override IEnumerable<string> ArgumentsFor(CommandLine cmd)
        {
            return new string[] { "xp.runtime.Version" };
        }


        /// <summary>Entry point</summary>
        public override int Execute(CommandLine cmd, ConfigSource configuration)
        {
            var self = Assembly.GetExecutingAssembly();
            Console.WriteLine(
                "Runners {0} {{ .NET {1} }} @ {2}",
                self.GetName().Version,
                self.ImageRuntimeVersion,
                Paths.Binary()
            );

            return base.Execute(cmd, configuration);
        }
    }
}