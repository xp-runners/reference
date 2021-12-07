using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using Xp.Runners.IO;
using Xp.Runners.Exec;
using Xp.Runners.Config;

namespace Xp.Runners.Commands
{
    /// <summary>The list command lists available subcommands</summary>
    public class List : Command
    {

        /// <summary>Returns all builtin commands</summary>
        private IEnumerable<Type> BuiltinsIn(Assembly assembly)
        {
            return assembly
                .GetTypes()
                .Where(t => t.Namespace == "Xp.Runners.Commands")
                .Where(t => t.IsSubclassOf(typeof(Command)))
            ;
        }

        /// <summary>Verifies a given script is indeed a composer script by checking for shebang</summary>
        private bool isComposerScript(string name)
        {
            using (var file = new FileStream(name, FileMode.Open, FileAccess.Read))
            {
                var header = new byte[2];
                return 2 == file.Read(header, 0, 2) && header.SequenceEqual(new byte[2] { (byte)'#', (byte)'!' });
            }
        }

        /// <summary>Returns all scripts inside a given vendor/bin directory</summary>
        private IEnumerable<EntryPoint> ScriptsIn(string dir)
        {
            return Directory
                .GetFiles(dir, "xp.*")
                .Where(f => !f.EndsWith(".bat"))
                .Where(isComposerScript)
                .Select(f => new EntryPoint(Path.GetFileName(f)))
            ;
        }

        /// <summary>Display commands of a certain kind in a given directory</summary>
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
                        Console.WriteLine("{0} @ {1}", kind, dir);
                        Console.WriteLine();
                        empty = false;
                    }
                    Console.WriteLine("  $ xp {0} (» \x1b[35;1;4mfrom {1}\x1b[0m)", entry.Command, entry.Module);
                }
            }

            return !empty;
        }

        /// <summary>Entry point</summary>
        public override int Execute(CommandLine cmd, ConfigSource configuration)
        {
            var self = Assembly.GetExecutingAssembly();

            Output.Origin(Console.Out, Paths.Binary());
            Output.Message(Console.Out, "XP Subcommands");
            Output.Separator(Console.Out);

            Console.WriteLine("\x1b[33;1m>\x1b[0m Builtin @ {0}", self.GetName().Version);
            Console.WriteLine();
            foreach (var type in BuiltinsIn(self))
            {
                Console.WriteLine("  $ xp {0}", type.Name.ToLower());
            }
            Console.WriteLine();

            if (File.Exists(ComposerFile.NAME))
            {
                using (var composer = new ComposerFile(ComposerFile.NAME))
                {
                    if (composer.Definitions.Scripts.Count > 0)
                    {
                        Console.WriteLine("\x1b[33;1m>\x1b[0m Defined via scripts in @ {0}", composer.SourceUri);
                        Console.WriteLine();

                        foreach (var script in composer.Definitions.Scripts)
                        {
                            Console.WriteLine("  $ xp {0}", script.Key);
                        }
                        Console.WriteLine();
                    }
                }
            }

            foreach (var dir in cmd.Path["modules"])
            {
                if (DisplayCommandsIn("\x1b[33;1m>\x1b[0m Module", Paths.Resolve(dir))) Console.WriteLine();
            }

            if (DisplayCommandsIn("\x1b[33;1m>\x1b[0m Local", Directory.GetCurrentDirectory()))
            {
                Console.WriteLine();
            }

            foreach (var dir in ComposerLocations())
            {
                if (DisplayCommandsIn("\x1b[33;1m>\x1b[0m Installed", Paths.Compose(dir, "vendor"))) Console.WriteLine();
            }
            return 0;
        }
    }
}