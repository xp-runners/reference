using System;
using System.Collections;
using System.Collections.Generic;

namespace Xp.Runners.Test
{
    public class ModifiedEnvironment : IDisposable
    {
        private Stack<DictionaryEntry> _restore = new Stack<DictionaryEntry>();

        /// <summary>Adds an environment variable to this environment</summary>
        public ModifiedEnvironment With(string name, string value)
        {
            _restore.Push(new DictionaryEntry(name, Environment.GetEnvironmentVariable(name)));
            Environment.SetEnvironmentVariable(name, value);
            return this;
        }

        /// <summary>Removes environment variables with a given prefix from this environment</summary>
        public ModifiedEnvironment RemoveAny(string prefix)
        {
            foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
            {
                var name = (string)entry.Key;
                if (name.StartsWith(prefix))
                {
                    _restore.Push(entry);
                    Environment.SetEnvironmentVariable(name, null);
                }
            }
            return this;
        }

        /// <summary>Resets environment variables</summary>
        public void Dispose()
        {
            foreach (var entry in _restore)
            {
                Environment.SetEnvironmentVariable((string)entry.Key, (string)entry.Value);
            }
        }
    }
}