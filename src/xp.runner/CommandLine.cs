using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using Xp.Runners.IO;
using Xp.Runners.Commands;
using Xp.Runners.Exec;
using Xp.Runners.Config;

namespace Xp.Runners
{
    public class CommandLine
    {
        const string ini = "xp.ini";

        private static Dictionary<string, Type> aliases = new Dictionary<string, Type>()
        {
            { "-v", typeof(Commands.Version) },
            { "-e", typeof(Commands.Eval) },
            { "-w", typeof(Commands.Write) },
            { "-d", typeof(Commands.Dump) },
            { "-?", typeof(Commands.Help) }
        };

        private static Dictionary<string, Action<CommandLine, string>> options = new Dictionary<string, Action<CommandLine, string>>()
        {
            { "-cp", (self, value) => self.path["classpath"].Add(value) },
            { "-cp?", (self, value) => self.path["classpath"].Add("?" + value) },
            { "-cp!", (self, value) => self.path["classpath"].Add("!" + value) },
            { "-m", (self, value) => self.path["modules"].Add(value) },
            { "-env", (self, value) => self.envFiles.Add(new Ini(value)) },
            { "-watch", (self, value) => self.executionModel = new RunWatching(value) },
            { "-repeat", (self, value) => self.executionModel = new RunRepeatedly(value) },
            { "-c", (self, value) => self.config = new CompositeConfigSource(
                new EnvironmentConfigSource(),
                new IniConfigSource(new Ini(Directory.Exists(value) ? Paths.Compose(value, ini) : value))
            ) }
        };

        private static Dictionary<string, Action<CommandLine>> flags = new Dictionary<string, Action<CommandLine>>()
        {
            { "-n", (self) => self.config = new EnvironmentConfigSource() },
            { "-supervise", (self) => self.executionModel = new Supervise() }
        };

        private Dictionary<string, List<string>> path = new Dictionary<string, List<string>>()
        {
            { "classpath", new List<string>() },
            { "modules", new List<string>() },
        };

        private List<Ini> envFiles = new List<Ini>();
        private Command command;
        private IEnumerable<string> arguments;
        private ExecutionModel executionModel;
        private ConfigSource config = null;

        /// <summary>Global path</summary>
        public Dictionary<string, List<string>> Path
        {
            get { return path; }
        }

        /// <summary>.env files</summary>
        public List<Ini> EnvFiles
        {
            get { return envFiles; }
        }

        /// <summary>Subcommand name</summary>
        public Command Command
        {
            get { return command ?? new Commands.Help(); }
        }

        /// <summary>Subcommand arguments</summary>
        public IEnumerable<string> Arguments
        {
            get { return arguments; }
        }

        /// <summary>Execution model - once, watching(dir), ...</summary>
        public ExecutionModel ExecutionModel
        {
            get { return executionModel ?? new RunOnce(); }
        }

        /// <summary>Configuration source</summary>
        public ConfigSource Configuration
        {
            get {
                if (null == config)
                {
                    var home = Environment.GetEnvironmentVariable("HOME");
                    var defaults = new ConfigSource[] {
                        new EnvironmentConfigSource(),
                        new IniConfigSource(new Ini(Paths.Compose(".", ini))),
                        new IniConfigSource(new Ini(Paths.Compose(home, ".xp", ini))),
                        new IniConfigSource(new Ini(Paths.Compose(Environment.SpecialFolder.LocalApplicationData, "Xp", ini))),
                        new IniConfigSource(new Ini(Paths.Compose(Paths.Binary().DirName(), ini)))
                    };
                    return new CompositeConfigSource(defaults.Where(source => source.Valid()));
                }
                return config;
            }
        }

        /// <summary>Creates the commandline representation from argv</summary>
        public CommandLine(string[] argv)
        {
            Parse(argv, ComposerFile.Empty);
        }

        /// <summary>Creates the commandline representation from argv and a composer file</summary>
        public CommandLine(string[] argv, ComposerFile composer)
        {
            using (composer)
            {
                path["classpath"].Add(
                    "?" + Paths.Compose(Paths.DirName(composer.SourceUri),
                    composer.Definitions.VendorDir,
                    "autoload.php"
                ));
                Parse(argv, composer);
            }
        }

        /// <summary>Adds an environment file if it exists</summary>
        public void TryAddEnv(string file)
        {
            if (File.Exists(file))
            {
                this.envFiles.Add(new Ini(file));
            }
        }

