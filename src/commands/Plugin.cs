using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

/// <summary>The plugin command searches for commands in Composer's globally required modules
/// for the "xp-framework" vendor.</summary>
public class Plugin : Command
{
    private string name;
    private string module;

    public Plugin(string name)
    {
        var module = Paths.Compose(Environment.SpecialFolder.ApplicationData, "Composer", "vendor", "xp-framework", name);
        if (!Directory.Exists(module))
        {
            throw new NotImplementedException(name);
        }

        this.name = name;
        this.module = module;
    }

    /// <summary>Additional modules to load. Overwrite in subclasses if necessary!</summary>
    protected override IEnumerable<string> ModulesFor(CommandLine cmd)
    {
        return new string[] { this.module };
    }

    /// <summary>Command line arguments.</summary>
    protected override IEnumerable<string> ArgumentsFor(CommandLine cmd)
    {
        return (new string[] { "xp." + this.name + ".Runner" }).Concat(cmd.Arguments);
    }
}