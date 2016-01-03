using System;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Xp.Runners
{
    public abstract class Command
    {
        const string VENDOR = "vendor";

        /// <summary>Returns well-known locations of Composer directories. Local installations have precedence!</summary>
        protected IEnumerable<string> ComposerLocations()
        {
            yield return Paths.Compose(".", VENDOR);
            yield return Paths.Compose(Environment.SpecialFolder.ApplicationData, "Composer", VENDOR);
        }

        /// <summary>Main script, e.g. "class-main.php". Overwrite in subclasses if necessary!</summary>
        protected virtual string MainFor(CommandLine cmd)
        {
            return Paths.Locate(new string[] { Paths.Binary().DirName() }, new string[] { "class-main.php" }).First();
        }

        /// <summary>Additional modules to load. Overwrite in subclasses if necessary!</summary>
        protected virtual IEnumerable<string> ModulesFor(CommandLine cmd)
        {
            return new string[] { };
        }

        /// <summary>Additional class path entries to load. Overwrite in subclasses if necessary!</summary>
        protected virtual IEnumerable<string> ClassPathFor(CommandLine cmd)
        {
            return new string[] { };
        }

        /// <summary>Command line arguments. Overwrite in subclasses if necessary!</summary>
        protected virtual IEnumerable<string> ArgumentsFor(CommandLine cmd)
        {
            return cmd.Arguments;
        }

        /// <summary>Format ini settings for use in command line</summary>
        private IEnumerable<string> IniSettings(IEnumerable<KeyValuePair<string, IEnumerable<string>>> arguments)
        {
            return arguments.SelectMany(pair => pair.Value.Select(setting => string.Format("-d {0}={1}", pair.Key, setting)));
        }

        /// <summary>Entry point</summary>
        public int Execute(CommandLine cmd, ConfigSource configuration)
        {
            var proc = new Process();
            var runtime = configuration.GetRuntime();
            var ini = new Dictionary<string, IEnumerable<string>>()
            {
                { "encoding", new string[] { "utf-7" } },
                { "magic_quotes_gpc", new string[] { "0" } },
                { "date.timezone", new string[] { TimeZoneInfo.Local.Olson() ?? "UTC" } },
                { "extension", configuration.GetExtensions(runtime) }
            };
            var use = configuration.GetUse() ?? new string[]
            {
                ComposerLocations().Select(dir => Paths.Compose(dir, "xp-framework", "core")).Where(Directory.Exists).First()
            };

            proc.StartInfo.RedirectStandardOutput = false;
            proc.StartInfo.RedirectStandardError = false;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.FileName = configuration.GetExecutable(runtime) ?? "php";
            proc.StartInfo.Arguments = string.Format(
                "-C -q -d include_path=\".{0}{1}{0}{0}.{0}{2}\" {3} {4} {5}",
                Paths.Separator,
                string.Join(Paths.Separator, use.Concat(cmd.Options["modules"].Concat(ModulesFor(cmd)))),
                string.Join(Paths.Separator, cmd.Options["classpath"].Concat(ClassPathFor(cmd))),
                string.Join(" ", IniSettings(ini.Concat(configuration.GetArgs(runtime)))),
                MainFor(cmd),
                string.Join(" ", ArgumentsFor(cmd).Select(Strings.AsArgument))
            );

            // Console.WriteLine("php {0}", proc.StartInfo.Arguments);
            var encoding = Console.OutputEncoding;

            Console.CancelKeyPress += (sender, args) => Console.OutputEncoding = encoding;
            Console.OutputEncoding = Encoding.UTF8;
            try
            {
                return cmd.ExecutionModel.Execute(proc);
            }
            finally
            {
                Console.OutputEncoding = encoding;
            }
        }
    }
}