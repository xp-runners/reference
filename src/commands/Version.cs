using System.Linq;
using System.Collections.Generic;

/// <summary>version</summary>
public class Version : Command
{
    public Version(ConfigSource configuration) : base(configuration) { }

    /// <summary>Command line arguments.</summary>
    protected override IEnumerable<string> ArgumentsFor(CommandLine cmd)
    {
        return new string[] { "xp.runtime.Version" };
    }
}