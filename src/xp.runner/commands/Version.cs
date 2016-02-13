using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Xp.Runners;
using Xp.Runners.IO;

namespace Xp.Runners.Commands
{
    /// <summary>version</summary>
    public class Version : Command
    {
        /// <summary>Command line arguments.</summary>
        protected override IEnumerable<string> ArgumentsFor(CommandLine cmd)
        {
            var self = Assembly.GetExecutingAssembly();
            Console.WriteLine("Runners {0} {{ .NET {1} }} @ {2}", self.GetName().Version, self.ImageRuntimeVersion, Paths.Binary());
            return new string[] { "xp.runtime.Version" };
        }
    }
}