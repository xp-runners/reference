using System.Linq;
using System.Collections.Generic;

/// <summary>eval $code [$arg0 [$arg1 [...]]]</summary>
public class Eval : Command
{
    public Eval(ConfigSource configuration) : base(configuration) { }

    /// <summary>Command line arguments.</summary>
    protected override IEnumerable<string> ArgumentsFor(CommandLine cmd)
    {
        return (new string[] { "xp.runtime.Evaluate" }).Concat(cmd.Arguments);
    }
}