using System.Linq;
using System.Collections.Generic;

/// <summary>version</summary>
public class Version : Command
{
    /// <summary>Command line arguments.</summary>
    protected override IEnumerable<string> ArgumentsFor(CommandLine cmd)
    {
        return new string[] { "xp.runtime.Version" };
    }
}