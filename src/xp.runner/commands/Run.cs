using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xp.Runners.IO;
using Xp.Runners;

namespace Xp.Runners.Commands
{
    /// <summary>run $class [$arg0 [$arg1 [...]]]</summary>
    public class Run : Command
    {
        private static Regex ns = new Regex("\\<\\?php namespace (?<ns>[^;]+);");

        /// <summary>Use path</summary>
        protected override IEnumerable<string> UseFor(CommandLine cmd)
        {
            var execute = cmd.Arguments.FirstOrDefault();

            // Extract PHP namespace from first line of file, except for classes
            if (File.Exists(execute) && !execute.Contains(".class.php"))
            {
                using (StreamReader sr = new StreamReader(execute)) {
                    var matches = ns.Matches(sr.ReadLine());
                    if (matches.Count > 0)
                    {
                        var use = Paths.Compose(Paths.UserDir("xp"), matches[0].Groups["ns"].Value, "vendor", "xp-framework", "core");
                        if (Directory.Exists(use))
                        {
                            return new string[] { use };
                        }
                    }
                }
            }

            // Use default "use" path
            return null;
        }
   }
}