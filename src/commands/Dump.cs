using System.Linq;
using System.Collections.Generic;

/// <summary>dump $code [$arg0 [$arg1 [...]]]</summary>
public class Dump : Command
{
    public Dump(ConfigSource configuration) : base(configuration) { }

    /// <summary>Command line arguments.</summary>
    protected override IEnumerable<string> ArgumentsFor(CommandLine cmd)
    {
        return (new string[] { "xp.runtime.Dump" }).Concat(cmd.Arguments);
    }
}