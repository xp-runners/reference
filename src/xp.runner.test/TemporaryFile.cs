using System;
using System.IO;

namespace Xp.Runners.Test
{
    public class TemporaryFile : IDisposable
    {
        public string Path { get; private set; }

        /// <summary>A temporary file with a given suffix</summary>
        public TemporaryFile(string suffix= "")
        {
            Path = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString("N") + suffix;
        }

        /// <summary>Creates an empty file</summary>
        public TemporaryFile Empty()
        {
            File.WriteAllText(Path, null);
            return this;
        }

        /// <summary>Creates a file with the given contents</summary>
        public TemporaryFile Containing(string contents)
        {
            File.WriteAllText(Path, contents);
            return this;
        }

        /// <summary>Removes file</summary>
        public void Dispose()
        {
            if (File.Exists(Path))
            {
                File.Delete(Path);
            }
        }
    }
}