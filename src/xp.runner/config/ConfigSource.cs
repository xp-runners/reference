using System;
using System.Collections.Generic;

namespace Xp.Runners.Config
{
    public interface ConfigSource 
    {
        /// Returns whether this config source is valid
        bool Valid();

        /// Returns the use_xp setting derived from this config source
        IEnumerable<string> GetUse();

        /// Returns the runtime to be used from this config source
        string GetRuntime();

        /// Returns the PHP executable to be used from this config source
        /// based on the given runtime version, using the default otherwise.
        string GetExecutable(string runtime);

        /// Returns the PHP extensions to be loaded from this config source
        /// based on the given runtime version and the defaults.
        IEnumerable<string> GetExtensions(string runtime);

        /// Returns the PHP runtime arguments to be used from this config source
        /// based on the given runtime version, overwriting the defaults.
        Dictionary<string, IEnumerable<string>> GetArgs(string runtime);
    }
}