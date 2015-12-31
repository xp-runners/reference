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
    private static Dictionary<string, string> COMMANDS = new Dictionary<string, string>()
    {
        { "-v", "version" },
        { "-e", "eval" },
        { "-w", "write" },
        { "-d", "dump" },
        { "-?", "help" }
    };

    private Dictionary<string, List<string>> options = new Dictionary<string, List<string>>()
    {
        { "classpath", new List<string>() },
        { "modules", new List<string>() }
    };
    private string command = "help";
    private IEnumerable<string> arguments;

    /// <summary>Global options</summary>
    public Dictionary<string, List<string>> Options
    {
        get { return options; }
    }

    /// <summary>Subcommand name</summary>
    public string Command
    {
        get { return command; }
    }

    /// <summary>Subcommand arguments</summary>
    public IEnumerable<string> Arguments
    {
        get { return arguments; }
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
                command = COMMANDS[argv[i]];
                offset = ++i;
                break;
            }
            else if (IsOption(argv[i]))
            {
                throw new ArgumentException("Unknown option `" + argv[i] + "`");
            }
            else if (IsCommand(argv[i]))
            {
                command = argv[i];
                offset = ++i;
                break;
            }
            else
            {
                command = "run";
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
            string.Join(", ", options.Select(pair =>
            {
                return string.Format("{0}=[{1}]", pair.Key, string.Join(", ", pair.Value));
            })),
            command,
            string.Join(", ", arguments)
        );
    }
}