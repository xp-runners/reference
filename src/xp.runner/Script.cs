using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xp.Runners.Config;
using Xp.Runners.IO;
using Xp.Runners;

namespace Xp.Runners
{
    /// <summary>Runs XP Scripts, extracting dependencies</summary>
    public class Script : Command
    {
        private const string NotFound = "?";

        private static Regex _from = new Regex("^use\\s+(.+)\\s+from\\s+'([^'@]+)(@([^']+))?';");

        private string _file;
        private string _namespace = null;
        private Dictionary<string, string> _libraries = new Dictionary<string, string>();

        /// <summary>Creates a new script from a given filename</summary>
        public Script(string file)
        {
            _file = file;

            var l = 0;
            foreach (string line in File.ReadAllLines(_file))
            {
                l++;
                if (line.StartsWith("<?php namespace "))
                {
                    var p = "<?php namespace ".Length;
                    _namespace = line.Substring(p, line.IndexOf(";") - p);
                }
                else if (line.StartsWith("namespace "))
                {
                    var p = "namespace ".Length;
                    _namespace = line.Substring(p, line.IndexOf(";") - p);
                }
                else
                {
                    var g = _from.Match(line).Groups;
                    if (g.Count > 3)
                    {
                        // "example/library" => "^1.0" vs "another/library" => "*"
                        _libraries[g[2].Captures[0].Value] = g[4].Captures.Count > 0
                            ? g[4].Captures[0].Value
                            : "*"
                        ;
                    }
                }
            }
        }


        /// <summary>Returns script namespace or NULL if no namespace is used</summary>
        public string Namespace {
            get { return _namespace; }
        }

        /// <summary>Returns a dictionary of used libraries, library name => version selector</summary>
        public Dictionary<string, string> Libraries {
            get { return _libraries; }
        }

        /// <summary>Additional class path entries to load.</summary>
        protected override IEnumerable<string> ClassPathFor(CommandLine cmd)
        {
            var locations = new string[] { Paths.ConfigDir(_namespace) }
                .Concat(ComposerLocations())
                .Select(Paths.Resolve)
                .ToArray()
            ;
            var loaders = _libraries
                .GroupBy(library => locations
                    .Where(dir => Directory.Exists(Paths.Compose(dir, "vendor", library.Key)))
                    .Select(dir => Paths.Compose(dir, "vendor", "autoload.php"))
                    .DefaultIfEmpty(NotFound)
                    .First()
                )
                .ToDictionary(group => group.Key, group => group.ToList())
            ;

            // Generate detailed error message including installation advice when any
            // of the required libraries are missing.
            if (loaders.ContainsKey(NotFound))
            {
                var error = "Cannot find " + string.Join(", ", loaders[NotFound]);
                var advice = new StringBuilder();

                // List search paths
                advice.Append("> Search paths:\n  ").Append(locations[0]).Append(" (» \x1b[35;1;4mdefault\x1b[0m)\n");
                foreach (var location in locations.Skip(1))
                {
                    advice.Append("  ").Append(location).Append("\n");
                }
                advice.Append("\n");

                // Show commands to install
                advice.Append("> Install by running the following:\n");
                advice.Append("\x1b[34m  mkdir -p '").Append(locations[0]).Append("'").Append("\x1b[0m\n");
                foreach (var missing in loaders[NotFound])
                {
                    advice.Append("\x1b[34m  composer require -d '").Append(locations[0]).Append("' ").Append(missing.Key);
                    if ("*" != missing.Value)
                    {
                        advice.Append(" '").Append(missing.Value).Append("'");
                    }
                    advice.Append("\x1b[0\n");
                }

                throw new CannotExecute(error, Paths.Resolve(_file)).Advise(advice.ToString());
            }

            return loaders.Keys;
        }

        /// <summary>Command line arguments.</summary>
        protected override IEnumerable<string> ArgumentsFor(CommandLine cmd)
        {
            return (new string[] { _file }).Concat(cmd.Arguments);
        }
    }
}