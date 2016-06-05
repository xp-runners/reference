using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Xp.Runners.IO;

namespace Xp.Runners.Config
{
    class EnvironmentConfigSource : ConfigSource 
    {
        /// Returns whether this config source is valid
        public bool Valid() 
        {
            return true;
        }

        /// Returns the path(s) for this config source
        public IEnumerable<string> Path()
        {
            return new string[] { };
        }

        /// Returns the use_xp setting derived from this config source
        public IEnumerable<string> GetUse() 
        {
            var env = Environment.GetEnvironmentVariable("USE_XP");
            return env == null ? null : Paths.Translate(
                Directory.GetCurrentDirectory(),
                env.Split(new char[] { System.IO.Path.PathSeparator })
            );
        }
        
        /// Returns the runtime to be used from this config source
        public string GetRuntime() 
        {
            return Environment.GetEnvironmentVariable("XP_RT");
        }

        /// Returns the PHP executable to be used from this config source
        /// based on the given runtime version, using the default otherwise.
        public string GetExecutable(string runtime) 
        {
            return null;
        }

        /// Returns the PHP extensions to be loaded from this config source
        /// based on the given runtime version and the defaults.
        public IEnumerable<string> GetExtensions(string runtime)
        {
            return null;
        }

        /// Returns the PHP runtime arguments to be used from this config source
        /// based on the given runtime version, overwriting the defaults.
        public Dictionary<string, IEnumerable<string>> GetArgs(string runtime)
        {
            return new Dictionary<string, IEnumerable<string>>();
        }

        /// Returns a string representation of this config source
        public override string ToString() 
        {
            return GetType().FullName;
        }
    }
}