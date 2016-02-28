using System;
using System.Text;

namespace Xp.Runners.Exec
{
    class Output : IDisposable
    {
        Encoding original = null;

        /// <summary>Starts output, enabling ANSI color support when necessary</summary>
        public Output()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                original = Console.OutputEncoding;

                Console.CancelKeyPress += (sender, args) => Console.OutputEncoding = original;
                Console.OutputEncoding = Encoding.UTF8;
                if (!Console.IsOutputRedirected)
                {
                    Console.SetOut(new ANSISupport(Console.Out));
                }
            }
        }

        public void Dispose()
        {
            if (null != original)
            {
                Console.OutputEncoding = original;
            }
        }        
    }
}