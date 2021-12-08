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

        /// <summary>Appends commands of a certain kind in a given directory</summary>
        private void AppendCommands(Output output, string kind, string dir)
        {
            var bin = Paths.Compose(dir, "bin");
            if (Directory.Exists(bin))
            {
                var section = new Output().Line();
                var count = ScriptsIn(bin)
                    .Select(entry => section.Line("$ xp " + entry.Command + " (» \x1b[35;1;4mfrom " + entry.Module + "\x1b[0m)"))
                    .Count()
                ;

                if (count > 0)
                {
                    output.Section(kind + " " + dir, section);
                }
            }
        }

        /// <summary>Entry point</summary>
        public override int Execute(CommandLine cmd, ConfigSource configuration)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var output = new Output(Console.Out)
                .Origin(Paths.Binary())
                .Header("XP Subcommands")
                .Section("Builtin @ " + assembly.GetName().Version, new Output()
                    .Line()
                    .Each(BuiltinsIn(assembly), (self, type) => self.Line("$ xp " + type.Name.ToLower()))
                )
            ;

            if (File.Exists(ComposerFile.NAME))
            {
                using (var composer = new ComposerFile(ComposerFile.NAME))
                {
                    if (composer.Definitions.Scripts.Count > 0)
                    {
                        output.Section("Defined via scripts in @ " + composer.SourceUri, new Output()
                            .Line()
                            .Each(composer.Definitions.Scripts, (self, script) => self.Line("$ xp " + script.Key))
                        );
                    }
                }
            }

            foreach (var dir in cmd.Path["modules"])
            {
                AppendCommands(output, "Module", Paths.Resolve(dir));
            }

            AppendCommands(output, "Local", Directory.GetCurrentDirectory());

            foreach (var dir in ComposerLocations())
            {
                AppendCommands(output, "Installed",  Paths.Compose(dir, "vendor"));
            }
            return 0;
        }
    }
}