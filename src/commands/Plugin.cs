using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

/// <summary>The plugin command searches for commands in Composer's globally required modules</summary>
public class Plugin : Command
{
    private string name;
    private string composer;
    private string module;

    public Plugin(string name)
    {
        foreach (var composer in ComposerLocations().Where(Directory.Exists))
        {
            var bin = Directory.GetFiles(Paths.Compose(composer, "bin"), "xp.*." + name).FirstOrDefault();
            if (null == bin) continue;

            this.name = name;
            this.composer = composer;
            this.module = Path.GetFileName(bin).Substring("xp.".Length).Replace('.', Path.DirectorySeparatorChar);
            return;
        }
        throw new NotImplementedException(name);
    }

    /// <summary>Additional modules to load. Overwrite in subclasses if necessary!</summary>
    protected override IEnumerable<string> ModulesFor(CommandLine cmd)
    {
        return new string[] { Paths.Compose(this.composer, this.module) };
    }

    /// <summary>Command line arguments.</summary>
    protected override IEnumerable<string> ArgumentsFor(CommandLine cmd)
    {
        return (new string[] { "xp." + this.name + ".Runner" }).Concat(cmd.Arguments);
    }
}