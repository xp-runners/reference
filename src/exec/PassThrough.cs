using System;
using System.IO;

namespace Xp.Runners
{
    public class PassThrough : IStdStreamReader
    {
        /// <summary>Creates a new reader</summary>
        public void Start(StreamReader reader) 
        {
            // NOOP
        }

        /// <summary>Wait until we've read until the end</summary>
        public bool WaitForEnd()
        {
            return true;
        }
    }
}