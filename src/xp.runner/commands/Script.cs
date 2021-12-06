using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xp.Runners.Config;
using Xp.Runners.IO;
using Xp.Runners;

namespace Xp.Runners.Commands
{
    /// <summary>run $file [$arg0 [$arg1 [...]]]</summary>
    public class Script : Command
    {
        private static Regex _from = new Regex("^use (.+) from '([^']+)';");
        private string _file;
        private string _namespace = null;
        private HashSet<string> _libraries = null;

        public string Namespace {
            get { Parse(); return _namespace; }
        }

        public HashSet<string> Libraries {
            get { Parse(); return _libraries; }
        }

        /// <summary>Creates a new script from a given filename</summary>
        public Script(string file)
        {
            _file = file;
        }

        private void Parse()
        {
            if (null != _libraries) return;

            _libraries = new HashSet<string>();
            foreach (string line in File.ReadAllLines(_file))
            {
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
                    var g = _from.Match(line).Groups[2];
                    if (g.Success)
                    {
                        _libraries.Add(g.Captures[0].Value);
                    }
                }
            }
        }

        /// <summary>Additional modules to load.</summary>
        protected override IEnumerable<string> ModulesFor(CommandLine cmd)
        {
            var user = new string[] { Paths.Compose(Paths.ConfigDir(_namespace), "vendor") };
            return Libraries
                .Select(library => user.Concat(ComposerLocations())
                    .Where(dir => Directory.Exists(Paths.Compose(dir, library)))
                    .Select(dir => Paths.Compose(dir, "autoload.php"))
                    .FirstOrDefault()
                )
                .Where(loader => loader != null)
                .Distinct()
            ;
        }

        /// <summary>Command line arguments.</summary>
        protected override IEnumerable<string> ArgumentsFor(CommandLine cmd)
        {
            return (new string[] { _file }).Concat(cmd.Arguments);
        }
    }
}