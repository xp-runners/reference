using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using Xp.Runners.IO;
using Xp.Runners.Config;
using Xp.Runners.Exec;

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

        private bool DisplayCommandsIn(TextWriter con, string kind, string dir)
        {
            var empty = true;
            var bin = Paths.Compose(dir, "bin");
            if (Directory.Exists(bin))
            {
                foreach (var entry in ScriptsIn(bin))
                {
                    if (empty)
                    {
                        con.WriteLine("{0} @ {1}", kind, dir);
                        con.WriteLine();
                        empty = false;
                    }
                    con.WriteLine("  $ xp {0} (from {1})", entry.Command, entry.Module);
                }
            }

            return !empty;
        }

        /// <summary>Entry point</summary>
        public override int Execute(CommandLine cmd, ConfigSource configuration)
        {
            var self = Assembly.GetExecutingAssembly();
            var con = new ANSISupport(Console.Out);

            con.WriteLine("\x1b[33m@{0}\x1b[0m", Paths.Binary());
            con.WriteLine("\x1b[1mXP Subcommands");
            con.WriteLine("\x1b[36m════════════════════════════════════════════════════════════════════════\x1b[0m");
            con.WriteLine();

            con.WriteLine("\x1b[33;1m>\x1b[0m Builtin @ {0}", self.GetName().Version);
            con.WriteLine();
            foreach (var type in BuiltinsIn(self))
            {
                con.WriteLine("  $ xp {0}", type.Name.ToLower());
            }
            con.WriteLine();

            if (DisplayCommandsIn(con, "\x1b[33;1m>\x1b[0m Local", Directory.GetCurrentDirectory()))
            {
                con.WriteLine();
            }

            foreach (var dir in ComposerLocations())
            {
                if (DisplayCommandsIn(con, "\x1b[33;1m>\x1b[0m Installed", dir)) con.WriteLine();
            }

            return 0;
        }
    }
}