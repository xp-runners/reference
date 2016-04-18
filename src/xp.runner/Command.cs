using System;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Xp.Runners.IO;
using Xp.Runners.Config;
using Xp.Runners.Commands;

namespace Xp.Runners
{
    public abstract class Command
    {
        /// <summary>Returns composer locations for current platform</summary>
        protected IEnumerable<string> ComposerLocations()
        {
            return IO.ComposerLocations.For(Environment.OSVersion.Platform);
        }

        /// <summary>Initialize this command</summary>
        public virtual void Initialize(CommandLine cmd, ConfigSource configuration)
        {
            // NOOP
        }

        /// <summary>Main script, e.g. "class". Overwrite in subclasses if necessary!</summary>
        protected virtual string MainFor(CommandLine cmd)
        {
            return "class";
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
            return arguments
                .Where(pair => null != pair.Value)
                .SelectMany(pair => pair.Value.Select(setting => string.Format("-d {0}={1}", pair.Key, setting)))
            ;
        }

        /// <summary>Returns path and path for all dependencies inside this path</summary>
        private IEnumerable<string> PathAndDependencies(string path, HashSet<string> loaded)
        {
            var result = new string[] { path };

            var file = Paths.Compose(path, ComposerFile.NAME);
            if (File.Exists(file))
            {
                using (var composer = new ComposerFile(file))
                {
                    return result.Concat(composer.Definitions.Require
                        .Where(require => require.Key.Contains('/'))
                        .SelectMany(require => ModuleAndDependencies(require.Key, loaded))
                    );
                }
            }
            else
            {
                return result;
            }
        }

        /// <summary>Returns paths to a given module and all of its unique dependencies</summary>
        private IEnumerable<string> ModuleAndDependencies(string module, HashSet<string> loaded)
        {
            if (loaded.Contains(module))
            {
                return new string[] { };
            }
            else
            {
                loaded.Add(module);
                return ComposerLocations()
                    .Select(location => Paths.Compose(location, module.Replace('/', Path.DirectorySeparatorChar)))
                    .Where(Directory.Exists)
                    .SelectMany(path => PathAndDependencies(path, loaded))
                ;
            }
        }

        /// <summary>Expand module path: Entries referring to existing paths are taken as-is,
        /// other entries are looked up in the composer locations.</summary>
        private IEnumerable<string> ExpandModulePath(IEnumerable<string> modules)
        {
            var loaded = new HashSet<string>();
            loaded.Add("xp-framework/core");

            return modules.SelectMany(module => Directory.Exists(module)
                ? PathAndDependencies(module, loaded)
                : ModuleAndDependencies(module, loaded)
            );
        }

        /// <summary>Entry point</summary>
        public virtual int Execute(CommandLine cmd, ConfigSource configuration)
        {
            Initialize(cmd, configuration);

            var proc = new Process();
            var runtime = configuration.GetRuntime();
            var ini = new Dictionary<string, IEnumerable<string>>()
            {
                { "magic_quotes_gpc", new string[] { "0" } },
                { "date.timezone", new string[] { TimeZoneInfo.Local.Olson() ?? "UTC" } },
                { "extension", configuration.GetExtensions(runtime) }
            };
            var use = configuration.GetUse() ?? new string[] { "xp-framework/core" };

            Encoding encoding;
            Func<string, string> args;
            var main = Paths.TryLocate(use, new string[] { Paths.Compose("tools", MainFor(cmd) + ".php") }).FirstOrDefault();
            if (null == main)
            {
                main = Paths.Locate(new string[] { Paths.Binary().DirName() }, new string[] { MainFor(cmd) + "-main.php" }).First();
                encoding = Encoding.UTF8;

                // Arguments are encoded in utf-7, which is binary-safe
                args = Arguments.Encode;
            }
            else
            {
                args = Arguments.Escape;
                encoding = Encoding.GetEncoding("iso-8859-1");
            }

            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.FileName = configuration.GetExecutable(runtime) ?? (runtime ?? "php");
            proc.StartInfo.Arguments = string.Format(
                "-C -q -d include_path=\".{0}{1}{0}{0}.{0}{2}\" {3} {4} {5}",
                Paths.Separator,
                string.Join(Paths.Separator, ExpandModulePath(use.Concat(cmd.Options["modules"].Concat(ModulesFor(cmd))))),
                string.Join(Paths.Separator, cmd.Options["classpath"].Concat(ClassPathFor(cmd))),
                string.Join(" ", IniSettings(ini.Concat(configuration.GetArgs(runtime)))),
                main,
                string.Join(" ", ArgumentsFor(cmd).Select(args))
            );

            return cmd.ExecutionModel.Execute(proc, encoding);
        }
    }
}