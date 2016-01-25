using System;
using System.Linq;
using System.Collections.Generic;
using Xp.Runners.Exec;
using Xp.Runners.Config;

namespace Xp.Runners
{
    public class CommandLine
    {
        private static Dictionary<string, Type> aliases = new Dictionary<string, Type>()
        {
            { "-v", typeof(Commands.Version) },
            { "-e", typeof(Commands.Eval) },
            { "-w", typeof(Commands.Write) },
            { "-d", typeof(Commands.Dump) },
            { "-?", typeof(Commands.Help) }
        };

        private Dictionary<string, List<string>> options = new Dictionary<string, List<string>>()
        {
            { "classpath", new List<string>() },
            { "modules", new List<string>() }
        };
        private Command command;
        private IEnumerable<string> arguments;
        private ExecutionModel executionModel;

        /// <summary>Global options</summary>
        public Dictionary<string, List<string>> Options
        {
            get { return options; }
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
                return new Commands.Plugin(arg);
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
                else if ("-cp" == argv[i])
                {
                    options["classpath"].Add(argv[++i]);
                    offset = i + 1;
                }
                else if ("-cp?" == argv[i] || "-cp!" == argv[i])
                {
                    options["classpath"].Add(argv[i].Substring("-cp".Length) + argv[++i]);
                    offset = i + 1;
                }
                else if ("-m" == argv[i])
                {
                    options["modules"].Add(argv[++i]);
                    offset = i + 1;
                }
                else if ("-watch".Equals(argv[i]))
                {
                    executionModel = new RunWatching(argv[++i]);
                    offset = i + 1;
                }
                else if ("-serve".Equals(argv[i]))
                {
                    executionModel = new Serve();
                    offset = i + 1;
                }
                else if (IsOption(argv[i]))
                {
                    throw new ArgumentException("Unknown option `" + argv[i] + "`");
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

            arguments = new ArraySegment<string>(argv, offset, argv.Length - offset);
        }

        /// <summary>Entry point</summary>
        public int Execute(ConfigSource configuration)
        {
            return Command.Execute(this, configuration);
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