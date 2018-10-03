using Xp.Runners;
using Xp.Runners.IO;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Xp.Runners.Commands
{
    /// <summary>run $class [$arg0 [$arg1 [...]]]</summary>
    public class Run : Command
    {
        private static Regex ns = new Regex("\\<\\?php namespace (?<ns>[^;]+);");

        /// <summary>Additional class path entries to load</summary>
        protected override IEnumerable<string> ClassPathFor(CommandLine cmd)
        {
            var execute = cmd.Arguments.FirstOrDefault();

            // Extract PHP namespace from first line of file, except for classes
            if (File.Exists(execute) && !execute.Contains(".class.php"))
            {
                using (StreamReader sr = new StreamReader(execute)) {
                    var matches = ns.Matches(sr.ReadLine());
                    if (matches.Count > 0)
                    {
                        var autoload = Paths.Compose(Paths.UserDir("xp"), matches[0].Groups["ns"].Value, "vendor", "autoload.php");
                        if (File.Exists(autoload))
                        {
                            return base.ClassPathFor(cmd).Append(autoload);
                        }
                    }
                }
            }

            return base.ClassPathFor(cmd);
        }
   }
}