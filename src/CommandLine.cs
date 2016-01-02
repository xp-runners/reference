using System;
using System.Linq;
using System.Collections.Generic;

public class CommandLine
{
    private static Dictionary<string, string> OPTIONS = new Dictionary<string, string>()
    {
        { "-cp", "classpath" },
        { "-m", "modules" }
    };
    private static Dictionary<string, Type> COMMANDS = new Dictionary<string, Type>()
    {
        { "-v", typeof(Version) },
        { "-e", typeof(Eval) },
        { "-w", typeof(Write) },
        { "-d", typeof(Dump) },
        { "-?", typeof(Help) }
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
        get { return command ?? new Help(); }
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
        var type = Type.GetType(arg.UpperCaseFirst());
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
            if (OPTIONS.ContainsKey(argv[i]))
            {
                options[OPTIONS[argv[i]]].Add(argv[++i]);
                offset = i + 1;
            }
            else if (COMMANDS.ContainsKey(argv[i]))
            {
                command = Activator.CreateInstance(COMMANDS[argv[i]]) as Command;
                offset = ++i;
                break;
            }
            else if ("-watch".Equals(argv[i]))
            {
                executionModel = new RunWatching(argv[++i]);
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
                command = new Run();
                offset = i;
                break;
            }
        } 

        arguments = new ArraySegment<string>(argv, offset, argv.Length - offset);
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