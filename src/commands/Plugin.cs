using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;

/// <summary>The plugin command searches for XP commands in Composer's vendor/bin directory</summary>
public class Plugin : Command
{
    private string name;
    private string module;
    private string composer;
    private static DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(Composer), new DataContractJsonSerializerSettings {
        UseSimpleDictionaryFormat = true
    });

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

    /// <summary>Returns the given module and a unique list of dependencies of a given module</summary>
    private IEnumerable<string> DependenciesAndSelf(string module, HashSet<string> loaded)
    {
        var path = Paths.Compose(composer, module);

        yield return path;
        loaded.Add(module);

        using (var stream = new FileStream(Paths.Compose(path, Composer.FILENAME), FileMode.Open))
        {
            var definitions = json.ReadObject(stream) as Composer;
            foreach (var require in definitions.Require.Where(pair => pair.Key.Contains('/')))
            {
                var dependency = require.Key.Replace('/', Path.DirectorySeparatorChar);
                if (loaded.Contains(dependency)) continue;

                foreach (var transitive in DependenciesAndSelf(dependency, loaded))
                {
                    yield return transitive;
                }
            }
        }
    }

    /// <summary>Additional modules to load. Overwrite in subclasses if necessary!</summary>
    protected override IEnumerable<string> ModulesFor(CommandLine cmd)
    {
        return DependenciesAndSelf(module, new HashSet<string>());
    }

    /// <summary>Command line arguments.</summary>
    protected override IEnumerable<string> ArgumentsFor(CommandLine cmd)
    {
        return (new string[] { "xp." + this.name + ".Runner" }).Concat(cmd.Arguments);
    }
}