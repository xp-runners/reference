using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using Xp.Runners;
using Xp.Runners.IO;
using Xp.Runners.Config;

namespace Xp.Runners
{
    /// <summary>The plugin command searches for XP commands in Composer's vendor/bin directory</summary>
    public class Plugin : Command
    {
        private string name;
        private EntryPoint entry;
        private IEnumerable<string> modules;

        public EntryPoint EntryPoint { get { return entry; }}

        public IEnumerable<string> Modules { get { return modules; }}

        /// <summary>Creates a new plugin with a given name</summary>
        public Plugin(string name)
        {
            this.name = name;
        }

        /// <summary>Initialize this command. Searches modules passed via command line, current directory
        /// and locally as well as globally installed locations for subcommands, in this order.</summary>
        public override void Initialize(CommandLine cmd, ConfigSource configuration)
        {
            foreach (var dir in cmd.Options["modules"].Concat(new string[] { "." }))
            {
                if (null == (entry = FindEntryPoint(dir, name))) continue;

                modules = new string[] { };
                return;
            }

            foreach (var dir in ComposerLocations())
            {
                if (null == (entry = FindEntryPoint(dir, name))) continue;

                modules = new string[] { entry.Module };
                return;
            }

            throw new NotImplementedException(name);
        }

        /// <summary>Finds command by name in a given directory</summary>
        private EntryPoint FindEntryPoint(string dir, string name)
        {
            var bin = Paths.Compose(dir, "bin");
            if (Directory.Exists(bin))
            {
                return Directory.GetFiles(bin, "xp.*." + name)
                    .Select((file) => new EntryPoint(Path.GetFileName(file)))
                    .FirstOrDefault()
                ;
            }
            return null;
        }

        /// <summary>Additional modules to load. Overwrite in subclasses if necessary!</summary>
        protected override IEnumerable<string> ModulesFor(CommandLine cmd)
        {
            return modules;
        }

        /// <summary>Command line arguments.</summary>
        protected override IEnumerable<string> ArgumentsFor(CommandLine cmd)
        {
            return (new string[] { entry.Type }).Concat(cmd.Arguments);
        }
    }
}