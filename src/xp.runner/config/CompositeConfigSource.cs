﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Xp.Runners.Config
{
    class CompositeConfigSource : ConfigSource 
    {
        private IEnumerable<ConfigSource> sources;

        /// <summary>Creates a new composite - supporting varargs</summary>
        public CompositeConfigSource(params ConfigSource[] sources)
        {
            this.sources = new List<ConfigSource>(sources);
        }

        /// <summary>Creates a new composite from a enumerable of sources</summary>
        public CompositeConfigSource(IEnumerable<ConfigSource> sources)
        {
            this.sources = sources;
        }

        /// Returns the path(s) for this config source
        public IEnumerable<string> Path()
        {
            foreach (var source in sources)
            {
                foreach (var path in source.Path())
                {
                    yield return path;
                }
            }
        }

        private T AskEach<T>(Func<ConfigSource, T> closure)
        {
            foreach (var source in sources) 
            {
                T value = closure(source);
                if (value != null) return value;
            }
            return default(T); 
        }

        /// Returns whether this config source is valid
        public bool Valid()
        {
            foreach (var source in sources)
            {
                if (!source.Valid()) return false;
            }
            return true;
        }

        /// Returns the use_xp setting derived from this config source
        public IEnumerable<string> GetUse() 
        {
            return AskEach<IEnumerable<string>>((s) => s.GetUse());
        }

        /// Returns the runtime to be used from this config source
        public string GetRuntime()
        {
            return AskEach<string>((s) => s.GetRuntime());
        }

        /// Returns the PHP executable to be used from this config source
        /// based on the given runtime version, using the default otherwise.
        public string GetExecutable(string runtime)
        {
            return AskEach<string>((s) => s.GetExecutable(runtime));
        }

        /// Returns the PHP extensions to be loaded from this config source
        /// based on the given runtime version and the defaults.
        public IEnumerable<string> GetExtensions(string runtime)
        {
            return AskEach<IEnumerable<string>>((s) => s.GetExtensions(runtime));
        }

        /// Returns the PHP runtime arguments to be used from this config source
        /// based on the given runtime version, overwriting the defaults.
        public Dictionary<string, IEnumerable<string>> GetArgs(string runtime)
        {
            var merged = new Dictionary<string, IEnumerable<string>>();
            foreach (var source in this.sources) 
            {
                var args = source.GetArgs(runtime);
                if (args == null) continue;

                foreach (var pair in args) 
                {
                    if (!merged.ContainsKey(pair.Key)) 
                    {
                        merged[pair.Key] = pair.Value;
                    }
                }
            }
            return merged;
        }
        
        /// Returns a string representation of this config source
        public override string ToString() 
        {
            var buffer = new StringBuilder(GetType().FullName);
            buffer.Append("(").Append(Environment.NewLine);
            foreach (var source in sources)
            {
                buffer.Append("  " + source + Environment.NewLine);
            }
            return buffer.Append(")").ToString();
        }
    }
}