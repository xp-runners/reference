using System.Linq;
using System.Collections.Generic;

/// <summary>help [$command]</summary>
public class Help : Command
{
    public Help(ConfigSource configuration) : base(configuration) { }

    /// <summary>Command line arguments.</summary>
    protected override IEnumerable<string> ArgumentsFor(CommandLine cmd)
    {
        return new string[]
        {
            "xp.runtime.ShowResource",
            (cmd.Arguments.FirstOrDefault() ?? "usage") + ".txt",
            "1"
        };
    }
}