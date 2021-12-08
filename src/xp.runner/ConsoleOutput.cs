using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

namespace Xp.Runners
{
    class ConsoleOutput : IDisposable
    {
        Encoding original = null;
        TextWriter output = null;
        TextWriter error = null;

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, int mode);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out int mode);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetStdHandle(int handle);

        const int STD_OUTPUT_HANDLE = -11;
        const int ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

        /// <summary>Starts output, enabling ANSI color support when necessary</summary>
        public ConsoleOutput()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                original = Console.OutputEncoding;

                Console.CancelKeyPress += (sender, args) => Console.OutputEncoding = original;
                Console.OutputEncoding = Encoding.UTF8;

                // See "Console Virtual Terminal Sequences"
                // https://msdn.microsoft.com/en-us/library/windows/desktop/mt638032(v=vs.85).aspx
                int mode;
                IntPtr handle = GetStdHandle(STD_OUTPUT_HANDLE);
                GetConsoleMode(handle, out mode);
                mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
                SetConsoleMode(handle, mode);
            }
        }

        /// <summary>Ends output, restoring previous console</summary>
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