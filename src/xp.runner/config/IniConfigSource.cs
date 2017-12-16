using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Xp.Runners.IO;

namespace Xp.Runners.Config
{
    class IniConfigSource : ConfigSource 
    {
        private Ini ini;
        private static string EXTENSION = Environment.OSVersion.Platform == PlatformID.Win32NT ? "php_{0}.dll" : "{0}.so";

        /// Returns whether this config source is valid
        public bool Valid() 
        {
            return ini.Exists();
        }

        /// Constructor
        public IniConfigSource(Ini ini) 
        {
            this.ini = ini;
        }
        
        /// Returns the use_xp setting derived from this config source
        public IEnumerable<string> GetUse() 
        {
            string value = ini.Get("default", "use");
            return null == value ? null : Paths.Translate(
                Paths.DirName(ini.FileName),
                value.Split(new char[] { Path.PathSeparator })
            );
        }

        /// Returns the runtime to be used from this config source
        public string GetRuntime()
        {
            return ini.Get("default", "rt");
        }

        /// Returns the PHP executable to be used from this config source
        /// based on the given runtime version, using the default otherwise.
        public string GetExecutable(string runtime) 
        {
            return ini.Get("runtime@" + runtime, "default") ?? ini.Get("runtime", "default");
        }

        /// Returns the PHP extensions to be loaded from this config source
        /// based on the given runtime version and the defaults.
        public IEnumerable<string> GetExtensions(string runtime)
        {
            var empty = new string[] {};
            foreach (var extension in ini.GetAll("runtime", "extension", empty))
            {
                yield return extension.Contains(".") ? extension : string.Format(EXTENSION, extension);
            }
            foreach (var extension in ini.GetAll("runtime@" + runtime, "extension", empty))
            {
                yield return extension.Contains(".") ? extension : string.Format(EXTENSION, extension);
            }
        }

        /// Returns all keys in a given section as key/value pair
        protected IEnumerable<KeyValuePair<string, IEnumerable<string>>> ArgsInSection(string section)
        {
            var empty = new string[] {};
            foreach (var key in ini.Keys(section, empty))
            {
                if (!("default".Equals(key) || "extension".Equals(key)))
                {
                    yield return new KeyValuePair<string, IEnumerable<string>>(key, ini.GetAll(section, key, empty));
                }
            }
        }

        /// Returns the PHP runtime arguments to be used from this config source
        /// based on the given runtime version, overwriting the defaults.
        public Dictionary<string, IEnumerable<string>> GetArgs(string runtime)
        {
            var args = new Dictionary<string, IEnumerable<string>>();
            foreach (var pair in ArgsInSection("runtime"))
            {
                args[pair.Key] = pair.Value;
            }
            foreach (var pair in ArgsInSection("runtime@" + runtime))
            {
                args[pair.Key] = pair.Value;
            }

            return args;
        }
        
        /// Returns a string representation of this config source
        public override string ToString() 
        {
            return new StringBuilder(GetType().FullName)
                .Append("<")
                .Append(ini.FileName)
                .Append(">")
                .ToString()
            ;
        }
    }
}