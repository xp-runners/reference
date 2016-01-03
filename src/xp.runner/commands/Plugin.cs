using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using Xp.Runners;

namespace Xp.Runners.Commands
{
    /// <summary>The plugin command searches for XP commands in Composer's vendor/bin directory</summary>
    public class Plugin : Command
    {
        const int VENDOR = 1;
        const int NAME = 2;
        const int COMMAND = 3;

        private string entry;
        private string module;
        private IEnumerable<string> modules;
        private static DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(Composer), new DataContractJsonSerializerSettings {
            UseSimpleDictionaryFormat = true
        });

        public Plugin(string name)
        {
            foreach (var dir in ComposerLocations())
            {
                if (FindCommand(dir, name))
                {
                    modules = DependenciesAndSelf(dir, module, new HashSet<string>());
                    return;
                }
            }

            if (FindCommand(".", name)) 
            {
                modules = new string[] { "." };
                return;
            }

            throw new NotImplementedException(name);
        }

        /// <summary>Finds command by name in a given directory</summary>
        private bool FindCommand(string dir, string name)
        {
            var bin = Paths.Compose(dir, "bin");
            if (!Directory.Exists(bin)) return false;

            var spec = Directory.GetFiles(bin, "xp.*." + name).FirstOrDefault();
            if (null == spec) return false;

            var file = Path.GetFileName(spec).Split('.');

            module = file[VENDOR] + "/" + file[NAME];
            entry = string.Format(
                "xp.{0}.{1}Runner",
                file[NAME],
                file.Length > COMMAND ? file[COMMAND].UpperCaseFirst() : string.Empty
            );
            return true;
        }

        /// <summary>Returns the given module and a unique list of dependencies of a given module</summary>
        private IEnumerable<string> DependenciesAndSelf(string dir, string module, HashSet<string> loaded)
        {
            var path = Paths.Compose(dir, module.Replace('/', Path.DirectorySeparatorChar));
            yield return path;
            loaded.Add(module);

            using (var stream = new FileStream(Paths.Compose(path, Composer.FILENAME), FileMode.Open))
            {
                var definitions = json.ReadObject(stream) as Composer;
                foreach (var require in definitions.Require.Where(pair => pair.Key.Contains('/')))
                {
                    if (loaded.Contains(require.Key)) continue;

                    foreach (var transitive in DependenciesAndSelf(dir, require.Key, loaded))
                    {
                        yield return transitive;
                    }
                }
            }
        }

        /// <summary>Additional modules to load. Overwrite in subclasses if necessary!</summary>
        protected override IEnumerable<string> ModulesFor(CommandLine cmd)
        {
            return modules;
        }

        /// <summary>Command line arguments.</summary>
        protected override IEnumerable<string> ArgumentsFor(CommandLine cmd)
        {
            return (new string[] { entry }).Concat(cmd.Arguments);
        }
    }
}