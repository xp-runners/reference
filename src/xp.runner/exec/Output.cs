using System;
using System.IO;
using System.Text;

namespace Xp.Runners.Exec
{
    class Output : IDisposable
    {
        Encoding original = null;
        TextWriter output = null;
        TextWriter error = null;

        /// <summary>Starts output, enabling ANSI color support when necessary</summary>
        public Output()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                original = Console.OutputEncoding;

                Console.CancelKeyPress += (sender, args) => Console.OutputEncoding = original;
                Console.OutputEncoding = Encoding.UTF8;
            }
        }

        public void Dispose()
        {
            if (null != original)
            {
                Console.OutputEncoding = original;
            }
            if (null != output)
            {
                Console.SetOut(output);
            }
            if (null != error)
            {
                Console.SetError(error);
            }
        }        
    }
}