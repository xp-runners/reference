using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

/// <summary>The plugin command searches for commands in Composer's globally required modules</summary>
public class Plugin : Command
{
    private string name;
    private string module;

    public Plugin(string name)
    {
        var composer = Paths.Compose(Environment.SpecialFolder.ApplicationData, "Composer", "vendor");
        var bin = Directory.GetFiles(Paths.Compose(composer, "bin"), "xp.*." + name).FirstOrDefault();

        if (null == bin)
        {
            throw new NotImplementedException(name);
        }

        this.name = name;
        this.module = Paths.Compose(composer, Path.GetFileName(bin).Substring("xp.".Length).Replace('.', Path.DirectorySeparatorChar));
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