using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using Xp.Runners.IO;
using Xp.Runners.Config;

namespace Xp.Runners.Commands
{
    /// <summary>The list command lists available subcommands</summary>
    public class List : Command
    {
        private IEnumerable<Type> BuiltinsIn(Assembly assembly)
        {
            return assembly
                .GetTypes()
                .Where(t => t.Namespace == "Xp.Runners.Commands")
                .Where(t => t.IsSubclassOf(typeof(Command)))
            ;
        }

        private IEnumerable<EntryPoint> ScriptsIn(string dir)
        {
            return Directory
                .GetFiles(dir, "xp.*")
                .Where(f => !f.EndsWith(".bat"))
                .Select(f => new EntryPoint(f))
            ;
        }

        private bool DisplayCommandsIn(string kind, string dir)
        {
            var empty = true;
            var bin = Paths.Compose(dir, "bin");
            if (Directory.Exists(bin))
            {
                foreach (var entry in ScriptsIn(bin))
                {
                    if (empty)
                    {
                        Console.WriteLine("{0} @ {1}:", kind, dir);
                        empty = false;
                    }
                    Console.WriteLine("  $ xp {0} (from {1})", entry.Command, entry.Module);
                }
            }

            return !empty;
        }

        /// <summary>Entry point</summary>
        public override int Execute(CommandLine cmd, ConfigSource configuration)
        {
            var self = Assembly.GetExecutingAssembly();

            Console.WriteLine("@{0}", Paths.Binary());
            Console.WriteLine("XP Subcommands");
            Console.WriteLine("════════════════════════════════════════════════════════════════════════");
            Console.WriteLine();

            Console.WriteLine("> Builtin({0})", self.GetName().Version);
            foreach (var type in BuiltinsIn(self))
            {
                Console.WriteLine("  $ xp {0}", type.Name.ToLower());
            }
            Console.WriteLine();

            if (DisplayCommandsIn("> Local", Directory.GetCurrentDirectory()))
            {
                Console.WriteLine();
            }

            foreach (var dir in ComposerLocations())
            {
                if (DisplayCommandsIn("> Installed", dir)) Console.WriteLine();
            }

            return 0;
        }
    }
}