        /// <summary>Expand environment variables</summary>
        public IEnumerable<KeyValuePair<string, string>> Expand(StringDictionary env)
        {
            var expand = new Regex("\\$((?<name>[a-zA-Z_]+)|{(?<name>[^}]+)})");
            foreach (var envFile in envFiles)
            {
                foreach (var key in envFile.Keys("default"))
                {
                    yield return new KeyValuePair<string, string>(key, expand.Replace(
                        envFile.Get("default", key, ""),
                        match => env[match.Groups["name"].Value] // Doesn't throw if name doesn't exist
                    ));
                }
            }
        }

        /// <summary>Determines if a command line arg is an option</summary>
        private bool IsOption(string arg)
        {
            return arg.StartsWith("-");
        }

        /// <summary>Determines if a command line arg refers to a command</summary>
        private bool IsCommand(string arg)
        {
            return arg.All(c => char.IsLower(c) || '-' == c);
        }

        /// <summary>Parses arguments from a given string</summary>
        private IEnumerable<string> ArgsOf(string arg)
        {
            int index, quote;
            var offset = 0;
            do
            {
                if ('"' == arg[offset] || '\'' == arg[offset])
                {
                    index = arg.IndexOf(arg[offset], offset + 1);
                    quote = 1;
                }
                else
                {
                    index = arg.IndexOf(' ', offset);
                    quote = 0;
                }

                if (-1 == index)
                {
                    yield return arg.Substring(offset + quote);
                    break;
                }
                else
                {
                    yield return arg.Substring(offset + quote, index - offset - quote);
                    offset = index + 1 + quote;
                }
            }
            while (offset < arg.Length);
        }

        /// <summary>Parses command line</summary>
        private void Parse(string[] argv, ComposerFile composer)
        {
            var offset = 0;
            for (var i = 0; i < argv.Length; i++)
            {
                if (aliases.ContainsKey(argv[i]))
                {
                    command = Activator.CreateInstance(aliases[argv[i]]) as Command;
                    offset = ++i;
                    break;
                }
                else if (options.ContainsKey(argv[i]))
                {
                    if (i >= argv.Length - 1)
                    {
                        throw new CannotExecute("Argument `" + argv[i] + "` requires a value");
                    }
                    options[argv[i]](this, argv[++i]);
                    offset = i + 1;
                }
                else if (flags.ContainsKey(argv[i]))
                {
                    flags[argv[i]](this);
                    offset = i + 1;
                }
                else if (IsOption(argv[i]))
                {
                    throw new CannotExecute("Unknown argument `" + argv[i] + "`");
                }
                else if (IsCommand(argv[i]))
                {
                    var name = argv[i];
                    offset = ++i;

                    // Check builtin commands
                    var type = Type.GetType("Xp.Runners.Commands." + name.UpperCaseFirst());
                    if (null != type)
                    {
                        command = Activator.CreateInstance(type) as Command;
                        break;
                    }

                    // Check composer.json for scripts
                    if (composer.Definitions.Scripts.ContainsKey(name))
                    {
                        command = null;
                        executionModel = null;
                        config = null;
                        Parse(ArgsOf(composer.Definitions.Scripts[name]).Skip(1).ToArray(), ComposerFile.Empty);
                        arguments = arguments.Concat(argv.Skip(offset));
                        return;
                    }

                    // Otherwise, it's a plugin defined via `bin/xp.{org}.{slug}.{name}`
                    command = new Plugin(name);
                    break;
                }
                else if (!argv[i].EndsWith(".class.php") && !argv[i].EndsWith(".xar") && File.Exists(argv[i]))
                {
                    command = new Script(argv[i]);
                    offset = i + 1;
                    break;
                }
                else
                {
                    command = new Run();
                    offset = i;
                    break;
                }
            } 

            arguments = argv.Skip(offset);
        }

        /// <summary>Entry point</summary>
        public int Execute()
        {
            return Command.Execute(this, Configuration);
        }

        public override string ToString()
        {
            return string.Format(
                "{0}({1}  Paths: {2}{1}  Config: {3}{1}  ExecutionModel: {4}{1}  Command: {5}{1}  Arguments: [{6}]{1})",
                typeof(CommandLine),
                Environment.NewLine,
                string.Join(", ", Path.Select(pair => string.Format("{0}=[{1}]", pair.Key, string.Join(", ", pair.Value)))),
                Configuration.ToString().Replace(Environment.NewLine, Environment.NewLine + "  "),
                ExecutionModel,
                Command,
                string.Join(", ", Arguments)
            );
        }
    }
}