using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Xp.Runners.IO;
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
            { "-watch", (self, value) => self.executionModel = new RunWatching(value) },
            { "-c", (self, value) => self.config = new IniConfigSource(new Ini(
                Directory.Exists(value) ? Paths.Compose(value, ini) : value
            )) }
        };

        private static Dictionary<string, Action<CommandLine>> flags = new Dictionary<string, Action<CommandLine>>()
        {
            { "-n", (self) => self.config = new EnvironmentConfigSource() },
            { "-supervise", (self) => self.executionModel = new Supervise() }
        };

        private Dictionary<string, List<string>> path = new Dictionary<string, List<string>>()
        {
            { "classpath", new List<string>() },
            { "modules", new List<string>() }
        };

        private Command command;
        private IEnumerable<string> arguments;
        private ExecutionModel executionModel;
        private ConfigSource config = null;

        /// <summary>Global path</summary>
        public Dictionary<string, List<string>> Path
        {
            get { return path; }
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
                    config = new CompositeConfigSource(
                        new EnvironmentConfigSource(),
                        new IniConfigSource(new Ini(Paths.Compose(".", ini))),
                        null != home ? new IniConfigSource(new Ini(Paths.Compose(home, ".xp", ini))) : null,
                        new IniConfigSource(new Ini(Paths.Compose(Environment.SpecialFolder.LocalApplicationData, "Xp", ini))),
                        new IniConfigSource(new Ini(Paths.Compose(Paths.Binary().DirName(), ini)))
                    );
                }
                return config;
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
            return arg.All(char.IsLower);
        }

        /// <summary>Returns a command by a given name</summary>
        private Command AsCommand(string arg)
        {
            var type = Type.GetType("Xp.Runners.Commands." + arg.UpperCaseFirst());
            if (null == type)
            {
                return new Plugin(arg);
            }
            else
            {
                return Activator.CreateInstance(type) as Command;
            }
        }

        /// <summary>Creates the commandline representation from argv</summary>
        public CommandLine(string[] argv)
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
                        throw new ArgumentException("Argument `" + argv[i] + "` requires a value");
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
                    throw new ArgumentException("Unknown argument `" + argv[i] + "`");
                }
                else if (IsCommand(argv[i]))
                {
                    command = AsCommand(argv[i]);
                    offset = ++i;
                    break;
                }
                else
                {
                    command = new Commands.Run();
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
                "{0}(Options: {1}, Command: {2}, Arguments: [{3}])",
                typeof(CommandLine),
                string.Join(", ", options.Select(pair => string.Format("{0}=[{1}]", pair.Key, string.Join(", ", pair.Value)))),
                command,
                string.Join(", ", arguments)
            );
        }
    }
}