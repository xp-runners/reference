using System;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Xp.Runners.IO;
using Xp.Runners.Config;

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
            if (!configuration.Valid())
            {
                throw new CannotExecute("Invalid configuration: " + configuration);
            }
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

        /// <summary>Use composer to find xp-framework/core</summary>
        private IEnumerable<string> UseComposer()
        {
            return ComposerLocations().Select(dir => Paths.Compose(dir, "vendor", "xp-framework", "core")).Where(Directory.Exists);
        }

        /// <summary>Entry point</summary>
        public virtual int Execute(CommandLine cmd, ConfigSource configuration)
        {
            Initialize(cmd, configuration);

            var binary = Paths.Binary();
            var proc = new Process();
            var runtime = configuration.GetRuntime();
            var ini = new Dictionary<string, IEnumerable<string>>()
            {
                { "date.timezone", new string[] { TimeZoneInfo.Local.Olson() ?? Environment.GetEnvironmentVariable("TZ") ?? "UTC" } },
                { "extension", configuration.GetExtensions(runtime) }
            };
            var use = configuration.GetUse() ?? UseComposer();

            Encoding encoding;
            Func<string, string> args;
            var main = Paths.TryLocate(use, new string[] { Paths.Compose("tools", MainFor(cmd) + ".php") }).FirstOrDefault();
            if (null == main)
            {
                main = Paths.Locate(new string[] { Paths.DirName(binary) }, new string[] { MainFor(cmd) + "-main.php" }).First();

                // Arguments are encoded in utf-7, which is binary-safe
                args = Arguments.Encode;
                encoding = Encoding.UTF8;
            }
            else
            {
                args = Arguments.Escape;
                encoding = Encoding.GetEncoding("iso-8859-1");
            }

            var shell = Shell.Parse(configuration.GetExecutable(runtime) ?? (runtime ?? "php"));
            var dot = new string[] { "." };

            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.FileName = shell.Executable;
            proc.StartInfo.Arguments = string.Format(
                "{1} -C -q -d include_path=\"{2}{0}{0}{3}\" {4} {5} {6}",
                Paths.Separator,
                string.Join(" ", shell.Arguments),
                string.Join(Paths.Separator, dot.Concat(use.Concat(cmd.Path["modules"].Concat(ModulesFor(cmd))))),
                string.Join(Paths.Separator, dot.Concat(cmd.Path["classpath"].Concat(ClassPathFor(cmd)))),
                string.Join(" ", IniSettings(ini.Concat(configuration.GetArgs(runtime)))),
                main,
                string.Join(" ", ArgumentsFor(cmd).Select(args))
            );

            var env = proc.StartInfo.EnvironmentVariables;
            env["XP_EXE"] = binary;
            env["XP_VERSION"] = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            env["XP_MODEL"] = cmd.ExecutionModel.Name;
            env["XP_COMMAND"] = GetType().Name.ToLower();
            foreach (var config in cmd.EnvFiles)
            {
                foreach (var key in config.Keys("default"))
                {
                    env[key] = config.Get("default", key).Trim('"');
                }
            }

            return cmd.ExecutionModel.Execute(proc, encoding);
        }
    }
}