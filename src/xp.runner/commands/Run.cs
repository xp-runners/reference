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

        /// <summary>Returns whether the environment indicates this system conforms to
        /// https://specifications.freedesktop.org/basedir-spec/basedir-spec-latest.html</summary>
        private static bool UseXDG()
        {
            foreach (string variable in Environment.GetEnvironmentVariables().Keys)
            {
                if (variable.StartsWith("XDG_")) return true;
            }
            return false;
        }

        /// <summary>Returns the user-specific config directory. Respects $HOME, XDG-compliant
        /// systems and falls back to using %APPDATA%</summary>
        private string UserDir()
        {
            if (UseXDG())
            {
                return Paths.Compose(
                    Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") ?? Paths.Compose(Paths.Home(), ".config"),
                    "xp"
                );
            }

            var home = Environment.GetEnvironmentVariable("HOME");
            if (!String.IsNullOrEmpty(home))
            {
                return Paths.Compose(home, ".xp");
            }

            return Paths.Compose(Environment.SpecialFolder.ApplicationData, "XP");
        }

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
                        var autoload = Paths.Compose(UserDir(), matches[0].Groups["ns"].Value, "vendor", "autoload.php");
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